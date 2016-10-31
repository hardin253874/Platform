// Copyright 2011-2016 Global Software Innovation Pty Ltd
/*global angular, _, console, spWorkflowConfiguration, spWorkflow, spEntity */

angular.module('sp.workflow.activities')
    .controller('switchActivityController', function ($scope, spWorkflowService, spWorkflowEditorViewService) {
        "use strict";

        //todo - consolidate this code ... have copied from the parameter directive
        var scope = $scope;
        scope.activity = scope.entity;
        scope.parameterAlias = 'core:switchActivityDecisionArgument';
        scope.parameterRelAlias = spWorkflowConfiguration.aliases.inputArguments;

        function updateParameterHints() {
            scope.parameterHints = _.map(spWorkflow.getWorkflowExpressionParameters(scope.workflow), function (p) {
                //console.log('updateParameterHints: workflow parameter hint ', p);
                return { name: p.name, description: p.description, typeName: 'Entity', entityTypeId: p.conformsToType || 'core:resource' };
            });

            var parameter = _.first(spWorkflow.getActivityParameters(scope.workflow, scope.activity, scope.parameterRelAlias, { aliasOrId: scope.parameterAlias }));
            if (!parameter) {
                console.warn('spWorkflowParameterExpression: cannot find parameter %o on activity %o', scope.parameterAlias, scope.activity.debugString);
            }
            var expression = parameter && parameter.expression;
            //console.log('spWorkflowParameterExpression: expression=', expression);

            if (expression) {
                scope.parameterHints = scope.parameterHints.concat(_.map(expression.getWfExpressionKnownEntities(), function (e) {
                    //console.log('updateParameterHints: expression known entity parameter hint ', e);
                    return { name: e.name, description: e.description, typeName: 'Entity', entityTypeId: _.first(e.isOfType).idP || 'core:resource' };
                }));
            }
        }

        function updateParameterViewModel() {
            scope.parameter = spWorkflow.getParameterAndExpression(scope.workflow, scope.activity,
                scope.parameterRelAlias || spWorkflowConfiguration.aliases.inputArguments, scope.parameterAlias);
        }
        // end consolidate


        $scope.addExit = function () {
            $scope.entity.exitPoints.add(spEntity.fromJSON({
                typeId: spWorkflowConfiguration.aliases.exitPoint,
                name: spWorkflow.getUniqueName(_.map($scope.entity.exitPoints, 'name'), 'exit'),
                description: '',
                isDefaultExitPoint: false,
                exitPointOrdinal: 1 + (Math.max.apply(null, [0].concat(_.compact(_.map($scope.entity.exitPoints, 'exitPointOrdinal')))))
            }));
            spWorkflowService.activityUpdated($scope.workflow, $scope.entity);
        };

        $scope.removeExit = function (exit) {
            $scope.entity.exitPoints.remove(exit);
            spWorkflowService.activityUpdated($scope.workflow, $scope.entity);
        };

        $scope.exitPointDescriptions = function () {
            return _.map($scope.entity.exitPoints, 'description');
        };

        $scope.$watch('exitPointDescriptions()', function () {
            var switchExpression = '';

            _.forEach($scope.entity.exitPoints, function (e) {
                var condition = e.description && e.description.indexOf('condition:') === 0 && e.description.substring(10);
                $scope.conditionExpressions[e.id()] = condition || '';
                switchExpression += 'iif(' + (condition || false) + ', \'' + e.name + '\', ';
            });
            switchExpression += '\'otherwise\')' + new Array($scope.entity.exitPoints.length).join(')');

            spWorkflow.updateParameterExpression($scope.workflow, $scope.activity,
                $scope.parameterRelAlias, $scope.parameterAlias, switchExpression, false);

            $scope.debug = switchExpression;
            $scope.entity.description = switchExpression; // debug todo todo remove this

        }, true);

        $scope.$watchCollection('conditionExpressions', function () {
            _.forEach($scope.entity.exitPoints, function (e) {
                e.description = 'condition:' + $scope.conditionExpressions[e.id()];
            });
        });

        $scope.open = function (actionName, id) {
            // make a parameter with the needed bits to make the expression editor work
            // and disable the in-expression resource chooser, at least until we handle merging
            // known entities in the combined expression... probably will never happen :0
            var parameter = {
                expression: {
                    expressionString: $scope.conditionExpressions[id],
                    wfExpressionKnownEntities: []
                },
                disableResourceChooser: true,
                exprParamHints: $scope.parameterHints
            };
            spWorkflowEditorViewService.openExprEditor({
                workflow: $scope.workflow,
                activity: $scope.entity,
                parameter: parameter
            }).then (function (){
                $scope.conditionExpressions[id] = parameter.expression.expressionString;
            });
        };

        $scope.parameterHints = [];
        $scope.conditionExpressions = {};

        updateParameterHints();
        updateParameterViewModel();

    });
