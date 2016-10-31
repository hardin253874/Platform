// Copyright 2011-2016 Global Software Innovation Pty Ltd
/*global _, console, angular, sp, spWorkflow, spWorkflowConfiguration, spEntity */

(function () {
    'use strict';

    angular.module('sp.workflow.runController')
        .controller('workflowRunControlController', function ($scope, $q, spAlertsService, spReportService, spWorkflowService, spWorkflowRunService, spEntityService, spUserTask, spPromiseService) {

            // convenience aliases
            var aliases = spWorkflowConfiguration.aliases;

            function setBusy() {
                $scope.message = "Running...";
                //$scope.busyIndicator.isBusy = true;
            }

            function clearBusy() {
                $scope.message = "";

                //$scope.busyIndicator.isBusy = false;
            }

            function controlForArgument(argumentEntity) {

                console.log('Run Parameter', argumentEntity, spEntity.toJSON(argumentEntity));

                var control = {
                    id: argumentEntity.id(),
                    name: argumentEntity.getName(),
                    description: argumentEntity.field('core:description'),
                    value: null,
                    type: argumentEntity.getType().alias()
                };

                if (control.type === aliases.resourceArgument) {

                    var conformsToType = argumentEntity.getLookup(aliases.conformsToType);
                    var resourceTypeId = conformsToType ? conformsToType.id() : 'core:resource';
                    var parameters = { resourceType: { value: resourceTypeId } };


                    return spReportService.runPickerReport('resources', parameters).then(function (report) {

                        if (!report || !report.results || !report.results.cols || !report.results.cols.length) {
                            return;
                        }

                        var cols = report.results.cols,
                            data = report.results.data;

                        control.selectOptions = data.map(function (row) {
                            var item = { id: row.id };

                            cols.forEach(function (col, i) {

                                item[col.title] = row.item[i].value;

                                // capture the name, if provided, into a 'name' member
                                if (col.title.toLowerCase() === 'name') {
                                    item.name = row.item[i].value;
                                }
                            });
                            if (!item.name) {
                                // no name found so set one from the first column
                                item.name = item[cols[0].title];
                            }
                            return item;
                        });
                        console.log('found %s resources for resource control %s', control.selectOptions.length, control.name);
                        control.selectOptions = control.selectOptions.filter(function (item) {
                            return !!item.name;
                        });
                        console.log('using %s resources having names for resource control %s', control.selectOptions.length, control.name);

                        return control;
                    })
                        .catch(function (err) {
                            console.log('run workflow error building parameter control', err);
                           return control;
                        });
                }

                return control;
            }

            function runNow() {
                var id = $scope.workflow.entity.id();
                var runOptions = $scope.inputArgumentControls
                    .filter(function (c) {
                        return c.name && c.value;
                    })
                    .map(function (c) {
                        console.log('processing inputArgumentControl', c);
                        return { name: c.name, value: c.value && '' + c.value, typeName: c.type };
                    });

                console.log('running', id, runOptions);
                return spWorkflowRunService.runWorkflow(id, runOptions, $scope.areTracing).then(function (workflowRunId) {
                    $scope.workflow.lastRunId = workflowRunId;
                });
            }

            function waitForWorkflowRun() {

                var options = { cancel: false };

                $scope.$root.$on('$stateChangeSuccess', function () { options.cancel = true; });

                return spWorkflowRunService.waitForRunToStopWithThrow($scope.workflow.lastRunId, options)
                    .then(function (result) {
                        $scope.workflow.lastRunId = result;

                        spWorkflowRunService.getWorkflowRunResults(result, $scope.areTracing).then(function (results) {
                            $scope.workflow.lastTrace = results;
                        });
                    });
            }

            function openFollowUpTasks() {
                if ($scope.areOpeningFollowUpTasks) {
                    return spUserTask.navigateToFollowOnTasks($scope.workflow.lastRunId, true, true);
                }
                return $q.when();
            }

            function clearResults() {
                $scope.workflow.lastRunId = 0;
                $scope.workflow.lastTrace = null;
            }

            // 
            // Tracing

            var trace_retries = 120;
            var trace_pause = 1000;        // Pause between polls

            var tracing;

            function refreshWorkflowTrace() {

                return spWorkflowRunService.getWorkflowRunResults($scope.workflow.lastRunId, true).then(function (traceResults) {
                    $scope.workflow.lastTrace = traceResults;
                });
            }

            function stillTracing() {
                return !tracing;
            }

            function startTraceRefresh(result) {


                if ($scope.areTracing) {
                    tracing = true;

                    spPromiseService.poll(refreshWorkflowTrace, stillTracing, trace_retries, trace_pause);
                }

                return result;
            }

            function endTraceRefresh(result) {
                tracing = false;

                return result;
            }

            function runWorkflow() {
                // technically I don't need to wait on the save before running, but not waiting
                // seems to be causing the backend to lock up.
                
                clearResults();

                return $q.when(setBusy())
                    .then(runNow)
                    .then(startTraceRefresh)
                    .then(waitForWorkflowRun)
                    .then(endTraceRefresh)
                    .then(openFollowUpTasks)
                    .catch(function (error) {
                        var msg = error ? (error.message || sp.result(error, 'data.Message') || error) : 'An error occurred.';
                        spAlertsService.addAlert(msg, { severity: spAlertsService.sev.Error });
                    })
                    .finally(clearBusy);
            }

            $scope.refreshRunResults = refreshWorkflowTrace;
            
            $scope.run = runWorkflow;

            $scope.$on('toolbar.clicked', function (event, id) {
                //console.log('toolbar button clicked "%s"', id);

                switch (id) {
                    case 'run.info':
                        break;
                    case 'run.edit':
                        break;
                    case 'run.list':
                        break;
                    case 'run.close':
                        break;
                }
            });

            $scope.busyIndicator = { isBusy: false };
            $scope.form = { sections: [] };
            $scope.areOpeningFollowUpTasks = false;

            $scope.$watch('workflow.updateCount', function () {
                // This is the main place to watch for updates to the workflow model and to
                // kick off the workflow process operation.
                //TODO - consider doing this in the service, but we'd have to have the controller somehow tell it to start or
                // stop anyway... we don't want it happening when nothing is currently viewing the workflow.
                if ($scope.workflow) {
                    spWorkflowService.processWorkflow($scope.workflow);
                }
            });

            $scope.$watch('workflow.processState.count', function () {
                var sections = [];                         
                $scope.form = { sections: sections };

                if ($scope.workflow) {

                    return $q.all(_.map($scope.workflow.entity.inputArguments, controlForArgument)).then(function (controls) {

                        console.log('runWorkflow - loaded controls', controls);

                        $scope.inputArgumentControls = controls;

                        sections.push({
                            name: 'Input Arguments',
                            controls: $scope.inputArgumentControls
                        });

                        $scope.readyToRun = true;
                    });
                }
            });

        });
}());
