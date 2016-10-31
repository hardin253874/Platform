// Copyright 2011-2016 Global Software Innovation Pty Ltd
/*global _, angular, console, module, describe, beforeEach, it, xit, inject, expect, runs, waitsFor, TestSupport, sp, spEntity, spReportEntityQueryManager */

describe('rnBoardService|intg:', function () {
    "use strict";

    beforeEach(module('mod.app.board'));

    beforeEach(inject(function ($injector) {
        TestSupport.setupIntgTests(this, $injector);
    }));

    xit('rnBoardService devonly - create default report', inject(function (spEntityService) {
        var rootEntity = spReportEntityQueryManager.createRootEntity('core:board');
        var reportColumns = spReportEntityQueryManager.createInitializeReportColumns(rootEntity, 'core:name', 'Name');
        var reportConditions = spReportEntityQueryManager.createInitializeReportCondition(rootEntity, 'core:name', 'Name');

        var report = spEntity.fromJSON({
            typeId: 'report',
            name: 'Boards',
            description: '',
            rootNode: rootEntity,
            reportColumns: reportColumns,
            hasConditions: reportConditions,
            reportUsesDefinition: { id: 'core:board' },
            isDefaultDisplayReportForTypes: jsonRelationship(),
            isDefaultPickerReportForTypes: jsonRelationship()
        });

        var promise = spEntityService.getEntityByNameAndTypeName('ReadiNow Core Data', 'Application', 'name').then(function (e) {
            report.setField('alias', 'boardsReport', 'String');
            report.setLookup('inSolution', e.idP);
            return spEntityService.putEntity(report);
        });

        var results = {};
        TestSupport.waitCheckReturn(promise, results);

        runs(function () {
            expect(results).toBeTruthy();
            expect(results.value).toBeTruthy();

        });

    }));

    it('rnBoardService', inject(function ($q, rnBoardService, spEntityService) {
        var results = {};

        expect(rnBoardService).toBeTruthy();

        var board = spEntity.fromJSON({
            typeId: 'board',
            name: 'temp board for test case',
            boardStyleDimension: {
                typeId: 'boardDimension'
            }
        });

        var promise = spEntityService.putEntity(board);

        TestSupport.waitCheckReturn(promise, results);

        runs(function () {
            expect(results).toBeTruthy();
            expect(results.value).toBeTruthy();

        });
    }));
});

