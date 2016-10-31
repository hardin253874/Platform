// Copyright 2011-2016 Global Software Innovation Pty Ltd
/*global _, angular, console, sp, module, describe, beforeEach, it, inject, expect, runs, waitsFor, TestSupport, spEntity, entityTestData, jsonLookup */

describe('Console|Navigation|navService|spec:', function () {
    "use strict";

    var sectionBehavior = spEntity.fromJSON({
            id: { id: 10000, ns: 'console', alias: 'navSectionBehavior' },
            "console:treeIconUrl": ';folder.png',
            "console:html5ViewId": 'folder',
            "console:consoleBehaviorUsesPageControl": { id: { id: 10001, ns: 'core', alias: "folderPageControl" } }
        });
    var folderBehavior = spEntity.fromJSON({
        id: { id: 20000, ns: 'console', alias: 'folderBehavior' },
        "console:treeIconUrl": ';folder.png',
        "console:html5ViewId": 'folder',
        "console:consoleBehaviorUsesPageControl": { id: { id: 10001, ns: 'core', alias: "folderPageControl" } }
    });
    var navSectionType = spEntity.fromJSON({
        id: { id: 200, ns: 'console', alias: 'navSection' },
        name: 'Navigation Section',
        "console:typeConsoleBehavior": sectionBehavior
    });
    var folderType = spEntity.fromJSON({
        id: { id: 300, ns: 'console', alias: 'folder' },
        name: 'Resource Folder',
        "console:typeConsoleBehavior": folderBehavior
    });

    var topMenuType = spEntity.fromJSON({
        id: { id: 100, ns: 'console', alias: 'topMenu' },
        instancesOfType: [
            {
                id: { id: 1000, ns: 'console', alias: 'adminMenu' },
                name: "Administration",
                "console:isTopMenuVisible": false,
                "console:consoleOrder": 999,
                "console:showApplicationTabs": false,
                "console:resourceConsoleBehavior": {
                    id: 'console:menuBehavior',
                    html5ViewId: 'menu',
                    treeIconUrl: 'slresource;images/menu.png'
                },
                "console:folderContents": [
                    {
                        id: { id: 1100, ns: 'console', alias: 'section1' },
                        name: "Section 1",
                        isOfType: navSectionType
                    },
                    {
                        id: { id: 1200, ns: 'console', alias: 'section2' },
                        name: "Section 2",
                        isOfType: navSectionType
                    }
                ]
            },
            {
                id: { id: 2000, ns: 'app1', alias: 'app1Menu' },
                name: "Application 1",
                "console:isTopMenuVisible": true,
                "console:consoleOrder": 2,
                "console:showApplicationTabs": true,
                "console:resourceConsoleBehavior": jsonLookup(null),
                "console:folderContents": [
                    {
                        id: { id: 2100, ns: 'app1', alias: 'tab1' },
                        name: "Tab 1",
                        isOfType: navSectionType
                    },
                    {
                        id: { id: 2200, ns: 'console', alias: 'tab2' },
                        name: "Tab 2",
                        isOfType: navSectionType
                    }
                ]
            }
        ]
    });

    var reportEntity = spEntity.fromJSON({
        id: { id: 3000, ns: 'test', alias: 'adminReport1' },
        name: 'Admin Report 1',

        "console:resourceInFolder": {
            id: { id: 5000, ns: 'test', alias: 'adminReportsFolder' },
            name: 'Admin Reports Folder',
            isOfType: folderType,

            "console:folderContents": [
                'test:adminReport1',
                {
                    id: { id: 5100, ns: 'test', alias: 'adminReport2' },
                    name: "Admin Report 2"
                }
            ],
            "console:resourceInFolder": {
                id: { id: 1100, ns: 'console', alias: 'section1' },
                name: "Section 1",
                isOfType: navSectionType,

                "console:folderContents": [
                    'test:adminReportsFolder'
                ],
                "console:resourceInFolder": {
                    id: { id: 1000, ns: 'console', alias: 'adminMenu' },
                    name: "Administration",
                    typeId: 'console:topMenu',
                    "console:isTopMenuVisible": false,
                    "console:consoleOrder": 999,
                    "console:showApplicationTabs": false,

                    "console:folderContents": [
                        'console:section1',
                        {id: 'console:section2', name: 'Section 2'}
                    ]
                }
            }
        }
    });

    var tenantGeneralSettingsInstanceEntity = spEntity.fromJSON({
        id: { id: 6000, ns: 'core', alias: 'tenantGeneralSettingsInstance' },
        "finYearStartMonth": { id: 6001, ns: 'test', alias: 'testFinYear' },
        "tenantCurrencySymbol": '$',
        "tenantConsoleThemeSettings" : { id: 6002, ns: 'test', alias: 'testConsoleThemeSettings' }
    });

    var fullConfigButton = spEntity.fromJSON({
        id: { id: 9000, ns: 'core', alias: 'fullConfigButton' }
    });

    var selfServiceConfigButton = spEntity.fromJSON({
        id: { id: 9009, ns: 'core', alias: 'selfServiceConfigButton' }
    });
   
    var adminToolboxStaticPage = spEntity.fromJSON({
        id: { id: 9001, ns: 'console', alias: 'adminToolboxStaticPage' }
    });

    var nameFieldEntity = spEntity.fromJSON({
        id: { id: 9001, ns: 'core', alias: 'name' }
    });

    beforeEach(module('sp.navService'));
    beforeEach(module('mockedEntityService'));
    beforeEach(module('mockedLoginService'));

    beforeEach(inject(function ($rootScope) {
        this.addMatchers(TestSupport.matchers);
        TestSupport.setScope($rootScope);
    }));

    describe('the "should be generic" tree functions', function () {

        var sampleTree = {
            item: null,
            children: [
                {
                    item: {id: 200},
                    children: [
                        {
                            item: {id: 'abc'}
                        }
                    ]
                },
                {
                    item: {id: 900, name: 'heyman'}
                }
            ]
        };

        it('findInTree can find items when it should', inject(function (spNavService) {
            var node = spNavService.findInTreeById(sampleTree, 200);

            expect(node).toBeTruthy();
            expect(node.item.id).toBe(200);

            expect(spNavService.findInTree(sampleTree, node.item)).toBeTruthy();

            expect(spNavService.findInTree(sampleTree, function (item) {
                return item.name === 'heyman';
            })).toBeTruthy();
        }));

        it('findInTree doesn\'t find when it shouldn\'t', inject(function (spNavService) {
            expect(spNavService.findInTreeById(sampleTree, 'shouldntbethere')).toBeFalsy();
        }));

        it('findInTree returns the path when it finds an item', inject(function (spNavService) {
            var path = [],
                node = spNavService.findInTreeById(sampleTree, 'abc', path);

            console.log('path=', path);

            expect(node).toBeTruthy();
            expect(path.length).toBe(2);

            path = [];
            spNavService.findInTreeById({}, 'xxxxxx', path);
            console.log('path=', path);
            expect(path.length).toBe(0);

            path = [];
            spNavService.findInTreeById({ item: null, children: [
                {item: {id: 99} }
            ]}, 99, path);
            console.log('path=', path);
            expect(path.length).toBe(1);
        }));

        it('merge tree works', inject(function (spNavService) {
            expect(spNavService.findInTreeById(sampleTree, 888)).toBeFalsy();
            spNavService.mergeTreeChild(sampleTree, { item: {id: 888} });
            expect(spNavService.findInTreeById(sampleTree, 888)).toBeTruthy();
        }));

        it('foreach in tree works', inject(function (spNavService) {
            expect(spNavService.findInTree(sampleTree, function (item) {
                return item.name === 'abc';
            })).toBeFalsy();
            spNavService.forEachItemInTree(sampleTree, function (item) {
                if (item) {
                    item.name = item.id;
                }
            });
            expect(spNavService.findInTree(sampleTree, function (item) {
                return item.name === 'abc';
            })).toBeTruthy();
        }));
    });

    describe('the nav related services', function () {

        beforeEach(inject(function ($q, spEntityService, spNavService, spNavDataService) {

            var result = spEntity.fromJSON(topMenuType);
            spyOn(spNavDataService, "getNavItems").andReturn($q.when(spNavDataService.entityToTree(result)));

            spEntityService.mockGetEntity(reportEntity);
            spEntityService.mockGetEntity(tenantGeneralSettingsInstanceEntity);
            spEntityService.mockGetEntity(fullConfigButton);
            spEntityService.mockGetEntity(selfServiceConfigButton);
            spEntityService.mockGetEntity(adminToolboxStaticPage);
            spEntityService.mockGetEntity(nameFieldEntity);
            
            spEntityService.mockGetInstancesOfTypeRawData('core:solution', entityTestData.thumbnailSizesTestData);
        }));

        it('spNavDataService can load the top of the navigation tree', inject(function (spNavDataService) {
            var results = {}, tree;

            expect(spNavDataService).toBeTruthy();

            TestSupport.waitCheckReturn(spNavDataService.getNavItems(), results);

            runs(function () {
                expect(results).toBeTruthy();
                expect(results.value).toBeTruthy();

                tree = results.value;

                // root node has null item
                expect(tree.item.id).toBeUndefined();
                // root node should have 2 children according to the mock data for topMenu instancesOfType
                expect(tree.children).toBeTruthy();
                expect(tree.children.length).toBe(2);
                // do a couple of data checks based on the mocked data
                expect(_.first(tree.children).item.alias).toBe('console:adminMenu');
                expect(_.first(tree.children).item.name).toBe('Administration');
                expect(_.first(tree.children).children.length).toBe(2);
            });
        }));

        it('spNavDataService processes a navigation entity as expected', inject(function (spNavDataService) {
            var results = {}, tree, node;

            expect(spNavDataService).toBeTruthy();

            TestSupport.waitCheckReturn(spNavDataService.getNavItems(), results);

            runs(function () {
                expect(results).toBeTruthy();
                expect(results.value).toBeTruthy();

                tree = results.value;
                node = _.first(tree.children);

                // do a couple of data checks based on the mocked data
                expect(node.item.alias).toBe('console:adminMenu');
                expect(node.item.viewType).toBe('menu');
                expect(node.item.iconUrl).toBe('images/menu.png');

                node = _.first(node.children);
                expect(node.item.alias).toBe('console:section1');
                expect(node.item.viewType).toBe('folder'); // this one uses the silverlight control mapping
            });
        }));

        it('spNavDataService can load a portion of the tree to effect an "expand" operation', inject(function (spNavDataService) {
            var results = {}, tree;

            expect(spNavDataService).toBeTruthy();

            TestSupport.waitCheckReturn(spNavDataService.getNavTreeExpanded([
                {id: 'test:adminReport1'}
            ]), results);

            runs(function () {
                expect(results).toBeTruthy();
                expect(results.value).toBeTruthy();

                tree = results.value;
                console.log('tree returned', tree);

                // the tree returned by this is an actual item
                expect(tree.item).not.toBeNull();
                expect(tree.item.alias).toBe('console:adminMenu');
                expect(tree.children).toBeTruthy();
                expect(tree.children.length).toBe(2);

                // do a couple of data checks based on the mocked data
                expect(_.first(tree.children).item.alias).toBe('console:section1');
                expect(_.first(tree.children).item.name).toBe('Section 1');
                expect(_.first(tree.children).children.length).toBe(1);
            });

        }));

        it('spNavService builds default tree after simulated login', inject(function ($rootScope, spNavService, spLoginService) {
            var tree;

            expect(spNavService).toBeTruthy();

            tree = spNavService.getNavTree();

            expect(tree).toBeTruthy();
            expect(tree.children).toBeTruthy();
            expect(tree.children.length).toBe(0);

            spLoginService.setLoggedIn();
            spLoginService.notifyLoggedIn();

            $rootScope.$apply();

            tree = spNavService.getNavTree();

            expect(tree.children.length).toBeGreaterThan(0);

            // the order should be switched due to consoleOrder
            expect(_.first(tree.children).item.alias).toBe('app1:app1Menu');
            expect(_.first(tree.children).item.name).toBe('Application 1');
            expect(_.first(tree.children).children.length).toBe(2);
        }));


    });

    describe('the spState service... ', function () {

        beforeEach(inject(function (spEntityService) {

            console.log('you funny type you: ', topMenuType);
            console.log('your data is worthless: ', reportEntity);

            spEntityService.mockGetEntity(topMenuType);
            spEntityService.mockGetEntity(reportEntity);
        }));

        xit('exposes the current state name and params', inject(function ($rootScope, spState, spLoginService) {

            spLoginService.setLoggedIn();
            spLoginService.notifyLoggedIn();
            $rootScope.$apply();

            var event = $rootScope.$broadcast('$stateChangeStart', { name: 'report' }, { eid: 'test:adminReport1', tenant: 'EDC' });
            if (!event.defaultPrevented) {
                $rootScope.$broadcast('$stateChangeSuccess', { name: 'report' }, { eid: 'test:adminReport1', tenant: 'EDC' });
                $rootScope.$apply();
            }

            expect(spState).toBeTruthy();
            expect(spState.name).toBeTruthy();
            expect(spState.params).toBeTruthy();

        }));

    });

    describe('edit/view model methods', function () {
        it('ensure can set and reset isEdit mode property', inject(function (spEntityService, spAppSettings, spNavService) {
            spEntityService.mockGetEntity(fullConfigButton);
            spEntityService.mockGetEntity(selfServiceConfigButton);
            spEntityService.mockGetEntity(adminToolboxStaticPage);

            TestSupport.wait(
                spAppSettings.getNavConfigEntities().then(function () {
                    spNavService.setIsEditMode(true);
                    expect(spNavService.isSelfServeEditMode).toBe(true);
                    expect(spNavService.isFullEditMode).toBe(true);
                    spNavService.setIsEditMode(false);
                    expect(spNavService.isSelfServeEditMode).toBe(false);
                    expect(spNavService.isFullEditMode).toBe(false);
                }));
        }));

        it('ensure can toggle edit mode via toggleIsEditMode', inject(function (spEntityService, spAppSettings, spNavService) {
            spEntityService.mockGetEntity(fullConfigButton);
            spEntityService.mockGetEntity(selfServiceConfigButton);
            spEntityService.mockGetEntity(adminToolboxStaticPage);

            TestSupport.wait(
                spAppSettings.getNavConfigEntities().then(function () {
                    spNavService.setIsEditMode(false);
                    spNavService.toggleIsEditMode();
                    expect(spNavService.isSelfServeEditMode).toBe(true);
                    expect(spNavService.isFullEditMode).toBe(true);
                    spNavService.toggleIsEditMode();
                    expect(spNavService.isSelfServeEditMode).toBe(false);
                    expect(spNavService.isFullEditMode).toBe(false);
                }));
        }));

        it('ensure full admin edit mode works', inject(function (spEntityService, spAppSettings, spNavService) {
            spEntityService.mockGetEntityNotFound(selfServiceConfigButton);
            spEntityService.mockGetEntity(fullConfigButton);
            spEntityService.mockGetEntity(adminToolboxStaticPage);

            TestSupport.wait(
                spAppSettings.getNavConfigEntities().then(function () {
                    spNavService.setIsEditMode(true);
                    expect(spNavService.isSelfServeEditMode).toBe(true);
                    expect(spNavService.isFullEditMode).toBe(true);
                }));
        }));
    });
});
