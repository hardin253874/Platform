// Copyright 2011-2016 Global Software Innovation Pty Ltd
(function () {
    'use strict';

    /**
    * Module containing conditional formatting constants.
    * It contains the following constants:    

    * @module spTypeOperatorService
    */
    angular.module('mod.common.ui.spTypeOperatorService', [])        
        .factory('spTypeOperatorService', spTypeOperatorService);

    /* @ngInject */
    function spTypeOperatorService() {
        var exports = {},
            defaultTypeOperators = {
                UserInlineRelationship: [
                    {
                        oper: 'Unspecified'
                    },
                    {
                        oper: 'IsNotNull'
                    },
                    {
                        oper: 'IsNull'
                    },
                    {
                        oper: 'AnyOf',
                        argCount: 1
                    },
                    {
                        oper: 'AnyExcept',
                        argCount: 1
                    },
                    {
                        oper: 'CurrentUser'
                    }
                ],
                InlineRelationship: [
                    {
                        oper: 'Unspecified'
                    },
                    {
                        oper: 'IsNotNull'
                    },
                    {
                        oper: 'IsNull'
                    },
                    {
                        oper: 'AnyOf',
                        argCount: 1
                    },
                    {
                        oper: 'AnyExcept',
                        argCount: 1
                    }
                ],
                RelatedResource: [
                    {
                        oper: 'Unspecified'
                    },
                    {
                        oper: 'IsNotNull'
                    },
                    {
                        oper: 'IsNull'
                    },
                    {
                        oper: 'AnyOf',
                        argCount: 1
                    },
                    {
                        oper: 'AnyExcept',
                        argCount: 1
                    }
                ],
                ChoiceRelationship: [
                    {
                        oper: 'Unspecified'
                    },
                    {
                        oper: 'IsNotNull'
                    },
                    {
                        oper: 'IsNull'
                    },
                    {
                        oper: 'AnyOf',
                        argCount: 1
                    },
                    {
                        oper: 'AnyExcept',
                        argCount: 1
                    }
                ],
                Bool: [
                    {
                        oper: 'Unspecified'
                    },
                    {
                        oper: 'IsTrue'
                    },
                    {
                        oper: 'IsFalse'
                    }
                ],
                Time: [
                    {
                        oper: 'Unspecified'
                    },
                    {
                        oper: 'Equal',
                        argCount: 1
                    },
                    {
                        oper: 'NotEqual',
                        argCount: 1
                    },
                    {
                        oper: 'GreaterThan',
                        argCount: 1
                    },
                    {
                        oper: 'GreaterThanOrEqual',
                        argCount: 1
                    },
                    {
                        oper: 'LessThan',
                        argCount: 1
                    },
                    {
                        oper: 'LessThanOrEqual',
                        argCount: 1
                    },
                    {
                        oper: 'IsNotNull'
                    },
                    {
                        oper: 'IsNull'
                    }
                ],
                Date: [
                    {
                        oper: 'Unspecified'
                    },
                    {
                        oper: 'Equal',
                        argCount: 1
                    },
                    {
                        oper: 'NotEqual',
                        argCount: 1
                    },
                    {
                        oper: 'GreaterThan',
                        argCount: 1
                    },
                    {
                        oper: 'GreaterThanOrEqual',
                        argCount: 1
                    },
                    {
                        oper: 'LessThan',
                        argCount: 1
                    },
                    {
                        oper: 'LessThanOrEqual',
                        argCount: 1
                    },
                    {
                        oper: 'IsNotNull'
                    },
                    {
                        oper: 'IsNull'
                    },
                    {
                        oper: 'Today'
                    },
                    {
                        oper: 'ThisWeek',
                    },
                    {
                        oper: 'ThisMonth'
                    },
                    {
                        oper: 'ThisQuarter'
                    },
                    {
                        oper: 'ThisYear'
                    },
                    {
                        oper: 'CurrentFinancialYear'
                    },
                    {
                        oper: 'LastNDays',
                        type: 'Int32',
                        argCount: 1
                    },
                    {
                        oper: 'LastNDaysTillNow',
                        type: 'Int32',
                        argCount: 1
                    },
                    {
                        oper: 'NextNDays',
                        type: 'Int32',
                        argCount: 1
                    },
                    {
                        oper: 'NextNDaysFromNow',
                        type: 'Int32',
                        argCount: 1
                    },
                    {
                        oper: 'LastNWeeks',
                        type: 'Int32',
                        argCount: 1
                    },
                    {
                        oper: 'NextNWeeks',
                        type: 'Int32',
                        argCount: 1
                    },
                    {
                        oper: 'LastNMonths',
                        type: 'Int32',
                        argCount: 1
                    },
                    {
                        oper: 'NextNMonths',
                        type: 'Int32',
                        argCount: 1
                    },
                    {
                        oper: 'LastNQuarters',
                        type: 'Int32',
                        argCount: 1
                    },
                    {
                        oper: 'NextNQuarters',
                        type: 'Int32',
                        argCount: 1
                    },
                    {
                        oper: 'LastNYears',
                        type: 'Int32',
                        argCount: 1
                    },
                    {
                        oper: 'NextNYears',
                        type: 'Int32',
                        argCount: 1
                    },
                    {
                        oper: 'LastNFinancialYears',
                        type: 'Int32',
                        argCount: 1
                    },
                    {
                        oper: 'NextNFinancialYears',
                        type: 'Int32',
                        argCount: 1
                    }
                ],
                DateTime: [
                    {
                        oper: 'Unspecified'
                    },
                    {
                        oper: 'Equal',
                        argCount: 1
                    },
                    {
                        oper: 'NotEqual',
                        argCount: 1
                    },
                    {
                        oper: 'GreaterThan',
                        argCount: 1
                    },
                    {
                        oper: 'GreaterThanOrEqual',
                        argCount: 1
                    },
                    {
                        oper: 'LessThan',
                        argCount: 1
                    },
                    {
                        oper: 'LessThanOrEqual',
                        argCount: 1
                    },
                    {
                        oper: 'IsNotNull'
                    },
                    {
                        oper: 'IsNull'
                    },
                    {
                        oper: 'DateEquals',
                        type: 'Date',
                        argCount: 1
                    },
                    {
                        oper: 'Today'
                    },
                    {
                        oper: 'ThisWeek'
                    },
                    {
                        oper: 'ThisMonth'
                    },
                    {
                        oper: 'ThisQuarter'
                    },
                    {
                        oper: 'ThisYear'
                    },
                    {
                        oper: 'CurrentFinancialYear'
                    },
                    {
                        oper: 'LastNDays',
                        type: 'Int32',
                        argCount: 1
                    },
                    {
                        oper: 'LastNDaysTillNow',
                        type: 'Int32',
                        argCount: 1
                    },
                    {
                        oper: 'NextNDays',
                        type: 'Int32',
                        argCount: 1
                    },
                    {
                        oper: 'NextNDaysFromNow',
                        type: 'Int32',
                        argCount: 1
                    },
                    {
                        oper: 'LastNWeeks',
                        type: 'Int32',
                        argCount: 1
                    },
                    {
                        oper: 'NextNWeeks',
                        type: 'Int32',
                        argCount: 1
                    },
                    {
                        oper: 'LastNMonths',
                        type: 'Int32',
                        argCount: 1
                    },
                    {
                        oper: 'NextNMonths',
                        type: 'Int32',
                        argCount: 1
                    },
                    {
                        oper: 'LastNQuarters',
                        type: 'Int32',
                        argCount: 1
                    },
                    {
                        oper: 'NextNQuarters',
                        type: 'Int32',
                        argCount: 1
                    },
                    {
                        oper: 'LastNYears',
                        type: 'Int32',
                        argCount: 1
                    },
                    {
                        oper: 'NextNYears',
                        type: 'Int32',
                        argCount: 1
                    },
                    {
                        oper: 'LastNFinancialYears',
                        type: 'Int32',
                        argCount: 1
                    },
                    {
                        oper: 'NextNFinancialYears',
                        type: 'Int32',
                        argCount: 1
                    }
                ],
                Currency: [
                    {
                        oper: 'Unspecified'
                    },
                    {
                        oper: 'Equal',
                        argCount: 1
                    },
                    {
                        oper: 'NotEqual',
                        argCount: 1
                    },
                    {
                        oper: 'GreaterThan',
                        argCount: 1
                    },
                    {
                        oper: 'GreaterThanOrEqual',
                        argCount: 1
                    },
                    {
                        oper: 'LessThan',
                        argCount: 1
                    },
                    {
                        oper: 'LessThanOrEqual',
                        argCount: 1
                    },
                    {
                        oper: 'IsNotNull'
                    },
                    {
                        oper: 'IsNull'
                    }
                ],
                Decimal: [
                    {
                        oper: 'Unspecified'
                    },
                    {
                        oper: 'Equal',
                        argCount: 1
                    },
                    {
                        oper: 'NotEqual',
                        argCount: 1
                    },
                    {
                        oper: 'GreaterThan',
                        argCount: 1
                    },
                    {
                        oper: 'GreaterThanOrEqual',
                        argCount: 1
                    },
                    {
                        oper: 'LessThan',
                        argCount: 1
                    },
                    {
                        oper: 'LessThanOrEqual',
                        argCount: 1
                    },
                    {
                        oper: 'IsNotNull'
                    },
                    {
                        oper: 'IsNull'
                    }
                ],
                Int32: [
                    {
                        oper: 'Unspecified'
                    },
                    {
                        oper: 'Equal',
                        argCount: 1
                    },
                    {
                        oper: 'NotEqual',
                        argCount: 1
                    },
                    {
                        oper: 'GreaterThan',
                        argCount: 1
                    },
                    {
                        oper: 'GreaterThanOrEqual',
                        argCount: 1
                    },
                    {
                        oper: 'LessThan',
                        argCount: 1
                    },
                    {
                        oper: 'LessThanOrEqual',
                        argCount: 1
                    },
                    {
                        oper: 'IsNotNull'
                    },
                    {
                        oper: 'IsNull'
                    }
                ],
                String: [
                    {
                        oper: 'Unspecified'
                    },
                    {
                        oper: 'Equal',
                        argCount: 1
                    },
                    {
                        oper: 'NotEqual',
                        argCount: 1
                    },
                    {
                        oper: 'Contains',
                        argCount: 1
                    },
                    {
                        oper: 'StartsWith',
                        argCount: 1
                    },
                    {
                        oper: 'EndsWith',
                        argCount: 1
                    },                    
                    {
                        oper: 'IsNotNull'
                    },
                    {
                        oper: 'IsNull'
                    }
                ],
                UserString: [
                    {
                        oper: 'Unspecified'
                    },
                    {
                        oper: 'Equal',
                        argCount: 1,
                        type: 'String'
                    },
                    {
                        oper: 'NotEqual',
                        argCount: 1,
                        type: 'String'
                    },
                    {
                        oper: 'Contains',
                        argCount: 1,
                        type: 'String'
                    },
                    {
                        oper: 'StartsWith',
                        argCount: 1,
                        type: 'String'
                    },
                    {
                        oper: 'EndsWith',
                        argCount: 1,
                        type: 'String'
                    },                    
                    {
                        oper: 'IsNotNull'
                    },
                    {
                        oper: 'IsNull'
                    },
                    {
                        oper: 'CurrentUser'
                    }
                ],
                StructureLevels: [
                    {
                        oper: 'Unspecified'
                    },
                    {
                        oper: 'AnyBelowStructureLevel',
                        argCount: 1
                    },
                    {
                        oper: 'AnyAtOrBelowStructureLevel',
                        argCount: 1
                    },
                    {
                        oper: 'AnyAboveStructureLevel',
                        argCount: 1
                    },
                    {
                        oper: 'AnyAtOrAboveStructureLevel',
                        argCount: 1
                    },                    
                    {
                        oper: 'Contains',
                        argCount: 1,
                        type: 'String'
                    },                    
                    {
                        oper: 'IsNotNull'
                    },
                    {
                        oper: 'IsNull'
                    }
                ]
            },
            defaultOperators = {
                Unspecified: '',
                IsNotNull: 'Is defined',
                IsNull: 'Is not defined',
                AnyOf: 'Any of',
                AnyExcept: 'Any except',
                IsTrue: 'Yes',
                IsFalse: 'No',
                Equal: '=',
                NotEqual: '<>',
                GreaterThan: '>',
                GreaterThanOrEqual: '>=',
                LessThan: '<',
                LessThanOrEqual: '<=',
                Today: 'Today',
                ThisWeek: 'This week',
                ThisMonth: 'This month',
                ThisQuarter: 'This quarter',
                ThisYear: 'This year',
                CurrentFinancialYear: 'This FY',
                LastNDays: 'Last N days',
                LastNDaysTillNow: 'Last N days until now',
                NextNDays: 'Next N days',
                NextNDaysFromNow: 'Next N days from now',
                LastNWeeks: 'Last N weeks',
                NextNWeeks: 'Next N weeks',
                LastNMonths: 'Last N months',
                NextNMonths: 'Next N months',
                LastNQuarters: 'Last N quarters',
                NextNQuarters: 'Next N quarters',
                LastNYears: 'Last N years',
                NextNYears: 'Next N years',
                LastNFinancialYears: 'Last N FYs',
                NextNFinancialYears: 'Next N FYs',
                DateEquals: 'Date equals',
                Contains: 'Contains',
                StartsWith: 'Starts with',
                EndsWith: 'Ends with',                
                CurrentUser: 'Current User',
                AnyBelowStructureLevel: 'Any below',
                AnyAtOrBelowStructureLevel: 'Any at or below',
                AnyAboveStructureLevel: 'Any above',                
                AnyAtOrAboveStructureLevel: 'Any at or above'
            };

        var resourceStringOperators = [
            {
                oper: 'Equal',
                argCount: 1,
                type: 'String'
            },
            {
                oper: 'NotEqual',
                argCount: 1,
                type: 'String'
            },
            {
                oper: 'Contains',
                argCount: 1,
                type: 'String'
            },
            {
                oper: 'StartsWith',
                argCount: 1,
                type: 'String'
            },
            {
                oper: 'EndsWith',
                argCount: 1,
                type: 'String'
            }            
        ];

        function addOperator(operators, operator) {
            var existingOperator = _.find(operators, function (r) {
                return r.oper == operator.oper;
            });

            if (!existingOperator) {
                operators.push(operator);
            }
        }

        _.forEach(resourceStringOperators, function (so) {
            addOperator(defaultTypeOperators.UserInlineRelationship, so);
            addOperator(defaultTypeOperators.InlineRelationship, so);
            addOperator(defaultTypeOperators.RelatedResource, so);            
        });
        
        /**
         * Return a list of operators applicable to the specified type             
         * @param type {string} - the promise returning function             
         * @returns {array} - The list of operators applicable to the type
         */
        exports.getApplicableOperators = function (type) {
            if (!type) {
                return [
                    {
                        id: 'Unspecified',
                        name: '[Select]',
                        argCount: 0,
                        type: null
                    }
                ];
            }

            var typeOperatorDefs = defaultTypeOperators[type],
                resultOperators = _.map(typeOperatorDefs, function (opDef) {
                    var operatorName = defaultOperators[opDef.oper];
                    if (opDef.oper === 'Unspecified') {
                        operatorName = '[Select]';
                    }
                    return {
                        id: opDef.oper,
                        oper: opDef.oper,
                        name: operatorName,
                        argCount: opDef.argCount || 0,
                        type: opDef.type || type
                    };
                });

            return resultOperators;
        };

        return exports;
    }
}());