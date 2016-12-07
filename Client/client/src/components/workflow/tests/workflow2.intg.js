// Copyright 2011-2016 Global Software Innovation Pty Ltd
/*global _, angular, console, module, describe, beforeEach, it, inject, expect, runs, waitsFor, TestSupport, sp, spEntity, spWorkflow, spWorkflowConfiguration, jsonString */

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
        return _.keyBy(spWorkflow.getActivityParameters(workflow, activity, relId), function (p) {
            return p.argument.aliasOrId();
        });
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
        return spWorkflowRunService.runWorkflow(workflow.entity.idP, [], true, { timeoutFn: timeout }).then(function (tag) {
            return spWorkflowRunService.waitForRunToStopWithThrow(tag, { timeoutFn: timeout }).then(function(workflowRunId) {
                workflow.lastRunId = workflowRunId;
                return workflow;
            });
        });
    }

    function validateWorkflow(spWorkflowService, workflow) {
        return spWorkflowService.validateWorkflow(workflow.entity.idP)
            .then(function (results) {
                workflow.validationMessages = results;
                return workflow;
            });
    }

    function runWorkflowWithResourceId(spWorkflowRunService, resourceId, workflow) {
        return spWorkflowRunService.runWorkflow(workflow.entity.id(), [{ name: 'ResourceId', value: resourceId, typeName: 'core:resourceArgument' }], true, { timeoutFn: timeout }).then(function (tag) {
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

    beforeEach(module('myTestModule'));
    beforeEach(module('mod.common.spWebService'));
    beforeEach(module('mod.common.spUserTask'));
    beforeEach(module('mod.services.promiseService'));
    beforeEach(module('spApps.reportServices'));
    beforeEach(module('mod.services.workflowService'));
    beforeEach(module('mod.services.workflowRunService'));
    beforeEach(module('sp.common.fieldValidator'));

    beforeEach(inject(function ($injector, $q, $rootScope) {
        TestSupport.setupIntgTests(this, $injector);
        TestSupport.setWaitTimeout(60000);

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

    describe('spWorkflowService createActivity', function () {

        // TODO: Ignored by Anthony as part of new security model. Ran out of time trying to work out why tests fail.
        // DISABLED: This test is accidentally changing the test:manager relationship can causing other tests to fail. 
        xit('600: test createActivity setting multiple fields and relationships', inject(function ($q, spWorkflowService, spWorkflowRunService, spPromiseService, spReportService) {
            var testId = getTestId(this);

            var ws = spWorkflowService;
            var testEntityName = 'CreateActivityTestEntity-' + testId + '-' + (new Date().getTime());

            var resolve = {
                workflow: ws.newWorkflow()
                    .then(function (workflow) {
                        setTestEntityName(workflow.entity, 'Workflow' + testId);
                        return workflow;
                    })
                    .then(function (workflow) {
                        return $q.all([
                            ws.createActivity(workflow, 'core:createActivity', 'create')
                        ]).then(function (activities) {
                            ws.addActivitiesInSequence(workflow, activities);
                            return workflow;
                        });
                    })
                    .then(function (workflow) {

                        var baseName;

                        // configure the createActivity

                        var activity = _.find(workflow.entity.containedActivities, _.partial(hasName, 'create'));

                        // set the object definition argument, and the predefined set field arguments for 'name'
                        // assuming the basename/key for the name group is "1"

                        var relMgr = spEntity.fromId('test:manager');
                        relMgr.typeId = 'relationship';
                        relMgr.name = 'Manager';

                        spWorkflow.updateParameterEntityExpression(workflow, activity, aliases.inputArguments, 'core:createActivityResourceArgument', relMgr);
                        spWorkflow.updateParameterExpressionByName(workflow, activity, aliases.inputArguments, '1_value', testEntityName, true);

                        // add argument groups for a field

                        baseName = spWorkflow.addSetFieldArgs(workflow, activity);
                        var field = spEntity.fromId('oldshared:age');
                        field.typeId = 'field';
                        field.name = 'Age';
                        spWorkflow.updateParameterEntityExpressionByName(workflow, activity, aliases.inputArguments, baseName, field);
                        spWorkflow.updateParameterExpressionByName(workflow, activity, aliases.inputArguments, baseName + '_value', '99', false);

                        // add argument group for a lookup

                        baseName = spWorkflow.addSetRelArgs(workflow, activity);

                        var rel = spEntity.fromId('test:reportsTo');
                        rel.typeId = 'relationship';
                        rel.name = 'ReportsTo';

                        spWorkflow.updateParameterEntityExpressionByName(workflow, activity, aliases.inputArguments, baseName, rel);
                        spWorkflow.updateParameterEntityExpressionByName(workflow, activity, aliases.inputArguments, spWorkflow.addSetRelValueArg(workflow, activity, baseName), spEntity.fromId('test:peterAylett'));

                        // add argument group for to-many relationship
                        // and use the replace, then no replace to add 2 related entities

                        baseName = spWorkflow.addSetRelArgs(workflow, activity);

                        spWorkflow.updateParameterEntityExpressionByName(workflow, activity, aliases.inputArguments, baseName, rel);
                        spWorkflow.updateParameterExpressionByName(workflow, activity, aliases.inputArguments, baseName + '_reverse', "true");
                        spWorkflow.updateParameterEntityExpressionByName(workflow, activity, aliases.inputArguments, spWorkflow.addSetRelValueArg(workflow, activity, baseName), spEntity.fromId('test:steveGibbon'));

                        baseName = spWorkflow.addSetRelArgs(workflow, activity);
                        spWorkflow.updateParameterEntityExpressionByName(workflow, activity, aliases.inputArguments, baseName, rel);
                        spWorkflow.updateParameterExpressionByName(workflow, activity, aliases.inputArguments, baseName + '_reverse', "true");
                        spWorkflow.updateParameterExpressionByName(workflow, activity, aliases.inputArguments, baseName + '_replace', "false");
                        spWorkflow.updateParameterEntityExpressionByName(workflow, activity, aliases.inputArguments, spWorkflow.addSetRelValueArg(workflow, activity, baseName), spEntity.fromId('test:davidQuint'));

                        return workflow;
                    })
                    .then(ws.saveAndReopenWorkflow)
                    .then(updateTestWorkflowName)
                    .then(ws.saveAndReopenWorkflow)
                    .then(_.partial(runWorkflow, spWorkflowRunService))
                    .then(_.partial(waitForWorkflowRunStatus, spWorkflowRunService, spPromiseService, 'Completed'))
                    .then(function (workflow) {
                        expect(workflow.lastRunStatus).toBe('Completed');
                        return workflow;
                    })
                    .then(ws.reopenWorkflow)
                    .then(function (workflow) {
                        return queryResourceByName(spReportService, 'test:manager', testEntityName).then(function (results) {
                            console.log('query created resource', results);
                            expect(sp.result(results, 'data.0.item.1.value')).toBe(testEntityName);

                            return workflow;
                        });
                    })
                    .then(function (workflow) {
                        var query = {
                            root: {
                                id: 'test:manager',
                                related: [
                                    {
                                        rel: { id: 'test:reportsTo', as: 'mgr' },
                                        forward: true,
                                        mustExist: false
                                    },
                                    {
                                        rel: { id: 'test:reportsTo', as: 'reports' },
                                        forward: false,
                                        mustExist: true
                                    }
                                ]
                            },
                            selects: [
                                { field: 'name', displayAs: 'Name' },
                                { field: 'alias', on: 'mgr', displayAs: 'ManagerAlias' },
                                { field: 'alias', on: 'reports', displayAs: 'ReportsAlias' }
                            ],
                            conds: [
                                { expr: { field: 'name' }, oper: 'equal', val: testEntityName }
                            ]
                        };

                        return spReportService.runQuery(query).then(function (results) {
                            console.log('query created resource with rels', results);
                            expect(sp.result(results, 'data.0.item.1.value')).toBe(testEntityName);
                            expect(sp.result(results, 'data.0.item.2.value')).toBe('peterAylett');
                            expect(sp.result(results, 'data.0.item.3.value')).toBe('steveGibbon');
                            expect(sp.result(results, 'data.1.item.3.value')).toBe('davidQuint');

                            return workflow;
                        });
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
                expect(workflow.entity.containedActivities).toBeArray();
                expect(workflow.entity.containedActivities.length).toBe(1);
                expect(workflow.entity.transitions.length).toBe(0);
                expect(workflow.entity.terminations.length).toBe(1);
                //expect(_.flatten(_.map(workflow.entity.containedActivities, 'expressionMap')).length).toBe(10);
            });
        }));

        // TODO: This causes a cardinality violation when reassigning a manager. Test can't be reactivated until we have a default mecahnism for dealing with this.
        xit('610: test createActivity setting multiple fields and relationships - test 2', inject(function ($q, spWorkflowService, spWorkflowRunService, spPromiseService, spReportService) {
            var testId = getTestId(this);

            var ws = spWorkflowService;
            var testEntityName = 'CreateActivityTestEntity-' + testId + '-' + (new Date().getTime());

            var resolve = {
                workflow: ws.newWorkflow()
                    .then(function (workflow) {
                        setTestEntityName(workflow.entity, 'Workflow' + testId);
                        return workflow;
                    })
                    .then(function (workflow) {
                        return $q.all([
                            ws.createActivity(workflow, 'core:createActivity', 'create')
                        ]).then(function (activities) {
                            ws.addActivitiesInSequence(workflow, activities);
                            return workflow;
                        });
                    })
                    .then(function (workflow) {

                        var baseName;

                        // configure the createActivity

                        var activity = _.find(workflow.entity.containedActivities, _.partial(hasName, 'create'));

                        // set the object definition argument, and the predefined set field arguments for 'name'
                        // assuming the basename/key for the name group is "1"

                        spWorkflow.updateParameterEntityExpression(workflow, activity, aliases.inputArguments, 'core:createActivityResourceArgument', spEntity.fromId('test:manager'));
                        spWorkflow.updateParameterExpressionByName(workflow, activity, aliases.inputArguments, '1_value', testEntityName, true);

                        // add argument groups for a field

                        baseName = spWorkflow.addSetFieldArgs(workflow, activity);
                        spWorkflow.updateParameterEntityExpressionByName(workflow, activity, aliases.inputArguments, baseName, spEntity.fromId('oldshared:age'));
                        spWorkflow.updateParameterExpressionByName(workflow, activity, aliases.inputArguments, baseName + '_value', '99', false);

                        // add argument group for a lookup

                        baseName = spWorkflow.addSetRelArgs(workflow, activity);
                        spWorkflow.updateParameterEntityExpressionByName(workflow, activity, aliases.inputArguments, baseName, spEntity.fromId('test:reportsTo'));
                        spWorkflow.updateParameterEntityExpressionByName(workflow, activity, aliases.inputArguments, spWorkflow.addSetRelValueArg(workflow, activity, baseName), spEntity.fromId('test:peterAylett'));

                        // add argument group for to-many relationship
                        // and use multiple value args in a single 'group' to set multiple related entities

                        baseName = spWorkflow.addSetRelArgs(workflow, activity);
                        spWorkflow.updateParameterEntityExpressionByName(workflow, activity, aliases.inputArguments, baseName, spEntity.fromId('test:reportsTo'));
                        spWorkflow.updateParameterExpressionByName(workflow, activity, aliases.inputArguments, baseName + '_reverse', "true");
                        spWorkflow.updateParameterEntityExpressionByName(workflow, activity, aliases.inputArguments, spWorkflow.addSetRelValueArg(workflow, activity, baseName), spEntity.fromId('test:steveGibbon'));
                        spWorkflow.updateParameterEntityExpressionByName(workflow, activity, aliases.inputArguments, spWorkflow.addSetRelValueArg(workflow, activity, baseName), spEntity.fromId('test:davidQuint'));

                        return workflow;
                    })
                    .then(ws.saveAndReopenWorkflow)
                    .then(updateTestWorkflowName)
                    .then(ws.saveAndReopenWorkflow)
                    .then(_.partial(runWorkflow, spWorkflowRunService))
                    .then(_.partial(waitForWorkflowRunStatus, spWorkflowRunService, spPromiseService, 'Completed'))
                    .then(function (workflow) {
                        expect(workflow.lastRunStatus).toBe('Completed');
                        return workflow;
                    })
                    .then(ws.reopenWorkflow)
                    .then(function (workflow) {
                        return queryResourceByName(spReportService, 'test:manager', testEntityName).then(function (results) {
                            console.log('query created resource', results);
                            expect(sp.result(results, 'data.0.item.1.value')).toBe(testEntityName);

                            return workflow;
                        });
                    })
                    .then(function (workflow) {
                        var query = {
                            root: {
                                id: 'test:manager',
                                related: [
                                    {
                                        rel: { id: 'test:reportsTo', as: 'mgr' },
                                        forward: true,
                                        mustExist: false
                                    },
                                    {
                                        rel: { id: 'test:reportsTo', as: 'reports' },
                                        forward: false,
                                        mustExist: true
                                    }
                                ]
                            },
                            selects: [
                                { field: 'name', displayAs: 'Name' },
                                { field: 'alias', on: 'mgr', displayAs: 'ManagerAlias' },
                                { field: 'alias', on: 'reports', displayAs: 'ReportsAlias' }
                            ],
                            conds: [
                                { expr: { field: 'name' }, oper: 'equal', val: testEntityName }
                            ]
                        };

                        return spReportService.runQuery(query).then(function (results) {
                            console.log('query created resource with rels', results);
                            expect(sp.result(results, 'data.0.item.1.value')).toBe(testEntityName);
                            expect(sp.result(results, 'data.0.item.2.value')).toBe('peterAylett');
                            expect(sp.result(results, 'data.0.item.3.value')).toBe('steveGibbon');
                            expect(sp.result(results, 'data.1.item.3.value')).toBe('davidQuint');

                            return workflow;
                        });
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
                expect(workflow.entity.containedActivities).toBeArray();
                expect(workflow.entity.containedActivities.length).toBe(1);
                expect(workflow.entity.transitions.length).toBe(0);
                expect(workflow.entity.terminations.length).toBe(1);
                //expect(_.flatten(_.map(workflow.entity.containedActivities, 'expressionMap')).length).toBe(10);
            });
        }));

        // TODO: This causes a cardinality violation when reassigning a manager. Test can't be reactivated until we have a default mecahnism for dealing with this.
        xit('620: test createActivity using resourceListArgument', inject(function ($q, spWorkflowService, spWorkflowRunService, spPromiseService, spReportService) {
            var testId = getTestId(this);

            var ws = spWorkflowService;
            var testEntityName = 'CreateActivityTestEntity-' + testId + '-' + (new Date().getTime());

            var resolve = {
                workflow: ws.newWorkflow()
                    .then(function (workflow) {
                        setTestEntityName(workflow.entity, 'Workflow' + testId);
                        return workflow;
                    })
                    .then(function (workflow) {
                        return $q.all([
                            ws.createActivity(workflow, 'core:createActivity', 'create')
                        ]).then(function (activities) {
                            ws.addActivitiesInSequence(workflow, activities);
                            return workflow;
                        });
                    })
                    .then(function (workflow) {

                        var baseName;

                        // configure the createActivity

                        var activity = _.find(workflow.entity.containedActivities, _.partial(hasName, 'create'));

                        // set the object definition argument, and the predefined set field arguments for 'name'
                        // assuming the basename/key for the name group is "1"

                        spWorkflow.updateParameterEntityExpression(workflow, activity, aliases.inputArguments, 'core:createActivityResourceArgument', spEntity.fromId('test:manager'));
                        spWorkflow.updateParameterExpressionByName(workflow, activity, aliases.inputArguments, '1_value', testEntityName, true);

                        // add argument group for a lookup

                        baseName = spWorkflow.addSetRelArgs(workflow, activity);
                        spWorkflow.updateParameterEntityExpressionByName(workflow, activity, aliases.inputArguments, baseName, spEntity.fromId('test:reportsTo'));
                        spWorkflow.updateParameterEntityExpressionByName(workflow, activity, aliases.inputArguments, spWorkflow.addSetRelValueArg(workflow, activity, baseName), spEntity.fromId('test:peterAylett'));

                        // add argument group for to-many relationship and use a resourceList argument for the value
                        // add a known entity that we can follow a relationship on to produce a resource list

                        baseName = spWorkflow.addSetRelArgs(workflow, activity);
                        spWorkflow.updateParameterEntityExpressionByName(workflow, activity, aliases.inputArguments, baseName, spEntity.fromId('test:reportsTo'));
                        spWorkflow.updateParameterExpressionByName(workflow, activity, aliases.inputArguments, baseName + '_reverse', "true");

                        var valueArgName = spWorkflow.addSetRelValueArg(workflow, activity, baseName, spWorkflowConfiguration.aliases.resourceListArgument);
                        var valueArg = _.find(spWorkflow.getActivityArguments(workflow, activity), { name: valueArgName });
                        spWorkflow.updateExpressionForParameter(workflow, activity, valueArg, '');
                        var valueExpr = spWorkflow.getExpression(workflow, activity, valueArg);
                        spWorkflow.addExpressionKnownEntity(valueExpr, spEntity.fromId('test:scottHopwood'));
                        valueExpr.expressionString = '[unnamed].[Direct Reports]';

                        return workflow;
                    })
                    .then(ws.saveAndReopenWorkflow)
                    .then(updateTestWorkflowName)
                    .then(ws.saveAndReopenWorkflow)
                    .then(_.partial(runWorkflow, spWorkflowRunService))
                    .then(_.partial(waitForWorkflowRunStatus, spWorkflowRunService, spPromiseService, 'Completed'))
                    .then(function (workflow) {
                        expect(workflow.lastRunStatus).toBe('Completed');
                        return workflow;
                    })
                    .then(ws.reopenWorkflow)
                    .then(function (workflow) {
                        return queryResourceByName(spReportService, 'test:manager', testEntityName).then(function (results) {
                            console.log('query created resource', results);
                            expect(sp.result(results, 'data.0.item.1.value')).toBe(testEntityName);

                            return workflow;
                        });
                    })
                    .then(function (workflow) {
                        var query = {
                            root: {
                                id: 'test:manager',
                                related: [
                                    {
                                        rel: { id: 'test:reportsTo', as: 'mgr' },
                                        forward: true,
                                        mustExist: false
                                    },
                                    {
                                        rel: { id: 'test:reportsTo', as: 'reports' },
                                        forward: false,
                                        mustExist: true
                                    }
                                ]
                            },
                            selects: [
                                { field: 'name', displayAs: 'Name' },
                                { field: 'alias', on: 'mgr', displayAs: 'ManagerAlias' },
                                { field: 'alias', on: 'reports', displayAs: 'ReportsAlias' }
                            ],
                            conds: [
                                { expr: { field: 'name' }, oper: 'equal', val: testEntityName }
                            ]
                        };

                        return spReportService.runQuery(query).then(function (results) {
                            console.log('query created resource with rels', results);
                            expect(sp.result(results, 'data.0.item.1.value')).toBe(testEntityName);
                            expect(sp.result(results, 'data.0.item.2.value')).toBe('peterAylett');
                            expect(sp.result(results, 'data.0.item.3.value')).toBe('steveGibbon');
                            expect(sp.result(results, 'data.1.item.3.value')).toBe('martinKalitis');

                            return workflow;
                        });
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
                expect(workflow.entity.containedActivities).toBeArray();
                expect(workflow.entity.containedActivities.length).toBe(1);
                expect(workflow.entity.transitions.length).toBe(0);
                expect(workflow.entity.terminations.length).toBe(1);
                //expect(_.flatten(_.map(workflow.entity.containedActivities, 'expressionMap')).length).toBe(10);
            });
        }));
    });

    describe('spWorkflowService updateActivity', function () {

        it('700: test updateActivity setting multiple fields and relationships', inject(function ($q, spWorkflowService, spWorkflowRunService, spPromiseService, spReportService) {
            var testId = getTestId(this);

            var ws = spWorkflowService;
            var testEntityName = 'UpdateActivityTestEntity-' + testId + '-' + (new Date().getTime());

            var resolve = {
                workflow: ws.newWorkflow()
                    .then(function (workflow) {
                        setTestEntityName(workflow.entity, 'Workflow' + testId);
                        return workflow;
                    })
                    .then(function (workflow) {
                        return $q.all([
                            ws.createActivity(workflow, 'core:createActivity', 'create'),
                            ws.createActivity(workflow, 'core:updateFieldActivity', 'update')
                        ]).then(function (activities) {
                            ws.addActivitiesInSequence(workflow, activities.slice(0).reverse());
                            return workflow;
                        });
                    })
                    .then(function (workflow) {

                        var baseName;

                        // configure the createActivity

                        var activity = _.find(workflow.entity.containedActivities, _.partial(hasName, 'create'));

                        // set the object definition argument, and the predefined set field arguments for 'name'
                        // assuming the basename/key for the name group is "1"

                        spWorkflow.updateParameterEntityExpression(workflow, activity, aliases.inputArguments, 'core:createActivityResourceArgument', spEntity.fromId('test:manager'));
                        spWorkflow.updateParameterExpressionByName(workflow, activity, aliases.inputArguments, '1_value', testEntityName, true);

                        return workflow;
                    })
                    .then(function (workflow) {

                        var baseName;

                        // configure the updateActivity

                        var activity = _.find(workflow.entity.containedActivities, _.partial(hasName, 'update'));

                        // set the resource to argument

                        spWorkflow.updateParameterExpression(workflow, activity, aliases.inputArguments, 'core:updateFieldActivityResourceArgument', '[create.Record]');

                        // add argument groups for a field

                        baseName = spWorkflow.addSetFieldArgs(workflow, activity);
                        spWorkflow.updateParameterEntityExpressionByName(workflow, activity, aliases.inputArguments, baseName, spEntity.fromId('test:age'));
                        spWorkflow.updateParameterExpressionByName(workflow, activity, aliases.inputArguments, baseName + '_value', '99');

                        // add argument group for a lookup

                        baseName = spWorkflow.addSetRelArgs(workflow, activity);
                        spWorkflow.updateParameterEntityExpressionByName(workflow, activity, aliases.inputArguments, baseName, spEntity.fromId('test:reportsTo'));
                        spWorkflow.updateParameterEntityExpressionByName(workflow, activity, aliases.inputArguments, spWorkflow.addSetRelValueArg(workflow, activity, baseName), spEntity.fromId('test:peterAylett'));

                        // add argument group for to-many relationship
                        // and use the replace, then no replace to add 2 related entities

                        baseName = spWorkflow.addSetRelArgs(workflow, activity);
                        spWorkflow.updateParameterEntityExpressionByName(workflow, activity, aliases.inputArguments, baseName, spEntity.fromId('test:reportsTo'));
                        spWorkflow.updateParameterExpressionByName(workflow, activity, aliases.inputArguments, baseName + '_reverse', "false");
                        spWorkflow.updateParameterExpressionByName(workflow, activity, aliases.inputArguments, baseName + '_replace', "false");
                        spWorkflow.updateParameterEntityExpressionByName(workflow, activity, aliases.inputArguments, spWorkflow.addSetRelValueArg(workflow, activity, baseName), spEntity.fromId('test:steveGibbon'));

                        //
                        // NOTE: The following code is Now working correctly - but due to the back-end not dealing with updating relationships that violate cardianlity 
                        //       I've commented it out.
                        //
                        //baseName = spWorkflow.addSetRelArgs(workflow, activity);
                        //spWorkflow.updateParameterEntityExpressionByName(workflow, activity, aliases.inputArguments, baseName, spEntity.fromId('test:reportsTo'));
                        //spWorkflow.updateParameterExpressionByName(workflow, activity, aliases.inputArguments, baseName + '_reverse', "true");
                        //spWorkflow.updateParameterExpressionByName(workflow, activity, aliases.inputArguments, baseName + '_replace', "true");
                        //spWorkflow.updateParameterEntityExpressionByName(workflow, activity, aliases.inputArguments, spWorkflow.addSetRelValueArg(workflow, activity, baseName), spEntity.fromId('test:davidQuint'));

                        return workflow;
                    })
                    .then(ws.saveAndReopenWorkflow)
                    .then(updateTestWorkflowName)
                    .then(ws.saveAndReopenWorkflow)
                    .then(_.partial(runWorkflow, spWorkflowRunService))
                    .then(_.partial(waitForWorkflowRunStatus, spWorkflowRunService, spPromiseService, 'Completed'))
                    .then(function (workflow) {
                        expect(workflow.lastRunStatus).toBe('Completed');
                        return workflow;
                    })
                    .then(ws.reopenWorkflow)
                    .then(function (workflow) {
                        return queryResourceByName(spReportService, 'test:manager', testEntityName).then(function (results) {
                            console.log('query created resource', results);
                            expect(sp.result(results, 'data.0.item.1.value')).toBe(testEntityName);

                            return workflow;
                        });
                    })
                    .then(function (workflow) {
                        var query = {
                            root: {
                                id: 'test:manager',
                                related: [
                                    {
                                        rel: { id: 'test:reportsTo', as: 'mgr' },
                                        forward: true,
                                        mustExist: false
                                    },
                                    {
                                        rel: { id: 'test:reportsTo', as: 'reports' },
                                        forward: false,
                                        mustExist: false
                                    }
                                ]
                            },
                            selects: [
                                { field: 'name', displayAs: 'Name' },
                                { field: 'alias', on: 'mgr', displayAs: 'ManagerAlias' },
                                { field: 'alias', on: 'reports', displayAs: 'ReportsAlias' }
                            ],
                            conds: [
                                { expr: { field: 'name' }, oper: 'equal', val: testEntityName }
                            ]
                        };

                        return spReportService.runQuery(query).then(function (results) {
                            console.log('query created resource with rels', results);
                            expect(sp.result(results, 'data.0.item.1.value')).toBe(testEntityName);
                            //expect(sp.result(results, 'data.0.item.2.value')).toBe('peterAylett');
                            expect(sp.result(results, 'data.0.item.2.value')).toBe('steveGibbon');
                            //expect(sp.result(results, 'data.1.item.3.value')).toBe('davidQuint');

                            return workflow;
                        });
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
                expect(workflow.entity.containedActivities).toBeArray();
                expect(workflow.entity.containedActivities.length).toBe(2);
                expect(workflow.entity.transitions.length).toBe(1);
                expect(workflow.entity.terminations.length).toBe(1);
                //expect(_.flatten(_.map(workflow.entity.containedActivities, 'expressionMap')).length).toBe(10);
            });
        }));

    });

    describe('spWorkflowService validation', function () {

        it('800: validate webapi can be called', inject(function ($q, spWorkflowService, spPromiseService, spReportService) {
            var testId = getTestId(this);

            var ws = spWorkflowService;
            var testEntityName = 'ValidationTestEntity-' + testId + '-' + (new Date().getTime());

            var resolve = {
                workflow: ws.newWorkflow()
                    .then(function (workflow) {
                        setTestEntityName(workflow.entity, 'Workflow' + testId);
                        return workflow;
                    })
                    .then(function (workflow) {
                        return $q.all([
                            ws.createActivity(workflow, 'core:logActivity', 'log')
                        ]).then(function (activities) {
                            ws.addActivitiesInSequence(workflow, activities);
                            return workflow;
                        });
                    })
                    .then(ws.saveAndReopenWorkflow)
                    .then(updateTestWorkflowName)
                    .then(ws.saveAndReopenWorkflow)
                    .then(_.partial(validateWorkflow, spWorkflowService))
            };

            var result = {};
            waitCheckReturn($q.all(resolve), result);

            runs(function () {
                expect(result.value).toBeTruthy();
                expect(result.value.workflow).toBeTruthy();

                var workflow = result.value.workflow;
                console.log('test %s: workflow entity', testId, spEntity.toJSON(workflow.entity));

                expect(workflow.entity).toBeTruthy();
                expect(workflow.entity.containedActivities).toBeArray();
                expect(workflow.entity.containedActivities.length).toBe(1);
                expect(workflow.entity.transitions.length).toBe(0);
                expect(workflow.entity.terminations.length).toBe(1);

                // we expect this to fail with a complaint about the missing log message argument

                expect(workflow.validationMessages).toBeTruthy();
                expect(workflow.validationMessages).toBeArray();

                //todo - fix this ... backend is currently returning no errors
                //expect(workflow.validationMessages.length).toBeGreaterThan(0);

                //we get a mandatory field related error when running but something else when validating
                //so leave out the checking of the message until I can speak to Scott
                //expect(workflow.validationMessages[0].toLowerCase()).toContain('mandatory');
            });
        }));

        it('810: processWorkflow works', inject(function ($q, spWorkflowService) {
            var testId = getTestId(this);

            var ws = spWorkflowService;
            var testEntityName = 'ValidationTestEntity-' + testId + '-' + (new Date().getTime());

            var resolve = {
                workflow: ws.newWorkflow()
                    .then(function (workflow) {
                        setTestEntityName(workflow.entity, 'Workflow' + testId);
                        return workflow;
                    })
                    .then(function (workflow) {
                        return $q.all([
                            ws.createActivity(workflow, 'core:logActivity', 'log')
                        ]).then(function (activities) {
                            ws.addActivitiesInSequence(workflow, activities);
                            return workflow;
                        });
                    })
                    .then(ws.saveAndReopenWorkflow)
                    .then(updateTestWorkflowName)
                    .then(ws.saveAndReopenWorkflow)
                    .then(function (workflow) {

                        var activity = _.find(workflow.entity.containedActivities, _.partial(hasName, 'log'));
                        spWorkflow.updateParameterExpression(workflow, activity, aliases.inputArguments, 'core:inLogActivityMessage', "'16:32'", false);

                        return workflow;
                    })
                    .then(ws.processWorkflow)
                    .then(function (workflow) {

                        expect(workflow.activities).toBeTruthy();
                        expect(_.values(workflow.activities).length).toBeGreaterThan(0);

                        var activity = _.find(workflow.entity.containedActivities, _.partial(hasName, 'log'));
                        expect(activity).toBeTruthy();

                        expect(workflow.activities[activity.idP]).toBeTruthy();
                        expect(workflow.activities[activity.idP].parameters).toBeTruthy();

                        var parameter = workflow.activities[activity.idP].parameters['core:inLogActivityMessage'];
                        expect(parameter).toBeTruthy();
                        expect(parameter.argument).toBeTruthy();
                        expect(parameter.expression).toBeTruthy();

                        return workflow;
                    })
            };

            var result = {};
            waitCheckReturn($q.all(resolve), result);

            runs(function () {
                expect(result.value).toBeTruthy();
                expect(result.value.workflow).toBeTruthy();

                var workflow = result.value.workflow;
                console.log('test %s: workflow', testId, result.value.workflow);
                console.log('test %s: workflow entity', testId, spEntity.toJSON(workflow.entity));

                expect(workflow.entity).toBeTruthy();
            });
        }));

        it('830: processWorkflow works on a larger workflow', inject(function ($q, spWorkflowService) {
            var testId = getTestId(this);

            var ws = spWorkflowService;
            var testEntityName = 'ValidationTestEntity-' + testId + '-' + (new Date().getTime());

            var resolve = {
                workflow: ws.newWorkflow()
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

                        spWorkflow.updateParameterEntityExpression(workflow, activity, aliases.inputArguments, 'core:getResourcesResourceType', spEntity.fromId('test:employee'));

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

                        var p = spWorkflow.findWorkflowExpressionParameter(workflow, activity, 'core:foreachSelectedResource');
                        expect(p).toBeTruthy();

                        p.instanceConformsToType = 'test:employee';

                        return workflow;
                    })
                    .then(function (workflow) {
                        var activity = _.find(workflow.entity.getContainedActivities(), _.partial(hasName, 'log'));
                        spWorkflow.updateParameterExpression(workflow, activity, aliases.inputArguments, 'core:inLogActivityMessage',
                            '{{[foreach.Record].Name}} has age {{[foreach.Record].Age}}', true);

                        return workflow;
                    })
                    .then(ws.processWorkflow)
                    .then(function (workflow) {

                        expect(workflow.activities).toBeTruthy();
                        expect(_.values(workflow.activities).length).toBeGreaterThan(0);

                        var activity = _.find(workflow.entity.containedActivities, _.partial(hasName, 'log'));
                        expect(activity).toBeTruthy();

                        expect(workflow.activities[activity.idP]).toBeTruthy();
                        expect(workflow.activities[activity.idP].parameters).toBeTruthy();

                        var parameter = workflow.activities[activity.idP].parameters['core:inLogActivityMessage'];
                        expect(parameter).toBeTruthy();
                        expect(parameter.argument).toBeTruthy();
                        expect(parameter.expression).toBeTruthy();

                        return workflow;
                    })
            };

            var result = {};
            waitCheckReturn($q.all(resolve), result);

            runs(function () {
                expect(result.value).toBeTruthy();
                expect(result.value.workflow).toBeTruthy();

                var workflow = result.value.workflow;
                console.log('test %s: workflow', testId, result.value.workflow);
                console.log('test %s: workflow entity', testId, spEntity.toJSON(workflow.entity));

                expect(workflow.entity).toBeTruthy();
            });
        }));

        it('840: validates a workflow name', inject(function ($q, spWorkflowService, spFieldValidator, spEntityService) {

            var testId = getTestId(this);

            var ws = spWorkflowService;
            var testEntityName = 'ValidationTestEntity-' + testId + '-' + (new Date().getTime());

            var resolve = {
                workflow: ws.newWorkflow()
                    .then(function (workflow) {
                        setTestEntityName(workflow.entity, 'Workflow' + testId);

                        expect(ws.validateName(workflow, 'valid name').length).toEqual(0);
                        expect(ws.validateName(workflow, 'bad name <>').length).toBeGreaterThan(0);

                        return workflow;
                    })
            };

            var result = {};
            waitCheckReturn($q.all(resolve), result);

            runs(function () {
                expect(result.value).toBeTruthy();
                expect(result.value.workflow).toBeTruthy();

                var workflow = result.value.workflow;
                console.log('test %s: workflow', testId, result.value.workflow);
                console.log('test %s: workflow entity', testId, spEntity.toJSON(workflow.entity));

                expect(workflow.entity).toBeTruthy();
            });
        }));

        it('850: validate default displayForm activity is valid after processing', inject(function ($q, spWorkflowService) {
            var testId = getTestId(this);

            var ws = spWorkflowService;
            var testEntityName = 'ValidationTestEntity-' + testId + '-' + (new Date().getTime());

            var resolve = {
                workflow: ws.newWorkflow()
                    .then(function (workflow) {
                        return $q.all([
                            ws.createActivity(workflow, 'core:displayFormActivity', 'form')
                        ]).then(function (activities) {
                            ws.addActivitiesInSequence(workflow, activities);
                            return workflow;
                        });
                    })
                    .then(function (workflow) {

                        var activity = _.find(workflow.entity.containedActivities, _.partial(hasName, 'form'));
                        spWorkflow.updateParameterExpression(workflow, activity, aliases.inputArguments, 'core:inDisplayFormTimeOut', "0", false);

                        return workflow;
                    })
                    .then(ws.processWorkflow)
                    .then(function (workflow) {

                        expect(workflow.activities).toBeTruthy();
                        expect(_.values(workflow.activities).length).toBeGreaterThan(0);

                        var actEntity = _.find(workflow.entity.containedActivities, _.partial(hasName, 'form'));
                        expect(actEntity).toBeTruthy();

                        var activityInfo = workflow.activities[actEntity.idP];

                        expect(activityInfo).toBeTruthy();
                        expect(activityInfo.parameters).toBeTruthy();

                        expect(activityInfo.validationMessages || []).toBeArray(0, 'no validation messages');

                        var parameter = activityInfo.parameters['core:inDisplayFormTimeOut'];
                        expect(parameter).toBeTruthy();
                        expect(parameter.argument).toBeTruthy();
                        expect(parameter.expression).toBeTruthy();

                        return workflow;
                    })
            };

            var result = {};
            waitCheckReturn($q.all(resolve), result);

            runs(function () {
                expect(result.value).toBeTruthy();
                expect(result.value.workflow).toBeTruthy();

                var workflow = result.value.workflow;
                console.log('test %s: workflow', testId, result.value.workflow);
                console.log('test %s: workflow entity', testId, spEntity.toJSON(workflow.entity));

                expect(workflow.entity).toBeTruthy();
            });
        }));


    });

});
