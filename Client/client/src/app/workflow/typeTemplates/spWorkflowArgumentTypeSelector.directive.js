// Copyright 2011-2016 Global Software Innovation Pty Ltd
/*global _, console, angular, sp, spWorkflowConfiguration, spEntity, spWorkflow, jsonString */

(function () {
    'use strict';

    angular.module('sp.workflow.activities.workflow')
        .directive('spWorkflowArgumentTypeSelector', function () {
            return {
                restrict: 'E',
                replace: true,
                template: '<span>' +
                    '  <span ng-show="argTypes.length == 0">loading...</span>' +
                    '  <select class="form-control" ng-show="argTypes.length > 0" ng-model="argTypeMap[argEntity.id()]" ng-change="setArgType(argEntity)" ng-options="t.alias as t.name for t in argTypes"></select>' +
                    '</span>'
            };
        });

}());