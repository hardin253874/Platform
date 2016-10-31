// Copyright 2011-2016 Global Software Innovation Pty Ltd
/*global _, console, angular, sp */

(function () {
    'use strict';

    angular.module('sp.workflow.entityExplorer', ['mod.common.spEntityService'])
        .controller('workflowEntityExplorerController', function ($scope) {

            // The "entity" is the spEntity that the view renders.
            // The "entityStack" allows for some interactivity.
            $scope.entity = null;
            $scope.entityStack = [];

            function setRootEntity(entity) {
                $scope.entity = entity;
                $scope.entityStack = [];
            }

            // The entity explored is the inherited "debugEntity" scope property. 
            // Todo - change this to either use a shared service (client side) 
            //  or maybe have it sent in $broadcast event
            //  or maybe all of the above

            $scope.$watch('debugEntity', function (debugEntity) {
                setRootEntity(debugEntity);
            });

            // scope methods for the view

            $scope.setRelationship = function (r) {
//                console.log('setRelationship', r);
                $scope.relationship = r;
            };

            $scope.setEntity = function (e) {
//                console.log('setEntity', e);
                $scope.entityStack.push($scope.entity);
                $scope.entity = e;
            };

            $scope.removeRelated = function (e) {
                $scope.relationship.remove(e);
            };

            $scope.popEntity = function () {
                $scope.entity = $scope.entityStack.pop();
            };

            $scope.withInstances = function (r) {
                return r && r.instances && r.instances.length;
            };

            $scope.relationships = function () {
                return $scope.relationship && $scope.relationship.id && $scope.relationship.id._alias &&
                    $scope.entity.getRelationship($scope.relationship.id._alias) || [];
            };
        });
}());
