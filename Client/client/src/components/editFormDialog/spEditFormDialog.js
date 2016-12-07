// Copyright 2011-2016 Global Software Innovation Pty Ltd
/*global angular, console, _, sp */

(function () {
    'use strict';

    /**
     * Reusable dialog for editing some arbitrary entity with a well-known form.
     *
     * Example usage:
     *   spEditFormDialog.showDialog({
    *      title: 'Chart Properties',
    *      entity: scope.model.chart,
    *      form: 'core:chartPropertiesForm'
    *      formLoaded: function(form) {...} //optional callback
    *   });
     *
     * Where:
     *  - entity is the entity being edited
     *  - title is the dialog title
     *  - form  is the alias of the form to use for editing. (typically store the form in Core Data)
     *
     * @module spEditFormDialog
     */
    angular.module('mod.common.ui.spEditFormDialog', [
        'ui.bootstrap',
        'sp.navService',
        'mod.common.spEntityService',
        'mod.common.ui.spDialogService',
        'mod.common.ui.spMeasureArrange',
        'mod.app.editForm.designerDirectives.spEditForm',
        'sp.spNavHelper',
        'sp.app.settings'
    ])
        .controller('spEditFormDialogController', ['$q', '$scope', '$timeout', 'spEntityService', 'spEditForm', '$uibModalInstance', 'spMeasureArrangeService', 'spAppSettings', 'options', function ($q, $scope, $timeout, spEntityService, spEditForm, $uibModalInstance, spMeasureArrangeService, spAppSettings, options) {

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
            var solns = _.compact([sp.result(options, 'entity.inSolution')]);
            // Setup the dialog model
            var model = $scope.model = {
                title: options.title,
                errors: [],
                isFormValid: true,
                formMode: options.formMode || spEditForm.formModes.edit,
                saveEntity: options.saveEntity ? options.saveEntity : false,
                entity: options.entity, // entity being edited
                form: null,
                formId: options.form,
                optionsEnabled: options.optionsEnabled,
                showAdvanced: spAppSettings.fullConfig, // only full admin can see advanced properties (esp 'in app', and 'custom form')
                formatTabActiveByDefault: !spAppSettings.fullConfig,
                applicationPickerOptions: {
                    selectedEntityId: null,
                    selectedEntity: null,
                    selectedEntities: solns,
                    pickerReportId: 'core:applicationsPickerReport',
                    entityTypeId: 'core:solution',
                    multiSelect: false,
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
                visualSettingsOptions: {
                    enableOnDesktop: !options.entity.hideOnDesktop,
                    enableOnTablet: !options.entity.hideOnTablet,
                    enableOnMobile: !options.entity.hideOnMobile
                },
                busyIndicator: {
                    type: 'spinner',
                    text: 'Please wait...',
                    placement: 'element',
                    isBusy: false
                }
            };

            // Bookmarks
            var bookmark;
            if (model.entity) {
                bookmark = model.entity.graph.history.addBookmark('Edit Properties');
            } else {
                console.warn('spEditFormDialog was passed a null entity');
            }

            if ($scope.model.optionsEnabled) {
                //set navigation element icon
                if (options.entity.navigationElementIcon) {
                    $scope.model.iconPickerOptions.selectedEntities = [options.entity.navigationElementIcon];
                }

                //Load more data to set picker options.
                var ids = ['core:templateReport', 'core:iconFileType', 'core:solution', 'core:applicationsPickerReport'];

                spEntityService.getEntities(ids, 'id', {
                    batch: 'true',
                    hint: 'pickerOptionsIds'
                }).then(function (entities) {
                    if (entities) {
                        var applicationsPickerReport = entities[3];
                        $scope.model.applicationPickerOptions.pickerReportId = applicationsPickerReport ? applicationsPickerReport.id() : entities[0].id();
                        $scope.model.applicationPickerOptions.entityTypeId = entities[2].id();
                        $scope.model.iconPickerOptions.pickerReportId = entities[0].id();
                        $scope.model.iconPickerOptions.entityTypeId = entities[1].id();
                    }
                });
            }


            // Load form
            spEditForm.getFormDefinition(model.formId).then(
                function (form) {
                    form.name = '';
                    if (options.formLoaded) {
                        options.formLoaded(form);
                    }
                    
                    $scope.model.form = form;

                    // register the relFilters
                    spEditForm.registerRelationshipControlFilters($scope.model.form);

                    $timeout(function () {
                        spMeasureArrangeService.performLayout('editFormDialog');
                    });
                },
                function (error) {
                    console.error('Error while trying to get form:' + error);
                });

            // OK click handler
            $scope.ok = function () {
                if (!$scope.model.entity)
                    $uibModalInstance.close(false);

                var entityOK = spEditForm.validateForm($scope.model.form, $scope.model.entity);
                if (entityOK) {
                    if ($scope.model.optionsEnabled) {

                        //Set Applications
                        var applications = sp.result($scope, 'model.applicationPickerOptions.selectedEntities');

                        if (applications && applications.length > 0) {
                            $scope.model.entity.setLookup('core:inSolution', applications[0]);
                        } else {
                            $scope.model.entity.setLookup('core:inSolution', null);
                        }

                        //Set Navigation Element Icon
                        var iconEntity = sp.result($scope, 'model.iconPickerOptions.selectedEntities.0');
                        $scope.model.entity.setLookup('console:navigationElementIcon', iconEntity || null);

                        $scope.model.entity.hideOnDesktop = !$scope.model.visualSettingsOptions.enableOnDesktop;
                        $scope.model.entity.hideOnTablet = !$scope.model.visualSettingsOptions.enableOnTablet;
                        $scope.model.entity.hideOnMobile = !$scope.model.visualSettingsOptions.enableOnMobile;
                    }
                    if ($scope.model.saveEntity) {
                        $scope.model.busyIndicator.isBusy = true;
                        // Update the client side entity
                        // Save the entity changes
                        spEntityService.putEntity($scope.model.entity).then(function (id) {
                            $scope.model.busyIndicator.isBusy = false;
                            // Update the client side entity
                            $scope.model.entity.setId(id);
                            $scope.model.entity.markAllUnchanged();

                            bookmark.endBookmark();
                            $uibModalInstance.close(true);
                        }, function (error) {
                            $scope.model.isFormValid = false;
                            $scope.model.busyIndicator.isBusy = false;
                            $scope.model.errors.push({msg: (error.data.ExceptionMessage || error.data.Message)});
                        });
                    } else {
                        bookmark.endBookmark();
                        $uibModalInstance.close(true);
                    }
                }
            };

            // Cancel click handler
            $scope.cancel = function () {
                if (bookmark) {
                    bookmark.endBookmark();
                }
                $uibModalInstance.close(false);
            };

            $scope.$on('$destroy', function () {
                spEditForm.clearRelationshipControlFilters($scope.model.form);
            });

            $scope.$on('filterSourceControlDataChanged', function (event, data) {
                $timeout(function () {
                    var sourceControl = data.sourceControl;
                    var filteredIds = spEditForm.getFilteredControlIds($scope.model.form, sourceControl);
                    if (filteredIds && filteredIds.length) {
                        $scope.$broadcast('updateFilteredControlData', { filteredControlIds: filteredIds });
                    }
                });
            });

            $scope.measureArrangeOptions = {
                id: 'editFormDialog'
            };
        }])
        .service('spEditFormDialog', ['spDialogService', function (spDialogService) {
            // setup the dialog
            var exports = {

                // options = { entity: entity, form: 'formAlias', title: 'Title' }

                showDialog: function (options) {

                    var dialogOptions = {
                        title: options.title,
                        templateUrl: 'editFormDialog/spEditFormDialog.tpl.html',
                        controller: 'spEditFormDialogController',
                        resolve: {
                            options: _.constant(options)
                        }
                    };

                    return spDialogService.showModalDialog(dialogOptions);
                }
            };

            return exports;
        }]);
}());