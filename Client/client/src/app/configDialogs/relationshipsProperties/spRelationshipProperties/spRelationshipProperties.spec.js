// Copyright 2011-2016 Global Software Innovation Pty Ltd
/* global fieldPropertiesTestHelper */

describe('Console|configDialogs|relationshipsProperties|spec:|spRelationshipPropertiesController', function () {
    "use strict";

    beforeEach(module('mod.app.configureDialog.relationshipProperties.spRelationshipProperties'));
    beforeEach(module('mod.app.configureDialog.service'));
    beforeEach(module('mockedEntityService'));
    
    var scope, controller;
    var mockConfigDialogService, mockSpFormBuilderService, q, deferred, $httpBackend;
    var mockControlConfigureDialogFactory = { };

    var _employee, _department, _address, _lookup, _lookupControl, _relationship, _relationshipControl;
    var _schemaFields, _schemaEntities;
    
    /// Set the mocked services data ///
    (function () {
        // **** Entities ******** // 
        beforeEach(inject(function (spEntityService) {
           
            spEntityService.mockTemplateReport();
            
            // employee 
            _employee = spEntity.fromJSON({
                id: { id: 1000, ns: 'test', alias: 'employee' },
                name: 'Employee',
                isAbstract: false,
                defaultPickerReport: jsonLookup(),
                derivedTypes: [
                    {
                        id: { id: 1001, ns: 'test', alias: 'manager' },
                        name: "Manager",
                        isAbstract: false,
                        derivedTypes: [
                            {
                                id: { id: 1002, ns: 'test', alias: 'salesManager' },
                                name: "Sales Manager"
                            }
                        ]
                    },
                    {
                        id: { id: 999, ns: 'test', alias: 'driver' },
                        name: "AA_Driver"
                    }
                ]
            });

            // department 
            _department = spEntity.fromJSON({
                id: { id: 2000, ns: 'test', alias: 'department' },
                name: 'Department',
                isAbstract: false,
                defaultPickerReport: {
                    name: 'department_Report_1',
                    typeId: 123
                }
            });

            // address 
            _address = spEntity.fromJSON({
                id: { id: 3000, ns: 'test', alias: 'address' },
                name: 'Address',
                isAbstract: false,
                defaultPickerReport: {
                    name: 'address_Report_1',
                    typeId: 123
                }
            });
            
            
            
            _lookup = spEntity.fromJSON({
                name: jsonString(''),
                description: jsonString(''),
                toName: jsonString(''),
                fromName: jsonString(''),
                toType: jsonLookup(), //_department, leave toType null
                fromType: _employee,
                toTypeDefaultValue: jsonLookup(),
                fromTypeDefaultValue: jsonLookup(),
                relationshipIsMandatory: false,
                revRelationshipIsMandatory: false,
                isRelationshipReadOnly: false,
                cardinality: jsonLookup('core:manyToOne'),
                relType: jsonLookup('core:relLookup'),
                cascadeDelete: false,
                cascadeDeleteTo: false,
                cloneAction: { alias: 'cloneReferences' },
                reverseCloneAction: { alias: 'drop' },
                implicitInSolution: false,
                reverseImplicitInSolution: false
            });
            
            _relationship = spEntity.fromJSON({
                name: jsonString(''),
                description: jsonString(''),
                toName: jsonString(''),
                fromName: jsonString(''),
                toType: jsonLookup(), //_address, leave toType null
                fromType: _employee,
                toTypeDefaultValue: jsonLookup(),
                fromTypeDefaultValue: jsonLookup(),
                relationshipIsMandatory: false,
                revRelationshipIsMandatory: false,
                isRelationshipReadOnly: false,
                cardinality: jsonLookup('core:oneToMany'),
                relType: jsonLookup('core:relExclusiveCollection'),
                cascadeDelete: false,
                cascadeDeleteTo: false,
                cloneAction: { alias: 'drop' },
                reverseCloneAction: { alias: 'cloneReferences' },
                implicitInSolution: false,
                reverseImplicitInSolution: false
            });
            
            _lookupControl = spEntity.fromJSON({
                typeId: 'console:inlineRelationshipRenderControl',
                description: jsonString(''),
                isOfType: [{
                    id: 'console:inlineRelationshipRenderControl',
                    alias: 'console:inlineRelationshipRenderControl'
                }],
                'console:mandatoryControl': false,
                'console:readOnlyControl': false,
                'console:showControlHelpText': false,
                'console:isReversed': false,
                canCreate: true,
                canCreateDerivedTypes: true,
                'console:pickerReport': jsonLookup(),
                relationshipToRender: _lookup
            });

            _relationshipControl = spEntity.fromJSON({
                typeId: 'console:inlineRelationshipRenderControl',
                description: jsonString(''),
                isOfType: [{
                    id: 'console:inlineRelationshipRenderControl',
                    alias: 'console:inlineRelationshipRenderControl'
                }],
                'console:mandatoryControl': false,
                'console:readOnlyControl': false,
                'console:isReversed': false,
                canCreate: true,
                canCreateDerivedTypes: true,
                'console:pickerReport': jsonLookup(),
                relationshipToRender: _relationship
            });
        }));
        
        //beforeEach(inject(function ($injector, $rootScope, $controller, $q) {
        //    this.addMatchers(TestSupport.matchers);
        //    $httpBackend = $injector.get('$httpBackend');
        //    scope = $injector.get('$rootScope');
        //    q = $q;
        //    //define mock service for the entityservice
        //    mockSpFormBuilderService = {
                
        //        getDefinitionType: function () {
        //            return _employee;
        //        }
        //    };
        //    spyOn(mockSpFormBuilderService, 'getDefinitionType').andCallThrough();
        //}));

        // **** Service ******** //
        beforeEach(inject(function ($injector, $rootScope, $controller, $q) {
            this.addMatchers(TestSupport.matchers);
            $httpBackend = $injector.get('$httpBackend');
            scope = $injector.get('$rootScope');
            q = $q;
            //define mock service for the entityservice
            mockConfigDialogService = {
                getSchemaInfo: function () {
                    deferred = q.defer();
                    deferred.resolve({
                        fields: _schemaFields,
                        entities: _schemaEntities
                    });
                    return deferred.promise;
                },

                getBaseRelControlOnFormEntity: function () {
                    deferred = q.defer();
                    deferred.resolve(spEntity.fromJSON({ 'description': '' }));   // base relcontrol entity is fetched from server to augument the passedin(existing) form control to the dialog. the passedin form control alredy has all the fields we need so so not need to get anything from server.  
                    return deferred.promise;
                },
                getBaseRelationshipEntity: function () {
                    deferred = q.defer();
                    deferred.resolve(spEntity.fromJSON({ 'description': '' }));   // base relationship entity is fetched from server to augument the passedin(existing) relationship to the dialog. the passedin relationship alredy has all the fields we need so so not need to get anything from server.  
                    return deferred.promise;
                },

                getFormsForTypeAndInheritedTypes: function (typeId) {
                    deferred = q.defer();
                    deferred.resolve(fieldPropertiesTestHelper.schemaFieldTypes);   // todo: 
                    return deferred.promise;
                },
                getReportsForType: function () {
                    deferred = q.defer();
                    deferred.resolve(fieldPropertiesTestHelper.schemaFieldTypes);   // todo: 
                    return deferred.promise;
                },
                getTypeAndInheritedTypes: function () {
                    deferred = q.defer();
                    deferred.resolve(fieldPropertiesTestHelper.schemaFieldTypes);   // todo: 
                    return deferred.promise;
                },
                
                getDummyFieldControlOnForm: function (field, fieldTitle) {
                    var fieldType = field.getIsOfType()[0];

                    var defaultRenderingControl = _.find(fieldType.getDefaultRenderingControls(), function (control) {
                        return control.getContext().getName() === 'Html';
                    });
                    if (!defaultRenderingControl) {
                        defaultRenderingControl = _.find(fieldType.getRenderingControl(), function (control) {
                            return control.getContext().getName() === 'Html';
                        });
                    }
                    var dummyFormControl = spEntity.fromJSON({
                        typeId: defaultRenderingControl.nsAlias,
                        'name': fieldTitle,
                        'description': '',
                        'console:fieldToRender': field,
                        'console:mandatoryControl': false,
                        'console:showControlHelpText': false,
                        'console:readOnlyControl': false,
                        'console:isReversed': false
                    });
                    return dummyFormControl;
                }
            };
            spyOn(mockConfigDialogService, 'getSchemaInfo').andCallThrough();
            spyOn(mockConfigDialogService, 'getBaseRelControlOnFormEntity').andCallThrough();
            spyOn(mockConfigDialogService, 'getBaseRelationshipEntity').andCallThrough();
        }));

        // mocked data
        (function() {

            _schemaFields = spEntity.fromJSON(
             [{
                 id: 8001,
                 alias: 'core:name',
                 'name': 'nameField',
                 isRequired: false,
                 allowMultiLines: false,
                 maxLength: jsonInt(),
                 minLength: jsonInt(),
                 isOfType: [{
                     id: 'stringField',
                     alias: 'core:stringField',
                     'k:fieldDisplayName': { name: 'Text' },
                     'k:defaultRenderingControls': [{
                         alias: 'console:singleLineTextControl',
                         'k:context': { name: 'Html' }
                     }]
                 }]
             },
              {
                  id: 8002,
                  alias: 'core:description',
                  'name': 'descriptionField',
                  isRequired: false,
                  allowMultiLines: true,
                  maxLength: jsonInt(),
                  minLength: jsonInt(),
                  isOfType: [{
                      id: 'stringField',
                      alias: 'core:stringField',
                      'k:fieldDisplayName': { name: 'Text' },
                      'k:defaultRenderingControls': [{
                          alias: 'console:singleLineTextControl',
                          'k:context': { name: 'Html' }
                      }]
                  }]
              },
             {
                 id: 8003,
                 alias: 'core:toName',
                 'name': 'toNameField',
                 isRequired: false,
                 allowMultiLines: false,
                 maxLength: jsonInt(),
                 minLength: jsonInt(),
                 isOfType: [{
                     id: 'stringField',
                     alias: 'core:stringField',
                     'k:fieldDisplayName': { name: 'Text' },
                     'k:defaultRenderingControls': [{
                         alias: 'console:singleLineTextControl',
                         'k:context': { name: 'Html' }
                     }]
                 }]
             },
              {
                  id: 8004,
                  alias: 'core:fromName',
                  'name': 'fromNameField',
                  isRequired: false,
                  allowMultiLines: true,
                  maxLength: jsonInt(),
                  minLength: jsonInt(),
                  isOfType: [{
                      id: 'stringField',
                      alias: 'core:stringField',
                      'k:fieldDisplayName': { name: 'Text' },
                      'k:defaultRenderingControls': [{
                          alias: 'console:singleLineTextControl',
                          'k:context': { name: 'Html' }
                      }]
                  }]
              },
             {
                 id: 98003,
                 alias: 'core:toScriptName',
                 'name': 'toScriptName',
                 isRequired: false,
                 allowMultiLines: false,
                 maxLength: jsonInt(),
                 minLength: jsonInt(),
                 isOfType: [{
                     id: 'stringField',
                     alias: 'core:stringField',
                     'k:fieldDisplayName': { name: 'Text' },
                     'k:defaultRenderingControls': [{
                         alias: 'console:singleLineTextControl',
                         'k:context': { name: 'Html' }
                     }]
                 }]
             },
              {
                  id: 98004,
                  alias: 'core:fromScriptName',
                  'name': 'fromScriptName',
                  isRequired: false,
                  allowMultiLines: true,
                  maxLength: jsonInt(),
                  minLength: jsonInt(),
                  isOfType: [{
                      id: 'stringField',
                      alias: 'core:stringField',
                      'k:fieldDisplayName': { name: 'Text' },
                      'k:defaultRenderingControls': [{
                          alias: 'console:singleLineTextControl',
                          'k:context': { name: 'Html' }
                      }]
                  }]
              }
             ]);
            
            _schemaEntities = spEntity.fromJSON(
             [{
                 id: 9001,
                 alias: 'core:definition',
                 'name': 'definition',
                 isRequired: false,
                 allowMultiLines: false,
                 maxLength: jsonInt(),
                 minLength: jsonInt(),
                 isOfType: [{
                     id: 'core:type',
                     alias: 'core:definition'
                 }]
             }
             ]);
            
            
        })();
    })();
    
    // lookup
    describe('spRelationshipPropertiesController tests for new lookup control representing new lookup', function () {

        beforeEach(inject(function ($injector, $rootScope, $controller) {
            var lookupCtrl = _lookupControl.cloneDeep(); // this is here to keep the base control untouched so that it can be used in different tests in its initial state
            
            scope.options = {
                formControl: lookupCtrl,
                isFormControl: true,
                relationshipType: 'lookup'
            };
            var modalInstance = {
                close: function (result) { return result; }
            };
            scope.modalInstance = modalInstance;

            controller = $controller('spRelationshipPropertiesController', {
                $scope: scope,
                configureDialogService: mockConfigDialogService,
                controlConfigureDialogFactory: mockControlConfigureDialogFactory
                //spFormBuilderService: mockSpFormBuilderService
            });

            scope.$root.$digest();
        }));

        it('spRelationshipPropertiesController is loaded', function () {
            expect(controller).toBeTruthy();
            //expect(mockSpFormBuilderService.getDefinitionType).toHaveBeenCalled();
            expect(mockConfigDialogService.getSchemaInfo).toHaveBeenCalled();
        });
        
        it('should have all the form controls are defined', function () {
            expect(scope.model.formCtrlNameControl).toBeTruthy();
            expect(scope.model.nameControl).toBeTruthy();
            expect(scope.model.reverseNameControl).toBeTruthy();
            expect(scope.model.relNameControl).toBeTruthy();
            expect(scope.model.relDescriptionControl).toBeTruthy();
        });
        
        it('all scope variables have correct values on initial load complete', function () {
            expect(scope.model.isNewControl).toBe(true);
            expect(scope.model.isNewRelationship).toBe(true); 
            expect(scope.model.relationshipToRender.name).toBe('');  // toType is not selected yet
            expect(scope.model.relationshipToRender.toName).toBe('');           // toType is not selected yet
            expect(scope.model.relationshipToRender.fromName).toBe('Employee');
            expect(scope.isRelTypeSectionCollapsed).toBe(true);
            expect(scope.isOwnershipSectionCollapsed).toBe(true);
            expect(scope.isGeneralSectionCollapsed).toBe(true);
        });
        
        it('selecting a related object should recalculate name fields', function () {

            // select the related object
            scope.model.objectTypePickerOptions.selectedEntities = [_department];
            scope.$digest();
            
            expect(scope.model.relationshipToRender.name).toBe('Employee - Department');
            expect(scope.model.relationshipToRender.toName).toBe('Department');
            expect(scope.model.relationshipToRender.fromName).toBe('Employee');
            expect(scope.model.formControl.name).toBe(null);
        });
        
    });
    
    describe('spRelationshipPropertiesController tests for new lookup control representing existing lookup', function () {

        beforeEach(inject(function ($injector, $rootScope, $controller) {
            var lookupCtrl = _lookupControl.cloneDeep(); // this is here to keep the base control untouched so that it can be used in different tests in its initial state

            lookupCtrl.relationshipToRender.setDataState(spEntity.DataStateEnum.Unchanged); // set the data state to 'Unchanged' so that the dialog can treat it as an existing lookup
            lookupCtrl.relationshipToRender.toType = [_department];   // set toType

            scope.options = {
                formControl: lookupCtrl,
                isFormControl: true,
                relationshipType: 'lookup'
            };
            var modalInstance = {
                close: function (result) { return result; }
            };
            scope.modalInstance = modalInstance;

            controller = $controller('spRelationshipPropertiesController', {
                $scope: scope,
                configureDialogService: mockConfigDialogService,
                controlConfigureDialogFactory: mockControlConfigureDialogFactory
                //spFormBuilderService: mockSpFormBuilderService
            });

            scope.$root.$digest();
        }));

        it('spRelationshipPropertiesController is loaded', function () {
            expect(controller).toBeTruthy();
            //expect(mockSpFormBuilderService.getDefinitionType).toHaveBeenCalled();
            expect(mockConfigDialogService.getSchemaInfo).toHaveBeenCalled();
        });

        it('should have all the form controls are defined', function () {
            expect(scope.model.formCtrlNameControl).toBeTruthy();
            expect(scope.model.nameControl).toBeTruthy();
            expect(scope.model.reverseNameControl).toBeTruthy();
            expect(scope.model.relNameControl).toBeTruthy();
            expect(scope.model.relDescriptionControl).toBeTruthy();
        });

        it('all scope variables have correct values on initial load complete', function () {
            expect(scope.model.isNewControl).toBe(true);
            expect(scope.model.isNewRelationship).toBe(false);
        });
    });
    
    describe('spRelationshipPropertiesController tests for existing lookup control representing existing lookup', function () {

        beforeEach(inject(function ($injector, $rootScope, $controller) {

            // init
            var lookupCtrl = _lookupControl.cloneDeep();    // this is here to keep the base control untouched so that it can be used in different tests in its initial state
            
            // set the data state to 'Unchanged' so that the dialog can treat it as an existing lookup control and existin lookup
            lookupCtrl.setDataState(spEntity.DataStateEnum.Unchanged);
            lookupCtrl.relationshipToRender.setDataState(spEntity.DataStateEnum.Unchanged);
            lookupCtrl.relationshipToRender.toType = [_department];   // set toType

            scope.options = {
                formControl: lookupCtrl,
                isFormControl: true,
                relationshipType: 'lookup'
            };
            var modalInstance = {
                close: function (result) { return result; }
            };
            scope.modalInstance = modalInstance;

            controller = $controller('spRelationshipPropertiesController', {
                $scope: scope,
                configureDialogService: mockConfigDialogService,
                controlConfigureDialogFactory: mockControlConfigureDialogFactory
                //spFormBuilderService: mockSpFormBuilderService
            });

            scope.$root.$digest();
        }));

        it('spRelationshipPropertiesController is loaded', function () {
            expect(controller).toBeTruthy();
            expect(mockConfigDialogService.getSchemaInfo).toHaveBeenCalled();
        });
        
        it('should have all the form controls are defined', function () {
            expect(scope.model.formCtrlNameControl).toBeTruthy();
            expect(scope.model.nameControl).toBeTruthy();
            expect(scope.model.reverseNameControl).toBeTruthy();
            expect(scope.model.relNameControl).toBeTruthy();
            expect(scope.model.relDescriptionControl).toBeTruthy();
        });
        
        it('all scope variables have correct values on initial load complete', function () {
            expect(scope.model.isNewControl).toBe(false);
        });
    });
    
    describe('spRelationshipPropertiesController tests for new lookup', function () {

        beforeEach(inject(function ($injector, $rootScope, $controller) {
            var lookup = _lookup.cloneDeep(); // this is here to keep the base control untouched so that it can be used in different tests in its initial state

            scope.options = {
                isFormControl: false,
                relationshipType: 'lookup',
                relationship: lookup
            };
            var modalInstance = {
                close: function (result) { return result; }
            };
            scope.modalInstance = modalInstance;

            controller = $controller('spRelationshipPropertiesController', {
                $scope: scope,
                configureDialogService: mockConfigDialogService,
                controlConfigureDialogFactory: mockControlConfigureDialogFactory
                //spFormBuilderService: mockSpFormBuilderService
            });

            scope.$root.$digest();
        }));

        it('spRelationshipPropertiesController is loaded', function () {
            expect(controller).toBeTruthy();
            //expect(mockSpFormBuilderService.getDefinitionType).toHaveBeenCalled();
            expect(mockConfigDialogService.getSchemaInfo).toHaveBeenCalled();
        });

        it('should have all the form controls are defined', function () {
            expect(scope.model.formCtrlNameControl).toBeUndefined();
            expect(scope.model.nameControl).toBeTruthy();
            expect(scope.model.reverseNameControl).toBeTruthy();
            expect(scope.model.relNameControl).toBeTruthy();
            expect(scope.model.relDescriptionControl).toBeTruthy();
        });

        it('all scope variables have correct values on initial load complete', function () {
            expect(scope.model.isNewRelationship).toBe(true);
            expect(scope.model.relationshipToRender.name).toBe('');  // toType is not selected yet
            expect(scope.model.relationshipToRender.toName).toBe('');           // toType is not selected yet
            expect(scope.model.relationshipToRender.fromName).toBe('Employee');
            expect(scope.isRelTypeSectionCollapsed).toBe(true);
            expect(scope.isOwnershipSectionCollapsed).toBe(true);
            expect(scope.isGeneralSectionCollapsed).toBe(true);
        });

        it('selecting a related object should recalculate name fields', function () {

            // select the related object
            scope.model.objectTypePickerOptions.selectedEntities = [_department];
            scope.$digest();

            expect(scope.model.relationshipToRender.name).toBe('Employee - Department');
            expect(scope.model.relationshipToRender.toName).toBe('Department');
            expect(scope.model.relationshipToRender.fromName).toBe('Employee');
            expect(scope.model.relationshipToRender.fromType.name).toBe('Employee');
            expect(scope.model.relationshipToRender.toType.name).toBe('Department');
            expect(scope.model.oneToOneOwnerOptions).toBeDefined();
        });
        
        it('selecting oneToOne cardinality and changing ownership and owner updates the relType correctly', function () {
            // set the related object
            scope.model.objectTypePickerOptions.selectedEntities = [_department];
            scope.$digest();
            
            // initial values
            expect(scope.model.isReverseRelationship).toBe(false);
            expect(scope.model.ui.relCardinality).toBe('manyToOne');
            expect(scope.model.ui.relOwnership).toBe('none');
            expect(scope.model.oneToOneOwnerOptions).toBeDefined();
            expect(scope.model.oneToOneOwnerOptions.length).toBe(2);
            expect(scope.model.oneToOneOwnerOptions[0].value).toBe('fromType');
            expect(scope.model.oneToOneOwnerOptions[1].value).toBe('toType');
            
            // *set the cardinality to 'oneToOne' 
            scope.model.ui.relCardinality = 'oneToOne';
            scope.$digest();

            // ownership 'none' 
            expect(scope.model.ui.relCardinality).toBe('oneToOne');
            expect(scope.model.actualRelType.alias()).toBe('core:relSingleLookup');
            
            // *set the ownership to 'full'
            scope.model.ui.relOwnership = 'full';
            scope.$digest();
            
            expect(scope.model.ui.relOwner.value).toBe('toType');
            expect(scope.model.actualRelType.alias()).toBe('core:relSingleComponentOf');
            
            // *set the owner to 'fromType'
            scope.model.ui.relOwner = scope.model.oneToOneOwnerOptions[0];
            scope.$digest();
            
            expect(scope.model.actualRelType.alias()).toBe('core:relSingleComponent');
            
            // click ok
            
            //chk values befor clicking ok
            expect(scope.options.relationship.toType).toBe(null);
            expect(scope.options.relationship.relType.alias()).toBe('core:relLookup');
            
            scope.form = { $valid: true };
            scope.ok();
            scope.$digest();
            
            // chk values after ok
            expect(scope.options.relationship.toType.name).toBe('Department');
            expect(scope.options.relationship.relType.alias()).toBe('core:relSingleComponent');
        });

    });
    
    describe('spRelationshipPropertiesController tests for existing lookup', function () {

        beforeEach(inject(function ($injector, $rootScope, $controller) {
            var lookup = _lookup.cloneDeep(); // this is here to keep the base control untouched so that it can be used in different tests in its initial state
            lookup.setDataState(spEntity.DataStateEnum.Unchanged); // set the data state to 'Unchanged' so that the dialog can treat it as an existing lookup

            scope.options = {
                isFormControl: false,
                relationshipType: 'lookup',
                relationship: lookup
            };
            var modalInstance = {
                close: function (result) { return result; }
            };
            scope.modalInstance = modalInstance;

            controller = $controller('spRelationshipPropertiesController', {
                $scope: scope,
                configureDialogService: mockConfigDialogService,
                controlConfigureDialogFactory: mockControlConfigureDialogFactory
                //spFormBuilderService: mockSpFormBuilderService
            });

            scope.$root.$digest();
        }));

        it('spRelationshipPropertiesController is loaded', function () {
            expect(controller).toBeTruthy();
            //expect(mockSpFormBuilderService.getDefinitionType).toHaveBeenCalled();
            expect(mockConfigDialogService.getSchemaInfo).toHaveBeenCalled();
        });

        it('should have all the form controls are defined', function () {
            expect(scope.model.formCtrlNameControl).toBeUndefined();
            expect(scope.model.nameControl).toBeTruthy();
            expect(scope.model.reverseNameControl).toBeTruthy();
            expect(scope.model.relNameControl).toBeTruthy();
            expect(scope.model.relDescriptionControl).toBeTruthy();
        });

        it('all scope variables have correct values on initial load complete', function () {
            expect(scope.model.isNewRelationship).toBe(false);
            expect(scope.model.relationshipToRender.name).toBe('');  // toType is not selected yet
            expect(scope.model.relationshipToRender.toName).toBe('');           // toType is not selected yet
            expect(scope.model.relationshipToRender.fromName).toBe('');
            expect(scope.isRelTypeSectionCollapsed).toBe(true);
            expect(scope.isOwnershipSectionCollapsed).toBe(true);
            expect(scope.isGeneralSectionCollapsed).toBe(true);
        });

        it('selecting a related object should recalculate name fields', function () {

            // select the related object
            scope.model.objectTypePickerOptions.selectedEntities = [_department];
            scope.$digest();

            // names should not get recalculted in case of existing lookup
            expect(scope.model.relationshipToRender.name).toBe(' - Department');
            expect(scope.model.relationshipToRender.toName).toBe('Department');
            expect(scope.model.relationshipToRender.fromName).toBe('');
        });

    });
    
    // relationship
    describe('spRelationshipPropertiesController tests for new relationship control representing new relationship', function () {

        beforeEach(inject(function ($injector, $rootScope, $controller) {
            var relCtrl = _relationshipControl.cloneDeep(); // this is here to keep the base control untouched so that it can be used in different tests in its initial state

            scope.options = {
                formControl: relCtrl,
                isFormControl: true,
                relationshipType: 'relationship'
            };
            var modalInstance = {
                close: function (result) { return result; }
            };
            scope.modalInstance = modalInstance;

            controller = $controller('spRelationshipPropertiesController', {
                $scope: scope,
                configureDialogService: mockConfigDialogService,
                controlConfigureDialogFactory: mockControlConfigureDialogFactory
                //spFormBuilderService: mockSpFormBuilderService
            });

            scope.$root.$digest();
        }));

        it('spRelationshipPropertiesController is loaded', function () {
            expect(controller).toBeTruthy();
            //expect(mockSpFormBuilderService.getDefinitionType).toHaveBeenCalled();
            expect(mockConfigDialogService.getSchemaInfo).toHaveBeenCalled();
        });

        it('should have all the form controls are defined', function () {
            expect(scope.model.formCtrlNameControl).toBeTruthy();
            expect(scope.model.nameControl).toBeTruthy();
            expect(scope.model.reverseNameControl).toBeTruthy();
            expect(scope.model.relNameControl).toBeTruthy();
            expect(scope.model.relDescriptionControl).toBeTruthy();
        });

        it('all scope variables have correct values on initial load complete', function () {
            expect(scope.model.isNewControl).toBe(true);
            expect(scope.model.isNewRelationship).toBe(true);
            expect(scope.model.relationshipToRender.name).toBe('');  // toType is not selected yet
            expect(scope.model.relationshipToRender.toName).toBe('');           // toType is not selected yet
            expect(scope.model.relationshipToRender.fromName).toBe('Employee');
            expect(scope.isRelTypeSectionCollapsed).toBe(true);
            expect(scope.isOwnershipSectionCollapsed).toBe(true);
            expect(scope.isGeneralSectionCollapsed).toBe(true);
        });

        it('selecting a related object should recalculate name fields', function () {

            // select the related object
            scope.model.objectTypePickerOptions.selectedEntities = [_department];
            scope.$digest();

            expect(scope.model.relationshipToRender.name).toBe('Employee - Department');
            expect(scope.model.relationshipToRender.toName).toBe('Department');
            expect(scope.model.relationshipToRender.fromName).toBe('Employee');
            expect(scope.model.formControl.name).toBe(null);
        });

    });
    
    describe('spRelationshipPropertiesController tests for new relationship control representing existing relationship', function () {

        beforeEach(inject(function ($injector, $rootScope, $controller) {
            var relCtrl = _relationshipControl.cloneDeep(); // this is here to keep the base control untouched so that it can be used in different tests in its initial state

            relCtrl.relationshipToRender.setDataState(spEntity.DataStateEnum.Unchanged); // set the data state to 'Unchanged' so that the dialog can treat it as an existing lookup
            relCtrl.relationshipToRender.toType = [_address];   // set toType

            scope.options = {
                formControl: relCtrl,
                isFormControl: true,
                relationshipType: 'relationship'
            };
            var modalInstance = {
                close: function (result) { return result; }
            };
            scope.modalInstance = modalInstance;

            controller = $controller('spRelationshipPropertiesController', {
                $scope: scope,
                configureDialogService: mockConfigDialogService,
                controlConfigureDialogFactory: mockControlConfigureDialogFactory
                //spFormBuilderService: mockSpFormBuilderService
            });

            scope.$root.$digest();
        }));

        it('spRelationshipPropertiesController is loaded', function () {
            expect(controller).toBeTruthy();
            //expect(mockSpFormBuilderService.getDefinitionType).toHaveBeenCalled();
            expect(mockConfigDialogService.getSchemaInfo).toHaveBeenCalled();
        });

        it('should have all the form controls are defined', function () {
            expect(scope.model.formCtrlNameControl).toBeTruthy();
            expect(scope.model.nameControl).toBeTruthy();
            expect(scope.model.reverseNameControl).toBeTruthy();
            expect(scope.model.relNameControl).toBeTruthy();
            expect(scope.model.relDescriptionControl).toBeTruthy();
        });

        it('all scope variables have correct values on initial load complete', function () {
            expect(scope.model.isNewControl).toBe(true);
            expect(scope.model.isNewRelationship).toBe(false);
        });
    });
    
    describe('spRelationshipPropertiesController tests for existing relationship control representing existing relationship', function () {

        beforeEach(inject(function ($injector, $rootScope, $controller) {

            // init
            var relCtrl = _relationshipControl.cloneDeep();    // this is here to keep the base control untouched so that it can be used in different tests in its initial state

            // set the data state to 'Unchanged' so that the dialog can treat it as an existing lookup control and existin lookup
            relCtrl.setDataState(spEntity.DataStateEnum.Unchanged);
            relCtrl.relationshipToRender.setDataState(spEntity.DataStateEnum.Unchanged);
            relCtrl.relationshipToRender.toType = [_address];   // set toType

            scope.options = {
                formControl: relCtrl,
                isFormControl: true,
                relationshipType: 'relationship'
            };
            var modalInstance = {
                close: function (result) { return result; }
            };
            scope.modalInstance = modalInstance;

            controller = $controller('spRelationshipPropertiesController', {
                $scope: scope,
                configureDialogService: mockConfigDialogService,
                controlConfigureDialogFactory: mockControlConfigureDialogFactory
                //spFormBuilderService: mockSpFormBuilderService
            });

            scope.$root.$digest();
        }));

        it('spRelationshipPropertiesController is loaded', function () {
            expect(controller).toBeTruthy();
            expect(mockConfigDialogService.getSchemaInfo).toHaveBeenCalled();
        });

        it('should have all the form controls are defined', function () {
            expect(scope.model.formCtrlNameControl).toBeTruthy();
            expect(scope.model.nameControl).toBeTruthy();
            expect(scope.model.reverseNameControl).toBeTruthy();
            expect(scope.model.relNameControl).toBeTruthy();
            expect(scope.model.relDescriptionControl).toBeTruthy();
        });

        it('all scope variables have correct values on initial load complete', function () {
            expect(scope.model.isNewControl).toBe(false);
        });
    });
    
    describe('spRelationshipPropertiesController tests for new relationship', function () {

        beforeEach(inject(function ($injector, $rootScope, $controller) {
            var relationship = _relationship.cloneDeep(); // this is here to keep the base control untouched so that it can be used in different tests in its initial state

            scope.options = {
                isFormControl: false,
                relationshipType: 'relationship',
                relationship: relationship
            };
            var modalInstance = {
                close: function (result) { return result; }
            };
            scope.modalInstance = modalInstance;

            controller = $controller('spRelationshipPropertiesController', {
                $scope: scope,
                configureDialogService: mockConfigDialogService,
                controlConfigureDialogFactory: mockControlConfigureDialogFactory
                //spFormBuilderService: mockSpFormBuilderService
            });

            scope.$root.$digest();
        }));

        it('spRelationshipPropertiesController is loaded', function () {
            expect(controller).toBeTruthy();
            //expect(mockSpFormBuilderService.getDefinitionType).toHaveBeenCalled();
            expect(mockConfigDialogService.getSchemaInfo).toHaveBeenCalled();
        });

        it('should have all the form controls are defined', function () {
            expect(scope.model.formCtrlNameControl).toBeUndefined();
            expect(scope.model.nameControl).toBeTruthy();
            expect(scope.model.reverseNameControl).toBeTruthy();
            expect(scope.model.relNameControl).toBeTruthy();
            expect(scope.model.relDescriptionControl).toBeTruthy();
        });

        it('all scope variables have correct values on initial load complete', function () {
            expect(scope.model.isNewRelationship).toBe(true);
            expect(scope.model.relationshipToRender.name).toBe('');  // toType is not selected yet
            expect(scope.model.relationshipToRender.toName).toBe('');           // toType is not selected yet
            expect(scope.model.relationshipToRender.fromName).toBe('Employee');
            expect(scope.isRelTypeSectionCollapsed).toBe(true);
            expect(scope.isOwnershipSectionCollapsed).toBe(true);
            expect(scope.isGeneralSectionCollapsed).toBe(true);
        });

        it('selecting a related object should recalculate name fields', function () {

            // select the related object
            scope.model.objectTypePickerOptions.selectedEntities = [_address];
            scope.$digest();

            expect(scope.model.relationshipToRender.name).toBe('Employee - Address');
            expect(scope.model.relationshipToRender.toName).toBe('Address');
            expect(scope.model.relationshipToRender.fromName).toBe('Employee');
        });

    });
    
    describe('spRelationshipPropertiesController tests for existing relationship', function () {

        beforeEach(inject(function ($injector, $rootScope, $controller) {
            var relationship = _relationship.cloneDeep(); // this is here to keep the base control untouched so that it can be used in different tests in its initial state
            relationship.setDataState(spEntity.DataStateEnum.Unchanged); // set the data state to 'Unchanged' so that the dialog can treat it as an existing lookup

            scope.options = {
                isFormControl: false,
                relationshipType: 'relationship',
                relationship: relationship
            };
            var modalInstance = {
                close: function (result) { return result; }
            };
            scope.modalInstance = modalInstance;

            controller = $controller('spRelationshipPropertiesController', {
                $scope: scope,
                configureDialogService: mockConfigDialogService,
                controlConfigureDialogFactory: mockControlConfigureDialogFactory
                //spFormBuilderService: mockSpFormBuilderService
            });

            scope.$root.$digest();
        }));

        it('spRelationshipPropertiesController is loaded', function () {
            expect(controller).toBeTruthy();
            //expect(mockSpFormBuilderService.getDefinitionType).toHaveBeenCalled();
            expect(mockConfigDialogService.getSchemaInfo).toHaveBeenCalled();
        });

        it('should have all the form controls are defined', function () {
            expect(scope.model.formCtrlNameControl).toBeUndefined();
            expect(scope.model.nameControl).toBeTruthy();
            expect(scope.model.reverseNameControl).toBeTruthy();
            expect(scope.model.relNameControl).toBeTruthy();
            expect(scope.model.relDescriptionControl).toBeTruthy();
        });

        it('all scope variables have correct values on initial load complete', function () {
            expect(scope.model.isNewRelationship).toBe(false);
            expect(scope.model.relationshipToRender.name).toBe('');  // toType is not selected yet
            expect(scope.model.relationshipToRender.toName).toBe('');           // toType is not selected yet
            expect(scope.model.relationshipToRender.fromName).toBe('');
            expect(scope.isRelTypeSectionCollapsed).toBe(true);
            expect(scope.isOwnershipSectionCollapsed).toBe(true);
            expect(scope.isGeneralSectionCollapsed).toBe(true);
        });

        it('selecting a related object should recalculate name fields', function () {

            // select the related object
            scope.model.objectTypePickerOptions.selectedEntities = [_department];
            scope.$digest();

            // names should not get recalculted in case of existing lookup
            expect(scope.model.relationshipToRender.name).toBe(' - Department');
            expect(scope.model.relationshipToRender.toName).toBe('Department');
            expect(scope.model.relationshipToRender.fromName).toBe('');
        });

    });
});