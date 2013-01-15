using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using FluentMigrator.Model;

namespace FluentMigrator.Runner.Versioning
{
    public interface IVersionInfo
    {
        long Version { get; set; }
        IVersionMetadata Metadata { get; set; }
    }
}
