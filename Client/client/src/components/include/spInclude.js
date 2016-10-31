// Copyright 2011-2016 Global Software Innovation Pty Ltd
/*global console, _, angular */

(function () {
    'use strict';

   /*
   * The spInclude directive is similar to the ng-include directive with the following changes:
   *  - uses the spCacheingCompile service to get already compiled link functions for cached urls
   *  - doesn't emit the events that ng-include does
   *  - doesn't support direct SVG content
   *
   * Usage: <div sp-include="getTemplateFile()"></div>   
   */
    angular.module('mod.common.spInclude', ['mod.common.spCachingCompile'])
        .directive('spInclude', spIncludeDirective)
        .directive('spInclude', spIncludeFillContentDirective);

    /* @ngInject */
    function spIncludeDirective($templateRequest, $anchorScroll, $animate, $templateCache) {
        return {
            restrict: 'ECA',
            priority: 400,
            terminal: true,
            transclude: 'element',
            controller: angular.noop,
            compile: function (element, attr) {
                var srcExp = attr.spInclude || attr.src,
                    onloadExp = attr.onload || '',
                    autoScrollExp = attr.autoscroll;
                
                return function (scope, $element, $attr, ctrl, $transclude) {
                    var changeCounter = 0,
                        currentScope,
                        previousElement,
                        currentElement;

                    var cleanupLastIncludeContent = function () {
                        if (previousElement) {
                            previousElement.remove();
                            previousElement = null;
                        }
                        if (currentScope) {
                            currentScope.$destroy();
                            currentScope = null;
                        }
                        if (currentElement) {
                            $animate.leave(currentElement).then(function () {
                                previousElement = null;
                            });
                            previousElement = currentElement;
                            currentElement = null;
                        }
                    };

                    scope.$watch(srcExp, function spIncludeWatchAction(src) {
                        var afterAnimation = function () {
                            if (angular.isDefined(autoScrollExp) && (!autoScrollExp || scope.$eval(autoScrollExp))) {
                                $anchorScroll();
                            }
                        };
                        var thisChangeId = ++changeCounter;

                        function updateTemplate(template) {
                            if (thisChangeId !== changeCounter) return;
                            var newScope = scope.$new();
                            ctrl.template = template;
                            ctrl.templateSrcUrl = src;

                            // Note: This will also link all children of sp-include that were contained in the original
                            // html. If that content contains controllers, ... they could pollute/change the scope.
                            // However, using ng-include on an element with additional content does not make sense...
                            // Note: We can't remove them in the cloneAttchFn of $transclude as that
                            // function is called before linking the content, which would apply child
                            // directives to non existing elements.
                            var clone = $transclude(newScope,
                                function(clone) {
                                    cleanupLastIncludeContent();
                                    $animate.enter(clone, null, $element).then(afterAnimation);
                                });

                            currentScope = newScope;
                            currentElement = clone;
                            scope.$eval(onloadExp);
                        }

                        if (src) {
                            var template = $templateCache.get(src);
                            if (template) {
                                updateTemplate(template);
                            } else {
                                //set the 2nd param to true to ignore the template request error so that the inner
                                //contents and scope can be cleaned up.
                                $templateRequest(src, true)
                                    .then(function(response) {
                                            updateTemplate(response);
                                        },
                                        function() {
                                            if (thisChangeId === changeCounter) {
                                                cleanupLastIncludeContent();
                                            }
                                        });
                            }
                        } else {
                            cleanupLastIncludeContent();
                            ctrl.template = null;
                            ctrl.templateSrcUrl = null;
                        }
                    });
                };
            }
        };        
    }

    /* @ngInject */
    function spIncludeFillContentDirective(spCachingCompile) {
        return {
            restrict: 'ECA',
            priority: -400,
            require: 'spInclude',
            link: function (scope, $element, $attr, ctrl) {
                var cachedLinkFunc = spCachingCompile.compile(ctrl.templateSrcUrl, ctrl.template);                
                cachedLinkFunc(scope, function (clone) {
                    $element.append(clone);
                });                
            }
        };
    }
}());