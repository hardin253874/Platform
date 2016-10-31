// Copyright 2011-2016 Global Software Innovation Pty Ltd
describe('Edit Form|spec:|tabRelationshipRenderControl directive', function () {
    describe('will create', function () {
        var controller, $scope, AA_Person;

        // Load the modules
        beforeEach(module('mod.app.editForm.designerDirectives.spTabRelationshipRenderControl'));
        beforeEach(module('editForm/directives/spTabRelationshipRenderControl/spTabRelationshipRenderControl.tpl.html'));
        beforeEach(module('mockedEntityService'));
        beforeEach(module('mod.common.ui.spDialogService'));
        beforeEach(module('sp.navService'));
        
        // Set the mocked data to create derived types menu
        beforeEach(inject(function (spEntityService) {
            // Set the data we wish the mock to return
            spEntityService.mockGetEntity(AA_Person);

            spEntityService.mockTemplateReport();
        }));

        AA_Person = spEntity.fromJSON({
            id: { id: 7644, ns: 'test', alias: 'person' },
            name: 'AA_Person',
            isAbstract: false,
            defaultPickerReport: { id: 111, name: 'toTypePickerReport' },
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

        var testDefAInstance = spEntity.fromJSON(
       {
           id: '333',
           typeId: 'intField'
       });

        var tabFwdRelContolJson = {
            id: 'myRelForm',
            typeid: 'tabRelationshipRenderControl',
            isReversed: false,
            canCreate: true,
            canCreateDerivedTypes: false,
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
            scope.formControl = spEntity.fromJSON(tabFwdRelContolJson);
            scope.templateReport = spEntityService.getEntity('core:templateReport', 'name');
        }

        it('it should be possible to load tab relationship render control', inject(function ($rootScope, $compile, spEntityService) {
            var scope = $rootScope,
                element;

            // init vars 
            init(scope, spEntityService);

            element = angular.element('<sp-tab-relationship-render-control form-control="formControl"/>');
            $compile(element)(scope);
            scope.$digest();
        }));
        
        it('the correct picker report is set', inject(function ($rootScope, $compile, spEntityService) {
            var scope = $rootScope,
                element;

            // init vars 
            init(scope, spEntityService);

            element = angular.element('<sp-tab-relationship-render-control form-control="formControl"/>');  // toTypePickerReport
            $compile(element)(scope);
            scope.$digest();
            
            expect(scope.$$childTail.pickerReportOptions.reportId).toBe(111);
        }));
        
        it('the display report is set correctly when form data is loaded', inject(function ($rootScope, $compile, spEntityService, $timeout) {
            var scope = $rootScope,
                element;

            // init vars 
            init(scope, spEntityService);
            
            // set formData (instance being edited)
            scope.formData = spEntity.fromJSON(testDefAInstance);

            element = angular.element('<sp-tab-relationship-render-control form-control="formControl" form-data="formData"/>');
            $compile(element)(scope);
            scope.$digest();
            $timeout.flush();

            expect(scope.$$childTail.displayReportOptions.reportId).toBe(22222);    // template report
        }));

        it('handleOk and remove function exists when form data is loaded', inject(function ($rootScope, $compile, spEntityService) {
            var scope = $rootScope,
                element;

            // init vars 
            init(scope, spEntityService);

            // set formData (instance being edited)
            scope.formData = spEntity.fromJSON(testDefAInstance);

            element = angular.element('<sp-tab-relationship-render-control form-control="formControl" form-data="formData"/>');
            $compile(element)(scope);
            scope.$digest();

            expect(scope.$$childTail.handleOk !== null).toBeTruthy();    // handleOK function exists when load
            expect(scope.$$childTail.remove !== null).toBeTruthy();    // handleOK function exists when load
        }));
    });
});