// Copyright 2011-2016 Global Software Innovation Pty Ltd
/* globals containerPropertiesTestHelper, choiceFieldPropertiesTestHelper */

describe('Console|configDialogs|containerProperties|spec:|containerPropertiesController', function () {
    "use strict";
    beforeEach(module('mod.app.configureDialog.containerProperties'));
    beforeEach(module('mod.app.configureDialog.service'));
    var scope, controller;
    var mockConfigDialogService, q, deferred, $httpBackend;

    beforeEach(inject(function ($injector, $rootScope, $controller, $q) {
        this.addMatchers(TestSupport.matchers);
        $httpBackend = $injector.get('$httpBackend');
        scope = $injector.get('$rootScope');
        q = $q;
        //define mock service for the configDialogService
        mockConfigDialogService = {
            getSchemaInfoForContainer: function () {
                deferred = q.defer();
                deferred.resolve({
                    field: containerPropertiesTestHelper.schemaFields,
                    resizeModes: containerPropertiesTestHelper.resizeModes
                });
                return deferred.promise;
            },

            getContainerControlEntity: function () {
                deferred = q.defer();
                deferred.resolve(containerPropertiesTestHelper.containerControl);
                return deferred.promise;
            },
            getDummyFieldControlOnForm: choiceFieldPropertiesTestHelper.getDummyFieldControlOnForm
        };
        
        spyOn(mockConfigDialogService, 'getContainerControlEntity').andCallThrough();
        spyOn(mockConfigDialogService, 'getSchemaInfoForContainer').andCallThrough();

    }));
    
    describe('containerController tests for existing form control', function () {

        beforeEach(inject(function ($injector, $rootScope, $controller) {

            scope.options = {
                formControl: containerPropertiesTestHelper.containerControl,
                isFormControl: true,
                relationshipType: 'container'
            };
            var modalInstance = {
                close: function (result) { return result; }
            };
            scope.modalInstance = modalInstance;

            controller = $controller('containerPropertiesController', {
                $scope: scope,
                configureDialogService: mockConfigDialogService
            });

            scope.$root.$digest();
        }));

        it('container property controller is loaded', function () {
            expect(controller).toBeTruthy();
            expect(mockConfigDialogService.getContainerControlEntity).toHaveBeenCalled();
            expect(mockConfigDialogService.getSchemaInfoForContainer).toHaveBeenCalled();
        });

        it('should have all the scope variables defined', function () {
            expect(scope.model.isFormControl).toBe(true);
            expect(scope.isNewEntity).toBe(false);
            expect(scope.field).toBeTruthy();
        });

        it('should set name of the container control', function () {
            scope.form = { $valid: true };
            expect(scope.options.formControl.getName()).toBe('container1');
            scope.model.formData.setField(scope.field, 'Test Form Control');
            scope.ok();
            expect(scope.options.formControl.getName()).toBe('Test Form Control');
        });

        it('should rollback the changes on clicking cancel', function () {
            scope.form = { $valid: true };
            expect(scope.options.formControl.getName()).toBe('Test Form Control');
            scope.model.formData.setField(scope.field, 'cancel test');
            scope.cancel();
            expect(scope.options.formControl.getName()).toBe('Test Form Control');
        });

    });


});