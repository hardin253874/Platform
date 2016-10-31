// Copyright 2011-2016 Global Software Innovation Pty Ltd
/*global console, _, angular */

(function() {
    'use strict';

    /////
    // The spAutoNumberFieldRenderControl directive provides a bridge between
    // the edit form scope and the isolated render control, mapping the
    // relevant properties from one to the other.
    /////
    angular.module('mod.app.editForm.designerDirectives.spAutoNumberFieldRenderControl', ['mod.app.editForm', 'mod.app.editForm.spFieldControlProvider', 'mod.common.spCachingCompile'])
        .directive('spAutoNumberFieldRenderControl', function (spEditForm, spFieldControlProvider, spCachingCompile) {

            /////
            // Directive structure.
            /////
            return {
                restrict: 'AE',
                replace: false,
                transclude: false,
                scope: {
                    formControl: '=?',
                    parentControl: '=?',
                    formData: '=?',
                    formMode: '=?',
                    isInTestMode: '=?',
                    isInDesign: '=?',
                    isInlineEditing: '=?'
                },
                link: function ($scope, element) {
                    /////
                    // Convert the current edit form scope values into a generic model with
                    // no ties to edit form.
                    /////
                    $scope.model = {
                        isReadOnly: true,
                        isInTestMode: $scope.isInTestMode,
                        isInDesign: $scope.isInDesign,
                        isInlineEditing: $scope.isInlineEditing
                    };


                    var pattern = null;
                    
                    /////
                    // Watch for changes to the form control.
                    /////
                    $scope.$watch("formControl", function() {
                        var fieldToRender;

                        if ($scope.formControl) {

                            fieldToRender = $scope.formControl.getFieldToRender();

                            spFieldControlProvider($scope);

                            $scope.model.isReadOnly = true;
                            //pattern = fieldToRender.getAutoNumberDisplayPattern();
                            
                            if ($scope.isInTestMode) {
                                $scope.testId = spEditForm.cleanTestId(_.result(fieldToRender, 'getName'));
                            }

                            pattern = fieldToRender.autoNumberDisplayPattern;
                            

                            /////
                            // When the form data changes, update the model.
                            /////
                            var dataWatch = 'formData && formData.getField(' + fieldToRender.id() + ')';
                            
                            $scope.$watch(dataWatch, function (value) {
                                    $scope.model.formattedValue = value && pattern ? '' + jQuery.formatNumber(value, { format: pattern, locale: "us" }) : value;
                            });

                           
                        }
                    });

                    $scope.$on('gather', function (event, callback) {
                        callback($scope.formControl, $scope.parentControl, element);
                    });

                    var cachedLinkFunc = spCachingCompile.compile('editForm/directives/spAutoNumberFieldRenderControl/spAutoNumberFieldRenderControl.tpl.html');
                    cachedLinkFunc($scope, function (clone) {
                        element.append(clone);
                    });
                }
            };
        });
}());