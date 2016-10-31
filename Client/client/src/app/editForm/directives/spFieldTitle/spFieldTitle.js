// Copyright 2011-2016 Global Software Innovation Pty Ltd
/*global console, _, angular */

(function () {
    'use strict';

    /////
    // The spFieldTitle directive provides a title to a field for
    // use in edit form.
    /////
    angular.module('mod.app.editForm.designerDirectives.spFieldTitle', ['mod.common.spCachingCompile'])
        .directive('spFieldTitle', function (spCachingCompile) {

            /////
            // Directive structure.
            /////
            return {
                restrict: 'AE',
                replace: false,
                transclude: false,
                scope: {
                    model: '=?'
                },
                link: function(scope, element) {
                    var cachedLinkFunc = spCachingCompile.compile('editForm/directives/spFieldTitle/spFieldTitle.tpl.html');

                    scope.toolTips = '';

                    scope.$watch('scope.model', function () {

                        var name = scope.model && scope.model.name ? scope.model.name : null;
                        var description = scope.model && scope.model.description ? scope.model.description : null;
                        if (name && description)
                            scope.toolTips = name ? name + '; ' + description : description;
                        else if (name)
                            scope.toolTips = name;
                        else
                            scope.toolTips = description;

                    });


                    cachedLinkFunc(scope, function (clone) {
                        element.append(clone);
                    });
                }
            };
        });
}());