// Copyright 2011-2016 Global Software Innovation Pty Ltd
(function () {
    'use strict';

    /**
      * Module implementing a popup provider.        
      *
      * @module spPopupProvider    
      * @example            
         
      Usage
     
      spPopupProvider(scope, iElement, options);
     
      where options is an object with the following properties:
         - preventCloseOnClick {bool}. True to prevent the popup closing on click
         - templatePopupUrl {string}. The url of the popup         
      */
    angular.module('mod.common.ui.spPopupProvider', ['ui.bootstrap', 'ui.bootstrap.position'])
        .provider('spPopupProvider', function () {
            this.$get = function ($parse, $compile, $uibPosition, $document, $templateCache) {
                return function spPopupProvider(scope, iElement, options) {

                    var exports = {},
                        lastClickTimestamp = -1,
                        lastClickX = -1,
                        lastClickY = -1,
                        previousElementPos = {},
                        previousHeight = -1,
                        popupElement,
                        isPopupOpen = false,
                        body;


                    // Update position timer callback
                    // Used to move the popup when the parent control moves
                    function updatePositionTimerCallback() {
                        var currentElementPos, currentHeight = -1;

                        if (!isPopupOpen) {
                            return;
                        }

                        currentElementPos = $uibPosition.offset(iElement);

                        if (popupElement) {
                            currentHeight = popupElement.prop('offsetHeight');    
                        }
                        
                        if (currentElementPos.width !== previousElementPos.width ||
                            currentElementPos.height !== previousElementPos.height ||
                            currentElementPos.top !== previousElementPos.top ||
                            currentElementPos.left !== previousElementPos.left ||
                            currentHeight !== previousHeight) {
                            scope.$apply(function () {
                                updatePopupPosition();
                            });
                        }
                        
                        _.delay(updatePositionTimerCallback, 100);
                    }


                    // Handle document click events
                    function documentClickBind(event) {
                        var parents, position, inBox;

                        if (!popupElement) {
                            return;
                        }

                        if (event.timeStamp === lastClickTimestamp &&
                            event.screenX === lastClickX &&
                            event.screenY === lastClickY) {
                            return;
                        }

                        if (angular.lowercase(event.target.tagName) === 'select') {
                            return;
                        }
                        // skip the datepicker popup control which raised from current popup window
                        if (event.target.closest("[ng-switch=datepickerMode]")) {
                            return;
                        }

                        if (options &&
                            options.preventCloseOnClick) {
                            parents = angular.element(event.target).parents();
                            position = $uibPosition.offset(popupElement);

                            inBox = (event.pageX >= position.left &&
                                event.pageX <= position.left + position.width &&
                                event.pageY >= position.top &&
                                event.pageY <= position.top + position.height);

                            if (inBox || _.includes(parents, popupElement[0])) {
                                return;
                            }
                        }

                        if (options &&
                            options.canClose &&
                            !options.canClose(event)) {
                            return;
                        }

                        scope.$apply(function () {
                            documentClick(event);
                        });
                    }


                    // Handle document click events
                    function documentClick(event) {
                        hidePopupImpl();
                    }


                    // Update the popup position
                    function updatePopupPosition() {
                        var position, puPosition, puWidth, puHeight;

                        if (!popupElement) {
                            return;
                        }

                        popupElement.css({ top: 0, left: 0, display: 'block' });

                        // Calculate the position for the drop down
                        position = $uibPosition.offset(iElement);
                        previousElementPos = position;

                        puWidth = popupElement.prop('offsetWidth');
                        puHeight = popupElement.prop('offsetHeight');

                        previousHeight = puHeight;

                        switch (angular.lowercase(options.placement)) {
                            case 'alignleft':
                                puPosition = {
                                    top: position.top + position.height,
                                    left: position.left
                                };
                                break;
                            case 'alignright':
                                puPosition = {
                                    top: position.top + position.height,
                                    left: (position.left + position.width) - puWidth
                                };
                                break;
                            default:
                                puPosition = {
                                    top: position.top + position.height,
                                    left: position.left
                                };
                                break;
                        }

                        // adjust the top and left if the popup is going off the screen
                        if (puHeight + puPosition.top > $(window).height()) {
                            puPosition.top = $(window).height() - puHeight - 5;
                        }

                        if (puWidth + puPosition.left > $(window).width()) {
                            puPosition.left = $(window).width() - puWidth - 5;
                        }

                        if (puPosition.left < 0) {
                            puPosition.left = 5;
                        }

                        puPosition.top += 'px';
                        puPosition.left += 'px';
                        puPosition.display = 'block';
                        puPosition.position = 'absolute';

                        // Now set the calculated positioning.
                        popupElement.css(puPosition);
                    }


                    // Build the popup element
                    function buildPopupElement() {
                        if (options &&
                            options.templatePopupUrl) {
                            return $compile($templateCache.get(options.templatePopupUrl))(scope);
                        } else {
                            return null;
                        }
                    }


                    function removePopup() {
                        if (!popupElement) {
                            return;
                        }

                        popupElement.remove();
                        popupElement = null;

                        isPopupOpen = false;

                        // Remove the document click handler
                        $document.off('click', documentClickBind);
                    }


                    function hidePopupImpl() {
                        if (!popupElement) {
                            return;
                        }

                        popupElement.css({ display: 'none' });

                        isPopupOpen = false;

                        // Remove the document click handler
                        $document.off('click', documentClickBind);
                    }


                    function showPopupImpl() {
                        if (!popupElement) {
                            popupElement = buildPopupElement();

                            body = body || $document.find('body');
                            body.append(popupElement);
                        }

                        isPopupOpen = true;

                        // Show the menu
                        updatePopupPosition();

                        // Add a document click handler
                        $document.on('click', documentClickBind);

                        _.delay(updatePositionTimerCallback, 100);
                    }


                    /**
                    * Hide the popup
                    */
                    exports.hidePopup = function (event) {
                        if (event) {
                            lastClickTimestamp = event.timeStamp;
                            lastClickX = event.screenX;
                            lastClickY = event.screenY;
                        }

                        if (popupElement) {
                            safeApply(hidePopupImpl);
                        }                        
                    };


                    /**
                    * Show the popup 
                    */
                    exports.showPopup = function (event) {
                        if (event) {
                            lastClickTimestamp = event.timeStamp;
                            lastClickX = event.screenX;
                            lastClickY = event.screenY;
                        }
                        
                        safeApply(showPopupImpl);
                    };


                    /**
                    * Toggle the display of the popup                   
                    */
                    exports.togglePopup = function (event) {
                        if (isPopupOpen) {
                            // Hide the popup
                            exports.hidePopup(event);
                        } else {
                            // Show the popup
                            exports.showPopup(event);
                        }
                    };


                    /**
                    * Returns true if the popup is open, false otherwise.      
                    *
                    * @returns {bool} True if the popup is open, false otherwise.
                    */
                    exports.getIsPopupOpen = function () {
                        return isPopupOpen;
                    };


                    /**
                    * Gets the popup element.
                    *
                    * @returns {objet} The popup element.
                    */
                    exports.getPopup = function () {
                        return popupElement;
                    };


                    // Hide the popup on location change
                    scope.$on('$locationChangeSuccess', function (event, newUrl, oldUrl) {
                        if (newUrl !== oldUrl) {
                            removePopup();
                        }
                    });


                    scope.$on('$destroy', function () {
                        removePopup();
                    });


                    function safeApply(fn) {
                        if (!scope.$root.$$phase) {
                            // digest or apply not in progress                            
                            scope.$apply(fn);
                        } else {
                            // digest or apply already in progress
                            fn();
                        }
                    }

                    return exports;
                };
            };
        });
}());