// Copyright 2011-2016 Global Software Innovation Pty Ltd
describe('Reports|View|Dialogs', function () {
    'use strict';

    // Load the modules
    beforeEach(module('mod.common.ui.spSortOptionsDialog'));
    beforeEach(module('app-templates'));
    beforeEach(module('component-templates'));


    afterEach(inject(function ($document) {
        var body = $document.find('body');
        body.find('div.modal').remove();
        body.find('div.modal-backdrop').remove();
        body.removeClass('modal-open');
    }));


    describe('sortOptionsDialog|spec:', function () {
        it('create dialog, show and cancel', inject(function ($rootScope, spSortOptionsDialog) {
            var scope = $rootScope,                
                sortDialogOptions = {
                    columns:[
                        {
                            id: 'C1',
                            name: 'ColumnC1'
                        },
                        {
                            id: 'C2',
                            name: 'ColumnC2'
                        }
                    ],
                    sortInfo: [
                        {
                            columnId: 'C1',
                            sortDirection: 'asc'
                        }
                    ]
                };          

            // Setup dialog options columns           
            spSortOptionsDialog.showModalDialog(sortDialogOptions).then(function (result) {
                expect(result).toBe(false);
            });

            scope.$digest();

            expect(scope.$$childHead.$$childHead.model.columns.length).toBe(sortDialogOptions.columns.length);
            expect(scope.$$childHead.$$childHead.model.sortInfo.length).toBe(1);
            expect(scope.$$childHead.$$childHead.model.sortInfo[0].column.id).toBe('C1');

            scope.$$childHead.$$childHead.cancel();

            scope.$digest();
        }));

        
        it('create dialog, show and ok no changes', inject(function ($rootScope, spSortOptionsDialog) {
            var scope = $rootScope,                
                sortDialogOptions = {
                    columns: [
                        {
                            id: 'C1',
                            name: 'ColumnC1'
                        },
                        {
                            id: 'C2',
                            name: 'ColumnC2'
                        }
                    ],
                    sortInfo: [
                        {
                            columnId: 'C1',
                            sortDirection: 'asc'
                        }
                    ]
                };

            // Setup dialog options columns           
            spSortOptionsDialog.showModalDialog(sortDialogOptions).then(function (result) {
                expect(_.isArray(result)).toBe(true);
                expect(result.length).toBe(1);
                expect(result[0].columnId).toBe('C1');
                expect(result[0].sortDirection).toBe('asc');
            });
            
            scope.$digest();

            expect(scope.$$childHead.$$childHead.model.columns.length).toBe(sortDialogOptions.columns.length);
            expect(scope.$$childHead.$$childHead.model.sortInfo.length).toBe(1);
            expect(scope.$$childHead.$$childHead.model.sortInfo[0].column.id).toBe('C1');

            scope.$$childHead.$$childHead.ok();

            scope.$digest();
        }));


        it('create dialog, show and ok add new sorted column', inject(function ($rootScope, spSortOptionsDialog) {
            var scope = $rootScope,                
                sortDialogOptions = {
                    columns: [
                        {
                            id: 'C1',
                            name: 'ColumnC1'
                        },
                        {
                            id: 'C2',
                            name: 'ColumnC2'
                        }
                    ],
                    sortInfo: [
                        {
                            columnId: 'C1',
                            sortDirection: 'asc'
                        }
                    ]
                };

            // Setup dialog options columns           
            spSortOptionsDialog.showModalDialog(sortDialogOptions).then(function (result) {
                expect(_.isArray(result)).toBe(true);
                expect(result.length).toBe(2);
                expect(result[0].columnId).toBe('C1');
                expect(result[0].sortDirection).toBe('asc');

                expect(result[1].columnId).toBe('C2');
                expect(result[1].sortDirection).toBe('desc');
            });

            scope.$digest();

            expect(scope.$$childHead.$$childHead.model.columns.length).toBe(sortDialogOptions.columns.length);
            expect(scope.$$childHead.$$childHead.model.sortInfo.length).toBe(1);
            expect(scope.$$childHead.$$childHead.model.sortInfo[0].column.id).toBe('C1');
            expect(scope.$$childHead.$$childHead.canAddSortInfo()).toBe(true);

            scope.$$childHead.$$childHead.addSortInfo();
            expect(scope.$$childHead.$$childHead.canAddSortInfo()).toBe(false);

            expect(scope.$$childHead.$$childHead.model.sortInfo.length).toBe(2);
            scope.$$childHead.$$childHead.model.sortInfo[1].column = scope.$$childHead.$$childHead.model.columns[1];
            scope.$$childHead.$$childHead.model.sortInfo[1].sortDirection = 'Descending';

            scope.$$childHead.$$childHead.ok();

            scope.$digest();
        }));


        it('create dialog, show and ok remove existing sorted column', inject(function ($rootScope, spSortOptionsDialog) {
            var scope = $rootScope,
                sortDialogOptions = {
                    columns: [
                        {
                            id: 'C1',
                            name: 'ColumnC1'
                        },
                        {
                            id: 'C2',
                            name: 'ColumnC2'
                        }
                    ],
                    sortInfo: [
                        {
                            columnId: 'C1',
                            sortDirection: 'asc'
                        }
                    ]
                };

            // Setup dialog options columns           
            spSortOptionsDialog.showModalDialog(sortDialogOptions).then(function (result) {
                expect(_.isArray(result)).toBe(true);
                expect(result.length).toBe(0);
            });

            scope.$digest();

            expect(scope.$$childHead.$$childHead.model.columns.length).toBe(sortDialogOptions.columns.length);
            expect(scope.$$childHead.$$childHead.model.sortInfo.length).toBe(1);
            expect(scope.$$childHead.$$childHead.model.sortInfo[0].column.id).toBe('C1');

            scope.$$childHead.$$childHead.removeSortInfo(0);

            expect(scope.$$childHead.$$childHead.model.sortInfo.length).toBe(0);            

            scope.$$childHead.$$childHead.ok();

            scope.$digest();
        }));


        it('create dialog, show and ok add new invalid sorted column', inject(function ($rootScope, spSortOptionsDialog) {
            var scope = $rootScope,
                sortDialogOptions = {
                    columns: [
                        {
                            id: 'C1',
                            name: 'ColumnC1'
                        },
                        {
                            id: 'C2',
                            name: 'ColumnC2'
                        }
                    ],
                    sortInfo: [
                        {
                            columnId: 'C1',
                            sortDirection: 'asc'
                        }
                    ]
                };

            // Setup dialog options columns           
            spSortOptionsDialog.showModalDialog(sortDialogOptions, null, null);

            scope.$digest();

            expect(scope.$$childHead.$$childHead.model.columns.length).toBe(sortDialogOptions.columns.length);
            expect(scope.$$childHead.$$childHead.model.sortInfo.length).toBe(1);
            expect(scope.$$childHead.$$childHead.model.sortInfo[0].column.id).toBe('C1');

            scope.$$childHead.$$childHead.addSortInfo();

            // Add new sort info without column
            expect(scope.$$childHead.$$childHead.model.sortInfo.length).toBe(2);            
            scope.$$childHead.$$childHead.model.sortInfo[1].sortDirection = 'Descending';

            scope.$$childHead.$$childHead.ok();

            scope.$digest();

            expect(scope.$$childHead.$$childHead.model.errors.length).toBe(1);

            scope.$$childHead.$$childHead.cancel();

            scope.$digest();
        }));


        it('create dialog, show and ok add new duplicate sorted column', inject(function ($rootScope, spSortOptionsDialog) {
            var scope = $rootScope,
                sortDialogOptions = {
                    columns: [
                        {
                            id: 'C1',
                            name: 'ColumnC1'
                        },
                        {
                            id: 'C2',
                            name: 'ColumnC2'
                        }
                    ],
                    sortInfo: [
                        {
                            columnId: 'C1',
                            sortDirection: 'asc'
                        }
                    ]
                };

            // Setup dialog options columns           
            spSortOptionsDialog.showModalDialog(sortDialogOptions, null, null);

            scope.$digest();

            expect(scope.$$childHead.$$childHead.model.columns.length).toBe(sortDialogOptions.columns.length);
            expect(scope.$$childHead.$$childHead.model.sortInfo.length).toBe(1);
            expect(scope.$$childHead.$$childHead.model.sortInfo[0].column.id).toBe('C1');

            scope.$$childHead.$$childHead.addSortInfo();

            // Add new sort info with duplicate column
            expect(scope.$$childHead.$$childHead.model.sortInfo.length).toBe(2);
            scope.$$childHead.$$childHead.model.sortInfo[1].column = scope.$$childHead.$$childHead.model.columns[0];
            scope.$$childHead.$$childHead.model.sortInfo[1].sortDirection = 'Descending';

            scope.$$childHead.$$childHead.ok();

            scope.$digest();

            expect(scope.$$childHead.$$childHead.model.errors.length).toBe(1);

            scope.$$childHead.$$childHead.cancel();

            scope.$digest();
        }));
    });
});