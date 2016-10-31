// Copyright 2011-2016 Global Software Innovation Pty Ltd
/*global _, angular, sp, spEntity */

angular.module('mod.common.spReportHelpers', ['sp.common.filters', 'mod.common.spTenantSettings']);

/**
 *  A set of stand-alone helpers for working with reports.
 *  Code moved from spReportService so that one can be mocked without the other.
 *  @module spReportHelpers
 */

angular.module('mod.common.spReportHelpers').factory('spReportHelpers', function ($filter, spTenantSettings) {
    'use strict';

    var currencySymbol;

    // Get the currency symbol for the current tenant
    spTenantSettings.getCurrencySymbol().then(function (symbol) {
        if (symbol) {
            currencySymbol = symbol;
        } else {
            currencySymbol = '$';
        }
    });

    /**
     * Returns a function for formatting data as a string
     * @param rcol
     * @param valRule
     * @returns Formatter function.
     */
    // Return a format function for the specified column.
    function getColumnFormatFunc(rcol, valRule) {
        var places,
            prefix,
            suffix,
            datetimefmt,
            columnFormatFunc = _.identity;

        // The specified column does not exist
        // Return a null
        if (!rcol ||
            !rcol.type) {
            return columnFormatFunc;
        }

        if (!valRule) {
            valRule = {};
        }

        switch (rcol.type) {
            case spEntity.DataType.Currency:
                places = angular.isDefined(valRule.places) ? valRule.places : 3;
                prefix = valRule.prefix || '';
                suffix = valRule.suffix || '';
                columnFormatFunc = function (value) {
                    return $filter('spCurrency')(value, currencySymbol, places, prefix, suffix);
                };
                break;
            case spEntity.DataType.Decimal:
                places = angular.isDefined(valRule.places) ? valRule.places : 3;
                prefix = valRule.prefix || '';
                suffix = valRule.suffix || '';
                columnFormatFunc = function (value) {
                    return $filter('spDecimal')(value, places, prefix, suffix);
                };
                break;
            case spEntity.DataType.Int32:
                prefix = valRule.prefix || '';
                suffix = valRule.suffix || '';
                columnFormatFunc = function (value) {
                    return $filter('spNumber')(value, prefix, suffix, rcol.anpat);
                };
                break;
            case spEntity.DataType.Bool:
                columnFormatFunc = $filter('spBoolean');
                break;
            case 'RelatedResource':
            case 'ChoiceRelationship':
            case 'InlineRelationship':
            case 'UserInlineRelationship':
            case 'Image':
                columnFormatFunc = $filter('relatedResource');
                break;
            case spEntity.DataType.Time:
                datetimefmt = valRule.datetimefmt;
                columnFormatFunc = function (value) {
                    return $filter('spTime')(value, datetimefmt);
                };
                break;
            case spEntity.DataType.Date:
                datetimefmt = valRule.datetimefmt;
                columnFormatFunc = function (value) {
                    return $filter('spDate')(value, datetimefmt);
                };
                break;
            case spEntity.DataType.DateTime:
                datetimefmt = valRule.datetimefmt;
                columnFormatFunc = function (value) {
                    return $filter('spDateTime')(value, datetimefmt);
                };
                break;
            case 'StructureLevels':
                columnFormatFunc = $filter('structureLevels');
                break;
        }

        return columnFormatFunc;
    }

    var exports = {
        getColumnFormatFunc: getColumnFormatFunc
    };

    return exports;

});
