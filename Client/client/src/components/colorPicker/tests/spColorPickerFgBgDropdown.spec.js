// Copyright 2011-2016 Global Software Innovation Pty Ltd
describe('Console|Controls|ColorPickers|spColorPickerFgBgDropdown|spec:|spColorPickerFgBgDropdown directive', function () {
    'use strict';

    // Load the modules
    beforeEach(module('mod.common.ui.spColorPickers'));
    beforeEach(module('mod.common.ui.spColorPickerConstants'));
    beforeEach(module('colorPicker/spColorPicker.tpl.html'));
    beforeEach(module('colorPicker/spColorPickerFgBgDropdown.tpl.html'));
    beforeEach(module('colorPicker/spColorPickerFgBgDropdownMenu.tpl.html'));

    it('should replace HTML element with appropriate content', inject(function ($rootScope, $compile, $templateCache, $document, $timeout, namedFgBgColors) {
        var scope = $rootScope,
            element,
            dropDownButton,
            colorName,
            namedColor,
            i,
            menuItems;

        scope.selectedColor = {
            foregroundColor: { a: 11, r: 12, g: 13, b: 14 },
            backgroundColor: { a: 21, r: 22, g: 23, b: 24 },
        };

        element = angular.element('<sp-color-picker-fg-bg-dropdown color="selectedColor"></<sp-color-picker-fg-bg-dropdown>');
        $compile(element)(scope);
        scope.$digest();

        // Verify that the html element has been replaced        
        expect(element.hasClass('spColorPicker-view')).toBe(true);
    }));


    it('should update selected fg bg color', inject(function ($rootScope, $compile, $templateCache, $document, $timeout, namedFgBgColors) {
        var scope = $rootScope,
            element,            
            dropDownButton,
            colorName,
            namedColor,
            i,
            menuItems;

        scope.selectedColor = {
            foregroundColor: { a: 11, r: 12, g: 13, b: 14 },
            backgroundColor: { a: 21, r: 22, g: 23, b: 24 },
        };

        element = angular.element('<sp-color-picker-fg-bg-dropdown color="selectedColor"></<sp-color-picker-fg-bg-dropdown>');
        $compile(element)(scope);
        scope.$digest();        

        dropDownButton = element.find('.dropdownButton');

        $(dropDownButton).click();
        
        scope.$digest();
        $timeout.flush();

        menuItems = $document.find('a.fgBgDropdownMenuItem');

        $(menuItems[2]).click();
        scope.$digest();        

        colorName = $(menuItems[2]).find('div').text();

        namedColor = _.find(namedFgBgColors, function (nc) {
            return nc.name === colorName;
        });

        expect(scope.selectedColor.foregroundColor.a).toBe(namedColor.foregroundColor.a);
        expect(scope.selectedColor.foregroundColor.r).toBe(namedColor.foregroundColor.r);
        expect(scope.selectedColor.foregroundColor.g).toBe(namedColor.foregroundColor.g);
        expect(scope.selectedColor.foregroundColor.b).toBe(namedColor.foregroundColor.b);

        expect(scope.selectedColor.backgroundColor.a).toBe(namedColor.backgroundColor.a);
        expect(scope.selectedColor.backgroundColor.r).toBe(namedColor.backgroundColor.r);
        expect(scope.selectedColor.backgroundColor.g).toBe(namedColor.backgroundColor.g);
        expect(scope.selectedColor.backgroundColor.b).toBe(namedColor.backgroundColor.b);
    }));    
});