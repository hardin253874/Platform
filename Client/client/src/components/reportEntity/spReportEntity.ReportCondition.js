// Copyright 2011-2016 Global Software Innovation Pty Ltd
var spReportEntity;

(function (spReportEntity) {

    /**
     * ReportCondition constructor. Do not use.
     * @private
     * @class
     * @name spReportEntity.ReportCondition
     *
     * @classdesc
     * Represents a wrapped ReportCondition as it should appear when viewing schema details for a report.     
     */
    var ReportCondition = (function () {

        function ReportCondition(reportConditionEntity) {
            if (!reportConditionEntity)
                throw new Error('reportConditionEntity is required');
            this._reportConditionEntity = reportConditionEntity;
        }

        /**
         * Returns the entity for this ReportCondition.
         *
         * @returns {spEntity.Entity} The Report Condition entity.
         *
         * @function
         * @name spReportEntity.ReportCondition#getEntity
         */
        ReportCondition.prototype.getEntity = function () {
            return this._reportConditionEntity;
        };
        
        /**
         * Returns the node id of the ReportCondition.
         *
         * @returns long The id of this ReportCondition.
         * @returns long The id of this ReportCondition.
         *
         * @function
         * @name spReportEntity.ReportCondition#id
         */
        ReportCondition.prototype.id = function () {
            return this._reportConditionEntity.id();
        };
        
        /**
          * Returns the name of the Report column with columnForCondition relationship, otherwise returns the name of report condition.
          *
          * @returns {object} The name of this ReportColumn or ReportCondition.
          *
          * @function
          * @name spReportEntity.ReportCondition#getName
          */
        ReportCondition.prototype.getName = function () {
            if (this._reportConditionEntity.getColumnForCondition() && (this._reportConditionEntity.getColumnForCondition())) {
                return this._reportConditionEntity.getColumnForCondition().getName();
            } else {
                return this._reportConditionEntity.getName();
            }
        };

        /**
        * Returns isHidden for this ReportCondition.
        *
        * @returns {bool} The isHidden of Report Condition.
        *
        * @function
        * @name spReportEntity.ReportCondition#isHidden
        */
        ReportCondition.prototype.isHidden = function () {
            return this._reportConditionEntity.getConditionIsHidden();
        };
        
        /**
       * Returns isLocked for this ReportCondition.
       *
       * @returns {bool} The isLocked of Report Condition.
       *
       * @function
       * @name spReportEntity.ReportCondition#isLocked
       */
        ReportCondition.prototype.isLocked = function () {
            return this._reportConditionEntity.getConditionIsLocked();
        };

        /**
          * Returns display order for this ReportCondition.
          *
          * @returns {int} The isLocked of Report Condition.
          *
          * @function
          * @name spReportEntity.ReportCondition#displayOrder
          */
        ReportCondition.prototype.displayOrder = function () {
            return this._reportConditionEntity.getConditionDisplayOrder();
        };
        
        /**
          * Returns display order for this ReportCondition.
          *
          * @returns {int} The isLocked of Report Condition.
          *
          * @function
          * @name spReportEntity.ReportCondition#displayOrder
          */
        ReportCondition.prototype.setDisplayOrder = function (value) {
            return this._reportConditionEntity.setConditionDisplayOrder(value);
        };

        ReportCondition.prototype.getExpression = function () {           
            if (this._reportConditionEntity.getConditionExpression()) {

                return new spReportEntity.Expression(this._reportConditionEntity.getConditionExpression());
            }

            return null;
        };


        /**
          * Returns operator for this ReportCondition.
          *
          * @returns {object} The operator of Report Condition.
          *
          * @function
          * @name spReportEntity.ReportCondition#getOperator
          */
        ReportCondition.prototype.getOperator = function() {
            return this._reportConditionEntity.getOperator();
        };
        
        ReportCondition.prototype.reset = function () {
            this._reportConditionEntity.setOperator(null);
            this._reportConditionEntity.setConditionParameter(null);
        };

        ReportCondition.prototype.apply = function (operator, parameter) {
            this._reportConditionEntity.setOperator(operator);
            this._reportConditionEntity.setConditionParameter(parameter);
        };
        /**
          * Returns Condition Parameter for this ReportCondition.
          *
          * @returns {object} The Condition Parameter of Report Condition.
          *
          * @function
          * @name spReportEntity.getConditionParameter#getConditionParameter
          */
        ReportCondition.prototype.getConditionParameter = function() {
            return this._reportConditionEntity.getConditionParameter();
        };
        return ReportCondition;
    })();

    spReportEntity.ReportCondition = ReportCondition;

})(spReportEntity || (spReportEntity = {}));