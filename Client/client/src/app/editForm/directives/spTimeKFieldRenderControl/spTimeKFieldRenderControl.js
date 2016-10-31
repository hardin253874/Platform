// Copyright 2011-2016 Global Software Innovation Pty Ltd
/*global console, _, angular */

(function () {
    'use strict';

    /////
    // The spTimeKFieldRenderControl directive provides a bridge between
    // the edit form scope and the isolated render control, mapping the
    // relevant properties from one to the other.
    /////
    angular.module('mod.app.editForm.designerDirectives.spTimeKFieldRenderControl', ['mod.app.editForm', 'mod.app.editForm.spFieldControlProvider', 'mod.app.editForm.designerDirectives.spTitlePlusMarkers', 'sp.common.fieldValidator', 'mod.common.spCachingCompile'])
        .directive('spTimeKFieldRenderControl', function (spEditForm, spFieldControlProvider, spFieldValidator, spCachingCompile) {

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
                        isInDesign: $scope.isInDesign,
                        isInlineEditing: $scope.isInlineEditing
                    };

                    /////
                    // Setup a title model for the title control to use.
                    /////
                    $scope.titleModel = {
                        hasName: false
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
                                if (_.isDate($scope.model.value) && _.isDate(existingValue) && $scope.model.value.getTime() === existingValue.getTime())
                                    return;

                                if ($scope.model.value === existingValue)
                                    return;

                                internalUpdate = true;
                                $scope.model.value = existingValue;
                            });

                            /////
                            // Watch the models value for updates.
                            /////
                            $scope.$watch("model.value", function (newValue, oldValue) {

                                if (internalUpdate) {
                                    internalUpdate = false;
                                    return;
                                }

                                if (_.isDate(newValue) && _.isDate(oldValue) && newValue.getTime() === oldValue.getTime())
                                    return;

                                if ($scope.formData) {
                                    // timecontrol now gives data back in server storage format. only for validation purpose, translate the value from server storage datetime but still save value retuned by time control as is
                                    var tempVal = _.isDate(newValue) ? spUtils.translateFromServerStorageDateTime(newValue) : newValue;

                                    // validate
                                    var msgs = [];
                                    validate(tempVal, msgs);
                                    if (msgs.length === 0 || !newValue) {

                                        if (_.isDate(newValue)) {
                                            $scope.formData.setField(fieldToRender.eid(), newValue, spEntity.DataType.Time);
                                        }
                                        else {
                                            $scope.formData.setField(fieldToRender.eid(), null, spEntity.DataType.Time);
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

                    /////
                    // Register this control with the form layout process
                    /////
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

                    var cachedLinkFunc = spCachingCompile.compile('editForm/directives/spTimeKFieldRenderControl/spTimeKFieldRenderControl.tpl.html');
                    cachedLinkFunc($scope, function (clone) {
                        element.append(clone);
                    });
                }
            };
        });
}());