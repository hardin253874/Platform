// Copyright 2011-2016 Global Software Innovation Pty Ltd
/*global _, console, angular, spWorkflowConfiguration, spWorkflow, sp, spEntity */

//TODO - simplify that results data structure - is a left over from the old report interface

(function () {
    'use strict';

    angular.module('sp.workflow.parameterViewControllers')
        .controller('workflowParameterEditViewController', workflowParameterEditViewController);

    function workflowParameterEditViewController($scope, $state, $q, spWorkflowService, spWorkflowChooserDataService, spViewRegionService, spAlertsService) {
        var currentView = null;

        var chooserRefreshFunctions = {
            'resourceChooser': refreshResources,
            'typeChooser': refreshTypes,
            'fieldChooser': refreshFields,
            'relChooser': refreshRels,
            'propertyChooser': refreshProps,
            'parameterChooser': refreshParameters,
            'functionChooser': refreshExprFunctions,
            'reportChooser': refreshReports
        };

        var throttledRefresh = _.throttle(refresh, 1000);

        console.log('workflowParameterEditViewController ctor');

        $scope.$on("$destroy", function () {
            console.log('workflowParameterEditViewController destroyed');
        });

        // some checks to ensure we are in the correct context
        console.assert($scope.view);
        console.assert($scope.view.apply);

        function setBusy() {
            $scope.busyIndicator.isBusy = true;
        }

        function clearBusy() {
            $scope.busyIndicator.isBusy = false;
        }

        // return a filtered list of items ordered with better matches first
        function sortedFilter(data, filterText) {

            return data;

        }

        function notifyLayoutChange() {
            $scope.$emit('app.layout');
        }

        function updateFilteredData() {
            $scope.filteredData = [];
            if ($scope.pickerReport) {
                $scope.filteredData = _.map(sortedFilter($scope.pickerReport.results.data, $scope.options.filterText), function (datum) {
                    return _.extend(datum, {
                        isCurrentSelection: !!sp.findByKey($scope.view.currentSelections, 'id', datum.id),
                        isCandidateSelection: !!sp.findByKey($scope.view.candidateSelections, 'id', datum.id)
                    });
                });
            }
            notifyLayoutChange();
        }

        function select(datum) {
            //todo - handle multi-select
            $scope.view.candidateSelections = [datum];
            updateFilteredData();
        }

        function selectNext() {
            //todo - scrolling into view
            //todo - handle multi-select

            var data = $scope.filteredData;
            var index = _.findIndex(data, 'isCandidateSelection');
            if (index < 0 && data.length > 0) {
                select(data[0]);
            } else if (index >= 0 && index < data.length - 1) {
                select(data[index + 1]);
            }
        }

        function selectPrev() {
            //todo - scrolling into view
            //todo - handle multi-select

            var data = $scope.filteredData;
            var index = _.findIndex(data, 'isCandidateSelection');
            if (data.length > 0) {
                if (index < 0) {
                    select(data[data.length - 1]);
                } else if (index > 0) {
                    select(data[index - 1]);
                }
            }
        }


        function refreshResources() {
            return spWorkflowChooserDataService.getResources($scope.view.resourceType, $scope.options.filterText);
        }

        function refreshTypes() {
            return spWorkflowChooserDataService.getTypes($scope.options.filterText);
        }

        function refreshFields() {
            return spWorkflowChooserDataService.getFields($scope.view.resourceType, $scope.options.filterText);
        }

        function refreshRels() {
            return spWorkflowChooserDataService.getRelationships($scope.view.resourceType, $scope.options.filterText);
        }

        function refreshProps() {
            return spWorkflowChooserDataService.getEntityProperties($scope.view.resourceType, $scope.options.filterText);
        }

        function refreshParameters() {
            return spWorkflowChooserDataService.getParameters($scope.view, $scope.options.filterText);
        }

        function refreshExprFunctions() {
            return spWorkflowChooserDataService.getFunctions($scope.options.filterText);
        }

        function refreshReports() {
            return spWorkflowChooserDataService.getReports($scope.view.resourceType, $scope.options.filterText);
        }

        function refresh() {

            if (!currentView) {
                console.error("No view set");
                return;
            }

            var refreshFn = chooserRefreshFunctions[currentView];

            if (!currentView) {
                console.error("Can't find function for view");
                return;
            }

            setBusy();

            refreshFn()
                .then(function (report) {
                    if (report) {
                        $scope.pickerReport = report;
                        updateFilteredData();
                    }
                })
                .catch(handleRequestError)
                .finally(clearBusy);
        }

        $scope.showTypeChooser = function () {

            //todo - this should be in a service

            var resourceType = $scope.view.resourceType;
            var currentSelections = resourceType ? [
                {id: resourceType}
            ] : [];

            var openView = {
                templateUrl: 'workflow/workflowParameterView/chooserView.tpl.html',
                viewName: 'typeChooser',
                currentSelections: currentSelections,
                resourceType: 'resource',
                apply: function () {
                    var selected = _.first(this.candidateSelections);
                    if (selected) {
                        $scope.view.resourceType = selected.id;
                        $scope.view.resourceTypeName = selected.name;
                        refresh();
                    }
                },
                cancel: function () {
                    this.currentSelections = currentSelections;
                },
                onPopped: function () {
                }
            };

            spViewRegionService.pushView($scope.region, openView);
        };

        $scope.$watch('view.viewName', function (viewName) {
            console.log('watched view', viewName, $scope.$id);

            if (viewName) {
                currentView = viewName;
                refresh();
            }
        });

        $scope.$watch('pickerReport', function (report) {
            console.log('workflowParameterEditViewController.watch(picker report): ', report && report.name);

            if (!report || !report.results) {
                return;
            }
        });

        $scope.$watch('view.resourceType', function (id) {

            if (!id) return;

            id = sp.coerseToNumberOrLeaveAlone(id);

            spWorkflowService.getCacheableEntity('entity:' + id, id, 'name,description,isOfType.name')
                .then(function (e) {
                    if (id === sp.coerseToNumberOrLeaveAlone($scope.view.resourceType)) {
                        $scope.view.resourceTypeName = e.name;
                    }
                });
        });

        $scope.$watch('options.filterText', updateFromFilter);


        function updateFromFilter(updated, original) {
            if (updated != original) {
                throttledRefresh();
            }
        }


        $scope.onClicked = function (datum) {
            select(datum);
        };

        $scope.onKeyup = function (e) {
            if (e.which === 40) {
                selectNext();
            } else if (e.which === 38) {
                selectPrev();
            }
        };

        $scope.view.candidateSelections = ($scope.view.currentSelections || []).slice(0);
        $scope.busyIndicator = {placement: 'element', isBusy: true};
        $scope.options = {
            filterText: '',
            showSystemTypes: false
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