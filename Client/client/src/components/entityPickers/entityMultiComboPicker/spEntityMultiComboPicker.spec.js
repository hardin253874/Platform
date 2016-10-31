// Copyright 2011-2016 Global Software Innovation Pty Ltd
/* globals entityTestData */
describe('Console|Pickers|spEntityMultiComboPicker|spec:|spEntityMultiComboPicker directive', function () {
    'use strict';

    // Load the modules
    beforeEach(module('mod.common.ui.spEntityMultiComboPicker'));
    beforeEach(module('entityPickers/entityMultiComboPicker/spEntityMultiComboPicker.tpl.html'));
    beforeEach(module('entityPickers/entityMultiComboPicker/spEntityMultiComboPickerPopup.tpl.html'));
    beforeEach(module('mockedEntityService'));

    // Set the mocked data
    beforeEach(inject(function (spEntityService) {
        // Set the data we wish the mock to return
        spEntityService.mockGetEntitiesOfType('console:thumbnailSizeEnum', entityTestData.thumbnailSizesTestEnumData);
    }));


    it('should replace HTML element with appropriate content', inject(function ($rootScope, $compile, $timeout, $document) {
        var scope = $rootScope,
            element,
            popup,
            menuItems,
            dropdown;

        // Load the thumbnails by setting the entity type id
        scope.thumbnailSizesPickerOptions = {
            selectedEntityIds: [],
            entityTypeId: 'console:thumbnailSizeEnum'
        };

        element = angular.element('<sp-entity-multi-combo-picker options="thumbnailSizesPickerOptions"></sp-entity-multi-combo-picker>');
        $compile(element)(scope);
        scope.$digest();
        
        expect(element.hasClass('entityPickers-view')).toBe(true);

        dropdown = element.find('.dropdownButton');
        dropdown.click();

        scope.$digest();
        $timeout.flush();

        popup = $document.find('.entityMultiComboPickerDropdownPopupMenu');

        menuItems = $(popup).find('li');

        expect(menuItems.length).toBe(3);

        expect($(menuItems[0]).find('span').text()).toBe('Small');
        expect($(menuItems[1]).find('span').text()).toBe('Large');
        expect($(menuItems[2]).find('span').text()).toBe('Icon');

        popup.remove();
    }));


    // Set large thumbnail as selected item
    it('should bind selected entity', inject(function ($rootScope, $compile, $timeout, $document) {
        var scope = $rootScope,
             element,
             popup,
             menuItems,
             dropdown;

        // Load the thumbnails by setting the entity type id
        scope.thumbnailSizesPickerOptions = {
            selectedEntityIds: [],
            entityTypeId: 'console:thumbnailSizeEnum'
        };

        element = angular.element('<sp-entity-multi-combo-picker options="thumbnailSizesPickerOptions"></sp-entity-multi-combo-picker>');
        $compile(element)(scope);
        scope.$digest();

        expect(element.hasClass('entityPickers-view')).toBe(true);        

        scope.$apply(function () {
            scope.thumbnailSizesPickerOptions.selectedEntityIds = [12522];
        });        

        dropdown = element.find('.dropdownButton');
        dropdown.click();

        scope.$digest();
        $timeout.flush();

        popup = $document.find('.entityMultiComboPickerDropdownPopupMenu');

        menuItems = $(popup).find('li');

        expect(menuItems.length).toBe(3);

        // Verify checkboxes are selected
        expect($(menuItems[0]).find(':checkbox')[0].checked).toBe(false);
        expect($(menuItems[1]).find(':checkbox')[0].checked).toBe(true);
        expect($(menuItems[2]).find(':checkbox')[0].checked).toBe(false);

        popup.remove();
    }));

    it('should update selected entity id', inject(function ($rootScope, $compile, $timeout, $document) {
        var scope = $rootScope,
             element,
             popup,
             menuItems,
             dropdown;

        // Load the thumbnails by setting the entity type id
        scope.thumbnailSizesPickerOptions = {
            selectedEntityIds: ['console:smallThumbnail'],
            entityTypeId: 'console:thumbnailSizeEnum'
        };

        element = angular.element('<sp-entity-multi-combo-picker options="thumbnailSizesPickerOptions"></sp-entity-multi-combo-picker>');
        $compile(element)(scope);
        scope.$digest();

        expect(element.hasClass('entityPickers-view')).toBe(true);

        dropdown = element.find('.dropdownButton');
        dropdown.click();

        scope.$digest();
        $timeout.flush();

        popup = $document.find('.entityMultiComboPickerDropdownPopupMenu');

        menuItems = $(popup).find('li');

        expect(menuItems.length).toBe(3);
        expect(scope.thumbnailSizesPickerOptions.selectedEntityIds.length).toBe(1);

        $(menuItems[2]).find(':checkbox').click();
        scope.$digest();                

        expect(scope.thumbnailSizesPickerOptions.selectedEntityIds.length).toBe(2);
        expect(scope.thumbnailSizesPickerOptions.selectedEntityIds).toContain('console:smallThumbnail');
        expect(scope.thumbnailSizesPickerOptions.selectedEntityIds).toContain(13732);

        popup.remove();
    }));

    it('should update selected entity', inject(function ($rootScope, $compile, $timeout, $document) {
        var scope = $rootScope,
            element,
            popup,
            menuItems,
            dropdown;

        // Load the thumbnails by setting the entity type id
        scope.thumbnailSizesPickerOptions = {
            selectedEntityIds: ['console:smallThumbnail'],
            selectedEntities: [],
            entityTypeId: 'console:thumbnailSizeEnum'
        };

        element = angular.element('<sp-entity-multi-combo-picker options="thumbnailSizesPickerOptions"></sp-entity-multi-combo-picker>');
        $compile(element)(scope);
        scope.$digest();

        expect(element.hasClass('entityPickers-view')).toBe(true);

        dropdown = element.find('.dropdownButton');
        dropdown.click();

        scope.$digest();
        $timeout.flush();

        popup = $document.find('.entityMultiComboPickerDropdownPopupMenu');

        menuItems = $(popup).find('li');

        expect(menuItems.length).toBe(3);
        expect(scope.thumbnailSizesPickerOptions.selectedEntityIds.length).toBe(1);

        $(menuItems[2]).find(':checkbox').click();
        scope.$digest();        

        expect(scope.thumbnailSizesPickerOptions.selectedEntityIds.length).toBe(2);
        expect(scope.thumbnailSizesPickerOptions.selectedEntityIds).toContain('console:smallThumbnail');
        expect(scope.thumbnailSizesPickerOptions.selectedEntityIds).toContain(13732);

        expect(scope.thumbnailSizesPickerOptions.selectedEntities.length).toBe(2);
        expect(scope.thumbnailSizesPickerOptions.selectedEntities.map(function (e) { return e.id(); })).toContain(12160);
        expect(scope.thumbnailSizesPickerOptions.selectedEntities.map(function (e) { return e.id(); })).toContain(13732);

        popup.remove();
    }));

    it('should update selected entity id on entity type changes', inject(function ($rootScope, $compile, $timeout, $document) {
        var scope = $rootScope,
            element,
            popup,
            menuItems,
            dropdown;

        // Load the thumbnails by setting the entity type id
        scope.thumbnailSizesPickerOptions = {
            selectedEntityIds: ['console:smallThumbnail'],
            entityTypeId: 'console:thumbnailSizeEnum'
        };

        element = angular.element('<sp-entity-multi-combo-picker options="thumbnailSizesPickerOptions"></sp-entity-multi-combo-picker>');
        $compile(element)(scope);
        scope.$digest();

        expect(element.hasClass('entityPickers-view')).toBe(true);

        dropdown = element.find('.dropdownButton');
        dropdown.click();

        scope.$digest();
        $timeout.flush();

        popup = $document.find('.entityMultiComboPickerDropdownPopupMenu');

        menuItems = $(popup).find('li');

        expect(menuItems.length).toBe(3);

        // Set icon thumbnail as selected
        $(menuItems[2]).find(':checkbox').click();

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

        popup.remove();
    }));

    it('should update selected entity on entity type changes', inject(function ($rootScope, $compile, $timeout, $document) {
        var scope = $rootScope,
            element,
            popup,
            menuItems,
            dropdown;

        // Load the thumbnails by setting the entity type id
        scope.thumbnailSizesPickerOptions = {
            selectedEntityIds: ['console:smallThumbnail'],
            selectedEntities: [],
            entityTypeId: 'console:thumbnailSizeEnum'
        };

        element = angular.element('<sp-entity-multi-combo-picker options="thumbnailSizesPickerOptions"></sp-entity-multi-combo-picker>');
        $compile(element)(scope);
        scope.$digest();

        expect(element.hasClass('entityPickers-view')).toBe(true);

        dropdown = element.find('.dropdownButton');
        dropdown.click();

        scope.$digest();
        $timeout.flush();

        popup = $document.find('.entityMultiComboPickerDropdownPopupMenu');

        menuItems = $(popup).find('li');

        expect(menuItems.length).toBe(3);

        // Set icon thumbnail as selected
        $(menuItems[2]).find(':checkbox').click();

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

        popup.remove();
    }));

    it('should not render hidden aliases', inject(function ($rootScope, $compile, $timeout, $document) {
        var scope = $rootScope,
            element,
            popup,
            menuItems,
            dropdown;

        // Load the thumbnails by setting the entity type id
        scope.thumbnailSizesPickerOptions = {
            selectedEntityIds: [],
            entityTypeId: 'console:thumbnailSizeEnum',
            hiddenAliases: ['console:largeThumbnail']
        };

        element = angular.element('<sp-entity-multi-combo-picker options="thumbnailSizesPickerOptions"></sp-entity-multi-combo-picker>');
        $compile(element)(scope);
        scope.$digest();

        expect(element.hasClass('entityPickers-view')).toBe(true);

        dropdown = element.find('.dropdownButton');
        dropdown.click();

        scope.$digest();
        $timeout.flush();

        popup = $document.find('.entityMultiComboPickerDropdownPopupMenu');

        menuItems = $(popup).find('li');

        expect(menuItems.length).toBe(2);

        expect($(menuItems[0]).find('span').text()).toBe('Small');
        expect($(menuItems[1]).find('span').text()).toBe('Icon');        

        popup.remove();
    }));

    it('should replace HTML element with appropriate entities content', inject(function ($rootScope, $compile, spEntityService, $timeout, $document) {
        var scope = $rootScope,
            mockedEntityService,
            element,
            popup,
            menuItems,
            dropdown;

        // Load the thumbnails by setting the entities
        scope.thumbnailSizesPickerOptions = {
            selectedEntityIds: [],
            entities: []
        };

        spEntityService.getEntitiesOfType('console:thumbnailSizeEnum', null, null).then(function (items) {
            scope.thumbnailSizesPickerOptions.entities = items;
        });

        element = angular.element('<sp-entity-multi-combo-picker options="thumbnailSizesPickerOptions"></sp-entity-multi-combo-picker>');
        $compile(element)(scope);
        scope.$digest();

        expect(element.hasClass('entityPickers-view')).toBe(true);

        dropdown = element.find('.dropdownButton');
        dropdown.click();

        scope.$digest();
        $timeout.flush();

        popup = $document.find('.entityMultiComboPickerDropdownPopupMenu');

        menuItems = $(popup).find('li');

        expect(menuItems.length).toBe(3);

        expect($(menuItems[0]).find('span').text()).toBe('Small');
        expect($(menuItems[1]).find('span').text()).toBe('Large');
        expect($(menuItems[2]).find('span').text()).toBe('Icon');

        popup.remove();
    }));
});