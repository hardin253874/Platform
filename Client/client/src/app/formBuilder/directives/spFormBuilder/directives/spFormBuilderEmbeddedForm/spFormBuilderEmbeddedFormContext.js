// Copyright 2011-2016 Global Software Innovation Pty Ltd
/*global console, _, angular */

(function () {
    'use strict';

    /**
    * Module implementing a form builder embedded form.
    * spFormBuilderEmbeddedForm renders a child form that has been placed on the form during design time.
    *
    * @module spFormBuilderEmbeddedForm
    * @example

    Using the spFormBuilderEmbeddedForm:

    &lt;sp-form-builder-embedded-form&gt;&lt;/sp-form-builder-embedded-form&gt

    */
    angular.module('mod.app.formBuilder.directives.spFormBuilderEmbeddedFormContext', [
        'mod.app.formBuilder.services.spFormBuilderService',
        'mod.app.editFormServices',
        'mod.app.formBuilder.directives.spFormBuilderAssignParent',
        'sp.navService',
        'mod.app.formBuilder.spTypeProperties',
        'mod.common.spEntityService',
        'mod.app.configureDialog.Controller',
        'sp.app.settings'
    ])
        .directive('spFormBuilderEmbeddedFormContext', function (spFormBuilderService, spEditForm, spNavService, spFormBuilderAssignParentService, spState, spTypeProperties, $q, spEntityService, controlConfigureDialogFactory, spAppSettings) {

            /////
            // Directive structure.
            /////
            return {
                restrict: 'AE',
                replace: true,
                transclude: false,
                scope: {
                    control: '=?' // the formRenderControl
                },
                templateUrl: 'formBuilder/directives/spFormBuilder/directives/spFormBuilderEmbeddedForm/spFormBuilderEmbeddedFormContext.tpl.html',
                link: function (scope, element, attrs, ctrl) {

                    scope.hoverOptions = {
                        className: 'sp-form-builder-embedded-form',
                        hoverClassName: 'sp-form-builder-hover',
                        childSelector: '> .ui-resizable-handle, > .sp-form-builder-toolbar'
                    };

                    scope.model = {
                        showContextMenu: false,
                        contextMenu: {
                            menuItems: _.filter([
                                {
                                    text: 'Modify Form',
                                    icon: 'assets/images/16x16/edit.png',
                                    type: 'click',
                                    click: 'onModifyFormClick()',
                                    visible: spAppSettings.fullConfig
                                },
                                {
                                    text: 'Form Properties',
                                    icon: 'assets/images/16x16/propertiesForm.png',
                                    type: 'click',
                                    click: 'onFormPropertiesClick()',
                                    visible: spAppSettings.fullConfig
                                },
                                {
                                    text: 'Container Properties',
                                    icon: 'assets/images/16x16/propertiesContainer.png',
                                    type: 'click',
                                    click: 'onContainerPropertiesClick()',
                                    visible: true
                                },
                                {
                                    text: 'Assign Parent',
                                    icon: 'assets/images/16x16/assign.png',
                                    type: 'click',
                                    click: 'onAssignDataClick()',
                                    disableForScreens: true,
                                    visible: true
                                }
                            ], 'visible')
                        }
                    };

                    scope.formModel = {
                        form: scope.control.formToRender,
                        fullyLoaded: false
                    };

                    // Handle configure click
                    scope.onConfigureClick = function () {
                        scope.model.showContextMenu = true;
                    };

                    // Handle 'Modify Form' click
                    scope.onModifyFormClick = function () {
                        var formId = scope.control.formToRender.idP;

                        spState.scope.state = spFormBuilderService.getState();

                        spNavService.navigateToChildState('formBuilder', formId);
                    };

                    scope.onFormPropertiesClick = function () {

                        var defer = $q.defer();

                        if (!scope.formModel.fullyLoaded) {

                            spEditForm.getFormDefinition(scope.formModel.form.id(), true)
                                .then(function (form) {

                                    scope.formModel.form = form;

                                    /////
                                    // Must set this to null first!?!
                                    /////
                                    scope.control.formToRender = null;
                                    scope.control.formToRender = form;

                                    var options;

                                    options = {
                                        fields: true,
                                        relationships: true,
                                        fieldGroups: true,
                                        ignoreInheritance: false,
                                        ignoreOverrides: false,
                                        derivedTypes: false,
                                        resourceKeys: true,
                                        additional: 'fields.allowMultiLines,inSolution.name,isAbstract,supportMultiTypes,inherits.name,k:defaultEditForm.id,typeScriptName'
                                    };

                                    var rq = spResource.makeTypeRequest(options);
                                    return spEntityService.getEntities([form.typeToEditWithForm.id()], rq);
                                }).then(function (definition) {

                                    if (definition && definition.length > 0) {
                                        /////
                                        // Must set this to null first!?!
                                        /////
                                        scope.formModel.form.typeToEditWithForm = null;
                                        scope.formModel.form.typeToEditWithForm = definition[0];
                                    }

                                }).finally(function () {

                                    scope.formModel.fullyLoaded = true;

                                    /////
                                    // Resolve the promise.
                                    /////
                                    defer.resolve();
                                });

                        } else {

                            /////
                            // Resolve the promise.
                            /////
                            defer.resolve();
                        }

                        defer.promise
                            .then(function () {
                                spTypeProperties.showDialog({ definition: scope.formModel.form.typeToEditWithForm, form: scope.formModel.form, mode: 'formBuilder' }).then(function (result) {
                                    if (result !== false) {

                                        spEntityService.putEntity(scope.formModel.form);
                                    }
                                });
                        });
                        
                    };

                    scope.onContainerPropertiesClick = function () {
                        var options = {
                            formControl: scope.control,
                            isFieldControl: true,
                            isFormControl: true,
                            relationshipType: controlConfigureDialogFactory.getRelationshipType(scope.control),
                            isReverseRelationship: scope.control.isReversed
                        };

                        controlConfigureDialogFactory.createDialog(options);
                    };

                    // Handle 'Assign Data' click
                    scope.onAssignDataClick = function () {
                        spFormBuilderAssignParentService.showDialog({
                            control: scope.control
                        });
                    };

                }
            };
        });
}());