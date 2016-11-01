// Copyright 2011-2016 Global Software Innovation Pty Ltd

using System;
using System.Collections.Generic;
using System.Linq;
using Autofac;
using EDC.ReadiNow.CAST.Marketplace.Model;
using EDC.ReadiNow.CAST.Marketplace.Services;
using EDC.ReadiNow.CAST.Model;
using EDC.ReadiNow.CAST.Services;
using EDC.ReadiNow.Core;
using EDC.ReadiNow.Diagnostics;
using EDC.ReadiNow.Diagnostics.ActivityLog;
using EDC.ReadiNow.Model;
using EDC.SoftwarePlatform.Activities;

namespace EDC.ReadiNow.CAST.Activities
{
    /// <summary>
    /// CAST workflow activity that takes all the applications and application versions included in a <see cref="MarketplaceProduct"/> and orders
    /// them suitably for installation based on any interdependencies. Then iterates over the ordered list and outputting the application ID and
    /// versions so follow on activities can be performed.
    /// </summary>
    public class ForEachApplicationImplementation : ActivityImplementationBase, IRunNowActivity
    {
        #region Private Properties

        private IMarketplaceService MarketplaceService { get; set; }

        private IPlatformService PlatformService { get; set; }

        private IActivityLogWriter ActivityLogWriter { get; set; }

        #endregion

        /// <summary>
        /// Basic constructor.
        /// </summary>
        public ForEachApplicationImplementation()
        {
            MarketplaceService = Factory.Current.Resolve<IMarketplaceService>();
            PlatformService = Factory.Current.Resolve<IPlatformService>();
            ActivityLogWriter = Factory.Current.Resolve<IActivityLogWriter>();
        }

        /// <summary>
        /// Runs when the activity is run by the workflow.
        /// </summary>
        /// <param name="context">The run state.</param>
        /// <param name="inputs">The inputs.</param>
        public void OnRunNow(IRunState context, ActivityInputs inputs)
        {
            var startedKey = GetArgumentKey(StartedArgumentAlias);
            var listKey = GetArgumentKey(ListArgumentAlias);
            var appIdKey = GetArgumentKey(ApplicationIdArgumentAlias);
            var appVersionKey = GetArgumentKey(ApplicationVersionArgumentAlias);

            var ids = new List<Guid>();
            var started = context.GetArgValue<bool?>(ActivityInstance, startedKey) ?? false;
            if (!started)
            {
                // set that the activity has started
                context.SetArgValue(ActivityInstance, startedKey, true);

                // retrieve the product
                var productSku = GetArgumentValue<string>(inputs, ProductSkuArgumentAlias);

                var product = MarketplaceService.GetProduct(productSku);
                if (product == null)
                {
                    throw new WorkflowRunException("Product {0} was not found.", productSku);
                }

                // process the included apps and app versions and sort their ids
                ids.AddRange(GetSortedApplicationIds(product));
            }
            else
            {
                // retrieve the current state of the ordered list from the context
                var list = context.GetArgValue<string>(ActivityInstance, listKey);
                if (!string.IsNullOrEmpty(list))
                {
                    ids.AddRange(list.Split(',').Select(Guid.Parse));
                }
            }

            // loop over the next id on the list
            var current = ids.FirstOrDefault();
            if (current != default(Guid))
            {
                // set the application id and any specific version info on the output
                var variables = GetAppVariables(context, current);
                if (variables.Item1 != Guid.Empty)
                {
                    context.SetArgValue(ActivityInstance, appIdKey, variables.Item1);
                    context.SetArgValue(ActivityInstance, appVersionKey, variables.Item2);
                }

                // remove this id from the list and store it again
                ids = ids.Skip(1).ToList();

                var list = string.Join(",", ids);

                context.SetArgValue(ActivityInstance, listKey, list);
                context.ExitPointId = new EntityRef(LoopExitPointAlias);
            }
            else
            {
                // we have finished
                context.SetArgValue(ActivityInstance, startedKey, false);
                context.SetArgValue(ActivityInstance, listKey, null);
                context.ExitPointId = new EntityRef(FinishedExitPointAlias);
            }
        }

        #region Private Methods

        /// <summary>
        /// Given an identifier, will examine if it is a known application or application version, and return the appropriate
        /// variables to pass along the workflow.
        /// </summary>
        /// <param name="context">The run state context.</param>
        /// <param name="id">The identifer.</param>
        /// <returns>The application id and optional version string.</returns>
        private Tuple<Guid, string> GetAppVariables(IRunState context, Guid id)
        {
            var appId = Guid.Empty;
            string version = null;

            try
            {
                var app = PlatformService.GetApp(id);
                if (app != null)
                {
                    appId = id;
                }
                else
                {
                    var appVersion = PlatformService.GetAppVersion(id);
                    if (appVersion != null &&
                        appVersion.Application != null &&
                        appVersion.Application.ApplicationId.HasValue)
                    {
                        appId = appVersion.Application.ApplicationId.Value;
                        version = appVersion.Version;
                    }
                }
            }
            catch (Exception)
            {
                LogToRun(context, string.Format("Failed to loop over '{0}'.", id));
            }

            return new Tuple<Guid, string>(appId, version);
        }

        /// <summary>
        /// Examines the applications and application versions in a product and produces a sorted list for a sequence of
        /// actions (usually installation) that takes dependencies into account.
        /// </summary>
        /// <param name="product">The product to examine.</param>
        /// <returns>The sorted list of ids.</returns>
        private IEnumerable<Guid> GetSortedApplicationIds(IMarketplaceProduct product)
        {
            var set = new List<Guid?>();

            set.AddRange(product.IncludesApps.Select(a => a.ApplicationId));
            set.AddRange(product.IncludesAppVersions.Select(a => a.VersionId));
            set.AddRange(product.IncludesApps.SelectMany(i => i.RequiredApps).Select(a => a.ApplicationId));
            set.AddRange(product.IncludesApps.SelectMany(i => i.RequiredAppVersions).Select(a => a.VersionId));
            set.AddRange(product.IncludesAppVersions.SelectMany(i => i.RequiredApps).Select(a => a.ApplicationId));
            set.AddRange(product.IncludesAppVersions.SelectMany(i => i.RequiredAppVersions).Select(a => a.VersionId));

            var ids = set.Where(s => s != null && s.Value != Guid.Empty).Select(s => s.Value).Distinct().ToList();

            var sorter = new TopologicalSorter(ids.Count);
            var indices = new Dictionary<Guid, int>();

            // add vertices  
            for (var i = 0; i < ids.Count; i++)
            {
                indices[ids[i]] = sorter.AddVertex(i);
            }

            // add edges
            foreach (var app in product.IncludesApps)
            {
                if (app.ApplicationId == null || app.ApplicationId.Value == Guid.Empty)
                {
                    continue;
                }

                AddRequiredAppEdgesToSorter(app.RequiredApps, sorter, ids, app.ApplicationId.Value);
                AddRequiredAppVersionEdgesToSorter(app.RequiredAppVersions, sorter, ids, app.ApplicationId.Value);
            }

            foreach (var appVersion in product.IncludesAppVersions)
            {
                if (appVersion.VersionId == null || appVersion.VersionId.Value == Guid.Empty)
                {
                    continue;
                }

                AddRequiredAppEdgesToSorter(appVersion.RequiredApps, sorter, ids, appVersion.VersionId.Value);
                AddRequiredAppVersionEdgesToSorter(appVersion.RequiredAppVersions, sorter, ids, appVersion.VersionId.Value);
            }

            var sorted = sorter.Sort();
            var result = sorted.Select(t => ids[t]).ToList();

            return result;
        }

        /// <summary>
        /// Processes a list of applications and their dependencies ready for sorting.
        /// </summary>
        /// <param name="requiredApps">The list of required apps.</param>
        /// <param name="sorter">The topological sorter.</param>
        /// <param name="ids">The full indexed list of ids.</param>
        /// <param name="currentId">The current id that owns the required apps.</param>
        private void AddRequiredAppEdgesToSorter(
            IEnumerable<ManagedApp> requiredApps,
            TopologicalSorter sorter,
            IList<Guid> ids,
            Guid currentId)
        {
            foreach (var req in requiredApps)
            {
                if (req.ApplicationId == null || req.ApplicationId.Value == Guid.Empty)
                {
                    continue;
                }

                // required app --> dependent app / app version
                sorter.AddEdge(ids.IndexOf(req.ApplicationId.Value), ids.IndexOf(currentId));

                AddRequiredAppEdgesToSorter(req.RequiredApps, sorter, ids, req.ApplicationId.Value);
                AddRequiredAppVersionEdgesToSorter(req.RequiredAppVersions, sorter, ids, req.ApplicationId.Value);
            }
        }

        /// <summary>
        /// Processes a list of application versions and their dependencies ready for sorting.
        /// </summary>
        /// <param name="requiredAppVersions">The list of required app versions.</param>
        /// <param name="sorter">The topological sorter.</param>
        /// <param name="ids">The full indexed list of ids.</param>
        /// <param name="currentId">The current id that owns the required app versions.</param>
        private void AddRequiredAppVersionEdgesToSorter(
            IEnumerable<ManagedAppVersion> requiredAppVersions,
            TopologicalSorter sorter,
            IList<Guid> ids,
            Guid currentId)
        {
            foreach (var req in requiredAppVersions)
            {
                if (req.VersionId == null || req.VersionId.Value == Guid.Empty)
                {
                    continue;
                }

                // required app version --> dependent app/app version
                sorter.AddEdge(ids.IndexOf(req.VersionId.Value), ids.IndexOf(currentId));

                AddRequiredAppEdgesToSorter(req.RequiredApps, sorter, ids, req.VersionId.Value);
                AddRequiredAppVersionEdgesToSorter(req.RequiredAppVersions, sorter, ids, req.VersionId.Value);
            }
        }

        /// <summary>
        /// Logs a message to the workflow run.
        /// </summary>
        /// <param name="context">The workflow run context.</param>
        /// <param name="message">The message to log.</param>
        private void LogToRun(IRunState context, string message)
        {
            var activityName = string.Empty;

            var wf = context != null ? context.GetSafeWorkflowDescription() : "";

            EventLog.Application.WriteTrace(wf + message);

            if (context == null || context.WorkflowRun == null)
            {
                return;
            }

            if (context.CurrentActivity != null)
            {
                activityName = context.CurrentActivity.Name;
            }

            var logEntry = new LogActivityLogEntry
            {
                Name = activityName,
                Description = message
            };

            ActivityLogWriter.WriteLogEntry(logEntry.As<TenantLogEntry>());
        }
        #endregion

        #region Internals

        internal static string FinishedExitPointAlias { get { return "cast:exitPointForEachApplicationActivityFinished"; } }

        internal static string LoopExitPointAlias { get { return "cast:exitPointForEachApplicationActivityLoop"; } }

        internal static string ProductSkuArgumentAlias { get { return "cast:inForEachApplicationActivityProductSku"; } }

        internal static string StartedArgumentAlias { get { return "cast:xxForEachApplicationActivityStarted"; } }

        internal static string ListArgumentAlias { get { return "cast:xxForEachApplicationActivityList"; } }

        internal static string ApplicationIdArgumentAlias { get { return "cast:outForEachApplicationActivityApplicationId"; } }

        internal static string ApplicationVersionArgumentAlias { get { return "cast:outForEachApplicationActivityApplicationVersion"; } }

        #endregion
    }
}
