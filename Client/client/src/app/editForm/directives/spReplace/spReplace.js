// Copyright 2011-2016 Global Software Innovation Pty Ltd
/*global console, _, angular */

(function () {
    'use strict';

    /*
    * The spReplace directive is similar in operation to ng-include but replaces the DOM element
    * with the source template. An alternate scope can also be provided.
    * Note* The contents of the <sp-replace> element are NOT transcluded. Any attributes defined
    * on the <sp-replace> element are not transcluded either.
    *
    * Usage: <sp-replace source="getTemplateFile()"></sp-replace>
    *        <sp-replace source="getTemplateFile()" scope='getScope()></sp-replace>
    */
    angular.module('mod.app.editForm.designerDirectives.spReplace', ['mod.common.spCachingCompile'])
        .directive('spReplace', function (spCachingCompile) {
            
            return {
                restrict: 'AE',
                replace: true,
                link: function ($scope, $element, $attrs) {

                    var templateUrl = $scope.$eval($attrs.source);
                    var appliedScope = $attrs.scope ? $scope.$eval($attrs.scope) : $scope;

                    if (!appliedScope)
                        appliedScope = $scope;

                    var cachedLinkFunc = spCachingCompile.compile(templateUrl);
                    cachedLinkFunc(appliedScope, function (clone) {
                        $element.replaceWith(clone);
                    });                    
                }
            };
        });
}());