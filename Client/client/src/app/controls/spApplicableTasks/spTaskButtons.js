// Copyright 2011-2016 Global Software Innovation Pty Ltd
/*global console, _, angular */

(function () {
    'use strict';

    /////
    // The spTaskButtons displays the buttons for a task. It must be given a task containing all the button info.
    // When the buttons are pressed it will navigate if the task is marked as one to follow.
    /////
    angular.module('app.controls.spApplicableTasks')
        .directive('spTaskButtons', spTaskButtons);

    /* @ngInject */
    function spTaskButtons($q, $rootScope, spUserTask, spNavService, spCachingCompile, spAlertsService) {

        /////
        // Directive structure.
        /////
        return {
            restrict: 'AE',
            replace: false,
            transclude: false,
            scope: {
                task: '=?',
                disabled: '=?',
                returnOnCompletion: '=?',
                inWizard: '=?',
                beforeSave: '&',  // a function that returns promise to run. Will transition only if successful.
                afterSave: '&'   // a function that returns promise to run. Will transition only if successful.
            },
            link: link
        };

        function link(scope, element) {

            var returningToParentMarker = "RETURNING TO PARENT";

            //
            // Process a user hitting one of the buttons
            //
            scope.selectExit = function (transition) {
                var exitName = sp.result(transition, 'fromExitPoint.name');
                var completeTask = _.partial(spUserTask.completeTask, scope.task, exitName);
                var afterSave = scope.afterSave || $q.when();
                showSpinner();

                scope.beforeSave()
                    .then(completeTask)
                    .then(afterSave)
                    .then(runAnyFollowUpTasks)
                    .then(refreshTask)
                    .catch(function (error) {
                        if (error != returningToParentMarker) {        // it's a little ugly but the best way to stop the promises half way
                            var msg = error ? (error.message || sp.result(error, 'data.Message') || error) : 'An error occurred.';
                            spAlertsService.addAlert(msg, { severity: spAlertsService.sev.Error });
                        }
                    })
                    .finally(hideSpinner);
            };

            //
            // Get task in order
            //
            scope.$watchCollection('task.availableTransitions', function (transitions) {
                scope.orderedTrans = _.sortBy(transitions, function (t) {
                    return t.fromExitPoint.exitPointOrdinal;
                });
            });

            function showSpinner() {
                spNavService.middleLayoutBusy = true;
            }

            function hideSpinner() {
                spNavService.middleLayoutBusy = false;
            }

            //
            // see if there are any follow up task to handle.
            //
            function runAnyFollowUpTasks() {
                var taskEntity = scope.task;
                var runEntity = taskEntity.workflowRunForTask;
                var runOptions = { cancel: false, requestParentRefresh: true };

                $rootScope.$on('$stateChangeSuccess', function () { runOptions.cancel = true; });
                
                if (taskEntity.waitForNextTask && runEntity) {
                    return spUserTask.waitToNavigateToFollowOnTasks(
                        runEntity.idP,
                        scope.inWizard,
                        scope.returnOnCompletion,
                        runOptions); // still to be written
                } else {
                    if (scope.returnOnCompletion) {
                        spNavService.navigateToParent(true);        // ask parent to refresh
                        return $q.reject(returningToParentMarker);
                    }
                }

                // no promise returned?
                return $q.when();
            }

            //
            // change the loaded task to make it look completed (rather than reloading it)
            //
            function refreshTask() {
                return spUserTask.getTask(scope.task.eidP)
                    .then(function (task) { scope.task = task; })
                    .catch(function(err) {
                        if (err !== 404) { throw err; } // task may have been deleted, legitimately
                    });
            }

            var cachedLinkFunc = spCachingCompile.compile('controls/spApplicableTasks/spTaskButtons.tpl.html');
            cachedLinkFunc(scope, function (clone) {
                element.append(clone);
            });
        }
    }
}());