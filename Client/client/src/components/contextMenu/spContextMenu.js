// Copyright 2011-2016 Global Software Innovation Pty Ltd
/*global _, console, angular, $, sp */

(function () {
    'use strict';

    /**
    * Module implementing a context menu.
    * Displays a context menu
    * 
    * @module spContextMenu
    * @example
        
    Using the spContextMenu:
    
    &lt;button sp-context-menu="contextMenu"&gt;&lt;/button&gt

    This will display a context menu when the button is clicked.

    where contextMenu is available on the scope with the following properties:
        - menuItems {array of objects} - the menu items to display in the context menu.
            - menuItems[].text - {string}. The menu item display text.
            - menuItems[].icon - {string}. The menu item icon.
            - menuItems[].type - {string}. The menu item type. Either 'click', 'href', 'divider'.
            - menuItems[].click - {string}. Only valid for type='click' items. The expression to run when the menu item is selected.
            - menuItems[].href - {string}. Only valid for type='href' items. The url to navigate to when the menu item is selected.
            - menuItems[].disabled - {bool expression}. True if the menu item is disabled, false otherwise.
            - menuItems[].hidden - {bool expression}. True if the menu item is hidden, false otherwise.
            - menuItems[].submenu - {array of menu items}. The sub menu items to display for this menu item.            
    
    The spContextMenu directive also supports the following optional attributes:
        - sp-context-menu-append-to-body - If this attribute exists the context menu is appended to the document body. 
        - This is useful in scenarios where the parent element clips the context menu.
            - e.g. &lt;button sp-context-menu="contextMenu" sp-context-menu-append-to-body&gt;&lt;/button&gt

        - sp-context-menu-is-open - This attribute allows the parent scope to determine if the context menu is open or not.
            - e.g. &lt;button sp-context-menu="contextMenu" sp-context-menu-is-open="isOpen"&gt;&lt;/button&gt            

        - sp-context-menu-placement - This attribute controls the placement of the context menu.
        - Valid values are 'mouse', 'alignleft', 'alignright'. Default is 'alignright'.
            - mouse: the context menu is displayed at the mouse position
            - alignleft: the context menu is aligned with the left hand edge of the parent element
            - alignright: the context menu is aligned with the right hand edge of the parent control
            - e.g. &lt;button sp-context-menu="contextMenu" sp-context-menu-placement="alignleft"&gt;&lt;/button&gt

        - sp-context-menu-trigger - This attribute controls what should trigger the context menu to display. 
        - Valid values are 'leftclick','rightclick'. Default is 'leftclick'
            - e.g. &lt;button sp-context-menu="contextMenu" sp-context-menu-trigger="rightclick"&gt;&lt;/button&gt
    * 
    * @example
    Sample contextMenu that displays a context menu with two items, a divider and a submenu.
    
    var contextMenu =
        {
            menuItems: [
                {
                    text: 'MenuItem1',
                    icon: 'images/menuitem1.png',
                    type: 'click',
                    click: 'doMenuItem1()'
                },
                {
                    text: 'MenuItem2',                    
                    type: 'href',
                    href: '/view2'
                },
                {
                    type: 'divider'
                },
                {
                    text: "Sub Menu",
                    submenu: [
                        {
                            text: "SubMenuItem1",
                            type: 'click',
                            click: 'doSubMenuItem1()'
                        },
                        {
                            text: "SubMenuItem2",
                            type: 'click',
                            click: 'doSubMenuItem2()'
                        }
                    ]
            ]
        };
    */

    angular.module('mod.common.ui.spContextMenu', ['ui.bootstrap.position', 'mod.common.ui.spActionsService', 'hmTouchEvents', 'mod.common.spMobile'])

        .service('spContextMenuService', function (spActionsService) {
           
            // Returns menu items for display from a list of console actions
            this.getItemsFromActions = buildContextMenuFromActions;
            this.getConsoleActionsAsMenu = getConsoleActionsAsMenuImpl;

            // -- Private --

            // Builds the context menu from the actions defined
            function getConsoleActionsAsMenuImpl(actionRequest) {
                return spActionsService.getConsoleActions(actionRequest)
                            .then(function (actionResponse) {
                                var menuKey = '';
                                if (actionRequest.display) {
                                    menuKey += actionRequest.display;
                                }
                                if (actionRequest.hostIds && actionRequest.hostIds.length > 0) {
                                    menuKey += actionRequest.hostIds.join('');
                                }
                                var items = buildContextMenuFromActions(actionResponse.actions, menuKey);
                                return items;
                            }, function (reason) {
                                console.error('getConsoleActionsAsMenu error:', reason);
                                throw reason;
                            });
            }

            

            

            // Builds the context menu from the actions defined
            function buildContextMenuFromActions(actions, parent) {
                var menuItems = [];

                _.forEach(actions, function(action) {
                    if (action.isseparator) {
                        menuItems.push({
                            type: 'divider'
                        });
                    } else {
                        var id = '' + menuItems.length;
                        if (parent) {
                            id = parent + ',' + id;
                        }
                        menuItems.push({
                            id: action.eid,
                            text: action.displayname || action.name,
                            icon: action.icon,
                            disabled: angular.isDefined(action.isenabled) && !action.isenabled,
                            submenu: buildContextMenuFromActions(action.children, id),
                            type: angular.isDefined(action.url) && action.url != null ? 'href' : 'click',
                            action: action,
                            click: 'onItemClick(\'' + id + '\')'
                        });
                    }
                });

                return menuItems;
            }
        })

        .directive('spContextMenu', function ($parse, $compile, $uibPosition, $document, $timeout, spActionsService, spMobileContext) {
            return {
                restrict: 'A',                
                link: function (originalScope, iElement, iAttrs) {
                    var body,                        
                        appendToBody,
                        placement,                        
                        contextMenu,
                        contextMenuCreator,
                        getterContextMenuConfig,
                        contextMenuConfig,
                        getterContextMenuIsOpen,
                        setterContextMenuIsOpen,
                        lastClickTimestamp = -1,
                        lastClickX = -1,
                        lastClickY = -1,
                        scope = originalScope.$new(),
                        contextMenuScope,
                        loadingItem = [{ text: 'Loading...', disabled: true }];

                    var addPressHandler = _.partial(spUtils.addHammerHandler, 'press');
                    var removePressHandler = _.partial(spUtils.removeHammerHandler, 'press');

                    // Expose the ability to open the menu ... mainly for testing.
                    originalScope.showContextMenuForTest = showContextMenu;

                    // Get the options from the attributes
                    // The context menu placement
                    iAttrs.$observe('spContextMenuPlacement', function (value) {
                        placement = angular.lowercase(value);
                    });

                    // The trigger that displays the context menu
                    iAttrs.$observe('spContextMenuTrigger', function (value) {
                        updateTrigger(value);
                    });
                    
                    // Whether to append the context menu to the document body or after the parent element
                    appendToBody = angular.isDefined(iAttrs.spContextMenuAppendToBody);

                    // Getter/setter for passing the is open state outside of this directive
                    if (iAttrs.spContextMenuIsOpen) {
                        getterContextMenuIsOpen = $parse(iAttrs.spContextMenuIsOpen);
                        setterContextMenuIsOpen = getterContextMenuIsOpen.assign;

                        scope.$watch(getterContextMenuIsOpen, function (isOpen) {
                            // only show or hide if it needs to change
                            if (isOpen !== scope.isOpen) {
                                if (isOpen) {
                                    showContextMenu();
                                } else {
                                    hideContextMenu();
                                }
                            }
                        });
                    }

                    // Getter/setter for the context menu itself
                    getterContextMenuConfig = $parse(iAttrs.spContextMenu);
                    contextMenuConfig = getterContextMenuConfig(scope);

                    scope.$watch(getterContextMenuConfig, function (config) {
                        // The context menu items
                        contextMenuConfig = config;
                    });

                    // Wire up the menu item callback if one is set
                    if (iAttrs.spContextMenuCreate) {
                        var getterContextMenuCreator = $parse(iAttrs.spContextMenuCreate);
                        scope.$watch(getterContextMenuCreator, function (creator) {
                            contextMenuCreator = creator;
                        });
                    }

                    // Start with the context menu closed
                    setContextMenuIsOpen(false);

                    // if a context menu is attached to <body> we need to remove it on
                    // location change.
                    if (appendToBody) {
                        scope.$on('$locationChangeSuccess', function () {
                            hideContextMenu();
                        });
                    }

                    // Make sure context menu is destroyed and removed.
                    originalScope.$on('$destroy', function () {                        
                        hideContextMenu();
                        scope.$destroy();

                        removeOpenTriggerHandlers(iElement);
                    });

                    scope.onItemClick = function (itemId) {
                        hideContextMenuBind();  // close the context menu
                        var getActionExecutionContext = $parse(iAttrs.spContextMenuExecutionContext);
                        var getContext = getActionExecutionContext(scope);
                        spActionsService.executeItem(itemId, getContext);
                    };

                    scope.backdropClick = function (event) {
                        hideContextMenuBind();                        
                        if (event.srcEvent) {
                            event.srcEvent.preventDefault();
                            event.srcEvent.stopPropagation();
                        } else {
                            event.preventDefault();
                            event.stopPropagation();
                        }
                    };

                    function isMobileDevice() {
                        return spMobileContext.isMobile;
                    }

                    function isTabletDevice() {
                        return spMobileContext.isTablet;
                    }

                    function isTouchDevice() {
                        return isMobileDevice() || isTabletDevice();
                    }

                    function convertIconUrl(icon) {
                        if (isTouchDevice()) {
                            if (icon.indexOf('16x16') !== -1) {
                                icon = icon.replace('16x16', 'mobile');
                            } else {
                                icon = icon.replace('assets/images/', 'assets/images/mobile/');
                            }
                        }

                        return icon;
                    }

                    // Private functions                 

                    function updateTrigger(trigger) {

                        // unbind handlers
                        removeOpenTriggerHandlers(iElement);
                        addOpenTriggerHandlers(iElement, trigger);
                    }

                    


                    // Set the context menu is open field
                    function setContextMenuIsOpen(value) {
                        scope.isOpen = value;
                        // Propagate the is open value to the parent scope
                        if (setterContextMenuIsOpen) {
                            setterContextMenuIsOpen(originalScope, value);
                        }
                    }

                    // Builds the context menu
                    function buildContextMenu() {
                        if (contextMenuCreator && angular.isFunction(contextMenuCreator)) {
                            buildContextMenuCallback();
                            return buildContextMenuElement(loadingItem);
                        } else if (contextMenuConfig && contextMenuConfig.menuItemsCallback) {
                            var menuItems = contextMenuConfig.menuItemsCallback();
                            return buildContextMenuElement(menuItems);
                        } else if (contextMenuConfig && contextMenuConfig.menuItems) {
                            return buildContextMenuElement(contextMenuConfig.menuItems);
                        }
                    }
                    
                    function buildContextMenuElement(items) {
                        if (!items || items.length <= 0) {
                            return null;
                        }

                        var html = buildContextMenuHtml(0, contextMenuScope, items);
                        var linkingFunction = $compile(html);
                        var elem = linkingFunction(contextMenuScope);

                        return elem;
                    }
                    
                    function buildContextMenuCallback() {
                        if (contextMenuCreator && angular.isFunction(contextMenuCreator)) {
                            contextMenuCreator(contextMenuScope).
                                then(function (items) {
                                    if (contextMenu) {
                                        contextMenu.remove();
                                        contextMenu = null;
                                    }
                                    
                                    if (!items || items.length <= 0) {
                                        hideContextMenu();
                                        return;
                                    }

                                    contextMenu = buildContextMenuElement(items);

                                    if (contextMenu == null) {
                                        return;
                                    }

                                    contextMenu.css({ top: 0, left: 0, display: 'block' });

                                    // Add it
                                    body = body || $document.find('body');
                                    body.append(contextMenu);

                                    // touch devices have fixed context menu - additionally this call was slowing down the context menus on iPad
                                    if (!isMobileDevice()) {
                                        // Place it (in the same pos where loading was shown)
                                        updateContextMenuPosition();
                                    }

                                    if (appendToBody) {
                                        scope.$on('$locationChangeSuccess', function () {
                                            hideContextMenu();
                                        });
                                    }

                            });
                        }
                    }
                    
                    // Right click handler
                    function elementRightClickBind(event) {
                        lastClickTimestamp = event.timeStamp;
                        var offset = getOffset(event);
                        lastClickX = offset.x;
                        lastClickY = offset.y;
                        
                        event.preventDefault();

                        if (scope.isOpen) {
                            hideContextMenuBind();
                        }
                        
                        showContextMenuBind(event);
                    }

                    var backdropElement;

                    function addBackdrop() {
                        if (contextMenu && !backdropElement) {
                            var clickHandler = isTouchDevice() ? 'sp-first-touch="backdropClick($event)"' : 'ng-click="backdropClick($event)" sp-right-click="backdropClick($event)"';
                            backdropElement = $compile('<div class="contextmenu-backdrop" ' + clickHandler + '></div>')(contextMenuScope);
                            contextMenu.after(backdropElement);
                        }
                    }

                    function removeBackdrop() {
                        if (backdropElement) {
                            backdropElement.remove();
                            backdropElement = null;
                        }
                    }

                    // Show the context menu
                    function showContextMenu() {
                        // Remove the existing menu
                        if (contextMenu) {
                            contextMenu.remove();
                            contextMenu = null;
                        }
                        
                        removeBackdrop();

                        if (contextMenuScope) {
                            contextMenuScope.$destroy();
                            contextMenuScope = null;
                        }

                        contextMenuScope = scope.$new();

                        // Build the context menu
                        contextMenu = buildContextMenu();
                        
                        if (!contextMenu) {
                            return;
                        }

                        setContextMenuIsOpen(true);

                        // update the position of the context menu
                        updateContextMenuPosition();

                        addBackdrop();
                    }

                    // Toggle display of context menu
                    function toggleContextMenuBind(event) {
                        lastClickTimestamp = event.timeStamp;

                        if (scope.isOpen) {
                            hideContextMenuBind();
                        } else {
                            showContextMenuBind(event);
                        }
                    }

                    // Show the context menu
                    function showContextMenuBind(event) {

                        var offset = getOffset(event);
                        lastClickX = offset.x;
                        lastClickY = offset.y;

                        scope.$apply(function () {
                            showContextMenu();
                        });
                    }

                    // Hide the context menu
                    function hideContextMenu() {
                        setContextMenuIsOpen(false);

                        removeBackdrop();

                        if (contextMenu) {
                            contextMenu.remove();
                            contextMenu = null;
                        }                        

                        if (contextMenuScope) {
                            contextMenuScope.$destroy();
                            contextMenuScope = null;
                        }

                    }


                    // Hide the context menu
                    function hideContextMenuBind() {
                        
                        safeApply(scope, function () {
                            hideContextMenu();
                        });
                    }
                    
                    function safeApply(selfscope, fn) {
                        if (!selfscope.$root.$$phase) {
                            // digest or apply not in progress
                            selfscope.$apply(fn);
                        } else {
                            fn();
                        }
                    }


                    // Update the context menu position
                    function updateContextMenuPosition() {
                        
                        var position, cmPosition, cmWidth, cmHeight;

                        if (!contextMenu) {
                            return;
                        }

                        contextMenu.css({ top: 0, left: 0, display: 'block' });

                        if (appendToBody) {
                            body = body || $document.find('body');
                            body.append(contextMenu);
                        } else {
                            contextMenu.insertAfter(iElement);
                        }

                        // Calculate the position for the context menu
                        position = appendToBody ? $uibPosition.offset(iElement) : $uibPosition.position(iElement);

                        cmWidth = contextMenu.prop('offsetWidth');
                        cmHeight = contextMenu.prop('offsetHeight');

                        switch (placement) {
                        case 'mouse':
                            cmPosition = {
                                top: lastClickY,
                                left: lastClickX
                            };
                            break;
                        case 'alignleft':
                            cmPosition = {
                                top: position.top + position.height,
                                left: position.left
                            };
                            break;
                        case 'alignright':
                            cmPosition = {
                                top: position.top + position.height,
                                left: (position.left + position.width) - cmWidth
                            };
                            break;
                        default:
                            cmPosition = {
                                top: position.top + position.height,
                                left: (position.left + position.width) - cmWidth
                            };
                            break;
                        }

                        // adjust the top and left if context menu is going off the screen
                        if (cmHeight + cmPosition.top > $(window).height()) {
                            cmPosition.top = $(window).height() - cmHeight - 5;
                        }

                        if (cmWidth + cmPosition.left > $(window).width()) {
                            cmPosition.left = $(window).width() - cmWidth - 5;
                        }

                        // Ensure context menu is on the screen
                        if (cmPosition.left < 0) {
                            cmPosition.left = 5;
                        }

                        cmPosition.top += 'px';
                        cmPosition.left += 'px';

                        // Now set the calculated positioning.
                        contextMenu.css(cmPosition);
                    }

                    // Add a DOM attribute
                    function addAttribute(attribute, value) {
                        return value ? ' ' + attribute + '="' + value + '"' : '';
                    }
                    
                    // Build the context menu html
                    // On a touch device, all submenus are collapsed into the
                    // root and a maximum height is added so that the
                    // menu can be scrolled.
                    // Without this there is no easy way to make the menu scrollable
                    // and to handle submenus cleanly.
                    function buildContextMenuHtml(depth, menuscope, items) {
                        if (!items || items.length <= 0) {
                            return null;
                        }

                        var isTouch = isTouchDevice();
                        var clickHandler, tpl = '';

                        if (depth === 0 || !isTouch) {
                            clickHandler = isTouch ? 'hm-tap="backdropClick($event)"' : 'ng-click="backdropClick($event)" sp-right-click="backdropClick($event)"';
                            tpl = '<ul class="dropdown-menu contextmenu-view action-view' + (isTouch ? ' contextMenuScrollable' : '') + '" ' + clickHandler + '>';
                        }                        

                        _.forEach(items, function (item, index) {
                            var isSubmenu, li, iconUrl, disabled = false, liClass = '', hidden = false, menuItemClass = '';

                            if (angular.isDefined(item.hidden)) {
                                if (_.isBoolean(item.hidden)) {
                                    hidden = item.hidden;
                                } else {
                                    hidden = menuscope.$eval(item.hidden);
                                }
                            }

                            if (hidden) {
                                return;
                            }

                            if (angular.lowercase(item.type) === 'divider') {
                                tpl += '<li class="contextMenuDivider"><div class="contextMenuDividerOuter"><div class="contextMenuDividerInner"></div></div></li>';
                            } else {                                
                                if (angular.isDefined(item.disabled)) {
                                    if (_.isBoolean(item.disabled)) {
                                        disabled = item.disabled;
                                    } else {
                                        disabled = menuscope.$eval(item.disabled);
                                    }
                                }

                                isSubmenu = item.submenu && item.submenu.length;

                                if (isSubmenu && !isTouch) {
                                    liClass = 'dropdown-submenu';                                    
                                }

                                if (disabled) {
                                    liClass += ' disabled';
                                }                                
                                
                                li = '<li' + addAttribute('class', liClass) + '>';
                                iconUrl = '';

                                menuItemClass = 'menuItem';

                                if (isSubmenu && isTouch) {
                                    menuItemClass += ' submenuItemTouch';
                                }

                                li += '<a class="' + menuItemClass + '" tabindex="-1"';

                                if (!disabled && !(isSubmenu && isTouch)) {
                                    // Add type specific attributes
                                    if (angular.lowercase(item.type) === 'href') {
                                        li += addAttribute('ng-href', item.href) + addAttribute('target', item.target);
                                    } else if (angular.lowercase(item.type) === 'click') {
                                        var clickHandler = isTouch ? 'hm-tap' : 'ng-click';
                                        li += addAttribute(clickHandler, item.click);
                                    }
                                }

                                li += '>';

                                if (item.icon) {
                                    iconUrl = 'background-image:url(' + convertIconUrl(item.icon) + ');';
                                    if (disabled) {
                                        iconUrl += 'opacity:0.3;';
                                    }
                                }

                                if (index === 0) {
                                    li += '<div class="iconPanel"><div class="iconPanelVerticalDivider"></div></div>';
                                }                                                                

                                li += '<div class="menuIcon" style="' + iconUrl + '"></div><span>' + (item.text || '') + '</span></a>';

                                if (isSubmenu) {
                                    // Recurse into submenus                                    
                                    li += buildContextMenuHtml(depth + 1, menuscope, item.submenu);
                                }

                                li += '</li>';
                                tpl += li;
                            }
                        });

                        if (depth === 0 || !isTouch) {
                            tpl += '</ul>';
                        }

                        return tpl;
                    }
                    
                    // Get the mouse position for menu placement
                    function getOffset(evt) {
                        if (evt.pointerType === 'touch') {
                            return {
                                x: evt.pointers[0].clientX, y: evt.pointers[0].clientY
                            };
                        }

                        if (appendToBody) {
                            return { x: evt.clientX, y: evt.clientY };
                        }

                        var oe = evt.originalEvent;
                        if (oe) {
                            return { x: oe.layerX, y: oe.layerY };
                        }

                        return { x: 0, y: 0 };
                    }

                    //
                    // Triggers
                    //

                    function addOpenTriggerHandlers(iElement, trigger) {
                        // on a non touch device default the trigger to left click
                        if (!trigger && !isTouchDevice())
                            trigger = 'leftclick';

                        // Configure display triggers
                        switch (angular.lowercase(trigger)) {
                            case 'leftclick': 
                                iElement.on('click', toggleContextMenuBind);
                                break;

                            case 'rightclick':
                                if (!isTouchDevice()) {
                                    iElement.on('contextmenu', elementRightClickBind);      // this is nor reliable on touch IOS devices
                                } else {
                                    addPressHandler(iElement[0], elementRightClickBind);
                                }
                                break;

                            case 'none': break;
                        }
                    }


                    function removeOpenTriggerHandlers(iElement) {
                        iElement.off('click', toggleContextMenuBind);

                        if (!isTouchDevice()) {
                            iElement.off('contextmenu', elementRightClickBind);
                        } else {
                            removePressHandler(iElement[0], elementRightClickBind);
                        }
                    }
                }
            };
        });
}());