// Copyright 2011-2016 Global Software Innovation Pty Ltd


(function () {
    'use strict';

    /**
    * Module implementing a dialog for creating a new type (definition/object/whatever).
    *
    * @module spNewTypeDialog
    */
    angular.module('mod.app.formBuilder.spNewTypeDialog', [
        'ui.bootstrap',
        'sp.navService',
        'mod.common.spEntityService',
        'mod.common.ui.spDialogService',
        'mod.app.editForm.designerDirectives.spEditForm',
        'mod.app.formBuilder.services.spFormBuilderService',
        'sp.spNavHelper',
        'mod.common.ui.spMeasureArrange'
    ])
        .controller('spNewTypeDialogController', spNewTypeDialogController)
        .service('spNewTypeDialog', spNewTypeDialog);

    function spNewTypeDialogController($q, $timeout, $scope, spEditForm, $uibModalInstance, spFormBuilderService,
                                       spNavService, options, spEntityService, spMeasureArrangeService) {
        'ngInject';

        // Setup the dialog model
        var model = {
            title: 'New Object',
            errors: [],
            isFormValid: true,
            formMode: spEditForm.formModes.edit,
            formData: null,
            formControl: null,
            definitionIconPickerOptions: {
                selectedEntityId: null,
                selectedEntity: null,
                selectedEntities: null,
                pickerReportId: 'core:iconsReport',
                entityTypeId: 'core:iconFileType',
                multiSelect: false,
                isDisabled: false
            }
        };

        $scope.model = model;

        function definitionNameExists(name) {
            if (name && options && options.types) {
                var lowerName = name.toLowerCase();

                var found = _.find(options.types, function(type) {
                    return type.name && (type.name.toLowerCase() === lowerName);
                });

                return !!found;
            }

            return false;
        }

        var formId = 'core:newObjectForm';  // c'est la vie

        spEditForm.getFormDefinition(formId).then(
            function (formControl) {
                formControl.name = '';
                model.formControl = formControl;

                spEditForm.createEntityWithDefaults('core:definition', formControl).then(function(form) {
                    model.formData = form;

                    $timeout(function () {
                        spMeasureArrangeService.performLayout('newTypeDialog');
                    });

                    var appId = spNavService.getCurrentApplicationId();
                    if (appId) {
                        model.formData.setInSolution(appId);
                    }
                });
            },
            function (error) {
                console.error('Error while trying to get form:' + error);
            });


        $scope.$watch('model.formData.name', function (newValue, oldValue) {
            // sync script name to name - but only for new definitions
            if (model.formData && model.formData.typeScriptName === oldValue)
                model.formData.typeScriptName = newValue;
        });

        // OK click handler
        $scope.ok = function () {

            model.errors = [];

            // Check form is valid
            if (spEditForm.validateForm(model.formControl, model.formData)) {

                var definition = model.formData;

                if (!definitionNameExists(definition.name)) {

                    // Create a definition object
                    var augmentDefn = spFormBuilderService.createDefinitionEntity();

                    // Set up default form
                    var form = spFormBuilderService.createFormEntity();
                    form.name = definition.name + ' Form';
                    form.typeToEditWithForm = definition;
                    form.inSolution = definition.inSolution;
                    //definition.setLookup('console:defaultEditForm', form);

                    // set definition icon for typeConsoleBehavior
                    var defIconEntity = sp.result($scope, 'model.definitionIconPickerOptions.selectedEntities.0');
                    if (defIconEntity) {
                        var typeConsoleBehavior = spFormBuilderService.createEmptyBehavior();
                        typeConsoleBehavior.treeIcon = defIconEntity;

                        definition.setLookup('console:typeConsoleBehavior', typeConsoleBehavior);
                    }

                    $uibModalInstance.close({
                        definition: definition,
                        augmentDefn: augmentDefn,
                        form: form,
                        isDefaultForm: true
                    });
                } else {
                    model.isFormValid = false;
                    var error = {
                        msg: 'Object with name \'' + definition.name + '\' already exists.'
                    };
                    model.errors.push(error);
                }
            }
        };

        // Cancel click handler
        $scope.cancel = function () {
            $uibModalInstance.close(null);
        };

        $scope.measureArrangeOptions = {
            id: 'newTypeDialog'
        };
    }

    function spNewTypeDialog(spDialogService, spNavService, spFormBuilderService, spState) {
        'ngInject';

        // setup the dialog
        // options: folder - id for the generated report
        var exports = {
            showDialog: function (options) {
                var opts = options || {};
                var dialogOptions = {
                    title: 'New Object',
                    templateUrl: 'formBuilder/controllers/spNewTypeDialog/spNewTypeDialog.tpl.html',
                    controller: 'spNewTypeDialogController',
                    resolve: {
                        options: function () {
                            return opts;
                        }
                    }
                };

                spDialogService.showModalDialog(dialogOptions).then(function (results) {
                    if (!results)
                        return; // cancel

                    spEntity.augment(results.definition, results.augmentDefn);

                    if (!results.definition.inherits.length) {
                        results.definition.inherits.add(results.augmentDefn.inherits[0]); // inherit user resource if nothing else specified
                    }

                    delete results.augmentDefn;

                    spState.scope.state = spFormBuilderService.getState();

                    results.moveItemCallback = opts.moveItemCallback;

                    var data = {
                        state: results
                    };

                    spNavService.navigateToChildState('formBuilder', -1, undefined, data);
                });
            }
        };

        return exports;
    }
}());