// Copyright 2011-2016 Global Software Innovation Pty Ltd
var spReportEntity;

(function (spReportEntity) {

    /**
     * ColumnFormattingRule constructor. Do not use.
     * @private
     * @class
     * @name spReportEntity.ColumnFormattingRule
     *
     * @classdesc
     * Represents a wrapped ColumnFormattingRule as it should appear when viewing schema details for a report.     
     */
    var ColumnFormattingRule = (function () {

        function ColumnFormattingRule(columnFormattingRule) {
            if (!columnFormattingRule)
                throw new Error('columnFormattingRule is required');
            this._columnFormattingRuleEntity = columnFormattingRule;
          
        }

        /**
         * Returns the entity for this columnFormattingRule.
         *
         * @returns {spEntity.Entity} The columnFormattingRule entity.
         *
         * @function
         * @name spReportEntity.ColumnFormattingRule#getEntity
         */
        ColumnFormattingRule.prototype.getEntity = function () {
            return this._columnFormattingRuleEntity;
        };

        /**
        * Returns the type array of the expression.
        *
        * @returns [{object}] The type array of this expression.
        *
        * @function
        * @name spReportEntity.ColumnFormattingRule#getTypes
        */
        ColumnFormattingRule.prototype.getType = function () {
            
            if (this._columnFormattingRuleEntity.getIsOfType && this._columnFormattingRuleEntity.getIsOfType()) {
                return this._columnFormattingRuleEntity.getIsOfType();
            }
            else if (this._columnFormattingRuleEntity.getType && this._columnFormattingRuleEntity.getType()) {
                return this._columnFormattingRuleEntity.getType();
            }
            else {
                return null;
            }
        };
        
        /**
        * Returns the first type alias in array of the reportNode.
        *
        * @returns [{object}] The first type alias array of this report Node.
        *
        * @function
        * @name spReportEntity.ColumnFormattingRule#getTypeAlias
        */
        ColumnFormattingRule.prototype.getTypeAlias = function () {

            if (this._columnFormattingRuleEntity.getIsOfType && this._columnFormattingRuleEntity.getIsOfType()) {
                return this._columnFormattingRuleEntity.getIsOfType()[0].alias();
            }
            else if (this._columnFormattingRuleEntity.getType && this._columnFormattingRuleEntity.getType()) {
                return this._columnFormattingRuleEntity.getType().alias();
            }
            else {
                return null;
            }
        };

        /**
        * Returns the bar color of the ColumnFormattingRule.
        *
        * @returns {object} The bar color  of this ColumnFormattingRule.
        *
        * @function
        * @name spReportEntity.ColumnFormattingRule#getBarColor
        */
        ColumnFormattingRule.prototype.getBarColor = function () {
            return this._columnFormattingRuleEntity.getBarColor();
        };

        /**
        * Returns the bar MinValue of the ColumnFormattingRule.
        *
        * @returns {object} The Bar MinValue of this ColumnFormattingRule.
        *
        * @function
        * @name spReportEntity.ColumnFormattingRule#getBarMinValue
        */
        ColumnFormattingRule.prototype.getBarMinValue = function () {
            return this._columnFormattingRuleEntity.getBarMinValue();
        };
        
        /**
            * Returns the bar MaxValue of the ColumnFormattingRule.
            *
            * @returns {object} The Bar MaxValue of this ColumnFormattingRule.
            *
            * @function
            * @name spReportEntity.ColumnFormattingRule#barMaxValue
            */
        ColumnFormattingRule.prototype.getBarMaxValue = function () {
            return this._columnFormattingRuleEntity.getBarMaxValue();
        };

        /**
           * Returns the Icon Rules of the ColumnFormattingRule.
           *
           * @returns {object} The Icon Rules of this ColumnFormattingRule.
           *
           * @function
           * @name spReportEntity.ColumnFormattingRule#getIconRules
           */
        ColumnFormattingRule.prototype.getIconRules = function () {
            return this._columnFormattingRuleEntity.getIconRules();
        };
        
        /**
          * Returns the color Rules of the ColumnFormattingRule.
          *
          * @returns {object} The color Rules of this ColumnFormattingRule.
          *
          * @function
          * @name spReportEntity.ColumnFormattingRule#getColorRules
          */
        ColumnFormattingRule.prototype.getColorRules = function () {
            return this._columnFormattingRuleEntity.getColorRules();
        };
                

        return ColumnFormattingRule;
    })();



    spReportEntity.ColumnFormattingRule = ColumnFormattingRule;

})(spReportEntity || (spReportEntity = {}));