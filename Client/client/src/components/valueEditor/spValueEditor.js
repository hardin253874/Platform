// Copyright 2011-2016 Global Software Innovation Pty Ltd
(function () {
    'use strict';

    /**
    * Module implementing a value editor.    
    * The value editor is an adapter allowing the edit form render controls to be used in contexts outside the edit form.
    * 
    * @module spValueEditor    
    * @example
        
    Using the spValueEditor:
    
    &lt;sp-value-editor type="" options="options" value=""&gt;&lt;/sp-value-editor&gt            

    where:
        - type {string} - one of the spEntity.DataType values
        - value {object|array} - the value for primitive types and is an array of Entity objects for entity types
        - options is an object with the following fields:                        
            - entityPickerType {string} - The entity picker control. Valid when type is Entity. One of Checkbox, Combo, MultiCombo    
            - entities {array of Entity} - the array of Entity objects to pick from.
            - entityTypeId - {number|string} - the type id of the entities to pick from. A service call will be made to get all instances of this type. If the entityTypeId that is specified is an enumType the entities are sorted by the enumOrder, otherwise they are sorted by name.
    */
    angular.module('mod.common.ui.spValueEditor', ['mod.app.editForm.designerDirectives.spCustomValidationMessage', 'mod.common.spMobile'])
        .directive('spValueEditor', function ($compile, spMobileContext) {
            return {
                restrict: 'E',
                templateUrl: 'valueEditor/spValueEditor.tpl.html',
                replace: true,
                transclude: false,
                scope: {
                    type: '=',
                    options: '=',
                    value: '='
                },                
                link: function ($scope, $element) {

                    var isTablet = spMobileContext.isTablet;
                    var isMobile = spMobileContext.isMobile;

                    var valueEditorContentElement = $element.find('#valueEditorContent'),
                        initialisedType,
                        initialisedPickerType;


                    $scope.valueEditorModel = {
                        value: null,
                        pickerOptions: {
                            selectedEntities: [],
                            selectedEntityId: 0,
                            selectedEntityIds: [],
                            entities: [],
                            entityTypeId: 0
                        }
                    };


                    // Initialises the value editor control

                    function initialiseControl(type, pickerType) {
                        var newElement,
                            html;

                        if (initialisedType === type &&
                            initialisedPickerType === pickerType) {
                            return;
                        }

                        html = getDirectiveHtml(type, pickerType);

                        initialisedType = type;
                        initialisedPickerType = pickerType;

                        $scope.valueEditorModel = {
                            value: null,
                            pickerOptions: {
                                selectedEntities: [],
                                selectedEntityId: 0,
                                selectedEntityIds: [],
                                entities: [],
                                entityTypeId: 0
                            }
                        };
                        
                        updateValueEditorModelValue($scope.value);

                        if ($scope.options) {
                            if ($scope.options.entities) {
                                $scope.valueEditorModel.pickerOptions.entities = $scope.options.entities;
                            }

                            if ($scope.options.pickerReportId) {
                                $scope.valueEditorModel.pickerOptions.pickerReportId = $scope.options.pickerReportId;
                            }

                            if ($scope.options.entityTypeId) {
                                $scope.valueEditorModel.pickerOptions.entityTypeId = $scope.options.entityTypeId;
                            }

                            if ($scope.options.relationDetail) {
                                $scope.valueEditorModel.pickerOptions.relationDetail = $scope.options.relationDetail;
                            }

                            if ($scope.options.relationshipFilters) {
                                $scope.valueEditorModel.pickerOptions.relationshipFilters = $scope.options.relationshipFilters;
                            }

                            if ($scope.options.filteredEntityIds) {
                                $scope.valueEditorModel.pickerOptions.filteredEntityIds = $scope.options.filteredEntityIds;
                            }
                        }

                        if (type === spEntity.DataType.Entity) {
                            switch ($scope.options.entityPickerType) {
                                case 'MultiPicker':
                                    $scope.valueEditorModel.pickerOptions.multiSelect = true;
                                    break;
                            }
                        }

                        // Clear the current contents
                        valueEditorContentElement.empty();

                        // Add the new contents
                        if (html) {                                                    
                            newElement = $compile(html)($scope);                            
                            valueEditorContentElement.append(newElement);                            
                        }                                              
                    }


                    // Setup watchers for the input options
                    $scope.$watch('options.entities', function (entities) {
                        if (entities) {
                            $scope.valueEditorModel.pickerOptions.entities = entities;
                        }
                    });


                    $scope.$watch('options.pickerReportId', function (pickerReportId) {
                        if (pickerReportId) {
                            $scope.valueEditorModel.pickerOptions.pickerReportId = pickerReportId;
                        }
                    });

                    $scope.$watch('options.relationDetail', function (relationDetail) {
                        if (relationDetail) {
                            $scope.valueEditorModel.pickerOptions.relationDetail = relationDetail;
                        }
                    });

                    $scope.$watch('options.relationshipFilters', function (relationshipFilters) {
                        if (relationshipFilters) {
                            $scope.valueEditorModel.pickerOptions.relationshipFilters = relationshipFilters;
                        }
                    });

                    $scope.$watch('options.filteredEntityIds', function (filteredEntityIds) {
                        if (filteredEntityIds) {
                            $scope.valueEditorModel.pickerOptions.filteredEntityIds = filteredEntityIds;
                        }
                    });

                    $scope.$watch('options.entityTypeId', function (entityTypeId) {
                        if (entityTypeId) {
                            $scope.valueEditorModel.pickerOptions.entityTypeId = entityTypeId;
                        }
                    });


                    $scope.$watch('options.entityPickerType', function (pickerType) {
                        if (pickerType) {
                            initialiseControl($scope.type, pickerType);
                        }
                    });


                    $scope.$watch('type', function (type) {
                        var pickerType;
                        
                        if ($scope.options) {
                            pickerType = $scope.options.entityPickerType;
                        }

                        initialiseControl(type, pickerType);
                    });


                    function updateValueEditorModelValue(value) {
                        if (!$scope.type) {
                            return;
                        }                        

                        if ($scope.type !== spEntity.DataType.Entity) {
                            if (!areValuesEqual($scope.valueEditorModel.value, value)) {
                                $scope.valueEditorModel.value = value;
                            }
                        } else if ($scope.options) {
                            switch ($scope.options.entityPickerType) {
                                case 'Combo':
                                    $scope.valueEditorModel.pickerOptions.selectedEntityId = value ? value.aliasOrId() : 0;
                                    break;
                                case 'Checkbox':
                                case 'MultiCombo':
                                    $scope.valueEditorModel.pickerOptions.selectedEntities = value || [];
                                    $scope.valueEditorModel.pickerOptions.selectedEntityIds = _.map(value, function (v) {
                                        return v.aliasOrId();
                                    });
                                    break;
                                case 'MultiPicker':
                                    $scope.valueEditorModel.pickerOptions.selectedEntities = value || [];
                                    break;
                                default:
                                    break;
                            }
                        }
                    }


                    // Watch for value changes
                    $scope.$watch('value', function (value) {
                        var newValue = value;

                        // Why does NaN behave stupidly !!
                        if (_.isNaN(newValue)) {
                            newValue = null;
                        }

                        updateValueEditorModelValue(newValue);
                    }, true);


                    function areValuesEqual(newValue, oldValue) {
                        if ($scope.type === spEntity.DataType.Time ||
                            $scope.type === spEntity.DataType.Date ||
                            $scope.type === spEntity.DataType.DateTime) {
                            return _.isEqual(newValue, oldValue);
                        } else {
                            return newValue === oldValue;
                        }
                    }


                    $scope.$watch('valueEditorModel.value', function (value) {
                        var newValue = value;

                        if (!$scope.type) {
                            return;
                        }


                        if ($scope.type === spEntity.DataType.Entity) {
                            return;
                        }                       

                        // Why does NaN behave stupidly !!
                        if (_.isNaN(newValue)) {
                            newValue = null;
                        }

                        if (!areValuesEqual($scope.value, newValue)) {
                            $scope.value = newValue;
                        }
                    }, true);                    


                    // Watch for selected entities changes
                    $scope.$watch('valueEditorModel.pickerOptions.selectedEntities', function (selectedEntities) {
                        var newValues,
                            oldValues;

                        if ($scope.type !== spEntity.DataType.Entity) {
                            // Ignore field types
                            return;
                        }

                        if ($scope.options.entityPickerType !== 'Checkbox' &&
                            $scope.options.entityPickerType !== 'MultiCombo' &&
                            $scope.options.entityPickerType !== 'MultiPicker') {
                            return;
                        }

                        newValues = _.map(selectedEntities, function (re) {
                            return re.aliasOrId();
                        });
                        newValues.sort();

                        oldValues = _.map($scope.value, function (re) {
                            return re.aliasOrId();
                        });
                        oldValues.sort();

                        if (_.isEqual(newValues, oldValues)) {
                            return;
                        }

                        $scope.value = selectedEntities;
                    });


                    // Watch for selected entity changes
                    $scope.$watch('valueEditorModel.pickerOptions.selectedEntity', function (selectedEntity) {
                        var newValue,
                            oldValue;

                        if ($scope.type !== spEntity.DataType.Entity) {
                            // Ignore field types
                            return;
                        }

                        if ($scope.options.entityPickerType !== 'Combo') {
                            return;
                        }

                        if (selectedEntity) {
                            newValue = selectedEntity.aliasOrId();
                        }

                        if ($scope.value) {
                            oldValue = $scope.value.aliasOrId();
                        }

                        if (_.isEqual(newValue, oldValue)) {
                            return;
                        }

                        $scope.value = selectedEntity;
                    });



                    function getDirectiveHtml(type, pickerType) {                        
                        switch (type) {
                            case spEntity.DataType.String:
                                return '<input ng-model="valueEditorModel.value" ></input>';

                            case spEntity.DataType.Int32:
                                return '<sp-number-control model="valueEditorModel"></sp-number-control>';

                            case spEntity.DataType.Decimal:
                                return '<sp-decimal-control model="valueEditorModel"></sp-decimal-control>';

                            case spEntity.DataType.Currency:
                                return '<sp-currency-control model="valueEditorModel"></sp-currency-control>';

                            case spEntity.DataType.Date:
                                return isMobile || isTablet ? 
                                    '<sp-date-mobile-control model="valueEditorModel"></sp-date-mobile-control>':
                                    '<sp-date-control model="valueEditorModel"></sp-date-control>';

                            case spEntity.DataType.Time:
                                return '<sp-time-control model="valueEditorModel"></sp-time-control>';

                            case spEntity.DataType.DateTime:
                                return '<sp-date-and-time-control model="valueEditorModel"></sp-date-and-time-control>';

                            case spEntity.DataType.Bool:
                                return '<sp-checkbox-control model="valueEditorModel"></sp-checkbox-control>';                            

                            case spEntity.DataType.Entity:
                                switch (pickerType) {
                                    case 'Checkbox':
                                        return '<span ng-include="\'valueEditor/entityCheckBoxPicker.tpl.html\'"></span>';
                                    case 'Combo':
                                        return '<span ng-include="\'valueEditor/entityComboPicker.tpl.html\'"></span>';                                        
                                    case 'MultiCombo':
                                        return '<span ng-include="\'valueEditor/entityMultiComboPicker.tpl.html\'"></span>';                                        
                                    case 'MultiPicker':
                                        return '<span ng-include="\'valueEditor/entityMultiPicker.tpl.html\'"></span>';                                        
                                    default:                                        
                                        return null;
                                }
                        }
                                 
                        return null;
                    }
                }
            };
        });
}());