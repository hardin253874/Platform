// Copyright 2011-2016 Global Software Innovation Pty Ltd


(function () {
    'use strict';

    /**
    * Module implementing a dialog for creating a new type (definition/object/whatever).
    *
    * @module spNewTypeDialog
    */
    angular.module('mod.app.chartBuilder.controllers.spNewChartDialog', [
        'sp.navService',
        'mod.common.ui.spDialogService',
        'mod.app.editForm.designerDirectives.spEditForm',
        'mod.app.chartBuilder.services.spChartBuilderService',
        'sp.spNavHelper',
        'mod.common.ui.spReportPicker',
        'mod.common.ui.spMeasureArrange',
        'app.editForm.customEditFormController',
        'mod.common.ui.spBusyIndicator',
        'sp.app.settings'
    ])
        .controller('spNewChartDialogController', function ($q, $scope, $timeout, $uibModalInstance,
                                                            spEntityService, spChartBuilderService, spNavService,
                                                            spEditForm, options, spMeasureArrangeService, spAppSettings,
                                                            spAlertsService) {

            $scope.isCollapsed = true;
            ////get option text 
            $scope.getOptionsText = function () {
                if ($scope.isCollapsed === true) {
                    $scope.imageUrl = 'assets/images/arrow_down.png';
                    return 'Options';
                }
                else {
                    $scope.imageUrl = 'assets/images/arrow_up.png';
                    return 'Options';
                }
            };

            // Setup the dialog model
            $scope.model = {
                title: 'New Chart',
                errors: [],
                isFormValid: true,
                formMode: spEditForm.formModes.edit,
                formControl: null,
                chartModel: spChartBuilderService.createChartModel(),
                chartType: null,
                busyIndicator: {
                    type: 'spinner',
                    text: 'Please wait...',
                    placement: 'element',
                    isBusy: false
                },
                reportPickerOptions: {
                    selectedEntityId: null,
                    selectedEntity: null,
                    selectedEntities: null,
                    pickerReportId: 'console:reportsReport',
                    entityTypeId: 'core:report',
                    multiSelect: false,
                    isDisabled: false,
                    reportOptions: { }
                },
                applicationPickerOptions: {
                    selectedEntityId: null,
                    selectedEntity: null,
                    selectedEntities: null,
                    pickerReportId: 'core:applicationsPickerReport',
                    entityTypeId: 'core:solution',
                    multiSelect: true,
                    isDisabled: false
                },
                iconPickerOptions: {
                    selectedEntityId: null,
                    selectedEntity: null,
                    selectedEntities: null,
                    pickerReportId: null,
                    entityTypeId: 'core:iconFileType',
                    multiSelect: false,
                    isDisabled: false
                },
                existingChart: false,
                visualSettingsOptions: {
                    enableOnDesktop: true,
                    enableOnTablet: false,
                    enableOnMobile: false
                },
                showAdvanced: spAppSettings.fullConfig, // only full admin can see advanced properties (esp 'in app', and 'custom form')
                formatTabActiveByDefault: !spAppSettings.fullConfig
            };

            $scope.measureArrangeOptions = {
                id: 'newChartDialog'
            };

            if (options.typeId) {
                $scope.model.reportPickerOptions.reportOptions.relationDetail = {
                    eid: options.typeId,
                    relid: 'reportUsesDefinition',
                    direction: 'fwd'
                };

                // Fill in default reports
                spEntityService.getEntity(options.typeId, 'defaultDisplayReport.id, defaultPickerReport.id', { batch: 'true', hint: 'defaultReports' }).then(function (type) {
                    var reportId = sp.result($scope, 'model.reportPickerOptions.selectedEntities.0.idP');
                    var defReport = sp.result(type, 'defaultDisplayReport.idP') || sp.result(type, 'defaultPickerReport.idP');
                    if (defReport && !reportId) {
                        $scope.model.reportPickerOptions.selectedEntities = [spEntity.fromId(defReport)];
                    }
                });
            }

            //Load more data to set picker options.
            var ids = ['core:templateReport', 'core:iconFileType', 'core:solution', 'core:applicationsPickerReport'];

            spEntityService.getEntities(ids, 'name', { batch: 'true', hint: 'pickerOptionsIds' }).then(function (entities) {
                if (entities) {
					var applicationsPickerReport = entities[3];
					$scope.model.applicationPickerOptions.pickerReportId = applicationsPickerReport ? applicationsPickerReport.id() : entities[0].id();
                    $scope.model.applicationPickerOptions.entityTypeId = entities[2].id();
                    $scope.model.iconPickerOptions.pickerReportId = entities[0].id();
                    $scope.model.iconPickerOptions.entityTypeId = entities[1].id();
                }
            });

            // Load the current application.
            var applicationId = spNavService.getCurrentApplicationId();
            spEntityService.getEntity(applicationId, 'name,description', { batch: 'true', hint: 'currentAppInfo' }).then(function (entity) {
                if (entity) {
                    $scope.model.applicationPickerOptions.selectedEntityId = applicationId;
                    $scope.model.applicationPickerOptions.selectedEntities = [entity];
                }
            });


            var formId = 'core:resourceForm';

            spEditForm.getFormDefinition(formId).then(
                    function (formControl) {
                        formControl.name = '';
                        $scope.model.formControl = formControl;
                        var appId = spNavService.getCurrentApplicationId();
                        if (appId) {
                            $scope.model.chartModel.chart.setInSolution(appId);
                        }
                        $timeout(function () {
                            spMeasureArrangeService.performLayout('newChartDialog');
                        });
                    },
                    function (error) {
                        raiseErrorAlert('Error while trying to get form:' + error);
                    });

            // Allow report to be passed in
            var reportId = sp.result(options, 'reportId');
            $scope.model.chartModel.chart.chartReport = reportId;
            $scope.model.reportPickerOptions.selectedEntityId = reportId;
            if (reportId) {
                // Determine a suitable default chart type
                spChartBuilderService.loadReportMetadata($scope.model.chartModel)
                    .then(function () {
                        // note: the chart name gets updated in the entity model, but the edit form does not seem to update
                        spChartBuilderService.applySuggestions($scope.model.chartModel);
                        $scope.model.chartType = $scope.model.chartModel.chart.chartHasSeries[0].chartType.eidP.getAlias();
                        $scope.model.reportPickerOptions.selectedEntities = [$scope.model.chartModel.chart.chartReport];
                });
            }

            // OK click handler 
            $scope.ok = function () {

                $scope.model.errors = [];

                // Check form is valid
                if (!spEditForm.validateForm($scope.model.formControl, $scope.model.chartModel.chart))
                    return;
                //Set report
                var reportId = sp.result($scope, 'model.reportPickerOptions.selectedEntities.0.idP');
                if (!reportId) {
                    $scope.model.errors.push({ msg: 'Please select a report.' });
                    return;
                }
                $scope.model.chartModel.chart.chartReport = reportId;
              
                $scope.model.chartModel.chart.hideOnDesktop = !$scope.model.visualSettingsOptions.enableOnDesktop;
                $scope.model.chartModel.chart.hideOnTablet = !$scope.model.visualSettingsOptions.enableOnTablet;
                $scope.model.chartModel.chart.hideOnMobile = !$scope.model.visualSettingsOptions.enableOnMobile;

                // Load report metadata
                // (This will apply automatic suggestions)
                var chartModel = $scope.model.chartModel;
                $scope.model.busyIndicator.isBusy = true;
                spChartBuilderService.loadReportMetadata(chartModel)
                    .then(function () {
                        spChartBuilderService.applySuggestions(chartModel, $scope.model.chartType);

                        if (options.folder) {
                            chartModel.chart.setRelationship('console:resourceInFolder', [options.folder]);
                        }

                        //Set Applications
                        var applications = sp.result($scope, 'model.applicationPickerOptions.selectedEntities');
                        if (applications != null && applications.length > 0) {
                            $scope.model.chartModel.chart.setLookup('core:inSolution', applications[0]);
                        } else {
                            $scope.model.chartModel.chart.setLookup('core:inSolution', null);
                        }

                        //Set Chart Icon
                        var iconEntity = sp.result($scope, 'model.iconPickerOptions.selectedEntities.0');
                        $scope.model.chartModel.chart.setLookup('console:navigationElementIcon', iconEntity || null);
                                            
                        // Save the chart
                        return spChartBuilderService.saveChart(chartModel).then(function (chartId) {
                            $scope.model.busyIndicator.isBusy = false;
                            return chartId;
                        }, function () {
                            $scope.model.busyIndicator.isBusy = false;
                        });
                    }, function () {
                        $scope.model.busyIndicator.isBusy = false;
                    })
                    .then(function (chartId) {
                        $uibModalInstance.close({ chartId: chartId, chartEntity: chartModel.chart });
                    });
            };

            // Cancel click handler
            $scope.cancel = function () {
                $uibModalInstance.close(null);
            };

            $scope.$on('calcTabsLayout', function (event, callback) {
                event.stopPropagation();
                callback($scope.measureArrangeOptions.id);
            });

            function raiseErrorAlert(message) {
                spAlertsService.addAlert(message, { severity: spAlertsService.sev.Error });
            }
        })
        .service('spNewChartDialog', function (spDialogService, spNavService) {
            // setup the dialog
            var exports = {
                showDialog: function (options) {
                    options = options || {};

                    var dialogOptions = {
                        title: 'New Chart',
                        templateUrl: 'chartBuilder/controllers/newChartDialog/spNewChartDialog.tpl.html',
                        controller: 'spNewChartDialogController',
                        resolve: {
                            options: function () {
                                return options;
                            }
                        }
                    };

                    return spDialogService.showModalDialog(dialogOptions).then(function (result) {                    
                        if (result &&
                            !options.preventBuilderNavigation) {
                            spNavService.navigateToChildState('chartBuilder', result.chartId);
                        }

                        return result;
                    });
                }
            };

            return exports;
        });
}());