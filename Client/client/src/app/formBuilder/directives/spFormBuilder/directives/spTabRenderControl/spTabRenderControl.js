// Copyright 2011-2016 Global Software Innovation Pty Ltd
/*global console, _, angular */

(function () {
    'use strict';

    /**
    * Module implementing a tab render control.
    * spTabRenderControl is an intermediate/bridge control that gets the information from editform tabcontrol and translates to what is needed for form builder.
    *
    * @module spTabRenderControl
    * @example

    Using the spFormBuilderContainer:

    &lt;sp-tab-render-control&gt;&lt;/sp-tab-render-control&gt

    */
    angular.module('mod.app.formBuilder.directives.spTabRenderControl', ['mod.app.formBuilder.services.spFormBuilderService', 'mod.common.spCachingCompile'])
        .directive('spTabRenderControl', function (spFormBuilderService, spCachingCompile) {

            ////
            // Directive structure.
            ////
            return {
                restrict: 'AE',
                replace: false,
                transclude: false,
                scope: {
                    tabItem: '=?',
                    isInTestMode: '=?',
                    isReadOnly: '=?',
                    isInDesign: '=?'
                },                
                link: function (scope, element) {

                    ////
                    // Sets the contained control that the tab will show
                    ////
                    if (scope.tabItem) {
                        scope.containedControl = scope.tabItem.model.formControl;
                    }                    

                    ////
                    // Get the field render control.
                    ////
                    scope.getFieldRenderControl = function (field) {
                        var type;

                        if (field && field.getType) {
                            type = field.getType();

                            if (type && type._alias) {
                                return 'formBuilder/directives/spFormBuilder/templates/' + type._alias + '.tpl.html';
                            }
                        }

                        return undefined;
                    };

                    var cachedLinkFunc = spCachingCompile.compile('formBuilder/directives/spFormBuilder/directives/spTabRenderControl/spTabRenderControl.tpl.html');
                    cachedLinkFunc(scope, function (clone) {
                        element.append(clone);
                    });
                }
            };
        });
}());