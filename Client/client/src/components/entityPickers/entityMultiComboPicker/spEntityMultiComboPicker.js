// Copyright 2011-2016 Global Software Innovation Pty Ltd
(function () {
    'use strict';

    /**
     * Module implementing an entity combo picker.
     *
     * @module spEntityMultiComboPicker
     * @example

     Using the spEntityMultiComboPicker:

     &lt;sp-entity-multi-combo-picker options="checkBoxOptions"&gt;&lt;/sp-entity-multi-combo-picker&gt

     where checkBoxOptions is available on the controller with the following properties:
     - selectedEntityIds {array of number | string} - the selected entity ids.
     - selectedEntities {array of Entity} - output only (optional). If this member is defined it will be assigned the selected entities.
     - entities {array of Entity} - the array of Entity objects to pick from.
     - entityTypeId - {number|string} - the type id of the entities to pick from. A service call will be made to get all instances of this type. If the entityTypeId that is specified is an enumType the entities are sorted by the enumOrder, otherwise they are sorted by name.
     - hiddenAliases - {array of string} - list of hidden aliases. Only valid when entityTypeId is specified

     Note: you only need to specify entityTypeId or entities.
     *
     */
    angular.module('mod.common.ui.spEntityMultiComboPicker', [
        'mod.common.ui.entityPickerControllers',
        'mod.common.ui.spPopupProvider', 'mod.common.ui.spPopupStackManager'
    ]);

    angular.module('mod.common.ui.spEntityMultiComboPicker')
        .directive('spEntityMultiComboPicker', spEntityMultiComboPicker);

    function spEntityMultiComboPicker(spPopupProvider, $document, spPopupStackManager) {
        return {
            replace: true,
            restrict: 'E',
            templateUrl: 'entityPickers/entityMultiComboPicker/spEntityMultiComboPicker.tpl.html',
            controller: 'multiEntityPickerController',
            scope: {
                options: '='
            },
            link: function (scope, iElement, iAttrs) {
                var body = $document.find('body')[0],
                    currentPopup,
                    closePopup = false,
                    lastClickTimestamp = -1,
                    lastClickX = -1,
                    lastClickY = -1;

                body.addEventListener('click', documentClick, true);
                body.addEventListener('scroll', documentScroll, true);

                var popupProvider = spPopupProvider(scope, iElement, {
                    preventCloseOnClick: true,
                    templatePopupUrl: 'entityPickers/entityMultiComboPicker/spEntityMultiComboPickerPopup.tpl.html',
                    canClose: canClosePopup
                });

                // Return true if the popup can close
                function canClosePopup(event) {

                    if (event.timeStamp !== lastClickTimestamp ||
                        event.screenX !== lastClickX ||
                        event.screenY !== lastClickY) {
                        return false;
                    }

                    if (closePopup) {
                        closePopup = false;
                        return true;
                    }

                    return (currentPopup === popupProvider.getPopup());
                }

                // Handle document click events.
                // This is raised before the local document click handler
                // is fired.
                function documentClick(event) {
                    // Find what popup is on top
                    currentPopup = spPopupStackManager.peekPopup();

                    if (event) {
                        lastClickTimestamp = event.timeStamp;
                        lastClickX = event.screenX;
                        lastClickY = event.screenY;
                    }
                }

                function documentScroll(event) {
                    // don't close this picker if the scroll event originated from this picker
                    var srcElementClassName = sp.result(event, 'srcElement.className');
                    if (srcElementClassName && srcElementClassName.includes('entityMultiComboPickerDropdownPopupMenu')) {
                        return;
                    }

                    //close the opened popup when document is scroll
                    if (popupProvider.getIsPopupOpen()) {
                        popupProvider.togglePopup(event);
                    }
                }

                scope.$on('$destroy', function () {
                    body.removeEventListener('click', documentClick, true);
                    body.removeEventListener('scroll', documentClick, true);
                });

                // Get the names of the selected entities
                scope.getSelectedEntityNames = function () {
                    var checkedEntities, names;

                    checkedEntities = _.filter(scope.entityCheckBoxItems, function (e) {
                        return e && e.entity && e.selected && !_.isEmpty(e.entity.getName());
                    });

                    names = _.map(checkedEntities, function (e) {
                        return e.entity.getName();
                    });

                    if (names) {
                        return names.join(', ');
                    } else {
                        return '';
                    }
                };


                scope.$watch(function () {
                    return popupProvider.getIsPopupOpen();
                }, function (isOpen) {
                    if (isOpen) {
                        spPopupStackManager.pushPopup(popupProvider.getPopup());
                    } else {
                        spPopupStackManager.popPopup(popupProvider.getPopup());
                    }
                });


                // Handle drop down button click events
                scope.dropDownButtonClicked = function (event) {
                    if (scope.disabled) {
                        event.stopPropagation();
                        return;                     // can't disable anchor tag
                    }
                    popupProvider.togglePopup(event);
                };
            }
        };
    }
}());