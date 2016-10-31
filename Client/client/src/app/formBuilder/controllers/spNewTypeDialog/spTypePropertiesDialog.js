// Copyright 2011-2016 Global Software Innovation Pty Ltd

(function () {
    'use strict';

    /**
    * Module implementing a dialog for creating a new type (definition/object/whatever).
    *
    * @module spNewTypeDialog
    */
    angular.module('mod.app.formBuilder.spTypeProperties', [
        'ui.bootstrap',
        'sp.navService',
        'mod.common.spEntityService',
        'mod.common.ui.spDialogService',
        'mod.app.editForm.designerDirectives.spEditForm',
        'mod.app.formBuilder.services.spFormBuilderService',
        'sp.spNavHelper',
        'mod.common.ui.spMeasureArrange',
        'sp.app.settings'
    ])
        .controller('spTypePropertiesController', spTypePropertiesController)
        .service('spTypeProperties', spTypeProperties);

    function spTypePropertiesController($q, $scope, spEditForm, $uibModalInstance, spFormBuilderService, spNavService, options, spEntityService, spMeasureArrangeService, $timeout, spAppSettings) {
        'ngInject';

        // Note: ensure any new fields get added to spFormBuilderService.loadDefinitions
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
        var model = $scope.model = {
            definition: options.definition,
            form: options.form,
            title: options.mode === spFormBuilderService.builders.form ? 'Object Properties' : 'Screen Properties',
            errors: [],
            isFormValid: true,
            formMode: spEditForm.formModes.edit,
            formForm: null,
            defnMode: spEditForm.formModes.edit,
            defnForm: null,
            formBuilder: options.mode === spFormBuilderService.builders.form,
            screenBuilder: options.mode === spFormBuilderService.builders.screen,
            tabHeading: options.mode === spFormBuilderService.builders.form ? 'Form Properties' : 'Properties',
            visualSettingsOptions: {},
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
            definitionIconPickerOptions: {
                selectedEntityId: null,
                selectedEntity: null,
                selectedEntities: null,
                pickerReportId: 'core:iconsReport',
                entityTypeId: 'core:iconFileType',
                multiSelect: false,
                isDisabled: false
            },
            initialState: {}
        };

        // Bookmarks
        var defnBookmark;
        var formBookmark;
        if (model.definition) {
            defnBookmark = model.definition.graph.history.addBookmark('Edit Object Properties');
        }
        if (model.form && model.form.graph !== sp.result(model, 'definition.graph')) {
            formBookmark = model.form.graph.history.addBookmark('Edit Form Properties');
        }

        //
        // Setup for definition
        //
        var defnFormId = 'core:newObjectForm';

        if (model.definition) {
            model.isNewDefn = model.definition.getDataState() === spEntity.DataStateEnum.Create;

            if (model.isNewDefn && !model.definition.typeScriptName)
                model.definition.typeScriptName = model.definition.name;

            spEditForm.getFormDefinition(defnFormId).then(
                function(defnForm) {
                    defnForm.name = '';
                    $scope.model.defnForm = defnForm;

                    _.find(spEntityUtils.walkEntities(defnForm), function(e) {
                        return sp.result(e, 'relationshipToRender.nsAlias') === 'core:inherits';
                    }).readOnlyControl = true;

                    $timeout(function () {
                        spMeasureArrangeService.performLayout('typePropertiesDialog');
                    });
                },
                function(error) {
                    console.error('Error while trying to get form:' + error);
                });

            $scope.$watch('model.definition.name', function (newValue, oldValue) {
                // sync script name to name - but only for new definitions
                if (model.isNewDefn && model.definition.typeScriptName === oldValue)
                    model.definition.typeScriptName = newValue;
            });
        }

        //
        // Setup for form
        //
        var formFormId = 'console:customEditFormPropertiesForm';
        if (model.screenBuilder) {
            formFormId = 'core:resourceForm';
        }

        if (model.form) {
            spEditForm.getFormDefinition(formFormId).then(
                function (formForm) {
                    $scope.model.formForm = formForm;

                    $timeout(function () {
                        spMeasureArrangeService.performLayout('typePropertiesDialog');
                    });
                },
                function (error) {
                    console.error('Error while trying to get form:' + error);
                });
            model.enableConvert = options.form.allowSelectMultiTypes;
            spEntityService.getEntity(model.form.idP, 'name,description,allowSelectMultiTypes', { hint: 'typeProps', batch: false }).then(function (missingData) {

                $timeout(function () {
                    spEntity.augment(model.form, missingData);
                }, 0);

            });
        }

        if (model.definition && model.form && model.definition.defaultEditForm) {
            model.defaultForm = model.definition.defaultEditForm.idP === model.form.idP;
        }

        //setup screen properties
        if (model.screenBuilder) {
            //Set application
            if (model.form.inSolution && model.form.inSolution.name) {
                model.applicationPickerOptions.selectedEntities = [model.form.inSolution];
            } else if (model.form.dataState === "create") //only insert current application into applicationPickerOption when create the screen
            {
                var currentAppId = spNavService.getCurrentApplicationId();
                if (currentAppId) {
                    spEntityService.getEntity(currentAppId, 'name,description', { batch: 'true', hint: 'appInfo' }).then(function (entity) {
                        if (entity) {
                            model.applicationPickerOptions.selectedEntities = [entity];
                        }
                    });
                }
            }

            //Set Navigation element Icon
            if (model.form.navigationElementIcon)
                model.iconPickerOptions.selectedEntities = [model.form.navigationElementIcon];

            //Load more data to set picker options.
            var ids = ['core:templateReport', 'core:iconFileType', 'core:solution', 'core:applicationsPickerReport'];

            spEntityService.getEntities(ids, 'id', { batch: 'true', hint: 'pickerOptionsIds' }).then(function (entities) {
                if (entities) {
                    var applicationsPickerReport = entities[3];
                    model.applicationPickerOptions.pickerReportId = applicationsPickerReport ? applicationsPickerReport.id() : entities[0].id();
                    model.applicationPickerOptions.entityTypeId = entities[2].id();
                    model.iconPickerOptions.pickerReportId = entities[0].id();
                    model.iconPickerOptions.entityTypeId = entities[1].id();
                }
            });

            //set visual settings
            model.visualSettingsOptions.enableOnDesktop = !model.form.hideOnDesktop;
            model.visualSettingsOptions.enableOnTablet = !model.form.hideOnTablet;
            model.visualSettingsOptions.enableOnMobile = !model.form.hideOnMobile;
        }

        if (model.formBuilder) {
            //Set Navigation element Icon
            var defTreeIcon = sp.result($scope, 'model.definition.typeConsoleBehavior.treeIcon');

            if (defTreeIcon)
                model.definitionIconPickerOptions.selectedEntities = [defTreeIcon];

            model.initialState.defaultTreeIcon = defTreeIcon;

            //Load more data to set definition picker options.
            var eids = ['core:templateReport', 'core:iconFileType'];

            spEntityService.getEntities(eids, 'id', { batch: 'true', hint: 'definitionIconPickerOptionsIds' }).then(function (entities) {
                if (entities) {
                    model.definitionIconPickerOptions.pickerReportId = entities[0].id();
                    model.definitionIconPickerOptions.entityTypeId = entities[1].id();
                }
            });
        }

        // OK click handler
        $scope.ok = function () {
            var defnOk = spEditForm.validateForm($scope.model.defnForm, $scope.model.definition);
            var formOk = !$scope.model.form || spEditForm.validateForm($scope.model.formForm, $scope.model.form);
            if (defnOk && formOk) {

                if (model.defaultForm) {
                    model.form.setLookup('console:isDefaultForEntityType', model.definition);
                } // todo: implement unticking (but what becomes the default form??)
                if (model.screenBuilder) {
                    //Set Applications
                    var applications = sp.result($scope, 'model.applicationPickerOptions.selectedEntities');

                    if (applications != null && applications.length > 0) {
                        model.form.setLookup('core:inSolution', applications[0]);
                    } else {
                        model.form.setLookup('core:inSolution', null);
                    }

                    //Set navigation element Icon
                    var iconEntity = sp.result($scope, 'model.iconPickerOptions.selectedEntities.0');
                    model.form.setLookup('console:navigationElementIcon', iconEntity || null);

                    model.form.hideOnDesktop = !model.visualSettingsOptions.enableOnDesktop;
                    model.form.hideOnTablet = !model.visualSettingsOptions.enableOnTablet;
                    model.form.hideOnMobile = !model.visualSettingsOptions.enableOnMobile;
                }

                if (model.formBuilder) {
                    setDefinitionIcon();
                }

                if (defnBookmark) defnBookmark.endBookmark();
                if (formBookmark) formBookmark.endBookmark();
                $uibModalInstance.close(true);
            }
        };

        // Cancel click handler
        $scope.cancel = function () {
            if (defnBookmark) defnBookmark.undo();
            if (formBookmark) formBookmark.undo();
            $uibModalInstance.close(false);
        };

        $scope.measureArrangeOptions = {
            id: 'typePropertiesDialog'
        };

        function setDefinitionIcon() {
            var defIconEntity = sp.result($scope, 'model.definitionIconPickerOptions.selectedEntities.0');

            if (model.isNewDefn || sp.result(model.initialState, 'defaultTreeIcon.id') !== sp.result(defIconEntity, 'id')) {
                var typeConsoleBehavior = sp.result($scope, 'model.definition.typeConsoleBehavior');
                if (!typeConsoleBehavior) {
                    typeConsoleBehavior = spFormBuilderService.createEmptyBehavior();
                    $scope.model.definition.setLookup('console:typeConsoleBehavior', typeConsoleBehavior);
                }
                if (typeConsoleBehavior) {
                    typeConsoleBehavior.setLookup('console:treeIcon', defIconEntity || null);
                }
            }
        }
    }

    function spTypeProperties(spDialogService) {
        'ngInject';

        // setup the dialog
        var exports = {

            // options = { definition: entity, form: form }

            showDialog: function (options) {
                var dialogOptions = {
                    title: 'Object Properties',
                    templateUrl: 'formBuilder/controllers/spNewTypeDialog/spTypePropertiesDialog.tpl.html',
                    controller: 'spTypePropertiesController',
                    resolve: {
                        options: function () {
                            return options;
                        }
                    }
                };

                var history;
                var bookmark;

                if (options.definition) {
                    history = options.definition.graph.history;
                    bookmark = history.addBookmark("Object Properties");
                }

                return spDialogService.showModalDialog(dialogOptions).then(function (results) {
                    if (!results) {
                        // cancel
                        if (history) {
                            history.undoBookmark(bookmark);
                        }
                    } else {
                        // accept
                        if (bookmark) {
                            bookmark.endBookmark();
                        }
                    }
                });
            }
        };

        return exports;
    }
}());