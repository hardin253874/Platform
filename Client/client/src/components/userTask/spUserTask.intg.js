// Copyright 2011-2016 Global Software Innovation Pty Ltd

describe('Entity Model|spUserTask|intg:', function() {
    "use strict";

    beforeEach(module('sp.common.loginService'));
    beforeEach(module('mod.common.spUserTask'));

    beforeEach(inject(function ($injector) {
        TestSupport.setupIntgTests(this, $injector);
    }));

   

    function json() {
        return {
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
            recordToPresent: { id: 'test:allFieldsForm'},
            assignedToUser: { id: 'administratorPerson'}
        };
    }

    
    describe('complete user task', function() {
        it('should completed a created user task', inject(function($rootScope, spUserTask, spEntityService) {
            // Create a definition and two fields
            
            var task = spEntity.fromJSON(json());
            var selectedExit;
            var result = {};

            TestSupport.waitCheckReturn($rootScope,
                // create test entity
                spEntityService.putEntity(task)
                    .then(function (id) {
                        task.setId(id);
                        return spUserTask.getTask(id);
                    })
                    .then(function (retrievedTask) {
                        return spUserTask.completeTask(retrievedTask, 'exitA');
                    })
                    .then(function (completeTaskId) {
                        return spUserTask.getTask(completeTaskId);
                    }),
                 
                result);
            
            runs(function () {
                var entity = result.value;
                expect(entity).toBeEntity();
                expect(entity.userTaskIsComplete).toBeTruthy();
                expect(entity.userTaskCompletedOn).toBeTruthy();
                expect(entity.userResponse).toBeTruthy();
                expect(entity.userResponse.fromExitPoint.getName()).toEqual('exitA');
            });
        }));
    });

    describe('getPendingTasksForRecord', function () {
        it('should return a task for a record', inject(function ($rootScope, spUserTask, spEntityService) {
            // Create a definition and two fields

            var task = spEntity.fromJSON(json());
            var selectedExit;
            var result = {};

            TestSupport.waitCheckReturn($rootScope,
                // create test entity
                spEntityService.putEntity(task)
                    .then(function (id) {
                        task.setId(id);
                    })
                    .then(function () {
                        return spEntityService.getEntity('test:allFieldsForm', 'name');
                    })
                    .then(function (allFields) {
                        return spUserTask.getPendingTasksForRecord(allFields);
                    }),

                result);

            runs(function () {
                var tasks = result.value;
                expect(tasks.length > 0).toBeTruthy();
                var myTask = _.find(tasks, { 'name': task.name });
                expect(myTask).toBeTruthy();
            });
        }));
    });



});



