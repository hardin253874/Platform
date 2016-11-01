// Copyright 2011-2016 Global Software Innovation Pty Ltd
/*global console, _, angular */

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

    angular.module('mod.app.configureDialog.containerProperties', ['ui.bootstrap', 'mod.app.editForm', 'mod.app.editFormServices', 'mod.app.editForm.designerDirectives', 'mod.common.ui.spDialogService', 'mod.app.configureDialog.spVisibilityCalculationControl', 'mod.app.spFormControlVisibilityService'])
        .directive('containerProperties', function () {
            return {
                restrict: 'E',
                transclude: false,
                replace: true,
                scope: {
                    options: '=?',
                    modalInstance: '=?'
                },
                templateUrl: 'configDialogs/containerProperties/views/containerProperties.tpl.html',
                controller: 'containerPropertiesController'
            };
        })
        .controller('containerPropertiesController', function ($scope, spEditForm, configureDialogService, spFormControlVisibilityService) {
            var schemaInfoLoaded = false;
            var entityLoaded = false;
            var controlLoaded = false;
            
            //set bookmark on form control object
            var history = $scope.options.formControl.graph.history;
            var bookmark = history.addBookmark();
            
            //define model object;
            $scope.model = {
                errors: [],
                isFormControl: $scope.options.isFormControl,
                formMode: 'edit',
                isReadOnly: false,
                showHelpText: false,
                isInTestMode: false,
                selectedHorizontalMode: {},
                selectedVerticalMode: {},
                busyIndicator: {
                    type: 'spinner',
                    text: 'Loading...',
                    placement: 'element',
                    isBusy: true
                },
                visibilityCalculationModel: {
                    typeId: null,
                    error: null,
                    isShowHideOn: spFormControlVisibilityService.isShowHideFeatureOn()
                }
            };

            // Clear any errors
            $scope.model.clearErrors = function () {
                $scope.model.errors = [];
            };

            // Add an error
            $scope.model.addError = function (errorMsg) {
                $scope.model.errors.push({ type: 'error', msg: errorMsg });
            };
            
            //
            //Get schema info to configure container control properties.
            //
            $scope.schemaInfo = configureDialogService.getSchemaInfoForContainer().then(function (result) {
                $scope.field = result.field;
                $scope.description = result.description;
                var values = _.sortBy(result.resizeModes, function (choiceValue) {
                    return choiceValue.enumOrder;
                });
                $scope.resizeModes = values;

                var disabled = $scope.options ? !!$scope.options.isTab : false;

                //set resize options
                $scope.model.resizeOptions = {
                    resizeModes: values,
                    isHresizeModeDisabled: disabled,
                    isVresizeModeDisabled: disabled
                };
                schemaInfoLoaded = true;
                if (schemaInfoLoaded && entityLoaded) {
                    loadTheControl();
                    controlLoaded = true;
                    $scope.model.busyIndicator.isBusy = false;
                }
            },function (error) {
                $scope.model.addError(error);
                entityLoaded = true;
            });
            

            //
            // Get formControl from the database.
            //
            $scope.requestContainerControlEntity = function () {
                configureDialogService.getContainerControlEntity($scope.options.formControl.idP).then(function (result) {
                    //Augemt the model field with the result to keep the client side changes.
                    spEntity.augment($scope.options.formControl, result, null);
                    $scope.model.formControl = $scope.options.formControl;
                    entityLoaded = true;
                    if (schemaInfoLoaded && entityLoaded) {
                        loadTheControl();
                        controlLoaded = true;
                        $scope.model.busyIndicator.isBusy = false;
                    }
                },function (error) {
                        $scope.model.addError(error);
                        entityLoaded = true;
                    });
            };
            
            $scope.onVisibilityScriptCompiled = function (script, error) {
                $scope.model.visibilityCalculationModel.isScriptCompiling = false;

                $scope.model.visibilityCalculationModel.error = error;

                if (!error) {
                    $scope.model.formControl.visibilityCalculation = script;
                }
            };            

            $scope.onVisibilityScriptChanged = function () {
                $scope.model.visibilityCalculationModel.isScriptCompiling = true;
            };

            $scope.isOkDisabled = function() {
                return $scope.model.visibilityCalculationModel.isScriptCompiling;
            };

            function initVisibilityCalculationModel() {
                if ($scope.options &&
                    $scope.options.definition &&
                    $scope.options.definition.getDataState() !== spEntity.DataStateEnum.Create) {
                    $scope.model.visibilityCalculationModel.typeId = $scope.options.definition.idP;                    
                }
            }

            //
            //Load the controls after all service calls finished.
            //
            function loadTheControl() {
                if (!controlLoaded) {
                    //define controls
                    $scope.model.formNameControl = configureDialogService.getDummyFieldControlOnForm($scope.field, 'Name');
                    $scope.model.formNameControl.mandatoryControl = true;
                   
                    //define formData
                    $scope.model.formData = getDummyFormData('controlEntityType', $scope.field, $scope.model.formControl.name);

                    //load the form properties controls
                    $scope.controlsFile = 'configDialogs/containerProperties/views/baseControlProperties.tpl.html';

                    initVisibilityCalculationModel();
                }
            }
            
            //
            //get dummy formData
            //
            function getDummyFormData(entityType, nameField,  name) {
                var formDataEntity = spEntity.createEntityOfType(entityType);
                var dbType = spEntityUtils.dataTypeForField(nameField);
                formDataEntity.setField(nameField.id(), name, dbType);

                return formDataEntity;
            }
            

            var formControlObject = {
                name: jsonString(''),
                description: jsonString(''),
                typeId: 'console:verticalStackContainerControl',
                'console:renderingBackgroundColor': 'white',
                'console:renderingHorizontalResizeMode': jsonLookup('console:resizeAutomatic'),
                'console:renderingVerticalResizeMode': jsonLookup('console:resizeAutomatic'),
                'console:hideLabel': jsonBool(false),
                'console:showControlHelpText': jsonBool(false),
                'console:visibilityCalculation': jsonString(''),
            };
            
            //
            //Returns a template function for the form control only object.
            //
            function getTempFnForContainerControl(curretnType) {
                var templateFn = function (type) {
                    if (type === curretnType)
                        return spEntity.fromJSON(formControlObject);
                    else {
                        return null;
                    }
                };
                return templateFn;
            }
            
            //check if its a new entity.
            $scope.isNewEntity = $scope.options.formControl.getDataState() === spEntity.DataStateEnum.Create ? true : false;
            if ($scope.isNewEntity) {
                $scope.model.formControl = $scope.options.formControl;
                var controlType = $scope.model.formControl.type.alias();
                //augment the control.
                spEntity.augment($scope.model.formControl, null, getTempFnForContainerControl(controlType));
                entityLoaded = true;
            } else {
                //get relationship Control from the Database
                $scope.requestContainerControlEntity();
            }
            
            // OK click handler
            $scope.ok = function() {
                if ($scope.form.$valid &&
                    !$scope.model.visibilityCalculationModel.error) {
                    //validate all the form controls.
                    if (!spEditForm.validateFormControls([$scope.model.formNameControl], $scope.model.formData))
                        return;
                    
                    var controlName = $scope.model.formData.getField($scope.field.idP);
                    if (controlName)
                        controlName = controlName.trim();
                    $scope.options.formControl.name = controlName;

                    
                    $scope.options.formControl.description = $scope.model.formControl.description;
                    //Close the Dialog.
                    $scope.modalInstance.close($scope.options.formControl);
                }
            };
            
            // Cancel click handler
            $scope.cancel = function () {
                undoChanges();
            };

            $scope.$on('$locationChangeSuccess', function () {
                undoChanges();
            });

            function undoChanges() {
                history.undoBookmark(bookmark);
                $scope.modalInstance.close(false);
            }

        });
}());