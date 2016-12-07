// Copyright 2011-2016 Global Software Innovation Pty Ltd

using NUnit.Framework;
using System.Collections.Generic;
using EDC.ReadiNow.Cache;
using EDC.ReadiNow.Model;
using EDC.ReadiNow.Core;
using EDC.SoftwarePlatform.Activities;
using ReadiNow.TenantHealth.Test.Infrastructure;

namespace ReadiNow.TenantHealth.Test.Components
{
    /// <summary>
    /// Test workflows in all tenants.
    /// </summary>
    [TestFixture]
    public class WorkflowTenantTests
    {
        /// <summary>
        /// CSV list of names of reports to flag as ignored
        /// </summary>
        const string WorkflowsToIgnore = "";

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
			// Clear Workflow Actions cache
			/////
			cacheService = Factory.WorkflowActionsFactory as ICacheService;
			cacheService?.Clear( );
		}

        [Test]
        [TestCaseSource( "CompileWorkflow_GetTestData" )]
        public void CompileWorkflow( TenantInfo tenant, long workflowId, string workflowName )
        {
			using ( tenant.GetTenantAdminContext( ) )
			using ( new MemoryGuard( 2 * TenantHealthHelpers.OneGb, ClearSelectedCaches ) )
			using ( new MemoryGuard( 3 * TenantHealthHelpers.OneGb, TenantHealthHelpers.ClearAllCaches ) )
			using ( new MemoryLogger( new KeyValuePair<string, object>( "Tenant", tenant.TenantName ), new KeyValuePair<string, object>( "Workflow Id", workflowId ) ) )
			{
				// Load workflow
				Workflow workflow = Factory.EntityRepository.Get<Workflow>( workflowId );
				Assert.That( workflow, Is.Not.Null, "Workflow entity not null" );

				// Compile workflow
				var metadata = new WorkflowMetadata( workflow );

				// Check for validation errors
				string messages = null;
				if ( metadata.ValidationMessages != null )
					messages = string.Join( "\r\n", metadata.ValidationMessages );

				Assert.That( metadata.HasViolations, Is.False, messages );
			}
        }


        /// <summary>
        /// Fetch list of all workflows across all tenants.
        /// </summary>
        public IEnumerable<TestCaseData> CompileWorkflow_GetTestData( )
        {
            return TenantHealthHelpers.GetInstancesAsTestData( "core:workflow", WorkflowsToIgnore );
        }
    }
}
