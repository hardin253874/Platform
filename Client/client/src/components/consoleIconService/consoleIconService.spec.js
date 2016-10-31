// Copyright 2011-2016 Global Software Innovation Pty Ltd

describe('sp.consoleIconService|spec:', function () {
    'use strict';
    var _icon, _getObject, _getReport, _getForm, _getChart, _getConsoleBehavior, _getBoard;

    beforeEach(module('sp.consoleIconService'));
    beforeEach(module('mod.common.spWebService'));

    // data for tests
    (function () {

        // icon 
        _icon = spEntity.fromJSON({
            id: { id: 1001 },
            name: 'Some icon',
            imageBackgroundColor: '#ff0000',
            isOfType: [{
                alias: 'core:iconFileType'
            }]
        });
        _getObject = function() {
            return spEntity.fromJSON({
                id: { id: 1000 },
                name: 'Location',
                isOfType: [{
                    alias: 'core:definition'
                }]
            });
        };

        _getReport = function() {
            return spEntity.fromJSON({
                id: { id: 1002 },
                name: 'Location report',
                isOfType: [{
                    alias: 'core:report'
                }],
                reportUsesDefinition: _getObject()
            });
        };

        _getConsoleBehavior = function() {
            return spEntity.fromJSON({
                id: { id: 2000 },
                name: 'Console Behavior',
                isOfType: [{
                    alias: 'k:consoleBehavior'
                }],
                'k:treeIcon': {
                    id: { id: 2001 },
                    name: 'Behavior icon',
                    imageBackgroundColor: '#ff00ff',
                    isOfType: [{
                        alias: 'core:iconFileType'
                    }]
                },
                'k:treeIconUrl': 'assets/images/nav/behaviorStaticIcon.svg'
            });
        };
        _getForm = function () {
            return spEntity.fromJSON({
                id: { id: 1003 },
                name: 'Some form',
                isOfType: [{
                    alias: 'console:customEditForm'
                }],
                'k:typeToEditWithForm': _getObject()
            });
        };

        // chart 
        _getChart = function() {
            return spEntity.fromJSON({
                id: { id: 1008 },
                name: 'Location chart',
                isOfType: [{
                    alias: 'core:chart'
                }],
                chartReport: _getReport()
            });
        };

        _getBoard = function() {
            return spEntity.fromJSON({
                id: { id: 1004 },
                name: 'Some board',
                isOfType: [{
                        alias: 'core:board'
                    }],
                boardReport: _getReport()
            });
        };

    })();

    beforeEach(inject(function ($injector, $rootScope) {
        TestSupport.setupUnitTests(this, $injector);
    }));

    it('console icon service should exist', inject(function (consoleIconService) {
        expect(consoleIconService).toBeTruthy();
    }));

    // report tests
    it('icon set on report is selected', inject(function (consoleIconService) {
        var report = _getReport();
        // set icon on report
        report.setLookup('console:navigationElementIcon', _icon);

        var iconInfo = consoleIconService.getNavItemIconUrlAndBackColor(report);
        expect(iconInfo).toBeDefined();
        expect(iconInfo.iconUrl).toBeDefined();
        expect(iconInfo.iconUrl).toStartWith('/spapi/data/v1/image/thumbnail/1001/console-iconThumbnailSize/core-scaleImageProportionally');
        expect(iconInfo.iconBackgroundColor).toBe('#ff0000');
    }));

    it('definition icon is selected for the report that does not have an icon set', inject(function (consoleIconService) {
        var report = _getReport();
        report.reportUsesDefinition.typeConsoleBehavior = _getConsoleBehavior();

        var iconInfo = consoleIconService.getNavItemIconUrlAndBackColor(report);
        expect(iconInfo).toBeDefined();
        expect(iconInfo.iconUrl).toBeDefined();
        expect(iconInfo.iconUrl).toStartWith('/spapi/data/v1/image/thumbnail/2001/console-iconThumbnailSize/core-scaleImageProportionally');
        expect(iconInfo.iconBackgroundColor).toBe('#ff00ff');
    }));

    it('resource console behavior icon of report is selected when no icon is set on report or the object', inject(function (consoleIconService) {
        var report = _getReport();
        report.setLookup('console:resourceConsoleBehavior', _getConsoleBehavior());
        
        var iconInfo = consoleIconService.getNavItemIconUrlAndBackColor(report);
        expect(iconInfo).toBeDefined();
        expect(iconInfo.iconUrl).toBeDefined();
        expect(iconInfo.iconUrl).toStartWith('/spapi/data/v1/image/thumbnail/2001/console-iconThumbnailSize/core-scaleImageProportionally');
        expect(iconInfo.iconBackgroundColor).toBe('#ff00ff');

        // remeove treeIcon of resource console behavior but leave static treeIconUrl
        report.resourceConsoleBehavior.treeIcon = null;

        iconInfo = consoleIconService.getNavItemIconUrlAndBackColor(report);
        expect(iconInfo).toBeDefined();
        expect(iconInfo.iconUrl).toBeDefined();
        expect(iconInfo.iconUrl).toBe('assets/images/nav/behaviorStaticIcon.svg');
        expect(iconInfo.iconBackgroundColor).toBeUndefined();

    }));

    it('type console behavior icon of report type is selected when no icon is set on report or the object', inject(function (consoleIconService) {
        var report = _getReport();
        var type = report.isOfType[0];
        type.setLookup('console:typeConsoleBehavior', _getConsoleBehavior());

        var iconInfo = consoleIconService.getNavItemIconUrlAndBackColor(report);
        expect(iconInfo).toBeDefined();
        expect(iconInfo.iconUrl).toBeDefined();
        expect(iconInfo.iconUrl).toStartWith('/spapi/data/v1/image/thumbnail/2001/console-iconThumbnailSize/core-scaleImageProportionally');
        expect(iconInfo.iconBackgroundColor).toBe('#ff00ff');

        // remeove treeIcon of resource console behavior but leave static treeIconUrl
        type.typeConsoleBehavior.treeIcon = null;

        iconInfo = consoleIconService.getNavItemIconUrlAndBackColor(report);
        expect(iconInfo).toBeDefined();
        expect(iconInfo.iconUrl).toBeDefined();
        expect(iconInfo.iconUrl).toBe('assets/images/nav/behaviorStaticIcon.svg');
        expect(iconInfo.iconBackgroundColor).toBeUndefined();

    }));

    it('default static icon is selected if report and its object do not have an icon set', inject(function (consoleIconService) {
        var report = _getReport();
        var iconInfo = consoleIconService.getNavItemIconUrlAndBackColor(report);
        expect(iconInfo).toBeDefined();
        expect(iconInfo.iconUrl).toBeDefined();
        expect(iconInfo.iconUrl).toBe('assets/images/default_app.png');
    }));

    // Form tests
    it('icon set on form is selected', inject(function (consoleIconService) {
        var form = _getForm();

        // set icon on form
        form.setLookup('console:navigationElementIcon', _icon);

        var iconInfo = consoleIconService.getNavItemIconUrlAndBackColor(form);
        expect(iconInfo).toBeDefined();
        expect(iconInfo.iconUrl).toBeDefined();
        expect(iconInfo.iconUrl).toStartWith('/spapi/data/v1/image/thumbnail/1001/console-iconThumbnailSize/core-scaleImageProportionally');
        expect(iconInfo.iconBackgroundColor).toBe('#ff0000');
    }));

    it('definition icon is selected for the form that does not have an icon set', inject(function (consoleIconService) {
        var form = _getForm();
        form.typeToEditWithForm.typeConsoleBehavior = _getConsoleBehavior();

        var iconInfo = consoleIconService.getNavItemIconUrlAndBackColor(form);
        expect(iconInfo).toBeDefined();
        expect(iconInfo.iconUrl).toBeDefined();
        expect(iconInfo.iconUrl).toStartWith('/spapi/data/v1/image/thumbnail/2001/console-iconThumbnailSize/core-scaleImageProportionally');
        expect(iconInfo.iconBackgroundColor).toBe('#ff00ff');
    }));

    it('type console behavior icon of form type is selected when no icon is set on form or form object', inject(function (consoleIconService) {
        var form = _getForm();
        var type = form.isOfType[0];
        type.setLookup('console:typeConsoleBehavior', _getConsoleBehavior());

        var iconInfo = consoleIconService.getNavItemIconUrlAndBackColor(form);
        expect(iconInfo).toBeDefined();
        expect(iconInfo.iconUrl).toBeDefined();
        expect(iconInfo.iconUrl).toStartWith('/spapi/data/v1/image/thumbnail/2001/console-iconThumbnailSize/core-scaleImageProportionally');
        expect(iconInfo.iconBackgroundColor).toBe('#ff00ff');

        // remeove treeIcon of resource console behavior but leave static treeIconUrl
        type.typeConsoleBehavior.treeIcon = null;

        iconInfo = consoleIconService.getNavItemIconUrlAndBackColor(form);
        expect(iconInfo).toBeDefined();
        expect(iconInfo.iconUrl).toBeDefined();
        expect(iconInfo.iconUrl).toBe('assets/images/nav/behaviorStaticIcon.svg');
        expect(iconInfo.iconBackgroundColor).toBeUndefined();

    }));

    it('default static icon is selected if board and board report object do not have an icon set', inject(function (consoleIconService) {
        var form = _getForm();
        var iconInfo = consoleIconService.getNavItemIconUrlAndBackColor(form);
        expect(iconInfo).toBeDefined();
        expect(iconInfo.iconUrl).toBeDefined();
        expect(iconInfo.iconUrl).toBe('assets/images/default_app.png');
    }));

    // Board tests
    it('icon set on board is selected', inject(function (consoleIconService) {
        var board = _getBoard();

        // set icon on board
        board.setLookup('console:navigationElementIcon', _icon);

        var iconInfo = consoleIconService.getNavItemIconUrlAndBackColor(board);
        expect(iconInfo).toBeDefined();
        expect(iconInfo.iconUrl).toBeDefined();
        expect(iconInfo.iconUrl).toStartWith('/spapi/data/v1/image/thumbnail/1001/console-iconThumbnailSize/core-scaleImageProportionally');
        expect(iconInfo.iconBackgroundColor).toBe('#ff0000');
    }));

    it('definition icon is selected for the board report if board does not have an icon set', inject(function (consoleIconService) {
        var board = _getBoard();
        board.boardReport.reportUsesDefinition.typeConsoleBehavior = _getConsoleBehavior();

        var iconInfo = consoleIconService.getNavItemIconUrlAndBackColor(board);
        expect(iconInfo).toBeDefined();
        expect(iconInfo.iconUrl).toBeDefined();
        expect(iconInfo.iconUrl).toStartWith('/spapi/data/v1/image/thumbnail/2001/console-iconThumbnailSize/core-scaleImageProportionally');
        expect(iconInfo.iconBackgroundColor).toBe('#ff00ff');
    }));

    it('type console behavior icon of board type is selected when no icon is set on board or board report object', inject(function (consoleIconService) {
        var board = _getBoard();
        var type = board.isOfType[0];
        type.setLookup('console:typeConsoleBehavior', _getConsoleBehavior());

        var iconInfo = consoleIconService.getNavItemIconUrlAndBackColor(board);
        expect(iconInfo).toBeDefined();
        expect(iconInfo.iconUrl).toBeDefined();
        expect(iconInfo.iconUrl).toStartWith('/spapi/data/v1/image/thumbnail/2001/console-iconThumbnailSize/core-scaleImageProportionally');
        expect(iconInfo.iconBackgroundColor).toBe('#ff00ff');

        // remeove treeIcon of resource console behavior but leave static treeIconUrl
        type.typeConsoleBehavior.treeIcon = null;

        iconInfo = consoleIconService.getNavItemIconUrlAndBackColor(board);
        expect(iconInfo).toBeDefined();
        expect(iconInfo.iconUrl).toBeDefined();
        expect(iconInfo.iconUrl).toBe('assets/images/nav/behaviorStaticIcon.svg');
        expect(iconInfo.iconBackgroundColor).toBeUndefined();

    }));

    it('default static icon is selected if board and board report object do not have an icon set', inject(function (consoleIconService) {
        var board = _getBoard();
        var iconInfo = consoleIconService.getNavItemIconUrlAndBackColor(board);
        expect(iconInfo).toBeDefined();
        expect(iconInfo.iconUrl).toBeDefined();
        expect(iconInfo.iconUrl).toBe('assets/images/default_app.png');
    }));

    // Chart tests
    it('icon set on chart is selected', inject(function (consoleIconService) {
        var chart = _getChart();

        // set icon on chart
        chart.setLookup('console:navigationElementIcon', _icon);

        var iconInfo = consoleIconService.getNavItemIconUrlAndBackColor(chart);
        expect(iconInfo).toBeDefined();
        expect(iconInfo.iconUrl).toBeDefined();
        expect(iconInfo.iconUrl).toStartWith('/spapi/data/v1/image/thumbnail/1001/console-iconThumbnailSize/core-scaleImageProportionally');
        expect(iconInfo.iconBackgroundColor).toBe('#ff0000');
    }));

    it('definition icon is selected for the chart report if chart does not have an icon set', inject(function (consoleIconService) {
        var chart = _getChart();
        chart.chartReport.reportUsesDefinition.typeConsoleBehavior = _getConsoleBehavior();

        var iconInfo = consoleIconService.getNavItemIconUrlAndBackColor(chart);
        expect(iconInfo).toBeDefined();
        expect(iconInfo.iconUrl).toBeDefined();
        expect(iconInfo.iconUrl).toStartWith('/spapi/data/v1/image/thumbnail/2001/console-iconThumbnailSize/core-scaleImageProportionally');
        expect(iconInfo.iconBackgroundColor).toBe('#ff00ff');
    }));

    it('type console behavior icon of chart type is selected when no icon is set on chart or chart report object', inject(function (consoleIconService) {
        var chart = _getChart();
        var type = chart.isOfType[0];
        type.setLookup('console:typeConsoleBehavior', _getConsoleBehavior());

        var iconInfo = consoleIconService.getNavItemIconUrlAndBackColor(chart);
        expect(iconInfo).toBeDefined();
        expect(iconInfo.iconUrl).toBeDefined();
        expect(iconInfo.iconUrl).toStartWith('/spapi/data/v1/image/thumbnail/2001/console-iconThumbnailSize/core-scaleImageProportionally');
        expect(iconInfo.iconBackgroundColor).toBe('#ff00ff');

        // remeove treeIcon of resource console behavior but leave static treeIconUrl
        type.typeConsoleBehavior.treeIcon = null;

        iconInfo = consoleIconService.getNavItemIconUrlAndBackColor(chart);
        expect(iconInfo).toBeDefined();
        expect(iconInfo.iconUrl).toBeDefined();
        expect(iconInfo.iconUrl).toBe('assets/images/nav/behaviorStaticIcon.svg');
        expect(iconInfo.iconBackgroundColor).toBeUndefined();

    }));

    it('default static icon is selected if chart and chart report object do not have an icon set', inject(function (consoleIconService) {
        var chart = _getChart();
        var iconInfo = consoleIconService.getNavItemIconUrlAndBackColor(chart);
        expect(iconInfo).toBeDefined();
        expect(iconInfo.iconUrl).toBeDefined();
        expect(iconInfo.iconUrl).toBe('assets/images/default_app.png');
    }));
});

