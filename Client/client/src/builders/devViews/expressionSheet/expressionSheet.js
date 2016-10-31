// Copyright 2011-2015 Global Software Innovation Pty Ltd

(function () {
    'use strict';

    angular.module('app.expressionSheet', ['sp.common.spCalcEngineService',
        'sp.common.ui.spExpressionSheet',
        'sp.common.ui.spExpressionCell',
        'sp.common.ui.spWatchLimiter',
        'mod.common.spEntityService',
        'ui.bootstrap'])
        .value('ui.config', {
            codemirror: {
                mode: 'text/x-spql',
                indentWithTabs: false,
                smartIndent: false,
                lineNumbers: false,
                lineWrapping: false,
                matchBrackets: true,
                autofocus: false
            }
        })
        .config(function ($stateProvider) {
            $stateProvider.state('expressionSheet', {
                url: '/{tenant}/{eid}/expressionSheet?path',
                templateUrl: 'devViews/expressionSheet/expressionSheet.tpl.html'
            });

            window.testNavItems = window.testNavItems || {};
            window.testNavItems.expressionSheet = { name: 'Expression Sheet' };
        })
        .controller('expressionSheetController', function ($scope, $timeout, spCalcEngineService, spEntityService) {
            $scope.myTitle = 'Expression Sheet';
            $scope.title = 'Expressions';
            $scope.data = {
            };
            $scope.busy = false;
            $scope.status = '';
            $scope.results = [];
            $scope.fields = ['name'];
            $scope.context = 'core:userSurvey';
            $scope.contextQuery = 'name, description, isOfType.name';
            $scope.contextFilter = 'true';
            $scope.worksheet = null;

            var onContextChangedDebounce = function () {
                updateResults();
            };
            var onContextChanged = _.debounce(onContextChangedDebounce, 100);
            $scope.$watch('contextFilter', onContextChanged);
            $scope.$watch('contextQuery', onContextChanged);
            $scope.$watch('context', onContextChanged);

            var onCalculationChangedDebounce = function() {
                calculate();
            };
            var onCalculationChanged = _.debounce(onCalculationChangedDebounce, 100);
            $scope.$watch('calculation', onCalculationChanged);

            $scope.getValue = function(obj, name) {
                if (!obj) return '';
                if (!name) return '';
                //return _.get(obj, name.toString());
                return sp.result(obj, name.toString());
            };
            
            var updateResultsTimeout;
            function updateResults() {
                console.log('updateResults');
                if (updateResultsTimeout) {
                    $timeout.cancel(updateResultsTimeout);
                }

                updateResultsTimeout = $timeout(getResults, 500);
            }

            function getFirstLevelRelatedFields(entity) {
                if (!entity) return [];
                return _.flatten(_.map(entity._relationships, function (relationship) {
                    var relationshipName = sp.result(relationship, 'id._alias');
                    var relatedEntity = _.first(relationship.entities);
                    var name = '';
                    var idx = relationship.isLookup ? '' : '.0';
                    if (relatedEntity) {
                        name = _.map(relatedEntity._fields, function (field) {
                            return relationshipName + idx + '.' + sp.result(field, 'id._alias');
                        });
                    }
                    return name;
                }));
            }

            function getResults() {
                console.log('getResults');

                $scope.results.length = 0;

                $scope.busy = true;
                $scope.status = '';

                spEntityService.getEntitiesOfType($scope.context, $scope.contextQuery, { filter: $scope.contextFilter }).then(function(response) {
                    if (response) {
                        $scope.results = response.slice(0, 10);
                        $scope.worksheet = {
                            inputs: $scope.results
                        };

                        var first = _.first($scope.results);
                        if (first) {
                            $scope.fields = _.map(first._fields, function (f) {
                                return sp.result(f, 'id._alias');
                            });

                            var related = getFirstLevelRelatedFields(first);
                            if (related.length) {
                                $scope.fields.push(related);
                            }
                        }
                    }
                }, function (e) {
                    $scope.status = 'An error occurred at the server. (' + (e.message || e) + ')';
                }).catch(function (e) {
                    $scope.status = 'An error occurred. (' + (e.message || e) + ')';
                }).finally(function() {
                    $scope.busy = false;
                });
            }

            $scope.calculation = 'let x = abs(123) select x';
            $scope.calculationResult = 'n/a';

            var calculateTimeout;
            function calculate() {
                console.log('calculate');
                if (calculateTimeout) {
                    $timeout.cancel(calculateTimeout);
                }

                calculateTimeout = $timeout(calculateResult, 500);
            }

            function calculateResult() {
                console.log('calculateResult');

                var params = _.map($scope.results, function (e) {
                    return {
                        name: e.name,
                        type: 'Entity',
                        entityTypeId: $scope.context,
                        value: e.idP
                    };
                });

                spCalcEngineService.evalExpression($scope.calculation, $scope.context, null, params).then(function (result) {
                    $scope.calculationResult = result.data;
                });
            }
        });
}());
