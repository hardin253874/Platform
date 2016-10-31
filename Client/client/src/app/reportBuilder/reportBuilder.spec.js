// Copyright 2011-2016 Global Software Innovation Pty Ltd
/* globals spReportEntity */

describe('Reports|report builder|spec', function () {
    
    describe('will create', function () {
        var reportBuilderPageController, reportBuilderService, $scope;
        var reportEntity, reportQueryEntity;
        var reportRootNode, reportColumn, reportCondition, reportOrderBy;
        
        beforeEach(module('app.reportBuilder'));
        
        beforeEach(inject(function ($injector) {
            TestSupport.setupUnitTests(this, $injector);
        }));

        beforeEach(inject(function ($controller, $rootScope) {

            $scope = $rootScope.$new();
            $scope.isDirty = false;
            reportBuilderPageController = $controller('reportBuilderPageController', { $scope: $scope });
           // reportBuilderService = $controller('reportBuilderService');
        }));


        beforeEach(function () {
            this.addMatchers(TestSupport.matchers);

            var nodeType = spEntity.fromJSON({
                id: 1234,
                typeId: 'core:definition',
                name: 'test Definition',
                description: 'My Test Definition.'
            });

            reportRootNode = spEntity.fromJSON({
                typeId: 'resourceReportNode',
                exactType: false,
                targetMustExist: false,
                resourceReportNodeType: jsonLookup(nodeType),
                relatedReportNodes: []
            });


            var field = spEntity.fromJSON({
                id: 2234,
                typeId: 'core:type',
                name: 'Name'
            });
            var stringType = spEntity.fromJSON({
                typeId: 'stringArgument',
                name: 'String',
                stringParameterValue: jsonString()
            });
            var expression = spEntity.fromJSON({
                typeId: 'fieldExpression',
                sourceNode: jsonLookup(reportRootNode),
                fieldExpressionField: jsonLookup(field),
                reportExpressionResultType: jsonLookup(stringType)
                }
            );


            reportColumn = spEntity.fromJSON({
                typeId: 'reportColumn',
                name: 'Column Name',
                columnDisplayOrder: 0,
                columnIsHidden: false,
                columnExpression: jsonLookup(expression),
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
            reportEntity = spEntity.fromJSON({
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
        });

        it('should pass a dummy test', inject(function () {
            expect(reportBuilderPageController).toBeTruthy();
        }));

        describe('reportBuilderService ready', function () {
            it('reportBuilderService and properties exists', inject(function(reportBuilderService) {
                expect(reportBuilderService).toBeTruthy();
                expect(reportBuilderService.setReportEntity).toBeTruthy();
                expect(reportBuilderService.setReportEntityFromReport).toBeTruthy();
                expect(reportBuilderService.noticeReportBuilder).toBeTruthy();
                expect(reportBuilderService.setActionFromReportBuilder).toBeTruthy();
                expect(reportBuilderService.getActionsFromReportBuilder).toBeTruthy();
                expect(reportBuilderService.setActionFromReport).toBeTruthy();
                expect(reportBuilderService.getActionsFromReport).toBeTruthy();
                expect(reportBuilderService.addRelationship).toBeTruthy();
                expect(reportBuilderService.addRelationships).toBeTruthy();
                expect(reportBuilderService.removeRelationship).toBeTruthy();
                expect(reportBuilderService.removeRelationships).toBeTruthy();
                expect(reportBuilderService.updateQueryEntity).toBeTruthy();
                expect(reportBuilderService.existsReportColumn).toBeTruthy();
                expect(reportBuilderService.addColumnToReport).toBeTruthy();
                expect(reportBuilderService.removeColumnFromReport).toBeTruthy();
                expect(reportBuilderService.removeColumnById).toBeTruthy();
                expect(reportBuilderService.reOrderColumnToReport).toBeTruthy();
                expect(reportBuilderService.updateColumnNameFromReport).toBeTruthy();
                expect(reportBuilderService.addCalculateColumnToReport).toBeTruthy();
                expect(reportBuilderService.updateCalculateColumnToReport).toBeTruthy();
                expect(reportBuilderService.addColumnToAnalyzer).toBeTruthy();
                expect(reportBuilderService.removeColumnFromAnalyzer).toBeTruthy();
                expect(reportBuilderService.removeAnalyzerById).toBeTruthy();
                expect(reportBuilderService.reOrderConditionToReport).toBeTruthy();
                expect(reportBuilderService.applyAnalysers).toBeTruthy();
                expect(reportBuilderService.updateReportConditions).toBeTruthy();
                expect(reportBuilderService.updateReportCondition).toBeTruthy();
                expect(reportBuilderService.resetReportConditions).toBeTruthy();
                expect(reportBuilderService.updateOrderByToReport).toBeTruthy();
                expect(reportBuilderService.addOrderByToReport).toBeTruthy();
                expect(reportBuilderService.removeOrderByFromReport).toBeTruthy();
                expect(reportBuilderService.updateReportRollupOptions).toBeTruthy();
                expect(reportBuilderService.updateSortOrderToMatchGroupingOrder).toBeTruthy();
                expect(reportBuilderService.addColumnGrouping).toBeTruthy();
                expect(reportBuilderService.removeColumnGrouping).toBeTruthy();
                expect(reportBuilderService.setReportColumnRollups).toBeTruthy();
                expect(reportBuilderService.updateConditionFormattingFromReport).toBeTruthy();
                expect(reportBuilderService.updateColumnDisplayFormat).toBeTruthy();
                expect(reportBuilderService.createSummarise).toBeTruthy();
                expect(reportBuilderService.updateAggregateColumns).toBeTruthy();
                expect(reportBuilderService.updateAggregateColumnsAfterColumn).toBeTruthy();
                expect(reportBuilderService.removeSummarise).toBeTruthy();
            }));
        });
        
        describe('Add/Remove Relationship', function () {
            it('Add Relationship to report', inject(function (reportBuilderService) {
                reportBuilderService.setReportEntity(reportQueryEntity.getEntity().id(), reportQueryEntity, null);


                var node = { "nid": 0, "name": 'Note', "etid": 3174, "pnid": 0, "panid": '', "relid": 4460, "reldir": 'Forward', "ftid": 3821, "ttid": 3174, qe: null, pe: reportBuilderService.getReportEntity().getRootNode() };

                var totalRelatedReportNodesCountBefore = 0;
                var totalRelatedReportNodesCountAfter = 0;

                totalRelatedReportNodesCountBefore = reportBuilderService.getReportEntity().getRootNode().getRelatedReportNodes().length;

                reportBuilderService.addRelationship(node, null, null);

                totalRelatedReportNodesCountAfter = reportBuilderService.getReportEntity().getRootNode().getRelatedReportNodes().length;

                expect(node).toBeTruthy();
                expect(totalRelatedReportNodesCountAfter > totalRelatedReportNodesCountBefore).toBeTruthy();
            }));          

            it('Remove Relationship from report', inject(function (reportBuilderService) {

                reportBuilderService.setReportEntity(reportQueryEntity.getEntity().id(), reportQueryEntity, null);


                var node = { "nid": 0, "name": 'Note', "etid": 3174, "pnid": 0, "panid": '', "relid": 4460, "reldir": 'Forward', "ftid": 3821, "ttid": 3174, qe: null, pe: reportBuilderService.getReportEntity().getRootNode() };

                var totalRelatedReportNodesCountBefore = 0;
                var totalRelatedReportNodesCountAfter = 0;

                totalRelatedReportNodesCountBefore = reportBuilderService.getReportEntity().getRootNode().getRelatedReportNodes().length;

                reportBuilderService.addRelationship(node, null, null);

                reportBuilderService.removeRelationship(node, null);

                totalRelatedReportNodesCountAfter = reportBuilderService.getReportEntity().getRootNode().getRelatedReportNodes().length;

                expect(node).toBeTruthy();
                expect(totalRelatedReportNodesCountAfter === totalRelatedReportNodesCountBefore).toBeTruthy();


            }));
        });

        describe('Add/Remove column', function () {
            it('Add column to report', inject(function (reportBuilderService) {
                reportBuilderService.setReportEntity(reportQueryEntity.getEntity().id(), reportQueryEntity, null);


                var column = { "fid": 1225, "fname": "Name", "svid": 0, "rid": 0, "dname": "Field Name", "ctype": "String", "aggm": 0, "ftid": 0, "ttid": 11111 };
                var totalColumnCountBefore = 0;
                var totalCounmnCountAfter = 0;

                totalColumnCountBefore = reportBuilderService.getReportEntity().getReportColumns().length;

                reportBuilderService.addColumnToReport(column, reportBuilderService.getReportEntity().getRootNode(), null);

                totalCounmnCountAfter = reportBuilderService.getReportEntity().getReportColumns().length;

                expect(column).toBeTruthy();
                expect(totalCounmnCountAfter > totalColumnCountBefore).toBeTruthy();
            }));

            it('Remove column to report', inject(function (reportBuilderService) {
                reportBuilderService.setReportEntity(reportQueryEntity.getEntity().id(), reportQueryEntity, null);


                var column = { "fid": 1225, "fname": "Name", "svid": 0, "rid": 0, "dname": "Field Name", "ctype": "String", "aggm": 0, "ftid": 0, "ttid": 11111 };
                var totalColumnCountBefore = 0;
                var totalCounmnCountAfter = 0;

                totalColumnCountBefore = reportBuilderService.getReportEntity().getReportColumns().length;

                reportBuilderService.addColumnToReport(column, reportBuilderService.getReportEntity().getRootNode(), null);
                reportBuilderService.removeColumnFromReport(column, reportBuilderService.getReportEntity().getRootNode(), null);

                totalCounmnCountAfter = reportBuilderService.getReportEntity().getReportColumns().length;

                expect(column).toBeTruthy();
                expect(totalCounmnCountAfter === totalColumnCountBefore).toBeTruthy();
            }));
            
            it('Update Column Name to report', inject(function (reportBuilderService) {
                reportBuilderService.setReportEntity(reportQueryEntity.getEntity().id(), reportQueryEntity, null);


                var column = { "fid": 1225, "fname": "Name", "svid": 0, "rid": 0, "dname": "Field Name", "ctype": "String", "aggm": 0, "ftid": 0, "ttid": 11111 };
             
                reportBuilderService.addColumnToReport(column, reportBuilderService.getReportEntity().getRootNode(), null);

                var columnId = reportBuilderService.getReportEntity().getReportColumns()[reportBuilderService.getReportEntity().getReportColumns().length - 1].id();
                reportBuilderService.updateColumnNameFromReport(columnId, 'New Name');

               
                expect(column).toBeTruthy();
                expect(reportBuilderService.getReportEntity().getReportColumns()[reportBuilderService.getReportEntity().getReportColumns().length - 1].getName() === 'New Name').toBeTruthy();
            }));
                     
            it('ReOrder Column to report', inject(function (reportBuilderService) {
                reportBuilderService.setReportEntity(reportQueryEntity.getEntity().id(), reportQueryEntity, null);
                
                var column = { "fid": 1225, "fname": "Name", "svid": 0, "rid": 0, "dname": "Field Name", "ctype": "String", "aggm": 0, "ftid": 0, "ttid": 11111 };

                reportBuilderService.addColumnToReport(column, reportBuilderService.getReportEntity().getRootNode(), null);

                var column1 = reportBuilderService.getReportEntity().getReportColumns()[0].id();
                var column1OldDisplayOrder = reportBuilderService.getReportEntity().getReportColumns()[0].displayOrder();
                var column2 = reportBuilderService.getReportEntity().getReportColumns()[reportBuilderService.getReportEntity().getReportColumns().length - 1].id();
                var column2OldDisplayOrder = reportBuilderService.getReportEntity().getReportColumns()[reportBuilderService.getReportEntity().getReportColumns().length - 1].displayOrder();

                reportBuilderService.reOrderColumnToReport(column2,column1 );
                
                var column1NewDisplayOrder = reportBuilderService.getReportEntity().getReportColumns()[0].displayOrder();
                var column2NewDisplayOrder = reportBuilderService.getReportEntity().getReportColumns()[reportBuilderService.getReportEntity().getReportColumns().length - 1].displayOrder();
                
                
                expect(column2NewDisplayOrder === column1OldDisplayOrder).toBeTruthy();
                expect(column1NewDisplayOrder === column1OldDisplayOrder + 1).toBeTruthy();
            }));
            
            it('Add Calculated Column to report', inject(function(reportBuilderService) {
                //nodeEntity, script, type, columnName, entityTypeId
                reportBuilderService.setReportEntity(reportQueryEntity.getEntity().id(), reportQueryEntity, null);

                var rootNode = reportBuilderService.getReportEntity().getRootNode();
                var script = 'test';
                var type = 'String';
                var columnName = 'Calculated';
                reportBuilderService.addCalculateColumnToReport(rootNode, script, type, columnName, null);

                var calculatedColumn = reportBuilderService.getReportEntity().getReportColumns()[reportBuilderService.getReportEntity().getReportColumns().length - 1];
                
                expect(calculatedColumn).toBeTruthy();
                expect(calculatedColumn.getName() === 'Calculated').toBeTruthy();
            }));
        });

        describe('Add/Remove condition', function () {
            it('Add condition to report', inject(function (reportBuilderService) {
                reportBuilderService.setReportEntity(reportQueryEntity.getEntity().id(), reportQueryEntity, null);


                var column = { "fid": 1225, "fname": "Name", "svid": 0, "rid": 0, "dname": "Field Name", "ctype": "String", "aggm": 0, "ftid": 0, "ttid": 11111 };
                var totalConditionCountBefore = 0;
                var totalConditionCountAfter = 0;

                totalConditionCountBefore = reportBuilderService.getReportEntity().getReportConditions().length;

                reportBuilderService.addColumnToAnalyzer(column, reportBuilderService.getReportEntity().getRootNode(), null);

                totalConditionCountAfter = reportBuilderService.getReportEntity().getReportConditions().length;

                expect(column).toBeTruthy();
                expect(totalConditionCountBefore < totalConditionCountAfter).toBeTruthy();
            }));
            
            it('Remove condition to report', inject(function (reportBuilderService) {
                reportBuilderService.setReportEntity(reportQueryEntity.getEntity().id(), reportQueryEntity, null);


                var column = { "fid": 1225, "fname": "Name", "svid": 0, "rid": 0, "dname": "Field Name", "ctype": "String", "aggm": 0, "ftid": 0, "ttid": 11111 };
                var totalConditionCountBefore = 0;
                var totalConditionCountAfter = 0;

                totalConditionCountBefore = reportBuilderService.getReportEntity().getReportConditions().length;

                reportBuilderService.addColumnToAnalyzer(column, reportBuilderService.getReportEntity().getRootNode(), null);
                reportBuilderService.removeColumnFromAnalyzer(column, reportBuilderService.getReportEntity().getRootNode());
                totalConditionCountAfter = reportBuilderService.getReportEntity().getReportConditions().length;

                expect(column).toBeTruthy();
                expect(totalConditionCountBefore === totalConditionCountAfter).toBeTruthy();
            }));

            it('Apply condition to report', inject(function (reportBuilderService) {
                reportBuilderService.setReportEntity(reportQueryEntity.getEntity().id(), reportQueryEntity, null);
                var condtionId = reportBuilderService.getReportEntity().getReportConditions()[0].id();
                var conditions = [{ argtype: "String", expid: condtionId.toString(), oper: "Contains", type: "String", value: "name" }];
                reportBuilderService.applyAnalysers(conditions);
                var condtion = reportBuilderService.getReportEntity().getReportConditions()[0];
                expect(condtion).toBeTruthy();
                expect(condtion.getOperator().alias() === 'core:operContains').toBeTruthy();
                expect(condtion.getConditionParameter().getParamTypeAndDefault().getStringParameterValue() === 'name').toBeTruthy();
            }));
        });

        describe('Add/Remove orderby', function() {
            it('Add orderby to report', inject(function (reportBuilderService) {
                reportBuilderService.setReportEntity(reportQueryEntity.getEntity().id(), reportQueryEntity, null);


                var columnId = reportBuilderService.getReportEntity().getReportColumns()[0].id();
                var oldReverseOrder = reportBuilderService.getReportEntity().getReportOrderBys()[0].getReverseOrder();

                var sortInfo = [{
                    columnId: columnId.toString(),
                    sortDirection : 'desc'
                }];
                reportBuilderService.updateOrderByToReport(sortInfo);

                var newReverseOrder = reportBuilderService.getReportEntity().getReportOrderBys()[0].getReverseOrder();
                
                expect(oldReverseOrder !== newReverseOrder).toBeTruthy();
            }));
            
            
            it('Remove orderby to report', inject(function (reportBuilderService) {
                reportBuilderService.setReportEntity(reportQueryEntity.getEntity().id(), reportQueryEntity, null);


                var totalOrderByCountBefore = 0;
                var totalOrderByCountAfter = 0;

                totalOrderByCountBefore = reportBuilderService.getReportEntity().getReportOrderBys().length;

                var columnId = reportBuilderService.getReportEntity().getReportColumns()[0].id();

                reportBuilderService.removeOrderByFromReport(columnId);
                
                totalOrderByCountAfter = reportBuilderService.getReportEntity().getReportOrderBys().length;
                expect(totalOrderByCountBefore > totalOrderByCountAfter).toBeTruthy();
            }));
        });
        
        describe('ReportColumn display format', function () {
            it('apply highlight color condition format to report column', inject(function (reportBuilderService) {
                reportBuilderService.setReportEntity(reportQueryEntity.getEntity().id(), reportQueryEntity, null);
                var columnJson = { "fid": 1225, "fname": "Name", "svid": 0, "rid": 0, "dname": "Field Name", "ctype": "String", "aggm": 0, "ftid": 0, "ttid": 11111 };

                reportBuilderService.addColumnToReport(columnJson, reportBuilderService.getReportEntity().getRootNode(), null);
                var columnId = reportBuilderService.getReportEntity().getReportColumns()[reportBuilderService.getReportEntity().getReportColumns().length - 1].id();
                var conditionFormatting = {
                    format: 'Highlight',
                    displayText: true,
                    highlightRules: [
                        {
                            color: {
                                backgroundColor: { a:255,b:153,g:153,r:255},
                                foregroundColor: { a:255,b:0,g:0,r:255}
                            },
                            operator: 'Contains',
                            type: 'String',
                            value:'test'
                        },
                        {
                            color: {
                                backgroundColor: { a: 255, b: 153, g: 255, r: 255 },
                                foregroundColor: { a: 255, b: 0, g: 0, r: 128 }
                            },
                            operator: 'Unspecified'
                        }
                    ]
                };

                reportBuilderService.updateColumnDisplayFormat(columnId.toString(), 'String', null, conditionFormatting);
                var column = reportBuilderService.getReportEntity().getReportColumns()[reportBuilderService.getReportEntity().getReportColumns().length - 1];
                expect(column).toBeTruthy();
                expect(column.getColumnFormattingRule().getColorRules().length === 2).toBeTruthy();
                expect(column.getColumnFormattingRule().getColorRules()[0].getColorRuleBackground() === 'ffff9999').toBeTruthy();
                expect(column.getColumnFormattingRule().getColorRules()[0].getColorRuleForeground() === 'ffff0000').toBeTruthy();
                expect(column.getColumnFormattingRule().getColorRules()[0].getRuleCondition().getOperator().alias() === 'core:operContains').toBeTruthy();
                expect(column.getColumnFormattingRule().getColorRules()[0].getRuleCondition().getConditionParameter().getParamTypeAndDefault().getStringParameterValue() === 'test').toBeTruthy();
            }));
            
            it('apply icon condition format to report column', inject(function (reportBuilderService) {
                reportBuilderService.setReportEntity(reportQueryEntity.getEntity().id(), reportQueryEntity, null);
                var columnJson = { "fid": 1225, "fname": "Name", "svid": 0, "rid": 0, "dname": "Field Name", "ctype": "String", "aggm": 0, "ftid": 0, "ttid": 11111 };

                reportBuilderService.addColumnToReport(columnJson, reportBuilderService.getReportEntity().getRootNode(), null);
                var columnId = reportBuilderService.getReportEntity().getReportColumns()[reportBuilderService.getReportEntity().getReportColumns().length - 1].id();
                var conditionFormatting = {
                    format: 'Icon',
                    displayText: true,
                    iconRules: [
                        {
                            imgId:2924,
                            operator: 'Contains',
                            type: 'String',
                            value: 'test'
                        },
                        {
                            imgId: 2917,
                            operator: 'Unspecified'
                        }
                    ]
                };

                reportBuilderService.updateColumnDisplayFormat(columnId.toString(), 'String', null, conditionFormatting);
                var column = reportBuilderService.getReportEntity().getReportColumns()[reportBuilderService.getReportEntity().getReportColumns().length - 1];
                expect(column).toBeTruthy();
                expect(column.getColumnFormattingRule().getIconRules().length === 2).toBeTruthy();
                expect(column.getColumnFormattingRule().getIconRules()[0].getIconRuleImage().id() === 2924).toBeTruthy();
                expect(column.getColumnFormattingRule().getIconRules()[0].getRuleCondition().getOperator().alias() === 'core:operContains').toBeTruthy();
                expect(column.getColumnFormattingRule().getIconRules()[0].getRuleCondition().getConditionParameter().getParamTypeAndDefault().getStringParameterValue() === 'test').toBeTruthy();
            }));
            
            it('apply value format to report column', inject(function(reportBuilderService) {
                reportBuilderService.setReportEntity(reportQueryEntity.getEntity().id(), reportQueryEntity, null);
                var columnJson = { "fid": 1225, "fname": "Name", "svid": 0, "rid": 0, "dname": "Field Name", "ctype": "String", "aggm": 0, "ftid": 0, "ttid": 11111 };

                reportBuilderService.addColumnToReport(columnJson, reportBuilderService.getReportEntity().getRootNode(), null);
                var columnId = reportBuilderService.getReportEntity().getReportColumns()[reportBuilderService.getReportEntity().getReportColumns().length - 1].id();
                var columnDisplayFormat = { lines: 2 };
                var conditionFormatting = { format: 'None', displayText: true };
                reportBuilderService.updateColumnDisplayFormat(columnId.toString(), 'String', columnDisplayFormat, conditionFormatting);
                var column = reportBuilderService.getReportEntity().getReportColumns()[reportBuilderService.getReportEntity().getReportColumns().length - 1];
                expect(column).toBeTruthy();
                expect(column.getColumnDisplayFormat().getColumnShowText() === true).toBeTruthy();
                expect(column.getColumnDisplayFormat().getMaxLineCount() === 2).toBeTruthy();
            }));            
        });

        describe('Summarise report', function () {
            it('add summarised node', inject(function (reportBuilderService) {
                expect(reportBuilderService.createSummarise).toBeTruthy();
                
                reportBuilderService.setReportEntity(reportQueryEntity.getEntity().id(), reportQueryEntity, null);
                var rootNode = reportQueryEntity.getRootNode();
                var node = { "nid": rootNode.id(), "name": rootNode.getResourceReportNodeType().getName(), "etid": rootNode.getResourceReportNodeType().id(), qe: rootNode, pe: null, pae:null };

                reportBuilderService.createSummarise(node, [], []);
                
                expect(rootNode).toBeTruthy();
                expect(node.pae).toBeTruthy();
                expect(reportQueryEntity.getRootNode().getTypeAlias() === 'core:aggregateReportNode').toBeTruthy();
                expect(reportQueryEntity.getRootNode().getGroupedNode()).toBeTruthy();
                expect(reportQueryEntity.getRootNode().getGroupedBy().length > 0).toBeTruthy();
                expect(reportQueryEntity.getRootNode().getGroupedBy()[0].getTypeAlias() === 'core:fieldExpression').toBeTruthy();
            }));
            
            it('update aggregate column', inject(function (reportBuilderService) {
                expect(reportBuilderService.updateAggregateColumns).toBeTruthy();
                reportBuilderService.setReportEntity(reportQueryEntity.getEntity().id(), reportQueryEntity, null);
                var rootNode = reportQueryEntity.getRootNode();
                var node = { "nid": rootNode.id(), "name": rootNode.getResourceReportNodeType().getName(), "etid": rootNode.getResourceReportNodeType().id(), qe: rootNode, pe: null, pae: null };
                var columnId = reportBuilderService.getReportEntity().getReportColumns()[0].id();
                reportBuilderService.createSummarise(node, [], []);

                var aggregatedNode = reportQueryEntity.getRootNode();

                var summariseActions = [
                    {
                        action: 'add',
                        aggregateMethod: 'Count All',
                        columnId: columnId,
                        originalColumnId: 0
                    },
                    {
                        action: 'remove',
                        aggregateMethod: 'Show values',
                        columnId: columnId,
                        originalColumnId: columnId
                    }
                ];

                reportBuilderService.updateAggregateColumns(aggregatedNode, summariseActions);

                expect(reportQueryEntity.getReportColumns().length > 0).toBeTruthy();
                expect(reportQueryEntity.getReportColumns()[0].getExpression().getTypeAlias() === 'core:aggregateExpression').toBeTruthy();
                expect(reportQueryEntity.getReportColumns()[0].getExpression().getAggregateMethod().alias() === 'core:aggCount').toBeTruthy();

            }));
            
            it('remove summarised node', inject(function (reportBuilderService) {
                reportBuilderService.setReportEntity(reportQueryEntity.getEntity().id(), reportQueryEntity, null);
                var rootNode = reportQueryEntity.getRootNode();
                var node = { "nid": rootNode.id(), "name": rootNode.getResourceReportNodeType().getName(), "etid": rootNode.getResourceReportNodeType().id(), qe: rootNode, pe: null, pae: null };

                reportBuilderService.createSummarise(node, [], []);
                
                var aggregatedNode = reportQueryEntity.getRootNode();

                reportBuilderService.removeSummarise(null, aggregatedNode);
                
                var newRootNode = reportQueryEntity.getRootNode();

                expect(newRootNode).toBeTruthy();
                expect(reportQueryEntity.getRootNode().getTypeAlias() === 'core:resourceReportNode').toBeTruthy();
                expect(reportQueryEntity.getEntity().getRootNode().getGroupedNode).toBeFalsy();
            }));
        });
    });
  
});

