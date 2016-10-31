// Copyright 2011-2016 Global Software Innovation Pty Ltd
/* globals spReportEntity, spReportEntityQueryManager */

describe('Report Entity Model|spReportEntity|intg:', function () {
    'use strict';

    var $injector, $http, $rootScope, $q,
       spWebService, webApiRoot, spEntityService,  headers;

  
    beforeEach(module('sp.common.loginService'));
    beforeEach(module('mod.common.spEntityService'));

    beforeEach(inject(function ($injector) {
        TestSupport.setupIntgTests(this, $injector);
    }));

    var getTestReportEntity, getEmptyReportEntity;

    beforeEach(inject(function ($injector) {

        // Use var e = getTestReportEntity() for any mutable tests
        getTestReportEntity = function (testReportEntityAlias) {
            var rq = spReportEntity.makeReportRequest();
            return spEntityService.getEntity(testReportEntityAlias || 'test:rpt_StandardSimpleTypes', rq);
        };

        getEmptyReportEntity = function () {
            return getTestReportEntity('test:rpt_StandardSimpleTypes');
        };
    }));

    var reportEntity, emptyReportEntity;
    var reportEntityQuery;
    
    beforeEach(inject(function($injector) {
        $http = $injector.get('$http');
        $rootScope = $injector.get('$rootScope');
        spWebService = $injector.get('spWebService');
        spEntityService = $injector.get('spEntityService');
       
        
        var result1 = {};
        TestSupport.waitCheckReturn($rootScope, getTestReportEntity(), result1);
        runs(function() {
            reportEntity = result1.value;
            reportEntityQuery = new spReportEntity.Query(reportEntity);
        });
        
    }));
    
    describe('spReportEntity', function () {

        // Introduction Tests

        it('should exist', function () {
            expect(spReportEntity).toBeTruthy();
        });

        it('test data ready', function () {
            expect(reportEntity).toBeTruthy();
           // expect(emptyReportEntity).toBeTruthy();
        });
        
        it('test report entity ready', function () {
            expect(reportEntityQuery).toBeTruthy();
            // expect(emptyReportEntity).toBeTruthy();
        });
    });

    describe('spReportEntity.Query', function() {
        it('should exist', function () {
            expect(spReportEntity.Query).toBeTruthy();
        });
    });

    describe('spReportEntity.ReportNode', function () {
        it('should exist', function () {
            expect(spReportEntity.ReportNode).toBeTruthy();
        });
        
        it('testReportNode should exist', function () {
            expect(reportEntityQuery.getRootNode).toBeTruthy();
        });
        
        it('testReportNode prototype properties should exist', function () {
            var testReportNode = reportEntityQuery.getRootNode();
            expect(testReportNode.getEntity).toBeTruthy();
            expect(testReportNode.id).toBeTruthy();
            expect(testReportNode.getName).toBeTruthy();
            expect(testReportNode.getType).toBeTruthy();
            expect(testReportNode.getTypeAlias).toBeTruthy();
            expect(testReportNode.getExactType).toBeTruthy();
            expect(testReportNode.getTargetMustExist).toBeTruthy();
            expect(testReportNode.getTargetNeedNotExist).toBeTruthy();
            expect(testReportNode.getResourceReportNodeType).toBeTruthy();
            expect(testReportNode.getRelatedReportNodes).toBeTruthy();
            expect(testReportNode.updateRelatedReportNodes).toBeTruthy();
            expect(testReportNode.removeParentAggregatedNode).toBeTruthy();
            expect(testReportNode.getFollowInReverse).toBeTruthy();
            expect(testReportNode.getFollowRecursive).toBeTruthy();
            expect(testReportNode.getIncludeSelfInRecursive).toBeTruthy();
            expect(testReportNode.getConstrainParent).toBeTruthy();
            expect(testReportNode.getCheckExistenceOnly).toBeTruthy();
            expect(testReportNode.getFollowRelationship).toBeTruthy();
            expect(testReportNode.getFollowRelationshipEntity).toBeTruthy();
            expect(testReportNode.getGroupedNode).toBeTruthy();
            expect(testReportNode.getGroupedBy).toBeTruthy();
            expect(testReportNode.addRelatedReportNode).toBeTruthy();
            expect(testReportNode.reomveRelatedReportNode).toBeTruthy();
            expect(testReportNode.removeRelatedReportNodeById).toBeTruthy();
            expect(testReportNode.setSummariseReportNode).toBeTruthy();
            expect(testReportNode.removeSummariseReportNode).toBeTruthy();
        });
    });
    
    describe('spReportEntity.ReportColumn', function () {
        it('should exist', function () {
            expect(spReportEntity.ReportColumn).toBeTruthy();
        });
        
        it('testReportColumn should exist', function () {
            expect(reportEntityQuery.getReportColumns).toBeTruthy();
            expect(reportEntityQuery.getReportColumns()).toBeTruthy();
            expect(reportEntityQuery.getReportColumns().length > 0).toBeTruthy();
        });

        it('testReportColumn prototype properties should exist', function () {
            var testReportColumn = reportEntityQuery.getReportColumns()[0];
            expect(testReportColumn.getEntity).toBeTruthy();
            expect(testReportColumn.id).toBeTruthy();
            expect(testReportColumn.getName).toBeTruthy();
            expect(testReportColumn.isHidden).toBeTruthy();
            expect(testReportColumn.displayOrder).toBeTruthy();
            expect(testReportColumn.getExpression).toBeTruthy();
            expect(testReportColumn.getColumnFormattingRule).toBeTruthy();
            expect(testReportColumn.setColumnFormattingRule).toBeTruthy();
            expect(testReportColumn.getColumnDisplayFormat).toBeTruthy();
            expect(testReportColumn.setColumnDisplayFormat).toBeTruthy();
            expect(testReportColumn.getColumnGroupingByGroupingPriority).toBeTruthy();
            expect(testReportColumn.getColumnGroupingByGroupingMethod).toBeTruthy();
            expect(testReportColumn.addColumnGrouping).toBeTruthy();
            expect(testReportColumn.removeColumnGroupingByGrouping).toBeTruthy();
            expect(testReportColumn.removeColumnGrouping).toBeTruthy();
            expect(testReportColumn.getColumnGrouping).toBeTruthy();
            expect(testReportColumn.getColumnRollup).toBeTruthy();
            expect(testReportColumn.getColumnRollupByRollupMethod).toBeTruthy();
            expect(testReportColumn.addColumnRollup).toBeTruthy();
            expect(testReportColumn.removeColumnRollup).toBeTruthy();            
        });
    });
    
    describe('spReportEntity.ReportCondition', function () {
        it('should exist', function () {
            expect(spReportEntity.ReportCondition).toBeTruthy();
        });
        
        it('testReportConditions should exist', function () {
            expect(reportEntityQuery.getReportConditions).toBeTruthy();
            expect(reportEntityQuery.getReportConditions()).toBeTruthy();
            expect(reportEntityQuery.getReportConditions().length > 0).toBeTruthy();
        });
        
        it('testReportCondition prototype properties should exist', function () {
            var testReportCondition = reportEntityQuery.getReportConditions()[0];
            expect(testReportCondition.getEntity).toBeTruthy();
            expect(testReportCondition.id).toBeTruthy();
            expect(testReportCondition.getName).toBeTruthy();
            expect(testReportCondition.isHidden).toBeTruthy();
            expect(testReportCondition.isLocked).toBeTruthy();
            expect(testReportCondition.displayOrder).toBeTruthy();
            expect(testReportCondition.getExpression).toBeTruthy();
            expect(testReportCondition.getOperator).toBeTruthy();
            expect(testReportCondition.reset).toBeTruthy();
            expect(testReportCondition.apply).toBeTruthy();
            expect(testReportCondition.getConditionParameter).toBeTruthy();                        
        });
    });
    
    describe('spReportEntity.ReportOrderBy', function () {
        it('should exist', function () {
            expect(spReportEntity.ReportOrderBy).toBeTruthy();
        });
    });
    
    describe('spReportEntity.Expression', function () {
        it('should exist', function () {
            expect(spReportEntity.Expression).toBeTruthy();
        });
        
        it('testExpression should exist', function () {
            expect(reportEntityQuery.getReportColumns).toBeTruthy();
            expect(reportEntityQuery.getReportColumns()).toBeTruthy();
            expect(reportEntityQuery.getReportColumns().length > 1).toBeTruthy();
            expect(reportEntityQuery.getReportColumns()[1].getExpression).toBeTruthy();
        });

        it('testExpression prototype properties should exist', function() {
            var testExpression = reportEntityQuery.getReportColumns()[1].getExpression();
            expect(testExpression.getEntity).toBeTruthy();
            expect(testExpression.id).toBeTruthy();
            expect(testExpression.getType).toBeTruthy();
            expect(testExpression.getTypeAlias).toBeTruthy();
            expect(testExpression.getReportExpressionResultType).toBeTruthy();
            expect(testExpression.getSourceNode).toBeTruthy();
            expect(testExpression.getEntityTypeId).toBeTruthy();
            expect(testExpression.getField).toBeTruthy();
            expect(testExpression.getReferencesColumn).toBeTruthy();
            expect(testExpression.getReportScript).toBeTruthy();
            expect(testExpression.getAggregateMethod).toBeTruthy();
            expect(testExpression.getAggregatedExpression).toBeTruthy();            
        });
    });
    
    describe('spReportEntity.ColumnDisplayFormat', function () {
        it('should exist', function () {
            expect(spReportEntity.ColumnDisplayFormat).toBeTruthy();
        });
    });
    
    describe('spReportEntity.ColumnFormattingRule', function () {
        it('should exist', function () {
            expect(spReportEntity.ColumnFormattingRule).toBeTruthy();
        });
    });

    describe('packageReportEntityNugget', function () {
        it('can pass report entity to the server and return report results', function () {

          

            var requestOptions = {
                startIndex: 0,
                pageSize: 200,
                metadata: 'full'
            };

            var metadata = null;
            var page = null;

            if (requestOptions) {
                if (requestOptions.metadata) {
                    metadata = requestOptions.metadata;
                }

                if (angular.isDefined(requestOptions.startIndex) &&
                    angular.isDefined(requestOptions.pageSize)) {
                    page = requestOptions.startIndex + ',' + requestOptions.pageSize;
                }
            }

         
            var reportData = spEntityService.packageEntityNugget(reportEntity);


            var worker = $http({
                method: 'POST',
                url: spWebService.getWebApiRoot() + '/spapi/data/v1/report/builder/entity/0',
                data: reportData,
                params: {
                    metadata: metadata,
                    page: page
                },
                headers: spWebService.getHeaders()
            }).then(function (response) {
                var data = response.data;
                return data;
            });

            var result = {};
            TestSupport.waitCheckReturn($rootScope, worker, result);
            runs(function () {
                var data = result.value;
                expect(data).toBeTruthy();
                expect(data.meta).toBeTruthy();
            });

        });




        it('can pass report entity to the server', function () {
            
            var reportData = spEntityService.packageEntityNugget(reportEntity);

            var postData = {
                myOtherData: 'report',
                myEntityData: reportData
            };

            var worker = $http({
                method: 'POST',
                url: spWebService.getWebApiRoot() + '/spapi/data/v1/test/entitytest/report',
                data: postData,
                headers: spWebService.getHeaders()
            }).then(function (response) {
                var data = response.data;
                return data;
            });

            var result = {};
            TestSupport.waitCheckReturn($rootScope, worker, result);
            runs(function () {
                var data = result.value;
                expect(data).toBeTruthy();
                expect(data.myOtherData).toBe('report');
                expect(data.success).toBeTruthy();
            });

        });
        
        xit('can pass update report - update report column order to the server', function () {
            if (reportEntity.getReportColumns() && reportEntity.getReportColumns().length > 0) {
                reportEntity.getReportColumns()[reportEntity.getReportColumns().length - 1].columnDisplayOrder = 100;
            }
            var tempReportEntity = spReportEntity.cloneReportEntityNugget(reportEntity);
            var reportData = spEntityService.packageEntityNugget(tempReportEntity);

            var postData = {
                myOtherData: 'report',
                myEntityData: reportData
            };

            var worker = $http({
                method: 'POST',
                url: spWebService.getWebApiRoot() + '/spapi/data/v1/test/entitytest/updatereportcolumnorder',
                data: postData,
                headers: spWebService.getHeaders()
            }).then(function (response) {
                var data = response.data;
                return data;
            });

            var result = {};
            TestSupport.waitCheckReturn($rootScope, worker, result);
            runs(function () {
                var data = result.value;
                expect(data).toBeTruthy();
                expect(data.myOtherData).toBe('report');               
                expect(data.success).toBeTruthy();
            });

        });
                
        it('can pass update report - add related report node to the server', function () {

            var relatedReportNode = spEntity.fromJSON({
                name: 'Test Entity',
                typeId: 'core:relationshipReportNode'
            });
            if (reportEntity.getRootNode() && reportEntity.getRootNode().getRelatedReportNodes()) {
                reportEntity.getRootNode().getRelatedReportNodes().add(relatedReportNode);
            }           
            var tempReportEntity = spReportEntity.cloneReportEntity(reportEntity);
            var reportData = spEntityService.packageEntityNugget(tempReportEntity);

            var postData = {
                myOtherData: 'report',
                myEntityData: reportData
            };

            var worker = $http({
                method: 'POST',
                url: spWebService.getWebApiRoot() + '/spapi/data/v1/test/entitytest/updatereportrelatednode',
                data: postData,
                headers: spWebService.getHeaders()
            }).then(function (response) {
                var data = response.data;
                return data;
            });

            var result = {};
            TestSupport.waitCheckReturn($rootScope, worker, result);
            runs(function () {
                var data = result.value;
                expect(data).toBeTruthy();
                expect(data.myOtherData).toBe('report');
                expect(data.success).toBeTruthy();
            });

        });

        // TODO: Ignored by Anthony as part of new security model. Ran out of time trying to work out why tests fail.
        xit('can pass update report - summarise root node to the server', function () {
            var aggregateReportNode = spEntity.fromJSON({
                typeId: 'core:aggregateReportNode',
                exactType: false,
                targetMustExist: false,
                followRecursive: false,
                includeSelfInRecursive: false,
                constrainParent: false,
                checkExistenceOnly: false,
                groupedNode: jsonLookup(reportEntity.rootNode),
                groupedBy: jsonRelationship([reportEntity.getReportColumns()[1].getColumnExpression()]),
                relatedReportNodes: jsonRelationship()
            });
            
            reportEntity.rootNode = aggregateReportNode;

            var tempReportEntity = spReportEntity.cloneReportEntityNugget(reportEntity);
            var reportData = spEntityService.packageEntityNugget(tempReportEntity);

            var postData = {
                myOtherData: 'report',
                myEntityData: reportData
            };

            var worker = $http({
                method: 'POST',
                url: spWebService.getWebApiRoot() + '/spapi/data/v1/test/entitytest/summariseReportRootNode',
                data: postData,
                headers: spWebService.getHeaders()
            }).then(function (response) {
                var data = response.data;
                return data;
            });

            var result = {};
            TestSupport.waitCheckReturn($rootScope, worker, result);
            runs(function () {
                var data = result.value;
                expect(data).toBeTruthy();
                expect(data.myOtherData).toBe('report');
                expect(data.success).toBeTruthy();
            });
        });
        
        xit('can pass clone report - clone report to the server', function () {

            var lastColumn = _.find(reportEntity.getReportColumns(), { name: 'RPT_AllTypes' });

            //create a reportOrderBy with the last column
            var isReverseOrder = false;
            var orderPriority = 0;
            var orderByExpression = spEntity.fromJSON({
                typeId: 'columnReferenceExpression',
                expressionReferencesColumn: jsonLookup(lastColumn)
            });

            var reportOrderBy = spEntity.fromJSON({
                typeId: 'reportOrderBy',
                reverseOrder: isReverseOrder,
                orderPriority: orderPriority,
                orderByExpression: jsonLookup(orderByExpression)
            });

            expect(reportEntity.getReportOrderBys()).toBeArray(0);
            reportEntity.getReportOrderBys().add(reportOrderBy);

            //test 1, the column's id is same as order by referenced column's id
            expect(lastColumn.id() === _.last(reportEntity.getReportOrderBys()).orderByExpression.expressionReferencesColumn.id()).toBeTruthy();

            var cloneReportentity = spReportEntity.cloneReportEntity(reportEntity);
            cloneReportentity.name = 'test report name' + Math.floor((Math.random() * 1000) + 1);


            var clonedLastColumn = _.find(cloneReportentity.getReportColumns(), { name: 'RPT_AllTypes' });
            //test 2, the client side cloned column's id is same as the client side cloned order by referenced column's id
            expect(clonedLastColumn.id() === _.last(cloneReportentity.getReportOrderBys()).orderByExpression.expressionReferencesColumn.id()).toBeTruthy();
            expect(clonedLastColumn.eidP._getIdOrDummyId() === _.last(cloneReportentity.getReportOrderBys()).orderByExpression.expressionReferencesColumn.eidP._getIdOrDummyId()).toBeTruthy();


            var result = {};
            TestSupport.waitCheckReturn($rootScope,
                spEntityService.putEntity(cloneReportentity)
                    .then(function (id) {
                        var rq = spReportEntity.makeReportRequest();
                        return spEntityService.getEntity(id, rq);
                    }),
                result);


            runs(function () {
                var clonedNewReportEntity = result.value;
                expect(clonedNewReportEntity).toBeEntity();
                var clonedNewLastColumn = _.find(clonedNewReportEntity.getReportColumns(), { name: 'RPT_AllTypes' });
                var clonedNewLastOrderBy = _.last(clonedNewReportEntity.getReportOrderBys());
                expect(clonedNewLastColumn).toBeEntity();
                expect(clonedNewLastOrderBy).toBeEntity();
                //test 3, the server side cloned column's id is same as the server side cloned order by referenced column's id
                expect(clonedNewLastColumn.id() === clonedNewLastOrderBy.orderByExpression.expressionReferencesColumn.id()).toBeTruthy();
            });
        });
        
        xit('can pass clone report with deleted column - clone report to the server', function () {
            var lastColumn = _.find(reportEntityQuery.getReportColumns(), function(rc) { return rc.getName() === 'RPT_AllTypes'; });
            var originalReportId = reportEntity.id();

            //add the last column again in report 
            var newColumn = reportEntityQuery.cloneReportColumn(lastColumn);           
            reportEntityQuery.addReportColumn(newColumn);

            //delete the origial last column 'RPT_AllTypes'
            reportEntityQuery.removeReportColumn(lastColumn);

            var cloneReportentity = spReportEntity.cloneReportEntity(reportEntityQuery.getEntity());
            cloneReportentity.name = 'test report name' + Math.floor((Math.random() * 1000) + 1);

            var result = {};
            TestSupport.waitCheckReturn($rootScope,
                spEntityService.putEntity(cloneReportentity)
                    .then(function (id) {
                        var rq = spReportEntity.makeReportRequest();
                        return spEntityService.getEntities([id, originalReportId], rq);
                    }),
                result);


            runs(function () {
                var clonedNewReportEntity = result.value[0];
                //Test1, the reportEntity is cloned
                expect(clonedNewReportEntity).toBeEntity();
                var originalReportEntity = result.value[1];
                //Test2 the original report is not saved, the original reportcolumn still here
                var originalLastColumn = _.find(originalReportEntity.getReportColumns(), { name: 'RPT_AllTypes' });
                expect(originalLastColumn).toBeEntity();

            });
        });

        it('can add rollup in report column to the server', function () {
            var lastColumn = _.find(reportEntityQuery.getReportColumns(), function (rc) { return rc.getName() === 'RPT_AllTypes'; });
            var reportId = reportEntity.id();
            //add the rollup information to the last column report 
            reportEntityQuery.getEntity().rollupGrandTotals = true;
            
            lastColumn.addColumnRollup(spReportEntityQueryManager.createColumnRollup("aggCountWithValues"));
            lastColumn.addColumnRollup(spReportEntityQueryManager.createColumnRollup("aggCountUniqueItems"));
            lastColumn.addColumnRollup(spReportEntityQueryManager.createColumnRollup("aggCount"));

          
            //Test1, before save, check the column's columnRollup 
            expect(lastColumn.getEntity().columnRollup[0].rollupMethod.nsAlias).toBe('core:aggCountWithValues');
            expect(lastColumn.getEntity().columnRollup[1].rollupMethod.nsAlias).toBe('core:aggCountUniqueItems');
            expect(lastColumn.getEntity().columnRollup[2].rollupMethod.nsAlias).toBe('core:aggCount');
            var result = {};
            TestSupport.waitCheckReturn($rootScope,
                spEntityService.putEntity(reportEntityQuery.getEntity())
                    .then(function (id) {
                        var rq = spReportEntity.makeReportRequest();
                        return spEntityService.getEntity(reportId, rq);
                    }),
                result);


            runs(function () {
                var reportEntity =  result.value;
                //Test2, the reportEntity is saved and retrived
                expect(reportEntity).toBeEntity();
               
                var originalLastColumn = _.find(reportEntity.getReportColumns(), { name: 'RPT_AllTypes' });
                //Test3, the originalLastColumn still exists
                expect(originalLastColumn).toBeEntity();

                //Test4, after save, check the column's columnRollup
                expect(originalLastColumn.columnRollup[0].rollupMethod.nsAlias).toBe('core:aggCount');
                expect(originalLastColumn.columnRollup[1].rollupMethod.nsAlias).toBe('core:aggCountUniqueItems');
                expect(originalLastColumn.columnRollup[2].rollupMethod.nsAlias).toBe('core:aggCountWithValues');


                //reset this intg test change back.
                reportEntity.rollupGrandTotals = false;
                var columnRollups = [];

                _.forEach(originalLastColumn.columnRollup, function (columnRollup) {
                    columnRollups.push(columnRollup);
                });

                _.forEach(columnRollups, function(columnRolup) {
                    originalLastColumn.columnRollup.remove(columnRolup);
                });
                
                TestSupport.waitCheckReturn($rootScope,
                spEntityService.putEntity(reportEntity));
               
               
            });
        });

        it('can pass update report - add resouceExpression condition to the server', function () {

            var relatedReportNode = spEntity.fromJSON({
                name: 'Test Entity',
                typeId: 'core:relationshipReportNode'
            });
            var activityArgument = spEntity.fromJSON({
                typeId: 'resourceListArgument',
                name: 'InlineRelationship',
                resourceListParameterValues: jsonRelationship(),
                conformsToType: jsonLookup()
            });

            var resourceExpression = spEntity.fromJSON({
                typeId: 'resourceExpression',
                sourceNode: jsonLookup(relatedReportNode),
                fieldExpressionField: jsonLookup(),
                reportExpressionResultType: jsonLookup(activityArgument)
            });

            var reportConditionParameter = spEntity.fromJSON({
                typeId: 'parameter',
                paramTypeAndDefault: activityArgument ? jsonLookup(activityArgument) : jsonLookup()
            });

            var reportCondition = spEntity.fromJSON({
                typeId: 'reportCondition',
                name: 'test',
                conditionDisplayOrder: 0,
                conditionIsHidden: false,
                conditionIsLocked: false,
                conditionExpression: jsonLookup(resourceExpression),
                operator: jsonLookup(),
                conditionParameter: jsonLookup(reportConditionParameter)
            });

            if (reportEntity.getRootNode() && reportEntity.getRootNode().getRelatedReportNodes()) {
                reportEntity.getRootNode().getRelatedReportNodes().add(relatedReportNode);
            }

            if (reportEntity.getHasConditions()) {
                reportEntity.getHasConditions().add(reportCondition);
            }

            var tempReportEntity = spReportEntity.cloneReportEntity(reportEntity);            
            var reportData = spEntityService.packageEntityNugget(tempReportEntity);

            var postData = {
                myOtherData: 'report',
                myEntityData: reportData
            };

            var worker = $http({
                method: 'POST',
                url: spWebService.getWebApiRoot() + '/spapi/data/v1/test/entitytest/updatereportrelatednode',
                data: postData,
                headers: spWebService.getHeaders()
            }).then(function (response) {
                var data = response.data;
                return data;
            });

            var result = {};
            TestSupport.waitCheckReturn($rootScope, worker, result);
            runs(function () {
                var data = result.value;
                expect(data).toBeTruthy();
                expect(data.myOtherData).toBe('report');
                expect(data.success).toBeTruthy();
            });

        });
    });
    
    

});