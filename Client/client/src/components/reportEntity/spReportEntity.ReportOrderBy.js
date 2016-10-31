// Copyright 2011-2016 Global Software Innovation Pty Ltd
var spReportEntity;

(function (spReportEntity) {

    /**
     * ReportOrderBy constructor. Do not use.
     * @private
     * @class
     * @name spReportEntity.ReportOrderBy
     *
     * @classdesc
     * Represents a wrapped ReportOrderBy as it should appear when viewing schema details for a report.     
     */
    var ReportOrderBy = (function () {

        function ReportOrderBy(reportOrderByEntity) {
            if (!reportOrderByEntity)
                throw new Error('reportOrderByEntity is required');
            this._reportOrderByEntity = reportOrderByEntity;
        }
        
        /**
        * Returns the entity for this ReportColumn.
        *
        * @returns {spEntity.Entity} The report column entity.
        *
        * @function
        * @name spReportEntity.ReportColumn#getEntity
        */
        ReportOrderBy.prototype.getEntity = function () {
            return this._reportOrderByEntity;
        };

        /**
        * Returns the node id of the ReportOrderBy.
        *
        * @returns long The id of this ReportOrderBy.
        *
        * @function
        * @name spReportEntity.ReportOrderBy#id
        */
        ReportOrderBy.prototype.id = function () {
            return this._reportOrderByEntity.id();
        };

        /**
          * Returns the ReverseOrder of the reportOrderBy.
          *
          * @returns {object} The ReverseOrder of this report orderby.
          *
          * @function
          * @name spReportEntity.ReportOrderBy#getReverseOrder
          */
        ReportOrderBy.prototype.getReverseOrder = function() {
            return this._reportOrderByEntity.getReverseOrder();
        };
        
        /**
         * Returns the OrderPriority of the reportOrderBy.
         *
         * @returns {object} The OrderPriority of this report orderby.
         *
         * @function
         * @name spReportEntity.ReportOrderBy#getOrderPriority
         */
        ReportOrderBy.prototype.getOrderPriority = function () {
            return this._reportOrderByEntity.getOrderPriority();
        };
        
        /**
        * Returns the expression of the reportOrderBy.
        *
        * @returns {object} The expression of this report orderby.
        *
        * @function
        * @name spReportEntity.ReportOrderBy#getExpression
        */
        ReportOrderBy.prototype.getExpression = function () {
            if (this._reportOrderByEntity.getOrderByExpression()) {
                return new spReportEntity.Expression(this._reportOrderByEntity.getOrderByExpression());
            }

            return null;
        };

        return ReportOrderBy;
    })();

    spReportEntity.ReportOrderBy = ReportOrderBy;

})(spReportEntity || (spReportEntity = {}));