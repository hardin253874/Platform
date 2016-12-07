// Copyright 2011-2016 Global Software Innovation Pty Ltd
/* globals reportTestData */

describe('Reports|View|spReport|spec:|spReport directive', function () {
    'use strict';

    var q;
    
    // Setup
    beforeEach(module('ng'));

    beforeEach(module('mod.common.ui.spReport'));
    beforeEach(module('app-templates'));
    beforeEach(module('component-templates'));    
    beforeEach(module('app.editFormModules'));
    beforeEach(module('mockedEntityService'));
    beforeEach(module('mockedReportService'));
    beforeEach(module('mockedNavService'));

    beforeEach(function() {


        var tenantSettingsStub = {
            getNameFieldEntity: function () {
                return {
                    then: function (caller) {
                        caller(null);
                    }
                };
            },
            getCurrencySymbol: function() {
                return {
                    then: function(caller) {
                        caller('$');
                    }
                };
            },
            getTemplateReportIds: function() {
                return {
                    then: function(caller1) {
                        caller1({});
                        return {
                            then: function (caller2) {
                                caller2({});
                            }
                        };
                    }
                };
            }
        };

        var actionsServiceStub = {
            getConsoleActions: function() {
                return q.defer().promise;
            },
            getEntityIdsFromDataGridSelection: function () {
               return [];
            }
        };

        module('mod.ui.spReportModelManager', function($provide) {
            $provide.value('spTenantSettings', tenantSettingsStub);
            $provide.value('spActionsService', actionsServiceStub);
        });
    });

    // Set the mocked data
    beforeEach(inject(function ($q, spReportService, spEntityService) {
        q = $q;
        var iconThumbnailSize,
            condFormatIcons = [];

        // Set the data we wish the mock to return
        iconThumbnailSize = spEntity.fromJSON({
            id: { id: 22222, ns: 'console', alias: 'iconThumbnailSize' },
            'console:thumbnailWidth': '16',
            'console:thumbnailHeight': '16'
        });
        spEntityService.mockGetEntity(iconThumbnailSize);

        condFormatIcons.push(spEntity.fromJSON({
            id: { id: 3000, ns: 'core', alias: 'blackCircleCondFormat' },
            'core:formatIconOrder': '0',
            'core:condFormatImage': {
                id: { id: 3001, ns: 'core', alias: 'blackCircleCondFormatIcon' }
            }
        }));

        condFormatIcons.push(spEntity.fromJSON({
            id: { id: 4000, ns: 'core', alias: 'greenCircleCondFormat' },
            'core:formatIconOrder': '1',
            'core:condFormatImage': {
                id: { id: 4001, ns: 'core', alias: 'greenCircleCondFormatIcon' }
            }
        }));

        spEntityService.mockGetEntitiesOfType('conditionalFormatIcon', condFormatIcons);

        // Mock the report data
        spReportService.mockGetReportData(12345, reportTestData.allFields);
        // Report with conditional formatting
        spReportService.mockGetReportData(22222, reportTestData.anlSimpleChoiceCond);
        // Report with rollup data
        spReportService.mockGetReportData(55555, reportTestData.rollupReport);

        spEntityService.mockGetEntitiesOfType('aggregateMethodEnum', null);

        // the most basic entity used in a few tests
        spEntityService.mockGetEntity(spEntity.fromJSON({id: 12345}));
    }));       
        

    afterEach(inject(function ($document) {
        var body = $document.find('body');
        body.find('div.modal').remove();
        body.find('div.modal-backdrop').remove();
        body.removeClass('modal-open');
    }));

        
    it('load report from id', inject(function ($rootScope, $compile, $templateCache, $timeout) {
        var scope = $rootScope,
            element,
            reportModel;

        // Setup the grid options        
        scope.reportOptions =
            {
                reportId: 12345
            };

        scope.$on('spReportEventModelReady', function (event, model) {
            reportModel = model;
        });

        element = angular.element('<sp-report options="reportOptions"></sp-report>');
        $compile(element)(scope);
        scope.$digest();
        $timeout.flush();

        // Verify that the html element has been replaced        
        expect(element.prop('localName')).toBe('div');
        expect(element.hasClass('spreport-view')).toBe(true);
        expect(reportModel).toBeTruthy();        
    }));


    it('load report from existing model', inject(function ($rootScope, $compile, $templateCache, $timeout) {
        var scope = $rootScope,
            element,
            done,
            reportModel;

        // Setup the grid options        
        scope.reportOptions =
            {
                reportId: 12345                
            };

        // Handle model loaded events
        scope.$on('spReportEventModelReady', function (event, model) {
            reportModel = model;
        });

        // Load a report and get the model
        element = angular.element('<sp-report options="reportOptions"></sp-report>');
        $compile(element)(scope);
        scope.$digest();
        $timeout.flush();

        // Setup the grid options again this time with the existing model
        scope.reportOptions =
            {
                reportId: 12345,
                reportModel: reportModel
            };
        // Set a selected item
        scope.reportOptions.reportModel.gridOptions.selectedItems = [
            {
                eid: 4279   
            }
        ];

        element = angular.element('<sp-report options="reportOptions"></sp-report>');
        $compile(element)(scope);
        scope.$digest();
        $timeout.flush();

        // Verify that the html element has been replaced        
        expect(element.prop('localName')).toBe('div');
        expect(element.hasClass('spreport-view')).toBe(true);
        expect(reportModel).toBeTruthy();

        // Allow report to execute code
        runs(function () {
            done = false;
            scope.$digest();

            setTimeout(function () {

                expect(scope.reportOptions.selectedItems.length).toBe(1);
                expect(scope.reportOptions.selectedItems[0].eid).toBe(4279);

                done = true;
            }, 2000);
        });

        waitsFor(function () {
            return done;
        }, "Report Done", 4000);
    }));


    it('select data grid row', inject(function ($rootScope, $compile, $templateCache, $timeout) {
        var scope = $rootScope,
            element,
            reportModel;

        // Setup the grid options        
        scope.reportOptions =
            {
                reportId: 12345
            };

        scope.$on('spReportEventModelReady', function (event, model) {
            reportModel = model;
        });

        element = angular.element('<sp-report options="reportOptions"></sp-report>');
        $compile(element)(scope);
        scope.$digest();
        $timeout.flush();

        //scope.$digest();
        //$timeout.flush();

        // select a row
        reportModel.gridOptions.dataGrid.selectRow(0, true);

        scope.$digest();

        // Ensure that it is selected
        expect(scope.reportOptions.selectedItems.length).toBe(1);
        expect(scope.reportOptions.selectedItems[0].eid).toBe(4279);
    }));


    // Enumerate the child scopes and find the one with the ngGridOptions.
    function findNgGridOptions(scope) {
        if (!scope) {
            return null;
        }

        var ngGridOptions = spUtils.result(scope, 'model.ngGridOptions');        

        if (!ngGridOptions &&
            scope.$$nextSibling) {
            ngGridOptions = findNgGridOptions(scope.$$nextSibling);            
        }

        if (!ngGridOptions &&
            scope.$$childHead) {
            ngGridOptions = findNgGridOptions(scope.$$childHead);            
        }

        return ngGridOptions;
    }


    it('ensure basic grid sorting context menu options work', inject(function ($rootScope, $compile, $templateCache, $timeout) {
        var scope = $rootScope,
            element,
            cols,
            reportModel;

        // Setup the grid options        
        scope.reportOptions =
            {
                reportId: 12345
            };

        scope.$on('spReportEventModelReady', function (event, model) {
            reportModel = model;
        });

        element = angular.element('<sp-report options="reportOptions"></sp-report>');
        $compile(element)(scope);
        scope.$digest();
        $timeout.flush();
        cols = _.map(findNgGridOptions(scope).ngGrid.config.columnDefs, function (cd) {
            return {
                colDef: cd    
            };
        });

        // Do sorting                
        scope.$$childHead.onContextMenuAction('sortAscending', cols[0]);
        expect(reportModel.gridOptions.sortInfo.length).toBe(1);        
        expect(reportModel.gridOptions.sortInfo[0].columnId).toBe(cols[0].colDef.spColumnDefinition.columnId);
        expect(reportModel.gridOptions.sortInfo[0].sortDirection).toBe('asc');
        expect(scope.$$childHead.isContextMenuActionDisabled('cancelSort', cols[0])).toBe(false);

        scope.$$childHead.onContextMenuAction('sortDescending', cols[0]);
        expect(reportModel.gridOptions.sortInfo.length).toBe(1);
        expect(reportModel.gridOptions.sortInfo[0].columnId).toBe(cols[0].colDef.spColumnDefinition.columnId);
        expect(reportModel.gridOptions.sortInfo[0].sortDirection).toBe('desc');
        expect(scope.$$childHead.isContextMenuActionDisabled('cancelSort', cols[0])).toBe(false);

        expect(scope.$$childHead.isContextMenuActionDisabled('cancelSort', cols[1])).toBe(true);
        scope.$$childHead.onContextMenuAction('sortAscending', cols[1]);
        expect(reportModel.gridOptions.sortInfo.length).toBe(2);
        expect(reportModel.gridOptions.sortInfo[0].columnId).toBe(cols[1].colDef.spColumnDefinition.columnId);
        expect(reportModel.gridOptions.sortInfo[0].sortDirection).toBe('asc');
        expect(scope.$$childHead.isContextMenuActionDisabled('cancelSort', cols[1])).toBe(false);
        expect(reportModel.gridOptions.sortInfo[1].columnId).toBe(cols[0].colDef.spColumnDefinition.columnId);
        expect(reportModel.gridOptions.sortInfo[1].sortDirection).toBe('desc');
        expect(scope.$$childHead.isContextMenuActionDisabled('cancelSort', cols[0])).toBe(false);

        scope.$$childHead.onContextMenuAction('sortAscending', cols[0]);
        expect(reportModel.gridOptions.sortInfo.length).toBe(2);
        expect(reportModel.gridOptions.sortInfo[0].columnId).toBe(cols[0].colDef.spColumnDefinition.columnId);
        expect(reportModel.gridOptions.sortInfo[0].sortDirection).toBe('desc');
        expect(scope.$$childHead.isContextMenuActionDisabled('cancelSort', cols[0])).toBe(false);
        expect(reportModel.gridOptions.sortInfo[1].columnId).toBe(cols[1].colDef.spColumnDefinition.columnId);
        expect(reportModel.gridOptions.sortInfo[1].sortDirection).toBe('asc');
        expect(scope.$$childHead.isContextMenuActionDisabled('cancelSort', cols[1])).toBe(false);

        scope.$$childHead.onContextMenuAction('cancelSort', cols[0]);
        expect(reportModel.gridOptions.sortInfo.length).toBe(1);
        expect(reportModel.gridOptions.sortInfo[0].columnId).toBe(cols[1].colDef.spColumnDefinition.columnId);
        expect(reportModel.gridOptions.sortInfo[0].sortDirection).toBe('asc');
        expect(scope.$$childHead.isContextMenuActionDisabled('cancelSort', cols[0])).toBe(true);

        scope.$$childHead.onContextMenuAction('cancelSort', cols[1]);
        expect(reportModel.gridOptions.sortInfo.length).toBe(0);
        expect(scope.$$childHead.isContextMenuActionDisabled('cancelSort', cols[1])).toBe(true);
    }));    


    it('ensure advanced grid sorting context menu option works', inject(function ($rootScope, $compile, $templateCache, $document, $timeout) {
        var scope = $rootScope,
            element,
            dialog,
            ok,
            cols,
            reportModel;

        // Setup the grid options        
        scope.reportOptions =
            {
                reportId: 12345
            };

        scope.$on('spReportEventModelReady', function (event, model) {
            reportModel = model;
        });

        element = angular.element('<sp-report options="reportOptions"></sp-report>');
        $compile(element)(scope);
        scope.$digest();
        $timeout.flush();

        cols = _.map(findNgGridOptions(scope).ngGrid.config.columnDefs, function (cd) {
            return {
                colDef: cd
            };
        });

        // Show the advanced sorting dialog
        scope.$$childHead.onContextMenuAction('sortOptions', cols[0]);
        scope.$digest();        
    }));


    xit('ensure column formatting context menu option works - icon formatting', inject(function ($rootScope, $compile, $templateCache, $document, $timeout) {
        var scope = $rootScope,
            element,
            dialog,
            ok,
            cols,
            reportModel;

        // Setup the grid options        
        scope.reportOptions =
            {
                reportId: 22222
            };

        scope.$on('spReportEventModelReady', function (event, model) {
            reportModel = model;
        });

        element = angular.element('<sp-report options="reportOptions"></sp-report>');
        $compile(element)(scope);
        scope.$digest();
        $timeout.flush();
        
        cols = _.map(findNgGridOptions(scope).ngGrid.config.columnDefs, function (cd) {
            return {
                colDef: cd
            };
        });

        // Show the formatting dialog
        scope.$$childHead.onContextMenuAction('formatColumn', cols[0]);        
    }));

    xit('ensure column formatting context menu option works - highlight formatting', inject(function ($rootScope, $compile, $templateCache, $document, $timeout) {
        var scope = $rootScope,
            element,
            dialog,
            ok,
            cols,
            reportModel;

        // Setup the grid options        
        scope.reportOptions =
            {
                reportId: 22222
            };

        scope.$on('spReportEventModelReady', function (event, model) {
            reportModel = model;
        });

        element = angular.element('<sp-report options="reportOptions"></sp-report>');
        $compile(element)(scope);
        scope.$digest();
        $timeout.flush();

        cols = _.map(findNgGridOptions(scope).ngGrid.config.columnDefs, function (cd) {
            return {
                colDef: cd
            };
        });

        // Show the formatting dialog
        scope.$$childHead.onContextMenuAction('formatColumn', cols[2]);
    }));


    xit('ensure column formatting context menu option works - progress bar formatting', inject(function ($rootScope, $compile, $templateCache, $document, $timeout) {
        var scope = $rootScope,
            element,
            dialog,
            ok,
            cols,
            reportModel;

        // Setup the grid options        
        scope.reportOptions =
            {
                reportId: 22222
            };

        scope.$on('spReportEventModelReady', function (event, model) {
            reportModel = model;
        });

        element = angular.element('<sp-report options="reportOptions"></sp-report>');
        $compile(element)(scope);
        scope.$digest();
        $timeout.flush();

        cols = _.map(findNgGridOptions(scope).ngGrid.config.columnDefs, function (cd) {
            return {
                colDef: cd
            };
        });

        // Show the formatting dialog
        scope.$$childHead.onContextMenuAction('formatColumn', cols[4]);
    }));


    it('ensure changing sorting sets hasAdHocSorting flag', inject(function ($rootScope, $compile, $templateCache, $timeout) {
        var scope = $rootScope,
            element,
            cols,
            reportModel;

        // Setup the grid options        
        scope.reportOptions =
            {
                reportId: 12345
            };

        scope.$on('spReportEventModelReady', function (event, model) {
            reportModel = model;
        });

        element = angular.element('<sp-report options="reportOptions"></sp-report>');
        $compile(element)(scope);
        scope.$digest();
        $timeout.flush();

        expect(reportModel.hasAdHocSorting).toBeFalsy();

        cols = _.map(findNgGridOptions(scope).ngGrid.config.columnDefs, function (cd) {
            return {
                colDef: cd
            };
        });

        // Do sorting        
        scope.$$childHead.onContextMenuAction('sortAscending', cols[1]);

        scope.$digest();

        expect(reportModel.hasAdHocSorting).toBeTruthy();
    }));


    it('ensure column formatting settings are correct', inject(function ($rootScope, $compile, $templateCache, $timeout) {
        var scope = $rootScope,
            element,
            reportModel;

        // Setup the grid options        
        scope.reportOptions =
            {
                reportId: 22222
            };

        scope.$on('spReportEventModelReady', function (event, model) {
            reportModel = model;
        });

        element = angular.element('<sp-report options="reportOptions"></sp-report>');
        $compile(element)(scope);
        scope.$digest();
        $timeout.flush();

        expect(reportModel).toBeTruthy();

        expect(_.keys(reportModel.gridOptions.columnDefinitions[0].cellFormatting).length).toBe(3);
        expect(reportModel.gridOptions.columnDefinitions[0].cellFormatting[0].imageId).toBeDefined();
        expect(reportModel.gridOptions.columnDefinitions[0].cellFormatting[1].imageId).toBeDefined();
        expect(reportModel.gridOptions.columnDefinitions[0].cellFormatting[2].imageId).toBeDefined();
        expect(reportModel.gridOptions.columnDefinitions[0].hideValue).toBe(false);

        expect(_.keys(reportModel.gridOptions.columnDefinitions[1].cellFormatting).length).toBe(0);
        expect(reportModel.gridOptions.columnDefinitions[1].hideValue).toBe(false);

        expect(_.keys(reportModel.gridOptions.columnDefinitions[2].cellFormatting).length).toBe(4);
        expect(reportModel.gridOptions.columnDefinitions[2].cellFormatting[0].foregroundColor).toBeDefined();
        expect(reportModel.gridOptions.columnDefinitions[2].cellFormatting[0].backgroundColor).toBeDefined();
        expect(reportModel.gridOptions.columnDefinitions[2].cellFormatting[0].bounds).toBeUndefined();
        expect(reportModel.gridOptions.columnDefinitions[2].cellFormatting[1].foregroundColor).toBeDefined();
        expect(reportModel.gridOptions.columnDefinitions[2].cellFormatting[1].backgroundColor).toBeDefined();
        expect(reportModel.gridOptions.columnDefinitions[2].cellFormatting[1].bounds).toBeUndefined();
        expect(reportModel.gridOptions.columnDefinitions[2].cellFormatting[2].foregroundColor).toBeDefined();
        expect(reportModel.gridOptions.columnDefinitions[2].cellFormatting[2].backgroundColor).toBeDefined();
        expect(reportModel.gridOptions.columnDefinitions[2].cellFormatting[2].bounds).toBeUndefined();
        expect(reportModel.gridOptions.columnDefinitions[2].cellFormatting[3].foregroundColor).toBeDefined();
        expect(reportModel.gridOptions.columnDefinitions[2].cellFormatting[3].backgroundColor).toBeDefined();
        expect(reportModel.gridOptions.columnDefinitions[2].cellFormatting[3].bounds).toBeUndefined();
        expect(reportModel.gridOptions.columnDefinitions[2].hideValue).toBe(true);

        expect(_.keys(reportModel.gridOptions.columnDefinitions[3].cellFormatting).length).toBe(0);
        expect(reportModel.gridOptions.columnDefinitions[3].hideValue).toBe(false);

        expect(_.keys(reportModel.gridOptions.columnDefinitions[4].cellFormatting).length).toBe(1);
        expect(reportModel.gridOptions.columnDefinitions[4].cellFormatting[0].foregroundColor).toBeUndefined();
        expect(reportModel.gridOptions.columnDefinitions[4].cellFormatting[0].backgroundColor).toBeDefined();
        expect(reportModel.gridOptions.columnDefinitions[4].cellFormatting[0].bounds).toBeDefined();
        expect(reportModel.gridOptions.columnDefinitions[4].hideValue).toBe(true);

        expect(_.keys(reportModel.gridOptions.columnDefinitions[5].cellFormatting).length).toBe(1);
        expect(reportModel.gridOptions.columnDefinitions[5].cellFormatting[0].foregroundColor).toBeUndefined();
        expect(reportModel.gridOptions.columnDefinitions[5].cellFormatting[0].backgroundColor).toBeDefined();
        expect(reportModel.gridOptions.columnDefinitions[5].cellFormatting[0].bounds).toBeDefined();
        expect(reportModel.gridOptions.columnDefinitions[5].hideValue).toBe(true);

        expect(_.keys(reportModel.gridOptions.columnDefinitions[6].cellFormatting).length).toBe(0);
        expect(reportModel.gridOptions.columnDefinitions[6].hideValue).toBe(false);

        expect(_.keys(reportModel.gridOptions.columnDefinitions[7].cellFormatting).length).toBe(0);
        expect(reportModel.gridOptions.columnDefinitions[7].hideValue).toBe(false);

        expect(_.keys(reportModel.gridOptions.columnDefinitions[8].cellFormatting).length).toBe(0);
        expect(reportModel.gridOptions.columnDefinitions[8].hideValue).toBe(false);

        expect(_.keys(reportModel.gridOptions.columnDefinitions[9].cellFormatting).length).toBe(0);
        expect(reportModel.gridOptions.columnDefinitions[9].hideValue).toBe(false);

        expect(_.keys(reportModel.gridOptions.columnDefinitions[10].cellFormatting).length).toBe(4);
        expect(reportModel.gridOptions.columnDefinitions[10].cellFormatting[0].foregroundColor).toBeDefined();
        expect(reportModel.gridOptions.columnDefinitions[10].cellFormatting[0].backgroundColor).toBeDefined();
        expect(reportModel.gridOptions.columnDefinitions[10].cellFormatting[0].bounds).toBeUndefined();
        expect(reportModel.gridOptions.columnDefinitions[10].cellFormatting[1].foregroundColor).toBeDefined();
        expect(reportModel.gridOptions.columnDefinitions[10].cellFormatting[1].backgroundColor).toBeDefined();
        expect(reportModel.gridOptions.columnDefinitions[10].cellFormatting[1].bounds).toBeUndefined();
        expect(reportModel.gridOptions.columnDefinitions[10].cellFormatting[2].foregroundColor).toBeDefined();
        expect(reportModel.gridOptions.columnDefinitions[10].cellFormatting[2].backgroundColor).toBeDefined();
        expect(reportModel.gridOptions.columnDefinitions[10].cellFormatting[2].bounds).toBeUndefined();
        expect(reportModel.gridOptions.columnDefinitions[10].cellFormatting[3].foregroundColor).toBeDefined();
        expect(reportModel.gridOptions.columnDefinitions[10].cellFormatting[3].backgroundColor).toBeDefined();
        expect(reportModel.gridOptions.columnDefinitions[10].cellFormatting[3].bounds).toBeUndefined();        
        expect(reportModel.gridOptions.columnDefinitions[10].hideValue).toBe(false);
    }));

    // DISABLED: this test no longer works under jasmine 1.31
    xit('ensure rollup report', inject(function ($rootScope, $compile, $templateCache, $timeout) {
        var scope = $rootScope,
            element,
            reportModel;

        // Setup the grid options        
        scope.reportOptions =
            {
                reportId: 55555
            };

        scope.$on('spReportEventModelReady', function (event, model) {
            reportModel = model;
        });

        element = angular.element('<sp-report options="reportOptions"></sp-report>');
        $compile(element)(scope);
        scope.$digest();
        $timeout.flush();

        expect(reportModel).toBeTruthy();
        expect(reportModel.aggregateDataManager).toBeTruthy();
        expect(reportModel.aggregateDataManager.hasRollupData()).toBeTruthy();
        expect(reportModel.aggregateDataManager.getGroupColumns().length).toBe(1);
        expect(reportModel.aggregateDataManager.getGroupColumns()[0].value).toBe('Season');

        expect(reportModel.gridOptions.aggregateInfo.showGrandTotals).toBe(true);
        expect(reportModel.gridOptions.aggregateInfo.showSubTotals).toBe(true);
        expect(reportModel.gridOptions.aggregateInfo.groupedColumns.length).toBe(1);

        expect(reportModel.gridOptions.columnDefinitions[2].totals.length).toBe(4);

        expect(reportModel.gridOptions.columnDefinitions[2].totals[0].id).toBe('aggCount');
        expect(reportModel.gridOptions.columnDefinitions[2].totals[1].id).toBe('aggSum');
        expect(reportModel.gridOptions.columnDefinitions[2].totals[2].id).toBe('aggMin');
        expect(reportModel.gridOptions.columnDefinitions[2].totals[3].id).toBe('aggMax');
    }));
});