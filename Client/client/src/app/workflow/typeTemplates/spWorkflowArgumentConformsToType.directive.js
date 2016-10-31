// Copyright 2011-2016 Global Software Innovation Pty Ltd
/*global _, console, angular, sp, spWorkflowConfiguration, spEntity, spWorkflow, jsonString */

(function () {
    'use strict';

    angular.module('sp.workflow.activities.workflow')
        .directive('spWorkflowArgumentConformsToType', function () {
            return {
                restrict: 'E',
                replace: true,
                template: '<span ng-show="isResourceArgument(argEntity)">' +
                    '  <input type="text" class="form-control" ng-model="argEntity.conformsToType.name"' +
                    '         title="optionally choose a Definition for this argument" placeholder="base definition"' +
                    '         ng-readonly="true" ng-click="showConformsToTypePicker(argEntity)" />' +
                    '  <span class="btn-icon" ng-click="showConformsToTypePicker(argEntity)">' +
                    '      <img src="assets/images/icon_picker_open.png" />' +
                    '  </span>' +
                    '</span>'
            };
        });

}());