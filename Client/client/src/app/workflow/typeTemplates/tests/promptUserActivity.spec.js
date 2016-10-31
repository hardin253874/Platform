// Copyright 2011-2016 Global Software Innovation Pty Ltd

describe('Workflow|Activities|spec:', function () {
    'use strict';

    describe('promptUserActivity', function () {

        var controller, $scope;

        beforeEach(module('sp.workflow.activities'));

        beforeEach(inject(function ($controller, $rootScope) {
            $scope = $rootScope.$new();
            controller = $controller('displayFormActivityController', { $scope: $scope });
        }));

        it('controller should exist', inject(function () {
            expect(controller).toBeTruthy();
        }));
    });
});
