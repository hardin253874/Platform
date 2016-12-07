// Copyright 2011-2016 Global Software Innovation Pty Ltd
/* global imageFieldPropertiesTestHelper */

describe('Console|configDialogs|imageFieldProperties|spec:|imageFieldPropertiesController', function () {
    "use strict";

    beforeEach(module('mod.app.configureDialog.imageFieldProperties'));
    beforeEach(module('mod.app.configureDialog.service'));
    var scope, controller;
    var mockConfigDialogService,mockSpEditForm, q, deferred, $httpBackend;

    beforeEach(inject(function ($injector, $rootScope, $controller, $q) {
        this.addMatchers(TestSupport.matchers);
        $httpBackend = $injector.get('$httpBackend');
        scope = $injector.get('$rootScope');
        q = $q;
        //define mock service for the entityservice
        mockConfigDialogService = {
            getSchemaInfoForImageField: function () {
                deferred = q.defer();
                deferred.resolve({
                    fields: imageFieldPropertiesTestHelper.schemaFields,
                    thumbNailSize: imageFieldPropertiesTestHelper.thumbNailSize,
                    thumbNailScaling: imageFieldPropertiesTestHelper.thumbNailScaling,
                    resizeModes: imageFieldPropertiesTestHelper.resizeModes,
                    toType: imageFieldPropertiesTestHelper.toType,
                    templateReport: imageFieldPropertiesTestHelper.templateReport
                });
                return deferred.promise;
            },

            getImageControlEntity: function () {
                deferred = q.defer();
                deferred.resolve(imageFieldPropertiesTestHelper.dummyFormControl);
                return deferred.promise;
            },
            getImageRelationshipEntity: function () {
                deferred = q.defer();
                deferred.resolve(imageFieldPropertiesTestHelper.imageField);
                return deferred.promise;
            }
        };
        mockSpEditForm = {
            getFormControls: function() {
                return [];
            },
            getTemplateReport: function () {
                deferred = q.defer();
                deferred.resolve({ id: 1234, alias: 'templateReport' });
                return deferred.promise;
            }
        };
        spyOn(mockConfigDialogService, 'getImageControlEntity').andCallThrough();
        spyOn(mockConfigDialogService, 'getImageRelationshipEntity').andCallThrough();
        spyOn(mockConfigDialogService, 'getSchemaInfoForImageField').andCallThrough();

    }));

    describe('imageFieldPropertiesController tests for existing field control', function() {

        beforeEach(inject(function($injector, $rootScope, $controller) {

            scope.options = {
                formControl: imageFieldPropertiesTestHelper.dummyFormControl,
                isFormControl: true,
                relationshipType:'image'
            };
            var modalInstance = {
                close: function(result) { return result; }
            };
            scope.modalInstance = modalInstance;
            controller = $controller('imagePropertiesController', {
                $scope: scope,
                configureDialogService: mockConfigDialogService,
                spEditForm: mockSpEditForm
            });

            scope.$root.$digest();
        }));
        
        it('image field property controller is loaded', function () {
            expect(controller).toBeTruthy();
            expect(mockConfigDialogService.getImageControlEntity).toHaveBeenCalled();
            expect(mockConfigDialogService.getSchemaInfoForImageField).toHaveBeenCalled();
        });
        it('should have all the scope variables defined', function () {
            expect(scope.isCollapsed).toBe(true);
            expect(scope.model.isFormControl).toBe(true);
            expect(scope.model.relationshipToRender).toBeTruthy();
            expect(scope.isNewEntity).toBe(false);
            expect(scope.model.isFormDetailEnabled).toBe(true);
            expect(scope.model.thumbNailSize).toBeArray(2);
            expect(scope.model.thumbNailScaling).toBeArray(3);
            expect(scope.fields).toBeArray(2);
        });

        it('should set display name of the field', function () {
            scope.form = { $valid: true };
            expect(scope.options.formControl.getName()).toBe('');
            scope.model.formControl.setName('Test Form Control');
            scope.ok();
            expect(scope.options.formControl.getName()).toBe('Test Form Control');
        });
        it('should set control is mandatory', function () {
            scope.form = { $valid: true };
            expect(scope.options.formControl.mandatoryControl).toBe(false);
            scope.model.formControl.setMandatoryControl(true);
            scope.ok();
            expect(scope.options.formControl.mandatoryControl).toBe(true);
        });
        it('should set control is readOnly', function () {
            scope.form = { $valid: true };
            expect(scope.options.formControl.readOnlyControl).toBe(false);
            scope.model.formControl.setReadOnlyControl(true);
            scope.ok();
            expect(scope.options.formControl.readOnlyControl).toBe(true);
        });
        it('should set controls thumbnailScaling Setting', function () {
            scope.form = { $valid: true };
            expect(scope.options.formControl.thumbnailScalingSetting).toBe(null);
            scope.model.thumbnailScalingSetting=scope.model.thumbNailScaling[0];
            scope.ok();
            expect(scope.options.formControl.thumbnailScalingSetting.name).toBe('Fit proportional');
        });
        it('should set controls thumbnailSize Setting', function () {
            scope.form = { $valid: true };
            expect(scope.options.formControl.thumbnailSizeSetting).toBe(null);
            scope.model.thumbnailSizeSetting=scope.model.thumbNailSize[0];
            scope.ok();
            expect(scope.options.formControl.thumbnailSizeSetting.name).toBe('Small');
        });
    });
    
    describe('imageFieldPropertiesController tests for existing image field', function () {

        beforeEach(inject(function ($injector, $rootScope, $controller) {

            scope.options = {
                formControl: imageFieldPropertiesTestHelper.imageField,
                isFormControl: false,
                relationshipType: 'image'
            };
            var modalInstance = {
                close: function (result) { return result; }
            };
            scope.modalInstance = modalInstance;
            controller = $controller('imagePropertiesController', {
                $scope: scope,
                configureDialogService: mockConfigDialogService,
                spEditForm: mockSpEditForm
            });

            scope.$root.$digest();
        }));

        it('image field property controller is loaded', function () {
            expect(controller).toBeTruthy();
            expect(mockConfigDialogService.getImageRelationshipEntity).toHaveBeenCalled();
            expect(mockConfigDialogService.getSchemaInfoForImageField).toHaveBeenCalled();
        });
        it('should have all the scope variables defined', function () {
            expect(scope.isCollapsed).toBe(true);
            expect(scope.model.isFormControl).toBe(false);
            expect(scope.model.relationshipToRender).toBeTruthy();
            expect(scope.isNewEntity).toBe(false);
            expect(scope.model.thumbNailSize).toBeArray(2);
            expect(scope.model.thumbNailScaling).toBeArray(3);
            expect(scope.fields).toBeArray(2);
        });
        it('should set name of the relationship', function () {
            scope.form = { $valid: true };
            expect(scope.options.formControl.getName()).toBe('Image Field Test');
            scope.model.formData.setField(scope.fields[0], 'Test image Relationship');
            scope.ok();
            expect(scope.options.formControl.getName()).toBe('Test image Relationship');
        });
        it('should set description of the relationship', function () {
            scope.form = { $valid: true };
            expect(scope.options.formControl.getDescription()).toBe('');
            scope.model.formData.setField(scope.fields[1], 'Test Relationship Description');
            scope.ok();
            expect(scope.options.formControl.getDescription()).toBe('Test Relationship Description');
        });
        
        it('should set mandatory on the relationship', function () {
            scope.form = { $valid: true };
            expect(scope.options.formControl.getRelationshipIsMandatory()).toBe(false);
            scope.model.relationshipToRender.setRelationshipIsMandatory(true);
            scope.ok();
            expect(scope.options.formControl.getRelationshipIsMandatory()).toBe(true);
        });
     
    });


});