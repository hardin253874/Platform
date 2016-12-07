// Copyright 2011-2016 Global Software Innovation Pty Ltd
/*global console, _, angular */

(function() {
    'use strict';

    /////
    // The spCurrencyKFieldRenderControl directive provides a bridge between
    // the edit form scope and the isolated render control, mapping the
    // relevant properties from one to the other.
    /////
    angular.module('mod.app.editForm.designerDirectives.spCurrencyKFieldRenderControl', ['mod.app.editForm', 'mod.app.editForm.spFieldControlProvider', 'mod.common.spCachingCompile', 'mod.common.spMobile','mod.app.editForm.spDblclickToEdit'])
        .directive('spCurrencyKFieldRenderControl', function (spEditForm, spFieldControlProvider, spCachingCompile, $state, spMobileContext) {

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

                    function setShowPrefix() {
                        if ($scope.model) {
                            $scope.model.showPrefix = $state.current.name !== 'createForm';
                        }
                    }

                    /////
                    // Watch for changes to the form control.
                    /////
                    $scope.$watch("formControl", function() {
                        var fieldToRender;
                        var existingValue;


                        if ($scope.formControl) {
                            
                            fieldToRender = $scope.formControl.getFieldToRender();

                            if (fieldToRender.getMinDecimal) {
                                $scope.model.minimumValue = fieldToRender.getMinDecimal();
                            }

                            if (fieldToRender.getMaxDecimal) {
                                $scope.model.maximumValue = fieldToRender.getMaxDecimal();
                            }

                            if (fieldToRender.getDecimalPlaces) {
                                $scope.model.decimalPlaces = fieldToRender.getDecimalPlaces();
                            }

                            // $scope is passed twice here, once as a context and once as the scope to $watch etc
                            // It is done like this in preparation for moving to "components"
                            spFieldControlProvider($scope, $scope);

                            /////
                            // When the form data changes, update the model.
                            /////
                            var dataWatch = 'formData && formData.getField(' + fieldToRender.id() + ')';

                            $scope.$watch(dataWatch, function (existingValue) {
                                $scope.model.value = existingValue;
                                setShowPrefix();
                            });

                            /////
                            // Watch the models value for updates.
                            /////
                            $scope.$watch("model.value", function(newValue, oldValue) {
                                if (oldValue === newValue || (spMobileContext.isMobile && newValue === undefined))
                                    return;                               

                                if ($scope.formData) {

                                    if (newValue || newValue === 0) {
                                        newValue = parseFloat(newValue);
                                        $scope.formData.setField(fieldToRender.id(), newValue, spEntity.DataType.Currency);
                                    } else {
                                        $scope.formData.setField(fieldToRender.id(), null, spEntity.DataType.Currency);
                                    }
                                }
                            });

                        }
                    });

                    if (spInlineEditForm) {
                        $scope.$watch('customValidationMessages.length', function () {
                            // Note - should generalise this somewhat, but at present adding in special
                            // for inline editing forms. And to make it somewhat more efficient etc.
                            spInlineEditForm.setValidationMessages($scope.formControl, $scope.customValidationMessages);
                        });
                    }

                    $scope.$on('gather', function (event, callback) {
                        callback($scope.formControl, $scope.parentControl, element);
                    });

                    var cachedLinkFunc = spCachingCompile.compile('editForm/directives/spCurrencyKFieldRenderControl/spCurrencyKFieldRenderControl.tpl.html');
                    cachedLinkFunc($scope, function (clone) {
                        element.append(clone);
                    });
                }
            };
        });
}());