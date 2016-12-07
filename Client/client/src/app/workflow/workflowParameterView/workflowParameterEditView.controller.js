// Copyright 2011-2016 Global Software Innovation Pty Ltd
/*global _, console, angular, spWorkflowConfiguration, spWorkflow, sp, spEntity */

(function () {
    'use strict';

    angular.module('sp.workflow.parameterViewControllers')
        .controller('workflowParameterEditViewController', workflowParameterEditViewController);

    function workflowParameterEditViewController($scope, $state, $timeout, $q, spWorkflowService, spWorkflowEditorViewService, spViewRegionService, spAppSettings, spAlertsService) {

        console.log('workflowParameterEditViewController ctor');

        $scope.$on("$destroy", function () {
            console.log('workflowParameterEditViewController destroyed');
        });

        // some checks to ensure we are in the correct context
        console.assert($scope.view);
        console.assert($scope.view.apply);

        function safeApply(scope, fn) {
            if (!scope.$root.$$phase) {
                scope.$apply(fn);
            } else {
                fn();
            }
        }

        function setBusy() {
            $scope.busyIndicator.isBusy = true;
        }

        function clearBusy() {
            $scope.busyIndicator.isBusy = false;
            $timeout(function () { $scope.gridLoaded = true; });
        }

        function refresh() {
            if ($scope.view && $scope.view.load) {
                setBusy();
                $scope.view.load().then(function(data) {
                    $scope.view.rows = data;
                }).catch(handleRequestError).finally(clearBusy);
            }
        }

        function refreshTypeData(typeEntity) {
            if (!typeEntity || !typeEntity.idP) {
                $scope.view.reportOptions.entityTypeId = null;
                $scope.view.reportOptions.reportId = null;
                return;
            }
            spWorkflowService.getTemplateReport().then(function (templateReport) {
                $scope.view.reportOptions.entityTypeId = typeEntity.idP;

                var reportId = sp.result(typeEntity, 'defaultPickerReport.idP') || templateReport.idP;
                $scope.view.reportOptions.reportId = $scope.options.useDefaultPicker ? reportId : templateReport.idP;
            });
        }

        function refreshGrid() {
            safeApply($scope, function () {
                var grid = sp.result($scope.view, 'gridOptions.ngGrid');
                var gridScope = sp.result($scope.view, 'gridOptions.$gridScope');
                if (grid && gridScope) {
                    var gridDomService = sp.result($scope.view, 'gridOptions.$gridServices.DomUtilityService');
                    if (gridDomService && gridDomService.RebuildGrid) {
                        gridDomService.RebuildGrid(gridScope, grid);
                    }
                }
            });
        }

        var refreshGridDebounced = _.debounce(refreshGrid, 100);

        $scope.showTypeChooser = function () {

            var openApply = function (selected) {
                if (selected) {
                    $scope.view.resourceType = selected.id;
                    $scope.view.resourceTypeName = selected.name;

                    refreshTypeData(selected.entity);
                }
            };

            var selected = $scope.view.resourceType;
            var resourceType = 'core:type';

            spWorkflowService.getTemplateReport().then(function (templateReport) {
                spWorkflowService.getCacheableType(resourceType).then(function (typeEntity) {
                    spWorkflowService.getCacheableType(selected).then(function (selectedEntity) {

                        var report = sp.result(typeEntity, 'defaultPickerReport') || templateReport;
                        var selection = _.castArray(selectedEntity || []);
                        var openView = spWorkflowEditorViewService.getReportChooserView('typeChooser', typeEntity, report, selection, openApply);
                        spViewRegionService.pushView($scope.region, openView);

                    });
                });
            });
        };

        // Show dev options 
        $scope.showAdvancedOption = function () {
            return spAppSettings.initialSettings.devMode && $scope.view.viewName === 'typeChooser';
        };

        $scope.$watch('view.viewName', function (viewName) {
            console.log('watched view', viewName, $scope.$id);

            if (viewName) {
                refresh();
            }
        });

        $scope.$watch('view.rows', function (newValue, oldValue) {
            if (newValue !== oldValue) {
                $scope.view.quickSearch.onSearchValueChanged();
            }
        });

        $scope.$watch('view.resourceType', function (id) {
            if (!id) return;

            id = sp.coerseToNumberOrLeaveAlone(id);

            spWorkflowService.getCacheableEntity('entity:' + id, id, 'name, description, isOfType.name').then(function (e) {
                if (id === sp.coerseToNumberOrLeaveAlone($scope.view.resourceType)) {
                    $scope.view.resourceTypeName = e.name;
                }
            });
        });

        $scope.$watch('options.useDefaultPicker', function (newValue, oldValue) {
            if (newValue !== oldValue) {
                spWorkflowService.getTemplateReport().then(function(templateReport) {
                    if (newValue && $scope.view.reportOptions.reportId) {
                        if ($scope.options.showAll) {
                            // turn off show dev and let it update the report
                            $scope.options.showAll = false;
                        } else {
                            spWorkflowService.getCacheableType($scope.view.resourceType).then(function (typeEntity) {
                                if (!typeEntity || !typeEntity.idP) {
                                    $scope.view.reportOptions.reportId = null;
                                    return;
                                }
                                $scope.view.reportOptions.reportId = sp.result(typeEntity, 'defaultPickerReport.idP') || templateReport.idP;
                            });
                        }
                    } else {
                        $scope.view.reportOptions.reportId = templateReport.idP;
                    }
                });
            }
        });

        $scope.$watch('options.showAll', function (newValue, oldValue) {
            if (newValue !== oldValue) {
                var type = $scope.view.resourceType;
                if ($scope.view.viewName === 'typeChooser') {
                    type = newValue ? 'core:type' : 'core:managedType';
                }
                spWorkflowService.getCacheableType(type).then(function (typeEntity) {
                    refreshTypeData(typeEntity);
                });
            }
        });
        
        $scope.onGridDoubleClicked = function() {
            event.stopPropagation();
            $scope.view.apply();
            $scope.close();
        };

        $scope.$on('spReportEventGridDoubleClicked', function (event) {
            event.stopPropagation();
            $scope.view.apply();
            $scope.close();
        });

        $scope.$on('spReportEventModelReady', function (event, model) {
            if ($scope.view.customDataProvider) {
                model.customDataProvider = $scope.view.customDataProvider;
            }
        });

        $scope.$on('sp.app.ui-refresh', refreshGridDebounced);

        $scope.busyIndicator = { placement: 'element', isBusy: true };
        $scope.gridLoaded = false;
        $scope.options = {
            useDefaultPicker: true,
            showAll: false
        };

        function handleRequestError(error) {
            console.error("Request failed: ", error);
            var message = sp.result(error, 'data.Message') || 'Request failed';
            alert(message);
        }

        function alert(msg, severity) {
            spAlertsService.addAlert(msg, {severity: severity || 'error', page: $state.current});
        }
    }

}());