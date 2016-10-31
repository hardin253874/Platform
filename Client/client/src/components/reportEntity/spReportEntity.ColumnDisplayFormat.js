// Copyright 2011-2016 Global Software Innovation Pty Ltd
var spReportEntity;

(function (spReportEntity) {

    /**
     * ColumnDisplayFormat constructor. Do not use.
     * @private
     * @class
     * @name spReportEntity.ColumnDisplayFormat
     *
     * @classdesc
     * Represents a wrapped ColumnDisplayFormat as it should appear when viewing schema details for a report.     
     */
    var ColumnDisplayFormat = (function () {

        function ColumnDisplayFormat(columnDisplayFormatEntity) {
            if (!columnDisplayFormatEntity)
                throw new Error('columnDisplayFormatEntity is required');
            this._columnDisplayFormatEntity = columnDisplayFormatEntity;
           
        }

        /**
        * Returns the entity for this ColumnDisplayFormat.
        *
        * @returns {spEntity.Entity} The ColumnDisplayFormat entity.
        *
        * @function
        * @name spReportEntity.ColumnDisplayFormat#getEntity
        */
        ColumnDisplayFormat.prototype.getEntity = function () {
            return this._columnDisplayFormatEntity;
        };
        
        /**
        * Returns the ColumnShowText for this ColumnDisplayFormat.
        *
        * @returns {bool} The ColumnShowText of ColumnDisplayFormat.
        *
        * @function
        * @name spReportEntity.ColumnDisplayFormat#getColumnShowText
        */
        ColumnDisplayFormat.prototype.getColumnShowText = function () {
            return this._columnDisplayFormatEntity.getColumnShowText();
        };


        /**
        * Returns the FormatImageScale for this ColumnDisplayFormat.
        *
        * @returns {object} The FormatImageScale of ColumnDisplayFormat.
        *
        * @function
        * @name spReportEntity.ColumnDisplayFormat#getFormatImageScale
        */
        ColumnDisplayFormat.prototype.getFormatImageScale = function () {
            return this._columnDisplayFormatEntity.getFormatImageScale();
        };
        
        /**
        * Returns the timeColumnFormat for this ColumnDisplayFormat.
        *
        * @returns {object} The timeColumnFormat of ColumnDisplayFormat.
        *
        * @function
        * @name spReportEntity.ColumnDisplayFormat#getTimeColumnFormat
        */
        ColumnDisplayFormat.prototype.getTimeColumnFormat = function () {
            return this._columnDisplayFormatEntity.getTimeColumnFormat();
        };
        
        /**
        * Returns the dateColumnFormat for this ColumnDisplayFormat.
        *
        * @returns {object} The dateColumnFormat of ColumnDisplayFormat.
        *
        * @function
        * @name spReportEntity.ColumnDisplayFormat#getDateColumnFormat
        */
        ColumnDisplayFormat.prototype.getDateColumnFormat = function () {
            return this._columnDisplayFormatEntity.getDateColumnFormat();
        };

        /**
        * Returns the dateTimeColumnFormat for this ColumnDisplayFormat.
        *
        * @returns {object} The dateTimeColumnFormat of ColumnDisplayFormat.
        *
        * @function
        * @name spReportEntity.ColumnDisplayFormat#getDateTimeColumnFormat
        */
        ColumnDisplayFormat.prototype.getDateTimeColumnFormat = function () {
            return this._columnDisplayFormatEntity.getDateTimeColumnFormat();
        };
        
        /**
        * Returns the formatDecimalPlaces for this ColumnDisplayFormat.
        *
        * @returns {int} The formatDecimalPlaces of ColumnDisplayFormat.
        *
        * @function
        * @name spReportEntity.ColumnDisplayFormat#getFormatDecimalPlaces
        */
        ColumnDisplayFormat.prototype.getFormatDecimalPlaces = function () {
            return this._columnDisplayFormatEntity.getFormatDecimalPlaces();
        };
        
        /**
        * Returns the formatPrefix for this ColumnDisplayFormat.
        *
        * @returns {string} The formatPrefix of ColumnDisplayFormat.
        *
        * @function
        * @name spReportEntity.ColumnDisplayFormat#getFormatPrefix
        */
        ColumnDisplayFormat.prototype.getFormatPrefix = function () {
            return this._columnDisplayFormatEntity.getFormatPrefix();
        };
        
        /**
        * Returns the formatSuffix for this ColumnDisplayFormat.
        *
        * @returns {string} The formatSuffix of ColumnDisplayFormat.
        *
        * @function
        * @name spReportEntity.ColumnDisplayFormat#getFormatSuffix
        */
        ColumnDisplayFormat.prototype.getFormatSuffix = function () {
            return this._columnDisplayFormatEntity.getFormatSuffix();
        };
        
        /**
        * Returns the maxLineCount for this ColumnDisplayFormat.
        *
        * @returns {int} The maxLineCount of ColumnDisplayFormat.
        *
        * @function
        * @name spReportEntity.ColumnDisplayFormat#getMaxLineCount
        */
        ColumnDisplayFormat.prototype.getMaxLineCount = function () {
            return this._columnDisplayFormatEntity.getMaxLineCount();
        };
        
        /**
        * Returns the formatAlignment for this ColumnDisplayFormat.
        *
        * @returns {object} The formatAlignment of ColumnDisplayFormat.
        *
        * @function
        * @name spReportEntity.ColumnDisplayFormat#getFormatAlignment
        */
        ColumnDisplayFormat.prototype.getFormatAlignment = function () {
            return this._columnDisplayFormatEntity.getFormatAlignment();
        };

        return ColumnDisplayFormat;
    })();



    spReportEntity.ColumnDisplayFormat = ColumnDisplayFormat;

})(spReportEntity || (spReportEntity = {}));