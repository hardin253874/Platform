// Copyright 2011-2016 Global Software Innovation Pty Ltd
(function () {
    'use strict';

    angular.module('app.applicableTasksTest', ['app.controls.spApplicableTasks'])
        .controller('applicableTasksTest', function ($scope, spUserTask, spEntityService) {

            $scope.selectedRecord = null;

            function updateRecords() {

                spUserTask.getPendingTasksForRecord().then(function (tasks) {
                    $scope.recordList = _.uniq(_.map(tasks, 'recordToPresent'));
                });
            }

            var employees = spEntityService.getEntitiesOfType('test:manager', 'name').then(function (e) {
                console.log('Got it');
                return e;
            }, function (err) {
                console.log(err);
            });

            $scope.addTask = function() {
                employees.then(_.sample).then(createEmployeeTask);
            };

            function createEmployeeTask(employeeId) {
                var json = {
                    typeId: 'displayFormUserTask',
                    name: 'displayFormTask1'  + (new Date()).getTime(),
                    availableTransitions: [
                        {
                            typeId: 'transitionStart',
                            fromExitPoint: {
                                typeId: 'exitPoint',
                                name: 'exitA'
                            }
                        },
                        {
                            typeId: 'transitionStart',
                            fromExitPoint: {
                                typeId: 'exitPoint',
                                name: 'exitB'
                            }
                        }
                    ],
                    recordToPresent: { id: employeeId.idP},
                    assignedToUser: { id: 'administratorPerson'}

                };

                var task = spEntity.fromJSON(json);

                spEntityService.putEntity(task)
                    .then(updateRecords);
            }

            updateRecords();

            $scope.$watch('selectedRecord', function (record) {
                if (record) {
                    spUserTask.getPendingTasksForRecord(record.idP).then(function (tasks) {
                        $scope.taskList = tasks;
                    });
                }
            });

        });
}());