// Copyright 2011-2016 Global Software Innovation Pty Ltd
/*global _, console, angular, $, sp, spEntity */

(function () {
    'use strict';

    /**
     * Module implementing a report control.
     *
     * @module spReport
     * @example

     Using the sp-report:

     &lt;sp-report options="options" host-id="<hostId>" &gt;&lt;/sp-report&gt

     where options is available on the controller with the following properties:
     - isEditMode - {boolean}. True if the report is in edit mode false otherwise.

     - reportId - {number}. The entity of the report id to display.

     - multiSelect - {boolean}. True to select many items, false otherwise.

     - reportModel - {object}. An existing report model. This is used to initialise the report directive from a prior
     state. e.g. when navigating back from a child edit form.
     If both the reportModel and reportId are specified, reportModel is favoured.

     - isMobile - are we using a restricted mobile phone presentation

     - externalConds - additional conditions enforced on report (eg when binding to chart)

     - quickSearch - if there is a external quickSearch control, pass an empty object to both it and this.

     This control emits the following events:
     - spReportEventModelReady - This event is raised when the report model is ready.
     - spDataGridEventGridDoubleClicked - This event is raised when the grid is double clicked.
     - spDataGridEventSelectionChanged - This event is raised when a grid row is selected.

     hostId is some identifier of the container of the report. Used to assist with
     keying the component state for the report component. Is optional.

     *
     */

    angular.module('mod.common.ui.spReport').directive('spReport', spReport);

    /* @ngInject */
    function spReport(spMobileContext, spThemeService) {
        return {
            restrict: 'E',

            // TODO: This should use the options.IsMobile
            templateUrl: spMobileContext.isMobile ? 'reports/spReportMobile.tpl.html' : 'reports/spReport.tpl.html',

            replace: true,
            controller: ReportController,
            scope: {
                options: '=',
                hostId: '<?'
            },
            link: function (scope) {

                scope.isReady = true;

                scope.isTablet = spMobileContext.isTablet;

                // maybe this should be done in the controller, but a little confused over the use
                // of the controller versus just doing it here and in a service
                scope.$watch('model.moreDataAvailable', function (value) {
                    scope.options.moreDataAvailable = value;
                });

                scope.getTitleTheme = function () {
                    return spThemeService.getTitleStyle();
                };
            }
        };
    }

    /* @ngInject */
    function ReportController($scope, $rootScope, $element, $stateParams, spDataGridUtils, spSortOptionsDialog,
                              spReportModelManager, spConditionalFormattingDialog, spReportMetadataManager,
                              spContextMenuService, spActionsService, condFormattingConstants, spReportActionsDialog,
                              spTenantSettings, $timeout, $parse, $q, reportBuilderService, spAggregateOptionsDialog,
                              spAlertsService, spTypeOperatorService, spNavService, spMobileContext, spAppSettings,
                              spInlineEditService, spEntityService, rnFeatureSwitch, spState, $window) {

        var reportModelManager = spReportModelManager(null);
        var selectLastSelectedItems;
        var isMobile = spMobileContext.isMobile;
        var lastRequestTime = 0;
        var requestStarted = false;
        var getReportActionQuickMenuDebounce = _.debounce(getReportActionQuickMenu, 100);
        var updateActionButtonsEnabledDebounce = _.debounce(updateActionButtonsEnabled, 100);
        var cancelInlineEditsPending = false;
        let canInlineEdit = false;

        // this is the interface used by dependent components & directives
        this.getResourceEditingState = getResourceEditingState;
        this.getFormControl = getFormControl;
        this.getSelectedRowIndex = getSelectedRowIndex;
        this.getMetadata = getMetadata;
        this.focusInlineEditCellElement = focusInlineEditCellElement;
        this.getInlineEditingState = getInlineEditingState;

        ///////////////////////////////////////////////////////////////////////////
        // Setup all the bindables

        // This is the only expected scope property due to this controller
        // being used in a directive that has an isolate scope with options.
        $scope.options = $scope.options || {};
        $scope.model = reportModelManager.createModel($scope.options.quickSearch);

        // Aliases for lean and fast access to the key data structures
        const {model} = $scope;
        const {gridOptions} = model;

        $scope.inlineEditingFeatureIsOn = isMobile ? rnFeatureSwitch.isFeatureOn('inlineEditingOnMobile') : true;        
        $scope.dropOptions = {onAllowDrop, onDragOver, onDragLeave, onDrop};

        $scope.getActionExecutionContext = defaultGetActionExecutionContext;
        $scope.onContextMenuAction = onContextMenuAction;
        $scope.isContextMenuActionDisabled = isContextMenuActionDisabled;
        $scope.isContextMenuActionHidden = isContextMenuActionHidden;
        $scope.getActionMenuItems = getActionMenuItems;
        $scope.showActionsDialog = showActionsDialog;
        $scope.executeNewAction = executeNewAction;
        $scope.pullDownRefresh = pullDownRefresh;
        $scope.loadMore = loadMore;
        $scope.onAnalyzerButtonClicked = onAnalyzerButtonClicked;
        $scope.onAggregateContextMenuAction = onAggregateContextMenuAction;
        $scope.isAggregateContextMenuActionHidden = isAggregateContextMenuActionHidden;

        $scope.showEditActionsButton = () => (model.isEditMode || model.isInDesign) && !model.isInPicker && model.userCanEditActions && !model.isInlineEditing;
        $scope.showActionsMenu = () => !(model.isEditMode || model.isInDesign || model.isInPicker);
        $scope.disabledActions = () => model.disableActions || !model.reportId || model.isInlineEditing;

        $scope.showNewToolMenu = () => model.showNewMenu && !model.isInPicker && model.newMenu.menuItems.length > 1;
        $scope.showNewToolButton = () => model.showNewMenu && !model.isInPicker && model.newMenu.menuItems.length === 1;
        $scope.disabledNewTool = () => model.isEditMode || model.isInDesign || model.disableNew || model.disableActions || model.isInlineEditing;

        $scope.showExportTool = () => model.showExportMenu && !model.isInPicker;
        $scope.disabledExportTool = () => model.isEditMode || model.isInDesign || model.isInlineEditing;

        $scope.showActionButtons = () => !model.isInPicker;
        $scope.disabledActionButtons = (ab) => ab.disabled || model.isInDesign || model.disableActions || model.isInlineEditing;

        $scope.showInlineEditingButtons = () => {
            // We only allow inline editing when the action menu button is enabled and not in a picker or in the report builder
            // and when the feature is on and when we have rows (just checking selected) with
            // an entity id (summary reports don't have a single entity for the row so we don't inline edit).
            // Note: in builder mode the button is shown but is disabled.
            return model.showEditInlineButton && ((canInlineEdit && !model.isInPicker && !model.isEditMode && !!getSelectedRowEid()) || model.isEditMode);
        };

        $scope.$on('spControlOnFormClick',
            function(event, control) {
                if (!model.isInlineEditing || !control || control.readonly || control.disabled) {
                    return;
                }

                control.select();
            });

        gridOptions.getActionExecutionContext = defaultGetActionExecutionContext;
        gridOptions.isMobile = $scope.options.isMobile;
        gridOptions.pageSize = $scope.options.pageSize;
        gridOptions.hideReportHeader = false;
        gridOptions.reportStyle = '';
        gridOptions.getIsInlineEditing = getIsInlineEditing;
        gridOptions.getInlineEditingState = getInlineEditingState;
        gridOptions.getInlineEditingTemplate = getInlineEditingTemplate;
        gridOptions.getInlineEditingMessages = getInlineEditingMessages;

        model.newMenu = {};
        model.actionMenu = [];
        model.actionButtons = [];
        model.disableActions = false;
        model.disableNew = false;
        model.hideActionBar = false;
        model.reportRanAtleastOnce = false;  // used in report render control to know if the report has ran at least once
        model.actionMenuItemsForSelection = {};

        model.userCanEditActions = spAppSettings.fullConfig; // full admins may edit actions

        $scope.typeIdFilter = $stateParams.typeIdFilter && parseInt($stateParams.typeIdFilter, 10);

        $scope.enterInlineEditMode = enterInlineEditMode;
        $scope.saveInlineEdits = saveInlineEdits;
        $scope.cancelInlineEdits = cancelInlineEdits;

        ///////////////////////////////////////////////////////////////////////////
        // establish watchers and listeners

        $element.on('$destroy', function () {
            model.gridBusyIndicator.isBusy = false;
            model.destroyed = true;

            // ensure any outstanding inline editing sessions are cleared.
            cancelInlineEdits();
        });

        $scope.$watch('options.isEditMode', function () {
            model.isEditMode = $scope.options.isEditMode;
            gridOptions.isEditMode = $scope.options.isEditMode;
            model.analyzerOptions.isEditMode = $scope.options.isEditMode;
            model.schemaOnly = $scope.options.schemaOnly;
        });

        $scope.$watch('options.isInDesign', function () {
            model.isInDesign = $scope.options.isInDesign;
            model.refreshButtonOptions.isInDesign = $scope.options.isInDesign;
            model.analyzerOptions.isInDesign = $scope.options.isInDesign;
        });

        $scope.$watch('options.isInPicker', function () {
            model.isInPicker = $scope.options.isInPicker;
            gridOptions.hideGridContextMenu = $scope.options.isInPicker;   // reports in picker shouldn't show grid context menu. #22127

            if (model.isInPicker && gridOptions.multiSelect) {
                gridOptions.showSelectionCheckbox = true;
            }
        });

        $scope.$watch('options.formControlEntity', function () {
            model.formControlEntity = $scope.options.formControlEntity;
        });

        $scope.$watch('options.formDataEntity', function () {
            model.formDataEntity = $scope.options.formDataEntity;
            gridOptions.formDataEntity = $scope.options.formDataEntity;
        });

        $scope.$watch('options.disableNew', function () {
            model.disableNew = $scope.options.disableNew;
        });

        $scope.$watch('options.disableActions', function () {
            model.disableActions = $scope.options.disableActions;
        });

        $scope.$watch('options.existingAggregateRows', function () {
            gridOptions.existingAggregateRows = $scope.options.existingAggregateRows;
        });

        $scope.$watch('options.selectedItems', function () {
            model.selectedItems = $scope.options.selectedItems;
            model.firstSelectedItem = null;
            if ($scope.options.selectedItems) {
                model.firstSelectedItem = $scope.options.selectedItems[0];
                reportModelManager.updateInlineEditingMetadata(getSelectedRowIndex());
                updateActionButtonsEnabledDebounce($scope.options.selectedItems);
            }
        });

        $scope.$watch('options.quickSearch', updateQuickSearchInCurrentItem);
        $scope.$watch('model.quickSearch', updateQuickSearchInCurrentItem);

        $scope.$watch('options.multiSelect', function () {
            gridOptions.multiSelect = $scope.options.multiSelect;

            if (model.isInPicker && gridOptions.multiSelect) {
                gridOptions.showSelectionCheckbox = true;
            }
        });

        $scope.$watch('options.reportId', reportIdChanged);

        $scope.$watch('options.entityTypeId', entityTypeIdChanged);

        // watch the included entity identifiers (faux relationships).
        $scope.$watch('options.inclids', function () {
            if (!_.difference($scope.options.inclids, model.inclids).length) {
                return;
            }

            if ($scope.options.inclids) {
                model.inclids = $scope.options.inclids;
            }

            if (!$scope.options.reportModel) {
                triggerGetReportData(defaultReportParams());
            }
        });

        // watch the excluded entity identifiers (faux relationships).
        $scope.$watch('options.exclids', function () {
            if (!_.difference($scope.options.exclids, model.exclids).length) {
                return;
            }

            if ($scope.options.exclids) {
                model.exclids = $scope.options.exclids;
            }

            if (!$scope.options.reportModel) {
                triggerGetReportData(defaultReportParams());
            }
        });

        // watch the relationship detail.
        $scope.$watch('options.relationDetail', function () {
            if (_.isEqual($scope.options.relationDetail, model.relationDetail)) {
                return;
            }

            model.relationDetail = $scope.options.relationDetail;

            // warning: executed when report is being used in the tab relationship control, within a form
            // within a screen.
            if (model.reportId && model.relationDetail) {
                triggerGetReportData(defaultReportParams());

                // update the toolbar
                getReportActionQuickMenuDebounce();
            }
        });

        // watch the related entity filters
        $scope.$watch('options.relationshipFilters', function () {
            if (_.isEqual($scope.options.relationshipFilters, model.relationshipFilters)) {
                return;
            }

            model.relationshipFilters = $scope.options.relationshipFilters;

            if (!$scope.options.reportModel) {
                triggerGetReportData(defaultReportParams());
            }
        });

        // watch the related entity filters
        $scope.$watch('options.filteredEntityIds', function () {
            if (!_.difference($scope.options.filteredEntityIds, model.filteredEntityIds).length) {
                return;
            }

            model.filteredEntityIds = $scope.options.filteredEntityIds;

            if (!$scope.options.reportModel) {
                triggerGetReportData(defaultReportParams());
            }
        });


        // watch for generic filter details (binding to pivot chart)
        $scope.$watch('options.externalConds', function (value, last) {
            if (value !== last) {
                if ($scope.options.externalConds) {
                    model.externalConds = $scope.options.externalConds;
                    model.startIndex = 0;
                    triggerGetReportData(defaultReportParams());
                    reportModelManager.mergeFilterConditions();
                }
            }
        });

        // Watch for report Entity changes
        $scope.$watch('options.reportEntity', function () {
            loadReportEntity();
        });

        $scope.$watch('options.reportEntityChanged', function () {
            loadReportEntity();
        });

        // watch model's invalidreportinfo
        $scope.$watch('model.invalidReportInfos', function () {
            if (model.invalidReportInfos) {
                $scope.options.invalidReportInfos = model.invalidReportInfos;
            }
        });

        // watch model's withInvalidReportCondition flag, in viewmode, raise the error message
        $scope.$watch('model.withInvalidReportCondition', function () {
            if (!model.isEditMode && model.withInvalidReportCondition === true) {
                spAlertsService.addAlert('This report contains breaking links. Please contact your administrator', 'error');
            }
        });

        $scope.$watch('model.reportRanAtleastOnce', function (newVal, oldVal) {
            if (newVal === oldVal) {
                return;
            }

            var recordCount = gridOptions.rowData.length;
            // Raise an event to indicate that the report ran and send record count.
            $scope.$emit('spReportEventReportRanAtleastOnce', recordCount);
        });

        $scope.$watch('options.title', function () {
            model.title = $scope.options && $scope.options.title;
        });

        $scope.$watch('options.modifyAccessDenied', function () {
            model.modifyAccessDenied = $scope.options && $scope.options.modifyAccessDenied;
        });

        // Watch for report model changes
        $scope.$watch('options.reportModel', reportModelChanged);

        $scope.$watch('options.getActionExecutionContext', function () {
            if ($scope.options.getActionExecutionContext) {
                $scope.getActionExecutionContext = $scope.options.getActionExecutionContext;
                gridOptions.getActionExecutionContext = $scope.options.getActionExecutionContext;
            }
        });

        $scope.$watch('options.customDataProvider', function () {
            if ($scope.options.customDataProvider) {
                model.customDataProvider = $scope.options.customDataProvider;
            }
        });

        $scope.$watch('model.moreDataAvailable', function () {
            gridOptions.moreDataAvailable = model.moreDataAvailable;
        });

        $scope.$on('$destroy', function () {
            if (model &&
                gridOptions) {
                gridOptions.getActionExecutionContext = null;
            }
            $scope.getActionExecutionContext = null;
        });

        // Handle grid event data events
        $scope.$on('spDataGridEventData', onDataGridEventData);

        $scope.$on('spDataGridGroupsStateChanged', onDataGridGroupsStateChanged);

        $scope.$on('spDataGridEventSelectionChanged', onDataGridEventSelectionChanged);

        $scope.$on('spDataGridEventGridDoubleClicked', onDataGridEventGridDoubleClicked);

        $scope.$on('spDataGridEventDataRequested', onDataGridEventDataRequested);

        $scope.$on('spDataGridAggregateRowStateChanged', onDataGridAggregateRowStateChanged);

        $scope.$on('spDataGridEventGridSorted', function (event, sortInfo) {
            event.stopPropagation();
            onDataGridSorted();
        });

        $scope.$on('spAnalyzerEventApplyConditions', onAnalyzerEventApplyConditions);

        // respond to an action being called, either pass or fail
        $scope.$on('actionExecuted', function (evt, args) {

            /////
            // clear the selected items. datagrid should be doing the same.
            /////
            if ($scope.options.selectedItems) {
                $scope.options.selectedItems.length = 0;
            }

            var selected = $scope.options && $scope.options.selectedItems || [];
            updateActionButtonsEnabledDebounce(selected);
        });

        $scope.$on("spDataGridEventGridCellLinkClicked", onDataGridCellLinkClicked);

        //////////////////////////////////////////////////////////////////////
        // We are ready to go....

        activate();

        //////////////////////////////////////////////////////////////////////
        // The implementation

        function focusInlineEditCellElement() {
            if (gridOptions.dataGrid &&
                gridOptions.dataGrid.focusInlineEditCellElement) {
                gridOptions.dataGrid.focusInlineEditCellElement();
            }
        }

        function activate() {
            initialiseInlineEditing();

            // Raise an event to indicate that the report model is ready
            $scope.$emit('spReportEventModelReady', model);
        }

        function reportIdChanged() {

            const {model, options} = $scope;

            //sp.logTime('Detect options.reportId ' + options.reportId);

            model.reportId = options.reportId;

            if (!options.reportModel && model.reportId) {
                // Don't have an existing report model so reload it using the report id.

                // Pass in the quickSearch to allow an external control to do the searching.
                reportModelManager.resetModel(options.quickSearch);

                triggerGetReportData(defaultReportParams());

                // update the toolbar
                getReportActionQuickMenuDebounce();

                // Initialise inline editing state for the new report
                initialiseInlineEditing();
            }
        }

        function entityTypeIdChanged() {
            model.entityTypeId = $scope.options.entityTypeId ? $scope.options.entityTypeId : $scope.typeIdFilter;

            if (!$scope.options.reportModel && model.entityTypeId) {
                triggerGetReportData(defaultReportParams());
            }
        }

        function reportModelChanged() {
            if ($scope.options.reportModel) {

                reportModelManager.resetModel();
                reportModelManager.initializeFromModel($scope.options.reportModel);

                // Clear the row data as we are going to reload the report
                gridOptions.rowData = [];

                selectLastSelectedItems = true;

                if ($scope.options.reportModel.reportId) {
                    triggerGetReportData(defaultReportParams());
                }

                updateCanInlineEdit();
            }
        }

        function onDataGridGroupsStateChanged() {
            if (_.get(model, 'aggRowClickDetails.scrollToGroup') &&
                _.get(model, 'gridOptions.dataGrid.scrollToGroup')) {
                // Scroll to group
                const {aggRowClickDetails} = model;
                gridOptions.dataGrid.scrollToGroup(aggRowClickDetails.aggRowIndex, aggRowClickDetails.aggRowOffsetTop);
                aggRowClickDetails.scrollToGroup = false;
            }
        }

        function onDataGridEventData() {
            const {options} = $scope;

            updateCanInlineEdit();

            if (cancelInlineEditsPending) {
                cancelInlineEdits();
            }

            if (selectLastSelectedItems) {

                reportModelManager.updateSelectedItemIndexes(gridOptions.selectedItems);
                options.selectedItems = gridOptions.selectedItems;

                if (!options.selectedItems || options.selectedItems.length === 0) {

                    if (gridOptions.rowData && gridOptions.rowData.length) {

                        options.selectedItems = options.selectedItems || [];

                        if (gridOptions.dataGrid && gridOptions.dataGrid.getFirstSelectableRowDataItem && !gridOptions.showSelectionCheckbox) {

                            const firstRow = gridOptions.dataGrid.getFirstSelectableRowDataItem();
                            if (firstRow) {
                                options.selectedItems.push(firstRow);
                            }
                        }
                    }
                }

                reportModelManager.setSelectedItems(options.selectedItems, true);
                selectLastSelectedItems = false;

            } else {

                if (!options.selectedItems || options.selectedItems.length === 0) {
                    if (gridOptions.rowData && gridOptions.rowData.length) {

                        options.selectedItems = options.selectedItems || [];

                        if (!isMobile && gridOptions.dataGrid &&
                            gridOptions.dataGrid.getFirstSelectableRowDataItem &&
                            !gridOptions.showSelectionCheckbox) {
                            // on desktop we want the first row to be selected by default
                            const firstRow = gridOptions.dataGrid.getFirstSelectableRowDataItem();
                            if (firstRow) {
                                options.selectedItems.push(firstRow);
                            }
                        }
                    }
                }

                reportModelManager.setSelectedItems(options.selectedItems, false);
                updateActionButtonsEnabledDebounce(options.selectedItems);
            }
        }

        // propagate event as 'spReportEventSelectionChanged'
        function onDataGridEventSelectionChanged(event, selectedItems) {
            $scope.options.selectedItems = selectedItems;
            $scope.$emit('spReportEventSelectionChanged', selectedItems);
            safeApply();
        }

        // respond to the double click event by loading the first action
        function onDataGridEventGridDoubleClicked(event, selectedItems) {

            if (model.isInlineEditing) {
                return; // ignore
            }

            if (model.isInPicker) {
                $scope.$emit('spReportEventGridDoubleClicked', selectedItems);
                return;
            }

            //update current item data's groupbyState by current grid
            var currentItem = spNavService.getCurrentItem();
            if (currentItem && currentItem.data && currentItem.data.reportModelManager) {
                currentItem.data.reportModelManager.setAggregateRows(gridOptions.dataGrid.getAllAggregateRowState());
            }

            getDefaultContextMenuAction(selectedItems).then(function (action) {
                var ids = [];
                if (selectedItems && selectedItems.length > 0) {
                    ids = _.map(selectedItems, 'eid');
                }

                if (action && action.isenabled === true) {
                    var ctx = {};
                    if ($scope.getActionExecutionContext) {
                        ctx = $scope.getActionExecutionContext(action, ids);
                    }

                    spActionsService.executeAction(action, ctx);
                }
            });

            $scope.$emit('spReportEventGridDoubleClicked', selectedItems);
        }

        function convertGroupDataFilterToAnalyzerConditions(groupDataFilter) {
            if (!model.aggregateDataManager || !model.reportMetadata || !groupDataFilter) {

                return null;
            }

            var metadataManager = spReportMetadataManager(model.reportMetadata);
            var groupColumns = model.aggregateDataManager.getGroupColumns();

            if (!groupColumns) {
                return null;
            }

            var groupFilterAnalyzerFields = _.map(groupDataFilter, function (value, index) {
                var groupColumn = groupColumns[index], valueAsString;

                var type = groupColumn.rcol.type;

                var cond = {
                    expid: groupColumn.id,
                    type: type
                };

                if (sp.isNullOrUndefined(value)) {
                    cond.oper = 'IsNull';
                } else {
                    if (metadataManager.isPrimitiveType(type)) {
                        valueAsString = metadataManager.convertValueToString(type, value);
                        if (type === spEntity.DataType.Bool) {
                            cond.oper = valueAsString === 'True' ? 'IsTrue' : 'IsFalse';
                        } else {
                            cond.value = valueAsString;
                            cond.oper = 'Equal';
                        }
                    } else {
                        cond.values = value;
                        cond.oper = 'AnyOf';
                    }
                }

                return cond;
            });

            return groupFilterAnalyzerFields;
        }

        function onDataGridEventDataRequested(event, eventArgs) {
            var isRollupReport;
            var index = eventArgs.index;
            var source = eventArgs.source;
            var pageSizeMultipler = null;
            var isPagingPerGroupEnabled = false;
            var lastAggRowClickDetails;
            var groupFilterConds = null;
            var defaultParams;

            event.stopPropagation();
            // Raised by the grid to request more data
            if (!model.moreDataAvailable) {
                // There is no more data available.
                return;
            }

            // Grid is raising the event twice for some reason.
            // This is needing to prevent the data being requested twice.
            if (index === model.startIndex) {
                return;
            }

            if (source === 'scroll') {
                // Disable infinite scrolling for grouped reports
                isRollupReport = model.aggregateDataManager ? model.aggregateDataManager.hasRollupData() : false;
                if (isRollupReport) {
                    return;
                }
            }

            model.startIndex = index;

            if (!model.aggRowClickDetails) {
                model.aggRowClickDetails = {
                    scrollToGroup: false
                };
            }

            if (source === 'aggRowClick') {
                lastAggRowClickDetails = model.aggRowClickDetails;

                if (lastAggRowClickDetails.aggRowIndex === eventArgs.aggRowIndex) {
                    // Same group was clicked, increase the page size
                    pageSizeMultipler = 10;
                }

                model.aggRowClickDetails = {
                    aggRowIndex: eventArgs.aggRowIndex,
                    aggRowOffsetTop: eventArgs.aggRowOffsetTop,
                    scrollToGroup: true
                };

                if (eventArgs.groupDataFilter) {
                    isPagingPerGroupEnabled = true;

                    model.startIndex = eventArgs.childRowsCount || 0;

                    // Get the group filter to add to the analyzer conditions
                    groupFilterConds = convertGroupDataFilterToAnalyzerConditions(eventArgs.groupDataFilter);
                }
            } else {
                model.aggRowClickDetails.scrollToGroup = false;
            }

            if (model.isEditMode === true && model.reportEntity !== null) {
                defaultParams = {};

                if (pageSizeMultipler) {
                    defaultParams.pageSizeMultipler = pageSizeMultipler;
                }

                reportModelManager.getReportDataByReportEntity(defaultParams, model.schemaOnly);
            } else {
                defaultParams = defaultReportParams({includeMetadata: false});

                if (isPagingPerGroupEnabled) {
                    if (!groupFilterConds) {
                        console.error('An error has occured getting the report group filter. The report data will not be loaded.');
                        return;
                    }

                    defaultParams.isGroupPage = true;
                    defaultParams.aggRowIndex = eventArgs.aggRowIndex;
                    defaultParams.groupFilterConds = groupFilterConds;
                }

                if (pageSizeMultipler) {
                    defaultParams.pageSizeMultipler = pageSizeMultipler;
                }

                reportModelManager.getReportData(defaultParams);
            }
        }

        function onDataGridAggregateRowStateChanged(event, aggregateRowState) {
            // Aggragate state has changed, save it in the model and
            // pass it onto the grid
            reportModelManager.setAggregateRows(aggregateRowState);
            gridOptions.existingAggregateRows = aggregateRowState;
        }

        function onAnalyzerEventApplyConditions(event, analyzerFields, isReset) {
            var metadataManager;
            var isRollupReport = model.aggregateDataManager ?
                model.aggregateDataManager.hasRollupData() : false;

            event.stopPropagation();

            metadataManager = spReportMetadataManager(model.reportMetadata);

            // Reset paging info
            reportModelManager.resetPagingInfo();

            // Update the metadata
            model.analyzerOptions.conds = metadataManager.getAnalyzerFieldsAsConds(model.analyzerOptions.analyzerFields);

            model.hasAdHocAnalyzerConditions = true;

            reportModelManager.mergeFilterConditions();
            // Get the latest data

            if (model.isEditMode === true && model.reportEntity !== null) {
                //var reportbuilderoptions = { conds: model.analyzerOptions.conds };
                //reportBuilderService.setActionFromReportBuilder('applyAnalysers', null, null, reportbuilderoptions);
                reportBuilderService.applyAnalysers(metadataManager.getAnalyzerFieldsAsCondEntitys(model.analyzerOptions.analyzerFields));

            } else {
                //if isRollupReport or isReset is true,  require full metadata
                reportModelManager.getReportData(defaultReportParams({
                    updateMetadataOnly: !isRollupReport && !isReset,
                    isReset: isReset
                }));
            }
        }

        function onAllowDrop(source, target, dragData) {
            if (!target) {
                return false;
            }

            var t = $(target);

            try {
                if (dragData.fid) {
                    return true;
                } else {
                    return false;
                }
            } catch (e) {
                return false;
            }
        }

        function onDragOver(event, source, target) {
            var t, jTarget;

            if (!target) {
                return false;
            }

            t = $(target);

            jTarget = t.closest('.analyzerButton-view');

            if (jTarget && jTarget.length > 0) {
                jTarget.css('background-image', 'url(assets/images/AnalyzerActiveFilters.png)');
                jTarget.css('background-position', '5px center');
                jTarget.css('background-repeat', 'no-repeat');
                jTarget.css('background-color', '#dbf0ff');
                jTarget.css('border-bottom', '1px solid #cbeaff');
            }

            return true;

        }

        function onDragLeave(event, source, target) {
            var t, jTarget;

            if (!target) {
                return false;
            }

            t = $(target);

            jTarget = t.closest('.analyzerButton-view');

            if (jTarget && jTarget.length > 0) {
                jTarget.css('background-image', 'url(assets/images/AnalyzerNoFilters.png)');
                jTarget.css('background-position', '5px center');
                jTarget.css('background-repeat', 'no-repeat');
                jTarget.css('background-color', 'transparent');
                jTarget.css('border-bottom', '1px solid transparent');
            }

            return true;
        }

        function onDrop(event, source, target, dragData) {
            var t, jTarget;

            if (!target) {
                return;
            }

            t = $(target);

            jTarget = t.closest('.analyzerButton');

            if (jTarget && jTarget.length > 0) {
                jTarget.css('background-color', '#dddddd');
            }

            reportBuilderService.setActionFromReport('addAnalyzerByDragDrop', dragData, null);
            _.delay(function () {
                $scope.$apply();
            });
        }

        // Handle context menu items
        function onContextMenuAction(menuAction, col) {
            var columnId,
                groupColumnIds,
                sortInfoChanged = false;

            if (!col || !col.colDef || !col.colDef.spColumnDefinition || !col.colDef.spColumnDefinition.columnId) {
                return;
            }

            columnId = col.colDef.spColumnDefinition.columnId;

            if ((menuAction === 'sortAscending' ||
                menuAction === 'sortDescending') &&
                model.aggregateDataManager) {
                groupColumnIds = _.map(model.aggregateDataManager.getGroupColumns(), 'id');
            }

            switch (menuAction) {
                case 'sortAscending':
                    sortInfoChanged = spDataGridUtils.updateColumnSortInfo(gridOptions.sortInfo, columnId, 'asc', groupColumnIds);
                    break;
                case 'sortDescending':
                    sortInfoChanged = spDataGridUtils.updateColumnSortInfo(gridOptions.sortInfo, columnId, 'desc', groupColumnIds);
                    break;
                case 'cancelSort':
                    sortInfoChanged = spDataGridUtils.removeColumnSortInfo(gridOptions.sortInfo, columnId);
                    break;
                case 'sortOptions':
                    showSortOptionsDialog();
                    break;
                case 'formatColumn':
                    showColumnFormattingDialog(columnId);
                    break;
                case 'groupBy':
                    showColumnGroupingDialog(columnId);
                    gridOptions.selectedItems = [];
                    break;
                case 'showTotals':
                    showColumnRollupDialog(columnId);
                    break;
                case 'removeColumn':
                    removeColumn(columnId);
                    break;
                case 'editCalculation':
                    showEditCalculationDialog(columnId);
                    break;
                case 'renameColumn':
                    showRenameDialog(columnId);
                    break;
                case 'summarise':
                    showSummariseDialog(columnId);
                    break;
            }

            if (sortInfoChanged) {
                gridOptions.sortInfo = _.toArray(gridOptions.sortInfo);
                onDataGridSorted();
            }
        }

        // Returns true if the context menu action is disabled, false otherwise.
        function isContextMenuActionDisabled(menuAction, col) {
            var disabled = false,
                columnSortInfo,
                columnId;

            if (!col || !col.colDef || !col.colDef.spColumnDefinition || !col.colDef.spColumnDefinition.columnId) {
                return;
            }

            columnId = col.colDef.spColumnDefinition.columnId;

            switch (menuAction) {
                case 'cancelSort':
                    columnSortInfo = _.find(gridOptions.sortInfo, function (si) {
                        return si.columnId === columnId;
                    });

                    if (!columnSortInfo) {
                        disabled = true;
                    }
                    break;
            }

            return disabled;
        }

        // Returns true if the context menu action is hidden, false otherwise
        function isContextMenuActionHidden(menuAction, col) {
            var hidden = false;

            switch (menuAction) {
                case 'renameColumn':
                case 'removeColumn':
                case 'editCalculation':
                case 'showTotals':
                case 'summarise':
                case 'aggregateDivider':
                case 'formatColumn':
                case 'groupByDivider':
                    hidden = !model.isEditMode;
                    break;
            }

            //only allow user set three levels groupby in report
            if (menuAction === 'groupBy') {
                if (model.isEditMode === true) {
                    var groupByColumns = _.filter(model.reportEntity.getReportColumns(), function (column) {
                        return column && column.columnGrouping && column.columnGrouping.length > 0;
                    });
                    //if there are more than 3 group by column exists in report, hide the groupby action
                    if (groupByColumns.length >= 3)
                        hidden = true;
                } else {
                    hidden = true;
                }
            }

            if (menuAction === 'editCalculation' && model.isEditMode === true) {
                var selectedColumn = _.find(model.reportEntity.getReportColumns(), function (column) {
                    return column.id().toString() === col.colDef.spColumnDefinition.columnId.toString();
                });

                try {
                    hidden = !(selectedColumn && selectedColumn.getColumnExpression().getType().alias() === 'core:scriptExpression');
                } catch (e) {
                    hidden = true;
                }
            }

            return hidden;
        }

        // Returns the list of items for the actions dropdown
        function getActionMenuItems() {
            var deferred = $q.defer();

            var ids = [];
            var lastId = -1;
            var reportId = model.reportId;
            var entityTypeId = model.entityTypeId;
            var hostIds = [];
            var hostTypeIds = [];
            var formDataEntityId = model.formDataEntity ? model.formDataEntity.idP : -1;

            ids = spActionsService.getEntityIdsFromDataGridSelection($scope.options.selectedItems, gridOptions.columnDefinitions);
            lastId = _.last(ids);
            if (model.formControlEntity) {
                if (model.formControlEntity.dataState !== spEntity.DataStateEnum.Create) {
                    hostIds = [model.formControlEntity.idP];
                }

                hostTypeIds = _.map(model.formControlEntity.typesP, 'idP');
            }

            var selected = $scope.options.selectedItems && _.some($scope.options.selectedItems, 'obj') ? _.map($scope.options.selectedItems, 'obj') : undefined;

            if (reportId > 0) {
                var actionRequest = {
                    ids: ids,
                    lastId: lastId,
                    cellId: -1,
                    reportId: reportId,
                    formDataEntityId: formDataEntityId,
                    hostIds: hostIds,
                    hostTypeIds: hostTypeIds,
                    entityTypeId: entityTypeId,
                    data: {},
                    display: 'actionsmenu',
                    selected: selected
                };

                // Check the cached action menu items
                var cachedRequest = sp.result($scope, 'model.actionMenuItemsForSelection.actionRequest');
                var cachedItems = sp.result($scope, 'model.actionMenuItemsForSelection.actionItems');
                if (cachedItems && _.isEqual(cachedRequest, actionRequest)) {
                    deferred.resolve(cachedItems);
                    return deferred.promise;
                }

                spActionsService.getConsoleActions(actionRequest)
                    .then(function (response) {
                        if (response) {
                            var menuKey = '';
                            if (actionRequest.display) {
                                menuKey += actionRequest.display;
                            }
                            if (actionRequest.hostIds && actionRequest.hostIds.length > 0) {
                                menuKey += actionRequest.hostIds.join('');
                            }

                            var items = spContextMenuService.getItemsFromActions(response.actions, menuKey);

                            // Cache the action items for the current request
                            model.actionMenuItemsForSelection.actionItems = items;
                            model.actionMenuItemsForSelection.actionRequest = actionRequest;

                            deferred.resolve(items);
                        }
                    }, function (error) {
                        console.error('spReportController.getActionMenuItems error:', error);
                        //spAlertsService.addAlert('Failed to show action menu.', 'error');
                    });
            }

            return deferred.promise;
        }

        // Launches the dialog for altering report and instance actions
        function showActionsDialog() {
            var reportActionsOptions = {
                formControlEntity: model.formControlEntity,
                reportId: model.reportId,
                entityTypeId: model.entityTypeId
            };
            var reportEntity = reportBuilderService.getReportEntity();
            if (reportEntity &&
                reportEntity._query &&
                reportEntity._query.idP === reportActionsOptions.reportId) {
                reportActionsOptions.reportEntity = reportEntity._query;
            }
            var hostIds = [];
            var formControl = model.formControlEntity;
            if (formControl && formControl.dataState !== spEntity.DataStateEnum.Create) {
                hostIds = [formControl.idP];
            }
            var menuKey = 'quickmenu';
            if (hostIds.length > 0) {
                menuKey += hostIds.join('');
            }
            spReportActionsDialog.showModalDialog(reportActionsOptions).then(function (result) {
                if (result) {
                    if (reportEntity) {
                        reportBuilderService.setReportEntityFromReport(reportEntity);
                    }

                    var newMenuItems = spContextMenuService.getItemsFromActions(result.newMenu, menuKey);
                    var exportMenuItems = spContextMenuService.getItemsFromActions(result.exportMenu, menuKey);
                    model.newMenu = {menuItems: newMenuItems};
                    model.showNewMenu = result.showNewMenu && _.some(model.newMenu.menuItems);
                    model.exportMenu = {menuItems: exportMenuItems};
                    model.showExportMenu = result.showExportMenu && _.some(model.exportMenu.menuItems);
                    model.showEditInlineButton = result.showEditInlineButton;

                    // redress the actions buttons
                    model.actionButtons.length = 0;

                    _.forEach(result.actionButtons, createActionButton);

                    updateActionButtonsEnabledDebounce($scope.options.selectedItems);
                }
            });
        }

        // Executes the first (or only) "Create" action for the report
        function executeNewAction() {
            if (sp.result($scope, 'model.newMenu.menuItems.length')) {
                var createItem = model.newMenu.menuItems[0];
                if (createItem.action) {
                    var ids = spActionsService.getEntityIdsFromDataGridSelection(gridOptions.selectedItems, gridOptions.columnDefinitions);
                    var ctx = gridOptions.getActionExecutionContext(createItem.action, ids);
                    spActionsService.executeAction(createItem.action, ctx);
                }
            }
        }

        function pullDownRefresh() {
            // Only refresh when scrolled to the top
            if (gridOptions.dataGrid &&
                gridOptions.dataGrid.isScrolledToTop &&
                gridOptions.dataGrid.isScrolledToTop()) {
                // Reset paging info
                reportModelManager.resetPagingInfo();
                triggerGetReportData(defaultReportParams({includeMetadata: false}));
            }
        }

        function loadMore() {
            model.startIndex = gridOptions.rowData.length;
            reportModelManager.getReportData(defaultReportParams({includeMetadata: false}));
        }

        function onAnalyzerButtonClicked(isOpen) {
            if (isOpen) {
                reportModelManager.loadAnalyzerFieldDefinitions(model.reportMetadata);
            }
        }

        function onAggregateContextMenuAction(menuAction, row) {

            var sortInfoChanged,
                groupColumnIds,
                groupColumnId;

            if ((menuAction === 'sortAscending' ||
                menuAction === 'sortDescending' ||
                menuAction === 'cancelGroupBy') &&
                model.aggregateDataManager) {
                groupColumnIds = _.map(model.aggregateDataManager.getGroupColumns(), 'id');
                groupColumnId = groupColumnIds[row.depth];
            }

            switch (menuAction) {
                case 'hideLabel':
                    // Set this in the entity model
                    model.reportMetadata.rmeta.showlbl = false;
                    gridOptions.aggregateInfo.showAggregateLabel = false;
                    updateReportRollupOptions(null, false);
                    break;
                case 'showLabel':
                    // Set this in the entity model
                    model.reportMetadata.rmeta.showlbl = true;
                    gridOptions.aggregateInfo.showAggregateLabel = true;
                    updateReportRollupOptions(null, true);
                    break;
                case 'hideCount':
                    // Set this in the entity model
                    model.reportMetadata.rmeta.showcnt = false;
                    gridOptions.aggregateInfo.showCount = false;
                    updateReportRollupOptions(false, null);
                    break;
                case 'showCount':
                    // Set this in the entity model
                    model.reportMetadata.rmeta.showcnt = true;
                    gridOptions.aggregateInfo.showCount = true;
                    updateReportRollupOptions(true, null);
                    break;
                case 'sortAscending':
                    sortInfoChanged = spDataGridUtils.updateColumnSortInfo(gridOptions.sortInfo, groupColumnId, 'asc', groupColumnIds);
                    break;
                case 'sortDescending':
                    sortInfoChanged = spDataGridUtils.updateColumnSortInfo(gridOptions.sortInfo, groupColumnId, 'desc', groupColumnIds);
                    break;
                case 'sortOptions':
                    showSortOptionsDialog();
                    break;
                case 'cancelGroupBy':
                    removeColumnGrouping(groupColumnId);
                    gridOptions.selectedItems = [];
                    break;
                case 'expandAll':
                    gridOptions.dataGrid.expandAggregateRows(row.depth);
                    saveAggregateRowStateToModel(row, false);
                    break;
                case 'collapseAll':
                    gridOptions.dataGrid.collapseAggregateRows(row.depth);
                    saveAggregateRowStateToModel(row, true);
                    break;
            }

            if (sortInfoChanged) {
                gridOptions.sortInfo = _.toArray(gridOptions.sortInfo);
                onDataGridSorted();
            }
        }

        function isAggregateContextMenuActionHidden(menuAction, row) {
            var hidden = false;

            switch (menuAction) {
                case 'hideLabel':
                    hidden = !(model.isEditMode && gridOptions.aggregateInfo.showAggregateLabel);
                    break;
                case 'showLabel':
                    hidden = !(model.isEditMode && !gridOptions.aggregateInfo.showAggregateLabel);
                    break;
                case 'hideCount':
                    hidden = !(model.isEditMode && gridOptions.aggregateInfo.showCount);
                    break;
                case 'showCount':
                    hidden = !(model.isEditMode && !gridOptions.aggregateInfo.showCount);
                    break;
                case 'groupByDivider':
                    hidden = !model.isEditMode;
                    break;
                case 'cancelGroupBy':
                    hidden = !model.isEditMode;
                    break;
            }

            return hidden;
        }

        function loadReportEntity() {
            model.reportEntity = $scope.options.reportEntity;

            if (model.isEditMode === true && model.reportEntity !== null) {
                reportModelManager.resetBasicModel();

                reportModelManager.getReportDataByReportEntity(defaultReportParams(), model.schemaOnly);

                selectLastSelectedItems = true;
            }

            // update the toolbar
            if (model.reportEntity) {
                // in edit mode
                var update = model.reportId !== model.reportEntity.idP;
                model.reportId = model.reportEntity.idP;
                if (update) {
                    getReportActionQuickMenuDebounce();
                }
            } else {
                getReportActionQuickMenuDebounce();
            }
        }

        function updateQuickSearchInCurrentItem(value, oldValue) {
            if (value && value !== oldValue) {
                var currentItem = spNavService.getCurrentItem();
                if (currentItem && currentItem.data) {
                    currentItem.data.quickSearch = value;
                }
                $scope.options.quickSearch = value;
                model.quickSearch = value;
            }
        }

        function defaultReportParams(overrideParams) {

            var result = {
                includeMetadata: true,
                isMobile: $scope.options.isMobile,
                isInScreen: $scope.options.isInScreen
            };

            if (overrideParams) {
                result = _.extend(result, overrideParams);
            }

            return result;
        }

        // Trigger a getReportData to happen at the end of the digest cycle. This ensures that only
        // one request occurs even if multiple options are changed.
        function triggerGetReportData(params) {

            if (model.isInDesign)
                return;

            //sp.logTime('triggerGetReportData');

            // ensure that only one refresh occurs within a digest cycle.
            if (!requestStarted) {
                requestStarted = true;

                var now = new Date();
                var timeSinceLast = now - lastRequestTime;
                if (lastRequestTime && timeSinceLast < 250) {
                    console.log("spReportController: Multiple requests for data in report update. If this occurs frequently (every report run) this could indicate a performance problem. Time in ms since last data request: ", timeSinceLast);
                }
                lastRequestTime = now;

                runAfterDigest(function () {
                    //sp.logTime('triggerGetReportData actioned');
                    requestStarted = false;
                    reportModelManager.getReportData(params);
                });
            }
        }

        function runAfterDigest(callback) {
            // Note: $evalAsync runs sooner, but in some circumstances that would result in not catching all settings, resulting in a re-request
            if ($scope.options.fastRun) {
                $scope.$evalAsync(callback);
            } else {
                $timeout(callback);
            }
        }

        // Get the actions immediately available on the toolbar
        function getReportActionQuickMenu() {
            var entityTypeId = model.entityTypeId;
            var reportId = model.reportId;
            var report = model.reportEntity;
            if (!reportId && report) {
                reportId = report.idP;
            }
            if (reportId > 0) {
                var hostIds = [];
                var hostTypeIds = [];
                var formControl = model.formControlEntity;
                if (formControl) {
                    if (formControl.dataState !== spEntity.DataStateEnum.Create) {
                        hostIds = [formControl.idP];
                    }

                    hostTypeIds = _.map(formControl.typesP, 'idP');
                }

                model.actionButtons.length = 0;

                var actionRequest =
                {
                    ids: [],
                    lastId: -1,
                    cellId: -1,
                    reportId: reportId,
                    formDataEntityId: model.formDataEntity ? model.formDataEntity.idP : -1,
                    hostIds: hostIds,
                    hostTypeIds: hostTypeIds,
                    entityTypeId: entityTypeId,
                    data: {},
                    display: 'quickmenu'
                };

                spActionsService.getConsoleActions(actionRequest)
                    .then(function (response) {
                        if (response && response.actions) {

                            var menuKey = '';
                            if (actionRequest.display) {
                                menuKey += actionRequest.display;
                            }
                            if (actionRequest.hostIds && actionRequest.hostIds.length > 0) {
                                menuKey += actionRequest.hostIds.join('');
                            }

                            var menuItems = spContextMenuService.getItemsFromActions(response.actions, menuKey);

                            if (response.showNewMenu || $scope.options.isMobile) {
                                var newMenuItems = _.filter(menuItems, function (menuItem) {
                                    return menuItem.action && menuItem.action.ismenu && menuItem.action.isnew;
                                });
                                model.newMenu = {menuItems: newMenuItems};
                            }

                            if (response.showExportMenu) {
                                var exportMenuItems = _.filter(menuItems, function (menuItem) {
                                    return menuItem.action && menuItem.action.ismenu && menuItem.action.method === 'export';
                                });
                                model.exportMenu = {menuItems: exportMenuItems};
                            }

                            // show the new menu or not
                            model.showNewMenu = (response.showNewMenu || $scope.options.isMobile) && _.some(model.newMenu.menuItems);
                            model.showExportMenu = response.showExportMenu && _.some(model.exportMenu.menuItems);
                            model.showEditInlineButton = response.showEditInlineButton;

                            // On mobile we want to have the add button. On desktop only the ones marked as a button
                            var filter = isMobile ? {'alias': 'console:addRelationshipAction'} : {'isbutton': true};
                            var filteredActions = _.filter(response.actions, filter);

                            _.forEach(filteredActions, createActionButton);
                        }

                        updateActionButtonsEnabledDebounce($scope.options.selectedItems);
                    }, function (error) {
                        console.error('spReportController.getReportActionQuickMenu error:', error);
                        // REM -- Does user really need to see this? The report itself doesn't even notify of failure.
                        //spAlertsService.addAlert('Failed to load the toolbar.', 'error');
                    });
            }
        }

        function getDefaultContextMenuAction(selectedItems) {
            var ids = [];
            var lastId = -1;
            var reportId = model.reportId;
            var entityTypeId = model.entityTypeId;
            var hostIds = [];
            var hostTypeIds = [];

            ids = spActionsService.getEntityIdsFromDataGridSelection(selectedItems, gridOptions.columnDefinitions);
            lastId = _.last(ids);
            if (model.formControlEntity) {
                if (model.formControlEntity.dataState !== spEntity.DataStateEnum.Create) {
                    hostIds = [model.formControlEntity.idP];
                }

                hostTypeIds = _.map(model.formControlEntity.typesP, 'idP');
            }

            var selected = selectedItems && _.some(selectedItems, 'obj') ? _.map(selectedItems, 'obj') : undefined;
            var actionRequest =
            {
                ids: ids,
                lastId: lastId,
                reportId: reportId,
                hostIds: hostIds,
                hostTypeIds: hostTypeIds,
                entityTypeId: entityTypeId,
                data: {},
                display: 'contextmenu',
                selected: selected
            };

            return spActionsService.getConsoleActions(actionRequest).then(function (response) {
                if (response && response.actions && response.actions.length > 0) {
                    return _.first(response.actions);
                }
                return undefined;
            });
        }

        function createActionButton(action, ignoreIsButton) {
            if (action.isbutton || ignoreIsButton) {
                model.actionButtons.push({
                    name: action.name,
                    description: action.description,
                    nameshort: spActionsService.getShortName(action.name),
                    multiname: action.multiname,
                    emptyname: action.emptyname,
                    displayname: action.displayname || action.name,
                    displaynameshort: spActionsService.getShortName(action.displayname || action.name),
                    icon: action.icon,
                    order: action.order,
                    action: action,
                    disabled: !action.isenabled,
                    select: action.isselect,
                    multiselect: action.ismultiselect,
                    execute: function () {
                        //when the report selecteditems array is null, insert the first selected row data into selectedItems array
                        if ((gridOptions.selectedItems.length === 0) &&
                            gridOptions.dataGrid &&
                            gridOptions.dataGrid.getFirstSelectableRowDataItem) {
                            var firstRow = gridOptions.dataGrid.getFirstSelectableRowDataItem();
                            if (firstRow) {
                                $scope.options.selectedItems.push(firstRow);
                            }
                        }

                        var ids = spActionsService.getEntityIdsFromDataGridSelection(gridOptions.selectedItems, gridOptions.columnDefinitions);
                        var ctx = gridOptions.getActionExecutionContext(this.action, ids);
                        spActionsService.executeAction(this.action, ctx);
                    }
                });
            }
        }

        function menuItemsToDictionary(menuItems) {
            var menuItemsFlattened = sp.walkGraph(function (mi) {
                return mi.submenu;
            }, menuItems);

            return _.keyBy(menuItemsFlattened, function (mi) {
                return sp.result(mi, 'action.id');
            });
        }

        function updateActionButtonsEnabled(selected) {
            //the action button is disabled in Edit Mode, Design Mode and Picker Mode.
            if (!(model.isEditMode || model.isInDesign || model.isInPicker || model.isInlineEditing)) {
                getActionMenuItems().then(function (menuItems) {
                    updateActionButtonsEnabledImpl(selected, menuItems);
                });
            }
        }

        function updateActionButtonsEnabledImpl(selected, menuItems) {
            var objs = selected && _.some(selected, 'obj') ? _.map(selected, 'obj') : undefined;
            var isSelection = selected && selected.length > 0;
            var isMultiSelection = selected && selected.length > 1;
            var sortedButtons = model.actionButtons.sort(function (a, b) {
                return a.order > b.order || (a.order === b.order && a.name > b.name);
            });

            var menuItemsDict = menuItemsToDictionary(menuItems);

            _.forEach(sortedButtons, function (button) {
                // enable / disable
                button.disabled = false;
                if (model.isEditMode === true) {
                    button.disabled = true;
                } else {
                    var menuItem = menuItemsDict[sp.result(button, 'action.id')];

                    if (button.multiselect === true) {
                        button.disabled = !isMultiSelection && (!isSelection && button.select);
                    } else if (button.select === true) {
                        button.disabled = !isSelection || isMultiSelection;
                    }

                    if (!menuItem || menuItem.disabled) {
                        button.disabled = true;
                    }

                    // if still enabled...
                    if (!button.disabled) {
                        // check the custom actions
                        var foundCanFalse = false;
                        if (button.action && button.action.method === 'custom' && button.action.state) {
                            var getter = $parse('can' + button.action.state);
                            if (objs) {
                                _.forEach(objs, function (obj) {
                                    if (obj) {
                                        foundCanFalse = getter(obj) === false;
                                    }
                                });
                            }
                        }

                        button.disabled = foundCanFalse;
                    }
                }

                // update the label
                var newName = button.emptyname ? button.emptyname : button.name;
                if (isSelection) {
                    newName = button.name;
                }
                if (isMultiSelection) {
                    newName = button.multiname ? button.multiname : button.name;
                }
                button.nameshort = spActionsService.getShortName(newName);
                button.name = newName;
            });
        }

        function defaultGetActionExecutionContext(action, ids) {
            return {
                scope: $scope,
                state: action.state,
                selectionEntityIds: ids,
                isEditMode: model.isEditMode,
                refreshDataCallback: refreshReport
            };
        }

        function showSummariseDialog(columnId) {
            var columnDefinition = getColumnDefinitionById(columnId);

            if (!columnDefinition) {
                return;
            }

            reportBuilderService.setActionFromReport('updateSummarise', columnDefinition, null);
        }

        function showRenameDialog(columnId) {
            var columnDefinition = getColumnDefinitionById(columnId);

            if (!columnDefinition) {
                return;
            }

            reportBuilderService.setActionFromReport('renameColumn', columnDefinition, null);
        }

        function showEditCalculationDialog(columnId) {
            var columnDefinition = getColumnDefinitionById(columnId);

            if (!columnDefinition) {
                return;
            }

            //$scope.$apply(function () {
            reportBuilderService.setActionFromReport('updateCalculation', columnDefinition, null);
        }

        function showColumnFormattingDialog(columnId) {
            spTenantSettings.getNameFieldEntity().then(function (name) {
                showColumnFormattingDialogImpl(name.id(), columnId);
            });
        }

        function showColumnFormattingDialogImpl(nameFieldId, columnId) {
            var dialogOptions,
                cfRules,
                rcol,
                valRule,
                columnDefinition = getColumnDefinitionById(columnId);

            if (!columnDefinition) {
                return;
            }

            rcol = columnDefinition.tag.rcol;

            if (model.reportMetadata.valrules) {
                valRule = model.reportMetadata.valrules[columnId];
            }

            if (model.reportMetadata.cfrules) {
                cfRules = model.reportMetadata.cfrules[columnId];
            }

            var filteredOprType = _.filter(spTypeOperatorService.getApplicableOperators(columnDefinition.oprtype), function (opDef) {
                //current column is calculated field and type is choicefield or inlinerelationship field
                //it means without typeid of current related resource, remove the AnyOf and anyExcept option for operate type
                if ((opDef.oper === 'AnyOf' || opDef.oper === 'AnyExcept' || opDef.oper === 'AnyBelowStructureLevel' || opDef.oper === 'AnyAtOrBelowStructureLevel' || opDef.oper === 'AnyAboveStructureLevel' || opDef.oper === 'AnyAtOrAboveStructureLevel') && _.isUndefined(columnDefinition.tag.rcol.tid)) {
                    return false;
                } else {
                    return true;
                }
            });

            dialogOptions = {
                name: columnDefinition.displayName,
                type: columnDefinition.type,
                isEntityNameColumn: rcol.fid === nameFieldId,
                isAggCol: rcol.aggcol,
                formats: model.reportMetadata.typefmtstyle[columnDefinition.type],
                operators: filteredOprType,
                condFormatting: {
                    displayText: valRule ? !valRule.hideval : true,
                    disableDefaultFormat: valRule ? valRule.disabledefft : false,
                    format: cfRules ? cfRules.style : condFormattingConstants.formatTypeEnum.None
                },
                valueFormatting: {},
                imageFormatting: {}
            };

            setValueFormattingOptions(dialogOptions, rcol, valRule, columnId);
            setImageFormattingOptions(dialogOptions, rcol, valRule);
            if (!_.isUndefined(columnDefinition.tag.rcol.tid)) {
                switch (columnDefinition.oprtype ? columnDefinition.oprtype : columnDefinition.type) {
                    case 'ChoiceRelationship':
                        dialogOptions.availableEntities = reportModelManager.getAvailableChoiceEntities(columnDefinition.tag.rcol.tid);
                        break;
                    case 'InlineRelationship':
                    case 'UserInlineRelationship':
                        dialogOptions.entityTypeId = columnDefinition.tag.rcol.tid;
                        dialogOptions.pickerReportId = reportModelManager.getInlineTypePickerReportId(columnDefinition.tag.rcol.tid);
                        dialogOptions.filteredEntityIds = columnDefinition.tag.rcol.filtereids;
                        break;
                    case 'StructureLevels':
                        dialogOptions.entityTypeId = columnDefinition.tag.rcol.tid;
                        dialogOptions.pickerReportId = reportBuilderService.getStructureViewIdForColumn(columnId) || reportModelManager.getInlineTypePickerReportId(columnDefinition.tag.rcol.tid);
                        dialogOptions.filteredEntityIds = columnDefinition.tag.rcol.filtereids;
                        break;
                }
            }

            if (cfRules) {
                if (angular.isDefined(cfRules.showval)) {
                    dialogOptions.condFormatting.displayText = cfRules.showval;
                }

                switch (cfRules.style) {
                    case condFormattingConstants.formatTypeEnum.ProgressBar:
                        setProgressBarFormattingOptions(dialogOptions, cfRules);
                        break;
                    case condFormattingConstants.formatTypeEnum.Highlight:
                        setHighlightFormattingOptions(dialogOptions, cfRules, spTypeOperatorService.getApplicableOperators(columnDefinition.oprtype ? columnDefinition.oprtype : columnDefinition.type));
                        break;
                    case condFormattingConstants.formatTypeEnum.Icon:
                        setIconFormattingOptions(dialogOptions, cfRules, spTypeOperatorService.getApplicableOperators(columnDefinition.oprtype ? columnDefinition.oprtype : columnDefinition.type));
                        break;
                }
            }

            // Show the dialog
            spConditionalFormattingDialog.showModalDialog(dialogOptions).then(function (result) {
                if (!result) {
                    return;
                }

                var reportbuilderoptions;
                var metadataManager;
                var rcol;

                if (model.isEditMode && model.reportEntity) {
                    rcol = model.reportMetadata.rcols[columnId];

                    var columnDisplayFormat = rcol.type === 'Image' ? result.imageFormatting : result.valueFormatting;
                    reportBuilderService.updateStructureViewExpression(columnId, result.valueFormatting.structureViewId, nameFieldId);
                    reportBuilderService.updateColumnDisplayFormat(columnId, rcol.oprtype ? rcol.oprtype : rcol.type, columnDisplayFormat, result.condFormatting);
                }
            });
        }

        function removeColumnGrouping(columnId) {
            var reportbuilderoptions,
                columnDefinition = getColumnDefinitionById(columnId);

            if (!model.isEditMode || !model.reportEntity) {
                return;
            }

            //reportbuilderoptions = { columnId: columnId };
            //reportBuilderService.setActionFromReportBuilder('removeColumnGrouping', null, null, reportbuilderoptions);
            reportBuilderService.removeColumnGrouping(columnId);
        }

        function showColumnGroupingDialog(columnId) {
            var reportbuilderoptions,
                columnDefinition = getColumnDefinitionById(columnId);

            if (!model.isEditMode || !model.reportEntity) {
                return;
            }

            // Hardcode to list for the moment
            var groupMethod = 'groupList';

            //reportbuilderoptions = { columnId: columnId, groupingMethod: groupMethod };
            //reportBuilderService.setActionFromReportBuilder('addColumnGrouping', null, null, reportbuilderoptions);
            reportBuilderService.addColumnGrouping(columnId, groupMethod);
        }

        function showColumnRollupDialog(columnId) {
            var haveGroups = false,
                reportbuilderoptions,
                columnDefinition = getColumnDefinitionById(columnId),
                aggregateOptions,
                rmeta = model.reportMetadata ? model.reportMetadata.rmeta : null;

            if (!model.isEditMode || !model.reportEntity) {
                return;
            }

            if (model.aggregateDataManager) {
                haveGroups = model.aggregateDataManager.getGroupColumns().length > 0;
            }

            aggregateOptions = {
                name: columnDefinition.displayName,
                type: columnDefinition.type,
                haveGroups: haveGroups,
                showGrandTotals: rmeta ? rmeta.showgt : false,
                showSubTotals: rmeta ? rmeta.showst : false,
                showOptionLabels: rmeta && !sp.isNullOrUndefined(rmeta.showoplbl) ? rmeta.showoplbl : true,
                aggregateMethodIds: _.map(columnDefinition.totals, function (t) {
                    return t.id;
                })
            };
            spAggregateOptionsDialog.showModalDialog(aggregateOptions).then(function (result) {
                var reportbuilderoptions;

                if (!result) {
                    return;
                }

                reportbuilderoptions = {
                    columnId: columnId,
                    rollupSubTotals: result.showSubTotals,
                    rollupGrandTotals: result.showGrandTotals,
                    rollupOptionLabels: result.showOptionLabels,
                    rollupMethods: result.aggregateMethodIds
                };
                //reportBuilderService.setActionFromReportBuilder('setReportColumnRollups', null, null, reportbuilderoptions);
                reportBuilderService.setReportColumnRollups(columnId, result.aggregateMethodIds, result.showSubTotals, result.showGrandTotals, result.showOptionLabels);
            });
        }

        function setValueFormattingOptions(options, rcol, valRule, columnId) {
            if (rcol &&
                (rcol.type === spEntity.DataType.Date ||
                rcol.type === spEntity.DataType.Time ||
                rcol.type === spEntity.DataType.DateTime) &&
                model.reportMetadata.valsels &&
                model.reportMetadata.valsels[rcol.type]) {
                options.dateTimeFormats = _.map(_.sortBy(model.reportMetadata.valsels[rcol.type], 'ord'), function (dtf) {
                    return {
                        name: dtf.name,
                        formatName: dtf['enum']
                    };
                });
            }

            if (rcol &&
                valRule) {

                if (!_.isUndefined(valRule.align) && !_.isNull(valRule.align)) {
                    options.valueFormatting.alignment = valRule.align;
                }                

                switch (rcol.type) {
                    case spEntity.DataType.String:
                    case 'ChoiceRelationship':
                    case 'InlineRelationship':
                    case 'UserInlineRelationship':
                        if (valRule.lines && valRule.lines > 0) {
                            options.valueFormatting.lines = valRule.lines;
                        }
                        if (!sp.isNullOrUndefined(valRule.entitylistcolfmt)) {
                            options.valueFormatting.entityListFormatId = valRule.entitylistcolfmt;
                        }
                        break;
                    case spEntity.DataType.Int32:
                    case spEntity.DataType.Decimal:
                    case spEntity.DataType.Currency:
                        if (rcol.type !== spEntity.DataType.Int32 && !_.isUndefined(valRule.places) && !_.isNull(valRule.places)) {
                            options.valueFormatting.decimalPlaces = valRule.places;
                        }
                        if (valRule.prefix) {
                            options.valueFormatting.prefix = valRule.prefix;
                        }
                        if (valRule.suffix) {
                            options.valueFormatting.suffix = valRule.suffix;
                        }
                        break;
                    case spEntity.DataType.Date:
                    case spEntity.DataType.Time:
                    case spEntity.DataType.DateTime:
                        if (valRule.datetimefmt) {
                            options.valueFormatting.dateTimeFormatName = valRule.datetimefmt;
                        }
                        break;
                    case 'StructureLevels':
                        options.valueFormatting.structureViewId = reportBuilderService.getStructureViewIdForColumn(columnId);
                        if (valRule.lines && valRule.lines > 0) {
                            options.valueFormatting.lines = valRule.lines;
                        }
                        if (!sp.isNullOrUndefined(valRule.entitylistcolfmt)) {
                            options.valueFormatting.entityListFormatId = valRule.entitylistcolfmt;
                        }
                        break;
                }
            }
        }

        function setImageFormattingOptions(options, rcol, valRule) {
            if (rcol &&
                valRule &&
                rcol.type === 'Image') {
                if (valRule.scaleid) {
                    options.imageFormatting.imageScaleId = valRule.scaleid;
                }
                if (valRule.sizeid) {
                    options.imageFormatting.imageSizeId = valRule.sizeid;
                }
            }
        }

        function setProgressBarFormattingOptions(options, cfRules) {
            if (cfRules.rules &&
                cfRules.rules.length > 0) {
                options.condFormatting.progressBarRule = {};

                if (cfRules.rules[0].bgcolor) {
                    options.condFormatting.progressBarRule.color = _.cloneDeep(cfRules.rules[0].bgcolor);
                }

                if (cfRules.rules[0].bounds) {
                    options.condFormatting.progressBarRule.minimumValue = reportModelManager.convertToNativeType(options.type, cfRules.rules[0].bounds.lower);
                    options.condFormatting.progressBarRule.maximumValue = reportModelManager.convertToNativeType(options.type, cfRules.rules[0].bounds.upper);
                }
            }
        }

        function setHighlightFormattingOptions(options, cfRules, typeopers) {
            if (cfRules.rules &&
                cfRules.rules.length > 0) {
                options.condFormatting.highlightRules = _.map(cfRules.rules, function (rule) {
                    var type,
                        opDef,
                        result = {
                            operator: rule.oper,
                            color: {
                                foregroundColor: _.cloneDeep(rule.fgcolor),
                                backgroundColor: _.cloneDeep(rule.bgcolor)
                            }
                        };

                    opDef = _.find(typeopers, function (to) {
                        return to.oper === rule.oper;
                    });

                    if (opDef &&
                        opDef.argCount > 0) {
                        type = opDef.type || options.type;

                        if (rule.val) {
                            result.value = reportModelManager.convertToNativeType(type, rule.val);
                        } else if (rule.vals) {
                            result.value = reportModelManager.convertToNativeType(type, rule.vals);
                        }
                    }

                    return result;
                });
            }
        }

        function setIconFormattingOptions(options, cfRules, typeopers) {
            if (cfRules.rules &&
                cfRules.rules.length > 0) {
                options.condFormatting.iconRules = _.map(cfRules.rules, function (rule) {
                    var type,
                        opDef,
                        result = {
                            operator: rule.oper,
                            imgId: rule.imgid,
                            cfId: rule.cfid
                        };

                    opDef = _.find(typeopers, function (to) {
                        return to.oper === rule.oper;
                    });

                    if (opDef &&
                        opDef.argCount > 0) {
                        type = opDef.type || options.type;

                        if (rule.val) {
                            result.value = reportModelManager.convertToNativeType(type, rule.val);
                        } else if (rule.vals) {
                            result.value = reportModelManager.convertToNativeType(type, rule.vals);
                        }
                    }

                    return result;
                });
            }
        }

        function removeColumn(columnId) {
            var columnDefinition = getColumnDefinitionById(columnId);

            if (!columnDefinition) {
                return;
            }

            //$scope.$apply(function () {
            reportBuilderService.setActionFromReport('removeColumn', columnDefinition, null);
            //$scope.$apply();
            //});
        }

        function showSortOptionsDialog() {
            var sortDialogOptions = {
                columns: []
            };

            // Add the grouping columns to the list of available columns
            if (model.aggregateDataManager) {
                sortDialogOptions.columns = _.map(model.aggregateDataManager.getGroupColumns(), function (gc, index) {
                    return {
                        id: gc.id,
                        name: gc.rcol.title + ' (Group By Column ' + (index + 1) + ')',
                        isGroupingColumn: true,
                        groupingColumnIndex: index
                    };
                });
            }

            // Add available visible columns that haven't been added already
            sortDialogOptions.columns = sortDialogOptions.columns.concat(_.map(_.filter(gridOptions.columnDefinitions, function (cd) {
                var sortedColumn = _.find(sortDialogOptions.columns, function (sc) {
                    return sc.id === cd.tag.id;
                });
                return cd.visible && !sortedColumn;
            }), function (cd) {
                return {
                    id: cd.tag.id,
                    name: cd.displayName
                };
            }));

            // Setup dialog options sort info
            sortDialogOptions.sortInfo = _.map(gridOptions.sortInfo, function (si) {
                return {
                    columnId: si.columnId,
                    sortDirection: si.sortDirection
                };
            });


            // Show the dialog
            spSortOptionsDialog.showModalDialog(sortDialogOptions).then(function (result) {
                if (result &&
                    _.isArray(result)) {

                    if (angular.equals(sortDialogOptions.sortInfo, result)) {
                        return;
                    }

                    // Update the sort info
                    gridOptions.sortInfo = _.map(result, function (si) {
                        return {
                            columnId: si.columnId,
                            sortDirection: si.sortDirection
                        };
                    });

                    onDataGridSorted();
                }
            });
        }

        function onDataGridSorted() {
            var reportbuilderoptions, metadataManager,
                isRollupReport = model.aggregateDataManager ? model.aggregateDataManager.hasRollupData() : false;

            //reset all selected items
            gridOptions.selectedItems = [];
            reportModelManager.resetSelectedItems();

            // Reset paging info
            reportModelManager.resetPagingInfo();

            if (model.isEditMode && model.reportEntity) {
                reportBuilderService.updateOrderByToReport(gridOptions.sortInfo);
            }
            else {
                metadataManager = spReportMetadataManager(model.reportMetadata);

                metadataManager.updateSortInfoMetadata(gridOptions.sortInfo);

                model.hasAdHocSorting = true;

                // Get the latest data with the specified sort info
                reportModelManager.getReportData(defaultReportParams({includeMetadata: isRollupReport}));
            }
        }

        function updateReportRollupOptions(rollupRowCounts, rollupRowLabels) {
            var reportbuilderoptions = {};

            if (!_.isUndefined(rollupRowCounts) && !_.isNull(rollupRowCounts)) {
                reportbuilderoptions.rollupRowCounts = rollupRowCounts;
            }

            if (!_.isUndefined(rollupRowLabels) && !_.isNull(rollupRowLabels)) {
                reportbuilderoptions.rollupRowLabels = rollupRowLabels;
            }

            //reportBuilderService.setActionFromReportBuilder('updateReportRollupOptions', null, null, reportbuilderoptions);
            reportBuilderService.updateReportRollupOptions(reportbuilderoptions.rollupRowCounts, reportbuilderoptions.rollupRowLabels);
        }

        function getColumnDefinitionById(columnId) {
            if (!model || !gridOptions) {
                return null;
            }

            return _.find(gridOptions.columnDefinitions, function (cd) {
                return cd.columnId === columnId;
            });
        }

        // Save the collapsed / expanded state to the model when in edit mode
        function saveAggregateRowStateToModel(row, collapsed) {
            var groupColumnIds, groupColumnId;

            if (!row || !model.isEditMode || !model.reportEntity || !model.aggregateDataManager) {
                return;
            }

            // Update the report entity model
            groupColumnIds = _.map(model.aggregateDataManager.getGroupColumns(), 'id');
            groupColumnId = groupColumnIds[row.depth];
            reportBuilderService.setColumnGroupingCollapsedState(groupColumnId, collapsed);
        }

        function safeApply() {
            if (!$rootScope.$$phase) {
                $rootScope.$apply();
            }
        }

        function refreshReport() {
            return reportModelManager.refreshReportData(defaultReportParams({isRefresh: true}));
        }

        function updateCanInlineEdit() {
            canInlineEdit = false;

            if (!$scope.inlineEditingFeatureIsOn) {
                return;
            }

            const groups = sp.result(model.aggregateDataManager, 'getGroupColumns');
            if (_.some(groups, g => !g.rcol.rid)) {
                console.log('Inline editing not available due to grouping on non-relationship column');
                return;
            }

            // didn't find reason to not be on....
            canInlineEdit = true;
        }

        function enterInlineEditMode() {
            if (!$scope.inlineEditingFeatureIsOn || !canInlineEdit) {
                return;
            }

            resetInlineEditSession();
            setIsInlineEditing(true, $scope.model.isInPicker);

            let rowIndex = getSelectedRowIndex();
            if (rowIndex < 0) {
                rowIndex = 0;
            }
            reportModelManager.updateInlineEditingMetadata(rowIndex);
        }

        function cancelInlineEdits() {
            const attempts = _.filter(model.inlineEditState.session, 'lastSaveResult');
            const okSaves = _.filter(attempts, {lastSaveResult: 'ok'});

            cancelInlineEditsPending = false;                                                
            spInlineEditService.endSession(model.inlineEditState.session);
            model.inlineEditState.session = null;
            setIsInlineEditing(false, $scope.model.isInPicker);            

            if (okSaves.length < attempts.length) {
                // Have ok and failed saves. We need to refresh the report so that succeeded saves
                // get latest data
                refreshReport();
            }
        }

        function saveInlineEdits() {

            if (!spInlineEditService.anyChanged(model.inlineEditState.session)) {
                setIsInlineEditing(false, $scope.model.isInPicker);
                return;
            }

            model.inlineEditBusy.isBusy = true;
            spInlineEditService.saveSession(model.inlineEditState.session).then(session => {
                model.inlineEditBusy.isBusy = false;
                model.inlineEditState.session = session;

                const attempts = _.filter(session, 'lastSaveResult');
                const saves = _.filter(attempts, {lastSaveResult: 'ok'});

                if (saves.length) {
                    $scope.$root.$broadcast('invalidateScreenResources', _.map(saves, 'eid'));
                    spAlertsService.addAlert(`Saved changes to ${saves.length} record(s)`, {expires: 3});
                }

                if (saves.length < attempts.length) {
                    //TODO - fix up this handling. At the moment it simply shows a general error
                    spAlertsService.addAlert(`Failed to save changes to ${attempts.length - saves.length} record(s)`,
                        {severity: 'error'});

                } else {
                    // no errors so refresh and stop editing
                    cancelInlineEditsPending = true;
                    refreshReport();
                }
            }, () => {
                model.inlineEditBusy.isBusy = false;
            });
        }

        function isInlineEditSaving() {
            return model.inlineEditBusy.isBusy || cancelInlineEditsPending;
        }

        function initialiseInlineEditing() {

            model.inlineEditBusy = {isBusy: false, text: 'saving'};
            model.inlineEditState = getInlineEditAppState();

            //console.log(`initialiseInlineEditing scope id=${$scope.$id} host id=${$scope.hostId} report id=${$scope.options.reportId} session exists ${!!model.inlineEditState.session}`);

            if (!model.inlineEditState.session) {
                resetInlineEditSession();
            }
            setIsInlineEditing(!!model.inlineEditState.isInlineEditing, $scope.model.isInPicker);

            if ($scope.inlineEditingFeatureIsOn && !getInlineEditAppState().isDirty) {
                getInlineEditAppState().isDirty = () => anyInlineEditingChanges();
            }
        }

        function resetInlineEditSession() {
            spInlineEditService.endSession(model.inlineEditState.session);
            model.inlineEditState.session = spInlineEditService.startSession();
        }

        function getInlineEditAppState() {
            // TODO - need a better key that doesn't involve the reportId as this
            // might fail if we have a page that has the same report on it twice
            // e.g. say on a screen

            // urg... if we are used on the launch page we won't have a current nave item
            // so don't use spState ... it assumes we do. this is something that needs to be fixed.
            // For now return empty object...
            if (!spState.navItem) return {};

            return spState.getComponentState(`inlineEdit-${$scope.hostId || ''}-${$scope.options.reportId || ''}`);
        }

        /**
         * Must be fast, called from grid cell template
         * Return truthy if this cell is on the selected row
         * or if for a resource that exists in the current editing session.
         * @param {number} rowIndex
         * @returns {*}
         */
        function getIsInlineEditing(rowIndex) {
            return !!getInlineEditingState(rowIndex);
        }

        /**
         * Must be fast, called from grid cell template
         * Return 'edit' or 'view' depending on whether actively editing or just viewing prev changes
         * Return 'error' if there is a problem with the resource, either validation or prev save
         * attempt failed.
         * Return '' if not editing.
         * @param {number} rowIndex
         * @returns {string|undefined}
         */
        function getInlineEditingState(rowIndex) {
            if (!model.isInlineEditing) return;

            const row = getRow(rowIndex);
            if (!row || !row.eid) return;

            const metadata = model.inlineEditingMetadata[row.eid];
            const canModify = metadata ? metadata.canModify : false;

            if (!canModify) return;

            if (spInlineEditService.isInError(model.inlineEditState.session, row.eid)) {
                return 'error';
            }
            if (spInlineEditService.isSaved(model.inlineEditState.session, row.eid)) {
                return 'saved';
            }
            if (!isInlineEditSaving() && row.eid === getSelectedRowEid()) {
                return 'edit';
            }
            if (spInlineEditService.isChanged(model.inlineEditState.session, row.eid)) {
                return 'changed';
            }
        }

        function getInlineEditingTemplate(rowIndex, colIndex) {
            // must be fast, called from grid cell template

            return 'reports/cellEditTemplate.tpl.html';
        }

        /**
         * Must be fast - called from a template.
         * @param {number} rowIndex
         * @returns {Object|null}
         */
        function getInlineEditingMessages(rowIndex) {
            if (!model.isInlineEditing || !model.inlineEditState.session) return;

            const row = getRow(rowIndex);
            if (!row || !row.eid) return;

            //TODO complete this... the idea is that we can display the validation issues
            //somewhere on the report...
            //- but a problem is that this is only collecting issues for the controls
            // for the columns... what if other controls cause validation issues...
            // but then we'd like to remove those other controls....
            // Just not sure about all this

            const state = spInlineEditService.getEditingState(model.inlineEditState.session, row.eid);
            if (state) {
                // state.validationMessages is an array of message lists
                // the array being indexed by colIndex
                // - convert to map by col index to string array of messages
                const messages = _.reduce(state.validationMessages, (a, v, i) => {
                    if (!_.isEmpty(v)) {
                        a[i.toString()] = v;
                    }
                    return a;
                }, {});
                //console.log('validation => ', messages);
                return messages;
            }
        }

        function setIsInlineEditing(value, isInPicker) {
            model.inlineEditState.isInlineEditing = value;

            // sync derived values to control other elements and for fast and convenient access
            model.isInlineEditing = value;
            gridOptions.isInlineEditing = value;
            gridOptions.hideGridContextMenu = value || isInPicker;
            model.refreshButtonOptions.disabled = value;
        }

        function getResourceEditingState(rowIndex, colIndex) {

            if (!model.inlineEditState.session) return null;

            console.assert(rowIndex >= 0 && rowIndex < gridOptions.rowData.length);
            console.assert(colIndex >= 0 && colIndex < gridOptions.rowData[0].cells.length);

            const row = getRow(rowIndex);
            const col = getCol(colIndex);
            if (!row && !col) {
                console.error(`row index ${rowIndex} or col index ${colIndex} not found to edit`);
                return null;
            }

            // todo - if the col represents an attr on a related resource then work out...
            // the related resource id .... for now ignore these

            const resourceId = row.eid;

            let state = spInlineEditService.getEditingState(model.inlineEditState.session, resourceId);
            if (state) {
                // we already have state for this resource
                return state;
            }

            // Get the resource viewer for the report
            let formId = model.reportMetadata.rvfid;
            if (!formId) {
                // Don't have one fallback to the form for the row
                const metadata = model.inlineEditingMetadata[resourceId];
                formId = metadata ? metadata.formId : null;

                if (!formId) {
                    // Still don't have one fall back to the default form for the root type
                    formId = model.reportMetadata.dfid;
                }
            }

            const attrIds = []; // todo create list of field and rel ids editable for this resource

            // The following ensures state for the given resource and kicks off async requests that
            // will populate it with the form and resource.

            return spInlineEditService.startEditing(model.inlineEditState.session, resourceId, formId, attrIds);
        }

        function getSelectedRowIndex() {
            //debug - todo remove
            // const r1 = sp.result($scope.options.selectedItems, [0, 'rowIndex']);
            // const r2 = sp.result(gridOptions.selectedItems, [0, 'rowIndex']);
            // const r3 = sp.result(model.selectedItems, [0, 'rowIndex']);
            // if (!(r1 === r2 && r1 === r3)) {
            //     console.warn('unexpected difference between selectedItems', r1, r2, r3);
            // }
            //end debug
            if (!model.firstSelectedItem) {
                return -1;
            }

            return model.firstSelectedItem.rowIndex;
        }

        function getSelectedRowEid() {
            if (!model.firstSelectedItem) {
                return -1;
            }

            return model.firstSelectedItem.eid;
        }

        function getFormControl(state, colIndex) {
            return spInlineEditService.getFormControl(state, getCol(colIndex));
        }

        function getCol(index) {
            return _.find(model.reportMetadata.rcols, c => c.ord === index);
        }

        function getRow(index) {
            const {rowData} = gridOptions;
            return index >= 0 && index < rowData.length ? rowData[index] : null;
        }

        function anyInlineEditingChanges() {
            return spInlineEditService.anyChanged(model.inlineEditState.session);
        }

        function getMetadata() {
            return model.reportMetadata;
        }

        function onDataGridCellLinkClicked(event, args) {
            if (!event || !args) {
                return;
            }

            event.stopPropagation();

            if (model.isEditMode || model.isInDesign || model.isInlineEditing || model.isInPicker) {
                return;
            }            
            
            const columnIndex = args.columnIndex;

            const rowData = getRow(args.rowIndex);
            const col = getCol(columnIndex);

            if (!col || !rowData) {
                return;
            }

            let entityId = -1;
            let params = {};

            if (col.entityname) {
                entityId = rowData.eid;

                if (model.reportMetadata) {
                    // Get the resource viewer for the report
                    const formId = model.reportMetadata.rvfid || model.reportMetadata.dfid;
                    if (formId) {
                        params.formId = formId;
                    }
                }
            } else {
                if (!rowData.cells || columnIndex >= rowData.cells.length) {
                    return;
                }

                const value = sp.result(rowData.cells[columnIndex], "value");
                if (!value) {
                    return;
                }

                const keys = _.keys(value);
                if (!keys || keys.length > 1) {
                    return;
                }

                entityId = _.first(keys);                
            }            

            if (entityId <= 0 || !_.isFinite(_.parseInt(entityId))) {
                return;
            }            

            if (args.openInNewTab) {             
                const win = $window.open(spNavService.getChildHref("viewForm", entityId, params), "_blank");
                win.focus();
            } else {
                spNavService.navigateToChildState("viewForm", entityId, params);
            }            
        }
    }

}());
