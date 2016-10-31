// Copyright 2011-2016 Global Software Innovation Pty Ltd
/*global angular */

(function () {
    'use strict';
    
    /**
    * Module containing common filters.
    * It contains the following filters:
    * <ul>
    *   <li>reverse - an AngularJS filter to reverse the given array</li>
    *   <li>even - return the elements with even indexes, starting with the zeroth</li>
    *   <li>odd - return the elements with odd indexes</li>
    *   <li>truncate - truncate the specified string</li>
    *   <li>spCurrency - formats a value as a currency</li>
    *   <li>spBoolean - formats a boolean as Yes or No</li>
    * </ul>    
    * 
    * @module filters    
    */
    angular.module('sp.common.filters', []);

    /**
     * @ngdoc filter
     * @name sp_common.filter:reverse
     * @description An AngularJS filter to reverse the given array.
     */
    angular.module('sp.common.filters').filter('reverse', function () {
        return function (arr) {
            return arr ? arr.slice(0).reverse() : [];
        };
    });

    /**
     * @ngdoc filter
     * @name sp_common.filter:even
     * @description return the elements with even indexes, starting with the zeroth.
     */
    angular.module('sp.common.filters').filter('even', function () {
        return function (arr) {
            return arr ? arr.filter(function (item, index) {
                return (index % 2) === 0;
            }) : [];
        };
    });

    /**
     * @ngdoc filter
     * @name sp_common.filter:odd
     * @description return the elements with odd indexes.
     */
    angular.module('sp.common.filters').filter('odd', function () {
        return function (arr) {
            return arr ? arr.filter(function (item, index) {
                return (index % 2) !== 0;
            }) : [];
        };
    });
    /**
    * @ngdoc filter
    * @name sp_common.filter:truncate
    * @Param text
    * @Param length, default is 10
    * @Param end, default is "..."
    * @return string
    */
    angular.module('sp.common.filters').filter('truncate', function () {
        return function (text, length, end) {
            if (text) {
                if (isNaN(length))
                    length = 10;

                if (end === undefined)
                    end = "...";

                if (text.length <= length || text.length - end.length <= length) {
                    return text;
                } else {
                    return String(text).substring(0, length - end.length) + end;
                }
            }

        };
    });


    /**
    * @ngdoc filter
    * @name sp_common.filter:toTitleCase
    * @Param text
    * @return string
    */
    angular.module('sp.common.filters').filter('spTitleCase', function () {
        return function (text) {
            if (text) {
                return spUtils.toTitleCase(text);
            }

        };
    });


    // Format a value as currency
    angular.module('sp.common.filters').filter('spCurrency', function () {
        return function (value, symbol, places, prefix, suffix) {
            if (_.isNull(value) || _.isUndefined(value) || value === '') {
                return '';
            }

            var minusSymbol = '';

            if (!symbol) {
                symbol = '$';
            }

            if (!_.isNumber(value)) {
                value = Number(value);                
            }

            //if value is less than 0, change to positive value and set the minus symbol
            if (value < 0) {
                value = Math.abs(value);
                minusSymbol = '-';
            }

            if (!prefix) {
                prefix = '';
            }

            if (!suffix) {
                suffix = '';
            }                        

            return prefix + symbol + minusSymbol + Globalize.format(value, 'n' + (angular.isDefined(places) ? places : '3')) + suffix;
        };
    });

    // Format a value as a decimal
    angular.module('sp.common.filters').filter('spDecimal', function () {
        return function (value, places, prefix, suffix) {
            if (_.isNull(value) || _.isUndefined(value) || value === '') {
                return '';
            }

            if (!_.isNumber(value)) {
                value = Number(value);
            }

            if (!prefix) {
                prefix = '';
            }

            if (!suffix) {
                suffix = '';
            }            

            return prefix + Globalize.format(value, 'n' + (angular.isDefined(places) ? places : '3')) + suffix;
        };
    });

    // Format a value as a number
    angular.module('sp.common.filters').filter('spNumber', function () {
        return function (value, prefix, suffix, autoNumberPattern) {
            if (_.isNull(value) || _.isUndefined(value) || value === '') {
                return '';
            }

            if (!_.isNumber(value)) {
                value = Number(value);
            }

            if (!prefix) {
                prefix = '';
            }

            if (!suffix) {
                suffix = '';
            }

            if (autoNumberPattern) {
                return prefix + jQuery.formatNumber(value, { format: autoNumberPattern, locale: "us" }) + suffix;
            } else {
                return prefix + Globalize.format(value, 'n0') + suffix;
            }                        
        };
    });

    // Format a boolean value as Yes or No or True False
    angular.module('sp.common.filters').filter('spBoolean', function () {
        return function (value, format) {
            var boolValue,
                valIndex = 0,
                trueVals = ['Yes', 'True'],
                falseVals = ['No', 'False'];

            if (_.isUndefined(value) || _.isNull(value) || value === '') {
                boolValue = null;
            } else if (_.isString(value)) {
                boolValue = (angular.lowercase(value) === 'true');                    
            } else if (_.isBoolean(value)) {
                boolValue = value;
            }

            if (format === 'TrueFalse') {
                valIndex = 1;              
            }
            if (boolValue === null)
                return '';
            else
                return boolValue  ?   trueVals[valIndex] : falseVals[valIndex];
        };
    });

    // Format a time value
    angular.module('sp.common.filters').filter('spTime', function () {
        return function (value, format) {
            var formatString, isUtc;

            if (_.isNull(value) || _.isUndefined(value) || value === '') {
                return '';
            }

            if (!_.isDate(value)) {
                isUtc = _.includes(value, 'Z');
                // Globalize.format wants a Date object
                value = new Date(value);
                if (isUtc) {
                    value = new Date(value.getUTCFullYear(), value.getUTCMonth(), value.getUTCDay(), value.getUTCHours(), value.getUTCMinutes(), value.getUTCSeconds(), value.getUTCMilliseconds());
                }
            }

            switch (format) {
                case 'timeHour':
                    formatString = 'h:00 tt';
                    break;

                case 'time12Hour':
                    formatString = 'h:mm tt';
                    break;

                case 'time24Hour':
                    formatString = 'HH:mm';
                    break;

                default:
                    formatString = 'h:mm tt';
                    break;
            }

            return Globalize.format(value, formatString);
        };
    });

    function formatDateAsQuarterFormat(value) {
        var month = value.getMonth();
        var quarter = Math.floor((month / 3)) + 1;

        return 'Q' + quarter;
    }

    function formatDateAsWeekdayFormat(value) {
        var days = ['Sunday', 'Monday', 'Tuesday', 'Wednesday', 'Thursday', 'Friday', 'Saturday'];
        var day = value.getDay();
        var res = days[day];
        return res;
    }

    // Format a date value
    angular.module('sp.common.filters').filter('spDate', function () {
        return function (value, format) {
            var formatString;

            if (_.isNull(value) || _.isUndefined(value) || value === '') {
                return '';
            }

            if (!_.isDate(value)) {
                // Globalize.format wants a Date object
                value = new Date(value);
            }

            if (format === 'dateQuarter') {
                return formatDateAsQuarterFormat(value);
            } else if (format === 'dateQuarterYear') {
                return formatDateAsQuarterFormat(value) + ', ' + Globalize.format(value, 'yyyy');
            } else if (format === 'dateWeekday') {
                return formatDateAsWeekdayFormat(value);
            } else {
                switch (format) {
                case 'dateShort':
                    formatString = 'd';
                    break;

                case 'dateDayMonth':
                    formatString = 'M';
                    break;

                case 'dateLong':
                    formatString = 'D';
                    break;

                case 'dateMonth':
                    formatString = 'MMM';
                    break;

                case 'dateMonthYear':
                    formatString = 'Y';
                    break;

                case 'dateYear':
                    formatString = 'yyyy';
                    break;

                default:
                    formatString = 'd';
                    break;
                }

                return Globalize.format(value, formatString);
            }
        };
    });

    // Format a date value
    angular.module('sp.common.filters').filter('spDateTime', function () {        
        return function (value, format) {
            if (_.isNull(value) || _.isUndefined(value) || value === '') {
                return '';
            }

            if (!_.isDate(value)) {
                // Globalize.format wants a Date object
                value = new Date(value);
            }

            switch (format) {
                case 'dateTimeShort':
                    return Globalize.format(value, 'd') + ' ' + Globalize.format(value, 'h:mm tt');
                    
                case 'dateTime24Hour':
                    return Globalize.format(value, 'd') + ' ' + Globalize.format(value, 'HH:mm');

                case 'dateTimeDayMonth':
                    return Globalize.format(value, 'M');

                case 'dateTimeMonth':
                    return Globalize.format(value, 'MMM');

                case 'dateTimeDayMonthTime':
                    return Globalize.format(value, 'M') + ' ' + Globalize.format(value, 'h:mm tt');

                case 'dateTimeLong':
                    return Globalize.format(value, 'D') + ' ' + Globalize.format(value, 'h:mm tt');
                    
                case 'dateTimeSortable':
                    return Globalize.format(value, 'S');
                    
                case 'dateTimeMonthYear':
                    return Globalize.format(value, 'Y');
                
                case 'dateTimeQuarter':
                    return formatDateAsQuarterFormat(value);

                case 'dateTimeQuarterYear':
                    return formatDateAsQuarterFormat(value) + ', ' + Globalize.format(value, 'yyyy');

                case 'dateTimeYear':
                    return Globalize.format(value, 'yyyy');

                case 'dateTimeWeekday':
                    return formatDateAsWeekdayFormat(value);

                case 'dateTimeDate':
                    return Globalize.format(value, 'd');

                case 'dateTimeTime':
                    return Globalize.format(value, 'h:mm tt');

                default:
                    return Globalize.format(value, 'd') + ' ' + Globalize.format(value, 'h:mm tt');
            }
        };
    });
}());
