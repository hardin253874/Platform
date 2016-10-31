// Copyright 2011-2016 Global Software Innovation Pty Ltd
/*global console, _, angular */

(function () {
    'use strict';
    var registerMessage = 'layoutManagerRegister';
    var deregisterMessage = 'layoutManagerDeregister';
    var resizeDebounceWindow = 50;
    var maxEvalCycles = 20;

    /*
    ** spEditFormLayout and spEditFormLayoutManager work together to ensure that layout recalculations happen in a single pass
    ** and do not cause additional digest cycles to occur.
    */

    angular.module('mod.app.editForm.designerDirectives.spEditFormLayout', [])

    /*
    ** Register this element as one requiring layout management
    */
        .directive('spEditFormLayout', [function () {
            /////
            // Directive structure.
            /////
            return {
                restrict: 'A',
                replace: false,
                transclude: false,
                scope: false,
                link: function (scope, element) {

                    /////
                    // Resize when the parent elements size changes.
                    /////

                    scope.$emit(registerMessage, element);

                    scope.$on('$destroy', function () {
                        scope.$emit(deregisterMessage, element);
                    });
                }
            };
        }])

    /*
    ** Create a layout manager for all the the layout elements below it. 
    ** Layout managers should be able to be nested, although this is not an ideal arrangement as additional resize cycles can occur.
    */
        .directive('spEditFormLayoutManager', [function () {

            // Find a parent that has a size set against it.
            function findSizedParent(element) {
                var parent = element.parent();

                while (parent && parent.width() === 0) {
                    parent = parent.parent();
                }

                return parent;
            }

            /////
            // Resize function.
            // Resize an element based upon it' siblings and the first parent that has been specifically sized.
            // This function does not require scope to evaluate. 
            /////
            function resize(element) {

                var parent;
                var parentWidth;
                var siblings;
                var autoSizeControls = [];
                var totalManualWidth = 0;
                var totalAutoWidth = 0;

                siblings = element.siblings().andSelf();

                parent = findSizedParent(element);

                if (parent) {
                    parentWidth = parent.width();
                }

                if (parentWidth) {
                    _.forEach(siblings, function (sibling) {

                        if (sibling) {
                            var siblingElement = $(sibling);

                            if (siblingElement) {
                                var siblingScope = siblingElement.scope();

                                if (siblingScope) {
                                    var containedControlOnForm = siblingScope.formControl;

                                    if (containedControlOnForm && containedControlOnForm.renderingResizeMode && containedControlOnForm.renderingResizeMode.alias() !== 'console:resizeAutomatic') {

                                        if (!siblingElement.attr('data-sized')) {

                                            /////
                                            // Set the width and height based on the stored percentages.
                                            /////
                                            siblingElement.width(Math.floor((containedControlOnForm.renderingWidth * parentWidth) / 100));
                                            siblingElement.height(containedControlOnForm.renderingHeight);

                                            /////
                                            // Marker to indicate that this element has now been sized.
                                            /////
                                            siblingElement.attr('data-sized', 'true');
                                        }

                                        totalManualWidth += siblingElement.outerWidth(true);
                                    } else {
                                        autoSizeControls.push({ element: siblingElement, control: containedControlOnForm });
                                    }
                                }
                            }
                        }
                    });

                    if (autoSizeControls && autoSizeControls.length) {
                        var autoWidth = parentWidth - totalManualWidth;

                        if (autoWidth < 0) {
                            autoWidth = 0;
                        } else {
                            autoWidth = Math.floor(autoWidth / autoSizeControls.length);
                        }

                        var counter = 0;

                        _.forEach(autoSizeControls, function (autoSizeControl) {
                            if (autoSizeControl) {
                                var control = autoSizeControl.element;

                                counter++;

                                var thisWidth = autoWidth - parseFloat(control.css('margin-left')) - parseFloat(control.css('margin-right'));

                                control.width(thisWidth);

                                totalAutoWidth += control.outerWidth(true);

                                if (counter === autoSizeControls.length) {

                                    if (totalManualWidth + totalAutoWidth !== parentWidth) {

                                        thisWidth += parentWidth - (totalManualWidth + totalAutoWidth);
                                        control.width(thisWidth);
                                    }
                                }
                            }
                        });
                    }
                }
            }

            // for the given list of elementNodes, determine the new parent sizes.
            function calcParentSizes(elements) {
                _.map(elements, function (e) {
                    e.lastParentSize = e.parentSize;
                    var sizedParent = findSizedParent(e.element);
                    e.parentSize = sizedParent && sizedParent.width();
                });
            }

            // Do any of the elements have a parent that has changed size.
            function changedElements(elements) {
                return _.filter(elements, function (e) { return e.lastParentSize !== e.parentSize;});
            }

            // Keep resizing until there are no more parent changes.
            function resizeUntilStable(elements) {

                //console.log('resizeUntilStable ', elements.length);

                if (elements.length) {
                    var start = new Date();

                    var loopCount = 0;

                    var changed = changedElements(elements);

                    while (changed.length && loopCount < maxEvalCycles) {
                        _.forEach(_.map(elements, 'element'), resize);
                        calcParentSizes(elements);
                        changed = changedElements(elements);
                    }

                    if (loopCount >= maxEvalCycles) {
                        console.error('spEditFormLayoutManager: Exceeded maximum evaluation cycles. Probably loop.');
                    }

                    var duration = new Date() - start;
                    console.log('spEditFormLayoutManager: resize duration/elements/cycles: ', duration, '/', _.size(elements), '/', loopCount);
                }
            }


            return { 
                restrict: "A",

                link: function (scope) {
                    
                    var resizeElements = [];
                    var changeCount = 0;
                    
                    // The resize function is debounced and runs outside the digest cycle.
                    var debouncedResize = _.debounce(_.partial(resizeUntilStable, resizeElements), resizeDebounceWindow);

                    function registerElement(event, element) {
                        event.stopPropagation();
                        resizeElements.push({ element: element });
                        //console.log('spEditFormLayout registered ', _.size(resizeElements));
                        debouncedResize();
                    }

                    function deregisterElement(event, element) {
                        event.stopPropagation();
                        _.remove(resizeElements, function (e) { return e.element === element; });
                        //console.log('spEditFormLayout deregistered ', _.size(resizeElements));
                        debouncedResize();
                    }

                    // have any of the elements parents changed size? 
                    function parentsChanged() {
                        calcParentSizes(resizeElements);

                        var changed = changedElements(resizeElements);

                        if (changed.length) {
                            //console.log('parentsChanged');
                            changeCount++;
                        }

                        return changeCount;
                    }

                    var debouncedParentsChanged = _.debounce(parentsChanged, resizeDebounceWindow);

                    // add and remove the elements from the managed elements. Then every digest cycle check if any need to be resized.
                    scope.$on(registerMessage, registerElement);
                    scope.$on(deregisterMessage, deregisterElement);

                    scope.$watch(debouncedParentsChanged, debouncedResize);

                }
            };
        }])
    
        ;
}());