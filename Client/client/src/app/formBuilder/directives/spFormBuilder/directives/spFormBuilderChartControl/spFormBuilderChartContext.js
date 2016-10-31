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
    angular.module('mod.app.formBuilder.directives.spFormBuilderChartContext', [
        'mod.app.formBuilder.services.spFormBuilderService',
        'mod.app.editFormServices',
        'mod.app.editForm.designerDirectives',
        'mod.app.formBuilder.directives.spFormBuilderAssignParent',
        'sp.navService',
        'mod.app.chartBuilder.services.spChartBuilderService',
        'mod.common.ui.spEditFormDialog',
        'mod.app.configureDialog.Controller',
        'mod.common.spEntityService'
    ])
        .directive('spFormBuilderChartContext', function (spFormBuilderService, spNavService, spFormBuilderAssignParentService, spState, spChartBuilderService, spEditFormDialog, controlConfigureDialogFactory, $q, spEntityService) {

            /////
            // Directive structure.
            /////
            return {
                restrict: 'AE',
                replace: true,
                transclude: false,
                scope: {
                    control: '=?' // the chartRenderControl
                },
                templateUrl: 'formBuilder/directives/spFormBuilder/directives/spFormBuilderGenericControl/spFormBuilderGenericContext.tpl.html',
                link: function (scope, element, attrs, ctrl) {

                    scope.hoverOptions = {
                        className: 'sp-form-builder-report',
                        hoverClassName: 'sp-form-builder-hover',
                        childSelector: '> .ui-resizable-handle, > .sp-form-builder-toolbar'
                    };

                    scope.model = {
                        showContextMenu: false,
                        contextMenu: {
                            menuItems: [
                                {
                                    text: 'Modify Chart',
                                    icon: 'assets/images/16x16/edit.png',
                                    type: 'click',
                                    click: 'onModifyChartClick()'
                                },
                                {
                                    text: 'Chart Properties',
                                    icon: 'assets/images/16x16/propertiesChart.png',
                                    type: 'click',
                                    click: 'onChartPropertiesClick()'
                                },
                                {
                                    text: 'Container Properties',
                                    icon: 'assets/images/16x16/propertiesContainer.png',
                                    type: 'click',
                                    click: 'onContainerPropertiesClick()'
                                },
                                {
                                    text: 'Assign Parent',
                                    icon: 'assets/images/16x16/assign.png',
                                    type: 'click',
                                    click: 'onAssignDataClick()',
                                    disableForScreens: true
                                }
                            ]
                        }
                    };

                    /////
                    // Model to hold the chart for passing to the properties dialog
                    /////
                    scope.chartModel = {
                        chart: scope.control.chartToRender,
                        fullyLoaded: false
                    };

                    // Handle configure click
                    scope.onConfigureClick = function () {
                        scope.model.showContextMenu = true;
                    };

                    /////
                    // Show the chart properties.
                    /////
                    scope.onChartPropertiesClick = function () {

                        var defer = $q.defer();

                        if (!scope.chartModel.fullyLoaded) {
                            spChartBuilderService.reloadChart(scope.chartModel)
                                .then(function () {

                                    /////
                                    // Must set this to null first!?!
                                    /////
                                    scope.control.chartToRender = null;
                                    scope.control.chartToRender = scope.chartModel.chart;

                                }).finally(function () {

                                    scope.chartModel.fullyLoaded = true;

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

                                var options = {
                                    title: 'Chart Properties',
                                    entity: scope.control.chartToRender,
                                    form: 'core:chartPropertiesForm',
                                    optionsEnabled:true,
                                    formLoaded: function (form) {
                                        _.find(spEntityUtils.walkEntities(form), function (e) {
                                            return sp.result(e, 'relationshipToRender.nsAlias') === 'core:chartReport';
                                        }).readOnlyControl = true;
                                    }
                                };

                                spEditFormDialog.showDialog(options).then(function (result) {
                                    if (result !== false) {
                                        spEntityService.putEntity(scope.control.chartToRender);
                                    }
                                });
                            });
                    };

                    /////
                    // Show the container properties.
                    /////
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

                    // Handle 'Modify Chart' click
                    scope.onModifyChartClick = function () {
                        var formId = scope.control.chartToRender.idP;

                        spState.scope.state = spFormBuilderService.getState();

                        spNavService.navigateToChildState('chartBuilder', formId);
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