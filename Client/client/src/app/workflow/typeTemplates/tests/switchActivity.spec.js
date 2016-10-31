// Copyright 2011-2016 Global Software Innovation Pty Ltd
/*global _, angular, describe, it, beforeEach, inject, module, expect, spEntity, jsonString, jsonLookup */

describe('Workflow|Activities|spec:', function () {
    'use strict';

    describe('switchActivity', function () {

        var controller, $scope;

        beforeEach(module('sp.workflow.activities'));

        beforeEach(inject(function ($controller, $rootScope) {
            $scope = $rootScope.$new();

            $scope.workflow = {
                activityTypes: [spEntity.fromJSON({
                    id: 'core:logActivity',
                    inputArguments: []
                })],
                updateCount: 0,
                entity: spEntity.fromJSON({
                    typeId: 'core:workflow',
                    name: jsonString('Untitled'),
                    description: jsonString(''),
                    designerData: '{}',
                    exitPoints: [
                        {
                            typeId: 'core:exitPoint',
                            name: 'end',
                            isDefaultExitPoint: true,
                            exitPointOrdinal: 1
                        }
                    ],
                    firstActivity: jsonLookup(null),
                    containedActivities: [],
                    transitions: [],
                    terminations: [],
                    swimlanes: [],
                    inputArguments: [],
                    outputArguments: [],
                    variables: [],
                    expressionMap: [],
                    expressionParameters: [],
                    inputArgumentForAction: jsonLookup(null),
                    inputArgumentForRelatedResource: jsonLookup(null),
                    runtimeProperties: []
                })
            };
            $scope.entity = spEntity.fromJSON({
                typeId: 'core:logActivity',
                name: jsonString('noname'),
                description: jsonString(''),
                designerData: jsonString(''),
                exitPoints: [],
                inputArguments: [],
                outputArguments: [],
                expressionMap: []
            });

            controller = $controller('switchActivityController', { $scope: $scope });
        }));

        it('controller should exist', inject(function () {
            expect(controller).toBeTruthy();
        }));
    });
});
