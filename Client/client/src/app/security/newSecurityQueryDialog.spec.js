// Copyright 2011-2016 Global Software Innovation Pty Ltd
/* globals jasmine */

describe("Security|newSecurityQueryDialog", function () {
    "use strict";

    var spDialogServiceMock;

    beforeEach(module("mod.common.ui.spDialogService"));
    beforeEach(function() {
        spDialogServiceMock = jasmine.createSpyObj("spDialogService", ["showModalDialog"]);

        module("app.securityQuery.newQueryDialog", function($provide) {
            $provide.value('spDialogService', spDialogServiceMock);
        });
    });
    beforeEach(inject(function ($injector) {
        TestSupport.setupUnitTests(this, $injector);
    }));

    it("showDialog() throws on non-array subjects", inject(function (spNewSecurityQueryDialogFactory) {
        var subjects = null;
        var types = [];
        var defaultSubjectAlias = "";
        var defaultSecurableEntityAlias = "";

        expect(function () { spNewSecurityQueryDialogFactory.showDialog(subjects, types, defaultSubjectAlias, defaultSecurableEntityAlias); }).toThrow();
    }));

    it("showDialog() throws on non-array types", inject(function (spNewSecurityQueryDialogFactory) {
        var subjects = [];
        var types = null;
        var defaultSubjectAlias = "";
        var defaultSecurableEntityAlias = "";

        expect(function () { spNewSecurityQueryDialogFactory.showDialog(subjects, types, defaultSubjectAlias, defaultSecurableEntityAlias); }).toThrow();
    }));

    it("showDialog() throws on non-object defaultSubject", inject(function (spNewSecurityQueryDialogFactory) {
        var subjects = [];
        var types = [];
        var defaultSubjectAlias = null;
        var defaultSecurableEntityAlias = "";

        expect(function () { spNewSecurityQueryDialogFactory.showDialog(subjects, types, defaultSubjectAlias, defaultSecurableEntityAlias); }).toThrow();
    }));

    it("showDialog() throws on non-array defaultSecurableEntityAlias", inject(function (spNewSecurityQueryDialogFactory) {
        var subjects = [];
        var types = [];
        var defaultSubjectAlias = "";
        var defaultSecurableEntityAlias = null;

        expect(function () { spNewSecurityQueryDialogFactory.showDialog(subjects, types, defaultSubjectAlias, defaultSecurableEntityAlias); }).toThrow();
    }));

    it("showDialog() called with expected arguments", inject(function (spNewSecurityQueryDialogFactory) {
        var subjects = [
            { nsAlias: "core:userA", name: "user a" },
            { nsAlias: "core:userB", name: "user b" }
        ];
        var types = [
            { nsAlias: "core:typeA", name: "type a" },
            { nsAlias: "core:typeB", name: "type b" }
        ];
        var defaultSubjectAlias = "core:userA";
        var defaultSecurableEntityAlias = "core:typeB";
        var optionsResult;

        expect(function () { spNewSecurityQueryDialogFactory.showDialog(subjects, types, defaultSubjectAlias, defaultSecurableEntityAlias); }).not.toThrow();
        expect(spDialogServiceMock.showModalDialog.calls.length).toBe(1);
        expect(spDialogServiceMock.showModalDialog.calls[0].args[0].templateUrl).toBe("security/newSecurityQueryDialog.tpl.html");
        expect(spDialogServiceMock.showModalDialog.calls[0].args[0].controller).toBe("NewSecurityQueryDialogController");

        optionsResult = spDialogServiceMock.showModalDialog.calls[0].args[0].resolve.options();
        expect(optionsResult.subjects).toEqual(_.sortBy(subjects, "name"));
        expect(optionsResult.selectedSubject.nsAlias).toEqual(defaultSubjectAlias);
        expect(optionsResult.securableEntities).toEqual(_.sortBy(types, "name"));
        expect(optionsResult.selectedSecurableEntity.nsAlias).toEqual(defaultSecurableEntityAlias);
    }));
});