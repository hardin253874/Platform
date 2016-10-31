// Copyright 2011-2016 Global Software Innovation Pty Ltd

describe('Custom Form Controls|spec:|spSurveySectionsEditor', function () {
    var element;
    var scope;

    beforeEach(function () {
        module('mod.app.editForm.customDirectives.spSurveySectionsEditor');
        module('editForm/custom/spSurveySectionsEditor/spSurveySectionsEditor.tpl.html');
        module('mockedEntityService');
        element = angular.element('<sp-survey-sections-editor form-control="formControl" form-data="formData" />');
        inject(function ($rootScope, $compile) {
            scope = $rootScope.$new();

            scope.formControl = null;
            scope.formData = null;

            $compile(element)(scope);
            scope.$digest();
        });
    });

    it('should load as empty', inject(function () {
        expect(element).toBeTruthy();

        var innerScope = scope.$$childTail;

        var model = innerScope.model;
        expect(model).toBeTruthy();
        expect(model.sections).toEqual([]);
        expect(model.showContextMenu).toBe(false);
        expect(model.contextMenu).toBeTruthy();
        expect(model.dropOptions).toBeTruthy();
        expect(model.dragOptions).toBeTruthy();

        expect(innerScope.isMobile).toBe(false);
        expect(innerScope.canModify).toBe(false);
        expect(innerScope.canDelete).toBe(false);
        expect(innerScope.canCreateSections).toBe(false);
        expect(innerScope.canCreateQuestions).toBe(false);
        expect(innerScope.relationshipToRender).toBeNull();
    }));
});