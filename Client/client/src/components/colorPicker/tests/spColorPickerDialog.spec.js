// Copyright 2011-2016 Global Software Innovation Pty Ltd
describe('Console|Controls|ColorPickers|spColorPickerFgBgDialog|spec:|spColorPickerDialog', function () {
    'use strict';

    // Load the modules
    beforeEach(module('mod.common.ui.spColorPickers'));
    beforeEach(module('mod.common.ui.spColorPickerConstants'));
    beforeEach(module('colorPicker/spColorPicker.tpl.html'));
    beforeEach(module('colorPicker/spColorPickerDropdown.tpl.html'));
    beforeEach(module('colorPicker/spColorPickerFgBgDialog.tpl.html'));
    beforeEach(module('colorPicker/spColorPickerDialog.tpl.html'));


    afterEach(inject(function ($document) {
        var body = $document.find('body');
        body.find('div.modal').remove();
        body.find('div.modal-backdrop').remove();
        body.removeClass('modal-open');
    }));


    it('create dialog, show and cancel', inject(function ($rootScope, spColorPickerDialog) {
        var scope = $rootScope,
            dialogOptions = {
                color: { a: 255, r: 0, g: 0, b: 0 }
            };

        // Setup dialog options           
        spColorPickerDialog.showModalDialog(dialogOptions).then(function (result) {
            expect(result).toBe(false);
        });

        scope.$digest();

        scope.$$childHead.$$childHead.cancel();

        scope.$digest();
    }));


    it('create dialog, show and ok no changes', inject(function ($rootScope, spColorPickerDialog) {
        var scope = $rootScope,
            dialogOptions = {
                color: { a: 255, r: 0, g: 0, b: 0 }
            };

        // Setup dialog options           
        spColorPickerDialog.showModalDialog(dialogOptions).then(function (result) {
            expect(result.color.a).toBe(dialogOptions.color.a);
            expect(result.color.r).toBe(dialogOptions.color.r);
            expect(result.color.g).toBe(dialogOptions.color.g);
            expect(result.color.b).toBe(dialogOptions.color.b);
        });

        scope.$digest();

        scope.$$childHead.$$childHead.ok();

        scope.$digest();
    }));
});