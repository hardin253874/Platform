// Copyright 2011-2016 Global Software Innovation Pty Ltd
describe('Console|Controls|spContextMenu|spec:|spContextMenu directive', function () {
    'use strict';

    // Load the modules        
    beforeEach(function () {        
        var actionsServiceStub = {
            getConsoleActions: function () {
                return q.defer().promise;
            }
        };

        module('mod.common.ui.spContextMenu', function ($provide) {            
            $provide.value('spActionsService', actionsServiceStub);
        });
    });

    it('should display context menu', inject(function ($rootScope, $compile, $document) {
        var scope = $rootScope,
            element,
            contextMenu,
            ul,
            li;        

        // Setup the grid options        
        scope.menuConfig = {
            menuItems: [
                {
                    icon: 'assets/images/16x16/DownArrow.png',
                    text: "Menu Item 1",
                    type: 'href',
                    href: "#menuItem1Href"
                },
                {
                    type: 'click',
                    text: "Menu Item 2",
                    click: 'menuItem2Click()'
                },
                {
                    type: 'divider'
                },
                {
                    text: "Sub Menu",
                    submenu: [
                        {
                            text: "Sub Menu 1",
                        },
                        {
                            text: "Sub Menu 2",
                        }
                    ]
                }
            ]
        };

        element = angular.element('<button sp-context-menu="menuConfig" sp-context-menu-trigger="leftclick"  type="button">Show Context Menu</button>');
        $compile(element)(scope);
        scope.$digest();

        expect(element.html()).toBe('Show Context Menu');

        // Ensure context menu is now visible
        expect(element.next().length).toBe(0);

        element.click();

        scope.$digest();

        // Context menu should now be visible        

        expect(element.next().length).toBe(1);
        contextMenu = element.next().first();        

        ul = contextMenu.first();
        expect(ul.hasClass('dropdown-menu')).toBe(true);
        expect(ul.length).toBe(1);

        li = ul.find('li');

        expect(li.length).toBe(6);

        expect(angular.element(li[0]).find('a').first().text()).toBe('Menu Item 1');
        expect(angular.element(li[0]).find('a').first().attr('ng-href')).toBe('#menuItem1Href');        
        expect(angular.element(li[1]).find('a').first().text()).toBe('Menu Item 2');
        expect(angular.element(li[1]).find('a').first().attr('ng-click')).toBe('menuItem2Click()');
        expect(angular.element(li[2]).hasClass('contextMenuDivider')).toBe(true);
        expect(angular.element(li[3]).find('a').first().text()).toBe('Sub Menu');
        expect(angular.element(li[3]).hasClass('dropdown-submenu')).toBe(true);
        expect(angular.element(li[4]).find('a').first().text()).toBe('Sub Menu 1');
        expect(angular.element(li[5]).find('a').first().text()).toBe('Sub Menu 2');

        // Ensure context menu is hidden
        ul.click();

        scope.$digest();

        expect(element.next().length).toBe(0);
    }));
    
    it('should display context menu and set is open', inject(function ($rootScope, $compile, $document) {
        var scope = $rootScope,
            element;

        scope.isOpen = false;

        // Setup the grid options        
        scope.menuConfig = {
            menuItems: [
                {
                    icon: 'assets/images/16x16/DownArrow.png',
                    text: "Menu Item 1",
                    type: 'href',
                    href: "#menuItem1Href"
                },
                {
                    type: 'click',
                    text: "Menu Item 2",
                    click: 'menuItem2Click()'
                },
                {
                    type: 'divider'
                },
                {
                    text: "Sub Menu",
                    submenu: [
                        {
                            text: "Sub Menu 1",
                        },
                        {
                            text: "Sub Menu 2",
                        }
                    ]
                }
            ]
        };

        element = angular.element('<button sp-context-menu="menuConfig" sp-context-menu-trigger="leftclick" sp-context-menu-is-open="isOpen" type="button">Show Context Menu</button>');
        $compile(element)(scope);
        scope.$digest();

        expect(element.html()).toBe('Show Context Menu');
        expect(scope.isOpen).toBe(false);

        // Ensure context menu is visible        
        element.click();

        scope.$digest();

        // Context menu should now be visible        

        expect(scope.isOpen).toBe(true);

        // Ensure context menu is hidden
        var contextMenu = element.next().first();

        var ul = contextMenu.first();
        ul.click();

        scope.$digest();

        expect(scope.isOpen).toBe(false);
    }));

    it('should update context menu', inject(function ($rootScope, $compile, $document) {
        var scope = $rootScope,
            element,
            contextMenu,
            ul,
            li;

        // Setup the grid options        
        scope.menuConfig = {
            menuItems: [
                {
                    icon: 'assets/images/16x16/DownArrow.png',
                    text: "Menu Item 1",
                    type: 'href',
                    href: "#menuItem1Href"
                },
                {
                    type: 'click',
                    text: "Menu Item 2",
                    click: 'menuItem2Click()'
                },
                {
                    type: 'divider'
                },
                {
                    text: "Sub Menu",
                    submenu: [
                        {
                            text: "Sub Menu 1",
                        },
                        {
                            text: "Sub Menu 2",
                        }
                    ]
                }
            ]
        };

        element = angular.element('<button sp-context-menu="menuConfig" sp-context-menu-trigger="leftclick" type="button">Show Context Menu</button>');
        $compile(element)(scope);
        scope.$digest();

        expect(element.html()).toBe('Show Context Menu');

        // Ensure context menu is now visible
        expect(element.next().length).toBe(0);

        element.click();

        scope.$digest();

        // Context menu should now be visible        

        expect(element.next().length).toBe(1);
        contextMenu = element.next().first();

        ul = contextMenu.first();
        expect(ul.hasClass('dropdown-menu')).toBe(true);
        expect(ul.length).toBe(1);

        li = ul.find('li');

        expect(li.length).toBe(6);

        expect(angular.element(li[0]).find('a').first().text()).toBe('Menu Item 1');
        expect(angular.element(li[0]).find('a').first().attr('ng-href')).toBe('#menuItem1Href');
        expect(angular.element(li[1]).find('a').first().text()).toBe('Menu Item 2');
        expect(angular.element(li[1]).find('a').first().attr('ng-click')).toBe('menuItem2Click()');
        expect(angular.element(li[2]).hasClass('contextMenuDivider')).toBe(true);
        expect(angular.element(li[3]).find('a').first().text()).toBe('Sub Menu');
        expect(angular.element(li[3]).hasClass('dropdown-submenu')).toBe(true);
        expect(angular.element(li[4]).find('a').first().text()).toBe('Sub Menu 1');
        expect(angular.element(li[5]).find('a').first().text()).toBe('Sub Menu 2');

        // Ensure context menu is hidden
        ul.click();

        scope.$digest();

        expect(element.next().length).toBe(0);

        scope.$apply(function () {
            // Change the text
            scope.menuConfig.menuItems[0].text = 'Menu Item 1 Updated';            
        });

        element.click();

        scope.$digest();

        // Context menu should now be visible        

        expect(element.next().length).toBe(1);
        contextMenu = element.next().first();

        ul = contextMenu.first();
        expect(ul.hasClass('dropdown-menu')).toBe(true);
        expect(ul.length).toBe(1);

        li = ul.find('li');

        expect(li.length).toBe(6);

        expect(angular.element(li[0]).find('a').first().text()).toBe('Menu Item 1 Updated');
        expect(angular.element(li[0]).find('a').first().attr('ng-href')).toBe('#menuItem1Href');
        expect(angular.element(li[1]).find('a').first().text()).toBe('Menu Item 2');
        expect(angular.element(li[1]).find('a').first().attr('ng-click')).toBe('menuItem2Click()');
        expect(angular.element(li[2]).hasClass('contextMenuDivider')).toBe(true);
        expect(angular.element(li[3]).find('a').first().text()).toBe('Sub Menu');
        expect(angular.element(li[3]).hasClass('dropdown-submenu')).toBe(true);
        expect(angular.element(li[4]).find('a').first().text()).toBe('Sub Menu 1');
        expect(angular.element(li[5]).find('a').first().text()).toBe('Sub Menu 2');
    }));

    it('should display context menu- append to body', inject(function ($rootScope, $compile, $document) {
        var scope = $rootScope,
            element,
            contextMenu,
            ul,
            li;

        // Setup the grid options        
        scope.menuConfig = {
            menuItems: [
                {
                    icon: 'assets/images/16x16/DownArrow.png',
                    text: "Menu Item 1",
                    type: 'href',
                    href: "#menuItem1Href",
                    disabled: false
                },
                {
                    type: 'click',
                    text: "Menu Item 2",
                    click: 'menuItem2Click()'
                },
                {
                    type: 'divider'
                },
                {
                    text: "Sub Menu",
                    submenu: [
                        {
                            text: "Sub Menu 1",
                        },
                        {
                            text: "Sub Menu 2",
                        }
                    ]
                }
            ]
        };

        element = angular.element('<button sp-context-menu="menuConfig" type="button" sp-context-menu-trigger="leftclick" sp-context-menu-append-to-body>Show Context Menu</button>');
        $compile(element)(scope);
        scope.$digest();

        expect(element.html()).toBe('Show Context Menu');

        // Ensure context menu is now visible
        expect(element.find('ul').length).toBe(0);
        expect($document.find('body').find('ul.dropdown-menu.contextmenu-view').length).toBe(0);

        element.click();

        scope.$digest();

        // Context menu should now be visible but be in the body   
        expect(element.find('ul').length).toBe(0);
        
        expect($document.find('body').find('ul.dropdown-menu.contextmenu-view').length).toBe(2);

        contextMenu = $document.find('body').find('ul.dropdown-menu.contextmenu-view').first();

        ul = contextMenu.first();
        expect(ul.length).toBe(1);

        li = ul.find('li');

        expect(li.length).toBe(6);

        expect(angular.element(li[0]).find('a').first().text()).toBe('Menu Item 1');
        expect(angular.element(li[0]).find('a').first().attr('ng-href')).toBe('#menuItem1Href');
        expect(angular.element(li[1]).find('a').first().text()).toBe('Menu Item 2');
        expect(angular.element(li[1]).find('a').first().attr('ng-click')).toBe('menuItem2Click()');
        expect(angular.element(li[2]).hasClass('contextMenuDivider')).toBe(true);
        expect(angular.element(li[3]).find('a').first().text()).toBe('Sub Menu');
        expect(angular.element(li[3]).hasClass('dropdown-submenu')).toBe(true);
        expect(angular.element(li[4]).find('a').first().text()).toBe('Sub Menu 1');
        expect(angular.element(li[5]).find('a').first().text()).toBe('Sub Menu 2');

        // Ensure context menu is hidden
        ul.click();

        scope.$digest();

        expect(element.next().length).toBe(0);
        expect($document.find('body').find('ul.dropdown-menu.contextmenu-view').length).toBe(0);
    }));    
});