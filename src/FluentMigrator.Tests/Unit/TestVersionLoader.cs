using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using FluentMigrator.Model;
using FluentMigrator.Runner;
using FluentMigrator.Runner.Versioning;

namespace FluentMigrator.Tests.Unit
{
    public class TestVersionLoader
        : IVersionLoader
    {
        private VersionTableInfo.IVersionTableMetaData versionTableMetaData;

        public TestVersionLoader(IMigrationRunner runner, VersionTableInfo.IVersionTableMetaData versionTableMetaData)
        {
            this.versionTableMetaData = versionTableMetaData;
            this.Runner = runner;
            this.AppliedVersions = new AppliedVersions();
            this.Versions = new List<IVersionInfo>();
        }

        public bool AlreadyCreatedVersionSchema { get; set; }

        public bool AlreadyCreatedVersionTable { get; set; }

        public void DeleteVersion(long version)
        {
            var versionToRemove = Versions.First(v => v.Version == version);
            Versions.Remove(versionToRemove);
        }

        public VersionTableInfo.IVersionTableMetaData GetVersionTableMetaData()
        {
            return versionTableMetaData;
        }

        public void LoadVersionInfo()
        {
            this.AppliedVersions = new AppliedVersions();

            foreach (var version in Versions)
            {
                this.AppliedVersions.AddAppliedMigration(version);
            }

            this.DidLoadVersionInfoGetCalled = true;
        }

        public bool DidLoadVersionInfoGetCalled { get; private set; }

        public void RemoveVersionTable()
        {
            this.DidRemoveVersionTableGetCalled = true;
        }

        public bool DidRemoveVersionTableGetCalled { get; private set; }

        public IMigrationRunner Runner { get; set; }

        public void UpdateVersionInfo(IVersionInfo version)
        {
            this.Versions.Add(version);

            this.DidUpdateVersionInfoGetCalled = true;
        }

        public bool DidUpdateVersionInfoGetCalled { get; private set; }

        public Runner.Versioning.IAppliedVersions AppliedVersions { get; set; }

        public VersionTableInfo.IVersionTableMetaData VersionTableMetaData
        {
            get { return versionTableMetaData; }
        }

        public List<IVersionInfo> Versions { get; private set; }
    }
}
