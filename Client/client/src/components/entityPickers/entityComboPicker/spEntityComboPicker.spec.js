// Copyright 2011-2016 Global Software Innovation Pty Ltd
/* globals entityTestData */
/**
 * Tests sit right alongside the file they are testing, which is more intuitive
 * and portable than separating `src` and `test` directories. Additionally, the
 * build process will exclude all `.spec.js` files from the build
 * automatically.
 */
describe('Console|Pickers|spEntityComboBoxPicker|spec:|spEntityComboBoxPicker directive', function () {
    'use strict';

    // Load the modules
    beforeEach(module('mod.common.ui.spEntityComboPicker'));
    beforeEach(module('entityPickers/entityComboPicker/spEntityComboPicker.tpl.html'));
    beforeEach(module('mockedEntityService'));

    // Set the mocked data
    beforeEach(inject(function (spEntityService) {
        // Set the data we wish the mock to return
        spEntityService.mockGetInstancesOfTypeRawData('console:thumbnailSizeEnum', entityTestData.thumbnailSizesTestData);
    }));


    it('should replace HTML element with appropriate content', inject(function ($rootScope, $compile) {
        var scope = $rootScope,
            element,
            selectElements,
            options;

        // Load the thumbnails by setting the entity type id
        scope.thumbnailSizesPickerOptions = {
            selectedEntityId: 0,
            entityTypeId: 'console:thumbnailSizeEnum'
        };

        element = angular.element('<sp-entity-combo-picker options="thumbnailSizesPickerOptions"></sp-entity-combo-picker>');
        $compile(element)(scope);
        scope.$digest();

        selectElements = element.find('span select');
        expect(selectElements.length).toBe(1);

        options = element.find('span select option');
        expect(options.length).toBe(4);

        expect(options[0].innerText).toBe('[Select]');
        expect(options[1].innerText).toBe('Small');
        expect(options[2].innerText).toBe('Large');
        expect(options[3].innerText).toBe('Icon');
    }));

    // Set large thumbnail as selected item
    it('should bind selected entity', inject(function ($rootScope, $compile) {
        var scope = $rootScope,
            element,
            selectElements,
            options;

        // Load the thumbnails by setting the entity type id
        scope.thumbnailSizesPickerOptions = {
            selectedEntityId: 0,
            entityTypeId: 'console:thumbnailSizeEnum'
        };

        element = angular.element('<sp-entity-combo-picker options="thumbnailSizesPickerOptions"></sp-entity-combo-picker>');
        $compile(element)(scope);
        scope.$digest();

        selectElements = element.find('span select');
        expect(selectElements.length).toBe(1);

        options = element.find('span select option');
        expect(options.length).toBe(4);

        scope.$apply(function () {
            scope.thumbnailSizesPickerOptions.selectedEntityId = 12522;
        });

        selectElements = element.find('span select');
        expect(selectElements.length).toBe(1);

        options = element.find('span select option');
        expect(options.length).toBe(4);

        // Verify correct option button is selected
        expect(options[0].selected).toBe(false);
        expect(options[1].selected).toBe(false);
        expect(options[2].selected).toBe(true);
        expect(options[3].selected).toBe(false);
    }));

    it('should not render hidden aliases', inject(function ($rootScope, $compile) {
        var scope = $rootScope,
            element,
            selectElements,
            options;

        // Load the thumbnails by setting the entity type id
        scope.thumbnailSizesPickerOptions = {
            selectedEntityId: 0,
            entityTypeId: 'console:thumbnailSizeEnum',
            hiddenAliases: ['console:largeThumbnail']
        };

        element = angular.element('<sp-entity-combo-picker options="thumbnailSizesPickerOptions"></sp-entity-combo-picker>');
        $compile(element)(scope);
        scope.$digest();

        selectElements = element.find('span select');
        expect(selectElements.length).toBe(1);

        options = element.find('span select option');
        expect(options.length).toBe(3);

        expect(options[0].innerText).toBe('[Select]');
        expect(options[1].innerText).toBe('Small');
        expect(options[2].innerText).toBe('Icon');
    }));

    it('should replace HTML element with appropriate entities content', inject(function ($rootScope, $compile, spEntityService) {
        var scope = $rootScope,
            mockedEntityService,
            element,
            selectElements,
            options;

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

        element = angular.element('<sp-entity-combo-picker options="thumbnailSizesPickerOptions"></sp-entity-combo-picker>');
        $compile(element)(scope);
        scope.$digest();

        selectElements = element.find('span select');
        expect(selectElements.length).toBe(1);

        options = element.find('span select option');
        expect(options.length).toBe(4);

        expect(options[0].innerText).toBe('[Select]');
        expect(options[1].innerText).toBe('Small');
        expect(options[2].innerText).toBe('Large');
        expect(options[3].innerText).toBe('Icon');
    }));
});