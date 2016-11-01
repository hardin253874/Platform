// Copyright 2011-2016 Global Software Innovation Pty Ltd
/*global console, _, angular */

(function () {
    'use strict';

    /**
   * Module implementing a relationship properties configure control.
   * spRelationshipProperties displays and modifies properties of relationships (of type lookup, multi lookup and relationship).
   *
   * @module spRelationshipProperties
   * @example

   Using the spRelationshipProperties:

   &lt;sp-date-control model="myModel"&gt;&lt;/sp-date-control&gt

   where options is available on the scope with the following members:

   Properties:
       - formControl {object} - relationship/formControl object depending on configure button clicked from.
       -isFieldControl {bool} - True-if configuring properties of field control.

   */

    angular.module('mod.app.configureDialog.relationshipProperties.spRelationshipProperties', ['mod.app.editForm', 'mod.app.editForm.designerDirectives', 'mod.app.configureDialog.relationshipProperties.spRelationshipPropertiesHelper', 'mod.app.formBuilder.services.spFormBuilderService', 'mod.app.configureDialog.relationshipProperties.spRelationshipFilters', 'mod.app.configureDialog.fieldPropertiesHelper', 'mod.app.configureDialog.spVisibilityCalculationControl', 'mod.app.spFormControlVisibilityService'])
        .directive('spRelationshipProperties', function () {
            return {
                restrict: 'E',
                transclude: false,
                replace: true,
                scope: {
                    options: '=?',
                    modalInstance: '=?'
                },
                templateUrl: 'configDialogs/relationshipsProperties/spRelationshipProperties/views/spRelationshipProperties_modal.tpl.html',
                controller: 'spRelationshipPropertiesController'
            };
        })
        .controller('spRelationshipPropertiesController', function ($scope, spEditForm, configureDialogService, controlConfigureDialogFactory,
                                                                    spFormBuilderService, spDialogService,
                                                                    spRelationshipPropertiesHelper, fieldPropertiesHelper, $timeout,
                                                                    spAlertsService,
                                                                    spFormControlVisibilityService) {
            // init
            var helper = spRelationshipPropertiesHelper;
            var collapsedImageUrl = 'assets/images/arrow_down.png';
            var expandedImageUrl = 'assets/images/arrow_up.png';

            var fieldsLoaded = false;
            var entityLoaded = false;
            $scope.controlsLoaded = false;
            var _uiToNameIsDefault, _uiFromNameIsDefault, _relNameIsDefault;
            $scope.relCanCreateComboOptions = helper.relCanCreateComboOptions;
            $scope.typeForms = [];            
            // internal model
            $scope.model = {
                errors: [],
                isControl: true,
                formMode: spEditForm.formModes.edit,
                canModifyRelationship: true,
                objectTypePickerOptions: {
                    entityTypeId: undefined,
                    selectedEntity: null,
                    selectedEntities: null,
                    pickerReportId: undefined,
                    multiSelect: false
                },
                defaultValuePickerOptions: {
                    entityTypeId: undefined,
                    selectedEntity: null,
                    selectedEntities: null,
                    pickerReportId: undefined,
                    multiSelect: false
                },
                selectedDisplayAs: undefined,
                selectedCanCreate: undefined,
                selectedConsoleForm: undefined,
                currentUserDisplayText: 'use current',
                ui: {
                    dialogTitle: '',
                    fromName: '',
                    toName: '',
                    relCardinality: undefined,
                    relOwnership: undefined,
                    relOwner: undefined,
                    isControlTabActive: false,
                    isRelDetailTabActive: false
                },
                securityUi : {
                    fromName: '',
                    toName: '',
                    toFromSwapped: false
                },
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

            /// Load template report then schema info
            spEditForm.getTemplateReportP().then(function (report) {
                $scope.templateReport = report;
                return configureDialogService.getSchemaInfo();
            }).then(
                function (result) {
                    $scope.fields = result.fields;
                    $scope.entities = result.entities;
                    $scope.resizeModes = _.sortBy(result.resizeModes, 'enumOrder');
                    console.log('fields Loaded');
                    fieldsLoaded = true;
                    if (entityLoaded) {
                        loadDialogControls();
                    }
                },
                function (error) {
                    $scope.model.addError(error);
                    fieldsLoaded = true;
                    console.log('fields Loaded with error');
                });

            /////
            // Watch for changes to the form control.
            /////
            $scope.$watch("options.isFormControl", function (newVal) {
                $scope.model.isControl = newVal;
            });

            /////
            // Watch for changes to the form control.
            /////
            $scope.$watch("options.formControl", function () {
                if ($scope.options.formControl && $scope.options.isFormControl) {

                    if ($scope.options.relationshipType !== 'lookup' && $scope.options.relationshipType !== 'relationship') {
                        console.error('This dialog does not support editing the type of relationship provided: ' + $scope.options.relationshipType);
                        return;
                    }

                    // clone form control to internal form control
                    $scope.model.formControl = $scope.options.formControl.cloneDeep();

                    $scope.model.isNewControl = $scope.options.formControl.getDataState() === spEntity.DataStateEnum.Create;

                    if ($scope.model.isNewControl) {

                        // augment the lookup
                        if ($scope.options.relationshipType === 'lookup') {
                            spEntity.augment($scope.model.formControl, null, helper.getLookupControlTemplateFn());
                        }

                        // relationshipToRender 
                        $scope.model.relationshipToRender = $scope.model.formControl.relationshipToRender;

                        //check if the relationship is existing
                        $scope.model.isNewRelationship = $scope.model.relationshipToRender.getDataState() === spEntity.DataStateEnum.Create;

                        if ($scope.model.isNewRelationship) {
                            // augument the relationshipToRender with relationship only template
                            augmentRelationshipWithTemplate();

                            if ($scope.model.canModifyRelationship) {
                                checkDefaultScriptNames();
                            }

                            entityLoaded = true;
                            if (fieldsLoaded) {
                                loadDialogControls();
                            }

                        } else {
                            //get relationship from the Database
                            configureDialogService.getBaseRelationshipEntity($scope.model.relationshipToRender.id()).then(
                            function (result) {
                                // augment the internal relationshipToRender with the result to keep the client side changes.
                                spEntity.augment($scope.model.relationshipToRender, result, null);
                                checkDefaultScriptNames();

                                entityLoaded = true;

                                if (fieldsLoaded) {
                                    loadDialogControls();
                                }
                            },
                            function (error) {
                                $scope.model.addError(error);
                                entityLoaded = true;
                            });
                        }
                    } else {
                        configureDialogService.getBaseRelControlOnFormEntity($scope.options.formControl.id()).then(
                            function(result) {
                                spEntity.augment($scope.model.formControl, result, null);
                                $scope.model.relationshipToRender = $scope.model.formControl.relationshipToRender;
                                checkDefaultScriptNames();
                                entityLoaded = true;
                                if(fieldsLoaded) {
                                    loadDialogControls();
                                }
                            },
                            function(error) {
                                $scope.model.addError(error);
                                entityLoaded = true;
                                console.log('entity Loaded with error');
                            });
                    }
                }
            });

            ///
            // Watch for changes to the relationship.
            ///
            $scope.$watch("options.relationship", function () {
                if ($scope.options.relationship && !$scope.options.isFormControl) {

                    if ($scope.options.relationshipType !== 'lookup' && $scope.options.relationshipType !== 'relationship') {
                        console.error('This dialog does not support editing the type of relationship provided: ' + $scope.options.relationshipType);
                        return;
                    }

                    // clone relationship to internal relationshipToRender
                    $scope.model.relationshipToRender = $scope.options.relationship.cloneDeep();

                    $scope.model.isNewRelationship = $scope.options.relationship.getDataState() === spEntity.DataStateEnum.Create;

                    if ($scope.model.isNewRelationship) {
                        // augument the relationshipToRender with relationship only template
                        augmentRelationshipWithTemplate();

                        entityLoaded = true;
                        if (fieldsLoaded) {
                            loadDialogControls();
                        }

                    } else {
                        configureDialogService.getBaseRelationshipEntity($scope.options.relationship.id()).then(
                            function (result) {
                                spEntity.augment($scope.model.relationshipToRender, result, null);
                                checkDefaultScriptNames();
                                entityLoaded = true;

                                if (fieldsLoaded) {
                                    loadDialogControls();
                                }
                            },
                            function (error) {
                                $scope.model.addError(error);
                                entityLoaded = true;
                            });
                    }
                    $scope.model.isObjectTabActive = true;
                }
            });

            // Watch the selected 'object' and get applicable forms. also clear the default value.
            $scope.$watch('model.objectTypePickerOptions.selectedEntities', function () {

                if (!$scope.model.objectTypePickerOptions)
                    return;

                var entities = $scope.model.objectTypePickerOptions.selectedEntities;
                if (entities && entities.length > 0 && entities[0].idP > 0) {
                    if ($scope.model.isReverseRelationship) {
                        $scope.model.relationshipToRender.fromType = entities[0];
                    }
                    else {
                        $scope.model.relationshipToRender.toType = entities[0];
                    }

                    // reset the default value
                    if ($scope.model.defaultValuePickerOptions.entityTypeId !== entities[0].idP) {
                        $scope.model.defaultValuePickerOptions.entityTypeId = entities[0].idP;
                        $scope.model.defaultValuePickerOptions.selectedEntities = null;
                    }

                    // update the visibility of use current user checkbox for the default value
                    updateUseCurrentUserChkboxVisibility(entities[0].idP);

                    updateToNameIfNecessary();
                    updateNamesRelatedStuff();

                    if ($scope.model.isControl) {
                        // load forms
                        loadFormsForType(entities[0].idP);

                        // load reports
                        loadReportsForType(entities[0].idP);
                    }
                }
                else{
                    collapseAllSections();
                }
            });

            $scope.onVisibilityScriptCompiled = function (script, error) {
                $scope.model.visibilityCalculationModel.isScriptCompiling = false;

                if (!$scope.model.isControl || $scope.options.showInReverse) {
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

            function loadDialogControls(preventInitialStateUpdate) {

                //////  set watches  //////
                (function () {
                    /////
                    // Watch for changes to ui relType.
                    /////
                    $scope.$watch("model.ui.relCardinality", function (newValue, oldValue) {
                        if (newValue === oldValue)
                            return;
                        console.log('ui.relCardinality value changed');
                        updateRelTypeFromUI();
                    });

                    /////
                    // Watch for changes to ui relOwnership.
                    /////
                    $scope.$watch("model.ui.relOwnership", function (newValue, oldValue) {
                        if (newValue === oldValue)
                            return;
                        console.log('ui.relOwnership value changed');

                        // update relType based on the ui selection and update relationship internal values/flags accordingly
                        updateRelTypeFromUI();

                    });

                    /////
                    // Watch for changes to ui relOwner.
                    /////
                    $scope.$watch("model.ui.relOwner", function (newValue, oldValue) {
                        if (newValue === oldValue)
                            return;
                        console.log('ui.relOwner value changed');

                        if ($scope.model.ui.relOwnership === 'full' && $scope.model.ui.relCardinality === 'oneToOne') {
                            // update relType based on the ui selection and update relationship internal values/flags accordingly
                            updateRelTypeFromUI();
                        }
                    });

                    ///
                    // Watch for changes to model.relationshipToRender.toName.
                    ///
                    $scope.$watch("model.relationshipToRender.toName", function (newValue, oldValue) {
                        console.log('model.relationshipToRender.toName value changed. internal update: ' + $scope.model.isRelToNameInternalUpdate);

                        //refresh control
                        if ($scope.model.isReverseRelationship) {
                            $scope.model.reverseNameControl = jQuery.extend(true, {}, $scope.model.reverseNameControl);
                            _uiFromNameIsDefault = getIsRelationshipFromNameDefault();
                            // validate control
                            if ($scope.model.reverseNameControl.validateOnSchemaChange) {
                                $scope.model.reverseNameControl.validateOnSchemaChange($scope.model.relationshipToRender);
                            }
                        }
                        else {
                            $scope.model.nameControl = jQuery.extend(true, {}, $scope.model.nameControl);
                            _uiToNameIsDefault = getIsRelationshipToNameDefault();
                            // validate control
                            if ($scope.model.nameControl.validateOnSchemaChange) {
                                $scope.model.nameControl.validateOnSchemaChange($scope.model.relationshipToRender);
                            }
                            checkScriptName(newValue, oldValue);
                        }

                        if ($scope.model.isRelToNameInternalUpdate) {
                            $scope.model.isRelToNameInternalUpdate = false;
                            return;
                        }

                        updateRelNameIfNecessary();
                    });

                    ///
                    // Watch for changes to model.relationshipToRender.toName.
                    ///
                    $scope.$watch("model.relationshipToRender.fromName", function (newValue, oldValue) {
                        //refresh control
                        if ($scope.model.isReverseRelationship) {
                            $scope.model.nameControl = jQuery.extend(true, {}, $scope.model.nameControl);
                            _uiToNameIsDefault = getIsRelationshipToNameDefault();
                            // validate control
                            if ($scope.model.nameControl.validateOnSchemaChange) {
                                $scope.model.nameControl.validateOnSchemaChange($scope.model.relationshipToRender);
                            }
                            checkScriptName(newValue, oldValue);
                        }
                        else {
                            $scope.model.reverseNameControl = jQuery.extend(true, {}, $scope.model.reverseNameControl);
                            _uiFromNameIsDefault = getIsRelationshipFromNameDefault();
                            // validate control
                            if ($scope.model.reverseNameControl.validateOnSchemaChange) {
                                $scope.model.reverseNameControl.validateOnSchemaChange($scope.model.relationshipToRender);
                            }
                        }

                        console.log('model.relationshipToRender.fromName value changed. internal update: ' + $scope.model.isRelFromNameInternalUpdate);
                        if ($scope.model.isRelFromNameInternalUpdate) {
                            $scope.model.isRelFromNameInternalUpdate = false;
                            return;
                        }

                        updateRelNameIfNecessary();
                    });

                    ///
                    // Watch for changes to model.relationshipToRender.name.
                    ///
                    $scope.$watch("model.relationshipToRender.name", function () {
                       //refresh control
                        $scope.model.relNameControl = jQuery.extend(true, {}, $scope.model.relNameControl);

                        // validate control
                        if ($scope.model.relNameControl.validateOnSchemaChange) {
                            $scope.model.relNameControl.validateOnSchemaChange($scope.model.relationshipToRender);
                        }

                        console.log('model.relationshipToRender.name value changed. internal update: ' + $scope.model.isRelNameInternalUpdate);
                        if ($scope.model.isRelNameInternalUpdate) {
                            $scope.model.isRelNameInternalUpdate = false;
                            return;
                        }

                        _relNameIsDefault = getIsRelationshipNameDefault();
                    });

                    ///
                    // Watch for changes to model.relationshipToRender.name.
                    ///
                    $scope.$watch("model.useCurrentUserAsDefault", function () {

                        if ($scope.model.useCurrentUserAsDefault) {
                            $scope.model.defaultValuePickerOptions.selectedEntities = null;
                            $scope.model.defaultValuePickerOptions.isDisabled = true;
                        }
                        else {
                            $scope.model.defaultValuePickerOptions.isDisabled = false;
                        }

                        setUseCurrentDisplayText();

                        // disable default value if not allowed to modify relationship
                        if (!$scope.model.canModifyRelationship) {
                            $scope.model.defaultValuePickerOptions.isDisabled = true;
                        }
                    });

                    ///
                    // Watch for changes to model.selectedDisplayAs.
                    ///
                    $scope.$watch("model.selectedDisplayAs", function (newVal, oldVal) {
                        if (!newVal) {
                            return;
                        }

                        if (newVal === oldVal) {
                            return;
                        }

                        // set 'isRelationshipWithInlineDisplay' flag to show/hide display report option for relationship control
                        updateIsRelationshipWithInlineDisplayFlag();

                        if ($scope.isLookup) {
                            return;             // we are doing this only for relationship
                        }

                        // update 'isVresizeModeDisabled' flag
                        $scope.model.resizeOptions.isVresizeModeDisabled = $scope.isRelationshipWithInlineDisplay;

                        // for a relationship control, update vertical and horizontal size if its not set to manual
                        var formCtrl = $scope.model.formControl;
                        if (formCtrl) {
                            var resizeSpringMode = _.find($scope.model.resizeOptions.resizeModes, function (mode) {
                                return mode.getAlias() === 'console:resizeSpring';
                            });

                            var resizeAutomaticMode = _.find($scope.model.resizeOptions.resizeModes, function (mode) {
                                return mode.getAlias() === 'console:resizeAutomatic';
                            });

                            if (!resizeSpringMode || !resizeAutomaticMode) {
                                return;
                            }

                            var horizontalSizeAlias = sp.result(formCtrl, 'renderingHorizontalResizeMode.nsAlias');
                            var verticalSizeAlias = sp.result(formCtrl, 'renderingVerticalResizeMode.nsAlias');

                            if (horizontalSizeAlias !== 'console:resizeManual') {
                                if (newVal.value === 'Report') {
                                    formCtrl.setRenderingHorizontalResizeMode(resizeSpringMode);
                                } else if (newVal.value === 'Inline') {
                                    formCtrl.setRenderingHorizontalResizeMode(resizeAutomaticMode);
                                }
                            }

                            if (verticalSizeAlias !== 'console:resizeManual') {
                                if (newVal.value === 'Report') {
                                    formCtrl.setRenderingVerticalResizeMode(resizeSpringMode);
                                } else if (newVal.value === 'Inline') {
                                    formCtrl.setRenderingVerticalResizeMode(resizeAutomaticMode);
                                }
                            }
                        }
                    });
                })();

                if (!preventInitialStateUpdate) {
                    $scope.model.initialState = {
                        relationshipToRender: $scope.model.relationshipToRender.cloneDeep(),
                        defaultValuePickerOptionsSelectedEntityIds: _.sortBy(_.map($scope.model.defaultValuePickerOptions.selectedEntities, function (e) { return e ? e.id() : null; })),
                        objectTypePickerOptionsSelectedEntityIds: _.sortBy(_.map($scope.model.objectTypePickerOptions.selectedEntities, function (e) { return e ? e.id() : null; })),
                        useCurrentUserAsDefault: $scope.model.useCurrentUserAsDefault
                    };
                }

                // is it allowed to modify relationship
                var isNewRelationship = $scope.model.relationshipToRender.getDataState() === spEntity.DataStateEnum.Create;
                $scope.model.canModifyRelationship = isNewRelationship || spFormBuilderService.isDirectRelationship($scope.model.relationshipToRender.idP);

                // workout relationship direction
                var isReverse = $scope.options.isReverseRelationship;

                // calc rel direction
                if ((!isReverse && !$scope.options.showInReverse) ||
                    (isReverse && $scope.options.showInReverse)) {
                    // fwd
                    isReverse = false;
                }
                else if (($scope.model.isReverseRelationship && !$scope.options.showInReverse) ||
                        (!$scope.model.isReverseRelationship && $scope.options.showInReverse)) {
                    // rev
                    isReverse = true;
                }

                $scope.model.isReverseRelationship = isReverse;

                // set actual(stored) relType and effective relType
                setRelTypes($scope.model.relationshipToRender, $scope.model.isReverseRelationship);

                // set UI relationship values
                setUiRelationshipValues($scope.model);

                if ($scope.isLookup) {
                    $scope.displayAsComboOptions = helper.relDisplayAsLookupComboOptions;
                } else {
                    $scope.displayAsComboOptions = helper.relDisplayAsRelationshipComboOptions;
                }

                if($scope.model.isControl) {
                    loadFormControls();
                    $scope.model.resizeOptions = {
                        resizeModes: $scope.resizeModes,
                        isHresizeModeDisabled: false,
                        isVresizeModeDisabled: $scope.isLookup || $scope.isRelationshipWithInlineDisplay
                    };
                }

                loadRelationshipControls();

                initVisibilityCalculationModel();

                setDataOnInitialLoad();

                // set other ui values
                setDialogUiValues();

                // set tab selection
                if ($scope.model.isControl) {
                    $scope.model.isControlTabActive = true;
                }
                else {
                    $scope.model.isRelDetailTabActive = true;
                }
                
                // debug info visible
                $scope.model.debugInfoVisible = angular.isDefined(getEntity('core:testSolution')) ? true : false;
                $scope.controlsLoaded = true;
                $scope.model.busyIndicator.isBusy = false;
            }

            function initVisibilityCalculationModel() {
                if (!$scope.model.isControl || $scope.options.showInReverse) {
                    return;
                }                

                if ($scope.options &&
                    $scope.options.definition &&
                    $scope.options.definition.getDataState() !== spEntity.DataStateEnum.Create) {
                    $scope.model.visibilityCalculationModel.typeId = $scope.options.definition.idP;                    
                }
            }

            function validateForm() {
                //if is reverse relationship, use fromScriptName, and if the fromScriptName same as object script name, then use toName (reverse name) as newScriptName
                var newScriptName;
                // check for script name clashes
                var scriptName = getTypeScriptName();
                if ($scope.model.isReverseRelationship)
                {
                    if ($scope.model.relationshipToRender.fromScriptName === scriptName)
                        newScriptName = $scope.model.relationshipToRender.toName;
                    else
                        newScriptName = $scope.model.relationshipToRender.fromScriptName;
                } else {
                    newScriptName = $scope.model.relationshipToRender.toScriptName;
                }

                var relationshipId = $scope.model.relationshipToRender.id();

                var fields = fieldPropertiesHelper.getSelectedType(spFormBuilderService).getFields();

                if (fields) {

                    //check for duplicate names

                    var newName = $scope.model.relationshipToRender.name;

                    if (_.some(fields, function (field) {
						var fieldEntity = field.getEntity();
						return fieldEntity && fieldEntity.name && fieldEntity.name.toLowerCase().trim() === newName.toLowerCase().trim() && fieldEntity.id() !== relationshipId;
                    })) {
                        $scope.model.addError('The field name \'' + newName + '\' already exists on this object');
                        return;
                    }



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

                var relationships = fieldPropertiesHelper.getSelectedType(spFormBuilderService).getAllRelationships();

                if (relationships) {
                    if (_.some(relationships, function (relationship) {
						var relationshipEntity = relationship.getEntity();
						return relationshipEntity && relationshipEntity.toScriptName && relationshipEntity.toScriptName.toLowerCase().trim() === newScriptName.toLowerCase().trim() && relationshipEntity.id() !== relationshipId;
                    })) {
                        $scope.model.addError('The relationship script name \'' + newScriptName + '\' already exists on a different relationship');
                        return;
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

            // OK handler
            $scope.ok = function () {
                if ($scope.model.actualRelType.alias() === 'core:relCustom' || $scope.model.actualRelType.alias() === 'relCustom') {
                    $scope.modalInstance.close(false);
                    return;
                }

                $scope.model.clearErrors();
                if ($scope.form.$valid) {
                    validateForm();
                    if ($scope.model.errors.length === 0 &&
                        !$scope.model.visibilityCalculationModel.error) {
                        console.log('form valid');

                        // check if related object is selected
                        var isRelatedObjectSelected = !spUtils
                            .isNullOrUndefined($scope.model.objectTypePickerOptions.selectedEntities) &&
                            $scope.model.objectTypePickerOptions.selectedEntities.length > 0;
                        if (!isRelatedObjectSelected) {
                            $scope.model.isErrorMsgVisible = true;
                            $timeout(expireAlerts, 5000);
                            return;
                        }

                        // set the values
                        setDataOnSave();

                        // close the Dialog
                        $scope.modalInstance.close($scope.options.formControl);
                    }
                }
                else {
                    raiseErrorAlert('There are errors on the page. Please fix the error before clicking OK');
                }
            };

            // Cancel handler
            $scope.cancel = function () {
                $scope.modalInstance.close(false);
            };

            $scope.handleLinkClick = function () {
                //  Build option variable
                var options;
                if ($scope.model.formControl) {
                    // editing relationship control
                    options = {
                        showInReverse: true,
                        formControl: $scope.model.formControl,
                        isFieldControl: false,
                        isFormControl: true,
                        relationshipType: controlConfigureDialogFactory.getRelationshipType($scope.model.formControl)
                    };
                }
                else {
                    // editing relationship only
                    options = {
                        showInReverse: true,
                        isFieldControl: false,
                        isFormControl: false,
                        relationshipType: getRelationshipTypeInReverseDirection(),
                        relationship: $scope.model.relationshipToRender
                    };
                }

                controlConfigureDialogFactory.createDialog(options).then(function (result) {
                    if (result !== false) {
                        // refresh the dialog
                        $scope.model.busyIndicator.text = 'Re loading...';
                        $scope.model.busyIndicator.isBusy = true;
                        loadDialogControls(true);
                    }
                });
            };

            function updateIsRelationshipWithInlineDisplayFlag() {
                // set 'isRelationshipWithInlineDisplay' flag to show/hide display report option for relationship control
                if (!$scope.isLookup && $scope.model.selectedDisplayAs && $scope.model.selectedDisplayAs.value === helper.relDisplayAsComboItemInline) {
                    $scope.isRelationshipWithInlineDisplay = true;
                } else {
                    $scope.isRelationshipWithInlineDisplay = false;
                }
            }

            // Applies any relationship control filter changes from the source control to the target
            function applyRelationshipControlFilterChanges(sourceControl, targetControl) {
                var sourceRelationshipFilters,
                    sourceFilterInstances,
                    targetRelationshipFilters;

                if (!sourceControl.hasRelationship('console:relationshipControlFilters')) {
                    // Early out
                    return;
                }

                sourceRelationshipFilters = sourceControl.getRelationship('console:relationshipControlFilters');
                sourceFilterInstances = sourceRelationshipFilters.getInstances();

                if (sourceFilterInstances.length) {
                    targetControl.registerRelationship('console:relationshipControlFilters');
                    targetRelationshipFilters = targetControl.getRelationship('console:relationshipControlFilters');
                }

                _.forEach(sourceFilterInstances, function (ri) {
                    if (ri.getDataState() === spEntity.DataStateEnum.Delete) {
                        targetRelationshipFilters.deleteEntity(ri.entity);
                    } else {
                        targetRelationshipFilters.add(ri.entity);
                    }
                });
            }

            function setDataOnSave() {
                var control = $scope.options.formControl;
                var relationship = $scope.model.isControl ? control.relationshipToRender : $scope.options.relationship;

                // update formControl properties if we are not viewing the relationship in reverse direction.
                // in reverse direction we are only viewing relationship detail in reverse. and not of an arbitrary control representing this relationship in reverse direction. 
                if ($scope.model.isControl && !$scope.options.showInReverse) {
                    // label
                    control.setName($scope.model.formControl.name);

                    // control description
                    if (!control.hasField('core:description')) {
                        control.registerField('core:description', spEntity.DataType.String);
                        control.description = $scope.model.formControl.description;
                    } else {
                        control.setDescription($scope.model.formControl.description);
                    }



                    // picker report
                    control.setLookup('console:pickerReport', $scope.model.selectedPickerReport);

                    // display report
                    if (!$scope.isLookup) {
                        control.setLookup('console:relationshipDisplayReport', $scope.model.selectedDisplayReport);
                    }

                    //show help text
                    control.setField('console:showControlHelpText', $scope.model.formControl.showControlHelpText, spEntity.DataType.Bool);

                    // isReversed
                    control.setField('console:isReversed', $scope.model.isReverseRelationship, spEntity.DataType.Bool);

                    // mandatory
                    control.setField('console:mandatoryControl', $scope.model.formControl.mandatoryControl, spEntity.DataType.Bool);

                    // read only
                    control.setField('console:readOnlyControl', $scope.model.formControl.readOnlyControl, spEntity.DataType.Bool);

                    //background color
                    control.setField('console:renderingBackgroundColor', $scope.model.formControl.renderingBackgroundColor, spEntity.DataType.String);

                    // hide label
                    control.setField('console:hideLabel', $scope.model.formControl.hideLabel, spEntity.DataType.Bool);

                    //Horizontal resize mode
                    control.setLookup('console:renderingHorizontalResizeMode', $scope.model.formControl.renderingHorizontalResizeMode);

                    //Vertical resize mode
                    control.setLookup('console:renderingVerticalResizeMode', $scope.model.formControl.renderingVerticalResizeMode);

                    // display form
                    control.setLookup('resourceViewerConsoleForm', $scope.model.selectedConsoleForm);

                    // visibility calculation
                    control.setField('console:visibilityCalculation', $scope.model.formControl.visibilityCalculation, spEntity.DataType.String);

                    applyRelationshipControlFilterChanges($scope.model.formControl, control);

                    // allow create new
                    if ($scope.isLookup) {
                        var canCreate = false;
                        var canCreateDerivedTypes = false;
                        if ($scope.model.selectedCanCreate.value === helper.resourceViewerComboItemYesIncDerivedTypes) {
                            canCreate = true;
                            canCreateDerivedTypes = true;
                        } else if ($scope.model.selectedCanCreate.value === helper.resourceViewerComboItemYes) {
                            canCreate = true;
                            canCreateDerivedTypes = false;
                        } else if ($scope.model.selectedCanCreate.value === helper.resourceViewerComboItemNo) {
                            canCreate = false;
                            canCreateDerivedTypes = false;
                        }

                        control.setField('core:canCreate', canCreate, spEntity.DataType.Bool);
                        control.setField('core:canCreateDerivedTypes', canCreateDerivedTypes, spEntity.DataType.Bool);
                    }

                    // display as
                    if ($scope.model.selectedDisplayAs.value === helper.relDisplayAsComboItemInline) {
                        var inlineRelRenderCtrlEntity = getEntity('console:inlineRelationshipRenderControl');
                        if (inlineRelRenderCtrlEntity) {
                            $scope.options.formControl.type = inlineRelRenderCtrlEntity;
                            $scope.options.formControl.setRelationship('core:isOfType', [inlineRelRenderCtrlEntity]);
                        }
                    }
                    else if ($scope.model.selectedDisplayAs.value === helper.relDisplayAsComboItemDropdown) {
                        var dropDownRelRenderCtrlEntity = getEntity('console:dropDownRelationshipRenderControl');
                        if (dropDownRelRenderCtrlEntity) {
                            $scope.options.formControl.type = dropDownRelRenderCtrlEntity;
                            $scope.options.formControl.setRelationship('core:isOfType', [dropDownRelRenderCtrlEntity]);
                        }
                    }
                    else if ($scope.model.selectedDisplayAs.value === helper.relDisplayAsComboItemReport) {
                        var tabRelRenderCtrlEntity = getEntity('console:tabRelationshipRenderControl');
                        if (tabRelRenderCtrlEntity) {
                            $scope.options.formControl.type = tabRelRenderCtrlEntity;
                            $scope.options.formControl.setRelationship('core:isOfType', [tabRelRenderCtrlEntity]);
                        }
                    }

                }

                // update relationship properties if can modify relationship

                if (!$scope.model.canModifyRelationship)
                    return;

                var isNewRelationship = $scope.model.isNewRelationship;
                var initialState = $scope.model.initialState;

                // object type (toType/fromType)
                var relatedEntityIds = _.sortBy(_.map($scope.model.objectTypePickerOptions.selectedEntities, function( e ) { return e ? e.id() : null; }));

                if (isNewRelationship || !_.isEqual(relatedEntityIds, initialState.objectTypePickerOptionsSelectedEntityIds)) {
                    var relatedEntities = $scope.model.objectTypePickerOptions.selectedEntities;
                    var relatedDef = !spUtils.isNullOrUndefined(relatedEntities) && relatedEntities.length > 0 ? relatedEntities[0] : null;
                    if ($scope.model.isReverseRelationship) {
                        relationship.setFromType(relatedDef);
                    }
                    else {
                        relationship.setToType(relatedDef);
                    }
                }

                if (isNewRelationship || $scope.model.relationshipToRender.name !== initialState.relationshipToRender.name) {
                    relationship.setField('core:name', $scope.model.relationshipToRender.name, spEntity.DataType.String);
                }

                if (isNewRelationship || $scope.model.relationshipToRender.description !== initialState.relationshipToRender.description) {
                    relationship.setField('core:description', $scope.model.relationshipToRender.description, spEntity.DataType.String);
                }

                if (isNewRelationship || $scope.model.relationshipToRender.toName !== initialState.relationshipToRender.toName) {
                    relationship.setField('core:toName', $scope.model.relationshipToRender.toName, spEntity.DataType.String);
                }

                if (isNewRelationship || $scope.model.relationshipToRender.fromName !== initialState.relationshipToRender.fromName) {
                    relationship.setField('core:fromName', $scope.model.relationshipToRender.fromName, spEntity.DataType.String);
                }

                if (isNewRelationship || $scope.model.relationshipToRender.toScriptName !== initialState.relationshipToRender.toScriptName) {
                    relationship.setField('core:toScriptName', $scope.model.relationshipToRender.toScriptName, spEntity.DataType.String);
                }

                if (isNewRelationship || $scope.model.relationshipToRender.fromScriptName !== initialState.relationshipToRender.fromScriptName) {
                    relationship.setField('core:fromScriptName', $scope.model.relationshipToRender.fromScriptName, spEntity.DataType.String);
                }

                if (isNewRelationship || $scope.model.relationshipToRender.showRelationshipHelpText !== initialState.relationshipToRender.showRelationshipHelpText) {
                    relationship.setField('core:showRelationshipHelpText', $scope.model.relationshipToRender.showRelationshipHelpText, spEntity.DataType.Bool);
                }


                var selectedEntityIds = _.sortBy(_.map($scope.model.defaultValuePickerOptions.selectedEntities, function( e ) { return e ? e.id() : null; }));

                if (isNewRelationship || !_.isEqual(selectedEntityIds, initialState.defaultValuePickerOptionsSelectedEntityIds)) {
                    //set defaultValue
                    var selectedEntities = $scope.model.defaultValuePickerOptions.selectedEntities;
                    var defaultVal = selectedEntities && selectedEntities.length > 0 ? selectedEntities[0] : null;
                    var defaultValueRelAlias = $scope.model.isReverseRelationship ? 'core:fromTypeDefaultValue' : 'core:toTypeDefaultValue';
                    relationship.setLookup(defaultValueRelAlias, defaultVal);
                    if(!defaultVal) {
                        relationship.getRelationship(defaultValueRelAlias).clear();
                    }
                }

                if (isNewRelationship || $scope.model.useCurrentUserAsDefault !== initialState.useCurrentUserAsDefault) {
                    // set use current user as default value
                    var useCurrentUserAsDefault = spUtils.isNullOrUndefined($scope.model.useCurrentUserAsDefault) ? false : $scope.model.useCurrentUserAsDefault;
                    if ($scope.model.isReverseRelationship) {
                        relationship.setField('core:defaultFromUseCurrent', useCurrentUserAsDefault, spEntity.DataType.Bool);
                    }
                    else {
                        relationship.setField('core:defaultToUseCurrent', useCurrentUserAsDefault, spEntity.DataType.Bool);
                    }
                }

                // 'hideOnReverse'
                if (!$scope.model.isReverseRelationship &&
                    (isNewRelationship || $scope.model.relationshipToRender.hideOnToType !== initialState.relationshipToRender.hideOnToType)) {
                    if ($scope.model.relationshipToRender.hideOnToType) {
                        relationship.setField('core:hideOnToType', true, spEntity.DataType.Bool);
                        relationship.setField('core:hideOnToTypeDefaultForm', true, spEntity.DataType.Bool);
                    }
                    else {
                        relationship.setField('core:hideOnToType', false, spEntity.DataType.Bool);
                        relationship.setField('core:hideOnToTypeDefaultForm', true, spEntity.DataType.Bool);
                    }
                    // always available on from side i.e. in fwd direction
                    relationship.setField('core:hideOnFromType', false, spEntity.DataType.Bool);
                    relationship.setField('core:hideOnFromTypeDefaultForm', true, spEntity.DataType.Bool);
                }

                // copy values form 'relationshipToRender' to 'relationship'

                if (isNewRelationship || sp.result($scope.model, 'relationshipToRender.relType.alias') !== sp.result(initialState, 'relationshipToRender.relType.alias')) {
                    relationship.setLookup('core:relType', $scope.model.relationshipToRender.relType);
                }

                if (isNewRelationship || sp.result($scope.model, 'relationshipToRender.cardinality.alias') !== sp.result(initialState, 'relationshipToRender.cardinality.alias')) {
                    relationship.setLookup('core:cardinality', $scope.model.relationshipToRender.cardinality);
                }

                if (isNewRelationship || $scope.model.relationshipToRender.cascadeDelete !== initialState.relationshipToRender.cascadeDelete) {
                    relationship.setField('core:cascadeDelete', $scope.model.relationshipToRender.cascadeDelete, spEntity.DataType.Bool);
                }

                if (isNewRelationship || $scope.model.relationshipToRender.cascadeDeleteTo !== initialState.relationshipToRender.cascadeDeleteTo) {
                    relationship.setField('core:cascadeDeleteTo', $scope.model.relationshipToRender.cascadeDeleteTo, spEntity.DataType.Bool);
                }

                if (isNewRelationship || sp.result($scope.model, 'relationshipToRender.cloneAction.alias') !== sp.result(initialState, 'relationshipToRender.cloneAction.alias')) {
                    relationship.setLookup('core:cloneAction', $scope.model.relationshipToRender.cloneAction);
                }

                if (isNewRelationship || sp.result($scope.model, 'relationshipToRender.reverseCloneAction.alias') !== sp.result(initialState, 'relationshipToRender.reverseCloneAction.alias')) {
                    relationship.setLookup('core:reverseCloneAction', $scope.model.relationshipToRender.reverseCloneAction);
                }

                if (isNewRelationship || $scope.model.relationshipToRender.implicitInSolution !== initialState.relationshipToRender.implicitInSolution) {
                    relationship.setField('core:implicitInSolution', $scope.model.relationshipToRender.implicitInSolution, spEntity.DataType.Bool);
                }

                if (isNewRelationship || $scope.model.relationshipToRender.reverseImplicitInSolution !== initialState.relationshipToRender.reverseImplicitInSolution) {
                    relationship.setField('core:reverseImplicitInSolution', $scope.model.relationshipToRender.reverseImplicitInSolution, spEntity.DataType.Bool);
                }

                if (isNewRelationship || $scope.model.relationshipToRender.securesTo !== initialState.relationshipToRender.securesTo) {
                    relationship.setField('core:securesTo', $scope.model.relationshipToRender.securesTo, spEntity.DataType.Bool);
                }

                if (isNewRelationship || $scope.model.relationshipToRender.securesFrom !== initialState.relationshipToRender.securesFrom) {
                    relationship.setField('core:securesFrom', $scope.model.relationshipToRender.securesFrom, spEntity.DataType.Bool);
                }

                if (isNewRelationship || $scope.model.relationshipToRender.relationshipIsMandatory !== initialState.relationshipToRender.relationshipIsMandatory) {
                    relationship.setField('core:relationshipIsMandatory', $scope.model.relationshipToRender.relationshipIsMandatory, spEntity.DataType.Bool);
                }

                if (isNewRelationship || $scope.model.relationshipToRender.revRelationshipIsMandatory !== initialState.relationshipToRender.revRelationshipIsMandatory) {
                    relationship.setField('core:revRelationshipIsMandatory', $scope.model.relationshipToRender.revRelationshipIsMandatory, spEntity.DataType.Bool);
                }
            }

            ///----------- Initial load related functions ------///
            function augmentRelationshipWithTemplate() {
                if ($scope.options.relationshipType === 'lookup') {
                    spEntity.augment($scope.model.relationshipToRender, null, helper.getLookupTemplateFn());
                }
                else if ($scope.options.relationshipType === 'relationship') {
                    spEntity.augment($scope.model.relationshipToRender, null, helper.getRelationshipTemplateFn());
                }
                else {
                    console.error('provided relationship type is not supported');
                    return;
                }
            }

            function setRelTypes(relationship, isReverseRelationship) {
                if (relationship) {
                    var relType = relationship.getRelType();

                    if (!relType) {
                        relType = helper.getMissingActualRelType(relationship); // if relType is not set to something, then workout relType based on cardinality and ownership = none
                    }

                    $scope.model.actualRelType = relType;
                    $scope.model.effectiveRelType = isReverseRelationship ? helper.getEffectiveRelTypeByActualRelType($scope.model.actualRelType) : $scope.model.actualRelType;
                }
            }

            function setUiRelationshipValues() {
                // set relationshipType, ownership
                setUiCardinalityAndOwnership($scope.model);

                $scope.isLookup = ($scope.model.ui.relCardinality === 'manyToOne' || $scope.model.ui.relCardinality === 'oneToOne');

                updateNamesRelatedStuff();
            }

            function setUiCardinalityAndOwnership(model) {
                var effectiveRelType = model.effectiveRelType;
                // set cardinality (relationshipType radio option)
                var uiCardinality, uiOwnership, uiOwner;

                // 'effectiveRelType' (i.e 'compliment' column in 'A Combined View of Relationship Types' table in wiki
                if (effectiveRelType) {
                    var relType;
                    var relTypeAlias = effectiveRelType.alias();
                    if (relTypeAlias.indexOf(':') > 0) {
                        relType = relTypeAlias.substr(relTypeAlias.indexOf(':') + 1);
                    }
                    if (relType) {
                        switch (relType) {
                            case 'relLookup':
                                uiCardinality = 'manyToOne';
                                uiOwnership = 'none';
                                break;
                            case 'relDependantOf':
                                uiCardinality = 'manyToOne';
                                uiOwnership = 'part';
                                break;
                            case 'relComponentOf':
                                uiCardinality = 'manyToOne';
                                uiOwnership = 'full';
                                break;
                            case 'relChoiceField':
                                uiCardinality = 'manyToOne';
                                uiOwnership = 'none';
                                break;
                            case 'relSingleLookup':
                                uiCardinality = 'oneToOne';
                                uiOwnership = 'none';
                                //uiOwner = _.find($scope.model.oneToOneOwnerOptions, { 'value': 'fromType' }); // select the first option
                                break;
                            case 'relSingleComponentOf':
                                uiCardinality = 'oneToOne';
                                uiOwnership = 'full';
                                uiOwner = _.find($scope.model.oneToOneOwnerOptions, { 'value': 'toType' });
                                break;
                            case 'relSingleComponent':
                                uiCardinality = 'oneToOne';
                                uiOwnership = 'full';
                                uiOwner = _.find($scope.model.oneToOneOwnerOptions, { 'value': 'fromType' });
                                break;
                            case 'relExclusiveCollection':
                                uiCardinality = 'oneToMany';
                                uiOwnership = 'none';
                                break;
                            case 'relDependants':
                                uiCardinality = 'oneToMany';
                                uiOwnership = 'part';
                                break;
                            case 'relComponents':
                                uiCardinality = 'oneToMany';
                                uiOwnership = 'full';
                                break;
                            case 'relManyToMany':
                                uiCardinality = 'manyToMany';
                                uiOwnership = 'none';
                                break;
                            case 'relMultiChoiceField':
                                uiCardinality = 'manyToMany';
                                uiOwnership = 'none';
                                break;
                            case 'relSharedDependantsOf':
                                raiseErrorAlert('not implemented yet');
                                //uiCardinality = 'manyToMany';
                                //uiOwnership = 'part';
                                break;
                            case 'relSharedDependants':
                                raiseErrorAlert('not implemented yet');
                                //uiCardinality = 'manyToMany';
                                //uiOwnership = 'part';
                                break;
                            case 'relManyToManyFwd':
                                raiseErrorAlert('not implemented yet');
                                //uiCardinality = 'manyToMany';
                                //uiOwnership = 'part';
                                break;
                            case 'relManyToManyRev':
                                raiseErrorAlert('not implemented yet');
                                //uiCardinality = 'manyToMany';
                                //uiOwnership = 'part';
                                break;
                            default:
                                //uiCardinality = 'manyToMany';
                                //uiOwnership = 'none';
                                break;
                        }
                    }
                    else {
                        console.error('setUiCardinalityAndOwnership: invallid relType provided.');
                    }
                }

                model.ui.relCardinality = uiCardinality;
                model.ui.relOwnership = uiOwnership;

                if (uiOwner) {
                    model.ui.relOwner = uiOwner;
                }
            }

            function updateNamesRelatedStuff() {
                var relationship = $scope.model.relationshipToRender;
                if (!relationship || !relationship.fromType || !relationship.toType) {
                    return;
                }
                var fromTypeName = relationship.fromType.name ? relationship.fromType.name : 'Unnamed Resource';
                var toTypeName = relationship.toType.name ? relationship.toType.name : 'Unnamed Resource';

                // uiFromName
                $scope.model.ui.fromName = $scope.model.isReverseRelationship ? toTypeName : fromTypeName;//$scope.model.relationshipToRender.toType.name : $scope.model.relationshipToRender.fromType.name;

                // uiToName
                $scope.model.ui.toName = $scope.model.isReverseRelationship ? fromTypeName : toTypeName;

                // setOneToOneOwner select options
                updateOneToOneOwnerSelectOptions();

                // set the svg settings for relType
                setRelTypeSvgSettings($scope.model.ui.relCardinality, $scope.model.ui.fromName, $scope.model.ui.toName);

                // set the svg settings for ownership
                setOwnershipSvgSettings($scope.model.ui.relCardinality, $scope.model.ui.fromName, $scope.model.ui.toName);

                // set the svg settings for security
                setSecuritySvgSettings($scope.model.ui.relCardinality, $scope.model.relationshipToRender);
            }

            function updateOneToOneOwnerSelectOptions() {
                // oneToOne owner options
                var relationship = $scope.model.relationshipToRender;
                if (!relationship || !relationship.fromType || !relationship.toType) {
                    return;
                }

                var fromTypeName = spUtils.isNullOrUndefined(relationship.fromType.name) ? 'Unnamed Resource' : relationship.fromType.name;
                var toTypeName = spUtils.isNullOrUndefined(relationship.toType.name) ? 'Unnamed Resource' : relationship.toType.name;

                $scope.model.oneToOneOwnerOptions = [
                    { name: fromTypeName + ' owns ' + toTypeName, value: 'fromType' },
                    { name: toTypeName + ' owns ' + fromTypeName, value: 'toType' }
                ];

                var effectiveRelType = sp.result($scope.model, 'effectiveRelType.nsAlias');
                if (effectiveRelType === 'core:relSingleComponent') {
                    $scope.model.ui.relOwner = _.find($scope.model.oneToOneOwnerOptions, { 'value': 'fromType' });
                    // set values to update the info text
                    $scope.model.ui.relOwnerText = fromTypeName;
                    $scope.model.ui.relNonOwnerText = toTypeName;
                    return;
                }

                if (effectiveRelType === 'core:relSingleComponentOf') {
                    $scope.model.ui.relOwner = _.find($scope.model.oneToOneOwnerOptions, { 'value': 'toType' });
                    // set values to update the info text
                    $scope.model.ui.relOwnerText = toTypeName;
                    $scope.model.ui.relNonOwnerText = fromTypeName;
                    return;
                }
                
                // set default selection
                if ($scope.model.isReverseRelationship) {
                    $scope.model.ui.relOwner = _.find($scope.model.oneToOneOwnerOptions, { 'value': 'fromType' });
                    // set values to update the info text
                    $scope.model.ui.relOwnerText = fromTypeName;
                    $scope.model.ui.relNonOwnerText = toTypeName;
                }
                else {
                    $scope.model.ui.relOwner = _.find($scope.model.oneToOneOwnerOptions, { 'value': 'toType' });
                    // set values to update the info text
                    $scope.model.ui.relOwnerText = toTypeName;
                    $scope.model.ui.relNonOwnerText = fromTypeName;
                }
            }

            function setRelTypeSvgSettings(uiCardinality, fromName, toName) {
                if (uiCardinality) {
                    if (uiCardinality === 'manyToOne' || uiCardinality === 'oneToOne') {
                        $scope.model.manyToOneSvgConfig = { svgs: helper.getSvgSettingsByCardinality('manyToOne', toName, fromName) };  // todo: make the service function to return an object that can be assigned directly
                        $scope.model.oneToOneSvgConfig = { svgs: helper.getSvgSettingsByCardinality('oneToOne', toName, fromName) };
                    }
                    else if (uiCardinality === 'oneToMany' || uiCardinality === 'manyToMany') {
                        $scope.model.oneToManySvgConfig = { svgs: helper.getSvgSettingsByCardinality('oneToMany', fromName, toName) };
                        $scope.model.manyToManySvgConfig = { svgs: helper.getSvgSettingsByCardinality('manyToMany', fromName, toName) };
                    }
                }
            }

            function setSecuritySvgSettings(uiCardinality, relationshipToRender) {
                if (!uiCardinality || !relationshipToRender) {
                    return;
                }

                // Get the type names
                var fromTypeName = relationshipToRender.fromType.name ? relationshipToRender.fromType.name : 'Unnamed Resource';
                var toTypeName = relationshipToRender.toType.name ? relationshipToRender.toType.name : 'Unnamed Resource';

                // Reset flag that indicates if to and from types are swapped in the display
                $scope.model.securityUi.toFromSwapped = false;

                switch (uiCardinality) {
                case 'manyToOne':
                case 'oneToOne':
                    $scope.model.securityUi.toFromSwapped = !$scope.model.isReverseRelationship;
                    break;
                case 'oneToMany':
                case 'manyToMany':
                    $scope.model.securityUi.toFromSwapped = $scope.model.isReverseRelationship;
                    break;
                }

                // Set the type names                
                $scope.model.securityUi.fromName = $scope.model.securityUi.toFromSwapped ? toTypeName : fromTypeName;
                $scope.model.securityUi.toName = $scope.model.securityUi.toFromSwapped ? fromTypeName : toTypeName;

                // Create the svgs
                $scope.model.securesToSvgConfig = { svgs: helper.getSvgSettingsByCardinality(uiCardinality, $scope.model.securityUi.fromName, $scope.model.securityUi.toName, false, true) };
                $scope.model.securesFromSvgConfig = { svgs: helper.getSvgSettingsByCardinality(uiCardinality, $scope.model.securityUi.fromName, $scope.model.securityUi.toName, true, false) };
            }

            function setOwnershipSvgSettings(uiCardinality, fromNameText, toNameText) {
                // workout fromName and toName
                var fullOwnershipFromNameText = fromNameText;
                var fullOwnershipToNameText = toNameText;

                if ($scope.model.ui.relCardinality === 'oneToOne' && $scope.model.ui.relOwnership === 'full') {
                    if ($scope.model.ui.relOwner.value === 'fromType') {
                        fullOwnershipFromNameText = $scope.model.ui.toName;
                        fullOwnershipToNameText = $scope.model.ui.fromName;
                    }
                    else if ($scope.model.ui.relOwner.value === 'toType') {
                        fullOwnershipFromNameText = $scope.model.ui.fromName;
                        fullOwnershipToNameText = $scope.model.ui.toName;
                    }

                    // set values to update the info text 
                    $scope.model.ui.relOwnerText = fullOwnershipToNameText;
                    $scope.model.ui.relNonOwnerText = fullOwnershipFromNameText;
                }

                // set deleted svg settings
                $scope.model.noOwnershipDeletedSvgConfig = helper.getDeletedSvgSettingsByCardinalityAndOwnership(uiCardinality, 'none', toNameText, fromNameText);
                $scope.model.partOwnershipDeletedSvgConfig = helper.getDeletedSvgSettingsByCardinalityAndOwnership(uiCardinality, 'part', toNameText, fromNameText);
                $scope.model.fullOwnershipDeletedSvgConfig = helper.getDeletedSvgSettingsByCardinalityAndOwnership(uiCardinality, 'full', fullOwnershipToNameText, fullOwnershipFromNameText);

                // set duplicated svg settings
                $scope.model.noOwnershipDuplicatedSvgConfig = helper.getDuplicatedSvgSettingsByCardinalityAndOwnership(uiCardinality, 'none', toNameText, fromNameText);
                $scope.model.partOwnershipDuplicatedSvgConfig = helper.getDuplicatedSvgSettingsByCardinalityAndOwnership(uiCardinality, 'part', toNameText, fromNameText);
                $scope.model.fullOwnershipDuplicatedSvgConfig = helper.getDuplicatedSvgSettingsByCardinalityAndOwnership(uiCardinality, 'full', fullOwnershipToNameText, fullOwnershipFromNameText);

            }

            function loadFormControls() {

                // label (name field of formControl)
                $scope.model.formCtrlNameControl = configureDialogService.getDummyFieldControlOnForm(getField('core:name'), 'Display Name');

                if ($scope.isLookup) {

                    // Allow create new
                    if ($scope.model.formControl.canCreate && $scope.model.formControl.canCreateDerivedTypes) {
                        $scope.model.selectedCanCreate = _.find($scope.relCanCreateComboOptions, { 'name': helper.resourceViewerComboItemYesIncDerivedTypes });
                    } else if ($scope.model.formControl.canCreate) {
                        $scope.model.selectedCanCreate = _.find($scope.relCanCreateComboOptions, { 'name': helper.resourceViewerComboItemYes });
                    } else {
                        $scope.model.selectedCanCreate = _.find($scope.relCanCreateComboOptions, { 'name': helper.resourceViewerComboItemNo });
                    }
                }

                // display as
                var lookupFieldDisplayAs;
                if ($scope.model.formControl.typesP[0].alias() === 'console:dropDownRelationshipRenderControl') {
                    lookupFieldDisplayAs = helper.relDisplayAsComboItemDropdown;
                }
                else if ($scope.model.formControl.typesP[0].alias() === 'console:inlineRelationshipRenderControl') {
                    lookupFieldDisplayAs = helper.relDisplayAsComboItemInline;
                }
                else if ($scope.model.formControl.typesP[0].alias() === 'console:tabRelationshipRenderControl') {
                    lookupFieldDisplayAs = helper.relDisplayAsComboItemReport;
                }

                if (!lookupFieldDisplayAs) {

                    if (!$scope.isLookup) {
                        lookupFieldDisplayAs = helper.relDisplayAsComboItemInline;
                    } else {
                        lookupFieldDisplayAs = helper.relDisplayAsComboItemReport;
                    }
                }

                $scope.model.selectedDisplayAs = _.find($scope.displayAsComboOptions, { 'name': lookupFieldDisplayAs });

                // set 'isRelationshipWithInlineDisplay' flag to show/hide display report option for relationship control
                updateIsRelationshipWithInlineDisplayFlag();
            }

            function loadRelationshipControls() {
                // toName and fromName
                var nameField = $scope.model.isReverseRelationship ? 'core:fromName' : 'core:toName';

                //to check an existing relationship is set the fromName or toName, if not, the nameField is set to 'core:Name'
                if (!$scope.model.isNewRelationship && !spUtils.isNullOrUndefined($scope.model.relationshipToRender) &&
                    (($scope.model.isReverseRelationship && !$scope.model.relationshipToRender.fromName) ||
                    (!$scope.model.isReverseRelationship && !$scope.model.relationshipToRender.toName)))
                {
                    nameField = 'core:name';
                }

                var scriptNameField = $scope.model.isReverseRelationship ? 'core:fromScriptName' : 'core:toScriptName';
                var reverseNameField = $scope.model.isReverseRelationship ? 'core:toName' : 'core:fromName';
                $scope.model.nameControl = configureDialogService.getDummyFieldControlOnForm(getField(nameField), 'Name');
                $scope.model.nameControl.mandatoryControl = true;
                $scope.model.scriptNameControl = configureDialogService.getDummyFieldControlOnForm(getField(scriptNameField), 'Script Name');
                $scope.model.scriptNameControl.mandatoryControl = true;
                $scope.model.reverseNameControl = configureDialogService.getDummyFieldControlOnForm(getField(reverseNameField), 'Reverse Name');
                $scope.model.reverseNameControl.mandatoryControl = true;

                // relationship name
                $scope.model.relNameControl = configureDialogService.getDummyFieldControlOnForm(getField('core:name'), 'Relationship Name');
                $scope.model.relNameControl.mandatoryControl = true;

                // description
                $scope.model.relDescriptionControl = configureDialogService.getDummyFieldControlOnForm(getField('core:description'), 'Description');

            }

            function setDataOnInitialLoad() {
                // object
                setObjectTypeOptions();

                // default value
                setDefaultValueOptions();

                // update the relationship internal values/flags
                if ($scope.model.relationshipToRender) {
                    helper.setRelationshipInternalValues($scope.model.actualRelType, $scope.model.relationshipToRender);
                }

                // calculate toName,fromName,relName and set respective name related flags
                if ($scope.model.isNewRelationship) {
                    calcAllNames();
                }
            }

            function setDialogUiValues() {
                $scope.model.ui.dialogTitle = $scope.isLookup ? 'Lookup Properties' : 'Relationship Properties';

                collapseAllSections();
            }

            function setSelectedDisplayForm() {
                // display form
                if ($scope.model.formControl.resourceViewerConsoleForm) {
                    $scope.model.selectedConsoleForm = _.find($scope.typeForms, { 'idP': $scope.model.formControl.resourceViewerConsoleForm.idP });
                }
            }

            function setSelectedPickerReport() {
                // display form
                if ($scope.model.formControl.pickerReport) {
                    $scope.model.selectedPickerReport = _.find($scope.typeResourcePickers, { 'idP': $scope.model.formControl.pickerReport.idP });
                }
            }

            function setSelectedDisplayReport() {
                // display form
                if ($scope.model.formControl.relationshipDisplayReport) {
                    $scope.model.selectedDisplayReport = _.find($scope.typeReports, { 'idP': $scope.model.formControl.relationshipDisplayReport.idP });
                }
            }

            function setObjectTypeOptions() {
                var definitionEntity = getEntity('core:definition');
                $scope.model.objectTypePickerOptions.entityTypeId = definitionEntity.idP;

                if ($scope.model.relationshipToRender) {
                    $scope.model.objectTypePickerOptions.selectedEntities = $scope.model.isReverseRelationship ? $scope.model.relationshipToRender.getRelationship('core:fromType') : $scope.model.relationshipToRender.getRelationship('core:toType');
                }

                $scope.model.objectTypePickerOptions.pickerReportId = $scope.templateReport.idP;

                $scope.model.objectTypePickerOptions.isDisabled = !$scope.model.isNewRelationship || ($scope.model.isNewRelationship && $scope.options.showInReverse);
            }

            function setDefaultValueOptions() {
                //var definitionEntity = getEntity('core:definition');
                var relationship = $scope.model.isControl ? $scope.options.formControl.relationshipToRender : $scope.options.relationship;
                if (relationship) {
                    var entityType = $scope.model.isReverseRelationship ? relationship.getFromType() : relationship.getToType();
                    if (entityType) {
                        $scope.model.defaultValuePickerOptions.entityTypeId = entityType.idP;
                    }

                    $scope.model.defaultValuePickerOptions.selectedEntities = $scope.model.isReverseRelationship ? relationship.getRelationship('core:fromTypeDefaultValue') : relationship.getRelationship('core:toTypeDefaultValue');

                    // set useCurrentUser flag
                    $scope.model.useCurrentUserAsDefault = $scope.model.isReverseRelationship ? relationship.getField('core:defaultFromUseCurrent') : relationship.getField('core:defaultToUseCurrent');
                }

                $scope.model.defaultValuePickerOptions.pickerReportId = $scope.templateReport.idP;
            }

            function updateRelTypeFromUI() {
                // hack:
                // 'part' ownership is not a supported option for 'oneToOne' cardinality (set the ownership to 'none')
                if ($scope.model.ui.relCardinality === 'oneToOne' && $scope.model.ui.relOwnership === 'part') {
                    $scope.model.ui.relOwnership = 'none';
                }

                // 'full' ownership is not a supported option for 'manyToMany' cardinality (set the ownership to 'none')
                if ($scope.model.ui.relCardinality === 'manyToMany' && ($scope.model.ui.relOwnership === 'full' || $scope.model.ui.relOwnership === 'part')) {
                    $scope.model.ui.relOwnership = 'none';
                }

                // update actualRelType and effectiveRelType
                $scope.model.actualRelType = getActualRelType($scope.model.isReverseRelationship, $scope.model.ui.relCardinality, $scope.model.ui.relOwnership, $scope.model.ui.relOwner);

                $scope.model.effectiveRelType = $scope.model.isReverseRelationship ? helper.getEffectiveRelTypeByActualRelType($scope.model.actualRelType) : $scope.model.actualRelType;

                // update the relationship internal values/flags
                helper.setRelationshipInternalValues($scope.model.actualRelType, $scope.model.relationshipToRender);

                // update the svg settings for relType and ownership
                setRelTypeSvgSettings($scope.model.ui.relCardinality, $scope.model.ui.fromName, $scope.model.ui.toName);
                setOwnershipSvgSettings($scope.model.ui.relCardinality, $scope.model.ui.fromName, $scope.model.ui.toName);
                setSecuritySvgSettings($scope.model.ui.relCardinality, $scope.model.relationshipToRender);
            }

            function getActualRelType(isReverseRelationship, effectiveCardinality, ownership, relOwner) {
                var actualRelType;

                if (effectiveCardinality === 'manyToOne') {
                    if (ownership === 'none') {
                        actualRelType = isReverseRelationship ? helper.getRelType('core:relExclusiveCollection') : helper.getRelType('core:relLookup');
                    }
                    else if (ownership === 'part') {
                        actualRelType = isReverseRelationship ? helper.getRelType('core:relDependants') : helper.getRelType('core:relDependantOf');
                    }
                    else if (ownership === 'full') {
                        actualRelType = isReverseRelationship ? helper.getRelType('core:relComponents') : helper.getRelType('core:relComponentOf');
                    }
                }
                else if (effectiveCardinality === 'oneToOne') {
                    if (ownership === 'none') {
                        actualRelType = helper.getRelType('core:relSingleLookup');
                    }
                    else if (ownership === 'full') {
                        var tempRelType;
                        if (isReverseRelationship) {
                            tempRelType = (relOwner.value === 'fromType') ? 'core:relSingleComponentOf' : 'core:relSingleComponent';
                        }
                        else {
                            tempRelType = (relOwner.value === 'fromType') ? 'core:relSingleComponent' : 'core:relSingleComponentOf';
                        }
                        actualRelType = helper.getRelType(tempRelType);
                    }
                    else if (ownership === 'part') {
                        // todo:
                        raiseErrorAlert('part ownership is not a valid option');
                    }
                }
                else if (effectiveCardinality === 'oneToMany') {
                    if (ownership === 'none') {
                        actualRelType = isReverseRelationship ? helper.getRelType('core:relLookup') : helper.getRelType('core:relExclusiveCollection');
                    }
                    else if (ownership === 'part') {
                        actualRelType = isReverseRelationship ? helper.getRelType('core:relDependantOf') : helper.getRelType('core:relDependants');
                    }
                    else if (ownership === 'full') {
                        actualRelType = isReverseRelationship ? helper.getRelType('core:relComponentOf') : helper.getRelType('core:relComponents');
                    }
                }
                else if (effectiveCardinality === 'manyToMany') {
                    // ** note: ownership is not supported for M:M in UI. set ownership to 'none'
                    actualRelType = helper.getRelType('core:relManyToMany');
                }

                return actualRelType;
            }

            // Keep the script name in sync with the effective 'To name', for new relationships
            // Called when the effective 'to name' gets changed (i.e. from name if in reverse)
            function checkScriptName(newValue, oldValue) {
                // don't edit script name automatically unless the relationship is new
                if (!$scope.model.isNewRelationship) return;
                var prop = $scope.model.isReverseRelationship ? 'fromScriptName' : 'toScriptName';
                var rel = $scope.model.relationshipToRender;
                if (rel[prop] === oldValue) {
                    rel[prop] = newValue;
                    refreshControl('scriptNameControl');
                }
            }

            function checkDefaultScriptNames() {
                var rel = $scope.model.relationshipToRender;
                if (rel && !rel.fromScriptName) rel.fromScriptName = rel.fromName;
                if (rel && !rel.toScriptName) rel.toScriptName = rel.toName;
            }

            function refreshControl(controlModelPropertyName) {
                $scope.model[controlModelPropertyName] = jQuery.extend(true, {}, $scope.model[controlModelPropertyName]);
            }

            function loadFormsForType(typeId) {
                configureDialogService.getFormsForTypeAndInheritedTypes(typeId).then(
                    function (result) {
                        var forms = [];
                        var inherits = spResource.getAncestorsAndSelf(result);
                        _.forEach(inherits, function (inherit) {
                            //bug 26694, PMs want to hide the "Resource with Application Form" which is the default form of core:resource.  
                            if (inherit.nsAlias !== 'core:resource') {
                                var filtertedTypeForms = [];
                                var typeforms = inherit.getRelationship('console:formsToEditType');
                                _.forEach(typeforms, function (form) {
                                    if (form.alias() !== 'console:resourceInfoEditForm' && form.alias() !== 'core:resourceForm') {  // filter the system forms
                                        filtertedTypeForms.push(form);
                                    }
                                });

                                forms = forms.concat(filtertedTypeForms);
                            }
                        });

                        $scope.typeForms = forms;

                        if ($scope.model.isControl) {
                            setSelectedDisplayForm();
                        }
                    },
                    function (error) {
                        $scope.model.addError(error);
                    });
            }

            function loadReportsForType(typeId) {
                configureDialogService.getReportsForType(typeId).then(
                    function (result) {
                        var typeResourcePickers = [];
                        var inherits = spResource.getAncestorsAndSelf(result);
                        _.forEach(inherits, function (inherit) {
                            var typeReports = inherit.getRelationship('definitionUsedByReport');
                            typeResourcePickers = typeResourcePickers.concat(typeReports);
                        });

                        // filter out the access rule reports
                        var filteredResourcePickers = _.filter(typeResourcePickers, function (r) {
                            return r.getRelationship('reportForAccessRule').length === 0;
                        });
                        $scope.typeResourcePickers = filteredResourcePickers;

                        var reports = [];
                        _.forEach(typeResourcePickers, function (resourcePicker) {
                            if (resourcePicker.type.alias() === 'core:report') {  // filter the reports only
                                reports.push(resourcePicker);
                            }
                        });
                        $scope.typeReports = reports;

                        if ($scope.model.isControl) {

                            // set picker report
                            setSelectedPickerReport();

                            // set display report
                            if (!$scope.isLookup) {
                                setSelectedDisplayReport();
                            }
                        }
                    },
                    function (error) {
                        $scope.model.addError(error);
                    });
            }

            function updateUseCurrentUserChkboxVisibility(typeId) {
                configureDialogService.getTypeAndInheritedTypes(typeId).then(
                    function (result) {
                        var showUseCurrentOption = false;
                        var inherits = spResource.getAncestorsAndSelf(result);

                        for (var ctr = 0; ctr < inherits.length; ctr++) {
                            var entity = inherits[ctr];

                            if (typeId === entity.idP)
                            { $scope.model.currentUserDisplayText = 'Use Current \'' + entity.name + '\''; } // set the display text

                            if (entity.alias() === "core:person" || entity.alias() === "core:userAccount") {
                                showUseCurrentOption = true;
                                break;
                            }
                        }

                        $scope.model.showUseCurrentUserOption = showUseCurrentOption;

                        setUseCurrentDisplayText();
                    },
                    function (error) {
                        $scope.model.addError(error);
                    });
            }

            function setUseCurrentDisplayText() {
                if ($scope.model.defaultValuePickerOptions) {
                    $scope.model.defaultValuePickerOptions.displayString = $scope.model.useCurrentUserAsDefault ? $scope.model.currentUserDisplayText : "";
                }
            }

            function calcAllNames() {
                var relationship = $scope.model.relationshipToRender;                

                // set relationship Name default flag
                _relNameIsDefault = getIsRelationshipNameDefault();
                _uiToNameIsDefault = getIsRelationshipToNameDefault();
                _uiFromNameIsDefault = getIsRelationshipFromNameDefault();

                //if ($scope.model.isReverseRelationship) {
                //    // rev
                //    _uiToNameIsDefault = !relationship.fromName || (!spUtils.isNullOrUndefined(relationship.fromType) && relationship.fromName === relationship.fromType.name);
                //    _uiFromNameIsDefault = !relationship.toName || (!spUtils.isNullOrUndefined(relationship.toType) && relationship.toName === relationship.toType.name);
                //}
                //else {
                //    // fwd
                //    _uiToNameIsDefault = !relationship.toName || (!spUtils.isNullOrUndefined(relationship.toType) && relationship.toName === relationship.toType.name);
                //    _uiFromNameIsDefault = !relationship.fromName || (!spUtils.isNullOrUndefined(relationship.fromType) && relationship.fromName === relationship.fromType.name);
                //}


                // set the values
                if ($scope.model.isReverseRelationship) {
                    if (_uiFromNameIsDefault && !spUtils.isNullOrUndefined(relationship.toType)) {
                        $scope.model.isRelFromNameInternalUpdate = true;
                        relationship.fromName = relationship.toType.name; // set fromName
                    }
                    if (_uiToNameIsDefault && !spUtils.isNullOrUndefined(relationship.fromType)) {
                        $scope.model.isRelToNameInternalUpdate = true;
                        relationship.toName = relationship.fromType.name; // set toName
                    }
                    if (_relNameIsDefault && relationship.fromName && relationship.toName) {
                        $scope.model.isRelNameInternalUpdate = true;
                        relationship.name = relationship.toName + " - " + relationship.fromName;    // set rel name
                    }
                }
                else {
                    if (_uiFromNameIsDefault && !spUtils.isNullOrUndefined(relationship.fromType)) {
                        $scope.model.isRelFromNameInternalUpdate = true;
                        relationship.fromName = relationship.fromType.name; // set fromName
                    }
                    if (_uiToNameIsDefault) {
                        $scope.model.isRelToNameInternalUpdate = true;
                        relationship.toName = !spUtils.isNullOrUndefined(relationship.toType) ? relationship.toType.name : ''; // set toName
                    }
                    if (_relNameIsDefault && relationship.fromName && relationship.toName) {
                        $scope.model.isRelNameInternalUpdate = true;
                        relationship.name = relationship.fromName + " - " + relationship.toName;    // set rel name
                    }
                }
            }

            function updateToNameIfNecessary() {
                if (_uiToNameIsDefault) {
                    var relationship = $scope.model.relationshipToRender;
                    if ($scope.model.isReverseRelationship && relationship.fromType) {
                        $scope.model.isRelFromNameInternalUpdate = true;
                        relationship.fromName = relationship.fromType.name;
                    }
                    else if (!spUtils.isNullOrUndefined(relationship.toType)) {
                        $scope.model.isRelToNameInternalUpdate = true;
                        relationship.toName = relationship.toType.name;
                    }

                    updateRelNameIfNecessary();
                }
            }

            function updateRelNameIfNecessary() {
                if (_relNameIsDefault) {
                    $scope.model.isRelNameInternalUpdate = true;
                    $scope.model.relationshipToRender.name = calcRelName();
                }
            }

            function getIsRelationshipToNameDefault() {
                var relationship = $scope.model.relationshipToRender;
                if (!relationship) {
                    return '';
                }

                if ($scope.model.isReverseRelationship) {
                    // rev
                    return  !relationship.fromName || (!spUtils.isNullOrUndefined(relationship.fromType) && relationship.fromName === relationship.fromType.name);
                }
                else {
                    // fwd
                    return !relationship.toName || (!spUtils.isNullOrUndefined(relationship.toType) && relationship.toName === relationship.toType.name);
                }
            }

            function getIsRelationshipFromNameDefault() {
                var relationship = $scope.model.relationshipToRender;
                if (!relationship) {
                    return '';
                }

                if ($scope.model.isReverseRelationship) {
                    // rev
                    return !relationship.toName || (!spUtils.isNullOrUndefined(relationship.toType) && relationship.toName === relationship.toType.name);
                }
                else {
                    // fwd
                    return !relationship.fromName || (!spUtils.isNullOrUndefined(relationship.fromType) && relationship.fromName === relationship.fromType.name);
                }
            }

            function getIsRelationshipNameDefault() {
                var relationship = $scope.model.relationshipToRender;
                var relName = relationship ? relationship.name : '';

                return  !relName || (relationship.toName && relationship.fromName && relName === calcRelName());
            }

            function calcRelName() {
                return $scope.model.relationshipToRender.fromName + " - " + $scope.model.relationshipToRender.toName;
            }

            ///---------------- helpers -----------------------------///
            function getField(alias) {
                return _.find($scope.fields, { 'nsAlias': alias });
            }

            function getEntity(alias) {
                return _.find($scope.entities, { 'nsAlias': alias });
            }

            // Add an error
            $scope.model.addError = function (errorMsg) {
                console.error(errorMsg);
                $scope.model.errors.push({ type: 'error', msg: errorMsg });
            };

            // Clear any errors
            $scope.model.clearErrors = function () {
                $scope.model.errors = [];
            };

            // collapse/expand sections 
            $scope.toggleSection = function (section) {
                // check if object is selected. if not then keep everything collapsed
                if ($scope.model.objectTypePickerOptions.selectedEntities.length > 0) {
                    if (section === 'relType') {
                        $scope.isRelTypeSectionCollapsed = !$scope.isRelTypeSectionCollapsed;
                        if ($scope.isRelTypeSectionCollapsed) {
                            $scope.relTypeToggleImageUrl = collapsedImageUrl;
                        } else {
                            $scope.relTypeToggleImageUrl = expandedImageUrl;
                        }
                    }
                    else if (section === 'ownership') {
                        $scope.isOwnershipSectionCollapsed = !$scope.isOwnershipSectionCollapsed;
                        if ($scope.isOwnershipSectionCollapsed) {
                            $scope.ownershipToggleImageUrl = collapsedImageUrl;
                        } else {
                            $scope.ownershipToggleImageUrl = expandedImageUrl;
                        }
                    }
                    else if (section === 'security') {
                        $scope.isSecuritySectionCollapsed = !$scope.isSecuritySectionCollapsed;
                        if ($scope.isSecuritySectionCollapsed) {
                            $scope.securityToggleImageUrl = collapsedImageUrl;
                        } else {
                            $scope.securityToggleImageUrl = expandedImageUrl;
                        }
                    }
                    else if (section === 'general') {
                        $scope.isGeneralSectionCollapsed = !$scope.isGeneralSectionCollapsed;
                        if ($scope.isGeneralSectionCollapsed) {
                            $scope.generalToggleImageUrl = collapsedImageUrl;
                        } else {
                            $scope.generalToggleImageUrl = expandedImageUrl;
                        }
                    }
                }
                else {
                    $scope.model.isErrorMsgVisible = true;
                    $timeout(expireAlerts, 5000);
                }
            };

            function expireAlerts() {
                $scope.model.isErrorMsgVisible = false;
            }

            function collapseAllSections() {
                $scope.isRelTypeSectionCollapsed = true;
                $scope.isOwnershipSectionCollapsed = true;
                $scope.isGeneralSectionCollapsed = true;
                $scope.isSecuritySectionCollapsed = true;
                $scope.relTypeToggleImageUrl = collapsedImageUrl;
                $scope.ownershipToggleImageUrl = collapsedImageUrl;
                $scope.generalToggleImageUrl = collapsedImageUrl;
                $scope.securityToggleImageUrl = collapsedImageUrl;
            }

            function getRelationshipTypeInReverseDirection() {
                if ($scope.model.ui.relCardinality === 'oneToOne' || $scope.model.ui.relCardinality === 'oneToMany') {
                    return 'lookup';
                }
                else if($scope.model.ui.relCardinality === 'manyToOne' || $scope.model.ui.relCardinality === 'manyToMany') {
                    return 'relationship';
                }
                else {
                    console.error('getRelationshipTypeInReverseDirection: relationship cardinality is not set');
                    return '';
                }
            }

            ///***************************************** Debug info popup - remove once the testing is complete *****************************************************************************///
            //////  Debug info popup  //////
            (function () {
                var modalInstanceCtrl = ['$scope', '$uibModalInstance', 'outerScopeModel', function ($scope, $uibModalInstance, outerScopeModel) {

                    $scope.model = {};
                    $scope.model.outerScopeModel = outerScopeModel;

                    $scope.ok = function () {
                        $uibModalInstance.close($scope.model);

                    };
                }];

                $scope.openDetail = function () {
                    var defaults = {
                        /*jshint multistr: true */
                        template: '<table id="relTypeDebugInfo">\
                                <tr>\
                                    <th>Cardinality</th>\
                                    <th>relType</th>\
                                    <th>mandatory</th>\
                                    <th>mandatory (rev)</th>\
                                    <th>cascadeDelete</th>\
                                    <th>cascadeDeleteTo</th>\
                                    <th>cloneAction</th>\
                                    <th>RevCloneAction</th>\
                                    <th>inSolution</th>\
                                    <th>RevInSolution</th>\
                                </tr>\
                                <tr>\
                                    <td>{{model.outerScopeModel.relationshipToRender.cardinality.alias()}}</td>\
                                    <td>{{model.outerScopeModel.actualRelType.alias()}}</td>\
                                    <td>{{model.outerScopeModel.relationshipToRender.relationshipIsMandatory}}</td>\
                                    <td>{{model.outerScopeModel.relationshipToRender.revRelationshipIsMandatory}}</td>\
                                    <td>{{model.outerScopeModel.relationshipToRender.cascadeDelete}}</td>\
                                    <td>{{model.outerScopeModel.relationshipToRender.cascadeDeleteTo}}</td>\
                                    <td>{{model.outerScopeModel.relationshipToRender.cloneAction.alias()}}</td>\
                                    <td>{{model.outerScopeModel.relationshipToRender.reverseCloneAction.alias()}}</td>\
                                    <td>{{model.outerScopeModel.relationshipToRender.implicitInSolution}}</td>\
                                    <td>{{model.outerScopeModel.relationshipToRender.reverseImplicitInSolution}}</td>\
                                </tr>\
                            </table>',
                        controller: modalInstanceCtrl,
                        windowClass: 'modal sp-relationship-properties-modal-debug',
                        resolve: {
                            outerScopeModel: function () {
                                return $scope.model;
                            }
                        }
                    };

                    var options = {};

                    spDialogService.showDialog(defaults, options).then(function () {

                    });

                };
            })();
            ///*****************************************************************************************************************************************************************************///


            function raiseErrorAlert(message) {
                spAlertsService.addAlert(message, { severity: spAlertsService.sev.Error });
            }

        });
}());