// Copyright 2011-2016 Global Software Innovation Pty Ltd

describe('Custom Form Controls|spec:|spChoiceQuestionOptionsEditor', function () {
    var element;
    var scope;

    beforeEach(function () {
        module('mod.app.editForm.customDirectives.spChoiceQuestionOptionsEditor');
        module('editForm/custom/spChoiceQuestionOptionsEditor/spChoiceQuestionOptionsEditor.tpl.html');
        module('mockedEntityService');
        element = angular.element('<sp-choice-question-options-editor form-control="formControl" form-data="formData" />');
        inject(function ($rootScope, $compile) {
            scope = $rootScope.$new();

            scope.formControl = null;
            scope.formData = null;

            $compile(element)(scope);
            scope.$digest();
        });
    });

    function setup(scope, spEntityService) {
        scope.formControl = spEntity.fromJSON({
            id: 1111111,
            relationshipToRender: jsonLookup('2222222')
        });

        spEntityService.mockGetEntityJSON({ id: 3333333, alias: 'core:choiceQuestionChoiceSet' });
        spEntityService.mockGetEntityJSON({ id: 2222222, name: 'target' });

        scope.$digest();
    }

    it('should load as empty', inject(function () {
        expect(element).toBeTruthy();

        var innerScope = scope.$$childTail;

        var model = innerScope.model;
        expect(model).toBeTruthy();
        expect(model.choiceOptions).toEqual([]);
        expect(model.choiceOptionSet).toBeNull();
        expect(model.choiceOptionGridOptions).toBeTruthy();
        expect(model.choiceOptionPickerOptions).toBeTruthy();

        expect(innerScope.isMobile).toBe(false);
        expect(innerScope.isExisting).toBe(false);
        expect(innerScope.isLegacy).toBe(false);
        expect(innerScope.canModify).toBe(false);
        expect(innerScope.canDelete).toBe(false);
        expect(innerScope.gridLoaded).toBe(false);
        expect(innerScope.relationshipToRender).toBeNull();
    }));

    it('should load with form control', inject(function (spEntityService) {
        setup(scope, spEntityService);

        var innerScope = scope.$$childTail;

        expect(innerScope.relationshipToRender).toBeTruthy();
        expect(innerScope.relationshipToRender.idP).toBe(2222222);
    }));

    it('should work with form data', inject(function(spEntityService) {
        setup(scope, spEntityService);

        var mockChoiceSetEntity = {
            id: 4444444,
            canModify: true,
            canDelete: true
        };

        var choiceSetEntity = spEntity.fromJSON(mockChoiceSetEntity);

        scope.formData = spEntity.fromJSON(choiceSetEntity);

        spEntityService.mockGetEntityJSON(mockChoiceSetEntity);

        scope.$digest();

        var innerScope = scope.$$childTail;

        expect(innerScope.canModify).toBe(true);
        expect(innerScope.canDelete).toBe(true);
        expect(innerScope.model.choiceOptionSet).toBeTruthy();
        expect(innerScope.model.choiceOptionPickerOptions).toBeTruthy();
        expect(innerScope.model.choiceOptionPickerOptions.selectedEntities).toEqual([innerScope.model.choiceOptionSet]);
        expect(innerScope.model.choiceOptions).toBeTruthy();
    }));
});