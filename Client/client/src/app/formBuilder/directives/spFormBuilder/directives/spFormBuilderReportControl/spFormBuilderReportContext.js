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
    angular.module('mod.app.formBuilder.directives.spFormBuilderReportContext', [
        'mod.app.formBuilder.services.spFormBuilderService',
        'mod.app.formBuilder.directives.spFormBuilderAssignParent',
        'sp.navService',
        'mod.app.reportProperty',
        'mod.common.spEntityService',
        'mod.common.spReportEntity',
        'mod.app.configureDialog.Controller'
    ])
        .directive('spFormBuilderReportContext', function (spFormBuilderService, spNavService, spFormBuilderAssignParentService, spState, spReportPropertyDialog, spEntityService, spReportEntity, $q, controlConfigureDialogFactory) {

            /////
            // Directive structure.
            /////
            return {
                restrict: 'AE',
                replace: true,
                transclude: false,
                scope: {
                    control: '=?' // the reportRenderControl
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
                                    text: 'Modify Report',
                                    icon: 'assets/images/16x16/edit.png',
                                    type: 'click',
                                    click: 'onModifyReportClick()'
                                },
                                {
                                    text: 'Report Properties',
                                    icon: 'assets/images/16x16/propertiesReport.png',
                                    type: 'click',
                                    click: 'onReportPropertiesClick()'
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
                        },
                        reportName: 'bob',
                        selectedEntityId: 123
                    };

                    scope.reportModel = {
                        report: scope.control.reportToRender,
                        fullyLoaded: false
                    };

                    // Handle configure click
                    scope.onConfigureClick = function () {
                        scope.model.showContextMenu = true;
                    };

                    /////
                    // Show the chart properties.
                    /////
                    scope.onReportPropertiesClick = function () {

                        var defer = $q.defer();

                        if (!scope.reportModel.fullyLoaded) {
                            var rq = spReportEntity.makeReportRequest();
                            spEntityService.getEntity(scope.reportModel.report.idP, rq)
                                .then(function (reportEntity) {

                                    /////
                                    // Must set this to null first!?!
                                    /////
                                    scope.control.reportToRender = null;
                                    scope.control.reportToRender = reportEntity;

                                    scope.reportModel.report = new spReportEntity.Query(reportEntity);

                                }).finally(function () {

                                    scope.reportModel.fullyLoaded = true;

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

                                var reportPropertyeOptions = { reportId: scope.control.reportToRender.id(), reportEntity: scope.reportModel.report };

                                spReportPropertyDialog.showModalDialog(reportPropertyeOptions).then(function (result) {

                                    if (result) {
                                        if (result.reportEntity) {
                                            scope.control.reportToRender = null;
                                            scope.control.reportToRender = result.reportEntity.getEntity();
                                            scope.reportModel.report = result.reportEntity;

                                            spEntityService.putEntity(scope.control.reportToRender);
                                        }
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
                            relationshipType: 'container'
                        };

                        controlConfigureDialogFactory.createDialog(options);
                    };

                    // Handle 'Modify Report' click
                    scope.onModifyReportClick = function () {
                        var formId = scope.control.reportToRender.idP;

                        spState.scope.state = spFormBuilderService.getState();

                        spNavService.navigateToChildState('reportBuilder', formId);
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