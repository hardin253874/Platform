// Copyright 2011-2016 Global Software Innovation Pty Ltd
/*global _, angular, console, spEntity, sp */

/// <reference path="../lib/angular/angular.js" />

(function () {
    'use strict';

    angular.module('app.demo', ['ui.router', 'titleService', 'mod.common.spEntityService', 'spApps.reportServices', 'sp.common.charts.spD3SimpleBar'])
        .config(function ($stateProvider) {
            $stateProvider.state('demo', {
                url: '/{tenant}/{eid}/demo?path',
                templateUrl: 'devViews/demo/demo.tpl.html'
            });

            // register a nav item for this page (most fields are defaulted, but you can provide your own viewType, iconUrl, href and order)
            window.testNavItems = window.testNavItems || {};
            window.testNavItems.demo = { name: 'Demo page' };
        })
        .controller('DemoController', function DemoController($scope, $stateParams, $timeout, titleService, spEntityService, spReportService) {

            function refresh() {
                var entityTypeIdOrAlias = $scope.entityType;
                if (!entityTypeIdOrAlias) {
                    return;
                }
                entityTypeIdOrAlias = sp.coerseToNumberOrLeaveAlone(entityTypeIdOrAlias);
                spEntityService.getEntity(entityTypeIdOrAlias, $scope.entityInfoRequest).then(function (entity) {
                    if (!entity) {
                        return;
                    }

                    $scope.entityJson = spEntity.toJSON(entity);

                    $scope.entity = {};
                    $scope.entity.id = entity.id();
                    $scope.entity.name = entity.name;
                    $scope.entity.description = entity.field('description');
                    $scope.entity.fields = entity.relationship('fields').map(function (e) {
                        return {id: e.id(), name: e.name};
                    });
                    $scope.entity.fields.unshift({id: 'name', name: 'Name', selected: true });
                });
            }

            console.log('routeParams=' + JSON.stringify($stateParams));

            $scope.open = function () {
                $timeout(function () {
                    $scope.opened = true;
                });
            };
            $scope.myDate = new Date();
            $scope.dateOptions = {
                'year-format': "'yy'",
                'starting-day': 1
            };

            titleService.setTitle('Report');

            $scope.d3Data = [];

            $scope.entityType = 'test:person';
            $scope.entity = {};
            $scope.showQuery = true;
            $scope.entityInfoRequest = 'name,description,fields.name';

            $scope.navRequest = '';

            $scope.dragOptions = {
                onDragStart: function (event, data) {
                    console.log('demo: onDragStart');
                },
                onDragEnd: function (event, data) {
                    console.log('demo: onDragEnd');
                }
            };

            $scope.dropOptions = {
                simpleEventsOnly: false,
                propagateDragEnter: false,
                propagateDragLeave: false,
                propagateDrop: false,
                propagateDragOver: false,

                onAllowDrop: function (source, target, dragData, dropData) {
                    //console.log('demo: onAllowDrop');
                    return true;
                },
                onDrop: function (event, source, target, dragData, dropData) {
                    console.log('demo: onDrop');
                    dragData.selected = !dragData.selected;
                },
                onDragOver: function (event, source, target, dragData, dropData) {
                    console.log('demo: onDragOver');
                },
                onDragEnter: function (event, source, target, dragData, dropData) {
                        console.log('demo: onDragEnter');
                },
                onDragLeave: function (event, source, target, dragData, dropData) {
                    console.log('demo: onDragLeave');
                }
            };

            $scope.go = function () {
                console.log($scope.entityType);
                refresh();
            };

            $scope.$watch('entityType', function (entityType) {
                refresh();
            });

            $scope.$watch('entity.fields', function (fields) {
                var selects, query;

                if (!fields) {
                    return;
                }

                selects = fields
                    .filter(function (f) {
                        return f.selected;
                    })
                    .map(function (f) {
                        return { field: f.id, displayAs: f.name };
                    });
                query = {
                    root: { id: $scope.entity.id },
                    selects: selects,
                    conds: []
                };

                spReportService.runQuery(query).then(function (results) {
                    if (results) {
                        $scope.results = results;

                        $scope.d3Data = results.data.map(function (f) {
                            if (f.item && f.item.length > 1) {
                                return f.item[1].value;
                            }
                            return '';
                        });
                    }
                });
            }, true);
        });

}());
