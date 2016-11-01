// Copyright 2011-2016 Global Software Innovation Pty Ltd

using EDC.ReadiNow.Configuration;
using Quartz;
using Quartz.Impl;
using Quartz.Impl.Matchers;
using Quartz.Simpl;
using System;
using System.Linq;
using System.Collections.Specialized;
using System.Diagnostics;
using CL = Common.Logging;
using EventLog = EDC.ReadiNow.Diagnostics.EventLog;

namespace EDC.ReadiNow.Scheduling
{
    public static class SchedulingHelper
    {
        private static string _connectionString;            // only fetch this once to ensure that we don't get stale config errors.
        private static ISchedulerFactory _factory;

        public static IScheduler Instance 
        { 
            get 
            {
                if (_factory == null)
                    _factory = CreateFactory();

                // we are always fetching from the factory. This means that if someone has shutdown the scheduler (Looking at you deployment) a new instance will be returned.
                return GetOrCreateScheduler(); 
            } 
        }

        /// <summary>
        /// Has an instance been created?
        /// </summary>
        /// <returns></returns>
        public static bool HasInstanceBeenCreated()
        {
            return _factory != null;
        }


        private static ISchedulerFactory CreateFactory()
        {
            // configure the common logger to use the adapter to our logging.
            CL.LogManager.Adapter = new EdcLoggerFactoryAdapter();

            var properties = GetSchedulerProperties();

            var schedFact = new StdSchedulerFactory();
            schedFact.Initialize(properties);

            return schedFact;
        }


		/// <summary>
		/// Get the scheduler. If it has not been created, or if the existing one has been shutdown then get a new one.
		/// </summary>
		/// <returns></returns>
        private static IScheduler GetOrCreateScheduler()
        {
            var sched = _factory.GetScheduler();

            if (sched == null)
            {
                EventLog.Application.WriteWarning("Failed to get a scheduler with the given properties.");
            }

            return sched;
        }
        
        private static NameValueCollection GetSchedulerProperties()
        {
            var properties = new NameValueCollection();

            if (_connectionString == null)
            {
                var dbSettings = ConfigurationSettings.GetDatabaseConfigurationSection().ConnectionSettings;
                var dbInfo = DatabaseConfigurationHelper.Convert(dbSettings);
                _connectionString = dbInfo.ConnectionString + ";Enlist=false";
            }

            // Note that all types are referred to using n Assembly qualified name. This seems to be be best approach when the assemblies are in the GAC.

            properties["quartz.scheduler.instanceName"] = "ReadiNow Scheduler";
            properties["quartz.scheduler.instanceId"] = "AUTO";
			properties["quartz.scheduler.typeLoadHelper.type"] = typeof(SchedulingTypeLoadHelper).AssemblyQualifiedName;
			properties["quartz.scheduler.makeSchedulerThreadDaemon"] = "false";
            properties["quartz.threadPool.type"] = typeof(SimpleThreadPool).AssemblyQualifiedName; // "Quartz.Simpl.SimpleThreadPool, Quartz";
            properties["quartz.threadPool.threadCount"] = "5";
            properties["quartz.threadPool.threadPriority"] = "Normal";
			properties["quartz.threadPool.makeThreadsDaemons"] = "false";

            properties["quartz.jobStore.misfireThreshold"] = "60000";
            properties["quartz.jobStore.type"] = typeof(Quartz.Impl.AdoJobStore.JobStoreTX).AssemblyQualifiedName; //"Quartz.Impl.AdoJobStore.JobStoreTX, Quartz";
            properties["quartz.jobStore.useProperties"] = "true";
            properties["quartz.jobStore.dataSource"] = "default";
            properties["quartz.jobStore.tablePrefix"] = "QRTZ_";

            properties["quartz.jobStore.clustered"] = "true";
            properties["quartz.jobStore.driverDelegateType"] = typeof(EdcDelegate).AssemblyQualifiedName;   //"EDC.ReadiNow.Scheduling.EdcDelegate, EDC.ReadiNow.Common, Version=1.0.0.0, Culture=neutral";

            properties["quartz.dataSource.default.connectionString"] = _connectionString;
            properties["quartz.dataSource.default.provider"] = "SqlServer-20";

            return properties;
        }


		/// <summary>
		/// Pause the current tenants jobs before performing the action, then restart them once it completes.
		/// </summary>
		/// <param name="tenantId">The tenant identifier.</param>
		/// <param name="act">The act.</param>
        public static void PauseAndRestartTenantJobs(long tenantId, Action act)
        {
           
            PauseJobsForTenant(tenantId, SchedulingHelper.Instance);

            act();

            RestartJobsForTenant(tenantId, SchedulingHelper.Instance);  
        }



        public static GroupMatcher<JobKey> PauseJobsForTenant(long tenantId, IScheduler sch)
        {
            EventLog.Application.WriteInformation(
                string.Format("Pausing all Jobs for tenant Id: {0}", tenantId));

            var matchTenantJobs = GetTenantJobs(tenantId);

            sch.PauseJobs(matchTenantJobs);

            return matchTenantJobs;
        }


        public static GroupMatcher<JobKey> RestartJobsForTenant(long tenantId, IScheduler sch)
        {
            EventLog.Application.WriteInformation(
                string.Format("Restarting all Jobs for tenant Id: {0}", tenantId));

            var matchTenantJobs = GetTenantJobs(tenantId);

            sch.ResumeJobs(matchTenantJobs);

            return matchTenantJobs;
        }

        static GroupMatcher<JobKey> GetTenantJobs(long tenantId)
        {
            return GroupMatcher<JobKey>.GroupEquals(GetTenantGroupId(tenantId));
        }


        public static void PauseAndDeleteJobsForTenant(long tenantId, IScheduler sch)
        {
            EventLog.Application.WriteInformation(
                string.Format("Deleting all Jobs for tenant Id: {0}", tenantId));

            var tenantJobs = PauseJobsForTenant(tenantId, sch);

            var jobsKeys = sch.GetJobKeys(tenantJobs).ToList();

            sch.DeleteJobs(jobsKeys);
        }

        /// <summary>
        /// Get the ID string used to identify all the jobs and triggers for the given Tenant
        /// </summary>
        /// <returns></returns>
        public static string GetTenantGroupId(long tenantId)
        {
            return tenantId.ToString();
        }

    }

}
