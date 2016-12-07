// Copyright 2011-2016 Global Software Innovation Pty Ltd
(function () {
    'use strict';

    angular.module('mod.common.ui.spReportBuilder.spCustomJoinExpression', [])
        .directive('spCustomJoinExpression', CustomJoinExpression);

    function CustomJoinExpression() {
        return {
            restrict: 'E',
            scope: {
                script: '=',
                options: '='   // { childTypeId, parentTypeId:   //expectedResultType, disabled, choosers.{etc..} }
            },
            template: '<sp-expression-editor ng-model="model.script" context="model.context" host="model.host" options="model.options" params="model.params"></sp-expression-editor>',
            link: CustomJoinExpressionLink
        };
    }

    function CustomJoinExpressionLink(scope, options, elm, attrs) {

        // options : { parentTypeId, childTypeId }

        var model = {
            context: options.childTypeId,
            host: 'Report',
            script: '',
            options: {
                dataType: spEntity.DataType.Bool
            },
            params: [
                {
                    name: "parent",
                    typeName: "Entity",
                    entityTypeId: options.parentTypeId,
                    isList: true
                }
            ]
        };
        scope.model = model;

        scope.$watch('options.parentTypeId', function (newValue) {
            model.params[0].entityTypeId = '' + newValue;
        });
        scope.$watch('options.childTypeId', function (newValue) {
            model.context = '' + newValue;
            if (newValue) {
                model.script = model.initScript;
                model.active = true;
            }
        });
        scope.$watch('script', function (newValue) {
            model.initScript = newValue;
            if (model.context && model.context !== "0") {
                model.script = model.initScript;
                model.active = true;
            }
        });
        scope.$watch('model.script', function () {
            if (model.active) {
                scope.script = model.script;
            }
        });
    }

}());