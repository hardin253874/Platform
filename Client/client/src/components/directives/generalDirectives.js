// Copyright 2011-2016 Global Software Innovation Pty Ltd
/*global angular, console, $, Hammer */

(function () {
    'use strict';

    angular.module('sp.common.directives')
        .directive('spClickToEdit', function ($compile, $timeout) {
            return {
                restrict: 'E',
                scope: {
                    model: '=',
                    placeholder: '@',
                    editMode: '=?'
                },
                replace: true,
                template: '<div></div>',
                link: function (scope, el, attrs) {

                    scope.$watch('editMode', function () {

                        var inner;

                        scope.value = scope.model;

                        if (scope.editMode) {
                            inner = $compile('<input ng-model="value"/>')(scope);
                            inner.bind('keyup', function (e) {
                                if (e.keyCode === 27) {
                                    scope.$apply('editMode = false');
                                }
                            });
                            inner.bind('keypress', function (e) {
                                if (e.which === 13) {
                                    scope.$apply('model = value; editMode = false');
                                }
                            });
                            inner.bind('blur', function (e) {
                                if (scope.editMode) {
                                    scope.$apply('model = value; editMode = false');
                                }
                            });
                            $timeout(function () {
                                inner.focus();
                                inner.select();
                            }, 0);

                        } else {
                            inner = $compile('<div ng-click="editMode = true">{{model || placeholder}}</div>')(scope);
                        }
                        el.empty();
                        el.append(inner);
                    });
                }
            };
        })
        .directive('edcEnter', function () {
            return function (scope, el, attrs) {
                el.bind('keypress', function (e) {
                    if (e.which === 13 || e.which === 10) {
                        //console.log('edcEnter.enter pressed');
                        scope.$apply(attrs.edcEnter);

                        return false;
                    }
                });
                el.bind('blur', function (e) {
                    //console.log('edcEnter.blur, editMode=', scope.isEditMode);
                    if (scope.isEditMode) {
                        scope.$apply(attrs.edcEnter);
                    }
                });
            };
        })
        .directive('edcCancel', function () {
            return function (scope, el, attrs) {
                el.bind('keyup', function (e) {
                    if (e.keyCode === 27) {
                        scope.$apply(attrs.edcCancel);
                    }
                });
            };
        })
        .directive('edcFocus', function ($timeout) {
            return function (scope, el, attrs) {
                scope.$watch(attrs.edcFocus, function (val) {
                    if (angular.isDefined(val) && val) {
                        $timeout(function () {
                            el[0].focus();
                        }, 250);
                    }
                });
            };
        })
        .directive('spSetFocusDialogInput', function ($timeout) {
            return function (scope, element, attrs) {
                $timeout(function () {
                    if (element) {
                        var inputs = element.find('input:visible:first');
                        if (inputs && inputs.length > 0) {
                            var elem = inputs[0];
                            if (elem) {
                                elem.focus();
                            }
                        }
                    }
                }, 500);
            };
        })
        /**
         *
         * Directive implementing ellipsis on singline and multiline text
         * Truncate a long text and puts ellipsis at the end.
         *
         * @directive ellipsis
         * @example

         using ellipsis directive
         &lt;span class=styleClassName ellipsis={{textString}} callback=ellipsisCallback&gt;&lt;/span&gt;
         where 'textString' is the scope variable and the value will be set as element text.
         the directive returns a call back function(e:g 'ellipsisCallback') with value true/false indicating whether the text is truncated or not,
         which going to execute on the controller.

         Note: in the styleClass width of the element has to specify to make the ellipsis work.

         *
         *
         **/
        .directive('ellipsis', function () {
            return {
                scope: {
                    ellipsis: '@',
                    callback: '='
                },
                link: function (scope, element, attrs) {
                    attrs.$observe('ellipsis', function () {
                        if (attrs.ellipsis.length > 0) {
                            //console.log(' type:', attrs.ellipsis);
                            element.text(attrs.ellipsis);
                            //using third party plugin to get ellipsis for multiline and singleline.
                            $(element).dotdotdot({wrap: 'letter'});
                            var isTruncated = $(element).triggerHandler('isTruncated');
                            scope.callback(isTruncated);
                        } else {
                            element.text('');
                        }
                    });
                }
            };
        })


        /**
         *
         * Directive used to provide tool box buttons.
         * buttons takes an array of objects with the following properties:
         *       * text: The alternative text for the button.
         *       * icon: (Optional) The image to display
         *       * click: The function to run when the button is clicked
         *       * order: (Optional) The order the buttons will appear.
         *       * disabled: (Optional) Is the button disabled.
         *       * hidden: (Optional) Is the button hidden.
         *
         **/
        .directive('spToolBox', function () {
            return {
                restrict: 'E',
                scope: {
                    buttons: '=buttons'
                },
                templateUrl: 'directives/spToolBox.tpl.html'

            };
        })
        .directive('spProgress', function () {
            return {
                // Replace the directive
                replace: true,
                // Only use as a element
                restrict: 'E',
                // The actual html that will be used
                templateUrl: 'directives/spProgressbar.tpl.html',
                scope: {
                    percent: '@'
                },
                link: function (scope, $element, $attrs, $controller) {
                    // Watch the percent on the $rootScope. As soon as percent changes to something that
                    // isn't undefined or null, change the percent on $scope and also the width of
                    // the progressbar.
                    // console.log('from directive', scope.model.importPercent);
                    console.log('from directive', $attrs.percent);

                    scope.$watch('percent', function (newVal) {
                        if (newVal !== undefined) {
                            scope.percent = newVal;
                            $element.eq(0).children().children().css('width', newVal + '%');
                        }
                    });
                }
            };
        })
        /*
         ** Directive to respond to a touch start. Used in cases where 'ghost clicks' are causing issues on mobile.
         */
        .directive('spFirstTouch', ['$parse', function ($parse) {


            return function (scope, element, attrs) {
                var fn = $parse(attrs.spFirstTouch);

                element.bind('touchstart', function (event) {
                    event.preventDefault();
                    event.stopPropagation();

                    if (event.handled !== true) {
                        scope.$apply(function () {
                            fn(scope, {$scope: scope, $event: event});
                        });
                    } else {
                        return false;
                    }
                });
            };
        }])

        /*
         * Directive marks any drag events generated by it as approved to propergate on a mobile device. This is used to prevent the "bouncing" display on IOS.
         * this works in conjunction with spMobileDenyUnapprovedDrags.
         *
         **/
        .directive('xxxxspMobileAllowDefaultDrag', function () {

            //THIS IS NOT WORKING AT THE MOMENT on IOS.

            return {
                replace: false,
                restrict: 'A',
                link: function (scope, element, attrs) {
                    var hammer = Hammer(element[0]);    // jshint ignore:line

                    hammer.on('dragup dragdown', function (event) {
                        console.log("allowDrag:", event);
                        event.allowMobileDrag = true;
                    });
                }
            };
        })
        /*
         * This directive prevents the default behavior for drag events unless the event has been marked using spMobileAllowDefaultDrag
         *
         **/
        .directive('xxxxspMobileDenyUnapprovedDrags', function () {

            //THIS IS NOT WORKING AT THE MOMENT on IOS.

            return {
                replace: false,
                restrict: 'A',
                link: function (scope, element, attrs) {
                    var hammer = Hammer(element[0]);    // jshint ignore:line

                    hammer.on('dragup dragdown', function (event) {
                        if (!event.allowMobileDrag) {
                            console.log("denyDrag:", event);
                            event.gesture.preventDefault();
                        }
                    });
                }
            };
        })
        /*
         * This directive injects XSRF Token if any to the SRC attribute of IMG, SCRIPT, etc elements in order to pass potential server-side XSRF verification.
         * If XSRF Token is not available [e.g. pre login SRC interpolation] then we simply don't add the token assuming server is happy to server anonymously.
         * To added reliability this directive chains with ng's built-in ngSrc directive with priority set above 99 to ensure it runs after SRC attribute is populated.
         *
         */
        .directive('ngSrc', function (spXsrf) {
            return {
                restrict: 'A',
                priority: 100,
                link: function (scope, element, attr) {
                    attr.$observe('src', function (value) {
                        if (!value) {
                            return;
                        }

                        if (spXsrf.uriContainsXsrfToken(value)) {
                            return;
                        }

                        if (value.match("^data:")) {
                            return;
                        }

                        var newSrc = spXsrf.addXsrfTokenAsQueryString(value);

                        element[0].src = newSrc;
                        attr.src = newSrc; // IE is not always happy with just the line above
                    });
                }
            };
        });
}());
