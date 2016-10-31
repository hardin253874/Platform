// Copyright 2011-2016 Global Software Innovation Pty Ltd
describe('Console|Controls|ColorPickers|spColorPickerFgBgDialog|spec:|spColorPickerFgBgDialog', function () {
    'use strict';

    // Load the modules
    beforeEach(module('mod.common.ui.spColorPickers'));
    beforeEach(module('mod.common.ui.spColorPickerConstants'));
    beforeEach(module('colorPicker/spColorPicker.tpl.html'));
    beforeEach(module('colorPicker/spColorPickerDropdown.tpl.html'));
    beforeEach(module('colorPicker/spColorPickerFgBgDialog.tpl.html'));    
    

    afterEach(inject(function ($document) {
        var body = $document.find('body');
        body.find('div.modal').remove();
        body.find('div.modal-backdrop').remove();
        body.removeClass('modal-open');
    }));


    it('create dialog, show and cancel', inject(function ($rootScope, spColorPickerFgBgDialog) {
        var scope = $rootScope,          
            dialogOptions = {
                foregroundColor: { a: 0, r: 0, g: 0, b: 0 },
                backgroundColor: { a: 0, r: 0, g: 0, b: 0 }                    
            };

        // Setup dialog options           
        spColorPickerFgBgDialog.showModalDialog(dialogOptions).then(function (result) {
            expect(result).toBe(false);
        });

        scope.$digest();
            
        scope.$$childHead.$$childHead.cancel();

        scope.$digest();
    }));


    it('create dialog, show and ok no changes', inject(function ($rootScope, spColorPickerFgBgDialog) {
        var scope = $rootScope,            
            dialogOptions = {
                foregroundColor: { a: 11, r: 12, g: 13, b: 14 },
                backgroundColor: { a: 21, r: 22, g: 23, b: 24 }
            };

        // Setup dialog options           
        spColorPickerFgBgDialog.showModalDialog(dialogOptions).then(function (result) {
            expect(result.foregroundColor.a).toBe(dialogOptions.foregroundColor.a);
            expect(result.foregroundColor.r).toBe(dialogOptions.foregroundColor.r);
            expect(result.foregroundColor.g).toBe(dialogOptions.foregroundColor.g);
            expect(result.foregroundColor.b).toBe(dialogOptions.foregroundColor.b);

            expect(result.backgroundColor.a).toBe(dialogOptions.backgroundColor.a);
            expect(result.backgroundColor.r).toBe(dialogOptions.backgroundColor.r);
            expect(result.backgroundColor.g).toBe(dialogOptions.backgroundColor.g);
            expect(result.backgroundColor.b).toBe(dialogOptions.backgroundColor.b);
        });

        scope.$digest();        

        scope.$$childHead.$$childHead.ok();

        scope.$digest();
    }));    
});