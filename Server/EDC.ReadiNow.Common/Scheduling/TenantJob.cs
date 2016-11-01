// Copyright 2011-2016 Global Software Innovation Pty Ltd

using System;
using System.Globalization;
using EDC.ReadiNow.IO;
using Quartz;
using EDC.Diagnostics;
using EDC.ReadiNow.Model;
using EDC.ReadiNow.Messaging;

namespace EDC.ReadiNow.Scheduling
{
    /// <summary>
    /// A base class for a job which runs the execute code in the context of a tenant .
    /// </summary>
    [DisallowConcurrentExecution]
    public abstract class TenantJob : IJob
    {
        /// <summary>
        /// Called by the Quartz.IScheduler when a Quartz.ITrigger fires that is associated with the Quartz.IJob.
        /// </summary>
        /// <param name="jobContext">The execution context.</param>
        void IJob.Execute(IJobExecutionContext jobContext)
        {
            using (new DeferredChannelMessageContext())
            {
                var tenantName = "";

                var tenantId = TenantJobHelper.GetTenantIdFromContext(jobContext);
                if (tenantId < 0)
                {
                    throw new JobExecutionException("Failed to execute TenantJob: Tenant ID was not given.");
                }

                if (tenantId > 0)
                {
                    using (new GlobalAdministratorContext())
                    {
                        var tenant = Entity.Get<Tenant>(tenantId, new IEntityRef[] { Tenant.Name_Field, Tenant.IsTenantDisabled_Field });

                        if (tenant == null)
                        {
                            throw new JobExecutionException(string.Format("Failed to execute TenantJob: {0} was not a valid tenant.", tenantId));
                        }

                        if (tenant.IsTenantDisabled == true)
                        {
                            return;
                        }

                        tenantName = tenant.Name;
                    }
                }

                using (new TenantAdministratorContext(tenantId))
                {
                    DiagnosticsRequestContext.SetContext(tenantId, tenantName, null);

                    try
                    {
                        Execute(jobContext);
                    }
                    catch (Exception ex)
                    {
                        throw new JobExecutionException(string.Format("Failed to execute TenantJob:\n {0}", ex.Message), ex);
                    }
                    finally
                    {
                        DiagnosticsRequestContext.FreeContext();
                    }
                }
            }
        }

        protected abstract void Execute(IJobExecutionContext jobContext);
    }

    /// <summary>
    /// Helper class for executing tenant based schedule jobs.
    /// </summary>
    public static class TenantJobHelper
    {
        /// <summary>
        /// The key name to use for storing/retrieving the id of the tenant from the job execution context.
        /// </summary>
        public const string TenantIdContextKey = "TenantId";

        /// <summary>
        /// Gets the tenant id from the job execution context.
        /// </summary>
        /// <param name="context">The execution context.</param>
        /// <returns>The tenant id.</returns>
        public static long GetTenantIdFromContext(IJobExecutionContext context)
        {
            // DB has been set to store context as a string
            return GetLongFromJobContext(context, TenantIdContextKey);
        }

        #region Extension Methods

        /// <summary>
        /// Extension method for <see cref="JobBuilder"/> to ensure that the current tenant is being set in the job data.
        /// </summary>
        /// <param name="builder">The job builder.</param>
        /// <returns>The job builder.</returns>
        public static JobBuilder UsingCurrentTenant(this JobBuilder builder)
        {
            var context = RequestContext.GetContext();
            if (context == null || context.Tenant == null || context.Tenant.Id < 0)
            {
                return builder;
            }

            return builder.UsingJobData(TenantIdContextKey, context.Tenant.Id.ToString(CultureInfo.InvariantCulture));
        }

        #endregion

        #region Private Methods

        /// <summary>
        /// Retrieves the tenant id from the job execution context and attempts to parse it.
        /// </summary>
        /// <param name="context">The execution context.</param>
        /// <param name="key">The tenant id field key.</param>
        /// <returns>The tenant id.</returns>
        private static long GetLongFromJobContext(IJobExecutionContext context, string key)
        {
            var id = -1L;

            if (context != null && context.MergedJobDataMap != null && context.MergedJobDataMap.ContainsKey(key))
            {
                long.TryParse((string)context.MergedJobDataMap[key], out id);
            }

            return id;
        }

        #endregion
    }
}