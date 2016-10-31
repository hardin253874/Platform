// Copyright 2011-2016 Global Software Innovation Pty Ltd
(function () {
    'use strict';

    /**
      * Module implementing a foreground background color picker dropdown.        
      *
      * @module spColorPickerFgBgDropdown    
      * @example            
         
      Using the spColorPickerFgBgDialog:
     
      &lt;sp-color-picker-fg-bg-dropdown color="color"/&gt;
     
      where color is an object with the following properties:
         - foregroundColor - {object}. The foreground color.
             - a {number} - The alpha channel 0-255
             - r {number} - The red channel 0-255
             - g {number} - The green channel 0-255
             - b {number} - The blue channel 0-255
             
         - backgroundColor - {object}. The background color.            
             - a {number} - The alpha channel 0-255
             - r {number} - The red channel 0-255
             - g {number} - The green channel 0-255
             - b {number} - The blue channel 0-255      
      */
    angular.module('mod.common.ui.spColorPickerFgBgDropdown', ['mod.common.ui.spColorPickerConstants', 'mod.common.ui.spColorPickerFgBgDialog', 'mod.common.ui.spColorPickerUtils', 'mod.common.ui.spPopupProvider'])
        .directive('spColorPickerFgBgDropdown', function (namedFgBgColors, spColorPickerFgBgDialog, spColorPickerUtils, spPopupProvider) {
            return {
                restrict: 'E',
                replace: true,
                templateUrl: 'colorPicker/spColorPickerFgBgDropdown.tpl.html',
                scope: {
                    color: '='
                },
                link: function (scope, iElement, iAttrs) {
                    var popupProvider;

                    // Setup the model
                    scope.model = {                        
                        selectedColor: null,
                        availableColors: _.cloneDeep(namedFgBgColors)
                    };                   


                    popupProvider = spPopupProvider(scope, iElement, {                        
                        templatePopupUrl: 'colorPicker/spColorPickerFgBgDropdownMenu.tpl.html'
                    });                              


                    // Handle drop down button click events
                    scope.dropDownButtonClicked = function (event) {
                        popupProvider.togglePopup(event);
                    };


                    // Select the specified colour
                    scope.selectColor = function (color, userClicked) {
                        var dialogOptions = {
                            foregroundColor: color.foregroundColor,
                            backgroundColor: color.backgroundColor
                        };

                        scope.model.selectedColor = color;

                        if (userClicked &&
                            color.name === 'Custom') {
                            // Display the custom dialog
                            spColorPickerFgBgDialog.showModalDialog(dialogOptions).then(function (result) {
                                if (result) {

                                    color.foregroundColor = result.foregroundColor;
                                    color.backgroundColor = result.backgroundColor;

                                    updateOutputColor(color);
                                }
                            });
                        }

                        // Update output                        
                        updateOutputColor(color);
                    };


                    // Gets the color as a valid css color string
                    scope.getCssFgBgColor = function (color) {
                        var style = {};

                        if (color) {
                            if (color.foregroundColor) {
                                style.color = spColorPickerUtils.getCssColorFromRgba(color.foregroundColor);
                            }
                            if (color.backgroundColor) {
                                style['background-color'] = spColorPickerUtils.getCssColorFromRgba(color.backgroundColor);
                            }
                        }

                        return style;
                    };
                    

                    // Watch for changes in the color
                    scope.$watch('color', function () {
                        var foundColor;

                        // The input color is undefined. Select None by default
                        if (!scope.color) {
                            foundColor = _.find(scope.model.availableColors, function (nc) {
                                return nc.name === 'None';
                            });
                        }

                        if (!foundColor &&
                            scope.color &&
                            scope.color.foregroundColor &&
                            scope.color.backgroundColor) {
                            // The input has a color try to find it.
                            foundColor = _.find(scope.model.availableColors, function (nc) {
                                return spColorPickerUtils.areColorsEqual(nc.foregroundColor, scope.color.foregroundColor) &&
                                    spColorPickerUtils.areColorsEqual(nc.backgroundColor, scope.color.backgroundColor);
                            });

                            if (!foundColor) {
                                // Find the custom color
                                foundColor = _.find(scope.model.availableColors, function (nc) {
                                    return nc.name === 'Custom';
                                });

                                // Update the custom color color
                                spColorPickerUtils.copyColor(scope.color.foregroundColor, foundColor.foregroundColor);
                                spColorPickerUtils.copyColor(scope.color.backgroundColor, foundColor.backgroundColor);
                            }
                        }

                        if (!foundColor) {
                            // Still haven't found a color.
                            // If there is no value then select None
                            foundColor = _.find(scope.model.availableColors, function (nc) {
                                return nc.name === 'None';
                            });
                        }

                        scope.selectColor(foundColor, false);
                    }, true);


                    // Update the output color
                    function updateOutputColor(color) {
                        if (!scope.color ||
                            (spColorPickerUtils.areColorsEqual(color.foregroundColor, scope.color.foregroundColor) &&
                            spColorPickerUtils.areColorsEqual(color.backgroundColor, scope.color.backgroundColor))) {
                            return;
                        }

                        spColorPickerUtils.copyColor(color.foregroundColor, scope.color.foregroundColor);
                        spColorPickerUtils.copyColor(color.backgroundColor, scope.color.backgroundColor);                        
                    }                                       
                }
            };
        });
}());