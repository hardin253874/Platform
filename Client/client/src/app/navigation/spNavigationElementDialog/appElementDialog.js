// Copyright 2011-2016 Global Software Innovation Pty Ltd
(function () {
    'use strict';

    /**
    * Module implementing a dialog for configuring application entity.
    *
    * @module appElementDialog
    */
    angular.module('mod.app.navigation.appElementDialog', [
        'ui.bootstrap',
        'mod.common.ui.spDialogService',
        'mod.common.ui.spMeasureArrange',
        'mod.common.spEntityService',
        'mod.app.editForm.designerDirectives.spEditForm'
    ])
        .controller('appElementDialogController', function ($scope, $timeout, $uibModalInstance, options, spEditForm, spEntityService, spMeasureArrangeService) {

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
                title: options.title || 'Name and Description',
                errors: [],
                isFormValid: true,
                formMode: spEditForm.formModes.edit,
                entity: options.entity,
                formControl: null,
                busyIndicator: {
                    type: 'spinner',
                    text: 'Please wait...',
                    placement: 'element',
                    isBusy: false
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
                }
            };

            $scope.measureArrangeOptions = {
                id: 'appElementDialog'
            };

            // Bookmarks
            var entityBookmark;

            if (options.entity) {
                entityBookmark = options.entity.graph.history.addBookmark('Form Data Properties');

                //Set Navigation element Icon
                if (options.entity.navigationElementIcon)
                    $scope.model.iconPickerOptions.selectedEntities = [options.entity.navigationElementIcon];

                //Load more data to set picker options.
                var ids = ['core:templateReport', 'core:iconFileType'];

                spEntityService.getEntities(ids, 'name', { batch: 'true', hint: 'pickerOptionsIds' }).then(function (entities) {
                    if (entities) {
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
                            spMeasureArrangeService.performLayout('appElementDialog');
                        });
                    },
                    function (error) {
                        console.error('Error while trying to get form:' + error);
                    });

            // OK click handler
            $scope.ok = function () {
                $scope.model.errors = [];
                $scope.model.isFormValid = true;

                if (spEditForm.validateForm($scope.model.formControl, $scope.model.entity)) {
                    $scope.model.busyIndicator.isBusy = true;
                    //Set navigation element Icon
                    var iconEntity = sp.result($scope, 'model.iconPickerOptions.selectedEntities.0');
                    if (options.entity.type && options.entity.type.alias() === 'core:solution')
                        $scope.model.entity.setLookup('console:applicationIcon', iconEntity || null);
                    else
                        $scope.model.entity.setLookup('console:navigationElementIcon', iconEntity || null);
                   
                    $scope.model.entity.hideOnDesktop = !$scope.model.visualSettingsOptions.enableOnDesktop;
                    $scope.model.entity.hideOnTablet = !$scope.model.visualSettingsOptions.enableOnTablet;
                    $scope.model.entity.hideOnMobile = !$scope.model.visualSettingsOptions.enableOnMobile;

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
        .service('appElementDialog', function (spDialogService) {
            // setup the dialog
            var exports = {
                showDialog: function (options) {
                    var dialogOptions = {
                        title: options.title || 'Application',
                        templateUrl: 'navigation/spNavigationElementDialog/appElementDialog.tpl.html',
                        controller: 'appElementDialogController',
                        windowClass: 'appElementDialog',
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