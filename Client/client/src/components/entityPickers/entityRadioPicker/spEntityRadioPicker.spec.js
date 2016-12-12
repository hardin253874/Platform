// Copyright 2011-2016 Global Software Innovation Pty Ltd
/* globals entityTestData */
/**
 * Tests sit right alongside the file they are testing, which is more intuitive
 * and portable than separating `src` and `test` directories. Additionally, the
 * build process will exclude all `.spec.js` files from the build
 * automatically.
 */
describe('Console|Pickers|spEntityRadioPicker|spec:|spEntityRadioPicker directive', function () {
    'use strict';

    // Load the modules
    beforeEach(module('mod.common.spWebService'));
    beforeEach(module('mod.common.ui.spEntityRadioPicker'));
    beforeEach(module('entityPickers/entityRadioPicker/spEntityRadioPicker.tpl.html'));
    beforeEach(module('mockedEntityService'));

    // Set the mocked data
    beforeEach(inject(function (spEntityService) {
        // Set the data we wish the mock to return
        spEntityService.mockGetInstancesOfTypeRawData('console:thumbnailSizeEnum', entityTestData.thumbnailSizesTestData);
    }));


    it('should replace HTML element with appropriate content', inject(function ($rootScope, $compile) {
        var scope = $rootScope,
            element,
            radioInputs,
            divs;

        // Load the thumbnails by setting the entity type id
        scope.thumbnailSizesPickerOptions = {
            selectedEntityId: 0,
            entityTypeId: 'console:thumbnailSizeEnum'
        };

        element = angular.element('<sp-entity-radio-picker options="thumbnailSizesPickerOptions"></sp-entity-radio-picker>');
        $compile(element)(scope);
        scope.$digest();

        radioInputs = element.find('span :radio');
        expect(radioInputs.length).toBe(3);

        divs = element.find('span div.entityRadioLabels div');
        expect(divs.length).toBe(6);

        expect(divs[0].innerText).toBe('Small');
        expect(divs[1].innerText).toBe('150 x 150 (pixels)');
        expect(divs[2].innerText).toBe('Large');
        expect(divs[3].innerText).toBe('300 x 300 (pixels)');
        expect(divs[4].innerText).toBe('Icon');
        expect(divs[5].innerText).toBe('16 x 16 (pixels)');
    }));

    // Set large thumbnail as selected radio button
    it('should bind selected entity', inject(function ($rootScope, $compile) {
        var scope = $rootScope,
            element,
            radioInputs,
            divs;

        // Load the thumbnails by setting the entity type id
        scope.thumbnailSizesPickerOptions = {
            selectedEntityId: 0,
            entityTypeId: 'console:thumbnailSizeEnum'
        };

        element = angular.element('<sp-entity-radio-picker options="thumbnailSizesPickerOptions"></sp-entity-radio-picker>');
        $compile(element)(scope);
        scope.$digest();

        radioInputs = element.find('span :radio');
        expect(radioInputs.length).toBe(3);

        divs = element.find('span div.entityRadioLabels div');
        expect(divs.length).toBe(6);

        scope.$apply(function () {
            scope.thumbnailSizesPickerOptions.selectedEntityId = 12522;
        });

        radioInputs = element.find('span :radio');
        expect(radioInputs.length).toBe(3);

        divs = element.find('span div.entityRadioLabels div');
        expect(divs.length).toBe(6);

        // Verify correct radio button is selected
        expect(radioInputs[0].checked).toBe(false);
        expect(radioInputs[1].checked).toBe(true);
        expect(divs[2].innerText).toBe('Large');
        expect(radioInputs[2].checked).toBe(false);
    }));

    it('should update selected entity id', inject(function ($rootScope, $compile) {
        var scope = $rootScope,
            element,
            radioInputs;

        // Load the thumbnails by setting the entity type id
        scope.thumbnailSizesPickerOptions = {
            selectedEntityId: 0,
            entityTypeId: 'console:thumbnailSizeEnum'
        };

        element = angular.element('<sp-entity-radio-picker options="thumbnailSizesPickerOptions"></sp-entity-radio-picker>');
        $compile(element)(scope);
        scope.$digest();

        radioInputs = element.find('span :radio');
        expect(radioInputs.length).toBe(3);

        scope.thumbnailSizesPickerOptions.selectedEntityId = 0;
        expect(scope.thumbnailSizesPickerOptions.selectedEntityId).toBe(0);

        // Set icon thumbnail as selected radio button
        scope.$digest();

        radioInputs[2].click();

        expect(scope.thumbnailSizesPickerOptions.selectedEntityId).toBe(13732);
    }));

    it('should update selected entity id on entity type changes', inject(function ($rootScope, $compile) {
        var scope = $rootScope,
            element,
            radioInputs;

        // Load the thumbnails by setting the entity type id
        scope.thumbnailSizesPickerOptions = {
            selectedEntityId: 0,
            entityTypeId: 'console:thumbnailSizeEnum'
        };

        element = angular.element('<sp-entity-radio-picker options="thumbnailSizesPickerOptions"></sp-entity-radio-picker>');
        $compile(element)(scope);
        scope.$digest();

        radioInputs = element.find('span :radio');
        expect(radioInputs.length).toBe(3);

        scope.thumbnailSizesPickerOptions.selectedEntityId = 0;
        expect(scope.thumbnailSizesPickerOptions.selectedEntityId).toBe(0);

        // Set icon thumbnail as selected radio button
        scope.$digest();

        radioInputs[2].click();

        expect(scope.thumbnailSizesPickerOptions.selectedEntityId).toBe(13732);

        // Clear the entity type
        scope.$apply(function () {
            scope.thumbnailSizesPickerOptions.entityTypeId = 0;
        });

        scope.$digest();

        expect(scope.thumbnailSizesPickerOptions.selectedEntityId).toBe(0);
    }));

    it('should update select entity on entity type changes', inject(function ($rootScope, $compile) {
        var scope = $rootScope,
            element,
            radioInputs;

        // Load the thumbnails by setting the entity type id
        scope.thumbnailSizesPickerOptions = {
            selectedEntity: null,
            selectedEntityId: 0,
            entityTypeId: 'console:thumbnailSizeEnum'
        };

        element = angular.element('<sp-entity-radio-picker options="thumbnailSizesPickerOptions"></sp-entity-radio-picker>');
        $compile(element)(scope);
        scope.$digest();

        radioInputs = element.find('span :radio');
        expect(radioInputs.length).toBe(3);

        scope.thumbnailSizesPickerOptions.selectedEntityId = 0;
        expect(scope.thumbnailSizesPickerOptions.selectedEntityId).toBe(0);

        // Set icon thumbnail as selected radio button
        scope.$digest();

        radioInputs[2].click();

        expect(scope.thumbnailSizesPickerOptions.selectedEntityId).toBe(13732);
        expect(scope.thumbnailSizesPickerOptions.selectedEntity.id()).toBe(13732);

        // Clear the entity type
        scope.$apply(function () {
            scope.thumbnailSizesPickerOptions.entityTypeId = 0;
        });

        scope.$digest();

        expect(scope.thumbnailSizesPickerOptions.selectedEntityId).toBe(0);
        expect(scope.thumbnailSizesPickerOptions.selectedEntity).toBe(null);
    }));

    it('should update selected entity', inject(function ($rootScope, $compile) {
        var scope = $rootScope,
            element,
            radioInputs;

        // Load the thumbnails by setting the entity type id
        scope.thumbnailSizesPickerOptions = {
            selectedEntityId: 0,
            selectedEntity: null,
            entityTypeId: 'console:thumbnailSizeEnum'
        };

        element = angular.element('<sp-entity-radio-picker options="thumbnailSizesPickerOptions"></sp-entity-radio-picker>');
        $compile(element)(scope);
        scope.$digest();

        radioInputs = element.find('span :radio');
        expect(radioInputs.length).toBe(3);

        scope.thumbnailSizesPickerOptions.selectedEntityId = 0;
        expect(scope.thumbnailSizesPickerOptions.selectedEntityId).toBe(0);

        // Set icon thumbnail as selected radio button
        scope.$digest();

        radioInputs[2].click();

        expect(scope.thumbnailSizesPickerOptions.selectedEntityId).toBe(13732);
        expect(scope.thumbnailSizesPickerOptions.selectedEntity.id()).toBe(13732);
    }));

    it('should not render hidden aliases', inject(function ($rootScope, $compile) {
        var scope = $rootScope,
            element,
            radioInputs,
            divs;

        // Load the thumbnails by setting the entity type id
        scope.thumbnailSizesPickerOptions = {
            selectedEntityId: 0,
            entityTypeId: 'console:thumbnailSizeEnum',
            hiddenAliases: ['console:largeThumbnail']
        };

        element = angular.element('<sp-entity-radio-picker options="thumbnailSizesPickerOptions"></sp-entity-radio-picker>');
        $compile(element)(scope);
        scope.$digest();

        radioInputs = element.find('span :radio');
        expect(radioInputs.length).toBe(2);

        divs = element.find('span div.entityRadioLabels div');
        expect(divs.length).toBe(4);

        expect(divs[0].innerText).toBe('Small');
        expect(divs[1].innerText).toBe('150 x 150 (pixels)');
        expect(divs[2].innerText).toBe('Icon');
        expect(divs[3].innerText).toBe('16 x 16 (pixels)');
    }));

    it('should replace HTML element with appropriate entities content', inject(function ($rootScope, $compile, spEntityService) {
        var scope = $rootScope,
            mockedEntityService,
            element,
            radioInputs,
            divs;

        // Load the thumbnails by setting the entities        
        scope.thumbnailSizesPickerOptions = {
            selectedEntityId: 0,
            entities: []
        };

        spEntityService.getInstancesOfType('console:thumbnailSizeEnum', null, null).then(function (items) {
            scope.thumbnailSizesPickerOptions.entities = _.map(items, function (i) {
                return i.entity;
            });
        });

        element = angular.element('<sp-entity-radio-picker options="thumbnailSizesPickerOptions"></sp-entity-radio-picker>');
        $compile(element)(scope);
        scope.$digest();

        radioInputs = element.find('span :radio');
        expect(radioInputs.length).toBe(3);

        divs = element.find('span div.entityRadioLabels div');
        expect(divs.length).toBe(6);

        expect(divs[0].innerText).toBe('Small');
        expect(divs[1].innerText).toBe('150 x 150 (pixels)');
        expect(divs[2].innerText).toBe('Large');
        expect(divs[3].innerText).toBe('300 x 300 (pixels)');
        expect(divs[4].innerText).toBe('Icon');
        expect(divs[5].innerText).toBe('16 x 16 (pixels)');
    }));
});