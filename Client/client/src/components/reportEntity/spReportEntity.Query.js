// Copyright 2011-2016 Global Software Innovation Pty Ltd
var spReportEntity;

(function (spReportEntity) {

    /**
     * spReportEntity Query.
     * @private
     * @class
     * @name spReportEntity.Query
     *
     * @classdesc
     * TODO
     */
    var Query = (function() {
        /**
         * spReportEntity Query constructor. Wraps the query to provide convenient access to schema information.          
         *
         * @param {query} typeEnties A type entity, or array of type entities, to be wrapped.
         *
         * @example
         * <pre>
          * var rq = spReportEntity.makeTypeRequest();
         * spEntityService.getEntity(reportId, rq).then(function(reportQuery) {
         *    var reportQuery = new spReportEntity.Query(reportQuery);
         *    var reportNodes = reportQuery.getNodes();
         *    var reportColumns = reportQuery.getColumns();
         *    // etc..
         * });   
         * </pre>
         *
         * @private
         * @class
         * @name spReportEntity.Query
         */
        function Query(reportQuery)
        {
            this._query = reportQuery;
        }
        
        /**
        * Get the report entity that was passed into the constructor.
        *
        * @returns {spEntity.Entity} The entity. (Or the first if multiple were passed, or null/undefined).
        *
        * @function
        * @name spReportEntity.Query#getEntity
        */
        Query.prototype.getEntity = function getEntity() {
            return this._query;
        };


        /**
        * Get the name for this report.
        *
        * @returns {string} The description.
        *
        * @function
        * @name spReportEntity.Query#getName
        */
        Query.prototype.getName = function getName() {
            return this.getEntity().getName();
        };

        /**
         * Get the description for this report.
         *
         * @returns {string} The description.
         *
         * @function
         * @name spReportEntity.Query#getDescription
         */
        Query.prototype.getDescription = function getDescription() {
            return this.getEntity().getField('description');
        };

        /**
         * Get the rootNode for this report.
         *
         * @returns {reportNode} The report root node.
         *
         * @function
         * @name spReportEntity.Query#getRootNode
         */
        Query.prototype.getRootNode = function getRootNode() {          
            if (!this._rootNode) {                
                this.updateRootNode();
            }
            return this._rootNode;
        };

        Query.prototype.updateRootNode = function updateRootNode() {
            if (this.getEntity() && this.getEntity().getRootNode())
            {
                this._rootNode = new spReportEntity.ReportNode(this.getEntity().getRootNode());
            }
        };

        /**
         * Get the reportColumns for this report.
         *
         * @returns [{reportColumn}] array The report columns.
         *
         * @function
         * @name spReportEntity.Query#getReportColumns
         */
        Query.prototype.getReportColumns = function getReportColumns() {            
            if (!this._reportColumns) {
                this.updateReportColumns();
            }
            return this._reportColumns;
        };

        Query.prototype.updateReportColumns = function updateReportColumns() {
            
            this._reportColumns = _.map(this.getEntity().getReportColumns(), function (col) {
                return new spReportEntity.ReportColumn(col);
            });
        };

        /**
        * Get the reportOrderBys for this report.
        *
        * @returns [{reportOrderBy}] array The report OrderBys.
        *
        * @function
        * @name spReportEntity.Query#getReportOrderBys
        */
        Query.prototype.getReportOrderBys = function getReportOrderBys() {
            
            if (!this._reportOrderBys) {
                this.updateReportOrderBys();
            }
            return this._reportOrderBys;
        };

        Query.prototype.updateReportOrderBys = function updateReportOrderBys() {
           
            this._reportOrderBys = _.map(this.getEntity().getReportOrderBys(), function (orderby) {
                return new spReportEntity.ReportOrderBy(orderby);
            });
        };

        /**
        * Get the reportOrderBys for this report.
        *
        * @returns [{reportOrderBy}] array The report OrderBys.
        *
        * @function
        * @name spReportEntity.Query#getReportOrderBys
        */
        Query.prototype.getReportConditions = function getReportConditions() {
            
            if (!this._reportConditions) {
                this.updateReportConditions();
            }
            return this._reportConditions;
        };


        Query.prototype.updateReportConditions = function updateReportConditions() {        
            this._reportConditions = _.map(this.getEntity().getHasConditions(), function (con) {
                return new spReportEntity.ReportCondition(con);
            });
        };

        /**
           * Add a report column to reportcolumns array
           * @function
           * @name spReportEntity.Query#addReportColumn
           */
        Query.prototype.addReportColumn = function (reportColumn) {

            this.getEntity().getReportColumns().add(reportColumn.getEntity());
           
            this.updateReportColumns();
        };
        
        /**
           * clone a existing report column 
           * @function
           * @name spReportEntity.Query#cloneReportColumn
           */
        Query.prototype.cloneReportColumn = function (reportColumn) {
            var reportColumnEntity = reportColumn.getEntity();

            var newReportColumnEntity = spEntity.fromJSON({
                typeId: 'reportColumn',
                name: reportColumn.getName(),
                columnDisplayOrder: this.getEntity().getReportColumns().length,
                columnIsHidden: reportColumn.isHidden(),
                columnExpression: reportColumnEntity.getColumnExpression(),
                columnFormattingRule: reportColumnEntity.getColumnFormattingRule() ? reportColumnEntity.getColumnFormattingRule() : jsonLookup(),
                columnDisplayFormat: reportColumnEntity.getColumnDisplayFormat() ? reportColumnEntity.getColumnDisplayFormat() : jsonLookup(),
                columnGrouping: reportColumnEntity.getColumnGrouping() ? reportColumnEntity.getColumnGrouping() : jsonRelationship(),
                columnRollup: reportColumn.getColumnRollup() ? reportColumn.getColumnRollup() : jsonRelationship()
            });

            newReportColumnEntity.setId(spEntity._getNextId());
            newReportColumnEntity.setDataState(spEntity.DataStateEnum.Create);


            var cloneReportColumnEntity = spReportEntity.cloneReportEntity(newReportColumnEntity);

            return new spReportEntity.ReportColumn(cloneReportColumnEntity);
        };

        /**
         * remove report column
         * @function
         * @name spReportEntity.Query#removeReportColumn
         */
        Query.prototype.removeReportColumn = function (reportColumn) {    
            this.getEntity().getReportColumns().deleteEntity(reportColumn.getEntity());
                this.updateReportColumns();
        };

        
        /**
           * remove report column by report column id
           * @function
           * @name spReportEntity.Query#removeReportColumnByColumnId
           */
        Query.prototype.removeReportColumnByColumnId = function (reportColumnId) {
            

            var currentReportColumn = _.find(this.getReportColumns(), function (reportColumn) {
                return reportColumn.id().toString() === reportColumnId.toString();
            });

            if (currentReportColumn) {
                this.getEntity().getReportColumns().deleteEntity(currentReportColumn.getEntity());
                
                this.updateReportColumns();
            }
          
        };

        
        /**
          * Add a report condition to hasconditions array
          * @function
          * @name spReportEntity.Query#addReportCondition
          */
        Query.prototype.addReportCondition = function (reportCondition) {
            this.getEntity().getHasConditions().add(reportCondition.getEntity());
            this.updateReportConditions();
        };


        /**
          * clone a existing report condition 
          * @function
          * @name spReportEntity.Query#cloneReportCondition
          */
        Query.prototype.cloneReportCondition = function (reportCondition) {
            var reportConditionEntity = reportCondition.getEntity();


            var newReportConditionEntity = spEntity.fromJSON({
                typeId: 'reportCondition',
                name: reportCondition.getName(),
                conditionDisplayOrder: this.getEntity().getHasConditions().length,
                conditionIsHidden: reportCondition.isHidden(),
                conditionIsLocked: reportCondition.isLocked,
                conditionExpression: jsonLookup(reportConditionEntity.getConditionExpression()),
                operator: reportCondition.getOperator() ? reportCondition.getOperator() : jsonLookup(),
                conditionParameter: reportCondition.getConditionParameter() ?  reportCondition.getConditionParameter() : jsonLookup()
            });


            newReportConditionEntity.setId(spEntity._getNextId());
            newReportConditionEntity.setDataState(spEntity.DataStateEnum.Create);


            var cloneReportConditionEntity = spReportEntity.cloneReportEntity(newReportConditionEntity);

            return new spReportEntity.ReportCondition(cloneReportConditionEntity);
        };


        /**
          * remove report condition 
          * @function
          * @name spReportEntity.Query#removeReportCondition
          */
        Query.prototype.removeReportCondition = function(reportCondition) {
            this.getEntity().getHasConditions().deleteEntity(reportCondition.getEntity());
            this.updateReportConditions();
        };

               
        /**
           * remove report condition by source node id
           * @function
           * @name spReportEntity.Query#removeReportConditionByConditionId
           */
        Query.prototype.removeReportConditionByConditionId = function (conditionId) {

            var currentReportCondition = _.find(this.getEntity().getHasConditions(), function (reportCondition) {
                return reportCondition.id().toString() === conditionId.toString();
            });
            
            if (currentReportCondition) {
                this.getEntity().getHasConditions().deleteEntity(currentReportCondition);
                this.updateReportConditions();
            }
                                        
        };


        
        /**
          * Add a report orderby to ReportOrderBys array
          * @function
          * @name spReportEntity.Query#addReportOrderBy
          */
        Query.prototype.addReportOrderBy = function (reportOrderBy) {

            this.getEntity().getReportOrderBys().add(reportOrderBy.getEntity());
            this.updateReportOrderBys();
        };

        /**
         * remove report orderby
         * @function
         * @name spReportEntity.Query#removeReportOrderBy
         */
        Query.prototype.removeReportOrderBy = function (reportOrderBy) {
            this.getEntity().getReportOrderBys().deleteEntity(reportOrderBy.getEntity());
            this.updateReportOrderBys();
        };
        
        /**
         * Get the reportColumns' columnGroupings  for this report.
         *
         * @returns [{object}] array The report columns' columnGroupings.
         *
         * @function
         * @name spReportEntity.Query#getReportColumnGroupings
         */
        Query.prototype.getReportColumnGroupings = function getReportColumnGroupings() {
            if (!this._reportColumns) {
                this.updateReportColumns();
            }
            var reportColumnGRoupings = [];
            _.forEach(this._reportColumns, function (col) {
                _.forEach(col.getColumnGrouping(), function(grouping) {
                    reportColumnGRoupings.push(grouping);
                });
            });

            return reportColumnGRoupings;
        };
        
        /**
         * Add a report columngrouping to into related reportColumn
         * @function
         * @name spReportEntity.Query#addReportColumnGrouping
         */
        Query.prototype.addReportColumnGrouping = function (reportColumnId, columnGrouping) {

            var currentReportColumn = _.find(this.getReportColumns(), function (reportColumn) {
                return reportColumn.id().toString() === reportColumnId.toString();
            });
            
            if (currentReportColumn) {
                currentReportColumn.addColumnGrouping(columnGrouping);
                this.updateReportColumns();
            }
            
        };
        
        /**
         * remove a report columngrouping to from related reportColumn
         * @function
         * @name spReportEntity.Query#removeReportColumnGrouping
         */
        Query.prototype.removeReportColumnGrouping = function(reportColumnId) {
            var currentReportColumn = _.find(this.getReportColumns(), function(reportColumn) {
                return reportColumn.id().toString() === reportColumnId.toString();
            });

            if (currentReportColumn) {
                currentReportColumn.removeColumnGrouping();
                this.updateReportColumns();
            }
        };

        /**
         * remove a report columngrouping to from related reportColumn
         * @function
         * @name spReportEntity.Query#removeReportColumnGroupingByGrouping
         */
        Query.prototype.removeReportColumnGroupingByGrouping = function (reportColumnId, columnGrouping) {

            var currentReportColumn = _.find(this.getReportColumns(), function (reportColumn) {
                return reportColumn.id().toString() === reportColumnId.toString();
            });

            if (currentReportColumn) {
                currentReportColumn.removeColumnGrouping(columnGrouping);
                this.updateReportColumns();
            }

        };

        /**
         * Get the reportColumns' columnRollups  for this report.
         *
         * @returns [{object}] array The report columns' columnRollups.
         *
         * @function
         * @name spReportEntity.Query#getReportColumnRollups
         */
        Query.prototype.getReportColumnRollups = function getReportColumnRollups() {
            if (!this._reportColumns) {
                this.updateReportColumns();
            }
            var reportColumnRollups = [];
            _.forEach(this._reportColumns, function (col) {
                _.forEach(col.getColumnRollup(), function (rollup) {
                    reportColumnRollups.push(rollup);
                });
            });
        };

        /**
        * Add a report columnRollups to into related reportColumn
        * @function
        * @name spReportEntity.Query#addReportColumnRollup
        */
        Query.prototype.addReportColumnRollup = function (reportColumnId, columnRollup) {

            var currentReportColumn = _.find(this.getReportColumns(), function (reportColumn) {
                return reportColumn.id().toString() === reportColumnId.toString();
            });

            if (currentReportColumn) {
                currentReportColumn.addColumnRollup(columnRollup);
                this.updateReportColumns();
            }

        };

        /**
         * remove a report columnRollups to from related reportColumn
         * @function
         * @name spReportEntity.Query#removeReportColumnRollup
         */
        Query.prototype.removeReportColumnRollup = function (reportColumnId, columnRollup) {

            var currentReportColumn = _.find(this.getReportColumns(), function (reportColumn) {
                return reportColumn.id().toString() === reportColumnId.toString();
            });

            if (currentReportColumn) {
                currentReportColumn.removeColumnRollup(columnRollup);
                this.updateReportColumns();
            }

        };

        return Query;
    })();
    
    spReportEntity.Query = Query;

})(spReportEntity || (spReportEntity = {}));