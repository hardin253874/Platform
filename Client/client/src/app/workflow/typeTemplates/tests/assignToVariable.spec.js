// Copyright 2011-2016 Global Software Innovation Pty Ltd
/*global _, angular, describe, it, beforeEach, inject, module, expect */

describe('Workflow|Activities|spec:', function () {
    'use strict';

    describe('Assign To Variable activity', function () {

        var controller, $scope;

        beforeEach(module('sp.workflow.activities'));

        beforeEach(inject(function ($controller, $rootScope) {
            $scope = $rootScope.$new();
            controller = $controller('assignToVariableController', { $scope: $scope });
        }));

        it('controller should exist', inject(function () {
            expect(controller).toBeTruthy();
        }));
    });
});