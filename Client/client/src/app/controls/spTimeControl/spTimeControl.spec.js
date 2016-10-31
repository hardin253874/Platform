// Copyright 2011-2016 Global Software Innovation Pty Ltd
/**
 * Tests sit right alongside the file they are testing, which is more intuitive
 * and portable than separating `src` and `test` directories. Additionally, the
 * build process will exclude all `.spec.js` files from the build
 * automatically.
 */
describe('Console|controls|spec:|spTimecontrol directive', function () {
    beforeEach(module('app.demoDialog'));

    // Load the modules
    beforeEach(module('app.controls.spTimeControl'));
    beforeEach(module('controls/spTimeControl/spTimeControl.tpl.html'));
    beforeEach(module('app.controls.providers'));

    it('it should be possible to load time control', inject(function ($rootScope, $compile) {
        var scope = $rootScope,
            element;

        scope.model = {};

        element = angular.element('<sp-time-control model="model"/>');
        $compile(element)(scope);
        scope.$digest();
    }));

    it('it should be possible to set a valid time', inject(function ($rootScope, $compile) {
        var scope = $rootScope,
            element;

        var dt = new Date(1975, 0, 1, 0, 0, 0, 0);

        scope.model = { value: dt };

        element = angular.element('<sp-time-control model="model"/>');
        $compile(element)(scope);
        scope.$digest();

        expect(scope.$$childTail.internalModel.value.getTime() === spUtils.translateFromServerStorageDateTime(dt).getTime()).toBe(true);
    }));
    
    it('selecting a valid value from time control should return a valid server storage datetime value', inject(function ($rootScope, $compile) {
        var scope = $rootScope,
            element;

        var dt = new Date(1975, 0, 1, 0, 0, 0, 0);

        scope.model = { value: dt };

        element = angular.element('<sp-time-control model="model"/>');
        $compile(element)(scope);
        scope.$digest();

        expect(scope.$$childTail.internalModel.value.getTime() === spUtils.translateFromServerStorageDateTime(dt).getTime()).toBe(true);
        
        // set internal model value to a valid local time value
        var internalDt = new Date(1975, 0, 1, 11, 30, 0, 0);
        scope.$$childTail.internalModel.value = internalDt;
        
        scope.$digest();
        
        expect(scope.model.value.getTime() === spUtils.translateToServerStorageDateTime(internalDt).getTime()).toBe(true);
        
    }));

    it('pushing null value to time control should reset hour and minute input values', inject(function ($rootScope, $compile) {
        var scope = $rootScope,
            element;

        var dt = new Date(1975, 0, 1, 0, 0, 0, 0);  // initial value

        scope.model = { value: dt };

        element = angular.element('<sp-time-control model="model"/>');
        $compile(element)(scope);
        scope.$digest();

        expect(scope.$$childTail.internalModel.value.getTime() === spUtils.translateFromServerStorageDateTime(dt).getTime()).toBe(true);

        // set value to null
        scope.model.value = null;

        scope.$digest();

        expect(scope.$$childTail.internalModel.value === null).toBe(true);

    }));
    
    it('selecting an invalid value from time control should not change the model value', inject(function ($rootScope, $compile) {
        var scope = $rootScope,
            element;

        var dt = new Date(1975, 0, 1, 0, 0, 0, 0);

        scope.model = { value: dt };

        element = angular.element('<sp-time-control model="model"/>');
        $compile(element)(scope);
        scope.$digest();

        expect(scope.$$childTail.internalModel.value.getTime() === spUtils.translateFromServerStorageDateTime(dt).getTime()).toBe(true);

        // set internal model value to an invalid string
        scope.$$childTail.internalModel.value = 'invalidValue';
        scope.model.isValidValue = false;   // set this flag to false as internalModel.value is onlyu updated by the spBootstrapTimePicker

        try {
            scope.$digest();
        } catch (e) {
            
        }
        
        expect(scope.model.value.getTime() === dt.getTime()).toBe(true);

    }));
    
});

