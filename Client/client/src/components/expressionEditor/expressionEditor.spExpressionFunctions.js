// Copyright 2011-2016 Global Software Innovation Pty Ltd
/*global _, angular, console, CodeMirror, sp, spResource, spEntity */

(function () {
    'use strict';

    angular
        .module('sp.common.ui.expressionEditor')
        .value('spExpressionFunctions', {
            //when update the categories list, please update the defineMIME class in /Components/codemirror/spql.js  
            categories: [
                {
                    name: 'General functions',
                    functions: [
                        { name: 'and', signature: 'x and y', description: 'Evaluates to true if both the left and right hand input are true.' },
                        { name: 'or', signature: 'x or y', description: 'Evaluates to true if either the left or right hand input are true.' },
                        { name: 'not', signature: 'not x', description: 'Negates a boolean (yes/no) value. That is, true returns false, and false returns true.' },
                        { name: 'iif', signature: 'iif(condition, true-value, false-value)', description: 'Evaluates a condition. If the condition is true, then the true-value is returned, otherwise the false-value is returned.' },
                        { name: 'null', signature: 'null', description: 'Represents an empty value.' },
                        { name: 'isnull', signature: 'isnull(value, default-value)', description: 'Check whether a value is null. If it is null then another default-value is returned. Both inputs must be of the same data type.' },
                        { name: 'is null', signature: 'x is null', description: 'Returns true if the input is null, otherwise returns false.' },
                        { name: 'is not null', signature: 'x is not null', description: 'Returns false if the input is null, otherwise returns true.' },
                        { name: 'convert', signature: 'convert(type, data)', description: 'Converts data to a particular type. Or resources to a particular resource type.' }
                    ]
                },
                {
                    name: 'Data functions',
                    functions: [
                        //{ name: 'resource', signature: 'resource()', description: '' },
                        { name: 'all', signature: 'all(resource-type)', description: 'Returns all resources of the specified resource type.' },
                        { name: 'context', signature: 'context()', description: 'Returns the starting resource that this formula is operating on. For example, the resource of a report row.' }
                    ]
                },
                {
                    name: 'Mathematical functions',
                    functions: [
                        { name: 'abs', signature: 'abs(number)', description: 'Calculates the absolute (i.e. positive) value of a number.' },
                        { name: 'ceiling', signature: 'ceiling(number)', description: 'Rounds a decimal number up to the nearest whole number.' },
                        { name: 'exp', signature: 'exp(number)', description: 'Calculates the natural exponent of a number.' },
                        { name: 'floor', signature: 'floor(number)', description: 'Rounds a decimal number down to the nearest whole number.' },
                        { name: 'log', signature: 'log(number)', description: 'Calculates the natural logarithm of a number.' },
                        { name: 'log10', signature: 'log10(number)', description: 'Calculates the base-10 logarithm of a number.' },
                        { name: 'power', signature: 'power(number, base)', description: 'Raises a number to the power of the specified base.' },
                        { name: 'round', signature: 'round(number, places)', description: 'Rounds a number to the specified number of decimal places.' },
                        { name: 'sign', signature: 'sign(number)', description: 'Determines the sign of a value. Returns 1 for positive values, -1 for negative values, and 0 for zero.' },
                        { name: 'square', signature: 'square(number)', description: 'Calculates the square of a value.' },
                        { name: 'sqrt', signature: 'sqrt(number)', description: 'Calculates the square root of a value.' }
                    ]
                },
                {
                    name: 'Text functions',
                    functions: [
                        { name: 'charindex', signature: 'charindex(text-to-find, text-to-search, starting-position)', description: 'Searches text for a particular value and returns the character number where it is located. The first character is position 1. Returns zero if the text could not be found. Optionally a starting position can be specified.' },
                        { name: 'left', signature: 'left(text, number)', description: 'Returns a specified number of characters from the left hand side of some text.' },
                        { name: 'len', signature: 'len(text)', description: 'Returns the length of some text. That is, the number of characters.' },
                        { name: 'replace', signature: 'replace(initial-text, find, replace-with)', description: 'Replaces all instances of a specified text value with another text value.' },
                        { name: 'right', signature: 'right(text, number)', description: 'Returns a specified number of characters from the right hand side of some text.' },
                        { name: 'substring', signature: 'substring(text, start-pos, length)', description: 'Returns part of a text value, starting at a given position, returning the specified number of characters. The first character is at position 1.' },
                        { name: 'tolower', signature: 'tolower(text)', description: 'Converts text input to lower-case.' },
                        { name: 'toupper', signature: 'toupper(text)', description: 'Converts text input to upper-case.' },
                        { name: 'like', signature: 'x like y', description: 'Compares one text string to another, and returns true if they are the same. The text on the right may start or end with % to indicate partial matching.' },
                        { name: 'not like', signature: 'x not like y', description: 'Compares one text string to another, and returns true if they are different. The text on the right may start or end with % to indicate partial matching.' }
                    ]
                },
                {
                    name: 'Date/Time functions',
                    functions: [
                        { name: 'datefromparts', signature: 'datefromparts(year, month, day)', description: 'Creates a date value from individual year, month, day, hour, minute and second components.' },
                        { name: 'timefromparts', signature: 'timefromparts(hour, minute, second)', description: 'Creates a time value from individual hour, minute and second components.' },
                        { name: 'datetimefromparts', signature: 'datetimefromparts(year, month, day, hour, minute, second)', description: 'Creates a datetime value from individual year, month, day, hour, minute and second components.' },
                        { name: 'year', signature: 'year(date)', description: 'Returns the year component of a date or date-time.' },
                        { name: 'month', signature: 'month(date)', description: 'Returns the month component of a date or date-time.' },
                        { name: 'day', signature: 'day(date)', description: 'Returns the day component of a date or date-time.' },
                        { name: 'hour', signature: 'hour(time)', description: 'Returns the hour component of a time or date-time.' },
                        { name: 'minute', signature: 'minute(time)', description: 'Returns the minute component of a time or date-time.' },
                        { name: 'second', signature: 'second(time)', description: 'Returns the second component of a time or date-time.' },
                        { name: 'getdate', signature: 'getdate()', description: 'Returns the current date in the local timezone.' },
                        { name: 'getdatetime', signature: 'getdatetime()', description: 'Returns the current date-time.' },
                        { name: 'gettime', signature: 'gettime()', description: 'Returns the current time in the local timezone.' },
                        { name: 'dateadd', signature: 'dateadd(datepart,number,date)', description: 'Adds or subtracts a specified time interval from a date. Date is a valid date expression and number is the number of interval you want to add. The number can either be positive, for dates in the future, or negative, for dates in the past. Datepart can be one of the following: year, quarter, month, dayofyear, day, week, weekday, hour, minute.' },
                        { name: 'datediff', signature: 'datediff(datepart, startdate, enddate)', description: 'Where startdate and enddate are valid date expressions and datepart can be one of the following: year, quarter, month, dayofyear, day, week, weekday, hour, minute.' },
                        { name: 'datename', signature: 'datename(datepart, date)', description: 'Returns the text name of part of a date, such as a month name or a weekday name. Datepart can be one of:  year, quarter, month, dayofyear, day, week, weekday, hour, minute' },
                        { name: 'dayofyear', signature: 'dayofyear(date)', description: 'Returns the day of the year of a date or date-time.' },
                        { name: 'quarter', signature: 'quarter(date)', description: 'Returns the quarter of a date or date-time, between 1 and 4.' },
                        { name: 'week', signature: 'week(date)', description: 'Returns the week of the year of a date or date-time.' },
                        { name: 'weekday', signature: 'weekday(date)', description: 'Returns the day of week of a date or date-time. Sunday through to Saturday return 1-7 respectively.' }
                    ]
                },
                {
                    name: 'Aggregate functions',
                    functions: [
                        { name: 'any', signature: 'any(bool)', description: 'Returns true if any condition in a list of conditions is true.' },
                        { name: 'count', signature: 'count()', description: 'Returns the number of resources in a collection.' },
                        { name: 'every', signature: 'every(bool)', description: 'Returns true if every condition in a list of conditions is true.' },
                        { name: 'max', signature: 'max(number)', description: 'Returns the maximum value in a list of values.' },
                        { name: 'min', signature: 'min(number)', description: 'Returns the minimum value in a list of values.' },
                        { name: 'sum', signature: 'sum(number)', description: 'Returns the sum of a list of numeric values.' },
                        { name: 'avg', signature: 'avg(number)', description: 'Returns the average of a list of numeric values.' },
                        { name: 'stdev', signature: 'stdev(number)', description: 'Returns the standard deviation of a list of numeric values.' },
                        { name: 'join', signature: 'join(text)', description: 'Concatenates a list of text fields together with commas.' }
                    ]
                }
            ]});
})();
