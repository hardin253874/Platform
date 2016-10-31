// Copyright 2011-2016 Global Software Innovation Pty Ltd
/*global _, angular, console, module, describe, beforeEach, it, inject, expect, runs, waitsFor, TestSupport,
 sp, spEntity, jsonString, jsonBool, jsonLookup,
 rnBoardTestData */

describe('rnBoardService|spec:', function () {
    'use strict';

    beforeEach(module('mod.app.board'));
    beforeEach(module('mockedEntityService'));
    beforeEach(module('mockedReportService'));

    beforeEach(inject(function ($injector, $rootScope) {
        TestSupport.setupUnitTests(this, $injector);
    }));

    beforeEach(inject(function ($q, spEntityService) {
        // patch in a mock putEntity
        let newEntityId = 99999; // use this if the put entity has id 0
        spEntityService.putEntity = entity => $q.when(entity && (entity.idP || newEntityId));
    }));

    beforeEach(inject(function (spEntityService, spReportService) {
        _.forEach([
            rnBoardTestData.boardEntity,
            rnBoardTestData.teamMemberEntity,
            rnBoardTestData.accountEntity,
            rnBoardTestData.projectEntity,
            rnBoardTestData.kanbanStateEntity,
            rnBoardTestData.kanbanStateColumn,
            rnBoardTestData.itemTypeColumn,
            rnBoardTestData.categoryColumn,
            rnBoardTestData.priorityColumn,
            rnBoardTestData.createdByColumn,
            rnBoardTestData.lastModifiedByColumn,
            rnBoardTestData.ownerColumn,
            rnBoardTestData.projectColumn
        ], spEntityService.mockGetEntity);
        _.forEach([
            [50835, rnBoardTestData.storiesResults],
            [81579, rnBoardTestData.kanbanStatesResults],
            [29643, rnBoardTestData.projectResults],
            [38726, rnBoardTestData.teamMemberResults],
            [72756, rnBoardTestData.tasksReport]
        ], ([id, data]) => spReportService.mockGetReportData(id, data));
    }));

    it('returns a service object', inject(function (rnBoardService) {
        expect(rnBoardService).toBeTruthy();
    }));

    it('the service has all expected functions', inject(function (rnBoardService) {
        let fns = [
            'quickAddEntity',
            'getRootTypeId',
            'isAdmin',
            'getFilteredItems',
            'requestModel',
            'saveBoard',
            'sendEntityUpdate',
            'updateBoardDimension',
            'updateBoardReport',
            'updateCardTemplate',
            'updateChildBoard',
            'updateChildBoardReport',
            'updateEntityDimensions',
            'updateShowQuickAdd'
        ];
        fns.map(f => expect(_.isFunction(rnBoardService[f])).toBeTruthy());
    }));

    it('the isAdmin function completes', inject(function (rnBoardService) {
        let {isAdmin} = rnBoardService;
        expect(isAdmin()).toBeFalsy();
    }));

    describe('the requestModel function', function () {

        it('returns a promise', inject(function (rnBoardService) {
            let {requestModel} = rnBoardService;
            let {boardEntity} = rnBoardTestData;
            let promise = requestModel(boardEntity.idP, {});
            expect(promise).toBePromise();
        }));

        it('returns a board entity', inject(function (rnBoardService) {
            let {requestModel} = rnBoardService;
            let {boardEntity} = rnBoardTestData;
            let results = {};
            let promise = requestModel(boardEntity.idP, {});
            TestSupport.waitCheckReturn(promise, results);

            runs(() => {
                expect(results.value).toBeTruthy();

                let {board} = results.value;
                expect(board).toBeEntity();
                _.forEach(['boardReport', 'boardColumnDimension',
                        'boardChildReport', 'drilldownTargetBoard',
                        'boardSwimlaneDimension', 'boardStyleDimension'
                    ],
                    name => { expect(board[name]).toBeEntity(name); });
                _.forEach([],
                    name => { expect(!board[name]).toBeTruthy(name); });
            });
        }));

        it('returns its report\'s results', inject(function (rnBoardService) {
            let {requestModel} = rnBoardService;
            let {boardEntity} = rnBoardTestData;
            let results = {};
            let promise = requestModel(boardEntity.idP, {});
            TestSupport.waitCheckReturn(promise, results);

            runs(() => {
                expect(results.value).toBeTruthy();
                expect(results.value.meta).toBeTruthy('meta');
                expect(results.value.data).toBeTruthy('data');
            });
        }));


    });

    it('the sendEntityUpdate function completes without error', inject(function (spEntityService, rnBoardService) {
        let {sendEntityUpdate} = rnBoardService;
        let {testEntity} = rnBoardTestData;
        let updates = [
            {
                relOrChoiceId: 'otherHalf',
                existingValId: testEntity.otherHalf.idP,
                newValId: jsonLookup(null)
            }
        ];

        spEntityService.mockGetEntity(testEntity);

        let promise = sendEntityUpdate(testEntity.idP, updates);

        expect(promise).toBePromise();

        var results = {};
        TestSupport.waitCheckReturn(promise, results);

        runs(function () {
            expect(results.value).toBeTruthy();
        });
    }));

    it('the quickAddEntity function completes without error', inject(function (spEntityService, rnBoardService) {
        let {requestModel, quickAddEntity} = rnBoardService;
        let {boardEntity} = rnBoardTestData;

        let promise = requestModel(boardEntity.idP, {})
            .then(model => quickAddEntity(model, "a new item"));

        var results = {};
        TestSupport.waitCheckReturn(promise, results);

        runs(function () {
            expect(results.value).toBeTruthy();
            expect(results.value).toBeGreaterThan(0);
        });
    }));

    it('the saveBoard function completes without error', inject(function (spEntityService, rnBoardService) {
        let {requestModel, saveBoard} = rnBoardService;
        let {boardEntity} = rnBoardTestData;

        let promise = requestModel(boardEntity.idP, {})
            .then(model => saveBoard(model));

        var results = {};
        TestSupport.waitCheckReturn(promise, results);

        runs(function () {
            expect(results.value).toBeTruthy();
            expect(results.value).toBeGreaterThan(0);
        });
    }));

    it('structureViewToString function works', inject(function (rnBoardService) {
        let {structureViewToString} = rnBoardService;
        const STX = '\u0002'; // separates the components in a path
        const ETX = '\u0003'; // separates the paths
        let data = [
            [null, ''],
            ['', ''],
            [`87956:Administration${ETX}91194:Corporate${STX}87956:Administration`, 'Corporate > Administration']
        ];

        _.forEach(data, ([inp,outp]) => {
            expect(structureViewToString(inp)).toBe(outp);
        });
    }));

});

