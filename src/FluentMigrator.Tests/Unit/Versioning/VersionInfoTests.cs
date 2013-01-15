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

using System.Linq;
using FluentMigrator.Runner.Versioning;
using NUnit.Framework;
using NUnit.Should;

namespace FluentMigrator.Tests.Unit.Versioning
{
    [TestFixture]
    public class VersionInfoTests
    {
        private AppliedVersions appliedVersions;

        [SetUp]
        public void SetUp()
        {
            appliedVersions = new AppliedVersions();
        }

        [Test]
        public void CanAddAppliedMigration()
        {
            appliedVersions.AddAppliedMigration(new VersionInfo() { Version = 200909060953 });
            appliedVersions.HasAppliedMigration(200909060953).ShouldBeTrue();
        }

        [Test]
        public void CanGetLatestMigration()
        {
            appliedVersions.AddAppliedMigration(new VersionInfo() { Version = 200909060953 });
            appliedVersions.AddAppliedMigration(new VersionInfo() { Version = 200909060935 });
            appliedVersions.Latest().Version.ShouldBe(200909060953);
        }

        [Test]
        public void CanGetAppliedMigrationsLatestFirst()
        {
            appliedVersions.AddAppliedMigration(new VersionInfo() { Version = 200909060953 });
            appliedVersions.AddAppliedMigration(new VersionInfo() { Version = 200909060935 });
            var applied = appliedVersions.AppliedMigrations().ToList();
            applied[0].Version.ShouldBe(200909060953);
            applied[1].Version.ShouldBe(200909060935);
        }

        [Test]
        public void CanGetVersionMetadata()
        {
            var version = new VersionInfo { Version = 201301150001 };
            version.Metadata["Id"] = 1;
            appliedVersions.AddAppliedMigration(version);
            appliedVersions.Latest().Metadata["Id"].ShouldBe(1);
        }
    }
}
