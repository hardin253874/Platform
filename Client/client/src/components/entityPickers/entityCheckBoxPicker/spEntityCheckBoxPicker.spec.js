// Copyright 2011-2016 Global Software Innovation Pty Ltd
/* globals entityTestData */
/**
 * Tests sit right alongside the file they are testing, which is more intuitive
 * and portable than separating `src` and `test` directories. Additionally, the
 * build process will exclude all `.spec.js` files from the build
 * automatically.
 */
describe('Console|Pickers|spEntityCheckBoxPicker|spec:|spEntityCheckBoxPicker directive', function () {
    'use strict';

    // Load the modules    
    beforeEach(module('mod.common.ui.spEntityMultiComboPicker'));
    beforeEach(module('mod.common.ui.spEntityCheckBoxPicker'));
    beforeEach(module('entityPickers/entityCheckBoxPicker/spEntityCheckBoxPicker.tpl.html'));
    beforeEach(module('mockedEntityService'));

    // Set the mocked data
    beforeEach(inject(function (spEntityService) {
        // Set the data we wish the mock to return
        spEntityService.mockGetEntitiesOfType('console:thumbnailSizeEnum', entityTestData.thumbnailSizesTestEnumData);
    }));


    it('should replace HTML element with appropriate content', inject(function ($rootScope, $compile) {
        var scope = $rootScope,
            element,
            checkBoxInputs,
            spans;

        // Load the thumbnails by setting the entity type id
        scope.thumbnailSizesPickerOptions = {
            selectedEntityIds: [],
            entityTypeId: 'console:thumbnailSizeEnum'
        };

        element = angular.element('<sp-entity-check-box-picker options="thumbnailSizesPickerOptions"></sp-entity-check-box-picker>');
        $compile(element)(scope);
        scope.$digest();

        checkBoxInputs = element.find('span :checkbox');
        expect(checkBoxInputs.length).toBe(3);

        spans = element.find('span span');
        expect(spans.length).toBe(12);

        expect(spans[0].innerText).toBe('Small');
        expect(spans[3].innerText).toBe('150 x 150 (pixels)');
        expect(spans[4].innerText).toBe('Large');
        expect(spans[7].innerText).toBe('300 x 300 (pixels)');
        expect(spans[8].innerText).toBe('Icon');
        expect(spans[11].innerText).toBe('16 x 16 (pixels)');
    }));

    // Set large thumbnail as selected item
    it('should bind selected entity', inject(function ($rootScope, $compile) {
        var scope = $rootScope,
            element,
            checkBoxInputs,
            spans;

        // Load the thumbnails by setting the entity type id
        scope.thumbnailSizesPickerOptions = {
            selectedEntityIds: [],
            entityTypeId: 'console:thumbnailSizeEnum'
        };

        element = angular.element('<sp-entity-check-box-picker options="thumbnailSizesPickerOptions"></sp-entity-check-box-picker>');
        $compile(element)(scope);
        scope.$digest();

        checkBoxInputs = element.find('span :checkbox');
        expect(checkBoxInputs.length).toBe(3);

        spans = element.find('span span');
        expect(spans.length).toBe(12);

        scope.$apply(function () {
            scope.thumbnailSizesPickerOptions.selectedEntityIds = [12522];
        });

        checkBoxInputs = element.find('span :checkbox');
        expect(checkBoxInputs.length).toBe(3);

        spans = element.find('span span');
        expect(spans.length).toBe(12);

        // Verify correct radio button is selected
        expect(checkBoxInputs[0].checked).toBe(false);
        expect(checkBoxInputs[1].checked).toBe(true);
        expect(spans[4].innerText).toBe('Large');
        expect(checkBoxInputs[2].checked).toBe(false);
    }));

    it('should update selected entity id', inject(function ($rootScope, $compile) {
        var scope = $rootScope,
            element,
            checkBoxInputs;

        // Load the thumbnails by setting the entity type id
        scope.thumbnailSizesPickerOptions = {
            selectedEntityIds: ['console:smallThumbnail'],
            entityTypeId: 'console:thumbnailSizeEnum'
        };

        element = angular.element('<sp-entity-check-box-picker options="thumbnailSizesPickerOptions"></sp-entity-check-box-picker>');
        $compile(element)(scope);
        scope.$digest();

        checkBoxInputs = element.find('span :checkbox');
        expect(checkBoxInputs.length).toBe(3);

        expect(scope.thumbnailSizesPickerOptions.selectedEntityIds.length).toBe(1);

        // Set icon thumbnail as selected
        checkBoxInputs[2].click();

        scope.$digest();

        expect(scope.thumbnailSizesPickerOptions.selectedEntityIds.length).toBe(2);
        expect(scope.thumbnailSizesPickerOptions.selectedEntityIds).toContain('console:smallThumbnail');
        expect(scope.thumbnailSizesPickerOptions.selectedEntityIds).toContain(13732);
    }));

    it('should update selected entity', inject(function ($rootScope, $compile) {
        var scope = $rootScope,
            element,
            checkBoxInputs;

        // Load the thumbnails by setting the entity type id
        scope.thumbnailSizesPickerOptions = {
            selectedEntityIds: ['console:smallThumbnail'],
            selectedEntities: [],
            entityTypeId: 'console:thumbnailSizeEnum'
        };

        element = angular.element('<sp-entity-check-box-picker options="thumbnailSizesPickerOptions"></sp-entity-check-box-picker>');
        $compile(element)(scope);
        scope.$digest();

        checkBoxInputs = element.find('span :checkbox');
        expect(checkBoxInputs.length).toBe(3);

        expect(scope.thumbnailSizesPickerOptions.selectedEntityIds.length).toBe(1);

        // Set icon thumbnail as selected
        checkBoxInputs[2].click();

        scope.$digest();

        expect(scope.thumbnailSizesPickerOptions.selectedEntityIds.length).toBe(2);
        expect(scope.thumbnailSizesPickerOptions.selectedEntityIds).toContain('console:smallThumbnail');
        expect(scope.thumbnailSizesPickerOptions.selectedEntityIds).toContain(13732);

        expect(scope.thumbnailSizesPickerOptions.selectedEntities.length).toBe(2);
        expect(scope.thumbnailSizesPickerOptions.selectedEntities.map(function (e) { return e.id(); })).toContain(12160);
        expect(scope.thumbnailSizesPickerOptions.selectedEntities.map(function (e) { return e.id(); })).toContain(13732);
    }));

    it('should update selected entity id on entity type changes', inject(function ($rootScope, $compile) {
        var scope = $rootScope,
            element,
            checkBoxInputs;

        // Load the thumbnails by setting the entity type id
        scope.thumbnailSizesPickerOptions = {
            selectedEntityIds: ['console:smallThumbnail'],
            entityTypeId: 'console:thumbnailSizeEnum'
        };

        element = angular.element('<sp-entity-check-box-picker options="thumbnailSizesPickerOptions"></sp-entity-check-box-picker>');
        $compile(element)(scope);
        scope.$digest();

        checkBoxInputs = element.find('span :checkbox');
        expect(checkBoxInputs.length).toBe(3);

        expect(scope.thumbnailSizesPickerOptions.selectedEntityIds.length).toBe(1);

        // Set icon thumbnail as selected
        checkBoxInputs[2].click();

        scope.$digest();

        expect(scope.thumbnailSizesPickerOptions.selectedEntityIds.length).toBe(2);
        expect(scope.thumbnailSizesPickerOptions.selectedEntityIds).toContain('console:smallThumbnail');
        expect(scope.thumbnailSizesPickerOptions.selectedEntityIds).toContain(13732);

        // Clear the entity type
        scope.$apply(function () {
            scope.thumbnailSizesPickerOptions.entityTypeId = 0;
        });

        scope.$digest();

        expect(scope.thumbnailSizesPickerOptions.selectedEntityIds.length).toBe(0);
    }));

    it('should update selected entity on entity type changes', inject(function ($rootScope, $compile) {
        var scope = $rootScope,
            element,
            checkBoxInputs;

        // Load the thumbnails by setting the entity type id
        scope.thumbnailSizesPickerOptions = {
            selectedEntityIds: ['console:smallThumbnail'],
            selectedEntities: [],
            entityTypeId: 'console:thumbnailSizeEnum'
        };

        element = angular.element('<sp-entity-check-box-picker options="thumbnailSizesPickerOptions"></sp-entity-check-box-picker>');
        $compile(element)(scope);
        scope.$digest();

        checkBoxInputs = element.find('span :checkbox');
        expect(checkBoxInputs.length).toBe(3);

        expect(scope.thumbnailSizesPickerOptions.selectedEntityIds.length).toBe(1);

        // Set icon thumbnail as selected
        checkBoxInputs[2].click();

        scope.$digest();

        expect(scope.thumbnailSizesPickerOptions.selectedEntityIds.length).toBe(2);
        expect(scope.thumbnailSizesPickerOptions.selectedEntityIds).toContain('console:smallThumbnail');
        expect(scope.thumbnailSizesPickerOptions.selectedEntityIds).toContain(13732);

        expect(scope.thumbnailSizesPickerOptions.selectedEntities.length).toBe(2);
        expect(scope.thumbnailSizesPickerOptions.selectedEntities.map(function (e) { return e.id(); })).toContain(12160);
        expect(scope.thumbnailSizesPickerOptions.selectedEntities.map(function (e) { return e.id(); })).toContain(13732);

        // Clear the entity type
        scope.$apply(function () {
            scope.thumbnailSizesPickerOptions.entityTypeId = 0;
        });

        scope.$digest();

        expect(scope.thumbnailSizesPickerOptions.selectedEntityIds.length).toBe(0);
        expect(scope.thumbnailSizesPickerOptions.selectedEntities.length).toBe(0);
    }));

    it('should not render hidden aliases', inject(function ($rootScope, $compile) {
        var scope = $rootScope,
            element,
            checkBoxInputs,
            spans;

        // Load the thumbnails by setting the entity type id
        scope.thumbnailSizesPickerOptions = {
            selectedEntityIds: [],
            entityTypeId: 'console:thumbnailSizeEnum',
            hiddenAliases: ['console:largeThumbnail']
        };

        element = angular.element('<sp-entity-check-box-picker options="thumbnailSizesPickerOptions"></sp-entity-check-box-picker>');
        $compile(element)(scope);
        scope.$digest();

        checkBoxInputs = element.find('span :checkbox');
        expect(checkBoxInputs.length).toBe(2);

        spans = element.find('span span');
        expect(spans.length).toBe(8);

        expect(spans[0].innerText).toBe('Small');
        expect(spans[3].innerText).toBe('150 x 150 (pixels)');
        expect(spans[4].innerText).toBe('Icon');
        expect(spans[7].innerText).toBe('16 x 16 (pixels)');
    }));

    it('should replace HTML element with appropriate entities content', inject(function ($rootScope, $compile, spEntityService) {
        var scope = $rootScope,
            mockedEntityService,
            element,
            checkBoxInputs,
            spans;

        // Load the thumbnails by setting the entities
        scope.thumbnailSizesPickerOptions = {
            selectedEntityIds: [],
            entities: []
        };

        spEntityService.getEntitiesOfType('console:thumbnailSizeEnum', null, null).then(function (items) {
            scope.thumbnailSizesPickerOptions.entities = items;
        });

        element = angular.element('<sp-entity-check-box-picker options="thumbnailSizesPickerOptions"></sp-entity-check-box-picker>');
        $compile(element)(scope);
        scope.$digest();

        checkBoxInputs = element.find('span :checkbox');
        expect(checkBoxInputs.length).toBe(3);

        spans = element.find('span span');
        expect(spans.length).toBe(12);

        expect(spans[0].innerText).toBe('Small');
        expect(spans[3].innerText).toBe('150 x 150 (pixels)');
        expect(spans[4].innerText).toBe('Large');
        expect(spans[7].innerText).toBe('300 x 300 (pixels)');
        expect(spans[8].innerText).toBe('Icon');
        expect(spans[11].innerText).toBe('16 x 16 (pixels)');
    }));
});