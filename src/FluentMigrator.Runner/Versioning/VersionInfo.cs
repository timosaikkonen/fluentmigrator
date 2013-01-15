using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using FluentMigrator.Model;

namespace FluentMigrator.Runner.Versioning
{
    public class VersionInfo : IVersionInfo
    {
        public long Version { get; set; }
        public IVersionMetadata Metadata { get; set; }

        public VersionInfo()
        {
            Metadata = new VersionMetadata();
        }
    }
}
