// Copyright 2011-2016 Global Software Innovation Pty Ltd
/*global angular, sp*/

// Main wizard for configuring a spreadsheet import

(function () {
    'use strict';

    angular.module('mod.app.importFile.controllers.importFileWizard', [
        'mod.app.importFile.services.spImportFileService',
        'mod.app.importFile.controllers.spMemberMappingOptions',
        'ui.router',
        'sp.app.navigation'
    ])
        .controller('importFileWizardController', ImportFileWizardController);

    /* @ngInject */
    function ImportFileWizardController($scope, $stateParams, $q,
                                        $templateCache, spNavService, spImportFileService, navDirtyMessage) {

        var importConfigId = sp.coerseToNumberOrLeaveAlone($stateParams.eid) || 0;

        // Attempt to restore chart from nav item
        var navItem = spNavService.getCurrentItem();
        var model = $scope.model = sp.result(navItem, 'data.importConfigModel');
        $scope.svc = spImportFileService;
        $scope.navItem = navItem;

        // Define mapping grid
        $scope.mappingGridOptions = {
            data: 'model.mappingTable',
            multiSelect: false,
            enableSorting: false,
            columnDefs: [
                {
                    field: 'spreadsheetColumnName',
                    displayName: 'Spreadsheet Column',
                    sortable: false,
                    groupable: false
                },
                {
                    displayName: 'Object Fields',
                    sortable: false,
                    groupable: false,
                    cellTemplate: $templateCache.get('importFile/views/mappingCell.tpl.html')
                },
                {
                    displayName: 'Field Details',
                    sortable: false,
                    groupable: false,
                    cellTemplate: $templateCache.get('importFile/views/fieldDetailsCell.tpl.html')
                },
                {
                    field: 'sample1',
                    displayName: 'Sample 1',
                    sortable: false,
                    groupable: false
                },
                {
                    field: 'sample2',
                    displayName: 'Sample 2',
                    sortable: false,
                    groupable: false
                }
            ]
        };

        var promise = $q.when();

        if (!$scope.model) {
            // Something to get us started until the callbacks run
            model = $scope.model = spImportFileService.createEmptyModel();

            // Create/load chart model
            promise = spImportFileService.createModel(importConfigId);

            // Store model
            promise = promise.then(function (newModel) {
                model = $scope.model = newModel;
                navItem.data = navItem.data || {};
                navItem.data.importConfigModel = model;
            });
        }

        function initialize() {
            // Page dirty handler
            navItem.isDirty = function () {                
                return spImportFileService.hasUnsavedChanges(model, !model.initialBookmark);
            };

            // Page dirty message
            navItem.dirtyMessage = function dirtyMessage() {                
                if (spImportFileService.hasUnsavedChanges(model, !model.initialBookmark))
                    return navDirtyMessage.defaultMsg;
                return undefined;
            };
        }

       $scope.$watch('model.typePickerOptions.selectedEntities', function () {
            if (!model.typePickerOptions)
                return;

            var typeEntity = sp.result(model, 'typePickerOptions.selectedEntities.0');
            if (!typeEntity || !typeEntity.idP)
                return;

            spImportFileService.setSelectedType(model, typeEntity);
        });

        // Page 1
        $scope.headingRowChanged = function headingRowChanged() {
            spImportFileService.headingRowChanged(model);
        };

        $scope.dataRowChanged = function dataRowChanged() {
            spImportFileService.dataRowChanged(model);
        };

        $scope.noHeadingChanged = function noHeadingChanged() {
            spImportFileService.noHeadingChanged(model);
        };

        $scope.separatorChanged = function separatorChanged() {
            spImportFileService.separatorChanged(model);
        };

        $scope.lastRowChanged = function lastRowChanged() {
            spImportFileService.lastRowChanged(model);
        };

        $scope.$watch('model.sheetName', function (newSheet, oldSheet) {
            if (!oldSheet)
                return;
            spImportFileService.sheetChanged(model);
        });

        // NAVIGATION WIZARD

        // Cannot click restart button while import is in progress
        $scope.isBackButtonDisabled = function () {
            return sp.result(model, 'importStatus.importStatus') === 'inProgress';
        };

        // Disable the cancel button if the import process is cancelled or completed
        $scope.isNextButtonDisabled = function () {
            return !spImportFileService.isPageValid(model);
        };

        $scope.isCurrentPage = function (pageIndex) {
            return model.nav.curPage === pageIndex;
        };

        $scope.isFirstPage = function isFirstPage() {
            return model.nav.curPage === 1;
        };

        function isLastPage() {
            return model.nav.curPage === model.nav.importPage;
        }

        function isSecondLastPage() {
            return model.nav.curPage + 1 === model.nav.importPage;
        }

        $scope.getNextLabel = function () {
            if (isSecondLastPage()) {
                return 'Import';
            } else if (isLastPage()) {
                return model.importRunning ? 'Cancel' : 'Done';
            } else {
                return 'Next';
            }
        };

        $scope.getBackLabel = function () {
            return isLastPage() ? 'Restart' : 'Back';
        };

        $scope.handlePrevious = function () {
            if (sp.result(model, 'importStatus.importStatus') === 'inProgress')
                return;

            if (model.nav.curPage === model.nav.importPage) {
                spImportFileService.restartWizard(model);
            } else if (model.nav.curPage > 1) {
                model.nav.curPage--;
            }
        };

        $scope.handleNext = function () {
            var nextPage = model.nav.curPage + 1;
            if (!spImportFileService.isPageValid(model))
                return;
            if (nextPage === model.nav.mappingPage) {
                spImportFileService.prepareMappings(model);
                model.nav.curPage = nextPage;
            } else if (nextPage === model.nav.importPage) {
                spImportFileService.startImport(model, true);
                model.nav.curPage = nextPage;
            } else if (isLastPage()) {
                spImportFileService.cancelImport(model);
            } else {
                model.nav.curPage = nextPage;
            }
        };

        $scope.saveConfig = function () {
            spImportFileService.saveAndReload(model);
        };

        // END NAVIGATION

        initialize();
    }

}());
