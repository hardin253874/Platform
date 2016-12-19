// Copyright 2011-2016 Global Software Innovation Pty Ltd

using EDC.ReadiNow.Database;
using FluentAssertions;
using NUnit.Framework;

namespace EDC.ReadiNow.Test.Database
{
    /// <summary>
    /// Tests for the licensing metrics gathered by the platform db on a schedule.
    /// </summary>
    [TestFixture]
    class LicensingMetricsTests
    {
        /// <summary>
        /// Just ensure that the stored procs still at least run without error after any schema changes.
        /// </summary>
        [Test]
        [RunWithoutTransaction]
        //[Category("ExtendedTests")]
        public void RunsLikeTheScheduledAgentJob()
        {
            using (var ctx = DatabaseContext.GetContext(true))
            {
                // Create index entry
                var n = ctx.CreateCommand("INSERT INTO [dbo].[Lic_Index] DEFAULT VALUES").ExecuteNonQuery();
                n.Should().Be(1);

                // Get Table Metrics
                n = ctx.CreateCommand("spLicTable").ExecuteNonQuery();
                n.Should().NotBe(0);

                // Get Tenants
                n = ctx.CreateCommand("spLicTenant").ExecuteNonQuery();
                n.Should().NotBe(0);

                // Get Applications
                n = ctx.CreateCommand("spLicApplication").ExecuteNonQuery();
                n.Should().NotBe(0);

                // Object Counts Update
                n = ctx.CreateCommand("spLicObjectCount").ExecuteNonQuery();
                n.Should().NotBe(0);

                // Users Update
                n = ctx.CreateCommand("spLicUser").ExecuteNonQuery();
                n.Should().NotBe(0);

                // Records Update
                n = ctx.CreateCommand("spLicRecord").ExecuteNonQuery();
                n.Should().NotBe(0);

                // File Counts Update
                n = ctx.CreateCommand("spLicFileCount").ExecuteNonQuery();
                n.Should().NotBe(0);

                // Workflows Update
                n = ctx.CreateCommand("spLicWorkflow").ExecuteNonQuery();
                n.Should().NotBe(0);

                ctx.CommitTransaction();
            }
        }
    }
}
