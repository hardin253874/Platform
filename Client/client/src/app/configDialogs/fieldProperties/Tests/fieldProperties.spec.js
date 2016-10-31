// Copyright 2011-2016 Global Software Innovation Pty Ltd
/* globals fieldPropertiesTestHelper */

describe('Console|configDialogs|fieldProperties|spec:|fieldPropertiesController', function () {
    "use strict";

    beforeEach(module('mod.app.configureDialog.fieldProperties'));
    beforeEach(module('mod.app.configureDialog.service'));

    var scope, controller;
    var mockConfigDialogService, q, deferred, $httpBackend;

    beforeEach(inject(function ($injector, $rootScope, $controller, $q) {
        this.addMatchers(TestSupport.matchers);
        $httpBackend = $injector.get('$httpBackend');
        scope = $injector.get('$rootScope');
        q = $q;
        //define mock service for the entityservice
        mockConfigDialogService = {
            getSchemaInfoForFields: function () {
                deferred = q.defer();
                deferred.resolve({
                    stringPatterns: fieldPropertiesTestHelper.stringPatterns,
                    fields: fieldPropertiesTestHelper.schemaFields,
                    fieldTypes: fieldPropertiesTestHelper.schemaFieldTypes,
                    resizeModes: fieldPropertiesTestHelper.resizeModes
                });
                return deferred.promise;
            },

            getFormControlEntity: function () {
                deferred = q.defer();
                deferred.resolve(fieldPropertiesTestHelper.dummyFormControl);
                return deferred.promise;
            },
            getFieldEntity: function () {
                deferred = q.defer();
                deferred.resolve(fieldPropertiesTestHelper.stringField);
                return deferred.promise;
            },

            isTypeHasInstances: function () {
                deferred = q.defer();
                deferred.resolve(true);
                return deferred.promise;
            }
        };
        spyOn(mockConfigDialogService, 'getFormControlEntity').andCallThrough();
        spyOn(mockConfigDialogService, 'getFieldEntity').andCallThrough();
      
    }));



    describe('fieldPropertiesController tests for existing field control', function () {

        beforeEach(inject(function ($injector, $rootScope, $controller) {

            scope.options = {
                formControl: fieldPropertiesTestHelper.dummyFormControl,
                isFieldControl: true
            };
            var modalInstance = {
                close: function (result) { return result; }
            };
            scope.modalInstance = modalInstance;

            controller = $controller('fieldPropertiesController', {
                $scope: scope,
                configureDialogService: mockConfigDialogService
            });

            scope.$root.$digest();
        }));

        it('field property controller is loaded', function () {
            expect(controller).toBeTruthy();
            expect(mockConfigDialogService.getFormControlEntity).toHaveBeenCalled();
        });

        it('should have all the scope variables defined', function () {
            expect(scope.isCollapsed).toBe(true);
            expect(scope.model.isFieldControl).toBe(true);
            expect(scope.model.fieldToRender).toBeTruthy();
            expect(scope.isNewEntity).toBe(false);
            expect(scope.fieldType).toBe('String');
            expect(scope.formHeader).toBe('Text Field Properties');
            expect(scope.stringPatterns).toBeArray(5);
           
            expect(scope.model.isFormDetailEnabled).toBe(true);
        });
    
        it('should have all the Form controlsdefined', function () {
            expect(scope.model.fieldNameControl).toBeTruthy();
            expect(scope.model.fieldDescriptionControl).toBeTruthy();
            expect(scope.model.fieldMinimumControl).toBeTruthy();
            expect(scope.model.fieldMaximumControl).toBeTruthy();
            expect(scope.model.fieldDefaultControl).toBeTruthy();
            expect(scope.model.fieldDisplayNameControl).toBeTruthy();
        });
        //tests to able to set all the formcontrol properties.

        it('should set display name of the field', function () {
            scope.form = { $valid: true };
            expect(scope.model.formControl.getName()).toBe('');
            scope.model.formData.setField(scope.fields[0].id(), 'Test Form Control');
            scope.ok();
            expect(scope.model.formControl.getName()).toBe('Test Form Control');
        });
        
        it('should set the mandatory property of the form control', function () {
            scope.form = { $valid: true };
            expect(scope.model.mandatoryControl).toBe(false);
            scope.model.mandatoryControl = true;
            scope.ok();
            expect(scope.model.formControl.getMandatoryControl()).toBe(true);
        });
        
        it('should set the readonly property of the form control', function () {
            scope.form = { $valid: true };
            expect(scope.model.readOnlyControl).toBe(false);
            scope.model.readOnlyControl = true;
            scope.ok();
            expect(scope.model.formControl.getReadOnlyControl()).toBe(true);
        });
    });
    describe('fieldPropertiesController tests for existing field', function () {

        beforeEach(inject(function ($injector, $rootScope, $controller) {

            scope.options = {
                formControl: fieldPropertiesTestHelper.stringField,
                isFieldControl: false
            };
            var modalInstance = {
                close: function (result) { return result; }
            };
            scope.modalInstance = modalInstance;

            controller = $controller('fieldPropertiesController', {
                $scope: scope,
                configureDialogService: mockConfigDialogService
            });

            scope.$root.$digest();
        }));

        it('field property controller should load', function () {
            expect(controller).toBeTruthy();
            expect(mockConfigDialogService.getFieldEntity).toHaveBeenCalled();
        });

        it('should have all the scope variables defined', function () {
            expect(scope.isCollapsed).toBe(true);
            expect(scope.model.isFieldControl).toBe(false);
            expect(scope.model.fieldToRender).toBeTruthy();
            expect(scope.isNewEntity).toBe(false);
            expect(scope.fieldType).toBe('String');
            expect(scope.formHeader).toBe('Text Field Properties');
            expect(scope.stringPatterns).toBeArray(5);
        });

        it('should have all the Form controls and fromData defined', function () {
            expect(scope.model.fieldNameControl).toBeTruthy();
            expect(scope.model.fieldDescriptionControl).toBeTruthy();
            expect(scope.model.fieldMinimumControl).toBeTruthy();
            expect(scope.model.fieldMaximumControl).toBeTruthy();
            expect(scope.model.fieldDefaultControl).toBeTruthy();
            expect(scope.model.fieldDefaultData).toBeTruthy();
        });

        it('should set the name of the field', function() {
            scope.form = { $valid: true };
            expect(scope.model.fieldToRender.getName()).toBe('nameField');
            scope.model.fieldData.setField(scope.fields[0].id(), 'Test Field');
            scope.ok();
            expect(scope.model.fieldToRender.getName()).toBe('Test Field');
        });
        
        it('should set the description of the field', function () {
            scope.form = { $valid: true };
            expect(scope.model.fieldToRender.getDescription()).toBe('');
            scope.model.fieldData.setField(scope.fields[1].id(), 'Test Field Description');
            scope.ok();
            expect(scope.model.fieldToRender.getDescription()).toBe('Test Field Description');
        });
        
        it('should set the isRequired property of the field', function () {
            scope.form = { $valid: true };
            expect(scope.model.isFieldRequired).toBe(false);
            scope.model.isFieldRequired = true;
            scope.ok();
            expect(scope.model.fieldToRender.getIsRequired()).toBe(true);
        });
        
        it('should set the minimum value of the field', function () {
            scope.form = { $valid: true };
            expect(scope.model.fieldToRender.getMinLength()).toBe(null);
            scope.model.fieldData.setField(scope.model.minField.id(), 10);
            scope.ok();
            expect(scope.model.fieldToRender.getMinLength()).toBe(10);
        });
        
        it('should set the maximum value of the field', function () {
            scope.form = { $valid: true };
            expect(scope.model.fieldToRender.getMaxLength()).toBe(null);
            scope.model.fieldData.setField(scope.model.maxField.id(), 100);
            scope.ok();
            expect(scope.model.fieldToRender.getMaxLength()).toBe(100);
        });
        
        //it('should get validation message if minimun value set to greater then maximum value', function () {
        //    scope.form = { $valid: true };
        //    scope.model.fieldData.setField(scope.model.minField.id(), 30, 'core:intField');
        //    scope.model.fieldData.setField(scope.model.maxField.id(), 25, 'core:intField');
        //    scope.ok();
        //    expect(scope.model.errors.length).toBeGreaterThan(0);
        //    expect(scope.model.errors[0].msg).toBe('Minimum value cannot be greater then maximum value');
        //});
    });
});
