using EDC.ReadiNow.IO;
using EDC.ReadiNow.Model;
using EDC.ReadiNow.Diagnostics;
using System;
using System.Collections.Generic;
using System.Linq;

namespace EDC.ReadiNow.Metadata.Solutions.DataUpgrade
{
    /// <summary>
    /// Handles the upgrade of the send email workflow activity 
    /// </summary>
    internal class SendEmailWorkflowActivity
    {
        /// <summary>
        /// Method for testing
        /// </summary>
        public static void UpgradeAllTenants()
        {
            using (new AdministratorContext())
            {
                foreach (var tenant in Entity.GetInstancesOfType<Tenant>(false))
                {
                    using (new TenantAdministratorContext(tenant.Id))
                    {
                        Upgrade(null);
                    }
                }
            }
        }

        public static void Upgrade(IEnumerable<Solution> solutions)
        {
            //var version = SystemInfo.PlatformVersion;
            var activitiesList = Entity.GetInstancesOfType<SendEmailActivity>().ToList();

            EventLog.Application.WriteTrace($"Upgrading '{activitiesList.Count}' Email Workflow activity instances for tenant '{RequestContext.GetContext()?.Tenant?.Name}'");

            foreach (var activity in activitiesList)
            {
                var wfActivity = Entity.Get<WfActivity>(activity, true);
                if ( wfActivity == null )
                    continue;

                var arg = wfActivity.ExpressionMap?.FirstOrDefault(x => x.ArgumentToPopulate?.Alias == "core:sendEmailRecipientList");
                if (string.IsNullOrEmpty(arg?.ExpressionString))
                    wfActivity.ContainingWorkflow.AddUpdateEntityExpressionToInputArgument(wfActivity, "Email To", new EntityRef("core:sendEmailActivityRecipientsAddress"));
                else
                    wfActivity.ContainingWorkflow.AddUpdateEntityExpressionToInputArgument(wfActivity, "Email To", new EntityRef("core:sendEmailActivityRecipientsList"));

                wfActivity.ContainingWorkflow.AddUpdateEntityExpressionToInputArgument(wfActivity, "Send As", new EntityRef("core:sendEmailActivityGroupDistribution"));

                var arg2 = wfActivity.ExpressionMap?.FirstOrDefault(x => x.ArgumentToPopulate?.Alias == "core:sendEmailNoReply");
                if (string.IsNullOrEmpty(arg2?.ExpressionString))
                    wfActivity.ContainingWorkflow.AddUpdateExpressionToArgument(wfActivity, "No Reply", "true");

                wfActivity.Save();
            }
        }
    }
}
