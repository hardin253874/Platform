// Copyright 2011-2016 Global Software Innovation Pty Ltd
/*global _, console, $, angular, sp, spEntity */

(function () {
    'use strict';

    /**
     * Module containing functions for interacting with console actions.
     *
     * @module spActionsService
     */
    angular.module('mod.common.ui.spActionsService', [
        'ng',
        'ui.bootstrap.modal',
        'mod.common.alerts',
        'mod.common.spWebService',
        'mod.common.spEntityService',
        'sp.navService',
        'mod.services.workflowRunService',
        'mod.common.spXsrf',
        'mod.common.spUserTask',
        'mod.services.promiseService',
        'mod.app.reportProperty',
        'mod.app.reportTemplateService',
        'mod.app.exportServices',
        'mod.common.ui.spChartService',
        'mod.common.ui.spDialogService',
        'mod.app.navigation.appElementDialog',
        'mod.app.navigation.spNavigationElementDialog',
        'mod.app.chartBuilder.controllers.spNewChartDialog',
        'mod.common.ui.spEditFormDialog',
        'mod.app.formBuilder.spNewTypeDialog',
        'mod.common.ui.spDeleteService',
        'sp.app.settings',
        'mod.app.editForm',
        'mod.app.spExportXml'
    ]);

    angular.module('mod.common.ui.spActionsService')
        .factory('spActionsService', spActionsService);

    /* @ngInject */
    function spActionsService($http, $q, $parse, $uibModalStack, spWebService, spEntityService, spNavService, spAlertsService, spWorkflowRunService, spUserTask,
                              spPromiseService, rptTemplateService, spExportService, spXsrf, spChartService, spDialogService, appElementDialog,
                              spReportPropertyDialog, spNavigationElementDialog, spNewChartDialog, spEditFormDialog, spNewTypeDialog, spDeleteService, spAppSettings, spEditForm, spExportXml) {
        var exports = {};
        var selectedId;
        var selectedIds = [];
        var loadedActions = [];
        var templateReport;
        var formActionQuery = '{ k:resourceConsoleBehavior }' +
                                '.k:behaviorActionMenu.{ ' +
                                '   { k:menuItems, k:suppressedActions, k:includeActionsAsButtons }.{ ' +
                                '      { name, ' +
                                '        description, ' +
                                '        k:menuIconUrl, ' +
                                '        htmlActionState, ' +
                                '        htmlActionMethod, ' +
                                '        k:isActionButton, ' +
                                '        k:appliesToSelection, ' +
                                '        k:isMenuSeparator, ' +
                                '        k:menuOrder, ' +
                                '        { isOfType }.{ alias,name }, ' +
                                '        { k:actionMenuItemToWorkflow }.{ name }, ' +
                                '        { k:actionMenuItemToReportTemplate }.{ name } ' +
								'      } ' +
                                '   }, ' +
                                '   { k:includeTypesForNewButtons, k:suppressedTypesForNewMenu }.id ' +
                                '}';

        exports.getConsoleActions = function (actionRequest) {
            var url = spWebService.getWebApiRoot() + '/spapi/data/v1/actions';
            
            if (actionRequest) {
                if (actionRequest.ids && actionRequest.ids.length) {
                    selectedIds = actionRequest.ids;
                }
                if (actionRequest.lastId) {
                    selectedId = actionRequest.lastId;
                }

                return $http({
                    method: 'POST',
                    url: url,
                    headers: spWebService.getHeaders(),
                    data: actionRequest
                }).then(function (response) {
                    if (angular.isDefined(response) && angular.isDefined(response.data)) {
                        var menuKey = '';
                        if (actionRequest.display) {
                            menuKey += actionRequest.display;
                        }
                        if (actionRequest.hostIds && actionRequest.hostIds.length > 0) {
                            menuKey += actionRequest.hostIds.join('');
                        }
                        updateActionsEnabled(response.data.actions, actionRequest);
                        setActions(response.data.actions, menuKey);
                        return response.data;
                    }
                    return null;
                });
            }

            return undefined;
        };

        exports.getActionsMenu = function (actionRequest) {
            var url = spWebService.getWebApiRoot() + '/spapi/data/v1/actions/menu';

            if (actionRequest) {
                return $http({
                    method: 'POST',
                    url: url,
                    headers: spWebService.getHeaders(),
                    data: actionRequest
                }).then(function (response) {
                    if (angular.isDefined(response) && angular.isDefined(response.data)) {
                        return response.data;
                    }
                    return null;
                });
            }

            return undefined;
        };

        exports.getFormActionsMenu = function (actionRequest) {
            var url = spWebService.getWebApiRoot() + '/spapi/data/v1/actions/formActions';

            if (actionRequest) {
                return $http({
                    method: 'POST',
                    url: url,
                    headers: spWebService.getHeaders(),
                    data: actionRequest
                }).then(function (response) {
                    if (angular.isDefined(response) && angular.isDefined(response.data)) {
                        return response.data;
                    }
                    return null;
                });
            }

            return undefined;
        };

        exports.getFormActionButtons = function (formId) {
            var actionBtns = [];

            if (formId && _.isNumber(formId) && formId > 0) {
                return spEntityService.getEntity(formId, formActionQuery).then(function (result) {
                    return result;
                });
            }

            return actionBtns;
        };

        exports.executeItem = function (itemId, contextCreate) {
            // fish out the action that was clicked by its index
            var action = getAction(itemId);

            if (!action)
                return;

            var ids = [];
            var id = action.eid > 0 ? action.eid : selectedId;
            if (id > 0) {
                ids = [id];
            }
            if (selectedIds && selectedIds.length > 0) {
                ids = selectedIds;
            }
            
            var ctx = {state: action.state, selectionEntityIds: ids};
            if (contextCreate) {
                $q.when().then(function () {
                    return contextCreate(action, ids);
                }).then(function (c) {
                    executeAction(action, c || ctx);
                });
            } else {
                executeAction(action, ctx);
            }
        };

        exports.updateActions = function (reportId, hostIds, actions) {
            var url = spWebService.getWebApiRoot() + '/spapi/data/v1/actions/update';

            return $http({
                method: 'POST',
                url: url,
                headers: spWebService.getHeaders(),
                data: {
                    reportId: reportId,
                    hostIds: hostIds,
                    actions: actions
                }
            }).then(function (response) {
                if (angular.isDefined(response) && angular.isDefined(response.data)) {
                    return response.data;
                }
                return undefined;
            });
        };

        exports.setActions = setActions;

        exports.getAction = getAction;

        exports.executeAction = executeAction;

        exports.ensureSelected = function ensureSelected(ids) {
        };

        // Helpers
        exports.getShortName = function (name) {
            return (name && name.length > 30) ? name.substring(0, 26) + '...' : name;
        };

        exports.getEntityIdsFromDataGridSelection = function (selectedItems, columns) {
            var ids = [];

            // Try get the entity ids (if given)
            if (_.some(selectedItems, 'eid')) {
                ids = _.map(selectedItems, function (item) {
                    return item.eid;
                });
            }

            // Fallback. Get the first column of interest (ending with 'Id').
            if (angular.isUndefined(ids) || ids.length <= 0) {
                var idColumn = _.find(columns, function (columnDefinition) {
                    if (angular.isDefined(columnDefinition.displayName)) {
                        return (columnDefinition.displayName.match(/Id$/) && columnDefinition.type === 'Int32');
                    }
                    return false;
                });

                if (angular.isDefined(idColumn)) {
                    // Then get the values
                    ids = _.map(selectedItems, function (item) {
                        return item.cells[idColumn.cellIndex].value;
                    });
                }
            }

            return ids;
        };

        exports.getActionsOnResource = function (id) {
            var menuItemQuery = 'alias, name, description, ' +
            'isOfType.{alias, name}, ' +
            'htmlActionTarget, htmlActionMethod, htmlActionState, ' +
            'k:menuOrder, ' +
            'multiSelectName, emptySelectName, ' +
            'k:menuIconUrl, ' +
            'k:isMenuSeparator, k:isActionButton, k:isActionItem, k:isContextMenu, k:isSystem, ' +
            'k:appliesToSelection, k:appliesToMultiSelection, '+
            'k:actionRequiresPermission.isOfType.id, ' +
            'k:actionRequiresParentPermission.isOfType.id, ' +
            'k:actionMenuItemToWorkflow.{ isOfType.name, name, inputArgumentForRelatedResource.name }, ' +
            'k:navigateToCreateFormActionDefinition.name, k:navigateToCreateFormActionForm.name';
            var menuQuery = 'isOfType.id, ' +
            'k:showNewActionsButton, ' +
            'k:suppressNewActions, ' +
            'k:showExportActionsButton, ' +
            'k:showEditInlineActionsButton, ' +
            '{ k:menuItems, k:includeActionsAsButtons }.{' + menuItemQuery + '},' +
            'k:suppressedActions.id, k:suppressedTypesForNewMenu.id, k:includeActionsAsButtons.id, k:includeTypesForNewButtons.name';
            var behaviourQuery = 'isOfType.id, ' +
            'k:html5CreateId, ' +
            'k:suppressActionsForType, ' +
            'k:behaviorActionMenu.{' + menuQuery + '}';
            var query = 'alias, name, k:resourceConsoleBehavior.{' + behaviourQuery + '}';
            return spEntityService.getEntity(id, query);
        };

        // pull an action from its location given a path in the structure
        function getAction(itemId) {
            var location = (angular.isDefined(itemId) && (('' + itemId).indexOf(',') < 0)) ? ['', itemId] : ('' + itemId).split(',');

            if (location && location.length > 0) {
                var menuKey = location[0];
                var menu = _.find(loadedActions, {'key': menuKey});
                if (menu) {
                    var n = location.length > 1 ? 1 : 0;
                    var idx = _.isNumber(itemId) ? itemId : location[n];
                    var action = menu.actions[idx];
                    n++;

                    while (n < location.length) {
                        idx = location[n];
                        action = action.children[idx];
                        n++;
                    }

                    return action;
                }
            }

            return null;
        }

        // place any action loaded into a structure for retrieval
        function setActions(actions, menuKey) {
            if (!menuKey) {
                menuKey = '';
            }
            var menu = _.find(loadedActions, {'key': menuKey});
            if (menu) {
                menu.actions.length = 0;
                menu.actions = actions;
            } else {
                loadedActions.push({key: menuKey, actions: actions});
            }
        }

        // -- Private Methods --

        // Updates the enabled state of certain actions based on the context and configuration
        function updateActionsEnabled(actions, request) {
            _.forEach(actions, function (action) {
                if (action && action.isenabled) {

                    // custom actions disabled
                    var foundCanFalse = false;
                    var stateIndicatesEnable = (action.method === 'custom' || action.method === 'delete');
                    if (stateIndicatesEnable && action.state) {
                        var getter = $parse('can' + action.state);
                        if (request && request.selected) {
                            _.forEach(request.selected, function (selected) {
                                if (!foundCanFalse) {
                                    foundCanFalse = getter(selected) === false;
                                }
                            });
                        }
                    }

                    action.isenabled = !foundCanFalse;
                }
            });
        }

        // Gets the template report. A special report that exists in the system.
        function getTemplateReport() {
            if (!templateReport) {
                return spEntityService.getEntity('core:templateReport', 'name').then(function (report) {
                    templateReport = report;
                    return templateReport;
                });
            }

            return $q.when().then(function () {
                return templateReport;
            });
        }

        // Looks at the current state of selection and action data to build an appropriate message to confirm a delete.
        function getDeleteMessage(ids, data) {
            var deferred = $q.defer();
            var msg = 'Are you sure that you want to delete this?';

            if (ids && ids.length) {
                if (ids.length > 1) {
                    msg = 'Are you sure that you want to delete these ' + ids.length + ' items?';
                } else {
                    if (data && data['%Resource%']) {
                        if (data['%Resource%'] === data['%Type%']) {
                            return spEntityService.getEntity(ids[0], 'name').then(function (e) {
                                if (e.getName()) {
                                    return 'Are you sure that you want to delete \'' + e.getName() + '\'?';
                                } else {
                                    return msg;
                                }
                            });
                        } else {
                            msg = 'Are you sure that you want to delete \'' + data['%Resource%'] + '\'?';
                        }
                    }
                }
            }

            deferred.resolve(msg);
            return deferred.promise;
        }

        // Executes an action based on the settings of the action object
        function executeAction(action, context) {
            var fn = function () {
                return $q.when();
            };

            if (!context.isEditMode) {
                // perform the correct action
                // !!! NOTE !!! - all these functions should return a promise
                switch (action.method) {
                    case 'navigate':
                        fn = navigate;
                        break;
                    case 'drilldown':
                        fn = drilldown;
                        break;
                    case 'delete':
                        fn = deleteEntities;
                        break;
                    case 'run':
                        fn = runWorkflow;
                        break;
                    case 'generate':
                        fn = generateDocument;
                        break;
                    case 'export':
                        fn = exportReport;
                        break;
                    case 'exportXml':
                        fn = exportXml;
                        break;
                    case 'cancelWorkflowRun':
                        fn = cancelWorkflowRun;
                        break;
                    case 'custom':
                        fn = executeFunction;
                        break;
                }
            }

            return fn(action, context).finally(function () {
                if (context) {
                    // Check if the action did not run for any reason
                    if (context.aborted === true) {
                        context.aborted = false;
                        return;
                    }

                    if (context.scope) {
                        // UN-HACK? Reports no longer seem to clear selection on reload
                        //if (context.scope.options && context.scope.options.selectedItems) {
                        //    context.scope.options.selectedItems.length = 0;
                        //}
                        context.scope.$broadcast('actionExecuted', action.method);
                    }
                }
            });
        }

        // Navigate to a form for an entity
        function navigate(action, context) {
            return $q.when().then(function () {
                
                if (context.disallowCreateRelatedEntityInNewMode) {
                    var msg = spEditForm.getDisallowCreateRelatedEntityInNewModeMessage(context.scope.relationshipToRender, context.scope.isReversed);
                    spAlertsService.addAlert(msg, { severity: spAlertsService.sev.Error });
                    return;
                }

                var ids = context.selectionEntityIds;
                var id = action.eid;
                if (ids && ids.length > 0 && action.isselect) {
                    id = ids[0];
                }
                var stateName = action.state;
                var params = { eid: id };
                params.returnToParent = context.returnToParent;

                if (action) {
                    if (action.data) {
                        
                        // 'Special' create actions that use dialogs
                        if (action.state === 'createForm' && action.data.Dialog) {
                            switch (action.data.Dialog) {
                                case 'newScreen': newScreen(action, context); break;
                                case 'newReport': newReport(action, context); break;
                                case 'newChart': newChart(action, context); break;
                                case 'newDefinition': newDefinition(action, context); break;
                                case 'newApplication': newApplication(action, context); break;
                                default: console.error('unsupported custom create dialog: ' + action.data.Dialog);
                            }
                            return;
                        }

                        if (action.data.TypeDefaultForm) {
                            var key = '' + action.eid;
                            if (_.has(action.data.TypeDefaultForm, key)) {
                                var fid = action.data.TypeDefaultForm[key];
                                if (fid && _.isNumber(fid)) {
                                    params.formId = fid;
                                }
                            }
                        }

                        if (action.data.CustomForm) {

                            if (action.state !== 'createForm' || (action.state === 'createForm' && action.data.CustomFormEditsTypeId === action.eid)) // if creating new, only use custom form if it is for the same type/definition as of instance being created (and not of a derived type)
                            params.formId = action.data.CustomForm;
                        }
                    }
                }

                var proceed = true;
                if (context.onBeforeNavigate) {
                    proceed = context.onBeforeNavigate(stateName, id, params);
                }

                // Perform the navigation action
                if (proceed !== false) {

                    // navigation breaks badly if we are somehow already in a dialog
                    var modalsExist = !!$uibModalStack.getTop();
                    if (!modalsExist) {
                        spNavService.navigateToChildState(stateName, id, params);
                    } else {
                        if (params.formId) {
                            newWhatever(action, context, params.formId);
                        }
                    }
                }
            });
        }

        // Drills down to a report showing instances related to the initial selection
        function drilldown(action, context) {
            return getTemplateReport().then(function (tr) {
                var query = 'defaultDisplayReport.id, inherits.defaultDisplayReport.id';
                var id = context.selectionEntityIds[0];

                // get default display report for type
                spEntityService.getEntity(id, query).then(function (drilldownData) {
                    var reportId = 0;

                    if (drilldownData) {
                        if (drilldownData.defaultDisplayReport) {
                            reportId = drilldownData.defaultDisplayReport.idP;
                        } else {
                            var displayReport = _.find(drilldownData.inherits, function (inheritedType) {
                                return inheritedType.defaultDisplayReport;
                            });
                            if (displayReport) {
                                reportId = displayReport.defaultDisplayReport.idP;
                            } else {
                                if (tr) {
                                    // if falling back to template
                                    reportId = tr.idP;
                                }
                            }
                        }
                    }

                    return reportId;
                }).then(function (rid) {
                    if (rid && rid > 0) {
                        // navigate to the report as a child, and filter by type
                        var params = {
                            eid: rid,
                            typeIdFilter: id
                        };
                        spNavService.navigateToChildState('drilldown', rid, params);
                    }
                });
            });
        }

        // Deletes many entities
        function deleteEntities(action, context) {
            var ids = context.selectionEntityIds;
            var title = 'Confirm delete';

            return getDeleteMessage(ids, action.data).then(function (message) {
                var btns = [
                    {result: true, label: 'OK'},
                    {result: false, label: 'Cancel'}
                ];
                
                var options = {
                    title: title,
                    message: message,
                    ids: ids,
                    btns: btns
                };

                return spDeleteService.showDialog(options).then(function (result) {
                    if (result === true) {

                        spNavService.middleLayoutBusy = true;

                        return spEntityService.deleteEntities(ids).then(function () {
                            spNavService.setCacheMarker();

                            if (context && context.refreshDataCallback) {
                                return $q.when().then(function (data) { return context.refreshDataCallback('delete', data); });
                            }
                            return $q.when();
                        }).finally(function () {
                            spNavService.middleLayoutBusy = false;
                        });
                    }
                    context.aborted = true;
                    return $q.when();
                });
            });
        }

        function getInputParamName(action) {
            return action.state;
        }

        function getRelatedResourceParamName(action) {
            if (action.data && action.data.RelatedResourceArg) {
                return action.data.RelatedResourceArg;
            } else {
                return undefined;
            }
        }

        function createArgument(argName, value) {
            return { name: argName, typeName: 'core:resourceArgument', value: value };
        }

        function createListArgument(argName, values) {
            return { name: argName, typeName: 'core:resourceListArgument', value: values };
        }

        // Runs a given workflow with an optional parameter
        function runWorkflow(action, context) {

            var workflowArgs = [];
            var inputParamName = getInputParamName(action);
            var relatedRelParamName = getRelatedResourceParamName(action);

            // The selected entity
            if (inputParamName) {
                var ids = context.selectionEntityIds;
                if (ids && ids.length > 0 && ids[0] && action.isselect) {
                    if (action.ismultiselect) {
                        workflowArgs.push(createListArgument(inputParamName, JSON.stringify(ids)));
                    } else {
                        workflowArgs.push(createArgument(inputParamName, '' + ids[0]));
                    }
                }
            }

            // The related entity (in the case of a tab relationship, the entity the tab is on.
            if (relatedRelParamName && context.entityContextId) {
                workflowArgs.push(createArgument(relatedRelParamName, '' + context.entityContextId));
            }

            spNavService.middleLayoutBusy = true;

            var options = { cancel: false };

            return spWorkflowRunService.runWorkflow(action.eid, workflowArgs).then(function (tag) {
                return spWorkflowRunService.waitForRunToStopWithThrow(tag, options).then(function(runId) {
                    return spUserTask.waitToNavigateToFollowOnTasks(runId, false, true).then(function (result) {
                        spNavService.setCacheMarker();

                        // If we are not doing any navigating, or if its a survey, do a refresh.
                        var isSurvey = sp.result(result, 'type.nsAlias') === 'core:userSurveyTask';

                        if (!result || isSurvey) {
                            context.refreshDataCallback();
                        }

                        return result;
                    });
                });
            }).catch(function (error) {
                var msg = error ? (error.message || sp.result(error, 'data.Message') || error) : 'An error occurred.';
                spAlertsService.addAlert(action.name + ' failed. ' + msg, { severity: spAlertsService.sev.Error });
            }).finally(function () {
                spNavService.middleLayoutBusy = false;
            });
        }

        // Cancels a workflow run
        function cancelWorkflowRun(action, context) {
            var ids = context.selectionEntityIds;
            return confirmCancelRunDialog(ids).then(function () {
                return $q.all(_.forEach(ids, function (runId) {
                    return spWorkflowRunService.cancelRun(runId)
                        .then(context.refreshDataCallback);
                })); 
            });
        }
        
        // prompt, terminate the promise on a no. 
        function confirmCancelRunDialog(ids) {
            var message = ids.count > 1 ? "Cancelling " + ids.count + " workflow runs. Are you sure?" : "Cancelling the workflow run. Are you sure?";
            return spDialogService.showMessageBox("Cancel Runs", message, [
                        { result: 'yes', label: 'Yes', cssClass: 'btn-primary' },
                        { result: 'no', label: 'No' }
            ]).then(function (result) {
                if (result === 'yes')
                    return $q.when();
                else
                    return $q.reject();
            });
        }
        
        // Download the entity as an xml document
        function exportXml(action, context) {
            return $q.when().then(function () {
                var ids = context.selectionEntityIds;
                spExportXml.exportEntities(ids);
            });
        }

        // Generates a document from the current selection and applied template
        function generateDocument(action, context) {
            var token = '';
            var templateId = 0;
            var ids = context.selectionEntityIds;
            if (action && action.data) {
                templateId = action.data.ReportTemplateId;
            }

            spNavService.middleLayoutBusy = true;

            // Checks the status to see if complete
            function checkGenerateDocumentDone(result) {
                console.log('Document Generation: ' + result.status);
                return ((result.status === 'Success') || (result.status === 'Failed'));
            }

            // Gets the current document generation status
            function getGeneratedDocumentStatus() {
                return rptTemplateService.getLongRunningProgress(token);
            }

            // Moves to download the generated document
            function generateDocumentDone(result) {
                var msg = 'Failed to generate the document.';
                if (result.status === 'Failed') {
                    if (result.errormsg) {
                        msg = msg + ' ' + result.errormsg;
                    }
                    throw new Error(msg);
                }
                if (!result.result) {
                    if (result.status === 'InProgress') {
                        throw new Error(msg + ' Maximum number of retries exceeded.');
                    }
                    throw new Error(msg + ' Location of generated document is invalid.');
                }
                return rptTemplateService.getGeneratedDocument(result.result).then(function () {
                    console.log('Document Generated! ID=' + result.result);
                });
            }

            return rptTemplateService.generateDocument(templateId, ids).then(function (t) {
                token = t;

                // poll, 600 retries, 1 second intervals. Give it 10 minutes to finish
                return spPromiseService.poll(getGeneratedDocumentStatus, checkGenerateDocumentDone, 600, 1000).then(function (result) {
                    return generateDocumentDone(result);
                });
            }).catch(function (error) {
                var msg = 'Failed to generate the document.';
                if (error.message) {
                    msg = error.message;
                }
                spAlertsService.addAlert(msg, 'error');
            }).finally(function () {
                spNavService.middleLayoutBusy = false;
            });
        }

        // Exports report data to a variety of supported formats
        function exportReport(action, context) {
            var format = action.state;
            spNavService.middleLayoutBusy = true;
            var title, reportId, reportParameter;
            if (context.scope.formControl && context.scope.displayReportOptions) {
                //report is on the form.
                var options = context.scope.displayReportOptions;
                title = options.title;
                reportId = options.reportId;
                reportParameter = {
                    sort: options.reportModel.reportMetadata.sort,
                    conds: options.reportModel.analyzerOptions.conds,
                    qsearch: options.reportModel.quickSearch.value,
                    relationDetail: options.relationDetail,
                    entityTypeId: options.entityTypeId
                };
            } else {
                var model = context.scope.model;
                title = model.reportMetadata.title;
                reportId = model.reportId;
                reportParameter = {
                    sort: model.reportMetadata.sort,
                    conds: model.conds,
                    qsearch: model.quickSearch.value
                };
            }

            return spExportService.exportData(format, reportId, reportParameter).then(function (result) {
                if (result.responseMessage) {
                    var msgTitle = 'Exported Info';
                    var message = result.responseMessage;
                    var btns = [
                        {result: true, label: 'OK'}
                    ];
                    spDialogService.showMessageBox(msgTitle, message, btns);
                }
                spExportService.getExportedDocument(result.fileHash, title, format);
            }).catch(function (error) {
                console.error('spActionsService.exportReport error:', error);
                spAlertsService.addAlert('Failed to export the report data.', 'error');
            }).finally(function () {
                spNavService.middleLayoutBusy = false;
            });
        }
        
        // Executes an arbitrary function
        function executeFunction(action, context) {
            var functionName = action.state;
            var scope = context.scope;

            if (functionName && scope) {
                var fn = $parse(functionName);
                var call = fn(scope);
                if (call && _.isFunction(call)) {
                    var callResult = call(scope);
                    var callResultThen = function(result) {
                        spNavService.setCacheMarker();

                        if (context && context.refreshDataCallback && result !== false) {
                            return $q.when().then(context.refreshDataCallback);
                        }

                        return $q.when();
                    };
                    return $q.when(callResult).then(callResultThen);
                }
            }
            return $q.when();
        }

        // Custom creation overrides involving dialogs
        function newChart(action, context) {
            var options = {
                solution: spNavService.getCurrentApplicationId(),
                preventBuilderNavigation: true
            };
            spNewChartDialog.showDialog(options).then(function (result) {
                if (result) {
                    result.chartEntity.setId(result.chartId);
                    result.chartEntity.markAllUnchanged();
                    spNavService.navigateToSibling('chart', result.chartId).finally(function () {
                        spNavService.navigateToChildState('chartBuilder', result.chartId);
                    });
                }
            });
        }

        function newReport(action, context) {
            var options = { solution: spNavService.getCurrentApplicationId() };
            spReportPropertyDialog.showModalDialog(options).then(function(result) {
                if (result) {
                    result.report.setId(result.reportId);
                    result.report.markAllUnchanged();
                    spNavService.navigateToSibling('report', result.reportId).finally(function () {
                        spNavService.navigateToChildState('reportBuilder', result.reportId);
                    });
                }
            });
        }

        function newScreen(action, context) {
            var screen = {
                typeId: 'console:screen',
                'console:navigationElementIcon': jsonLookup(),
                name: 'New Screen',
                description: '',
                hideOnDesktop: false,
                hideOnTablet: true,
                hideOnMobile: true,
                inSolution: jsonLookup(spNavService.getCurrentApplicationId()),
                isPrivatelyOwned: !spAppSettings.publicByDefault
            };
            var options = {
                title: 'New Screen',
                entity: spEntity.fromJSON(screen)
            };
            spNavigationElementDialog.showDialog(options).then(function (result) {
                if (result && result.entity) {
                    spNavService.navigateToSibling('screen', result.entity.id()).finally(function () {
                        spNavService.navigateToChildState('screenBuilder', result.entity.id());
                    });
                }
            });
        }

        function newDefinition(action, context) {
            var options = {};
            spNewTypeDialog.showDialog(options);
        }

        function newApplication(action, context) {
            var applicaion = {
                typeId: 'core:solution',
                'console:applicationIcon': jsonLookup(),
                'core:name': 'New Application',
                'core:description': '',
                'core:hideOnDesktop': false,
                'core:hideOnTablet': true,
                'core:hideOnMobile': true,
                'core:solutionVersionString': '1.0'
            };
            var options = {
                title: 'New Application',
                entity: spEntity.fromJSON(applicaion)
            };
            appElementDialog.showDialog(options).then(function(result) {
                if (context && context.refreshDataCallback && result !== false) {
                    context.refreshDataCallback();
                }
            });
        }

        function newWhatever(action, context, formId) {
            var options = {
                title: 'New ' + action.name,
                entity: spEntity.fromJSON({
                    typeId: action.eid
                }),
                form: formId,
                formMode: 'edit',
                optionsEnabled: false,
                saveEntity: true
            };
            spEditFormDialog.showDialog(options).then(function(result) {
                if (context && context.refreshDataCallback && result !== false) {
                    context.refreshDataCallback();
                }
            });
        }

        return exports;
    }
}());