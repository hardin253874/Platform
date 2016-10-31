// Copyright 2011-2016 Global Software Innovation Pty Ltd
/**
 * Tests sit right alongside the file they are testing, which is more intuitive
 * and portable than separating `src` and `test` directories. Additionally, the
 * build process will exclude all `.spec.js` files from the build
 * automatically.
 */
describe('Console|controls|spec:|spDateAndTimecontrol directive', function () {
    beforeEach(module('app.demoDialog'));

    // Load the modules
    beforeEach(module('app.controls.spDateAndTimeControl'));
    beforeEach(module('controls/spDateAndTimeControl/spDateAndTimeControl.tpl.html'));
    beforeEach(module('app.controls.providers'));

    it('it should be possible to load date control', inject(function ($rootScope, $compile) {
        var scope = $rootScope,
            element;

        scope.model = {};

        element = angular.element('<sp-date-and-time-control ng-model="model.value" model="model"/>');
        $compile(element)(scope);
        scope.$digest();
    }));

    it('it should be possible to set a valid date', inject(function ($rootScope, $compile) {
        var scope = $rootScope,
            element;

        var dt = new Date(1965, 0, 1, 0, 0, 0, 0);

        scope.model = { value: dt };

        element = angular.element('<sp-date-and-time-control ng-model="model.value" model="model"/>');
        $compile(element)(scope);
        scope.$digest();
        expect(scope.$$childTail.model.value.getTime() === dt.getTime()).toBe(true);
    }));

    it('the time component of date time control should be disabled until a valid date value is set', inject(function ($rootScope, $compile) {
        var scope = $rootScope,
            element;

        var dt = new Date(1965, 0, 1, 0, 0, 0, 0);
        scope.model = {};

        element = angular.element('<sp-date-and-time-control model="model"/>');
        $compile(element)(scope);
        scope.$digest();

        expect(scope.$$childTail.timeCtrlModel.disableControl).toBe(true);

        scope.model.value = dt;
        scope.$digest();

        expect(scope.$$childTail.timeCtrlModel.disableControl).toBe(false);
    }));

    //it('it should be possible to display the date in valid format', inject(function ($rootScope, $compile) {
    //    var scope = $rootScope,
    //        element,
    //        input;

    //    var dt = new Date(1965, 0, 1, 0, 0, 0, 0);

    //    scope.model = { value: dt };

    //    element = angular.element('<sp-date-and-time-control model="model"/>');
    //    $compile(element)(scope);
    //    scope.$digest();

    //    // find inputbox
    //    input = element.find('input')[0];
    //    expect(input).toBeDefined();

    //    expect(input.value).toBe(Globalize.format(dt, 'd'));
    //}));
});

