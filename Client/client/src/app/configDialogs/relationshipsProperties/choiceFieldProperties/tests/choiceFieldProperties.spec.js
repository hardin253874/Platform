// Copyright 2011-2016 Global Software Innovation Pty Ltd
/* globals choiceFieldPropertiesTestHelper */

describe('Console|configDialogs|choiceFieldProperties|spec:|choiceFieldPropertiesController', function () {
    "use strict";

    beforeEach(module('mod.app.configureDialog.choiceFieldProperties'));
    beforeEach(module('mod.app.configureDialog.service'));
    var scope, controller;
    var mockConfigDialogService,mockSpEntityService, q, deferred, $httpBackend;

    beforeEach(inject(function ($injector, $rootScope, $controller, $q) {
        this.addMatchers(TestSupport.matchers);
        $httpBackend = $injector.get('$httpBackend');
        scope = $injector.get('$rootScope');
        q = $q;
        //define mock service for the configDialogService
        mockConfigDialogService = {
            getSchemaInfoForChoiceField: function () {
                deferred = q.defer();
                deferred.resolve({
                    fields: choiceFieldPropertiesTestHelper.schemaFields,
                    enumTypeReport: choiceFieldPropertiesTestHelper.enumTypeReport,
                    enumType: choiceFieldPropertiesTestHelper.enumType,
                    resizeModes: choiceFieldPropertiesTestHelper.resizeModes
                });
                return deferred.promise;
            },

            getChoiceFieldControlEntity: function () {
                deferred = q.defer();
                deferred.resolve(choiceFieldPropertiesTestHelper.dummyFormControl);
                return deferred.promise;
            },
            getChoiceRelationshipEntity: function () {
                deferred = q.defer();
                deferred.resolve(choiceFieldPropertiesTestHelper.choiceFieldControl);
                return deferred.promise;
            },
            getChoiceValuesOfType:function() {
                deferred = q.defer();
                deferred.resolve(choiceFieldPropertiesTestHelper.getChoiceValuesOfType);
                return deferred.promise;
            },
            getDummyFieldControlOnForm: choiceFieldPropertiesTestHelper.getDummyFieldControlOnForm
        };
        
        //define mock service for the spEntityservice
        mockSpEntityService = {
            putEntity: function (entity) {
                deferred = q.defer();
                deferred.resolve(choiceFieldPropertiesTestHelper.putEntity(entity));
                return deferred.promise;
            }
        };
        spyOn(mockConfigDialogService, 'getChoiceFieldControlEntity').andCallThrough();
        spyOn(mockConfigDialogService, 'getChoiceRelationshipEntity').andCallThrough();
        spyOn(mockConfigDialogService, 'getSchemaInfoForChoiceField').andCallThrough();

    }));

    describe('choicePropertiesController tests for existing form control', function () {

        beforeEach(inject(function($injector, $rootScope, $controller) {

            scope.options = {
                formControl: choiceFieldPropertiesTestHelper.choiceFieldControl,
                isFormControl: true,
                relationshipType: 'choice'
            };
            var modalInstance = {
                close: function(result) { return result; }
            };
            scope.modalInstance = modalInstance;

            controller = $controller('choicePropertiesController', {
                $scope: scope,
                configureDialogService: mockConfigDialogService,
                spEntityService: mockSpEntityService
            });

            scope.$root.$digest();
        }));
        
        it('choice field property controller is loaded', function () {
            expect(controller).toBeTruthy();
            expect(mockConfigDialogService.getChoiceFieldControlEntity).toHaveBeenCalled();
            expect(mockConfigDialogService.getSchemaInfoForChoiceField).toHaveBeenCalled();
        });
        
        it('should have all the scope variables defined', function () {
            expect(scope.isCollapsed).toBe(true);
            expect(scope.model.isFormControl).toBe(true);
            expect(scope.model.relationshipToRender).toBeTruthy();
            expect(scope.isNewEntity).toBe(false);
            expect(scope.model.isFormDetailEnabled).toBe(true);
            expect(scope.fields).toBeArray(3);
            expect(scope.model.choiceValues).toBeArray(7);
        });

        it('should set display name of the form control', function () {
            scope.form = { $valid: true };
            expect(scope.options.formControl.getName()).toBe('');
            scope.model.formControlData.setField(scope.fields[0],'Test Form Control');
            scope.ok();
            expect(scope.options.formControl.getName()).toBe('Test Form Control');
        });
        
        //it('should rollback the changes on clicking cancel', function () {
        //    scope.form = { $valid: true };
        //    expect(scope.options.formControl.mandatoryControl).toBe(false);
        //    scope.model.formControl.setMandatoryControl(true);
        //    scope.cancel();
        //    expect(scope.options.formControl.mandatoryControl).toBe(false);
        //});

    });

    describe('choicePropertiesController tests for existing choice field', function () {

        beforeEach(inject(function ($injector, $rootScope, $controller) {

            scope.options = {
                formControl: choiceFieldPropertiesTestHelper.choiceField,
                isFormControl: false,
                relationshipType: 'choice'
            };
            var modalInstance = {
                close: function (result) { return result; }
            };
            scope.modalInstance = modalInstance;

            controller = $controller('choicePropertiesController', {
                $scope: scope,
                configureDialogService: mockConfigDialogService,
                spEntityService: mockSpEntityService
            });

            scope.$root.$digest();
        }));

        it('choice field property controller is loaded', function () {
            expect(controller).toBeTruthy();
            expect(mockConfigDialogService.getChoiceRelationshipEntity).toHaveBeenCalled();
            expect(mockConfigDialogService.getSchemaInfoForChoiceField).toHaveBeenCalled();
        });
        
        it('should have all the scope variables defined', function () {
            expect(scope.isCollapsed).toBe(true);
            expect(scope.model.isFormControl).toBe(false);
            expect(scope.model.relationshipToRender).toBeTruthy();
            expect(scope.isNewEntity).toBe(false);
            expect(scope.fields).toBeArray(3);
            expect(scope.model.choiceValues).toBeArray(7);
        });
        
        it('should set name of the relationship', function () {
            scope.form = { $valid: true };
            expect(scope.options.formControl.getName()).toBe('Choice Field Test');
            scope.model.formData.setField(scope.fields[0], 'Test Choice Relationship');
            scope.ok();
            expect(scope.options.formControl.getName()).toBe('Test Choice Relationship');
        });
        
        it('should set description of the relationship', function () {
            scope.form = { $valid: true };
            scope.model.formData.setField(scope.fields[1], 'Test Relationship Description');
            scope.ok();
            expect(scope.options.formControl.getDescription()).toBe('Test Relationship Description');
        });
        
        it('can add new choice value at the end of the array', function () {
            scope.addChoiceFieldValue();
            expect(scope.model.choiceValues).toBeArray(8);
            expect(scope.model.choiceValues[7].name).toBe('New Value');
        });
        
        it('can able to move down the choice value in a array', function () {
            scope.selectedRowIndex = 0;
            expect(scope.model.choiceValues[0].name).toBe('Sunday');
            scope.moveDownChoiceFieldValue();
            expect(scope.model.choiceValues[1].name).toBe('Sunday');
        });

        it('can able to move up the choice value in a array', function () {
            scope.selectedRowIndex = 1;
            expect(scope.model.choiceValues[1].name).toBe('Monday');
            scope.moveUpChoiceFieldValue();
            expect(scope.model.choiceValues[0].name).toBe('Monday');
        });
    });


});