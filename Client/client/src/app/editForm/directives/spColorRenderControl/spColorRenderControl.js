// Copyright 2011-2016 Global Software Innovation Pty Ltd
(function() {
    'use strict';

    /////
    // The spCurrencyKFieldRenderControl directive provides a bridge between
    // the edit form scope and the isolated render control, mapping the
    // relevant properties from one to the other.
    /////
    angular.module('mod.app.editForm.designerDirectives.spColorRenderControl', ['mod.app.editForm', 'mod.common.ui.spDialogService', 'mod.common.spCachingCompile'])
     .directive('spColorRenderControl', function (spEditForm, spFieldValidator, spDialogService, spCachingCompile) {
         return {
             restrict: 'E',
             replace: false,
             scope: {                 
                 colorstring: '='
             },
             link: function (scope, element) {
                 scope.color = {};
                 
                 // Watching the input colorstring
                 scope.$watch('colorstring', function () {
                     if (scope.colorstring && scope.colorstring.length > 0) {
                         scope.color = spUtils.getColorFromARGBString(scope.colorstring);
                     } else {
                         scope.color = { a: 0, r: 0, g: 0, b: 0 };
                     }
                 }, true);
                 
                 // Watching the color from spColorPicker
                 scope.$watch('color', function () {
                     if (scope.color && scope.color.a !== undefined && scope.color.a !== null) {
                         if (scope.color.a === 0 &&
                             scope.color.r === 0 &&
                             scope.color.g === 0 &&
                             scope.color.b === 0) {
                             scope.colorstring = null;
                         } else {
                             scope.colorstring = spUtils.getARGBStringFromRgb(scope.color);
                         }
                     } else {
                         scope.colorstring = null;
                     }
                 }, true);

                 var cachedLinkFunc = spCachingCompile.compile('editForm/directives/spColorRenderControl/spColorRenderControl.tpl.html');
                 cachedLinkFunc(scope, function (clone) {
                     element.append(clone);
                 });
             }
         };
     });
}());