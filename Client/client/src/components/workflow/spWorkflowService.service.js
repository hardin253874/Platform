// Copyright 2011-2016 Global Software Innovation Pty Ltd
/*global _, angular, console, sp, spEntity, jsonLookup, jsonString, jsonBool, jsonInt, spWorkflowConfiguration, spWorkflow */

/**
 * Client side service to handle everything workflow. Layers on top of the Entity modules and services to
 * allow managing workflows and their underlying entity model.
 */

(function () {
    'use strict';

    angular.module('mod.services.workflowService')
        .factory('spWorkflowService', spWorkflowService);

    /* @ngInject */
    function spWorkflowService($timeout, $http, $q, spEntityService, activityTypesRequest, spWebService,
                               workflowEntityRequest, activityTypeConfig, spReportService, spPromiseService,
                               spCalcEngineService, spFieldValidator, spAppSettings,
                               spWorkflowActivityService, spWorkflowCacheService, rnFeatureSwitch, spEntitySaveAsDialog) {

        var aliases = spWorkflowConfiguration.aliases;

        //
        // Constants (These should probably be obtained from the server)
        //
        var WaitForWorkflowStop_Retries = 240;
        var WaitForWorkflowStop_Pause = 500;        // Pause between polls

        // aliases for commonly used utility functions
        var first = _.first, filter = _.filter, forEach = _.forEach, partial = _.partial;
        var assert = console.assert.bind(console);

        var argTypeToParamTypeMap = {
            'core:resourceArgument': 'Entity',
            'core:resourceListArgument': 'Entity',
            'core:stringArgument': 'String',
            'core:integerArgument': 'Int32',
            'core:decimalArgument': 'Decimal',
            'core:currencyArgument': 'Currency',
            'core:dateTimeArgument': 'DateTime',
            'core:timeArgument': 'DateTime',
            'core:dateArgument': 'DateTime',
            'core:boolArgument': 'Bool',
            'core:guidArgument': 'Guid'
        };

        // Cache helpers
        var getTemplateReport = _.partial(spWorkflowCacheService.getCacheableEntity, 'templateReport', 'core:templateReport', 'name');
        var getCacheableType = function(type) {
            if (!type) {
                return $q.when();
            }
            return spWorkflowCacheService.getCacheableEntity('type:' + type, type, 'name, description, isOfType.name, defaultPickerReport.name');
        };
        
        ///////////////////////////////////////////////////////////////////////
        // The interface
        //

        var exports = {
            getActivityTypeConfig: getActivityTypeConfig,
            getActivityTypes: getActivityTypes,
            getArgumentTypes: getArgumentTypes,
            getWorkflows: getWorkflows,
            newWorkflow: newWorkflow,
            openWorkflow: openWorkflow,
            reopenWorkflow: _.flowRight(openWorkflow, _.partialRight(sp.result, 'entity.id')),
            validateName: validateName,
            workflowUpdated: workflowUpdated,
            saveWorkflow: saveWorkflow,
            saveAsWorkflow: saveAsWorkflow,
            saveAndReopenWorkflow: saveAndReopenWorkflow,
            validateWorkflow: validateWorkflow,
            processWorkflow: processWorkflow,
            createActivity: createActivity,
            activityUpdated: activityUpdated,
            addActivities: addActivities,
            removeActivity: removeActivity,
            addSwimlane: addSwimlane,
            removeSwimlane: removeSwimlane,
            addEndEvent: addEndEvent,
            removeEndEvent: removeEndEvent,
            addActivitiesInSequence: addActivitiesInSequence,
            addSequence: addSequence,
            removeSequence: removeSequence,
            canAddSequence: canAddSequence,
            updateSequence: updateSequence,
            deleteSelected: deleteSelected,
            getWorkflowMenu: getWorkflowMenu,
            getExpressionCompileResult: getExpressionCompileResult,
            getExpressionEvalResult: getExpressionEvalResult,
            applyUpdate: applyUpdate,
            getPausedRuns: getPausedRuns,

            getTemplateReport: getTemplateReport,
            getCacheableType: getCacheableType,
            getCacheableEntity: spWorkflowCacheService.getCacheableEntity,
            resetCache: spWorkflowCacheService.resetCache
        };

        return exports;
        
        ///////////////////////////////////////////////////////////////////////
        // The implementation
        //


        function getValidateWorkflowUrl(id) {
            return spWebService.getWebApiRoot() + '/spapi/data/v1/workflow/validate/' + (id || '');
        }

        function getWorkflowEntityRequest() {
            // Concat with comma separators any type specific requests and sub in the main request string.
            // Note to lead with a comma if we have any.
            var typesRequest = _(activityTypeConfig).map('request').compact().join(',');
            return workflowEntityRequest.replace('{{activityTypeRequest}}', typesRequest.length > 0 ? ',' + typesRequest : '');
        }

        /**
         * Get the configuration for the given activity type alias or id
         * @param typeAliasOrId
         */
        function getActivityTypeConfig(typeAlias) {
            var config = {};

            if (activityTypeConfig) {
                _.extend(config, activityTypeConfig['default'] || {}, activityTypeConfig[typeAlias] || {});
            }

            var typeBaseName = typeAlias.replace(/core:/, '').replace(':', '_');
            if (!config.image) {
                config.image = typeBaseName + '.svg';
            }
            if (!config.menuImage) {
                config.menuImage = typeBaseName + '.png';
            }

            return config;
        }

        /**
         * Get a promise for an array of Activity Types that includes all meta-data
         */
        function getActivityTypes(refresh) {
            return spWorkflowCacheService.getCacheableEntity('activityTypes', aliases.activityType, 'instancesOfType.{' + activityTypesRequest + '}', refresh)
                .then(function (value) {
                    return value.getInstancesOfType();
                });
        }

        /**
         * Get a promise for an map of field entities (map of name to entity) that we want to make use of for
         * things like validation.
         *
         * Right now it's only the name field.
         */
        function getFieldEntities(refresh) {

            return spWorkflowCacheService.getCacheableEntity('fieldEntities', 'core:name', 'name, ' + spFieldValidator.getFieldQueryFragment(), refresh)
                .then(function (value) {
                    return { 'core:name': value };
                });
        }

        /**
         * Get a promise for an array of the available activity argument types
         */
        function getArgumentTypes(refresh) {
            return spWorkflowCacheService.getCacheableEntity('parameterTypes', aliases.argumentType, 'instancesOfType.{alias,name,description}', refresh)
                .then(function (value) {
                    return value.getInstancesOfType();
                });
        }

        /**
         * Get a promise for an array of Workflows that includes all Activity Type meta-data
         */
        function getWorkflows(refresh) {
            return spWorkflowCacheService.getCacheableEntity('workflows', aliases.workflow, 'instancesOfType.{' + activityTypesRequest + '}', refresh)
                .then(function (value) {
                    return value.getInstancesOfType();
                });
        }

        /**
         * Invalidate the cached type info for the given workflow.
         */
        function invalidateCachedWorkflow(id) {
            spWorkflowCacheService.invalidateCache('workflow:' + id);
        }

        function createDefaultExitPoint(refresh) {
            var exitPoint = spEntity.fromJSON({
                typeId: aliases.exitPoint,
                name: jsonString("Exit"),
                description: jsonString(""),
                isDefaultExitPoint: true
            });
            return exitPoint;
        }

        function getTimeoutExitPoint(refresh) {
            var request = 'alias,name,description,isDefaultExitPoint,exitPointOrdinal';
            return spWorkflowCacheService.getCacheableEntity('timeoutExitPoint', aliases.timeoutExitPoint, request, refresh);
        }

        function getOptionEnumValues(enumType, refresh) {
            var request = 'instancesOfType.{alias,name,description,enumOrder}';
            return spWorkflowCacheService.getCacheableEntity(enumType, enumType, request, refresh)
                .then(function (value) {
                    return value.getInstancesOfType();
                });
        }

        /**
         * Create a new in-memory workflow entity model with default configuration for a 'new workflow'.
         * Returns a promise for a workflow model object with properties including:
         *      { entity, selected, updateCount }
         * where the entity is the root of the workflow entity graph.
         * @returns {promise} promise for workflow model object
         */
        function newWorkflow() {
            console.log('spWorkflowService: new workflow');

            return $q.all({
                activityTypes: getActivityTypes(),
                entity: spEntity.fromJSON({
                    typeId: aliases.workflow,
                    name: jsonString('New Workflow'),
                    description: jsonString(''),
                    exitPoints: [
                        {
                            typeId: aliases.exitPoint,
                            name: 'end',
                            isDefaultExitPoint: true,
                            exitPointOrdinal: 1
                        }
                    ],
                    firstActivity: jsonLookup(null),
                    containedActivities: [],
                    transitions: [],
                    terminations: [],
                    swimlanes: [],
                    inputArguments: [],
                    outputArguments: [],
                    variables: [],
                    expressionMap: [],
                    expressionParameters: [],
                    inputArgumentForAction: jsonLookup(null),
                    inputArgumentForRelatedResource: jsonLookup(null),
                    workflowHasErrors: false,
                    workflowVersion: 1
                }),
                fieldEntities: getFieldEntities()
            })
                .then(spWorkflow.initialiseHistory)
                .then(processWorkflow);
        }

        /**
         * Loads the workflow with the given id and returns a promise for a workflow model object with the
         * loaded entity.
         * @param {number|string} id or alias for of the workflow instance to load
         * @returns {promise} promise for workflow model object, not the same as the argument
         */
        function openWorkflow(id) {
            console.log('spWorkflowService: opening workflow ' + id);

            var logTimeKey = 'open workflow';
            console.time(logTimeKey);

            return $q.all({
                activityTypes: getActivityTypes(),
                entity: spEntityService.getEntity(spWorkflow.makeIdOrAlias(id), getWorkflowEntityRequest(), { hint: 'wf', batch: false }),
                fieldEntities: getFieldEntities()
            })
                .then(function (workflow) {
                    //                        console.log('spWorkflowService: opened workflow %s => %o', id, workflow.entity);
                    console.log('spWorkflowService: opened workflow ' + id);
                    console.timeEnd(logTimeKey);
                    return workflow;
                })
                .then(function (workflow) {
                    //TODO - move this to 'processWorkflow'
                    // clean up any expressions missing a target argument... can happen if we have removed some
                    // arguments (like readfield res type arg)
                    spWorkflow.cleanExpressions(workflow.entity);
                    forEach(workflow.entity.containedActivities,
                        spWorkflow.cleanExpressions);

                    return workflow;
                })
                .then(spWorkflow.initialiseHistory)
                .then(processWorkflow)
                .catch(function (error) {
                    console.error('spWorkflowService: error opening workflow %s => %o', id, error);
                    console.timeEnd(logTimeKey);
                    throw error;
                });
        }

        function validateName(workflow, name) {
            var validator = spFieldValidator.getValidator(workflow.fieldEntities['core:name']);
            return validator(name);
        }

        /**
         * Call workflowUpdated if you have updated the underlying workflow model outside of this service.
         * For example when setting the workflow name.
         */
        function workflowUpdated(workflow) {
            spWorkflow.setBookmark(workflow);
            if (workflow.selectedEntity && !spWorkflow.findWorkflowComponentEntity(workflow, workflow.selectedEntity.id())) {
                workflow.selectedEntity = null;
            }
        }

        /**
         * Save the workflow entity referenced in the given workflow model object.
         * @param {object} workflow model object
         * @returns {promise} promise for workflow model object (with id updated if it was a new workflow)
         *                    and updateCount reset to 0
         */
        function saveWorkflow(workflow) {
            var savePromise;

            if (!workflow || !workflow.entity) {
                throw new Error('Cannot save undefined workflow');
            }

            //todo - remove this once the process to always keep these up to date is in place
            spWorkflow.updateExpressionParameters(workflow);
            spWorkflow.removeUnusedExpressionKnownEntities(workflow);

            var logTimeKey = 'saved workflow (' + workflow.entity.getContainedActivities().length + ' activities)';
            console.time(logTimeKey);
            var endLogTime = _.partial(console.timeEnd, logTimeKey);

            var areCreating = workflow.entity.dataState === spEntity.DataStateEnum.Create;

            if (areCreating) {
                savePromise = spEntityService.putEntity(workflow.entity)
                    .then(function (id) {
                        console.log('spWorkflowService: saved workflow %o => %o', workflow.entity.id(), id);
                        finalizeWorkflow(workflow);
                        resetWorkflowId(workflow, id);
                        return workflow;
                    });
            } else {
                savePromise = workflowUpdateService(workflow.entity)
                    .then(function (hasCloned) {
                        console.log('spWorkflowService: has workflow been cloned: ', hasCloned);
                        finalizeWorkflow(workflow);
                        return workflow;
                    });
            }

            return savePromise
                .catch(function(error) {
                    console.error('spWorkflowService: error saving workflow %o => %o', workflow, error);
                    throw error;
                })
                .finally(endLogTime);
        }

        function saveAsWorkflow(workflow) {
            if (!workflow || !workflow.entity) {
                throw new Error('Cannot save undefined workflow');
            }

            // Set a bookmark so we can update the version and revert after
            let entity = workflow.entity;
            let history = entity.graph.history;
            let preCloneBookmark = history.addBookmark('PreCloneEntity');

            entity.workflowVersion = 1;

            //todo - remove this once the process to always keep these up to date is in place
            spWorkflow.updateExpressionParameters(workflow);
            spWorkflow.removeUnusedExpressionKnownEntities(workflow);

            var logTimeKey = 'saveas workflow (' + workflow.entity.getContainedActivities().length + ' activities)';
            console.time(logTimeKey);            

            var options = {
                entity: workflow.entity,
                typeName: 'Workflow'
            };

            return spEntitySaveAsDialog.showModalDialog(options).finally( () => history.undoBookmark(preCloneBookmark ));
            
        }

        function workflowUpdateService(workflow) {
            var serviceUrl = spWebService.getWebApiRoot() + '/spapi/data/v1/workflow/update/' + workflow.idP ;
            var entityData = spEntityService.packageEntityNugget(workflow);

            return $http({
                method: 'POST',
                url: serviceUrl,
                data: entityData,
                params: {},
                headers: spWebService.getHeaders()
            }).then(function (response) {
                return response.data;
            });
        }

        function saveAndReopenWorkflow(workflow) {
            return saveWorkflow(workflow).then(_.flowRight(openWorkflow, _.partialRight(sp.result, 'entity.idP')));
        }

        function finalizeWorkflow(workflow) {
            spWorkflow.resetHistory(workflow);
            invalidateCachedWorkflow(workflow.idP);
        }

        function resetWorkflowId(workflow, id) {
            workflow.entity.setId(id);
        }

        /**
         * Validate the workflow with the given id.
         * @returns {promise} promise for a list of validation issues.
         */
        function validateWorkflow(id) {
            console.log('spWorkflowService: validateWorkflow', id);

            return $http({
                method: 'GET',
                url: getValidateWorkflowUrl(id),
                data: {},
                headers: spWebService.getHeaders()
            })
                .then(function (response) {
                    var data = response.data;
                    console.log('spWorkflowService: validate returned data %o', data);
                    return data;
                });
        }

        /*
         * Get the workflow runs that are paused for a given workflow
         */
        function getPausedRuns(workflowId) {
            return spEntityService.getEntitiesOfType('core:workflowRun', "id", { filter: " id(Workflow) = " + workflowId + " and [Status] = 'Paused' ", hint: 'getPausedRuns' });
        }

        function addValidationMessage(workflow, message, entity) {
            workflow.validationMessages = (workflow.validationMessages || []).concat({ message: message, entity: entity });
        }

        function makeExprParamHintFromParameter(argInst) {
            var argEntity = argInst.argumentInstanceArgument;
            var typeAlias = sp.result(argEntity, 'type.nsAlias');
            //                if (!typeAlias) debugger;
            var hint = {
                name: argInst.name,
                description: argInst.description,
                typeName: typeAlias && argTypeToParamTypeMap[typeAlias] || typeAlias || '???'
            };
            if (hint.typeName === 'Entity') {
                hint.entityTypeId = sp.result(argInst, 'instanceConformsToType.idP') ||
                    sp.result(argEntity, 'conformsToType.idP') ||
                    'core:resource';
                hint.isList = argInst.argumentInstanceArgument.type.nsAlias === 'core:resourceListArgument';
            }
            return hint;
        }

        function getKnownEntityParamHints(expression) {
            var knownEntities = expression.wfExpressionKnownEntities;
            return _.map(knownEntities, function (namedReference) {
                return {
                    name: namedReference.name,
                    description: namedReference.description,
                    typeName: 'Entity',
                    entityTypeId: sp.result(namedReference.referencedEntity, 'type.idP') || 'core:resource'
                };
            });
        }

        function compileExpressions(parameters) {
            var first = _.first(parameters);
            if (!first) {
                return $q.when([]);
            }
            var expr = sp.result(first, 'expression.expressionString');
            if (expr && first.expression.isTemplateString) {
                expr = expr.replace(/{{(.*?)}}/gm, "' + join($1) + '");
                expr = "'" + expr + "'";
            }
            //console.log('compile', first.argument.nsAliasOrId, expr);
            return $q.when(expr && spCalcEngineService.compile(expr, { context: null, params: first.exprParamHints, host: 'Evaluate' }))
                .then(function (result) {

                    if (!expr) {
                        // not an error, just to be ignored
                        result = { expression: '', resultType: 'None' };
                    }

                    //console.log('compile complete %o => ', expr, result.resultType, result.entityTypeId, result.error);

                    first.compileResult = result;

                    return compileExpressions(_.tail(parameters)).then(function (restResults) {
                        return [result].concat(restResults);
                    });
                });
        }

        function getActivitySequences(workflow, activity, path, activityPaths) {
            activity = activity || workflow.entity.firstActivity;
            activityPaths = activityPaths || {};
            path = path || [];

            if (activity) {
                activityPaths[activity.idP] = activityPaths[activity.idP] || [];
                if (path.indexOf(activity) < 0) {
                    activityPaths[activity.idP].push(path);
                    _.forEach(workflow.entity.transitions, function (t) {
                        if (t.fromActivity === activity) {
                            getActivitySequences(workflow, t.toActivity, path.concat([activity]), activityPaths);
                        }
                    });
                }
            }
            return activityPaths;
        }

        function validateEntityName(workflow, entity) {
            var nameValidationMessages = validateName(workflow, entity.name);
            if (nameValidationMessages.length > 0) {
                addValidationMessage(workflow, '"' + entity.name + '": ' + _.first(nameValidationMessages), entity);
            }
            return nameValidationMessages;
        }

        function initWorkflowProcess(workflow) {
            console.assert(workflow && workflow.entity);

            workflow.processState = workflow.processState || { count: 0 };

            if (workflow.processState.updateCountAtStart === workflow.updateCount) {
                console.log('processWorkflow: skipping as nothing has changed');
                return;
            }

            if (workflow.processState.processing) {
                console.log('processWorkflow: skipping as already processing');
                return;
            }

            console.time('processWorkflow');

            workflow.processState.processing = true;
            workflow.processState.updateCountAtStart = workflow.updateCount;

            // validation messages are list of message and entity
            workflow.validationMessages = [];

            // to build activities map by id
            // each containing
            // - an ordered array of prior activities
            // - a parameters map by arg id
            // each parameter is
            // - the argument entity
            // - expression entity
            // - last compile result of the expression
            workflow.activities = {};

            // validate the workflow name
            validateEntityName(workflow, workflow.entity);

            return workflow;
        }

        function finaliseWorkflowProcess(workflow) {

            // remove any workflow expression parameters (arginstances) that are no longer valid
            // todo - the following rebuilds... don't need to do that, just remove those we don't want

            spWorkflow.updateExpressionParameters(workflow);

            // now wrap up, maybe starting again if stuff has changed

            workflow.processState.count += 1;
            workflow.processState.updateCountAtEnd = workflow.updateCount;

            console.log('processWorkflow done: ' + JSON.stringify(workflow.processState));
            console.timeEnd('processWorkflow');

            var autoStarted;

            if (workflow.processState.updateCountAtEnd > workflow.processState.updateCountAtStart) {

                workflow.processState.autoProcessCount += 1;

                if (workflow.processState.autoProcessCount > 10) {
                    console.error('processWorkflow: WOULD have started new process cycle due to detected changes BUT TOO MANY');

                } else {
                    console.log('processWorkflow: started new process cycle due to detected changes .....');
                    autoStarted = $timeout(function () {
                        workflow.processState.processing = false;
                        processWorkflow(workflow);
                    }, 0);
                }
            }

            if (!autoStarted) {
                workflow.processState.processing = false;
                workflow.processState.autoProcessCount = 0;
            }

            workflow.entity.workflowHasErrors = workflow.validationMessages.length > 0;

            return workflow;
        }

        function processWorkflowInputs(workflow) {

            // validate the workflow inputs and add them to the workflow expression parameters

            _.forEach(workflow.entity.inputArguments, function (argEntity) {
                var ok = true;
                if (_.isEmpty(argEntity.name)) {
                    addValidationMessage(workflow, 'input argument is missing a name', argEntity);
                    ok = false;
                }
                if (ok) {
                    spWorkflow.updateExpressionParameter(workflow, workflow.entity, argEntity);
                }
            });

            // build activity and parameters data

            var act = spWorkflow.activityData(workflow, workflow.entity);

            var parameterMap = _(workflow.entity.inputArguments)
                .map(function (e) {
                    return {
                        argument: e,
                        expression: null
                    };
                })
                .keyBy(function (p) {
                    return p.argument.nsAliasOrId;
                })
                .value();

            act.parameters = _.extend(act.parameters || {}, parameterMap);

            return workflow;
        }

        function processWorkflowVariables(workflow) {

            // validate the workflow variables and add them to the workflow expression parameters

            _.forEach(workflow.entity.variables, function (argEntity) {
                var ok = true;
                if (_.isEmpty(argEntity.name)) {
                    addValidationMessage(workflow, 'workflow variable is missing a name', argEntity);
                    ok = false;
                }

                if (ok) {
                    spWorkflow.updateExpressionParameter(workflow, workflow.entity, argEntity);
                }
            });

            // compile any initialisation expressions for variables, if only to validate

            var act = spWorkflow.activityData(workflow, workflow.entity);

            var parameterMap = _(workflow.entity.variables)
                .map(function (argEntity) {
                    var exprEntity = spWorkflow.getExpression(workflow, workflow.entity, argEntity);
                    var knownEntityHints = getKnownEntityParamHints(exprEntity);
                    var hints = _.map(workflow.entity.inputArguments, function (argEntity) {
                        var argInst = spWorkflow.findWorkflowExpressionParameter(workflow, workflow.entity, argEntity.idP);
                        return makeExprParamHintFromParameter(argInst);
                    });

                    return {
                        argument: argEntity,
                        expression: exprEntity,
                        exprParamHints: hints.concat(knownEntityHints)
                    };
                })
                .keyBy(function (p) {
                    return p.argument.nsAliasOrId;
                })
                .value();

            act.parameters = _.extend(act.parameters || {}, parameterMap);

            return compileExpressions(_.values(act.parameters)).then(function (results) {
                // don't need to do anything with the results... compileExpressions has stashed them already

                // need to return the workflow from each processing step
                return workflow;
            });
        }

        function processWorkflowOutputs(workflow) {

            // compile output expressions for validation

            //todo - complete this

            return workflow;
        }

        function buildOrderedActivitiesList(workflow) {

            workflow.activitySeqs = getActivitySequences(workflow);
            workflow.orderedActivities = workflow.entity.containedActivities.slice(0);
            workflow.orderedActivities.sort(function (a, b) {
                var a_paths = _.flatten(workflow.activitySeqs[a.idP]);
                var b_paths = _.flatten(workflow.activitySeqs[b.idP]);
                //                    console.log('sort', a.name, _.map(a_paths, 'name'), b.name, _.map(b_paths, 'name'));
                if (_.includes(a_paths, b)) return +1;
                if (_.includes(b_paths, a)) return -1;
                return 0;
            });

            //                console.log('orderedActivities: ...\n', _.map(workflow.orderedActivities, 'debugString').join('\n'));

            return workflow;
        }

        function projectActArgs(argEntities, actEntity) {
            return _.map(argEntities, function (e) {
                return { argument: e, activity: actEntity };
            });
        }

        function isTypeCompatible(argEntity, compileResult) {

            // this is pretty rough... just covering certain cases and not comprehensive

            var typeAlias = argEntity.type.nsAliasOrId;
            var typeName = argTypeToParamTypeMap[typeAlias];

            // ignore no result
            if (compileResult.resultType === 'None') return true;

            // pass for the 'any' type
            if (typeAlias === 'core:objectArgument') return true;

            // fail if not a list when expected
            //if (typeAlias === 'core:resourceListArgument' && !compileResult.isList) return false;

            // pass for strings
            if (typeName === 'String') return true;

            // pass for matching types
            if (typeName === compileResult.resultType) return true;

            // pass for ints going into float
            if (compileResult.resultType === 'Int32' && (typeName === 'Decimal' || typeName === 'Currency')) return true;

            // otherwise fail
            return false;
        }

        function processActivity(workflow, activity) {

            //                console.log('processActivity %o', activity.name);

            // update the workflow expression parameters (argumentInstances)

            forEach(spWorkflow.activityTypeEntity(workflow, activity).outputArguments, function (e) {
                spWorkflow.updateExpressionParameter(workflow, activity, e);
            });
            forEach(activity.outputArguments, function (e) {
                spWorkflow.updateExpressionParameter(workflow, activity, e);
            });


            // build parameter data

            var act = spWorkflow.activityData(workflow, activity);
            var workflowType = sp.findByKey(workflow.activityTypes, 'alias', spWorkflowConfiguration.aliases.workflow);

            var msgs = validateEntityName(workflow, activity);
            if (msgs.length > 0) {
                act.validationMessages = (act.validationMessages || []).concat(msgs);
            }

            //act.priorActivities = getPriorActivities(workflow, activity);
            act.priorActivities = _.flatten(workflow.activitySeqs[activity.idP]);

            //                console.log('prior acts for ', activity.name, _.map(act.priorActivities, 'name'));

            var availParams = projectActArgs(workflow.entity.inputArguments, workflow.entity)
                .concat(projectActArgs(workflow.entity.variables, workflow.entity))
                .concat(projectActArgs(workflowType.runtimeProperties, workflow.entity))
                .concat(_.flatten(_.map(act.priorActivities, function (actEntity) {
                    return projectActArgs(spWorkflow.getActivityArguments(workflow, actEntity, 'outputArguments'), actEntity);
                })));

            //                console.log('availParams for ', activity.name, sp.pluckResult(availParams, 'argument.name'), _.map(act.priorActivities, function (actEntity) {
            //                    return projectActArgs(getActivityArguments(workflow, actEntity, 'outputArguments'), actEntity);
            //                }));

            act.exprParamHints = _.map(availParams, function (p) {
                var argEntity = p.argument;
                var argInst = spWorkflow.findWorkflowExpressionParameter(workflow, p.activity, argEntity.nsAliasOrId);
                return makeExprParamHintFromParameter(argInst);
            });

            //                console.log('exprParamHints for ', activity.name, _.map(act.exprParamHints, 'name'));

            var args = spWorkflow.getActivityArguments(workflow, activity, spWorkflowConfiguration.aliases.inputArguments);
            var parameterMap = _(args)
                .map(function (argEntity) {
                    var exprEntity = spWorkflow.getExpression(workflow, activity, argEntity);
                    var knownEntityHints = getKnownEntityParamHints(exprEntity);

                    return {
                        argument: argEntity,
                        expression: exprEntity,
                        exprParamHints: act.exprParamHints.concat(knownEntityHints)
                    };
                })
                .keyBy(function (p) {
                    return p.argument.nsAliasOrId;
                })
                .value();

            act.parameters = _.extend(act.parameters || {}, parameterMap);

            // compile any initialisation expressions for variables, if only to validate

            return compileExpressions(_.values(act.parameters))
                .then(function () {
                    // compileExpressions stashes the results in the parameters

                    // Call activity type specific logic to do such as update the instanceConformsToType
                    // and do inter argument validation.... now that we know expression result types

                    var handlerFn = spWorkflowActivityService.activityProcessFns[activity.type.nsAlias];
                    return $q.when(handlerFn ? handlerFn(workflow, activity) : workflow);
                })
                .then(function () {

                    // do some generic validation

                    _.forEach(act.parameters, function (p) {

                        var typeAlias = p.argument.type.nsAliasOrId;
                        var typeName = argTypeToParamTypeMap[typeAlias];

                        if (!isTypeCompatible(p.argument, p.compileResult)) {

                            //                                console.log('processActivity %o: arg %o type %o (%o) expr %o type %o list %o', activity.name,
                            //                                    p.argument.nsAliasOrId, typeAlias, typeName,
                            //                                    p.compileResult.expression, p.compileResult.resultType, p.compileResult.isList);

                            addValidationMessage(workflow, ['activity', '"' + activity.name + '"', 'has parameter of wrong type'].join(' '), activity);
                            act.validationMessages = (act.validationMessages || []).concat(['activity has parameter', '"' + p.argument.name + '"', 'of wrong type'].join(' '));
                            p.errors = (p.errors || []).concat('has the wrong type');
                        }

                        if (p.argument.argumentIsMandatory && _.isEmpty(p.expression.expressionString)) {

                            //                                console.log('processActivity %o: mandatory arg %o type %o (%o) expr %o type %o list %o', activity.name,
                            //                                    p.argument.nsAliasOrId, typeAlias, typeName,
                            //                                    p.compileResult.expression, p.compileResult.resultType, p.compileResult.isList);

                            addValidationMessage(workflow, ['activity', '"' + activity.name + '"', 'is missing a mandatory parameter'].join(' '), activity);
                            act.validationMessages = (act.validationMessages || []).concat(['activity is missing mandatory parameter', '"' + p.argument.name + '"'].join(' '));
                        }
                    });

                    // check for any errors

                    if (_.some(act.parameters, function (p) {
                            return p.compileResult.error;
                        })) {
                        addValidationMessage(workflow, ['activity', '"' + activity.name + '"', 'has parameter errors'].join(' '), activity);
                        act.validationMessages = (act.validationMessages || []).concat('activity has expression errors');
                    }

                    //                        console.log('processActivity %o .... done', activity.name);

                    // need to return the workflow from each processing step
                    return workflow;
                });
        }

        /**
         *
         * @param xs - sequence to iterate over
         * @param fn - potentially promise returning function taking elements of the seq
         * @returns {promise} for an array of the resolved result of each
         *
         * @note not used yet and not tested
         */
        function asyncForEach(xs, fn) {
            if (!xs || _.isEmpty(xs)) {
                return $q.when([]);
            }
            return $q.when(fn(_.first(xs)).then(function (result) {
                return asyncForEach(_.tail(xs), fn).then(function (restResults) {
                    return [result].concat(restResults);
                });
            }));
        }

        function processActivitiesInOrder(workflow) {
            return asyncForEach(workflow.orderedActivities, _.partial(processActivity, workflow))
                .then(function () {
                    return workflow;
                });
        }

        /**
         * Call this after updates to the workflow entity model to update parameters
         * compile expressions and perform validation.
         *
         * @returns {promise} a promise for the updated workflow
         */
        function processWorkflow(workflow) {

            if (!initWorkflowProcess(workflow)) {
                return $q.when(workflow);
            }

            return $q.when(workflow)
                .then(processWorkflowInputs)
                .then(processWorkflowVariables)
                .then(buildOrderedActivitiesList)
                .then(processActivitiesInOrder)
                .then(processWorkflowOutputs)
                .finally(_.partial(finaliseWorkflowProcess, workflow));
        }

        /**
         * Create and initialise an activity of the given type and return a promise for the resulting entity.
         * @returns {promise} promise for the created activity
         */
        function createActivity(workflow, typeIdOrAlias, name, description) {

            var activity = spEntity.fromJSON({
                typeId: typeIdOrAlias,
                name: jsonString(name),
                description: jsonString(description),
                designerData: jsonString(''),
                exitPoints: [],
                inputArguments: [],
                outputArguments: [],
                expressionMap: []
            });

            if (spWorkflow.getExitPoints(workflow, activity).length === 0) {
                activity.getExitPoints().add(createDefaultExitPoint());
            }

            return $q.when().then(function () {
                var handlerFn = spWorkflowActivityService.activityCreatedFns[activity.getType().getNsAlias()];
                return $q.when(handlerFn ? handlerFn(workflow, activity) : workflow);
            }).then(function () {
                return activity;
            });
        }

        /**
         * Call activityUpdated after any changes to the activity configuration. This is required
         * as some activity types have knock on effects from configuration changes.
         * @param {object} workflow model object
         * @param {object} activity entity
         * @param {string} hint to give context to the update
         * @returns {promise} promise for the workflow model object
         */
        function activityUpdated(workflow, activity, hint) {
            assert(activity && activity.getType, 'missing expected arguments', activity);
            assert(activity.getType(), 'given entity is missing type', activity);

            var handlerFn = spWorkflowActivityService.activityUpdatedFns[activity.getType().getNsAlias()];
            return $q.when(handlerFn ? handlerFn(workflow, activity, hint) : workflow)
                .then(function (workflow) {

                    // This is arguably overkill. We should look to pass in the activity
                    // so it only needs to do the minimum updates. And maybe we should look
                    // for a way to only trigger the syncing if the relevant parts of the
                    // updated activity are updated - in this case the name or outputs.
                    // @todo Some profiling on workflows with lots of activities
                    spWorkflow.updateExpressionParameters(workflow);

                    spWorkflow.setBookmark(workflow);
                    return workflow;
                });
        }

        /**
         * Add the given activities to the given workflow. The sequences are not modified.
         */
        function addActivities(workflow, activities) {
            assert(workflow && activities, 'missing expected arguments');
            assert(workflow.entity, 'workflow argument is missing its entity');

            forEach(activities, _.bindKey(workflow.entity.getContainedActivities(), 'add'));
            spWorkflow.setDefaultActivityNames(workflow);
            spWorkflow.updateExpressionParameters(workflow);
            spWorkflow.setBookmark(workflow);

            return workflow;
        }

        /**
         * Remove the activity from the workflow and adjust any sequences doing a reasonable job of
         * reconnecting those before to those after the removed activity.
         */
        function removeActivity(workflow, activity) {
            assert(workflow && activity, 'missing expected arguments');
            assert(workflow.entity, 'workflow argument is missing its entity');

            function hasValue(path, value, o) {
                return sp.result(o, path) === value;
            }

            var workflowEntity = workflow.entity;
            var activityId = activity.id();

            // Remove from the list of activities.

            if (!_.includes(workflowEntity.getContainedActivities(), activity)) {
                // quietly do nothing
                return workflow;
            }

            workflowEntity.containedActivities.deleteEntity(activity);

            // Find the sequences in and out of this activity that we'll drop unless reconnected.

            var transIn = filter(workflowEntity.getTransitions(), partial(hasValue, 'getToActivity.id', activityId));
            var transOut = filter(workflowEntity.getTransitions(), partial(hasValue, 'getFromActivity.id', activityId));
            var termsOut = filter(workflowEntity.getTerminations(), partial(hasValue, 'getFromActivity.id', activityId));
            var firstToActivity = first(transOut) && first(transOut).getToActivity(); // may be null

            // Reset the first activity if necessary, and setting to null of no 'next' activities to set it to.

            if (workflowEntity.getFirstActivity() && workflowEntity.getFirstActivity().id() === activityId) {
                workflowEntity.setFirstActivity(firstToActivity);
            }

            // Connect any incoming sequences to the first following activity, if one.

            if (firstToActivity) {
                while (transIn.length > 0 && first(transIn).getFromActivity().id() !== firstToActivity.id()) {
                    first(transIn).setToActivity(firstToActivity);
                    transIn = _.tail(transIn);
                }
            }

            if (transIn.length > 0 && termsOut.length > 0) {
                first(termsOut).setFromActivity(first(transIn).getFromActivity());
                first(termsOut).setFromExitPoint(first(transIn).getFromExitPoint());
                termsOut = _.tail(termsOut);
            }

            // Drop any sequences now not needed

            workflowEntity.transitions.deleteEntity(transIn);
            workflowEntity.transitions.deleteEntity(transOut);
            workflowEntity.terminations.deleteEntity(termsOut);

            spWorkflow.updateExpressionParameters(workflow);
            workflowUpdated(workflow);
            return workflow;
        }

        function addSwimlane(workflow) {
            var swimlanes = workflow.entity.swimlanes;
            var swimlane = spEntity.createEntityOfType(aliases.swimlane,
                spWorkflow.getUniqueName(_.map(swimlanes, 'name'), 'untitled'));
            swimlanes.add(swimlane);
            workflowUpdated(workflow);
            return swimlane;
        }

        function removeSwimlane(workflow, swimlane) {
            if (workflow.entity.swimlanes.length) {
                workflow.entity.swimlanes.deleteEntity(swimlane);
                workflowUpdated(workflow);
            }
        }

        function addEndEvent(workflow) {
            var exitPoints = workflow.entity.exitPoints,
                exitPoint = spEntity.fromJSON({
                    typeId: aliases.exitPoint,
                    name: spWorkflow.getUniqueName(_.map(exitPoints, 'name'), 'end'),
                    isDefaultExitPoint: false,
                    exitPointOrdinal: 1 + _.reduce(exitPoints, function (a, e) {
                        return Math.max(a, e.getExitPointOrdinal());
                    }, 0)
                });
            exitPoints.add(exitPoint);
            workflowUpdated(workflow);
            return exitPoint;
        }

        function removeEndEvent(workflow, endEvent) {
            workflow.entity.exitPoints.deleteEntity(endEvent);
            workflow.entity.terminations.remove(function (t) {
                return t.getWorkflowExitPoint().eid().matches(endEvent.eid());
            });
            workflowUpdated(workflow);
        }

        function addTransition(workflow, fromActivity, fromExitPoint, toActivity, name) {
            var newTran =
                spEntity.fromJSON({
                    name: jsonString(name),
                    typeId: aliases.transition,
                    fromActivity: jsonLookup(fromActivity),
                    fromExitPoint: jsonLookup(fromExitPoint),
                    toActivity: jsonLookup(toActivity)
                });
            workflow.entity.transitions.add(newTran);
            spWorkflow.setBookmark(workflow);
        }

        function addTermination(workflow, fromActivity, fromExitPoint, workflowExitPoint) {
            workflow.entity.getTerminations().add(spEntity.fromJSON({
                typeId: aliases.termination,
                fromActivity: jsonLookup(fromActivity),
                fromExitPoint: jsonLookup(fromExitPoint),
                workflowExitPoint: jsonLookup(workflowExitPoint)
            }));
            spWorkflow.setBookmark(workflow);
        }

        function addActivityInSequence(workflow, activity) {

            //console.log('add acts', workflow, activity, spEntity.toJSON(workflow.entity), spEntity.toJSON(activity));

            if (!activity) {
                return workflow;
            }

            if (activity.dataState === spEntity.DataStateEnum.Delete) {
                activity.dataState = spEntity.DataStateEnum.Update;
            }

            workflow.entity.getContainedActivities().add(activity);

            // fixup the going from selectedEntity to id and back again... is historical
            // but remember that the selectedEntity might not be an activity

            var fromActivityId = (workflow.selectedEntity && workflow.selectedEntity.id()) || workflow.entity.id();
            var fromExitId = (workflow.selectedExit && workflow.selectedExit.id()) || null;
            var defaultExitPoint = _(spWorkflow.getExitPoints(workflow, activity)).sortBy('exitPointOrdinal').first();
            var defaultWorkflowExitPoint = _(spWorkflow.getExitPoints(workflow, workflow.entity)).sortBy('exitPointOrdinal').first();

            if (fromActivityId === workflow.entity.id()) {
                // insert as the first activity

                var oldFirstActivity = workflow.entity.getFirstActivity();

                workflow.entity.setFirstActivity(activity);
                if (oldFirstActivity) {
                    addTransition(workflow, activity, defaultExitPoint, oldFirstActivity);
                } else {
                    addTermination(workflow, activity, defaultExitPoint, defaultWorkflowExitPoint);
                }

            } else if (fromActivityId) {
                // insert after the given activity, in front of any following activities

                var fromActivity = spEntity.findByEid(workflow.entity.getContainedActivities(), fromActivityId);
                if (fromActivity) {

                    var fromExitPoints = spWorkflow.getExitPoints(workflow, fromActivity);

                    assert(fromExitPoints && fromExitPoints.length);

                    // if the fromExit is specified then use it
                    // otherwise look for the first exit that is free or used by a termination
                    // giving priority to free before terminations

                    var fromExit = fromExitId && spEntity.findByEid(fromExitPoints, fromExitId) ||
                        _.find(fromExitPoints, function (e) {
                            var seq = spWorkflow.getSequenceUsingExit(workflow, fromActivity, e);
                            return !seq;
                        }) ||
                        _.find(fromExitPoints, function (e) {
                            var seq = spWorkflow.getSequenceUsingExit(workflow, fromActivity, e);
                            return !seq || seq.getType().alias() === aliases.termination;
                        }) ||
                        _.first(fromExitPoints);

                    assert(fromExit);

                    var seq = spWorkflow.getSequenceUsingExit(workflow, fromActivity, fromExit);
                    if (seq) {
                        // update the sequence using the desired 'fromexit' to come from the new activity
                        seq.setFromActivity(activity);
                        seq.setFromExitPoint(defaultExitPoint);
                    } else {
                        // nothing to insert in front of, so terminate
                        addTermination(workflow, activity, defaultExitPoint, defaultWorkflowExitPoint);
                    }

                    // sequence from the 'from' to our new activity
                    addTransition(workflow, fromActivity, fromExit, activity);
                }
            }

            spWorkflow.setBookmark(workflow);
            return workflow;
        }

        /**
         * Add the given activities to the workflow and insert them into the existing workflow
         * sequence, if any, after the selected activity and exit.
         */
        function addActivitiesInSequence(workflow, activities) {

            assert(!activities || _.isArray(activities));

            if (!activities || !activities.length) {
                return workflow;
            }

            forEach(activities, partial(addActivityInSequence, workflow));

            spWorkflow.setDefaultActivityNames(workflow);
            spWorkflow.updateExpressionParameters(workflow);
            return workflow;
        }

        /**
         * Add sequences (transitions or terminations).
         */
        function addSequence(workflow, from, fromExit, to, name) {

            assert(!from || _.isObject(from));
            assert(!fromExit || _.isObject(fromExit));
            assert(!to || _.isObject(to));

            var activityExitPoints = spWorkflow.getExitPoints(workflow, from);
            var workflowExitPoints = spWorkflow.getExitPoints(workflow, workflow.entity);

            assert(!from || activityExitPoints && activityExitPoints.length > 0);
            assert(workflowExitPoints && workflowExitPoints.length > 0);

            if (!from || from === workflow.entity) {
                workflow.entity.setFirstActivity(to);
                spWorkflow.setBookmark(workflow);
                return $q.when(workflow);
            }

            var orderedExitPoints = _.sortBy(activityExitPoints, function (ep) {
                return ep.getExitPointOrdinal();
            });

            fromExit = fromExit ||
                _.find(orderedExitPoints, _.partial(spWorkflow.isFreeExit, workflow, from)) ||
                _.first(orderedExitPoints);

            if (!spWorkflow.isFreeExit(workflow, from, fromExit)) {
                // tinkering with exceptions....
                // this reject the promise....
                throw { message: 'the connection point is not available', name: 'AddSequenceError' };
            }

            if (to && _.includes(workflow.entity.getContainedActivities(), to)) {
                addTransition(workflow, from, fromExit, to, name);
            } else {
                if (to && !_.includes(workflowExitPoints, to)) {
                    throw { message: 'unknown "to" entity', name: 'AddSequenceError' };
                }
                to = to || _.first(workflowExitPoints);
                addTermination(workflow, from, fromExit, to);
            }

            spWorkflow.setBookmark(workflow);
            return workflow;
        }

        function removeSequence(workflow, seq) {
            return spWorkflow.removeSequence(workflow, seq);
        }

        function canAddSequence(workflow, from, fromExit, to) {

            assert(from && _.isObject(from));

            return fromExit && spWorkflow.isFreeExit(workflow, from, fromExit) ||
                _.some(spWorkflow.getExitPoints(workflow, from), _.partial(spWorkflow.isFreeExit, workflow, from));
        }

        function updateSequence(workflow, seq, from, fromExit, to) {

            function sameFrom(seq, from, fromExit) {
                return spWorkflow.matchingEids(seq.getFromActivity(), from) &&
                    spWorkflow.matchingEids(seq.getFromExitPoint(), fromExit);
            }

            assert(seq && _.isObject(seq));
            assert(!to || _.isObject(to));

            if (seq.nsAliasOrId === aliases.firstActivity) {
                if (!sp.findByKey(workflow.entity.getContainedActivities(), 'id', to.id())) {
                    console.error('cannot connect start to non-activity');
                } else {
                    workflow.entity.firstActivity = to;
                }
                spWorkflow.setBookmark(workflow);
                return workflow;
            }

            assert(from && _.isObject(from));

            if (!(sameFrom(seq, from, fromExit) || canAddSequence(workflow, from, fromExit, to))) {
                throw {message: 'cannot add sequence', name: 'AddSequenceError'};
            }

            //todo - rather than drop and recreate - we should adjust the given one
            removeSequence(workflow, seq);
            return addSequence(workflow, from, fromExit, to);
        }

        function deleteSelected(workflow) {
            assert(workflow, 'missing expected argument: workflow');

            if (workflow.selectedEntity.type.alias() === "core:dummy") {
                // special case the start transition - some of the other removes don't cope with the dummy entity.
                removeSequence(workflow, workflow.selectedEntity);
            } else {
                // each of the following will do nothing if the entity is not the correct type...
                // and need to check selectedEntity before each as it may be cleared
                if (workflow.selectedEntity) removeActivity(workflow, workflow.selectedEntity);
                if (workflow.selectedEntity) removeSequence(workflow, workflow.selectedEntity);
                if (workflow.selectedEntity) removeSwimlane(workflow, workflow.selectedEntity);
                if (workflow.selectedEntity) removeEndEvent(workflow, workflow.selectedEntity);
            }
        }

        function getWorkflowMenu(workflow) {

            function visibleInTree(e) {
                
                // filter on feature switch
                var alias = e.alias();
                console.log("110:", alias);

                // filter based upon hidden category, when not in dev mode
                var category = e.wfActivityCategory;

                if (category &&
                    category.alias() === 'core:activityCategoryEnum_Hidden' &&
                    !spAppSettings.initialSettings.devMode) {
                    return false;
                }

                return true;
            }

            var addMenuItems = _(workflow.activityTypes)
                .filter(visibleInTree)
                .map(function (e) {
                    var config = getActivityTypeConfig(e.eid().getNsAlias());
                    var category = e.getLookup('core:wfActivityCategory') || {};
                    var catOrdinal = _.result(category, 'getOrdinal') || 99;
                    var itemOrdinal = e.activityTypeOrder || 99;
                    return {
                        id: e.nsAliasOrId,
                        name: e.getName(),
                        description: e.getDescription(),
                        category: _.result(category, 'getName') || 'Other',
                        ordinal: catOrdinal * 100 + itemOrdinal,
                        menuImage: config.imagePath + 'menu/' + config.menuImage
                    };
                })
                .sort(function (a, b) {
                    if (a.ordinal === b.ordinal) {
                        return (a.name < b.name) ? -1 : +1;
                    }
                    return (a.ordinal < b.ordinal) ? -1 : +1;

                })
                .value();

            // todo: model this in data
            addMenuItems.push({
                id: 'sequence', name: 'Sequence to End', category: 'Other Activities', ordinal: 990,
                description: 'Link this activity to the end.',
                menuImage: 'assets/images/activities/menu/sequenceToEnd.png'
            });
            addMenuItems.push({
                id: 'endEvent', name: 'End Event', category: 'Other Activities', ordinal: 992,
                description: 'End Activity.',
                menuImage: 'assets/images/activities/menu/endEvent.png'
            });
            addMenuItems.push({
                id: 'swimlane', name: 'Add Swimlane', category: 'Other Activities', ordinal: 994,
                description: 'Add a new swimlane.',
                menuImage: 'assets/images/activities/menu/swimlane.png'
            });

            return _(addMenuItems)
                .groupBy('category')
                .map(function (arr, key) {
                    return [
                        { id: 0, name: key, ordinal: _.first(arr).ordinal, isHeader: true }
                    ].concat(arr);
                })
                .flatten()
                .value();
        }

        function getExpressionCompileResult(workflow, activity, parameterAlias, parameterRelAlias) {

            var parameter = _.first(spWorkflow.getActivityParameters(workflow, activity, parameterRelAlias, { aliasOrId: parameterAlias }));
            if (!parameter) {
                console.warn('getExpressionCompileResult: cannot find parameter %o on activity %o', parameterAlias, activity.debugString);
            }
            var expression = parameter && parameter.expression;

            if (!expression) return $q.when(null);

            // Grab all the workflow expression parameters

            var parameterHints = _.map(spWorkflow.getWorkflowExpressionParameters(workflow), function (p) {
                //console.log('getExpressionCompileResult: workflow parameter hint ', p);
                return { name: p.name, description: p.description, typeName: 'Entity', entityTypeId: p.conformsToType || 'core:resource' };
            });

            // Add in any known entities for the expression as local parameters

            parameterHints = parameterHints.concat(_.map(expression.wfExpressionKnownEntities, function (e) {
                //console.log('getExpressionCompileResult: expression known entity parameter hint ', e);
                var refEntity = e.referencedEntity;
                return { name: refEntity.name, description: refEntity.description, typeName: 'Entity', entityTypeId: refEntity.getType().id() || 'core:resource' };
            }));

            // Compile the expression

            return spCalcEngineService.compile(expression.expressionString, { context: null, params: parameterHints, host: 'Evaluate' });
        }

        function getExpressionEvalResult(workflow, activity, parameterAlias, parameterRelAlias) {

            var parameter = _.first(spWorkflow.getActivityParameters(workflow, activity, parameterRelAlias, { aliasOrId: parameterAlias }));
            if (!parameter) {
                console.warn('getExpressionCompileResult: cannot find parameter %o on activity %o', parameterAlias, activity.debugString);
            }
            var expression = parameter && parameter.expression;

            if (!expression) return $q.when(null);

            // Grab all the workflow expression parameters

            var parameterHints = _.map(spWorkflow.getWorkflowExpressionParameters(workflow), function (p) {
                //console.log('getExpressionCompileResult: workflow parameter hint ', p);
                return {
                    name: p.name, description: p.description, typeName: 'Entity',
                    entityTypeId: p.conformsToType || 'core:resource'
                };
            });

            // Add in any known entities for the expression as local parameters

            parameterHints = parameterHints.concat(_.map(expression.wfExpressionKnownEntities, function (e) {
                //console.log('getExpressionCompileResult: expression known entity parameter hint ', e);
                return {
                    name: e.name, description: e.description, typeName: 'Entity',
                    entityTypeId: e.getType().id(),
                    value: e.id()
                };
            }));

            // Compile the expression

            return spCalcEngineService.evalExpression(expression.expressionString, null, null, parameterHints);
        }

        /**
         * applyUpdate is a way to perform some function on the workflow model and have the updateCount
         * automatically updated. At present we always update, however a thought is to check if the
         * workflow actually changed.
         *
         * Any additional arguments to applyUpdate are passed to the fn following the workflow itself.
         *
         * @param fn
         * @param workflow
         * @returns the return value of calling the fn
         */
        function applyUpdate(fn, workflow) {
            if (!workflow) {
                return null;
            }
            var result = fn.apply(null, Array.prototype.slice.call(arguments, 1));
            spWorkflow.setBookmark(workflow);
            return result;
        }
    }

})();


