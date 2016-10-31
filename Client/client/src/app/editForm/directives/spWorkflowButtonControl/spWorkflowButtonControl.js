// Copyright 2011-2016 Global Software Innovation Pty Ltd
/*global console, _, angular */

(function() {
    'use strict';

    /////
    // The spWorkflowButtonControl directive provides a bridge between
    // the edit form scope and the isolated render control, mapping the
    // relevant properties from one to the other.
    /////
    angular.module('mod.app.editForm.designerDirectives.spWorkflowButtonControl', ['mod.app.editForm', 'mod.common.spUserTask', 'mod.common.spCachingCompile'])
        .directive('spWorkflowButtonControl', function ($rootScope, spEditForm, spUserTask, spCachingCompile) {

            function link(scope, element) {
                scope.workflowId = scope.formControl.getWbcWorkflowToRun().id();

                /////
                // Watch for changes to the form control.
                /////
                scope.$watch("formControl", function() {
                    var fieldToRender;

                    if (scope.formControl) {


                    }
                });

                scope.onPaused = function(workflowRunId) {
                    spUserTask.navigateToFollowOnTasks(workflowRunId, true, true);
                };


                var cachedLinkFunc = spCachingCompile.compile('editForm/directives/spWorkflowButtonControl/spWorkflowButtonControl.tpl.html');
                cachedLinkFunc(scope, function (clone) {
                    element.append(clone);
                });
            }

            /////
            // Directive structure.
            /////
            return {
                restrict: 'AE',
                replace: false,
                transclude: false,
                scope: {
                    formControl: '=?',
                    isInTestMode: '=?',
                    isReadOnly: '=?'
                },
                link: link
            };
        });
}());