#region License
// 
// Copyright (c) 2007-2009, Sean Chambers <schambers80@gmail.com>
// 
// Licensed under the Apache License, Version 2.0 (the "License");
// you may not use this file except in compliance with the License.
// You may obtain a copy of the License at
//
//   http://www.apache.org/licenses/LICENSE-2.0
//
// Unless required by applicable law or agreed to in writing, software
// distributed under the License is distributed on an "AS IS" BASIS,
// WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
// See the License for the specific language governing permissions and
// limitations under the License.
//
#endregion

using System.Collections.Generic;
using System.Linq;

namespace FluentMigrator.Runner.Versioning
{
    public class AppliedVersions : FluentMigrator.Runner.Versioning.IAppliedVersions
    {
        private IList<IVersionInfo> _versionsApplied = new List<IVersionInfo>();

        public IVersionInfo Latest()
        {
            return _versionsApplied.OrderByDescending(x => x.Version).FirstOrDefault();
        }

        public void AddAppliedMigration(IVersionInfo migration)
        {
            _versionsApplied.Add(migration);
        }

        public bool HasAppliedMigration(long version)
        {
            return _versionsApplied.Any(v => v.Version == version);
        }

        public IEnumerable<IVersionInfo> AppliedMigrations()
        {
            return _versionsApplied.OrderByDescending(x => x.Version).AsEnumerable();
        }
    }
}
