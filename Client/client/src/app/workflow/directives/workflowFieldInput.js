// Copyright 2011-2016 Global Software Innovation Pty Ltd
/*global _, console, angular, spWorkflowConfiguration, spWorkflow, sp, spEntity */

(function () {
    'use strict';

    angular.module('sp.workflow.directives.fieldInput', ['sp.common.directives'])
        .directive('spWorkflowFieldInput', function () {
            return {
                restrict: 'E',
                replace: true,
                template: '<div><sp-click-to-edit model="value" placeholder="{{placeholder}}" edit-mode="editMode"></sp-click-to-edit></div>',
                scope: {
                    entity: '=',
                    fieldIdOrAlias: '@field',
                    onChanged: '&changed',
                    placeholder: '@'
                },
                controller: function ($scope) {

                    function updateViewFromModel() {
                        $scope.value = $scope.entity && $scope.entity.getField($scope.fieldIdOrAlias);
                        $scope.editMode = false;
                    }

                    $scope.$watch('entity', updateViewFromModel);
                    $scope.$watch('entity.getField(scope.fieldIdOrAlias)', updateViewFromModel);

                    $scope.$watch('value', function (value) {
                        if ($scope.entity) {
                            var oldValue = $scope.entity.getField($scope.fieldIdOrAlias);
                            if (value !== oldValue) {
                                $scope.entity.setField($scope.fieldIdOrAlias, value);
                                if ($scope.onChanged) {
                                    $scope.onChanged($scope.entity, $scope.fieldIdOrAlias);
                                }
                            }
                        }
                    });
                }
            };
        });

}());