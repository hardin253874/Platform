// Copyright 2011-2016 Global Software Innovation Pty Ltd
/*global $, _, console, angular, sp, spUtils, spEntity */

(function () {
    'use strict';

    angular.module('mod.common.ui.spDataGrid')
        .directive('spDataGridRowColScope', spDataGridRowColScopeDirective);

    /* @ngInject */
    function spDataGridRowColScopeDirective() {
        return {
            scope: true,
            priority: 10000,
            compile: function() {
                return {
                    pre: function(scope, iElement, iAttrs) {
                        scope.col = scope.renderedColumns[parseInt(iAttrs.spDataGridRowColScope)];
                    }
                };
            }
        };
    }

}());