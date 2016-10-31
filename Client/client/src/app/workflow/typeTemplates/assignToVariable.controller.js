// Copyright 2011-2016 Global Software Innovation Pty Ltd
/*global _, angular, console, spWorkflow, spWorkflowConfiguration, sp, spEntity */

(function () {
    'use strict';

    angular.module('sp.workflow.activities')
        .controller('assignToVariableController', function ($scope) {

            function getHelp(entity, relationshipId, aliasOrId) {
                if (!entity || !aliasOrId) {
                    return '';
                }
                var e = spEntity.findByEid(entity.getRelationship(relationshipId), aliasOrId);
                return e && e.getDescription() || 'no description';
            }

            $scope.$watch('entity', function (entity) {

                if (!entity) {
                    return;
                }

                var actType = spEntity.findByEid($scope.workflow.activityTypes, entity.getType());
                var variable = entity.getLookup('core:targetVariable');
                $scope.targetVariable = variable ? variable.id() : null;
                $scope.targetVariableHelp = getHelp(actType, 'relationships', 'core:targetVariable');

                var allAssignable = _.union($scope.workflow.entity.variables, $scope.workflow.entity.outputArguments);
                $scope.workflowVariables = _.map(allAssignable, function (v) {
                    return { id: v.id(), name: v.name, description: v.field('description') };
                });
            });

            $scope.$watch('targetVariable', function (targetVariable) {

                if (_.isString(targetVariable) && targetVariable.length > 0 || _.isNumber(targetVariable)) {
                    $scope.entity.setLookup('core:targetVariable', targetVariable);
                    $scope.$emit('workflow.properties.updated', $scope.entity);
                }
            });
        });
}());
