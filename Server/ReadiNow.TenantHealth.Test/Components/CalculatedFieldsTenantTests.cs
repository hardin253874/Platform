// Copyright 2011-2016 Global Software Innovation Pty Ltd

using System;
using NUnit.Framework;
using System.Linq;
using System.Collections.Generic;
using EDC.ReadiNow.Cache;
using EDC.ReadiNow.Core;
using EDC.ReadiNow.IO;
using EDC.ReadiNow.Metadata.Tenants;
using ReadiNow.Expressions.CalculatedFields;
using ReadiNow.TenantHealth.Test.Infrastructure;

namespace ReadiNow.TenantHealth.Test.Components
{
    /// <summary>
    /// Test calculated fields in all tenants.
    /// </summary>
    [TestFixture]
    public class CalculatedFieldsTenantTests
    {
        /// <summary>
        /// CSV list of names of calculated fields to flag as ignored (with typename in brackets as per sql below)
        /// </summary>
        const string CalculatedFieldsToIgnore = "Calc Missing Field (on AA_Calculations)";

		/// <summary>
		/// Clears the selected caches.
		/// </summary>
		private void ClearSelectedCaches( )
		{
			/////
			// Clear Report caches
			/////
			ICacheService cacheService = Factory.QueryRunner as ICacheService;
			cacheService?.Clear( );

			cacheService = Factory.QueryRepository as ICacheService;
			cacheService?.Clear( );

			cacheService = Factory.QuerySqlBuilder as ICacheService;
			cacheService?.Clear( );

			/////
			// Clear Calculated Field cache
			/////
			cacheService = Factory.CalculatedFieldMetadataProvider as ICacheService;
			cacheService?.Clear( );
		}

        [Test]
        [TestCaseSource( "CompileCalculation_GetTestData" )]
        public void CompileField( TenantInfo tenant, long calcFieldId, string calcFieldName )
        {
			using ( tenant.GetTenantAdminContext( ) )
			using ( new MemoryGuard( 2 * TenantHealthHelpers.OneGb, ClearSelectedCaches ) )
			using ( new MemoryGuard( 3 * TenantHealthHelpers.OneGb, TenantHealthHelpers.ClearAllCaches ) )
			using ( new MemoryLogger( new KeyValuePair<string, object>( "Tenant", tenant.TenantName ), new KeyValuePair<string, object>( "Calculated Field Id", calcFieldId ) ) )
			{
				CalculatedFieldMetadata staticResult;
				long [ ] fields = new [ ] { calcFieldId };

				// Perform static calculation
				staticResult = Factory.CalculatedFieldMetadataProvider.GetCalculatedFieldMetadata( fields, CalculatedFieldSettings.Default ).Single( );

				// We got a result (assert true)
				Assert.That( staticResult, Is.Not.Null, "CalculatedFieldMetadata is not null" );

				// It has no errors
				Assert.That( staticResult.Exception, Is.Null, $"{staticResult.Exception}" );

				// And we have an expression (assert true, if no error)
				Assert.That( staticResult.Expression, Is.Not.Null, "CalculatedFieldMetadata.Expression is not null." );
			}
        }


        /// <summary>
        /// Fetch list of all workflows across all tenants.
        /// </summary>
        public IEnumerable<TestCaseData> CompileCalculation_GetTestData( )
        {
            // Must return TenantId,EntityId,Name for all calculated fields
            string customSql = @"
                select icfData.TenantId, icfData.EntityId, name.Data + ' (on ' + typeName.Data + ')'
                from Data_Bit icfData
                join Data_Alias nameAlias on icfData.TenantId = nameAlias.TenantId and nameAlias.Data='name'
                join Data_Alias fieldIsOnTypeAlias on icfData.TenantId = fieldIsOnTypeAlias.TenantId and fieldIsOnTypeAlias.Data='fieldIsOnType'
                join Data_Alias icfAlias on icfData.TenantId = icfAlias.TenantId and icfData.FieldId = icfAlias.EntityId and icfAlias.Data='isCalculatedField' and icfAlias.Namespace='core'
                left join Data_NVarChar name on icfData.EntityId = name.EntityId and icfData.TenantId = name.TenantId and name.FieldId = nameAlias.EntityId
                left join Relationship fieldType on icfData.EntityId = fieldType.FromId and icfData.TenantId = fieldType.TenantId and fieldType.TypeId = fieldIsOnTypeAlias.EntityId
                left join Data_NVarChar typeName on fieldType.ToId = typeName.EntityId and fieldType.TenantId = typeName.TenantId and typeName.FieldId = nameAlias.EntityId
                where icfData.TenantId <> 0 and icfData.Data = 1 --isCalculatedField=true
                order by icfData.TenantId, name.Data";

            return TenantHealthHelpers.GetInstancesAsTestData( null, CalculatedFieldsToIgnore, customSql );
        }
    }
}
