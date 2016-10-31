// Copyright 2011-2016 Global Software Innovation Pty Ltd
/*global angular, console, _, sp */
(function () {
    'use strict';

    /**
     * Module implementing a field control provider.
     *
     * It seems the purpose of this service (initially implemented as a angular provide, hence the name)
     * is a function to perform common setup for a field control on a form... in addition to the
     * common field initialisation done for the field itself by the edit form service.
     *
     * This service function expects a bunch of things on the given scope and
     * will mutate the given scope object and the scope.model.
     *
     * @module spFieldControlProvider

     */
    angular.module('mod.app.editForm.spFieldControlProvider', [
        'mod.app.editForm', 'sp.common.fieldValidator', 'mod.common.spMobile']);

    angular.module('mod.app.editForm.spFieldControlProvider')
        .factory('spFieldControlProvider', spFieldControlProvider);

    function spFieldControlProvider(spEditForm, spFieldValidator, spMobileContext) {
        'ngInject';

        return function ($scope) {

            var model = $scope.model;
            var formControl = $scope.formControl;

            var fieldToRender = formControl.fieldToRender;
            var customValidationMessages = [];

            $scope.isMobile = spMobileContext.isMobile;
            $scope.isTablet = spMobileContext.isTablet;
            $scope.customValidationMessages = customValidationMessages;
            $scope.titleModel = spEditForm.createTitleModel(formControl, model.isInDesign);
            $scope.testId = spEditForm.cleanTestId(_.result(fieldToRender, 'getName'));

            spEditForm.commonFieldControlInit(fieldToRender);        // we should move this code into here

            model.isMandatory = formControl.mandatoryControl;
            model.isRequired = model.isMandatory || fieldToRender.isRequired;
            model.watermark = fieldToRender.fieldWatermark;
            model.customTypeParser = spFieldValidator.getCustomParser(fieldToRender, customValidationMessages);
            model.customValidator = spFieldValidator.getCustomValidator(fieldToRender, model.isRequired, customValidationMessages);

            // provide a function to validate the model is correct. Used when saving
            formControl.spValidateControl = function (entity) {

                // if it is already invalid, don't bother testing anything
                if (customValidationMessages.length > 0) {
                    return false;
                }

                var value = entity.getField(fieldToRender.id());
                var convertedVal = getConvertedValue(formControl, value);
                model.customValidator(convertedVal);

                return customValidationMessages.length === 0;
            };

            // provide a function to validate the model is correct. Used when schema changes.
            formControl.validateOnSchemaChange = function (entity) {
                //  $scope.customValidationMessages = [];
                //  fieldToRender = formControl.getFieldToRender();
                // why is this re-creating the validator??
                model.customValidator = spFieldValidator.getCustomValidator(fieldToRender, model.isRequired, customValidationMessages);
                var value = entity.getField(fieldToRender.id());
                var convertedVal = getConvertedValue(formControl, value);
                model.customValidator(convertedVal);
            };

            $scope.$watch("isReadOnly", function (value) {
                updateReadonly();
            });

            $scope.$on('formControlUpdated', function () {
                updateMandatory();
                $scope.titleModel = spEditForm.createTitleModel(formControl, model.isInDesign);
            });

            function updateReadonly() {
                var isReadOnly = $scope.isReadOnly || formControl.readOnlyControl || fieldToRender.isFieldReadOnly;

                if (isReadOnly && $scope.customValidationMessages.length) {
                    // when we flip to readonly clear the validation messages
                    $scope.customValidationMessages.length = 0;
                }

                model.isReadOnly = isReadOnly;
            }

            function updateMandatory() {
                model.isRequired = formControl.mandatoryControl ||
                    (formControl.fieldToRender && formControl.fieldToRender.isRequired);
            }

            ///
            //  this function converts the value that form data hold to the local values.
            //  this is required because a common validator is used by the render control(which expects values in local)
            // and the validate function called by 'save' against 'formData' (which is in server storage value)
            ///
            function getConvertedValue(formCtrl, value) {
                var controlAlias = formCtrl.firstTypeId().getAlias();
                var retVal = value;
                if (value) {
                    switch (controlAlias) {
                        case 'timeKFieldRenderControl':
                            retVal = sp.translateFromServerStorageDateTime(value);
                            break;
                        case 'dateKFieldRenderControl':
                            retVal = sp.translateToLocal(value);
                            break;
                        case 'dateConfigControl':
                        case 'dateAndTimeConfigControl': {
                            retVal = new Date(value);
                        }
                            break;
                        default:
                            break;
                    }
                }
                return retVal;
            }
        };
    }

}());