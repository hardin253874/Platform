// Copyright 2011-2016 Global Software Innovation Pty Ltd
(function () {
    'use strict';

    /**
    * Module implementing an analyzer popup.            
    * spAnalyzerPopup displays the analyzer in a popup.    
    *
    * @module spAnalyzerPopup    
    * @example           
    
    Using the spAnalyzerPopup:

    &lt;div sp-analyzer-popup="options"&gt;&lt;/div&gt      

    where options is available on the controller with the following properties:
        - isOpen - {bool}. If the popup is open or not
        - analyzerOptions - {object}. See the options of the sp-analyzer directive. 
    */
    angular.module('mod.common.ui.spAnalyzerPopup', ['mod.common.ui.spPopupProvider', 'mod.common.ui.spAnalyzer', 'mod.common.ui.spPopupStackManager', 'mod.common.ui.spResizable'])
        .directive('spAnalyzerPopup', function ($parse, spPopupProvider, $document, spPopupStackManager) {
            return {
                restrict: 'A',
                link: function (originalScope, iElement, iAttrs) {
                    var body = $document.find('body')[0],
                        scope = originalScope.$new(true),
                        currentPopup,
                        lastClickTimestamp = -1,
                        lastClickX = -1,
                        lastClickY = -1,
                        popupProvider,
                        getterAnalyzerPopupOptions,
                        // Get the dialog parent
                        dialogsParents = iElement.parents('.modal'),
                        dialogZIndex = 1000;

                    body.addEventListener('click', documentClick, true);

                    popupProvider = spPopupProvider(scope, iElement, {
                        preventCloseOnClick: true,
                        placement: 'alignright',
                        templatePopupUrl: 'analyzer/spAnalyzerPopup.tpl.html',
                        canClose: canClosePopup
                    });

                    // Create the model with the options
                    scope.model = {
                        analyzerPopupZIndex: 1000,
                        popupOptions: {
                            isOpen: false
                        },
                        resizeOptions: {                            
                            minHeight: 120,                            
                            minWidth: 556,
                            maxWidth: 556,
                            enableDynamicOptionChanges: true,
                            changeId: 0                            
                        }
                    };

                    if (dialogsParents &&
                        dialogsParents.length) {
                        // Get the z-index of the first parent
                        dialogZIndex = dialogsParents.first().css('z-index');
                        if (_.isNull(dialogZIndex) ||
                            _.isUndefined(dialogZIndex)) {
                            dialogZIndex = 1000;
                        }
                        scope.model.analyzerPopupZIndex = dialogZIndex;
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

                    scope.$on('$destroy', function () {                        
                        body.removeEventListener('click', documentClick, true);
                    });

                    getterAnalyzerPopupOptions = $parse(iAttrs.spAnalyzerPopup);
                    scope.model.popupOptions = getterAnalyzerPopupOptions(originalScope);


                    scope.$on('spAnalyzerEventApplyConditions', function () {
                        if (scope.model.popupOptions) {
                            scope.model.popupOptions.isOpen = false;
                        }
                    });

                    // Watch the options for changes
                    originalScope.$watch(getterAnalyzerPopupOptions, function (popupOptions) {
                        scope.model.popupOptions = popupOptions;
                    });
                    

                    scope.$watch(function () {
                        return popupProvider.getIsPopupOpen();
                    }, function (isOpen) {
                        if (scope.model.popupOptions) {
                            scope.model.popupOptions.isOpen = isOpen;
                        }

                        if (isOpen) {
                            spPopupStackManager.pushPopup(popupProvider.getPopup());
                        } else {
                            spPopupStackManager.popPopup(popupProvider.getPopup());
                        }
                    });


                    // Watch the value of isOpen for changes
                    scope.$watch(function () {
                        return scope.model.popupOptions.isOpen;
                    }, function (isOpen) {                        
                        if (isOpen) {
                            popupProvider.showPopup();
                        } else {
                            popupProvider.hidePopup();
                        }                        
                    });                    


                    // Make sure popup is destroyed and removed.
                    originalScope.$on('$destroy', function () {
                        if (scope.model.popupOptions) {
                            scope.model.popupOptions.isOpen = false;
                        }
                        scope.$destroy();
                    });


                    // Return true if the popup can close
                    function canClosePopup(event) {

                        if (event.timeStamp !== lastClickTimestamp ||
                            event.screenX !== lastClickX ||
                            event.screenY !== lastClickY) {
                            return false;
                        }

                        return (currentPopup === popupProvider.getPopup());
                    }


                    function getPopupHeight(fieldsLength, minHeight, maxHeight) {
                        var popupHeight;

                        if (!_.isNumber(fieldsLength) || _.isNaN(fieldsLength)) {
                            fieldsLength = 0;
                        }

                        popupHeight = 90 + ((fieldsLength - 1) * 30);

                        if (minHeight && (popupHeight < minHeight)) {
                            popupHeight = minHeight;
                        } else if (maxHeight && (popupHeight > maxHeight)) {
                            popupHeight = maxHeight;
                        }

                        return popupHeight;
                    }


                    scope.$watch('model.popupOptions.analyzerOptions.analyzerFields.length', function (fieldsLength) {                        
                        if (fieldsLength > 7) {
                            // Enable resizing when we have more than 7 fields
                            scope.model.resizeOptions.maxHeight = getPopupHeight(fieldsLength, 120, 750);                            
                            scope.model.resizeOptions.disabled = false;
                            scope.model.resizeOptions.changeId++;
                        } else {
                            // Prevent resizing                            
                            scope.model.resizeOptions.disabled = true;
                            scope.model.resizeOptions.changeId++;
                        }
                    });


                    scope.getPopupStyle = function () {
                        var fieldsLength = sp.result(scope, 'model.popupOptions.analyzerOptions.analyzerFields.length');                         

                        // Get the height of the popup constrained by the specified min and max height, which is min 2 fields, maximum 7 fields                        
                        return {
                            'z-index': scope.model.analyzerPopupZIndex,
                            height: getPopupHeight(fieldsLength, 120, 270) + 'px'
                        };
                    };
                }
            };
        });
}());