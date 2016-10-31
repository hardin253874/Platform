// Copyright 2011-2016 Global Software Innovation Pty Ltd
(function () {
    'use strict';

    /**
    * Module implementing a busy indicator.
    * Displays a busy indicator.
    * spBusyIndicator displays an inline busy indicator.
    * spBusyIndicatorPopup displays a busy indicator in a popup.
    * 
    * @module spBusyIndicator
    * @example
        
    Using the spBusyIndicator:

    &lt;sp-busy-indicator options="options"&gt;&lt;/sp-busy-indicator&gt      

    Using the spBusyIndicatorPopup:

    &lt;div sp-busy-indicator-popup="options"&gt;&lt;/div&gt      

    where options is available on the controller with the following properties:
        - type - {string}. 'progressBar' or 'spinner'
        - text - {string}. The text to display
        - isBusy - {bool}. True to display the busy indicator, false otherwise.
        - textPlacement - {string}. 'bottom' or 'right'. The location of the text. Only valid with spinner type
        - percent - {number}. The percent complete when type is progressBar

    in addition to the above, the following options are only valid for popups:
        
        - placement - {string}. The location of the popup. Valid values are 'element' or 'window'
        - backdropPlacement - {string}. The location of the backdrop. Valid values are 'element' or 'window'
        - backdropHidden - {bool}. True to hide the backdrop, false otherwise.                
    */

    angular.module('mod.common.ui.spBusyIndicator', ['ui.bootstrap', 'mod.common.spCachingCompile', 'mod.common.spInclude'])
        .constant('spBusyIndicatorSettings', { popupDelay: 100, refreshDelay: 200 })                           // how long a busy pop-up wait before popping up, and how often it refreshes its position
        .directive('spBusyIndicator', function () {
            var defaultText = 'Preparing content';

            return {
                restrict: 'E',
                replace: true,
                templateUrl: 'busyIndicator/spBusyIndicator.tpl.html',
                scope: {
                    options: '='
                },
                link: function (scope, iElement, iAttrs) {
                    scope.biModel = {
                        type: 'spinner',
                        busyIndicatorTemplateUrl: 'busyIndicator/spBusyIndicatorSpinnerTextBottom.tpl.html',
                        text: defaultText,
                        isBusy: scope.options.isBusy,
                        isBusyReady: false,
                        percent: scope.options.percent
                    };


                    setTemplateUrl();


                    // Watch for type changes
                    scope.$watch('options.type', function (type) {
                        setTemplateUrl();
                    });


                    // Watch for text changes
                    scope.$watch('options.text', function (text) {
                        if (!angular.isUndefined(text)) {
                            scope.biModel.text = formatText(text);
                        }
                    });


                    // Watch for isBusy changes
                    scope.$watch('options.isBusy', function (isBusy) {
                        scope.biModel.isBusy = isBusy;
                    });


                    // Watch for percent changes
                    scope.$watch('options.percent', function (percent) {
                        if (angular.isUndefined(percent)) {
                            scope.biModel.percent = 100;
                        } else {                            
                            scope.biModel.percent = percent;
                        }
                    });



                    // Enforce consistent message
                    function formatText(text) {
                        if (text === 'Loading...' || text === 'Please wait...') {
                            return defaultText;
                        } else {
                            return text;
                        }
                    }

                    function setTemplateUrl() {
                        var type = angular.lowercase(scope.options.type);
                        if (!type) {
                            scope.biModel.type = 'spinner';
                        }                        

                        switch (type) {
                            case 'progressbar':
                                scope.biModel.busyIndicatorTemplateUrl = 'busyIndicator/spBusyIndicatorProgressBar.tpl.html';
                                break;

                            case 'spinner':
                                scope.biModel.busyIndicatorTemplateUrl = 'busyIndicator/spBusyIndicatorSpinnerTextBottom.tpl.html';
                                break;
                        }
                    }
                }
            };
        })
        .directive('spBusyIndicatorPopup', function ($parse, $compile, $uibPosition, $document, $templateCache, spCachingCompile) {
            return {
                restrict: 'A',
                link: function (originalScope, iElement, iAttrs) {
                    var scope = originalScope.$new(true),
                        body,
                        getterBusyIndicatorPopupOptions,
                        busyIndicator,
                        busyIndicatorScope,
                        busyIndicatorBackdrop;                                       

                    // Create the model with the busy indicator popup
                    // and busy indicator options.
                    scope.bipModel = {
                        // busy indicator popup options
                        bipOptions: {
                            backdropPlacement: 'window',
                            placement: 'window'
                        },
                        // busy indicator options
                        biOptions: {
                            isBusy: true
                        },
                        _isBusyShown: false
                    };

                    iAttrs.$observe('spBusyIndicatorPopup', function (val) {
                        if (val) {
                            // Get the options
                            getterBusyIndicatorPopupOptions = $parse(iAttrs.spBusyIndicatorPopup);
                            scope.bipModel.bipOptions = getterBusyIndicatorPopupOptions(originalScope);

                            // Watch the options for changes
                            originalScope.$watch(getterBusyIndicatorPopupOptions, function (options) {
                                if (options) {
                                    // Save a copy of the popup options
                                    scope.bipModel.bipOptions = options;

                                    if (!scope.bipModel.bipOptions.placement) {
                                        scope.bipModel.bipOptions.placement = 'window';
                                    }

                                    // Check for a valid backdrop placement
                                    if (!scope.bipModel.bipOptions.backdropPlacement) {
                                        scope.bipModel.bipOptions.backdropPlacement = scope.bipModel.bipOptions.placement;
                                        if (!scope.bipModel.bipOptions.backdropPlacement) {
                                            scope.bipModel.bipOptions.backdropPlacement = 'window';
                                        }
                                    }

                                    // Update the busy indicator options
                                    scope.bipModel.biOptions.type = options.type;
                                    scope.bipModel.biOptions.textPlacement = options.textPlacement;
                                    scope.bipModel.biOptions.text = options.text;
                                    scope.bipModel.biOptions.percent = options.percent;
                                }
                            });
                        }
                    });

                    // Watch for options type changes
                    scope.$watch(function () {
                        return scope.bipModel.bipOptions.type;
                    }, function (type) {
                        scope.bipModel.biOptions.type = type;
                    });


                    // Watch for options text placement changes
                    scope.$watch(function () {
                        return scope.bipModel.bipOptions.textPlacement;
                    }, function (textPlacement) {
                        scope.bipModel.biOptions.textPlacement = textPlacement;
                    });


                    // Watch for options text changes
                    scope.$watch(function () {
                        return scope.bipModel.bipOptions.text;
                    }, function (text) {
                        scope.bipModel.biOptions.text = text;
                    });


                    // Watch for options is busy changes
                    scope.$watch('bipModel.bipOptions.isBusy', function (isBusy, oldValue) {

                        // suppress these when testing
                        if (scope.$root.__spTestMode) {
                            console.log('Ignoring busy in test mode : value was ' + isBusy);
                            return;
                        }

                        // note: relying on oldValue doesn't work reliably
                        if (isBusy === scope._isBusyShown) return;

                        if (isBusy) {
                            showBusyIndicator();
                        } else {
                            hideBusyIndicator();
                        }
                        scope._isBusyShown = isBusy;

                        if (window.performance) {
                            if (isBusy) {
                                scope.startTime = window.performance.now();
                            } else if (scope.startTime) {
                                var elapsed = window.performance.now() - scope.startTime;
                                //console.log('Busy indicator shown for ' + Math.round(elapsed) + 'ms');
                            }
                        }
                    });


                    // Watch for options percent changes
                    scope.$watch(function () {
                        return scope.bipModel.bipOptions.percent;
                    }, function (percent) {
                        scope.bipModel.biOptions.percent = percent;
                    });

                    
                    // Make sure busy indicator is destroyed and removed.
                    originalScope.$on('$destroy', function () {
                        hideBusyIndicator();
                        scope.$destroy();                        
                    });
                   

                    // Private functions                        

                    // Build the busy indicator
                    function buildBusyIndicator() {
                        var busyIndicatorElement;
                        var cachedLinkFunc = spCachingCompile.compile('busyIndicator/spBusyIndicatorPopup.tpl.html');
                        cachedLinkFunc(busyIndicatorScope, function(clone) {
                            busyIndicatorElement = clone;
                        });

                        return busyIndicatorElement;
                    }


                    // Build the backdrop
                    function buildBusyIndicatorBackdrop() {
                        var busyIndicatorElement;
                        var cachedLinkFunc = spCachingCompile.compile('busyIndicator/spBusyIndicatorPopupBackdrop.tpl.html');
                        cachedLinkFunc(busyIndicatorScope, function (clone) {
                            busyIndicatorElement = clone;
                        });

                        return busyIndicatorElement;
                    }


                    // Show the busy indicator menu
                    function showBusyIndicator() {
                        if (scope.bipModel.bipOptions.isBusy) {

                            if (busyIndicatorBackdrop) {
                                busyIndicatorBackdrop.remove();
                                busyIndicatorBackdrop = null;
                            }

                            // Remove the existing busy indicator
                            if (busyIndicator) {
                                busyIndicator.remove();
                                busyIndicator = null;
                            }

                            if (busyIndicatorScope) {
                                busyIndicatorScope.$destroy();
                                busyIndicatorScope = null;
                            }

                            busyIndicatorScope = scope.$new();

                            // Build the busy indicator and backdrop
                            busyIndicatorBackdrop = buildBusyIndicatorBackdrop();
                            busyIndicator = buildBusyIndicator();

                            // update the position of the busy indicator and backdrop
                            if (scope.bipModel.bipOptions.backdropHidden) {
                                busyIndicatorBackdrop.css('opacity', 0);
                            }

                            if (scope.bipModel.bipOptions.placement === 'element') {
                                iElement.append(busyIndicatorBackdrop);
                                iElement.append(busyIndicator);
                            } else {
                                body = body || $document.find('body');
                                body.append(busyIndicatorBackdrop);
                                body.append(busyIndicator);
                            }
                        }
                    }
                                       
                   
                    // Hide the busy indicator
                    function hideBusyIndicator() {
                        if (busyIndicatorBackdrop) {
                            busyIndicatorBackdrop.remove();
                            busyIndicatorBackdrop = null;
                        }

                        if (busyIndicator) {
                            busyIndicator.remove();
                            busyIndicator = null;
                        }                        

                        if (busyIndicatorScope) {
                            busyIndicatorScope.$destroy();
                            busyIndicatorScope = null;
                        }
                    }                   
                }
            };
        });
}());