// Copyright 2011-2016 Global Software Innovation Pty Ltd
/*global angular, _, console, sp, spEntity, spWorkflowConfiguration, spWorkflow, jsonString */

(function () {
    'use strict';

    angular.module('sp.workflow.builder')
        .controller('sp.workflow.updateEntityActivityController', function ($scope, $q, spWorkflowService, spWorkflowEditorViewService) {

            // this controller handles createActivity, updateFieldActivity and clone activity

            var aliases = spWorkflowConfiguration.aliases;

            /**
             * Returns a promise for the member entity that the given expression represents
             * @param argEntity
             * @returns promise for the memberEntity that includes the member meta data to determine things
             * like if a field, or relationship, the cardinality and totype
             */
            function getMemberEntity(id) {
                return spWorkflowService.getCacheableEntity('memberEntity:' + id, id, 'alias,name,isOfType.{alias,name},cardinality.{alias,name},{toType,fromType}.{alias,name,isOfType.{alias,name}}')
                    .then(function (entity) {
                        //console.log('get memberEntity => ', spEntity.toJSON(entity));
                        return entity;
                    });
            }

            function getMemberEntityForArg(argEntity) {
                var exprEntity = spWorkflow.getExpression($scope.workflow, $scope.entity, argEntity);
                console.assert(exprEntity);

                // so we don't have to evaluate the expression to find its value let's assume it is a
                // single known entity reference.
                //todo: wfExpressionKnownEntities are not being replaced.. hence us getting "last" here rather than first... should be fixed
                var e = _.last(exprEntity.wfExpressionKnownEntities);
                return $q.when(sp.result(e, 'referencedEntity') ? getMemberEntity(e.referencedEntity.idP) : null);
            }

            /**
             * Get an argument from the given group for the given name.
             * Leave name undefined for the 'member' argument.
             */
            function getGroupArg(g, name) {
                return _.find($scope.entity.inputArguments, function (e) {
                    return !name ? e.name === g.key : e.name.indexOf(g.key + '_' + name) === 0;
                });
            }

            function getGroupArgs(g, name) {
                return _.filter($scope.entity.inputArguments, function (e) {
                    return e.name.indexOf(g.key + (name ? '_' + name : '')) === 0;
                });
            }

            function getGroup(argEntity) {
                return _.find($scope.groups, { key: _.first(argEntity.name.split('_')) });
            }

            function isResourceTypeArg(argEntity) {
                return argEntity.eid().getNsAlias() === 'core:createActivityResourceArgument' ||
                    argEntity.eid().getNsAlias() === 'core:updateFieldActivityResourceArgument'||
                    argEntity.eid().getNsAlias() === 'core:resourceToCloneArgument';
            }

            function isMemberArg(argEntity) {
                return !isResourceTypeArg(argEntity) && argEntity.name.indexOf('_') < 0;
            }

            function isMemberValueArg(argEntity) {
                return argEntity.name.indexOf('_value') > 0;
            }

            /**
             * Get the "member type" for the group.
             */
            function getMemberTypeForGroup(g) {

                if (!g || !g.memberEntity) return null;

                var memberEntity = g.memberEntity;

                var isRel = sp.result(memberEntity, 'type.nsAlias') === 'core:relationship';
                var isChoice = isRel && sp.result(memberEntity, 'toType.type.nsAlias') === 'core:enumType';
                var isLookup = false;

                var cardinality = (sp.result(memberEntity, 'cardinality.alias') || 'ManyToMany').toLowerCase();

                if (isRel) {
                    var reverseArg = getGroupArg(g, 'reverse');
                    var expr = reverseArg && spWorkflow.getExpression($scope.workflow, $scope.entity, reverseArg);
                    if (expr) {
                        // we assume this expression is only bool value and not a more complex expression with refs or functions
                        var isReverse = expr.expressionString === 'true';
                        isLookup = isRel && (!isReverse && cardinality.indexOf('toone') >= 0 || isReverse && cardinality.indexOf('oneto') >= 0);
                    }
                }

                return isChoice ? 'choice' : isLookup ? 'lookup' : isRel ? 'rel' : 'field';
            }

            function removeGroup(group) {
                var groupArgs = getGroupArgs(group);
                var groupArgIds = _.map(groupArgs, 'idP');

                $scope.entity.inputArguments.deleteEntity(groupArgs);

                var argExpressions = _.filter($scope.entity.expressionMap, function(e) {
                    return groupArgIds.indexOf(sp.result(e, 'argumentToPopulate.idP')) >= 0;
                });

                $scope.entity.expressionMap.deleteEntity(argExpressions);
            }

            /**
             * Update the argument group based on the 'member type' of the given memberEntity
             */
            function updateArgGroup(key, memberEntity, direction) {

                //temp removing all - later todo sync if the existing are compatible
                $scope.entity.inputArguments.remove(function (e) {
                    return e.name.indexOf(key + '_value') === 0 ||
                        e.name === key + '_reverse' ||
                        e.name === key + '_replace';
                });

                direction = direction || 'forward';

                var isRel = sp.result(memberEntity, 'isOfType.0.eid.getNsAlias') === 'core:relationship';
                var isChoice = isRel && sp.result(memberEntity, 'toType.isOfType.0.eid.getNsAlias') === 'core:enumType';

                var cardinality = (sp.result(memberEntity, 'cardinality.alias') || 'ManyToMany').toLowerCase();
                var isLookup = isRel && (
                    direction === 'forward' && cardinality.indexOf('toone') >= 0 ||
                    direction === 'reverse' && cardinality.indexOf('oneto') >= 0
                    );

                console.log('updateArgGroup for %s %o %s => rel=%s choice=%s lookup=%s',
                    key, spEntity.toJSON(memberEntity), direction, isRel, isChoice, isLookup);

                if (isChoice) {
                    // single value for now
                    spWorkflow.addSetRelValueArg($scope.workflow, $scope.entity, key);

                } else if (isLookup) {
                    spWorkflow.addActivityArgument($scope.workflow, $scope.entity,
                        aliases.inputArguments, aliases.boolArgument, key + '_reverse');

                    spWorkflow.updateParameterExpressionByName($scope.workflow, $scope.entity,
                        aliases.inputArguments, key + '_reverse', direction === 'reverse' ? 'true' : 'false');

                    spWorkflow.addSetRelValueArg($scope.workflow, $scope.entity, key);

                } else if (isRel) {
                    spWorkflow.addActivityArgument($scope.workflow, $scope.entity,
                        aliases.inputArguments, aliases.boolArgument, key + '_reverse');

                    spWorkflow.updateParameterExpressionByName($scope.workflow, $scope.entity,
                        aliases.inputArguments, key + '_reverse', direction === 'reverse' ? 'true' : 'false');

                    spWorkflow.addActivityArgument($scope.workflow, $scope.entity,
                        aliases.inputArguments, aliases.boolArgument, key + '_replace');

                    // single value for now
                    spWorkflow.addSetRelValueArg($scope.workflow, $scope.entity, key);

                } else {
                    spWorkflow.addActivityArgument($scope.workflow, $scope.entity,
                        aliases.inputArguments, aliases.objectArgument, key + '_value');
                }
            }

            function memberChosen(resource, argId) {
                console.log('memberChosen', resource, argId);

                var arg = spEntity.findByEid($scope.entity.inputArguments, argId);
                console.assert(arg);

                getMemberEntityForArg(arg).then(function (memberEntity) {
                    updateArgGroup(arg.name, memberEntity, resource.isReverse ? 'reverse' : 'forward');
                });
            }

            function getOutputArgInstance() {
                if ($scope.entity.type.nsAlias === 'core:createActivity') {
                    return spWorkflow.findWorkflowExpressionParameter($scope.workflow, $scope.entity, 'core:createActivityCreatedResource');
                } else if ($scope.entity.type.nsAlias === 'core:cloneActivity') {
                    return spWorkflow.findWorkflowExpressionParameter($scope.workflow, $scope.entity, 'core:clonedResourceArgument');
                } else {
                    console.error('Unexpected type');
                }
            }

            function resourceTypePicked(resource) {

                console.log('onResourceTypePicked', resource);

                var argInstance = getOutputArgInstance();

                if (argInstance) {
                    argInstance.instanceConformsToType = spEntity.fromJSON({ id: resource.id, name: resource.name });
                    spWorkflowService.workflowUpdated($scope.workflow);
                }
            }

            function getResourceType(parameter) {
                // the following is used in choosers to constrain the selection when choosing entities
                // - if the member arg of a group then constrain by the resource type we are creating
                // - otherwise if the member is a rel then use the toType or fromType
                // - otherwise we don't care

                if (isMemberArg(parameter.argument)) {
                    var newDefinitionCloneArgument = $scope.activityParameters['core:newDefinitionCloneArgument'];
                    if (newDefinitionCloneArgument && newDefinitionCloneArgument.expression) {
                        var typeParameter = spWorkflow.getAsSingleKnownEntity($scope.workflow, newDefinitionCloneArgument.expression);
                        if (typeParameter) {
                            return typeParameter.idP;
                        }
                    }

                    return $scope.resourceType;
                }

                var g = getGroup(parameter.argument);
                if (!g || g.memberType === 'field') return null;

                var revArg = getGroupArg(g, 'reverse');
                var rev = revArg && sp.stringToBoolean(sp.result($scope.activityParameters[revArg.id()], 'expression.expressionString'), 'false');

                return sp.result(getGroup(parameter.argument).memberEntity, rev ? 'fromType.id' : 'toType.id') || '';
            }

            function openSingleKnownEntityChooser(parameter, action) {

                return spWorkflowEditorViewService.openSingleKnownEntityChooser({
                    chooserType: action.name,
                    workflow: $scope.workflow,
                    activity: $scope.entity,
                    parameter: parameter,
                    resourceType: getResourceType(parameter),
                    apply: function (selected) {
                        if (isResourceTypeArg(parameter.argument)) {
                            resourceTypePicked(selected);
                        } else if (isMemberArg(parameter.argument)) {
                            memberChosen(selected, parameter.argument.aliasOrId());
                        }
                    }
                });
            }

            function openSingleParameterChooser(parameter, action) {

                return spWorkflowEditorViewService.openSingleParameterChooser({
                    workflow: $scope.workflow,
                    activity: $scope.entity,
                    parameter: parameter,
                    resourceType: getResourceType(parameter)
                });
            }

            function openExprEditor(parameter, action) {

                return spWorkflowEditorViewService.openExprEditor({
                    workflow: $scope.workflow,
                    activity: $scope.entity,
                    parameter: parameter,
                    resourceType: getResourceType(parameter)
                });
            }

            function workflowUpdated() {

                console.group('updateEntityActivityController: workflow updated');
                console.time('workflowUpdated');

                if ($scope.activityParameters !== $scope.workflow.activities[$scope.entity.idP].parameters) {
                    //debugger;
                }

                // gather data on the arguments

                $scope.groups = _($scope.entity.inputArguments)
                    .map(function (e) {
                        var index = parseInt(_.first(e.name.split('_')), 10);
                        return !_.isNaN(index) ? { key: index.toString() } : null;
                    })
                    .compact()
                    .uniqBy('key')
                    .sortBy('key')
                    .value();

//                console.log('groups', $scope.groups);

                _.forEach($scope.groups, function (group) {

                    var arg = getGroupArg(group);
                    console.assert(arg);

                    getMemberEntityForArg(arg).then(function (memberEntity) {
                        group.memberEntity = memberEntity;
                        group.memberType = getMemberTypeForGroup(group);
                        //console.log('set memberEntity', group.key, spEntity.toJSON(memberEntity), group.memberType);
                    });

                });

                // capture the resource type for use in member choosers
                // - for createActivity it is based on the type of the output (but could be on its input)
                // - for updateActivity it is based on the chosen resource
                // - for cloneActivity it is the same as the source unless an target definition is specified
                // and do some activity dependent action overrides
                // TODO: Refactor out the switch
                if ($scope.entity.type.nsAlias === 'core:createActivity') {

                    $scope.resourceType = sp.result(getOutputArgInstance(), 'instanceConformsToType.id');

                    $scope.activityParameters['core:createActivityResourceArgument'].actions.typeChooser.action = openSingleKnownEntityChooser;

                } else if ($scope.entity.type.nsAlias === 'core:updateFieldActivity') {

                    spWorkflowService.getExpressionCompileResult($scope.workflow, $scope.entity, 'core:updateFieldActivityResourceArgument', spWorkflowConfiguration.aliases.inputArguments)
                        .then(function (result) {
                            $scope.resourceType = result && result.entityTypeId;
                        });

                    $scope.activityParameters['core:updateFieldActivityResourceArgument'].actions.resourceChooser.action = openSingleKnownEntityChooser;

                } else if ($scope.entity.type.nsAlias === 'core:cloneActivity') {

                    spWorkflowService.getExpressionCompileResult($scope.workflow, $scope.entity, 'core:resourceToCloneArgument', spWorkflowConfiguration.aliases.inputArguments)
                        .then(function (result) {
                            $scope.resourceType = result && result.entityTypeId;
                    });

                    $scope.activityParameters['core:resourceToCloneArgument'].actions.resourceChooser.action = openSingleKnownEntityChooser;

                } else {
                    console.error('unexpected activity type', $scope.entity.debugString, $scope.entity.getType());
                }

                console.log('updateEntityActivityController: resourceType', $scope.resourceType);

                // override the default action handlers

                _.forEach($scope.groups, function (g) {

                    var id = getGroupArg(g).idP;
                    var actions = $scope.activityParameters[id].actions;

                    actions.fieldChooser.action = openSingleKnownEntityChooser;
                    actions.relChooser.action = openSingleKnownEntityChooser;
                    actions.exprEditor.action = openExprEditor;

                    _.forEach(getGroupArgs(g, 'value'), function (a) {
                        if (a.idP && $scope.activityParameters[a.idP]) {
                            var actions = $scope.activityParameters[a.idP].actions;
                            if (g.memberType === 'field') {
                                actions.parameterChooser.action = openSingleParameterChooser;
                                actions.exprEditor.action = openExprEditor;
                            } else {
                                actions.resourceChooser.action = openSingleKnownEntityChooser;
                                actions.parameterChooser.action = openSingleParameterChooser;
                                actions.exprEditor.action = openExprEditor;

                            }
                        }
                    });
                });

                console.timeEnd('workflowUpdated');
                console.groupEnd();
            }

            $scope.addMemberArg = _.partial(spWorkflow.addSetMemberArg, $scope.workflow, $scope.entity);
            $scope.addRelValueArg = _.partial(spWorkflow.addSetRelValueArg, $scope.workflow, $scope.entity);
            $scope.getGroupArg = getGroupArg;
            $scope.getGroupArgs = getGroupArgs;
            $scope.removeGroup = removeGroup;

            // trigger the following on the activityParameters to ensure we are refreshed after they have
            // been rebuilt, and this is effectively after any workflow update.
            $scope.$watch('activityParameters', function () {
                if ($scope.activityParameters) {
                    workflowUpdated();
                }
            });
        });

}());
