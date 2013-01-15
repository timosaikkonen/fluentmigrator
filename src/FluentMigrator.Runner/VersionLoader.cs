using System;
using System.Collections.Generic;
using System.Data;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Xml;
using System.Xml.Serialization;
using FluentMigrator.Expressions;
using FluentMigrator.Model;
using FluentMigrator.Runner.Versioning;
using FluentMigrator.VersionTableInfo;

namespace FluentMigrator.Runner
{
    public class VersionLoader : IVersionLoader
    {
        private bool _versionSchemaMigrationAlreadyRun;
        private bool _versionMigrationAlreadyRun;
        private bool _versionUniqueMigrationAlreadyRun;
        private bool _versionMetadataMigrationAlreadyRun;
        private IAppliedVersions appliedVersions;
        private IMigrationConventions Conventions { get; set; }
        private IMigrationProcessor Processor { get; set; }
        protected Assembly Assembly { get; set; }
        public IVersionTableMetaData VersionTableMetaData { get; private set; }
        public IMigrationRunner Runner { get; set; }
        public VersionSchemaMigration VersionSchemaMigration { get; private set; }
        public IMigration VersionMigration { get; private set; }
        public IMigration VersionUniqueMigration { get; private set; }
        public IMigration VersionMetadataMigration { get; private set; }
        
        public VersionLoader(IMigrationRunner runner, Assembly assembly, IMigrationConventions conventions)
        {
            Runner = runner;
            Processor = runner.Processor;
            Assembly = assembly;

            Conventions = conventions;
            VersionTableMetaData = GetVersionTableMetaData();
            VersionMigration = new VersionMigration(VersionTableMetaData);
            VersionSchemaMigration = new VersionSchemaMigration(VersionTableMetaData);
            VersionUniqueMigration = new VersionUniqueMigration(VersionTableMetaData);
			VersionMetadataMigration = new VersionMetadataMigration(VersionTableMetaData);

            LoadVersionInfo();
        }

        public void UpdateVersionInfo(IVersionInfo version)
        {
            var dataExpression = new InsertDataExpression();
            dataExpression.Rows.Add(CreateVersionInfoInsertionData(version));
            dataExpression.TableName = VersionTableMetaData.TableName;
            dataExpression.SchemaName = VersionTableMetaData.SchemaName;
            dataExpression.ExecuteWith(Processor);
        }

        public IVersionTableMetaData GetVersionTableMetaData()
        {
            Type matchedType = Assembly.GetExportedTypes().FirstOrDefault(t => Conventions.TypeIsVersionTableMetaData(t));

            if (matchedType == null)
            {
                return new DefaultVersionTableMetaData();
            }

            return (IVersionTableMetaData)Activator.CreateInstance(matchedType);
        }

        protected virtual InsertionDataDefinition CreateVersionInfoInsertionData(IVersionInfo version)
        {
            var xmlSerializer = new XmlSerializer(typeof(VersionMetadata));

            var metadata = new XmlDocument();
            using (var writer = metadata.CreateNavigator().AppendChild())
            {
                version.Metadata.WriteXml(writer);
            }
            metadata.InsertBefore(metadata.CreateXmlDeclaration("1.0", null, null), metadata.DocumentElement);

            return new InsertionDataDefinition
                       {
                           new KeyValuePair<string, object>(VersionTableMetaData.ColumnName, version.Version),
                           new KeyValuePair<string, object>("AppliedOn", DateTime.UtcNow),
                           new KeyValuePair<string, object>(VersionTableMetaData.MetadataColumnName, metadata)
                       };
        }

        public IAppliedVersions AppliedVersions
        {
            get
            {
                return appliedVersions;
            }
            set
            {
                if (value == null)
                    throw new ArgumentException("Cannot set VersionInfo to null");

                appliedVersions = value;
            }
        }

        public bool AlreadyCreatedVersionSchema
        {
            get
            {
                return Processor.SchemaExists(VersionTableMetaData.SchemaName);
            }
        }

        public bool AlreadyCreatedVersionTable
        {
            get
            {
                return Processor.TableExists(VersionTableMetaData.SchemaName, VersionTableMetaData.TableName);
            }
        }

        public bool AlreadyMadeVersionUnique
        {
            get
            {
                return Processor.ColumnExists(VersionTableMetaData.SchemaName, VersionTableMetaData.TableName, "AppliedOn");
            }
        }

        public bool AlreadyCreatedMetadataColumn
        {
            get
            {
                return Processor.ColumnExists(VersionTableMetaData.SchemaName, VersionTableMetaData.TableName, VersionTableMetaData.MetadataColumnName);
            }
        }


        public void LoadVersionInfo()
        {
            if (!AlreadyCreatedVersionSchema && !_versionSchemaMigrationAlreadyRun)
            {
                Runner.Up(VersionSchemaMigration);
                _versionSchemaMigrationAlreadyRun = true;
            }

            if (!AlreadyCreatedVersionTable && !_versionMigrationAlreadyRun)
            {
                Runner.Up(VersionMigration);
                _versionMigrationAlreadyRun = true;
            }

            if (!AlreadyMadeVersionUnique && !_versionUniqueMigrationAlreadyRun)
            {
                Runner.Up(VersionUniqueMigration);
                _versionUniqueMigrationAlreadyRun = true;
            }

            if (!AlreadyCreatedMetadataColumn && !_versionMetadataMigrationAlreadyRun)
            {
                Runner.Up(VersionMetadataMigration);
                _versionMetadataMigrationAlreadyRun = true;
            }

            appliedVersions = new AppliedVersions();

            if (!AlreadyCreatedVersionTable) return;

            var dataSet = Processor.ReadTableData(VersionTableMetaData.SchemaName, VersionTableMetaData.TableName);
            
            foreach (DataRow row in dataSet.Tables[0].Rows)
            {
                var versionInfo = GetVersionInfo(row);
                appliedVersions.AddAppliedMigration(versionInfo);
            }
        }

        public IVersionInfo GetVersionInfo(DataRow row)
        {
            using (var stringReader = new StringReader((string)row[VersionTableMetaData.MetadataColumnName]))
            using (var metadataReader = new XmlTextReader(stringReader))
            {
                return new VersionInfo
                {
                    Version = (long)row[VersionTableMetaData.ColumnName],
                    Metadata = row[VersionTableMetaData.MetadataColumnName] == DBNull.Value ? null : new VersionMetadata(metadataReader)
                };
            }
        }

        public void RemoveVersionTable()
        {
            var expression = new DeleteTableExpression { TableName = VersionTableMetaData.TableName, SchemaName = VersionTableMetaData.SchemaName };
            expression.ExecuteWith(Processor);

            if (!string.IsNullOrEmpty(VersionTableMetaData.SchemaName))
            {
                var schemaExpression = new DeleteSchemaExpression { SchemaName = VersionTableMetaData.SchemaName };
                schemaExpression.ExecuteWith(Processor);
            }
        }

        public void DeleteVersion(long version)
        {
            var expression = new DeleteDataExpression { TableName = VersionTableMetaData.TableName, SchemaName = VersionTableMetaData.SchemaName };
            expression.Rows.Add(new DeletionDataDefinition
                                    {
                                        new KeyValuePair<string, object>(VersionTableMetaData.ColumnName, version)
                                    });
            expression.ExecuteWith(Processor);
        }
    }
}