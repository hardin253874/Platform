// Copyright 2011-2016 Global Software Innovation Pty Ltd
/*global angular, console, Showdown, spUtils, spEntity */

/// <reference path="../lib/angular/angular.js" />

(function () {
    'use strict';

    angular.module('app.home', ['ui.router', 'titleService',
            'mod.common.spEntityService', 'spApps.reportServices', 'sp.navService'])
        .config(function ($stateProvider) {

            $stateProvider.state('home', {
                url: '/{tenant}/{eid}/home?path',
                templateUrl: 'home/home.tpl.html',
                data: { showBreadcrumb: true }
            });

            $stateProvider.state('folder', {
                url: '/{tenant}/{eid}/folder?path',
                templateUrl: 'home/home.tpl.html',
                data: { showBreadcrumb: true }
            });
        })
        .controller('HomeController',
        function HomeController($scope, $state, $stateParams, $sce, titleService, spEntityService, spNavService) {

//            console.log('HomeController ctor', $scope.$id);
//            $scope.$on('$destroy', function() { console.log('HomeController destroyed', $scope.$id);});

            function refresh() {
                var entityIdOrAlias = $stateParams.eid;
                if (!entityIdOrAlias) {
                    return;
                }
                entityIdOrAlias = spUtils.coerseToNumberOrLeaveAlone(entityIdOrAlias);
                spEntityService.getEntity(entityIdOrAlias, $scope.entityInfoRequest, { hint: 'home', batch:true }).then(function (entity) {
                    var desc;
                    if (!entity) {
                        return;
                    }
                    $scope.entity = {};
                    $scope.entity.id = entity.id();
                    $scope.entity.name = entity.name;
                    $scope.entity.typeAlias = entity.getType().alias();

                    desc = entity.field('description') || '_no description_';
                    $scope.entity.description = desc;
                    $scope.docInput = desc;
                    renderDoc(desc);

                    titleService.setTitle($scope.entity.name || 'Home');

                    $scope.$broadcast('explore-entity', $scope.entity.id);
                });
            }

            function renderDoc(value) {
                $scope.entity.doc = value;

                //We don't include Showdown as not using anywhere else.
                //$scope.entity.doc = new Showdown.converter().makeHtml(value);
            }

            $scope.entityDoc = function () {
                return $scope.entity.doc;

                //We don't include Showdown as not using anywhere else.
                //return $sce.trustAsHtml($scope.entity.doc);
            };

            $scope.entity = {
                id: 0,
                description: '_nothing to see here... please choose from the menu somewhere there at the top_'
            };
            $scope.showQuery = true;
            $scope.entityInfoRequest = 'alias,name,description,isOfType.{alias,name}';

            renderDoc($scope.entity.description);

            $scope.saveDoc = function () {
                var value = $scope.docInput;
                spEntityService.putEntity(spEntity.fromId($scope.entity.id).setField('description', value, 'String'));
                $scope.entity.description = value;
                renderDoc(value);
            };

            $scope.$watch('showCustomLeftPanel', function (showCustomLeftPanel) {
                if ($state.current.data) {
                    $state.current.data.leftPanelTemplate = showCustomLeftPanel ? 'home/left.tpl.html' : null;
                }
            });


            titleService.setTitle('Home');
            refresh();
        });

}());
