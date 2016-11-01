// Copyright 2011-2016 Global Software Innovation Pty Ltd
/*global console, _, angular */

(function () {
    'use strict';

    /**
    * Module implementing a image field properties.
    * imagePropertiesController displays the properties of all image field to configure
    *imageProperties is a directive to pass the options(Schema info) from parents to current scope.
    *
    *directive
    * @module imagePropertiesController
    * @example

    Using the imageProperties:

    &lt;image-properties options="options" modal-instance="modalInstance&gt;&lt;/image-properties&gt

    where options is available on the controller with the following properties:
        - formControl {object} - relationship/formControl object depending on configure button clicked from.            
        -isFieldControl {bool} - True-if configuring properties of form control. False - If configuring properties of a image field from definition
    modalInstance is a modalinstance of a dialog to close and return the value to the parent window.
    */

    angular.module('mod.app.configureDialog.imageFieldProperties', ['ui.bootstrap', 'mod.app.editForm', 'mod.app.editForm.designerDirectives', 'mod.common.ui.spDialogService', 'mod.app.configureDialog.spVisibilityCalculationControl', 'mod.app.spFormControlVisibilityService'])
        .directive('imageProperties', function () {
            return {
                restrict: 'E',
                transclude: false,
                replace: true,
                scope: {
                    options: '=?',
                    modalInstance: '=?'
                },
                templateUrl: 'configDialogs/relationshipsProperties/imageFieldProperties/views/imageFieldProperties.tpl.html',
                controller: 'imagePropertiesController'
            };
        })
        .controller('imagePropertiesController', function ($scope, spEditForm, spFieldValidator, configureDialogService, spFormControlVisibilityService) {

            $scope.isCollapsed = true;
            var schemaInfoLoaded = false;
            var entityLoaded = false;
            var controlLoaded = false;
            
            //set bookmark on form control object
            var history = $scope.options.formControl.graph.history;
            var bookmark = history.addBookmark();

            //get option text 
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

            //define model object;
            $scope.model = {
                errors: [],
                isFormControl: $scope.options.isFormControl,
                formMode: 'edit',
                isReadOnly: false,
                isInTestMode: false,
                isFormValid: true,
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

            //Get schema info to configure image field properties.
            $scope.schemaInfo = configureDialogService.getSchemaInfoForImageField().then(function(result) {
                $scope.fields = result.fields;
                $scope.model.thumbNailSize =_.filter(result.thumbNailSize, function (thumbnail) {
                    return (!thumbnail.isSystemThumbnail);
                });
                $scope.model.thumbNailScaling = _.sortBy(result.thumbNailScaling, function(scaling) {
                    return scaling.enumOrder;
                });
                $scope.resizeModes = _.sortBy(result.resizeModes, 'enumOrder');
                $scope.toType = result.toType;
                $scope.templateReport = result.templateReport;
                schemaInfoLoaded = true;
                if (schemaInfoLoaded && entityLoaded) {
                    loadTheControl();
                    controlLoaded = true;
                    $scope.model.busyIndicator.isBusy = false;
                }
            });
            
            //
            // Get relationship entity from the database.
            //
            $scope.requestRelationshipEntity = function () {

                configureDialogService.getImageRelationshipEntity($scope.model.relationshipToRender.idP).then(
                    function (result) {
                        entityLoaded = true;
                        //Augemt the model field with the result to keep the client side changes.
                        spEntity.augment($scope.model.relationshipToRender, result, null);
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
            $scope.requestRelationshipControlEntity = function () {
                configureDialogService.getImageControlEntity($scope.options.formControl.idP).then(
                    function (result) {
                        //Augemt the model field with the result to keep the client side changes.
                        spEntity.augment($scope.options.formControl, result, null);
                        $scope.model.formControl = $scope.options.formControl;
                        entityLoaded = true;
                        $scope.model.relationshipToRender = $scope.model.formControl.relationshipToRender;
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
            
            $scope.onVisibilityScriptCompiled = function (script, error) {
                $scope.model.visibilityCalculationModel.isScriptCompiling = false;

                if (!$scope.model.isFormControl) {
                    return;
                }

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

            $scope.visibilityTabClicked = function() {
                _.delay(function() {
                    $scope.$broadcast('sp.app.ui-refresh');
                }, 100);
            };

            function initVisibilityCalculationModel() {
                if (!$scope.model.isFormControl) {
                    return;
                }                

                if ($scope.options &&
                    $scope.options.definition &&
                    $scope.options.definition.getDataState() !== spEntity.DataStateEnum.Create) {
                    $scope.model.visibilityCalculationModel.typeId = $scope.options.definition.idP;                    
                }
            }

            //
            //Load all the form controls after server side requests complete.
            //
            function loadTheControl() {
                if (!controlLoaded) {
                    //define model properties
                    $scope.model.imageNameControl = getDummyFormControl($scope.fields[0], 'Name');
                    $scope.model.imageNameControl.mandatoryControl = true;
                    $scope.model.imageDisplayNameControl = getDummyFormControl($scope.fields[0], 'Display Name');
                    $scope.model.imageDescriptionControl = getDummyFormControl($scope.fields[1], 'Description');
                    var relName = $scope.model.relationshipToRender.toName || $scope.model.relationshipToRender.name;
                    $scope.model.formData = getDummyFormData('controlEntityType', $scope.fields[0], relName);
                    setFormDataField($scope.model.formData, $scope.fields[1], $scope.model.relationshipToRender.description);
                    
                    //load base controls
                    $scope.baseControlsFile = 'configDialogs/relationshipsProperties/imageFieldProperties/views/imageControlBaseProperties.tpl.html';
                    if ($scope.options.isFormControl) {
                        //set thumbnailsize settings
                        if ($scope.model.formControl.thumbnailSizeSetting) {
                            var currentSetting = _.find($scope.model.thumbNailSize, function(size) {
                                return $scope.model.formControl.thumbnailSizeSetting.alias() === size.alias();
                            });
                            if (currentSetting)
                                $scope.model.thumbnailSizeSetting = currentSetting;
                            else {
                                $scope.model.thumbnailSizeSetting = $scope.model.thumbNailSize[0];
                            }
                        }
                        //set thumbnail scaling settings
                        if ($scope.model.formControl.thumbnailScalingSetting) {
                            var currentSaclingSetting = _.find($scope.model.thumbNailScaling, function(sclae) {
                                return $scope.model.formControl.thumbnailScalingSetting.alias() === sclae.alias();
                            });
                            if (currentSaclingSetting)
                                $scope.model.thumbnailScalingSetting = currentSaclingSetting;
                            else {
                                $scope.model.thumbnailScalingSetting = $scope.model.thumbNailScaling[0];
                            }
                        }
                        //set resize options
                        $scope.model.resizeOptions = {
                            resizeModes: $scope.resizeModes,
                            isHresizeModeDisabled: false,
                            isVresizeModeDisabled: false
                        };

                        $scope.formControlFile = 'configDialogs/relationshipsProperties/imageFieldProperties/views/imageFormProperties.tpl.html';
                        $scope.formatControlFile = 'configDialogs/relationshipsProperties/imageFieldProperties/views/imageFormatProperties.tpl.html';
                        $scope.visibilityControlFile = 'configDialogs/relationshipsProperties/imageFieldProperties/views/imageVisibilityProperties.tpl.html';
                    }

                    $scope.model.defaultValuePickerOptions = {
                        entityTypeId: $scope.toType.idP,
                        selectedEntity: null,
                        selectedEntities: null,
                        pickerReportId: $scope.templateReport.idP,
                        multiSelect: false
                    };
                    
                    //set default value 
                    var relationship = $scope.model.relationshipToRender;
                     relationship.setToType($scope.toType);
                    if (relationship.toTypeDefaultValue) {
                        $scope.model.defaultValuePickerOptions.selectedEntities = [relationship.toTypeDefaultValue];
                    }

                    $scope.model.relationshipIsMandatory = $scope.model.relationshipToRender.relationshipIsMandatory;
                    
                    $scope.definitionControlFile = 'configDialogs/relationshipsProperties/imageFieldProperties/views/imageDefinitionProperties.tpl.html';

                    $scope.model.initialState = {
                        relName: $scope.model.formData.getField($scope.fields[0].idP),
                        relDescription: $scope.model.formData.getField($scope.fields[1].idP),
                        relationshipIsMandatory: $scope.model.relationshipIsMandatory,                    
                        defaultValuePickerOptionsSelectedEntityIds: _.sortBy(_.map($scope.model.defaultValuePickerOptions.selectedEntities, function( e ) { return e ? e.id() : null; }))
                    };

                    initVisibilityCalculationModel();
                }
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
            //Get dummy form Control json object for the field.
            //
            function getDummyFormControl(field, fieldTitle) {
                var fieldType = field.getIsOfType()[0];
              
                var defaultRenderingControl = _.find(fieldType.getDefaultRenderingControls(), function (control) {
                    return control.getContext().getName() === 'Html';
                });
                if (!defaultRenderingControl) {
                    defaultRenderingControl = _.find(fieldType.getRenderingControl(), function (control) {
                        return control.getContext().getName() === 'Html';
                    });
                }
                var dummyFormControl = spEntity.fromJSON({
                    typeId:defaultRenderingControl.nsAlias,
                    'name':fieldTitle,
                    'description':'',
                    'console:fieldToRender':field,
                    'console:mandatoryControl':false,
                    'console:readOnlyControl':false,
                    'console:isReversed':false
                });
                return dummyFormControl;
            }                        
         
            var relationshipDefaultObject = {
                name:jsonString(''),
                description: jsonString(''),
                typeId: 'core:relationship',
                toName:jsonString(''),
                toType: jsonLookup(),
                cardinality: jsonLookup('core:manyToOne'),
                relationshipIsMandatory: false,
                revRelationshipIsMandatory: false,
                toTypeDefaultValue: jsonLookup(),
                fromTypeDefaultValue: jsonLookup(),
                isRelationshipReadOnly: false,
                relType:jsonLookup('core:relLookup'),
                cascadeDelete:false,
                cascadeDeleteTo:false,
                cloneAction: jsonLookup('core:cloneReferences'),
                reverseCloneAction: jsonLookup('core:drop'),
                implicitInSolution:false,
                reverseImplicitInSolution:false
            };

            var defaultThumbnailSizeSetting = spEntity.fromJSON({
                name: 'Small',
                typeId: 'console:smallThumbnail',
                'console:thumbnailWidth': 150,
                'console:thumbnailHeight': 150
            });

            var formControlDefaultObject = {
                name: jsonString(''),
                description: jsonString(''),
                typeId: 'console:imageRelationshipRenderControl',
                'console:renderingBackgroundColor': 'white',
                'console:mandatoryControl': false,
                'console:showControlHelpText': false,
                'console:readOnlyControl': false,
                'console:renderingHorizontalResizeMode': jsonLookup('console:resizeAutomatic'),
                'console:renderingVerticalResizeMode': jsonLookup('console:resizeAutomatic'),
                'console:thumbnailScalingSetting': jsonLookup('core:scaleImageProportionally'),
                'console:thumbnailSizeSetting': jsonLookup(defaultThumbnailSizeSetting)
            };
            
            //
            //Returns a template function for the form control object.
            //
            function getTempFnForImageControl() {
                var templateFn = function (type) {
                    if (type === 'console:imageRelationshipRenderControl')
                        return spEntity.fromJSON(formControlDefaultObject);
                    else
                        return null;
                };
                return templateFn;
            }
            
            //
            //Returns a template function for the form relationship object.
            //
            function getTempFnForImageRelationship() {
                var templateFn = function (type) {
                    if(type==='core:relationship')
                        return spEntity.fromJSON(relationshipDefaultObject);
                    else
                        return null;
                };
                return templateFn;
            }
            
            //check if its a new entity.
            $scope.isNewEntity = $scope.options.formControl.getDataState() === spEntity.DataStateEnum.Create ? true : false;
            $scope.isNewRelationship = false;
            
              //get formcontrol info if user cinfiguring the control or get relationship info if user configuring image field.
            if ($scope.options.isFormControl) {
                if ($scope.isNewEntity) {
                    $scope.model.formControl = $scope.options.formControl;
                    $scope.model.relationshipToRender = $scope.options.formControl.relationshipToRender;
                    //augment the control only.
                    spEntity.augment($scope.model.formControl, null, getTempFnForImageControl());
                    //check if the relationship is existing
                    if ($scope.model.relationshipToRender.getDataState() === spEntity.DataStateEnum.Create) {
                        entityLoaded = true;
                        $scope.isNewRelationship = true;
                        spEntity.augment($scope.model.relationshipToRender, null, getTempFnForImageRelationship());
                    } else {
                        //get field from the Database
                        $scope.requestRelationshipEntity();
                    }
                } else {
                    //get relationship Control from the Database
                    $scope.requestRelationshipControlEntity();
                }
                $scope.model.isFormDetailEnabled = true;
               
            } else {
                $scope.model.relationshipToRender = $scope.options.formControl;
                if ($scope.isNewEntity) {
                    entityLoaded = true;
                    $scope.isNewRelationship = true;
                    spEntity.augment($scope.model.relationshipToRender, null, getTempFnForImageRelationship());
                } else {
                    //get relationship from the Database
                    $scope.requestRelationshipEntity();
                }
                $scope.model.isObjectTabActive = true;
            }
            

            function validateForm() {
                var haveDefaultValue = $scope.model.defaultValuePickerOptions.selectedEntities &&
                    $scope.model.defaultValuePickerOptions.selectedEntities.length;

                if ($scope.model.isFormControl &&
                    $scope.model.formControl.visibilityCalculation &&
                    $scope.model.formControl.relationshipToRender.relationshipIsMandatory &&
                    !haveDefaultValue) {
                    $scope.model.addError("Visibility calculation cannot be defined as the field is mandatory and no default value is specified.");
                    return false;
                }

                return true;
            }

            // OK click handler
            $scope.ok = function () {
                if (validateForm() &&
                    $scope.form.$valid &&
                    !$scope.model.visibilityCalculationModel.error) {
                    var relationshipToRender;
                    var control = $scope.options.formControl;
                    if ($scope.options.isFormControl) {
                        relationshipToRender = control.relationshipToRender;
                        //set formControl properties
                        control.setThumbnailSizeSetting($scope.model.thumbnailSizeSetting);
                        control.setThumbnailScalingSetting($scope.model.thumbnailScalingSetting);                        
                    } else {
                        relationshipToRender = $scope.options.formControl;
                    }

                    var initialState = $scope.model.initialState;

                    //Set all the relationship properties
                   //set name and description of the relationship.
                    var relName = _.trim($scope.model.formData.getField($scope.fields[0].idP));
                    var originalName = _.trim(initialState.relName);

                    if ($scope.isNewRelationship || originalName !== relName) {
                        relationshipToRender.setName(relName);
                        relationshipToRender.setToName(relName);
                    }
                    
                    if ($scope.isNewRelationship || $scope.model.formData.getField($scope.fields[1].idP) !== initialState.relDescription) {
                        relationshipToRender.setDescription($scope.model.formData.getField($scope.fields[1].idP));    
                    }

                    if ($scope.isNewRelationship || $scope.model.relationshipIsMandatory !== initialState.relationshipIsMandatory) {
                        relationshipToRender.relationshipIsMandatory = $scope.model.relationshipIsMandatory;
                    }
                    
                    //set defaultValue
                    //var entities = relationship.getRelationship(relationship.id());
                    var selectedEntityIds = _.sortBy(_.map($scope.model.defaultValuePickerOptions.selectedEntities, function( e ) { return e ? e.id() : null; }));

                    if ($scope.isNewRelationship || !_.isEqual(selectedEntityIds, initialState.defaultValuePickerOptionsSelectedEntityIds)) {
                        var entities = $scope.model.defaultValuePickerOptions.selectedEntities;
                        if (entities && entities.length > 0 && entities[0].idP > 0) {
                            relationshipToRender.setToTypeDefaultValue(entities[0]);
                        } else {
                            relationshipToRender.setToTypeDefaultValue(null);
                        }
                    }
                    
                    //set all the relationship properties.
                    if ($scope.isNewRelationship) {
                        relationshipToRender.setRelType('relLookup');
                        relationshipToRender.setCardinality('manyToOne');
                        relationshipToRender.setCascadeDelete(false);
                        relationshipToRender.setCascadeDeleteTo(false);
                        relationshipToRender.setCloneAction('cloneReferences');
                        relationshipToRender.setReverseCloneAction('drop');
                        relationshipToRender.setImplicitInSolution(false);
                        relationshipToRender.setReverseImplicitInSolution(false);
                        relationshipToRender.setRevRelationshipIsMandatory(false);
                    }
                    
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