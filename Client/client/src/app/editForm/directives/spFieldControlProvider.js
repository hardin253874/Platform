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
        .factory('spFieldControlService', spFieldControlService)
        .factory('spFieldControlProvider', spFieldControlProvider);

    function spFieldControlService(spFieldValidator, spMobileContext) {
        'ngInject';

        return {
            getFieldControlProps,
            getFieldInputModel,
            getFormControlValidators
        };

        function getFieldControlProps(control, {editing}) {
            console.assert(control && control.fieldToRender);

            const fieldToRender = control.fieldToRender;

            const isReadOnly = !editing || control.readOnlyControl || fieldToRender.isFieldReadOnly;
            const isRequired = control.mandatoryControl || fieldToRender.isRequired;
            const customValidationMessages = [];
            const customTypeParser = spFieldValidator.getCustomParser(fieldToRender, customValidationMessages);
            const customValidator = spFieldValidator.getCustomValidator(fieldToRender, isRequired, customValidationMessages);
            const isMobile = spMobileContext.isMobile;
            const isTablet = spMobileContext.isTablet;

            return {
                customValidationMessages, customTypeParser, customValidator,
                isReadOnly, isRequired, isMobile, isTablet
            };
        }

        function getFieldInputModel(control, ctrl) {
            console.assert(control && control.fieldToRender);

            const fieldToRender = control.fieldToRender;

            const isMandatory = control.mandatoryControl;
            const isReadOnly = ctrl.isReadOnly;
            const isRequired = ctrl.isRequired;
            const watermark = fieldToRender.fieldWatermark;
            const customTypeParser = ctrl.customTypeParser;
            const customValidator = ctrl.customValidator;

            return {isMandatory, isRequired, isReadOnly, watermark, customTypeParser, customValidator};
        }

        function getFormControlValidators(control, ctrl) {

            //At the moment the caller will add these methods to the form control's entity object
            //TODO - review this. don't like adding things to the form control's entity object

            return {spValidateControl, validateOnSchemaChange};

            // provide a function to validate the model is correct. Used when saving
            function spValidateControl(entity) {
                if (ctrl.customValidationMessages.length === 0) {
                    performValidation(entity);
                }
                return ctrl.customValidationMessages.length === 0;
            }

            // provide a function to validate the model is correct. Used when schema changes.
            function validateOnSchemaChange(entity) {
                ctrl.customValidator = spFieldValidator.getCustomValidator(control.fieldToRender, ctrl.isRequired, ctrl.customValidationMessages);
                performValidation(entity);
            }

            function performValidation(entity) {
                const value = entity.getField(ctrl.control.fieldToRender.idP);
                const convertedVal = getConvertedValue(ctrl.control, value);
                ctrl.customValidator(convertedVal);
            }
        }
    }

    function spFieldControlProvider(spEditForm, spFieldValidator, spMobileContext) {
        'ngInject';

        return function (ctrl, scope) {

            // note - in our new components we are using "control" rather than "formControl"
            // so for now look for either
            const getFormControl = () => ctrl.formControl || ctrl.control;

            const formControl = getFormControl();
            const model = ctrl.model;
            const fieldToRender = formControl.fieldToRender;
            const customValidationMessages = [];

            ctrl.isMobile = spMobileContext.isMobile;
            ctrl.isTablet = spMobileContext.isTablet;
            ctrl.customValidationMessages = customValidationMessages;
            ctrl.titleModel = spEditForm.createTitleModel(formControl, model.isInDesign);
            ctrl.testId = spEditForm.cleanTestId(_.result(fieldToRender, 'getName'));

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

                const value = entity.getField(fieldToRender.id());
                const convertedVal = getConvertedValue(formControl, value);
                model.customValidator(convertedVal);

                return customValidationMessages.length === 0;
            };

            // provide a function to validate the model is correct. Used when schema changes.
            formControl.validateOnSchemaChange = function (entity) {
                model.customValidator = spFieldValidator.getCustomValidator(fieldToRender, model.isRequired, customValidationMessages);
                const value = entity.getField(fieldToRender.id());
                const convertedVal = getConvertedValue(formControl, value);
                model.customValidator(convertedVal);
            };

            if (scope) {
                // HERE for compatibility
                scope.$watch("isReadOnly", onIsReadOnlyChanged);
                scope.$on('formControlUpdated', onFormControlChanged);
                // end compat... to remove this once all callers do it themselves
                // end then we can remove dependency on scope
            }

            // The caller should establish the $watch and $on listener, if they want
            ctrl.onIsReadOnlyChanged = onIsReadOnlyChanged;
            ctrl.onFormControlChanged = onFormControlChanged;

            function onIsReadOnlyChanged() {
                const formControl = getFormControl();
                const fieldToRender = formControl.fieldToRender;
                const isReadOnly = ctrl.isReadOnly || formControl.readOnlyControl || fieldToRender.isFieldReadOnly;

                if (isReadOnly && ctrl.customValidationMessages.length) {
                    // when we flip to readonly clear the validation messages
                    ctrl.customValidationMessages.length = 0;
                }

                ctrl.model.isReadOnly = isReadOnly;
            }

            function onFormControlChanged() {
                const formControl = getFormControl();
                updateMandatory();
                ctrl.titleModel = spEditForm.createTitleModel(formControl, ctrl.model.isInDesign);
            }

            function updateMandatory() {
                const formControl = getFormControl();
                ctrl.model.isRequired = formControl.mandatoryControl ||
                    (formControl.fieldToRender && formControl.fieldToRender.isRequired);
            }
        };
    }

    //
    // this function converts the value that form data hold to the local values.
    // this is required because a common validator is used by the render control(which expects values in local)
    // and the validate function called by 'save' against 'formData' (which is in server storage value)
    //
    function getConvertedValue(formControl, value) {
        const controlAlias = formControl.firstTypeId().getAlias();
        let retVal = value;
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

}());