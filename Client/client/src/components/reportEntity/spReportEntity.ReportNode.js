// Copyright 2011-2016 Global Software Innovation Pty Ltd
var spReportEntity;

(function (spReportEntity) {

    /**
     * ReportNode constructor. Do not use.
     * @private
     * @class
     * @name spReportEntity.ReportNode
     *
     * @classdesc
     * Represents a wrapped ReportNode as it should appear when viewing schema details for a report.     
     */
    var ReportNode = (function () {

        function ReportNode(reportNodeEntity) {
            if (!reportNodeEntity)
                throw new Error('reportNodeEntity is required');
            this._reportNodeEntity = reportNodeEntity;
        }

        /**
       * Returns the entity for this reportNode.
       *
       * @returns {spEntity.Entity} The report node entity.
       *
       * @function
       * @name spReportEntity.ReportNode#getEntity
       */
        ReportNode.prototype.getEntity = function () {
            return this._reportNodeEntity;
        };


        /**
         * Returns the node id of the reportNode.
         *
         * @returns long The id of this report Node.
         *
         * @function
         * @name spReportEntity.ReportNode#id
         */
        ReportNode.prototype.id = function () {
            return this._reportNodeEntity.id();
        };

        /**
         * Returns the node name of the reportNode.
         *
         * @returns [{string}] The type array of this report Node.
         *
         * @function
         * @name spReportEntity.ReportNode#getTypes
         */
        ReportNode.prototype.getName = function () {
            return this._reportNodeEntity.getName();
        };


        /**
         * Returns the type array of the reportNode.
         *
         * @returns [{object}] The type array of this report Node.
         *
         * @function
         * @name spReportEntity.ReportNode#getTypes
         */
        ReportNode.prototype.getType = function () {
            if (this._reportNodeEntity.getIsOfType && this._reportNodeEntity.getIsOfType()) {
                return this._reportNodeEntity.getIsOfType();
            }
            else if (this._reportNodeEntity.getType && this._reportNodeEntity.getType()) {
                return this._reportNodeEntity.getType();
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
         * @name spReportEntity.ReportNode#getTypes
         */
        ReportNode.prototype.getTypeAlias = function () {
            if (this._reportNodeEntity.getIsOfType && this._reportNodeEntity.getIsOfType()) {
                return this._reportNodeEntity.getIsOfType()[0].alias();
            }
            else if (this._reportNodeEntity.getType && this._reportNodeEntity.getType()) {
                return this._reportNodeEntity.getType().alias();
            }
            else {
                return null;
            }
        };

        /**
         * Returns the exactType of the reportNode.
         *
         * @returns {bool} The exactType of this report Node.
         *
         * @function
         * @name spReportEntity.ReportNode#getExactType
         */
        ReportNode.prototype.getExactType = function () {
            if (this._reportNodeEntity.getExactType) {
                return this._reportNodeEntity.getExactType();
            } else {
                return false;
            }
        };

        /**
         * Returns the targetMustExist of the reportNode.
         *
         * @returns {bool} The targetMustExist of this report Node.
         *
         * @function
         * @name spReportEntity.ReportNode#getTargetMustExist
         */
        ReportNode.prototype.getTargetMustExist = function () {
            return this._reportNodeEntity.getTargetMustExist();
        };
        
        /**
        * Returns the targetMustExist of the reportNode.
        *
        * @returns {bool} The targetMustExist of this report Node.
        *
        * @function
        * @name spReportEntity.ReportNode#getTargetMustExist
        */
        ReportNode.prototype.getTargetNeedNotExist = function () {
            if (this._reportNodeEntity.getTargetNeedNotExist) {
                return this._reportNodeEntity.getTargetNeedNotExist();
            } else {
                return false;
            }
        };

        /**
        * Returns the Resource Report NodeType of the reportNode.
        *
        * @returns {object} The NodeType of this report Node.
        *
        * @function
        * @name spReportEntity.ReportNode#getResourceReportNodeType
        */
        ReportNode.prototype.getResourceReportNodeType = function () {
            if (this.getTypeAlias() === 'core:aggregateReportNode' && this.getGroupedNode()) {
                return this.getGroupedNode().getResourceReportNodeType();
            }
            else if (this._reportNodeEntity.getResourceReportNodeType) {
                return this._reportNodeEntity.getResourceReportNodeType();
            } else {
                return false;
            }
        };

        /**
        * Returns the related report nodes array of the reportNode.
        *
        * @returns [{object}] The related report array of this report Node.
        *
        * @function
        * @name spReportEntity.ReportNode#getRelatedReportNodes
        */
        ReportNode.prototype.getRelatedReportNodes = function() {

            if (!this._relatedReportNodes) {
                this.updateRelatedReportNodes();
            }
            return this._relatedReportNodes;

        };

        ReportNode.prototype.updateRelatedReportNodes = function () {
            if (this._reportNodeEntity && this._reportNodeEntity.getRelatedReportNodes()) {
                this._relatedReportNodes = _.map(this._reportNodeEntity.getRelatedReportNodes(), function(relReportNode) {
                    return new spReportEntity.ReportNode(relReportNode);
                });
            }
        };

        /**
       * Returns the FollowInReverse of the reportNode.
       *
       * @returns {bool} The FollowInReverse of this report Node.
       *
       * @function
       * @name spReportEntity.ReportNode#GetFollowInReverse
       */
        ReportNode.prototype.getFollowInReverse = function () {
            return this._reportNodeEntity.getFollowInReverse ? this._reportNodeEntity.getFollowInReverse() : false;
        };
        
        /**
       * Returns the followRecursive of the reportNode.
       *
       * @returns {bool} The followRecursive of this report Node.
       *
       * @function
       * @name spReportEntity.ReportNode#getFollowRecursive
       */
        ReportNode.prototype.getFollowRecursive = function () {
            return this._reportNodeEntity.getFollowRecursive ? this._reportNodeEntity.getFollowRecursive() : false;
        };


        /**
         * Returns the includeSelfInRecursive of the reportNode.
         *
         * @returns {bool} The includeSelfInRecursive of this report Node.
         *
         * @function
         * @name spReportEntity.ReportNode#getTargetMustExist
         */
        ReportNode.prototype.getIncludeSelfInRecursive = function () {
            return this._reportNodeEntity.getIncludeSelfInRecursive ? this._reportNodeEntity.getIncludeSelfInRecursive() : false;
        };
        
        /**
         * Returns the constrainParent of the reportNode.
         *
         * @returns {bool} The constrainParent of this report Node.
         *
         * @function
         * @name spReportEntity.ReportNode#getConstrainParent
         */
        ReportNode.prototype.getConstrainParent = function () {
            return this._reportNodeEntity.getConstrainParent ? this._reportNodeEntity.getConstrainParent() : false;
        };
        
        /**
         * Returns the checkExistenceOnly of the reportNode.
         *
         * @returns {bool} The checkExistenceOnly of this report Node.
         *
         * @function
         * @name spReportEntity.ReportNode#getCheckExistenceOnly
         */
        ReportNode.prototype.getCheckExistenceOnly = function () {
            return this._reportNodeEntity.getCheckExistenceOnl ? this._reportNodeEntity.getCheckExistenceOnly() : false;
        };

        /**
         * Returns the getFollowRelationship of the reportNode.
         *
         * @returns {object} The getFollowRelationship of this report Node.
         *
         * @function
         * @name spReportEntity.ReportNode#getFollowRelationship
         */
        ReportNode.prototype.getFollowRelationship = function() {
            
            if (this._reportNodeEntity.getFollowRelationship()) {                
                var relationship = new spResource.Relationship(this._reportNodeEntity.getFollowRelationship(), this._reportNodeEntity.getResourceReportNodeType(), this._reportNodeEntity.getFollowInReverse());
                return relationship;
            }
            return null;
        };
        
        /**
        * Returns the getFollowRelationship of the reportNode.
        *
        * @returns {object} The getFollowRelationship of this report Node.
        *
        * @function
        * @name spReportEntity.ReportNode#getFollowRelationship
        */
        ReportNode.prototype.getFollowRelationshipEntity = function () {

            return this._reportNodeEntity.getFollowRelationship();
        };

        /**
       * Returns the getGroupedNode of the reportNode.
       *
       * @returns {object} The getGroupedNode of this report Node.
       *
       * @function
       * @name spReportEntity.ReportNode#getGroupedNode
       */
        ReportNode.prototype.getGroupedNode = function() {          
            if (this._reportNodeEntity.getGroupedNode && this._reportNodeEntity.getGroupedNode()) {
                return new spReportEntity.ReportNode(this._reportNodeEntity.getGroupedNode());
            } else {
                return null;
            }
        };

        /**
      * Returns the grouped by expression of the reportNode.
      *
      * @returns {object} The getGroupedNode of this report Node.
      *
      * @function
      * @name spReportEntity.ReportNode#getGroupedBy
      */
        ReportNode.prototype.getGroupedBy = function() {

            if (this._reportNodeEntity.getGroupedBy()) {

                return _.map(this._reportNodeEntity.getGroupedBy(),
                    function(groupedBy) {
                        if (groupedBy)
                            return new spReportEntity.Expression(groupedBy);
                        else
                            return null;

                    }
                );
            } else {
                return null;
            }
        };

        /**
           * remove the parentAggregatedNode of the reportNode.
           *
           * @function
           * @name spReportEntity.ReportNode#removeParentAggregatedNode
           */
        ReportNode.prototype.removeParentAggregatedNode = function() {
            if (this._reportNodeEntity.parentAggregatedNode) {
                //get parentAggregatedNode relationship and set deleteExisting to true
                this._reportNodeEntity.getRelationship('parentAggregatedNode').deleteExisting();
            }
        };


        /**
        * Add a related report node to current reportnode's relatedReportNodes array
        * @function
        * @name spReportEntity.ReportNode#addRelatedReportNode
        */
        ReportNode.prototype.addRelatedReportNode = function(relatedReportNode) {
           
            this._reportNodeEntity.getRelatedReportNodes().add(relatedReportNode.getEntity());           
            this.updateRelatedReportNodes();
        };
        
        /**
       * Add a related report node to current reportnode's relatedReportNodes array
       * @function
       * @name spReportEntity.ReportNode#addRelatedReportNode
       */
        ReportNode.prototype.reomveRelatedReportNode = function (relatedReportNode) {

            this._reportNodeEntity.getRelatedReportNodes().remove(relatedReportNode.getEntity());
            this.updateRelatedReportNodes();
        };

        /**
       * Reomve related report node from current reportnode's relatedReportNodes array
       * @function
       * @name spReportEntity.ReportNode#removeRelatedReportNode
       */
        ReportNode.prototype.removeRelatedReportNodeById = function (reportNodeId) {
            //detect current node is aggregated node or not, if yes, get current groupedNode's related report nodes.
            var underAggregatedNode = this.getGroupedNode && this.getGroupedNode();
            var relatedReportNodes = underAggregatedNode ? this.getGroupedNode().getRelatedReportNodes() : this.getRelatedReportNodes();

            //find relatedReportNode which match the reportNodeId
            var relatedReportNode = _.find(relatedReportNodes, function (reportNode) {
                return reportNode.id() === reportNodeId;
            });

            if (relatedReportNode) {
                //remove current relatedReportNode from nodes list, if is aggregated node, remove from current groupedNode
                if (underAggregatedNode) {
                    this._reportNodeEntity.getGroupedNode().getRelatedReportNodes().remove(relatedReportNode.getEntity());
                } else {
                    this._reportNodeEntity.getRelatedReportNodes().remove(relatedReportNode.getEntity());
                }
                this.updateRelatedReportNodes();
            }                      
        };

        /**
        * set aggregatereportnode to replace existing related reportNode
        * @function
        * @name spReportEntity.ReportNode#setSummariseReportNode
        */
        ReportNode.prototype.setSummariseReportNode = function (relatedNode, aggregateNode) {
            var relatedReportNodes = this._reportNodeEntity.getRelatedReportNodes();            
            relatedReportNodes.remove(relatedNode);
            relatedReportNodes.add(aggregateNode);
            this.updateRelatedReportNodes();
        };


        /**
        * remove aggregatereportnode to replace existing related reportNode
        * @function
        * @name spReportEntity.ReportNode#removeSummariseReportNode
        */
        ReportNode.prototype.removeSummariseReportNode = function (aggregateNode) {
            var relatedReportNodes = this._reportNodeEntity.getRelatedReportNodes();

            var groupedNode = aggregateNode.getGroupedNode();
            
            if (groupedNode) {
                if (this._reportNodeEntity.groupedNode) {
                    this._reportNodeEntity.groupedNode = null;
                }
                groupedNode.removeParentAggregatedNode();
                relatedReportNodes.remove(aggregateNode.getEntity());                
                relatedReportNodes.add(groupedNode.getEntity());
                this.updateRelatedReportNodes();
            }                                   
        };

        return ReportNode;
    })();

    spReportEntity.ReportNode = ReportNode;

})(spReportEntity || (spReportEntity = {}));