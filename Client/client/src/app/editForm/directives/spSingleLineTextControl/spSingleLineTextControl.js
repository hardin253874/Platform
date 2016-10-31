// Copyright 2011-2016 Global Software Innovation Pty Ltd
/*global console, _, angular, sp, spEntity, Globalize */

(function () {
    'use strict';

    /////
    // The spSingleLineTextControl directive provides a bridge between
    // the edit form scope and the isolated render control, mapping the
    // relevant properties from one to the other.
    /////
    angular.module('mod.app.editForm.designerDirectives.spSingleLineTextControl', ['mod.common.spInclude', 'mod.app.editForm', 'mod.common.spMobile', 'mod.common.ui.spDialogService', 'mod.common.spCachingCompile'])
        .directive('spSingleLineTextControl', spSingleLineTextControl);

    /* @ngInject */
    function spSingleLineTextControl(spEditForm, spFieldValidator, spMobileContext, spDialogService, spCachingCompile) {
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
                isFormReadOnly: '=?isReadOnly',
                isInDesign: '=?',
                isInlineEditing: '=?',
                version2: '@'
            },
            link: linkFunc,
            require: '^^?spInlineEditForm'
        };

        function linkFunc(scope, element, attrs, spInlineEditForm) {
            ///////////////////////////////////////
            // Variables and bindables
            var fieldToRender, sanitizer, validator, customValidationMessages = [];
            var tablet = spMobileContext.isTablet;
            var mobile = spMobileContext.isMobile;

            var templatePath = 'editForm/directives/spSingleLineTextControl/spSingleLineTextControl.tpl.html';

            if (scope.$eval(scope.version2)) {
                templatePath = 'editForm/directives/spSingleLineTextControl/spSingleLineTextControl2.tpl.html';
            }

            scope.customValidationMessages = customValidationMessages;
            scope.httperizeUrl = spEditForm.httperizeUrl;
            scope.fieldPrefix = '';

            scope.model = {
                fieldValue: null,
                isInDesign: scope.isInDesign,
                isInlineEditing: scope.isInlineEditing
            };

            scope.modalOpts = {
                backdrop: true,
                dialogFade: true,
                backdropClick: true
            };

            // Setup a title model for the title control to use.            
            scope.titleModel = {
                hasName: false
            };

            ///////////////////////////////////////
            // Bindable functions

            scope.openDetail = openDetail;
            scope.ShowExpandButton = showExpandButton;
            scope.cleanFieldValue = cleanFieldValue;
            scope.getCssColorFromColorString = getCssColorFromColorString;
            scope.getFormValueStyle = function () {
                if (mobile || tablet) {
                    return {height: 'calc(100% - 25px)'};
                }
                return {height: '100%'};
            };
            scope.getExpanderStyle = function () {
                if (mobile || tablet) {
                    return {height: '100%', 'min-height': '69px'};
                }
                return {position: 'relative', height: '100%', 'min-height': '57px'};
            };
            scope.getExpanderImageStyle = function () {
                if (mobile || tablet) {
                    return {margin: '0'};
                }
                return {position: 'absolute', margin: '0', bottom: '0'};
            };

            ///////////////////////////////////////
            // Watchers and listeners

            scope.$watch('formControl', function () {
                processModelUpdate();

                if (scope.formControl) {
                    scope.titleModel = spEditForm.createTitleModel(scope.formControl, scope.isInDesign);

                    /////
                    // When the form data changes, update the model.
                    /////
                    var dataWatch = 'formData && formData.getField(' + fieldToRender.id() + ')';

                    scope.$watch(dataWatch, function () {
                        processFormData();
                    });
                }
            });

            if (spInlineEditForm) {
                scope.$watch('customValidationMessages.length', function () {
                    // Note - should generalise this somewhat, but at present adding in special
                    // for inline editing forms. And to make it somewhat more efficient etc.
                    spInlineEditForm.setValidationMessages(scope.formControl, scope.customValidationMessages);
                });
            }

            scope.$watch('isFormReadOnly', function () {
                processModelUpdate();
            });

            scope.$watch('isInTestMode', function () {
                processModelUpdate();
            });

            scope.$on('formControlUpdated', function () {
                processModelUpdate();
            });

            scope.$watch('model.fieldValue', function (value) {
                if (scope.formData) {
                    scope.formData.setField(fieldToRender.eidP, value, spEntity.DataType.String);
                    updateDisplayString(value);
                } else {
                    updateDisplayString(null);
                }
            });

            scope.$on('gather', function (event, callback) {
                callback(scope.formControl, scope.parentControl, element);
            });

            scope.onClick = function (event) {
                if (!event) return;

                scope.$emit('spControlOnFormClick', event.target);                
            };

            scope.onMultiLineTextControlFocus = function(event) {
                if (!event || !event.target || !scope.model.isInlineEditing) return;

                event.target.select();
            };

            ///////////////////////////////////////
            // Initialise

            spEditForm.commonFormControlInit(scope, scope.formControl);

            ///////////////////////////////////////
            // Implementation

            function getFieldDisplayType(field) {
                var represents = field.getLookup('core:fieldRepresents');

                switch (represents && represents.alias()) {
                    case 'core:fieldRepresentsEmail':
                        return 'email';
                    case 'core:fieldRepresentsUrl':
                        return 'url';
                    case 'core:fieldRepresentsPhoneNumber':
                        return 'phoneNumber';
                    case 'core:fieldRepresentsColor':
                        return 'color';
                    case 'core:fieldRepresentsPassword':
                        return 'password';
                    default:
                        return 'string';
                }
            }

            function getFieldInputType(field) {
                var represents = field.getLookup('core:fieldRepresents');

                switch (represents && represents.alias()) {
                    case 'core:fieldRepresentsPassword':
                        return 'password';
                    default:
                        return 'text';
                }
            }

            function updateDisplayString(value) {
                switch (scope.fieldDisplayType) {
                    //TODO: Remove
                    case 'dateTime':
                        if (value && _.isDate(value)) {
                            scope.displayString = Globalize.format(value, 'd') + ' ' + Globalize.format(value, 't');
                        }
                        break;
                    //TODO: Remove
                    case 'time':
                        if (value && _.isDate(value)) {
                            var tempDate = new Date(1973, 0, 1, value.getUTCHours(), value.getUTCMinutes(), 0, 0);
                            scope.displayString = Globalize.format(tempDate, 't');
                        }
                        break;
                    case 'url':
                    case 'email':
                    case 'color':
                    case 'string':
                        scope.displayString = value || '';
                        break;
                }
            }

            function processModelUpdate() {
                if (scope.formControl) {
                    fieldToRender = scope.formControl.fieldToRender;
                    sanitizer = spFieldValidator.getSanitizer(fieldToRender);
                    validator = spFieldValidator.getValidator(fieldToRender);

                    scope.allowMultiline = fieldToRender.allowMultiLines;
                    scope.isMandatoryOnForm = scope.formControl.mandatoryControl;
                    scope.isRequired = scope.isMandatoryOnForm || (fieldToRender && fieldToRender.isRequired);
                    scope.titleModel = spEditForm.createTitleModel(scope.formControl, scope.isInDesign);

                    spEditForm.commonFieldControlInit(fieldToRender);
                    scope.isReadOnly = scope.isFormReadOnly || scope.formControl.readOnlyControl || fieldToRender.isCalculatedField;

                    if (scope.isReadOnly && customValidationMessages.length) {
                        customValidationMessages.length = 0;
                    }

                    if (scope.formData) {
                        scope.model.fieldValue = scope.formData.getField(fieldToRender.eid());
                    }

                    scope.fieldDisplayType = getFieldDisplayType(fieldToRender);
                    scope.fieldInputType = getFieldInputType(fieldToRender);

                    if (fieldToRender.getFieldWatermark) {
                        scope.model.watermark = fieldToRender.getFieldWatermark();
                    }

                    var customValidator = spFieldValidator.getCustomValidator(fieldToRender, scope.isRequired, customValidationMessages);
                    scope.customValidator = customValidator;

                    // provide a function to validate the model is correct. Used when saving
                    scope.formControl.spValidateControl = function (entity) {

                        if (customValidationMessages.length === 0) {
                            var value = entity.getField(fieldToRender.eid());
                            customValidator(value);
                        }

                        return customValidationMessages.length === 0;
                    };

                    scope.formControl.validateOnSchemaChange = function (entity) {
                        customValidator = spFieldValidator.getCustomValidator(fieldToRender, scope.isRequired, customValidationMessages);
                        scope.customValidator = customValidator;

                        var value = entity.getField(fieldToRender.eid());
                        customValidator(value);
                    };

                    if (scope.formData) {
                        updateDisplayString(scope.model.fieldValue);
                    }

                    scope.testId = spEditForm.cleanTestId(_.result(fieldToRender, 'getName'));
                }
            }

            function processFormData() {
                if (scope.formData) {
                    scope.model.fieldValue = scope.formData.getField(fieldToRender.eid());
                } else {
                    scope.model.fieldValue = null;
                }

                updateDisplayString(scope.model.fieldValue);
            }

            function modalInstanceCtrl(scope, $uibModalInstance, fieldTitle, fieldValue) {
                scope.model = {};
                scope.model.fieldTitle = fieldTitle;
                scope.model.fieldValue = fieldValue;

                scope.clearFieldValue = function () {
                    scope.model.fieldValue = '';
                };

                scope.closeDetail = function () {
                    $uibModalInstance.close(scope.model);
                };
            }

            function openDetail(templateUrl) {

                var defaults = {
                    templateUrl: templateUrl,
                    controller: ['$scope', '$uibModalInstance', 'fieldTitle', 'fieldValue', modalInstanceCtrl],
                    resolve: {
                        fieldTitle: function () {
                            return scope.titleModel.name;
                        },
                        fieldValue: function () {
                            return scope.model.fieldValue;
                        }
                    }
                };

                var options = {
                    callback: function (result) {
                        scope.model.fieldValue = result.fieldValue;
                        scope.customValidator(scope.model.fieldValue);
                    }
                };

                spDialogService.showDialog(defaults, options);
            }

            function showExpandButton(isTruncated) {
                scope.isTruncated = !!isTruncated; // double bang ensures it is a boolean
            }

            function cleanFieldValue() {
                scope.model.fieldValue = sanitizer(scope.model.fieldValue);
            }

            function getCssColorFromColorString(colorString) {
                var style = {};

                if (colorString && colorString.length > 0) {
                    style['display'] = 'inline-block';
                    style['width'] = '50px';
                    style['height'] = '20px';
                    style['background-color'] = sp.getCssColorFromARGBString(colorString);
                }

                return style;
            }


            var cachedLinkFunc = spCachingCompile.compile(templatePath);
            cachedLinkFunc(scope, function (clone) {
                element.append(clone);
            });
        }
    }
}());