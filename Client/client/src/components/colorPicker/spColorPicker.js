// Copyright 2011-2016 Global Software Innovation Pty Ltd
(function () {
    'use strict';

    /**
    * Module implementing a color picker.
    * spColorPicker displays an inline color picker.
    *
    * @module spColorPicker
    * @example

    Using the spColorPicker:

    &lt;sp-color-picker color="color"&gt;&lt;/sp-color-picker&gt

    where color is available on the controller with the following properties:
        - a {number} - The alpha channel 0-255
        - r {number} - The red channel 0-255
        - g {number} - The green channel 0-255
        - b {number} - The blue channel 0-255
        
    */
    angular.module('mod.common.ui.spColorPicker', ['ui.bootstrap', 'ui.bootstrap.position', 'mod.common.ui.spColorPickerDropdown', 'mod.common.ui.spColorPickerUtils'])
        .directive('spColorPicker', function ($uibPosition, $document, spColorPickerUtils) {
            return {
                restrict: 'E',
                replace: true,
                templateUrl: 'colorPicker/spColorPicker.tpl.html',
                scope: {
                    color: '='
                },
                link: function (scope, iElement, iAttrs) {
                    var satValPicker,
                        satValDragger,
                        isDraggingSatValDragger,
                        satValPickerOffset = {},
                        satValPickerMaxHeight = 0,
                        satValPickerMaxWidth = 0,
                        satValDraggerWidthOffset,
                        satValDraggerHeightOffset,
                        throttledUpdateModelFromSatValPosition,
                        throttledHueSliderChanged,
                        throttledASliderChanged,
                        throttledRSliderChanged,
                        throttledGSliderChanged,
                        throttledBSliderChanged;

                    // Setup model
                    scope.model = {
                        hsvColor: {
                            h: 0,
                            s: 0,
                            v: 0
                        },
                        rgbColor: {
                            a: 255,
                            r: 0,
                            g: 0,
                            b: 0
                        },
                        rgbColorDropdown: {
                            a: 255,
                            r: 0,
                            g: 0,
                            b: 0
                        }
                    };


                    initialize();


                    // Setup callbacks so we know when the RGB text box values change.


                    // Transparency changed
                    scope.onRgbColorANumberChanged = function () {
                        // Update the corresponding slider
                        setSliderValue('#aSlider', scope.model.rgbColor.a);
                    };


                    // Red channel changed
                    scope.onRgbColorRNumberChanged = function () {
                        // Update the corresponding sliders
                        setSliderValue('#rSlider', scope.model.rgbColor.r);
                        updateHsvValuesAndControls();
                    };


                    // Green channel changed
                    scope.onRgbColorGNumberChanged = function () {
                        // Update the corresponding sliders
                        setSliderValue('#gSlider', scope.model.rgbColor.g);
                        updateHsvValuesAndControls();
                    };


                    // Blue channel changed
                    scope.onRgbColorBNumberChanged = function () {
                        // Update the corresponding sliders
                        setSliderValue('#bSlider', scope.model.rgbColor.b);
                        updateHsvValuesAndControls();
                    };


                    // Watching the input color
                    scope.$watch('color', function () {
                        initializeModel(scope.color);
                    }, true);

                    // Watch the rgb color
                    scope.$watch('model.rgbColor', function () {
                        // Update the color drop down
                        spColorPickerUtils.copyColor(scope.model.rgbColor, scope.model.rgbColorDropdown);
                        // Update the output
                        spColorPickerUtils.copyColor(scope.model.rgbColor, scope.color);
                    }, true);


                    // Watch the rgb color from the drop down
                    scope.$watch('model.rgbColorDropdown', function () {
                        if (!scope.model.rgbColorDropdown) {
                            return;
                        }

                        if (spColorPickerUtils.areColorsEqual(scope.model.rgbColor, scope.model.rgbColorDropdown)) {
                            return;
                        }

                        // The drop down color has changed
                        spColorPickerUtils.copyColor(scope.model.rgbColorDropdown, scope.model.rgbColor);
                        // Update the hsv calue
                        updateHsvFromRgb();

                        // Set sliders
                        setSliderValue('#aSlider', scope.model.rgbColor.a);
                        setSliderValue('#rSlider', scope.model.rgbColor.r);
                        setSliderValue('#gSlider', scope.model.rgbColor.g);
                        setSliderValue('#bSlider', scope.model.rgbColor.b);
                        setHueSliderValue();
                        setSatValDraggerPos(scope.model.hsvColor.s, scope.model.hsvColor.v);
                    }, true);


                    // Return the hue as a style. This is used to color the sat val picker
                    scope.getHueColorAsStyle = function () {
                        var hsl = {
                            h: scope.model.hsvColor.h * 360.0,
                            s: '100%',
                            l: '50%'
                        };
                        return { 'background-color': getCssColorFromHsl(hsl) };
                    };


                    // Handle the scope being destroyed
                    scope.$on('$destroy', function () {
                        satValPicker.off('mousedown', satValPickerMouseDownBind);
                        $document.off('mousemove', documentMouseMove);
                        $document.off('mouseup', documentMouseUpBind);
                    });

                    // Private methods

                    // Create some throttled versions of mouse move and slider move callbacks

                    throttledUpdateModelFromSatValPosition = _.throttle(function (dragX, satValPickerMaxWidth, dragY, satValPickerMaxHeight) {
                        safeApply(scope, function () {
                            updateModelFromSatValPosition(dragX, satValPickerMaxWidth, dragY, satValPickerMaxHeight);
                        });
                    }, 100);


                    throttledHueSliderChanged = _.throttle(function (event, ui) {
                        hueSliderChanged(event, ui);
                    }, 100);


                    throttledASliderChanged = _.throttle(function (event, ui) {
                        aSliderChanged(event, ui);
                    }, 100);


                    throttledRSliderChanged = _.throttle(function (event, ui) {
                        rSliderChanged(event, ui);
                    }, 100);


                    throttledGSliderChanged = _.throttle(function (event, ui) {
                        gSliderChanged(event, ui);
                    }, 100);


                    throttledBSliderChanged = _.throttle(function (event, ui) {
                        bSliderChanged(event, ui);
                    }, 100);


                    // Initialize the directive
                    function initialize() {
                        // Setup JQuery sliders
                        createSliders();

                        // Get the sat value picker window
                        satValPicker = iElement.find('#saturationValuePickerId');

                        // Get the dragger
                        satValDragger = iElement.find('#saturationValueDraggerId');

                        satValPickerMaxWidth = satValPicker.width();
                        satValPickerMaxHeight = satValPicker.height();

                        // Calculate the width and height offsets for the dragger
                        satValDraggerWidthOffset = satValDragger.width() / 2.0;
                        satValDraggerHeightOffset = satValDragger.height() / 2.0;

                        // Setup a mouse down event for the sat val picker window
                        satValPicker.on('mousedown', satValPickerMouseDownBind);
                    }


                    // Handle sat val picker mouse down events
                    function satValPickerMouseDownBind(event) {
                        var rightClick = (event.which) ? (event.which === 3) : (event.button === 2);
                        if (rightClick || isDraggingSatValDragger) {
                            return;
                        }

                        event.preventDefault();
                        event.stopPropagation();

                        safeApply(scope, function () {
                            satValPickerMouseDown(event);
                        });
                    }


                    // Handle sat val picker mouse down events
                    function satValPickerMouseDown(event) {
                        var draggerPos,
                            pageX = event.pageX,
                            pageY = event.pageY,
                            dragX,
                            dragY,
                            rightClick = (event.which) ? (event.which === 3) : (event.button === 2);

                        if (rightClick || isDraggingSatValDragger) {
                            return;
                        }

                        isDraggingSatValDragger = true;

                        // Setup events on document
                        $document.on('mousemove', documentMouseMove);
                        $document.on('mouseup', documentMouseUpBind);

                        satValPickerOffset = satValPicker.offset();

                        dragX = Math.max(0, Math.min(pageX - satValPickerOffset.left, satValPickerMaxWidth));
                        dragY = Math.max(0, Math.min(pageY - satValPickerOffset.top, satValPickerMaxHeight));

                        draggerPos = {
                            display: 'block',
                            top: (dragY - satValDraggerHeightOffset) + 'px',
                            left: (dragX - satValDraggerWidthOffset) + 'px'
                        };

                        satValDragger.css(draggerPos);

                        updateModelFromSatValPosition(dragX, satValPickerMaxWidth, dragY, satValPickerMaxHeight);
                    }


                    // Set the position of the color picker dragger based on the s and v values.
                    function setSatValDraggerPos(s, v) {
                        var draggerPos,
                            xOffset = parseFloat(satValPickerMaxWidth * s),
                            yOffset = parseFloat(satValPickerMaxHeight - satValPickerMaxHeight * v);

                        draggerPos = {
                            display: 'block',
                            top: (yOffset - satValDraggerHeightOffset) + 'px',
                            left: (xOffset - satValDraggerWidthOffset) + 'px'
                        };

                        satValDragger.css(draggerPos);
                    }


                    // Handle the document mouse move events
                    function documentMouseMove(event) {
                        var pageX = event.pageX,
                            pageY = event.pageY,
                            dragX,
                            dragY,
                            draggerPos;

                        if (!isDraggingSatValDragger) {
                            return;
                        }

                        dragX = Math.max(0, Math.min(pageX - satValPickerOffset.left, satValPickerMaxWidth));
                        dragY = Math.max(0, Math.min(pageY - satValPickerOffset.top, satValPickerMaxHeight));

                        draggerPos = {
                            display: 'block',
                            top: (dragY - satValDraggerHeightOffset) + 'px',
                            left: (dragX - satValDraggerWidthOffset) + 'px'
                        };

                        satValDragger.css(draggerPos);

                        throttledUpdateModelFromSatValPosition(dragX, satValPickerMaxWidth, dragY, satValPickerMaxHeight);
                    }


                    // Update the model from saturation, value position
                    function updateModelFromSatValPosition(dragX, maxWidth, dragY, maxHeight) {
                        // Update the model
                        scope.model.hsvColor.s = parseFloat(dragX / maxWidth);
                        scope.model.hsvColor.v = parseFloat((maxHeight - dragY) / maxHeight);

                        updateRgbFromHsv();

                        setSliderValue('#rSlider', scope.model.rgbColor.r);
                        setSliderValue('#gSlider', scope.model.rgbColor.g);
                        setSliderValue('#bSlider', scope.model.rgbColor.b);
                    }


                    // Handle the document mouse up events
                    function documentMouseUpBind() {
                        safeApply(scope, function () {
                            documentMouseUp();
                        });
                    }


                    // Handle the document mouse up events
                    function documentMouseUp() {
                        if (isDraggingSatValDragger) {
                            $document.off('mousemove', documentMouseMove);
                            $document.off('mouseup', documentMouseUp);
                        }

                        isDraggingSatValDragger = false;
                    }


                    // Update the hsv values from the rgb values
                    function updateHsvFromRgb() {
                        scope.model.hsvColor = rgbToHsv(scope.model.rgbColor.r, scope.model.rgbColor.g, scope.model.rgbColor.b);
                    }


                    // Updat the rgb values from the hsv values
                    function updateRgbFromHsv() {
                        var rgb = hsvToRgb(scope.model.hsvColor.h, scope.model.hsvColor.s, scope.model.hsvColor.v);

                        scope.model.rgbColor.r = rgb.r;
                        scope.model.rgbColor.g = rgb.g;
                        scope.model.rgbColor.b = rgb.b;
                    }


                    // Create the jquery sliders
                    function createSliders() {
                        // Slider for the hue
                        iElement.find('#hueSlider').slider({
                            value: 0,
                            min: 0,
                            max: 1000,
                            step: 1,
                            orientation: 'vertical',
                            change: hueSliderChanged,
                            slide: hueSliderSlide
                        });

                        // Slider for the transparency
                        iElement.find('#aSlider').slider({
                            value: 255,
                            min: 0,
                            max: 255,
                            step: 1,
                            change: aSliderChanged,
                            slide: aSliderSlide
                        });

                        // Slider for red
                        iElement.find('#rSlider').slider({
                            value: 0,
                            min: 0,
                            max: 255,
                            step: 1,
                            change: rSliderChanged,
                            slide: rSliderSlide
                        });

                        // Slider for green
                        iElement.find('#gSlider').slider({
                            value: 0,
                            min: 0,
                            max: 255,
                            step: 1,
                            change: gSliderChanged,
                            slide: gSliderSlide
                        });

                        // Slider for blue
                        iElement.find('#bSlider').slider({
                            value: 0,
                            min: 0,
                            max: 255,
                            step: 1,
                            change: bSliderChanged,
                            slide: bSliderSlide
                        });
                    }


                    // Safe apply function which only calls apply if it is not already in progress.
                    function safeApply(scope, fn) {
                        if (!scope.$root.$$phase) {
                            // digest or apply not in progress
                            scope.$apply(fn);
                        } else {
                            // digest or apply already in progress
                            fn();
                        }
                    }


                    // Converts an RGB color value to HSV. Conversion formula
                    // Assumes r, g, and b are contained in the set [0, 255] and
                    // returns h, s, and v in the set [0, 1].
                    function rgbToHsv(r, g, b) {
                        r = r / 255;
                        g = g / 255;
                        b = b / 255;

                        var max = Math.max(r, g, b),
                            min = Math.min(r, g, b),
                            h,
                            s,
                            v = max,
                            d = max - min;

                        s = max === 0 ? 0 : d / max;

                        if (max === min) {
                            h = 0; // achromatic
                        } else {
                            switch (max) {
                            case r:
                                h = (g - b) / d + (g < b ? 6 : 0);
                                break;
                            case g:
                                h = (b - r) / d + 2;
                                break;
                            case b:
                                h = (r - g) / d + 4;
                                break;
                            }
                            h /= 6;
                        }

                        return {
                            h: h,
                            s: s,
                            v: v
                        };
                    }


                    // Converts an HSV color value to RGB. Conversion formula
                    // Assumes h, s, and v are contained in the set [0, 1] and
                    // returns r, g, and b in the set [0, 255].
                    function hsvToRgb(h, s, v) {
                        var r, g, b, i, f, p, q, t;

                        i = Math.floor(h * 6);
                        f = h * 6 - i;
                        p = v * (1 - s);
                        q = v * (1 - f * s);
                        t = v * (1 - (1 - f) * s);
                        switch (i % 6) {
                        case 0:
                            r = v;
                            g = t;
                            b = p;
                            break;
                        case 1:
                            r = q;
                            g = v;
                            b = p;
                            break;
                        case 2:
                            r = p;
                            g = v;
                            b = t;
                            break;
                        case 3:
                            r = p;
                            g = q;
                            b = v;
                            break;
                        case 4:
                            r = t;
                            g = p;
                            b = v;
                            break;
                        case 5:
                            r = v;
                            g = p;
                            b = q;
                            break;
                        }
                        return {
                            r: Math.floor(r * 255),
                            g: Math.floor(g * 255),
                            b: Math.floor(b * 255)
                        };
                    }


                    // Called when the hueSlider has changed.
                    function hueSliderChanged(event, ui) {
                        // Skip events not originating from ui i.e. from mouse, keyboard
                        if (!event ||
                            !event.originalEvent ||
                            !event.originalEvent.type) {
                            return;
                        }

                        safeApply(scope, function () {
                            scope.model.hsvColor.h = (1000.0 - ui.value) / 1000.0;

                            updateRgbFromHsv();

                            setSliderValue('#rSlider', scope.model.rgbColor.r);
                            setSliderValue('#gSlider', scope.model.rgbColor.g);
                            setSliderValue('#bSlider', scope.model.rgbColor.b);
                        });
                    }


                    // Called when the hue slider has been moved
                    function hueSliderSlide(event, ui) {
                        throttledHueSliderChanged(event, ui);
                    }


                    // Called when the aSlider has changed.
                    function aSliderChanged(event, ui) {
                        // Skip events not originating from ui i.e. from mouse, keyboard
                        if (!event ||
                            !event.originalEvent ||
                            !event.originalEvent.type) {
                            return;
                        }

                        safeApply(scope, function () {
                            scope.model.rgbColor.a = ui.value;
                        });
                    }


                    // called when the a slider has been moved
                    function aSliderSlide(event, ui) {
                        throttledASliderChanged(event, ui);
                    }


                    // Called when the rSlider has changed.
                    function rSliderChanged(event, ui) {
                        // Skip events not originating from ui i.e. from mouse, keyboard
                        if (!event ||
                            !event.originalEvent ||
                            !event.originalEvent.type) {
                            return;
                        }

                        safeApply(scope, function () {
                            scope.model.rgbColor.r = ui.value;
                            updateHsvValuesAndControls();
                        });
                    }


                    // Called when the r slider has been moved
                    function rSliderSlide(event, ui) {
                        throttledRSliderChanged(event, ui);
                    }


                    // Called when the gSlider has changed.
                    function gSliderChanged(event, ui) {
                        // Skip events not originating from ui i.e. from mouse, keyboard
                        if (!event ||
                            !event.originalEvent ||
                            !event.originalEvent.type) {
                            return;
                        }

                        safeApply(scope, function () {
                            scope.model.rgbColor.g = ui.value;
                            updateHsvValuesAndControls();
                        });
                    }


                    // Called when the g slider has been moved
                    function gSliderSlide(event, ui) {
                        throttledGSliderChanged(event, ui);
                    }


                    // Called when the bSlider has changed.
                    function bSliderChanged(event, ui) {
                        // Skip events not originating from ui i.e. from mouse, keyboard
                        if (!event ||
                            !event.originalEvent ||
                            !event.originalEvent.type) {
                            return;
                        }

                        safeApply(scope, function () {
                            scope.model.rgbColor.b = ui.value;
                            updateHsvValuesAndControls();
                        });
                    }


                    // Called when the b slider has been moved
                    function bSliderSlide(event, ui) {
                        throttledBSliderChanged(event, ui);
                    }


                    // Update the hsv values and controls from the rgb values
                    function updateHsvValuesAndControls() {
                        updateHsvFromRgb();
                        setHueSliderValue();
                        setSatValDraggerPos(scope.model.hsvColor.s, scope.model.hsvColor.v);
                    }


                    // Sets the specified slider value
                    function setSliderValue(sliderId, value) {
                        var currentValue = iElement.find(sliderId).slider('value');
                        if (angular.isUndefined(value)) {
                            value = 0;
                        }
                        if (currentValue !== value) {
                            iElement.find(sliderId).slider('value', value);
                        }
                    }


                    // Sets the hue slider value
                    function setHueSliderValue() {
                        setSliderValue('#hueSlider', Math.round(1000 - (scope.model.hsvColor.h * 1000)));
                    }


                    // Get the css color from a hsl color
                    function getCssColorFromHsl(color) {
                        var h = 0, s = '100%', l = '50%';

                        if (!color) {
                            return null;
                        }

                        if (color.h) {
                            h = color.h;
                        }

                        if (color.s) {
                            s = color.s;
                        }

                        if (color.l) {
                            l = color.l;
                        }

                        return 'hsl(' + h + ',' + s + ',' + l + ')';
                    }                    
                    

                    // Initialize the model
                    function initializeModel(color) {
                        var newColor = {
                            a: 255,
                            r: 0,
                            g: 0,
                            b: 0
                        };

                        if (angular.isDefined(color)) {
                            newColor.a = angular.isDefined(color.a)? color.a : 255;
                            newColor.r = angular.isDefined(color.r) ? color.r : 0;
                            newColor.g = angular.isDefined(color.g) ? color.g : 0;
                            newColor.b = angular.isDefined(color.b) ? color.b : 0;
                        }

                        if (spColorPickerUtils.areColorsEqual(scope.model.rgbColor, newColor)) {
                            // No change
                            return;
                        }

                        // Setup rgb colors
                        spColorPickerUtils.copyColor(newColor, scope.model.rgbColor);
                        spColorPickerUtils.copyColor(newColor, scope.model.rgbColorDropdown);

                        // Setup initial hsv color
                        updateHsvFromRgb();

                        // Set sliders
                        setSliderValue('#aSlider', scope.model.rgbColor.a);
                        setSliderValue('#rSlider', scope.model.rgbColor.r);
                        setSliderValue('#gSlider', scope.model.rgbColor.g);
                        setSliderValue('#bSlider', scope.model.rgbColor.b);
                        setHueSliderValue();
                        setSatValDraggerPos(scope.model.hsvColor.s, scope.model.hsvColor.v);
                    }
                }
            };
        });
}());