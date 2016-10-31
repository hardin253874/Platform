// Copyright 2011-2016 Global Software Innovation Pty Ltd
(function () {
    'use strict';

    /**
     * Module implementing a time control popup.
     * spTimeControlPopup displays the bootstrap time control in a popup.
     *
     * @module spTimeControlPopup
     * @example

     Using the spTimeControlPopup:

     &lt;div sp-time-control-popup="options"&gt;&lt;/div&gt

     where options is available on the controller with the following properties:
     - isOpen - {bool}. If the popup is open or not
     - model - {object}. The time control value model.
     */
    angular.module('app.controls.spTimeControlPopup',
        [
            'mod.common.ui.spPopupProvider',
            'mod.common.ui.spPopupStackManager'
        ])
        .directive('spTimeControlPopup', spTimeControlPopup);

    /* @ngInject */
    function spTimeControlPopup($parse, spPopupProvider, $document, spPopupStackManager) {
        return {
            restrict: 'A',
            link: link
        };

        function link(originalScope, iElement, iAttrs) {
            const body = $document.find('body')[0],
                scope = originalScope.$new(true),
                dialogsParents = iElement.parents('.modal');
            let currentPopup,
                lastClickTimestamp = -1,
                lastClickX = -1,
                lastClickY = -1;

            body.addEventListener('click', documentClick, true);

            // Create the model
            scope.model = {
                popupZIndex: 1000,
                popupOptions: {
                    isOpen: false
                },
                timeOptions: {
                    hstep: [1, 2, 3],
                    mstep: [1, 5, 10, 15, 25, 30],
                    ismeridian: true
                }
            };

            const model = scope.model;

            const getterTimeControlPopupOptions = $parse(iAttrs.spTimeControlPopup);
            // Get current options
            model.popupOptions = getterTimeControlPopupOptions(originalScope);
            // Setup watcher to watch for changes
            originalScope.$watch(getterTimeControlPopupOptions, (popupOptions) => model.popupOptions = popupOptions);

            // Create the popup provider
            const popupProvider = spPopupProvider(scope,
                iElement,
                {
                    preventCloseOnClick: true,
                    placement: 'alignright',
                    templatePopupUrl: 'controls/spTimeControl/spTimeControlPopup.tpl.html',
                    canClose: canClosePopup
                });

            if (dialogsParents &&
                dialogsParents.length) {
                // Get the z-index of the first parent
                let dialogZIndex = dialogsParents.first().css('z-index');
                if (spUtils.isNullOrUndefined(dialogZIndex)) {
                    dialogZIndex = 1000;
                }
                model.popupZIndex = dialogZIndex;
            }

            // Make sure popup is destroyed and removed.
            originalScope.$on('$destroy', onOriginalScopeDestroy);
            scope.$on('$destroy', () => body.removeEventListener('click', documentClick, true));

            // Watch for popup open/close changes
            scope.$watch(() => popupProvider.getIsPopupOpen(), onProviderIsOpenChanged);
            scope.$watch(() => model.popupOptions.isOpen, (isOpen) => isOpen ? popupProvider.showPopup() : popupProvider.hidePopup());

            scope.onDoneClick = function () {
                // Can only close if valid
                if (!model.value) return;

                // Set the new value and close the popup
                model.popupOptions.model.value = model.value;
                model.popupOptions.isOpen = false;
            };

            scope.getPopupStyle = function () {
                return {
                    'z-index': model.popupZIndex
                };
            };

            // Private functions
            function onProviderIsOpenChanged(isOpen) {
                if (model.popupOptions) {
                    model.popupOptions.isOpen = isOpen;
                }

                if (isOpen) {
                    model.value = sp.result(scope, 'model.popupOptions.model.value');
                    if (!model.value) {
                        model.value = new Date();
                    }
                }

                if (isOpen) {
                    spPopupStackManager.pushPopup(popupProvider.getPopup());
                } else {
                    spPopupStackManager.popPopup(popupProvider.getPopup());
                }
            }

            // Original scope is destroyed
            function onOriginalScopeDestroy() {
                if (model.popupOptions) {
                    model.popupOptions.isOpen = false;
                }
                scope.$destroy();
            }

            // Return true if the popup can close
            function canClosePopup(event) {
                if (event.timeStamp !== lastClickTimestamp ||
                    event.screenX !== lastClickX ||
                    event.screenY !== lastClickY) {
                    return false;
                }

                return (currentPopup === popupProvider.getPopup());
            }

            // Handle document click events.
            // This is raised before the local document click handler
            // is fired.
            function documentClick(event) {
                // Find what popup is on top
                currentPopup = spPopupStackManager.peekPopup();

                if (!event) {
                    return;
                }

                lastClickTimestamp = event.timeStamp;
                lastClickX = event.screenX;
                lastClickY = event.screenY;
            }
        }
    }
}());