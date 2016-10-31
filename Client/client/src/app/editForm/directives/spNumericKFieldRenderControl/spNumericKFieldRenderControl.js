// Copyright 2011-2016 Global Software Innovation Pty Ltd
/*global console, _, angular */

(function() {
    'use strict';

    /////
    // The spNumericKFieldRenderControl directive provides a bridge between
    // the edit form scope and the isolated render control, mapping the
    // relevant properties from one to the other.
    /////
    angular.module('mod.app.editForm.designerDirectives.spNumericKFieldRenderControl', ['mod.app.editForm', 'mod.app.editForm.spFieldControlProvider', 'mod.common.spCachingCompile', 'mod.common.spMobile'])
        .directive('spNumericKFieldRenderControl', function (spEditForm, spFieldControlProvider, spCachingCompile, spMobileContext) {

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
                require: '^^?spInlineEditForm',
                link: function ($scope, element, attrs, spInlineEditForm) {
                    /////
                    // Convert the current edit form scope values into a generic model with
                    // no ties to edit form.
                    /////
                    $scope.model = {
                        isReadOnly: $scope.isReadOnly,
                        isInDesign: $scope.isInDesign,
                        isInTestMode: $scope.isInTestMode,
                        isInlineEditing: $scope.isInlineEditing
                    };

                    if (spInlineEditForm) {
                        $scope.$watch('customValidationMessages.length', function () {
                            // Note - should generalise this somewhat, but at present adding in special
                            // for inline editing forms. And to make it somewhat more efficient etc.
                            spInlineEditForm.setValidationMessages($scope.formControl, $scope.customValidationMessages);
                        });
                    }

                    /////
                    // Watch for changes to the form control.
                    /////
                    $scope.$watch("formControl", function() {
                        var fieldToRender;

                        if ($scope.formControl) {

                            fieldToRender = $scope.formControl.getFieldToRender();

                            if (fieldToRender.getMinInt) {
                                $scope.model.minimumValue = fieldToRender.getMinInt();
                            }

                            if (fieldToRender.getMaxInt) {
                                $scope.model.maximumValue = fieldToRender.getMaxInt();
                            }

                            spFieldControlProvider($scope);
                            

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
                                
                                if (oldValue === newValue || (spMobileContext.isMobile && newValue === undefined))
                                    return;

                                if ($scope.formData) {

                                    if (newValue || newValue === 0) {
                                        newValue = parseFloat(newValue);
                                        $scope.formData.setField(fieldToRender.eid(), newValue, spEntity.DataType.Int32);
                                    } else {
                                        $scope.formData.setField(fieldToRender.eid(), null, spEntity.DataType.Int32);
                                    }
                                }
                            });

                        }
                    });

                    $scope.$on('gather', function (event, callback) {
                        callback($scope.formControl, $scope.parentControl, element);
                    });


                    var cachedLinkFunc = spCachingCompile.compile('editForm/directives/spNumericKFieldRenderControl/spNumericKFieldRenderControl.tpl.html');
                    cachedLinkFunc($scope, function (clone) {
                        element.append(clone);
                    });
                }
            };
        });
}());