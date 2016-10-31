// Copyright 2011-2016 Global Software Innovation Pty Ltd
var spReportEntity;

(function (spReportEntity) {

    /**
     * ReportColumn constructor. Do not use.
     * @private
     * @class
     * @name spReportEntity.ReportColumn
     *
     * @classdesc
     * Represents a wrapped reportcolumn as it should appear when viewing schema details for a report.     
     */
    var ReportColumn = (function () {

        function ReportColumn(reportColumnEntity) {
            if (!reportColumnEntity)
                throw new Error('reportColumnEntity is required');
            this._reportColumnEntity = reportColumnEntity;
        }
        

        /**
          * Returns the entity for this ReportColumn.
          *
          * @returns {spEntity.Entity} The report column entity.
          *
          * @function
          * @name spReportEntity.ReportColumn#getEntity
          */
        ReportColumn.prototype.getEntity = function () {
            return this._reportColumnEntity;
        };

        /**
         * Returns the node id of the ReportColumn.
         *
         * @returns long The id of this ReportColumn.
         *
         * @function
         * @name spReportEntity.ReportColumn#id
         */
        ReportColumn.prototype.id = function () {
            return this._reportColumnEntity.id();
        };

        /**
          * Returns the entity  name for this ReportColumn.
          *
          * @returns {spEntity.Entity} The report column name.
          *
          * @function
          * @name spReportEntity.ReportColumn#getName
          */
        ReportColumn.prototype.getName = function() {
            return this._reportColumnEntity.getName();
        };

        /**
          * Returns the isHidden of the ReportColumn.
          *
          * @returns {bool} The isHidden of this ReportColumn.
          *
          * @function
          * @name spReportEntity.ReportColumn#isHidden
          */
        ReportColumn.prototype.isHidden = function() {
            return this._reportColumnEntity.getColumnIsHidden();
        };

        /**
          * Sets the isHidden of the ReportColumn.          
          *
          * @function
          * @name spReportEntity.ReportColumn#setIsHidden
          */
        ReportColumn.prototype.setIsHidden = function (value) {
            return this._reportColumnEntity.setColumnIsHidden(value);
        };

        /**
        * Returns the displayOrder of the ReportColumn.
        *
        * @returns {int} The displayOrder of this ReportColumn.
        *
        * @function
        * @name spReportEntity.ReportColumn#displayOrder
        */
        ReportColumn.prototype.displayOrder = function() {
            return this._reportColumnEntity.getColumnDisplayOrder();
        };
        
        /**
        * Returns the displayOrder of the ReportColumn.
        *
        * @returns {int} The displayOrder of this ReportColumn.
        *
        * @function
        * @name spReportEntity.ReportColumn#displayOrder
        */
        ReportColumn.prototype.setDisplayOrder = function (value) {
            return this._reportColumnEntity.setColumnDisplayOrder(value);
        };

        /**
       * Returns the expression of the ReportColumn.
       *
       * @returns {object} The expression of this ReportColumn.
       *
       * @function
       * @name spReportEntity.ReportColumn#getExpression
       */
        ReportColumn.prototype.getExpression = function() {

            if (this._reportColumnEntity.getColumnExpression()) {

                return new spReportEntity.Expression(this._reportColumnEntity.getColumnExpression());
            }

            return null;

        };

        /**
      * Returns the ColumnFormattingRule of the ReportColumn.
      *
      * @returns {object} The ColumnFormattingRule of this ReportColumn.
      *
      * @function
      * @name spReportEntity.ReportColumn#getColumnFormattingRule
      */
        ReportColumn.prototype.getColumnFormattingRule = function () {
            
            if (this._reportColumnEntity.getColumnFormattingRule()) {

                return new spReportEntity.ColumnFormattingRule(this._reportColumnEntity.getColumnFormattingRule());
            }
            
            return null;
        };

        ReportColumn.prototype.setColumnFormattingRule = function (columnFormattingRule) {
            if (columnFormattingRule)
                this._reportColumnEntity.columnFormattingRule = columnFormattingRule.getEntity();
                //this._reportColumnEntity.setColumnFormattingRule(columnFormattingRule.getEntity());
            else
                this._reportColumnEntity.setColumnFormattingRule(null);
        };

        /**
          * Returns the ColumnDisplayFormat of the ReportColumn.
          *
          * @returns {object} The ColumnDisplayFormat of this ReportColumn.
          *
          * @function
          * @name spReportEntity.ReportColumn#getColumnDisplayFormat
          */
        ReportColumn.prototype.getColumnDisplayFormat = function () {
            
            if (this._reportColumnEntity.getColumnDisplayFormat()) {

                return new spReportEntity.ColumnDisplayFormat(this._reportColumnEntity.getColumnDisplayFormat());
            }

            return null;           
        };

        ReportColumn.prototype.setColumnDisplayFormat = function (columnDisplayFormat) {
            if (columnDisplayFormat)
                this._reportColumnEntity.columnDisplayFormat = columnDisplayFormat.getEntity();
                //this._reportColumnEntity.setColumnDisplayFormat(columnDisplayFormat.getEntity());
            else
                this._reportColumnEntity.setColumnDisplayFormat(null);
        };


        /* ColumnGrouping       */
        
        ReportColumn.prototype.getColumnGroupingByGroupingPriority = function (groupingPriority) {
            if (this._reportColumnEntity.getColumnGrouping()) {
                return _.find(this._reportColumnEntity.getColumnGrouping(), function (grouping) {
                    return grouping.getGroupingPriority() === groupingPriority;
                }
                );
            } else {
                return null;
            }
        };
        
        ReportColumn.prototype.getColumnGroupingByGroupingMethod = function (groupingMethodAlias) {
            if (this._reportColumnEntity.getColumnGrouping()) {
                return _.find(this._reportColumnEntity.getColumnGrouping(), function (grouping)
                    {
                        return grouping._id.getAlias() === groupingMethodAlias;
                    }
                );
            } else {
                return null;
            }
        };
        
        ReportColumn.prototype.addColumnGrouping = function (columnGrouping) {
            this.getEntity().getColumnGrouping().add(columnGrouping);
        };
        
        ReportColumn.prototype.removeColumnGroupingByGrouping = function (columnGrouping) {
            this.getEntity().getColumnGrouping().remove(columnGrouping);
        };
        
        ReportColumn.prototype.removeColumnGrouping = function () {
            var columnGroupings = this.getEntity().getColumnGrouping(),
                columnGroupingInstances = _.clone(columnGroupings.getInstances());            

            _.forEach(columnGroupingInstances, function (columnGroupingInstance) {                            
                columnGroupings.remove(columnGroupingInstance.entity, true);                                    
            });
        };

        ReportColumn.prototype.setColumnGroupingCollapsedState = function(collapsed) {
            var columnGroupings = this.getEntity().getColumnGrouping();                

            _.forEach(columnGroupings, function (columnGrouping) {
                columnGrouping.setField('core:groupingCollapsed', collapsed, spEntity.DataType.Bool);
            });
        };
        
        ReportColumn.prototype.getColumnGrouping = function () {
            if (!this._reportColumnEntity.columnGrouping) {
                this._reportColumnEntity.registerRelationship('core:columnGrouping');
            }

            return this._reportColumnEntity.getColumnGrouping();            
        };
        
        /* ColumnRollup       */
        ReportColumn.prototype.getColumnRollup = function () {
            if (!this._reportColumnEntity.columnRollup) {
                this._reportColumnEntity.registerRelationship('core:columnRollup');
            }

            return this._reportColumnEntity.getColumnRollup();
        };
        
        ReportColumn.prototype.getColumnRollupByRollupMethod = function (rollupMethodAlias) {
            var columnRollups = this._reportColumnEntity.getColumnRollup();
            if (columnRollups) {
                return _.find(columnRollups, function (rollup) {
                    return rollup.getRollupMethod().getAlias() === rollupMethodAlias;
                });
            } else {
                return null;
            }
        };
        
        ReportColumn.prototype.addColumnRollup = function (columnRollup) {
            this.getEntity().getColumnRollup().add(columnRollup);
        };

        ReportColumn.prototype.removeColumnRollup = function (columnRollup) {
            this.getEntity().getColumnRollup().remove(columnRollup);
        };

        return ReportColumn;
    })();

    spReportEntity.ReportColumn = ReportColumn;

})(spReportEntity || (spReportEntity = {}));