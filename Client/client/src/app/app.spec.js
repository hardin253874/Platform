// Copyright 2011-2016 Global Software Innovation Pty Ltd
/*global _, angular, console, module, describe, beforeEach, it, inject, expect, runs, waitsFor, TestSupport */

describe('Console|AppController|spec:', function () {
    "use strict";

    var AppController, $location, $scope, $httpBackend;

    beforeEach(module('app', 'mockedEntityInfo'));

    beforeEach(inject(function ($controller, _$location_, $rootScope, _$httpBackend_, $q, entityInfoResponses) {
        $location = _$location_;
        $httpBackend = _$httpBackend_;
        entityInfoResponses.forEach(function (item) {
            console.log('training $httpBackend with ', item.request.toString(), item.response);
            $httpBackend.whenGET(item.request).respond(item.response);
        });
        $scope = $rootScope.$new();
        AppController = $controller('AppController', { $location: $location, $scope: $scope, $q: $q });
    }));

    it('login should have default settings', inject(function () {
        expect(AppController).toBeTruthy();
        expect($scope.appData).toBeTruthy();
    }));
});
