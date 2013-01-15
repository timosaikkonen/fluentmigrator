using FluentMigrator.Model;
using FluentMigrator.Runner.Versioning;

namespace FluentMigrator.Runner
{
    public interface IVersionLoader
    {
        bool AlreadyCreatedVersionSchema { get; }
        bool AlreadyCreatedVersionTable { get; }
        void DeleteVersion(long version);
        FluentMigrator.VersionTableInfo.IVersionTableMetaData GetVersionTableMetaData();
        void LoadVersionInfo();
        void RemoveVersionTable();
        IMigrationRunner Runner { get; set; }
        void UpdateVersionInfo(IVersionInfo version);
        FluentMigrator.Runner.Versioning.IAppliedVersions AppliedVersions { get; set; }
        FluentMigrator.VersionTableInfo.IVersionTableMetaData VersionTableMetaData { get; }
    }
}