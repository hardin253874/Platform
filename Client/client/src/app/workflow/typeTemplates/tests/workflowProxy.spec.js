// Copyright 2011-2016 Global Software Innovation Pty Ltd
/*global _, angular, describe, it, beforeEach, inject, module, expect */

describe('Workflow|Activities|spec:', function () {
    'use strict';

    describe('Workflow Proxy activity', function () {

        // Setup
        var controller, $scope;

        beforeEach(module('sp.workflow.activities'));

        beforeEach(inject(function ($controller, $rootScope) {

            $scope = $rootScope.$new();
            controller = $controller('workflowProxyController', { $scope: $scope });
        }));



        // Tests

        it('controller should exist', inject(function () {
            expect(controller).toBeTruthy();
        }));

    });
});
