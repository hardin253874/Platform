// Copyright 2011-2016 Global Software Innovation Pty Ltd
(function () {
    'use strict';

    /**
    * Module implementing an analyzer.    
    * 
    * @module spAnalyzer    
    * @example
        
    Using the spAnalyzer:
    
    &lt;sp-analyzer options="options"&gt;&lt;/sp-analyzer&gt         

    where options is available on the controller with the following properties:
        - isEditMode - {boolean}. True if the analyzer is in edit mode false otherwise.
        
        - analyzerFields - {array}. The array of analyzer fields.        
            - analyzerFields[].name - {string}. The display name of the analyzer field.
            - analyzerFields[].operators - {array}. The list of operators applicable to the field. (if not specified a default set is used)
                - id {string} - The operator id, AnyOf, Equal etc
                - name {string} - The name of the operator
                - argCount {number} - The number of arguments for the operator.
                - type {string} - The type name of the operator argument
            - analyzerFields[].operator - {string}. The id of the selected operator.
            - analyzerFields[].defaultOperator - {string}. The id of the default operator.            
            - analyzerFields[].type - {string}. The analyzer field type.
            - analyzerFields[].value - {object}. The analyzer field value.        

    This control emits the following events:
        - spAnalyzerEventApplyConditions - This event is raised when the analyzer conditions are applied.   
    */
    angular.module('mod.common.ui.spAnalyzer', ['mod.common.ui.spValueEditor', 'mod.common.ui.spTypeOperatorService', 'mod.common.ui.spAnalyzerFieldConfigPopup', 'mod.common.ui.spPopupStackManager', 'app.reportBuilder'])
        .directive('spAnalyzer', function (spAnalyzerFieldConfigPopup, spPopupStackManager, reportBuilderService, $timeout) {
            return {
                restrict: 'E',
                templateUrl: 'analyzer/spAnalyzer.tpl.html',
                replace: true,
                transclude: false,
                scope: {
                    options: '='
                },
                link: function (scope, iElement, attrs ) {

                    var fieldsPanel = $(iElement).find('#analyserFieldsPanel');

                    scope.model = {
                        isEditMode: true,
                        showDropArea: false,
                        isReset: false,
                        analyzerFields: [],
                        onResetClicked: function () {
                            scope.model.isReset = true;
                            resetConditions();
                            onApplyClicked();
                        },
                        onApplyClicked: function () {
                            onApplyClicked();
                        }
                    };

                    // Expose a remove method so that the analyzer field can remove itself

                    function removeAnalyzerField(field) {
                        var index = _.indexOf(scope.model.analyzerFields, field);
                        if (index !== -1) {
                            scope.model.analyzerFields.splice(index, 1);
                            //scope.$apply(function () {
                            reportBuilderService.setActionFromReport('removeAnalyzer', field, null);
                            _.delay(function() {
                                scope.$apply();
                            });
                            
                            //});                                              
                        }
                    }

                    // Expose a configure method so that the analyzer field can be configured
                    function configureAnalyzerField(field) {
                        var analyzerFieldConfigOptions = { field: field };
                        spPopupStackManager.pushPopup(iElement);

                        // Show the dialog
                        spAnalyzerFieldConfigPopup.showModalDialog(analyzerFieldConfigOptions).then(function (result) {
                            spPopupStackManager.popPopup(iElement);

                            if (field.pickerReportId !== result) {
                                field.pickerReportId = result;  // result is either undefined or an id number

                                reportBuilderService.setActionFromReport('updateAnalyzerFieldConfig', field, null);
                                _.delay(function () {
                                    scope.$apply();
                                });
                            }

                        }, function () {
                            spPopupStackManager.popPopup(iElement);
                        });
                    }

                    // Initialize Watchers
                    scope.$watch('options.isEditMode', function () {
                        scope.model.isEditMode = scope.options.isEditMode;
                        showDropArea();
                    });

                    scope.$watch('options.analyzerFields', function () {
                        scope.model.analyzerFields = scope.options.analyzerFields;
                        showDropArea();

                        $timeout(function () {
                            $(fieldsPanel).scrollTop(0);
                        }, 250);
                    });                                       
                    


                    scope.$on('spAnalyzerEventRemoveField', function (event, field) {
                        event.stopPropagation();
                        removeAnalyzerField(field);
                    });

                    scope.$on('spAnalyzerEventConfigureField', function (event, field) {
                        event.stopPropagation();
                        configureAnalyzerField(field);
                    });

                    scope.dropOptions = {
                        onAllowDrop: function (source, target, dragData) {
                            if (!target || scope.model.isEditMode !== true) {
                                return false;
                            }

                            var t = $(target);

                            try {
                                if (dragData.fid || dragData.tag) {
                                    return true;
                                } else {
                                    return false;
                                }
                            } catch (e) {
                                return false;
                            }

                        },
                        onDragOver: function (event, source, target, dragData) {
                            if (!target || scope.model.isEditMode !== true) {
                                return false;
                            }

                            var t = $(target);

                            var jTarget = t.closest('.spAnalyzerFieldContainer');

                            if (jTarget && jTarget.length > 0) {
                                jTarget.css('background-color', '#D9E4EE');
                                if (dragData.tag) {                                   
                                    jTarget.css('border-top-style', 'solid');
                                    jTarget.css('border-top-color', 'blue');
                                } else {                                    
                                    jTarget.css('border-bottom-style', 'solid');
                                    jTarget.css('border-bottom-color', 'blue');
                                }
                            }

                            return true;

                        },
                        onDragLeave: function (event, source, target, dragData) {
                            if (!target || scope.model.isEditMode !== true) {
                                return false;
                            }

                            var t = $(target);

                            var jTarget = t.closest('.spAnalyzerFieldContainer');

                            if (jTarget && jTarget.length > 0) {
                                jTarget.css('background-color', 'transparent');
                                if (dragData.tag) {
                                    
                                    jTarget.css('border-top-style', 'none');
                                    jTarget.css('border-top-color', 'transparent');
                                } else {
                                    
                                    jTarget.css('border-bottom-style', 'none');
                                    jTarget.css('border-bottom-color', 'transparent');
                                }
                            }

                            return true;
                        },
                        onDrop: function (event, source, target, dragData, dropData) {

                            if (!target || scope.model.isEditMode !== true) {
                                return;
                            }

                            if (dragData.tag) {
                                if (dragData.tag.id !== dropData.tag.id) {
                                    scope.$apply(function() {
                                        reportBuilderService.setActionFromReport('reOrderAnalyzerByDragDrop', dragData, dropData);
                                    });
                                }
                            } else {

                                scope.$apply(function() {
                                    reportBuilderService.setActionFromReport('addAnalyzerByDragDrop', dragData, dropData);
                                });
                            }
                            return;
                        }
                    };


                    // Private methods

                    function showDropArea() {
                        if (scope.model.isEditMode === true && (!scope.model.analyzerFields || scope.model.analyzerFields.length === 0)) {
                            scope.model.showDropArea = true;
                        } else {
                            scope.model.showDropArea = false;
                        }
                    }

                    function resetConditions() {
                        _.forEach(scope.model.analyzerFields, function (af) {
                            af.value = null;
                            af.operator = af.defaultOperator ? af.defaultOperator : 'Unspecified';
                        });
                    }                    

                    function onApplyClicked() {                        
                        scope.$emit('spAnalyzerEventApplyConditions', scope.model.analyzerFields, scope.model.isReset);
                        scope.model.isReset = false;
                    }
                }
            };
        })
        .directive('spAnalyzerField', function (spTypeOperatorService) {
            return {
                restrict: 'E',
                templateUrl: 'analyzer/spAnalyzerField.tpl.html',
                replace: true,
                transclude: false,
                scope: {
                    field: '=',
                    isEditMode: '='
                },
                link: function (scope, iElement, attrs) {

                    // Setup the directive's modal
                    scope.model = {
                        selectedOperator: null,
                        currentArgType: null,
                        valueEditorOptions: {}
                    };                    


                    // Callback for when the field operation has changed
                    scope.onFieldOperationChanged = function () {
                        var fieldArgType = scope.getFieldArgType();

                        if (scope.model.currentArgType !== fieldArgType) {
                            scope.model.currentArgType = fieldArgType;
                            scope.field.value = null;
                        }

                        setResourceValueEditorOptions();
                    };


                    // Return the type for the current field argument
                    scope.getFieldArgType = function () {
                        var type = null;

                        if (scope.model.selectedOperator &&
                            scope.model.selectedOperator.argCount > 0) {
                            type = scope.model.selectedOperator.type || scope.field.type;

                            if (spEntity.DataType.isResource(type)) {
                                type = spEntity.DataType.Entity;
                            }
                        }

                        return type;
                    };


                    // Return true to show the value controls, false otherwise
                    scope.showValueControls = function () {
                        if (!scope.model.selectedOperator) {
                            return false;
                        }

                        return (scope.model.selectedOperator.argCount > 0);
                    };


                    // Remove the analyzer field
                    scope.removeAnalyzerField = function () {
                        scope.$emit('spAnalyzerEventRemoveField', scope.field);
                    };

                    // Configure the analyzer field
                    scope.configureAnalyzerField = function () {
                        scope.$emit('spAnalyzerEventConfigureField', scope.field);
                    };


                    scope.dragOptions = {
                        onDragStart: function () {

                        },
                        onDragEnd: function () {

                        }
                    };


                    // Watch for selected operator changes and update the field
                    scope.$watch('model.selectedOperator', function (selectedOperator) {
                        if (selectedOperator) {
                            scope.field.operator = selectedOperator.id;
                        }
                    });


                    // Watch for field changes
                    scope.$watch('field.operator', function () {
                        var field = scope.field;

                        if (!field) {
                            return;
                        }

                        if (!field.operators) {
                            field.operators = spTypeOperatorService.getApplicableOperators(field.type);
                        }

                        if (field.operator) {
                            scope.model.selectedOperator = _.find(field.operators, function (o) {
                                return o.id === field.operator;
                            });
                        } else {
                            scope.model.selectedOperator = _.find(field.operators, function (o) {
                                return o.id === 'Unspecified';
                            });
                        }

                        scope.model.currentArgType = scope.getFieldArgType();

                        setResourceValueEditorOptions();
                    });


                    // Set the value editor options for the field

                    function setResourceValueEditorOptions() {
                        var type = null,
                            pickerType,
                            entityTypeId,
                            pickerReportId,
                            availableEntities,                          
                            filteredEntityIds;

                        if (scope.model.selectedOperator &&
                            scope.model.selectedOperator.argCount > 0) {
                            type = scope.model.selectedOperator.type || scope.field.type;

                            switch (type) {
                            case 'ChoiceRelationship':
                                pickerType = 'MultiCombo';
                                availableEntities = scope.field.availableEntities;
                                break;
                            case 'InlineRelationship':
                            case 'UserInlineRelationship':
                            case 'StructureLevels':
                                pickerType = 'MultiPicker';
                                entityTypeId = scope.field.entityTypeId;
                                pickerReportId = scope.field.pickerReportId;                                
                                filteredEntityIds = scope.field.filteredEntityIds ? scope.field.filteredEntityIds : null;
                                break;
                            }
                        }

                        if (!scope.model.valueEditorOptions) {
                            scope.model.valueEditorOptions = {};
                        }

                        scope.model.valueEditorOptions.entityPickerType = pickerType;
                        scope.model.valueEditorOptions.entities = availableEntities;
                        scope.model.valueEditorOptions.entityTypeId = entityTypeId;
                        scope.model.valueEditorOptions.pickerReportId = pickerReportId;                        
                        scope.model.valueEditorOptions.filteredEntityIds = filteredEntityIds;
                    }
                }
            };
        });
}());