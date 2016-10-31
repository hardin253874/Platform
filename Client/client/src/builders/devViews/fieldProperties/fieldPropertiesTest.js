// Copyright 2011-2016 Global Software Innovation Pty Ltd
(function () {
    'use strict';

    /////
    // Field Properties test controller.
    /////
    angular.module('app.fieldPropertiesTest', ['mod.common.ui.spDialogService', 'ui.bootstrap', 'titleService', 'mod.common.spEntityService', 'ngGrid', 'mod.app.editFormServices',
                   'mod.app.editForm.designerDirectives', 'mod.common.alerts', 'mod.app.editForm', 'mod.app.configureDialog'])
        .controller('fieldPropertiesTestController', function ($scope, spDialogService, titleService, spEntityService, $q, $timeout, spEditForm, spAlertsService, configureDialogService, controlConfigureDialogFactory) {
            titleService.setTitle('Field Properties Test');

            $scope.fieldType =
                [
                    {
                        name: 'string',
                        dataType: 'core:stringField'
                    },
                    {
                        name: 'Multiline',
                        dataType: 'core:stringField'
                    },
                    {
                        name: 'Int',
                        dataType: 'core:intField'
                    },
                    {
                        name: 'Decimal',
                        dataType: 'core:decimalField'
                    },
                    {
                        name: 'Currency',
                        dataType: 'core:currencyField'
                    },
                    {
                        name: 'AutoNumber',
                        dataType: 'core:autoNumberField'
                    },
                    {
                        name: 'Date Only',
                        dataType: 'core:dateField'
                    },
                    {
                        name: 'Time Only',
                        dataType: 'core:timeField'
                    },
                    {
                        name: 'Date and Time',
                        dataType: 'core:dateTimeField'
                    },
                    {
                        name: 'Yes/No',
                        dataType: 'core:boolField'
                    }
                ];
            $scope.fields = [];
            $scope.fieldControls = [];

            $scope.addNewField = function() {
                var field = spEntity.fromJSON({
                    typeId: $scope.selectedType.dataType,
                    isOfType: [{
                        id: $scope.selectedType.dataType,
                        alias: $scope.selectedType.dataType
                    }],
                    name: 'new' + $scope.selectedType.name +'field',
                    description: 'new field description'
                });
                $scope.fields.push(field);
            };

            //	Save	the	updates	to	the	resource
            $scope.save = function () {
                spEntityService.putEntity($scope.formControl).then(
                    function (result) {
                        {
                            window.alert('Saved', 'form is saved');
                        }
                    },
                    function (error) {
                        window.alert(error);
                    });
            };

            $scope.closeAlert = function (index) {
                $scope.alerts.splice(index, 1);
            };


            $scope.getRelationshipName = function (control) {
                var relationship = control.relationshipToRender;
                return control.getName() || relationship.getName();
            };
            /* getFormDefinition promise */

            function getFormDefinition(selectedFormId) {
                return spEditForm.getFormDefinition(selectedFormId, true);
            }

            //
            //	Init
            //
            $scope.model = {
                formId: 'test:allFieldsForm',
                dirty: false
            };

           // configureDialogService.setFormId($scope.model.formId);

            $scope.fieldValidationMessages = [];
            $scope.alerts = [];


            $scope.isInDesigner = true;
            $scope.formControl = null;
            $scope.formData = null;
            
            /*	getFormControl	*/
            $scope.getFormDefinition = function () {
                getFormDefinition($scope.model.formId).then(
                   function (formControl) {
                       $scope.alerts.push({ type: 'success', msg: 'Got formControl' });

                       $scope.formControl = formControl;
                       $scope.allControls = getAllControl();
                       console.log('form controls', formControl.getContainedControlsOnForm());
                       $scope.errorMessage = "";
                   },
                   function (error) {
                       console.log('Error while trying to get Form:', error);
                       $scope.errorMessage = 'Error while trying to get Form:' + error.data.ErrorMessage;
                       $scope.alerts.push({ type: 'error', msg: 'Error while trying to get form:' + error.data.ErrorMessage }); // NOTE: Need better error message
                   });
            };
           
            $scope.isFirstStructureControl = true;
            $scope.isReadOnly = true;
            $scope.isInTestMode = true;

            $scope.controlClass = 'vertical-stack-container-control-design';
         
            function getAllControl() {
                if ($scope.formControl && $scope.formControl.getContainedControlsOnForm) {

                    //  return $scope.formControl.getContainedControlsOnForm();

                    var flat = spUtils.walkGraph(
                           function (e) {
                               var b = e.getRelationship('console:containedControlsOnForm');
                               return b || [];
                           },
                           $scope.formControl);
                    return flat;
                }
                return [];
            }
            /////
            // Get the controls contained within this control directly referencing the entity model.
            /////
            $scope.getFormControls = function () {
                var fieldControls = [];
                  
                _.forEach($scope.allControls, function (control) {
                      //  console.log('control alias', control.getIsOfType()[0].getAlias());
                        if (isFieldContainerControl(control)) {
                            fieldControls.push(control);
                        }

                    });
                    return fieldControls;
              
            };

            $scope.getRelationshipControls = function() {
                var relationshipControls = [];
                _.forEach($scope.allControls, function (control) {
                  //  console.log('control alias', control.getIsOfType()[0].getAlias());
                    if (isrelationshipContainerControl(control)) {
                        relationshipControls.push(control);
                    }

                });
                return relationshipControls;
            };
            
            function isrelationshipContainerControl(control) {
                var controlType = control.getIsOfType()[0].getAlias();
                switch (controlType) {
                    case 'console:tabRelationshipRenderControl':
                    case 'console:imageRelationshipRenderControl':
                    case 'console:multiChoiceRelationshipRenderControl':
                    case 'console:choiceRelationshipRenderControl':
                    case 'console:inlineRelationshipRenderControl':
                    case 'console:dropDownRelationshipRenderControl':
                        return true;
                    default:
                        return false;
                }
            }

            function isFieldContainerControl(control) {
                var fieldControl = true;
                var controlType = control.getIsOfType()[0].getAlias();
                switch (controlType) {
                    case 'console:verticalStackContainerControl':
                    case 'console:horizontalStackContainerControl':
                    case 'console:tabContainerControl':
                    case 'console:customEditForm':
                    case 'console:headerColumnContainerControl':
                    case 'console:tabRelationshipRenderControl':
                    case 'console:imageRelationshipRenderControl':
                    case 'console:multiChoiceRelationshipRenderControl':
                    case 'console:choiceRelationshipRenderControl':
                    case 'console:inlineRelationshipRenderControl':
                    case 'console:dropDownRelationshipRenderControl':
                        fieldControl = false;
                        break;
                }
                return fieldControl;
            }

            /////
            // Filter used by the repeater to order the controls.
            /////
            $scope.renderingOrdinal = function (formControl) {

                if (formControl && formControl.renderingOrdinal) {
                    return formControl.renderingOrdinal || 0;
                }

                return null;
            };
            /////////////////////////////////////////////////////////////////////////
            //Code needed to integrate with configure dialog from the form builder
            /////////////////////////////////////////////////////////////////////////////
            
            $scope.onClick = function (fieldFormControl) {
                var field = fieldFormControl.getFieldToRender();
                if (field) {

                    //Build option variable
                    var options = {
                        formControl: fieldFormControl,
                        isFieldControl:true
                    };

                    controlConfigureDialogFactory.createDialog(options).then(function (result) {

                        if (result !== false) {
                            fieldFormControl = result;
                        }
                    });
                } else {
                    window.alert('Its not a field');
                }
            };
            $scope.onFieldClick = function (fieldFormControl) {
                var field = fieldFormControl.getFieldToRender();
                if (field) {

                    //Build option variable
                    var options = {
                        formControl: field,
                        isFieldControl: false
                    };

                    controlConfigureDialogFactory.createDialog(options).then(function (result) {

                        if (result !== false) {
                            field = result;
                        }
                    });
                } else {
                    window.alert('Its not a field');
                }
            };

            $scope.onNewFieldClick = function(field) {
                if (field) {

                    //Build option variable
                    var options = {
                        formControl: field,
                        isFieldControl: false
                    };

                    controlConfigureDialogFactory.createDialog(options).then(function (result) {

                        if (result !== false) {
                            field = result;
                        }
                    });
                } else {
                    window.alert('Its not a field');
                }
            };
            
            $scope.onRelationshipControlClick = function (relationshipControl) {
                var relationship = relationshipControl.getRelationshipToRender();
                if (relationship) {

                    //Build option variable
                    var options = {
                        formControl: relationshipControl,
                        isFormControl: true,
                        relationshipType: getRelationshipType(relationshipControl)
                    };

                    controlConfigureDialogFactory.createDialog(options).then(function (result) {

                        if (result !== false) {
                            relationshipControl = result;
                        }
                    });
                }
            };

            $scope.onRelationshipClick = function(relationshipControl) {
                var relationship = relationshipControl.getRelationshipToRender();
                if (relationship) {

                    //Build option variable
                    var options = {
                        formControl: relationship,
                        isFormControl: false,
                        relationshipType: getRelationshipType(relationshipControl),
                        relationship: relationship
                    };

                    controlConfigureDialogFactory.createDialog(options).then(function(result) {

                        if (result !== false) {
                            relationshipControl = result;
                            relationship = result;
                        }
                    });
                }
            };

            /////////////////////////////////////////////////////////////////////////
            // END OF Code needed to integrate with configure dialog from the form builder 
            /////////////////////////////////////////////////////////////////////////
            
            function getRelationshipType(relationshipControl) {
                var controlType = relationshipControl.getIsOfType()[0].getAlias();
                switch (controlType) {
                    case 'console:imageRelationshipRenderControl':
                        return 'image';
                    case 'console:multiChoiceRelationshipRenderControl':
                    case 'console:choiceRelationshipRenderControl':
                        return 'choice';
                    case 'console:inlineRelationshipRenderControl':
                    case 'console:dropDownRelationshipRenderControl':
                        return 'lookup';
                    case 'console:tabRelationshipRenderControl':
                        return 'relationship';
                    default:
                        return '';
                }
            }


            $scope.isDirty = function () {
                if (!$scope.formControl || !$scope.formControl.hasChangesRecursive) {
                    return false;
                }

                $scope.model.dirty = $scope.formControl.hasChangesRecursive();

                return $scope.model.dirty;
            };
            
            var newFormControl = spEntity.fromJSON(
                {
                    alias: 'myControlEntity',
                    name: '',
                    description: '',
                    dataState: 1,
                    'console:mandatoryControl': false,
                    'console:readOnlyControl': false,
                    'console:showControlHelpText': false,
                    'console:isReversed': false,
                    typeId: 'console:singleLineTextControl',
                    isOfType: { id: 'k:singleLineTextControl' },
                    'k:renderingBackgroundColor': { a: 255, r: 255, g: 255, b: 255 },
                    fieldToRender: {
                        name: 'nameField',
                        description: '',
                        dataState: 1,
                        allowMultiLines: false,
                        typeId: 'stringField',
                        isOfType: [{
                            id: 'stringField',
                            alias: 'core:stringField',
                            'k:fieldDisplayName': { name: 'Text' },
                            'k:defaultRenderingControls': [{
                                alias: 'console:singleLineTextControl',
                                'k:context': { name: 'Html' }
                            }]
                        }]
                    }
                });

            $scope.configureNewControl = function() {
                //Build option variable
                var options = {
                    formControl: newFormControl,
                    isFieldControl: true
                };

                controlConfigureDialogFactory.createDialog(options).then(function (result) {

                    if (result !== false) {
                        newFormControl = result;
                    }
                });
            };
            
            /////////////////////////////////////////////////////////////////////////////
            ///////////////Code related to New instance of Choice Filed and Control//////

            var newChoiceFieldObject = spEntity.fromJSON({
                name: 'new choice field',
                description: 'new choice Field Description',
                dataState: 1,
                cardinality: { alias: 'manyToOne' },
                toType: jsonLookup(),
                relType: { alias: 'choiceField' },
                cascadeDelete: false,
                cascadeDeleteTo: false,
                cloneAction: { alias: 'cloneReferences' },
                reverseCloneAction: { alias: 'drop' },
                implicitInSolution: false,
                reverseImplicitInSolution: false
            });
            var formControlDefaultObject =spEntity.fromJSON( {
                name: jsonString(''),
                description: jsonString(''),
                dataState:1,
                isOfType: [{ alias: 'console:choiceRelationshipRenderControl' }],
                'k:renderingBackgroundColor': { a: 255, r: 255, g: 255, b: 255 },
                'k:mandatoryControl': false,
                'k:readOnlyControl': false,
                'k:relationshipToRender': {
                    name: 'new choice field',
                    description: 'new choice Field Description',
                    dataState: 1,
                    cardinality: { alias: 'manyToOne' },
                    toType: jsonLookup(),
                    relType: { alias: 'choiceField' },
                    cascadeDelete: false,
                    cascadeDeleteTo: false,
                    cloneAction: { alias: 'cloneReferences' },
                    reverseCloneAction: { alias: 'drop' },
                    implicitInSolution: false,
                    reverseImplicitInSolution: false
                }
            });
            
            $scope.onNewChoiceFieldClick = function (){
                
                //Build option variable
                var options = {
                    formControl: newChoiceFieldObject,
                    isFormControl: false,
                    relationshipType: 'choice'
                };

            controlConfigureDialogFactory.createDialog(options).then(function(result) {
                if (result !== false) {
                    newChoiceFieldObject = result;
                }
            });
              
            };
            $scope.onNewChoiceFieldControlClick = function () {
                //Build option variable
                var options = {
                    formControl: formControlDefaultObject,
                    isFormControl: true,
                    relationshipType: 'choice'
                };

                controlConfigureDialogFactory.createDialog(options).then(function (result) {
                    if (result !== false) {
                        formControlDefaultObject = result;
                    }
                });

            };
            
            $scope.svgSettings = {
                svgs: [{
                    leftRects: {
                        numOfRects: 1,
                        text: 'leftText',
                        rectStyle: 'dotted',
                        textStyle: 'normal'
                        //connectorStyle: 'dotted'
                    },
                    rightRects: {
                        numOfRects: 3,
                        text: 'rightText',
                        rectStyle: 'dotted',
                        textStyle: 'light',
                        hasOffsetDown: true
                    }
                },
                    {
                        leftRects: {
                            numOfRects: 1,
                            text: 'leftText',
                            rectStyle: 'strong',
                            textStyle: 'normal'
                            //connectorStyle: 'normal'
                        },
                        rightRects: {
                            numOfRects: 0,
                            text: 'rightText',
                            rectStyle: 'strong',
                            textStyle: 'normal',
                            hasOffsetDown: true
                        }
                    }
                ]
            };
        });
}());