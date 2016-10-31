// Copyright 2011-2016 Global Software Innovation Pty Ltd
describe('Console|Controls|spDataGrid|spec:|spDataGrid directive', function () {
    'use strict';    

    // Load the modules    
    beforeEach(module('app-templates'));
    beforeEach(module('component-templates'));
    beforeEach(module('mockedEntityService'));

    beforeEach(function () {
        // Stub out the nav service        
        var navServiceStub = {

        };

        var actionsServiceStub = {
            getConsoleActions: function () {
                return q.defer().promise;
            }
        };

        module('mod.common.ui.spDataGrid', function ($provide) {
            $provide.value('spNavService', navServiceStub);
            $provide.value('spActionsService', actionsServiceStub);
        });
    });

    // Helper functions
    function getColumnFromColumnDef(spColumnDefinition) {
        return {
            colDef: {
                spColumnDefinition: spColumnDefinition
            }
        };
    }

    function getTemplateHtml($templateCache, templateName) {
        var CUSTOM_FILTERS = /CUSTOM_FILTERS/g;
        var COL_FIELD = /COL_FIELD/g;

        var cellTemplate = $templateCache.get(templateName);

        return cellTemplate.replace(COL_FIELD, '$eval(\'row.entity.\' + col.field)').replace(CUSTOM_FILTERS, '');
    }

    it('should replace HTML element with appropriate content - no column formatting', inject(function ($rootScope, $compile, $templateCache) {
        var scope = $rootScope,
            element,
            gridScope,
            i,
            r,
            c,
            bgStyle,
            fgStyle,
            imgStyle,
            showPercent,
            gridDiv;

        // Setup the grid options        
        scope.gridOptions =
            {                
                rowData: [
                    { cells: [{ value: 1 }, { value: 'Name 1' }, { value: 'Description 1' }] },
                    { cells: [{ value: 2 }, { value: 'Name 2' }, { value: 'Description 2' }] },
                    { cells: [{ value: 3 }, { value: 'Name 3' }, { value: 'Description 3' }] }
                ],
                selectedItems: [],
                columnDefinitions: [
                    {
                        cellIndex: 1,
                        displayName: 'Name'
                    },
                    {
                        cellIndex: 2,
                        displayName: 'Description'
                    }],            
            };

        element = angular.element('<sp-data-grid grid-options="gridOptions"></sp-data-grid>');
        $compile(element)(scope);
        scope.$digest();
        
        // Verify that the html element has been replaced
        gridDiv = element.first('span').first('div.dataGrid-view');
        expect(gridDiv.length).toBe(1);

        gridScope = scope.$$childHead;

        // Verify that the grid options have been passed in correctly and the grid is using them        
        expect(gridScope.model.columnDefinitions.length).toBe(scope.gridOptions.columnDefinitions.length);
        expect(gridScope.model.columnDefinitions[0].displayName).toBe(scope.gridOptions.columnDefinitions[0].displayName);
        expect(gridScope.model.columnDefinitions[0].field).toBe('cells[1].value');
        expect(gridScope.model.columnDefinitions[0].cellTemplate).toBe(getTemplateHtml($templateCache, 'dataGrid/defaultCellTemplate.tpl.html'));
        expect(gridScope.model.columnDefinitions[1].displayName).toBe(scope.gridOptions.columnDefinitions[1].displayName);
        expect(gridScope.model.columnDefinitions[1].field).toBe('cells[2].value');
        expect(gridScope.model.columnDefinitions[1].cellTemplate).toBe(getTemplateHtml($templateCache, 'dataGrid/defaultCellTemplate.tpl.html'));
        expect(gridScope.model.rowData.length).toBe(scope.gridOptions.rowData.length);        

        // Check row data is correct                        
        for (i = 0; i < gridScope.model.ngGridOptions.ngGrid.data.length; i++) {
            expect(gridScope.model.ngGridOptions.ngGrid.data[i].cells[0].value).toBe(i + 1);
            expect(gridScope.model.ngGridOptions.ngGrid.data[i].cells[1].value).toBe('Name ' + (i + 1));
            expect(gridScope.model.ngGridOptions.ngGrid.data[i].cells[2].value).toBe('Description ' + (i + 1));
        }

        // Check cell formatting
        for (r = 0; r < 3; r++) {
            for (c = 0; c < 2; c++) {                
                bgStyle = gridScope.getFormattedCellBackgroundStyle(gridScope.model.ngGridOptions.ngGrid.rowCache[r], getColumnFromColumnDef(scope.gridOptions.columnDefinitions[c]));
                fgStyle = gridScope.getFormattedCellForegroundStyle(gridScope.model.ngGridOptions.ngGrid.rowCache[r], getColumnFromColumnDef(scope.gridOptions.columnDefinitions[c]));
                imgStyle = gridScope.getFormattedCellImageStyle(gridScope.model.ngGridOptions.ngGrid.rowCache[r], getColumnFromColumnDef(scope.gridOptions.columnDefinitions[c]));
                showPercent = gridScope.showPercentBar(gridScope.model.ngGridOptions.ngGrid.rowCache[r], getColumnFromColumnDef(scope.gridOptions.columnDefinitions[c]));

                expect(bgStyle).toBeNull();                
                expect(fgStyle['line-height']).toBeTruthy();                
                expect(imgStyle).toBeUndefined();
                expect(showPercent).toBe(false);
            }
        }        
    }));            

    it('should replace HTML element with appropriate content - highlight column formatting', inject(function ($rootScope, $compile, $templateCache) {
        var scope = $rootScope,
            element,
            gridScope,
            i,
            r,
            c,
            bgStyle,
            fgStyle,
            imgStyle,
            showPercent,
            gridDiv;

        scope.gridOptions =
            {               
                rowData: [
                    { cells: [{ value: 1 }, { value: 'Name 1', formattingIndex: 0 }, { value: 'Description 1' }] },
                    { cells: [{ value: 2 }, { value: 'Name 2' }, { value: 'Description 2' }] },
                    { cells: [{ value: 3 }, { value: 'Name 3' }, { value: 'Description 3' }] }
                ],
                selectedItems: [],
                columnDefinitions: [                
                    {
                        cellIndex: 1,
                        displayName: 'Name',
                        cellFormatting: [
                           {
                               backgroundColor: { r: 255, g: 0, b: 0, a: 255 },
                               foregroundColor: { r: 0, g: 0, b: 0, a: 255 }
                           }]
                    },
                    {
                        cellIndex: 2,
                        displayName: 'Description'              
                    }],            
        };

        element = angular.element('<sp-data-grid grid-options="gridOptions"></sp-data-grid>');
        $compile(element)(scope);
        scope.$digest();

        // Verify that the html element has been replaced
        gridDiv = element.first('span').first('div.dataGrid-view');
        expect(gridDiv.length).toBe(1);

        gridScope = scope.$$childHead;

        // Verify that the grid options have been passed in correctly and the grid is using them        
        expect(gridScope.model.columnDefinitions.length).toBe(scope.gridOptions.columnDefinitions.length);        
        expect(gridScope.model.columnDefinitions[0].displayName).toBe(scope.gridOptions.columnDefinitions[0].displayName);
        expect(gridScope.model.columnDefinitions[0].field).toBe('cells[1].value');
        expect(gridScope.model.columnDefinitions[0].cellTemplate).toBe(getTemplateHtml($templateCache, 'dataGrid/formattedCellTemplate.tpl.html'));
        expect(gridScope.model.columnDefinitions[1].displayName).toBe(scope.gridOptions.columnDefinitions[1].displayName);
        expect(gridScope.model.columnDefinitions[1].field).toBe('cells[2].value');
        expect(gridScope.model.columnDefinitions[1].cellTemplate).toBe(getTemplateHtml($templateCache, 'dataGrid/defaultCellTemplate.tpl.html'));
        expect(gridScope.model.rowData.length).toBe(scope.gridOptions.rowData.length);

        // Check row data is correct
        for (i = 0; i < gridScope.model.ngGridOptions.ngGrid.data.length; i++) {
            expect(gridScope.model.ngGridOptions.ngGrid.data[i].cells[0].value).toBe(i + 1);
            expect(gridScope.model.ngGridOptions.ngGrid.data[i].cells[1].value).toBe('Name ' + (i + 1));
            expect(gridScope.model.ngGridOptions.ngGrid.data[i].cells[2].value).toBe('Description ' + (i + 1));
        }

        // Check cell formatting
        for (r = 0; r < 3; r++) {
            for (c = 0; c < 2; c++) {
                bgStyle = gridScope.getFormattedCellBackgroundStyle(gridScope.model.ngGridOptions.ngGrid.rowCache[r], getColumnFromColumnDef(scope.gridOptions.columnDefinitions[c]));
                fgStyle = gridScope.getFormattedCellForegroundStyle(gridScope.model.ngGridOptions.ngGrid.rowCache[r], getColumnFromColumnDef(scope.gridOptions.columnDefinitions[c]));
                imgStyle = gridScope.getFormattedCellImageStyle(gridScope.model.ngGridOptions.ngGrid.rowCache[r], getColumnFromColumnDef(scope.gridOptions.columnDefinitions[c]));
                showPercent = gridScope.showPercentBar(gridScope.model.ngGridOptions.ngGrid.rowCache[r], getColumnFromColumnDef(scope.gridOptions.columnDefinitions[c]));

                if (r === 0 &&
                    c === 0) {
                    // Only row 0, column 0 has formatting                    
                    expect(bgStyle['background-color']).toBe('rgba(255,0,0,1)');
                    expect(bgStyle.height).toBe('100%');
                    expect(bgStyle.width).toBe('100%');
                    expect(fgStyle.color).toBe('rgba(0,0,0,1)');
                    expect(fgStyle['line-height']).toBeTruthy();
                    expect(imgStyle).toBeUndefined();
                    expect(showPercent).toBe(false);                                        
                } else {
                    expect(bgStyle).toBeNull();
                    expect(fgStyle['line-height']).toBeTruthy();                    
                    expect(fgStyle.color).toBeUndefined();
                    expect(imgStyle).toBeUndefined();
                    expect(showPercent).toBe(false);
                }
            }
        }
    }));

    it('should replace HTML element with appropriate content - progress column formatting - numeric', inject(function ($rootScope, $compile, $templateCache) {
        var scope = $rootScope,
            element,
            gridScope,
            i,
            r,
            c,
            bgStyle,
            fgStyle,
            imgStyle,
            showPercent,
            gridDiv;

        scope.gridOptions =
            {                
                rowData: [
                    { cells: [{ value: 1 }, { value: 10, formattingIndex: 0 }, { value: 'Description 1' }] },
                    { cells: [{ value: 2 }, { value: 20, formattingIndex: 0 }, { value: 'Description 2' }] },
                    { cells: [{ value: 3 }, { value: 30, formattingIndex: 0 }, { value: 'Description 3' }] }
                ],
                selectedItems: [],
                columnDefinitions: [
                {
                    cellIndex: 1,
                    displayName: 'Value',
                    type: 'Int32',
                    cellFormatting: [
                        {
                            backgroundColor: { r: 0, g: 255, b: 0, a: 255 },
                            foregroundColor: { r: 255, g: 255, b: 255, a: 255 },
                            bounds: {
                                lower: 0,
                                upper: 100
                            }
                        }]
                },
                {
                    cellIndex: 2,
                    displayName: 'Description'
                }],            
            };

        element = angular.element('<sp-data-grid grid-options="gridOptions"></sp-data-grid>');
        $compile(element)(scope);
        scope.$digest();

        // Verify that the html element has been replaced
        gridDiv = element.first('span').first('div.dataGrid-view');
        expect(gridDiv.length).toBe(1);

        gridScope = scope.$$childHead;

        // Verify that the grid options have been passed in correctly and the grid is using them        
        expect(gridScope.model.columnDefinitions.length).toBe(scope.gridOptions.columnDefinitions.length);
        expect(gridScope.model.columnDefinitions[0].displayName).toBe(scope.gridOptions.columnDefinitions[0].displayName);
        expect(gridScope.model.columnDefinitions[0].field).toBe('cells[1].value');
        expect(gridScope.model.columnDefinitions[0].cellTemplate).toBe(getTemplateHtml($templateCache, 'dataGrid/progressBarCellTemplate.tpl.html'));
        expect(gridScope.model.columnDefinitions[1].displayName).toBe(scope.gridOptions.columnDefinitions[1].displayName);
        expect(gridScope.model.columnDefinitions[1].field).toBe('cells[2].value');
        expect(gridScope.model.columnDefinitions[1].cellTemplate).toBe(getTemplateHtml($templateCache, 'dataGrid/defaultCellTemplate.tpl.html'));
        expect(gridScope.model.rowData.length).toBe(scope.gridOptions.rowData.length);

        // Check row data is correct
        for (i = 0; i < gridScope.model.ngGridOptions.ngGrid.data.length; i++) {
            expect(gridScope.model.ngGridOptions.ngGrid.data[i].cells[0].value).toBe(i + 1);
            expect(gridScope.model.ngGridOptions.ngGrid.data[i].cells[1].value).toBe((i + 1) * 10);
            expect(gridScope.model.ngGridOptions.ngGrid.data[i].cells[2].value).toBe('Description ' + (i + 1));
        }

        // Check cell formatting
        for (r = 0; r < 3; r++) {
            for (c = 0; c < 2; c++) {
                bgStyle = gridScope.getFormattedCellBackgroundStyle(gridScope.model.ngGridOptions.ngGrid.rowCache[r], getColumnFromColumnDef(scope.gridOptions.columnDefinitions[c]));
                fgStyle = gridScope.getFormattedCellForegroundStyle(gridScope.model.ngGridOptions.ngGrid.rowCache[r], getColumnFromColumnDef(scope.gridOptions.columnDefinitions[c]));
                imgStyle = gridScope.getFormattedCellImageStyle(gridScope.model.ngGridOptions.ngGrid.rowCache[r], getColumnFromColumnDef(scope.gridOptions.columnDefinitions[c]));
                showPercent = gridScope.showPercentBar(gridScope.model.ngGridOptions.ngGrid.rowCache[r], getColumnFromColumnDef(scope.gridOptions.columnDefinitions[c]));

                if (c === 0) {
                    // Only column 1 has formatting                    
                    expect(bgStyle['background-color']).toBe('rgba(0,255,0,1)');
                    expect(bgStyle.height).toBe(gridScope.PROGRESS_BAR_HEIGHT);
                    expect(bgStyle.width).toBe('1px'); // column widths are undefined in phantom
                    expect(fgStyle.color).toBe('rgba(255,255,255,1)');
                    expect(fgStyle['line-height']).toBeTruthy();
                    expect(imgStyle).toBeUndefined();
                    expect(showPercent).toBe(true);
                } else {
                    expect(bgStyle).toBeNull();
                    expect(fgStyle['line-height']).toBeTruthy();
                    expect(fgStyle.color).toBeUndefined();
                    expect(imgStyle).toBeUndefined();
                    expect(showPercent).toBe(false);
                }
            }
        }
    }));

    it('should replace HTML element with appropriate content - progress column formatting - numeric 0 percent', inject(function ($rootScope, $compile, $templateCache) {
        var scope = $rootScope,
            element,
            gridScope,
            i,
            r,
            c,
            bgStyle,
            fgStyle,
            imgStyle,
            showPercent,
            gridDiv;

        scope.gridOptions =
            {
                rowData: [
                    { cells: [{ value: 1 }, { value: 10, formattingIndex: 0 }, { value: 'Description 1' }] }                    
                ],
                selectedItems: [],
                columnDefinitions: [
                {
                    cellIndex: 1,
                    displayName: 'Value',
                    type: 'Int32',
                    cellFormatting: [
                        {
                            backgroundColor: { r: 0, g: 255, b: 0, a: 255 },
                            foregroundColor: { r: 255, g: 255, b: 255, a: 255 },
                            bounds: {
                                lower: 10,
                                upper: 100
                            }
                        }]
                },
                {
                    cellIndex: 2,
                    displayName: 'Description'
                }],
            };

        element = angular.element('<sp-data-grid grid-options="gridOptions"></sp-data-grid>');
        $compile(element)(scope);
        scope.$digest();

        // Verify that the html element has been replaced
        gridDiv = element.first('span').first('div.dataGrid-view');
        expect(gridDiv.length).toBe(1);

        gridScope = scope.$$childHead;               

        // Ensure percent width is 1px for row 0 col 0
        for (c = 0; c < 2; c++) {
            bgStyle = gridScope.getFormattedCellBackgroundStyle(gridScope.model.ngGridOptions.ngGrid.rowCache[0], getColumnFromColumnDef(scope.gridOptions.columnDefinitions[c]));
            fgStyle = gridScope.getFormattedCellForegroundStyle(gridScope.model.ngGridOptions.ngGrid.rowCache[0], getColumnFromColumnDef(scope.gridOptions.columnDefinitions[c]));
            imgStyle = gridScope.getFormattedCellImageStyle(gridScope.model.ngGridOptions.ngGrid.rowCache[0], getColumnFromColumnDef(scope.gridOptions.columnDefinitions[c]));
            showPercent = gridScope.showPercentBar(gridScope.model.ngGridOptions.ngGrid.rowCache[0], getColumnFromColumnDef(scope.gridOptions.columnDefinitions[c]));

            if (c === 0) {
                // Only column 1 has formatting                    
                expect(bgStyle['background-color']).toBe('rgba(0,255,0,1)');
                expect(bgStyle.height).toBe(gridScope.PROGRESS_BAR_HEIGHT);

                expect(bgStyle.width).toBe('1px');
                expect(fgStyle.color).toBe('rgba(255,255,255,1)');
                expect(fgStyle['line-height']).toBeTruthy();
                expect(imgStyle).toBeUndefined();
                expect(showPercent).toBe(true);
            } else {
                expect(bgStyle).toBeNull();
                expect(fgStyle['line-height']).toBeTruthy();
                expect(fgStyle.color).toBeUndefined();
                expect(imgStyle).toBeUndefined();
                expect(showPercent).toBe(false);
            }
        }        
    }));

    it('should replace HTML element with appropriate content - progress column formatting - date', inject(function ($rootScope, $compile, $templateCache) {
        var scope = $rootScope,
            element,
            gridScope,
            i,
            r,
            c,
            bgStyle,
            fgStyle,
            imgStyle,
            showPercent,
            gridDiv;

        scope.gridOptions =
            {
                rowData: [
                    { cells: [{ value: 1 }, { value: '2013-10-11T03:00:00Z', formattingIndex: 0 }, { value: 'Description 1' }] },
                    { cells: [{ value: 2 }, { value: '2013-10-12T03:00:00Z', formattingIndex: 0 }, { value: 'Description 2' }] },
                    { cells: [{ value: 3 }, { value: '2013-10-13T03:00:00Z', formattingIndex: 0 }, { value: 'Description 3' }] }
                ],
                selectedItems: [],
                columnDefinitions: [
                {
                    cellIndex: 1,
                    displayName: 'Value',
                    type: 'Date',
                    cellFormatting: [
                        {
                            backgroundColor: { r: 0, g: 255, b: 0, a: 255 },
                            foregroundColor: { r: 255, g: 255, b: 255, a: 255 },
                            bounds: {
                                lower: '2013-10-10T03:00:00Z',
                                upper: '2013-10-20T03:00:00Z'
                            }
                        }]
                },
                {
                    cellIndex: 2,
                    displayName: 'Description'
                }],
            };

        element = angular.element('<sp-data-grid grid-options="gridOptions"></sp-data-grid>');
        $compile(element)(scope);
        scope.$digest();

        // Verify that the html element has been replaced
        gridDiv = element.first('span').first('div.dataGrid-view');
        expect(gridDiv.length).toBe(1);

        gridScope = scope.$$childHead;

        // Verify that the grid options have been passed in correctly and the grid is using them        
        expect(gridScope.model.columnDefinitions.length).toBe(scope.gridOptions.columnDefinitions.length);
        expect(gridScope.model.columnDefinitions[0].displayName).toBe(scope.gridOptions.columnDefinitions[0].displayName);
        expect(gridScope.model.columnDefinitions[0].field).toBe('cells[1].value');
        expect(gridScope.model.columnDefinitions[0].cellTemplate).toBe(getTemplateHtml($templateCache, 'dataGrid/progressBarCellTemplate.tpl.html'));
        expect(gridScope.model.columnDefinitions[1].displayName).toBe(scope.gridOptions.columnDefinitions[1].displayName);
        expect(gridScope.model.columnDefinitions[1].field).toBe('cells[2].value');
        expect(gridScope.model.columnDefinitions[1].cellTemplate).toBe(getTemplateHtml($templateCache, 'dataGrid/defaultCellTemplate.tpl.html'));
        expect(gridScope.model.rowData.length).toBe(scope.gridOptions.rowData.length);        

        // Check cell formatting
        for (r = 0; r < 3; r++) {
            for (c = 0; c < 2; c++) {
                bgStyle = gridScope.getFormattedCellBackgroundStyle(gridScope.model.ngGridOptions.ngGrid.rowCache[r], getColumnFromColumnDef(scope.gridOptions.columnDefinitions[c]));
                fgStyle = gridScope.getFormattedCellForegroundStyle(gridScope.model.ngGridOptions.ngGrid.rowCache[r], getColumnFromColumnDef(scope.gridOptions.columnDefinitions[c]));
                imgStyle = gridScope.getFormattedCellImageStyle(gridScope.model.ngGridOptions.ngGrid.rowCache[r], getColumnFromColumnDef(scope.gridOptions.columnDefinitions[c]));
                showPercent = gridScope.showPercentBar(gridScope.model.ngGridOptions.ngGrid.rowCache[r], getColumnFromColumnDef(scope.gridOptions.columnDefinitions[c]));

                if (c === 0) {
                    // Only column 1 has formatting                    
                    expect(bgStyle['background-color']).toBe('rgba(0,255,0,1)');
                    expect(bgStyle.height).toBe(gridScope.PROGRESS_BAR_HEIGHT);
                    expect(bgStyle.width).toBe('1px'); // column widths are undefined in phantom
                    expect(fgStyle.color).toBe('rgba(255,255,255,1)');
                    expect(fgStyle['line-height']).toBeTruthy();
                    expect(imgStyle).toBeUndefined();
                    expect(showPercent).toBe(true);
                } else {
                    expect(bgStyle).toBeNull();
                    expect(fgStyle['line-height']).toBeTruthy();
                    expect(fgStyle.color).toBeUndefined();
                    expect(imgStyle).toBeUndefined();
                    expect(showPercent).toBe(false);
                }
            }
        }
    }));

    it('should replace HTML element with appropriate content - icon column formatting', inject(function ($rootScope, $compile, $templateCache) {
        var scope = $rootScope,
            element,
            gridScope,
            i,
            r,
            c,
            bgStyle,
            fgStyle,
            imgStyle,
            showPercent,
            gridDiv;

        scope.gridOptions =
            {                
                rowData: [
                    { cells: [{ value: 1 }, { value: 'Name 1', formattingIndex: 0 }, { value: 'Description 1' }] },
                    { cells: [{ value: 2 }, { value: 'Name 2' }, { value: 'Description 2' }] },
                    { cells: [{ value: 3 }, { value: 'Name 3' }, { value: 'Description 3' }] }
                ],
                selectedItems: [],
                columnDefinitions: [
                    {
                        cellIndex: 1,
                        displayName: 'Name',                                    
                        imageScaleId: 111,
                        imageSizeId: 222,
                        imageWidth: 30,
                        imageHeight: 30,
                        cellFormatting: [
                            {
                                imageId: 12345
                            }]
                    },
                    {
                        cellIndex: 2,
                        displayName: 'Description'                        
                    }],            
            };

        element = angular.element('<sp-data-grid grid-options="gridOptions"></sp-data-grid>');
        $compile(element)(scope);
        scope.$digest();

        // Verify that the html element has been replaced
        gridDiv = element.first('span').first('div.dataGrid-view');
        expect(gridDiv.length).toBe(1);

        gridScope = scope.$$childHead;

        // Verify that the grid options have been passed in correctly and the grid is using them        
        expect(gridScope.model.columnDefinitions.length).toBe(scope.gridOptions.columnDefinitions.length);
        expect(gridScope.model.columnDefinitions[0].displayName).toBe(scope.gridOptions.columnDefinitions[0].displayName);
        expect(gridScope.model.columnDefinitions[0].field).toBe('cells[1].value');
        expect(gridScope.model.columnDefinitions[0].cellTemplate).toBe(getTemplateHtml($templateCache, 'dataGrid/formattedCellImageTemplate.tpl.html'));
        expect(gridScope.model.columnDefinitions[1].displayName).toBe(scope.gridOptions.columnDefinitions[1].displayName);
        expect(gridScope.model.columnDefinitions[1].field).toBe('cells[2].value');
        expect(gridScope.model.columnDefinitions[1].cellTemplate).toBe(getTemplateHtml($templateCache, 'dataGrid/defaultCellTemplate.tpl.html'));
        expect(gridScope.model.rowData.length).toBe(scope.gridOptions.rowData.length);

        // Check row data is correct
        for (i = 0; i < gridScope.model.ngGridOptions.ngGrid.data.length; i++) {
            expect(gridScope.model.ngGridOptions.ngGrid.data[i].cells[0].value).toBe(i + 1);
            expect(gridScope.model.ngGridOptions.ngGrid.data[i].cells[1].value).toBe('Name ' + (i + 1));
            expect(gridScope.model.ngGridOptions.ngGrid.data[i].cells[2].value).toBe('Description ' + (i + 1));
        }

        // Check cell formatting
        for (r = 0; r < 3; r++) {
            for (c = 0; c < 2; c++) {
                bgStyle = gridScope.getFormattedCellBackgroundStyle(gridScope.model.ngGridOptions.ngGrid.rowCache[r], getColumnFromColumnDef(scope.gridOptions.columnDefinitions[c]));
                fgStyle = gridScope.getFormattedCellForegroundStyle(gridScope.model.ngGridOptions.ngGrid.rowCache[r], getColumnFromColumnDef(scope.gridOptions.columnDefinitions[c]));
                imgStyle = gridScope.getFormattedCellImageStyle(gridScope.model.ngGridOptions.ngGrid.rowCache[r], getColumnFromColumnDef(scope.gridOptions.columnDefinitions[c]));
                showPercent = gridScope.showPercentBar(gridScope.model.ngGridOptions.ngGrid.rowCache[r], getColumnFromColumnDef(scope.gridOptions.columnDefinitions[c]));

                if (r === 0 &&
                    c === 0) {
                    // Only row 0, column 0 has formatting                    
                    expect(bgStyle).not.toBeNull();
                    expect(bgStyle['background-color']).toBeUndefined();
                    expect(bgStyle.height).toBe('100%');
                    expect(bgStyle.width).toBe('100%');
                    expect(fgStyle['line-height']).toBeTruthy();
                    expect(fgStyle.color).toBeUndefined();
                    // on the next test, need to allow for a possible addition of say XSRF token on the URL
                    expect(imgStyle['background-image'].indexOf('url(\'/spapi/data/v1/image/thumbnail/12345/222/111') === 0).toBeTruthy();
                    expect(imgStyle['width']).toBe('30px');
                    expect(imgStyle['height']).toBe('30px');
                    expect(showPercent).toBe(false);
                } else {
                    expect(bgStyle).toBeNull();
                    expect(fgStyle['line-height']).toBeTruthy();
                    expect(fgStyle.color).toBeUndefined();
                    expect(imgStyle).toBeUndefined();
                    expect(showPercent).toBe(false);
                }
            }
        }
    }));      

    it('verify selected items are set correctly', inject(function ($rootScope, $compile) {
        var scope = $rootScope,
            element,
            gridScope,
            i,
            gridDiv;

        scope.gridOptions =
            {                
                rowData: [
                    { cells: [{ value: 1 }, { value: 'Name 1' }, { value: 'Description 1' }] },
                    { cells: [{ value: 2 }, { value: 'Name 2' }, { value: 'Description 2' }] },
                    { cells: [{ value: 3 }, { value: 'Name 3' }, { value: 'Description 3' }] }
                ],
                selectedItems: [],
                columnDefinitions: [
                    {
                        cellIndex: 1,
                        displayName: 'Name'
                    },
                    {
                        cellIndex: 2,
                        displayName: 'Description'
                    }],            
            };

        element = angular.element('<sp-data-grid grid-options="gridOptions"></sp-data-grid>');
        $compile(element)(scope);
        scope.$digest();

        // Verify that the html element has been replaced
        gridDiv = element.first('span').first('div.dataGrid-view');
        expect(gridDiv.length).toBe(1);

        gridScope = scope.$$childHead;

        // Verify that the grid options have been passed in correctly and the grid is using them        
        expect(gridScope.model.columnDefinitions.length).toBe(scope.gridOptions.columnDefinitions.length);
        expect(gridScope.model.columnDefinitions[0].displayName).toBe(scope.gridOptions.columnDefinitions[0].displayName);
        expect(gridScope.model.columnDefinitions[0].field).toBe('cells[1].value');
        expect(gridScope.model.columnDefinitions[1].displayName).toBe(scope.gridOptions.columnDefinitions[1].displayName);
        expect(gridScope.model.columnDefinitions[1].field).toBe('cells[2].value');
        expect(gridScope.model.rowData.length).toBe(scope.gridOptions.rowData.length);

        // Check row data is correct                
        for (i = 0; i < gridScope.model.ngGridOptions.ngGrid.data.length; i++) {
            expect(gridScope.model.ngGridOptions.ngGrid.data[i].cells[0].value).toBe(i + 1);
            expect(gridScope.model.ngGridOptions.ngGrid.data[i].cells[1].value).toBe('Name ' + (i + 1));
            expect(gridScope.model.ngGridOptions.ngGrid.data[i].cells[2].value).toBe('Description ' + (i + 1));
        }

        // Verify selected row
        gridScope.model.ngGridOptions.selectRow(1, true);

        expect(scope.gridOptions.selectedItems.length).toBe(1);
        expect(scope.gridOptions.selectedItems[0].cells[0].value).toBe(2);
        expect(scope.gridOptions.selectedItems[0].cells[1].value).toBe('Name 2');
        expect(scope.gridOptions.selectedItems[0].cells[2].value).toBe('Description 2');
    }));


});