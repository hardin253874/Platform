// Copyright 2011-2016 Global Software Innovation Pty Ltd
using NUnit.Framework;
using System.Collections.Generic;
using EDC.ReadiNow.Model;
using EDC.ReadiNow.Core;
using EDC.SoftwarePlatform.Activities;

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


        [Test]
        [TestCaseSource( "CompileWorkflow_GetTestData" )]
        public void CompileWorkflow( TenantInfo tenant, long workflowId, string workflowName )
        {
            using ( tenant.GetTenantAdminContext( ) )
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
