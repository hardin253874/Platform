// Copyright 2011-2016 Global Software Innovation Pty Ltd
describe('Console|Navigation|Navigation Providers', function () {
    'use strict';

    var entityChanges;

    beforeEach(module('app-templates'));
    beforeEach(module('component-templates'));    
    beforeEach(module('sp.app.navigation'));
    beforeEach(module('mod.app.navigationProviders'));
    beforeEach(module('mod.app.navigation.appElementDialog'));
    beforeEach(module('mod.app.navigation.spNavigationElementDialog'));
    beforeEach(module('sp.common.spEntityHelper'));    

    beforeEach(module('mockedEntityService'));
    beforeEach(module('mockedLoginService'));

    beforeEach(inject(function (appElementDialog, $q) {
        appElementDialog.showDialog = function (options) {
            var deferred = $q.defer();

            options.entity.name = options.entity.name + '-Modified';

            deferred.resolve(options);

            return deferred.promise;
        };
    }));

    beforeEach(inject(function (spNavigationElementDialog, $q) {
        spNavigationElementDialog.showDialog = function (options) {
            var deferred = $q.defer();

            options.entity.name = options.entity.name + '-Modified';

            deferred.resolve(options);

            return deferred.promise;
        };
    }));

    // Mock the nav service
    beforeEach(inject(function (spEntityService, $q) {        

        var tenantGeneralSettingsInstanceEntity = spEntity.fromJSON({
            id: { id: 6000, ns: 'core', alias: 'tenantGeneralSettingsInstance' },
            "finYearStartMonth": { id: 6001, ns: 'test', alias: 'testFinYear' },
            "tenantCurrencySymbol": '$',
            "tenantConsoleThemeSettings": { id: 6002, ns: 'test', alias: 'testConsoleThemeSettings' }
        });

        spEntityService.mockGetEntity(tenantGeneralSettingsInstanceEntity);

        spEntityService.mockGetEntity(spEntity.fromJSON({
            id: { id: 77777, ns: 'core', alias: 'name' }
        }));

        spEntityService.mockGetEntity(spEntity.fromJSON({
            id: { id: 88888, ns: 'core', alias: 'description' }
        }));

        spEntityService.mockGetEntity(spEntity.fromJSON({
            id: { id: 99999, ns: 'console', alias: 'navigationElementIcon' }
        }));

        spEntityService.mockGetEntity(spEntity.fromJSON({
            id: { id: 11111, ns: 'core', alias: 'hideOnDesktop' }
        }));

        spEntityService.mockGetEntity(spEntity.fromJSON({
            id: { id: 22222, ns: 'core', alias: 'hideOnTablet' }
        }));

        spEntityService.mockGetEntity(spEntity.fromJSON({
            id: { id: 33333, ns: 'core', alias: 'hideOnMobile' }
        }));


        spEntityService.mockGetEntity(spEntity.fromJSON({
            id: { id: 44444, ns: 'core', alias: 'inSolution' }
        }));

        spEntityService.mockGetEntity(spEntity.fromJSON({
            id: { id: 55555, ns: 'core', alias: 'boardShowQuickAdd' }
        }));

        spEntityService.putEntity = function (entity) {
            var deferred = $q.defer();
            deferred.resolve(12345);

            entityChanges = entity;

            return deferred.promise;
        };
    }));

    // Mock the spEntityHelper service
    beforeEach(inject(function (spEntityHelper, $q) {
        spEntityHelper.promptDelete = function () {
            var deferred = $q.defer();
            deferred.resolve(true);

            return deferred.promise;
        };
    }));

    function createApp1MockData(spNavDataService) {
        var app1TopMenuNode = {}, app1Tab1Node = {}, app1NavSection1Node = {}, app1NavSection2Node = {}, app1Folder1Node = {},
            app1, app1TopMenu, app1Tab1, app1NavSection1, app1NavSection2, app1Folder1;

        app1 = spEntity.fromJSON({
            id: 10000,
            typeId: 'core:solution',
            name: 'Application 1'
        });

        app1TopMenu = spEntity.fromJSON({
            id: 10001,
            typeId: 'console:topMenu',
            'console:consoleOrder': 0,
            'core:inSolution': jsonLookup(app1),
            name: 'Application 1'
        });

        app1Tab1 = spEntity.fromJSON({
            id: 10002,
            typeId: 'console:navSection',
            'console:consoleOrder': 0,
            'core:inSolution': jsonLookup(app1),
            name: 'Application 1',
            'core:hideOnDesktop': false,
            'core:hideOnTablet': false,
            'core:hideOnMobile': false,
            'console:isAppTab': true,
            'console:resourceInFolder': jsonRelationship([app1TopMenu])
        });

        app1NavSection1 = spEntity.fromJSON({
            id: 10003,
            typeId: 'console:navSection',
            'console:consoleOrder': 0,
            'core:inSolution': jsonLookup(app1),
            name: 'Nav Section 1',
            'core:hideOnDesktop': false,
            'core:hideOnTablet': false,
            'core:hideOnMobile': false,
            'console:isAppTab': false,
            'console:resourceInFolder': jsonRelationship([app1Tab1])
        });

        app1NavSection2 = spEntity.fromJSON({
            id: 10004,
            typeId: 'console:navSection',
            'console:consoleOrder': 1,
            'core:inSolution': jsonLookup(app1),
            name: 'Nav Section 2',
            'core:hideOnDesktop': false,
            'core:hideOnTablet':false,
            'core:hideOnMobile': false,
            'console:isAppTab': false,
            'console:resourceInFolder': jsonRelationship([app1Tab1])
        });

        app1Folder1 = spEntity.fromJSON({
            id: 10005,
            typeId: 'console:folder',
            'console:consoleOrder': 2,
            'core:inSolution': jsonLookup(app1),
            name: 'Folder 1',
            'console:resourceInFolder': jsonRelationship([app1Tab1])
        });

        app1TopMenuNode.item = spNavDataService.navItemFromEntity(app1TopMenu);
        app1TopMenuNode.item.depth = 1;
        app1TopMenuNode.children = [app1Tab1Node];

        app1Tab1Node.item = spNavDataService.navItemFromEntity(app1Tab1);
        app1Tab1Node.item.depth = 2;
        app1Tab1Node.children = [app1NavSection1Node, app1NavSection2Node, app1Folder1Node];
        app1Tab1Node.parent = app1TopMenuNode;

        app1NavSection1Node.item = spNavDataService.navItemFromEntity(app1NavSection1);
        app1NavSection1Node.item.depth = 3;
        app1NavSection1Node.children = [];
        app1NavSection1Node.parent = app1Tab1Node;

        app1NavSection2Node.item = spNavDataService.navItemFromEntity(app1NavSection2);
        app1NavSection2Node.item.depth = 3;
        app1NavSection2Node.children = [];
        app1NavSection2Node.parent = app1Tab1Node;

        app1Folder1Node.item = spNavDataService.navItemFromEntity(app1Folder1);
        app1Folder1Node.item.depth = 3;
        app1Folder1Node.children = [];
        app1Folder1Node.parent = app1Tab1Node;

        return app1TopMenuNode;
    }

    function createApp2MockData(spNavDataService) {
        var app2TopMenuNode = {}, app2Tab1Node = {}, app2NavSection1Node = {}, app2NavSection2Node = {},
            app2, app2TopMenu, app2Tab1, app2NavSection1, app2NavSection2;

        app2 = spEntity.fromJSON({
            id: 20000,
            typeId: 'core:solution',
            name: 'Application 2'
        });

        app2TopMenu = spEntity.fromJSON({
            id: 20001,
            typeId: 'console:topMenu',
            'console:consoleOrder': 0,
            'core:inSolution': jsonLookup(app2),
            name: 'Application 2'
        });

        app2Tab1 = spEntity.fromJSON({
            id: 20002,
            typeId: 'console:navSection',
            'console:consoleOrder': 0,
            'core:inSolution': jsonLookup(app2),
            name: 'Application 2',
            'console:isAppTab': true,
            'console:resourceInFolder': jsonRelationship([app2TopMenu])
        });

        app2NavSection1 = spEntity.fromJSON({
            id: 20003,
            typeId: 'console:navSection',
            'console:consoleOrder': 0,
            'core:inSolution': jsonLookup(app2),
            name: 'Nav Section 1',
            'console:isAppTab': false,
            'console:resourceInFolder': jsonRelationship([app2Tab1])
        });

        app2NavSection2 = spEntity.fromJSON({
            id: 20004,
            typeId: 'console:navSection',
            'console:consoleOrder': 1,
            'core:inSolution': jsonLookup(app2),
            name: 'Nav Section 2',
            'console:isAppTab': false,
            'console:resourceInFolder': jsonRelationship([app2Tab1])
        });

        app2TopMenuNode.item = spNavDataService.navItemFromEntity(app2TopMenu);
        app2TopMenuNode.item.depth = 1;
        app2TopMenuNode.children = [app2Tab1Node];

        app2Tab1Node.item = spNavDataService.navItemFromEntity(app2Tab1);
        app2Tab1Node.item.depth = 2;
        app2Tab1Node.children = [app2NavSection1Node, app2NavSection2Node];
        app2Tab1Node.parent = app2TopMenuNode;

        app2NavSection1Node.item = spNavDataService.navItemFromEntity(app2NavSection1);
        app2NavSection1Node.item.depth = 3;
        app2NavSection1Node.children = [];
        app2NavSection1Node.parent = app2Tab1Node;

        app2NavSection2Node.item = spNavDataService.navItemFromEntity(app2NavSection2);
        app2NavSection2Node.item.depth = 3;
        app2NavSection2Node.children = [];
        app2NavSection2Node.parent = app2Tab1Node;

        return app2TopMenuNode;
    }

    beforeEach(inject(function (spNavService, spNavDataService, $q) {
        var app1Node = createApp1MockData(spNavDataService),
            app2Node = createApp2MockData(spNavDataService);

        spNavService.getNavTree = function () {
            return {
                item: null,
                children: [app1Node, app2Node]
            };
        };

        spNavService.getCurrentApplicationId = function () {
            return 12345;
        };

        spNavService.makelink = function (navItem) {
            return '';
        };

        spNavService.requestInitialNavTree = function () {
            var deferred = $q.defer();
            deferred.resolve();

            return deferred.promise;
        };

        spNavService.refreshTreeBranch = function () {
            var deferred = $q.defer();
            deferred.resolve();

            return deferred.promise;
        };

        spNavService.navigateToSibling = function (state, id) {
        };

        spNavService.navigateToParent = function () {
        };

        spNavService.navigateToChildState = function (state, id) {
        };
    }));

    describe('spNavigationBuilderProvider|spec:', function () {

        // Find the nav node with the specified id
        function findNavNode(navItem, id) {
            var result = null;

            // Check if the current nav item is a match
            if (navItem &&
                navItem.item &&
                navItem.item.id === id) {
                return navItem;
            }

            // Enumerate the children
            if (navItem.children) {
                _.forEach(navItem.children, function (c) {
                    result = findNavNode(c, id);
                    if (result) {
                        return false;
                    }
                });
            }

            return result;
        }

        it('removeNavItem view mode', inject(function ($rootScope, spAppSettings, spNavigationBuilderProvider, spEntityHelper, spNavService) {
            spAppSettings.fullConfig = true;
            spAppSettings.selfServeOrAdmin = true;

            var scope = $rootScope,
                navBuilder = spNavigationBuilderProvider(scope),
                deletedNode;

            spNavService.setIsEditMode(false);

            spyOn(spEntityHelper, 'promptDelete').andCallThrough();

            navBuilder.removeNavItem(spEntity.fromJSON({
                    id: 10003
                }
            ));

            // Is in view mode so the delete method should not been called
            expect(spEntityHelper.promptDelete).not.toHaveBeenCalled();

            // Node should not be deleted
            deletedNode = findNavNode(spNavService.getNavTree(), 10003);

            expect(deletedNode).toBeTruthy();
        }));

        it('removeNavItem config mode', inject(function ($rootScope, spAppSettings, spNavigationBuilderProvider, spEntityHelper, spNavService) {
            spAppSettings.fullConfig = true;
            spAppSettings.selfServeOrAdmin = true;

            var scope = $rootScope,
               navBuilder = spNavigationBuilderProvider(scope),
               deletedNode;            

            spNavService.setIsEditMode(true);

            spyOn(spEntityHelper, 'promptDelete').andCallThrough();

            navBuilder.removeNavItem(spEntity.fromJSON({
                    id: 10003
                }
            ));

            scope.$apply();

            // Is in config mode so the delete method should been called
            expect(spEntityHelper.promptDelete).toHaveBeenCalled();

            // Node should be deleted
            deletedNode = findNavNode(spNavService.getNavTree(), 10003);

            expect(deletedNode).toBeFalsy();
        }));

        it('allow move nav section view mode', inject(function ($rootScope, spAppSettings, spNavigationBuilderProvider, spNavService) {
            spAppSettings.fullConfig = true;
            spAppSettings.selfServeOrAdmin = true;

            var scope = $rootScope,
                navBuilder = spNavigationBuilderProvider(scope),
                navSection1, navSection2;
            
            scope.getSelectedTabNode = function () {
                return findNavNode(spNavService.getNavTree(), 10002);
            };
            scope.$$childHead.getSelectedTabNode = scope.getSelectedTabNode;

            navSection1 = findNavNode(spNavService.getNavTree(), 10003);
            navSection2 = findNavNode(spNavService.getNavTree(), 10004);

            spNavService.setIsEditMode(false);

            expect(navBuilder.allowDropItem(null, null, navSection1, navSection2)).toBe(false);
        }));

        it('allow move nav section config mode', inject(function ($rootScope, spAppSettings, spNavigationBuilderProvider, spNavService) {
            spAppSettings.fullConfig = true;
            spAppSettings.selfServeOrAdmin = true;

            var scope = $rootScope,
                navBuilder = spNavigationBuilderProvider(scope),
                navSection1, navSection2;

            scope.getSelectedTabNode = function () {
                return findNavNode(spNavService.getNavTree(), 10002);
            };
            scope.$$childHead.getSelectedTabNode = scope.getSelectedTabNode;

            navSection1 = findNavNode(spNavService.getNavTree(), 10003);
            navSection2 = findNavNode(spNavService.getNavTree(), 10004);

            spNavService.setIsEditMode(true);

            expect(navBuilder.allowDropItem(null, null, navSection1, navSection2)).toBe(true);
        }));

        it('allow move topMenu within app dropdown', inject(function ($rootScope, spAppSettings, spNavigationBuilderProvider, spNavService) {
            spAppSettings.fullConfig = true;
            spAppSettings.selfServeOrAdmin = true;

            var scope = $rootScope,
                navBuilder = spNavigationBuilderProvider(scope),
                topMenu1, topMenu2;

            scope.getSelectedTabNode = function () {
                return findNavNode(spNavService.getNavTree(), 10002);
            };
            scope.$$childHead.getSelectedTabNode = scope.getSelectedTabNode;

            topMenu1 = findNavNode(spNavService.getNavTree(), 10001);
            topMenu2 = findNavNode(spNavService.getNavTree(), 20001);

            spNavService.setIsEditMode(true);

            expect(navBuilder.allowDropItem(null, null, topMenu1, topMenu2)).toBe(true);
        }));

        it('allow move topMenu outside app dropdown', inject(function ($rootScope, spAppSettings, spNavigationBuilderProvider, spNavService) {
            spAppSettings.fullConfig = true;
            spAppSettings.selfServeOrAdmin = true;

            var scope = $rootScope,
                navBuilder = spNavigationBuilderProvider(scope),
                topMenu1, navSection1;

            scope.getSelectedTabNode = function () {
                return findNavNode(spNavService.getNavTree(), 10002);
            };
            scope.$$childHead.getSelectedTabNode = scope.getSelectedTabNode;

            topMenu1 = findNavNode(spNavService.getNavTree(), 20001);
            navSection1 = findNavNode(spNavService.getNavTree(), 10003);

            spNavService.setIsEditMode(true);

            expect(navBuilder.allowDropItem(null, null, topMenu1, navSection1)).toBe(false);
            expect(navBuilder.allowDropItem(null, null, navSection1, topMenu1)).toBe(false);
        }));

        it('dragOverItem navSection to navSection', inject(function ($rootScope, spAppSettings, spNavigationBuilderProvider, spNavService, $document) {
            spAppSettings.fullConfig = true;
            spAppSettings.selfServeOrAdmin = true;

            var scope = $rootScope,
                navBuilder = spNavigationBuilderProvider(scope),
                navSection1, navSection2,
                navInsertionIndicator,
                event = {
                    originalEvent: {
                        clientX: 0,
                        clientY: 2
                    }
                },
                target = {
                    clientHeight: 20,
                    clientWidth: 20,
                    getBoundingClientRect: function () {
                        return {
                            bottom: 20,
                            height: 20,
                            left: 0,
                            right: 20,
                            top: 0,
                            width: 20
                        };
                    }
                };

            scope.getSelectedTabNode = function () {
                return findNavNode(spNavService.getNavTree(), 10002);
            };
            scope.$$childHead.getSelectedTabNode = scope.getSelectedTabNode;

            navSection1 = findNavNode(spNavService.getNavTree(), 10003);
            navSection2 = findNavNode(spNavService.getNavTree(), 10004);

            spNavService.setIsEditMode(true);

            navBuilder.dragOverItem(event, null, target, navSection1, navSection2);

            scope.$apply();

            navInsertionIndicator = $document.find('body').find('div.navInsertionIndicator');

            expect(navInsertionIndicator.length).toBe(1);

            navBuilder.dragEnd();

            scope.$apply();

            navInsertionIndicator = $document.find('body').find('div.navInsertionIndicator');

            expect(navInsertionIndicator.length).toBe(0);
        }));

        it('configure navItem - navSection', inject(function ($rootScope, spAppSettings, spNavigationBuilderProvider, spNavService) {
            spAppSettings.fullConfig = true;
            spAppSettings.selfServeOrAdmin = true;

            var scope = $rootScope,
                navBuilder = spNavigationBuilderProvider(scope),
                navSection1,
                originalName;
          
            navSection1 = findNavNode(spNavService.getNavTree(), 10003);

            originalName = navSection1.item.name;

            spNavService.setIsEditMode(true);

            navBuilder.configureNavItem(navSection1.item.entity);

            scope.$apply();

            expect(navSection1.item.name).toBe(originalName + '-Modified');
            expect(navSection1.item.entity.name).toBe(originalName + '-Modified');
        }));

        it('canAddNewTab works', inject(function ($rootScope, spAppSettings, spNavigationBuilderProvider, spNavService) {
            spAppSettings.fullConfig = true;
            spAppSettings.selfServeOrAdmin = true;

            var scope = $rootScope,
                navBuilder = spNavigationBuilderProvider(scope);

            scope.getSelectedMenuNode = function () {
                return findNavNode(spNavService.getNavTree(), 10001);
            };
            scope.$$childHead.getSelectedMenuNode = scope.getSelectedMenuNode;
           
            spNavService.setIsEditMode(false);
            expect(navBuilder.canAddNewTab()).toBe(false);

            spNavService.setIsEditMode(true);
            expect(navBuilder.canAddNewTab()).toBe(true);            
        }));

        it('addNewTab works', inject(function ($rootScope, spAppSettings, spNavigationBuilderProvider, spNavService) {
            spAppSettings.fullConfig = true;
            spAppSettings.selfServeOrAdmin = true;

            var scope = $rootScope,
                navBuilder = spNavigationBuilderProvider(scope),
                menuNode = findNavNode(spNavService.getNavTree(), 10001);

            scope.getSelectedMenuNode = function () {
                return menuNode;
            };
            scope.$$childHead.getSelectedMenuNode = scope.getSelectedMenuNode;

            spNavService.setIsEditMode(true);
            navBuilder.addNewTab();

            scope.$apply();

            expect(menuNode.children.length).toBe(2);
            expect(_.last(menuNode.children).item.name).toBe('New Tab-Modified');
        }));

        it('dropItem - reorder navSection before navSection', inject(function ($rootScope, spAppSettings, spNavigationBuilderProvider, spNavService) {
            spAppSettings.fullConfig = true;
            spAppSettings.selfServeOrAdmin = true;

            var scope = $rootScope,
                navBuilder = spNavigationBuilderProvider(scope),
                navSection1, navSection2,
                navInsertionIndicator,
                event = {
                    originalEvent: {
                        clientX: 0,
                        clientY: 2
                    }
                },
                target = {
                    clientHeight: 20,
                    clientWidth: 20,
                    getBoundingClientRect: function () {
                        return {
                            bottom: 20,
                            height: 20,
                            left: 0,
                            right: 20,
                            top: 0,
                            width: 20
                        };
                    }
                };

            scope.middleBusy = {
                isBusy: false    
            };

            scope.getSelectedTabNode = function () {
                return findNavNode(spNavService.getNavTree(), 10002);
            };
            scope.$$childHead.getSelectedTabNode = scope.getSelectedTabNode;

            navSection1 = findNavNode(spNavService.getNavTree(), 10003);
            navSection2 = findNavNode(spNavService.getNavTree(), 10004);

            expect(navSection1.item.order).toBeLessThan(navSection2.item.order);

            spNavService.setIsEditMode(true);

            navBuilder.dropItem(event, null, target, navSection2, navSection1);

            scope.$apply();
            
            navBuilder.dragEnd();

            scope.$apply();

            var navSection1Entity = _.find(entityChanges.folderContents, function (e) {
                return e.id() === 10003;
            });

            var navSection2Entity = _.find(entityChanges.folderContents, function (e) {
                return e.id() === 10004;
            });

            expect(navSection2Entity.consoleOrder).toBeLessThan(navSection1Entity.consoleOrder);
        }));

        it('dropItem - reorder navSection after navSection', inject(function ($rootScope, spAppSettings, spNavigationBuilderProvider, spNavService) {
            spAppSettings.fullConfig = true;
            spAppSettings.selfServeOrAdmin = true;

            var scope = $rootScope,
                navBuilder = spNavigationBuilderProvider(scope),
                navSection1, navSection2,
                navInsertionIndicator,
                event = {
                    originalEvent: {
                        clientX: 0,
                        clientY: 12
                    }
                },
                target = {
                    clientHeight: 20,
                    clientWidth: 20,
                    getBoundingClientRect: function () {
                        return {
                            bottom: 20,
                            height: 20,
                            left: 0,
                            right: 20,
                            top: 0,
                            width: 20
                        };
                    }
                };

            scope.middleBusy = {
                isBusy: false
            };

            scope.getSelectedTabNode = function () {
                return findNavNode(spNavService.getNavTree(), 10002);
            };
            scope.$$childHead.getSelectedTabNode = scope.getSelectedTabNode;

            navSection1 = findNavNode(spNavService.getNavTree(), 10003);
            navSection2 = findNavNode(spNavService.getNavTree(), 10004);

            expect(navSection1.item.order).toBeLessThan(navSection2.item.order);

            spNavService.setIsEditMode(true);

            navBuilder.dropItem(event, null, target, navSection1, navSection2);

            scope.$apply();

            navBuilder.dragEnd();

            scope.$apply();

            var navSection1Entity = _.find(entityChanges.folderContents, function (e) {
                return e.id() === 10003;
            });

            var navSection2Entity = _.find(entityChanges.folderContents, function (e) {
                return e.id() === 10004;
            });

            expect(navSection2Entity.consoleOrder).toBeLessThan(navSection1Entity.consoleOrder);
        }));

        it('dropItem - reorder folder inside navSection', inject(function ($rootScope, spAppSettings, spNavigationBuilderProvider, spNavService) {
            spAppSettings.fullConfig = true;
            spAppSettings.selfServeOrAdmin = true;

            var scope = $rootScope,
                navBuilder = spNavigationBuilderProvider(scope),
                navSection1, folder1,
                navInsertionIndicator,
                event = {
                    originalEvent: {
                        clientX: 0,
                        clientY: 10
                    }
                },
                target = {
                    clientHeight: 20,
                    clientWidth: 20,
                    getBoundingClientRect: function () {
                        return {
                            bottom: 20,
                            height: 20,
                            left: 0,
                            right: 20,
                            top: 0,
                            width: 20
                        };
                    }
                };

            scope.middleBusy = {
                isBusy: false
            };

            scope.getSelectedTabNode = function () {
                return findNavNode(spNavService.getNavTree(), 10002);
            };
            scope.$$childHead.getSelectedTabNode = scope.getSelectedTabNode;

            navSection1 = findNavNode(spNavService.getNavTree(), 10003);
            folder1 = findNavNode(spNavService.getNavTree(), 10005);

            expect(navSection1.item.order).toBeLessThan(folder1.item.order);

            spNavService.setIsEditMode(true);

            navBuilder.dropItem(event, null, target, folder1, navSection1);

            scope.$apply();

            navBuilder.dragEnd();

            scope.$apply();            

            expect(entityChanges.id()).toBe(10003);

            expect(entityChanges.folderContents[0].id()).toBe(10005);
        }));
    });
});