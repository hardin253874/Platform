// Copyright 2011-2016 Global Software Innovation Pty Ltd
/*global console, _, angular */

(function() {
    'use strict';

    /**
    * Module implementing a choice field properties.
    * choicePropertiesController displays the properties of all choice field to configure
    *choiceProperties is a directive to pass the options(Schema info) from parents to current scope.
    *
    *directive
    * @module choicePropertiesController
    * @example

    Using the choiceProperties:

    &lt;choice-properties options="options" modal-instance="modalInstance&gt;&lt;/choice-properties&gt

    where options is available on the controller with the following properties:
        - formControl {object} - relationship/formControl object depending on configure button clicked from.            
        -isFieldControl {bool} - True-if configuring properties of form control. False - If configuring properties of a choice field from definition
        -relationshipType{string}-'choice' for edititng choice field relationship
    modalInstance is a modalinstance of a dialog to close and return the value to the parent window.
    */
    angular.module('mod.app.configureDialog.choiceFieldProperties', ['ui.bootstrap', 'mod.app.editForm', 'mod.app.editForm.designerDirectives', 'mod.common.ui.spDialogService', 'ngGrid', 'ngGrid.services', 'mod.common.spEntityService', 'sp.navService', 'mod.app.configureDialog.relationshipProperties.spRelationshipFilters', 'spApps.enumValueService', 'mod.app.configureDialog.fieldPropertiesHelper', 'mod.app.formBuilder.services.spFormBuilderService'])
        .directive('choiceProperties', function() {
            return {
                restrict: 'E',
                transclude: false,
                replace: true,
                scope: {
                    options: '=?',
                    modalInstance: '=?'
                },
                templateUrl: 'configDialogs/relationshipsProperties/choiceFieldProperties/views/choiceFieldProperties.tpl.html',
                controller: 'choicePropertiesController'
            };
        })        
        .controller('choicePropertiesController', function ($scope, $templateCache, spEditForm, spFieldValidator, configureDialogService, spEntityService, spNavService, spEnumValueService, fieldPropertiesHelper, spFormBuilderService) {

            $scope.isCollapsed = true;


            var schemaInfoLoaded = false;
            var entityLoaded = false;
            var controlLoaded = false;
            var formControls = [];
            var originalRows = [];
            var initializingOption = true;            
           // var isDefaultValueSet = false;
            
            //set bookmark on form control object
            var history = $scope.options.formControl.graph.history;
            var bookmark = history.addBookmark();
            var nameSanitizer, descriptionSanitizer;
            
            //get option text 
            $scope.getOptionsText = function() {
                if ($scope.isCollapsed === true) {
                    $scope.imageUrl = 'assets/images/arrow_down.png';
                    return 'Options';
                } else {
                    $scope.imageUrl = 'assets/images/arrow_up.png';
                    return 'Options';
                }
            };
      
            //define model object;
            $scope.model = {
                errors: [],
                isFormControl: $scope.options.isFormControl,
                formMode: 'edit',
                isReadOnly: false,
                isInTestMode: false,
                isFormValid: true,
                choiceValues: [],
                choiceValuesDeleted:[],
                choiceValuesGridOptions: {
                    data: 'model.choiceValues',
                    multiSelect:false,
                    enableSorting: false,
                    enableCellEdit: true,
                    selectedItems:[],
                    columnDefs: [
                        {
                            field: 'name',
                            displayName: 'Name',
                            sortable: false,
                            groupable: false,
                            enableCellEdit: true,
                            cellEditableCondition: 'canEditChoiceValueCell(row)'
                        },
                        {
                            field: 'description',
                            displayName: 'Description',
                            sortable: false,
                            groupable: false,
                            enableCellEdit: true,
                            cellEditableCondition: 'canEditChoiceValueCell(row)'
                        }
                    ],
                    afterSelectionChange:function(row) {
                        $scope.selectedRowIndex = row.rowIndex;
                    }
                },
                typeOptions: [{ name: 'Single select', value: 'single' }, { name: 'Multi select', value: 'multi' }],
                selectedType:{},
                choiceFieldOptions: [{ name: 'New', value: 'new' }, { name: 'Use Existing', value: 'useExisting' }],
                selectedOption: {},
                selectedHorizontalMode: {},
                selectedVerticalMode: {},
                busyIndicator: {
                    type: 'spinner',
                    text: 'Loading...',
                    placement: 'element',
                    isBusy: true
                },
                initialState: {}
            };

            $scope.canEditChoiceValueCell = function (row) {
                return canModifyEntity(sp.result(row, 'entity'));
            };
            
            // Clear any errors
            $scope.model.clearErrors = function () {
                $scope.model.errors = [];
                $scope.model.isFormValid = true;
            };

            // Add an error
            $scope.model.addError = function (errorMsg) {
                $scope.model.errors.push({ type: 'error', msg: errorMsg });
                $scope.model.isFormValid = false;
            };

            $scope.isPickerVisible = function() {
                if ($scope.model.selectedOption && $scope.model.selectedOption.value === 'useExisting') {
                    return true;
                }
                return false;
            };

            //
            //Add new choice value item to the list.
            //
            $scope.addChoiceFieldValue = function () {
                var appId = spNavService.getCurrentApplicationId();

                var newEntity = spEntity.fromJSON({
                    name: getNextAvailableValueName(),
                    description: jsonString(),
                    enumOrder: jsonInt()
                });

                if (appId) {
                    newEntity.setLookup('core:inSolution', appId);
                }

                $scope.model.choiceValues.push(newEntity);
                $scope.model.clearErrors();
                $scope.model.isFormValid = true;                
            };
            //
            //Get next available new value text to create new choice value.
            //
            function getNextAvailableValueName() {
                var foundName = false;
                var count = 0;
                var valueString = 'New Value';
                var valueName = valueString;
                if ($scope.model.choiceValues.length > 0) {
                    while (!foundName) {
                        if (count !== 0) {
                            valueName = valueString + count;
                        }
                        var serchValue = searchChoiceValue(valueName);
                        if (serchValue) {
                            count++;
                        } else {
                            foundName = true;
                        }
                    }
                }
                return valueName;
            }
            
            function searchChoiceValue(valueName) {
               return  _.find($scope.model.choiceValues, function (value) {
                   return value.name === valueName;
                });
            }
         
            //
            //Move the choicevalue item up in the list.
            //
            $scope.moveUpChoiceFieldValue = function () {
                if ($scope.isUpButtonDisabled()) {
                    return;
                }

                //var selectedIndex = $scope.selectedRowIndex;
                if ($scope.selectedRowIndex > 0) {                    
                    var arr = $scope.model.choiceValues.concat();
                    arr = arraymove(arr, $scope.selectedRowIndex, $scope.selectedRowIndex - 1);
                    //refresh grid
                    $scope.model.choiceValues = arr;
                    $scope.selectedRowIndex = $scope.selectedRowIndex - 1;
                    var grid = $scope.model.choiceValuesGridOptions.ngGrid;
                    if(grid)
                        grid.$viewport.scrollTop(grid.rowMap[$scope.selectedRowIndex] * grid.config.rowHeight);
                }
            };
            
            //
            //Move the choicevalue item down in the list.
            //
            $scope.moveDownChoiceFieldValue = function () {
                if ($scope.isDownButtonDisabled()) {
                    return;
                }

              //  var selectedIndex = $scope.selectedRowIndex;
                if ($scope.selectedRowIndex !== $scope.model.choiceValues.length - 1) {                    
                    var arr = $scope.model.choiceValues.concat();
                    arr = arraymove(arr, $scope.selectedRowIndex, $scope.selectedRowIndex + 1);
                    //refresh grid
                    $scope.model.choiceValues = arr;
                    $scope.selectedRowIndex = $scope.selectedRowIndex+1;
                    var grid = $scope.model.choiceValuesGridOptions.ngGrid;
                    if (grid) {
                        var noOfRowsToScroll = parseInt(grid.$viewport.height() / grid.config.rowHeight);
                        grid.$viewport.scrollTop(grid.rowMap[Math.max(0, ($scope.selectedRowIndex - noOfRowsToScroll))] * grid.config.rowHeight);
                    }
                }
            };
            
            // Driver function support
            $scope.clearExistingValues = function() {
                $scope.model.choiceValuesDeleted = _.filter($scope.model.choiceValues,
                    function(c) { return c.getDataState() !== spEntity.DataStateEnum.Create; });
                _.forEach($scope.model.choiceValuesDeleted, function(c) { c.setDataState(spEntity.DataStateEnum.Delete); });
                $scope.model.choiceValues = [];
            };

            //
            //Delete the choice value item from the list.
            //
            $scope.deleteChoiceFieldValue = function () {
                if ($scope.isDeleteButtonDisabled()) {
                    return;
                }

                var selectedEntity = $scope.model.choiceValues[$scope.selectedRowIndex];
                if (selectedEntity && selectedEntity.getDataState() !== spEntity.DataStateEnum.Create) {
                    selectedEntity.setDataState(spEntity.DataStateEnum.Delete);
                    $scope.model.choiceValuesDeleted.push(selectedEntity);
                } 
                var arr = $scope.model.choiceValues.concat();
                arr.splice($scope.selectedRowIndex, 1);
                $scope.model.choiceValues = arr;
                $scope.model.choiceValuesGridOptions.selectedItems = [];
                $scope.selectedRowIndex = null;                
            };

            //
            //Get if new button to add choice value is disabled.
            //
            $scope.isNewButtonDisabled = function() {
                if ($scope.isNewRelationship && $scope.model.selectedOption.value === 'useExisting') {
                    var relEntity = $scope.model.choiceValuePickerOptions.selectedEntities;
                    if (relEntity && relEntity.length===0)
                       return true;
                }
                return false;
            };
            
            //
            //Get if up button to move choice value is disabled.
            //
            $scope.isUpButtonDisabled = function () {
                var selectedEntity, previousEntity;

                if ($scope.selectedRowIndex) {
                    selectedEntity = $scope.model.choiceValues[$scope.selectedRowIndex];
                    previousEntity = $scope.model.choiceValues[$scope.selectedRowIndex - 1];

                    return !(canModifyEntity(selectedEntity) && canModifyEntity(previousEntity));
                } else {
                    return true;
                }
            };
            
            //
            //Get if down button to move choice value is disabled.
            //
            $scope.isDownButtonDisabled = function () {
                var selectedEntity, nextEntity;

                if (($scope.selectedRowIndex || $scope.selectedRowIndex === 0) && ($scope.selectedRowIndex !== $scope.model.choiceValues.length - 1)) {
                    selectedEntity = $scope.model.choiceValues[$scope.selectedRowIndex];
                    nextEntity = $scope.model.choiceValues[$scope.selectedRowIndex + 1];

                    return !(canModifyEntity(selectedEntity) && canModifyEntity(nextEntity));
                } else {
                    return true;
                }
            };
            
            //
            //Get if delete button to delete choice value is disabled.
            //
            $scope.isDeleteButtonDisabled = function () {
                var selectedEntity;

                if ($scope.selectedRowIndex || $scope.selectedRowIndex === 0) {
                    selectedEntity = $scope.model.choiceValues[$scope.selectedRowIndex];

                    return !canDeleteEntity(selectedEntity);
                } else {
                    return true;
                }
            };                                

            //
            //Move the array item from fromIndex to toIndex.
            //
            function arraymove(arr, fromIndex, toIndex) {
                var element = arr.splice(fromIndex, 1)[0];
                arr.splice(toIndex, 0, element);
                return arr;
            }
            
            //
            //Save the row entity before start editing the cell.
            //
            $scope.$on('ngGridEventStartCellEdit', function (evt) {
                originalRows[evt.targetScope.row.entity.nsAliasOrId] = evt.targetScope.row.entity.cloneDeep();
                $scope.editedRowIndex = $scope.selectedRowIndex;
            });

            //
            // Validate name and description values and check for the duplicates in name value.
            //
            $scope.$on('ngGridEventEndCellEdit', function(event) {
                console.log('ngGridEventEndCellEdit fired');
                //get edited entity
                var entity = event.targetScope.row.entity;
                if (event.targetScope.col.index === 0) {
                    //validate name
                    entity.name = validateName(entity.name);
                    entity.name = nameSanitizer(entity.name);
                    entity.name = getValidatedName(entity);
                } else {
                    //validate description
                    entity.description = descriptionSanitizer(entity.description);
                }

            });
            
            //
            //If the edited value is already present in the array replace the value with old value.
            //
            function getValidatedName(value) {
                var valueName = value.name;
                //find if the name is exist
                var index = 0;
                var foundName = false;
                if (valueName) {
                    _.forEach($scope.model.choiceValues, function(value) {
                        if (value.name.toString().toLowerCase() === valueName.toString().toLowerCase() && index !== $scope.editedRowIndex) {
                            foundName = true;
                        }
                        index++;
                    });
                    if (foundName) {
                        return originalRows[value.nsAliasOrId].name;
                    }
                } else {
                    return originalRows[value.nsAliasOrId].name;
                }
                return valueName;
            }
            
            //
            //Remove angular brackets in name value.
            //
            function validateName(value) {
                if (value) {
                    return value.replace(/[<>]+/g, '');
                }
                return value;
            }

            //
            //Get schema info to configure choice field properties.
            //
            $scope.schemaInfo = configureDialogService.getSchemaInfoForChoiceField().then(function (result) {
                $scope.fields = result.fields;
                $scope.enumTypeReport = result.enumTypeReport;
                $scope.enumType = result.enumType;
                $scope.resizeModes = _.sortBy(result.resizeModes, 'enumOrder');
                nameSanitizer = spFieldValidator.getSanitizer(result.fields[0]);
                descriptionSanitizer = spFieldValidator.getSanitizer(result.fields[1]);
                schemaInfoLoaded = true;
                if (schemaInfoLoaded && entityLoaded) {
                    loadTheControl();
                    $scope.model.busyIndicator.isBusy = false;
                }
            });
            
            //
            // Get relationship entity from the database.
            //
            $scope.requestChoiceRelationshipEntity = function () {

                configureDialogService.getChoiceRelationshipEntity($scope.model.relationshipToRender.idP).then(function (result) {
                        entityLoaded = true;
                        //Augemt the model field with the result to keep the client side changes.
                        spEntity.augment($scope.model.relationshipToRender, result, null);
                        if (schemaInfoLoaded && entityLoaded) {
                            loadTheControl();
                            $scope.model.busyIndicator.isBusy = false;
                        }
                    },
                    function (error) {
                        $scope.model.addError(error);
                        entityLoaded = true;
                    });
            };
            
            //
            // Get formControl from the database.
            //
            $scope.requestChoiceRelationshipControlEntity = function () {
                configureDialogService.getChoiceFieldControlEntity($scope.options.formControl.idP).then(function (result) {
                        //Augemt the model field with the result to keep the client side changes.
                        spEntity.augment($scope.options.formControl, result, null);
                        $scope.model.formControl = $scope.options.formControl;
                        entityLoaded = true;
                        $scope.model.relationshipToRender = $scope.model.formControl.relationshipToRender;
                        if (schemaInfoLoaded && entityLoaded) {
                            loadTheControl();
                            $scope.model.busyIndicator.isBusy = false;
                        }
                    },
                    function (error) {
                        $scope.model.addError(error);
                        entityLoaded = true;
                    });
            };

            //
            //Load the controls aftre all service calls finished.
            //
            function loadTheControl() {
                if (controlLoaded)
                    return;
                controlLoaded = true;

                //define formData
                $scope.model.formData = spEntity.createEntityOfType('relationshipEntityType');

                //define controls
                $scope.model.choiceNameControl = configureDialogService.getDummyFieldControlOnForm($scope.fields[0], 'Name');
                $scope.model.choiceNameControl.mandatoryControl = true;
                $scope.model.choiceDescriptionControl = configureDialogService.getDummyFieldControlOnForm($scope.fields[1], 'Description');
                $scope.model.scriptNameControl = configureDialogService.getDummyFieldControlOnForm($scope.fields[2], 'Script Name');
                $scope.model.scriptNameControl.mandatoryControl = true;
                if ($scope.options.isFormControl) {
                    formControls.push($scope.model.choiceNameControl, $scope.model.scriptNameControl);
                } else {
                    formControls.push($scope.model.choiceNameControl, $scope.model.choiceDescriptionControl, $scope.model.scriptNameControl);
                }
                var relName = '';
                if ($scope.model.relationshipToRender.name && $scope.model.relationshipToRender.name.toLowerCase().indexOf("new \'choice\' field") === -1) {
                    relName = $scope.model.relationshipToRender.toName || $scope.model.relationshipToRender.name;
                }
                setFormDataField($scope.model.formData, $scope.fields[0], relName);
                if (!$scope.options.isFormControl) 
                    setFormDataField($scope.model.formData, $scope.fields[1], $scope.model.relationshipToRender.description);

                var scriptName = $scope.model.relationshipToRender.toScriptName || relName;
                setFormDataField($scope.model.formData, $scope.fields[2], scriptName);

                if ($scope.options.isFormControl) {
                    $scope.model.choiceDisplayNameControl = configureDialogService.getDummyFieldControlOnForm($scope.fields[0], 'Display Name');
                    $scope.model.choiceDescriptionControl = configureDialogService.getDummyFieldControlOnForm($scope.fields[1], 'Description');
                    $scope.model.formControlData = getDummyFormData('controlEntityType', $scope.fields[0], $scope.model.formControl.name);                    
                    $scope.model.formControlData.setField($scope.fields[1], $scope.model.formControl.description, spEntity.DataType.String);
                }

                

                if ($scope.isNewRelationship) {
                    $scope.model.selectedOption = $scope.model.choiceFieldOptions[0];
                }
                    
                //get the instances of type if the totype is existing enumtype
                if ($scope.model.relationshipToRender.toType && $scope.model.relationshipToRender.toType.getDataState() !== spEntity.DataStateEnum.Create) {
                    $scope.model.selectedOption = $scope.model.choiceFieldOptions[1];
                    getChoiceValuesOfType($scope.model.relationshipToRender.toType.idP);
                }

                $scope.model.choiceValuePickerOptions = {
                    entityTypeId: $scope.enumType.idP,
                    selectedEntity: null,
                    selectedEntities: null,
                    pickerReportId: $scope.enumTypeReport.idP,
                    multiSelect: false,
                    isDisabled: !$scope.isNewRelationship
                };

                if ($scope.model.relationshipToRender.toType && $scope.model.relationshipToRender.toType.getDataState() !== spEntity.DataStateEnum.Create) {
                    $scope.model.choiceValuePickerOptions.selectedEntities = [$scope.model.relationshipToRender.toType];
                } 
                  
                //Load base the controls
                $scope.baseControlsFile = 'configDialogs/relationshipsProperties/choiceFieldProperties/views/choiceControlBaseProperties.tpl.html';
                if ($scope.options.isFormControl) {
                    //set resize options
                    $scope.model.resizeOptions = {
                        resizeModes: $scope.resizeModes,
                        isHresizeModeDisabled: false,
                        isVresizeModeDisabled: true
                    };
                      
                    //load the form properties controls
                    $scope.formControlFile = 'configDialogs/relationshipsProperties/choiceFieldProperties/views/choiceFormProperties.tpl.html';
                    $scope.formatControlFile = 'configDialogs/relationshipsProperties/choiceFieldProperties/views/choiceFormatProperties.tpl.html';
                }
                    
                //set selected type Option
                if ($scope.model.relationshipToRender.cardinality.alias() === 'core:manyToOne') {
                    $scope.model.selectedType = $scope.model.typeOptions[0];
                } else {
                    $scope.model.selectedType = $scope.model.typeOptions[1];
                }
                 
                $scope.definitionControlFile = 'configDialogs/relationshipsProperties/choiceFieldProperties/views/choiceDefinitionProperties.tpl.html';
                //
                //Get the choice values if the selected entity changed.
                //
                $scope.$watch('model.choiceValuePickerOptions.selectedEntities', function (newVal, oldVal) {
                    if (spUtils.isNullOrUndefined(newVal) && spUtils.isNullOrUndefined(oldVal))
                        return;
                    if (newVal === oldVal)
                        return;
                    var relEntities = $scope.model.choiceValuePickerOptions.selectedEntities;
                    if (relEntities[0] && relEntities[0].idP) {
                        getChoiceValuesOfType(relEntities[0].idP);                        
                        var oldName = spUtils.isNonEmptyArray(oldVal) ? oldVal[0].name : null;
                        if ($scope.isNewRelationship) {
                            //update the choice relationship name if name field is empty or name field is updated as choice field name
                            var updateName = false;
                            if ($scope.model.formData.name) {
                                if (!spUtils.isNullOrUndefined(oldName) && oldName === $scope.model.formData.name)
                                    updateName = true;
                            } else {
                                updateName = true;
                            }
                            if (updateName) {
                                $scope.model.formData.name = relEntities[0].name;
                                //validate the field value
                                if ($scope.model.choiceNameControl.validateOnSchemaChange) {
                                    $scope.model.choiceNameControl.validateOnSchemaChange($scope.model.formData);
                                }
                            }
                        }
                    }
                });
                    
                //
                //clear the choice values if the otion value is changed.
                //
                $scope.$watch('model.selectedOption', function (newVal, oldVal) {
                    if (spUtils.isNullOrUndefined(newVal) && spUtils.isNullOrUndefined(oldVal))
                        return;
                    if (initializingOption) {
                        initializingOption = false;
                    } else {
                        if ($scope.model.selectedOption.value === 'new') {
                            //set the toType relationship to null
                            if ($scope.model.toTypeRelationship)
                                $scope.model.toTypeRelationship.setRelationship($scope.model.toTypeRelationship.idP, null);
                        } 
                        //clear the choice values
                        $scope.model.choiceValues = [];
                        $scope.selectedRowIndex = null;
                    }
                });

                //
                //clear the choice values if the otion value is changed.
                //
                $scope.$watch('model.formData.name', function (newVal, oldVal) {
                    if ($scope.isNewRelationship) {
                        if ($scope.model.formData.toScriptName === oldVal)
                            $scope.model.formData.toScriptName = newVal;
                    }
                });

                $scope.model.initialState.formData = $scope.model.formData.cloneDeep();
            }
            
            //
            //get dummy formData
            //
            function getDummyFormData(entityType, field, value) {
                var formDataEntity = spEntity.createEntityOfType(entityType);
                var dbType = spEntityUtils.dataTypeForField(field);
                formDataEntity.setField(field.id(), value, dbType);

                return formDataEntity;
            }
            
            //
            //set the formdata Field.
            //
            function setFormDataField(formData, field, value) {
                var dbType = spEntityUtils.dataTypeForField(field);
                formData.registerField(field.eid(), dbType);
                formData.setField(field.id(), value, dbType);
            }
            
            //
            //Get the default value.
            //
            function getDefaultValue() {
                //set the default value if there is any.
                var defalutValue = $scope.model.relationshipToRender.toTypeDefaultValue;
                if (defalutValue) {
                    $scope.model.toTypeDefaultValue = _.find($scope.model.choiceValues, function (value) { return value.name === defalutValue.name; });
                } else {
                    $scope.model.toTypeDefaultValue = null;
                }

                $scope.model.initialState.defaultValue = $scope.model.toTypeDefaultValue;                
            }

            //
            //Get the choice values of selected type.
            //
            function getChoiceValuesOfType(idP) {
                //get the choice values of selected type
                configureDialogService.getChoiceValuesOfType(idP).then(function (result) {
                    //set toType of the relationship
                    $scope.model.relationshipToRender.setToType(result);

                    var values = _.sortBy(result.instancesOfType, function (choiceValue) {
                        return choiceValue.enumOrder;
                    });
                    $scope.model.choiceValues = values;
                    getDefaultValue();
                  
                    $scope.model.clearErrors();
                    $scope.model.isFormValid = true;
                });
            }

            var toTypeEntity = spEntity.fromJSON({
                name: jsonString(''),
                typeId: 'core:enumType',
                inherits: [{ id: 'core:enumValue' }],
                instancesOfType: [],
                defaultPickerReport: jsonLookup()
            });

            var choiceRelationshipDefaultObject = {
                name: jsonString(''),
                description: jsonString(''),
                typeId: 'core:relationship',
                toName: jsonString(''),
                fromName: jsonString(''),
                toType: jsonLookup(toTypeEntity),
                cardinality: jsonLookup('core:manyToOne'),
                relationshipIsMandatory: false,
                revRelationshipIsMandatory: false,
                toTypeDefaultValue: jsonLookup(),
                fromTypeDefaultValue: jsonLookup(),
                isRelationshipReadOnly: false,
                relType:jsonLookup('core:relChoiceField'),
                cascadeDelete: false,
                cascadeDeleteTo: false,
                cloneAction:jsonLookup('core:cloneReferences'),
                reverseCloneAction:jsonLookup('core:drop'),
                implicitInSolution: false,
                reverseImplicitInSolution: false
            };

            var formControlOnlyObject = {
                name: jsonString(''),
                description: jsonString(''),
                typeId: 'console:choiceRelationshipRenderControl',
                'console:renderingBackgroundColor': 'white',
                'console:renderingHorizontalResizeMode': jsonLookup('console:resizeAutomatic'),
                'console:renderingVerticalResizeMode': jsonLookup('console:resizeAutomatic'),
                'console:mandatoryControl': false,
                'console:showControlHelpText': false,
                'console:readOnlyControl': false
            };
           
            //
            //Returns a template function for the form control only object.
            //
            function getTempFnForChoiceFieldControlOnly() {
                var templateFn = function (type) {
                    if (type === 'console:choiceRelationshipRenderControl')
                        return spEntity.fromJSON(formControlOnlyObject);
                    else {
                        return null;
                    }
                };
                return templateFn;
            }

            //
            //Returns a template function for the form relationship object.
            //
            function getTempFnForChoiceRelationship() {
                var templateFn = function (type) {
                    if (type === 'core:relationship')
                        return spEntity.fromJSON(choiceRelationshipDefaultObject);
                    else
                        return null;
                };
                return templateFn;
            }

            //check if its a new entity.
            $scope.isNewEntity = $scope.options.formControl.getDataState() === spEntity.DataStateEnum.Create ? true : false;
            $scope.isNewRelationship = false;

            //get formcontrol info if user cinfiguring the control or get relationship info if user configuring choice field.
            if ($scope.options.isFormControl) {
                if ($scope.isNewEntity) {
                    $scope.model.formControl = $scope.options.formControl;
                    $scope.model.relationshipToRender = $scope.options.formControl.relationshipToRender;
                    //augment the control only.
                    spEntity.augment($scope.model.formControl, null, getTempFnForChoiceFieldControlOnly());
                    //check if the relationship is existing
                    if ($scope.model.relationshipToRender.getDataState() === spEntity.DataStateEnum.Create) {
                        entityLoaded = true;
                        $scope.isNewRelationship = true;
                        spEntity.augment($scope.model.relationshipToRender, null, getTempFnForChoiceRelationship());
                    } else {
                        //get field from the Database
                        $scope.requestChoiceRelationshipEntity();
                    }
                } else {
                    //get relationship Control from the Database
                    $scope.requestChoiceRelationshipControlEntity();
                }
                $scope.model.isFormDetailEnabled = true;

            } else {
                $scope.model.relationshipToRender = $scope.options.formControl;
                if ($scope.isNewEntity) {
                    entityLoaded = true;
                    $scope.isNewRelationship = true;
                    spEntity.augment($scope.model.relationshipToRender, null, getTempFnForChoiceRelationship());
                } else {
                    //get relationship from the Database
                    $scope.requestChoiceRelationshipEntity();
                }
                $scope.model.isObjectTabActive = true;
            }

            function validateForm() {

                var scriptField = _.find($scope.fields, function (f) {
                    return f.alias() === 'core:toScriptName';
                });

                var newScriptName = $scope.model.formData.getField(scriptField.id());

                var relationshipId = $scope.model.relationshipToRender.id();

                var fields = fieldPropertiesHelper.getSelectedType(spFormBuilderService).getFields();

                if (fields) {
                    var nameField = _.find($scope.fields, function (f) {
                        return f.alias() === 'core:name';
                    });

                    if (nameField) {
                        //check for duplicate names

                        var newName = $scope.model.formData.getField(nameField.id());

                        if (_.some(fields, function (field) {
							var fieldEntity = field.getEntity();
							return fieldEntity && fieldEntity.name && fieldEntity.name.toLowerCase().trim() === newName.toLowerCase().trim() && fieldEntity.id() !== relationshipId;
                        })) {
                            $scope.model.addError('The field name \'' + newName + '\' already exists on this object');
                            return;
                        }

                        if (scriptField) {
                            // check for script name clashes

                            var scriptName = getTypeScriptName();

                            if (scriptName && newName.toLowerCase().trim() === scriptName.toLowerCase().trim()) {
                                $scope.model.addError('The field name \'' + newName + '\' is currently being used as the object\'s script name');
                                return;
                            }

                            if (newScriptName && scriptName && newScriptName.toLowerCase().trim() === scriptName.toLowerCase().trim()) {
                                $scope.model.addError('The script name \'' + newScriptName + '\' is currently being used as the object\'s script name');
                                return;
                            }

                            if (_.some(fields, function (field) {
								var fieldEntity = field.getEntity();
								return fieldEntity && fieldEntity.fieldScriptName && fieldEntity.fieldScriptName.toLowerCase().trim() === newScriptName.toLowerCase().trim() && fieldEntity.id() !== relationshipId;
                            })) {
                                $scope.model.addError('The field script name \'' + newScriptName + '\' already exists on a different field');
                                return;
                            }
                        }
                    }
                }

                var relationships = fieldPropertiesHelper.getSelectedType(spFormBuilderService).getAllRelationships();

                if (relationships) {
                    if (scriptField) {

                        if (_.some(relationships, function (relationship) {
							var relationshipEntity = relationship.getEntity();
                            if (relationshipEntity && relationshipEntity.toScriptName) {
                                return relationshipEntity.toScriptName.toLowerCase().trim() === newScriptName.toLowerCase().trim() && relationshipEntity.id() !== relationshipId;
                            } else {
                                return relationshipEntity.name.toLowerCase().trim() === newScriptName.toLowerCase().trim() && relationshipEntity.id() !== relationshipId;
                            }
                        })) {
                            $scope.model.addError('The choice script name \'' + newScriptName + '\' already exists on a different relationship');
                            return;
                        }
                    }
                }
            }

            /**
            * Gets the Type script name
            */
            function getTypeScriptName() {
                var targetType = fieldPropertiesHelper.getSelectedType(spFormBuilderService);

                if (targetType && targetType.getEntity) {
                    var type = targetType.getEntity();

                    if (type && type.typeScriptName) {
                        return type.typeScriptName;
                    }
                }

                return null;
            }
            
            // OK click handler
            $scope.ok = function () {
                $scope.model.clearErrors();
                if ($scope.form.$valid) {
                    validateForm();
                    if ($scope.model.errors.length === 0) {
                        //validate all the form controls.
                        if (!spEditForm.validateFormControls(formControls, $scope.model.formData))
                            return;
                        //check if the choice values defined or not.
                        if ($scope.model.choiceValues.length === 0) {
                            $scope.model.clearErrors();
                            $scope.model.addError('At least one choice value has to be defined.');
                            $scope.model.isFormValid = false;
                            return;
                        }
                        var relationshipToRender;
                        if ($scope.options.isFormControl) {
                            relationshipToRender = $scope.options.formControl.relationshipToRender;
                        } else {
                            relationshipToRender = $scope.options.formControl;
                        }

                        if ($scope.isNewRelationship) {
                            //set cardinality
                            if ($scope.model.selectedType.value === 'single') {
                                relationshipToRender.setCardinality('manyToOne');
                            } else {
                                relationshipToRender.setCardinality('manyToMany');
                            }    
                        }                        

                        if ($scope.options.isFormControl) {
                            var disName = $scope.model.formControlData.getField($scope.fields[0].idP);
                            if (disName)
                                disName = disName.trim();
                            $scope.options.formControl.setName(disName);

                            //set description
                            $scope.options.formControl.setField('core:description', $scope.model.formControlData.getField($scope.fields[1].idP), spEntity.DataType.String);
                           
                            //show help text
                            $scope.options.formControl.setField('console:showControlHelpText', $scope.model.formControl.showControlHelpText, spEntity.DataType.Bool);


                            if (relationshipToRender.cardinality.alias() === 'core:manyToOne') {
                                $scope.options.formControl.type = new spEntity.EntityRef('console:choiceRelationshipRenderControl');
                            } else {
                                $scope.options.formControl.type = new spEntity.EntityRef('console:multiChoiceRelationshipRenderControl');
                            }
                        }

                        var initialState = $scope.model.initialState;

                        //set name and description of the relationship.
                        var relName = _.trim($scope.model.formData.getField($scope.fields[0].idP));
                        var existingName = _.trim(initialState.formData.getField($scope.fields[0].idP));

                        if ($scope.isNewRelationship || relName !== existingName) {
                            relationshipToRender.setName(relName);
                            relationshipToRender.setToName(relName);    
                        }
                        
                        if ($scope.isNewRelationship || $scope.model.formData.getField($scope.fields[1].idP) !== initialState.formData.getField($scope.fields[1].idP)) {
                            relationshipToRender.setDescription($scope.model.formData.getField($scope.fields[1].idP));
                        }                        

                        var relScriptName = _.trim($scope.model.formData.getField($scope.fields[2].idP));
                        var existingScriptName = _.trim(initialState.formData.getField($scope.fields[2].idP));

                        if ($scope.isNewRelationship || relScriptName !== existingScriptName)
                            relationshipToRender.toScriptName = relScriptName;

                        //set the toType entity if its a new relationship
                        if ($scope.isNewRelationship && $scope.model.selectedOption.value === 'new') {
                            var entity = spEntity.fromJSON({
                                name: $scope.model.relationshipToRender.name,
                                typeId: 'core:enumType',
                                inherits: [{ id: 'core:enumValue' }],
                                defaultPickerReport: jsonLookup($scope.enumTypeReport.idP),
                                inSolution: jsonLookup()
                            });

                            var appId = spNavService.getCurrentApplicationId();

                            if (appId) {
                                entity.setInSolution(appId);
                            }

                            //save the new entity to the database
                            spEntityService.putEntity(entity).then(function(result) {
                                //set totype
                                relationshipToRender.setToType(result);

                                var order = 1;
                                //Set the emun order of choice values
                                _.forEach($scope.model.choiceValues, function(value) {
                                    value.enumOrder = order++;
                                    value.type = result;
                                });
                                
                                var relEntity = spEntity.fromJSON({
                                    id: result,
                                    instancesOfType: $scope.model.choiceValues
                                });

                                spEntityService.putEntity(relEntity).then(function(id) {
                                    //instances also saved
                                    setDefaultValue(id);
                                }, function(error) {
                                    var errorText;

                                    if (error && error.data) {
                                        errorText = error.data.ExceptionMessage || error.data.Message;
                                    } else {
                                        errorText = error;
                                    }

                                    $scope.model.addError(errorText);
                                });
                            });

                        } else {
                            var order = 1;
                            var choiceFieldEntity = relationshipToRender.getToType();
                            var changedChoiceFields = [];
                            //Set the emun order of choice values
                            _.forEach($scope.model.choiceValues, function (value) {
                                if (canModifyEntity(value)) {
                                    value.enumOrder = order;    
                                }

                                order++;

                                if (value.getDataState() === spEntity.DataStateEnum.Create) {
                                    value.type = choiceFieldEntity.idP;
                                }

                                if (value.getDataState() !== spEntity.DataStateEnum.Unchanged) {
                                    // Value has changed
                                    changedChoiceFields.push(value);
                                }
                            });

                            if (changedChoiceFields.length || $scope.model.choiceValuesDeleted.length) {
                                // choiceFieldEntity.setInstancesOfType($scope.model.choiceValues);
                                var saveEntity = spEntity.fromJSON({
                                    id: choiceFieldEntity.idP
                                });
                                
                                saveEntity.registerRelationship('core:instancesOfType');
                                
                                _.forEach(changedChoiceFields, function(value) {
                                    saveEntity.instancesOfType.add(value);
                                });

                                _.forEach($scope.model.choiceValuesDeleted, function(value) {
                                    saveEntity.instancesOfType.remove(value);
                                });

                                spEntityService.putEntity(saveEntity).then(function(id) {
                                    //instances also saved
                                    setDefaultValue(id);
                                }, function(error) {
                                    var errorText;

                                    if (error && error.data) {
                                        errorText = error.data.ExceptionMessage || error.data.Message;
                                    } else {
                                        errorText = error;
                                    }

                                    $scope.model.addError(errorText);
                                });    
                            } else {
                                setDefaultValue(choiceFieldEntity.idP);
                            }                            

                            spEnumValueService.invalidateEnumValuesForEntityType(choiceFieldEntity.idP);
                        }

                        if ($scope.isNewRelationship) {
                            //set all the relationship properties.
                            if (relationshipToRender.cardinality.alias() === 'core:manyToOne') {
                                relationshipToRender.setRelType('relChoiceField');
                            } else {
                                relationshipToRender.setRelType('relMultiChoiceField');
                            }    

                            relationshipToRender.setCascadeDelete(false);
                            relationshipToRender.setCascadeDeleteTo(false);
                            relationshipToRender.setCloneAction('cloneReferences');
                            relationshipToRender.setReverseCloneAction('drop');
                            relationshipToRender.setImplicitInSolution(false);
                            relationshipToRender.setReverseImplicitInSolution(false);
                            //relationshipToRender.setRelationshipIsMandatory(false); // set by binding
                            relationshipToRender.setRevRelationshipIsMandatory(false);
                        }                                            
                    }
                }
            };
            
            //
            //Set the default value for the relationship.
            //
            function setDefaultValue(entityId) {
                if ($scope.model.toTypeDefaultValue && $scope.model.toTypeDefaultValue.getDataState() === spEntity.DataStateEnum.Create) {
                    //get the entity from database to set the default value
                    configureDialogService.getChoiceValuesOfType(entityId).then(function (result) {
                        //set toType of the relationship
                        $scope.model.relationshipToRender.setToType(result);
                        
                        var defaultValue = _.find(result.instancesOfType, function (value) { return value.name === $scope.model.toTypeDefaultValue.name; });
                        $scope.model.relationshipToRender.setToTypeDefaultValue(defaultValue);
                        bookmark.endBookmark();
                        //Close the Dialog.
                        $scope.modalInstance.close($scope.options.formControl);
                    });
                    
                } else {
                    if (sp.result($scope.model, 'initialState.defaultValue.id') !== sp.result($scope.model, 'toTypeDefaultValue.id')) {
                        $scope.model.relationshipToRender.setToTypeDefaultValue($scope.model.toTypeDefaultValue);
                    }
                    
                    bookmark.endBookmark();
                    //Close the Dialog.
                    $scope.modalInstance.close($scope.options.formControl);
                }
            }

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

            // Return true if the entity can be modified, false otherwise
            function canModifyEntity(entity) {
                return sp.result(entity, 'canModify') || sp.result(entity, 'dataState') === spEntity.DataStateEnum.Create;
            }

            // Return true if the entity can be deleted, false otherwise
            function canDeleteEntity(entity) {
                return sp.result(entity, 'canDelete') || sp.result(entity, 'dataState') === spEntity.DataStateEnum.Create;
            }
        });
}());