// Copyright 2011-2016 Global Software Innovation Pty Ltd
/* globals EditFormTestHelper */

describe('Edit Form|spec:|inlineRelationshipPicker directive', function () {
    describe('will create', function () {
        var controller, $scope, AA_Person;

        // Load the modules
        beforeEach(module('app.editForm.spInlineRelationshipPicker'));
        beforeEach(module('app.editForm.spInlineRelationshipPickerController'));
        beforeEach(module('editForm/partials/spInlineRelationshipPicker.tpl.html'));
        beforeEach(module('mockedEntityService'));

        // Set the mocked data
        beforeEach(inject(function (spEntityService) {
            spEntityService.mockTemplateReport();
        }));
        
        // Set the mocked data to create derived types menu
        beforeEach(inject(function (spEntityService) {
            // Set the data we wish the mock to return
            spEntityService.mockGetEntity(AA_Person);
        }));

        AA_Person = spEntity.fromJSON({
            id: { id: 7644, ns: 'test', alias: 'person' },
            name: 'AA_Person',
            isAbstract: false,
            defaultPickerReport: jsonLookup(),
            derivedTypes: [
                {
                    id: { id: 4487, ns: 'test', alias: 'employee' },
                    name: "AA_Employee",
                    isAbstract: false,
                    derivedTypes: [
                        {
                            id: { id: 5942, ns: 'test', alias: 'manager' },
                            name: "AA_Manager"
                        }
                    ]
                },
                {
                    id: { id: 6518, ns: 'test', alias: 'driver' },
                    name: "AA_Driver"
                }
            ]

        });

        var controlPickerReport = {
            id: { id: 9111, ns: 'test', alias: 'controlPickerReport' }
        };
        
        var toTypeDefaultPickerReport = {
            id: { id: 9222, ns: 'test', alias: 'toTypePickerReport' }
        };
        
        var inlineFwdRelContolJson = {
            id: 'myRelForm',
            description: jsonString(''),
            typeid: 'inlineRelationshipRenderControl',
            isReversed: false,
            canCreate: true,
            canCreateDerivedTypes: false,
            'console:pickerReport': jsonLookup(),
            'console:relationshipToRender': {
                id: 9992,
                name: 'A has relationship to AA_Person',
                fromType: { id: 1111, name: 'A', isAbstract: false, defaultPickerReport: { id: 11, name: 'fromTypePickerReport' } },
                toType: spEntity.fromJSON(AA_Person),
                toTypeDefaultValue: { id: 999, typeId: 'myToType' }
            }
        };
        
        function init(scope, spEntityService) {
            // init vars 
            scope.formControl = spEntity.fromJSON(inlineFwdRelContolJson);
            scope.templateReport = spEntityService.getEntity('core:templateReport', 'name');
            scope.pickerOptions = {
                formControl: scope.formControl,
                entityTypeId: scope.formControl.getRelationshipToRender().getToType().id(),
                selectedEntities: null
            };
        }

        it('it should be possible to load relationship control', inject(function ($rootScope, $compile, spEntityService) {
            var scope = $rootScope,
                element;

            // init vars 
            init(scope, spEntityService);

            element = angular.element('<sp-inline-relationship-picker options="pickerOptions" />');
            $compile(element)(scope);
            scope.$digest();
        }));
        
        it('the picker report defined on the relationship control is used for picking related resources', inject(function ($rootScope, $compile, spEntityService) {
            var scope = $rootScope,
                element;

            // init vars 
            init(scope, spEntityService);
            
            // set the control picker report
            scope.formControl.setPickerReport(spEntity.fromJSON(controlPickerReport));

            element = angular.element('<sp-inline-relationship-picker options="pickerOptions" />');
            $compile(element)(scope);
            scope.$digest();

            expect(scope.$$childTail.reportOptions.reportId).toBe(9111);    // controlPickerReport
        }));
        
        it('the default picker report of toType is used for picking related resources if no picker report is defined on the relationship control on form', inject(function ($rootScope, $compile, spEntityService) {
            var scope = $rootScope,
                element;

            // init vars 
            init(scope, spEntityService);

            // set the default picker report of toType
            scope.formControl.getRelationshipToRender().getToType().setDefaultPickerReport(spEntity.fromJSON(toTypeDefaultPickerReport));

            element = angular.element('<sp-inline-relationship-picker options="pickerOptions" />');  
            $compile(element)(scope);
            scope.$digest();

            expect(scope.$$childTail.reportOptions.reportId).toBe(9222);    // toTypeDefaultPickerReport
        }));
        
        
        it('the template report is provided as a picker report when toType does not has a default picker report specified', inject(function ($rootScope, $compile, spEntityService) {
            var scope = $rootScope,
                element;

            // init vars 
            init(scope, spEntityService);
            
            // set the default picker report of toType to null first as the above test has set to a valid entity
            scope.formControl.getRelationshipToRender().getToType().setDefaultPickerReport(null);

            element = angular.element('<sp-inline-relationship-picker options="pickerOptions" />');  
            $compile(element)(scope);
            scope.$digest();

            expect(scope.$$childTail.reportOptions.reportId).toBe(22222);   // template report
        }));
        
    });
});