// Copyright 2011-2016 Global Software Innovation Pty Ltd
(function () {
    'use strict';

    /**
    * Module implementing a color picker popup.            
    * spColorPickerPopup displays the color picker in a popup.    
    *
    * @module spColorPickerPopup    
    * @example           

    Using the spColorPickerPopup:

    &lt;div sp-color-picker-popup="options"&gt;&lt;/div&gt      

    where options is available on the controller with the following properties:
        - isOpen - {bool}. If the popup is open or not
        - color - {object}. The color
            - a {number} - The alpha channel 0-255
            - r {number} - The red channel 0-255
            - g {number} - The green channel 0-255
            - b {number} - The blue channel 0-255   
    */
    angular.module('mod.common.ui.spColorPickerPopup', ['ui.bootstrap', 'ui.bootstrap.position', 'mod.common.ui.spColorPicker', 'mod.common.ui.spColorPickerUtils'])
        .directive('spColorPickerPopup', function ($parse, $compile, $uibPosition, $document, $templateCache, $timeout, spColorPickerUtils) {
            return {
                restrict: 'A',
                link: function (originalScope, iElement, iAttrs) {
                    var scope = originalScope.$new(true),
                        colorPickerPopup,
                        getterColorPickerPopupOptions;


                    // Create the model with the color picker
                    // and busy indicator options.
                    scope.model = {
                        selectedColor: {
                            
                        },                        
                        options: {
                            
                        }
                    };


                    getterColorPickerPopupOptions = $parse(iAttrs.spColorPickerPopup);
                    scope.model.options = getterColorPickerPopupOptions(originalScope);


                    // Watch the options for changes
                    originalScope.$watch(getterColorPickerPopupOptions, function (options) {
                        scope.model.options = options;
                        if (angular.isUndefined(scope.model.options.color)) {
                            scope.model.options.color = {
                                a: 255,
                                r: 0,
                                g: 0,
                                b: 0
                            };
                        }

                        spColorPickerUtils.copyColor(options.color, scope.model.selectedColor);
                    });


                    // Watch for options color changes
                    scope.$watch(function () {
                        return scope.model.options.color;
                    }, function (color) {
                        spColorPickerUtils.copyColor(color, scope.model.selectedColor);
                    }, true);


                    // Watch the value of isOpen for changes
                    scope.$watch(function () {
                        return scope.model.options.isOpen;
                    }, function (isOpen) {
                        $timeout(function () {
                            if (isOpen) {
                                onShowColorPickerPopup();
                            } else {
                                onHideColorPickerPopup();
                            }
                        });
                    });


                    // Make sure popup is destroyed and removed.
                    originalScope.$on('$destroy', function () {
                        scope.model.options.isOpen = false;
                        scope.$destroy();
                    });


                    // Build the popup and add it after the specified element
                    colorPickerPopup = buildColorPickerPopup();
                    colorPickerPopup.insertAfter(iElement);


                    // Build the popup indicator

                    function buildColorPickerPopup() {
                        return $compile($templateCache.get('colorPicker/spColorPickerPopup.tpl.html'))(scope);
                    }


                    // Called to hide the popup

                    function onHideColorPickerPopup() {
                        var cppPosition = {
                            display: 'none'
                        };

                        if (!colorPickerPopup) {
                            return;
                        }

                        // Hide the popup                                                
                        colorPickerPopup.css(cppPosition);

                        // Remove the document click handler
                        $document.off('click', documentClickBind);
                    }


                    // Called to show the popup

                    function onShowColorPickerPopup() {
                        if (!colorPickerPopup) {
                            return;
                        }

                        // Update the selected color
                        spColorPickerUtils.copyColor(scope.model.options.color, scope.model.selectedColor);

                        // Show the popup
                        updateColorPickerPosition();

                        // Add a document click handler
                        $document.on('click', documentClickBind);
                    }


                    // Called when the document click event occurs.
                    // Used to hide the popup when clicking off it.

                    function documentClickBind(event) {
                        scope.$apply(function () {
                            documentClick(event);
                        });
                    }


                    // Called when the document click event occurs.
                    // Used to hide the popup when clicking off it.

                    function documentClick(event) {
                        if (!colorPickerPopup) {
                            return;
                        }

                        var position = $uibPosition.offset(colorPickerPopup);

                        // Ignore if the popup is not open
                        if (!scope.model.options.isOpen) {
                            return;
                        }

                        // Ignore if the click happened inside parent element
                        // or inside the color drop down
                        if (event.target === iElement[0] ||
                            event.target.className === 'dropdownMenuItem' ||
                            event.target.className === 'colorDropdownColorBox' ||
                            (event.target.parentNode &&
                                event.target.parentNode.className === 'dropdownMenuItem')) {
                            return;
                        }

                        // Ignore if the click happened inside the popup
                        if (event.pageX >= position.left &&
                            event.pageX <= position.left + position.width &&
                            event.pageY >= position.top &&
                            event.pageY <= position.top + position.height) {
                            return;
                        }

                        scope.model.options.isOpen = false;
                    }


                    // Update the position of the color picker popup

                    function updateColorPickerPosition() {
                        var position, cppPosition;

                        if (!colorPickerPopup) {
                            return;
                        }

                        colorPickerPopup.css({ top: 0, left: 0 });

                        position = $uibPosition.position(iElement);

                        cppPosition = {
                            top: position.top + iElement.prop('offsetHeight'),
                            left: position.left
                        };

                        cppPosition.top += 'px';
                        cppPosition.left += 'px';
                        cppPosition.display = 'block';
                        cppPosition.position = 'absolute';

                        // Now set the calculated positioning.
                        colorPickerPopup.css(cppPosition);
                    }                   


                    // Ok clicked, copy the selected color to the output and close.
                    scope.okClicked = function () {
                        // Push the color to the output        
                        spColorPickerUtils.copyColor(scope.model.selectedColor, scope.model.options.color);

                        scope.model.options.isOpen = false;
                    };


                    // Cancel clicked, close the popup
                    scope.cancelClicked = function () {
                        scope.model.options.isOpen = false;
                    };
                }
            };
        });
}());