// Copyright 2011-2016 Global Software Innovation Pty Ltd
(function () {
    'use strict';

    /**
    * Module implementing a event update directives.
    * These directives update in response to events as opposed to regular dirty watching using watches.
    * This module contains the following directives:
    * <ul>
    *   <li>spEvtStyle - designed to replace ngStyle</li>
    *   <li>spEvtText - designed to replace interpolated text</li>
    *   <li>spEvtClass - designed to replace ngClass</li>    
    * </ul>    
    * 
    * @module spEventDirectives            
    * @example
        
    Using the directives:
    
    &lt;div sp-evt-style="getStyle()" sp-evt-class="getClass()" sp-evt-text="getText()" sp-evt-name="'evtUpdate'" /div&gt;          
    
    When the evtUpdate is raised on the scope the directives will evaluate the set expression.

    Note: the directives also support using a local event name instead of a shared one.

    &lt;div sp-evt-style="getStyle()" sp-evt-style-name="'evtStyleUpdate'" sp-evt-class="getClass()" sp-evt-class-name="'evtClassUpdate'" sp-evt-text="getText()" sp-evt-text-name="'evtUpdateText'" /div&gt;          

    When evtStyleUpdate is raised the style will be updated.
    When evtClassUpdate is raised the class will be updated.
    When evtUpdateText is raised the text will be updated.
    * 
    */


    // Get the options for the directive
    function getOptions(directive, scope, iAttrs, $parse) {
        var expression = $parse(iAttrs[directive]),
            eventName =  $parse(iAttrs[directive + 'Name'])(scope);        

        // Fallback to shared event name
        if (!eventName) {
            eventName =  $parse(iAttrs.spEvtName)(scope);                    
        }

        return {
            expression: expression,
            eventName: eventName
        };
    }    

    function setOneTimeBinding(scope, options, callback) {
        if (!scope || !options || !options.expression || !callback) {
            return;
        }

        // Check for a value
        var bindingValue = options.expression(scope);        
        if (bindingValue !== undefined) {
            // Run the callback and exit
            callback(bindingValue);
            return;
        }

        // Create a watcher to wait for a value
        var watcherRemover = scope.$watch(function() {
            if (!options || !options.expression) {
                return undefined;
            }
            return options.expression(scope);
        }, function(newValue) {
            if (newValue === undefined) {
                return;
            }

            removeWatcher();

            callback(newValue);
        });

        function removeWatcher() {
            if (watcherRemover) {
                watcherRemover();
            }
        }

        scope.$on('$destroy', removeWatcher);
    }

    angular.module('mod.common.spEventDirectives', ['ng'])
        .directive('spEvtStyle', spEvtStyle)
        .directive('spEvtText', spEvtText)
        .directive('spEvtClass', spEvtClass);

    /* @ngInject */
    function spEvtStyle($parse) {
        return {
            restrict: 'A',
            link: spEvtStyleLink
        };

        function spEvtStyleLink(scope, iElement, iAttrs) {
            // Get the options
            var options = getOptions('spEvtStyle', scope, iAttrs, $parse),
                oldStyle;

            // Early out if not valid
            if (!options ||
                !options.eventName ||
                !options.expression) {
                return;
            }

            scope.$on('$destroy',
                function() {
                    options = null;
                });

            function updateElement(newStyle) {
                if (_.isEqual(newStyle, oldStyle)) {
                    return;
                }

                if (oldStyle) {
                    // Clear old style
                    angular.forEach(oldStyle, function(val, style) { iElement.css(style, ''); });
                }

                if (newStyle) {
                    // Apply new style
                    iElement.css(newStyle);
                }

                oldStyle = newStyle;
            }

            // Setup event handler and update style
            scope.$on(options.eventName,
                function() {
                    if (!options || !options.expression) {
                        return;
                    }

                    var newStyle = options.expression(scope);

                    updateElement(newStyle);
                });

            setOneTimeBinding(scope, options, updateElement);
        }
    }

     /* @ngInject */
    function spEvtText($parse) {
        return {
            restrict: 'A',
            link: spEvtTextLink
        };

        function spEvtTextLink(scope, iElement, iAttrs) {
            // Get the options
            var options = getOptions('spEvtText', scope, iAttrs, $parse),
                oldText;

            // Early out if not valid
            if (!options ||
                !options.eventName ||
                !options.expression) {
                return;
            }

            scope.$on('$destroy',
                function() {
                    options = null;
                });

            function updateElement(newText) {
                if (newText !== oldText) {
                    iElement.text(newText);
                }

                oldText = newText;
            }

            // Setup event handler and update text
            scope.$on(options.eventName,
                function() {
                    if (!options || !options.expression) {
                        return;
                    }

                    var newText = options.expression(scope);

                    updateElement(newText);
                });

            setOneTimeBinding(scope, options, updateElement);
        }
    }

    /* @ngInject */
    function spEvtClass($parse) {
        return {
            restrict: 'A',
            link: spEvtClassLink
        };

        function spEvtClassLink(scope, iElement, iAttrs) {
            // Get the options
            var options = getOptions('spEvtClass', scope, iAttrs, $parse),
                oldClasses;

            // Early out if not valid
            if (!options ||
                !options.eventName ||
                !options.expression) {
                return;
            }


            // Convert the class object to a space separated class list
            function convertToString(classVal) {
                if (_.isArray(classVal)) {
                    return classVal.join(' ');
                } else if (_.isObject(classVal)) {
                    var classes = [];
                    angular.forEach(classVal,
                        function(v, k) {
                            if (v) {
                                classes.push(k);
                            }
                        });
                    return classes.join(' ');
                }

                return classVal;
            }

            function updateElement(newClass) {
                var newClasses = convertToString(newClass),
                    newClassesArray = [],
                    oldClassesArray,
                    classesToAdd,
                    classesToRemove;

                if (!oldClasses) {
                    iElement.addClass(newClasses);
                } else if (newClasses !== oldClasses) {
                    if (newClasses) {
                        newClassesArray = newClasses.split(/\s+/);
                    }
                    oldClassesArray = oldClasses.split(/\s+/);

                    classesToAdd = _.without(newClassesArray, oldClassesArray);
                    classesToRemove = _.without(oldClassesArray, newClassesArray);

                    if (classesToAdd &&
                        classesToAdd.length) {
                        iElement.addClass(convertToString(classesToAdd));
                    }

                    if (classesToRemove &&
                        classesToRemove.length) {
                        iElement.removeClass(convertToString(classesToRemove));
                    }
                }

                oldClasses = newClasses;
            }

            scope.$on('$destroy',
                function() {
                    options = null;
                });

            // Setup event handler and update class
            scope.$on(options.eventName,
                function() {
                    if (!options || !options.expression) {
                        return;
                    }

                    var newClass = options.expression(scope);

                    updateElement(newClass);
                });

            setOneTimeBinding(scope, options, updateElement);
        }
    }   
}());