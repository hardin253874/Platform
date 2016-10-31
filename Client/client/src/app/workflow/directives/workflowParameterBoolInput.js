// Copyright 2011-2016 Global Software Innovation Pty Ltd
/*global _, console, angular, spWorkflowConfiguration, spWorkflow, sp, spEntity */

(function () {
    'use strict';

    angular.module('sp.workflow.directives.parameterBoolInput', ['sp.workflow.directives.propertiesForm'])
        .directive('spWorkflowParameterBoolInput', function () {
            return {
                restrict: 'E',
                replace: true,
                template: '<div class="checkbox">' +
                    '  <label title="{{parameter.description}}"><input type="checkbox" ng-model="parameter.boolValue" ' +
                    '       ng-readonly="{{readonly}}" ng-change="onChange()" />' +
                    '  {{label || parameter.name}}</label>' +
                    '</div>',
                scope: {
                    label: '@',
                    aliasOrId: '=parameter',
                    onChanged: '&changed',
                    readonly: '@'
                },
                require: '^spWorkflowPropertiesForm',
                link: function (scope, element, attrs, propsFormController) {

                    scope.onChange = function () {
                        scope.parameter.text = scope.parameter.boolValue.toString();
                    };

                    scope.stringToBoolean = sp.stringToBoolean;

                    scope.$watch('parameter.text', function (text) {
                        if (!_.isUndefined(text)) {
                            spWorkflow.updateParameterExpression(scope.workflow, scope.activity,
                                spWorkflowConfiguration.aliases.inputArguments, scope.aliasOrId, text, false);

                            scope.parameter.boolValue = sp.stringToBoolean(text);
                        }

                    });

                    scope.cancelAliasOrIdWatch =
                        scope.$watch('aliasOrId', function () {
                            if (scope.aliasOrId) {
                                scope.cancelAliasOrIdWatch();

                                scope.parameter = spWorkflow.getParameterAndExpression(scope.workflow, scope.activity,
                                    spWorkflowConfiguration.aliases.inputArguments, scope.aliasOrId);
                            }
                        });

                    scope.workflow = propsFormController.getWorkflow();
                    scope.activity = propsFormController.getEntity();
                    scope.parameter = null;
                }
            };
        });
}());