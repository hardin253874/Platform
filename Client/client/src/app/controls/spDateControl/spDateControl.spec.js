// Copyright 2011-2016 Global Software Innovation Pty Ltd
/* globals xit */
/**
 * Tests sit right alongside the file they are testing, which is more intuitive
 * and portable than separating `src` and `test` directories. Additionally, the
 * build process will exclude all `.spec.js` files from the build
 * automatically.
 */
describe('Console|controls|spec:|spDatecontrol directive', function () {
    beforeEach(module('app.demoDialog'));
    
    // Load the modules
    beforeEach(module('app.controls.spDateControl'));
    beforeEach(module('controls/spDateControl/spDateControl.tpl.html'));
    beforeEach(module('app.controls.providers'));
    
    it('it should be possible to load date control', inject(function ($rootScope, $compile) {
        var scope = $rootScope,
            element;
        
        scope.model = {};

        element = angular.element('<sp-date-control model="model"/>');
        $compile(element)(scope);
        scope.$digest();
    }));

    it('it should be possible to locate inputbox in date control', inject(function ($rootScope, $compile) {
        var scope = $rootScope,
            element,
            input;

        scope.model = {};

        element = angular.element('<sp-date-control model="model"/>');
        $compile(element)(scope);
        scope.$digest();
        
        // find inputbox
        input = element.find('input')[0];
        expect(input).toBeDefined();
    }));
    
    it('it should be possible to set a valid date', inject(function ($rootScope, $compile) {
        var scope = $rootScope,
            element;

        var dt = new Date(1965, 0, 1, 0, 0, 0, 0);

        scope.model = { value: dt };

        element = angular.element('<sp-date-control model="model"/>');
        $compile(element)(scope);
        scope.$digest();

        
        expect(scope.$$childTail.internalModel.value.getTime() === dt.getTime());
    }));
    
    // DISABLED: This test no longer works in Jasmine 1.31
    xit('it should be possible to display the date in valid format', inject(function ($rootScope, $compile) {
        var scope = $rootScope,
            element,
            input;

        var dt = new Date(1965, 0, 1, 0, 0, 0, 0);

        scope.model = { value: dt };

        element = angular.element('<sp-date-control model="model"/>');
        $compile(element)(scope);
        scope.$digest();

        // find inputbox
        input = element.find('input')[0];
        expect(input).toBeDefined();
        
        expect(input.value).toBe(Globalize.format(dt, 'd'));
    }));
    
    it('entering invalid date in the input box should not update model value.', inject(function ($rootScope, $compile, $sniffer) {
        var scope = $rootScope,
            element,
            inputEle,
            input;

        var dt = new Date(1965, 0, 1, 0, 0, 0, 0);

        scope.model = { value: dt };

        element = angular.element('<sp-date-control model="model"/>');
        $compile(element)(scope);
        scope.$digest();
        
        // find inputbox
        inputEle = element.find('input')[0];
        expect(inputEle).toBeDefined(); // use 'angular.element()' to get jquery object. using just 'inputs[0]' will get underlying input element only.
        
        input = angular.element(inputEle);
        
        input.val('invalidDate');  // set new date to some invalid date

        input.trigger($sniffer.hasEvent('input') ? 'input' : 'change'); // fire 'change' event of input
        
        scope.$digest();
        

        // check that 'model.value' is unchanged
        expect(scope.$$childTail.model.value.getTime() === dt.getTime());
        
        // check that 'internalModel.value' is null
        expect(scope.$$childTail.internalModel.value === null);
        
        // valid value flag is set to false
        expect(scope.$$childTail.model.isValidValue === false);

        // invalid date format error message is selected
        expect(scope.$$childTail.validationModel.message === spUtils.invalidDateMsgText());
    }));
});

