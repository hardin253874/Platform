// Copyright 2011-2016 Global Software Innovation Pty Ltd
/*global _, angular, console, sp, module, describe, beforeEach, it, inject, expect, runs, waitsFor, TestSupport, spEntity, entityTestData, jsonLookup */

describe('Console|Navigation|spNavDataService|spec:', function () {
    "use strict";

    beforeEach(module('sp.navService'));
    beforeEach(module('mockedEntityService'));
    beforeEach(module('mockedLoginService'));

    beforeEach(inject(function ($rootScope) {
        this.addMatchers(TestSupport.matchers);
        TestSupport.setScope($rootScope);
    }));

    describe('getNavItemIconUrl', function () {

        it('handles null', inject(function (spNavDataService) {
            expect(spNavDataService.getNavItemIconUrl(null)).toBeNull();
        }));

        it('handles default', inject(function (spNavDataService) {
            var navItem = spEntity.fromJSON({
                name: 'whatever'
            });
            var res = spNavDataService.getNavItemIconUrl(navItem);
            expect(res).toBe('assets/images/default_app.png');
        }));

        it('handles navigationElementIcon', inject(function (spNavDataService) {

            var navItem = spEntity.fromJSON({
                navigationElementIcon:'asdf'
            });
            var res = spNavDataService.getNavItemIconUrl(navItem);
            expect(res).toBeTruthy();
        }));

        it('handles resourceConsoleBehavior only', inject(function (spNavDataService) {

            var navItem = spEntity.fromJSON({
                "console:resourceConsoleBehavior": {
                    treeIconUrl: 'resourceUrl'
                }
            });
            var res = spNavDataService.getNavItemIconUrl(navItem);
            expect(res).toBe('resourceUrl');
        }));

        it('handles typeConsoleBehavior only', inject(function (spNavDataService) {

            var navItem = spEntity.fromJSON({
                isOfType: [{
                    "console:typeConsoleBehavior": {
                        treeIconUrl: 'typeUrl'
                    }
                }]
            });
            var res = spNavDataService.getNavItemIconUrl(navItem);
            expect(res).toBe('typeUrl');
        }));

        it('handles both behaviors together', inject(function (spNavDataService) {

            var navItem = spEntity.fromJSON({
                "console:resourceConsoleBehavior": {
                    treeIconUrl: 'resourceUrl'
                },
                isOfType: [{
                    "console:typeConsoleBehavior": {
                        treeIconUrl: 'typeUrl'
                    }
                }]
            });
            var res = spNavDataService.getNavItemIconUrl(navItem);
            expect(res).toBe('resourceUrl');
        }));

        it('handles fallback to type', inject(function (spNavDataService) {

            var navItem = spEntity.fromJSON({
                "console:resourceConsoleBehavior": {
                    name: 'whatever'
                },
                isOfType: [{
                    "console:typeConsoleBehavior": {
                        treeIconUrl: 'typeUrl'
                    }
                }]
            });
            var res = spNavDataService.getNavItemIconUrl(navItem);
            expect(res).toBe('typeUrl');
        }));

        it('handles reformatting', inject(function (spNavDataService) {

            var navItem = spEntity.fromJSON({
                "console:resourceConsoleBehavior": {
                    treeIconUrl: 'assembly;component/resourceUrl'
                }
            });
            var res = spNavDataService.getNavItemIconUrl(navItem);
            expect(res).toBe('resourceUrl');
        }));


    });
});
