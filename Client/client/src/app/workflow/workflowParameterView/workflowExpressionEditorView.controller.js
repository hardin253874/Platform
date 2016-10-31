// Copyright 2011-2016 Global Software Innovation Pty Ltd
/*global _, console, angular, spWorkflowConfiguration, spWorkflow, sp, spEntity */

//TODO - simplify that results data structure - is a left over from the old report interface

(function () {
    'use strict';

    angular.module('sp.workflow.parameterViewControllers')
        .controller('workflowExpressionEditorViewController', workflowExpressionEditorViewController);
    
    function workflowExpressionEditorViewController($scope, spViewRegionService, spWorkflowService) {

        console.log('workflowExpressionEditorViewController ctor', $scope);

        $scope.$on("$destroy", function () {
            console.log('workflowExpressionEditorViewController destroyed');
        });

        $scope.$watch('view.isTopView', function (value, prev) {
            if (value) {
                $scope.$broadcast('sp.view.setDefaultFocus');
            }
        });

    }
}());