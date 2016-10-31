// Copyright 2011-2016 Global Software Innovation Pty Ltd
/*global _, angular, console, module, describe, beforeEach, it, inject, expect, runs, waitsFor, TestSupport, sp, spEntity, spWorkflow, spWorkflowConfiguration, jsonString */

// tests to add:
// - workflows that update fields in some entity.. test the field gets updated
// - removing activities and sequences
// - trying to add a sequence to an occupied exit point
// - renaming activities
// - saving a workflow, run it, load it, modify and save it, run it again

describe('Console|Workflow|intg:', function () {
    "use strict";

    var aliases = spWorkflowConfiguration.aliases,
        waitCheckReturn = TestSupport.waitCheckReturn,
        getTestEntityName = TestSupport.getTestEntityName,
        getUpdatedTestEntityName = TestSupport.getUpdatedTestEntityName;

    var testEmployees;



    // functions defined below in a beforeEach as they require injected 'services'
    var timeout, getTestEmployees;

    function getParameterMap(workflow, activity, relId) {
        return _.keyBy(spWorkflow.getActivityParameters(workflow, activity, relId), function (p) { return p.argument.aliasOrId(); });
    }

    function expectParameterAndExpressionCounts(workflow, activity, n1, n2) {
        var parameters = getParameterMap(workflow, activity);
        expect(_.keys(parameters).length).toBe(n1);
        expect(_.compact(_.map(parameters, 'expression')).length).toBe(n2);
    }

    function setTestEntityName(entity, baseName) {
        entity.setName(getTestEntityName(baseName, 'tbd'));
    }

    function updateTestEntityName(entity) {
        entity.setName(getUpdatedTestEntityName(entity.getName(), 'tbd', entity.id() + ' ' + (new Date().toLocaleString())));
    }

    function updateTestWorkflowName(workflow) {
        updateTestEntityName(workflow.entity);
        return workflow;
    }

    function hasName(name, entity) {
        return entity.getName() === name;
    }

    function getTestId(test) {
        return test.description.split(':')[0];
    }

    function logStuff(logString, workflow) {

        // don't use spEntity.toJSON in the map calls due to its additional 'hidden' args
        function toJson(e) {
            return spEntity.toJSON(e);
        }

        console.log(logString,
            'wf pars', _.map(workflow.entity.getExpressionParameters(), toJson),
            'wf exprs', _.map(workflow.entity.getExpressionMap(), toJson),
            'act exprs', _.map(_.flatten(_.invokeMap(workflow.entity.getContainedActivities(), 'getExpressionMap')), toJson),
            'contained', _.map(workflow.entity.getContainedActivities(), toJson),
            'trans', _.map(workflow.entity.getTransitions(), toJson)
        );
        return workflow;
    }

    function runWorkflow(spWorkflowRunService, workflow) {
        return spWorkflowRunService.runWorkflow(workflow.entity.id(), [], true).then(function (tag) {
            return spWorkflowRunService.waitForRunToStopWithThrow(tag, { timeoutFn: timeout }).then(function(workflowRunId) {
                workflow.lastRunId = workflowRunId;
                return workflow;
            });
        });
    }

    function runWorkflowWithResourceId(spWorkflowRunService, resourceId, workflow) {
        return spWorkflowRunService.runWorkflow(workflow.entity.id(), [{ name: 'ResourceId', value: resourceId.toString(), typeName: 'core:resourceArgument' }], true).then(function(tag) {
            return spWorkflowRunService.waitForRunToStopWithThrow(tag, { timeoutFn: timeout }).then(function(workflowRunId) {
                workflow.lastRunId = workflowRunId;
                return workflow;
            });
        });
    }

    function runWorkflowWithEmployeeResourceId(spWorkflowRunService, workflow) {
        return getTestEmployees().then(function (testEmployees) {
            return runWorkflowWithResourceId(spWorkflowRunService, _.first(testEmployees).id(), workflow);
        });
    }

    function runWorkflowWithThisWorkflowResourceId(spWorkflowRunService, workflow) {
        return runWorkflowWithResourceId(spWorkflowRunService, workflow.entity.id(), workflow);
    }

    function waitForWorkflowRunStatus(spWorkflowRunService, spPromiseService, status, workflow) {
        var logTimeKey = 'run workflow (' + workflow.entity.getContainedActivities().length + ' activities)';
        console.time(logTimeKey);

        return spWorkflowRunService.waitForRunToStopWithThrow(workflow.lastRunId, { timeoutFn: timeout })
        .then(_.partial(spWorkflowRunService.getWorkflowRunResults, workflow.lastRunId))
        .then(function (results) {
            console.timeEnd(logTimeKey);

            workflow.lastRunStatus = results.workflowRunStatus.name;
            workflow.lastRunResults = results;
            return workflow;
        });
    }

    function addResourceIdWorkflowArgument(workflow) {
        workflow.entity.getInputArguments().add(spEntity.fromJSON({
            typeId: aliases.resourceArgument,
            name: 'ResourceId',
            description: 'The Resource that this workflow is run against',
            defaultExpression: jsonString(null),
            argumentIsMandatory: false,
            conformsToType: jsonLookup(null)
        }));
        return workflow;
    }

    function queryResourceByName(spReportService, type, name) {
        var query = {
            root: {
                id: type
            },
            selects: [
                { field: 'name', displayAs: 'Name' }
            ],
            conds: [
                { expr: { field: 'name' }, oper: 'equal', val: name }
            ]
        };
        return spReportService.runQuery(query);
    }

    angular.module('myTestModule', []).config(function ($provide) {
        $provide.decorator("$exceptionHandler", function ($delegate) {
            return function (exception, cause) {
                // DO NOT delegate to the default and don't rethrow as there is a "bug" in $q
                // such that if we do rethrow then $q fails to reject any current promise.
                //$delegate(exception, cause);

                console.error('exception: %s, %o %s', exception.message, exception, cause);
            };
        });
    });

    beforeEach(module('sp.common.loginService'));
    beforeEach(module('mod.common.spEntityService'));
    beforeEach(module('myTestModule'));
    beforeEach(module('mod.common.spWebService'));
    beforeEach(module('mod.common.spUserTask'));
    beforeEach(module('mod.services.promiseService'));
    beforeEach(module('spApps.reportServices'));
    beforeEach(module('mod.services.workflowService'));
    beforeEach(module('mod.services.workflowRunService'));
    beforeEach(module('mod.featureSwitch'));

    beforeEach(inject(function ($injector) {
        TestSupport.setupIntgTests(this, $injector);
    }));



    beforeEach(inject(function ($injector, $q, $rootScope, spEntityService, spWorkflowService, spWorkflowRunService) {
        TestSupport.setWaitTimeout(120000);

        timeout = function (f, t) {
            var d = $q.defer();
            setTimeout(function () {
                d.resolve(f());
                $rootScope.$apply();
            }, t);
            return d.promise;
        };
    }));

    beforeEach(inject(function ($q, spEntityService) {

        getTestEmployees = function () {
            return $q.when(testEmployees || spEntityService
                .getEntity('test:employee', 'instancesOfType.*').then(function (entity) {
                    testEmployees = entity.getInstancesOfType();
                    console.log('received test entities, first: ', spEntity.toJSON(_.first(testEmployees)));
                    return testEmployees;
                }));
        };
    }));

    describe('spWorkflowService', function () {

        it('100: should load', inject(function (spWorkflowService) {
            expect(spWorkflowService).toBeTruthy();
        }));

        it('101: should load', inject(function (spWorkflowRunService) {
            expect(spWorkflowRunService).toBeTruthy();
        }));

        it('110: can get activity type meta data', inject(function ($q, spWorkflowService, rnFeatureSwitch) {

            var resolve = {
                activityTypes: spWorkflowService.getActivityTypes()
            };

            var result = {};
            waitCheckReturn($q.all(resolve), result);

            runs(function () {
                console.log('test 110', result);
                expect(result.value.activityTypes).toBeArray();
                expect(result.value.activityTypes.length).toBeGreaterThan(0);
            });
        }));

        it('120: can get activities menu', inject(function ($q, spWorkflowService, rnFeatureSwitch) {

            var resolve = {
                menu: spWorkflowService.newWorkflow().then(spWorkflowService.getWorkflowMenu)
            };

            var result = {};
            waitCheckReturn($q.all(resolve), result);

            runs(function () {
                console.log('test 120', result);
                expect(result.value.menu).toBeArray();
                
                var expectedCount =  26;  // activities plus headers
                
                expect(result.value.menu.length).toBe(expectedCount); 
            });
        }));

        it('200: can create a new (unsaved) workflow', inject(function ($q, spWorkflowService) {
            var testId = getTestId(this);

            var resolve = {
                workflow: spWorkflowService.newWorkflow()
            };

            var result = {};
            waitCheckReturn($q.all(resolve), result);

            runs(function () {
                expect(result.value).toBeTruthy();
                expect(result.value.workflow).toBeTruthy();

                var workflow = result.value.workflow;
                console.log('test %s: workflow entity', testId, spEntity.toJSON(workflow.entity));

                expect(workflow.entity).toBeTruthy();
                expect(workflow.entity.getName()).toBe('New Workflow');

                expect(workflow.entity.getContainedActivities()).toBeArray(0);
                expect(workflow.entity.getTransitions()).toBeArray(0);
                expect(workflow.entity.getTerminations()).toBeArray(0);
                expect(workflow.entity.getInputArguments()).toBeArray(0);
                expect(workflow.entity.getOutputArguments()).toBeArray(0);
                expect(workflow.entity.getVariables()).toBeArray(0);
                expect(workflow.entity.getExitPoints()).toBeArray(1);
            });
        }));

        it('210: can save our new clean useless workflow', inject(function ($q, spWorkflowService) {
            var testId = getTestId(this);

            var ws = spWorkflowService;
            var resolve = {
                workflow: spWorkflowService.newWorkflow()
                    .then(addResourceIdWorkflowArgument)
                    .then(spWorkflowService.saveWorkflow)
            };

            var result = {};
            waitCheckReturn($q.all(resolve), result);

            runs(function () {
                expect(result.value).toBeTruthy();
                expect(result.value.workflow).toBeTruthy();

                var workflow = result.value.workflow;
                console.log('test %s: workflow entity', testId, spEntity.toJSON(workflow.entity));

                expect(workflow.entity).toBeTruthy();
                expect(workflow.entity.getName()).toBe('New Workflow');
                expect(workflow.entity.getContainedActivities()).toBeArray(0);
                expect(workflow.entity.getExpressionParameters()).toBeArray();
                expect(workflow.entity.getExpressionParameters().length).toBe(1);
                expect(workflow.entity.getExpressionParameters()[0].getName()).toBe('ResourceId');
            });
        }));

        it('220: can rename our default workflow, save it, reload it', inject(function ($q, spWorkflowService) {
            var testId = getTestId(this);

            var ws = spWorkflowService;
            var resolve = {
                workflow: ws.newWorkflow()
                    .then(addResourceIdWorkflowArgument)
                    .then(function (workflow) {
                        workflow.entity.setName('Workflow' + testId);
                        return workflow;
                    })
                    .then(ws.saveAndReopenWorkflow)
            };

            var result = {};
            waitCheckReturn($q.all(resolve), result);

            runs(function () {
                expect(result.value).toBeTruthy();
                expect(result.value.workflow).toBeTruthy();

                var workflow = result.value.workflow;
                console.log('test %s: workflow entity', testId, spEntity.toJSON(workflow.entity));

                expect(workflow.entity).toBeTruthy();
                expect(workflow.entity.getName()).toBe('Workflow' + testId);
                expect(workflow.entity.getContainedActivities()).toBeArray(0);
            });
        }));

        it('230: build out a simple workflow (2 acts), save and reload it', inject(function ($q, spWorkflowService) {
            var testId = getTestId(this);

            var ws = spWorkflowService;
            var resolve = {
                workflow: ws.newWorkflow()
                    .then(addResourceIdWorkflowArgument)
                    .then(function (workflow) {
                        setTestEntityName(workflow.entity, 'Workflow' + testId + '-no seqs');
                        return workflow;
                    })
                    .then(function (workflow) {
                        return $q.all([
                                ws.createActivity(workflow, 'core:logActivity'),
                                ws.createActivity(workflow, 'core:logActivity')
                            ]).then(function (activities) {
                                return ws.addActivities(workflow, activities);
                            });
                    })
                    .then(function (workflow) {
                        var workflowVar1 = spWorkflow.addActivityArgument(workflow, workflow.entity, 'variables', 'dateTimeArgument');
                        spWorkflow.updateArgument(workflow, workflow.entity, workflowVar1, { name: 'Created Date', text: "convert(date, '2000-01-01')" });
                        return workflow;
                    })
                    .then(ws.saveAndReopenWorkflow)
                    .then(updateTestWorkflowName)
                    .then(ws.saveAndReopenWorkflow)
            };

            var result = {};
            waitCheckReturn($q.all(resolve), result);

            runs(function () {
                expect(result.value).toBeTruthy();
                expect(result.value.workflow).toBeTruthy();

                var workflow = result.value.workflow;
                console.log('test %s: workflow entity', testId, spEntity.toJSON(workflow.entity));

                expect(workflow.entity).toBeTruthy();
                expect(workflow.entity.getContainedActivities()).toBeArray();
                expect(workflow.entity.getContainedActivities().length).toBe(2);
                expect(workflow.entity.getTransitions().length).toBe(0);
                expect(workflow.entity.getTerminations().length).toBe(0);
            });
        }));

        it('240: simple workflow, save and reload it, add and remove things', inject(function ($q, spWorkflowService) {
            var testId = getTestId(this);

            var ws = spWorkflowService;
            var resolve = {
                workflow: ws.newWorkflow()
                    .then(function (workflow) {
                        setTestEntityName(workflow.entity, 'Workflow' + testId);
                        return workflow;
                    })
                    .then(function (workflow) {
                        return $q.all([
                                ws.createActivity(workflow, 'core:logActivity', 'Log 1'),
                                ws.createActivity(workflow, 'core:logActivity', 'Log 2')
                            ]).then(function (activities) {

                                // the following will add each in turn after the start event and so we'd expect
                                // the end result for them to be in reverse.

                                return ws.addActivitiesInSequence(workflow, activities);
                            });
                    })
                    .then(function (workflow) {
                        return $q.all([
                                ws.createActivity(workflow, 'core:updateFieldActivity', 'Update Field 1'),
                                ws.createActivity(workflow, 'core:assignToVariable', 'Assign to Variable 1')
                            ]).then(function (activities) {

                                // setting the selected entity to the last activity added (which should be the first act in the workflow)
                                // means any further additions should each in turn insert after the selected, so insert into position 2...

                                workflow.selectedEntity = _.last(workflow.entity.getContainedActivities());
                                return ws.addActivitiesInSequence(workflow, activities);
                            });
                    })
                    .then(ws.saveAndReopenWorkflow)
                    .then(updateTestWorkflowName)
                    .then(ws.saveAndReopenWorkflow)
                    .then(function (workflow) {

                        // Do some checking for the activity arrangement we expect after the above activity insertions.

                        // this should be the order...
                        var log2 = _.find(workflow.entity.getContainedActivities(), _.partial(hasName, 'Log 2'));
                        var assign1 = _.find(workflow.entity.getContainedActivities(), _.partial(hasName, 'Assign to Variable 1'));
                        var update1 = _.find(workflow.entity.getContainedActivities(), _.partial(hasName, 'Update Field 1'));
                        var log1 = _.find(workflow.entity.getContainedActivities(), _.partial(hasName, 'Log 1'));

                        expect(log2).toBeTruthy();
                        expect(assign1).toBeTruthy();
                        expect(update1).toBeTruthy();
                        expect(log1).toBeTruthy();

                        expect(workflow.entity.getFirstActivity()).toBe(log2);
                        expect(workflow.entity.getTransitions().length).toBe(3);
                        expect(workflow.entity.getTerminations().length).toBe(1);

                        // remove the first activity and see ....

                        ws.removeActivity(workflow, log2);

                        console.log('removed activity', log2, spEntity.toJSON(log2));
                        console.log('first', workflow.entity.getFirstActivity(), assign1);

                        expect(_.find(workflow.entity.getContainedActivities(), _.partial(hasName, 'Log 2'))).toBeFalsy();
                        expect(workflow.entity.getFirstActivity()).toBe(assign1);
                        expect(workflow.entity.getTransitions().length).toBe(2);
                        expect(workflow.entity.getTerminations().length).toBe(1);

                        // stick it back

                        workflow.selectedEntity = workflow.entity;

                        ws.addActivitiesInSequence(workflow, [log2]);

                        expect(workflow.entity.getFirstActivity()).toBe(log2);
                        expect(workflow.entity.getTransitions().length).toBe(3);
                        expect(workflow.entity.getTerminations().length).toBe(1);

                        return workflow;
                    })
                    .then(function (workflow) {

                        // more of the same ... see comments above

                        var log2 = _.find(workflow.entity.getContainedActivities(), _.partial(hasName, 'Log 2'));
                        var read1 = _.find(workflow.entity.getContainedActivities(), _.partial(hasName, 'Read Field 1'));
                        var update1 = _.find(workflow.entity.getContainedActivities(), _.partial(hasName, 'Update Field 1'));
                        var log1 = _.find(workflow.entity.getContainedActivities(), _.partial(hasName, 'Log 1'));

                        expect(workflow.entity.getFirstActivity()).toBe(log2);
                        expect(workflow.entity.getTransitions().length).toBe(3);
                        expect(workflow.entity.getTerminations().length).toBe(1);

                        // remove the last activity and see ....

                        ws.removeActivity(workflow, log1);

                        console.log('removed activity', log1, spEntity.toJSON(log1));

                        expect(_.find(workflow.entity.getContainedActivities(), _.partial(hasName, 'Log 1'))).toBeFalsy();
                        expect(workflow.entity.getFirstActivity()).toBe(log2);
                        expect(workflow.entity.getTransitions().length).toBe(2);
                        expect(workflow.entity.getTerminations().length).toBe(1);

                        // stick it back at the front

                        workflow.selectedEntity = workflow.entity;

                        ws.addActivitiesInSequence(workflow, [log1]);

                        expect(_.find(workflow.entity.getContainedActivities(), _.partial(hasName, 'Log 1'))).toBeTruthy();
                        expect(workflow.entity.getFirstActivity()).toBe(log1);
                        expect(workflow.entity.getTransitions().length).toBe(3);
                        expect(workflow.entity.getTerminations().length).toBe(1);

                        return workflow;
                    })
                    .then(_.partial(logStuff, '1'))
                    .then(ws.saveAndReopenWorkflow)
                    .then(_.partial(logStuff, '2'))
            };

            var result = {};
            waitCheckReturn($q.all(resolve), result);

            runs(function () {
                expect(result.value).toBeTruthy();
                expect(result.value.workflow).toBeTruthy();

                var workflow = result.value.workflow;
                console.log('test %s: workflow entity', testId, spEntity.toJSON(workflow.entity));

                expect(workflow.entity).toBeTruthy();
                expect(workflow.entity.getContainedActivities()).toBeArray();
                expect(workflow.entity.getContainedActivities().length).toBe(4);
                expect(workflow.entity.getTransitions().length).toBe(3);
                expect(workflow.entity.getTerminations().length).toBe(1);
            });
        }));

        it('242: simple workflow, save and reload it, remove and save it, and reload it', inject(function ($q, spWorkflowService) {
            var testId = getTestId(this);

            var ws = spWorkflowService;
            var resolve = {
                workflow: ws.newWorkflow()
                    .then(function (workflow) {
                        setTestEntityName(workflow.entity, 'Workflow' + testId);
                        return workflow;
                    })
                    .then(function (workflow) {
                        return $q.all([
                                ws.createActivity(workflow, 'core:logActivity'),
                                ws.createActivity(workflow, 'core:logActivity')
                            ]).then(function (activities) {
                                return ws.addActivitiesInSequence(workflow, activities);
                            });
                    })
                    .then(_.partial(logStuff, '1'))
                    .then(ws.saveAndReopenWorkflow)
                    .then(function (workflow) {
                        expect(workflow.entity.getContainedActivities().length).toBe(2);
                        var a = _.first(workflow.entity.getContainedActivities());
                        expect(a).toBeTruthy();
                        ws.removeActivity(workflow, a);
                        expect(workflow.entity.getContainedActivities().length).toBe(1);
                        return workflow;
                    })
                    .then(_.partial(logStuff, '2'))
                    .then(ws.saveAndReopenWorkflow)
                    .then(function (workflow) {
                        expect(workflow.entity.getContainedActivities().length).toBe(1);
                        return workflow;
                    })
                    .then(updateTestWorkflowName)
                    .then(_.partial(logStuff, '3'))
                    .then(ws.saveAndReopenWorkflow)
                    .then(_.partial(logStuff, '4'))
            };

            var result = {};
            waitCheckReturn($q.all(resolve), result);

            runs(function () {
                expect(result.value).toBeTruthy();
                expect(result.value.workflow).toBeTruthy();

                var workflow = result.value.workflow;
                console.log('test %s: workflow entity', testId, spEntity.toJSON(workflow.entity));

                expect(workflow.entity).toBeTruthy();
                expect(workflow.entity.getContainedActivities()).toBeArray();
                expect(workflow.entity.getContainedActivities().length).toBe(1);
                expect(workflow.entity.getTransitions().length).toBe(0);
                expect(workflow.entity.getTerminations().length).toBe(1);
            });
        }));

        it('250: build workflow - connections', inject(function ($q, spWorkflowService) {
            var testId = getTestId(this);

            var ws = spWorkflowService;
            var resolve = {
                workflow: ws.newWorkflow()
                    .then(function (workflow) {
                        setTestEntityName(workflow.entity, 'Workflow' + testId);
                        return workflow;
                    })
                    .then(function (workflow) {
                        return $q.all([
                                ws.createActivity(workflow, 'core:logActivity'),
                                ws.createActivity(workflow, 'core:logActivity')
                            ]).then(function (activities) {
                                ws.addActivities(workflow, activities);
                                ws.addSequence(workflow, null, null, activities[0]);
                                ws.addSequence(workflow, activities[0], null, activities[1]);
                                ws.addSequence(workflow, activities[1], null, null);
                                return workflow;
                            });
                    })
                    .then(ws.saveAndReopenWorkflow)
                    .then(updateTestWorkflowName)
                    .then(ws.saveAndReopenWorkflow)
            };

            var result = {};
            waitCheckReturn($q.all(resolve), result);

            runs(function () {
                expect(result.value).toBeTruthy();
                expect(result.value.workflow).toBeTruthy();

                var workflow = result.value.workflow;
                console.log('test %s: workflow entity', testId, spEntity.toJSON(workflow.entity));

                expect(workflow.entity).toBeTruthy();
                expect(workflow.entity.getContainedActivities()).toBeArray();
                expect(workflow.entity.getContainedActivities().length).toBe(2);
                expect(workflow.entity.getTransitions().length).toBe(1);
                expect(workflow.entity.getTerminations().length).toBe(1);
            });
        }));

        it('260: build workflow - add connections by id', inject(function ($q, spWorkflowService) {
            var testId = getTestId(this);

            var ws = spWorkflowService;
            var resolve = {
                workflow: ws.newWorkflow()
                    .then(function (workflow) {
                        setTestEntityName(workflow.entity, 'Workflow' + testId);
                        return workflow;
                    })
                    .then(function (workflow) {
                        return $q.all([
                                ws.createActivity(workflow, 'core:updateFieldActivity'),
                                ws.createActivity(workflow, 'core:logActivity')
                            ]).then(function (activities) {
                                ws.addActivities(workflow, activities);
                                ws.addSequence(workflow, null, null, activities[1]);
                                ws.addSequence(workflow, activities[0], null, null);

                                ws.addSequence(workflow,
                                    spWorkflow.findWorkflowComponentEntity(workflow, activities[1].id()), null,
                                    spWorkflow.findWorkflowComponentEntity(workflow, activities[0].id()));
                                return workflow;
                            });
                    })
                    .then(ws.saveAndReopenWorkflow)
                    .then(updateTestWorkflowName)
                    .then(ws.saveAndReopenWorkflow)
            };

            var result = {};
            waitCheckReturn($q.all(resolve), result);

            runs(function () {
                expect(result.value).toBeTruthy();
                expect(result.value.workflow).toBeTruthy();

                var workflow = result.value.workflow;
                console.log('test %s: workflow entity', testId, spEntity.toJSON(workflow.entity));

                expect(workflow.entity).toBeTruthy();
                expect(workflow.entity.getContainedActivities()).toBeArray();
                expect(workflow.entity.getContainedActivities().length).toBe(2);
                expect(workflow.entity.getTransitions().length).toBe(1);
                expect(workflow.entity.getTerminations().length).toBe(1);
            });
        }));

        it('270: build workflow - build and run', inject(function ($q, spWorkflowService, spWorkflowRunService, spPromiseService) {
            var testId = getTestId(this);

            var ws = spWorkflowService;
            var wrs = spWorkflowRunService;
            var resolve = {
                workflow: ws.newWorkflow()
                    .then(addResourceIdWorkflowArgument)
                    .then(function (workflow) {
                        setTestEntityName(workflow.entity, 'Workflow' + testId);
                        return workflow;
                    })
                    .then(function (workflow) {
                        var inputs = workflow.entity.getInputArguments();
                        expect(inputs.length).toBe(1);
                        expect(_.first(inputs).getName()).toBe('ResourceId');
                        _.first(inputs).setConformsToType('test:employee');
                        return workflow;
                    })
                    .then(ws.saveAndReopenWorkflow)
                    .then(function (workflow) {
                        var inputs = workflow.entity.getInputArguments();
                        expect(inputs.length).toBe(1);
                        expect(_.first(inputs).getName()).toBe('ResourceId');
                        expect(_.first(inputs).getConformsToType().eid().getNsAlias()).toBe('test:employee');
                        return workflow;
                    })
                    .then(function (workflow) {
                        return $q.all([
                                ws.createActivity(workflow, 'core:assignToVariable', 'assign var'),
                                ws.createActivity(workflow, 'core:logActivity', 'log')
                            ]).then(function (activities) {
                                ws.addActivities(workflow, activities);
                                ws.addSequence(workflow, null, null, activities[0]);
                                ws.addSequence(workflow,
                                    spWorkflow.findWorkflowComponentEntity(workflow, activities[0].id()), null,
                                    spWorkflow.findWorkflowComponentEntity(workflow, activities[1].id()));
                                ws.addSequence(workflow, activities[1], null, null);
                                return workflow;
                            });
                    })
                    .then(_.partial(logStuff, '0 expressions'))
                    .then(function (workflow) {

                        // test getting and setting the configuration for the assign var activity

                        var workflowVar1 = spWorkflow.addActivityArgument(workflow, workflow.entity, 'variables', 'stringArgument');
                        spWorkflow.updateArgument(workflow, workflow.entity, workflowVar1, { name: 'Email Address', text: "'xyz@abc.com'" });

                        var activity = _.find(workflow.entity.getContainedActivities(), _.partial(hasName, 'assign var'));
                        //console.log('test %s: assign var activity', testId, spEntity.toJSON(activity));

                        expectParameterAndExpressionCounts(workflow, activity, 1, 1);

                        activity.setTargetVariable(workflowVar1);
                        spWorkflow.updateParameterExpression(workflow, activity, aliases.inputArguments, 'core:assignValueArgument', '[ResourceId].[Email Address]', false);

                        expectParameterAndExpressionCounts(workflow, activity, 1, 1);

                        return workflow;
                    })
                    .then(function (workflow) {

                        // test getting and setting the configuration for the log activity

                        var activity = _.find(workflow.entity.getContainedActivities(), _.partial(hasName, 'log'));
//                        console.log('test %s: log activity', testId, spEntity.toJSON(activity));

                        var parameters = getParameterMap(workflow, activity);
                        expectParameterAndExpressionCounts(workflow, activity, 2, 2);

                        spWorkflow.updateParameterExpression(workflow, activity, aliases.inputArguments, 'core:inLogActivityMessage',
                            'Hello {{[ResourceId]}} with email: {{[Email Address]}} and age: {{ResourceId.Age}}', true);

                        expectParameterAndExpressionCounts(workflow, activity, 2, 2);

                        expect(workflow.entity.getContainedActivities().length).toBe(2);
                        return workflow;
                    })
                    .then(_.partial(logStuff, '1 expressions'))
                    .then(ws.saveAndReopenWorkflow)
                    .then(_.partial(logStuff, '2 expressions'))
                    .then(updateTestWorkflowName)
                    .then(function (workflow) {
                        expect(workflow.entity.getContainedActivities().length).toBe(2);
                        var activity = _.find(workflow.entity.getContainedActivities(), _.partial(hasName, 'assign var'));
                        activity.setName('get created date');
                        logStuff('3 expressions', workflow);
                        return ws.activityUpdated(workflow, activity);
                    })
                    .then(_.partial(logStuff, '4 expressions'))
                    .then(ws.saveAndReopenWorkflow)
                    .then(function (workflow) {
                        expect(workflow.entity.getContainedActivities().length).toBe(2);
                        return workflow;
                    })
                    .then(_.partial(logStuff, '5 contained'))
                    .then(ws.saveAndReopenWorkflow)
                    .then(_.partial(logStuff, '6 contained'))
                    .then(_.partial(runWorkflowWithEmployeeResourceId, spWorkflowRunService))
                    .then(_.partial(waitForWorkflowRunStatus, spWorkflowRunService, spPromiseService, 'Completed'))
            };

            var result = {};
            waitCheckReturn($q.all(resolve), result);

            runs(function () {
                expect(result.value).toBeTruthy();
                expect(result.value.workflow).toBeTruthy();

                var workflow = result.value.workflow;
                console.log('test %s: workflow entity', testId, spEntity.toJSON(workflow.entity));

                logStuff('stuff ' + testId, workflow);

                expect(workflow.entity).toBeTruthy();
                expect(workflow.entity.getContainedActivities()).toBeArray();
                expect(workflow.entity.getContainedActivities().length).toBe(2);
                expect(workflow.entity.getTransitions().length).toBe(1);
                expect(workflow.entity.getTerminations().length).toBe(1);
                expect(_.flatten(_.invokeMap(workflow.entity.getContainedActivities(), 'getExpressionMap')).length).toBe(3);

                expect(workflow.lastRunStatus).toBe('Completed');
            });
        }));

        it('280: build workflow - build and run nested workflow', inject(function ($q, spWorkflowService, spWorkflowRunService, spPromiseService) {
            var testId = getTestId(this);

            var ws = spWorkflowService;
            var wrs = spWorkflowRunService;
            var resolve = {
                workflow: ws.newWorkflow()
                    .then(addResourceIdWorkflowArgument)
                    .then(function (workflow) {
                        setTestEntityName(workflow.entity, 'Workflow' + testId + '-inner');
                        return $q.all([
                                ws.createActivity(workflow, 'core:assignToVariable', 'assign var'),
                                ws.createActivity(workflow, 'core:logActivity', 'log')
                            ]).then(function (activities) {
                                return ws.addActivitiesInSequence(workflow, activities.reverse());
                            });
                    })
                    .then(function (workflow) {
                        var workflowVar1 = spWorkflow.addActivityArgument(workflow, workflow.entity, 'variables', 'dateTimeArgument');
                        spWorkflow.updateArgument(workflow, workflow.entity, workflowVar1, { name: 'Created Date', text: "convert(date, '2000-01-01')" });

                        var activity = _.find(workflow.entity.getContainedActivities(), _.partial(hasName, 'assign var'));

                        activity.setTargetVariable(workflowVar1);
                        spWorkflow.updateParameterExpression(workflow, activity, aliases.inputArguments, 'core:assignValueArgument', '[ResourceId].[Created Date]', false);

                        return workflow;
                    })
                    .then(function (workflow) {
                        var activity = _.find(workflow.entity.getContainedActivities(), _.partial(hasName, 'log'));
                        spWorkflow.updateParameterExpression(workflow, activity, aliases.inputArguments, 'core:inLogActivityMessage',
                            'Inner: Hello {{[ResourceId]}} with created date {{[Created Date]}}', true);

                        return workflow;
                    })
                    .then(ws.saveAndReopenWorkflow)
                    .then(updateTestWorkflowName)
                    .then(ws.saveAndReopenWorkflow)
                    .then(function (workflow) {
                        // stash the workflow as the one to run in the new workflow we are about to create
                        var inner = workflow.entity;
                        return ws.newWorkflow()
                            .then(addResourceIdWorkflowArgument)
                            .then(function (workflow) {
                            return ws.createActivity(workflow, aliases.workflowProxy, 'run inner').then(function (activity) {
                                setTestEntityName(workflow.entity, 'Workflow' + testId + '-outer');
                                ws.addActivitiesInSequence(workflow, [activity]);
                                activity.setLookup(aliases.workflowToProxy, inner.id());
                                return ws.activityUpdated(workflow, activity, 'workflowToProxy').then(function (workflow) {
                                    // to configure the input parameters
                                    var parameters = getParameterMap(workflow, activity);
                                    expect(_.keys(parameters).length).toBe(1);
                                    spWorkflow.updateParameterExpression(workflow, activity, aliases.inputArguments, _.first(_.keys(parameters)), '[ResourceId]', false);
                                    return workflow;
                                });
                            });
                        });
                    })
                    .then(ws.saveAndReopenWorkflow)
                    .then(updateTestWorkflowName)
                    .then(ws.saveAndReopenWorkflow)
                    .then(_.partial(runWorkflowWithThisWorkflowResourceId, spWorkflowRunService))
                    .then(_.partial(waitForWorkflowRunStatus, spWorkflowRunService, spPromiseService, 'Completed'))
            };

            var result = {};
            waitCheckReturn($q.all(resolve), result);

            runs(function () {
                expect(result.value).toBeTruthy();
                expect(result.value.workflow).toBeTruthy();

                var workflow = result.value.workflow;
                console.log('test %s: workflow entity', testId, spEntity.toJSON(workflow.entity));

                expect(workflow.entity).toBeTruthy();
                expect(workflow.entity.getContainedActivities()).toBeArray();
                expect(workflow.entity.getContainedActivities().length).toBe(1);
                expect(workflow.entity.getTransitions().length).toBe(0);
                expect(workflow.entity.getTerminations().length).toBe(1);
                expect(_.flatten(_.invokeMap(workflow.entity.getContainedActivities(), 'getExpressionMap')).length).toBe(1);

                expect(workflow.lastRunStatus).toBe('Completed');
            });
        }));


        it('300: build workflow - build and run workflow with gateway', inject(function ($q, spWorkflowService, spWorkflowRunService, spPromiseService) {
            var testId = getTestId(this);

            var ws = spWorkflowService;
            var wrs = spWorkflowService;
            var resolve = {
                workflow: ws.newWorkflow()
                    .then(addResourceIdWorkflowArgument)
                    .then(function (workflow) {
                        setTestEntityName(workflow.entity, 'Workflow' + testId);
                        return workflow;
                    })
                    .then(function (workflow) {
                        return $q.all([
                                ws.createActivity(workflow, 'core:logActivity', 'log'),
                                ws.createActivity(workflow, 'core:decisionActivity', 'gateway'),
                                ws.createActivity(workflow, 'core:updateFieldActivity', 'update A'),
                                ws.createActivity(workflow, 'core:updateFieldActivity', 'update B'),
                                ws.createActivity(workflow, 'core:logActivity', 'log2')
                            ]).then(function (activities) {
                                ws.addActivities(workflow, activities);
                                ws.addSequence(workflow, null, null, activities[0]);
                                ws.addSequence(workflow, activities[0], null, activities[1]);
                                ws.addSequence(workflow, activities[1], null, activities[2]);
                                ws.addSequence(workflow, activities[1], null, activities[3]);
                                ws.addSequence(workflow, activities[2], null, activities[4]);
                                ws.addSequence(workflow, activities[3], null, activities[4]);
                                ws.addSequence(workflow, activities[4], null, null);
                                return workflow;
                            });
                    })
                    .then(function (workflow) {
                        var activity = _.find(workflow.entity.getContainedActivities(), _.partial(hasName, 'log'));
                        spWorkflow.updateParameterExpression(workflow, activity, aliases.inputArguments, 'core:inLogActivityMessage',
                            '{{[ResourceId].Description}}', true);

                        activity = _.find(workflow.entity.getContainedActivities(), _.partial(hasName, 'log2'));
                        spWorkflow.updateParameterExpression(workflow, activity, aliases.inputArguments, 'core:inLogActivityMessage',
                            '{{[ResourceId].Description}}', true);

                        return workflow;
                    })
                    .then(function (workflow) {
                        var activity = _.find(workflow.entity.getContainedActivities(), _.partial(hasName, 'gateway'));
                        spWorkflow.updateParameterExpression(workflow, activity, aliases.inputArguments, 'core:decisionActivityDecisionArgument', '1 > 10', false);
                        return workflow;
                    })
                    .then(function (workflow) {

                        var activity = _.find(workflow.entity.getContainedActivities(), _.partial(hasName, 'update A'));

                        expectParameterAndExpressionCounts(workflow, activity, 1, 1);

                        spWorkflow.updateParameterExpression(workflow, activity, aliases.inputArguments, 'core:updateFieldActivityResourceArgument', '[ResourceId]', false);

                        var baseName = spWorkflow.addSetFieldArgs(workflow, activity);
                        var field = spEntity.fromId('description');
                        field.typeId = 'field';
                        field.name = 'Description';
                        field.setDataState(spEntity.DataStateEnum.Unchanged);

                        spWorkflow.updateParameterEntityExpressionByName(workflow, activity, aliases.inputArguments, baseName, field);
                        spWorkflow.updateParameterExpressionByName(workflow, activity, aliases.inputArguments, baseName + '_value', "'A'");

                        expectParameterAndExpressionCounts(workflow, activity, 3, 3);

                        return workflow;
                    })
                    .then(function (workflow) {

                        var activity = _.find(workflow.entity.getContainedActivities(), _.partial(hasName, 'update B'));

                        spWorkflow.updateParameterExpression(workflow, activity, aliases.inputArguments, 'core:updateFieldActivityResourceArgument', '[ResourceId]', false);

                        var baseName = spWorkflow.addSetFieldArgs(workflow, activity);
                        var field = spEntity.fromId('description');
                        field.typeId = 'field';
                        field.name = 'Description';
                        field.setDataState(spEntity.DataStateEnum.Unchanged);

                        spWorkflow.updateParameterEntityExpressionByName(workflow, activity, aliases.inputArguments, baseName, field);
                        spWorkflow.updateParameterExpressionByName(workflow, activity, aliases.inputArguments, baseName + '_value', "'B'");

                        return workflow;
                    })
                    .then(ws.saveAndReopenWorkflow)
                    .then(updateTestWorkflowName)
                    .then(ws.saveAndReopenWorkflow)
                    .then(_.partial(runWorkflowWithThisWorkflowResourceId, spWorkflowRunService))
                    .then(_.partial(waitForWorkflowRunStatus, spWorkflowRunService, spPromiseService, 'Completed'))
                    .then(function (workflow) {
                        expect(workflow.lastRunStatus).toBe('Completed');
                        return workflow;
                    })
                    .then(ws.reopenWorkflow)
                    .then(function (workflow) {
                        expect(workflow.entity.getDescription()).toBe('B');
                        return workflow;
                    })
            };

            var result = {};
            waitCheckReturn($q.all(resolve), result);

            runs(function () {
                expect(result.value).toBeTruthy();
                expect(result.value.workflow).toBeTruthy();

                var workflow = result.value.workflow;
                console.log('test %s: workflow entity', testId, spEntity.toJSON(workflow.entity));

                expect(workflow.entity).toBeTruthy();
                expect(workflow.entity.getContainedActivities()).toBeArray();
                expect(workflow.entity.getContainedActivities().length).toBe(5);
                expect(workflow.entity.getTransitions().length).toBe(5);
                expect(workflow.entity.getTerminations().length).toBe(1);
                expect(_.flatten(_.invokeMap(workflow.entity.getContainedActivities(), 'getExpressionMap')).length).toBe(11);
            });
        }));

        it('310: build workflow - build and run workflow with gateway and auto sequence when adding', inject(function ($q, spWorkflowService, spWorkflowRunService, spPromiseService) {
            var testId = getTestId(this);

            var ws = spWorkflowService;
            var resolve = {
                workflow: ws.newWorkflow()
                    .then(addResourceIdWorkflowArgument)
                    .then(function (workflow) {
                        setTestEntityName(workflow.entity, 'Workflow' + testId);
                        return workflow;
                    })
                    .then(function (workflow) {
                        return $q.all([
                                ws.createActivity(workflow, 'core:logActivity', 'log'),
                                ws.createActivity(workflow, 'core:decisionActivity', 'gateway'),
                                ws.createActivity(workflow, 'core:updateFieldActivity', 'update A'),
                                ws.createActivity(workflow, 'core:updateFieldActivity', 'update B'),
                                ws.createActivity(workflow, 'core:logActivity', 'log2')
                        ])
                            .then(function (activities) {
                                // add each inserting at the start, in reverse to they appear in the order above
                                ws.addActivitiesInSequence(workflow, activities.slice(0).reverse());
                                // find seq into "update B" and move it to come from the gateway
                                var seq = _.find(workflow.entity.getTransitions(), function (t) {
                                    return t.getToActivity() === activities[3];
                                });
                                // this should use the free "no" exit on the gateway
                                ws.updateSequence(workflow, seq, activities[1], null, activities[3]);
                                // add in a seq from "update A" to "log 2"
                                ws.addSequence(workflow, activities[2], null, activities[4]);
                                return workflow;
                            });
                    })
                    .then(function (workflow) {
                        var activity = _.find(workflow.entity.getContainedActivities(), _.partial(hasName, 'log'));
                        spWorkflow.updateParameterExpression(workflow, activity, aliases.inputArguments, 'core:inLogActivityMessage',
                            '{{[ResourceId].Description}}', true);

                        activity = _.find(workflow.entity.getContainedActivities(), _.partial(hasName, 'log2'));
                        spWorkflow.updateParameterExpression(workflow, activity, aliases.inputArguments, 'core:inLogActivityMessage',
                            '{{[ResourceId].Description}}', true);

                        return workflow;
                    })
                    .then(function (workflow) {
                        var activity = _.find(workflow.entity.getContainedActivities(), _.partial(hasName, 'gateway'));
                        spWorkflow.updateParameterExpression(workflow, activity, aliases.inputArguments, 'core:decisionActivityDecisionArgument', '11 > 10', false);
                        return workflow;
                    })
                    .then(function (workflow) {

                        var activity = _.find(workflow.entity.getContainedActivities(), _.partial(hasName, 'update A'));

                        spWorkflow.updateParameterExpression(workflow, activity, aliases.inputArguments, 'core:updateFieldActivityResourceArgument', '[ResourceId]', false);

                        var baseName = spWorkflow.addSetFieldArgs(workflow, activity);
                        var field = spEntity.fromId('description');
                        field.typeId = 'field';
                        field.name = 'Description';
                        field.setDataState(spEntity.DataStateEnum.Unchanged);

                        spWorkflow.updateParameterEntityExpressionByName(workflow, activity, aliases.inputArguments, baseName, field);
                        spWorkflow.updateParameterExpressionByName(workflow, activity, aliases.inputArguments, baseName + '_value', "'A'");

                        return workflow;
                    })
                    .then(function (workflow) {

                        var activity = _.find(workflow.entity.getContainedActivities(), _.partial(hasName, 'update B'));

                        spWorkflow.updateParameterExpression(workflow, activity, aliases.inputArguments, 'core:updateFieldActivityResourceArgument', '[ResourceId]', false);

                        var baseName = spWorkflow.addSetFieldArgs(workflow, activity);
                        var field = spEntity.fromId('description');
                        field.typeId = 'field';
                        field.name = 'Description';
                        field.setDataState(spEntity.DataStateEnum.Unchanged);

                        spWorkflow.updateParameterEntityExpressionByName(workflow, activity, aliases.inputArguments, baseName, field);
                        spWorkflow.updateParameterExpressionByName(workflow, activity, aliases.inputArguments, baseName + '_value', "'B'");

                        return workflow;
                    })
                    .then(ws.saveAndReopenWorkflow)
                    .then(updateTestWorkflowName)
                    .then(ws.saveAndReopenWorkflow)
                    .then(_.partial(runWorkflowWithThisWorkflowResourceId, spWorkflowRunService))
                    .then(_.partial(waitForWorkflowRunStatus, spWorkflowRunService, spPromiseService, 'Completed'))
                    .then(function (workflow) {
                        expect(workflow.lastRunStatus).toBe('Completed');
                        return workflow;
                    })
                    .then(ws.reopenWorkflow)
                    .then(function (workflow) {
                        expect(workflow.entity.getDescription()).toBe('A');
                        return workflow;
                    })
            };

            var result = {};
            waitCheckReturn($q.all(resolve), result);

            runs(function () {
                expect(result.value).toBeTruthy();
                expect(result.value.workflow).toBeTruthy();

                var workflow = result.value.workflow;
                console.log('test %s: workflow entity', testId, spEntity.toJSON(workflow.entity));

                expect(workflow.entity).toBeTruthy();
                expect(workflow.entity.getContainedActivities()).toBeArray();
                expect(workflow.entity.getContainedActivities().length).toBe(5);
                expect(workflow.entity.getTransitions().length).toBe(5);
                expect(workflow.entity.getTerminations().length).toBe(1);
                expect(_.flatten(_.invokeMap(workflow.entity.getContainedActivities(), 'getExpressionMap')).length).toBe(11);
            });
        }));

        it('320: build workflow - build and run workflow with gateway and some manual sequencing', inject(function ($q, spWorkflowService, spWorkflowRunService, spPromiseService) {
            var testId = getTestId(this);

            var ws = spWorkflowService;
            var resolve = {
                workflow: ws.newWorkflow()
                    .then(addResourceIdWorkflowArgument)
                    .then(function (workflow) {
                        setTestEntityName(workflow.entity, 'Workflow' + testId);
                        return workflow;
                    })
                    .then(function (workflow) {
                        return $q.all([
                                ws.createActivity(workflow, 'core:logActivity', 'log'),
                                ws.createActivity(workflow, 'core:decisionActivity', 'gateway')
                            ]).then(function (activities) {
                                // add each inserting at the start, in reverse to they appear in the order above
                                expect(workflow.entity.getTransitions().length).toBe(0);
                                ws.addActivitiesInSequence(workflow, activities.slice(0).reverse());
                                expect(workflow.entity.getTransitions().length).toBe(1);
                                return workflow;
                            });
                    })
                    .then(function (workflow) {
                        var activity = _.find(workflow.entity.getContainedActivities(), _.partial(hasName, 'gateway'));
                        expect(spWorkflow.getExitPoints(workflow, activity).length).toBe(2);
                        workflow.selectedEntity = activity;
                        workflow.selectedExit = spEntity.findByEid(spWorkflow.getExitPoints(workflow, activity), 'decisionActivityYesExitPoint');
                        expect(workflow.selectedExit).toBeTruthy();
                        return workflow;
                    })
                    .then(function (workflow) {
                        return $q.all([
                                ws.createActivity(workflow, 'core:updateFieldActivity', 'update A'),
                                ws.createActivity(workflow, 'core:logActivity', 'log2')
                            ]).then(function (activities) {
                                ws.addActivitiesInSequence(workflow, [activities[0]]);
                                workflow.selectedEntity = activities[0];
                                ws.addActivitiesInSequence(workflow, [activities[1]]);
                                expect(workflow.entity.getTransitions().length).toBe(3);
                                return workflow;
                            });
                    })
                    .then(function (workflow) {
                        var activity = _.find(workflow.entity.getContainedActivities(), _.partial(hasName, 'gateway'));
                        workflow.selectedEntity = activity;
                        expect(spWorkflow.getExitPoints(workflow, activity).length).toBe(2);
                        workflow.selectedExit = spEntity.findByEid(spWorkflow.getExitPoints(workflow, activity), 'decisionActivityNoExitPoint');
                        expect(workflow.selectedExit).toBeTruthy();
                        return workflow;
                    })
                    .then(function (workflow) {
                        return $q.all([
                                ws.createActivity(workflow, 'core:updateFieldActivity', 'update B')
                            ]).then(function (activities) {
                                expect(workflow.entity.getTerminations().length).toBe(1);
                                ws.addActivitiesInSequence(workflow, activities);
                                expect(workflow.entity.getTransitions().length).toBe(4);
                                expect(workflow.entity.getTerminations().length).toBe(2);
                                // find seq out of "update B" and send it to "log 2"
                                var exit = spWorkflow.getExitPoints(workflow, activities[0])[0];
                                var seq = spWorkflow.getSequenceUsingExit(workflow, activities[0], exit);
                                var toActivity = _.find(workflow.entity.getContainedActivities(), _.partial(hasName, 'log2'));
                                expect(seq).toBeTruthy();
                                expect(toActivity).toBeTruthy();
                                ws.updateSequence(workflow, seq, seq.getFromActivity(), seq.getFromExitPoint(), toActivity);
                                return workflow;
                            });
                    })
                    .then(function (workflow) {
                        var activity = _.find(workflow.entity.getContainedActivities(), _.partial(hasName, 'log'));
                        spWorkflow.updateParameterExpression(workflow, activity, aliases.inputArguments, 'core:inLogActivityMessage',
                            '{{[ResourceId].Description}}', true);

                        activity = _.find(workflow.entity.getContainedActivities(), _.partial(hasName, 'log2'));
                        spWorkflow.updateParameterExpression(workflow, activity, aliases.inputArguments, 'core:inLogActivityMessage',
                            '{{[ResourceId].Description}}', true);

                        return workflow;
                    })
                    .then(function (workflow) {
                        var activity = _.find(workflow.entity.getContainedActivities(), _.partial(hasName, 'gateway'));
                        spWorkflow.updateParameterExpression(workflow, activity, aliases.inputArguments, 'core:decisionActivityDecisionArgument', '11 > 10', false);
                        return workflow;
                    })
                    .then(function (workflow) {

                        var activity = _.find(workflow.entity.getContainedActivities(), _.partial(hasName, 'update A'));

                        expectParameterAndExpressionCounts(workflow, activity, 1, 1);

                        spWorkflow.updateParameterExpression(workflow, activity, aliases.inputArguments, 'core:updateFieldActivityResourceArgument', '[ResourceId]', false);

                        var baseName = spWorkflow.addSetFieldArgs(workflow, activity);
                        var field = spEntity.fromId('description');
                        field.typeId = 'field';
                        field.name = 'Description';
                        field.setDataState(spEntity.DataStateEnum.Unchanged);

                        spWorkflow.updateParameterEntityExpressionByName(workflow, activity, aliases.inputArguments, baseName, field);
                        spWorkflow.updateParameterExpressionByName(workflow, activity, aliases.inputArguments, baseName + '_value', "'A'");

                        expectParameterAndExpressionCounts(workflow, activity, 3, 3);

                        return workflow;
                    })
                    .then(function (workflow) {

                        var activity = _.find(workflow.entity.getContainedActivities(), _.partial(hasName, 'update B'));

                        spWorkflow.updateParameterExpression(workflow, activity, aliases.inputArguments, 'core:updateFieldActivityResourceArgument', '[ResourceId]', false);

                        var baseName = spWorkflow.addSetFieldArgs(workflow, activity);
                        var field = spEntity.fromId('description');
                        field.typeId = 'field';
                        field.name = 'Description';
                        field.setDataState(spEntity.DataStateEnum.Unchanged);

                        spWorkflow.updateParameterEntityExpressionByName(workflow, activity, aliases.inputArguments, baseName, field);
                        spWorkflow.updateParameterExpressionByName(workflow, activity, aliases.inputArguments, baseName + '_value', "'B'");

                        return workflow;
                    })
                    .then(ws.saveAndReopenWorkflow)
                    .then(updateTestWorkflowName)
                    .then(ws.saveAndReopenWorkflow)
                    .then(_.partial(runWorkflowWithThisWorkflowResourceId, spWorkflowRunService))
                    .then(_.partial(waitForWorkflowRunStatus, spWorkflowRunService, spPromiseService, 'Completed'))
                    .then(function (workflow) {
                        expect(workflow.lastRunStatus).toBe('Completed');
                        return workflow;
                    })
                    .then(ws.reopenWorkflow)
                    .then(function (workflow) {
                        expect(workflow.entity.getDescription()).toBe('A');
                        return workflow;
                    })
            };

            var result = {};
            waitCheckReturn($q.all(resolve), result);

            runs(function () {
                expect(result.value).toBeTruthy();
                expect(result.value.workflow).toBeTruthy();

                var workflow = result.value.workflow;
                console.log('test %s: workflow entity', testId, spEntity.toJSON(workflow.entity));

                expect(workflow.entity).toBeTruthy();
                expect(workflow.entity.getContainedActivities()).toBeArray();
                expect(workflow.entity.getContainedActivities().length).toBe(5);
                expect(workflow.entity.getTransitions().length).toBe(5);
                expect(workflow.entity.getTerminations().length).toBe(1);
                expect(_.flatten(_.invokeMap(workflow.entity.getContainedActivities(), 'getExpressionMap')).length).toBe(11);
            });
        }));

        // removing test until we know what is going on
        xit('330: build and run workflow with get resources and forEeach resource loop', inject(function ($q, spWorkflowService, spWorkflowRunService, spPromiseService) {
            var testId = getTestId(this);

            var ws = spWorkflowService;
            var resolve = {
                workflow: ws.newWorkflow()
                    .then(function (workflow) {
                        setTestEntityName(workflow.entity, 'Workflow' + testId);
                        // remove the default ResourceId input as it isn't needed here
                        workflow.entity.getInputArguments().clear();
                        return workflow;
                    })
                    .then(function (workflow) {
                        return $q.all([
                                ws.createActivity(workflow, 'core:getResourcesActivity', 'get resources'),
                                ws.createActivity(workflow, 'core:forEachResource', 'foreach'),
                                ws.createActivity(workflow, 'core:logActivity', 'log')
                            ]).then(function (activities) {
                                ws.addActivities(workflow, activities);
                                ws.addSequence(workflow, null, null, activities[0]);
                                ws.addSequence(workflow, activities[0], null, activities[1]);
                                // the following assumes the loop exit is the default exit and will be used first
                                ws.addSequence(workflow, activities[1], null, null);
                                ws.addSequence(workflow, activities[1], null, activities[2]);
                                // loop back
                                ws.addSequence(workflow, activities[2], null, activities[1]);
                                return workflow;
                            });
                    })
                    .then(function (workflow) {
                        var activity = _.find(workflow.entity.getContainedActivities(), _.partial(hasName, 'get resources'));
                        expectParameterAndExpressionCounts(workflow, activity, 2, 2);

                        spWorkflow.updateParameterEntityExpression(workflow, activity, aliases.inputArguments, 'core:getResourcesResourceType', spEntity.fromId('test:employee'));
                        expectParameterAndExpressionCounts(workflow, activity, 2, 2);

                        var p = spWorkflow.findWorkflowExpressionParameter(workflow, activity, 'core:getResourcesFirst');
                        var p2 = spWorkflow.findWorkflowExpressionParameter(workflow, activity, 'core:getResourcesList');

                        expect(p).toBeTruthy();
                        expect(p2).toBeTruthy();

                        p.instanceConformsToType = 'test:employee';
                        p2.instanceConformsToType = 'test:employee';

                        return workflow;
                    })
                    .then(function (workflow) {
                        var activity = _.find(workflow.entity.getContainedActivities(), _.partial(hasName, 'foreach'));

                        spWorkflow.updateParameterExpression(workflow, activity, aliases.inputArguments, 'core:foreachList', '[get resources.List]', false);
                        expectParameterAndExpressionCounts(workflow, activity, 1, 1);

                        var p = spWorkflow.findWorkflowExpressionParameter(workflow, activity, 'core:foreachSelectedResource');

                        expect(p).toBeTruthy();

                        p.instanceConformsToType = 'test:employee';

                        return workflow;
                    })
                    .then(function (workflow) {
                        var activity = _.find(workflow.entity.getContainedActivities(), _.partial(hasName, 'log'));
                        spWorkflow.updateParameterExpression(workflow, activity, aliases.inputArguments, 'core:inLogActivityMessage',
                            '{{[foreach.Record].Name}} has age {{[foreach.Record].Age}}', true);

                        expectParameterAndExpressionCounts(workflow, activity, 1, 1);
                        return workflow;
                    })
                    .then(ws.saveAndReopenWorkflow)
                    .then(function (workflow) {

                        _.forEach(workflow.entity.expressionParameters, function (p) {
                            console.log('expressionParameter: ', spEntity.toJSON(p));
                        });

                        return workflow;
                    })
                    .then(updateTestWorkflowName)
                    .then(ws.saveAndReopenWorkflow)
                    .then(_.partial(runWorkflowWithEmployeeResourceId, spWorkflowRunService))
                    .then(_.partial(waitForWorkflowRunStatus, spWorkflowRunService, spPromiseService, 'Completed'))
                    .then(function (workflow) {
                        expect(workflow.lastRunStatus).toBe('Completed');
                        return workflow;
                    })
                    .then(ws.reopenWorkflow)
            };

            var result = {};
            waitCheckReturn($q.all(resolve), result);

            runs(function () {
                expect(result.value).toBeTruthy();
                expect(result.value.workflow).toBeTruthy();

                var workflow = result.value.workflow;
                console.log('test %s: workflow entity', testId, spEntity.toJSON(workflow.entity));

                expect(workflow.entity).toBeTruthy();
                expect(workflow.entity.getContainedActivities()).toBeArray();
                expect(workflow.entity.getContainedActivities().length).toBe(3);
                expect(workflow.entity.getTransitions().length).toBe(3);
                expect(workflow.entity.getTerminations().length).toBe(1);
                expect(workflow.entity.getExpressionParameters().length).toBe(4);
                expect(_.flatten(_.invokeMap(workflow.entity.getContainedActivities(), 'getExpressionMap')).length).toBe(4);
            });
        }));

        it('340: build workflow - build and run BIG workflow with 40+ acts', inject(function ($q, spWorkflowService, spWorkflowRunService, spPromiseService) {
            var testId = getTestId(this);

            var ws = spWorkflowService;
            var resolve = {
                workflow: ws.newWorkflow()
                    .then(addResourceIdWorkflowArgument)
                    .then(function (workflow) {
                        setTestEntityName(workflow.entity, 'Workflow' + testId);
                        return workflow;
                    })
                    .then(function (workflow) {
                        var manyLogActs = _.map(_.range(40), function (n) { return ws.createActivity(workflow, 'core:logActivity', 'logX' + n); });
                        return $q.all([
                            ws.createActivity(workflow, 'core:logActivity', 'log'),
                            ws.createActivity(workflow, 'core:decisionActivity', 'gateway'),
                            ws.createActivity(workflow, 'core:updateFieldActivity', 'update A'),
                            ws.createActivity(workflow, 'core:updateFieldActivity', 'update B'),
                            ws.createActivity(workflow, 'core:logActivity', 'log2')
                        ].concat(manyLogActs))
                            .then(function (activities) {
                                // add each inserting at the start, in reverse to they appear in the order above
                                ws.addActivitiesInSequence(workflow, activities.slice(0).reverse());
                                // find seq into "update B" and move it to come from the gateway
                                var seq = _.find(workflow.entity.getTransitions(), function (t) {
                                    return t.getToActivity() === activities[3];
                                });
                                // this should use the free "no" exit on the gateway
                                ws.updateSequence(workflow, seq, activities[1], null, activities[3]);
                                // add in a seq from "update A" to "log 2"
                                ws.addSequence(workflow, activities[2], null, activities[4]);
                                return workflow;
                            });
                    })
                    .then(function (workflow) {
                        _.forEach(workflow.entity.containedActivities, function (actEntity) {
                            if (actEntity.type.nsAlias === 'core:logActivity') {
                                spWorkflow.updateParameterExpression(workflow, actEntity, aliases.inputArguments, 'core:inLogActivityMessage',
                                        "hello from activity: " + actEntity.name, true);
                            }
                        });

                        return workflow;
                    })
                    .then(function (workflow) {
                        var activity = _.find(workflow.entity.getContainedActivities(), _.partial(hasName, 'log'));
                        spWorkflow.updateParameterExpression(workflow, activity, aliases.inputArguments, 'core:inLogActivityMessage',
                            '{{[ResourceId].Description}}', true);

                        activity = _.find(workflow.entity.getContainedActivities(), _.partial(hasName, 'log2'));
                        spWorkflow.updateParameterExpression(workflow, activity, aliases.inputArguments, 'core:inLogActivityMessage',
                            '{{[ResourceId].Description}}', true);

                        return workflow;
                    })
                    .then(function (workflow) {
                        var activity = _.find(workflow.entity.getContainedActivities(), _.partial(hasName, 'gateway'));
                        spWorkflow.updateParameterExpression(workflow, activity, aliases.inputArguments, 'core:decisionActivityDecisionArgument', '11 > 10', false);
                        return workflow;
                    })
                    .then(function (workflow) {

                        var activity = _.find(workflow.entity.getContainedActivities(), _.partial(hasName, 'update A'));

                        spWorkflow.updateParameterExpression(workflow, activity, aliases.inputArguments, 'core:updateFieldActivityResourceArgument', '[ResourceId]', false);

                        var baseName = spWorkflow.addSetFieldArgs(workflow, activity);
                        var field = spEntity.fromId('description');
                        field.typeId = 'field';
                        field.name = 'Description';
                        field.setDataState(spEntity.DataStateEnum.Unchanged);

                        spWorkflow.updateParameterEntityExpressionByName(workflow, activity, aliases.inputArguments, baseName, field);
                        spWorkflow.updateParameterExpressionByName(workflow, activity, aliases.inputArguments, baseName + '_value', "'A'");

                        return workflow;
                    })
                    .then(function (workflow) {

                        var activity = _.find(workflow.entity.getContainedActivities(), _.partial(hasName, 'update B'));

                        spWorkflow.updateParameterExpression(workflow, activity, aliases.inputArguments, 'core:updateFieldActivityResourceArgument', '[ResourceId]', false);

                        var baseName = spWorkflow.addSetFieldArgs(workflow, activity);
                        var field = spEntity.fromId('description');
                        field.typeId = 'field';
                        field.name = 'Description';
                        field.setDataState(spEntity.DataStateEnum.Unchanged);
                        
                        spWorkflow.updateParameterEntityExpressionByName(workflow, activity, aliases.inputArguments, baseName, field);
                        spWorkflow.updateParameterExpressionByName(workflow, activity, aliases.inputArguments, baseName + '_value', "'B'");

                        return workflow;
                    })
                    .then(ws.saveAndReopenWorkflow)
                    .then(updateTestWorkflowName)
                    .then(ws.saveAndReopenWorkflow)
                    .then(_.partial(runWorkflowWithThisWorkflowResourceId, spWorkflowRunService))
                    .then(_.partial(waitForWorkflowRunStatus, spWorkflowRunService, spPromiseService, 'Completed'))
                    .then(function (workflow) {
                        expect(workflow.lastRunStatus).toBe('Completed');
                        return workflow;
                    })
                    .then(ws.reopenWorkflow)
                    .then(function (workflow) {
                        expect(workflow.entity.getDescription()).toBe('A');
                        return workflow;
                    })
            };

            var result = {};
            waitCheckReturn($q.all(resolve), result);

            runs(function () {
                expect(result.value).toBeTruthy();
                expect(result.value.workflow).toBeTruthy();

                var workflow = result.value.workflow;
                console.log('test %s: workflow entity', testId, spEntity.toJSON(workflow.entity));

                expect(workflow.entity).toBeTruthy();
                expect(workflow.entity.getContainedActivities()).toBeArray();
                expect(workflow.entity.getContainedActivities().length).toBe(45);
                expect(workflow.entity.getTransitions().length).toBe(45);
                expect(workflow.entity.getTerminations().length).toBe(1);
                expect(_.flatten(_.invokeMap(workflow.entity.getContainedActivities(), 'getExpressionMap')).length).toBe(91);
            });
        }));

        it('500: can calculate argumentInstance instanceConformsToType', inject(function(spWorkflowService) {
            var testId = getTestId(this);
            var ws = spWorkflowService;
            expect(ws).toBeTruthy();
        }));
    });
});
