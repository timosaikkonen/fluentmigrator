namespace FluentMigrator.Runner.Versioning
{
    public interface IAppliedVersions
    {
        void AddAppliedMigration(IVersionInfo migration);
        System.Collections.Generic.IEnumerable<IVersionInfo> AppliedMigrations();
        bool HasAppliedMigration(long version);
        IVersionInfo Latest();
    }
}
