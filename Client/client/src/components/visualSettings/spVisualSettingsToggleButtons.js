// Copyright 2011-2016 Global Software Innovation Pty Ltd
(function () {
    'use strict';

    /**
      * Module implementing visual settings toggle buttons.        
      *
      * @module spVisualSettingsToggleButtons    
      * @example            
         
      Using the spVisualSettingsToggleButtons:
     
      &lt;sp-visual-settings-toggle-buttons options="options"/&gt;
     
      where options is an object with the following properties:
         - enableOnDesktop {bool}. Enabled on desktop.
         - enableOnTablet {bool}. Enabled on tablet.
         - enableOnMobile {bool}. Enabled on mobile.
      */
    angular.module('mod.common.ui.spVisualSettingsToggleButtons', [])
        .directive('spVisualSettingsToggleButtons', function () {
            return {
                restrict: 'E',
                replace: true,
                templateUrl: 'visualSettings/spVisualSettingsToggleButtons.tpl.html',
                scope: {
                    options: '='
                }
            };
        });
}());