// Copyright 2011-2016 Global Software Innovation Pty Ltd
/*global console, _, angular */

(function() {
    'use strict';

    /**
    * Module implementing the form builder configuration button.
    * spFormBuilderConfigureButton provides the button for configuring form elements.
    *
    * @module spFormBuilderConfigurationButton
    * @example

    Using the spFormBuilderConfigurationButton:

    &lt;sp-form-builder-configure-button&gt;&lt;/sp-form-builder-configure-button&gt

    */
    angular.module('mod.app.formBuilder.directives.spFormBuilderConfigureButton', [
    ])
        .directive('spFormBuilderConfigureButton', function() {

            /////
            // Directive structure.
            /////
            return {
                restrict: 'AE',
                replace: false,
                transclude: false,
                scope: {},
                templateUrl: 'formBuilder/directives/spFormBuilder/directives/spFormBuilderConfigureButton/spFormBuilderConfigureButton.tpl.html',
                link: function(scope) {

                    /////
                    // Configure the edit form.
                    /////
                    scope.onClick = function() {
                        console.error('configure button click');
                    };
                }
            };
        });
}());