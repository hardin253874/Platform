// Copyright 2011-2015 Global Software Innovation Pty Ltd

(function () {
    'use strict';

    angular.module('sp.common.ui.spExpressionCell', ['mod.common.spCachingCompile'])
        .directive('spExpressionCell', function($q, $parse, $timeout, spCachingCompile) {
            return {
                restrict: 'E',
                replace: false,
                transclude: false,
                scope: {
                    cell: "=?"
                },
                link: function (scope, element) {

                    scope.model = {
                        getClass: getClass
                    };

                    function getClass() {
                        if (scope.cell) {
                            if (scope.cell.result && scope.cell.result.error) {
                                return 'ex-cell-error';
                            }
                            if (scope.cell.selected) {
                                return 'ex-selected-cell';
                            }
                        }

                        return '';
                    }
                    
                    var cachedLinkFunc = spCachingCompile.compile('devViews/expressionSheet/spExpressionCell.tpl.html');
                    cachedLinkFunc(scope, function(clone) {
                        element.append(clone);
                    });
                }
            };
        });
}());