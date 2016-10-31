// Copyright 2011-2016 Global Software Innovation Pty Ltd
describe('Console|report property|spec:|', function () {
    'use strict';

    beforeEach(module('mod.app.reportProperty'));
    beforeEach(module('app-templates'));
    beforeEach(module('component-templates'));
    beforeEach(module('reportBuilder/dialogs/reportProperty/reportProperty.tpl.html'));
    beforeEach(module('app.editForm.spInlineRelationshipPickerController'));
    beforeEach(module('editForm/partials/spInlineRelationshipPicker.tpl.html'));
    beforeEach(module('entityPickers/entityComboPicker/spEntityComboPicker.tpl.html'));

    afterEach(inject(function ($document) {
        var body = $document.find('body');
        body.find('div.modal').remove();
        body.find('div.modal-backdrop').remove();
        body.removeClass('modal-open');
    }));

    describe('spReportPropertyDialog|spec:', function () {
        it('create dialog, show and cancel', inject(function ($rootScope, spReportPropertyDialog) {
            var scope = $rootScope,
                dialogOptions = {

                };

            // Show the dialog
            spReportPropertyDialog.showModalDialog(dialogOptions).then(function (result) {
                expect(result).toBe(false);
            });

            expect(spReportPropertyDialog).toBeTruthy();
        }));
    });

});