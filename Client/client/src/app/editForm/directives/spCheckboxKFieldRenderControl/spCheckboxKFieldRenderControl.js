// Copyright 2011-2016 Global Software Innovation Pty Ltd
/*global console, _, angular */

(function () {
    'use strict';

    /////
    // The spCheckboxKFieldRenderControl directive provides a bridge between
    // the edit form scope and the isolated render control, mapping the
    // relevant properties from one to the other.
    /////
    angular.module('mod.app.editForm.designerDirectives.spCheckboxKFieldRenderControl', ['mod.app.editForm', 'mod.app.editForm.spFieldControlProvider', 'mod.common.spCachingCompile', 'mod.app.editForm.spDblclickToEdit'])
        .directive('spCheckboxKFieldRenderControl', function (spEditForm, spFieldControlProvider, spCachingCompile) {

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
                    isReadOnly: '=?',
                    isInDesign: '=?',
                    isInlineEditing: '=?'
                },
                link: function ($scope, element) {
                    /////
                    // Convert the current edit form scope values into a generic model with
                    // no ties to edit form.
                    /////
                    $scope.model = {
                        isReadOnly: $scope.isReadOnly,
                        isInTestMode: $scope.isInTestMode,
                        isInDesign: $scope.isInDesign,
                        isInlineEditing: $scope.isInlineEditing
                    };


                    /////
                    // Watch for changes to the form control.
                    /////
                    $scope.$watch("formControl", function () {

                        var fieldToRender;

                        if ($scope.formControl) {

                            fieldToRender = $scope.formControl.getFieldToRender();

                            // $scope is passed twice here, once as a context and once as the scope to $watch etc
                            // It is done like this in preparation for moving to "components"
                            spFieldControlProvider($scope, $scope);

                            /////
                            // When the form data changes, update the model.
                            /////
                            var dataWatch = 'formData && formData.getField(' + fieldToRender.id() + ')';

                            $scope.$watch(dataWatch, function (existingValue) {
                                $scope.model.value = existingValue;
                            });

                            /////
                            // Watch the models value for updates.
                            /////
                            $scope.$watch("model.value", function (newValue, oldValue) {
                                newValue = !!newValue;
                                oldValue = !!oldValue;

                                if (oldValue === newValue)
                                    return;
                                
                                if ($scope.formData) {                                    
                                    $scope.formData.setField(fieldToRender.eidP, newValue, spEntityUtils.dataTypeForField(fieldToRender));
                                }
                            });
                        }
                    });

                    $scope.$on('gather', function (event, callback) {
                        callback($scope.formControl, $scope.parentControl, element);
                    });

                    var cachedLinkFunc = spCachingCompile.compile('editForm/directives/spCheckboxKFieldRenderControl/spCheckboxKFieldRenderControl.tpl.html');
                    cachedLinkFunc($scope, function (clone) {
                        element.append(clone);
                    });
                }
            };
        });
}());