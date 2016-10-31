// Copyright 2011-2016 Global Software Innovation Pty Ltd
(function () {
    'use strict';

    angular.module('mod.common.ui.spValueEditor2', [])
        .directive('spValueEditor2', function($compile) {
            return {
                restrict: 'E',
                templateUrl: 'valueEditor/spValueEditor2.tpl.html',
                replace: true,
                transclude: false,
                scope: {
                    type: '=',
                    options: '=',
                    value: '='
                },
                link: function($scope, $element) {
                    var element = $element;

                    /////
                    // Get the directive.
                    /////

                    function getDirective(type) {
                        switch (type) {
                        case spEntity.DataType.Int32:
                            return 'spNumberControl';
                        case spEntity.DataType.Decimal:
                            return 'spDecimalControl';
                        case spEntity.DataType.Currency:
                            return 'spCurrencyControl';
                        case spEntity.DataType.Bool:
                            return 'spCheckboxControl';
                        case spEntity.DataType.Date:
                            return 'spDateControl';
                        case spEntity.DataType.Time:
                        case spEntity.DataType.DateTime:
                        case spEntity.DataType.String:
                        case spEntity.DataType.Guid:
                        case spEntity.DataType.None:
                            return null;
                        }
                    }

                    /////
                    // Convert an alias into its equivalent directive name.
                    /////

                    function getDirectiveName(str) {

                        return str.replace(/(.*?)([A-Z])/g, '$1-$2').toLowerCase();
                    }

                    $scope.$watch('type', function(type) {

                        var template;
                        var directive;
                        var directiveName;
                        var newElement;

                        directive = getDirective(type);

                        if (directive) {
                            directiveName = getDirectiveName(directive);

                            $scope.valueEditorModel = {
                                value: $scope.value
                            };

                            template = '<' + directiveName + ' model="valueEditorModel"></' + directiveName + '>';

                            newElement = $compile(template)($scope);

                            /////
                            // May want to use 'replaceWith' rather than 'append' here.
                            /////
                            element.replaceWith(newElement);

                            element = newElement;
                        } else {
                            element.css('visibility', 'hidden');
                        }
                    });

                    $scope.$watch('value', function(newValue, oldValue) {

                        if (newValue === oldValue)
                            return;

                        $scope.valueEditorModel.value = newValue;
                    });

                    $scope.$watch('valueEditorModel.value', function (newValue, oldValue) {

                        if (newValue === oldValue)
                            return;

                        $scope.value = newValue;
                    });
                }
            };
        });
}());