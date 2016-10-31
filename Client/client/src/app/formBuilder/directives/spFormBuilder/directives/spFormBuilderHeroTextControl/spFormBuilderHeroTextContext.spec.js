// Copyright 2011-2016 Global Software Innovation Pty Ltd
describe('Form Builder|spec|Hero Text|spFormBuilderHeroTextContext', function () {

    beforeEach(module('mod.app.formBuilder.directives.spFormBuilderHeroTextContext'));
    beforeEach(module('formBuilder/directives/spFormBuilder/directives/spFormBuilderGenericControl/spFormBuilderGenericContext.tpl.html'));

    beforeEach(inject(function ($injector) {
        TestSupport.setupUnitTests(this, $injector);
    }));

    it('should load', inject(function ($rootScope, $compile) {

        var scope = $rootScope.$new();
        scope.control = spEntity.fromJSON({});

        var element = $compile('<sp-form-builder-hero-text-context control="control"></sp-form-builder-hero-text-context>')(scope);
        scope.$digest();

        expect(element[0]).toBeTruthy();

        expect(scope.onConfigureClick).not.toBeNull();
        expect(scope.onHeroTextPropsClick).not.toBeNull();
        expect(scope.onAssignDataClick).not.toBeNull();

    }));

});
