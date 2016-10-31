// Copyright 2011-2016 Global Software Innovation Pty Ltd

describe('Custom Form Controls|spec:|spTargetCampaignSurveyTakerPicker', function () {
    var element;
    var scope;

    beforeEach(function () {
        module('mod.app.editForm.customDirectives.spTargetCampaignSurveyTakerPicker');
        module('editForm/custom/spTargetCampaignSurveyTakerPicker/spTargetCampaignSurveyTakerPicker.tpl.html');
        module('editForm/directives/spTitlePlusMarkers/spTitlePlusMarkers.tpl.html');
        module('editForm/directives/spFieldTitle/spFieldTitle.tpl.html');
        module('editForm/directives/spCustomValidationMessage/spCustomValidationMessage.tpl.html');
        module('mockedEntityService');
        element = angular.element('<sp-target-campaign-survey-taker-picker form-control="formControl" form-data="formData" />');
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
            id: 1111111
        });

        spEntityService.mockGetEntityJSON({ id: 2222222, alias: 'core:campaignTarget' });
        spEntityService.mockGetEntityJSON({ id: 3333333, alias: 'core:campaignTargetRelationship' });
        spEntityService.mockGetEntityJSON({ id: 4444444, alias: 'core:campaignTargetRelationshipDirection' });

        scope.$digest();
    }

    it('should load as empty', inject(function () {
        expect(element).toBeTruthy();

        var innerScope = scope.$$childTail;

        var model = innerScope.model;
        expect(model).toBeTruthy();
        expect(model.options).toEqual([]);
        expect(model.selectedOption).toBeNull();
        expect(model.targetTypeEntity).toBeNull();

        expect(innerScope.isMandatory).toBe(true);
        expect(innerScope.titleModel).toEqual({
            name: 'Survey taker',
            description: 'The relationship between the recipients and the object of the survey.',
            hasName: true,
            readonly: true
        });
        expect(innerScope.customValidationMessages).toEqual([]);
        expect(innerScope.relationshipToRender).toBeNull();
        expect(innerScope.relTarget).toEqual({ id: -1, isReverse: false });
        expect(innerScope.relSurveyTaker).toEqual({ id: -1, isReverse: false });
    }));

    it('should load with form control', inject(function (spEntityService) {
        setup(scope, spEntityService);

        var innerScope = scope.$$childTail;

        expect(innerScope.formControl.spValidateControl).toBeTruthy();
        expect(innerScope.relationshipToRender).toBeTruthy();
        expect(innerScope.relTarget).toEqual({ id: 2222222, isReverse: false });
        expect(innerScope.relSurveyTaker).toEqual({ id: 3333333, isReverse: false });
    })); 

    it('should work with form data', inject(function (spEntityService) {
        setup(scope, spEntityService);

        var mockTargetTypeEntity = {
            id: 5555555,
            name: 'targetTypeEntity'
        };

        var targetTypeEntity = spEntity.fromJSON(mockTargetTypeEntity);

        var selectedEntity = spEntity.fromJSON({
            id: 6666666,
            name: 'selectedEntity',
            toType: jsonLookup(targetTypeEntity)
        });

        scope.formData = {
            getRelationship: function (rel) {
                var e = [];
                switch (rel.id) {
                    case 2222222:
                        e.push(targetTypeEntity);
                        break;
                    case 3333333:
                        e.push(selectedEntity);
                        break;
                }
                return e;
            },
            getLookup: function(rel) {
                var e = null;
                switch (rel.id) {
                    case 6666666:
                        e = spEntity.fromJSON({
                            id: 8888888
                        });
                        break;
                }
                return e;
            },
            setLookup: function(rel, val) {
            }
        };

        var relationships = [];
        relationships.push(spEntity.fromJSON({
            id: 9999999,
            name: 'foundRelationship',
            toType: jsonLookup(targetTypeEntity)
        }));

        spEntityService.mockGetEntityJSON({ id: 7777777, alias: 'core:person' });
        spEntityService.mockGetEntityJSON(mockTargetTypeEntity);
        spEntityService.mockGetEntitiesOfType('core:relationship', relationships);

        scope.$digest();

        var innerScope = scope.$$childTail;

        expect(innerScope.model.targetTypeEntity.idP).toBe(5555555);
        expect(innerScope.model.selectedOption.id).toBe(6666666);
        expect(innerScope.model.options).toEqual([{id: 9999999, name: 'foundRelationship', from: undefined, to: 5555555, forward: false}]);
    }));
});