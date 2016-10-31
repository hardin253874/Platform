// Copyright 2011-2016 Global Software Innovation Pty Ltd
(function () {
    'use strict';

    /**
    * Module implementing a refresh button which will raise
    *
    * @module spRefreshButton    
    * @example           
    
    Using the spRefreshButton:

    &lt;&gt;sp-refresh-button="options"/&gt;

    where options is available on the scope with the following properties:        
        - refreshCallback - {function}. A function that is called when either a manual or automatic refresh is triggered.
        - autoRefreshTimeoutMin - {number}. The selected auto refresh timeout in seconds. One of 0, 5, 10, 15, 30.
        - disabled {boolean=}
    */
    angular.module('mod.common.ui.spRefreshButton', ['mod.common.ui.spContextMenu', 'mod.common.spCachingCompile'])
        .constant('refreshMenuItemConfig', {
            menuItemConfig: [
                {
                    timeoutMin: 5,
                    menuItemText: 'Auto Refresh 5 Minutes',
                    buttonText: '(5 min)'
                },
                {
                    timeoutMin: 10,
                    menuItemText: 'Auto Refresh 10 Minutes',
                    buttonText: '(10 min)'
                },
                {
                    timeoutMin: 15,
                    menuItemText: 'Auto Refresh 15 Minutes',
                    buttonText: '(15 min)'
                },
                {
                    timeoutMin: 30,
                    menuItemText: 'Auto Refresh 30 Minutes',
                    buttonText: '(30 min)'
                },
                {
                    timeoutMin: 0,
                    menuItemText: 'Auto Refresh Off',
                    buttonText: ''
                }                
            ]
        })
        .directive('spRefreshButton', function (refreshMenuItemConfig, spCachingCompile, $timeout) {
            return {
                restrict: 'E',
                replace: false,
                transclude: false,
                scope: {
                    options: '='                  
                },
                link: function (scope, iElement, iAttrs) {
                    var timeoutPromise;

                    // Setup the model
                    scope.model = {
                        selectedMenuItem: null,
                        contextMenu:
                        {
                            menuItems: [                                
                            ]
                        },
                        isInDesign: !!scope.options.isInDesign
                    };


                    // Build the menu items
                    buildRefreshMenuItems();


                    // Private function

                    // Build the refresh menu items from the config
                    function buildRefreshMenuItems() {
                        scope.model.contextMenu.menuItems = _.map(refreshMenuItemConfig.menuItemConfig, function (menuItemConfig) {
                            return {
                                buttonText: menuItemConfig.buttonText,
                                text: menuItemConfig.menuItemText,
                                icon: '',
                                type: 'click',
                                click: 'refreshMenuItemClick(' + menuItemConfig.timeoutMin + ')',
                                timeoutMin: menuItemConfig.timeoutMin
                            };
                        });
                    }


                    // Set the selected menu item
                    function setSelectedMenuItem(timeoutMin) {
                        var selectedMenuItem = null;

                        // Set the icon for the selected menu item
                        _.forEach(scope.model.contextMenu.menuItems, function (mi) {
                            if (mi.timeoutMin === timeoutMin) {                                
                                mi.icon = 'assets/images/16x16/tick.png';
                                selectedMenuItem = mi;
                            } else {
                                mi.icon = '';
                            }
                        });

                        // Set the selected menu item
                        scope.model.selectedMenuItem = selectedMenuItem;
                        if (selectedMenuItem) {
                            scope.options.autoRefreshTimeoutMin = selectedMenuItem.timeoutMin;
                        } else {
                            scope.options.autoRefreshTimeoutMin = 0;
                        }

                        // Cancel any pending timers
                        cancelRefreshTimer();

                        // Start the refresh timer
                        if (scope.options.autoRefreshTimeoutMin) {
                            startAutoRefreshTimer(scope.options.autoRefreshTimeoutMin);
                        }
                    }


                    // Start the refresh timer using the specified number of minutes
                    function startAutoRefreshTimer(timeoutMin) {                        
                        if (timeoutMin) {
                            timeoutPromise = $timeout(refreshTimerCallback, timeoutMin * 60 * 1000);
                        }
                    }


                    // The refresh timer callback.
                    // Call the registered callback and restart the timer.
                    function refreshTimerCallback() {
                        callRefreshCallback();

                        if (scope.options.autoRefreshTimeoutMin) {
                            startAutoRefreshTimer(scope.options.autoRefreshTimeoutMin);
                        }                        
                    }


                    // Call the registered callback.
                    function callRefreshCallback() {
                        if (scope.options.refreshCallback) {
                            scope.options.refreshCallback();
                        }
                    }
                    

                    // Cancel any pending refresh timers
                    function cancelRefreshTimer() {
                        if (timeoutPromise) {
                            // Cancel any pending timeout
                            $timeout.cancel(timeoutPromise);
                            timeoutPromise = null;
                        }
                    }


                    // Watch for any options changes
                    scope.$watch('options.autoRefreshTimeoutMin', function (timeoutMin) {
                        setSelectedMenuItem(timeoutMin);
                    });


                    // Handle menu item click events
                    scope.refreshMenuItemClick = function (timeoutMin) {
                        setSelectedMenuItem(timeoutMin);
                    };


                    // Handle manual refresh click events
                    scope.onRefreshClicked = function () {
                        callRefreshCallback();
                    };


                    // Cancel any pending refresh timers
                    scope.$on('$destroy', function () {
                        cancelRefreshTimer();
                    });

                    var cachedLinkFunc = spCachingCompile.compile('refreshButton/spRefreshButton.tpl.html');
                    cachedLinkFunc(scope, function (clone) {
                        iElement.append(clone);
                    });
                }
            };
        });
}());