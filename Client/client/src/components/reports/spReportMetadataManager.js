// Copyright 2011-2016 Global Software Innovation Pty Ltd
(function () {
    'use strict';

    angular.module('mod.ui.spReportMetadataManager', ['mod.ui.spConditionalFormattingConstants', 'mod.common.ui.spTypeOperatorService'])
        .provider('spReportMetadataManager', function () {
            this.$get = function (condFormattingConstants, spTypeOperatorService) {
                return function spReportMetadataManager(metadata) {

                    /**
                    * Module implementing the report model manager service.                        
                    * @module spReportMetadataManager                    
                    */
                    var exports = {
                        isPrimitiveType: isPrimitiveType,
                        convertValueToString: convertValueToString
                    };


                    function convertResourcesToJSON(type, resources) {
                        var result = {};

                        if (_.isNull(resources) ||
                            _.isUndefined(resources)) {
                            return null;
                        }

                        switch (type) {
                            case 'ChoiceRelationship':
                            case 'InlineRelationship':
                            case 'UserInlineRelationship':
                            case 'StructureLevels':
                                _.forEach(resources, function (r) {
                                    result[r.id()] = r.getName();
                                });
                                break;
                        }

                        return result;
                    }


                    function convertValueToString(type, value) {
                        var result, utcDate;

                        if (_.isNull(value) ||
                            _.isUndefined(value) ||
                            value === '') {
                            return '';
                        }

                        switch (type) {
                            case spEntity.DataType.Decimal:
                            case spEntity.DataType.Currency:
                            case spEntity.DataType.Int32:
                                if (_.isFinite(value)) {
                                    result = value.toString(10);
                                } else {
                                    result = value;
                                }
                                break;
                            case spEntity.DataType.Date:
                                if (_.isDate(value)) {
                                    result = Globalize.format(value, 'yyyy\'-\'MM\'-\'dd');
                                } else {
                                    result = value;
                                }
                                break;
                            case spEntity.DataType.Time:
                            case spEntity.DataType.DateTime:
                                if (_.isDate(value)) {
                                    result = value.toISOString();
                                } else {
                                    result = value;
                                }
                                break;
                            case spEntity.DataType.Bool:
                                if (_.isBoolean(value)) {
                                    result = value ? 'True' : 'False';                                    
                                } else {
                                    result = value;
                                }
                                break;
                            default:
                                result = value;
                        }

                        return result;
                    }


                    function isPrimitiveType(type) {
                        var result = false;

                        switch (type) {
                            case spEntity.DataType.Decimal:
                            case spEntity.DataType.Currency:
                            case spEntity.DataType.Int32:
                            case spEntity.DataType.Date:
                            case spEntity.DataType.Time:
                            case spEntity.DataType.DateTime:
                            case spEntity.DataType.Bool:
                            case spEntity.DataType.String:
                                result = true;
                                break;
                        }

                        return result;
                    }


                    /**
                    * Updates the sorting info       
                    * @param {object} sortInfo The sort info                    
                    */
                    exports.updateSortInfoMetadata = function (sortInfo) {
                        // Get the updated sort data to post back
                        var sort = _.map(sortInfo, function (si) {
                            return {
                                colid: si.columnId,
                                order: si.sortDirection === 'asc' ? 'Ascending' : 'Descending'
                            };
                        });

                        if (sort &&
                            sort.length > 0) {
                            metadata.sort = sort;
                        } else {
                            if (metadata.sort) {
                                delete metadata.sort;
                            }
                        }
                    };


                    function getAnalyerFieldAsCondition(fieldId, analyzerField) {
                        var operators,
                            operInfo,
                            operatorsType,
                            fieldArgType,
                            cond = {
                                expid: fieldId,
                                type: analyzerField.type,
                                oper: 'Unspecified'
                            };

                        operatorsType = analyzerField.operatorsType || analyzerField.type;
                        operators = spTypeOperatorService.getApplicableOperators(operatorsType);
                        if (operators) {
                            operInfo = _.find(operators, function (o) {
                                return o.oper === analyzerField.operator;
                            });
                        }

                        if (operInfo) {
                            cond.oper = analyzerField.operator;

                            if (operInfo.argCount > 0) {
                                if (!spUtils.isNullOrUndefined(analyzerField.value)) {
                                    // A value is specified
                                    fieldArgType = operInfo.type || analyzerField.type;

                                    if (isPrimitiveType(fieldArgType)) {
                                        cond.value = convertValueToString(fieldArgType, analyzerField.value);
                                    } else if (spEntity.DataType.isResource(fieldArgType)) {
                                        cond.values = convertResourcesToJSON(fieldArgType, analyzerField.value);
                                    }
                                } else {
                                    // A value is not specified. Set the cond to null
                                        cond = null;
                                }
                            }
                        }

                        return cond;
                    }

                    exports.getAnalyzerFieldsAsConds = function (analyzerFields) {
                        var conditions = _.map(analyzerFields, function (af) {
                            return getAnalyerFieldAsCondition(af.tag.id, af);
                        });

                        return _.filter(conditions, function (c) {
                            return c;
                        });
                    };

                    function getAnalyerFieldAsConditionEntity(fieldId, analyzerField) {
                        var operators,
                            operInfo,
                            operatorsType,
                            fieldArgType,
                            cond = {
                                expid: fieldId,
                                type: analyzerField.type,
                                oper: 'Unspecified'

                            };

                        operatorsType = analyzerField.operatorsType || analyzerField.type;
                        operators = spTypeOperatorService.getApplicableOperators(operatorsType);
                        if (operators) {
                            operInfo = _.find(operators, function (o) {
                                return o.oper === analyzerField.operator;
                            });
                        }

                        if (operInfo) {
                            cond.oper = analyzerField.operator;

                            if (operInfo.argCount > 0) {
                                if (!spUtils.isNullOrUndefined(analyzerField.value)) {
                                    // A value is specified
                                    fieldArgType = operInfo.type || analyzerField.type;
                                    cond.argtype = fieldArgType;
                                    if (isPrimitiveType(fieldArgType)) {
                                        cond.value = analyzerField.value;
                                    } else if (spEntity.DataType.isResource(fieldArgType)) {
                                        cond.values = convertResourcesToJSON(fieldArgType, analyzerField.value);
                                    }
                                } else {
                                    // A value is not specified. the cond type should only set the operator to Unspecified not the condition object to null, 
                                    // otherwise, if user set a analyzer value apply first then reset, the analzyer update function will skip the condtion value change
                                    cond.oper = 'Unspecified';
                                }
                            }
                        }

                        return cond;
                    }

                    exports.getAnalyzerFieldsAsCondEntitys = function (analyzerFields) {
                        var conditions = _.map(analyzerFields, function (af) {
                            return getAnalyerFieldAsConditionEntity(af.tag.id, af);
                        });

                        return _.filter(conditions, function (c) {
                            return c;
                        });
                    };


                    function updateValueFormattingAlignmentOption(valRule, alignment) {
                        if (!alignment) {
                            return;
                        }

                        switch (alignment) {
                            case 'Left':
                                break;
                            case 'Right':
                                break;
                            case 'Center':
                                break;
                            default:
                                break;
                        }
                    }


                    return exports;
                };
            };
        });
}());