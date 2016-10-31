// Copyright 2011-2016 Global Software Innovation Pty Ltd
describe('Security|spAccessControlService', function () {
    'use strict';

    beforeEach(module('mod.app.accessControl.service'));
    beforeEach(inject(function ($injector) {
        TestSupport.setupUnitTests(this, $injector);
    }));

    it('new SecurityQueryRow() throws on null auth', inject(function (spAccessControlService) {
        expect(function () {
            spAccessControlService._newSecurityQueryRow(null, true);
        }).toThrow();
    }));

    it('new SecurityQueryRow() throws on null defaultDirty', inject(function (spAccessControlService) {
        expect(function () {
            spAccessControlService._newSecurityQueryRow(new spEntity._Entity(), null);
        }).toThrow();
    }));

    it('new SecurityQueryRow()', inject(function (spAccessControlService) {
        var accessRule = new spEntity._Entity();
        accessRule.allowAccessBy = {
            name: "allowAccessBy",
            isOfType: [{ name: "type" }]
        };
        accessRule.permissionAccess = [
            { nsAlias: "core:read" },
            { nsAlias: "core:modify" }
        ];
        accessRule.accessRuleReport = { name: "accessRuleReport" };
        accessRule.accessRuleEnabled = true;
        var defaultDirty = true;

        var securityQueryRow = spAccessControlService._newSecurityQueryRow(accessRule, defaultDirty);

        expect(securityQueryRow.roleOrUserAccount).toBe("allowAccessBy (type)");
        expect(securityQueryRow.accessRule).toBe(accessRule);
        expect(securityQueryRow.permissions).toBe("core:modify,core:read");
        expect(securityQueryRow.queryName).toBe(accessRule.accessRuleReport.name);
        expect(securityQueryRow.enabled).toBe(accessRule.accessRuleEnabled);
        expect(securityQueryRow.dirty).toBe(defaultDirty);
    }));


    it('new SecurityQueryRow() with missing allowAccessBy', inject(function (spAccessControlService) {
        var accessRule = new spEntity._Entity();
        accessRule.allowAccessBy = null;
        accessRule.permissionAccess = [
            { nsAlias: "core:read" },
            { nsAlias: "core:modify" }
        ];
        accessRule.accessRuleReport = { name: "accessRuleReport" };
        accessRule.accessRuleEnabled = true;
        var defaultDirty = true;

        var securityQueryRow = spAccessControlService._newSecurityQueryRow(accessRule, defaultDirty);

        expect(securityQueryRow.roleOrUserAccount).toBe("(Deleted) (Unknown Type)");
        expect(securityQueryRow.accessRule).toBe(accessRule);
        expect(securityQueryRow.permissions).toBe("core:modify,core:read");
        expect(securityQueryRow.queryName).toBe(accessRule.accessRuleReport.name);
        expect(securityQueryRow.enabled).toBe(accessRule.accessRuleEnabled);
        expect(securityQueryRow.dirty).toBe(defaultDirty);
    }));
});