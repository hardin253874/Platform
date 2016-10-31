// Copyright 2011-2016 Global Software Innovation Pty Ltd
(function() {
    'use strict';

    /**
    * Directive implementing resize functionality for dialogs.
    * This directive is intended to be placed as a child of the root element of dialog templates.
    * spResizableDialog allows dialog windows to be resized by dragging their resize handles.
    *
    * @module spResizableDialog.
    * @example
            
    Using the spResizableDialog:

    &lt;sp-resizable-dialog min-width='100' min-height='100' &lt;/sp-resizable-dialog&gt;    
    */
    angular.module('mod.common.ui.spResizableDialog', ['mod.common.ui.spResizable', 'mod.common.spLocalStorage', 'mod.common.spMobile'])
        .directive('spResizableDialog', function(spLocalStorage, $timeout, spMobileContext) {
            return {
                restrict: 'E',
                transclude: false,
                replace: true,
                scope: {
                    dialogId: '@',
                    persistSize: '@',
                    minWidth: '@',
                    minHeight: '@',
                    initialWidth: '@',
                    initialHeight: '@'
                },
                templateUrl: 'resizable/spResizableDialog.tpl.html',
                link: function(scope, element) {
                    var modalDialogElement,
                        persistedSizeKeySuffix = 'Size',
                        dialogSize,
                        persistedSize,
                        resizeDisabled;

                    modalDialogElement = element.parents('.modal-dialog').first();

                    // Disable dialog resize on touch devices
                    resizeDisabled = spMobileContext.isMobile || spMobileContext.isTablet || !modalDialogElement || _.isEmpty(modalDialogElement);

                    // Configure the resize options
                    scope.model = {
                        resizeOptions: {
                            resizableParentClass: '.modal-content',
                            minWidth: scope.minWidth,
                            minHeight: scope.minHeight,
                            disabled: resizeDisabled,
                            onResizeStart: onResizeStart,
                            onResizeStop: onResizeStop
                        }
                    };


                    function convertToInt(val) {
                        var convertedValue = _.parseInt(val, 10);
                        return _.isNaN(convertedValue) ? 0 : convertedValue;
                    }


                    function setInitialSize(initialSize) {
                        var modalContentElement,
                            initialWidth,
                            initialHeight,
                            deltaWidth,
                            deltaHeight,
                            minWidth,
                            minHeight;

                        if (!initialSize || resizeDisabled) {
                            return;
                        }

                        modalContentElement = element.parents('.modal-content').first();

                        if (!modalContentElement || _.isEmpty(modalContentElement)) {
                            return;
                        }

                        minWidth = convertToInt(scope.minWidth);
                        minHeight = convertToInt(scope.minHeight);

                        initialWidth = convertToInt(initialSize.width);
                        initialHeight = convertToInt(initialSize.height);

                        if (initialWidth > 0 && initialWidth >= minWidth) {
                            deltaWidth = initialWidth - modalContentElement.width();
                            modalContentElement.width(initialWidth);                            
                            modalDialogElement.width(modalDialogElement.width() + deltaWidth);
                        }

                        if (initialHeight > 0 && initialHeight >= minHeight) {
                            deltaHeight = initialHeight - modalContentElement.height();
                            modalContentElement.height(initialHeight);                            
                            modalDialogElement.height(modalDialogElement.height() + deltaHeight);
                        }
                    }


                    function onResizeStart(event, resizedElement) {
                        if (!resizedElement || !resizedElement.size || resizeDisabled) {
                            return;
                        }

                        // Capture resize events start width and height
                        dialogSize = {
                            startWidth: resizedElement.size.width,
                            startHeight: resizedElement.size.height
                        };
                    }


                    function onResizeStop(event, resizedElement) {
                        var deltaWidth,
                            deltaHeight;

                        if (!resizedElement || !resizedElement.size || !dialogSize || resizeDisabled) {
                            return;
                        }

                        // Calculate deltas. The modal-dialog is a different size
                        // to the modal-content so just apply deltas
                        deltaWidth = resizedElement.size.width - dialogSize.startWidth;
                        deltaHeight = resizedElement.size.height - dialogSize.startHeight;

                        dialogSize = null;

                        if (deltaWidth) {
                            modalDialogElement.width(modalDialogElement.width() + deltaWidth);
                        }

                        if (deltaHeight) {
                            modalDialogElement.height(modalDialogElement.height() + deltaHeight);
                        }

                        if (scope.persistSize &&
                            scope.dialogId &&
                            (deltaWidth || deltaHeight)) {
                            scope.$emit('eventRelayEvent', { eventName: 'forceRebuildGrid' });
                            spLocalStorage.setObject(scope.dialogId + persistedSizeKeySuffix, {
                                width: resizedElement.size.width,
                                height: resizedElement.size.height
                            });
                        }
                    }


                    if (scope.persistSize &&
                        scope.dialogId &&
                        !resizeDisabled) {
                        var initialWidth = scope.initialWidth;
                        var initialHeight = scope.initialHeight;

                        // Load the dimensions from storage
                        persistedSize = spLocalStorage.getObject(scope.dialogId + persistedSizeKeySuffix);
                        if (persistedSize &&
                            persistedSize.width &&
                            persistedSize.height) {
                            $timeout(function() {
                                setInitialSize(persistedSize);
                            });
                        } else if (initialWidth && initialHeight) {
                            var initialSize = {
                                width: initialWidth,
                                height: initialHeight
                            };
                            $timeout(function() {
                                setInitialSize(initialSize);
                            });
                        }
                    }
                }
            };
        });
}());