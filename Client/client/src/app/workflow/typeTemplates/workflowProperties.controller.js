// Copyright 2011-2016 Global Software Innovation Pty Ltd
/*global _, console, angular, sp, spWorkflowConfiguration, spEntity, spWorkflow, jsonString */

(function () {
    'use strict';

    angular.module('sp.workflow.activities.workflow')
        .controller('workflowPropertiesController', function ($scope, $q, spWorkflowService, spWorkflowEditorViewService) {

            // convenience vars
            var aliases = spWorkflowConfiguration.aliases;

            // We use an index against the arguments that we patch onto the entity object itself
            // so when we add a new argument at the end of the list it stays at the end after a refresh
            // That's the plan anyway...

            var nextSortKey = 0;

            console.assert($scope.workflow && $scope.workflow.entity);
            console.assert($scope.entity && $scope.workflow.entity === $scope.entity);

            var defaultContext = {
                workflow: $scope.workflow,
                activity: $scope.entity
            };

            var defaultActions = [
                {
                    name: 'exprEditor',
                    action: function (parameter, action) {
                        return spWorkflowEditorViewService.openExprEditor(_.defaults({parameter: parameter}, defaultContext));
                    }
                },
                {
                    name: 'resourceChooser',
                    action: function (parameter, action) {
                        return spWorkflowEditorViewService.openSingleKnownEntityChooser(_.defaults({chooserType: 'resourceChooser', parameter: parameter}, defaultContext));
                    }
                },
                {
                    name: 'parameterChooser',
                    action: function (parameter, action) {
                        return spWorkflowEditorViewService.openSingleParameterChooser(_.defaults({parameter: parameter}, defaultContext));
                    }
                }
            ];

            function makeParameterHint(e, typeId) {
                return { name: e.name, description: e.description, typeName: 'Entity',
                    entityTypeId: typeId || (e.isOfType && _.first(e.isOfType).id()) || 'core:resource' };
            }

            function extendParameter(parameter) {

                var knownEntityParameters = parameter.expression && _.map(parameter.expression.wfExpressionKnownEntities, function (e) {
                    return makeParameterHint(e.referencedEntity);
                });

                return _.extend(parameter, {
                    parameterHints: $scope.parameterHints.concat(knownEntityParameters || []),
                    actions: _.keyBy(defaultActions, 'name')
                });
            }

            function findArgumentRel(argEntity) {
                var w = $scope.entity;
                var id = argEntity.id();
                return spEntity.findByEid(w.inputArguments, id) ? aliases.inputArguments :
                    spEntity.findByEid(w.outputArguments, id) ? aliases.outputArguments :
                        spEntity.findByEid(w.variables, id) ? aliases.variables :
                            null;
            }

            function addArgument(typeAlias, relAlias, name, baseName) {
                var argEntity = spEntity.fromJSON({
                    typeId: typeAlias,
                    name: name ||
                        spWorkflow.getUniqueName(_.map($scope.entity.getRelationship(relAlias), 'name'), baseName || 'Param'),
                    description: jsonString(''),
                    defaultExpression: jsonString(null),
                    argumentIsMandatory: false
                });
                if (spWorkflow.isResourceArgument(argEntity)) {
                    argEntity.setLookup(aliases.conformsToType, aliases.resource);
                }
                argEntity.__spTempSortKey = (nextSortKey += 1);
                $scope.entity.getRelationship(relAlias).add(argEntity);
                $scope.argTypeMap[argEntity.id()] = argEntity.getType().getNsAlias();

                //TODO - change to only do what is needed rather than a full rebuild of all
                spWorkflow.updateExpressionParameters($scope.workflow);

                spWorkflowService.workflowUpdated($scope.workflow);

                return argEntity;
            }

            function removeArgument(argEntity) {

                var relAlias = findArgumentRel(argEntity);
                if (relAlias) {
                    $scope.entity.getRelationship(relAlias).deleteEntity(argEntity.idP);

                    //TODO - change to only do what is needed rather than a full rebuild of all
                    spWorkflow.updateExpressionParameters($scope.workflow);
                }
            }

            function updateArgumentType(argEntity, typeAlias) {
                if (argEntity.getType().getNsAlias() !== typeAlias) {
                    addArgument(typeAlias, findArgumentRel(argEntity), argEntity.name)
                        .__spTempSortKey = argEntity.__spTempSortKey;
                    $scope.removeArgument(argEntity);
                }
            }

            $scope.actionArgumentId = 0;
            $scope.relatedResourceArgumentId = 0;

            $scope.addInputArgument = function () {
                var newArg = addArgument(aliases.resourceArgument, aliases.inputArguments, null, 'Input');

                if (!$scope.actionArgumentId) {

                    $scope.actionArgumentId = newArg.idP;
                } else {
                    if (!$scope.relatedResourceArgumentId) {

                        $scope.relatedResourceArgumentId = newArg.idP;
                    }
                }
            };

            $scope.addOutputArgument = function () {
                addArgument(aliases.resourceArgument, aliases.outputArguments, null, 'Output');
            };

            $scope.addVariable = function () {
                addArgument(aliases.resourceArgument, aliases.variables, null, 'Var');
            };

            $scope.removeArgument = function (argEntity) {
                removeArgument(argEntity);
            };

            $scope.isResourceArgument = function (argEntity) {
                return spWorkflow.isResourceArgument(argEntity);
            };

            $scope.setArgType = function (argEntity) {
                updateArgumentType(argEntity, $scope.argTypeMap[argEntity.id()]);
            };

            $scope.addEndEvent = function () {
                spWorkflowService.addEndEvent($scope.workflow);
            };

            $scope.removeEndEvent = function (value) {
                spWorkflowService.removeEndEvent($scope.workflow, value);
            };

            $scope.showConformsToTypePicker = function (argEntity) {
                spWorkflowEditorViewService.chooseResource(null, sp.result(argEntity, 'conformsToType.id'), 'typeChooser').then(function (resource) {
                    argEntity.conformsToType = resource.entity;
                    spWorkflowService.workflowUpdated($scope.workflow);
                });
            };

            
            // filter out any of the arguments that are not record arguments
            $scope.filterResourceArgs = function (element) {
                return !!element.conformsToType;
            };

            $scope.filterActionArg = function (element) {
                return element.idP !== $scope.actionArgumentId;
            };


            $scope.filterRelatedArg = function (element) {
                return element.idP !== $scope.relatedResourceArgumentId;
            };

            $scope.$watch('actionArgumentId', function (actionArgumentId) {

                if ($scope.entity) {

                    console.assert($scope.workflow.entity === $scope.entity);

                    var existingArgId = sp.result($scope.entity, 'inputArgumentForAction.idP');

                    // triggering input was set to none
                    if ((_.isUndefined(actionArgumentId) || (_.isNull(actionArgumentId))) && existingArgId && $scope.entity.inputArgumentForAction) {
                        $scope.entity.inputArgumentForAction = null;
                    } else {

                        // triggering input has changed
                        if (existingArgId !== actionArgumentId && actionArgumentId > 0) {
                            $scope.entity.inputArgumentForAction = spEntity.fromId(actionArgumentId);
                        }
                    }
                }
            });

            $scope.$watch('relatedResourceArgumentId', function (relatedResourceArgumentId) {

                if ($scope.entity) {

                    var existingRelId = sp.result($scope.entity, 'inputArgumentForRelatedResource.idP');

                    if ((_.isUndefined(relatedResourceArgumentId) || _.isNull(relatedResourceArgumentId)) && existingRelId && $scope.entity.inputArgumentForRelatedResource) {
                        $scope.entity.inputArgumentForRelatedResource = 0;
                    } else {
                        if (existingRelId !== relatedResourceArgumentId && relatedResourceArgumentId > 0) {
                            $scope.entity.inputArgumentForRelatedResource = spEntity.fromId(relatedResourceArgumentId);
                        }
                    }
                }
            });

            $scope.$watch('workflow.processState.count', function () {

                console.time('workflowController.workflowUpdated');

                // map of argument to its type alias, so we can bind an angular SELECT to it

                $scope.argTypeMap = {};

                _($scope.entity.inputArguments)
                    .concat($scope.entity.outputArguments)
                    .concat($scope.entity.variables)
                    .each(function (argEntity) {
                        if (!argEntity.__spTempSortKey) {
                            argEntity.__spTempSortKey = (nextSortKey += 1);
                        }
                        $scope.argTypeMap[argEntity.id()] = argEntity.getType().getNsAlias();
                    });

                // set up the parameters (argument + expression + other stuff)

                $scope.parameterHints = _.map(spWorkflow.getWorkflowExpressionParameters($scope.workflow), function (e) {
                    return makeParameterHint(e, e.conformsToType);
                });

                $scope.inputParameters = _(spWorkflow.getActivityParameters($scope.workflow, $scope.entity, 'inputArguments'))
                    .map(extendParameter)
                    .keyBy(function (parameter) {
                        return parameter.argument.aliasOrId();
                    })
                    .value();

                $scope.outputParameters = _(spWorkflow.getActivityParameters($scope.workflow, $scope.entity, 'outputArguments'))
                    .map(extendParameter)
                    .keyBy(function (parameter) {
                        return parameter.argument.aliasOrId();
                    })
                    .value();

                $scope.variableParameters = _(spWorkflow.getActivityParameters($scope.workflow, $scope.entity, 'variables'))
                    .map(extendParameter)
                    .keyBy(function (parameter) {
                        return parameter.argument.aliasOrId();
                    })
                    .value();

                // capture other stuff that we wish to render

                $scope.actionArgumentId = sp.result($scope.entity, 'inputArgumentForAction.idP');
                $scope.relatedResourceArgumentId = sp.result($scope.entity, 'inputArgumentForRelatedResource.idP');

                console.timeEnd('workflowController.workflowUpdated');
            });

            $scope.$watch('entity.inputArgumentForAction.idP', function (value, prev) {
                if (value !== prev) {
                    spWorkflowService.workflowUpdated($scope.workflow);
                }
            });

            $scope.$watch('entity.inputArgumentForRelatedResource.idP', function (value, prev) {
                if (value !== prev) {
                    spWorkflowService.workflowUpdated($scope.workflow);
                }
            });

            // async call to build select options list of argument types

            $scope.argTypes = [];

            spWorkflowService.getArgumentTypes().then(function (argTypesList) {
                $scope.argTypes = _(argTypesList)
                    .reject({nsAlias: 'core:activityArgument'})
                    .reject({nsAlias: 'core:objectArgument'})
                    .map(function (e) {
                        return {
                            id: e.idP,
                            alias: e.nsAlias,
                            name: e.name
                        };
                    })
                    .value();
            });
        });

}());