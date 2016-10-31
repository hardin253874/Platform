// Copyright 2011-2016 Global Software Innovation Pty Ltd
/*global console, _, angular */

(function () {
    'use strict';

    /////
    // The spDateAndTimeKFieldRenderControl directive provides a bridge between
    // the edit form scope and the isolated render control, mapping the
    // relevant properties from one to the other.
    /////
    angular.module('mod.app.editForm.designerDirectives.spDateAndTimeKFieldRenderControl', ['mod.app.editForm', 'mod.app.editForm.spFieldControlProvider', 'mod.common.spCachingCompile'])
        .directive('spDateAndTimeKFieldRenderControl', function (spEditForm, spFieldControlProvider, spControlProvider, spCachingCompile) {

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


                    /////
                    // Watch for changes to the form control.
                    /////
                    $scope.$watch("formControl", function () {
                        var fieldToRender;

                        if ($scope.formControl) {

                            fieldToRender = $scope.formControl.getFieldToRender();

                            if (fieldToRender.getMinDateTime) {
                                $scope.model.minimumValue = fieldToRender.getMinDateTime();
                            }

                            if (fieldToRender.getMaxDateTime) {
                                $scope.model.maximumValue = fieldToRender.getMaxDateTime();
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

                                if (angular.equals($scope.model.value, existingValue))
                                    return;
                                
                                $scope.model.value = existingValue;
                            });

                            /////
                            // Watch the models value for updates.
                            /////
                            $scope.$watch("model.value", function (newValue, oldValue) {

                                if (_.isDate(newValue) && _.isDate(oldValue) && newValue.getTime() === oldValue.getTime())
                                    return;
                                


                                if (!$scope.isReadOnly && $scope.formData) {

                                    if (newValue || newValue === 0) {
                                        newValue = spUtils.parseDate(newValue);  
                                    } else {
                                        newValue = null;
                                    }

                                    $scope.formData.setField(fieldToRender.id(), newValue, spEntity.DataType.DateTime);

                                }
                            }, true);       // NOTE! using value test as watching on a date does not work well.

                        }
                    });

                    if (spInlineEditForm) {
                        $scope.$watch('customValidationMessages.length',
                            function() {
                                // Note - should generalise this somewhat, but at present adding in special
                                // for inline editing forms. And to make it somewhat more efficient etc.
                                spInlineEditForm.setValidationMessages($scope.formControl, $scope.customValidationMessages);
                            });
                    }

                    $scope.$on('gather', function (event, callback) {
                        callback($scope.formControl, $scope.parentControl, element);
                    });

                    var cachedLinkFunc = spCachingCompile.compile('editForm/directives/spDateAndTimeKFieldRenderControl/spDateAndTimeKFieldRenderControl.tpl.html');
                    cachedLinkFunc($scope, function (clone) {
                        element.append(clone);
                    });
                }
            };
        });
}());