// Copyright 2011-2016 Global Software Innovation Pty Ltd
var spReportEntity;

(function (spReportEntity) {

    /**
     * Expression constructor. Do not use.
     * @private
     * @class
     * @name spReportEntity.Expression
     *
     * @classdesc
     * Represents a wrapped Expression as it should appear when viewing schema details for a report.     
     */
    var Expression = (function () {

        function Expression(expressionEntity) {
            if (!expressionEntity)
                throw new Error('expressionEntity is required');
            this._expressionEntity = expressionEntity;
        }


        /**
        * Returns the entity for this expression.
        *
        * @returns {spEntity.Entity} The field entity.
        *
        * @function
        * @name spReportEntity.Expression#getEntity
        */
        Expression.prototype.getEntity = function () {
            return this._expressionEntity;
        };
        
        /**
         * Returns the entity id of the expression.
         *
         * @returns long The id of this expression.
         *
         * @function
         * @name spReportEntity.Expression#id
         */
        Expression.prototype.id = function () {
            return this._expressionEntity.id();
        };

        /**
         * Returns the type array of the expression.
         *
         * @returns [{object}] The type array of this expression.
         *
         * @function
         * @name spReportEntity.Expression#getTypes
         */
        Expression.prototype.getType = function () {

            if (this._expressionEntity.getIsOfType && this._expressionEntity.getIsOfType()) {
                return this._expressionEntity.getIsOfType();
            }
            else if (this._expressionEntity.getType && this._expressionEntity.getType()) {
                return this._expressionEntity.getType();
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
         * @name spReportEntity.Expression#getTypeAlias
         */
        Expression.prototype.getTypeAlias = function () {

            if (this._expressionEntity.getIsOfType && this._expressionEntity.getIsOfType()) {
                return this._expressionEntity.getIsOfType()[0].alias();
            }
            else if (this._expressionEntity.getType && this._expressionEntity.getType()) {
                return this._expressionEntity.getType().alias();
            }
            else {
                return null;
            }
        };

        /**
        * Returns the resulttype of the expression.
        *
        * @returns [{object}] The type array of this expression.
        *
        * @function
        * @name spReportEntity.Expression#getTypes
        */
        Expression.prototype.getReportExpressionResultType = function() {
            return this._expressionEntity.getReportExpressionResultType();
        };

        /**
        * Returns the source node of the expression.
        *
        * @returns {object} The source node of this expression.
        *
        * @function
        * @name spReportEntity.Expression#getSourceNode
        */
        Expression.prototype.getSourceNode = function () {
            if (this.getTypeAlias() !== 'core:structureViewExpression') {
                return this._expressionEntity.getSourceNode();
            } else {
                return this._expressionEntity.structureViewExpressionSourceNode;
            }            
        };

        Expression.prototype.getEntityTypeId = function() {
            var conditionExpression;
            if (this.getTypeAlias() === 'core:aggregateExpression') {
                conditionExpression = this.getAggregatedExpression();
            } else {
                conditionExpression = this;
            }

            if (conditionExpression) {
                try {
                    return conditionExpression.getSourceNode().getResourceReportNodeType().id();
                } catch (e) {
                    return 0;
                }
            } else {
                return 0;
            }
        };

        /**
        * Returns the field of the expression.
        *
        * @returns {object} The field of this expression.
        *
        * @function
        * @name spReportEntity.Expression#getField
        */
        Expression.prototype.getField = function () {
            if (this._expressionEntity.getFieldExpressionField) {
                return this._expressionEntity.getFieldExpressionField();
            } else {
                return null;
            }
        };
        
        /**
      * Returns the References Field of the expression.
      *
      * @returns {object} The References Field of this expression.
      *
      * @function
      * @name spReportEntity.Expression#getReferencesColumn
      */
        Expression.prototype.getReferencesColumn = function () {
            if (this._expressionEntity.getExpressionReferencesColumn) {
                return this._expressionEntity.getExpressionReferencesColumn();
            } else {
                return null;
            }
        };
        
        /**
         * Returns the Report Script of the expression.
         *
         * @returns {string} The Report Script  of this expression.
         *
         * @function
         * @name spReportEntity.Expression#getReportScript
         */
        Expression.prototype.getReportScript = function() {
            return this._expressionEntity.getReportScript();
        };
        
        /**
         * Returns the Report Aggregate Method of the expression.
         *
         * @returns {object} The Aggregate Method  of this expression.
         *
         * @function
         * @name spReportEntity.Expression#getAggregateMethod
         */
        Expression.prototype.getAggregateMethod = function() {
            return this._expressionEntity.getAggregateMethod();
        };

        /**
         * Returns the Aggregated Expression of the expression.
         *
         * @returns {object} The Aggregated Expression of this expression.
         *
         * @function
         * @name spReportEntity.Expression#getAggregatedExpression
         */
        Expression.prototype.getAggregatedExpression = function () {
            if (this._expressionEntity.getAggregatedExpression && this._expressionEntity.getAggregatedExpression()) {
                return new spReportEntity.Expression(this._expressionEntity.getAggregatedExpression());
            }
            return null;
        };
        
        return Expression;
    })();



    spReportEntity.Expression = Expression;

})(spReportEntity || (spReportEntity = {}));