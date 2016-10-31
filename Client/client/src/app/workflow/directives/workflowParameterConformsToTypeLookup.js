// Copyright 2011-2016 Global Software Innovation Pty Ltd
/*global _, console, angular, spWorkflowConfiguration, spWorkflow, sp, spEntity */

(function () {
    'use strict';

    angular.module('sp.workflow.directives.parameterConformsToTypeLookup', ['sp.workflow.directives.propertiesForm'])
        .directive('spWorkflowParameterConformsToTypeLookup', function (spWorkflowService, spWorkflowEditorViewService) {

            return {
                restrict: 'E',
                replace: true,
                template: '<span class="workflow-expression-control">' +
                    '  <label class="control-label">{{label || argInstance.name}}</label>' +
                    '  <input class="form-control" readonly="true" ng-click="open()" ng-model="argInstance.instanceConformsToType.name" />' +
                    '  <sp-parameter-expression-context-menu tools="typeChooser"></sp-parameter-expression-context-menu>' +
                    '</span>',
                scope: {
                    label: '@',
                    aliasOrId: '=parameter',
                    onChanged: '&changed'
                },
                require: '^spWorkflowPropertiesForm',
                link: function (scope, element, attrs, propsFormController) {

                    scope.open = function (actionName, parameter) {

                        spWorkflowEditorViewService.chooseResource(null, sp.result(scope.argInstance, 'id'), 'typeChooser').then(function (resource) {
                            scope.argInstance.instanceConformsToType = spEntity.fromJSON({ id: resource.id, name: resource.name });
                            spWorkflowService.workflowUpdated(scope.workflow);
                            if (scope.onChanged) {
                                scope.onChanged({parameter: scope.aliasOrId, typeEntity: scope.argInstance.instanceConformsToType});
                            }
                        });

                    };

                    scope.workflow = propsFormController.getWorkflow();
                    scope.activity = propsFormController.getEntity();
                    scope.argInstance = spWorkflow.findWorkflowExpressionParameter(scope.workflow, scope.activity, scope.aliasOrId);
                }
            };
        });
}());