// Copyright 2011-2016 Global Software Innovation Pty Ltd
/**
 * Tests sit right alongside the file they are testing, which is more intuitive
 * and portable than separating `src` and `test` directories. Additionally, the
 * build process will exclude all `.spec.js` files from the build
 * automatically.
 */
describe('Console|controls|spec:|spDateConfigcontrol directive', function () {
    beforeEach(module('app.demoDialog'));

    // Load the modules
    beforeEach(module('app.controls.spDateConfigControl'));
    beforeEach(module('mod.common.spInclude'));
    beforeEach(module('controls/spDateConfigControl/spDateConfigControl.tpl.html'));
    beforeEach(module('app.controls.providers'));
    beforeEach(module('mod.app.editFormServices'));// 
    beforeEach(module('mod.app.editForm.spFieldControlProvider'));
    beforeEach(module('mod.app.editForm.designerDirectives.spTitlePlusMarkers'));
    beforeEach(module('editForm/directives/spTitlePlusMarkers/spTitlePlusMarkers.tpl.html'));
    beforeEach(module('mod.app.editForm.designerDirectives.spFieldTitle'));
    beforeEach(module('editForm/directives/spFieldTitle/spFieldTitle.tpl.html'));
    beforeEach(module('mod.app.editForm.designerDirectives.spCustomValidationMessage'));
    beforeEach(module('editForm/directives/spCustomValidationMessage/spCustomValidationMessage.tpl.html'));
    
    var dummyFormControl = spEntity.fromJSON(
                {
                    isMandatoryForForm: false,
                    mandatoryControl: false,
                    fieldToRender: {
                        id: 0,
                        decimalPlaces: 4,
                        minDate: jsonDate(new Date('2013-12-01')),
                        maxDate: jsonDate(new Date('2013-12-31')),
                        isRequired: true,
                        isOfType: [{
                            id: 'stringField'    // ** 'stringField' not 'dateField'
                        }]
                    }
                });
    
    var dummyFormData = {
        _field: new Date('2013-12-25').toISOString(),
        getField: function () {
            return this._field;
        },
        setField: function (id, value) {
            this._field = value;
        }
    };
    
    it('it should be possible to load date config control', inject(function ($rootScope, $compile) {
        var scope = $rootScope,
            element;

        scope.model = {};

        element = angular.element('<sp-date-config-control is-read-only="model.isReadOnly" is-in-test-mode="model.isInTestMode" form-control="model.formControl" form-data="model.formData"></sp-date-config-control>');
        $compile(element)(scope);
        scope.$digest();
    }));

    it('it should be possible to locate visual controls in date config control', inject(function ($rootScope, $compile) {
        var scope = $rootScope,
            element,
            chkbox,
            datectrl,
            titleMarker;

        scope.model = {};

        element = angular.element('<sp-date-config-control  form-control="model.formControl" form-data="model.formData"></sp-date-config-control>');
        $compile(element)(scope);
        scope.$digest();
        
        chkbox = element.find('input')[0];                      // Today checkbox
        datectrl = element.find('sp-date-control')[0];          // date control
        titleMarker = element.find('sp-title-plus-markers')[0]; // error marker

        expect(chkbox).toBeDefined();
        expect(datectrl).toBeDefined();
        expect(titleMarker).toBeDefined();
    }));

    it('it should be possible to set a valid date', inject(function ($rootScope, $compile) {
        var scope = $rootScope,
            element;

        scope.model = {};
        scope.model.formControl = dummyFormControl;
        scope.model.formData = dummyFormData;
        var dt = new Date(dummyFormData.getField());

        element = angular.element('<sp-date-config-control  form-control="model.formControl" form-data="model.formData"></sp-date-config-control>');
        $compile(element)(scope);
        scope.$digest();


        expect(scope.$$childTail.model.value.getTime() === spUtils.translateToLocal(dt).getTime()).toBe(true);
    }));
});

