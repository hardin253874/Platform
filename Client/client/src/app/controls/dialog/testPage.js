// Copyright 2011-2016 Global Software Innovation Pty Ltd
(function () {
    'use strict';
    angular.module('app.testPage', ['ui.router', 'mod.common.ui.spDialogService', 'mod.common.spEntityService', 'spApps.reportServices'])

         .constant('TestPageState', {
             name: 'test page',
             url: '/controls/dialog',
             views: {
                 'content@': {
                     controller: 'testPageController',
                     templateUrl: 'controls/dialog/testPage.tpl.html'
                 }

             }
         })
        .controller('testPageController', function ($scope, $stateParams, spDialogService, spEntityService, spReportService) {

            function refresh() {
                if (!$scope.entityType) {
                    return;
                }
                $scope.entityPromise = spEntityService.getEntity($scope.entityType, 'name,description');
            }
            $scope.openDialog = function (item) {
                var entityType = $scope.entity;
                var currentItem = item;
                spDialogService.showDialog({
                    controller: 'createEntityController',
                    templateUrl: 'controls/dialog/entityNameDescriptionControl.tpl.html',
                    resolve: {
                        entityType: function() { return angular.copy(entityType); },
                        entity: function() {
                            if (item) {
                                
                                console.log(currentItem[0].value);
                                console.log(currentItem[2].value);
                                console.log(currentItem.id);
                                var entity = new spEntity.fromId(Number(currentItem[0].value));
                                entity.setName(currentItem[1].value);
                                entity.registerField('description', spEntity.DataType.String);
                                entity.setDescription(currentItem[2].value || '');
                               
                                return entity;
                            } else {
                                return null;
                            }
                        },
                        beforeSave: function () {
                            return beforeSave;
                        }
            }
                   
                }).then(function (result) {
                    if (result)
                        console.log('entity saved with id' + result);

                }, function (error) {


                });
            };

            //Only for test before save function it is added
            function beforeSave(entity) {
                if (entity) {
                    entity.setField('core:firstName', entity.getField('name'), spEntity.DataType.String);
                    entity.setField('core:lastName', entity.getField('description')||"Last Name", spEntity.DataType.String);
                }
                return entity;
            }
            
            $scope.go = function () {
                console.log($scope.entityType);
                //$scope.entityPromise = spEntityService.getEntity($scope.entityType, 'name,description');
                refresh();
            };
            
            $scope.entityType = 'core:person';
            $scope.entity = {};
            
            $scope.$watch('entityPromise', function (entity) {
                if (!entity) {
                    return;
                }
                    console.log(entity.name);
                    console.log(entity.id());
               
                var selects, query;
                selects = [
                    { field: 'name', displayAs: 'Name' },
                    { field: 'description', displayAs: 'Description' }
                ];
                
                $scope.entity = entity;
               // $scope.entity.id = entity.id();
               // $scope.entity.name = entity.name;
                query = {
                    root: { id: $scope.entity.id() },
                    selects: selects,
                    conds: []
                };
                $scope.resultsPromise = spReportService.runQuery(query);
            });

            $scope.$watch('resultsPromise', function (instances) {
                console.log(instances.data[0].id);
            });
        });

}());