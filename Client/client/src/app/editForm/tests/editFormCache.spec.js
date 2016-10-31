// Copyright 2011-2016 Global Software Innovation Pty Ltd

describe('editFormCache', function () {
    "use strict";

    beforeEach(module('mod.app.editFormCache'));
    beforeEach(module('mockedEditFormWebServices'));
    beforeEach(module('mockedEntityService'));
    beforeEach(module('mod.common.spLocalStorage'));
    beforeEach(module('sp.common.loginService'));

    beforeEach(inject(function ($injector) {
        TestSupport.setupUnitTests(this, $injector);
    }));

    it('can load service', inject(function (editFormCache) {
        expect(editFormCache).toBeTruthy();
    }));
    
    /////
    // Setup the mocked entity service.
    /////
    beforeEach(inject(function (spEntityService, editFormCache, editFormWebServices) {
        editFormCache.removeAll();  // make sure it is flushed.
    }));

    describe('editFormCache is working', function () {

        describe('getFormDefinition', function () {

            beforeEach(inject(function (spEntityService, editFormCache, editFormWebServices) {

                /////
                // Mock a form.
                /////
                var json = {
                    id: 456,
                    typeId: 'console:customEditForm',
                    name: 'test Form',
                    description: 'My Test Form.',
                    cacheChangeMarker: '123456',
                    typeToEditWithForm: jsonLookup()
                };

                spEntityService.mockGetEntityJSON(json);
                editFormWebServices.mockGetFormDefinition(spEntity.fromJSON(json));

            }));

            function getCheckForm(editFormCache, editFormWebServices, formId, haveCalledService) {
                var request = editFormCache.getFormDefinition(formId)
                    .then(function (form) {
                        expect(form).toBeTruthy();
                        expect(form.id()).toBe(formId);
                        expect(editFormWebServices.haveCalled === haveCalledService).toBeTruthy();

                        return form;
                    });

                return request;
            }
            it('should be able to fetch a form', inject(function (editFormCache, editFormWebServices) {
                var result = {};

                var request = getCheckForm(editFormCache, editFormWebServices, 456, true);

                TestSupport.wait(request);

            }));

            it('should be able to fetch a form from cache', inject(function (editFormCache, editFormWebServices) {
                var result = {};

                var request = getCheckForm(editFormCache, editFormWebServices, 456, true).then(function () {

                    editFormWebServices.haveCalled = false;

                    var request2 = getCheckForm(editFormCache, editFormWebServices, 456, false);
                });

                TestSupport.wait(request);

            }));

            it('should check for stale form and not refresh because the form has not changed', inject(function (editFormCache, editFormWebServices, editFormCacheSettings) {
                var result = {};

                var oldSettings = editFormCacheSettings.FormRefreshCycleInSeconds;
                editFormCacheSettings.FormRefreshCycleInSeconds = 0;

                var request = getCheckForm(editFormCache, editFormWebServices, 456, true).then(function () {

                    editFormWebServices.haveCalled = false;

                    var request2 = getCheckForm(editFormCache, editFormWebServices, 456, false);
                }).finally(function () {
                    editFormCacheSettings.FormRefreshCycleInSeconds = oldSettings;    // reset it or it screws up the other tests
                });

                TestSupport.wait(request);

            }));

            it('should check for stale form and refresh because the form has changed', inject(function (spEntityService, editFormCache, editFormWebServices, editFormCacheSettings) {
                var result = {};

                var oldSettings = editFormCacheSettings.FormRefreshCycleInSeconds;
                editFormCacheSettings.FormRefreshCycleInSeconds = 0;

                var request = getCheckForm(editFormCache, editFormWebServices, 456, true).then(function (form) {

                    // replace the form
                    var json = {
                        id: 456,
                        typeId: 'console:customEditForm',
                        name: 'test Form',
                        description: 'My Test Form.',
                        cacheChangeMarker: '777777',        // new change marker
                        typeToEditWithForm: jsonLookup()
                    };

                    spEntityService.mockGetEntityJSON(json);
                    editFormWebServices.mockGetFormDefinition(spEntity.fromJSON(json));

                    editFormWebServices.haveCalled = false;

                    var request2 = getCheckForm(editFormCache, editFormWebServices, 456, true)
                        .finally(function () {
                            editFormCacheSettings.FormRefreshCycleInSeconds = oldSettings;    // reset it or it screws up the other tests
                        });
                });

                TestSupport.wait(request);

            }));

        });

        describe('getFormForDefinition', function () {

            it('should fetch a form for a type with a default form', inject(function (spEntityService, editFormCache, editFormWebServices) {
                var result = {};

                // replace the form
                var json = {
                    id: 1456,
                    typeId: 'console:customEditForm',
                    name: 'test Form',
                    description: 'My Test Form.',
                    cacheChangeMarker: '777777',        // new change marker
                    //typeToEditWithForm: jsonLookup(789)
                };

                var typeJson = {
                    typeId: 'definition',
                    id: 1789,
                    name: 'test type',
                    defaultEditForm: jsonLookup(1456)
                };

                spEntityService.mockGetEntityJSON(typeJson);
                spEntityService.mockGetEntityJSON(json);
                editFormWebServices.mockGetFormDefinition(spEntity.fromJSON(json));

                editFormWebServices.haveCalled = false;

                var request = editFormCache.getFormForDefinition(1789)
                    .then(function (form) {
                        expect(form).toBeTruthy();
                        expect(form.id()).toBe(1456);
                        expect(editFormWebServices.haveCalled === true).toBeTruthy();

                        return form;
                    });

                TestSupport.wait(request);

            }));

            it('should fetch a form for a type without a default form', inject(function (spEntityService, editFormCache, editFormWebServices) {
                var result = {};

                // replace the form
                var json = {
                    id: 2456,
                    typeId: 'console:customEditForm',
                    name: 'test Form',
                    description: 'My Test Form.',
                    cacheChangeMarker: '777777',        // new change marker
                    //typeToEditWithForm: jsonLookup(789)
                };

                var typeJson = {
                    typeId: 'definition',
                    id: 2789,
                    name: 'test type',
                    //defaultEditForm: jsonLookup(456)
                };

                spEntityService.mockGetEntityJSON(typeJson);
                spEntityService.mockGetEntityJSON(json);
                editFormWebServices.mockGetFormForDefinition(2789, spEntity.fromJSON(json));

                editFormWebServices.haveCalled = false;

                var request = editFormCache.getFormForDefinition(2789)
                    .then(function (form) {
                        expect(form).toBeTruthy();
                        expect(form.id()).toBe(2456);
                        expect(editFormWebServices.haveCalled === true).toBeTruthy();

                        return form;
                    });

                TestSupport.wait(request);

            }));

            it('should not refresh a generated form', inject(function (spEntityService, editFormCache, editFormWebServices, editFormCacheSettings) {
                var result = {};

                
                var json = {
                    id: 3456,
                    typeId: 'console:customEditForm',
                    name: 'test Form',
                    description: 'My Test Form.',
                    cacheChangeMarker: '777777',        // new change marker
                };

                var typeJson = {
                    typeId: 'definition',
                    id: 3789,
                    name: 'test type',
                };

                spEntityService.mockGetEntityJSON(typeJson);
                spEntityService.mockGetEntityJSON(json);
                editFormWebServices.mockGetFormForDefinition(3789, spEntity.fromJSON(json));

                var oldSettings = editFormCacheSettings.FormRefreshCycleInSeconds;
                editFormCacheSettings.FormRefreshCycleInSeconds = 0;                        //  force a refresh attempt

                editFormWebServices.haveCalled = false;

                var request = editFormCache.getFormForDefinition(3789)
                    .then(function (form1) {
                        editFormWebServices.haveCalled = false;
                        json.cacheChangeMarker = '99999';                                   // change the form so we know if it was refreshed.
                        spEntityService.mockGetEntityJSON(json);

                        return editFormCache.getFormForDefinition(3789)
                            .then(function (form) {
                                expect(editFormWebServices.haveCalled).toBeFalsy();
                            });
                    }).then(function () {
                        editFormCacheSettings.FormRefreshCycleInSeconds = oldSettings;
                    });

                TestSupport.wait(request);

            }));
        });

        describe('getFormForDefinition', function () {

            it('should get a form for a instance with a default form', inject(function (spEntityService, editFormCache, editFormWebServices, editFormCacheSettings) {
                var result = {};

                // replace the form
                var formJson = {
                    id: 4456,
                    typeId: 'console:customEditForm',
                    name: 'test Form',
                    description: 'My Test Form.',
                    cacheChangeMarker: '777777',        // new change marker
                };

                var instanceJson = {
                    typeId: 4789,
                    id: 4890,
                    isOfType: [{
                        typeId: 'definition',
                        id: 4789,
                        name: 'test type',
                        defaultEditForm: jsonLookup(4456)
                    }],
                    name: 'test type instance',
                };

                spEntityService.mockGetEntityJSON(formJson);
                spEntityService.mockGetEntityJSON(instanceJson);
                editFormWebServices.mockGetFormForDefinition(4789, spEntity.fromJSON(formJson));
                editFormWebServices.mockGetFormDefinition(spEntity.fromJSON(formJson));

                editFormWebServices.haveCalled = false;

                var request = editFormCache.getFormForInstance(4890)
                    .then(function (form) {
                        expect(form).toBeTruthy();
                        expect(form.id()).toBe(4456);
                        expect(editFormWebServices.haveCalled === true).toBeTruthy();

                        return form;
                    }).then(function () {
                        editFormWebServices.haveCalled = false;
                        var request2 = editFormCache.getFormForInstance(4890).then(function (form) {
                            expect(form).toBeTruthy();
                            expect(form.id()).toBe(4456);
                            expect(editFormWebServices.haveCalled, 'that the cache is used the second time around').toBeFalsy();
                        });
                    });

                TestSupport.wait(request);

            }));

            it('should get a generated form for a instance without a default form', inject(function (spEntityService, editFormCache, editFormWebServices) {
                var result = {};

                // replace the form
                var generatedJson = {
                    id: 9999,
                    typeId: 'console:customEditForm',
                    name: 'Generated Form',
                    description: 'Generated  Form.',
                    cacheChangeMarker: '11111',        // new change marker
                };

                //var typeJson = {
                //    typeId: 'definition',
                //    id: 789,
                //    name: 'test type',
                //};


                var instanceJson = {
                    typeId: 5789,
                    id: 5890,
                    isOfType: [{ id: 5789 }],
                    name: 'test type instance',
                };

                //spEntityService.mockGetEntityJSON(typeJson);
                spEntityService.mockGetEntityJSON(generatedJson);
                spEntityService.mockGetEntityJSON(instanceJson);
                editFormWebServices.mockGetFormForDefinition(5789, spEntity.fromJSON(generatedJson));

                editFormWebServices.haveCalled = false;

                var request = editFormCache.getFormForInstance(5890)
                    .then(function (form) {
                        expect(form).toBeTruthy();
                        expect(form.id()).toBe(9999);
                        expect(editFormWebServices.haveCalled === true).toBeTruthy();

                        return form;
                    }).then(function () {
                        editFormWebServices.haveCalled = false;

                        var request2 = editFormCache.getFormForInstance(5890).then(function (form) {
                            expect(form).toBeTruthy();
                            expect(form.id()).toBe(9999);
                            expect(editFormWebServices.haveCalled, 'that the cache is used the second time around').toBeFalsy();
                        });
                    });

                TestSupport.wait(request);

            }));
        });
    });
});

