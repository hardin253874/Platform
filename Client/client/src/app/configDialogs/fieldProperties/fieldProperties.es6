// Copyright 2011-2016 Global Software Innovation Pty Ltd
/*global console, _, angular, sp */

(function () {
    'use strict';

    /**
     * Module implementing a field properties.
     * fieldPropertiesController displays the properties of all field type to configure
     *fieldProperties is a directive to pass the options(Schema info) from parents to current scope.
     *
     *directive
     * @module fieldPropertiesController
     * @example

     Using the fieldProperties:

     &lt;field-properties options="options" modal-instance="modalInstance&gt;&lt;/field-properties&gt

     where options is available on the controller with the following properties:
     - formControl {object} - field/formControl object depending on configure button clicked from.
     - stringPatterns {array(object)} - schema infomation of string pattern.
     - fields {array(object)} - schema info (array of all fields like minInt, maxInt, defaultValue etc etc)
     - fieldTypes {array(object)} - schema info (array of all field type entities like stringField,intField etc)
     -isFieldControl {bool} - True-if configuring properties of form control. False - If configuring properties of a field from definition
     modalInstance is a modalinstance of a dialog to close and return the value to the parent window.
     */

    angular.module('mod.app.configureDialog.fieldProperties', ['ui.bootstrap', 'mod.app.editFormServices', 'mod.app.editForm.designerDirectives', 'mod.common.ui.spDialogService', 'sp.common.fieldValidator', 'mod.app.configureDialog.fieldPropertiesHelper', 'mod.common.ui.spColorPickerConstants', 'mod.common.ui.spColorPickerUtils', 'mod.app.formBuilder.services.spFormBuilderService', 'mod.app.configureDialog.spVisibilityCalculationControl', 'mod.app.spFormControlVisibilityService'])
        .directive('fieldProperties', function () {
            return {
                restrict: 'E',
                transclude: false,
                replace: true,
                scope: {
                    options: '=?',
                    modalInstance: '=?'
                },
                templateUrl: 'configDialogs/fieldProperties/views/fieldProperties.tpl.html',
                controller: 'fieldPropertiesController'
            };
        })
        .controller('fieldPropertiesController', function ($scope, spEditForm, spFieldValidator, configureDialogService, fieldPropertiesHelper, spFormBuilderService, spFormControlVisibilityService) {

            var schemaInfoLoaded = false;
            var entityLoaded = false;
            var controlLoaded = false;
            var formControls = [];
            $scope.isCollapsed = true;            

            // Get option text 
            $scope.getOptionsText = function () {
                if ($scope.isCollapsed === true) {
                    $scope.imageUrl = 'assets/images/arrow_down.png';
                    return 'Options';
                }
                else {
                    $scope.imageUrl = 'assets/images/arrow_up.png';
                    return 'Options';
                }
            };
            //
            // Get field type of current field
            //
            $scope.getFieldType = function () {
                if ($scope.model.fieldToRender) {
                    var type = $scope.model.fieldToRender.getIsOfType()[0];
                    return type.alias();
                }
                return "";
            };

            // Setup the dialog model
            $scope.model = {
                errors: [],
                formMode: 'edit',
                isReadOnly: false,
                isInTestMode: false,
                isFieldControl: $scope.options.isFieldControl,
                mandatoryControl: false,
                readOnlyControl: false,
                showHelpTextControl: false,
                isFieldRequired: false,
                canModifyField: true,
                pattern: null,
                fieldProperties: {
                    patternHelpStrings: [
                        {
                            value: '"0"',
                            description: 'Zero placeholder',
                            example: '1234.5678 ("00000") -> 01235'
                        },
                        {
                            value: '"#"',
                            description: 'Digit placeholder',
                            example: '1234.5678 ("#####") -> 1235'
                        },
                        {
                            value: '","',
                            description: 'Group separator',
                            example: '2147483647 ("##,#") -> 2,147,483,647'
                        },
                        {
                            value: '"."',
                            description: 'Decimal point',
                            example: '0.45678 ("0.00") -> 0.46'
                        },
                        {
                            value: '\'string\'',
                            description: 'Literal string',
                            example: '68 ("# \'degrees\'") -> 68 degrees'
                        }
                    ]
                },
                calc: {
                    script: null, // the calculation
                    host: 'Any', // calculation engine that may be used (so syntax checker will only allow calculations that are supported by all of them)
                    context: null, // typeId to use as context for resolving fields names
                    resultType: null, // alias of the result type
                    resultTypes: [], // all possible result types
                    options: {
                        //hideHintsButtons: true,
                        expectedResultType: null,
                        onCompile: onCompile
                    },
                    lastResult: null
                },
                showStringPattern: false,
                showDecimalPlaces: false,
                isBoolField: false,
                busyIndicator: {
                    type: 'spinner',
                    text: 'Loading...',
                    placement: 'element',
                    isBusy: true
                },
                initialState: {},
                visibilityCalculationModel: {
                    typeId: null,
                    error: null,
                    script: null,
                    isShowHideOn: spFormControlVisibilityService.isShowHideFeatureOn()
                }
            };

            // Clear any errors
            $scope.model.clearErrors = function () {
                $scope.model.errors = [];
            };


            // Add an error
            $scope.model.addError = function (errorMsg) {
                $scope.model.errors.push({type: 'error', msg: errorMsg});
            };


            configureDialogService.getSchemaInfoForFields()
                .then(function (result) {
                    $scope.fields = result.fields;
                    $scope.stringPatterns = result.stringPatterns;
                    $scope.fieldTypes = result.fieldTypes;
                    $scope.model.calc.resultTypes = getCalcResultTypes(result.fieldTypes);
                    $scope.resizeModes = _.sortBy(result.resizeModes, 'enumOrder');
                    $scope.fieldRepresentsEnums = result.fieldRepresentsEnums;
                    schemaInfoLoaded = true;
                    if (schemaInfoLoaded && entityLoaded) {
                        loadTheControl();
                        controlLoaded = true;
                        $scope.model.busyIndicator.isBusy = false;
                    }
                }, function (error) {
                    $scope.model.addError(error);
                    schemaInfoLoaded = true;
                });

            //
            // Get original field from the database.
            //
            $scope.requestFieldEntity = function () {

                configureDialogService.getFieldEntity($scope.model.fieldToRender.idP).then(
                    function (result) {
                        //Augemt the model field with the result to keep the client side changes.
                        spEntity.augment($scope.model.fieldToRender, result, null);
                        loadCalculationInfo();
                        entityLoaded = true;
                        if (schemaInfoLoaded && entityLoaded) {
                            loadTheControl();
                            controlLoaded = true;
                            $scope.model.busyIndicator.isBusy = false;
                        }
                    },
                    function (error) {
                        $scope.model.addError(error);
                        entityLoaded = true;
                    });
            };

            //
            // Get original formControl from the database.
            //
            $scope.requestFormControlEntity = function () {

                configureDialogService.getFormControlEntity($scope.model.formControl.idP).then(
                    function (result) {
                        //Augemt the model field with the result to keep the client side changes.
                        spEntity.augment($scope.model.formControl, result, null);
                        $scope.model.fieldToRender = $scope.model.formControl.fieldToRender;
                        loadCalculationInfo();
                        entityLoaded = true;
                        if (schemaInfoLoaded && entityLoaded) {
                            loadTheControl();
                            controlLoaded = true;
                            $scope.model.busyIndicator.isBusy = false;
                        }
                    },
                    function (error) {
                        $scope.model.addError(error);
                        entityLoaded = true;
                    });
            };

            //check if its a new entity.
            $scope.isNewEntity = $scope.options.formControl.getDataState() === spEntity.DataStateEnum.Create ? true : false;
            var currentType;
            //get all the fieldProperties
            if ($scope.model.isFieldControl) {
                $scope.model.formControl = $scope.options.formControl;
                $scope.model.fieldToRender = $scope.model.formControl.fieldToRender;
                if (!$scope.isNewEntity) {
                    //get formControl from the Database
                    $scope.requestFormControlEntity();
                } else {
                    var controlType = $scope.model.formControl.type.alias();
                    //augment the formControl.
                    spEntity.augment($scope.model.formControl, null, fieldPropertiesHelper.getTemplateFnForFormControlOnly(controlType));
                    //check if the field is new entity
                    if ($scope.model.fieldToRender.getDataState() === spEntity.DataStateEnum.Create) {
                        entityLoaded = true;
                        //spEntity.augment($scope.model.formControl, null, fieldPropertiesHelper.getTemplateFnForFormControl());//TODO template function does not work one level deeper.
                        currentType = $scope.model.fieldToRender.getType().alias();
                        spEntity.augment($scope.model.fieldToRender, null, fieldPropertiesHelper.getTemplateFnForField(currentType));
                    } else {
                        //get field from the Database
                        $scope.requestFieldEntity();
                    }
                }
                if (isAutoNumberField()) {
                    $scope.model.isObjectTabActive = true;
                } else {
                    $scope.model.isFormDetailEnabled = true;
                }

            } else {
                $scope.model.fieldToRender = $scope.options.formControl;
                if (!$scope.isNewEntity) {
                    //get field from the Database
                    $scope.requestFieldEntity();
                } else {
                    entityLoaded = true;
                    currentType = $scope.model.fieldToRender.getType().alias();
                    spEntity.augment($scope.model.fieldToRender, null, fieldPropertiesHelper.getTemplateFnForField(currentType));
                }
                //get field from the database
                $scope.model.isObjectTabActive = true;
            }

            if (!$scope.model.fieldToRender) {
                console.warn('fieldProperties: unexpected missing fieldToRender');
            }

            // Prep calculations
            if ($scope.options.definition && $scope.options.definition.getDataState() !== spEntity.DataStateEnum.Create) {
                $scope.model.calc.context = $scope.options.definition.idP;                
            }

            if ($scope.model.fieldToRender) {
                loadCalculationInfo();
            }

            // hide options for newly calculated fields until the type has been locked down
            $scope.showOptions = function () {
                return !$scope.model.isCalculated || $scope.model.calc.typeLocked;
            };

            var minMaxDataType = fieldPropertiesHelper.getDataTypeForMinMax($scope.model.fieldToRender);

            ////Load the controller.
            function loadTheControl() {
                if (!controlLoaded && $scope.model.fieldToRender) {
                    $scope.fieldType = spEntityUtils.dataTypeForField($scope.model.fieldToRender);
                    $scope.formHeader = getFormHeader();

                    $scope.model.showStringPattern = fieldPropertiesHelper.isStringPatternVisible($scope.model.fieldToRender);
                    $scope.model.showDecimalPlaces = fieldPropertiesHelper.isDecimalPlacesVisible($scope.model.fieldToRender);
                    $scope.model.isBoolField = isBoolField();

                    //limit patterns available for calculations #26958
                    if ($scope.model.isCalculated && $scope.stringPatterns) {
                        $scope.stringPatterns = _.filter($scope.stringPatterns, p =>
                        p.nsAlias === 'core:emailPattern' || p.nsAlias === 'core:phonePattern' || p.nsAlias === 'core:webAddressPattern');
                    }
                    //pre select a string pattern
                    if ($scope.model.fieldToRender.pattern)
                        $scope.model.pattern = _.find($scope.stringPatterns, function (pattern) {
                            return pattern.id() === $scope.model.fieldToRender.getPattern().id();
                        });

                    if ($scope.fieldType === 'Date' || $scope.fieldType === 'DateTime') {
                        // $scope.model.defaultField = $scope.fields[3];
                        $scope.model.defaultField = $scope.model.fieldToRender.cloneDeep();
                        $scope.model.defaultField.setTypes(['core:stringField']);
                        $scope.model.defaultField.setRelationship('core:isOfType', ['core:stringField']);
                    } else {
                        //get default field as clone of field to render.
                        $scope.model.defaultField = $scope.model.fieldToRender.cloneDeep();
                    }
                    $scope.model.defaultField.setIsRequired(false);

                    //Get min and Max fields.
                    $scope.model.minField = fieldPropertiesHelper.getMinField($scope.fields, $scope.model.fieldToRender);
                    $scope.model.maxField = fieldPropertiesHelper.getMaxField($scope.fields, $scope.model.fieldToRender);

                    //load form control name properties
                    if ($scope.model.isFieldControl || $scope.model.calc.showDisplayName) {
                        $scope.model.formData = spEntity.createEntityOfType('formControlEntityType');
                        $scope.model.fieldDisplayNameControl = getDummyFormControl('core:name', 'Display Name');
                        $scope.model.fieldDescriptionControl = getDummyFormControl('core:description', 'Description');
                        formControls.push($scope.model.fieldDisplayNameControl, $scope.model.fieldDescriptionControl);
                    }
                    if ($scope.model.isFieldControl) {
                      
                        setFormDataField($scope.model.formData, 'core:name', $scope.model.formControl.name);
                        setFormDataField($scope.model.formData, 'core:description', $scope.model.formControl.description);
                        $scope.model.formControlLocal = $scope.model.formControl.cloneDeep();
                        $scope.model.mandatoryControl = $scope.model.formControl.mandatoryControl;
                        $scope.model.readOnlyControl = $scope.model.formControl.readOnlyControl;

                        initVisibilityCalculationModel();

                        //set resize options
                        $scope.model.resizeOptions = {
                            resizeModes: $scope.resizeModes,
                            isHresizeModeDisabled: getHorizontalResizeModeDisabled(),
                            isVresizeModeDisabled: getVerticalResizeModeDisabled()
                        };

                        //set if the field can be modified or not.
                        $scope.model.canModifyField = spFormBuilderService.isDirectField($scope.model.fieldToRender.idP);
                        if (!$scope.model.canModifyField) {
                            $scope.model.calc.options = _.defaults({
                                hideHintsButtons: true,
                                disabled: true
                            }, $scope.model.calc.options);
                        }

                        //load format control file.
                        $scope.formControlFile = 'configDialogs/fieldProperties/views/fieldFormProperties.tpl.html';
                        $scope.formatControlFile = 'configDialogs/fieldProperties/views/fieldFormatProperties.tpl.html';
                        $scope.visibilityTemplate = 'configDialogs/fieldProperties/views/fieldVisibilityProperties.tpl.html';                        
                    } else if ($scope.model.calc.showDisplayName) {
                        $scope.model.formData.registerField(getFieldByAlias('core:name'), spEntity.DataType.String);
                    }
                    
                    $scope.model.fieldData = spEntity.createEntityOfType('fieldEntityType');
                    $scope.model.fieldNameControl = getDummyFormControl('core:name', 'Field Name');
                    $scope.model.fieldNameControl.setMandatoryControl(true);
                    setFormDataField($scope.model.fieldData, 'core:name', $scope.model.fieldToRender.name);
                    $scope.model.fieldScriptNameControl = getDummyFormControl('core:fieldScriptName', 'Script Name');
                    setFormDataField($scope.model.fieldData, 'core:fieldScriptName', $scope.model.fieldToRender.fieldScriptName);
                    if (!$scope.model.isFieldControl) {
                        $scope.model.fieldDescriptionControl = getDummyFormControl('core:description', 'Description');
                        setFormDataField($scope.model.fieldData, 'core:description', $scope.model.fieldToRender.description);
                        formControls.push($scope.model.fieldNameControl, $scope.model.fieldDescriptionControl, $scope.model.fieldScriptNameControl);
                    } else {
                        formControls.push($scope.model.fieldNameControl, $scope.model.fieldScriptNameControl);
                    }

                    
                    
                    $scope.model.formControlDescription = $scope.model.formControl ? $scope.model.formControl.description : '';
                    $scope.model.showControlHelpText = $scope.model.formControl && $scope.model.formControl.showControlHelpText;


                    switch ($scope.getFieldType()) {
                        case 'core:autoNumberField': {
                            //get current definition
                            var defType = spFormBuilderService.getDefinitionType().getEntity();
                            if (defType && defType.getDataState() !== spEntity.DataStateEnum.Create) {
                                configureDialogService.isTypeHasInstances(defType.idP).then(function (result) {
                                    $scope.isFieldTypeHasInstances = result;
                                });
                            } else {
                                $scope.isFieldTypeHasInstances = false;
                            }

                            $scope.model.autoSeedControl = getDummyFormControl($scope.fields[6], 'Starting Number');
                            setFormDataField($scope.model.fieldData, $scope.fields[6], $scope.model.fieldToRender.autoNumberSeed ? $scope.model.fieldToRender.autoNumberSeed : $scope.fields[6].defaultValue);
                            $scope.model.autoPatternControl = getDummyFormControl($scope.fields[5], 'Pattern');
                            setFormDataField($scope.model.fieldData, $scope.fields[5], $scope.model.fieldToRender.autoNumberDisplayPattern);
                            formControls.push($scope.model.autoSeedControl, $scope.model.autoPatternControl);
                            if (!$scope.model.canModifyField) {
                                $scope.model.isFormatTabActive = true;
                                $scope.model.isObjectTabActive = false;
                            }
                        }
                            break;
                        case 'core:boolField': {
                            if ($scope.model.fieldToRender.getDefaultValue) {
                                var value = spUtils.convertDbStringToNative($scope.fieldType, $scope.model.fieldToRender.getDefaultValue());
                                $scope.model.defaultValue = !!value;
                            } else {
                                $scope.model.defaultValue = false;
                            }
                        }
                            break;
                        default: {
                            $scope.model.isFieldRequired = $scope.model.fieldToRender.isRequired;
                            $scope.model.fieldMinimumControl = getDummyFormControl($scope.model.minField, 'Minimum');
                            setFormDataField($scope.model.fieldData, $scope.model.minField, fieldPropertiesHelper.getMinimumFieldValue($scope.model.fieldToRender));
                            $scope.model.fieldMaximumControl = getDummyFormControl($scope.model.maxField, 'Maximum');
                            setFormDataField($scope.model.fieldData, $scope.model.maxField, fieldPropertiesHelper.getMaximumFieldValue($scope.model.fieldToRender));
                            $scope.model.fieldDecimalPlacesControl = getDummyFormControl($scope.fields[4], 'Decimal Places');

                            setFormDataField($scope.model.fieldData, $scope.fields[4], $scope.model.fieldToRender.decimalPlaces ? $scope.model.fieldToRender.decimalPlaces : getDefaultDecimalPlaces());
                            //set formcontrol and formdata for default value
                            var defaultValue = $scope.model.fieldToRender.getDefaultValue ? convertDbStringToNative($scope.fieldType, $scope.model.fieldToRender.getDefaultValue()) : null;

                            $scope.model.fieldDefaultControl = getDummyFormControl($scope.model.defaultField, 'Default');
                            $scope.model.fieldDefaultControl.setMandatoryControl(false);
                            $scope.model.fieldDefaultData = getDummyFormData('fieldDefaultEntityType', $scope.model.defaultField, defaultValue);

                            formControls.push($scope.model.fieldMinimumControl, $scope.model.fieldMaximumControl, $scope.model.fieldDecimalPlacesControl);
                        }
                            break;
                    }

                    $scope.baseControlTemplate = 'configDialogs/fieldProperties/views/fieldControlBaseProperties.tpl.html';
                    $scope.definitionControlTemplate = 'configDialogs/fieldProperties/views/fieldDefinitionBaseProperties.tpl.html';

                    $scope.model.initialState = {
                        fieldData: $scope.model.fieldData.cloneDeep(),
                        isFieldRequired: $scope.model.isFieldRequired,
                        showControlHelpText: $scope.model.showControlHelpText,
                        calcScript: $scope.model.calc.script,
                        calcResultType: $scope.model.calc.resultType,
                        defaultValue: $scope.model.defaultValue,
                        fieldDefaultData: $scope.model.fieldDefaultData ? $scope.model.fieldDefaultData.cloneDeep() : null,
                        patternId: sp.result($scope.model, 'pattern.id')
                    };
                }
            }

            $scope.isAutoseedReadOnlyControl = function () {
                return $scope.isFieldTypeHasInstances && ($scope.model.fieldToRender.getDataState() !== spEntity.DataStateEnum.Create);
            };             

            function initVisibilityCalculationModel() {
                if (!$scope.model.isFieldControl) {
                    return;
                }

                if ($scope.options && $scope.options.definition && $scope.options.definition.getDataState() !== spEntity.DataStateEnum.Create) {
                    $scope.model.visibilityCalculationModel.typeId = $scope.options.definition.idP;                    
                }

                if ($scope.model.formControl) {
                    $scope.model.visibilityCalculationModel.script = $scope.model.formControl.visibilityCalculation;   
                }             
            }

            //
            // Determine whether the horizontal resize mode option is disabled.
            //
            function getHorizontalResizeModeDisabled() {

                switch ($scope.getFieldType()) {
                    case 'core:boolField':
                    case 'core:dateField':
                    case 'core:timeField':
                    case 'core:dateTimeField':
                        return true;
                }
                return false;
            }

            //
            // Determine whether the vertical resize mode option is disabled.
            //
            function getVerticalResizeModeDisabled() {
                switch ($scope.getFieldType()) {
                    case 'core:autoNumberField':
                    case 'core:boolField':
                    case 'core:currencyField':
                    case 'core:dateField':
                    case 'core:decimalField':
                    case 'core:intField':
                    case 'core:timeField':
                    case 'core:dateTimeField':
                        return true;
                    case 'core:stringField':
                        return !( $scope.model.fieldToRender.getAllowMultiLines && $scope.model.fieldToRender.getAllowMultiLines());
                }
                return false;
            }

            ////
            //Get default decimal places if decimal places is not defined.
            //
            function getDefaultDecimalPlaces() {
                if ($scope.model.fieldToRender.decimalPlaces !== 0) {
                    if ($scope.fieldType === 'Decimal')
                        return $scope.fields[4].defaultValue;
                    if ($scope.fieldType === 'Currency') {
                        return 2;
                    }
                }
                return $scope.model.fieldToRender.decimalPlaces;
            }

            //
            //get the form header from the field type.
            //
            function getFormHeader() {
                var suffix = ' Field Properties';
                if ($scope.model.isCalculated) {
                    return 'Calculated' + suffix;
                }
                var alias = $scope.model.fieldToRender.getIsOfType()[0].alias();
                var fieldType = _.find($scope.fieldTypes, function (type) {
                    return alias === type.alias();
                });
                var fieldTypeName = fieldType.getFieldDisplayName().getName();
                if ($scope.getFieldType() === 'core:stringField' && ( $scope.model.fieldToRender.getAllowMultiLines && $scope.model.fieldToRender.getAllowMultiLines() ))
                    return 'Multiline ' + fieldTypeName + suffix;
                else {
                    return fieldTypeName + suffix;
                }
            }

            //
            //Get dummy formdata json object for the field.
            //
            function getFieldByAlias(nsAlias) {
                var field = _.find($scope.fields, f => f.eid().nsAlias === nsAlias);
                return field;
            }

            function getDummyFormData(entityType, field, value) {
                // field may be an alias or a field entity
                if (_.isString(field)) {
                    field = getFieldByAlias(field);
                }
                var formDataEntity = spEntity.createEntityOfType(entityType);
                var dbType = spEntityUtils.dataTypeForField(field);
                formDataEntity.setField(field.id(), value, dbType);

                return formDataEntity;
            }

            function setFormDataField(formData, field, value) {
                // field may be an alias or a field entity
                if (_.isString(field)) {
                    field = getFieldByAlias(field);
                }
                var dbType = spEntityUtils.dataTypeForField(field);
                formData.registerField(field.eid(), dbType);
                formData.setField(field.id(), value, dbType);
            }

            function getDefaultFieldType(fieldType) {
                var alias = fieldType.alias() === 'core:currencyField' ? 'core:decimalField' : fieldType.alias();
                return _.find($scope.fieldTypes, function (type) {
                    return alias === type.alias();
                });
            }

            //
            //Get dummy formdata json object for the field.
            //
            function getDummyFormControl(field, fieldTitle) {
                // field may be an alias or a field entity
                if (_.isString(field)) {
                    field = getFieldByAlias(field);
                }

                var fieldType = field.getIsOfType()[0];
                var specialAlias = null;
                if (fieldTitle === 'Default') {
                    fieldType = getDefaultFieldType(fieldType);
                    if ($scope.fieldType === 'Date') {
                        specialAlias = 'dateConfigControl';
                    }
                    if ($scope.fieldType === 'DateTime') {
                        specialAlias = 'dateAndTimeConfigControl';
                    }
                }
                var defaultRenderingControl = _.find(fieldType.getDefaultRenderingControls(), function (control) {
                    return control.getContext().getName() === 'Html';
                });
                if (!defaultRenderingControl) {
                    defaultRenderingControl = _.find(fieldType.getRenderingControl(), function (control) {
                        return control.getContext().getName() === 'Html';
                    });
                }
                var dummyFormControl = spEntity.fromJSON({
                    typeId: specialAlias || defaultRenderingControl.nsAlias,
                    'name': fieldTitle,
                    'description': '',
                    'console:fieldToRender': field,
                    'console:mandatoryControl': false,
                    'console:readOnlyControl': false,
                    'console:showControlHelpText': false,
                    'console:isReversed': false,
                    'console:visibilityCalculation': ''
                });
                return dummyFormControl;
            }

            //
            //returns true if the datatype is bool
            //
            function isBoolField() {
                if ($scope.model.fieldToRender) {
                    var type = $scope.model.fieldToRender.getIsOfType()[0];
                    if (type.alias() === 'core:boolField')
                        return true;
                }
                return false;
            }

            //
            //returns true if the datatype is autonumber
            //
            function isAutoNumberField() {
                if ($scope.model.fieldToRender) {
                    var type = $scope.model.fieldToRender.getIsOfType()[0];
                    if (type.alias() === 'core:autoNumberField')
                        return true;
                }
                return false;
            }

            function getCalcResultTypes(fieldTypes) {
                var res = _.chain(fieldTypes)
                    .map(ft => ({
                        typeAlias: ft.nsAlias,
                        name: ft.fieldDisplayName.name
                    }))
                    .filter(ft => ft.typeAlias !== 'core:autoNumberField')
                    .value();
                res.splice(1, 0, {typeAlias: 'core:stringField|multiline', name: 'Multiline Text'});
                return res;
            }

            /**
             * Convert a string from the database into a native js object. Used for default values.
             *
             * @param {String} dataType The dbType of the string.
             * @param {String} value the value.
             * @returns {Object} The converted value.
             */
            function convertDbStringToNative(dataType, value) {
                var result;
                if (value === null) {
                    result = null;
                } else {
                    switch (dataType) {
                        case 'Decimal':
                        case 'Currency':
                            result = value === '' ? null : parseFloat(value);
                            break;
                        case 'Number':
                        case 'Int32':
                            result = value === '' ? null : parseInt(value, 10);
                            break;
                        case 'String':
                        case 'Date':
                        case 'DateTime':
                            result = value === '' ? null : value;   // empty strings should be represented as nulls
                            break;
                        case 'Time':
                            result = value === '' ? null : spUtils.getUtcDateByUtcTimeString(value);    //string value represents the utc time.
                            break;
                        case 'Bool':
                            result = value === '' ? false : spUtils.stringToBoolean(value);
                            break;
                        default:
                            console.log('Unrecognized field type', dataType, value || '<null>');
                            throw new Error('Unrecognized field type: ' + dataType + ' ' + (value || '<null>'));
                    }
                }
                return result;
            }

            //
            //Set the fieldScriptName to be the same as the name
            //
            $scope.$watch('model.fieldData.name', function (newValue, oldValue) {
                // only if it was initially null                
                if (!$scope.isNewEntity)
                    return;
                var fieldData = $scope.model.fieldData;
                // and only if it hasn't been otherwise modified by the user
                if (fieldData && fieldData.fieldScriptName === oldValue)
                    fieldData.fieldScriptName = fieldData.name;
            });

            //
            //Set the default field max value if the maximum value is changed.
            //
            $scope.$watch('model.fieldData.max' + minMaxDataType, function () {
                if ($scope.model.maxField) {
                    validateMinMaxValue();
                    //set maximum value of default field
                    var maxValue = $scope.model.fieldData.getField($scope.model.maxField.id());
                    if (typeof (maxValue) !== 'undefined' && $scope.model.fieldDefaultControl.validateOnSchemaChange) {
                        fieldPropertiesHelper.setMaxValue($scope.fieldType, maxValue, $scope.model.defaultField);
                        if ($scope.model.fieldDefaultControl) {
                            $scope.model.fieldDefaultControl.setFieldToRender($scope.model.defaultField);
                            //refresh control
                            $scope.model.fieldDefaultControl = jQuery.extend(true, {}, $scope.model.fieldDefaultControl);
                            $scope.model.fieldDefaultControl.validateOnSchemaChange($scope.model.fieldDefaultData);
                        }
                    }
                }
            });

            //
            //Set the default field min value if the minimum value is changed.
            //
            $scope.$watch('model.fieldData.min' + minMaxDataType, function () {
                if ($scope.model.minField) {
                    validateMinMaxValue();
                    //set minimum value of the default field
                    var minValue = $scope.model.fieldData.getField($scope.model.minField.id());
                    if (typeof (minValue) !== 'undefined' && $scope.model.fieldDefaultControl.validateOnSchemaChange) {
                        fieldPropertiesHelper.setMinValue($scope.fieldType, minValue, $scope.model.defaultField);
                        if ($scope.model.fieldDefaultControl && $scope.model.fieldDefaultData) {
                            $scope.model.fieldDefaultControl.setFieldToRender($scope.model.defaultField);
                            //refresh control
                            $scope.model.fieldDefaultControl = jQuery.extend(true, {}, $scope.model.fieldDefaultControl);
                            $scope.model.fieldDefaultControl.validateOnSchemaChange($scope.model.fieldDefaultData);
                        }
                    }
                }

            });

            //
            //Set the default field decimal places if the decimalplaces value is changed.
            //
            $scope.$watch('model.fieldData.decimalPlaces', function () {
                if ($scope.model.showDecimalPlaces) {
                    var decimalPlaces = $scope.model.fieldData.getField($scope.fields[4].id());
                    if (typeof (decimalPlaces) !== 'undefined' && $scope.model.fieldDefaultControl.validateOnSchemaChange) {
                        $scope.model.defaultField.setDecimalPlaces(decimalPlaces);
                        if ($scope.model.fieldDefaultControl) {
                            $scope.model.fieldDefaultControl.setFieldToRender($scope.model.defaultField);
                            $scope.model.fieldDefaultControl.validateOnSchemaChange($scope.model.fieldDefaultData);
                        }
                    }
                }
            });

            //
            //Set the default field decimal places if the decimalplaces value is changed.
            //
            $scope.$watch('model.calc.resultType', function () {
                var fieldType = $scope.model.calc.resultType;
                if (fieldType) {
                    fieldType = fieldType.split('|')[0];
                    $scope.model.calc.options.expectedResultType = {
                        dataType: spEntityUtils.dataTypeForFieldTypeAlias(fieldType),
                        disallowList: true
                    };
                }
            });

            //
            //Set the default field pattern if the string pattern value is changed.
            //
            $scope.updateDefaultField = function () {
                if ($scope.model.showStringPattern && $scope.model.defaultField) {
                    $scope.model.defaultField.setPattern($scope.model.pattern);
                    if ($scope.model.fieldDefaultControl && $scope.model.fieldDefaultControl.validateOnSchemaChange) {
                        $scope.model.fieldDefaultControl.setFieldToRender($scope.model.defaultField);
                        $scope.model.fieldDefaultControl = jQuery.extend(true, {}, $scope.model.fieldDefaultControl);
                        $scope.model.fieldDefaultControl.validateOnSchemaChange($scope.model.fieldDefaultData);
                    }
                }
            };

            $scope.onVisibilityScriptCompiled = function (script, error) {
                $scope.model.visibilityCalculationModel.isScriptCompiling = false;

                if (!$scope.model.isFieldControl) {
                    return;
                }

                $scope.model.visibilityCalculationModel.error = error;

                if (!error) {                    
                    $scope.model.visibilityCalculationModel.script = script;
                }
            };
            
            $scope.onVisibilityScriptChanged = function () {                
                $scope.model.visibilityCalculationModel.isScriptCompiling = true;
            };

            //
            //  sets the known fieldRepresents enum value.
            //
            function setFieldRepresentsRelationship(field, pattern) {
                if (field && pattern) {
                    //var patternAlias = pattern.getAlias();

                    switch (pattern.nsAlias) {
                        case 'core:emailPattern':
                            field.setLookup('core:fieldRepresents', _.find($scope.fieldRepresentsEnums, {'nsAlias': 'core:fieldRepresentsEmail'}));
                            break;
                        case 'core:phonePattern':
                            field.setLookup('core:fieldRepresents', _.find($scope.fieldRepresentsEnums, {'nsAlias': 'core:fieldRepresentsPhoneNumber'}));
                            break;
                        case 'core:webAddressPattern':
                            field.setLookup('core:fieldRepresents', _.find($scope.fieldRepresentsEnums, {'nsAlias': 'core:fieldRepresentsUrl'}));
                            break;
                    }
                }
            }

            //
            //validate minimum and maximum value.
            //
            function validateMinMaxValue() {
                $scope.model.clearErrors();
                var maxValue = $scope.model.fieldData.getField($scope.model.maxField.id());
                var minValue = $scope.model.fieldData.getField($scope.model.minField.id());
                if (maxValue && minValue) {
                    if (minValue > maxValue) {
                        $scope.model.addError('Minimum value cannot be greater then maximum value');
                    }
                    if (minValue.toString() === maxValue.toString()) {
                        $scope.model.addError('Minimum value cannot be equal to maximum value');
                    }
                }
            }

            //
            //do not validate the default value if the value is 'TODAY' for dateField type and for 'NOW' for dateTimeField type.
            //
            function isDefaultHasNativeValue(value) {
                var type = $scope.getFieldType();
                if (type === 'core:dateField' || type === 'core:dateTimeField') {
                    if (value === 'TODAY' || value === 'NOW') {
                        return false;
                    }
                }
                return true;
            }

            function validateForm() {
                if (!isBoolField() && !isAutoNumberField()) {
                    //validate min and max value
                    validateMinMaxValue();
                }

                var fieldId = $scope.model.fieldToRender.id();

                // validate name field value
                var nameField = _.find($scope.fields, function (f) {
                    return f.alias() === 'core:name';
                });

                var newName = $scope.model.fieldData.getField(nameField.id());

                if (nameField && newName) {
                    const result = spFormBuilderService.validateFieldName(newName, fieldId);
                    if (result.hasError) {
                        $scope.model.addError(result.message);
                        return;
                    }
                }

                // validate script name field value
                var scriptField = _.find($scope.fields, function (f) {
                    return f.alias() === 'core:fieldScriptName';
                });

                var newScriptName = $scope.model.fieldData.getField(scriptField.id());

                if (scriptField && newScriptName) {
                    const result = spFormBuilderService.validateFieldScriptName(newScriptName, fieldId);
                    if (result.hasError) {
                        $scope.model.addError(result.message);
                        return;
                    }
                }

                let visibilityCalculation;

                if ($scope.model.isFieldControl) {
                    visibilityCalculation = $scope.model.visibilityCalculationModel.script;
                } else {
                    // Need to find the form control that is being rendered 
                    // You gotta do what you gotta do. Avert your eyes !
                    const allControls = spEditForm.getFormControls(spFormBuilderService.form);
                    const fieldToRenderId = sp.result($scope, "model.fieldToRender.idP");
                    const fieldControl = _.find(allControls, c => sp.result(c, "fieldToRender.idP") === fieldToRenderId);
                    visibilityCalculation = sp.result(fieldControl, "visibilityCalculation");
                }

                if (visibilityCalculation &&
                    $scope.model.isFieldRequired &&
                    spUtils.isNullOrUndefined($scope.model.fieldDefaultData.getField($scope.model.defaultField.id()))) {
                    $scope.model.addError("Visibility calculation cannot be defined as the field is mandatory and no default value is specified.");
                    return;
                }
            }

            function loadCalculationInfo() {
                var field = $scope.model.fieldToRender;
                var calc = $scope.model.calc;
                $scope.model.isCalculated = field.isCalculatedField;
                if ($scope.isNewEntity && !field.fieldScriptName) {
                    field.fieldScriptName = field.name;
                }
                if (field.isCalculatedField) {
                    calc.resultType = field.type.nsAlias;
                    if (field.allowMultiLines)
                        calc.resultType += '|multiline';
                    calc.script = field.fieldCalculation;
                    calc.showDisplayName = $scope.options.isInitialCreate && $scope.options.source === 'form';
                    calc.typeLocked = !$scope.options.isInitialCreate;
                }
            }

            function onCompile(res) {
                $scope.model.calc.lastResult = res.result;
            }

            $scope.disableOk = function disableOk() {
                // Disable OK button if calculation is both changed and invalid                
                var field = $scope.model.fieldToRender;
                var calc = $scope.model.calc;
                var res = (calc.script !== field.fieldCalculation && calc.lastResult && calc.lastResult.error) || $scope.model.visibilityCalculationModel.error || $scope.model.visibilityCalculationModel.isScriptCompiling;
                return res;
            };

            $scope.visibilityTabClicked = function() {
                _.delay(function() {
                    $scope.$broadcast('sp.app.ui-refresh');
                }, 100);
            };

            // Ok click handler
            $scope.ok = function () {
                $scope.model.clearErrors();
                var valid = spEditForm.validateFormControls(formControls, $scope.model.fieldData);
                var defaultValueValid = true;
                var initialState = $scope.model.initialState;

                //No need to validate the default value if the field type is boolean or auto number type.
                if (!isBoolField() && !isAutoNumberField())
                    defaultValueValid = spEditForm.validateFormControls([$scope.model.fieldDefaultControl], $scope.model.fieldDefaultData);
                if (valid && defaultValueValid) {
                    validateForm();
                    if ($scope.model.errors.length === 0) {
                        var field = $scope.model.fieldToRender;
                        if ($scope.model.isFieldControl) {
                            //Set all the formControl properties
                            $scope.model.formControl.setName($scope.model.formData.getField($scope.fields[0].id()));
							$scope.model.formControl.setDescription($scope.model.formData.getField($scope.fields[1].id()));                               
                            $scope.model.formControl.setMandatoryControl($scope.model.mandatoryControl);
                            $scope.model.formControl.setReadOnlyControl($scope.model.readOnlyControl);
                            $scope.model.formControl.setShowControlHelpText($scope.model.showControlHelpText);                            
                            $scope.model.formControl.setField('console:visibilityCalculation', $scope.model.visibilityCalculationModel.script, spEntity.DataType.String);
							$scope.options.formControl.setRenderingBackgroundColor($scope.model.formControlLocal.renderingBackgroundColor);
                            $scope.options.formControl.setRenderingHorizontalResizeMode($scope.model.formControlLocal.renderingHorizontalResizeMode);
                            $scope.options.formControl.setRenderingVerticalResizeMode($scope.model.formControlLocal.renderingVerticalResizeMode);
                        } else if ($scope.model.calc.showDisplayName) {
                            field.unmanagedControlLabel = $scope.model.formData.getField($scope.fields[0].id());
                        }

                        // Set field properties
                        if ($scope.model.canModifyField) {
                            if ($scope.isNewEntity || $scope.model.fieldData.getField($scope.fields[0].id()) !== initialState.fieldData.getField($scope.fields[0].id())) {
                                field.name = $scope.model.fieldData.getField($scope.fields[0].id());
                            }                                                
                            

                            if ($scope.isNewEntity || $scope.model.fieldData.getField($scope.fields[1].id()) !== initialState.fieldData.getField($scope.fields[1].id())) {
                                field.description = $scope.model.fieldData.getField($scope.fields[1].id());
                            }

                            if ($scope.isNewEntity || $scope.model.isFieldRequired !== initialState.isFieldRequired) {
                                field.isRequired = $scope.model.isFieldRequired;
                            }
                            // Calculated
                            if ($scope.isNewEntity || $scope.model.fieldData.fieldScriptName !== initialState.fieldData.fieldScriptName) {
                                field.fieldScriptName = $scope.model.fieldData.fieldScriptName;
                            }

                            if ($scope.model.isCalculated &&
                                ($scope.isNewEntity || initialState.calcScript !== $scope.model.calc.script)) {
                                field.isFieldReadOnly = true; // just in case
                                field.fieldCalculation = $scope.model.calc.script;
                                if (!$scope.model.calc.typeLocked) {
                                    var resultType = $scope.model.calc.resultType.split('|')[0];
                                    field.type = resultType;
                                    field.setRelationship('core:isOfType', resultType); // keep type and isOfType in sync
                                }
                            }

                            if (field.type && field.type.alias) {
                                switch (field.type.alias()) {
                                    case 'core:autoNumberField':
                                        field.registerField('core:autoNumberDisplayPattern', spEntity.DataType.String);
                                        field.registerField('core:autoNumberSeed', spEntity.DataType.Int32);
                                        break;
                                    case 'core:intField':
                                        field.registerField('core:minInt', spEntity.DataType.Int32);
                                        field.registerField('core:maxInt', spEntity.DataType.Int32);
                                        break;
                                    case 'core:decimalField':
                                    case 'core:currencyField':
                                        field.registerField('core:minDecimal', spEntity.DataType.Decimal);
                                        field.registerField('core:maxDecimal', spEntity.DataType.Decimal);

                                        if (!field.hasField('core:decimalPlaces')) {
                                            field.registerField('core:decimalPlaces', spEntity.DataType.Int32);
                                            if ($scope.isNewEntity) {
                                                field.decimalPlaces = 3;
                                            }
                                        }
                                        break;
                                    case 'core:dateField':
                                        field.registerField('core:minDate', spEntity.DataType.Date);
                                        field.registerField('core:maxDate', spEntity.DataType.Date);
                                        break;
                                    case 'core:timeField':
                                        field.registerField('core:minTime', spEntity.DataType.Time);
                                        field.registerField('core:maxTime', spEntity.DataType.Time);
                                        break;
                                    case 'core:dateTimeField':
                                        field.registerField('core:minDateTime', spEntity.DataType.DateTime);
                                        field.registerField('core:maxDateTime', spEntity.DataType.DateTime);
                                        break;
                                    case 'core:stringField':
                                        if (!field.hasField('core:allowMultiLines')) {
                                            field.registerField('core:allowMultiLines', spEntity.DataType.Bool);

                                            if ($scope.isNewEntity) {
                                                field.allowMultiLines = false;
                                            }
                                        }

                                        field.registerField('core:minLength', spEntity.DataType.Int32);
                                        field.registerField('core:maxLength', spEntity.DataType.Int32);
                                        field.registerLookup('core:pattern');
                                        break;
                                }
                            }

                            if ($scope.model.isCalculated) {
                                if (!$scope.model.calc.typeLocked &&
                                    ($scope.isNewEntity || $scope.model.calc.resultType !== initialState.calcResultType)) {
                                    field.allowMultiLines = $scope.model.calc.resultType === 'core:stringField|multiline';
                                }
                            }

                            switch ($scope.getFieldType()) {
                                case 'core:autoNumberField': {
                                    if (!$scope.isAutoseedReadOnlyControl()) {
                                        var autoNumberSeed = $scope.model.fieldData.getField($scope.fields[6].id());
                                        var existingAutoNumberSeed = initialState.fieldData.getField($scope.fields[6].id());
                                        if ($scope.isNewEntity || autoNumberSeed !== existingAutoNumberSeed) {
                                            //set autonumber seed value
                                            $scope.model.fieldToRender.setAutoNumberSeed(autoNumberSeed);
                                        }
                                    }
                                    var patternValue = $scope.model.fieldData.getField($scope.fields[5].id());
                                    if (patternValue) {
                                        var regEx = /[0#]/;
                                        if (!patternValue.match(regEx))
                                            patternValue += '####';
                                    }
                                    var existingPatternValue = initialState.fieldData.getField($scope.fields[5].id());
                                    if ($scope.isNewEntity || patternValue !== existingPatternValue) {
                                        $scope.model.fieldToRender.setAutoNumberDisplayPattern(patternValue);
                                    }
                                }
                                    break;
                                case 'core:boolField': {
                                    //set default value of bool field
                                    var sBoolDefault = $scope.model.defaultValue ? 'true' : 'false';
                                    var existingDefaultValue = initialState.defaultValue ? 'true' : 'false';

                                    if ($scope.isNewEntity || existingDefaultValue !== sBoolDefault) {
                                        $scope.model.fieldToRender.defaultValue = sBoolDefault;
                                    }
                                }
                                    break;
                                default: {
                                    var fieldDataType = spEntityUtils.dataTypeForField(field);
                                    //set min and max values
                                    var maxValue = $scope.model.fieldData.getField($scope.model.maxField.id());
                                    var existingMaxValue = initialState.fieldData.getField($scope.model.maxField.id());
                                    if ($scope.isNewEntity || maxValue !== existingMaxValue) {
                                        fieldPropertiesHelper.setMaxValue(fieldDataType, maxValue, $scope.model.fieldToRender);
                                    }

                                    var minValue = $scope.model.fieldData.getField($scope.model.minField.id());
                                    var existingMinValue = initialState.fieldData.getField($scope.model.minField.id());
                                    if ($scope.isNewEntity || minValue !== existingMinValue) {
                                        fieldPropertiesHelper.setMinValue(fieldDataType, minValue, $scope.model.fieldToRender);
                                    }

                                    //set defaultValue
                                    var defaultValueString = $scope.model.fieldDefaultData.getField($scope.model.defaultField.id());
                                    var existingDefaultValueString = initialState.fieldDefaultData.getField($scope.model.defaultField.id());
                                    if (typeof (defaultValueString) !== 'undefined') {
                                        if (defaultValueString !== null) {
                                            if (fieldDataType === 'Time') {
                                                defaultValueString = isDefaultHasNativeValue(defaultValueString) ? Globalize.format(spUtils.translateFromServerStorageDateTime(defaultValueString), 't') : defaultValueString;
                                            } else {
                                                defaultValueString = defaultValueString.toString();
                                            }
                                        }
                                    }
                                    if ($scope.isNewEntity || defaultValueString !== existingDefaultValueString) {
                                        $scope.model.fieldToRender.setDefaultValue(defaultValueString);
                                    }

                                    if ($scope.model.showStringPattern && field.type && field.type.alias && field.type.alias() === 'core:stringField') {
                                        //set string pattern value
                                        if ($scope.isNewEntity || initialState.patternId !== sp.result($scope.model, 'pattern.id')) {
                                            $scope.model.fieldToRender.setPattern($scope.model.pattern);
                                            setFieldRepresentsRelationship($scope.model.fieldToRender, $scope.model.pattern);
                                        }
                                    }

                                    if ($scope.model.showDecimalPlaces && field.type && field.type.alias && (field.type.alias() === 'core:decimalField' || field.type.alias() === 'core:currencyField')) {
                                        //set decimalPlaces value
                                        var decimalPlaces = $scope.model.fieldData.getField($scope.fields[4].id());
                                        var existingDecimalPlaces = initialState.fieldData.getField($scope.fields[4].id());

                                        if ($scope.isNewEntity || decimalPlaces !== existingDecimalPlaces) {
                                            $scope.model.fieldToRender.setDecimalPlaces(decimalPlaces);
                                        }
                                    }
                                }
                                    break;
                            }
                        }
                    }
                }
                if ($scope.model.errors.length === 0 && valid && defaultValueValid) {
                    if ($scope.model.isFieldControl) {
                        $scope.modalInstance.close($scope.model.formControl);
                    } else {
                        $scope.modalInstance.close($scope.model.fieldToRender);
                    }

                }
            };


            // Cancel click handler
            $scope.cancel = function () {
                $scope.modalInstance.close(false);
            };


        });
}());