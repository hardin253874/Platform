// Copyright 2011-2016 Global Software Innovation Pty Ltd
(function () {
    'use strict';

    angular.module('app.entityRequestTest', ['mod.common.spEntityService'])
        .controller('entityRequestTestController', ['$scope', 'spEntityService', function ($scope, spEntityService) {
            $scope.resource = 'test:peterAylett';
            $scope.request = 'name,core:firstName,core:lastName,s:age,s:manager.{name}';

            $scope.runRequest = function () {
                $scope.url = spEntityService._getEntityRequestUrl($scope.resource, { version: 2 }) + '?request=' + encodeURIComponent($scope.request);

                spEntityService._getEntityData($scope.resource, $scope.request, { version: 2 }).then(function(res) {
                    $scope.rawResponse = JSON.stringify(res, null, 4);
                    var entities = spEntity.entityDataVer2ToEntities(res);
                    var json = spEntity.toJSON(entities[0]);
                    $scope.mockJSON = JSON.stringify(json, null, 4);
                }, function () {
                    $scope.rawResponse = 'Failed';
                    $scope.mockJSON = 'Failed';
                });
            };
        }]);

}());