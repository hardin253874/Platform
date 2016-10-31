// Copyright 2011-2016 Global Software Innovation Pty Ltd
(function () {
    'use strict';

    /**
    * Module implementing a color picker drop down.            
    * spColorPickerDropdown displays a drop down of predefined colors.
    *
    * @module spColorPickerDropdown    
    * @example            

    Using the spColorPickerDropdown:
    
    &lt;sp-color-dropdown color="color"&gt;&lt;/sp-color-dropdown&gt 

     where color is available on the controller with the following properties:
        - a {number} - The alpha channel 0-255
        - r {number} - The red channel 0-255
        - g {number} - The green channel 0-255
        - b {number} - The blue channel 0-255
    */
    angular.module('mod.common.ui.spColorPickerDropdown', ['ui.bootstrap', 'ui.bootstrap.position', 'mod.common.ui.spColorPickerConstants', 'mod.common.ui.spColorPickerUtils', 'mod.common.ui.spColorPickerDialog', 'mod.common.ui.spPopupProvider'])
        .directive('spColorPickerDropdown', function ($uibPosition, $document, namedColors, namedColorsWithCustom, spColorPickerUtils, spColorPickerDialog, spPopupProvider) {
            return {
                restrict: 'E',
                replace: true,
                templateUrl: 'colorPicker/spColorPickerDropdown.tpl.html',
                scope: {
                    color: '=',
                    showcustom: '=?'
                },
                link: function (scope, iElement, iAttrs) {
                    var popupProvider;
                    
                    // Setup the model
                    scope.model = {
                        selectedColor: null,
                        availableColors: !scope.showcustom ? namedColors : namedColorsWithCustom
                    };
                    
                    popupProvider = spPopupProvider(scope, iElement, {
                        templatePopupUrl: 'colorPicker/spColorPickerDropdownMenu.tpl.html'
                    });

                    // Handle drop down button click events
                    scope.dropDownButtonClicked = function (event) {
                        popupProvider.togglePopup(event);
                    };

                    // Watch for changes in the color
                    scope.$watch('color', function () {
                        var foundColor;
                        if (!scope.showcustom) {
                            // The input color is undefined. Select Black by default
                            if (!scope.color) {
                                foundColor = _.find(namedColors, function(nc) {
                                    return nc.value.a === 255 &&
                                        nc.value.r === 0 &&
                                        nc.value.g === 0 &&
                                        nc.value.b === 0;
                                });
                            }

                            if (!foundColor &&
                                scope.color) {
                                // The input has a color try to find it.
                                foundColor = _.find(namedColors, function(nc) {
                                    return nc.value.a === scope.color.a &&
                                        nc.value.r === scope.color.r &&
                                        nc.value.g === scope.color.g &&
                                        nc.value.b === scope.color.b;
                                });

                                if (!foundColor) {
                                    // A value is set. Create a custom color
                                    foundColor = {
                                        name: 'Custom',
                                        value: {
                                            a: scope.color.a,
                                            r: scope.color.r,
                                            g: scope.color.g,
                                            b: scope.color.b
                                        }
                                    };
                                }
                            }

                            if (!foundColor) {
                                // Still haven't found a color.
                                // If there is no value then select black
                                foundColor = _.find(namedColors, function(nc) {
                                    return nc.value.a === 255 &&
                                        nc.value.r === 0 &&
                                        nc.value.g === 0 &&
                                        nc.value.b === 0;
                                });
                            }

                            scope.selectColor(foundColor);
                        } else {
                            // The input has a color try to find it.
                            foundColor = _.find(namedColorsWithCustom, function (nc) {
                                return scope.color &&
                                    nc.value.a === scope.color.a &&
                                    nc.value.r === scope.color.r &&
                                    nc.value.g === scope.color.g &&
                                    nc.value.b === scope.color.b;
                            });

                            if (foundColor) {
                                scope.selectColor(foundColor);
                            } else {
                                scope.model.selectedColor = {
                                    name: 'Custom',
                                    value: { a: 0, r: 128, g: 128, b: 128 }
                                };
                            }
                        }
                    }, true);


                    // Select the specified colour
                    scope.selectColor = function (color, userClicked) {
                        if (!scope.showcustom) {
                            scope.model.selectedColor = color;

                            // Update output                        
                            if (!scope.color || spColorPickerUtils.areColorsEqual(color, scope.color)) {
                                return;
                            }

                            spColorPickerUtils.copyColor(color.value, scope.color);
                        } else {
                            scope.model.selectedColor = color;

                            // Update output                        
                            if (!scope.color || spColorPickerUtils.areColorsEqual(color, scope.color)) {
                                return;
                            }

                            if (userClicked &&
                                color.name === 'Custom') {
                                var dialogOptions = {
                                    color: scope.color
                                };


                                // Display the custom dialog
                                spColorPickerDialog.showModalDialog(dialogOptions).then(function (result) {
                                    if (result) {
                                        spColorPickerUtils.copyColor(result.color, scope.color);                                    
                                    }
                                });
                            }
                            if (color.name !== 'Custom') {
                                spColorPickerUtils.copyColor(color.value, scope.color);
                            }
                        }
                    };


                    // Gets the color as a valid css color string
                    scope.getCssColor = function (color) {
                        var style = {};

                        if (color &&
                            color.value) {
                            if (color.name === 'Custom') {
                                style['background-color'] = spColorPickerUtils.getCssColorFromRgba(scope.color);
                            } else {
                                style['background-color'] = spColorPickerUtils.getCssColorFromRgba(color.value);
                            }
                        }

                        return style;
                    };                   
                }
            };
        });
}());