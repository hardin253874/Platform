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
    angular.module('mod.app.formBuilder.directives.spFormBuilderEmbeddedForm', [
        'mod.common.spUuidService',
        'mod.common.ui.spDialogService',
        'mod.app.formBuilder.services.spFormBuilderService',
        'mod.app.editFormServices',
        'mod.app.editForm.designerDirectives',
        'mod.common.ui.spMeasureArrange'
    ])
        .directive('spFormBuilderEmbeddedForm', function ($timeout, spUuidService, spFormBuilderService, spEditForm, spMeasureArrangeService) {

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
                templateUrl: 'formBuilder/directives/spFormBuilder/directives/spFormBuilderEmbeddedForm/spFormBuilderEmbeddedForm.tpl.html',
                link: function (scope, element, attrs, ctrl) {

                    scope.hoverOptions = {
                        className: 'sp-form-builder-embedded-form',
                        hoverClassName: 'sp-form-builder-hover',
                        childSelector: '> .ui-resizable-handle, > .sp-form-builder-toolbar'
                    };

                    scope.model = {
                        formMode: spEditForm.formModes.view,
                        formData: null
                    };

                    scope.measureArrangeEmbeddedOptions = {
                        id: 'embeddedForm'
                    };

                    /////
                    // Define the 'name' property.
                    /////
                    Object.defineProperty(scope, 'name', {
                        get: function () {
                            return scope.control.name || scope.control.formToRender.name;
                        },
                        set: function (newName) {
                            scope.control.name = newName;
                        },
                        enumerable: true,
                        configurable: true
                    });

                    /////
                    // Get the field render control.
                    /////
                    if (scope.control) {
                        scope.model.formId = sp.result(scope.control, 'formToRender.id');

                        spEditForm.getFormDefinition(scope.model.formId).then(
                                function (formControl) {
                                    scope.model.formControl = formControl;

                                    doLayout();

                                    // The type/definition that the embedded form applies to
                                    var typeId = formControl.typeToEditWithForm ? formControl.typeToEditWithForm.id() : null;
                                    scope.model.typeId = typeId;

                                    if (!scope.model.typeId) {
                                        _.forEach(scope.model.contextMenu.menuItems, function(mi) {
                                            mi.disabled = mi.disableForScreens;
                                        });
                                    }

                                    //
                                    // Create the entity if necessary
                                    //
                                    spEditForm.createEntityWithDefaults(typeId, formControl).then(function(formData) {
                                        scope.model.formData = formData;
                                    });
                                },
                                function (error) {
                                    console.error('Error while trying to get form:' + error);
                                    scope.busyOptions.isBusy = false;
                                });

                    }

                    function doLayout() {
                        $timeout(function () {
                            spMeasureArrangeService.performLayout(scope.measureArrangeEmbeddedOptions.id);
                        });
                    }

                    scope.$on('measureArrangeComplete', function (event, args) {

                        if (scope && scope.model && scope.model.formControl) {

                            // adjust for the title if present                            
                            if (element) {

                                var formElement = element.find('> .form-render-control');
                                if (formElement) {

                                    var titleElement = element.find('> .sp-form-builder-container-header');
                                    if (titleElement) {

                                        // subtract the title height
                                        var titleHeight = titleElement.outerHeight(true);
                                        if (titleHeight > 0) {

                                            formElement.css('height', 'calc(100% - ' + titleHeight + 'px)');
                                        }
                                    }
                                }
                            }
                        }

                        if (args) {

                            var isFormBuilderComplete = sp.result(args, 'source.measureArrangeOptions.id') === 'formBuilder';
                            if (isFormBuilderComplete) {

                                // refresh the layout structure of the embedded form
                                doLayout();
                            }
                        }
                    });
                }
            };
        });
}());