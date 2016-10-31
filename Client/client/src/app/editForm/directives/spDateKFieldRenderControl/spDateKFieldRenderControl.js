// Copyright 2011-2016 Global Software Innovation Pty Ltd
/*global console, _, angular */

(function () {
    'use strict';

    /////
    // The spDateKFieldRenderControl directive provides a bridge between
    // the edit form scope and the isolated render control, mapping the
    // relevant properties from one to the other.
    /////
    angular.module('mod.app.editForm.designerDirectives.spDateKFieldRenderControl', ['mod.app.editForm', 'mod.app.editForm.spFieldControlProvider', 'sp.common.fieldValidator', 'mod.common.spCachingCompile'])
        .directive('spDateKFieldRenderControl', function (spEditForm, spFieldControlProvider, spFieldValidator, spCachingCompile) {

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
                        disableControl: $scope.isInDesign,
                        isInTestMode: $scope.isInTestMode,
                        isValidValue: true,
                        supressErrMsgs: true,
                        isInDesign: $scope.isInDesign,
                        isInlineEditing: $scope.isInlineEditing
                    };

                    var internalUpdate = false;


                    /////
                    // Watch for changes to the form control.
                    /////
                    $scope.$watch("formControl", function () {
                        var fieldToRender;

                        if ($scope.formControl) {

                            fieldToRender = $scope.formControl.getFieldToRender();

                            if (fieldToRender.getMinDate) {
                                $scope.model.minimumValue = fieldToRender.getMinDate();
                            }

                            if (fieldToRender.getMaxDate) {
                                $scope.model.maximumValue = fieldToRender.getMaxDate();
                            }

                            if (fieldToRender.getIsRequired) {
                                $scope.model.isRequired = fieldToRender.getIsRequired();
                            }

                            spFieldControlProvider($scope);
                            
                            /////
                            // When the form data changes, update the model.
                            /////
                            var dataWatch = 'formData && formData.getField(' + fieldToRender.id() + ')';

                            $scope.$watch(dataWatch, function (existingValue) {
                                
                                if (!$scope.model.value && !existingValue)
                                    return;
                                
                                internalUpdate = true;
                                
                                $scope.model.value = spUtils.translateToLocal(existingValue);
                            });
                            

                            /////
                            // Watch the models value for updates.
                            /////
                            $scope.$watch("model.value", function (newValue, oldValue) {

                                if (_.isDate(newValue) && _.isDate(oldValue) && newValue.getTime() === oldValue.getTime())
                                    return;
                                
                                if ($scope.formData) {
                                    
                                    // validate
                                    var msgs = [];
                                    validate(newValue, msgs);
                                    if (msgs.length === 0 || !newValue) {
                                        if (newValue || newValue === 0) {
                                            newValue = spUtils.translateToUtc(spUtils.parseDate(newValue));         // Dates are not in local time - always UTC, this prevents a date flipping if you change timezone.
                                            $scope.formData.setField(fieldToRender.id(), newValue, spEntity.DataType.Date);
                                        } else {
                                            $scope.formData.setField(fieldToRender.id(), null, spEntity.DataType.Date);
                                        }
                                    }
                                }
                            }, true);       // NOTE! using value test as watching on a date does not work well.

                        }
                    });

                    // runs custom validations
                    function validate(value, msgs) {
                        if ($scope.model.customValidator) {
                            $scope.model.customValidator(value, msgs);
                        }
                    }

                    $scope.$on('gather', function (event, callback) {
                        callback($scope.formControl, $scope.parentControl, element);
                    });

                    if (spInlineEditForm) {
                        $scope.$watch('customValidationMessages.length',
                            function() {
                                // Note - should generalise this somewhat, but at present adding in special
                                // for inline editing forms. And to make it somewhat more efficient etc.
                                spInlineEditForm.setValidationMessages($scope.formControl, $scope.customValidationMessages);
                            });
                    }

                    var cachedLinkFunc = spCachingCompile.compile('editForm/directives/spDateKFieldRenderControl/spDateKFieldRenderControl.tpl.html');
                    cachedLinkFunc($scope, function (clone) {
                        element.append(clone);
                    });
                }
            };
        });
}());