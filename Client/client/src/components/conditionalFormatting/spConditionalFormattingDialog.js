// Copyright 2011-2016 Global Software Innovation Pty Ltd
(function () {
    'use strict';

    /**
    * Module implementing the sort options dialog.    
    * 
    * @module spConditionalFormattingDialog   
    * @example
        
    Using the spConditionalFormattingDialog:
    
    spConditionalFormattingDialog.showModalDialog(options).then(function(result){});
    
    where options is an object with the following properties:   
        - name {string}. The name of the column.
        - type {string}. The data type.        
        - availableEntities - Array of available entities (used when type is a resource type)
        - entityTypeId {number} - The entity type of the available entities (used when type is a resource type)
        - pickerReportId {number} - The id of the report to use when displaying available entities (used when type is a resource type)        
        - operators {array} Array of operators (if not specified a default set is used)        
            - id {string} - The operator id, AnyOf, Equal etc
            - name {string} - The name of the operator
            - argCount {number} - The number of arguments for the operator.
            - type {string} - The type name of the operator argument
        - formats {array}. Array of valid format names e.g. Highlight, Icon, ProgressBar. (if not specified a default set is used))
        - dateTimeFormats {array}. Array of valid date/date time/time formats.
        - condFormatting {object}. The conditional formatting options
            - format {string}. The selected formatting style. Highlight, Icon, ProgressBar
            - displayText {boolean}. True to display the text false otherwise.
            - progressBarRule {object}. Progress bar formatting rule.
                - minimumValue {number|date} - The minimum value
                - maximumValue {number|date} - The maximum value
                - color {object}
                    - a {number} - The alpha channel
                    - r {number} - The red channel
                    - g {number} - The green channel
                    - b {number} - The blue channel
            - highlightRules {array}. Highlight formatting rules.
                - operator {string} - The id of the operator
                - value {number|date|string|array of Entity} - The value to match
                - color
                    - foregroundColor
                        - a {number} - The alpha channel
                        - r {number} - The red channel
                        - g {number} - The green channel
                        - b {number} - The blue channel
                    - backgroundColor
                        - a {number} - The alpha channel
                        - r {number} - The red channel
                        - g {number} - The green channel
                        - b {number} - The blue channel
            - iconRules {array}. Icon formatting rules.
                - operator {string} - The id of the operator
                - value {number|date|string|array of Entity} - The value to match
                - imgId {number} - The id of the image entity to display
        - valueFormatting {object}. The value formatting options.
            - lines {number} - The number of lines to display multi line text
            - prefix {string} - The prefix for numeric types
            - suffix {string} - The suffix for numeric types
            - decimalPlaces {number} - The number of decimal places for numeric types
            - dateTimeFormatName {string} - The name of the date or time format to use for date/time/datetime types.
            - alignment {string}
            - boolDisplayAs {string}
            - structureViewId {number} - The selected structure view id
        - imageFormatting {object}. The image formatting options.
            - imageScaleId {number} - The entity id of the image scaling
            - imageSizeId {number} - The entity id of the image size
            - alignment {string}

    where result is:
        - object with the following fields if ok is clicked
           - condFormatting {object} as above. 
           - valueFormatting {object} as above.
           - imageFormatting {object} as above.
        - false, if cancel is clicked        
    */
    angular.module('mod.common.ui.spConditionalFormattingDialog', [
        'ui.bootstrap',
        'mod.ui.spConditionalFormattingConstants',
        'mod.common.ui.spColorPickers',
        'mod.common.ui.spIconPickers',
        'mod.common.ui.spValueEditor',
        'mod.common.spEntityService',
        'mod.common.ui.spDialogService',
        'mod.common.ui.spTypeOperatorService',
        'mod.common.spTenantSettings',
        'sp.common.filters',
        'mod.featureSwitch',
        'ngGrid'
    ])
        .controller('spConditionalFormattingDialogController', function ($scope, $templateCache, $uibModalInstance, options, condFormattingConstants, namedFgBgColors, spEntityService, spTypeOperatorService, spTenantSettings, $filter, rnFeatureSwitch) {

            if (!options.type) {
                options.type = null;
            }

            //
            // enable default choice field formatting
            $scope.defaultChoiceFieldFormatEnabled = rnFeatureSwitch.isFeatureOn('defaultChoiceFieldFormat');

            // Setup the dialog model
            $scope.model = {                
                name: options.name || '',
                type: options.type,                
                errors: [],                
                imageFormatting: {
                    hasImageFormatting: false,
                    imageTabActive: false,
                    thumbnailSizePicker: {
                        selectedEntityId: 0,
                        selectedEntity: null,
                        showSelectOption: false,
                        entityTypeId: 'console:thumbnailSizeEnum'
                    },
                    thumbnailScalingPicker: {
                        selectedEntityId: 0,
                        selectedEntity: null,
                        entityTypeId: 'core:imageScaleEnum'
                    },
                    alignmentOptions: [],
                    selectedAlignmentOption: null
                },
                valueFormatting: {
                    dateOrTimeLabel: '',
                    hasValueFormatting: false,
                    currencySymbol: '$',
                    isString: false,
                    isResource: false,
                    isFloatingPointNumeric: false,
                    isNumeric: false,
                    isDateOrTime: false,
                    showSampleText: false,
                    isBoolean: false,                    
                    linesModel: {
                        value: 2,
                        minimumValue: 1,
                        maximumValue: 10
                    },
                    decimalPlacesModel: {
                        value: 2,
                        minimumValue: 0,
                        maximumValue: 10                        
                    },
                    prefix: null,
                    suffix: null,
                    sampleText: '',                    
                    dateTimeFormats: [],
                    selectedDateTimeFormat: null,
                    booleanFormats: [],
                    selectedBooleanFormat: null,
                    alignmentOptions: [],
                    selectedAlignmentOption: null,					
                    showStructureView: false,
                    structureViewPicker: {
                        selectedEntityId: 0,
                        selectedEntity: null,
                        showSelectOption: true,
                        entities: []
                    },
					showEntityListFormat: false,
                    selectedEntityListFormatOption: null
                },
                condFormatting: {
                    hasCondFormatting: false,
                    iconWidth: 16,
                    iconHeight: 16,
                    iconSizeId: new spEntity.EntityRef('console:iconThumbnailSize'),
                    availableIconIds: [],
                    availableIconNames: {},
                    condFormatEntities: {},
                    operators: [],
                    selectedFormatType: null,
                    selectedScheme: null,
                    formatTypes: [],
                    displayText: true,
                    disableDefaultFormat: false,
                    useDefaultFormat: true,
                    progressBarRule: {
                        minimumValue: null,
                        maximumValue: null,
                        color: {
                            a: 255,
                            r: 0,
                            g: 0,
                            b: 0
                        }
                    },
                    highlightRules: [],
                    iconRules: [],
                    iconRuleGridOptions: {
                        data: 'model.condFormatting.iconRules',
                        multiSelect: false,
                        enableSorting: false,
                        noTabInterference: true,
                        columnDefs: [
                            {
                                field: 'operator',
                                displayName: 'Operation',
                                sortable: false,
                                groupable: false,
                                width: '20%',
                                cellTemplate: $templateCache.get('conditionalFormatting/spOperationCell.tpl.html')
                            },
                            {
                                field: 'value',
                                displayName: 'Value',
                                sortable: false,
                                groupable: false,
                                width: '47%',
                                cellTemplate: $templateCache.get('conditionalFormatting/spValueCell.tpl.html')
                            },
                            {
                                field: 'icon',
                                displayName: 'Icon',
                                sortable: false,
                                groupable: false,
                                width: '33%',
                                cellTemplate: $templateCache.get('conditionalFormatting/spIconCell.tpl.html')
                            }
                        ]
                    },
                    highlightRuleGridOptions: {
                        data: 'model.condFormatting.highlightRules',
                        multiSelect: false,
                        enableSorting: false,
                        enableColumnResize: true,
                        noTabInterference: true,
                        columnDefs: [
                            {
                                field: 'operator',
                                displayName: 'Operation',
                                sortable: false,
                                groupable: false,
                                width: '20%',
                                cellTemplate: $templateCache.get('conditionalFormatting/spOperationCell.tpl.html')
                            },
                            {
                                field: 'value',
                                displayName: 'Value',
                                sortable: false,
                                groupable: false,
                                width: '44%',
                                cellTemplate: $templateCache.get('conditionalFormatting/spValueCell.tpl.html')
                            },
                            {
                                field: 'color',
                                displayName: 'Colour',
                                sortable: false,
                                groupable: false,
                                width: '36%',
                                cellTemplate: $templateCache.get('conditionalFormatting/spHighlightColorCell.tpl.html')
                            }
                        ]
                    }
                }
            };                        


            // Get the currency symbol for the current tenant
            spTenantSettings.getCurrencySymbol().then(function (symbol) {
                if (symbol) {
                    $scope.model.valueFormatting.currencySymbol = symbol;
                } else {
                    $scope.model.valueFormatting.currencySymbol = '$';
                }
            });


            // Methods                       

            // Add a new rule to the current rule type
            $scope.addRule = function () {
                $scope.clearErrors();

                switch ($scope.model.condFormatting.selectedFormatType.id) {
                case condFormattingConstants.formatTypeEnum.Highlight:
                    addHighlightRule();
                    break;
                case condFormattingConstants.formatTypeEnum.Icon:
                    addIconRule();
                    break;
                }
            };


            // Cancel click handler
            $scope.cancel = function () {
                $uibModalInstance.close(false);
            };


            // Returns true if the add rules button can be shown, false otherwise
            $scope.canShowAddRulesButton = function () {                
                if (!$scope.model.condFormatting.selectedFormatType) {
                    return false;
                }                

                if ($scope.model.condFormatting.selectedFormatType.id === condFormattingConstants.formatTypeEnum.Highlight ||
                    $scope.model.condFormatting.selectedFormatType.id === condFormattingConstants.formatTypeEnum.Icon) {
                    return true;
                }

                return false;
            };

            // Returns true if current field is choice Field
            $scope.isChoiceField = function () {
                return options.type === 'ChoiceRelationship' && $scope.defaultChoiceFieldFormatEnabled;
            };

            // Clear any errors
            $scope.clearErrors = function () {
                $scope.model.errors = [];
            };


            // Return the type of the specified rule operator
            $scope.getRuleArgType = function (row) {
                var type = null;

                if (row.entity.operator &&
                    row.entity.operator.argCount > 0) {
                    type = row.entity.operator.type || $scope.model.type;

                    if (spEntity.DataType.isResource(type)) {
                        type = spEntity.DataType.Entity;
                    }
                }

                return type;
            };


            // Returns true if the specified row represents the else row
            $scope.isElseRule = function (row) {
                var rulesLength,
                    id = $scope.model.condFormatting.selectedFormatType.id;

                if (id === condFormattingConstants.formatTypeEnum.Highlight) {
                    rulesLength = $scope.model.condFormatting.highlightRules.length;
                } else if (id === condFormattingConstants.formatTypeEnum.Icon) {
                    rulesLength = $scope.model.condFormatting.iconRules.length;
                }

                if (row.rowIndex < rulesLength - 1) {
                    return false;
                }

                return true;
            };


            $scope.onValueFormattingOptionsChanged = function () {
                $scope.clearErrors();
                updateValueFormattingSampleText();
            };


            // Callback for when the format type is changed
            $scope.onFormatTypeChanged = function () {
                $scope.clearErrors();
                $scope.model.condFormatting.selectedScheme = $scope.model.condFormatting.selectedFormatType.schemes[0];
                $scope.onSchemeChanged();
            };
               

            // Ok click handler
            $scope.ok = function () {                
                $scope.clearErrors();

                // Validate the settings
                validateSettings();

                if ($scope.model.errors.length === 0) {
                    $uibModalInstance.close(getDialogSettingsAsResult());
                }
            };


            // Callback for when the scheme changes
            $scope.onSchemeChanged = function () {
                $scope.clearErrors();

                if (!$scope.model.condFormatting.selectedFormatType) {
                    return;
                }

                switch ($scope.model.condFormatting.selectedFormatType.id) {
                case condFormattingConstants.formatTypeEnum.Highlight:
                    loadHighlightRulesFromScheme($scope.model.condFormatting.selectedScheme);
                    break;
                case condFormattingConstants.formatTypeEnum.Icon:
                    loadIconRulesFromScheme($scope.model.condFormatting.selectedScheme);
                    break;
                case condFormattingConstants.formatTypeEnum.ProgressBar:
                    loadProgressBarRulesFromScheme($scope.model.condFormatting.selectedScheme);
                    break;
                }
            };

            
            // Removes the specified highlight rule
            $scope.removeHighlightRule = function (row) {
                $scope.clearErrors();
                _.pull($scope.model.condFormatting.highlightRules, row.entity);
            };


            // Removes the specified icon rule
            $scope.removeIconRule = function (row) {
                $scope.clearErrors();
                _.pull($scope.model.condFormatting.iconRules, row.entity);
            };


            $scope.ruleOperationChanged = function (row) {
                var ruleArgType = $scope.getRuleArgType(row);
                if (row.entity.currentArgType !== ruleArgType) {
                    row.entity.currentArgType = ruleArgType;
                    row.entity.value = null;
                }

                setResourceValueEditorOptions(row.entity);
            };


            // Return true to show the value controls, false otherwise
            $scope.showValueControls = function (row) {
                if ($scope.isElseRule(row)) {
                    return false;
                }

                if (!row.entity.operator) {
                    return false;
                }

                return (row.entity.operator.argCount > 0);
            };
                                                                                          
            $scope.disabledControl = function () {
                return $scope.defaultChoiceFieldFormatEnabled && options.type === 'ChoiceRelationship' && $scope.model.condFormatting.useDefaultFormat;
            };
            // Setup watchers

           
            $scope.$watch('model.condFormatting.highlightRules', function () {
                $scope.clearErrors();
            }, true);


            $scope.$watch('model.condFormatting.iconRules', function () {
                $scope.clearErrors();
            }, true);


            $scope.$watch('model.condFormatting.progressBarRule', function () {
                $scope.clearErrors();
            }, true);


            $scope.$watch('model.valueFormatting.decimalPlacesModel.value', function () {
                $scope.onValueFormattingOptionsChanged();
            });


            // Return the style for the sample text
            $scope.getValueFormattingSampleTextStyle = function () {
                var style = {};

                if ($scope.model &&
                    $scope.model.valueFormatting &&
                    $scope.model.valueFormatting.selectedAlignmentOption) {
                    switch ($scope.model.valueFormatting.selectedAlignmentOption.id) {
                    case 'Left':
                        style['text-align'] = 'left';
                        break;
                    case 'Centre':
                        style['text-align'] = 'center';
                        break;
                    case 'Right':
                        style['text-align'] = 'right';
                        break;
                    default:
                        switch (options.type) {
                        case spEntity.DataType.Int32:
                        case spEntity.DataType.Decimal:
                        case spEntity.DataType.Currency:
                            style['text-align'] = 'right';
                            break;
                        default:
                            style['text-align'] = 'left';
                            break;
                        }
                        break;
                    }
                }

                return style;
            };

            // Load icon entities
            loadIconEntities();
            // Load the format types from the options and defaults
            loadFormatTypes();
            // Load the operators from the options and defaults
            loadOperators();                        
            // Load the rules from the options
            loadRules();
            // Load the value formatting options.
            loadValueFormattingOptions();
            // Load structure views
            loadStructureViews();
            //Load the image formatting options
            loadImageFormattingOptions();

            // Private methods

            // Add an error
            function addError(errorMsg) {
                $scope.model.errors.push({ type: 'error', msg: errorMsg });
            }


            // Add a new highlight rule
            function addHighlightRule() {
                var rule,
                    noneFgBgColor = _.find(namedFgBgColors, function (nc) {
                    return nc.id === 'none';
                });

                rule = {
                    operator: $scope.model.condFormatting.operators[0],
                    currentArgType: getCurrentArgType($scope.model.condFormatting.operators[0]),
                    value: null,
                    valueEditorOptions: {                        
                    },
                    color: {
                        foregroundColor: _.clone(noneFgBgColor.foregroundColor),
                        backgroundColor: _.clone(noneFgBgColor.backgroundColor)
                    }
                };

                setResourceValueEditorOptions(rule);

                $scope.model.condFormatting.highlightRules.splice(-1, 0, rule);
            }


            // Add a new icon rule
            function addIconRule() {
                var rule = {
                    operator: $scope.model.condFormatting.operators[0],
                    currentArgType: getCurrentArgType($scope.model.condFormatting.operators[0]),
                    value: null,
                    valueEditorOptions: {
                    },
                    icon: {
                        iconWidth: $scope.model.condFormatting.iconWidth,
                        iconHeight: $scope.model.condFormatting.iconHeight,
                        iconSizeId: $scope.model.condFormatting.iconSizeId,
                        iconIds: $scope.model.condFormatting.availableIconIds,
                        iconNames: $scope.model.condFormatting.availableIconNames,
                        selectedIconId: $scope.model.condFormatting.availableIconIds[1]
                    }
                };

                setResourceValueEditorOptions(rule);

                $scope.model.condFormatting.iconRules.splice(-1, 0, rule);
            }


            // Gets the current dialog settings ready to be returned to the caller.
            function getDialogSettingsAsResult() {
                var result = {                    
                };

                if ($scope.model.condFormatting.hasCondFormatting) {
                    result.condFormatting = getCondFormattingDialogSettingsAsResult();
                }

                if ($scope.model.valueFormatting.hasValueFormatting) {
                    result.valueFormatting = getValueFormattingDialogSettingsAsResult();
                }

                if ($scope.model.imageFormatting.hasImageFormatting) {
                    result.imageFormatting = getImageFormattingDialogSettingsAsResult();
                }                

                return result;
            }


            function getCondFormattingDialogSettingsAsResult() {
                var condFormatting = {
                    format: condFormattingConstants.formatTypeEnum.None,
                    displayText: $scope.model.condFormatting.displayText,
                    disableDefaultFormat: !$scope.model.condFormatting.useDefaultFormat
                };

                var structureViewId = sp.result($scope, 'model.valueFormatting.structureViewPicker.selectedEntity.id');

                if ($scope.model.valueFormatting.showStructureView &&
                    ((structureViewId && options.type !== 'StructureLevels') ||
                     (!structureViewId && options.type === 'StructureLevels'))) {
                    // A bit of a hack here.
                    // A structure view has been applied/cleared. Clear any formatting rules as these
                    // will use operators that no longer apply.
                    return condFormatting;
                }                

                if ($scope.model.condFormatting.selectedFormatType) {
                    condFormatting.format = $scope.model.condFormatting.selectedFormatType.id;
                }

                switch ($scope.model.condFormatting.selectedFormatType.id) {
                case condFormattingConstants.formatTypeEnum.Highlight:
                    condFormatting.highlightRules = getHighlightRulesAsResult();
                    break;
                case condFormattingConstants.formatTypeEnum.Icon:
                    condFormatting.iconRules = getIconRulesAsResult();
                    break;
                case condFormattingConstants.formatTypeEnum.ProgressBar:
                    condFormatting.progressBarRule = $scope.model.condFormatting.progressBarRule;
                    break;
                }

                return condFormatting;
            }


            function getValueFormattingDialogSettingsAsResult() {
                var valueFormatting = {
                };

                valueFormatting.alignment = $scope.model.valueFormatting.selectedAlignmentOption.id;                

                switch (options.type) {
                case spEntity.DataType.String:
                case 'ChoiceRelationship':
                case 'InlineRelationship':
                case 'UserInlineRelationship':
                case 'StructureLevels':
                    valueFormatting.lines = $scope.model.valueFormatting.linesModel.value;
                    if ($scope.model.valueFormatting.showEntityListFormat &&
                        $scope.model.valueFormatting.selectedEntityListFormatOption) {
                        valueFormatting.entityListFormatId = $scope.model.valueFormatting.selectedEntityListFormatOption.id;
                    }
                    break;
                case spEntity.DataType.Bool:
                    //valueFormatting.boolDisplayAs = $scope.model.valueFormatting.selectedBooleanFormat.id;
                    break;
                case spEntity.DataType.Int32:
                    valueFormatting.prefix = $scope.model.valueFormatting.prefix;
                    valueFormatting.suffix = $scope.model.valueFormatting.suffix;
                    break;
                case spEntity.DataType.Decimal:
                case spEntity.DataType.Currency:
                    valueFormatting.decimalPlaces = $scope.model.valueFormatting.decimalPlacesModel.value;
                    valueFormatting.prefix = $scope.model.valueFormatting.prefix;
                    valueFormatting.suffix = $scope.model.valueFormatting.suffix;
                    break;
                case spEntity.DataType.Date:
                case spEntity.DataType.Time:
                case spEntity.DataType.DateTime:
                    valueFormatting.dateTimeFormatName = $scope.model.valueFormatting.selectedDateTimeFormat.formatName;
                    break;                                   
                }

                if ($scope.model.valueFormatting.showStructureView) {
                    valueFormatting.structureViewId = sp.result($scope, 'model.valueFormatting.structureViewPicker.selectedEntity.id') || 0;
                }

                return valueFormatting;
            }


            function getImageFormattingDialogSettingsAsResult() {
                var imageFormatting = {
                    imageScaleId: $scope.model.imageFormatting.thumbnailScalingPicker.selectedEntityId,
                    imageSizeId: $scope.model.imageFormatting.thumbnailSizePicker.selectedEntityId,
                    alignment: $scope.model.imageFormatting.selectedAlignmentOption.id
                };

                return imageFormatting;
            }            

            function setResourceValueEditorOptions(rule) {
                var type = null,
                    pickerType,
                    entityTypeId,
                    pickerReportId,
                    availableEntities;

                if (rule.operator &&
                    rule.operator.argCount > 0) {
                    type = rule.operator.type || $scope.model.type;

                    switch (type) {
                    case 'ChoiceRelationship':
                        pickerType = 'MultiCombo';
                        availableEntities = options.availableEntities;
                        break;
                    case 'InlineRelationship':
                    case 'UserInlineRelationship':
                    case 'StructureLevels':
                        pickerType = 'MultiPicker';
                        entityTypeId = options.entityTypeId;
                        pickerReportId = options.pickerReportId;
                        break;
                    }
                }

                if (!rule.valueEditorOptions) {
                    rule.valueEditorOptions = {};
                }

                rule.valueEditorOptions.entityPickerType = pickerType;
                rule.valueEditorOptions.entities = availableEntities;
                rule.valueEditorOptions.entityTypeId = entityTypeId;
                rule.valueEditorOptions.pickerReportId = pickerReportId;
            }


            // Gets the current highlight settings ready to be returned to the caller.
            function getHighlightRulesAsResult() {
                var rules = $scope.model.condFormatting.highlightRules;

                return _.map(rules, function (r, index) {
                    if (index === (rules.length - 1)) {
                        // Else rule
                        return {
                            operator: 'Unspecified',
                            color: r.color
                        };
                    }

                    return {
                        operator: r.operator.id,
                        type: r.operator.type,
                        value: r.value,
                        color: r.color
                    };
                });
            }


            // Gets the current icon settings ready to be returned to the caller.
            function getIconRulesAsResult() {
                var rules = $scope.model.condFormatting.iconRules;

                return _.map(rules, function (r, index) {
                    var foundAvailableIconId,
                        iconAlias,
                        iconId = 0,
                        cfId = 0;
                    

                    if (r.icon && r.icon.selectedIconId) {
                        iconId = r.icon.selectedIconId.getId();
                        iconAlias = r.icon.selectedIconId.getNsAlias();
                        if (iconId === 0 &&
                            iconAlias) {
                            // attempt to find the id of the icon by alias
                            foundAvailableIconId = _.find($scope.model.condFormatting.availableIconIds, function (aid) {
                                return aid.getNsAlias() === iconAlias;
                            });
                            if (foundAvailableIconId) {
                                iconId = foundAvailableIconId.getId();
                            }
                        }
                        if (iconId > 0) {
                            // Find conditional format id for the selected icon
                            cfId = $scope.model.condFormatting.condFormatEntities[iconId] ? $scope.model.condFormatting.condFormatEntities[iconId].idP : 0;
                        }
                    }

                    if (index === (rules.length - 1)) {
                        // Else rule
                        return {
                            operator: 'Unspecified',
                            imgId: iconId,
                            cfId: cfId
                        };
                    }

                    return {
                        operator: r.operator.id,
                        type: r.operator.type,
                        value: r.value,
                        imgId: iconId,
                        cfId: cfId
                    };
                });
            }           


            // Get the operator with the specified id
            function getOperator(operatorId) {
                return _.find($scope.model.condFormatting.operators, function (op) {
                    return op.id === operatorId;
                });
            }            


            // Load the format types from the options and the defaults
            function loadFormatTypes() {
                var formatTypes;
                
                if (options.formats) {
                    // Load the format types from the options
                    formatTypes = _.filter(condFormattingConstants.formatTypes, function (ft) {
                        return ft.id === condFormattingConstants.formatTypeEnum.None ||
                            _.some(options.formats, function (f) {
                                return angular.lowercase(f) === angular.lowercase(ft.id);
                            });
                    });
                } else {
                    // Load them from the defaults
                    // Load the format types from the options
                    formatTypes = _.filter(condFormattingConstants.formatTypes, function (ft) {
                        return ft.id === condFormattingConstants.formatTypeEnum.None ||
                            _.some(condFormattingConstants.defaultTypeFormats[options.type], function (f) {
                                return angular.lowercase(f) === angular.lowercase(ft.id);
                            });
                    });
                }

                if (formatTypes) {                    
                    if (options.type === spEntity.DataType.Bool) {                        
                        formatTypes = _.map(formatTypes, function (ft) {
                            return {
                                id: ft.id,
                                name: ft.name,
                                schemes: _.filter(ft.schemes, function (s) {
                                    return s.length <= 2;
                                })
                            };
                        });
                    }

                    $scope.model.condFormatting.selectedFormatType = formatTypes[0];
                    $scope.model.condFormatting.selectedScheme = formatTypes[0].schemes[0];
                    $scope.model.condFormatting.formatTypes = formatTypes;                    
                }
            }            


            // Load the highlight rules from the options
            function loadHighlightRules() {
                var srcRules;

                if (!options.condFormatting.highlightRules) {
                    return;
                }

                // Clone the input rules so we don't change them
                srcRules = options.condFormatting.highlightRules;
                $scope.model.condFormatting.highlightRules = _.map(srcRules, function (hr) {
                    var r = {
                        operator: getOperator(hr.operator),
                        value: hr.value,
                        valueEditorOptions: {                            
                        },
                        color: hr.color
                    };

                    r.currentArgType = getCurrentArgType(r.operator);
                    setResourceValueEditorOptions(r);

                    return r;
                });
            }


            // Load the highlight rules from the specified scheme
            function loadHighlightRulesFromScheme(scheme) {
                if (scheme &&
                    scheme.colors) {

                    $scope.model.condFormatting.highlightRules = _.map(scheme.colors, function (c) {
                        var r = {
                            operator: $scope.model.condFormatting.operators[0],
                            value: null,
                            valueEditorOptions: {
                            },
                            color: {
                                foregroundColor: _.cloneDeep(c.foregroundColor),
                                backgroundColor: _.cloneDeep(c.backgroundColor)
                            }
                        };

                        r.currentArgType = getCurrentArgType(r.operator);
                        setResourceValueEditorOptions(r);

                        return r;
                    });
                }
            }


            // Load the available icon entities
            function loadIconEntities() {
                // Get the width and height of the specified icon size.
                spEntityService.getEntity($scope.model.condFormatting.iconSizeId.getNsAliasOrId(), 'k:thumbnailWidth, k:thumbnailHeight').then(function (e) {
                    if (e) {
                        $scope.model.condFormatting.iconWidth = e.getThumbnailWidth();
                        $scope.model.condFormatting.iconHeight = e.getThumbnailHeight();
                    } else {
                        $scope.model.condFormatting.iconWidth = 16;
                        $scope.model.condFormatting.iconHeight = 16;
                    }

                    // Update the properties of any existing rules
                    updateIconRulesSizeInfo();
                });

                // Get the available icons and sort them
                spEntityService.getEntitiesOfType('conditionalFormatIcon', 'formatIconOrder, condFormatImage.{id, alias, name}', { hint: 'cfIcons', batch: true }).then(function (entities) {
                    var iconIds = [],
                        sortedEntities = [],
                        iconNames = {},
                        cfEntities = {};

                    if (entities) {
                        // Sort the icons by the sort order
                        sortedEntities = _.sortBy(entities, function (e) {
                            var iconOrder = e.getFormatIconOrder();
                            if (!iconOrder) {
                                iconOrder = 0;
                            }
                            return iconOrder;
                        });

                        // Return the icon ids
                        iconIds = _.map(sortedEntities, function (e) {
                            if (e.getCondFormatImage()) {
                                var iconId = e.getCondFormatImage().id();
                                iconNames[iconId] = e.getCondFormatImage().name;
                                cfEntities[iconId] = e;
                                return e.getCondFormatImage().eid();
                            } else {
                                return null;
                            }
                        });
                     

                        $scope.model.condFormatting.availableIconIds = _.filter(iconIds, function (id) {
                            return id !== null;
                        });
                        $scope.model.condFormatting.availableIconNames = iconNames;
                        $scope.model.condFormatting.availableIconIds.unshift(new spEntity.EntityRef(0));
                        $scope.model.condFormatting.condFormatEntities = cfEntities;
                    } else {
                        $scope.model.condFormatting.availableIconIds = [];
                    }

                    // Update any icons rules
                    updateIconRulesAvailableIcons();
                });
            }


            function loadStructureViews() {
                var valueFormatting = options.valueFormatting || {};

                if (($scope.model.valueFormatting.isResource || options.isEntityNameColumn) && options.entityTypeId) {
                    $scope.model.valueFormatting.showStructureView = true;
                    // Get the available structure views for this type
                    spEntityService.getEntitiesOfType('core:structureView', 'name', { hint: 'cfStructureViews', batch: true, filter: 'id([Report to definition])=' + options.entityTypeId }).then(function (entities) {
                        if (entities && entities.length) {
                            $scope.model.valueFormatting.structureViewPicker.entities = entities;
                            $scope.model.valueFormatting.structureViewPicker.selectedEntityId = valueFormatting.structureViewId;
                        }
                    });
                }                
            }


            // Load the icon rules from the options
            function loadIconRules() {
                var srcRules;

                if (!options.condFormatting.iconRules) {
                    return;
                }

                // Clone the input rules so we don't change them
                srcRules = options.condFormatting.iconRules;
                $scope.model.condFormatting.iconRules = _.map(srcRules, function (hr) {
                    var r = {
                        operator: getOperator(hr.operator),                        
                        value: hr.value,
                        valueEditorOptions: {
                        },
                        icon: {
                            iconWidth: $scope.model.condFormatting.iconWidth,
                            iconHeight: $scope.model.condFormatting.iconHeight,
                            iconSizeId: $scope.model.condFormatting.iconSizeId,
                            iconIds: $scope.model.condFormatting.availableIconIds,
                            iconNames: $scope.model.condFormatting.availableIconNames,
                            selectedIconId: new spEntity.EntityRef(hr.imgId)
                        }                        
                    };

                    r.currentArgType = getCurrentArgType(r.operator);
                    setResourceValueEditorOptions(r);

                    return r;
                });
            }


            // Load the icon rules from the specified scheme
            function loadIconRulesFromScheme(scheme) {
                if (scheme &&
                    scheme.icons) {

                    $scope.model.condFormatting.iconRules = _.map(scheme.icons, function (c) {
                        var r = {
                            operator: $scope.model.condFormatting.operators[0],                            
                            value: null,
                            valueEditorOptions: {
                            },
                            icon: {
                                iconWidth: $scope.model.condFormatting.iconWidth,
                                iconHeight: $scope.model.condFormatting.iconHeight,
                                iconSizeId: $scope.model.condFormatting.iconSizeId,
                                iconIds: $scope.model.condFormatting.availableIconIds,
                                iconNames: $scope.model.condFormatting.availableIconNames,
                                selectedIconId: c
                            }
                        };

                        r.currentArgType = getCurrentArgType(r.operator);
                        setResourceValueEditorOptions(r);

                        return r;
                    });
                }
            }


            // Load the operators, first from options and then fallback to the defaults.
            function loadOperators() {
                var operators;

                if (options.operators) {
                    // Get the operators from the options
                    operators = options.operators;
                } else {
                    // Get the operators from the defaults
                    operators = spTypeOperatorService.getApplicableOperators(options.type);
                }                                

                $scope.model.condFormatting.operators = operators;
            }            


            // Load the progress bar rules from the options
            function loadProgressBarRules() {
                if (!options.condFormatting.progressBarRule) {
                    return;
                }

                $scope.model.condFormatting.progressBarRule = _.cloneDeep(options.condFormatting.progressBarRule);                
            }


            // Load the progress bar rules from the specified scheme
            function loadProgressBarRulesFromScheme(scheme) {
                if (scheme &&
                    scheme.color) {
                    $scope.model.condFormatting.progressBarRule.color = _.cloneDeep(scheme.color.foregroundColor);
                } else {
                    $scope.model.condFormatting.progressBarRule.color = { a: 255, r: 0, g: 0, b: 0 };
                }
            }


            // Load the rules from the options
            function loadRules() {
                var selectedFormatType;

                if (options.type === 'Image') {
                    $scope.model.condFormatting.hasCondFormatting = false;
                    $scope.model.imageFormatting.hasImageFormatting = true;
                    $scope.model.imageFormatting.imageTabActive = true;                    
                } else {
                    $scope.model.condFormatting.hasCondFormatting = true;
                    $scope.model.imageFormatting.hasImageFormatting = false;                    
                }

                if (!options.condFormatting) {
                    return;
                }
                
                selectedFormatType = _.find($scope.model.condFormatting.formatTypes, function (ft) {
                    return angular.lowercase(ft.id) === angular.lowercase(options.condFormatting.format);
                });

                if (!selectedFormatType) {
                    // Choose none
                    selectedFormatType = _.find($scope.model.condFormatting.formatTypes, function (ft) {
                        return angular.lowercase(ft.id) === 'none';
                    });
                }

                $scope.model.condFormatting.selectedFormatType = selectedFormatType;

                $scope.onFormatTypeChanged();

                if (angular.isDefined(options.condFormatting.displayText)) {
                    $scope.model.condFormatting.displayText = options.condFormatting.displayText;
                }

                if (angular.isDefined(options.condFormatting.disableDefaultFormat)) {
                    $scope.model.condFormatting.disableDefaultFormat = options.condFormatting.disableDefaultFormat;
                    $scope.model.condFormatting.useDefaultFormat = !options.condFormatting.disableDefaultFormat;
                } else {
                    $scope.model.condFormatting.disableDefaultFormat = false;
                    $scope.model.condFormatting.useDefaultFormat = true;
                }

                switch (options.condFormatting.format) {
                case condFormattingConstants.formatTypeEnum.Highlight:
                    loadHighlightRules();
                    break;
                case condFormattingConstants.formatTypeEnum.Icon:
                    loadIconRules();
                    break;
                case condFormattingConstants.formatTypeEnum.ProgressBar:
                    loadProgressBarRules();
                    break;
                }
            }

            // Load the value formatting options
            function loadValueFormattingOptions() {
                var valueFormatting = options.valueFormatting || {};

                $scope.model.valueFormatting.hasValueFormatting = false;                
                $scope.model.valueFormatting.isString = false;
                $scope.model.valueFormatting.isResource = false;
                $scope.model.valueFormatting.isFloatingPointNumeric = false;
                $scope.model.valueFormatting.isNumeric = false;
                $scope.model.valueFormatting.isDateOrTime = false;
                $scope.model.valueFormatting.showSampleText = false;
                $scope.model.valueFormatting.isBoolean = false;
                $scope.model.valueFormatting.showEntityListFormat = false;

                $scope.model.valueFormatting.alignmentOptions = condFormattingConstants.alignmentOptions;                
                $scope.model.valueFormatting.selectedAlignmentOption = _.find($scope.model.valueFormatting.alignmentOptions, function (ao) {
                    return ao.id === valueFormatting.alignment;
                });

                if (!$scope.model.valueFormatting.selectedAlignmentOption) {
                    $scope.model.valueFormatting.selectedAlignmentOption = $scope.model.valueFormatting.alignmentOptions[0];
                }                

                switch (options.type) {
                case spEntity.DataType.String:
                case 'ChoiceRelationship':
                case 'InlineRelationship':
                case 'UserInlineRelationship':
                case 'StructureLevels':
                    if (options.type === spEntity.DataType.String) {
                        $scope.model.valueFormatting.isString = true;
                    } else {
                        $scope.model.valueFormatting.isResource = true;
                    }
                    
                    if (!_.isUndefined(valueFormatting.lines) &&
                        !_.isNull(valueFormatting.lines)) {
                        $scope.model.valueFormatting.linesModel.value = valueFormatting.lines;
                    } else {
                        $scope.model.valueFormatting.linesModel.value = 1;
                    }
                    $scope.model.valueFormatting.hasValueFormatting = true;
                    if ($scope.model.valueFormatting.isResource && options.isAggCol) {
                        $scope.model.valueFormatting.showEntityListFormat = true;
                    }
                    break;
                case spEntity.DataType.Bool:
                    // Set hasValueFormatting to false for now. As bool display as is unsupported by server
                    $scope.model.valueFormatting.hasValueFormatting = true;
                    $scope.model.valueFormatting.isBoolean = true;
                    $scope.model.valueFormatting.booleanFormats = condFormattingConstants.booleanFormats;

                    $scope.model.valueFormatting.selectedBooleanFormat = _.find($scope.model.valueFormatting.booleanFormats, function (bf) {
                        return bf.id === valueFormatting.boolDisplayAs;
                    });

                    if (!$scope.model.valueFormatting.selectedBooleanFormat) {
                        $scope.model.valueFormatting.selectedBooleanFormat = $scope.model.valueFormatting.booleanFormats[0];
                    }
                    break;
                case spEntity.DataType.Int32:
                    $scope.model.valueFormatting.isFloatingPointNumeric = false;
                    loadNumericValueFormattingOptions(valueFormatting);
                    break;
                case spEntity.DataType.Decimal:
                case spEntity.DataType.Currency:
                    $scope.model.valueFormatting.isFloatingPointNumeric = true;
                    loadNumericValueFormattingOptions(valueFormatting);
                    break;
                case spEntity.DataType.Date:
                    $scope.model.valueFormatting.dateOrTimeLabel = 'Date format';
                    loadDateTimeFormattingOptions(valueFormatting);
                    break;
                case spEntity.DataType.Time:
                    $scope.model.valueFormatting.dateOrTimeLabel = 'Time format';
                    loadDateTimeFormattingOptions(valueFormatting);
                    break;
                case spEntity.DataType.DateTime:
                    $scope.model.valueFormatting.dateOrTimeLabel = 'Date time format';
                    loadDateTimeFormattingOptions(valueFormatting);
                    break;                
                }

                if ($scope.model.valueFormatting.showEntityListFormat) {
                    $scope.model.valueFormatting.entityListFormatOptions = condFormattingConstants.entityListFormatOptions;
                    $scope.model.valueFormatting.selectedEntityListFormatOption = _.find($scope.model.valueFormatting.entityListFormatOptions, o => {
                        return o.id === valueFormatting.entityListFormatId;
                    });

                    if (!$scope.model.valueFormatting.selectedEntityListFormatOption) {
                        $scope.model.valueFormatting.selectedEntityListFormatOption = $scope.model.valueFormatting.entityListFormatOptions[0];
                    }
                }

                updateValueFormattingSampleText();
            }


            // Load the image formatting options
            function loadImageFormattingOptions() {
                var sizeId = 'console:iconThumbnailSize',
                    scaleId = 'core:scaleImageProportionally',
                    imageFormatting = options.imageFormatting || {};

                if (options.type !== 'Image') {
                    return;
                }

                if (imageFormatting) {
                    scaleId = imageFormatting.imageScaleId || 'core:scaleImageProportionally';
                    sizeId = imageFormatting.imageSizeId || 'console:iconThumbnailSize';
                }                

                $scope.model.imageFormatting.alignmentOptions = condFormattingConstants.alignmentOptions;
                $scope.model.imageFormatting.selectedAlignmentOption = _.find($scope.model.imageFormatting.alignmentOptions, function (ao) {
                    return ao.id === imageFormatting.alignment;
                });

                if (!$scope.model.imageFormatting.selectedAlignmentOption) {
                    $scope.model.imageFormatting.selectedAlignmentOption = $scope.model.imageFormatting.alignmentOptions[0];
                }

                $scope.model.imageFormatting.thumbnailSizePicker.selectedEntityId = sizeId;
                $scope.model.imageFormatting.thumbnailScalingPicker.selectedEntityId = scaleId;                   
            }


            // Load the date time value formatting options
            function loadDateTimeFormattingOptions(valueFormatting) {
                $scope.model.valueFormatting.hasValueFormatting = true;
                if (_.isEmpty(options.dateTimeFormats) || options.type !== spEntity.DataType.DateTime) {
                    $scope.model.valueFormatting.dateTimeFormats = condFormattingConstants.dateTimeFormats[options.type];
                } else {
                    $scope.model.valueFormatting.dateTimeFormats = options.dateTimeFormats;
                }

                $scope.model.valueFormatting.isDateOrTime = true;
                $scope.model.valueFormatting.showSampleText = true;

                $scope.model.valueFormatting.selectedDateTimeFormat = _.find($scope.model.valueFormatting.dateTimeFormats, function (df) {
                    return df.formatName === valueFormatting.dateTimeFormatName;
                });

                if (!$scope.model.valueFormatting.selectedDateTimeFormat) {
                    $scope.model.valueFormatting.selectedDateTimeFormat = $scope.model.valueFormatting.dateTimeFormats[0];
                }
            }


            // Load the numeric value formatting options
            function loadNumericValueFormattingOptions(valueFormatting) {
                $scope.model.valueFormatting.hasValueFormatting = true;
                $scope.model.valueFormatting.isNumeric = true;
                $scope.model.valueFormatting.showSampleText = true;

                $scope.model.valueFormatting.prefix = valueFormatting.prefix;
                $scope.model.valueFormatting.suffix = valueFormatting.suffix;
                if (!_.isUndefined(valueFormatting.decimalPlaces) &&
                    !_.isNull(valueFormatting.decimalPlaces)) {
                    $scope.model.valueFormatting.decimalPlacesModel.value = valueFormatting.decimalPlaces;
                } else {
                    $scope.model.valueFormatting.decimalPlacesModel.value = 3;
                }
            }


            // Gets the current arg type for the rule
            function getCurrentArgType(operator) {
                var argType;

                if (operator &&
                    operator.argCount > 0) {

                    argType = operator.type || $scope.model.type;

                    if (spEntity.DataType.isResource(argType)) {
                        argType = spEntity.DataType.Entity;
                    }

                    return argType;
                } else {
                    return null;
                }
            }


            // Update the value formatting sampe text
            function updateValueFormattingSampleText() {
                var formatName,
                    sampleValue = 1234.12345678912345,
                    places = $scope.model.valueFormatting.decimalPlacesModel.value;

                if ($scope.model.valueFormatting.selectedDateTimeFormat) {
                    formatName = $scope.model.valueFormatting.selectedDateTimeFormat.formatName;
                }

                if (!_.isNumber(places) || _.isNaN(places)) {
                    places = 0;
                }

                $scope.model.valueFormatting.sampleText = '';

                switch (options.type) {
                case spEntity.DataType.Int32:
                    $scope.model.valueFormatting.sampleText = $filter('spNumber')(sampleValue, $scope.model.valueFormatting.prefix, $scope.model.valueFormatting.suffix);
                    break;
                case spEntity.DataType.Decimal:
                    $scope.model.valueFormatting.sampleText = $filter('spDecimal')(sampleValue, places, $scope.model.valueFormatting.prefix, $scope.model.valueFormatting.suffix);
                    break;
                case spEntity.DataType.Currency:
                    $scope.model.valueFormatting.sampleText = $filter('spCurrency')(sampleValue, $scope.model.valueFormatting.currencySymbol, places, $scope.model.valueFormatting.prefix, $scope.model.valueFormatting.suffix);
                    break;
                case spEntity.DataType.Date:
                    if (formatName) {
                        $scope.model.valueFormatting.sampleText = $filter('spDate')(Date.now(), formatName);
                    }
                    break;
                case spEntity.DataType.Time:
                    if (formatName) {
                        $scope.model.valueFormatting.sampleText = $filter('spTime')(Date.now(), formatName);
                    }
                    break;
                case spEntity.DataType.DateTime:
                    if (formatName) {
                        $scope.model.valueFormatting.sampleText = $filter('spDateTime')(Date.now(), formatName);
                    }
                    break;
                }
            }


            // Update the icon sizes for all the icon rules
            function updateIconRulesSizeInfo() {
                _.each($scope.model.condFormatting.iconRules, function (ir) {
                    ir.icon.iconWidth = $scope.model.condFormatting.iconWidth;
                    ir.icon.iconHeight = $scope.model.condFormatting.iconHeight;
                });
            }


            // Update the available icons for all the icon rules
            function updateIconRulesAvailableIcons() {
                _.each($scope.model.condFormatting.iconRules, function (ir) {
                    ir.icon.iconIds = $scope.model.condFormatting.availableIconIds;
                    ir.icon.iconNames = $scope.model.condFormatting.availableIconNames;
                });
            }


            // Validate the settings
            function validateSettings() {
                if ($scope.model.condFormatting.hasCondFormatting) {
                    switch ($scope.model.condFormatting.selectedFormatType.id) {
                    case condFormattingConstants.formatTypeEnum.Highlight:
                        validateHighlightSettings();
                        break;
                    case condFormattingConstants.formatTypeEnum.Icon:
                        validateIconSettings();
                        break;
                    case condFormattingConstants.formatTypeEnum.ProgressBar:
                        validateProgressBarSettings();
                        break;
                    }
                }

                if ($scope.model.valueFormatting.hasValueFormatting) {
                    validateValueFormattingSettings();
                }

                if ($scope.model.imageFormatting.hasImageFormatting) {
                    validateImageFormattingSettings();
                }
            }


            function validateImageFormattingSettings() {
                if (!$scope.model.imageFormatting.selectedAlignmentOption) {
                    addError('A valid alignment option must be selected.');
                }
            }


            // Validate the highlight settings
            function validateHighlightSettings() {
                var allRulesAreValid,
                    rules = $scope.model.condFormatting.highlightRules;

                if (!rules ||
                    rules.length === 0) {
                    addError('One or more rules are invalid. Ensure that all rules have valid operations and values.');
                    return;
                }

                // Only an else rule is specified
                if (rules.length === 1) {
                    return;
                }

                allRulesAreValid = _.every(rules, function (r, index) {
                    if (index === (rules.length - 1)) {
                        // Skip else rule
                        return true;
                    }

                    // No operator is specified
                    if (!r.operator) {
                        return false;
                    }

                    // No operator is specified
                    if (r.operator.id === 'Unspecified') {
                        return false;
                    }

                    // An operator with no value is specified return true
                    if (r.operator.argCount === 0) {
                        return true;
                    }

                    var argType = r.operator.type ? r.operator.type : r.currentArgType;
                    
                    // The operator requires a value which is not specified
                    if (r.operator.argCount > 0 &&
                        (_.isUndefined(r.value) ||
                         _.isNull(r.value) ||
                         (_.isArray(r.value) && _.isEmpty(r.value)) ||
                         (isNumeric(argType) && _.isNaN(r.value) && r.value.toString() === 'NaN') ||
                         r.value === '')) {
                        return false;
                    }

                    return true;
                });

                if (!allRulesAreValid) {
                    addError('One or more rules are invalid. Ensure that all rules have valid operations and values.');
                }
            }


            // Validate the icon settings
            function validateIconSettings() {
                var allRulesAreValid,
                    rules = $scope.model.condFormatting.iconRules;

                if (!rules ||
                    rules.length === 0) {
                    addError('One or more rules are invalid. Ensure that all rules have valid operations and values.');
                    return;
                }

                // Only an else rule is specified
                if (rules.length === 1) {                    
                    return;
                }

                allRulesAreValid = _.every(rules, function (r, index) {
                    if (index === (rules.length - 1)) {
                        // Skip else rule
                        return true;
                    }

                    // No operator is specified
                    if (!r.operator) {
                        return false;
                    }

                    // No operator is specified
                    if (r.operator.id === 'Unspecified') {
                        return false;
                    }

                    // An operator with no value is specified return true
                    if (r.operator.argCount === 0) {
                        return true;
                    }


                    var argType = r.operator.type ? r.operator.type : r.currentArgType;

                    // The operator requires a value which is not specified
                    if (r.operator.argCount > 0 &&
                        (_.isUndefined(r.value) ||
                         _.isNull(r.value) ||
                         (_.isArray(r.value) && _.isEmpty(r.value)) ||
                         (isNumeric(argType) && _.isNaN(r.value) && r.value.toString() === 'NaN') ||
                         r.value === '')) {
                        return false;
                    }

                    return true;
                });

                if (!allRulesAreValid) {
                    addError('One or more rules are invalid. Ensure that all rules have valid operations and values.');
                }
            }


            // Validate the progress bar settings
            function validateProgressBarSettings() {
                var rule = $scope.model.condFormatting.progressBarRule,
                    minValue, maxValue;

                if (_.isUndefined(rule.minimumValue) ||
                    _.isUndefined(rule.maximumValue) ||
                    _.isNull(rule.minimumValue) ||
                    _.isNull(rule.maximumValue) ||
                    rule.minimumValue === '' ||
                    rule.maximumValue === '') {
                    return;
                }

                switch (options.type) {
                    case spEntity.DataType.Decimal:
                    case spEntity.DataType.Currency:
                    case spEntity.DataType.Int32:
                        minValue = parseFloat(rule.minimumValue);
                        maxValue = parseFloat(rule.maximumValue);

                        if (!_.isNumber(minValue) ||
                            _.isNaN(minValue)) {
                            addError('The minimum value must be a valid number.');
                        }

                        if (!_.isNumber(maxValue) ||
                            _.isNaN(maxValue)) {
                            addError('The maximum value must be a valid number.');
                        }

                        if (_.isNumber(minValue) &&
                            !_.isNaN(minValue) &&
                            _.isNumber(maxValue) &&
                            !_.isNaN(maxValue) &&
                            minValue >= maxValue) {
                            addError('The minimum value must be less than the maximum value.');
                        }
                        break;
                    case spEntity.DataType.Date:
                    case spEntity.DataType.Time:
                    case spEntity.DataType.DateTime:
                        if (rule.minimumValue >= rule.maximumValue) {
                            addError('The minimum value must be less than the maximum value.');
                        }
                        break;
                }                            
            }


            // Validate the value formatting settings
            function validateValueFormattingSettings() {
                var lines, places;               

                switch (options.type) {
                case spEntity.DataType.String:
                case 'ChoiceRelationship':
                case 'InlineRelationship':
                case 'UserInlineRelationship':
                case 'StructureLevels':
                    lines = _.parseInt($scope.model.valueFormatting.linesModel.value, 10);
                    if (!_.isNumber(lines) ||
                        _.isNaN(lines) ||
                        lines < $scope.model.valueFormatting.linesModel.minimumValue ||
                        lines > $scope.model.valueFormatting.linesModel.maximumValue) {
                        addError('The number of lines must be a valid number between ' + $scope.model.valueFormatting.linesModel.minimumValue + ' and ' + $scope.model.valueFormatting.linesModel.maximumValue + '.');
                    }
                    break;
                case spEntity.DataType.Decimal:
                case spEntity.DataType.Currency:
                    places = _.parseInt($scope.model.valueFormatting.decimalPlacesModel.value, 10);
                    if (!_.isNumber(places) ||
                        _.isNaN(places) ||
                        places < $scope.model.valueFormatting.decimalPlacesModel.minimumValue ||
                        places > $scope.model.valueFormatting.decimalPlacesModel.maximumValue) {
                        addError('The number of decimal places must be a valid number between ' + $scope.model.valueFormatting.decimalPlacesModel.minimumValue + ' and ' + $scope.model.valueFormatting.decimalPlacesModel.maximumValue + '.');
                    }
                    break;
                case spEntity.DataType.Date:
                case spEntity.DataType.Time:
                case spEntity.DataType.DateTime:
                    if (!$scope.model.valueFormatting.selectedDateTimeFormat) {
                        addError('A valid date format type must be selected.');
                    }
                    break;
                case spEntity.DataType.Bool:
                    //if (!$scope.model.valueFormatting.selectedBooleanFormat) {
                        //    addError('A valid boolean display as option must be selected.');
                        //}
                    break;
                }
            }
            
            function isNumeric (dataType) {
                switch (dataType) {
                    case 'Decimal':
                    case 'Currency':
                    case 'Number':
                    case 'Int32':
                        return true;
                    default:
                        return false;
                }
            }
        })
        .factory('spConditionalFormattingDialog', function (spDialogService) {
            // setup the dialog
            var exports = {
                showModalDialog: function (options, defaultOverrides) {
                    var dialogDefaults = {
                        windowClass: 'spConditionalFormattingDialog',
                        keyboard: true,
                        backdropClick: false,
                        templateUrl: 'conditionalFormatting/spConditionalFormattingDialog.tpl.html',
                        controller: 'spConditionalFormattingDialogController',
                        resolve: {
                            options: function () {
                                return options;
                            }
                        }
                    };

                    if (defaultOverrides) {
                        angular.extend(dialogDefaults, defaultOverrides);
                    }

                    return spDialogService.showModalDialog(dialogDefaults);
                }
            };            
            
            return exports;
        });
}());