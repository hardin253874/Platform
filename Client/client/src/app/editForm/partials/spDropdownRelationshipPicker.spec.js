// Copyright 2011-2016 Global Software Innovation Pty Ltd
/*global _, angular, expect, beforeEach, describe, it, xit, inject, entityTestData, module, spEntity */
/* globals EditFormTestHelper */

describe('Edit Form|spec:|dropdownRelationshipPicker directive', function () {
    describe('will create', function () {
        var controller, $scope;

        // Load the modules
        beforeEach(module('app.editForm.spDropdownRelationshipPicker'));
        beforeEach(module('mod.common.ui.entityPickerControllers'));
        beforeEach(module('editForm/partials/customEditForm.tpl.html'));
        beforeEach(module('editForm/partials/spDropdownRelationshipPicker.tpl.html'));
        beforeEach(module('mockedEntityService'));
        beforeEach(module('mockedLoginService'));
        beforeEach(module('mockedNavService'));


        var inlineFwdRelContolJson = {
            id: 'myRelControl66',
            typeid: 'inlineRelationshipRenderControl',
            isReversed: false,
            'console:relationshipToRender': {
                id: 9992,
                name: 'dummy company has dummyEmployee',
                fromType: { id: 1111, name: 'dummyCompany', isAbstract: false },
                toType: { id: 2222, name: 'dummyEmployee', isAbstract: false },
                toTypeDefaultValue: { id: 999, typeId: 'myToType' }

            }
        };

        // Set the mocked data
        beforeEach(inject(function (spEntityService, spNavService) {
            // Set the data we wish the mock to return
            spEntityService.mockTemplateReport();

            spEntityService.mockGetInstancesOfTypeRawData('2222', entityTestData.thumbnailSizesTestData); // using dummy data from 'thumbnailSizesTestData'

            spNavService.navigateToChildState = _.noop;
        }));


        function init(scope, spEntityService) {
            // init vars 
            scope.formControl = spEntity.fromJSON(inlineFwdRelContolJson);
            scope.templateReport = spEntityService.getEntity('core:templateReport', 'name');
            scope.pickerOptions = {
                formControl: scope.formControl,
                entityTypeId: scope.formControl.getRelationshipToRender().getToType().id(),
                selectedEntityId: null,
                selectedEntity: null
            };
        }
        
        //function getControlByName(rootElement, controlType, name) {
        //    var ctrl = null;

        //    return ctrl;
        //}


        it('it should be possible to load relationship control', inject(function ($rootScope, $compile, spEntityService) {
            var scope = $rootScope,
                element;

            // init vars 
            init(scope, spEntityService);
            
            element = angular.element('<sp-dropdown-relationship-picker options="pickerOptions" />');
            $compile(element)(scope);
            scope.$apply();

        }));

        it('clicking the clear button should clear the selection.', inject(function ($rootScope, $compile, spEntityService) {
            var scope = $rootScope,
                element,
                clearBtn;

            // init vars 
            init(scope, spEntityService);
            
            scope.pickerOptions.selectedEntityId = 12160;   // set selectedEntityId
            element = angular.element('<sp-dropdown-relationship-picker options="pickerOptions" />');
            $compile(element)(scope);
            scope.$apply();

            expect(scope.pickerOptions.selectedEntityId).toBe(12160);

            clearBtn = element.find('button')[1];

            expect(clearBtn.name).toBe('clearBtn');


            expect(clearBtn).toBeDefined();
            expect(clearBtn).not.toBe(null);

            //click the button
            clearBtn.click();
            scope.$digest();
            
            expect(scope.pickerOptions.selectedEntityId).toBe(0);
        }));
    });
});