// Copyright 2011-2016 Global Software Innovation Pty Ltd
/*global angular, _, console, sp */

(function () {
    'use strict';

    angular.module('app.editForm.formRenderControl', ['mod.app.editForm', 'spApps.reportServices',
        'mod.app.resourceScopeService', 'mod.common.alerts', 'mod.common.spUserTask', 'mod.common.spMobile',
        'sp.navService', 'mod.common.spReportDataCacheService']);

    angular.module('app.editForm.formRenderControl')
        .controller('formRenderControl', FormRenderControl)
        .factory('editCoordinator', editCoordinator);

    function FormRenderControl($scope, $q, spEditForm, spResourceScope, spAlertsService, editCoordinator,
                               spUserTask, spMobileContext, spState, spNavService, spReportDataCacheService) {
        "ngInject";

        var state = {};
        var pendingScopeUpdate;
        var contextWatches = [];
        var onScopeUpdateUnregisterListener;
        var isParentResourceAvailable;
        var channelId = spResourceScope.getChannelIdFromSender($scope.formControl);

        var editButton = {text: 'Edit', click: onEditClick, order: 0};
        var saveButton = {text: 'Save', click: onSaveClick, order: 1};
        var cancelButton = {text: 'Cancel', click: onCancelClick, order: 2};

        $scope.editSaveButtons = [editButton, saveButton, cancelButton];

        $scope.isMobile = spMobileContext.isMobile;

        $scope.requestStrings = [];     // a set of additional request string fragments that formControls can add to to get additional information

        $scope.formMode = spEditForm.formModes.view;
        $scope.isDisabled = false;

        $scope.onEditClick = onEditClick;

        $scope.onSaveClick = onSaveClick;

        $scope.onCancelClick = onCancelClick;

        $scope.onBeforeTransition = function () {
            if (isEditMode()) {
                return (save());
            } else {
                return $q.when();
            }
        };


        $scope.$on("enterEditModeFromDblclick", function (event) {
            event.stopPropagation();
            spUtils.safeApply($scope, function() {
                if (!isEditMode()) {
                    onEditClick();
                }
            });
        });

        $scope.$watch("formControl", formControlChanged);

        $scope.$watch('formData', formDataChanged);

        $scope.$watch('formMode', function (formMode) {
            setButtonsVisibility();
        });

        $scope.$watch('isDisabled || !formData', function (isDisabled) {
            editButton.disabled = isDisabled || !$scope.formData;
        });

        $scope.$on('spTabRelRenderCtrlEventIsParentResourceAvailable', function (event, callback) {
            // #23850. this event is to let tab render control know if it has to run report in context of a resource. If no resource is available then tab control does a dummy run of the report to bring the report columns.
            callback(isParentResourceAvailable);
        });

        $scope.$on('invalidateScreenResources', function (event, ids) {
            // note the current resource id, if any
            var id = sp.result(state, 'formData.idP');

            // ids should be a list of the ids to invalidate, but just clear all
            // since that is the routine we already have. todo - clear only those in ids list
            invalidateCache();

            // cause reload of existing, if any
            if (id){
                processScopeUpdate(id);
            }
        });

        $scope.$on('$destroy', function () {
            if (editCoordinator) {
                editCoordinator.unregisterIsDirty(isDirty);
                editCoordinator.unregisterSetDisabled(setDisabled);
            }

            if (onScopeUpdateUnregisterListener) {
                onScopeUpdateUnregisterListener();
                onScopeUpdateUnregisterListener = null;
            }
        });

        $scope.$on("addOnReturnFromChildCreate", function (event, onReturnFunction) {
            event.stopPropagation();
            $scope.model.onReturnFromChildCreateActions.push(onReturnFunction);
        });

        $scope.$on("addOnReturnFromChildUpdate", function (event, onReturnFunction) {
            event.stopPropagation();
            $scope.model.onReturnFromChildUpdateActions.push(onReturnFunction);
        });

        $scope.model = {
            taskInfo: {
                taskList: null,
                taskId: 0
            },
            onReturnFromChildCreateActions: [],
            onReturnFromChildUpdateActions: []
        };

        editCoordinator.registerIsDirty(isDirty);
        editCoordinator.registerSetDisabled(setDisabled);

        function onEditClick() {
            state.formMode = $scope.formMode = spEditForm.formModes.edit;
            editCoordinator.setDisabled(true);
        }

        function onSaveClick() {
            save();    // ignoring the promises
        }

        function onCancelClick() {
            editCoordinator.setDisabled(false);
            state.formMode = $scope.formMode = spEditForm.formModes.view;
            processPendingScopeUpdate();
        }

        function alert(msg, severity) {
            spAlertsService.addAlert(msg, {severity: severity || 'error'});
        }

        function getComponentItemKey(id) {
            if (!id || !$scope.formControl) throw new Error('Missing id');

            return 'formRenderCtrl:' + $scope.formControl.idP + ":" + id;
        }

        function getState(id) {
            return spState.getComponentState(getComponentItemKey(id));
        }

        function loadFormDataFromNav(id) {
            if (!state.formData) {
                return false;
            }
            $scope.formData = state.formData;
            $scope.formMode = state.formMode;
            $scope.model.taskInfo = state.taskInfo;
            $scope.model.onReturnFromChildCreateActions = state.onReturnFromChildCreateActions;
            $scope.model.onReturnFromChildUpdateActions = state.onReturnFromChildUpdateActions;

            resyncOnNavReturn();
            return true;
        }

        function loadFromDataFromServer(id) {
            return spEditForm.getFormData(id, $scope.requestStrings).then(
                function (formData) {
                    $scope.formData = formData;

                    // fetch tasks if form not in edit mode
                    if ($scope.formMode !== spEditForm.formModes.edit) {
                        fetchTasks(id);
                    }

                    // store the state info.
                    state.formData = formData;
                    state.formMode = $scope.formMode;
                    state.taskInfo = $scope.model.taskInfo;
                    state.onReturnFromChildCreateActions = $scope.model.onReturnFromChildCreateActions;
                    state.onReturnFromChildUpdateActions = $scope.model.onReturnFromChildUpdateActions;
                },
                function (error) {
                    alert('An error occurred getting the form data: ' + sp.result(error, 'data.Message'));
                });
        }

        function processScopeUpdate(actionData) {
            if (actionData.drilldownConds) {
                return;
            }

            var id = actionData;
            if (id && id !== -1) {
                isParentResourceAvailable = true;
                loadFormData(id);
            } else {
                isParentResourceAvailable = false;
                $scope.formData = null;
            }

            // broadcast message so that tab control can run report to get the report columns
            if (id === -1) {
                $scope.$broadcast('formRenderControlEventNoParentResourceAvailable');
            }
        }

        function loadFormData(id) {
            state = getState(id);
            resetTaskInfo();
            resetReturnFromChildActions();
            if (!loadFormDataFromNav(id)) {
                loadFromDataFromServer(id);
            }
        }

        function processPendingScopeUpdate() {
            if (pendingScopeUpdate) {
                processScopeUpdate(pendingScopeUpdate);
                pendingScopeUpdate = null;
            }
        }

        function resyncOnNavReturn() {
            var navItem = spNavService.getCurrentItem();
            if (!navItem || !$scope.formData) {
                return;
            }

            //
            // Run any pre-load processing (generally as the result of a return from a child navigate where something needs to be done on the parent
            //
            if (navItem.performOnReturnFromChildCreateActions === true) {
                _.map($scope.model.onReturnFromChildCreateActions, function (f) {
                    f($scope, $scope.formData);
                });
            }

            if (navItem.performOnReturnFromChildUpdateActions === true) {
                _.map($scope.model.onReturnFromChildUpdateActions, function (f) {
                    f($scope, $scope.formData);
                });
            }

            // reset
            navItem.performOnReturnFromChildCreateActions = false;
            navItem.performOnReturnFromChildUpdateActions = false;
            $scope.model.onReturnFromChildCreateActions = [];
            $scope.model.onReturnFromChildUpdateActions = [];
        }

        function formControlChanged(formControl) {
            var channelId;

            if (formControl) {

                var contextSender;

                $scope.formToRender = formControl.getFormToRender();

                if ($scope.formToRender) {
                    spEditForm.buildRequestStrings($scope.requestStrings, $scope.formToRender);
                }

                $scope.formTitle = formControl.getName() || $scope.formToRender.getName();

                // check if we are getting our context from elsewhere
                contextSender = formControl.getReceiveContextFrom();

                if (contextSender) {
                    $scope.formData = null;

                    channelId = spResourceScope.getChannelIdFromReceiver($scope.formControl);

                    if (onScopeUpdateUnregisterListener) {
                        onScopeUpdateUnregisterListener();
                        onScopeUpdateUnregisterListener = null;
                    }

                    //
                    // receive context update. Updates are not processed if we are in edit mode, but processed later.
                    //
                    onScopeUpdateUnregisterListener = spResourceScope.onScopeUpdate(channelId, function (id) {
                        if ($scope.formMode !== spEditForm.formModes.edit) {
                            processScopeUpdate(id);
                        } else {
                            pendingScopeUpdate = id;
                        }

                    });
                }
            }
        }

        function formDataChanged(formData, oldFormData) {
            if (!formData) return;
            if (oldFormData && formData.idP === oldFormData.idP) return;

            setButtonsVisibility();
            spResourceScope.sendScopeUpdate(channelId, formData.eid());
        }

        function save() {
            var formControls = spEditForm.getFormControls($scope.formToRender);
            if (spEditForm.validateFormControls(formControls, $scope.formData)) {
                return spEditForm.saveFormData($scope.formData).then(
                    function () {
                        editCoordinator.setDisabled(false);
                        formPostSave();
                        processPendingScopeUpdate();
                    },
                    function (error) {
                        alert(spEditForm.formatSaveErrorMessage(error));
                    }).then(invalidateCache);
            } else {
                return $q.reject("Validation error");
            }
        }

        /////
        // Invalidate cached screen components as well as clients-side cached report data.
        /////
        function invalidateCache() {
            var navItem = spNavService.getCurrentItem();

            if (navItem && navItem.componentData) {
                _.forOwn(navItem.componentData, function (value, key) {

                    var reportState = navItem.componentData[key].reportState;

                    if (reportState) {
                        var reportModelManager = reportState.reportModelManager;

                        if (reportModelManager && reportModelManager.getModel) {
                            var model = reportModelManager.getModel();

                            if (model && model.reportId) {
                                spReportDataCacheService.clearReportData(model.reportId);
                            }
                        }
                    }

                    delete navItem.componentData[key];
                });
            }
        }

        function formPostSave() {
            loadFromDataFromServer($scope.formData.idP).then(function () {
                // switch to view mode
                state.formMode = $scope.formMode = spEditForm.formModes.view;
            });
        }

        function setButtonsVisibility() {
            var modifyAccessDenied = sp.result($scope.formData, 'canModify') === false;

            editButton.hidden = ($scope.formMode !== spEditForm.formModes.view || modifyAccessDenied);
            cancelButton.hidden = $scope.formMode === spEditForm.formModes.view;
            saveButton.hidden = (cancelButton.hidden || modifyAccessDenied);
        }

        function setDisabled(value) {
            if ($scope.formMode !== spEditForm.formModes.edit) {
                $scope.isDisabled = value;
            }
        }

        function isDirty() {
            if (!$scope.formMode || $scope.formMode !== 'edit' || !$scope.formData) {
                return false;
            }
            return $scope.formData.hasChangesRecursive();
        }

        function resetTaskInfo() {
            $scope.model.taskInfo = {
                taskList: null,
                taskId: 0
            };
        }

        function resetReturnFromChildActions() {
            $scope.model.onReturnFromChildCreateActions = [];
            $scope.model.onReturnFromChildUpdateActions = [];
        }

        function isEditMode() {
            return $scope.formMode === spEditForm.formModes.edit;
        }

        function fetchTasks(recordId) {
            return spUserTask.getPendingTasksForRecord(recordId)
                .then(function (tasks) {
                    if (tasks && tasks.length) {
                        $scope.model.taskInfo.taskList = tasks;
                    }
                });
        }
    }

    function editCoordinator() {
        "ngInject";

        var exported = {};

        var registeredSetDisabled = [], registeredIsDirty = [];

        exported.registerSetDisabled = function (func) {
            registeredSetDisabled.push(func);
        };

        exported.setDisabled = function (value) {
            _.map(registeredSetDisabled, function (func) {
                func(value);
            });

        };

        exported.registerIsDirty = function (func) {
            registeredIsDirty.push(func);
        };

        exported.areAnyDirty = function () {
            return _.some(registeredIsDirty, function (fn) {
                return fn();
            });
        };

        exported.unregisterIsDirty = function (func) {
            _.pull(registeredIsDirty, func);
        };

        exported.unregisterSetDisabled = function (func) {
            _.pull(registeredSetDisabled, func);
        };

        return exported;
    }

}());