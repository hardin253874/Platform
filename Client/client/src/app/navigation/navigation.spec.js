// Copyright 2011-2016 Global Software Innovation Pty Ltd

describe('Console|Navigation|UI|spec:', function() {
    "use strict";

    var controller, $scope;
    var mockNavTree = {
        item: null,
        children:
            [
                {
                    item: {
                        id: 1,
                        name: "1",
                        depth: 0,
                        href: "",
                        showTabs: false,
                        selected: false
                    },
                    children:
                        [
                            {
                                item: {
                                    id: 11,
                                    name: "11",
                                    depth: 1,
                                    href: "",
                                    showTabs: false,
                                    selected: false
                                },
                                children:
                                    [
                                        {
                                            item: {
                                                id: 111,
                                                name: "111",
                                                depth: 2,
                                                href: "",
                                                showTabs: false,
                                                selected: false
                                            },
                                            children: []
                                        },
                                        {
                                            item: {
                                                id: 112,
                                                name: "112",
                                                depth: 2,
                                                href: "",
                                                showTabs: false,
                                                selected: false
                                            },
                                            children: []
                                        }
                                    ]
                            },
                            {
                                item: {
                                    id: 12,
                                    name: "12",
                                    depth: 1,
                                    href: "",
                                    showTabs: false,
                                    selected: false
                                },
                                children: []
                            }
                        ]
                },
                {
                    item: {
                        id: 2,
                        name: "2",
                        depth: 1,
                        href: "",
                        showTabs: false,
                        selected: false
                    }
                }
            ]
    };

    beforeEach(module('sp.app.navigation'));
    beforeEach(module('mockedEntityService'));

    beforeEach(inject(function (spEntityService) {
        spEntityService.mockGetEntityJSON(
            [
                {
                    id: 'core:fullConfigButton'
                },
                {
                    id: 'core:fullConfigButton'
                },
                {
                    id: 'core:selfServiceConfigButton'
                },
                {
                    id: 'console:adminToolboxStaticPage'
                }
            ]
            );
    }));

    beforeEach(inject(function($controller, $rootScope, spNavService, spEntityService) {
        // Replace getNavTree to allow unit testing. Ignore the 
        // item argument as calling with a specific item is
        // not supported.
        spNavService.getNavTree = function() {
            return mockNavTree;
        };

        $scope = $rootScope.$new();
        controller = $controller('NavController', { $scope: $scope, spNavService: spNavService });
    }));

    // test is not great. replacing a method on the service doesn't really work
    it('navigation controller was created ok', inject(function (spEntityService) {
        console.log('nav controller scope', $scope);
        expect(controller).toBeTruthy();
        expect($scope.nav).toBeTruthy();
        expect($scope.getMenuItems()).toBeTruthy();
        expect($scope.getMenuItems().length).toBe(2);
    }));

    // test is not great. replacing a method on the service doesn't really work
    it("constructs a tree", inject(function (spEntityService) {
        var menuItems;

        menuItems = $scope.getMenuItems();
        expect(menuItems.length).toBe(2);
        expect(menuItems[0].id).toBe(1);
        expect(menuItems[1].id).toBe(2);
    }));
});
