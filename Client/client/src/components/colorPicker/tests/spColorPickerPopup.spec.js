// Copyright 2011-2016 Global Software Innovation Pty Ltd
describe('Console|Controls|ColorPickers|spColorPickerPopup|spec:|spColorPickerPopup directive', function () {
    'use strict';

    // Load the modules
    beforeEach(module('mod.common.ui.spColorPickers'));
    beforeEach(module('mod.common.ui.spColorPickerConstants'));
    beforeEach(module('colorPicker/spColorPicker.tpl.html'));
    beforeEach(module('colorPicker/spColorPickerPopup.tpl.html'));
    beforeEach(module('colorPicker/spColorPickerDropdown.tpl.html'));        


    it('display popup', inject(function ($rootScope, $compile, $templateCache, $timeout) {
        var scope = $rootScope,
            element,
            popup;

        scope.options = {
            color: { a: 20, r: 30, g: 40, b: 50 },
            isOpen: false
        };

        element = angular.element('<div sp-color-picker-popup="options"></div>');
        $compile(element)(scope);
        scope.$digest();
        
        scope.options.isOpen = true;
        scope.$digest();
        $timeout.flush();
        
        popup = element.next();

        // Verify that the popup is visible
        expect(popup.hasClass('spColorPicker-view')).toBe(true);
        expect(popup.hasClass('dropdown-menu')).toBe(true);
        expect(popup.css('display')).not.toBe('none');
        
        scope.options.isOpen = false;
        scope.$digest();
        $timeout.flush();        

        popup = element.next();

        // Verify that the popup is hidden
        expect(popup.css('display')).toBe('none');        
    }));


    it('close on ok click', inject(function ($rootScope, $compile, $templateCache, $timeout) {
        var scope = $rootScope,
            element,
            okButton,
            popup;

        scope.options = {
            color: { a: 20, r: 30, g: 40, b: 50 },
            isOpen: false
        };

        element = angular.element('<div sp-color-picker-popup="options"></div>');
        $compile(element)(scope);
        scope.$digest();

        scope.options.isOpen = true;
        scope.$digest();
        $timeout.flush();

        popup = element.next();

        // Verify that the popup is visible
        expect(popup.hasClass('spColorPicker-view')).toBe(true);
        expect(popup.hasClass('dropdown-menu')).toBe(true);
        expect(popup.css('display')).not.toBe('none');

        okButton = _.find(popup.find(':button'), function (b) {
            return $(b).text() === 'OK';
        });

        okButton.click();        
        scope.$digest();
        $timeout.flush();

        popup = element.next();

        // Verify that the popup is hidden
        expect(popup.css('display')).toBe('none');
    }));


    it('close on cancel click', inject(function ($rootScope, $compile, $templateCache, $timeout) {
        var scope = $rootScope,
            element,
            cancelButton,
            popup;

        scope.options = {
            color: { a: 20, r: 30, g: 40, b: 50 },
            isOpen: false
        };

        element = angular.element('<div sp-color-picker-popup="options"></div>');
        $compile(element)(scope);
        scope.$digest();

        scope.options.isOpen = true;
        scope.$digest();
        $timeout.flush();

        popup = element.next();

        // Verify that the popup is visible
        expect(popup.hasClass('spColorPicker-view')).toBe(true);
        expect(popup.hasClass('dropdown-menu')).toBe(true);
        expect(popup.css('display')).not.toBe('none');

        cancelButton = _.find(popup.find(':button'), function (b) {
            return $(b).text() === 'Cancel';
        });

        cancelButton.click();
        scope.$digest();
        $timeout.flush();

        popup = element.next();

        // Verify that the popup is hidden
        expect(popup.css('display')).toBe('none');
    }));


    it('close on document click', inject(function ($rootScope, $compile, $templateCache, $timeout, $document) {
        var scope = $rootScope,
            element,            
            popup;

        scope.options = {
            color: { a: 20, r: 30, g: 40, b: 50 },
            isOpen: false
        };

        element = angular.element('<div sp-color-picker-popup="options"></div>');
        $compile(element)(scope);
        scope.$digest();

        scope.options.isOpen = true;
        scope.$digest();
        $timeout.flush();

        popup = element.next();

        // Verify that the popup is visible
        expect(popup.hasClass('spColorPicker-view')).toBe(true);
        expect(popup.hasClass('dropdown-menu')).toBe(true);
        expect(popup.css('display')).not.toBe('none');

        $document.click();        
        scope.$digest();
        $timeout.flush();

        popup = element.next();

        // Verify that the popup is hidden
        expect(popup.css('display')).toBe('none');
    }));
});