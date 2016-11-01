// Copyright 2011-2016 Global Software Innovation Pty Ltd
using NUnit.Framework;
using System.Collections.Generic;

namespace ReadiNow.TenantHealth.Test.Components
{
    /// <summary>
    /// Test calculated fields in all tenants.
    /// </summary>
    [TestFixture]
    public class AppTests
    {
        /// <summary>
        /// CSV list of names of apps to flag as ignored
        /// </summary>
        const string AppsToIgnore = "";


        [Test]
        [TestCaseSource( "InstalledApps_GetTestData" )]
        public void InstalledApps( TenantInfo tenant, long appId, string appName )
        {
            // The sole purpose of this is to generate test cases, one for each app, so we know what apps are installed.
            Assert.True( true );
        }


        /// <summary>
        /// Fetch list of all apps across all tenants.
        /// </summary>
        public IEnumerable<TestCaseData> InstalledApps_GetTestData( )
        {
            // Must return TenantId,EntityId,Name for all calculated fields
            string customSql = @"
                select isOfTypeRel.TenantId, isOfTypeRel.FromId, name.Data + ' (' + solutionVersion.Data + ')' from Relationship isOfTypeRel
                join Data_Alias isOfTypeAlias on isOfTypeRel.TypeId = isOfTypeAlias.EntityId and isOfTypeRel.TenantId = isOfTypeAlias.TenantId and isOfTypeAlias.Data='isOfType'
                join Data_Alias typeAlias on isOfTypeRel.ToId = typeAlias.EntityId and isOfTypeRel.TenantId = typeAlias.TenantId and typeAlias.Data='solution' and typeAlias.Namespace='core'
                join Data_Alias nameAlias on isOfTypeRel.TenantId = nameAlias.TenantId and nameAlias.Data='name'
                join Data_Alias solutionVersionStringAlias on isOfTypeRel.TenantId = solutionVersionStringAlias.TenantId and solutionVersionStringAlias.Data='solutionVersionString'
                left join Data_NVarChar name on isOfTypeRel.FromId = name.EntityId and isOfTypeRel.TenantId = name.TenantId and name.FieldId = nameAlias.EntityId
                left join Data_NVarChar solutionVersion on isOfTypeRel.FromId = solutionVersion.EntityId and isOfTypeRel.TenantId = solutionVersion.TenantId and solutionVersion.FieldId = solutionVersionStringAlias.EntityId
                where isOfTypeRel.TenantId <> 0";

            return TenantHealthHelpers.GetInstancesAsTestData( null, AppsToIgnore, customSql );
        }
    }
}
