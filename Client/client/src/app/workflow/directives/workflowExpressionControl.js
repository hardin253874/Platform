// Copyright 2011-2016 Global Software Innovation Pty Ltd
/*global _, console, angular, spWorkflowConfiguration, spWorkflow, sp, spEntity */

(function () {
    'use strict';

    angular.module('sp.workflow.directives.expressionControl', ['sp.common.ui.expressionEditor', 'mod.app.editForm.designerDirectives.spCustomValidationMessage'])
        .directive('spWorkflowExpressionControl', function () {
            return {
                restrict: 'E',
                replace: true,
                scope: {
                    parameter: '=',
                    showLabel: '=',
                    actions: '@'
                },
                templateUrl: 'workflow/directives/workflowExpressionControl.tpl.html',
                link: function (scope, elem, attrs) {

                    function update() {
                        if (scope.parameter && scope.actions) {
                            var names = scope.actions.split(',').map(function (s) {
                                return s.trim();
                            });
                            var toolNames = _.intersection(_.keys(scope.parameter.actions), names);
                            var labelMap = scope.parameter.actionLabelMap;

                            scope.tools = toolNames.join(',');
                            scope.toolLabels = (labelMap && _.map(toolNames, function (name) {return labelMap[name] || '';}) || []).join(',');

                            scope.errors = _.compact([sp.result(scope.parameter, 'compileResult.error')].concat(scope.parameter.errors));

                            // the expression controls may need to be refreshed if changes were made when not visible
                            scope.$broadcast('sp.app.ui-refresh');
                        }

                        scope.parameterType = sp.result(scope.parameter, 'argument.isOfType.0.name') || sp.result(scope.parameter, 'argument.isOfType.0.nsAlias');

                    }

                    scope.open = function (actionName, argId) {
                        console.log('spWorkflowExpressionControl.open', actionName, argId, scope.parameter.actions);
                        var action = scope.parameter.actions[actionName];
                        action.action(scope.parameter, action);
                    };

                    scope.$watch('parameter', update);
                    scope.$watch('actions', update);
                }
            };
        });

}());