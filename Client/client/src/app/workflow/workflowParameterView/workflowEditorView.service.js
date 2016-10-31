// Copyright 2011-2016 Global Software Innovation Pty Ltd
/*global _, console, angular, spWorkflowConfiguration, spWorkflow, sp, spEntity */

// TODO: have an issue with promises and the apply/cancel functions being callable multiple times
// TODO: don't use the name "apply" for the callback.... is confusing given function's apply function

(function () {
    'use strict';

    angular.module('sp.workflow.parameterViewServices')
        .factory('spWorkflowEditorViewService', spWorkflowEditorViewService);


    function spWorkflowEditorViewService($q, spViewRegionService, spWorkflowService) {

        // comments
        // this is a little odd... most of the routines are about editing a parameter
        // but some are for plain choosing... to capture that better

        var exports = {};

        // return a promise for a resource selected via a chooser UI
        function chooseResource(resourceType, currentSelections, chooserType) {

            var defer = $q.defer();

            currentSelections = currentSelections && (_.isArray(currentSelections) ? currentSelections : [currentSelections]) || [];
            chooserType = chooserType || 'resourceChooser';

            var view = {
                templateUrl: 'workflow/workflowParameterView/chooserView.tpl.html',
                viewName: chooserType,
                resourceType: resourceType,
                currentSelections: currentSelections,
                apply: function () {
                    var rq = 'name,description,isOfType.name';
                    if (chooserType === 'typeChooser')
                        rq = rq + ',inherits*.{id, alias}';
                    var selected = _.first(this.candidateSelections);
                    if (selected) {
                        spWorkflowService.getCacheableEntity('entity:' + selected.id, selected.id, rq)
                            .then(function (e) {
                                selected = _.extend({ name: e.name, entity: e }, selected);
                            })
                            .catch(function () {
                                console.error('error requesting entity for ', selected);
                            })
                            .finally(function () {
                                defer.resolve(selected);
                            });
                    } else {
                        defer.resolve(null);
                    }
                },
                cancel: function () {
                    this.currentSelections = currentSelections;
                },
                onPopped: function () {
                }
            };

            spViewRegionService.pushView('workflow-properties-sidepanel', view);

            return defer.promise;
        }

        exports.chooseResource = chooseResource;

        function openEntityChooser(context) {

            var workflow = context.workflow;
            var activity = context.activity || context.entity;
            var parameter = context.parameter;
            var resourceType = context.resourceType || parameter.resourceType;

            console.assert(workflow && activity && parameter && context.chooserType);

            var singleKnownEntity = spWorkflow.getAsSingleKnownEntity(workflow, parameter.expression);
            var currentSelections = singleKnownEntity ? [
                { id: singleKnownEntity.id(), name: singleKnownEntity.name }
            ] : [];

            //todo - switch this whole thing to using chooseResource, not just for resourceChoosers
            if (context.chooserType === 'resourceChooser') {
                return chooseResource(resourceType, currentSelections).then(function (selected) {
                    if (context.apply) {
                        context.apply(selected);
                    }
                    return selected && selected.name;
                });
            }

            var defer = $q.defer();

            var view = {
                templateUrl: 'workflow/workflowParameterView/chooserView.tpl.html',
                viewName: context.chooserType,
                workflow: workflow,
                activity: activity,
                resourceType: resourceType,
                currentSelections: currentSelections,
                apply: function () {
                    var selected = _.first(this.candidateSelections);
                    if (selected) {
                        // Need to get the entity with its type information as we need the type of the "known entity"
                        // when building parameter hint info for the expression editor.
                        spWorkflowService.getCacheableEntity('entity:' + selected.id, selected.id, 'name,description,isOfType.name')
                            .then(function (e) {
                                selected = _.extend({ name: e.name, entity: e }, selected);
                                if (context.apply) {
                                    context.apply(selected);
                                }
                            })
                            .catch(function () {
                                console.error('error requesting entity for ', selected);
                            })
                            .finally(function () {
                                defer.resolve(selected && selected.name);
                            });
                    } else {
                        defer.resolve(null);
                    }
                },
                cancel: function () {
                    this.currentSelections = currentSelections;
                },
                onPopped: function () {
                }
            };

            spViewRegionService.pushView('workflow-properties-sidepanel', view);

            return defer.promise;
        }

        exports.openEntityChooser = openEntityChooser;

        function openSingleKnownEntityChooser(context) {

            var callersApply = context.apply;
            context.apply = function (selected) {
                spWorkflow.setSingleKnownEntityExpression(this.workflow, this.activity, context.parameter.argument, selected.entity, selected.name);
                if (callersApply) {
                    callersApply(selected);
                }
            };

            return openEntityChooser(context);
        }

        exports.openSingleKnownEntityChooser = openSingleKnownEntityChooser;

        function openParameterChooser(context) {

            var workflow = context.workflow;
            var activity = context.activity || context.entity;
            var parameter = context.parameter;
            var resourceType = context.resourceType || parameter.resourceType;

            console.assert(workflow && activity && parameter);

            var defer = $q.defer();

            var currentSelections = _(spWorkflow.getWorkflowExpressionParameters(workflow))
                .filter(function (p) {
                    return parameter.expression.expressionString === '[' + p.name + ']';
                })
                .map(function (p) {
                    return _.pick(p, 'id', 'name');
                })
                .value();

            var view = {
                templateUrl: 'workflow/workflowParameterView/chooserView.tpl.html',
                viewName: 'parameterChooser',
                sourceFilter: context.sourceFilter,
                workflow: workflow,
                activity: activity,
                currentSelections: currentSelections,
                resourceType: resourceType,
                apply: function () {
                    var selected = _.first(this.candidateSelections);
                    if (selected) {
                        if (context.apply) {
                            context.apply(selected);
                        }
                    }
                    defer.resolve(selected && selected.name);
                },
                cancel: function () {
                    this.currentSelections = currentSelections;
                    defer.resolve(null);
                },
                onPopped: function () {
                }
            };

            spViewRegionService.pushView('workflow-properties-sidepanel', view);

            return defer.promise;
        }

        exports.openParameterChooser = openParameterChooser;

        function openSingleParameterChooser(context) {

            var callersApply = context.apply;
            context.apply = function (selected) {
                spWorkflow.updateExpressionForParameter(this.workflow, this.activity, context.parameter.argument, '[' + selected.name + ']');
                if (callersApply) {
                    callersApply(selected);
                }
            };
            context.sourceFilter = function (param) {
                var paramType = {
                    baseTypeAlias: param.argumentInstanceArgument.type.nsAlias,
                    entityTypeId: sp.result(param, 'instanceConformsToType.idP') || sp.result(param, 'argumentInstanceArgument.conformsToType.idP')
                };
                var targetType = {
                    baseTypeAlias: context.parameter.argument.type.nsAlias,
                    entityTypeId: sp.result(context.parameter.argument, 'conformsToType.idP')
                };

                //                    console.log('parameterChooser - to filter %o for target type %o', paramType, targetType);

                //todo - check entityTypeId compatibility
                var result =
                    targetType.baseTypeAlias === 'core:objectArgument' ||
                    (paramType.baseTypeAlias === 'core:resourceArgument' && targetType.baseTypeAlias === 'core:resourceListArgument') ||    // You can put a single item into a list argument
                    paramType.baseTypeAlias === targetType.baseTypeAlias;

                return result;
            };

            return openParameterChooser(context);
        }

        exports.openSingleParameterChooser = openSingleParameterChooser;

        function openFunctionChooser(context) {

            var defer = $q.defer();

            var view = {
                templateUrl: 'workflow/workflowParameterView/chooserView.tpl.html',
                viewName: 'functionChooser',
                apply: function () {
                    var selected = _.first(this.candidateSelections);
                    if (selected) {
                        defer.resolve(selected.name);
                    }
                },
                cancel: function () {
                    defer.resolve(null);
                },
                onPopped: function () {
                }
            };

            spViewRegionService.pushView('workflow-properties-sidepanel', view);

            return defer.promise;
        }

        exports.openFunctionChooser = openFunctionChooser;

        function openPropertyChooser(context) {

            var workflow = context.workflow;
            var activity = context.activity || context.entity;
            var resourceType = context.resourceType;

            var defer = $q.defer();

            var view = {
                templateUrl: 'workflow/workflowParameterView/chooserView.tpl.html',
                viewName: 'propertyChooser',
                workflow: workflow,
                activity: activity,
                resourceType: resourceType,
                currentSelections: [],
                apply: function () {
                    var selected = _.first(this.candidateSelections);
                    if (selected) {
                        defer.resolve(selected.name);
                    }
                },
                cancel: function () {
                    defer.resolve(null);
                },
                onPopped: function () {
                }
            };

            spViewRegionService.pushView('workflow-properties-sidepanel', view);

            return defer.promise;
        }

        exports.openPropertyChooser = openPropertyChooser;

        function openExprPropertyChooser(context, resourceTypeId) {

            return openPropertyChooser({
                workflow: context.workflow,
                activity: context.activity,
                parameter: context.parameter,
                resourceType: resourceTypeId
            });
        }

        function openExprResourceChooser(context) {

            return openEntityChooser(_.extend({}, context, {
                chooserType: 'resourceChooser',
                apply: function (selected) {
                    var parameterName = selected.name;
                    //todo - we need to deal with duplicates in name, whether same entity or different, and taking into account workflow params
                    this.parentView.knownEntitiesParameters = this.parentView.knownEntitiesParameters.concat({name: selected.name, entity: selected.entity});
                    this.parentView.params = this.parentView.params.concat({
                        name: parameterName, description: selected.entity.description,
                        typeName: 'Entity', entityTypeId: _.first(selected.entity.isOfType).id() || 'core:resource' });
                }
            }));
        }

        function openExprEditor(context) {

            var workflow = context.workflow;
            var activity = context.activity || context.entity;
            var parameter = context.parameter;
            var resourceType = context.resourceType || parameter.resourceType;

            var defer = $q.defer();

            // we are creating the object then extending it so we can have a ref to it within
            var exprView = {};

            var chooserContext = _.defaults({ resourceType: null, parentView: exprView }, context);
            var choosers = {
                functionChooser: {
                    label: 'fx',
                    iconUrl: null,
                    chooserFn: _.partial(openFunctionChooser, chooserContext)
                },
                propertyChooser: {
                    label: 'property',
                    iconUrl: null,
                    chooserFn: _.partial(openExprPropertyChooser, chooserContext)
                },
                parameterChooser: {
                    label: 'parameter',
                    iconUrl: null,
                    chooserFn: _.partial(openParameterChooser, chooserContext)
                }
            };
            if (!parameter.disableResourceChooser) {
                choosers.resourceChooser = {
                    label: 'record',
                    iconUrl: null,
                    chooserFn: _.partial(openExprResourceChooser, chooserContext)
                };
            }

            exprView = _.extend(exprView, {
                templateUrl: 'workflow/workflowParameterView/expressionEditorView.tpl.html',
                workflow: workflow,
                activity: activity,
                model: parameter.expression.expressionString,
                label: '',//parameter.argument.name,
                expressionType: parameter.expression.isTemplate ? 'template' : '',
                params: parameter.exprParamHints,
                knownEntitiesParameters: [],
                resourceType: resourceType,
                contextId: null,
                expressionInputOptions: {
                    useHintLinks: true,
                    choosers: choosers
                },
                apply: function () {
                    parameter.expression.expressionString = this.model;
                    _.forEach(this.knownEntitiesParameters, function (p) {
                        spWorkflow.addExpressionKnownEntity(parameter.expression, p.entity, p.name);
                    });
                    this.knownEntitiesParameters = [];

                    defer.resolve(this.model);
                },
                cancel: function () {
                    this.model = parameter.expression.expressionString;
                    this.params = parameter.exprParamHints;
                    this.knownEntitiesParameters = [];

                    defer.resolve(null);
                },
                onPopped: function () {
                }
            });

            spViewRegionService.pushView('workflow-properties-sidepanel', exprView);

            return defer.promise;
        }

        exports.openExprEditor = openExprEditor;

        // special case. see alex.
        function chooseReport(resourceType, currentSelections) {
            var defer = $q.defer();
            var view = {
                templateUrl: 'workflow/workflowParameterView/chooserView.tpl.html',
                viewName: 'reportChooser',
                resourceType: resourceType,
                currentSelections: currentSelections,
                apply: function () {
                    var selected = _.first(this.candidateSelections);
                    if (selected) {
                        spWorkflowService.getCacheableEntity('entity:' + selected.id, selected.id, 'name,description,isOfType.name')
                            .then(function (e) {
                                selected = _.extend({ name: e.name, entity: e }, selected);
                            })
                            .catch(function () {
                                console.error('error requesting entity for ', selected);
                            })
                            .finally(function () {
                                defer.resolve(selected);
                            });
                    } else {
                        defer.resolve(null);
                    }
                },
                cancel: function () { this.currentSelections = currentSelections; },
                onPopped: function () { }
            };
            spViewRegionService.pushView('workflow-properties-sidepanel', view);
            return defer.promise;
        }

        exports.chooseReport = chooseReport;

        return exports;
    }
})();