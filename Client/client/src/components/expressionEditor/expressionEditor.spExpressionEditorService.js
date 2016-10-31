// Copyright 2011-2016 Global Software Innovation Pty Ltd
/*global _, angular, console, CodeMirror, sp, spResource, spEntity */

(function () {
    'use strict';

    angular
        .module('sp.common.ui.expressionEditor')
        .factory('spExpressionEditorService', spExpressionEditorService);

    function spExpressionEditorService(spExpressionFunctions) {

        var functionList = _.flatten(_.map(spExpressionFunctions.categories, function (c) {
            return [
                { name: c.name, cssClass: 'category' }
            ].concat(_.map(c.functions, function (f, index) {
                return index % 2 === 0 ? f : _.extend(f, {cssClass: 'alt-row'});
            }));
        }));

        return {
            functionList: functionList
        };
    }

})();
