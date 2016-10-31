// Copyright 2011-2016 Global Software Innovation Pty Ltd
(function () {
    'use strict';

    /**
    * Module implementing a dialog for specifying the name and description and icon of a navigation element.
    *
    * @module spNavigationElementDialog
    */
    angular.module('mod.app.navigation.spNavigationElementDialog', [
        'ui.bootstrap',
        'mod.common.ui.spDialogService',
        'mod.common.ui.spMeasureArrange',
        'mod.common.spEntityService',
        'mod.app.editForm.designerDirectives.spEditForm',
        'sp.navService',
        'sp.app.settings'
    ])
        .controller('spNavigationElementDialogController', function ($scope, $timeout, $uibModalInstance, options, spEditForm, spEntityService, spMeasureArrangeService, spNavService, spAppSettings) {

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
                title: options.title || 'Item Properties',
                errors: [],
                isFormValid: true,
                formMode: spEditForm.formModes.edit,
                entity: options.entity,
                applicationPickerOptions: {
                    selectedEntityId: null,
                    selectedEntity: null,
                    selectedEntities: null,
                    pickerReportId: 'core:applicationsPickerReport',
                    entityTypeId: 'core:solution',
                    multiSelect: true,
                    isDisabled: false,
                },
                iconPickerOptions: {
                    selectedEntityId: null,
                    selectedEntity: null,
                    selectedEntities: null,
                    pickerReportId: null,
                    entityTypeId: 'core:iconFileType',
                    multiSelect: false,
                    isDisabled: false,
                },
                visualSettingsOptions: {
                    enableOnDesktop: !options.entity.hideOnDesktop,
                    enableOnTablet: !options.entity.hideOnTablet,
                    enableOnMobile: !options.entity.hideOnMobile,
                    isAppilication: options.isAppilication ? options.isAppilication : false,
                },
                formControl: null,
                busyIndicator: {
                    type: 'spinner',
                    text: 'Please wait...',
                    placement: 'element',
                    isBusy: false
                },
                showAdvanced: spAppSettings.fullConfig, // only full admin can see advanced properties (esp 'in app', and 'custom form')
                formatTabActiveByDefault: !spAppSettings.fullConfig
            };

            $scope.measureArrangeOptions = {
                id: 'navigationElementDialog'
            };

            // Bookmarks
            var entityBookmark;

            if (options.entity) {
                entityBookmark = options.entity.graph.history.addBookmark('Form Data Properties');

                //Set application
                if (options.entity.inSolution && options.entity.inSolution.name) {
                    $scope.model.applicationPickerOptions.selectedEntities = [options.entity.inSolution];
                }
                else if (options.entity.dataState === "create") //only insert current application into applicationPickerOption
                {
                    var currentAppId = spNavService.getCurrentApplicationId();
                    if (currentAppId) {
                        spEntityService.getEntity(currentAppId, 'name,description', { batch: 'true', hint: 'appInfo' }).then(function (entity) {
                            if (entity) {
                                $scope.model.applicationPickerOptions.selectedEntities = [entity];
                            }
                        });
                    }
                }

                //Set Navigation element Icon
                if (options.entity.navigationElementIcon)
                    $scope.model.iconPickerOptions.selectedEntities = [options.entity.navigationElementIcon];

                //Load more data to set picker options.
                var ids = ['core:templateReport', 'core:iconFileType', 'core:solution', 'core:applicationsPickerReport'];

                spEntityService.getEntities(ids, 'id', { batch: 'true', hint: 'pickerOptionsIds' }).then(function (entities) {
                    if (entities) {
                        var applicationsPickerReport = entities[3];
                        $scope.model.applicationPickerOptions.pickerReportId = applicationsPickerReport ? applicationsPickerReport.id() : entities[0].id();
                        $scope.model.applicationPickerOptions.entityTypeId = entities[2].id();
                        $scope.model.iconPickerOptions.pickerReportId = entities[0].id();
                        $scope.model.iconPickerOptions.entityTypeId = entities[1].id();
                    }
                });
            }

            var formId = 'core:resourceForm';


            $scope.$watch('model.entity.name', function () {
                $scope.model.errors = [];
            });


            $scope.$watch('model.entity.description', function () {
                $scope.model.errors = [];
            });           


            spEditForm.getFormDefinition(formId).then(
                    function (formControl) {
                        formControl.name = '';
                        $scope.model.formControl = formControl;

                        $timeout(function () {


                            spMeasureArrangeService.performLayout('navigationElementDialog');
                        });
                    },
                    function (error) {
                        console.error('Error while trying to get form:' + error);
                    });            


            // OK click handler
            $scope.ok = function () {
                $scope.model.errors = [];
                $scope.model.isFormValid = true;

                //before validate form, check find any another document folder with the same name in this folder or not
                //this validate function will be removed after we set back the folderUniqueNameKey later
                if ($scope.model.entity && $scope.model.entity.resourceInFolder && $scope.model.entity.resourceInFolder.length > 0) {
                    var duplicateNameInFolder = spNavService.duplicateNameInFolder($scope.model.entity.resourceInFolder[0].id(), 'documentFolder', $scope.model.entity.name);

                    if (duplicateNameInFolder === true) {
                        $scope.model.isFormValid = false;
                        $scope.model.errors.push({ msg: 'Fail to create document folder, There is another documentFolder with the same name in this folder.' });
                        return null;
                    }
                }

                if (spEditForm.validateForm($scope.model.formControl, $scope.model.entity)) {
                    $scope.model.busyIndicator.isBusy = true;

                    //Set Applications
                    var applications = sp.result($scope, 'model.applicationPickerOptions.selectedEntities');

                    if (applications != null && applications.length > 0) {
                        $scope.model.entity.setLookup('core:inSolution', applications[0]);
                    } else {
                        $scope.model.entity.setLookup('core:inSolution', null);
                    }

                    //Set navigation element Icon
                    var iconEntity = sp.result($scope, 'model.iconPickerOptions.selectedEntities.0');
                    $scope.model.entity.setLookup('console:navigationElementIcon', iconEntity || null);

                    $scope.model.entity.hideOnDesktop = !$scope.model.visualSettingsOptions.enableOnDesktop;
                    $scope.model.entity.hideOnTablet = !$scope.model.visualSettingsOptions.enableOnTablet;
                    $scope.model.entity.hideOnMobile = !$scope.model.visualSettingsOptions.enableOnMobile;

                    if (options.onBeforeEntitySave) {
                        $scope.model.entity = options.onBeforeEntitySave($scope.model.entity);
                    }

                    // Save the entity changes
                    spEntityService.putEntity($scope.model.entity).then(function (id) {
                        $scope.model.busyIndicator.isBusy = false;
                        // Update the client side entity
                        $scope.model.entity.setId(id);
                        $scope.model.entity.markAllUnchanged();

                        if (entityBookmark) {
                            entityBookmark.endBookmark();
                        }

                        $uibModalInstance.close({ entity: $scope.model.entity });
                    }, function (error) {
                        $scope.model.isFormValid = false;
                        $scope.model.busyIndicator.isBusy = false;
                        $scope.model.errors.push({ msg: (error.data.ExceptionMessage || error.data.Message) });
                    });
                }
            };

            // Cancel click handler
            $scope.cancel = function () {
                if (entityBookmark) {
                    entityBookmark.undo();
                }

                $uibModalInstance.close(null);
            };

            $scope.$on('calcTabsLayout', function (event, callback) {
                event.stopPropagation();
                callback($scope.measureArrangeOptions.id);
            });
        })
        .service('spNavigationElementDialog', function (spDialogService) {
            // setup the dialog
            var exports = {
                showDialog: function (options) {
                    var dialogOptions = {
                        title: options.title || 'Item Properties',
                        templateUrl: 'navigation/spNavigationElementDialog/spNavigationElementDialog.tpl.html',
                        controller: 'spNavigationElementDialogController',
                        windowClass: 'spNavigationElementDialog',
                        resolve: {
                            options: function () {
                                return options;
                            }
                        }
                    };

                    return spDialogService.showModalDialog(dialogOptions);
                }
            };

            return exports;
        });
}());