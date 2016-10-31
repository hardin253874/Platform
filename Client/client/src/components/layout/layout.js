// Copyright 2011-2016 Global Software Innovation Pty Ltd
/*global _, console, angular */

(function () {
    'use strict';

    // convenience aliases
    var forEach = _.forEach,
        partial = _.partial;

    /**
     * Some notes on this guy...
     *
     * Need to see if there are way to optimise the resizing say via changing styles on CSS classes
     * to reduce reflow effort and other. Anyway, is functional now but might be made faster,
     * important for slower devices.
     */

    /**
     * Each of the resizers includes references to DOM elements and a resize function.
     * See their usage in the directives below.
     */
    var resizers = [];

    var registerResizer, deregisterResizer;

    function hasClass(c, e) {
        return new RegExp('(^|\\s)' + c + '(\\s|$)').test(e.className);
    }

    var isLayoutHidden = partial(hasClass, 'layout-hide');
    var isLayoutOpen = partial(hasClass, 'layout-open');
    var isLayoutTop = partial(hasClass, 'layout-top');
    var isLayoutBottom = partial(hasClass, 'layout-bottom');
    var isLayoutLeft = partial(hasClass, 'layout-left');
    var isLayoutRight = partial(hasClass, 'layout-right');
    var isLayoutMiddle = partial(hasClass, 'layout-middle');

    function sumProperty(collection, property, initial) {
        return _.reduce(collection, function (a, o) {
            return a + o[property];
        }, initial || 0);
    }

    function setElementStyle(style, value, element) {
        element.style[style] = value;
    }

    function getDataAttribute(e, attrName, defaultValue) {
        var v = e.getAttribute('data-' + attrName);
        return v ? v : defaultValue;
    }

    function layoutWidth(e) {
        return getDataAttribute(e, 'layout-width', 200);
    }

    function layoutClosedWidth(e) {
        return getDataAttribute(e, 'layout-closed-width', 20);
    }

    function layoutHeight(e) {
        return getDataAttribute(e, 'layout-height', 0);
    }

    function layoutClosedHeight(e) {
        return getDataAttribute(e, 'layout-closed-height', 20);
    }

    function nodeDepth(e) {
        var depth = 0, parent = e;
        while (parent) {
            depth += 1;
            parent = parent.parentElement;
        }
        return depth;
    }

    angular.module('mod.common.ui.layout', [])
        .run(function ($rootScope, $timeout, $window) {

            // set up some functions to handle resizing

            var onResize = function () {
                //console.time('resize');
                forEach(resizers, function (resizer) {
                    resizer.resize(resizer);
                });
                                
                //console.timeEnd('resize');
            };

            var onTransitionEnd = function (resizer, event) {
                if (event.target.parentElement !== resizer.container && event.target !== resizer.container) {
                    //console.log('resizer: skip this transition end', [event.target], [resizer.container]);
                    return;
                }
                //console.log('resizer: transitionEnd', event, resizer);
                onResize();
                $rootScope.$broadcast('window.resize');

            };

            // the following are used by the layout and sizing directives below

            registerResizer = function (container, resizeCallback) {
                //console.log('register resizer', [container]);

                var resizer = _.find(resizers, { container: container, resize: resizeCallback });

                if (!resizer) {
                    resizer = {
                        container: container,
                        resize: resizeCallback,
                        depth: nodeDepth(container),
                        refCnt: 0
                    };

                    resizers = _.sortBy(resizers.concat([resizer]), 'depth');
                }

                if (!resizer.transitionEndHandler) {
                    resizer.transitionEndHandler = partial(onTransitionEnd, resizer);
                    //resizer.transitionEndHandler = _.debounce(partial(onTransitionEnd, resizer), 10);
                    resizer.container.addEventListener('animationend', resizer.transitionEndHandler);
                    resizer.container.addEventListener('transitionend', resizer.transitionEndHandler);
                }

                resizer.refCnt += 1;
                return resizer;
            };

            deregisterResizer = function (resizer) {
                //console.log('deregister resizer', resizer);

                if ((resizer.refCnt -= 1) === 0) {

                    resizers = _.reject(resizers, resizer);

                    if (resizer.transitionEndHandler) {
                        resizer.container.removeEventListener('animationend', resizer.transitionEndHandler);
                        resizer.container.removeEventListener('transitionend', resizer.transitionEndHandler);
                        resizer.transitionEndHandler = null;
                    }
                }
            };

            var resizeHandler = _.debounce(function () { $rootScope.$apply(onResize); }, 50);
            $window.addEventListener('resize', resizeHandler);
            $rootScope.$on('app.layout', function (){                                
                $timeout(resizeHandler, 0).then(function () {
                    //console.log('app.layout.done');
                    $rootScope.$broadcast('app.layout.done');
                });
            });

            // kick off an initial app layout...
            $timeout(resizeHandler, 0);
        })
        .directive('spAutoHeight', function ($window) {

            //console.log('spAutoHeight ctor');

            function resize(resizer) {

                var parent = resizer.container;
                var registered = resizer.elements;
                var availHeight = parent.offsetHeight;

//                console.log('auto height calcs here....', parent.children, registered);
//                console.log('spAutoHeight: parent height', availHeight, [parent], $window.getComputedStyle(parent, null).getPropertyValue('height'));

                forEach(parent.children, function (e) {
                    if (!_.includes(registered, e)) {
                        e.style.height = 'auto';
                        //console.log('size up unreg child', availHeight, e.offsetHeight, getDataAttribute(e, 'debug-id', '??'), e.className);

                        availHeight -= e.offsetHeight;

                        // doing the following to work around an issue on Chrome
                        e.style.height = e.offsetHeight + 'px';
                    }
                });

                if (registered.length && availHeight > 0) {
                    availHeight = availHeight / registered.length;
                    forEach(registered, function (e) {
//                        console.log('set size registered', availHeight);
                        e.style.height = availHeight + 'px';
                    });
                }
            }

            return {
                link: function (scope, el, attrs) {

                    //console.log('spAutoHeight link', scope, el);

                    var resizer = registerResizer(el[0].parentElement, resize);
                    resizer.elements = (resizer.elements || []).concat([el[0]]);

                    el.bind('$destroy', function () {
                        deregisterResizer(resizer);
                        resizer = null;
                    });
                }
            };
        })
        .directive('spLayout', function ($rootScope, $window) {

            //console.log('spLayout ctor');

            function showChildren(e) {
                forEach(_.filter(e.children, isLayoutHidden), partial(setElementStyle, 'opacity', '1.0'));
            }

            function hideChildren(e) {
                forEach(_.filter(e.children, isLayoutHidden), partial(setElementStyle, 'opacity', '0'));
            }

            function updateLayout(resizer) {

                var e = resizer.container;
                var children = e.children;

                resizer.depth = nodeDepth(e);
                resizer.elements = {
                    top: _.filter(children, isLayoutTop),
                    bottom: _.filter(children, isLayoutBottom),
                    left: _.filter(children, isLayoutLeft),
                    right: _.filter(children, isLayoutRight),
                    middle: _.filter(children, isLayoutMiddle)
                };
                resizer.elements.bottom = resizer.elements.bottom.reverse();
                resizer.elements.right = resizer.elements.right.reverse();
            }

            function resize(resizer) {
                // we are assuming CSS is used to position the top and bottom elements
                // and here we are only position the middle under the combined tops and
                // sizing to fit in between them and the bottoms.

                var availHeight = resizer.container.offsetHeight || $window.innerHeight;
                var availWidth = resizer.container.offsetWidth || $window.innerWidth;

//                console.group('spLayout resize: ', resizer.depth, availWidth, availHeight);

//                console.log('resize: heights', 'container', resizer.container.className, resizer.container.offsetHeight,
//                    'container parent', resizer.container.parentElement.className, resizer.container.parentElement.offsetHeight,
//                    'window', $window.innerHeight);
//                console.log('resize: widths', 'container', resizer.container.className, resizer.container.offsetWidth,
//                    'container parent', resizer.container.parentElement.className, resizer.container.parentElement.offsetWidth,
//                    'window', $window.innerWidth);

//                var cs = $window.getComputedStyle(resizer.container.parentElement, null);
//                availHeight -= parseInt(cs.getPropertyValue('padding-top'), 10) + parseInt(cs.getPropertyValue('padding-bottom'), 10);
//                availWidth -= parseInt(cs.getPropertyValue('padding-left'), 10) + parseInt(cs.getPropertyValue('padding-right'), 10);

//                console.log('resize avail after padding', availWidth, availHeight);

//                console.log('resize', [resizer.container], resizer.elements);
//                console.log('resize window', $window.innerWidth, $window.innerHeight);
//                console.log('resize offset', resizer.container.offsetWidth, resizer.container.offsetHeight);
//                console.log('resize parent', resizer.container.parentElement.offsetWidth, resizer.container.parentElement.offsetHeight, resizer.container.parentElement);
//                console.log('resize: parent and container', $window.getComputedStyle(resizer.container.parentElement, null), $window.getComputedStyle(resizer.container, null));

                var elements = resizer.elements;
                var top = 0;
                var bottom = availHeight;
                var left = 0;
                var right = availWidth;

                forEach(elements.top, function (e) {
                    var style = {
                        position: 'absolute',
                        top: top + 'px',
                        width: '100%'
                    };                    
                    $(e).css(style);
                    top += e.offsetHeight;
                });
                forEach(elements.bottom, function (e) {
                    var style = {
                        position: 'absolute',
                        width: '100%'
                    };
                    
                    if (isLayoutHidden(e)) {
                        style.overflow = 'hidden';
                        style.height = '0';
                    } else {
                        if (isLayoutOpen(e)) {
                            if (layoutHeight(e)) {
                                style.height = layoutHeight(e) + 'px';
                                bottom -= layoutHeight(e);
                            } else {
                                bottom -= e.offsetHeight;
                            }
                            showChildren(e);
                            style.overflow = 'visible';
                        } else {
                            style.height = layoutClosedHeight(e) + 'px';
                            bottom -= layoutClosedHeight(e);
                            hideChildren(e);
                            style.overflow = 'hidden';
                        }
                    }
                    style.top = bottom + 'px';
                    $(e).css(style);
                });
                forEach(elements.left, function (e) {
                    var style = {
                        position: 'absolute',
                        top: top + 'px',
                        height: (bottom - top) + 'px'
                    };                    

                    if (isLayoutHidden(e)) {
//                        console.log('hiding', getDataAttribute(e, 'debug-id', '??'), e.className);
                        style.overflow = 'hidden';
                        style.left = (-layoutWidth(e)) + 'px';
                        style.width = '0';

                    } else {
                        style.overflow = 'visible';
                        style.left = left + 'px';
                        if (isLayoutOpen(e)) {
//                            console.log('showing open', getDataAttribute(e, 'debug-id', '??'), e.className);
                            style.width = layoutWidth(e) + 'px';
                            left += layoutWidth(e);
                            showChildren(e);
                        } else {
//                            console.log('showing closed', getDataAttribute(e, 'debug-id', '??'), e.className);
                            style.width = layoutClosedWidth(e) + 'px';
                            left += layoutClosedWidth(e);
                            hideChildren(e);
                        }
                    }
                    $(e).css(style);
                });
                forEach(elements.right, function (e) {
                    var style = {
                        position: 'absolute',
                        top: top + 'px',
                        height: (bottom - top) + 'px'
                    };                    

                    if (isLayoutHidden(e)) {
                        style.overflow = 'hidden';
                        style.width = '0';
                        style.left = right + 'px';

                    } else {
                        if (isLayoutOpen(e)) {
                            style.width = layoutWidth(e) + 'px';
                            right -= layoutWidth(e);
                            showChildren(e);
                            style.overflow = 'visible';
                        } else {
                            style.width = layoutClosedWidth(e) + 'px';
                            right -= layoutClosedWidth(e);
                            hideChildren(e);
                            style.overflow = 'hidden';
                        }
                        style.left = right + 'px';
                    }
                    $(e).css(style);
                });
                var middleHeight = (bottom - top) / (elements.middle.length || 1);
                forEach(elements.middle, function (e) {
                    var style = {
                        position: 'absolute',
                        top: top + 'px',
                        height: middleHeight + 'px',
                        left: left + 'px',
                        width: (right - left) + 'px'
                    };                    
                    top += middleHeight;
                    $(e).css(style);
                });

//                console.groupEnd();
            }

            function onResize(resizer) {
                updateLayout(resizer);
                resize(resizer);                
            }

            return {
                link: function (scope, el, attrs) {

                    //console.log('spLayout link', scope, el);

                    var resizer = registerResizer(el[0], onResize);

                    el.bind('$destroy', function () {
                        console.log('spLayout $destroy', scope.$id);
                        deregisterResizer(resizer);
                        resizer = null;
                    });
                }
            };
        });

}());

