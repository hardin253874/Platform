// Copyright 2011-2016 Global Software Innovation Pty Ltd
(function () {
    'use strict';

    angular.module('app.resourceInfoTest', ['mod.common.spEntityService', 'mod.common.spResource'])
        .controller('resourceInfoTestController', ['$scope', 'spEntityService', 'spResource', function ($scope, spEntityService, spResource) {
            $scope.entityType = 'test:employee';
            $scope.ancestors = [];
            $scope.derivedTypes = [];
            $scope.entityPromise1 = {};
            $scope.entityPromise2 = {};
            $scope.message = 'Loading...';


            $scope.$watch('entityType', function (entityType) {
                $scope.message = 'Loading';

                var rq1 = spEntityService.makeGetEntityRequest(entityType, 'name, inherits.inherits*.name, derivedTypes.derivedTypes*.name');
                var rq2 = spEntityService.makeGetEntityRequest(entityType, spResource.makeTypeRequest());

                $scope.entityPromise1 = rq1.promise
                    .then(function (typeEntity) {
                        if (!typeEntity) {
                            $scope.ancestors = [];
                            $scope.derivedTypes = [];
                            return;
                        }
                        $scope.ancestors = spResource.getAncestorsAndSelf(typeEntity);
                        $scope.derivedTypes = spResource.getDerivedTypesAndSelf(typeEntity);
                    });

                $scope.entityPromise2 = rq2.promise
                    .then(function(typeEntity) {
                        $scope.message = '';
                        var type = new spResource.Type(typeEntity);

                        $scope.name = type.getName();
                        $scope.description = type.getDescription();
                        $scope.fields = type.getFields();
                        $scope.lookups = type.getLookups();
                        $scope.relationships = type.getRelationships();
                        $scope.choiceFields = type.getChoiceFields();
                        $scope.fieldGroups = type.getFieldGroups();
                    },
                        function () {
                            $scope.name = '';
                            $scope.description = '';
                            $scope.fields = [];
                            $scope.lookups = [];
                            $scope.relationships = [];
                            $scope.choiceFields = [];
                            $scope.fieldGroups = [];
                            $scope.message = 'Can\'t load definition.';
                        }
                    );

                var batch = new spEntityService.BatchRequest();
                batch.addRequest(rq1);
                batch.addRequest(rq2);
                batch.runBatch();
            });

        }]);
}());