// Copyright 2011-2016 Global Software Innovation Pty Ltd
/*global console, _, angular */

(function () {
    'use strict';

    /**
    * Module implementing a form builder report control.
    * spFormBuilderReportControl renders a child form that has been placed on the form during design time.
    *
    * @module spFormBuilderReportControl
    */
    angular.module('mod.app.formBuilder.directives.spFormBuilderReportControl', [
        'mod.common.spUuidService'
    ])
        .directive('spFormBuilderReportControl', function (spUuidService) {

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
                templateUrl: 'formBuilder/directives/spFormBuilder/directives/spFormBuilderReportControl/spFormBuilderReportControl.tpl.html',
                link: function (scope, element, attrs, ctrl) {

                    scope.hoverOptions = {
                        className: 'sp-form-builder-report-control',
                        hoverClassName: 'sp-form-builder-hover',
                        childSelector: '> .ui-resizable-handle, > .sp-form-builder-toolbar'
                    };

                    scope.model = {
                        reportOptions: {
                            reportId: scope.control.reportToRender.id(),
                            formControlEntity: scope.control,
                            isInDesign: true,
                            disableActions: false,
                            disableNew: true
                        }
                    };

                    /////
                    // Define the 'name' property.
                    /////
                    Object.defineProperty(scope, 'name', {
                        get: function () {
                            return scope.control.name || scope.control.reportToRender.name;
                        },
                        set: function (newName) {
                            scope.control.name = newName;
                        },
                        enumerable: true,
                        configurable: true
                    });
                }
            };
        });
}());