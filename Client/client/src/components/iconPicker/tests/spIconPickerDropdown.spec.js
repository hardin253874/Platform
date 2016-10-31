// Copyright 2011-2016 Global Software Innovation Pty Ltd
describe('Console|Controls|IconPickers|spIconPickerDropdown|spec:|spIconPickerDropdown directive', function () {
    'use strict';
    var iconThumbnailSize,
        iconIds;

    // Load the modules
    beforeEach(module('mod.common.ui.spIconPickers'));    
    beforeEach(module('iconPicker/spIconPickerDropdown.tpl.html'));
    beforeEach(module('iconPicker/spIconPickerDropdownPopup.tpl.html'));
    beforeEach(module('mockedEntityService'));

    // Set the mocked data
    beforeEach(inject(function (spEntityService) {
        iconThumbnailSize = spEntity.fromJSON({
            id: { id: 11111, ns: 'console', alias: 'iconThumbnailSize' },
            "console:thumbnailWidth": '16',
            "console:thumbnailHeight": '16'
        });

        // Set the data we wish the mock to return
        spEntityService.mockGetEntity(iconThumbnailSize);

        iconIds = [
            new spEntity.EntityRef('blackCircle'),
            new spEntity.EntityRef('blackTick')];            
    }));

    it('should replace HTML element with appropriate content', inject(function ($rootScope, $compile) {
        var scope = $rootScope,
            element;
            
        scope.options = {
            iconWidth: 16,
            iconHeight: 16,
            iconSizeId: iconThumbnailSize.eid(),
            iconIds: iconIds
        };

        element = angular.element('<sp-icon-picker-dropdown options="options"></<sp-icon-picker-dropdown>');
        $compile(element)(scope);
        scope.$digest();

        // Verify that the html element has been replaced        
        expect(element.hasClass('spIconPicker-view')).toBe(true);
    }));

    it('click displays popup', inject(function ($rootScope, $compile, $templateCache, $document) {
        var scope = $rootScope,
            element,
            dropDownButton,
            popup,
            iconContainers;

        scope.options = {
            iconWidth: 16,
            iconHeight: 16,
            iconSizeId: iconThumbnailSize.eid(),
            iconIds: iconIds
        };

        element = angular.element('<sp-icon-picker-dropdown options="options"></<sp-icon-picker-dropdown>');
        $compile(element)(scope);
        scope.$digest();

        dropDownButton = element.find('.dropdownButton');

        $(dropDownButton).click();

        scope.$digest();        

        iconContainers = $document.find('a.iconContainer');

        expect(iconContainers.length).toBe(2);

        // Hide the popup
        $(dropDownButton).click();

        scope.$digest();        

        popup = $document.find('.iconDropdownPopup');

        // Ensure the popup is removed
        if (popup) {
            popup.remove();
        }
    }));
    
    it('click displays popup update selected icon', inject(function ($rootScope, $compile, $templateCache, $document) {
        var scope = $rootScope,
            element,
            dropDownButton;            

        scope.options = {
            iconWidth: 16,
            iconHeight: 16,
            iconSizeId: iconThumbnailSize.eid(),
            iconIds: iconIds
        };

        element = angular.element('<sp-icon-picker-dropdown options="options"></<sp-icon-picker-dropdown>');
        $compile(element)(scope);
        scope.$digest();

        dropDownButton = element.find('.dropdownButton');

        $(dropDownButton).click();

        scope.$digest();        
          
        scope.$$childHead.selectIcon(scope.options.iconIds[0]);

        // Ensure that the multiple watchers fire
        scope.$digest();        
        scope.$digest();                

        expect(scope.options.selectedIconId.alias()).toBe(scope.options.iconIds[0].alias());        
    }));
});