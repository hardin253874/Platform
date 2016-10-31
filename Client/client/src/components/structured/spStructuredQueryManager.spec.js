// Copyright 2011-2016 Global Software Innovation Pty Ltd
/* globals spReportEntity, spReportEntityQueryManager */

describe('Internal|spReportEntityQuery Manager|spec:', function () {
    'use strict';


    var reportEntity, reportQueryEntity;
    var testReportEntity, testReportQueryEntity;
    var reportRootNode, reportColumn, reportCondition, reportOrderBy;
    var testFieldEntity, testRelationshipEntity;
    
    beforeEach(module('mockedEntityService'));

    beforeEach(inject(function ($injector) {
        TestSupport.setupUnitTests(this, $injector);
    }));

    beforeEach(inject(function (spEntityService) {

        /////
        // Mock a report.
        /////
        var json = {
            id: 1111,
            typeId: 'core:report',
            name: 'test report',
            description: 'My Test report',
            rootNode: {
                id: 1112,
                isOfType: [
                    {
                        id: 1113,
                        alias: 'core:resourceReportNode'
                    }
                ],
                resourceReportNodeType: {
                    id: 1114,
                    name: 'test Definition',
                    isOfType: [
                        {
                            id: 1115,
                            alias: 'core:definition'
                        }
                    ]
                },
                relatedReportNodes: jsonRelationship()
            },
            reportColumns: [
               {
                   id: 1211,
                   name: '_id',
                   columnIsHidden: true,
                   columnDisplayOrder: 0,
                   isOfType: [
                        {
                            id: 1212,
                            alias: 'core:reportColumn'
                        }
                   ],
                   columnExpression: {
                       id: 1213,
                       isOfType: [
                        {
                            id: 1214,
                            alias: 'core:idExpression'
                        }
                       ],
                       sourceNode: jsonLookup(1114)
                   }
               },
               {
                   id: 1221,
                   name: 'name',
                   columnIsHidden: false,
                   columnDisplayOrder: 1,
                   isOfType: [
                        {
                            id: 1212,
                            alias: 'core:reportColumn'
                        }
                   ],
                   columnExpression: {
                       id: 1223,
                       isOfType: [
                        {
                            id: 1224,
                            alias: 'core:fieldExpression'
                        }
                       ],
                       sourceNode: jsonLookup(1114),
                       fieldExpressionField: {
                           id: 1225,
                           alias: 'core:name'
                       }
                   }
               }
            ],
            hasConditions: [
                {
                    id: 1311,
                    name: 'name',
                    conditionIsHidden: false,
                    conditionDisplayOrder: 1,
                    columnForCondition: jsonLookup(1221),
                    isOfType: [
                         {
                             id: 1312,
                             alias: 'core:reportCondition'
                         }
                    ],
                    conditionExpression: {
                        id: 1313,
                        isOfType: [
                            {
                                id: 1224,
                                alias: 'core:fieldExpression'
                            }
                        ],
                        sourceNode: jsonLookup(1114),
                        fieldExpressionField: {
                            id: 1225,
                            alias: 'core:name'
                        }
                    },
                    operator: jsonLookup(),
                    conditionParameter: {
                        id: 1314,
                        paramTypeAndDefault: {
                            id: 1315,
                            isOfType: [
                                {
                                    id: 1316,
                                    alias: 'core:stringArgument'
                                }
                            ],
                            stringParameterValue: jsonString(),
                            conformsToType: jsonLookup()
                        }

                    }
                }
            ],
            reportOrderBys: [
                {
                    id: 1311,
                    isOfType: [
                         {
                             id: 1312,
                             alias: 'core:reportOrder'
                         }
                    ],
                    orderByExpression: {
                        id: 1313,
                        isOfType: [
                            {
                                id: 1314,
                                alais: 'core:columnReferenceExpression'
                            }
                        ],
                        expressionReferencesColumn: jsonLookup(1221)
                    },
                    orderPriority: jsonLookup()
                }
            ]
        };
        //spEntityService.mockGetEntityJSON(json);
        testReportEntity = spEntityService.mockGetEntityJSON(json);
        testReportQueryEntity = new spReportEntity.Query(testReportEntity);
        //json = {
        //    id: 11111,
        //    name: 'Field Name',
        //    typeId: 'core:field'
        //};
        
        //testFieldEntity =spEntityService.mockGetEntityJSON(json);
        
        //json = {
        //    id: 22222,
        //    name: 'Relationship Name',
        //    typeId: 'core:field'
        //};
        //testRelationshipEntity = spEntityService.mockGetEntityJSON(json);


    }));

    beforeEach(function () {        
        this.addMatchers(TestSupport.matchers);

        reportRootNode = spEntity.fromJSON({
            typeId: 'resourceReportNode',
            exactType: false,
            targetMustExist: false,
            resourceReportNodeType: jsonLookup('test:testEntity'),
            relatedReportNodes: []
        });

        reportColumn = spEntity.fromJSON({
            typeId: 'reportColumn',
            name: 'Column Name',
            columnDisplayOrder: 0,
            columnIsHidden: false,
            columnExpression: jsonLookup(),
            columnFormattingRule: jsonLookup(),
            columnDisplayFormat: jsonLookup()
        });
        var reportColumns = [reportColumn];

        reportCondition = spEntity.fromJSON({
            typeId: 'reportCondition',
            name: 'Column Name',
            conditionDisplayOrder: 0,
            conditionIsHidden: false,
            conditionIsLocked: false,
            columnForCondition: jsonLookup(reportColumn),
            conditionExpression: jsonLookup(),
            operator: jsonLookup(),
            conditionParameter: jsonLookup()
        });
        var reportConditions = [reportCondition];

        var orderByExpression = spEntity.fromJSON({
            typeId: 'columnReferenceExpression'
        });      
        orderByExpression.setLookup('expressionReferencesColumn', reportColumn);
        
        reportOrderBy = spEntity.fromJSON({
            typeId: 'reportOrderBy',
            reverseOrder: false,
            orderPriority: 0,
            orderByExpression: jsonLookup(orderByExpression)
        });        
        var reportOrderBys = [reportOrderBy];

        //init all test data
        reportEntity =  spEntity.fromJSON({
            typeId: 'report',
            name: jsonString('test report'),
            description: jsonString('test report description'),
            rootNode: jsonLookup(reportRootNode),
            reportColumns: jsonRelationship(reportColumns),
            hasConditions: jsonRelationship(reportConditions),
            reportOrderBys: jsonRelationship(reportOrderBys),
            resourceViewerConsoleForm: jsonLookup()
        });

        reportQueryEntity = new spReportEntity.Query(reportEntity);
    });

    

    describe('test data ready', function () {

        // Introduction Tests

        it('reportEntity should exist', function () {
            expect(reportEntity).toBeTruthy();
        });

        it('reportColumn should exist', function () {
            expect(reportColumn).toBeTruthy();
        });
        
        it('reportCondition should exist', function () {
            expect(reportCondition).toBeTruthy();
        });
        
        it('reportOrderBy should exist', function () {
            expect(reportOrderBy).toBeTruthy();
        });

        it('reportQueryEntity should exist', function () {
            expect(reportQueryEntity).toBeTruthy();
        });
        
        
        it('testReportEntity should exist', function () {
            expect(testReportEntity).toBeTruthy();
        });

        it('testReportQueryEntity should exist', function() {
            expect(testReportQueryEntity).toBeTruthy();
        });
    });

    describe('spReportEntityQueryManage', function () {
        it('spReportEntityQueryManage exists', function () {
            expect(spReportEntityQueryManager).toBeTruthy();
        });
    });
    
    describe('Add/Remove Relationship', function () {
        it('spReportEntityQueryManage addRelationship exists', function () {
            expect(spReportEntityQueryManager.addRelationship).toBeTruthy();
        });

        it('spReportEntityQueryManage addNodeToQuery exists', function () {
            expect(spReportEntityQueryManager.addNodeToQuery).toBeTruthy();
        });
        
        it('spReportEntityQueryManage reomveNodeFromQuery exists', function () {
            expect(spReportEntityQueryManager.reomveNodeFromQuery).toBeTruthy();
        });
        
        it('spReportEntityQueryManage reomveRelationship exists', function () {
            expect(spReportEntityQueryManager.reomveRelationship).toBeTruthy();
        });

        it('Add Relationship to report', function () {
            var rootNode = reportQueryEntity.getRootNode();
            var node = { "nid": 0, "name": 'Note', "etid": 3174, "pnid": 0, "panid": '', "relid": 4460, "reldir": 'Forward', "ftid": 3821, "ttid": 3174, qe: null, pe: rootNode };
            
            var totalRelatedReportNodesCountBefore = 0;
            var totalRelatedReportNodesCountAfter = 0;

            totalRelatedReportNodesCountBefore = rootNode.getRelatedReportNodes().length;

            spReportEntityQueryManager.addNodeToQuery(reportQueryEntity, node, null);
            
            totalRelatedReportNodesCountAfter = rootNode.getRelatedReportNodes().length;

            expect(node).toBeTruthy();
            expect(totalRelatedReportNodesCountAfter > totalRelatedReportNodesCountBefore).toBeTruthy();

            var column = { "fid": 1225, "fname": "Name", "svid": 0, "rid": 0, "dname": "AA_Herb", "ftype": "String", "aggm": 0, "ftid": 0, "ttid": 0 };
                   
            var newReportColumn = spReportEntityQueryManager.addColumnToReport(reportQueryEntity, column, rootNode.getRelatedReportNodes()[rootNode.getRelatedReportNodes().length - 1], -1);

            expect(newReportColumn.getEntity().columnExpression.typesP[0].nsAlias === "core:resourceExpression").toBeTruthy();
        });

        it('Remove Relationship from report', function () {

            var rootNode = reportQueryEntity.getRootNode();
            var node = { "nid": 0, "name": 'Note', "etid": 3174, "pnid": 0, "panid": '', "relid": 4460, "reldir": 'Forward', "ftid": 3821, "ttid": 3174, qe: null, pe: rootNode };

            var totalRelatedReportNodesCountBefore = 0;
            var totalRelatedReportNodesCountAfter = 0;

            totalRelatedReportNodesCountBefore = rootNode.getRelatedReportNodes().length;

            spReportEntityQueryManager.addNodeToQuery(reportQueryEntity, node, null);

            spReportEntityQueryManager.reomveNodeFromQuery(reportQueryEntity, node);

            totalRelatedReportNodesCountAfter = rootNode.getRelatedReportNodes().length;

            expect(node).toBeTruthy();
            expect(totalRelatedReportNodesCountAfter === totalRelatedReportNodesCountBefore).toBeTruthy();


        });
    });

    describe('Add/Remove Summarise', function() {
        it('spReportEntityQueryManage addAggregateNodeToQuery exists', function () {
            expect(spReportEntityQueryManager.addAggregateNodeToQuery).toBeTruthy();
        });
        
        it('spReportEntityQueryManage removeSummarise exists', function () {
            expect(spReportEntityQueryManager.removeSummarise).toBeTruthy();
        });
        
        it('spReportEntityQueryManage addAggregateNode exists', function () {
            expect(spReportEntityQueryManager.addAggregateNode).toBeTruthy();
        });

        it('spReportEntityQueryManage removeAggregateNode exists', function () {
            expect(spReportEntityQueryManager.removeSummarise).toBeTruthy();
        });
    });

    describe('Add/Remove column', function () {
        it('spReportEntityQueryManage addColumnToReport exists', function () {
            expect(spReportEntityQueryManager.addColumnToReport).toBeTruthy();
        });
        
        it('spReportEntityQueryManage removeReportColumnByColumnId exists', function () {
            expect(spReportEntityQueryManager.removeReportColumnByColumnId).toBeTruthy();
        });
        
        it('Add ResourceDataColumn column to report', function() {

            var column = { "fid": 1225, "fname": "Name", "svid": 0, "rid": 0, "dname": "Field Name", "ctype": "String", "aggm": 0, "ftid": 0, "ttid": 11111 };
            var totalColumnCountBefore = 0;
            var totalCounmnCountAfter = 0;

            var rootNode = reportQueryEntity.getRootNode();
                
            totalColumnCountBefore = reportQueryEntity.getReportColumns().length;

            spReportEntityQueryManager.addColumnToReport(reportQueryEntity, column, rootNode, -1);
                    
            totalCounmnCountAfter = reportQueryEntity.getReportColumns().length;
                    
            expect(totalCounmnCountAfter > totalColumnCountBefore).toBeTruthy(); 
            
            expect(column).toBeTruthy();
        });
        
        it('Remove ResourceDataColumn column from report', function () {

            var column = { "fid": 1225, "fname": "Name", "svid": 0, "rid": 0, "dname": "Field Name", "ctype": "String", "aggm": 0, "ftid": 0, "ttid": 11111 };
            var totalColumnCountBefore = 0;
            var totalCounmnCountAfter = 0;

            var rootNode = reportQueryEntity.getRootNode();

            totalColumnCountBefore = reportQueryEntity.getReportColumns().length;

            spReportEntityQueryManager.addColumnToReport(reportQueryEntity, column, rootNode, -1);

            var columnId = reportQueryEntity.getReportColumns()[0].id();

            spReportEntityQueryManager.removeReportColumnByColumnId(reportQueryEntity, columnId);

            totalCounmnCountAfter = reportQueryEntity.getReportColumns().length;

            expect(totalCounmnCountAfter === totalColumnCountBefore).toBeTruthy();

            expect(column).toBeTruthy();
        });     
    });

    describe('Add/Update calculated column', function() {
        it('spReportEntityQueryManage addCalculateColumnToReport exists', function () {
            expect(spReportEntityQueryManager.addCalculateColumnToReport).toBeTruthy();
        });
        
        it('spReportEntityQueryManage updateCalculateColumnToReport exists', function () {
            expect(spReportEntityQueryManager.updateCalculateColumnToReport).toBeTruthy();
        });
    });

    describe('Add/Update addAggregateColumn column', function () {
        it('spReportEntityQueryManage addAggregateColumn exists', function() {
            expect(spReportEntityQueryManager.addAggregateColumn).toBeTruthy();
        });
    });

    describe('Update column formatting', function() {
        it('spReportEntityQueryManage updateConditionFormatting exists', function () {
            expect(spReportEntityQueryManager.updateConditionFormatting).toBeTruthy();
        });
        
        it('spReportEntityQueryManage updateColumnDisplayFormat exists', function () {
            expect(spReportEntityQueryManager.updateConditionFormatting).toBeTruthy();
        });
    });
    
    describe('Add/update/reset/Remove cnodition', function () {
        it('spReportEntityQueryManage addCondition exists', function () {
            expect(spReportEntityQueryManager.addCondition).toBeTruthy();
        });

        it('spReportEntityQueryManage updateCondition exists', function () {
            expect(spReportEntityQueryManager.updateCondition).toBeTruthy();
        });
        
        it('spReportEntityQueryManage resetConditionById exists', function () {
            expect(spReportEntityQueryManager.resetConditionById).toBeTruthy();
        });
        
        it('spReportEntityQueryManage removeReportConditionByConditionId exists', function () {
            expect(spReportEntityQueryManager.removeReportConditionByConditionId).toBeTruthy();
        });

        it('Add cnodition to report', function () {
            
            var column = { "fid": 1225, "fname": "Name", "svid": 0, "rid": 0, "dname": "Field Name", "ctype": "String", "aggm": 0, "ftid": 0, "ttid": 11111 };
            var totalConditionCountBefore = 0;
            var totalConditionCountAfter = 0;

            var rootNode = reportQueryEntity.getRootNode();

            totalConditionCountBefore = reportQueryEntity.getReportConditions().length;

            spReportEntityQueryManager.addCondition(reportQueryEntity, column, rootNode, -1);

            totalConditionCountAfter = reportQueryEntity.getReportConditions().length;

            expect(column).toBeTruthy();
            expect(totalConditionCountAfter > totalConditionCountBefore).toBeTruthy();
        });
        
        it('Remove cnodition from report', function () {
            
            var column = { "fid": 1225, "fname": "Name", "svid": 0, "rid": 0, "dname": "Field Name", "ctype": "String", "aggm": 0, "ftid": 0, "ttid": 11111 };
            var totalConditionCountBefore = 0;
            var totalConditionCountAfter = 0;

            var rootNode = reportQueryEntity.getRootNode();

            totalConditionCountBefore = reportQueryEntity.getReportConditions().length;

            spReportEntityQueryManager.addCondition(reportQueryEntity, column, rootNode, -1);


            var cnoditionId = reportQueryEntity.getReportConditions()[0].id();

            spReportEntityQueryManager.removeReportConditionByConditionId(reportQueryEntity, cnoditionId);

            totalConditionCountAfter = reportQueryEntity.getReportConditions().length;

            expect(column).toBeTruthy();
            expect(totalConditionCountAfter === totalConditionCountBefore).toBeTruthy();
        });
    });
    
    describe('Add/Remove orderby', function () {
        it('spReportEntityQueryManage addOrderByToReport exists', function () {
            expect(spReportEntityQueryManager.addOrderByToReport).toBeTruthy();
        });
        
        it('spReportEntityQueryManage removeReportOrderByById exists', function () {
            expect(spReportEntityQueryManager.removeReportOrderByById).toBeTruthy();
        });

        it('Add orderby item to report', function () {
           
            var columnId = reportQueryEntity.getReportColumns()[0].id();

            var oldReverseOrder = reportQueryEntity.getReportOrderBys()[0].getReverseOrder();

            spReportEntityQueryManager.addOrderByToReport(reportQueryEntity, columnId, true, 0);
            
            var newReverseOrder = reportQueryEntity.getReportOrderBys()[0].getReverseOrder();

            expect(oldReverseOrder !== newReverseOrder).toBeTruthy();
        });

        it('Remove orderby item from report', function () {
            
            var orderById = reportQueryEntity.getReportOrderBys()[0].id();

            var totalOrderByCountBefore = 0;
            var totalOrderByCountAfter = 0;

            totalOrderByCountBefore = reportQueryEntity.getReportOrderBys().length;
            spReportEntityQueryManager.removeReportOrderByById(reportQueryEntity, orderById);
            totalOrderByCountAfter = reportQueryEntity.getReportOrderBys().length;
            expect(totalOrderByCountBefore > totalOrderByCountAfter).toBeTruthy();
        });
    });
    
    describe('Create Report', function () {
        it('spReportEntityQueryManage createRootEntity exists', function () {
            expect(spReportEntityQueryManager.createRootEntity).toBeTruthy();
        });

        it('spReportEntityQueryManage createInitializeReportColumns exists', function () {
            expect(spReportEntityQueryManager.createInitializeReportColumns).toBeTruthy();
        });

        it('spReportEntityQueryManage createInitializeReportCondition exists', function () {
            expect(spReportEntityQueryManager.createInitializeReportCondition).toBeTruthy();
        });       
    });

    describe('Structured Query Manager functions', function () {
        it('spReportEntityQueryManage createOperator', function () {
            var lastNDaysTillNow = spReportEntityQueryManager.createOperator('LastNDaysTillNow');
            var nextNDaysFromNow = spReportEntityQueryManager.createOperator('NextNDaysFromNow');
            var thisWeek = spReportEntityQueryManager.createOperator('ThisWeek');
            var lastNWeeks = spReportEntityQueryManager.createOperator('LastNWeeks');
            var nextNWeeks = spReportEntityQueryManager.createOperator('NextNWeeks');

            expect(lastNDaysTillNow.alias() === 'core:operLastNDaysTillNow').toBeTruthy();
            expect(nextNDaysFromNow.alias() === 'core:operNextNDaysFromNow').toBeTruthy();
            expect(thisWeek.alias() === 'core:operThisWeek').toBeTruthy();
            expect(lastNWeeks.alias() === 'core:operLastNWeeks').toBeTruthy();
            expect(nextNWeeks.alias() === 'core:operNextNWeeks').toBeTruthy();
        });

       
    });
   
});
