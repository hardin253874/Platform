// Copyright 2011-2016 Global Software Innovation Pty Ltd

(function () {
    'use strict';

    angular.module('sp.workflow.input', ['ui.router', 'mod.common.spEntityService', 'mod.common.alerts'])
        .config(function ($stateProvider) {
            $stateProvider.state('workflowInput', {
                url: '/{tenant}/{eid}/workflow/input?path',
                templateUrl: 'workflow/workflowInput/workflowInput.tpl.html',
                controller: 'workflowInputController'
            });
        })
        .controller('workflowInputController', function ($scope, $q, $stateParams, spNavService, spMobileContext, spEntityService, spUserTask, spAlertsService) {

            var inputTask;

            $scope.model = {};

            $scope.saveInput = function () {
                if (!validateInputs()) {
                    spAlertsService.addAlert('Unable to continue, there are validation errors on the form.', { severity: spAlertsService.sev.Error, expires: true });
                    return;
                }
                
                showSpinner();
                
                completeTask()
                    .then(runAnyFollowUpTasks)
                    .then(init)
                    .catch(function (error) {
                        var msg = error ? (error.message || sp.result(error, 'data.Message') || error) : 'An error occurred.';
                        spAlertsService.addAlert(msg, { severity: spAlertsService.sev.Error });
                    })
                    .finally(hideSpinner);
            };

            $scope.cancelInput = function () {
                spNavService.navigateToParent();
            };

            $scope.isPortableDevice = function () {
                return spMobileContext.isMobile || spMobileContext.isTablet;
            };

            function showSpinner() {
                spNavService.middleLayoutBusy = true;
            }

            function hideSpinner() {
                spNavService.middleLayoutBusy = false;
            }

            function validateInputs() {
                if (!inputTask)
                    return false;

                var allPassed = true;

                _.forEach($scope.model.inputs, function (i) {
                    var msg = 'A value is required.';
                    var val = sp.result(i, 'value.value');
                    i.messages.length = 0;
                    switch (i.type) {
                        case 'core:stringArgument': if (!_.isString(i.value) || _.isEmpty(i.value)) { allPassed = false; i.messages.push(msg); }
                            break;
                        case 'core:boolArgument': if (!_.isBoolean(val)) { allPassed = false; i.messages.push(msg); }
                            break;
                        case 'core:decimalArgument':
                        case 'core:currencyArgument':
                        case 'core:integerArgument': if (!_.isNumber(val)) { allPassed = false; i.messages.push(msg); }
                            break;
                        case 'core:dateArgument':
                        case 'core:dateTimeArgument': if (!_.isDate(val)) { allPassed = false; i.messages.push(msg); }
                            break;
                        case 'core:resourceArgument': if (!i.value.selectedEntity && i.value.selectedEntities.length === 0) { allPassed = false; i.messages.push(msg); }
                            break;
                        case 'core:resourceListArgument': if (i.value.selectedEntities.length === 0) { allPassed = false; i.messages.push(msg); }
                            break;
                        case 'core:guidArgument':
                            if (!i.value || !_.isString(i.value) || (/^[0-9a-f]{8}-[0-9a-f]{4}-[1-5][0-9a-f]{3}-[89ab][0-9a-f]{3}-[0-9a-f]{12}$/.test(i.value.toLowerCase()) === false)) {
                                allPassed = false;
                                i.messages.push(msg);
                            }
                            break;
                        default:
                            if (!i.value || !i.value.isValidValue) {
                                allPassed = false;
                                i.messages.push(msg);
                            }
                            break;
                    }
                });

                return allPassed;
            }

            function completeTask() {
                if (!inputTask)
                    return $q.reject();

                _.forEach(inputTask.promptForTaskStateInfo, function (si) {
                    var v = _.find($scope.model.inputs, function (i) {
                        return i.id === sp.result(si, 'stateInfoArgument.idP');
                    });
                    if (v) {
                        switch (v.type) {
                            case 'core:stringArgument':
                                si.stateInfoValue.stringParameterValue = v.value;
                                break;

                            case 'core:decimalArgument':
                            case 'core:currencyArgument':
                                si.stateInfoValue.decimalParameterValue = v.value.value;
                                break;

                            case 'core:integerArgument':
                                si.stateInfoValue.intParameterValue = v.value.value;
                                break;

                            case 'core:boolArgument':
                                si.stateInfoValue.boolParameterValue = v.value.value;
                                break;

                            case 'core:guidArgument':
                                si.stateInfoValue.guidParameterValue = v.value;
                                break;

                            case 'core:dateArgument':
                                si.stateInfoValue.dateParameterValue = v.value.value;
                                break;

                            case 'core:dateTimeArgument':
                                si.stateInfoValue.dateTimeParameterValue = v.value.value;
                                break;

                            case 'core:timeArgument':
                                si.stateInfoValue.timeParameterValue = v.value.value;
                                break;

                            case 'core:resourceArgument':
                                si.stateInfoValue.resourceParameterValue = v.value.selectedEntity || _.first(v.value.selectedEntities);
                                break;

                            case 'core:resourceListArgument':
                                si.stateInfoValue.resourceListParameterValues = v.value.selectedEntities;
                                break;
                        }
                    }
                });

                inputTask.userTaskIsComplete = true;
                inputTask.taskStatus = spEntity.fromJSON({ id: 'core:taskStatusCompleted' });

                return spEntityService.putEntity(inputTask);
            }

            function runAnyFollowUpTasks() {

                var runOptions = { cancel: false };

                $scope.$root.$on('$stateChangeSuccess', function () { runOptions.cancel = true; });

                if (inputTask && inputTask.waitForNextTask && inputTask.workflowRunForTask) {
                    return spUserTask.waitToNavigateToFollowOnTasks(inputTask.workflowRunForTask.idP, true, false, runOptions).then(function (results) {
                        if (!results) {
                            return spNavService.navigateToParent();
                        }
                        return results;
                    });
                } else {
                    return spNavService.navigateToParent();
                }
            }

            function getArgumentValue(type, id) {
                if (inputTask) {
                    var si = _.find(inputTask.promptForTaskStateInfo, function (s) {
                        return sp.result(s, 'stateInfoArgument.idP') === id;
                    });
                    if (si) {
                        switch (type) {
                            case 'core:stringArgument':
                                return sp.result(si, 'stateInfoValue.stringParameterValue');

                            case 'core:decimalArgument':
                                return {
                                    value: sp.result(si, 'stateInfoValue.decimalParameterValue'),
                                    decimalPlaces: sp.result(si, 'stateInfoValue.numberDecimalPlaces') || 3 // is this even used?
                                };

                            case 'core:currencyArgument':
                                return {
                                    value: sp.result(si, 'stateInfoValue.decimalParameterValue'),
                                    decimalPlaces: sp.result(si, 'stateInfoValue.numberDecimalPlaces') || 2
                                };

                            case 'core:integerArgument':
                                return { value: sp.result(si, 'stateInfoValue.intParameterValue') };

                            case 'core:boolArgument':
                                return { value: sp.result(si, 'stateInfoValue.boolParameterValue') };

                            case 'core:guidArgument':
                                return sp.result(si, 'stateInfoValue.guidParameterValue');

                            case 'core:dateArgument':
                                return { value: sp.result(si, 'stateInfoValue.dateParameterValue') };

                            case 'core:dateTimeArgument':
                                return { value: sp.result(si, 'stateInfoValue.dateTimeParameterValue') };

                            case 'core:timeArgument':
                                return { value: sp.result(si, 'stateInfoValue.timeParameterValue') };

                            case 'core:resourceArgument':
                                return sp.result(si, 'stateInfoValue.resourceParameterValue');

                            case 'core:resourceListArgument':
                                return sp.result(si, 'stateInfoValue.resourceListParameterValues');
                        }
                    }
                }
                return null;
            }

            function updatePickerValues() {
                var resourceArguments = _.filter($scope.model.inputs, function (i) {
                    return i.type === 'core:resourceArgument' || i.type === 'core:resourceListArgument';
                });

                _.forEach(resourceArguments, function(i) {
                    var selected = [];
                    if (i.value) {
                        if (i.value.length) {
                            selected = i.value;
                        } else {
                            if (i.value.idP) {
                                selected = [i.value];
                            }
                        }
                    }
                    var type = sp.result(i.pickerInfo, 'type') || 'core:resource';
                    var report = sp.result(i.pickerInfo, 'report') || 'core:templateReport';
                    i.value = {
                        selectedEntityId: null,
                        selectedEntity: null,
                        selectedEntityIds: [],
                        selectedEntities: selected,
                        pickerReportId: report,
                        entityTypeId: type,
                        multiSelect: i.type === 'core:resourceListArgument',
                        allowCreateRecords: false
                    };
                });

                return $q.when();
            }

            function init(isNavigating) {

                if (isNavigating)
                    return $q.when();

                var req = 'name, description, isOfType.{alias}, userTaskIsComplete, taskStatus.id, waitForNextTask,' +
                    ' promptForTaskMessage, promptForTaskArguments.{activityPromptOrdinal, activityPromptArgumentPickerReport.{id, name},' +
                    ' activityPromptArgument.{id, name, isOfType.{alias}, conformsToType.{id, defaultPickerReport.id, inherits*.{id, alias}}}},' +
                    ' workflowRunForTask.{id, name},' +
                    ' promptForTaskStateInfo.{name, stateInfoArgument.id, stateInfoValue.{' +
                    ' resourceParameterValue.{id, name}, resourceListParameterValues.{id, name},' +
                    ' stringParameterValue, decimalParameterValue, numberDecimalPlaces, intParameterValue, boolParameterValue,' +
                    ' guidParameterValue, dateParameterValue, dateTimeParameterValue, timeParameterValue' +
                    '}}';

                return spEntityService.getEntity($stateParams.eid, req, { hint: 'workflowInput', batch: true })
                    .then(function (task) {
                        inputTask = task;

                        if (inputTask) {
                            $scope.model.name = inputTask.workflowRunForTask.name || inputTask.name;
                            $scope.model.message = inputTask.promptForTaskMessage;
                            $scope.model.inputs = _.filter(_.map(inputTask.promptForTaskArguments, function (v) {
                                var arg = v.activityPromptArgument;
                                var rpt = v.activityPromptArgumentPickerReport;
                                if (arg) {
                                    var t = arg.getType().alias();
                                    var c = arg.conformsToType;
                                    var r = sp.result(rpt, 'idP') || sp.result(c, 'defaultPickerReport.idP');

                                    // choice fields are special
                                    var isChoiceField = false;
                                    if (c) {
                                        isChoiceField = _.map(spResource.getAncestorsAndSelf(c), 'nsAlias').indexOf('core:enumValue') >= 0;
                                    }

                                    return {
                                        id: arg.idP,
                                        name: arg.name,
                                        description: arg.description,
                                        type: t,
                                        value: getArgumentValue(t, arg.idP),
                                        displayOrder: v.activityPromptOrdinal || 0,
                                        messages: [],
                                        isRequired: t !== 'core:boolArgument',
                                        isChoiceField: isChoiceField,
                                        pickerInfo: {
                                            type: sp.result(c, 'idP'),
                                            report: r
                                        }
                                    };
                                }
                                return null;
                            }), function (m) { return m !== null; });
                        }
                    })
                    .then(updatePickerValues)
                    .catch(function (err) { if (err !== 404) { throw err; } }); // task may have been deleted, legitimately
            }

            if ($stateParams.eid) {
                init();
            }
        });
}());