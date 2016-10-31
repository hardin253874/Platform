// Copyright 2011-2016 Global Software Innovation Pty Ltd
/*global console, _, angular */

(function() {
    'use strict';

    /////
    // The custom validation message will display any custom validation message propegated up to its parent scope.
    /////
    angular.module('mod.app.editForm.designerDirectives.spCustomValidationMessage', ['mod.common.spCachingCompile', 'mod.common.spMobile'])
        .directive('spCustomValidationMessage', function (spCachingCompile, spMobileContext) {

            /////
            // Directive structure.
            /////
            return {
                restrict: 'E',
            
                scope: {
                    messages: '='
                },                                
                link: function ($scope, element) {
                    // set tooltip trigger and placement
                    $scope.tooltipTrigger = spMobileContext.isMobile ? 'click' : 'mouseenter';
                    $scope.tooltipPlacement = spMobileContext.isMobile ? 'left' : 'top';

                    $scope.$watch('messages', function (newMessages) {
                        if (newMessages) {
                            $scope.validationMessage = newMessages.join(' - ');
                        } else {
                            $scope.validationMessage = '';
                        }
                    }, true);

                    var cachedLinkFunc = spCachingCompile.compile('editForm/directives/spCustomValidationMessage/spCustomValidationMessage.tpl.html');
                    cachedLinkFunc($scope, function (clone) {
                        element.append(clone);
                    });
                }
            };
        });
}());