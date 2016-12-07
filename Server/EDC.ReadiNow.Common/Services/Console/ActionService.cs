// Copyright 2011-2016 Global Software Innovation Pty Ltd

using Autofac;
using System;
using System.Collections.Generic;
using System.Linq;
using EDC.Common;
using EDC.ReadiNow.Diagnostics;
using EDC.ReadiNow.Metadata.Query.Structured;
using EDC.ReadiNow.Model;
using EDC.ReadiNow.Security;
using QueryEntity = EDC.ReadiNow.Metadata.Query.Structured.Entity;
using EDC.ReadiNow.Core;
using EDC.ReadiNow.Expressions;
using EDC.ReadiNow.Model.CacheInvalidation;
using EDC.ReadiNow.Utc;

namespace EDC.ReadiNow.Services.Console
{
    /// <summary>
    /// Service for describing, persisting and executing user-defined and triggered operations.
    /// </summary>
    public class ActionService
    {
        #region Constructor
        

        /// <summary>
        /// Basic constructor (server-side).
        /// </summary>
        public ActionService()
        {
            EntityModelHelper = Factory.EntityRepository;
            EntityRepository = Factory.GraphEntityRepository;
            SecurityFilter = new SecurityActionMenuItemFilter( );
            _getActionsCache = Factory.Current.Resolve<ActionCache>( );
        }


        #endregion

        #region Internal Properties

        /// <summary>
        /// Gets a helper class for entity related calls.
        /// </summary>
        public IEntityRepository EntityModelHelper { get; set; }

        /// <summary>
        /// Gets a helper class for entity related calls.
        /// </summary>
        public IEntityRepository EntityRepository { get; set; }

        /// <summary>
        /// Filters (removes) action menu items the user lacks permission to use/see.
        /// </summary>
        public SecurityActionMenuItemFilter SecurityFilter { get; set; }

        /// <summary>
        /// 
        /// </summary>
        ActionCache _getActionsCache;


        #endregion


        /// <summary>
        /// The <see cref="ActionMenuItemInfo.HtmlActionState"/> value for menu items
        /// that create new entities.
        /// </summary>
        public static readonly string CreateMenuItemActionState = "createForm";
        
        /// <summary>
        /// The new holder menu item action state.
        /// </summary>
        internal static readonly string NewHolderMenuItemActionState = "newHolder";

        #region Target Help
        private readonly Func<ActionRequestExtended, ActionMenuItem, ActionTargetInfo> _targetActionReport = (request, item) => new ActionTargetInfo { Entity = request.Report, Name = request.Report?.Name };
        private readonly Func<ActionRequestExtended, ActionMenuItem, ActionTargetInfo> _targetActionForm = (request, item) => new ActionTargetInfo { Entity = request.Form, Name = request.Form?.Name };
        private readonly Func<ActionRequestExtended, ActionMenuItem, ActionTargetInfo> _targetActionReportType = (request, item) => new ActionTargetInfo { Entity = request.ReportBaseType, Name = request.ReportBaseType?.Name };
        private readonly Func<ActionRequestExtended, ActionMenuItem, ActionTargetInfo> _targetActionFormType = (request, item) => new ActionTargetInfo { Entity = request.TypeToEditWithForm, Name = request.TypeToEditWithForm?.Name };
        private readonly Func<ActionRequestExtended, ActionMenuItem, ActionTargetInfo> _targetActionSelected = (request, item) => new ActionTargetInfo { Entity = request.LastSelectedResource, Name = request.LastSelectedResource?.Name };
        private readonly Func<ActionRequestExtended, ActionMenuItem, ActionTargetInfo> _targetFormEntityData = (request, item) => new ActionTargetInfo { Entity = request.FormDataEntity, Name = request.FormDataEntity?.Name };
        private readonly Func<ActionRequestExtended, ActionMenuItem, ActionTargetInfo> _targetActionWorkflow = (request, item) =>
        {
            var wfItem = item.As<WorkflowActionMenuItem>();
            var wf = wfItem?.ActionMenuItemToWorkflow;
            if (wf != null)
            {
                return new ActionTargetInfo { Entity = wf, Name = wf.Name };
            }
            return null;
        };
        private readonly Func<ActionRequest, ActionMenuItem, ActionTargetInfo> _targetActionGenerateDocument = (request, item) =>
        {
            var rtItem = item.As<GenerateDocumentActionMenuItem>();
            var rt = rtItem?.ActionMenuItemToReportTemplate;
            if (rt != null)
            {
                return new ActionTargetInfo { Entity = rt, Name = rt.Name };
            }
            return null;
        };
        #endregion

        #region Public Methods

        /// <summary>
        /// Gets the console actions that are relevant to the details in the request object.
        /// </summary>
        /// <param name="request">The request params.</param>
        /// <returns>Details of the relevant console actions.</returns>
		public ActionResponse GetActions( ActionRequestExtended request )
        {
            if (request == null)
                throw new ArgumentException("The request object may not be null.");

            ActionResponse result;

            var isFromCache = _getActionsCache.TryGetOrAdd(request.GetRequestHash( ), out result, (key) =>
            {
	            using ( CacheContext cacheContext = new CacheContext( ) )
	            {
					if ( request.LastSelectedResourceId != null && request.LastSelectedResourceId.Value >= 0 )
					{
						cacheContext.Entities.Add( request.LastSelectedResourceId.Value );
					}

					bool needToSecure = NeedToSecureRequest( request );

		            if ( needToSecure )
		            {
			            return GetActionsImpl( request );
		            }

			        using ( new SecurityBypassContext( ) )
			        {
				        return GetActionsImpl( request );
			        }
	            }
            });

            // evaluate action expressions outside of cache
            DisableActionsByExpression(result.Actions, request, isFromCache);

            return result;
        }
        
        /// <summary>
        /// Get the action menu state relevant to an entity given by an id.
        /// </summary>
        /// <param name="request">The request params.</param>
        /// <returns>The action menu state.</returns>
		public ActionMenuState GetActionsMenuState( ActionRequestExtended request )
        {
            // Get the acting host for the actions
            var host = GetHost(request);
            var isNew = host.ResourceConsoleBehavior == null && host.SelectionBehavior == null;

            // Get all available actions
            var actions = ProcessActionRequest(request, host);

            // Get the behaviors that affect the menus
            var reportBehavior = GetMenuBehavior(host, h => h.ResourceConsoleBehavior, true);
            var recordBehavior = GetMenuBehavior(host, h => h.SelectionBehavior, true);

            // Access?
            if (reportBehavior.BehaviorActionMenu == null || recordBehavior.BehaviorActionMenu == null)
            {
                throw new Exception("Could not load or create action menus.");
            }

            if (!isNew)
            {
                // Deal with inherited actions having been disabled
                var suppressedIds = reportBehavior.BehaviorActionMenu.SuppressedActions.Select(a => a.Id).Union(
                    recordBehavior.BehaviorActionMenu.SuppressedActions.Select(a => a.Id)).ToList();

                // Note: exclude workflows from this as they are created/deleted rather than be suppressed and default to disabled anyway
                actions.Where(a => a.HtmlActionMethod != null && a.HtmlActionMethod != "run").ToList().ForEach(a => a.IsEnabled = !suppressedIds.Contains(a.Id));

                // Deal with actions set as buttons
                var buttonIds = reportBehavior.BehaviorActionMenu.IncludeActionsAsButtons.Select(a => a.Id).Union(
                    recordBehavior.BehaviorActionMenu.IncludeActionsAsButtons.Select(a => a.Id)).ToList();

                actions.Where(a => a.HtmlActionMethod != null && a.HtmlActionMethod != "run").ToList().ForEach(a => a.IsButton = a.IsEnabled && buttonIds.Contains(a.Id));

                // Deal with new actions
                var suppressedTypeIds = reportBehavior.BehaviorActionMenu.SuppressedTypesForNewMenu.Select(t => t.Id).Union(
                    recordBehavior.BehaviorActionMenu.SuppressedTypesForNewMenu.Select(t => t.Id)).ToList();

                // TODO: this should filter based on a.IsNew instead, but it causes the new 'API Resource Endpoint' to disappear in webapi
                actions.Where(a => a.HtmlActionMethod != null && a.HtmlActionState == CreateMenuItemActionState).ToList().ForEach(a => a.IsEnabled = !suppressedTypeIds.Contains(a.EntityId));

                // Deal with new actions on buttons
                var buttonTypeIds = reportBehavior.BehaviorActionMenu.IncludeTypesForNewButtons.Select(t => t.Id).Union(
                    recordBehavior.BehaviorActionMenu.IncludeTypesForNewButtons.Select(t => t.Id)).ToList();

                actions.Where(a => a.HtmlActionMethod != null && a.IsNew ).ToList().ForEach(a => a.IsButton = a.IsEnabled && buttonTypeIds.Contains(a.EntityId));
            }

            var menu = new ActionMenuState
            {
                ReportId = request.ReportId ?? 0,
                HostIds = request.HostResourceIds.ToList(),
                Actions = actions
            };

            return menu;
        }

        /// <summary>
        /// Get the action menu state relevant to an entity given by an id.
        /// </summary>
        /// <param name="request">The request params.</param>
        /// <returns>The action menu state.</returns>
		public ActionMenuState GetFormActionsMenuState(ActionRequestExtended request)
        {
            // Get the acting host for the actions
            var host = GetHost(request);
            
            // Get all available actions
            var actions = ProcessFormActionRequest(request, host);

            var menu = new ActionMenuState
            {
                //FormId = request.FormId ?? 0,
                //HostIds = request.HostResourceIds.ToList(),
                Actions = actions
            };

            return menu;
        }


        /// <summary>
        /// Flush the caches used by the service
        /// </summary>
        public void FlushCaches()
        {
            _getActionsCache.Clear();
        }

        #endregion

        #region Private Methods

        /// <summary>
        /// Determine whether or not we can safely do a security bypass context for most of the calculations.
        /// </summary>
        /// <param name="request"></param>
        /// <returns></returns>
        private bool NeedToSecureRequest( ActionRequestExtended request )
        {
            if ( request == null )
                return false;            
            if ( request.CellSelectedResourceId.HasValue && request.CellSelectedResourceId.Value > 0 )
                return true;
            if ( request.SelectedResourceIds != null && request.SelectedResourceIds.Length > 0 )
                return true;
            if ( request.LastSelectedResourceId.HasValue && request.LastSelectedResourceId.Value > 0 )
                return true;

            return false;
        }

        /// <summary>
        /// Gets the console actions that are relevant to the details in the request object.
        /// </summary>
        /// <param name="request">The request params.</param>
        /// <returns>Details of the relevant console actions.</returns>
        private ActionResponse GetActionsImpl( ActionRequestExtended request )
        {
            var host = GetHost( request );
            var actions = ProcessActionRequest( request, host );

            // Build response
            var response = new ActionResponse
            {
                ShowNewMenu = request.ShowNewActionsButton == true,
                ShowExportMenu = request.ShowExportActionsButton == true,
                ShowEditInlineButton = request.ShowEditInlineActionsButton == true,
                Actions = actions
            };

            return response;
        }

        /// <summary>
        /// Load up the entities to be used to retrieve the actions for.
        /// </summary>
        /// <param name="request">The actions request.</param>
        private void PreLoadEntities( ref ActionRequestExtended request )
        {
            if ( request.ReportId.HasValue && request.ReportId > 0 )
            {
                request.Report = EntityRepository.Get<Report>( request.ReportId.Value, ActionServiceHelpers.ReportRequest );
            }

            if ( request.FormId.HasValue && request.FormId > 0 && !EntityId.IsTemporary(request.FormId.Value) )
            {
                request.Form = EntityRepository.Get<CustomEditForm>(request.FormId.Value, ActionServiceHelpers.FormRequest);
            }

            if ( request.HostResourceIds != null && request.HostResourceIds.Length > 0 )
            {
                // Need to account for "generated" forms which never exist and pass temp ids around
                var actualHostResourceIds = request.HostResourceIds.Where(r => !EntityTemporaryIdAllocator.IsAllocatedId(r)).ToList();
                if (actualHostResourceIds.Count > 0)
                    request.HostResources = EntityRepository.Get<Resource>(actualHostResourceIds, ActionServiceHelpers.ResourceViewerRequest).ToList();
            }

            if ( request.SelectedResourceIds != null && request.SelectedResourceIds.Length > 0 )
            {
                if ( !request.LastSelectedResourceId.HasValue || request.LastSelectedResourceId <= 0 )
                {
                    request.LastSelectedResourceId = request.SelectedResourceIds.Last( );
                }
                request.SelectedResources = EntityRepository.Get<Resource>( request.SelectedResourceIds, ActionServiceHelpers.ResourceRequest ).ToList( );
            }

            if ( request.LastSelectedResourceId.HasValue && request.LastSelectedResourceId > 0 )
            {
                request.LastSelectedResource = EntityRepository.Get<Resource>( request.LastSelectedResourceId.Value, ActionServiceHelpers.ResourceRequest );
            }

            if ( request.FormDataEntityId.HasValue && request.FormDataEntityId > 0 && !EntityId.IsTemporary(request.FormDataEntityId.Value))
            {
                request.FormDataEntity = EntityRepository.Get<Resource>( request.FormDataEntityId.Value, ActionServiceHelpers.ResourceRequest );
            }
        }

        /// <summary>
        /// Ensure that all the structures in the request are ready for processing.
        /// </summary>
        /// <param name="request">The actions request.</param>
		private void Initialize( ref ActionRequestExtended request )
        {
            if (request.AdditionalData == null)
            {
                request.AdditionalData = new Dictionary<string, object>();
            }
            if (request.SelectedResourceTypes == null)
            {
                request.SelectedResourceTypes = new List<EntityType>();
            }
            if (request.SuppressedActionItems == null)
            {
                request.SuppressedActionItems = new List<ActionMenuItem>();
            }
            if (request.SuppressedNewActionTypes == null)
            {
                request.SuppressedNewActionTypes = new List<EntityType>();
            }
            if (request.IncludedAsButtonActionItems == null)
            {
                request.IncludedAsButtonActionItems = new List<ActionMenuItem>();
            }
            if (request.IncludedAsButtonNewActionTypes == null)
            {
                request.IncludedAsButtonNewActionTypes = new List<EntityType>();
            }
            if (request.HostResourceTypes == null)
            {
                request.HostResourceTypes = new List<EntityType>();
            }
            if (request.HostResources == null)
            {
                request.HostResources = new List<Resource>();
            }

            PreLoadEntities(ref request);
            PreLoadTypes(ref request);

            // Process custom form for additional data
            if (request.Report != null && request.Report.ResourceViewerConsoleForm != null)
            {
                request.AdditionalData["CustomForm"] = request.Report.ResourceViewerConsoleForm.Id;
                request.AdditionalData["CustomFormEditsTypeId"] = request.Report.ResourceViewerConsoleForm.TypeToEditWithForm.Id;
            }
        }

        /// <summary>
        /// Performs some post processing on the actions given the current context.
        /// </summary>
        /// <param name="actions">The list of actions.</param>
        /// <param name="ctx">The action context.</param>
        /// <returns>The finalized list</returns>
        private List<ActionMenuItemInfo> Finalize(List<ActionMenuItemInfo> actions, ActionContext ctx)
        {
            actions = FilterAndOrderActions(actions, ctx);

            // Only in the case of dropping down the action menu, add in another divider between "record" and "report" actions
            if (ctx == ActionContext.ActionsMenu)
            {
                var lastRecordAction = actions.LastOrDefault(a => a.IsContextMenu);
                if (lastRecordAction != null)
                {
                    var n = actions.IndexOf(lastRecordAction);
                    actions.Insert(n + 1, new ActionMenuItemInfo { Name = "zz_recorddivider", Order = lastRecordAction.Order, IsMenu = true, IsSeparator = true, IsSystem = true });
                }
            }

            // Strip out any unnecessary dividers
            return PostProcessSeparators(actions);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="actions"></param>
        /// <param name="ctx"></param>
        /// <returns></returns>
        private List<ActionMenuItemInfo> FilterAndOrderActions(List<ActionMenuItemInfo> actions, ActionContext ctx)
        {
            // Filter the final list based on context
            var finalActions = from a in actions
                               where ctx == ActionContext.All ||
                               (a.IsMenu && (ctx == ActionContext.ActionsMenu || ctx == ActionContext.QuickMenu)) ||
                               (a.IsContextMenu && ctx == ActionContext.ContextMenu) ||
                               (a.IsButton && ctx == ActionContext.QuickMenu)
                               select a;

            // Group for a unique list
            var distinctActions = finalActions.Where(a => a.Id > 0).GroupBy(a => new { a.Id, a.EntityId, a.HtmlActionMethod, a.HtmlActionState }).Select(a => a.First()).ToList();

            // "Special" menu items added back in
            distinctActions.AddRange(finalActions.Where(a => a.Id <= 0));

			distinctActions = distinctActions.Distinct( new ActionMenuItemInfoEqualityComparer( ) ).ToList( );

            // Process the child menus
            var ctxChildren = ctx;
            distinctActions.ForEach(d =>
            {
                if (d.Children == null)
                {
                    return;
                }
                d.Children = FilterAndOrderActions(d.Children, ctxChildren);
            });

            // Sort complete list
            return distinctActions.OrderBy(ca => ca.Order).ThenBy(ca => ca.Name).ToList();
        }

        /// <summary>
        /// Load the types to be used to retrieve the actions for.
        /// </summary>
        /// <param name="request">The actions request.</param>
		private void PreLoadTypes( ref ActionRequestExtended request )
        {
            if (request.Report != null)
            {
                // The report's "type" should be applied as the root resource type
                long reportBaseTypeId = 0;
                if (request.EntityTypeId.HasValue && request.EntityTypeId > 0)
                {
                    // Type is provided manually (as in use of template report as picker)
                    reportBaseTypeId = request.EntityTypeId.Value;
                }
                else
                {
                    var baseTypeId = GetBaseEntityTypeId(request.Report);
                    if (baseTypeId.HasValue)
                    {
                        reportBaseTypeId = baseTypeId.Value;
                    }
                }

                if ( reportBaseTypeId != 0 )
                {
                    request.ReportBaseType = EntityRepository.Get<EntityType>( reportBaseTypeId, ActionServiceHelpers.ReportBaseTypeRequest );
                }

                // temp hack : #25681: New action on 'Choice Values' tab shows a large list of choice fields.
                if (request.FormDataEntityId.HasValue &&
                    request.FormDataEntityId > 0 &&
                    request.ReportBaseType != null &&
                    request.ReportBaseType.Alias == "core:enumValue")
                {
                    request.ReportBaseType = EntityRepository.Get<EntityType>(request.FormDataEntityId.Value, ActionServiceHelpers.ReportBaseTypeRequest );
                }

                if (request.ReportBaseType != null)
                {
                    request.ReportTypes = GetInheritedEntityTypes(request.ReportBaseType);
                }
            }

            if (request.HostResources != null && request.HostResources.Count > 0)
            {
                var typeIds = request.HostResources.SelectMany(r => r.TypeIds).Distinct().ToArray();
                var types = EntityModelHelper.Get<EntityType>( typeIds );
                request.HostResourceTypes.AddRange(EntityTypeHelper.GetAllTypes(types));
            }

            if (request.HostTypeIds != null && request.HostTypeIds.Count > 0)
            {
                var existing = request.HostResourceTypes.Select(t => t.Id);
                var types = EntityModelHelper.Get<EntityType>(request.HostTypeIds.Where(h => !existing.Contains(h)).Distinct()).ToList();
                request.HostResourceTypes.AddRange(types);
            }

            if (request.LastSelectedResource != null)
            {
                var typeIds = request.LastSelectedResource.TypeIds;
                var types = EntityModelHelper.Get<EntityType>( typeIds );
                request.SelectedResourceTypes = EntityTypeHelper.GetAllTypes( types ).ToList( );
            }
            else
            {
                if (request.SelectedResources != null && request.SelectedResources.Count > 0)
                {
                    var typeIds = request.SelectedResources.SelectMany(r => r.TypeIds).Distinct().ToArray();
                    var types = EntityModelHelper.Get<EntityType>( typeIds );
                    request.SelectedResourceTypes = EntityTypeHelper.GetAllTypes( types ).ToList( );
                }
            }

            // In the case of form actions...
            if (request.Report == null && request.Form != null)
            {
                long typeToEditWithFormId = 0;

                if (request.EntityTypeId.HasValue && request.EntityTypeId > 0)
                {
                    typeToEditWithFormId = request.EntityTypeId.Value;
                }
                else
                {
                    var formType = request.Form.TypeToEditWithForm;
                    if (formType != null)
                    {
                        typeToEditWithFormId = formType.Id;
                    }
                }

                if (typeToEditWithFormId != 0)
                {
                    request.TypeToEditWithForm = EntityRepository.Get<EntityType>(typeToEditWithFormId, ActionServiceHelpers.FormToEditTypeRequest);
                }
            }
        }

        /// <summary>
        /// Based on all the involved types, this method generates an action for each that may be created.
        /// </summary>
        /// <param name="types">The applicable types.</param>
        /// <param name="actions">The current list of actions.</param>
        /// <param name="additionalData">The additional data present on the request.</param>
        private void AddCreateNewInstanceActions(IList<EntityType> types, List<ActionMenuItemInfo> actions, Dictionary<string, object> additionalData)
        {
            if (types != null && types.Count > 0)
            {
                actions.AddRange(types.Where(t => t != null).Select(t => GetNewAction(t, additionalData)));
            }
        }

        /// <summary>
        /// Based on all the involved types, this method generates an available action for workflows that
        /// take a similar type as their input parameter.
        /// </summary>
        /// <param name="types">The applicable types.</param>
        /// <param name="request"></param>
        /// <param name="actions">The current list of actions.</param>
        private void AddWorkflowRunActions(IList<EntityType> types, ActionRequest request, List<ActionMenuItemInfo> actions)
        {
            var additionalData = request.AdditionalData;

            if (types != null && types.Count > 0)
            {
                var typeIDs = types.Where(t => t != null).Select(t => t.Id).Distinct().ToSet();

                IEnumerable<Workflow> workflows = Factory.WorkflowActionsFactory.Fetch(typeIDs);

                actions.AddRange(workflows.Select(wf => new ActionMenuItemInfo
                {
                    Name = wf.Name,
                    DisplayName = wf.Name,
                    Order = 1000,
                    EntityId = wf.Id,
                    Icon = "assets/images/run.svg",
                    IsEnabled = false,
                    IsButton = false,
                    IsMenu = true,
                    IsContextMenu = true,
                    IsSystem = false,
                    AppliesToSelection = true,
                    AppliesToMultipleSelection = wf.InputArgumentForAction.IsOfType.Any(t => t.Alias == "core:resourceListArgument"),
                    HtmlActionMethod = "run",
                    HtmlActionState = wf.InputArgumentForAction.Name, // what if argument name or type changes?
                    AdditionalData = new Dictionary<string, object>(additionalData) { { ActionMenuItemInfo.AdditionalDataWorkflowKey, wf.Id } }
                }));
            }
        }

        /// <summary>
        /// Based on the type involved in the report, adds in actions for generating documents that have the involved
        /// types involved in any report templates.
        /// </summary>
        /// <param name="types">The applicable types.</param>
        /// <param name="request"></param>
        /// <param name="actions">The current list of actions.</param>
        private void AddGenerateDocumentActions(IList<EntityType> types, ActionRequest request, List<ActionMenuItemInfo> actions)
        {
            var selectedId = request.LastSelectedResourceId;
            var additionalData = request.AdditionalData;
            if (types != null && types.Count > 0)
            {
                var templates = types.Where(t => t != null)
                                     .Where(t => t.ReportTemplatesApplyToType != null && t.ReportTemplatesApplyToType.Any(r => r != null))
                                     .SelectMany(t => t.ReportTemplatesApplyToType)
                                     .OrderBy(t => t.Name)
                                     .ToList();

                actions.AddRange(templates.Select(rt => new ActionMenuItemInfo
                {
                    Name = rt.Name,
                    DisplayName = rt.Name,
                    Order = 900,
                    EntityId = selectedId ?? 0,
                    Icon = "assets/images/generate.svg",
                    IsEnabled = true, // on by default!
                    IsButton = false,
                    IsMenu = true,
                    IsContextMenu = true,
                    IsSystem = false,
                    AppliesToSelection = true,
                    AppliesToMultipleSelection = false,
                    HtmlActionMethod = "generate",
                    AdditionalData = new Dictionary<string, object>(additionalData) { { ActionMenuItemInfo.AdditionalDataReportTemplateIdKey, rt.Id } }
                }));
            }
        }

		private void AddReportActions( ActionRequestExtended request, List<ActionMenuItemInfo> actions, bool isReportHost, bool isNoHostBehavior )
		{
		    var inherit = isReportHost || isNoHostBehavior;

            var suppressActionsForType = default(bool);
            var showNewActionsButton = default(bool?);
            var showExportActionsButton = default(bool?);
		    bool? showEditInlineActionsButton = true;

            if (request.Report != null)
            {
                // Actions applied directly to the report
                var behavior = request.Report.ResourceConsoleBehavior;

                if (behavior != null && behavior.BehaviorActionMenu != null)
                {
                    if (behavior.BehaviorActionMenu.ShowNewActionsButton.HasValue)
                    {
                        showNewActionsButton = behavior.BehaviorActionMenu.ShowNewActionsButton.Value;
                    }
                }

                if (behavior != null &&
                    behavior.BehaviorActionMenu != null &&
                    behavior.BehaviorActionMenu.MenuItems != null)
                {
                    if (inherit)
                    {
                        request.SuppressedActionItems.AddRange(GetSuppressedActions(behavior));
                        request.SuppressedNewActionTypes.AddRange(GetSuppressedTypes(behavior));
                        request.IncludedAsButtonActionItems.AddRange(GetIncludedButtonActions(behavior));
                        request.IncludedAsButtonNewActionTypes.AddRange(GetIncludedButtonTypes(behavior));

                        actions.AddRange(behavior.BehaviorActionMenu.MenuItems.Select(b => b.ToInfo(request, GetTargetActionProvider(b), _targetActionReport)));
                    }
                }

                // Actions for selection applied directly to the report
                var selectBehavior = request.Report.SelectionBehavior;

                if (selectBehavior != null && selectBehavior.BehaviorActionMenu != null)
                {
                    if (selectBehavior.BehaviorActionMenu.ShowExportActionsButton.HasValue)
                    {
                        showExportActionsButton = selectBehavior.BehaviorActionMenu.ShowExportActionsButton.Value;
                    }

                    if (selectBehavior.BehaviorActionMenu.ShowEditInlineActionsButton.HasValue)
                    {
                        showEditInlineActionsButton = selectBehavior.BehaviorActionMenu.ShowEditInlineActionsButton.Value;
                    }
                }

                if (selectBehavior != null &&
                    selectBehavior.BehaviorActionMenu != null &&
                    selectBehavior.BehaviorActionMenu.MenuItems != null)
                {
                    if (inherit)
                    {
                        request.SuppressedActionItems.AddRange(GetSuppressedActions(selectBehavior));
                        request.SuppressedNewActionTypes.AddRange(GetSuppressedTypes(selectBehavior));
                        request.IncludedAsButtonActionItems.AddRange(GetIncludedButtonActions(selectBehavior));
                        request.IncludedAsButtonNewActionTypes.AddRange(GetIncludedButtonTypes(selectBehavior));

                        actions.AddRange(selectBehavior.BehaviorActionMenu.MenuItems.Select(s => s.ToInfo(request, GetTargetActionProvider(s), _targetActionSelected)));
                    }
                }

                // Continue with type processing?
                if (behavior != null)
                {
                    suppressActionsForType = behavior.SuppressActionsForType == true;
                }

                if (request.ReportTypes != null)
                {
                    if (!showNewActionsButton.HasValue)
                    {
                        request.ReportTypes.ToList().ForEach(t => SetShowNewActionsButton(t, ref showNewActionsButton));
                    }
                    if (!showExportActionsButton.HasValue)
                    {
                        request.ReportTypes.ToList().ForEach(t => SetShowExportActionsButton(t, ref showExportActionsButton));
                    }
                    if (!showEditInlineActionsButton.HasValue)
                    {
                        request.ReportTypes.ToList().ForEach(t => SetShowEditInlineActionsButton(t, ref showEditInlineActionsButton));
                    }

                    if (inherit)
                    {
                        request.SuppressedActionItems.AddRange(request.ReportTypes.SelectMany(GetSuppressedActions));
                        request.SuppressedNewActionTypes.AddRange(request.ReportTypes.SelectMany(GetSuppressedTypes));
                        request.IncludedAsButtonActionItems.AddRange(request.ReportTypes.SelectMany(GetIncludedButtonActions));
                        request.IncludedAsButtonNewActionTypes.AddRange(request.ReportTypes.SelectMany(GetIncludedButtonTypes));
                    }

                    // Actions applied directly to the report's type
                    if (!suppressActionsForType)
                    {
                        actions.AddRange(request.ReportTypes.Where(t => t.TypeConsoleBehavior != null &&
                                t.TypeConsoleBehavior.BehaviorActionMenu != null &&
                                t.TypeConsoleBehavior.BehaviorActionMenu.MenuItems != null)
                            .SelectMany(t => t.TypeConsoleBehavior.BehaviorActionMenu.MenuItems.Select(a => a.ToInfo(request, GetTargetActionProvider(a), _targetActionReport)))
                            .Where(a => a.AppliesToSelection != true && a.AppliesToMultipleSelection != true)
                            .ToList());

                        actions.AddRange(request.ReportTypes.Where(t => t.ResourceConsoleBehavior != null &&
                                t.ResourceConsoleBehavior.BehaviorActionMenu != null &&
                                t.ResourceConsoleBehavior.BehaviorActionMenu.MenuItems != null)
                            .SelectMany(t => t.ResourceConsoleBehavior.BehaviorActionMenu.MenuItems.Select(a => a.ToInfo(request, GetTargetActionProvider(a), _targetActionReport)))
                            .Where(a => a.AppliesToSelection != true && a.AppliesToMultipleSelection != true)
                            .ToList());

                        actions.AddRange(request.ReportTypes.Where(t => t.SelectionBehavior != null &&
                                t.SelectionBehavior.BehaviorActionMenu != null &&
                                t.SelectionBehavior.BehaviorActionMenu.MenuItems != null)
                            .SelectMany(t => t.SelectionBehavior.BehaviorActionMenu.MenuItems.Select(a => a.ToInfo(request, GetTargetActionProvider(a), _targetActionSelected))));
                    }
                }

                if (showNewActionsButton.HasValue)
                {
                    request.ShowNewActionsButton = showNewActionsButton.Value;
                }

                if (showExportActionsButton.HasValue)
                {
                    request.ShowExportActionsButton = showExportActionsButton.Value;
                }

                if (showEditInlineActionsButton.HasValue)
                {
                    request.ShowEditInlineActionsButton = showEditInlineActionsButton.Value;
                }
            }
        }

		private void AddHostActions( ActionRequestExtended request, List<ActionMenuItemInfo> actions, bool isReportHost, bool isNoHostBehavior )
		{
		    var inherit = isNoHostBehavior;

            var suppressActionsForType = default(bool);
            var showNewActionsButton = default(bool?);
            var showExportActionsButton = default(bool?);

            if (request.HostResources != null)
            {
                var host = request.HostResources.FirstOrDefault(h => h.ResourceConsoleBehavior != null);
                if (host != null)
                {
                    // Actions applied directly at the host control
                    var behavior = host.ResourceConsoleBehavior;

                    if (behavior != null && behavior.BehaviorActionMenu != null)
                    {
                        if (behavior.BehaviorActionMenu.ShowNewActionsButton.HasValue)
                        {
                            showNewActionsButton = behavior.BehaviorActionMenu.ShowNewActionsButton.Value;
                        }
                    }

                    if (behavior != null &&
                        behavior.BehaviorActionMenu != null &&
                        behavior.BehaviorActionMenu.MenuItems != null)
                    {
                        request.SuppressedActionItems.AddRange(GetSuppressedActions(behavior));
                        request.SuppressedNewActionTypes.AddRange(GetSuppressedTypes(behavior));
                        request.IncludedAsButtonActionItems.AddRange(GetIncludedButtonActions(behavior));
                        request.IncludedAsButtonNewActionTypes.AddRange(GetIncludedButtonTypes(behavior));

                        actions.AddRange(behavior.BehaviorActionMenu.MenuItems.Select(a => a.ToInfo(request, GetTargetActionProvider(a), _targetActionReport)));
                    }

                    // Actions for selection applied directly to the host control
                    var selectBehavior = host.SelectionBehavior;

                    if (selectBehavior != null && selectBehavior.BehaviorActionMenu != null)
                    {
                        if (selectBehavior.BehaviorActionMenu.ShowExportActionsButton.HasValue)
                        {
                            showExportActionsButton = selectBehavior.BehaviorActionMenu.ShowExportActionsButton.Value;
                        }
                    }

                    if (selectBehavior != null &&
                        selectBehavior.BehaviorActionMenu != null &&
                        selectBehavior.BehaviorActionMenu.MenuItems != null)
                    {
                        request.SuppressedActionItems.AddRange(GetSuppressedActions(selectBehavior));
                        request.SuppressedNewActionTypes.AddRange(GetSuppressedTypes(selectBehavior));
                        request.IncludedAsButtonActionItems.AddRange(GetIncludedButtonActions(selectBehavior));
                        request.IncludedAsButtonNewActionTypes.AddRange(GetIncludedButtonTypes(selectBehavior));

                        actions.AddRange(selectBehavior.BehaviorActionMenu.MenuItems.Select(s => s.ToInfo(request, GetTargetActionProvider(s), _targetActionSelected)));
                    }

                    // Continue with type processing?
                    if (behavior != null)
                    {
                        suppressActionsForType = behavior.SuppressActionsForType == true;
                    }
                }

                if (request.HostResourceTypes != null)
                {
                    if (!showNewActionsButton.HasValue)
                    {
                        request.HostResourceTypes.ToList().ForEach(t => SetShowNewActionsButton(t, ref showNewActionsButton));
                    }
                    if (!showExportActionsButton.HasValue)
                    {
                        request.HostResourceTypes.ToList().ForEach(t => SetShowExportActionsButton(t, ref showExportActionsButton));
                    }

                    if (inherit)
                    {
                        request.SuppressedActionItems.AddRange(request.HostResourceTypes.SelectMany(GetSuppressedActions));
                        request.SuppressedNewActionTypes.AddRange(request.HostResourceTypes.SelectMany(GetSuppressedTypes));
                        request.IncludedAsButtonActionItems.AddRange(request.HostResourceTypes.SelectMany(GetIncludedButtonActions));
                        request.IncludedAsButtonNewActionTypes.AddRange(request.HostResourceTypes.SelectMany(GetIncludedButtonTypes));
                    }

                    // Actions applied to the host's types
                    if (!suppressActionsForType)
                    {
                        actions.AddRange(request.HostResourceTypes.Where(t => t.TypeConsoleBehavior != null &&
                                t.TypeConsoleBehavior.BehaviorActionMenu != null &&
                                t.TypeConsoleBehavior.BehaviorActionMenu.MenuItems != null)
                            .SelectMany(t => t.TypeConsoleBehavior.BehaviorActionMenu.MenuItems.Select(a => a.ToInfo(request, GetTargetActionProvider(a), _targetActionReport)))
                            .Where(a => a.AppliesToSelection != true && a.AppliesToMultipleSelection != true)
                            .ToList());

                        actions.AddRange(request.HostResourceTypes.Where(t => t.ResourceConsoleBehavior != null &&
                                t.ResourceConsoleBehavior.BehaviorActionMenu != null &&
                                t.ResourceConsoleBehavior.BehaviorActionMenu.MenuItems != null)
                            .SelectMany(t => t.ResourceConsoleBehavior.BehaviorActionMenu.MenuItems.Select(a => a.ToInfo(request, GetTargetActionProvider(a), _targetActionReport)))
                            .Where(a => a.AppliesToSelection != true && a.AppliesToMultipleSelection != true)
                            .ToList());

                        actions.AddRange(request.HostResourceTypes.Where(t => t.SelectionBehavior != null &&
                                t.SelectionBehavior.BehaviorActionMenu != null &&
                                t.SelectionBehavior.BehaviorActionMenu.MenuItems != null)
                            .SelectMany(t => t.SelectionBehavior.BehaviorActionMenu.MenuItems.Select(a => a.ToInfo(request, GetTargetActionProvider(a), _targetActionSelected))));
                    }
                }

                if (showNewActionsButton.HasValue)
                {
                    request.ShowNewActionsButton = showNewActionsButton.Value;
                }

                if (showExportActionsButton.HasValue)
                {
                    request.ShowExportActionsButton = showExportActionsButton.Value;
                }
            }
        }

		private void AddMultiSelectActions( ActionRequestExtended request, List<ActionMenuItemInfo> actions, bool isReportHost, bool isNoHostBehavior )
        {
            var typesForActions = request.SelectedResourceTypes;

            if ((typesForActions == null || typesForActions.Count == 0) && request.ReportBaseType != null)
            {
                typesForActions.AddRange(GetInheritedEntityTypes(request.ReportBaseType));
            }

            if (typesForActions != null && typesForActions.Any(t => t != null))
            {
                if (!isReportHost && isNoHostBehavior)
                {
                    request.SuppressedActionItems.AddRange(typesForActions.SelectMany(GetSuppressedActions));
                    request.SuppressedNewActionTypes.AddRange(typesForActions.SelectMany(GetSuppressedTypes));
                    request.IncludedAsButtonActionItems.AddRange(typesForActions.SelectMany(GetIncludedButtonActions));
                    request.IncludedAsButtonNewActionTypes.AddRange(typesForActions.SelectMany(GetIncludedButtonTypes));
                }

                actions.AddRange(typesForActions.Where(t => t.TypeConsoleBehavior != null &&
                        t.TypeConsoleBehavior.BehaviorActionMenu != null &&
                        t.TypeConsoleBehavior.BehaviorActionMenu.MenuItems != null)
                    .SelectMany(t => t.TypeConsoleBehavior.BehaviorActionMenu.MenuItems.Select(a => a.ToInfo(request, GetTargetActionProvider(a), _targetActionSelected)))
                    .Where(a => a.AppliesToMultipleSelection && !a.AppliesToSelection).ToList());

                actions.AddRange(typesForActions.Where(t => t.ResourceConsoleBehavior != null &&
                        t.ResourceConsoleBehavior.BehaviorActionMenu != null &&
                        t.ResourceConsoleBehavior.BehaviorActionMenu.MenuItems != null)
                    .SelectMany(t => t.ResourceConsoleBehavior.BehaviorActionMenu.MenuItems.Select(a => a.ToInfo(request, GetTargetActionProvider(a), _targetActionReport)))
                    .Where(a => a.AppliesToMultipleSelection && !a.AppliesToSelection).ToList());

                actions.AddRange(typesForActions.Where(t => t.SelectionBehavior != null &&
                        t.SelectionBehavior.BehaviorActionMenu != null &&
                        t.SelectionBehavior.BehaviorActionMenu.MenuItems != null)
                    .SelectMany(t => t.SelectionBehavior.BehaviorActionMenu.MenuItems.Select(a => a.ToInfo(request, GetTargetActionProvider(a), _targetActionSelected)))
                    .Where(a => a.AppliesToMultipleSelection && !a.AppliesToSelection).ToList());
            }
        }

		private void AddSingleSelectActions( ActionRequestExtended request, List<ActionMenuItemInfo> actions, bool isReportHost, bool isNoHostBehavior )
        {
            var suppressActionsForType = default(bool);

            // Respect some of the exclusion directly on the selected resource (usually either Report or Form)...
            // But don't bring in the actions. This will just make right-clicking on these types in a report look odd.
            if (request.LastSelectedResource != null)
            {
                var behavior = request.LastSelectedResource.ResourceConsoleBehavior;

                request.SuppressedActionItems.AddRange(GetSuppressedActions(behavior));

                suppressActionsForType = behavior != null && behavior.SuppressActionsForType == true;
            }

            // Actions based on the types that would be selected
            if (!suppressActionsForType)
            {
                var baseType = request.ReportBaseType ?? request.TypeToEditWithForm;
                var typesForActions = request.SelectedResourceTypes;

                if ((typesForActions == null || typesForActions.Count == 0) && baseType != null)
                {
                    typesForActions.AddRange(GetInheritedEntityTypes(baseType));
                }

                if (typesForActions != null && typesForActions.Any(t => t != null))
                {
                    if (!isReportHost && isNoHostBehavior)
                    {
                        request.SuppressedActionItems.AddRange(typesForActions.SelectMany(GetSuppressedActions));
                        request.SuppressedNewActionTypes.AddRange(typesForActions.SelectMany(GetSuppressedTypes));
                        request.IncludedAsButtonActionItems.AddRange(typesForActions.SelectMany(GetIncludedButtonActions));
                        request.IncludedAsButtonNewActionTypes.AddRange(typesForActions.SelectMany(GetIncludedButtonTypes));
                    }
                    else
                    {
                        request.SuppressedActionItems.AddRange(GetSuppressedActions(baseType));
                        request.SuppressedNewActionTypes.AddRange(GetSuppressedTypes(baseType));
                        request.IncludedAsButtonActionItems.AddRange(GetIncludedButtonActions(baseType));
                        request.IncludedAsButtonNewActionTypes.AddRange(GetIncludedButtonTypes(baseType));
                    }

                    var defaultTargetGetter = request.Report == null && request.Form != null
                        ? _targetActionFormType
                        : _targetActionReportType;

                    if (request.LastSelectedResource != null)
                    {
                        defaultTargetGetter = _targetActionSelected;
                    }

                    actions.AddRange(typesForActions.Where(t => t.TypeConsoleBehavior?.BehaviorActionMenu?.MenuItems != null)
                        .SelectMany(t => t.TypeConsoleBehavior.BehaviorActionMenu.MenuItems.Select(a => a.ToInfo(request, GetTargetActionProvider(a), defaultTargetGetter)))
                        .Where(a => a.AppliesToSelection).ToList());

                    actions.AddRange(typesForActions.Where(t => t.ResourceConsoleBehavior?.BehaviorActionMenu?.MenuItems != null)
                        .SelectMany(t => t.ResourceConsoleBehavior.BehaviorActionMenu.MenuItems.Select(a => a.ToInfo(request, GetTargetActionProvider(a), defaultTargetGetter)))
                        .Where(a => a.AppliesToSelection).ToList());

                    actions.AddRange(typesForActions.Where(t => t.SelectionBehavior?.BehaviorActionMenu?.MenuItems != null)
                        .SelectMany(t => t.SelectionBehavior.BehaviorActionMenu.MenuItems.Select(a => a.ToInfo(request, GetTargetActionProvider(a), defaultTargetGetter)))
                        .Where(a => a.AppliesToSelection).ToList());
                }
            }
        }

        private void AddNewActionsSubMenu(List<ActionMenuItemInfo> actions)
        {
            var newActions = (from a in actions
                              where a.IsMenu && a.IsNew
                              select a).OrderBy(a => a.Order).ThenBy(a => a.Name).ToList();

            if (newActions.Count > 0)
            {
                if (newActions.Count > 1)
                {
                    var newSubMenuContainer = new ActionMenuItemInfo
                    {
                        Name = "New",
                        Order = 0,
                        IsMenu = true,
                        IsSystem = true,
                        Children = newActions,
                        HtmlActionState = NewHolderMenuItemActionState
                    };

                    newActions.ForEach(a => actions.Remove(a));
                    actions.Add(newSubMenuContainer);
                }
                else
                {
                    newActions[0].DisplayName = GetDisplayName("New '{0}'", newActions[0].Name); // When this is the only "new" action
                }

                actions.Add(new ActionMenuItemInfo { Name = "zz_newdivider", Order = 0, IsMenu = true, IsSeparator = true, IsSystem = true });
            }
        }

        private void AddExportActionsSubMenu(List<ActionMenuItemInfo> actions)
        {
            var exportActions = (from a in actions
                                 where a.IsMenu && a.HtmlActionMethod == "export"
                                 select a).OrderBy(a => a.Order).ThenBy(a => a.Name).ToList();

            if (exportActions.Count > 0)
            {
                if (exportActions.Count > 1)
                {
                    var exportSubMenuContainer = new ActionMenuItemInfo
                    {
                        Name = "Export to...",
                        Order = 1100,
                        IsMenu = true,
                        IsSystem = true,
                        Children = exportActions,
                        HtmlActionState = "exportHolder"
                    };

                    exportActions.ForEach(a => actions.Remove(a));
                    actions.Add(exportSubMenuContainer);
                }
                else
                {
                    exportActions[0].DisplayName = GetDisplayName("Export to {0}", exportActions[0].Name); // When this is the only "export" action
                }

                actions.Add(new ActionMenuItemInfo { Name = "zz_exportdivider", Order = 1100, IsMenu = true, IsSeparator = true, IsSystem = true });
            }
        }

        private void DisableActionsByExpression(IList<ActionMenuItemInfo> actions, ActionRequestExtended request, bool isFromCache)
        {
            if (actions.Any(a => !string.IsNullOrEmpty(a.Expression)))
            {
                // if this is from cache, will need to load the latest entities for evaluating
                if (isFromCache)
                {
                    PreLoadEntities(ref request);
                }

                var baseType = request.Report == null && request.Form != null ? request.TypeToEditWithForm : request.ReportBaseType;
                var selected = request.LastSelectedResource;
                var timeZone = request.TimeZone;

                if (baseType != null)
                {
                    var settings = new BuilderSettings
                    {
                        RootContextType = ExprTypeHelper.EntityOfType(new EntityRef(baseType.Id)),
                        ExpectedResultType = ExprType.Bool
                    };

                    var evalSettings = new EvaluationSettings
                    {
                        TimeZoneName = timeZone ?? TimeZoneHelper.SydneyTimeZoneName,
                        ContextEntity = selected != null ? new EntityRef(selected.Id).Entity : null
                    };

                    // Disable any actions, with required expressions, that error or evaluate to anything other than 'true'
                    actions.Where(a => !string.IsNullOrEmpty(a.Expression)).Select(a => a).ToList().ForEach(a =>
                    {
                        try
                        {
                            var knownEntities = a.ExpressionEntities
                                .Where(e => !string.IsNullOrEmpty(e.Key))
                                .Select(e => new
                                {
                                    e.Key,
                                    Reference = new EntityRef(e.Value.Id).Entity,
                                    Type = ExprTypeHelper.EntityOfType(new EntityRef(e.Value.TypeId))
                                }).ToList();

                            settings.ParameterNames = knownEntities.Select(e => e.Key).Distinct().ToList();
                            settings.StaticParameterResolver = paramName =>
                            {
                                return knownEntities.Where(e => e.Key == paramName).Select(e => e.Type).FirstOrDefault();
                            };

                            var expression = Factory.ExpressionCompiler.Compile(a.Expression, settings);

                            evalSettings.ParameterResolver = paramName =>
                            {
                                return knownEntities.Where(e => e.Key == paramName).Select(e => e.Reference).FirstOrDefault();
                            };

                            var result = Factory.ExpressionRunner.Run(expression, evalSettings).Value as bool?;
                            if (result.HasValue)
                            {
                                a.IsEnabled = a.IsEnabled && result.Value;
                            }
                        }
                        catch (Exception ex)
                        {
                            a.IsEnabled = false;
                            EventLog.Application.WriteError("Exception evaluating action expression. The action will be disabled. " + ex.Message);
                        }
                    });
                }
            }
        }

        private void DisableActionsBySelection(IList<ActionMenuItemInfo> actions, ActionRequestExtended request)
        {
            // Disable any actions that require selection if not met
            actions.Where(a => a.HtmlActionMethod != "custom" || a.Alias == "console:removeRelationshipAction")
                .Where(a => (!request.IsSingleSelection && a.AppliesToSelection && !a.AppliesToMultipleSelection) ||
                            (!request.IsMultipleSelection && a.AppliesToMultipleSelection && !a.AppliesToSelection) ||
                            (!request.IsSingleSelection && !request.IsMultipleSelection &&
                            (a.AppliesToSelection || a.AppliesToMultipleSelection)))
                .Select(a => a).ToList().ForEach(a => a.IsEnabled = false);
        }

        /// <summary>
        /// Based on the state in the request and the entities and types referred to
        /// builds a list of action to be presented or configured.
        /// 
        /// http://spwiki.sp.local/display/DEV/Console+Actions
        /// </summary>
        /// <param name="request">The actions request.</param>
        /// <param name="host">The host resource where any changes get saved.</param>
        /// <returns>A list of action menu items.</returns>
		private List<ActionMenuItemInfo> ProcessActionRequest( ActionRequestExtended request, Resource host )
        {
            if (request == null)
            {
                throw new ArgumentException("request");
            }

            var ctx = request.ActionDisplayContext;
            var actions = new List<ActionMenuItemInfo>();

            using (Profiler.Measure("ProcessActionRequest"))
            using (new SecurityBypassContext())
            {
                #region Initialize
                Initialize(ref request);

                var allInheritedTypes = new List<EntityType>();
                var allApplicableTypes = new List<EntityType>();
                var isForm = request.Report == null && request.Form != null;
                var isReportHost = host != null && request.Report != null && host.Id == request.Report.Id;
                var isNoHostBehavior = host == null || (host.ResourceConsoleBehavior == null && host.SelectionBehavior == null);
                var workflowActions = new List<ActionMenuItemInfo>();
                var templateActions = new List<ActionMenuItemInfo>();

                // System based override for removing ALL new actions (brute force)
                var hideAllNewActions = GetHideAllNewActions(host);

                // Go get all the types we might need to examine
                if (request.ReportBaseType != null)
                {
                    allApplicableTypes.AddRange(GetApplicableEntityTypes(request.ReportBaseType));
                    allInheritedTypes.AddRange(GetInheritedEntityTypes(request.ReportBaseType));

                    var q = from a in allApplicableTypes
                        where a.DefaultEditForm != null
                        group a by a.Id
                        into g
                        select g.OrderByDescending(t => t.Name).FirstOrDefault();

                    var typeDefaultForm =
                        new Dictionary<long, long>(q.ToDictionary(t => t.Id, t => t.DefaultEditForm.Id));
                    request.AdditionalData["TypeDefaultForm"] = typeDefaultForm;
                }
                else
                {
                    // Forms
                    if (request.TypeToEditWithForm != null)
                    {
                        allApplicableTypes.AddRange(GetApplicableEntityTypes(request.TypeToEditWithForm));
                        allInheritedTypes.AddRange(GetInheritedEntityTypes(request.TypeToEditWithForm));
                    }
                }

                // Extra custom form that comes via the edit form (not the report)
                if (host != null && host.Is<TabRelationshipRenderControl>())
                {
                    var tab = host.As<TabRelationshipRenderControl>();
                    if (tab != null && tab.ResourceViewerConsoleForm != null)
                    {
                        // (potentially) override the "customform" set on the Rerport
                        request.AdditionalData["CustomForm"] = tab.ResourceViewerConsoleForm.Id;
                        request.AdditionalData["CustomFormEditsTypeId"] = tab.ResourceViewerConsoleForm.TypeToEditWithForm.Id;
                    }
                }
                #endregion

                // Populate actions
                if (ctx == ActionContext.All || isNoHostBehavior)
                {
                    AddGenerateDocumentActions(allInheritedTypes, request, templateActions);
                }
                AddWorkflowRunActions(allInheritedTypes, request, workflowActions);
                AddSingleSelectActions(request, actions, isReportHost, isNoHostBehavior);
                if (!isForm)
                {
                    AddMultiSelectActions(request, actions, isReportHost, isNoHostBehavior);
                    AddReportActions(request, actions, isReportHost, isNoHostBehavior);
                }
                AddHostActions(request, actions, isReportHost, isNoHostBehavior);
                if (ctx != ActionContext.ContextMenu && hideAllNewActions == false && !isForm)
                {
                    // Add create actions only if not showing in right-click
                    AddCreateNewInstanceActions(allApplicableTypes, actions, request.AdditionalData);
                }

                var suppressItemIds = request.SuppressedActionItems.Select(s => s.Id).Distinct().ToSet();
                var suppressNewTypeIds = request.SuppressedNewActionTypes.Select(s => s.Id).Distinct().ToSet();
                var includedButtonIds = request.IncludedAsButtonActionItems.Select(s => s.Id).Distinct().ToSet();
                var includedButtonNewTypeIds = request.IncludedAsButtonNewActionTypes.Select(s => s.Id).Distinct().ToSet();

                // TODO: this should filter based on a.IsNew instead, but it causes the new 'API Resource Endpoint' to disappear in webapi
                var suppressByTypeActions = actions.Where(a => a.HtmlActionState == CreateMenuItemActionState && suppressNewTypeIds.Contains(a.EntityId)).ToList();

                // Update the IsButton state to reflect inclusion
                SetIncludedButtonActionItems(actions, includedButtonIds, includedButtonNewTypeIds);
                
                if (ctx != ActionContext.All)
                {
                    // Outside of edit mode / config dialog remove ALL suppressed actions
                    actions = actions.Where(a => !suppressItemIds.Contains(a.Id) && !suppressByTypeActions.Contains(a)).ToList();
                }

                if (ctx == ActionContext.All)
                {
                    SetActionItemsForConfigurationMenu(actions, request, suppressItemIds, suppressByTypeActions);

                    // Include other available workflow actions for selection
                    actions.AddRange(workflowActions.Where(wf => actions.All(a => a.EntityId != wf.EntityId)));
                }

                if (ctx == ActionContext.All || isNoHostBehavior)
                {
                    // Document generation items are 'on' by default, unless a host behavior overrides this
                    var rt = (from a in actions
                              where a.HtmlActionMethod == "generate"
                              where a.AdditionalData != null && a.AdditionalData.ContainsKey(ActionMenuItemInfo.AdditionalDataReportTemplateIdKey)
                              select (long)a.AdditionalData[ActionMenuItemInfo.AdditionalDataReportTemplateIdKey]).ToList();

                    actions.AddRange(templateActions.Where(t => !rt.Contains((long)t.AdditionalData[ActionMenuItemInfo.AdditionalDataReportTemplateIdKey])));
                }

                if (ctx == ActionContext.ContextMenu)
                {
                    if (request.IsMultipleSelection)
                    {
                        // Remove any single-select items
                        actions = actions.Where(a => a.AppliesToMultipleSelection || !a.AppliesToSelection).ToList();
                    }
                }

                if (ctx == ActionContext.ContextMenu || ctx == ActionContext.ActionsMenu)
                {
                    DisableActionsBySelection(actions, request);
                }
                
                if (ctx == ActionContext.ActionsMenu)
                {
                    // Pull the "new" actions out into their own submenu, if more than one
                    AddNewActionsSubMenu(actions);

                    // Pull the "export" actions into a sub menu
                    AddExportActionsSubMenu(actions);
                }
            }

            // Remove menu items the user cannot see/use
            SecurityBypassContext.RunAsUser(() =>
            {
                SecurityFilter.Filter(request.FormDataEntityId ?? 0, request.SelectedResourceIds ?? new long[0], actions);
            });

            FilterForFeatureSwitches(actions);

            using (new SecurityBypassContext())
            {
                return Finalize(actions, ctx);
            }
        }

        
        /// <summary>
        /// Supression actions if certain switches are not tuened on
        /// </summary>
        
        private void FilterForFeatureSwitches(List<ActionMenuItemInfo> actions)
        {
            if (!Factory.FeatureSwitch.Get("ftpExport"))
            {
                actions.RemoveAll(a => a.DisplayName == "Scheduled Export Configuration");
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="actions"></param>
        /// <param name="request"></param>
        /// <param name="suppressItemIds"></param>
        /// <param name="suppressByTypeActions"></param>
        private static void SetActionItemsForConfigurationMenu(List<ActionMenuItemInfo> actions, ActionRequestExtended request, ISet<long> suppressItemIds, List<ActionMenuItemInfo> suppressByTypeActions)
        {
            // When configuring action items show the verbose names
            actions.Where(a => a.IsNew).ToList().ForEach(a =>
            {
                a.DisplayName = GetDisplayName("New '{0}'", a.Name);
            });

            actions.Where(a => a.HtmlActionMethod == "export").ToList().ForEach(a =>
            {
                a.DisplayName = GetDisplayName("Export to {0}", a.Name);
            });

            // Add actions to represent the "special" menus for new and export menus outside the action menu
            actions.Add(new ActionMenuItemInfo
            {
                Id = -1,
                Order = -1,
                EntityId = -1,
                Name = "New (All)",
                DisplayName = "New (All)",
                Icon = "assets/images/icon_new.png",
                IsEnabled = true,
                IsContextMenu = true,
                IsSystem = true,
                IsButton = request.ShowNewActionsButton == true
            });

            actions.Add(new ActionMenuItemInfo
            {
                Id = -2,
                Order = -1,
                EntityId = -1,
                Name = "Export (All)",
                DisplayName = "Export (All)",
                Icon = "assets/images/16x16/export.png",
                IsEnabled = true,
                IsContextMenu = false,
                IsSystem = true,
                IsButton = request.ShowExportActionsButton == true
            });

            actions.Add(new ActionMenuItemInfo
            {
                Id = -3,
                Order = -2,
                EntityId = -1,
                Name = "Edit Inline",
                DisplayName = "Edit Inline",
                Icon = "assets/images/16x16/edit.svg",
                IsEnabled = true,
                IsContextMenu = false,
                IsSystem = true,
                IsButton = request.ShowEditInlineActionsButton == true
            });

            // Update the IsEnabled to reflect suppression
            actions.Where(a => suppressItemIds.Contains(a.Id) || suppressByTypeActions.Contains(a))
                .ToList()
                .ForEach(a => a.IsEnabled = false);
        }

        /// <summary>
        /// Based on the state in the request and the entities and types referred to
        /// builds a list of action to be presented or configured.
        /// </summary>
        /// <param name="request"></param>
        /// <param name="host"></param>
        /// <returns></returns>
        private List<ActionMenuItemInfo> ProcessFormActionRequest(ActionRequestExtended request, Resource host)
        {
            if (request == null)
            {
                throw new ArgumentException("request");
            }

            var ctx = request.ActionDisplayContext;
            var actions = new List<ActionMenuItemInfo>();

            using (Profiler.Measure("ProcessActionRequest"))
            using (new SecurityBypassContext())
            {
                #region Initialize
                Initialize(ref request);

                var allInheritedTypes = new List<EntityType>();
                var allApplicableTypes = new List<EntityType>();
                //var isReportHost =false;
                var isNoHostBehavior = host == null || (host.ResourceConsoleBehavior == null && host.SelectionBehavior == null);
                var workflowActions = new List<ActionMenuItemInfo>();
                var templateActions = new List<ActionMenuItemInfo>();

                // Hold the types applicable to the form
                var TypeToEditWithForm = request.TypeToEditWithForm;
                if (TypeToEditWithForm != null)
                {
                    if (TypeToEditWithForm.Alias != "core:resource")
                    {
                        allApplicableTypes.AddRange(GetAllDerivedTypes(TypeToEditWithForm));
                        if (TypeToEditWithForm.IsAbstract != true &&
                            allApplicableTypes.All(t => t.Id != TypeToEditWithForm.Id))
                        {
                            allApplicableTypes.Add(TypeToEditWithForm);
                        }
                    }
                    
                    allInheritedTypes.AddRange(GetInheritedEntityTypes(TypeToEditWithForm));
                }
                
                #endregion

                // Populate actions (only workflow and document generation)
                if (ctx == ActionContext.All || isNoHostBehavior)
                {
                    AddGenerateDocumentActions(allInheritedTypes, request, templateActions);
                }
                AddWorkflowRunActions(allInheritedTypes, request, workflowActions);


                // actions related to custom edit form
                // Actions applied directly at the host control i.e form
                var behavior = host?.ResourceConsoleBehavior;
                if (behavior?.BehaviorActionMenu?.MenuItems != null)
                {
                    request.SuppressedActionItems.AddRange(GetSuppressedActions(behavior));
                    request.IncludedAsButtonActionItems.AddRange(GetIncludedButtonActions(behavior));

                    actions.AddRange(behavior.BehaviorActionMenu.MenuItems.Select(a => a.ToInfo(request, GetTargetActionProvider(a), _targetActionReport)));
                }

                var suppressItemIds = request.SuppressedActionItems.Select(s => s.Id).Distinct().ToSet();
                var suppressNewTypeIds = request.SuppressedNewActionTypes.Select(s => s.Id).Distinct().ToSet();
                var includedButtonIds = request.IncludedAsButtonActionItems.Select(s => s.Id).Distinct().ToSet();
                var includedButtonNewTypeIds = request.IncludedAsButtonNewActionTypes.Select(s => s.Id).Distinct().ToSet();
                var suppressByTypeActions = actions.Where(a => a.HtmlActionState == CreateMenuItemActionState && suppressNewTypeIds.Contains(a.EntityId)).ToList();

                // Update the IsButton state to reflect inclusion
                var included =
                    actions.Where(
                        a =>
                            includedButtonIds.Contains(a.Id) ||
                            (a.HtmlActionState == CreateMenuItemActionState && (includedButtonNewTypeIds.Contains(a.EntityId))))
                        .ToList();
                included.ForEach(a => a.IsButton = true);

                if (ctx != ActionContext.All)
                {
                    // Outside of edit mode / config dialog remove ALL suppressed actions
                    actions =
                        actions.Where(a => !suppressItemIds.Contains(a.Id) && !suppressByTypeActions.Contains(a))
                            .ToList();
                }

                if (ctx == ActionContext.All)
                {
                    // Update the IsEnabled to reflect suppression
                    actions.Where(a => suppressItemIds.Contains(a.Id) || suppressByTypeActions.Contains(a))
                        .ToList()
                        .ForEach(a => a.IsEnabled = false);

                    // Include other available workflow actions for selection
                    actions.AddRange(workflowActions.Where(wf => actions.All(a => a.EntityId != wf.EntityId)));
                }

                if (ctx == ActionContext.All || isNoHostBehavior)
                {
                    // Document generation items are 'on' by default, unless a host behavior overrides this
                    var rt = (from a in actions
                              where a.HtmlActionMethod == "generate"
                              where a.AdditionalData != null && a.AdditionalData.ContainsKey(ActionMenuItemInfo.AdditionalDataReportTemplateIdKey)
                              select (long)a.AdditionalData[ActionMenuItemInfo.AdditionalDataReportTemplateIdKey]).ToList();

                    actions.AddRange(templateActions.Where(t => !rt.Contains((long)t.AdditionalData[ActionMenuItemInfo.AdditionalDataReportTemplateIdKey])));
                }
            }
            
            // Remove menu items the user cannot see/use
            SecurityBypassContext.RunAsUser(() =>
            {
                SecurityFilter.Filter(request.FormDataEntityId ?? 0, request.SelectedResourceIds ?? new long[0], actions);
            });

            using (new SecurityBypassContext())
            {
                return Finalize(actions, ctx);
            }
        }
        /// <summary>
        /// Filters out unnecessary dividers from between the action menu items.
        /// </summary>
        /// <param name="actions">The action list.</param>
        /// <returns>The filtered action list.</returns>
        private static List<ActionMenuItemInfo> PostProcessSeparators(List<ActionMenuItemInfo> actions)
        {
            var result = new List<ActionMenuItemInfo>();

            if (actions == null)
                return result;

            // Remove leading/trailing separators
            while (actions.Count > 0 && actions[0].IsSeparator)
            {
                actions.RemoveAt(0);
            }

            while (actions.Count > 0 && actions[actions.Count - 1].IsSeparator)
            {
                actions.RemoveAt(actions.Count - 1);
            }

            var isSeparator = false;

            // Remove contiguous dividers
            foreach (var action in actions)
            {
                if (isSeparator && action.IsSeparator)
                {
                    continue;
                }

                isSeparator = action.IsSeparator;
                result.Add(action);
            }

            return result;
        }
        
        /// <summary>
        /// Gets an action menu item that is set to create the given type when selected.
        /// </summary>
        /// <param name="t">The entity type.</param>
        /// <param name="additionalData">Additional data dictionary passed along on the request.</param>
        /// <returns></returns>
        private static ActionMenuItemInfo GetNewAction(EntityType t, Dictionary<string, object> additionalData)
        {
            if (t == null)
            {
                throw new ArgumentNullException("t");
            }

            var actionMenuItem = new ActionMenuItemInfo
            {
                Name = t.Name,
                DisplayName = t.Name,
                Order = 0,
                EntityId = t.Id,
                Icon = "assets/images/icon_new.png",
                IsMenu = true,
                IsSystem = true,
                IsNew = true,
                HtmlActionMethod = "navigate",
                HtmlActionState = CreateMenuItemActionState,
                AdditionalData = additionalData
            };

            ConsoleBehavior consoleBehavior = t.TypeConsoleBehavior;
            if ( consoleBehavior != null && consoleBehavior.Html5CreateId != null )
            {
                actionMenuItem.HtmlActionState = consoleBehavior.Html5CreateId;
            }

            // Override certain special system types to align with the console
            switch (t.Alias)
            {
                case "core:definition":
                    actionMenuItem.Icon = "assets/images/16x16/NewObject.png";
                    actionMenuItem.AdditionalData.Add("Dialog", "newDefinition");
                    actionMenuItem.Name = "Object";
                    actionMenuItem.DisplayName = "Object";
                    break;

                case "core:report":
                    actionMenuItem.Icon = "assets/images/16x16/NewReport.png";
                    actionMenuItem.AdditionalData.Add("Dialog", "newReport");
                    break;

                case "core:chart":
                    actionMenuItem.Icon = "assets/images/16x16/chart_new.png";
                    actionMenuItem.AdditionalData.Add("Dialog", "newChart");
                    break;

                case "console:screen":
                    actionMenuItem.Icon = "assets/images/16x16/screen_new.png";
                    actionMenuItem.AdditionalData.Add("Dialog", "newScreen");
                    break;
                case "core:solution":
                    actionMenuItem.Icon = "assets/images/16x16/New.png";
                    actionMenuItem.AdditionalData.Add("Dialog", "newApplication");
                    break;
            }

            return actionMenuItem;
        }
                
        /// <summary>
        /// Given a name, returns a function that knows how to retrieve target information important
        /// to an action item.
        /// </summary>
        /// <param name="item">The action menu item.</param>
        /// <returns>The target provider function.</returns>
		private Func<ActionRequestExtended, ActionMenuItem, ActionTargetInfo> GetTargetActionProvider( ActionMenuItem item )
        {
			Func<ActionRequestExtended, ActionMenuItem, ActionTargetInfo> fn = null;
            var targetType = item.HtmlActionTarget;
            if (!string.IsNullOrEmpty(targetType))
            {
                switch (targetType)
                {
                    case "selected":
                        fn = _targetActionSelected;
                        break;
                    case "report":
                        fn = _targetActionReport;
                        break;
                    case "form":
                        fn = _targetActionForm;
                        break;
                    case "reportbasetype":
                        fn = _targetActionReportType;
                        break;
                    case "formbasetype":
                        fn = _targetActionFormType;
                        break;
                    case "formentitydata":
                        fn = _targetFormEntityData;
                        break;
                }
            }
            else
            {
                if (item.Is<WorkflowActionMenuItem>())
                {
                    fn = _targetActionWorkflow;
                }
                if (item.Is<GenerateDocumentActionMenuItem>())
                {
                    fn = _targetActionGenerateDocument;
                }
            }

            return fn;
        }

        /// <summary>
        /// Finds the identifier for the base entity in the query of a report.
        /// </summary>
        /// <param name="report">The report.</param>
        /// <returns>The id of the base entity's type.</returns>
        private static long? GetBaseEntityTypeId(Report report)
        {
            long? baseEntityTypeId = null;

            if (report != null)
            {
                var root = report.RootNode;
                if (root != null)
                {
                    if (root.Is<ResourceReportNode>())
                    {
                        baseEntityTypeId = GetBaseEntityTypeId(root);
                    }
                    else if (root.Is<AggregateReportNode>())
                    {
                        var aggregateNode = root.As<AggregateReportNode>();
                        baseEntityTypeId = GetBaseEntityTypeId(aggregateNode.GroupedNode);
                    }
                }
            }

            return baseEntityTypeId;
        }

        private static long? GetBaseEntityTypeId(ReportNode node)
        {
            long? typeId = null;

            var resourceNode = node.As<ResourceReportNode>();
            if (resourceNode != null)
            {
                var baseEntityType = resourceNode.ResourceReportNodeType;
                if (baseEntityType != null)
                {
                    typeId = baseEntityType.Id;
                }
            }

            return typeId;
        }

        /// <summary>
        /// Gets the base entity type for a particular query.
        /// </summary>
        /// <param name="entity">The query entity.</param>
        /// <returns>The reference to the type info.</returns>
        private static EntityRef GetBaseEntityType(QueryEntity entity)
        {
            EntityRef baseEntityTypeId = null;

            if (entity != null)
            {
                if (entity.GetType() == typeof(ResourceEntity))
                {
                    var resourceEntity = entity as ResourceEntity;
                    if (resourceEntity != null)
                    {
                        baseEntityTypeId = resourceEntity.EntityTypeId;
                    }
                }
                else if (entity.GetType() == typeof(RelatedResource))
                {
                    var relatedResource = entity as RelatedResource;
                    if (relatedResource != null)
                    {
                        baseEntityTypeId = relatedResource.EntityTypeId;
                    }
                }
                else if (entity.GetType() == typeof(AggregateEntity))
                {
                    var aggregateEntity = entity as AggregateEntity;
                    if (aggregateEntity != null && aggregateEntity.GroupedEntity != null)
                    {
                        baseEntityTypeId = GetBaseEntityType(aggregateEntity.GroupedEntity);
                    }
                }
            }

            return baseEntityTypeId;
        }

        /// <summary>
        /// Gets all the inherited types of a particular entity type.
        /// </summary>
        /// <param name="type">The type to check.</param>
        /// <returns>The list of inherited types.</returns>
        private static List<EntityType> GetInheritedEntityTypes(EntityType type)
        {
            return type.GetAncestorsAndSelf( ).ToList( );
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="type"></param>
        /// <returns></returns>
        private static List<EntityType> GetApplicableEntityTypes(EntityType type)
        {
            var types = new List<EntityType>();

            // skip if the type is base resource. it's too all encompassing.
            if (type.Alias != "core:resource")
            {
                types.AddRange(GetAllDerivedTypes(type));
                if (type.IsAbstract != true && types.All(t => t.Id != type.Id))
                {
                    types.Add(type);
                }
            }

            return types;
        }

        /// <summary>
        /// Gets the host resource to apply any configuration changes for actions to.
        /// </summary>
        /// <param name="request">The action request object.</param>
        /// <returns>The host resource.</returns>
        private Resource GetHost(ActionRequest request)
        {
            Resource host = null;

            if (request != null && request.HostResourceIds != null)
            {
                var hostId = request.HostResourceIds.FirstOrDefault();
                if (hostId > 0)
                {
                    host = EntityModelHelper.Get<Resource>(hostId);
                }
                else
                {
                    if (request.ReportId.HasValue && request.ReportId > 0)
                    {
                        host = EntityModelHelper.Get<Resource>(request.ReportId.Value);
                    }
                }
            }

            return host;
        }

        /// <summary>
        /// Gets a behavior related to a host resource, specified by a getter call and optionally editable.
        /// </summary>
        /// <param name="host">The host resource.</param>
        /// <param name="getBehavior">The getter to pull the appropriate behavior.</param>
        /// <param name="isEditable">Retrieve the behavior as editable.</param>
        /// <returns>The behavior.</returns>
        private ConsoleBehavior GetMenuBehavior(Resource host, Func<Resource, ConsoleBehavior> getBehavior, bool isEditable = false)
        {
            ConsoleBehavior behavior = null;

            if (host != null)
            {
                behavior = getBehavior(host) ?? EntityModelHelper.Create<ConsoleBehavior>();
                behavior = isEditable ? behavior.AsWritable<ConsoleBehavior>() : behavior;
                if (behavior.BehaviorActionMenu == null)
                {
                    if (isEditable && behavior.IsReadOnly)
                    {
                        return behavior;
                    }
                    behavior.BehaviorActionMenu = EntityModelHelper.Create<ActionMenu>();
                }
            }

            return behavior;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="behavior"></param>
        /// <returns></returns>
        private static IEnumerable<ActionMenuItem> GetSuppressedActions(ConsoleBehavior behavior)
        {
            var actions = new List<ActionMenuItem>();

            if (behavior != null)
            {
                if (behavior.BehaviorActionMenu != null)
                {
                    actions.AddRange(behavior.BehaviorActionMenu.SuppressedActions);
                }
            }

            return actions;
        }

        /// <summary>
        /// Gets any action menu items that have been suppressed on the menu or behaviors for a host resource.
        /// </summary>
        /// <param name="host">The host resource.</param>
        /// <returns>A list of suppressed action menu items.</returns>
        private static IEnumerable<ActionMenuItem> GetSuppressedActions(Resource host)
        {
            var actions = new List<ActionMenuItem>();

            if (host != null)
            {
                actions.AddRange(GetSuppressedActions(host.ResourceConsoleBehavior));
                actions.AddRange(GetSuppressedActions(host.SelectionBehavior));
            }

            return actions;
        }

        /// <summary>
        /// Gets any action menu items that have been suppressed on the menu or behaviors for a type.
        /// </summary>
        /// <param name="type">The type.</param>
        /// <returns>A list of suppressed action menu items.</returns>
        private static IEnumerable<ActionMenuItem> GetSuppressedActions(EntityType type)
        {
            var actions = new List<ActionMenuItem>();

            if (type != null)
            {
                actions.AddRange(GetSuppressedActions(type.TypeConsoleBehavior));
                actions.AddRange(GetSuppressedActions(type.ResourceConsoleBehavior));
                actions.AddRange(GetSuppressedActions(type.SelectionBehavior));
            }

            return actions;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="behavior"></param>
        /// <returns></returns>
        private static IEnumerable<EntityType> GetSuppressedTypes(ConsoleBehavior behavior)
        {
            var types = new List<EntityType>();

            if (behavior != null && behavior.BehaviorActionMenu != null)
            {
                types.AddRange(behavior.BehaviorActionMenu.SuppressedTypesForNewMenu.Where(s => s != null));
            }

            return types;
        }

        /// <summary>
        /// Gets any entity types that are suppressing "new" actions on a menu or behavior for a host resource.
        /// </summary>
        /// <param name="host">The host resource.</param>
        /// <returns>A list of suppressed entity types.</returns>
        private static IEnumerable<EntityType> GetSuppressedTypes(Resource host)
        {
            var types = new List<EntityType>();

            if (host != null)
            {
                types.AddRange(GetSuppressedTypes(host.ResourceConsoleBehavior));
                types.AddRange(GetSuppressedTypes(host.SelectionBehavior));
            }

            return types;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="type"></param>
        /// <returns></returns>
        private static IEnumerable<EntityType> GetSuppressedTypes(EntityType type)
        {
            var types = new List<EntityType>();

            if (type != null)
            {
                types.AddRange(GetSuppressedTypes(type.TypeConsoleBehavior));
                types.AddRange(GetSuppressedTypes(type.ResourceConsoleBehavior));
                types.AddRange(GetSuppressedTypes(type.SelectionBehavior));
            }

            return types;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="behavior"></param>
        /// <returns></returns>
        private static IEnumerable<ActionMenuItem> GetIncludedButtonActions(ConsoleBehavior behavior)
        {
            var actions = new List<ActionMenuItem>();

            if (behavior != null && behavior.BehaviorActionMenu != null)
            {
                actions.AddRange(behavior.BehaviorActionMenu.IncludeActionsAsButtons);
            }

            return actions;
        }

        /// <summary>
        /// Gets any action menu items that have been set to show as buttons on the menu or behaviors of a host resource.
        /// </summary>
        /// <param name="host">The host resource.</param>
        /// <returns>A list of included actions.</returns>
        private static IEnumerable<ActionMenuItem> GetIncludedButtonActions(Resource host)
        {
            var actions = new List<ActionMenuItem>();

            if (host != null)
            {
                actions.AddRange(GetIncludedButtonActions(host.ResourceConsoleBehavior));
                actions.AddRange(GetIncludedButtonActions(host.SelectionBehavior));
            }

            return actions;
        }

        /// <summary>
        /// Gets any action menu items that have been set to show as buttons on the menu or behaviors for a type.
        /// </summary>
        /// <param name="type">The type.</param>
        /// <returns>A list of included actions.</returns>
        private static IEnumerable<ActionMenuItem> GetIncludedButtonActions(EntityType type)
        {
            var actions = new List<ActionMenuItem>();

            if (type != null)
            {
                actions.AddRange(GetIncludedButtonActions(type.TypeConsoleBehavior));
                actions.AddRange(GetIncludedButtonActions(type.ResourceConsoleBehavior));
                actions.AddRange(GetIncludedButtonActions(type.SelectionBehavior));
            }

            return actions;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="behavior"></param>
        /// <returns></returns>
        private static IEnumerable<EntityType> GetIncludedButtonTypes(ConsoleBehavior behavior)
        {
            var types = new List<EntityType>();

            if (behavior != null && behavior.BehaviorActionMenu != null)
            {
                types.AddRange(behavior.BehaviorActionMenu.IncludeTypesForNewButtons);
            }

            return types;
        }

        /// <summary>
        /// Gets any entity types that have their "new" actions included as buttons on a menu or behavior for a host resource.
        /// </summary>
        /// <param name="host">The host resource.</param>
        /// <returns>A list of included entity types.</returns>
        private static IEnumerable<EntityType> GetIncludedButtonTypes(Resource host)
        {
            var types = new List<EntityType>();

            if (host != null)
            {
                types.AddRange(GetIncludedButtonTypes(host.ResourceConsoleBehavior));
                types.AddRange(GetIncludedButtonTypes(host.SelectionBehavior));
            }

            return types;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="type"></param>
        /// <returns></returns>
        private static IEnumerable<EntityType> GetIncludedButtonTypes(EntityType type)
        {
            var types = new List<EntityType>();

            if (type != null)
            {
                types.AddRange(GetIncludedButtonTypes(type.TypeConsoleBehavior));
                types.AddRange(GetIncludedButtonTypes(type.ResourceConsoleBehavior));
                types.AddRange(GetIncludedButtonTypes(type.SelectionBehavior));
            }

            return types;
        }

        /// <summary>
        /// Determines if the host menu has been configured to hide ALL new/creation based actions.
        /// </summary>
        /// <param name="host"></param>
        /// <returns></returns>
        private static bool GetHideAllNewActions(Resource host)
        {
            // Only permit when hosted. Not on type.
            return ((host != null) &&
                    (host.ResourceConsoleBehavior != null) &&
                    (host.ResourceConsoleBehavior.BehaviorActionMenu != null) &&
                    (host.ResourceConsoleBehavior.BehaviorActionMenu.SuppressNewActions == true));
        }

        /// <summary>
        /// Checks the behaviors associated with any types to check if the "New" button should be shown on the quick menu.
        /// </summary>
        /// <param name="type">The type to check.</param>
        /// <param name="show">The value of whether to show.</param>
        private static void SetShowNewActionsButton(EntityType type, ref bool? show)
        {
            if (type.TypeConsoleBehavior != null &&
                type.TypeConsoleBehavior.BehaviorActionMenu != null &&
                type.TypeConsoleBehavior.BehaviorActionMenu.ShowNewActionsButton == true)
            {
                show = true;
            }
        }

        /// <summary>
        /// Checks the behaviors associated with any types to check if the "Export" button should be shown on the toolbar.
        /// </summary>
        /// <param name="type">The type to check.</param>
        /// <param name="show">The value of whether to show.</param>
        private static void SetShowExportActionsButton(EntityType type, ref bool? show)
        {
            if (type.TypeConsoleBehavior != null &&
                type.TypeConsoleBehavior.BehaviorActionMenu != null &&
                type.TypeConsoleBehavior.BehaviorActionMenu.ShowExportActionsButton == true)
            {
                show = true;
            }
        }

        /// <summary>
        /// Checks the behaviors associated with any types to check if the "Edit Inline" button should be shown on the toolbar.
        /// </summary>
        /// <param name="type">The type to check.</param>
        /// <param name="show">The value of whether to show.</param>
        private static void SetShowEditInlineActionsButton(EntityType type, ref bool? show)
        {
            if (type.TypeConsoleBehavior != null &&
                type.TypeConsoleBehavior.BehaviorActionMenu != null &&
                type.TypeConsoleBehavior.BehaviorActionMenu.ShowEditInlineActionsButton == true)
            {
                show = true;
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="actions"></param>
        /// <param name="includedButtonIds"></param>
        /// <param name="includedButtonNewTypeIds"></param>
        private static void SetIncludedButtonActionItems(List<ActionMenuItemInfo> actions, ISet<long> includedButtonIds, ISet<long> includedButtonNewTypeIds)
        {
            var included = actions.Where(a => includedButtonIds.Contains(a.Id) || (a.IsNew && includedButtonNewTypeIds.Contains(a.EntityId))).ToList();
            included.ForEach(a => a.IsButton = true);
        }

        /// <summary>
        /// Gets all the derived types that exist from a given type.
        /// </summary>
        /// <param name="type">The base type.</param>
        /// <returns>The derived types.</returns>
        private static IList<EntityType> GetAllDerivedTypes(EntityType type)
        {
            var res = type.GetDescendantsAndSelf( ).Where( t => t.IsAbstract != true ).ToList( );
            return res;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="template"></param>
        /// <param name="value"></param>
        /// <returns></returns>
        private static string GetDisplayName(string template, string value)
        {
            var valueString = value ?? "";
            if (string.IsNullOrEmpty(template))
            {
                return valueString;
            }
            return string.Format(template, valueString.Replace("'", "&#39;"));
        }
        #endregion
    }
}
