// Copyright 2011-2016 Global Software Innovation Pty Ltd
/*global console, _, angular */

(function() {
    'use strict';

    /**
    * Directive implementing resize functionality.
    * spResizable allows DOM elements to be resized by dragging their resize handles.
    *
    * @module spResizable.
    * @example
        
    Using the spResizable:

    &lt;sp-resizable="resizeOptions" sp-resizable-data="data" &gt;&lt;/sp-resizable&gt      

    where resizeOptions is an object containing the jQuery resizable options (http://api.jqueryui.com/resizable/) augmented with the following properties:
        - onResizeStart(event, element, data) {function}    - Function that fires when a resize operation begins.
            - event {object}                                - The jQuery 'resizestart' event object.
            - element {object}                              - The DOM element being resized.
            - data {object}                                 - The user-defined (optional) custom data.
        - onResizeStop(event, element, data) {function}     - Function fired when the resize operation completes.
            - event {object}                                - The jQuery 'resizestop' event object.
            - element {object}                              - The DOM element being resized.
            - data {object}                                 - The user-defined (optional) custom data.
        - enableDynamicOptionChanges {bool}                 - Enable dynamic changes of the resizable options.
        - changeId {int}                                    - An integer containing the config change id. Used to trigger config changes.
    where data is an object containing the custom data (optional) specified by the caller.
    */
    angular.module('mod.common.ui.spResizable', [])
        .directive('spResizable', function() {
            return {
                restrict: 'A',
                transclude: false,
                replace: true,
                scope: false,
                link: function(scope, element, attrs) {
                    var options = null,
                        data,
                        resizableElement,
                        jqueryWidgetInitialized = false;                    


                    attrs.$observe('spResizable', function (val) {                                             
                        if (!_.isUndefined(val)) {
                            /////
                            // Get the options.
                            /////
                            if (attrs.spResizable) {
                                options = scope.$eval(attrs.spResizable);
                            }                            

                            resizableElement = element;

                            if (options && options.resizableParentClass) {
                                resizableElement = element.parents(options.resizableParentClass).first();
                                if (!resizableElement || _.isEmpty(resizableElement)) {
                                    return;
                                }
                            }

                            if (attrs.spResizableData) {
                                data = scope.$eval(attrs.spResizableData);
                            }

                            if (!options || (!options.disabled)) {                                                           
                                createConfigureResizableElement();                                                                                                                                                             
                            }                            

                            // Handle dynamic resizable config changes
                            if (options && options.enableDynamicOptionChanges) {
                                scope.$watch(function () {
                                    return options ? options.changeId : 0;
                                }, function () {
                                    if (!options || !options.changeId) {
                                        return;
                                    }

                                    if (options.disabled) {
                                        // Destroy widget if it has been created
                                        if (jqueryWidgetInitialized) {
                                            resizableElement.resizable('destroy');
                                            resizableElement.off('resizestart', onResizeStart);
                                            resizableElement.off('resizestop', onResizeStop);
                                            jqueryWidgetInitialized = false;
                                        }                                        
                                    } else {
                                        // Apply config changes or re-create widget
                                        createConfigureResizableElement();
                                    }
                                });
                            }
                        }
                    });


                    function createConfigureResizableElement() {
                        var resizableOptions;

                        if (!resizableElement || _.isEmpty(resizableElement)) {
                            return;
                        }

                        if (options) {
                            if (options.handles) {
                                resizableOptions = {
                                    handles: options.handles
                                };
                            }

                            if (options.minHeight) {
                                resizableOptions = resizableOptions || {};
                                resizableOptions.minHeight = options.minHeight;
                            }

                            if (options.minWidth) {
                                resizableOptions = resizableOptions || {};
                                resizableOptions.minWidth = options.minWidth;
                            }

                            if (options.maxHeight) {
                                resizableOptions = resizableOptions || {};
                                resizableOptions.maxHeight = options.maxHeight;
                            }

                            if (options.maxWidth) {
                                resizableOptions = resizableOptions || {};
                                resizableOptions.maxWidth = options.maxWidth;
                            }
                        }

                        /////
                        // Mark the element as resizable.
                        /////
                        resizableElement.resizable(resizableOptions);

                        // Only create listeners if widget is being created not configured.
                        if (!jqueryWidgetInitialized) {
                            /////
                            // Fire the onResizeStart method if there is a listener.
                            /////
                            if (options && options.onResizeStart) {
                                resizableElement.on("resizestart", onResizeStart);
                            }

                            /////
                            // Fire the onResizeStop method if there is a listener.
                            /////
                            if (options && options.onResizeStop) {
                                resizableElement.on("resizestop", onResizeStop);
                            }
                        }

                        jqueryWidgetInitialized = true;
                    }


                    function onResizeStart(event, ui) {
                        event.stopPropagation();
                        options.onResizeStart(event, ui, data);
                    }


                    function onResizeStop(event, ui) {
                        event.stopPropagation();
                        options.onResizeStop(event, ui, data);
                    }
                }
            };
        });
}());