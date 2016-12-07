// Copyright 2011-2016 Global Software Innovation Pty Ltd
/*global _, angular, console, sp */

/**
 * A set of AngularJS services related to navigation.
 * @module navigation
 */

(function () {
    "use strict";

    angular.module('sp.navService')
        .factory('spNavService', spNavService);

    /* @ngInject */
    function spNavService($rootScope, $state, $stateParams, $q, $window, $location, spAppError, spLoginService, spNavDataService, spEntityService, spTenantSettings, spWebService, spAppSettings, navDirtyMessage, spThemeService, spMobileContext, spNgUtils, spDocumentationService) {

        /**
         * spNavService handles a few things.
         *
         * <ul>
         *  <li>Build a tree structure based on the configurable tree of menus and navigation elements.</li>
         *  <li>Maintain breadcrumbs, a list of navigation items, that kinda represents the location within the application
         * and provides a parent child relationship between certain page items so they can interact in any way they like</li>
         *  <li>Handle situations like not signed in and page dirty.</li>
         * </ul>
         *
         * The breadcrumbs is array of items.
         * Tree is a tree of nodes where each node is { item : {}, children: node[] }.
         * An item has an id and a bunch of other properties. #todo formalise the item, maybe in a 'class'
         * Items are not shared between the tree and breadcrumbs, although items in each may have the same id.
         *
         * @class spNavService
         * @memberOf module:navigation
         */

        // array of test nav item ids to not clean up as the nav tree is updated
        var testNavItemIds = [];

        var authId;
        var navTree;
        var breadcrumb;
        var cacheMarkerRefreshed = [];
        var cacheMarker = false;
        var isCacheable = false;

        // ReadiNow Url Bookmark Scheme key name
        var urlBookmarkSchemeSearchKey = 'rnurlbs';

        var pendingNavigation;
        var shouldRefreshTree = false;
        var pendingInternalNavigation;
        var previousLocationHref;
        var tenantTheme;
        var solutionThemes;
        var getThemesCompleted = false;
        var getFirstItemApplicationId = 0;
        var model = {
            _isEditMode: false
        };        
        var havePrivateContentSectionsInCurrentNavTreeValue = false;
        var havePrivateContentSectionsInCurrentNavTreeUpdateCount = -1;

        // I think there is a little too much stuffed in this service!
        // todo - factor some things out
        // like theming, navtree, and actual navigation
        var service = {
            /**
             * Counter that is incremented each time the navTree (or breadcrumb) is updated.
             * Handy for watching.
             * @memberof module:navigation.spNavService
             */
            treeUpdateCount: 0,

            sortNavNodes: sortNavNodes,
            requireRefreshTree: requireRefreshTree,
            refreshTreeBranch: refreshTreeBranch,
            duplicateNameInFolder: duplicateNameInFolder,
            refreshTree: refreshTree,
            isNavigationPending: isNavigationPending,
            continueNavigation: continueNavigation,
            cancelNavigation: cancelNavigation,
            getNavTree: getNavTree,
            getLastUsedAppMenuKey: getLastUsedAppMenuKey,
            getCurrentApplicationId: getCurrentApplicationId,
            getCurrentApplicationMenuEntity: getCurrentApplicationMenuEntity,
            getThemes: getThemes,
            getApplicationUpdated: getApplicationUpdated,
            getUpdatedTheme: getUpdatedTheme,
            getCurrentTheme: getCurrentTheme,
            getBreadcrumb: getBreadcrumb,
            getCurrentItem: getCurrentItem,
            getParentItem: getParentItem,
            getParentFolder: getParentFolder,
            getCurrentItemContainer: getCurrentItemContainer,
            getChildHref: getChildHref,
            getViewHref: getViewHref,
            navigateToState: navigateToState,
            navigateToSibling: navigateToSibling,
            navigateToChildState: navigateToChildState,
            navigateToParent: navigateToParent,
            navigateBack: navigateBack,
            isInternalNavigationPending: isInternalNavigationPending,
            navigateInternal: navigateInternal,
            navigateToDesktopSite: navigateToDesktopSite,
            reloadCurrentState: reloadCurrentState,
            canNavigateToParent: canNavigateToParent,
            isParentNavItem: isParentNavItem,
            flattenTree: flattenTree,
            findInTree: findInTree,
            findInTreeByNodeIdAndParentNodeId: findInTreeByNodeIdAndParentNodeId,
            findInTreeById: findInTreeById,
            forEachItemInTree: forEachItemInTree,
            mergeTreeChild: mergeTreeChild,
            makeLink: makeLink,
            requestInitialNavTree: requestInitialNavTree,
            canNavigateToItem: canNavigateToItem,
            setUrlBookmarkScheme: setUrlBookmarkScheme,
            getUrlBookmarkScheme: getUrlBookmarkScheme,
            validateUrlBookmarkScheme: validateUrlBookmarkScheme,
            toggleIsEditMode: toggleIsEditMode,
            setIsEditMode: setIsEditMode,
            debug: debug,
            getMenuNodes: getMenuNodes,
            getIsCacheable: getIsCacheable,
            getCacheMarker: getCacheMarker,
            setCacheMarker: setCacheMarker,
            clearCacheMarker: clearCacheMarker,
            havePrivateContentSectionsInTree: havePrivateContentSectionsInTree,
            canShowConfigMenu: canShowConfigMenu
        };

        var middleBusyCount = 0;

        Object.defineProperty(service, 'middleLayoutBusy', {
            get: function () {
                return middleBusyCount > 0;
            },
            set: function (val) {
                if (val) {
                    middleBusyCount++;
                } else {
                    if (middleBusyCount > 0) {
                        middleBusyCount--;
                    }
                }
            },
            enumerable: true,
            configurable: true
        });

        // Is the user a full administrator AND has entered 'configuration' mode.
        // Use this for build-time features that should not be available to self-serve users.
        Object.defineProperty(service, 'isFullEditMode', {
            get: function () {
                return model._isEditMode && spAppSettings.fullConfig;
            },
            enumerable: true,
            configurable: true
        });

        // Is the user a ('self-serve' user or an administrator) AND has entered 'configuration' mode.
        // Use this for build-time features that should be available to both admins and self-serve users.
        Object.defineProperty(service, 'isSelfServeEditMode', {
            get: function () {
                return model._isEditMode && spAppSettings.selfServeOrAdmin;
            },
            enumerable: true,
            configurable: true
        });

        /////////////////////////////////////////
        // Set up watches and event listeners

        $window.onbeforeunload = function () {
            var index = _.findIndex(breadcrumb, isItemDirty);
            if (index >= 0) {
                if ($rootScope.__spTestMode) {
                    // seems a pointless log (as we are unloading the app) but it may be captured during tests
                    console.warn('onbeforeunload: Skipping app dirty message as running test mode. Url is :' + $location.absUrl());

                } else {
                    return getDirtyMessage(breadcrumb[index]);
                }
            }
            // do not return anything to allow the window to close
            // ReSharper disable once NotAllPathsReturnValue
        };

        $rootScope.$on('signedin', function () {
            initialise();
            initThemeService();

            spDocumentationService.initializeDocoSettings();
        });

        $rootScope.$on('sp.cancelNavigation', function () {
            cancelNavigation();
        });

        $rootScope.$on('sp.continueNavigation', function () {
            continueNavigation();
        });

        $rootScope.$on('$locationChangeStart', function (event) {
            if (!validateUrlBookmarkScheme()) {
                event.preventDefault();
            }
        });

        $rootScope.$on('$locationChangeSuccess', function (event, to) {
            previousLocationHref = to;
        });

        $rootScope.$on('$stateChangeStart', function (event, toState, toParams) {            
            sp.resetTime();

            //todo - use as flag in the state data rather then hardcoded names
            if (toState.name === 'settings' || toState.name === 'login') {
                return;
            }

            if (toState.name === 'loginRedirect') {
                cancelStateChange(event);
                spAppError.clear();            

                if (spLoginService.getAuthenticatedIdentity()) {
                    spLoginService.redirectAfterLogin();
                } else {
                    spLoginService.readiNowLoginWithCookie().then(function() {
                        spLoginService.redirectAfterLogin();
                    }, function(result) {
                        var msg;

                        switch (result.status) {
                            case 401:
                                msg = result.data.Message;
                                break;

                            case 403:
                                msg = result.data.Message;
                                break;

                            case 404:
                                msg = 'Unable to connect to server.';
                                break;

                            default:
                                // This does not represent a data leakage security risk as the response is trivially obtained by other means.
                                msg = 'Unexpected server error: ' + result.status;
                                break;
                        }

                        console.warn('Was redirected after identity provider login, but did not authenticate with ReadiNow. ' + msg);
                    });
                }
                return;
            }

            if (pendingNavigation || pendingInternalNavigation) {
                cancelStateChange(event);
                console.warn('navService: on $stateChangeStart: already have pending navigation, dropping this one');
                return;
            }
            //if toParams's path property is missed and toState is taskView, reset the path from breadcrumb
            //todo these code only about fix bug 21826. after we fix the structural problem with navigation. these code should be confirmed. because if nav function works well, the toParams.path should be not missed

            if (!toParams.path && toState.name === 'taskView') {
                toParams.path = pathToPathString(breadcrumb);
            }

            // the following may return a boolean or other
            var canNav = canNavigate(toState, toParams);
            if (_.isBoolean(canNav)) {
                if (!canNav) {
                    cancelStateChange(event);
                    console.log('navService: on $stateChangeStart: navigation cancelled');
                    return;
                }
            } else {
                cancelStateChange(event);
                console.log('navService: on $stateChangeStart: navigation suspended, setting nav pending');
                return;
            }
        });

        $rootScope.$on('$stateChangeSuccess', function (event, toState, toParams) {
            spLoginService.saveRedirect(toState, toParams);

            if (toParams.eid) {
                var firstItemId = parseInt(toParams.eid, 10);
                var navItem = _.find(navTree.children, function (currentNavItem) {
                    return currentNavItem.item.id === firstItemId;
                });
                if (navItem) {
                    getFirstItemApplicationId = sp.result(navItem, 'item.applicationId');
                }
            }

            navigated(toState, toParams);

            // cache marker
            isCacheable = false;
            if (toState) {
                isCacheable = toState.name === 'screen' || toState.name === 'screenBuilder';
            }

            var item = getCurrentItem();
            cacheMarker = item && angular.isDefined(item.cacheMarker);
            if (cacheMarker) {
                delete item.cacheMarker;
            }

            if (!cacheMarkerRefreshed) {
                cacheMarkerRefreshed = [];
            }
            cacheMarkerRefreshed.length = 0;
        });

        /////////////////////////////////////////
        // Initialise the service state

        reset();

        if (!spEntityService.isMockService) {
            initialise();
        }

        initThemeService();

        return service;

        ////////////////////////////////////////////////////////////
        // Implementation functions

        function initThemeService() {
            if (spLoginService.isSignedIn()) {
                spTenantSettings.getTenantTheme().then(function (theme) {
                    tenantTheme = theme;
                    if (tenantTheme && solutionThemes) {
                        getThemesCompleted = true;
                    }
                });

                spTenantSettings.getSolutionThemes().then(function (themes) {
                    solutionThemes = themes;
                    if (tenantTheme && solutionThemes) {
                        getThemesCompleted = true;
                    }
                });
            }
        }

        /**
         * @memberof module:navigation.spNavService
         */
        function isNavigationPending() {
            return pendingNavigation;
        }

        /**
         * @memberof module:navigation.spNavService
         */
        function getNavTree() {
            return navTree;
        }

        /**
         * Returns LastUsedAppMenu string key
         */
        function getLastUsedAppMenuKey() {
            return 'lastUsedAppMenuKey';
        }

        /**
         * Get the id of the Application (aka Solution) corresponding to the top menu
         * item, or null if we have no breadcrumb, or it isn't in the tree root level
         * or that item doesn't have an application relationship.
         */
        function getCurrentApplicationId() {
            var entity = service.getCurrentApplicationMenuEntity();
            getFirstItemApplicationId = sp.result(entity, 'inSolution.idP');
            return getFirstItemApplicationId;
        }

        /**
         * Get the entity of the current Application (aka Solution) corresponding to the top menu
         * item, or null if we have no breadcrumb, or it isn't in the tree root level
         * or that item doesn't have an application relationship.
         */
        function getCurrentApplicationMenuEntity() {
            var firstItem = _.first(breadcrumb);
            if (!firstItem) {
                return null;
            }
            var navItem = _.find(navTree.children, function (navItem) {
                return navItem.item.id === firstItem.id;
            });
            var appEntity = sp.result(navItem, 'item.entity');
            return appEntity;
        }

        /*
        * Returns true if there are private content sections in the current nav tree
        */
        function havePrivateContentSectionsInCurrentNavTree() {
            if (havePrivateContentSectionsInCurrentNavTreeUpdateCount === service.treeUpdateCount) {                
                return havePrivateContentSectionsInCurrentNavTreeValue;
            }

            function findNodeInBreadcrumb(nodes, breadcrumbs) {
                if (!nodes || !breadcrumbs) {
                    return null;
                }

                return _.find(nodes,
                    function(node) {
                        if (!node || !node.item) {
                            return false;
                        }
                        return _.some(breadcrumbs, { id: node.item.id });
                    });
            }

            var currentMenuNode = findNodeInBreadcrumb(getMenuNodes(), breadcrumb);
            var currentTabNodes = currentMenuNode && currentMenuNode.item && currentMenuNode.item.showTabs ? currentMenuNode.children : [];
            var currentTabNode = findNodeInBreadcrumb(currentTabNodes, breadcrumb);

            if (currentTabNode) {
                havePrivateContentSectionsInCurrentNavTreeValue = _.some(currentTabNode.children,
                    function(node) {
                        return node && node.item && node.item.typeAlias === 'console:privateContentSection';
                    });
            } else {
                havePrivateContentSectionsInCurrentNavTreeValue = false;
            }

            havePrivateContentSectionsInCurrentNavTreeUpdateCount = service.treeUpdateCount;            
            return havePrivateContentSectionsInCurrentNavTreeValue;
        }

        /**
         * Returns true if we are currently in the 'home' tree
         */
        function havePrivateContentSectionsInTree() {
            return havePrivateContentSectionsInCurrentNavTree();            
        }

        function canShowConfigMenu(entity) {
            if (!entity)
                return false;
            if (service.isFullEditMode)
                return true;
            if (!service.isSelfServeEditMode)
                return false;
            return entity.isPrivatelyOwned;
        }

        function getThemes() {
            return getThemesCompleted;
        }

        function getApplicationUpdated() {
            return getFirstItemApplicationId;
        }


        function getUpdatedTheme(firstItemApplicationId) {
            if (firstItemApplicationId || tenantTheme) {
                var applicationTheme;
                var theme;
                applicationTheme = _.find(solutionThemes, function (solutionTheme) {
                    return solutionTheme.id === firstItemApplicationId;
                });

                if (applicationTheme && applicationTheme.entity && applicationTheme.entity.solutionConsoleTheme) {
                    theme = applicationTheme.entity.solutionConsoleTheme;
                } else if (tenantTheme) {
                    theme = tenantTheme;
                } else {
                    theme = null;
                }

                return theme;
            }
            return null;
        }

        function getCurrentTheme() {
            var firstItem = _.first(breadcrumb);

            if (firstItem && firstItem.theme) {
                spThemeService.setStyle(firstItem.theme);
                return firstItem.theme;
            }
            var applicationId = service.getCurrentApplicationId();
            if (applicationId || tenantTheme) {
                var theme = service.getUpdatedTheme(applicationId);

                firstItem = _.extend(firstItem || {}, {
                    theme: theme
                });
                spThemeService.setStyle(firstItem.theme);
                return firstItem.theme;
            }
            return null;
        }

        /**
         * @memberof module:navigation.spNavService
         */
        function getBreadcrumb() {
            return breadcrumb;
        }

        /**
         * Get the nav item related to the current page.
         * @returns {item} nav item {id, name, selected, active}
         * @memberof module:navigation.spNavService
         */
        function getCurrentItem() {
            return _.last(breadcrumb);
        }

        /**
         * Get the container item in the nav tree for the item that is the current page.
         * @returns {item} nav item {id, name, selected, active}
         * @memberof module:navigation.spNavService
         */
        function getCurrentItemContainer() {

            function isContainer(item) {
                return item && (item.typeAlias === 'console:navSection' || item.typeAlias === 'core:folder' || item.typeAlias === 'core:documentFolder' || item.typeAlias === 'console:privateContentSection');
            }

            function isContainerItem(item) {
                // item is from the bc, so need to find it in the navTree to find its type

                var node = findInTreeById(navTree, item.id);
                return node && isContainer(node.item);
            }

            var foundItem = _.findLast(breadcrumb, isContainerItem);
            return foundItem || findInTreeByIdAndState(navTree, foundItem);
        }

        /**
         * @memberof module:navigation.spNavService
         */
        function getParentItem() {
            var len = breadcrumb.length;
            return len > 1 ? breadcrumb[len - 2] : null;
        }

        /**
         * @memberof module:navigation.spNavService
         */
        function getParentFolder() {
            var len = breadcrumb.length;
            return len > 2 ? breadcrumb[len - 3] : null;
        }

        /**
         * @memberof module:navigation.spNavService
         */
        function getChildHref(stateName, eid, params) {

            var stateParams = _.extend(params || {},
                {
                    tenant: spLoginService.signedInTenant(),
                    eid: eid,
                    path: pathToPathString(breadcrumb)
                });

            return getStateHref(stateName, stateParams);
        }

        /**
         * @memberof module:navigation.spNavService
         */
        function getViewHref(stateName, params) {

            var stateParams = _.extend(
                params || {},
                {
                    tenant: spLoginService.signedInTenant(),
                    eid: (_(breadcrumb).last() || {id: 0}).id,
                    path: pathToPathString(_.initial(breadcrumb))
                });

            return getStateHref(stateName, stateParams);
        }

        /**
         * Navigate to a pages within the console. Note that the caller must pass in any breadcrumbing info. This
         * call would not normally be used. Use the sibling or child alternatives.
         * @memberof module:navigation.spNavService
         */
        function navigateToState(stateName, params) {

            var stateParams = _.extend(
                params || {},
                {
                    tenant: spLoginService.signedInTenant()
                });

            return $state.go(stateName, stateParams, {inherit: false});
        }

        /**
         * Navigate to a sibling of the current page. This preserves the current breadcrumb, replacing the last entry.
         * @param data - optional, if given it represents a data object that the caller wishes
         * to pass through to the new child.
         * @memberof module:navigation.spNavService
         */
        function navigateToSibling(stateName, eid, params, data) {

            var stateParams = _.extend(
                params || {},
                {
                    tenant: spLoginService.signedInTenant(),
                    eid: eid,
                    // The breadcrumb includes the current entity at the end but path does not
                    // so we need to allow for that.
                    path: pathToPathString(_.initial(breadcrumb, 1))
                });

            if (data) {
                // attach this data object temporarily to the parent (to be) to be passed to the child
                // once we get there
                var item = getParentItem();
                if (item) {
                    item.pendingDataObject = data;
                } else {
                    console.warn('Failed to set navigation state on root object');
                }
            }

            // Clear the current page just in case the navigate is to itself
            clearPageStateAndScope();


            return $state.go(stateName, stateParams, {inherit: false, location: 'replace'});
        }

        /*
         * Clear all the state and scope from current page. Used as a prelude to sibling navigagtion
         * (This should be in spState, but we have a dependency ordering issue.)
         */
        function clearPageStateAndScope() {
            var navItem = getCurrentItem();

            if (navItem) {
                navItem.data = {};
                navItem.componentData = {};

                if (navItem.scope) {
                    navItem.scope.$destroy();
                    navItem.scope = null;
                }
            }
        }

        /**
         * Navigate to a child of the current page. This adds another entry to the end of the breadcrumb.
         * @param data - optional, if given it represents a data object that the caller wishes
         * to pass through to the new child.
         * @memberof module:navigation.spNavService
         */
        function navigateToChildState(stateName, eid, params, data) {

            logBreadcrumbs('navigateToChild');

            var stateParams = _.extend(params || {}, {
                tenant: spLoginService.signedInTenant(),
                eid: eid,
                // The breadcrumb includes the current entity at the end but path does not
                // so we need to allow for that.
                path: pathToPathString(breadcrumb)
            });

            if (data) {
                // attach this data object temporarily to the parent (to be) to be passed to the child
                // once we get there
                var item = getCurrentItem();
                if (item) {
                    item.pendingDataObject = data;
                }
            }

            return $state.go(stateName, stateParams, {inherit: false});
        }

        /**
         * @memberof module:navigation.spNavService
          * @param tellParentToRefresh - optional, if true inform the parent that it needs to refresh data
         * @return true is successful, false otherwise.
         */
        function navigateToParent(tellParentToRefresh) {

            logBreadcrumbs('navigateToParent');

            var parent = getParentItem();

            if (parent) {

                if (tellParentToRefresh)
                    parent.refreshRequired = true;

                if (parent.state && parent.state.params) {

                    $state.go(parent.state.name, parent.state.params);

                    return true;
                } else if (parent.href) {

                    //sg - why are we looking in the tree when we have the URL?
                    //nodeInTree = findInTreeByIdAndState(navTree, parent);
                    $window.location = parent.href;                    

                    return true;
                } else if (parent.state) {
                    //the parent item only with state not params. e.g. from AdminToolbox window, just navigate back.
                    navigateBack();
                }
                else {

                    $rootScope.$broadcast('sp.nav.showNavigator');

                    return false;
                }
            } else {
                return false;
            }

            return false;
        }

        /**
         * @memberof module:navigation.spNavService
         * @return true is successful, false otherwise.
         */
        function navigateBack() {

            // TODO - do the dirty check here before we navigate, OR maintain our own history

            $window.history.back();
        }

        /**
         * @memberof module:navigation.spNavService
         */
        function isInternalNavigationPending() {
            return pendingInternalNavigation;
        }

        /**
         * @memberof module:navigation.spNavService
         * @Navigate internally in a page. This does not manipulate $state or href, but does check if the current item is dirty.
         * @return a promise to complete the navigation
         */
        function navigateInternal() {
            var deferred = $q.defer();

            if (isItemDirty(getCurrentItem())) {
                pendingInternalNavigation = {
                    onContinue: internalNavigationContinue,
                    onCancel: internalNavigationCancel,
                    deferred: deferred,
                    message: navDirtyMessage.defaultMsg
                };
            } else {
                deferred.resolve(null);
            }

            return deferred.promise;
        }

        function internalNavigationContinue() {
            var deferred = pendingInternalNavigation.deferred;
            pendingInternalNavigation = null;

            deferred.resolve(null);
        }

        function internalNavigationCancel() {
            var deferred = pendingInternalNavigation.deferred;
            pendingInternalNavigation = null;
            deferred.reject(null);
        }

        /**
        * @Switch the console and force it to run as a desktop
        */
        function navigateToDesktopSite() {
            $location.search('isDesktop', 'true');
            $window.location.reload();
        }

        /**
         * Reload the current state
         * @memberof module:navigation.spNavService
         */
        function reloadCurrentState() {
            $state.transitionTo($state.current, $stateParams, {reload: true, inherit: false, notify: true});
        }

        /**
         * @memberof module:navigation.spNavService
         */
        function canNavigateToParent() {
            return !!service.getParentItem();
        }

        /**
         * Is the parent a nav item. That is, does it appear in the nav tree.
         * @memberof module:navigation.spNavService
         */
        function isParentNavItem() {
            var parentItem = service.getParentItem();

            if (!parentItem) {
                return false;
            } else {
                return parentItem.state.name === 'folder';      // This is a little arbitrary. A better test would be to add the information to the breadcrumb.
            }
        }

        /**
         * Sets the url bookmark scheme value.
         */
        function setUrlBookmarkScheme(bookmarkScheme) {
            // Only change the bookmark scheme if it is different
            if (bookmarkScheme &&
                service.getUrlBookmarkScheme() !== bookmarkScheme) {
                $location.search(urlBookmarkSchemeSearchKey, bookmarkScheme);
            }
        }


        /**
         * Gets the current url bookmark scheme value.
         */
        function getUrlBookmarkScheme() {
            var searchObj = $location.search();
            return sp.result(searchObj, urlBookmarkSchemeSearchKey);
        }


        /**
         * Validate the current url's bookmark scheme value.
         */
        function validateUrlBookmarkScheme() {
            var expectedBookmarkScheme = sp.result(spAppSettings, 'sessionInfo.bookmarkScheme');
            if (!expectedBookmarkScheme) {
                // bookmark scheme is not set or is not ready
                return true;
            }

            // Get the actual bookmark scheme
            var actualBookmarkScheme = service.getUrlBookmarkScheme();

            if (actualBookmarkScheme && expectedBookmarkScheme !== actualBookmarkScheme) {
                console.log('Url bookmark scheme is invalid. Sending you home.');
                $window.location.replace($location.absUrl().split('#')[0] + '#/' + $stateParams.tenant);
                return true;
            }

            // Ensure current url has the bookmark scheme set
            service.setUrlBookmarkScheme(expectedBookmarkScheme);

            return true;
        }


        /**
         * Toggles the edit mode.
         */
        function toggleIsEditMode() {
            model._isEditMode = !model._isEditMode;
            treeUpdated();
        }

        function setIsEditMode(value) {
            model._isEditMode = value;
        }

        function debug() {
            return _.map(breadcrumb, function (item) {
                return {
                    id: item.id,
                    name: item.name,
                    stateName: sp.result(item, 'state.name'),
                    href: item.href
                };
            });
        }

        /**
         * Returns the top menu node items.
         */
        function getMenuNodes(ordered) {
            // WARNING - the call to getNavTree() here must be called via the service object
            // as we have some unit test code that is replacing the function.
            // TODO - work out a better way to mock this
            var result = _.filter(service.getNavTree().children, function (node) {
                return !node.item.hidden || node.item.name === 'Administration' || node.item.name === 'Help'; //hack
            });

            if (ordered) {
                result = _.sortBy(result, function (node) {
                    return node.item.order;
                });
            }
            return result;
        }

        function logState(/*message, state, params*/) {
            //here for when I want to debug stuff

            //console.log('DEBUG: navService: %s: state=%o, params=%o, url="%s"', message, state, params, $state.href(state, params));
        }

        /** log either the given breadcrumb like items or the breadcrumbs themselves */
        function logBreadcrumbs(/*message, items*/) {
            //here for when I want to debug stuff

            //var temp = _.map(items || breadcrumb, function (item) {
            //    return {
            //        id: item.id,
            //        stateName: sp.result(item, 'state.name'),
            //        href: item.href
            //    };
            //});
            //console.log('DEBUG: navService: ', message, ': ', temp);
        }

        function fixupItemId(item) {
            return _.extend(item, {id: sp.coerseToNumberOrLeaveAlone(item.id)});
        }

        /**
         * Make a list of items given a path string and add the option item.
         * Each nav item can be an entity id or alias, plus an optional state name.
         * Formatting is like:
         *  34|home,5566|home,2888|edit
         */
        function pathFromPathString(path, eid, name) {
            var pathItems = _(path ? path.split(',') : [])
                .map(function (itemString) {
                    var parts = itemString.split('|');
                    var item = {id: parts[0]};
                    var stateName = sp.result(parts, '1');
                    if (stateName) item.state = {name: stateName};
                    return item;
                })
                .concat(eid ? [
                    {id: eid, state: {name: name}}
                ] : [])
                .map(fixupItemId)
                .value();

//                logBreadcrumbs('pathFromPathString - items', pathItems);
//                logBreadcrumbs('pathFromPathString - bc');

            return pathItems;
        }

        /**
         * Convert the path items to a path string.
         */
        function pathToPathString(pathItems) {
            var path = _.map(pathItems, function (item) {
                return item.id + (item.state && item.state.name ? '|' + item.state.name : '');
            }).join();
            return path;
        }

        // Get the href for the given state name and parameters
        // and not inherit from the current url
        function getStateHref(name, params) {
            return $state.href(name, params, {inherit: false});
        }

        function makeLink(item) {

            var itemState = {};
            itemState.params = {
                tenant: spLoginService.signedInTenant(),
                eid: item.id,
                path: ''
            };

            var path = [];
            if (findInTree(navTree, item, path) && path.length > 1) {
                itemState.params.path = pathToPathString(_(path).map('item').compact().reverse().value());
            }

            itemState.name = item.viewType ? item.viewType : 'undefined';

            item.state = itemState;

            if (!canNavigateToItem(item)) {
                return null;
            }

            return getStateHref(itemState.name, itemState.params);
        }

        function canNavigateToItem(item) {
            if (spMobileContext.isMobile || spMobileContext.isTablet) {
                return true;
            }

            if (item &&
                ((item.typeAlias === 'console:navSection' && !item.isAppTab) ||
                item.typeAlias === 'core:folder' ||
                item.typeAlias === 'console:privateContentSection')) {
                // Cannot navigate to a navSection (that is not an application tab) or a folder.
                return false;
            }

            return true;
        }


        function hideOnMobile(item) {
            return sp.result(item, 'entity.hideOnMobile');
        }

        function hideOnTablet(item) {
            return sp.result(item, 'entity.hideOnTablet');
        }

        function hideOnDesktop(item) {
            return (!service.isSelfServeEditMode) && sp.result(item, 'entity.hideOnDesktop');
        }

        /**
         * Sort the immediate children by order then name.
         * @name module:navService#sortImmediateChildren
         */
        function sortImmediateChildren(tree) {
            if (tree && tree.children.length) {
                service.sortNavNodes(tree.children);
            }
        }

        function sortNavNodes(navNodes) {
            if (!navNodes || !navNodes.length) {
                return navNodes;
            }

            navNodes.sort(function (a, b) {
                if (!a.item || !b.item) {
                    return 0;
                }
                if ((a.item.order || 0) !== (b.item.order || 0)) {
                    return (a.item.order || 0) < (b.item.order || 0) ? -1 : +1;
                }
                return a.item.name < b.item.name ? -1 : +1;
            });

            return navNodes;
        }

        function treeUpdated() {

            var hideFn;

            if (spMobileContext.isMobile) {
                hideFn = hideOnMobile;
            } else if (spMobileContext.isTablet) {
                hideFn = hideOnTablet;
            } else {
                hideFn = hideOnDesktop;
            }

            if (navTree) {
                forEachItemInTree(navTree, function (item, depth, node) {
                    if (item) {
                        item.href = makeLink(item);
                        item.depth = depth;
                        item.hiddenByConfig = hideFn(item);
                    }

                    // Assign parents
                    _.forEach(node.children, function (c) {
                        c.parent = node;
                    });

                    sortImmediateChildren(node);
                });
            }

            service.treeUpdateCount += 1;
        }

        function cancelNavigation() {
            pendingNavigation = null;
        }

        function continueNavigation() {
            var nav = pendingNavigation;
            pendingNavigation = null;
            if (nav) {
                //logState('continuing with nav request', nav.state, nav.params);
                if (nav.breadcrumb) {
                    // we have saved what will become the new breadcrumb - this
                    // means the dirty items are dropped
                    breadcrumb = nav.breadcrumb;
                }
                $state.go(nav.state, nav.params);
            }
        }

        function saveNavRequest(reason, state, params, breadcrumb, message) {
            //logState('saving nav request, reason=' + reason, state, params);
            if (pendingNavigation) {
                throw new Error('navService: already have pending nav request');
            }
            pendingNavigation = {
                reason: reason,
                state: state,
                params: params,
                breadcrumb: breadcrumb,
                message: message
            };
        }

        function rebuildBreadcrumbs(newItems, oldItems) {

            // set to the new items

            breadcrumb = newItems;

            // ensure the scope chain is there

            _.reduce(breadcrumb, function (parentScope, item, index) {
                if (item.scope && item.scope.$parent !== parentScope) {
                    item.scope.$destroy();
                    item.scope = null;
                }
                if (!item.scope) {
                    item.scope = parentScope.$new(false);
                    item.scope['bcIndex' + index] = item.id; //todo remove
                }
                return item.scope; // the parent for the next bc
            }, $rootScope);

            //  destroy old scopes

            _.forEach(oldItems, function (item) {
                if (item.scope) {
                    item.scope.$destroy();
                    item.scope = null;
                }
            });
        }

        function syncBreadcrumb() {
            //todo - sync the breadcrumb... must keep the last, but to ensure
            // the path up from there lines up with the tree as we know it now.

            _.forEach(breadcrumb, function (item) {
                if (!item.href || !item.name) {
                    var node = findInTreeByIdAndState(navTree, item);
                    if (node) {
                        item.href = item.href || node.item.href;
                        item.name = item.name || node.item.name;
                        item.state = item.state || node.item.state;
                    }
                }
                if (item.uiroute) {
                    item.href = getStateHref(item.uiroute.name, item.uiroute.params);
                }
            });
        }

        function addTestPageNavItems() {

            var testSolutionNode = _.find(navTree.children, function (node) {
                return node.item.name === 'Test Solution';
            });
            if (testSolutionNode) {

                var id = 10000000;
                var testPagesNode = {
                    item: {
                        id: (id += 1),
                        name: 'Test Pages',
                        typeAlias: 'console:navSection',
                        viewType: 'folder',
                        iconUrl: 'assets/images/16x16/organization.png',
                        isAppTab: true,
                        order: 99999
                    },
                    children: []
                };
                testNavItemIds.push(testPagesNode.item.id);
                testPagesNode.item.href = makeLink(testPagesNode.item);

                mergeTreeChild(testSolutionNode, testPagesNode);

                _.forOwn(window.testNavItems || [], function (item, key) {
                    _.defaults(item, {
                        id: (id += 1),
                        viewType: key,
                        href: makeLink(item),
                        iconUrl: 'assets/images/16x16/dashboard.png',
                        order: 99999
                    });
                    testNavItemIds.push(item.id);
                    mergeTreeChild(testPagesNode, {item: item, children: []});
                });
            }
        }

        //set the refresh tree flag to true
        function requireRefreshTree() {
            shouldRefreshTree = true;
        }

        function refreshTreeBranch(item) {
            //the refreshTreeBranch method is called in different place,
            //for performance issue, call requireRefreshTree method to turn on the flag when nav item is change (modify name or delete)
            //only refresh tree branch when the flag is on.
            if (shouldRefreshTree) {
                shouldRefreshTree = false;
                var node = findInTreeById(navTree, item.id);
                if (node) {
                    return spNavDataService.getNavTreeExpanded([item]).then(function (node) {
                        if (node) {
                            mergeTreeChild(navTree, node);
                            treeUpdated();
                        } else {
                            console.log('navService: failed to expand tree for items');
                        }
                    });
                }                
            }

            var deferred = $q.defer();
            deferred.resolve();

            return deferred.promise;
        }

        //find any another current type item with the same name in this folder
        function duplicateNameInFolder(folderId, type, name) {
            var folderNode = findInTreeById(navTree, folderId);
            var duplicateName = false;
            if (folderNode && folderNode.item && (folderNode.item.viewType === 'documentFolder' || folderNode.item.viewType === 'folder')) {
                var duplicateNameItem = _.find(folderNode.children, function (childItem) {
                    return childItem.item && childItem.item.viewType === type && childItem.item.name === name;
                });
                duplicateName = !!duplicateNameItem;
            }
            return duplicateName;
        }

        function requestInitialNavTree(sync) {
            return spNavDataService.getNavItems().then(function (node) {
                _.forEach(node.children,
                    _.partial(mergeTreeChild, navTree));

                if (sync) {
                    _.remove(navTree.children, function (treeChild) {
                        return !_.find(node.children, function (nodeChild) {
                            return treeChild.item.id === nodeChild.item.id;
                        });
                    });
                }

                addTestPageNavItems();
                treeUpdated();


                syncBreadcrumb();
            });
        }

        function refreshTree(sync) {
            requestInitialNavTree(sync);
        }

        function prepareSyncOfBreadcrumb(pathItems) {
            // Return an array of three arrays: keep, add, drop
            // A new bc would be keep + add
            // It is important the retain the items if possible as they may carry
            // view state.

            function itemsMatch(item1, item2) {
                return item1.id === item2.id &&
                    (!item1.state && !item2.state ||
                    item1.state && item2.state && item1.state.name === item2.state.name);
            }

            var keep, add, drop;
            var current = _.first(pathItems) || {id: 0};
            var index = _.findIndex(breadcrumb, _.partial(itemsMatch, current));

            if (index >= 0) {
                // advance the index for each matched id in the path
                var i = 1;
                while (index + i < breadcrumb.length && i < pathItems.length && itemsMatch(pathItems[i], breadcrumb[index + i])) {
                    i += 1;
                }
                index += i;

                // build the sub-arrays
                keep = _(breadcrumb).take(index).value();
                add = _(pathItems).drop(i).value();
                drop = _(breadcrumb).drop(index).value();

            } else {
                keep = [];
                add = _(pathItems).value();
                drop = breadcrumb;
            }

            return [keep, add, drop];
        }

        function syncPath(eid, path) {
            // The path is the top down navigation state for the target entity and may be
            // represented by:
            // - a string using comma or slash separated nav entity id or aliases
            // - an array of the same

            var pathItems = pathFromPathString(path, eid, $state.current.name);

            // Sync with breadcrumb.
            // If the start of the path is in the breadcrumb then take the breadcrumb items
            // up to there and while they match the path. Then add new items for the remainder
            // of the path.. to be filled in with a request for name etc.

            var arrs = prepareSyncOfBreadcrumb(pathItems);
            rebuildBreadcrumbs(_(arrs[0]).concat(arrs[1]).value(), arrs[2]);

            // Sync the breadcrumb with the nav tree

            syncBreadcrumb();

            service.treeUpdateCount += 1;
        }

        function navigated(state, params) {
            logState('navigated', state, params);
            logBreadcrumbs('navigated - start');

            if (params) {
                syncPath(params.eid, params.path);

                var item = _.last(breadcrumb);
                if (item) {
                    item.href = getStateHref(state.name, params);
                    item.state = {
                        name: state.name, params: params
                    };
                }

                var parentItem = getParentItem();
                if (parentItem && parentItem.pendingDataObject) {
                    item.dataObject = parentItem.pendingDataObject;
                    parentItem.pendingDataObject = null;
                }
            } else {
                console.error('navService.navigated... missing params??');
            }

            logBreadcrumbs('navigated - done');
        }

        function isItemDirty(item) {
            return _.result(item, 'isDirty') || anyDirtyComponents(item);
        }

        function getDirtyMessage(item) {
            return _.result(item, 'dirtyMessage') || navDirtyMessage.defaultMsg;
        }

        /**
         * Return true or false if can or can't navigate or return null if we don't know yet.
         */
        function canNavigate(state, params) {
            logState('canNavigate? ', state, params);

            var pathItems = [];

            if (params && params.eid) {
                pathItems = pathFromPathString(params.path, params.eid, state.name);
            }

            //debug
            _.forEach(breadcrumb, function (item) {
                item.isDirtyCached = _.result(item, 'isDirty');
            });
            //end debug

            var arrs = prepareSyncOfBreadcrumb(pathItems);
            var dropping = arrs[2];
            var dirtyIndex = _.findLastIndex(dropping, isItemDirty);

            if (dirtyIndex >= 0) {
                saveNavRequest('dirty', state, params, arrs[0].concat(arrs[1]), getDirtyMessage(dropping[dirtyIndex]));
                return null;
            }

            return true;
        }

        function reset() {
            authId = spLoginService.getAuthenticatedIdentity();
            navTree = {item: null, children: []};
            breadcrumb = [];
            treeUpdated();
        }

        function initialise() {
            if (spLoginService.isSignedIn()) {
                if (authId && !spLoginService.isSignedInAsTenant(authId.tenant)) {
                    reset();
                }

                updateUrlSetTenantSegment();
                requestInitialNavTree();
                continueNavigation();
            }
        }

        function updateUrlSetTenantSegment() {
            var identity = spLoginService.getAuthenticatedIdentity();

            if (!identity) {
                return;
            }

            spNgUtils.updateUrlSetTenantSegment(identity.tenant);
        }

        function cancelStateChange(event) {
            event.preventDefault();
            if (previousLocationHref && previousLocationHref !== $window.location.href) {
                // not doing this right now... causes problems
                // note the reason I was trying this was because we have an issue when cancelling
                // a state change where the URL still becomes the new URL and so we have mismatched
                // url and state.
                //$window.location.href = previousLocationHref;
                previousLocationHref = null;
            }
        }

        /**
         * Search the tree for a node representing the given item where the
         * item parameter is either the item itself or a predicate.
         *
         * Assumes the tree nodes are { item, children } where children are an array of nodes.
         *
         * Optionally capture the path from the found item to the root. If an item is found then the
         * path (of nodes) is added to the given path array, if one is provided. The path or any existing
         * values in the path array are not touched.
         *
         * @param tree
         * @param itemOrFn
         * @param pathInReverse
         * @returns tree node referencing the item, or null if not found
         *
         * @name module:navigation#findInTree
         * @inner
         */
        function findInTree(tree, itemOrFn, pathInReverse) {
            var result = null;

            function match(item) {
                return _.isFunction(itemOrFn) ? itemOrFn(item) : item === itemOrFn;
            }

            if (!tree) {
                return null;
            }

            if (tree.item && match(tree.item)) {
                return tree;
            }

            if (tree.children) {

                _.some(tree.children, function (child) {
                    if (match(child.item)) {
                        result = child;
                    }
                    return !!result;
                });

                if (!result) {
                    _.some(tree.children, function (child) {
                        result = findInTree(child, itemOrFn, pathInReverse);
                        return !!result;
                    });
                }

                if (result && pathInReverse) {
                    pathInReverse.push(tree);
                }
            }

            return result;
        }

        /**
         * Search the tree for a node representing the given item where the
         * item parameter is either the item itself or a predicate.
         *
         * Assumes the tree nodes are { item, children } where children are an array of nodes.
         *
         * Optionally capture the path from the found item to the root. If an item is found then the
         * path (of nodes) is added to the given path array, if one is provided. The path or any existing
         * values in the path array are not touched.
         *
         * @param tree
         * @param itemOrFn
         * @param pathInReverse
         * @returns tree node referencing the item, or null if not found
         *
         * @name module:navigation#findInTree
         */
        function findInTreeByNodeIdAndParentNodeId(treeNode, itemOrFn, pathInReverse) {
            var result = null;

            function match(item) {
                return _.isFunction(itemOrFn) ? itemOrFn(item) : item === itemOrFn;
            }

            if (!treeNode) {
                return null;
            }

            if (match(treeNode)) {
                return treeNode;
            }

            if (treeNode.children) {

                _.some(treeNode.children, function (child) {
                    if (match(child)) {
                        result = child;
                    }
                    return !!result;
                });

                if (!result) {
                    _.some(treeNode.children, function (child) {
                        result = findInTreeByNodeIdAndParentNodeId(child, itemOrFn, pathInReverse);
                        return !!result;
                    });
                }

                if (result && pathInReverse) {
                    pathInReverse.push(treeNode);
                }
            }

            return result;
        }

        /**
         * Uses findInTree to find an item's node by id (or alias)
         * @name module:navigation#findInTreeById
         */
        function findInTreeById(tree, idOrAlias, pathInReverse) {
            return findInTree(tree, function (item) {
                return item && (item.id === idOrAlias || item.alias === idOrAlias);
            }, pathInReverse);
        }

        /**
         * Uses findInTree to find an item's node by id (or alias)
         * and if the item has a state then check it too
         * @name module:navigation#findInTreeById
         */
        function findInTreeByIdAndState(tree, item, pathInReverse) {
            return findInTree(tree, function (treeItem) {
                return treeItem && item &&
                    (treeItem.id === item.id || treeItem.alias === item.id) &&
                    (!item.state ||
                    treeItem.state && item.state.name === treeItem.state.name);
            }, pathInReverse);
        }

        /**
         * Merge the child node (sub-tree) into the children of the tree (node).
         * @name module:navigation#mergeTreeChild
         */
        function mergeTreeChild(tree, childNode) {

            var existing = false, id, childrenToRemove;
            if (tree.children) {
                if (childNode.item) {
                    id = childNode.item.id;
                    existing = _.find(tree.children, function (node) {
                        return node.item.id === id;
                    });
                }
            } else {
                tree.children = [];
            }
            if (!existing) {
                tree.children = tree.children.concat([childNode]);
            } else {
                // replace the existing item with the incoming's item and merge the children

                existing.item = childNode.item;

                if (!_.isEmpty(childNode.children)) {

                    // first find and remove immediate children that no longer exist

                    childrenToRemove = _.reject(existing.children, function (existingChild) {
                        return _.some(childNode.children, function (newChild) {
                            return newChild.item.id === existingChild.item.id || _.includes(testNavItemIds, existingChild.item.id);
                        });
                    });

                    if (!_.isEmpty(childrenToRemove)) {
                        // Remove the existing children that no longer exist
                        _.remove(existing.children, function (c) {
                            return _.includes(childrenToRemove, c);
                        });
                    }
                }

                _.each(childNode.children, function (node) {
                    mergeTreeChild(existing, node);
                });
            }
        }

        /**
         * Visit each node of the tree and call the given fn on the node passing the node and the current depth.
         * Visits in a depth first manner - this is assumed by some callers.
         *
         * Note - for the 'virtual root node' it may call the fn with item null.
         *
         * @param tree
         * @param fn
         * @param depth
         * @name module:navigation#forEachItemInTree
         */
        function forEachItemInTree(tree, fn, depth) {
            if (tree) {
                depth = depth || 0;
                fn(tree.item, depth, tree);
                if (tree.children) {
                    depth += 1;
                    _.each(tree.children, function (t) {
                        forEachItemInTree(t, fn, depth);
                    });
                }
            }
        }

        /**
         * Flatten the tree in a depth first manner to an ordered array of items, each with a depth attribute.
         * @param tree
         * @returns {Array}
         * @name module:navigation#flattenTree
         */
        function flattenTree(tree) {
            var flatTree = [];
            forEachItemInTree(tree, function (item, depth) {
                if (item) {
                    item.depth = depth;
                    flatTree.push(item);
                }
            });
            return flatTree;
        }

        /**
        * Checks if the current location suitable for local storage cache operations.
        */
        function getIsCacheable() {
            return isCacheable === true;
        }

        /**
        * Sets a marker for prior pages that will notify them their cached data may need updating.
        */
        function setCacheMarker() {
            var now = new Date();
            _.forEach(breadcrumb, function (b) {
                b.cacheMarker = now;
            });
            if (cacheMarkerRefreshed) {
                cacheMarkerRefreshed.length = 0;
            }
        }

        /**
        * Checks if a marker has been set that instructs revisited nodes to update any cached data they hold.
        * @cacheKey {string} The cache key associated with the request.
        */
        function getCacheMarker(cacheKey) {
            if (cacheMarker === true) {
                var i = cacheMarkerRefreshed.indexOf(cacheKey.noMetaName);
                if (i < 0) {
                    i = cacheMarkerRefreshed.indexOf(cacheKey.name);
                }
                return i < 0;
            }
            return false;
        }

        /**
        * Excludes a cache key from the marker operations. (i.e. when it has been refreshed)
        * @cacheKey {string} The cache key associated with the request.
        */
        function clearCacheMarker(cacheKey) {
            if (cacheKey && cacheKey.name && cacheKey.name.length > 0) {
                cacheMarkerRefreshed.push(cacheKey.name);
                if (cacheKey.noMetaName && cacheKey.noMetaName.length > 0) {
                    cacheMarkerRefreshed.push(cacheKey.noMetaName);
                }
            }
        }

        /**
         * If the current nav item has componentState (managed via the spState service)
         * and if any of these has a isDirty property (may be a function) then return
         * true if any return true. Else false.
         * @param {Object} item - the nav item
         * @returns {boolean}
         */
        function anyDirtyComponents(item) {
            return item && item.componentData &&
                _.some(item.componentData, function (x) { return _.result(x, 'isDirty');});
        }
    }
})();
