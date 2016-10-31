// Copyright 2011-2016 Global Software Innovation Pty Ltd
/*global angular, console, _, TestSupport */

describe('Console|Actions|intg:', function () {
    'use strict';

    beforeEach(module('ng'));
    beforeEach(module('mod.common.spWebService'));
    beforeEach(module('mod.services.promiseService'));
    beforeEach(module('sp.navService'));
    beforeEach(module('mod.common.spEntityService'));
    beforeEach(module('mod.common.ui.spActionsService'));

    beforeEach(inject(function ($injector) {
        TestSupport.setupIntgTests(this, $injector);
    }));

    describe('spActionsService.executeItem', function () {

        it('can load context action data', inject(function (spEntityService, spActionsService) {

            var actionRequest = {
                "ids": [],
                "lastId": 0,
                "hostIds": [],
                "display": 'contextmenu',
                "data": {}
            };

            var result = {};
            TestSupport.waitCheckReturn(
                spEntityService.getEntity('test:aaCoke', 'name')
                    .then(function (e) {
                        //result.id = e.id();
                        actionRequest.ids.push(e.id());
                        actionRequest.lastId = e.id();
                        return spActionsService.getConsoleActions(actionRequest);
                    }),
                result);

            runs(function () {
                var actions = result.value.actions;
                expect(actions).toBeTruthy();
                expect(actions.length >= 1).toBeTruthy();
                var action = actions[0];
                expect(action.eid > 0).toBeTruthy();
                expect(action.name).toBeTruthy();
                expect(action.method).toBeTruthy();
            });

        }));
    });

});
