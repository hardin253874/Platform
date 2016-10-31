// Copyright 2011-2016 Global Software Innovation Pty Ltd
(function () {
    'use strict';

    /**
      * Module implementing a directive allowing modal dialogs to be moved.        
      * This directive assumes modal dialog are created with bootstrap styles.
      * This directive requires that the modal dialog have the following:
      * <ul>
      *   <li>Modal Window - A div whose class is modal. This div is the modal window root element.
      *   <li>Modal Header - A div whose class is modal-header. This div will be draggable part of the modal.      
      *   <li>Modal Dialog - A div whose class is modal-dialog. This div is the div that is moved.
      * </ul>      
      *
      * @module spMoveableModal        
      * @example            
         
      Using the spMoveableModal:
     
      Add sp-moveable-modal as an attribute or as a class of the modal dialog that is to support being moved.            
      */
    angular.module('mod.common.ui.spMoveableModal', [])
        .directive('spMoveableModal', function ($document) {
            return {
                restrict: 'AC',                
                link: function (scope, iModalElement, iAttrs) {
                    var startX = 0,
                        startY = 0,
                        isMoving,
                        iModalHeaderElement,
                        iModalDialogElement,                        
                        body,
                        onDocumentMouseMoveThrottled,
                        iModalWindow;


                    // Find the modal window
                    iModalWindow = iModalElement.parents('.modal').first();


                    // Find the modal header
                    iModalHeaderElement = iModalElement.find('.modal-header').first();


                    // Find the modal dialog element
                    iModalDialogElement = iModalElement.parents('.modal-dialog').first();


                    // No modal header or window was found return
                    if (!iModalHeaderElement ||
                        !iModalDialogElement ||
                        !iModalWindow ||
                        _.isEmpty(iModalHeaderElement)) {
                        return;
                    }                                        


                    // Create a throttled mouse move handler
                    onDocumentMouseMoveThrottled = _.throttle(onDocumentMouseMove, 50);                                       


                    // Add a mouse down event handler to the modal header
                    iModalHeaderElement.on('mousedown', onModalHeaderElementMouseDown);


                    // Remove the outline from the modal window
                    iModalWindow.css({
                        'outline': 'none'
                    });


                    // Change the cursor style of the header
                    iModalHeaderElement.css({
                        cursor: 'move'
                    });                    
                     

                    iModalWindow.on('$destroy', function () {
                        // Remove the mouse down event
                        iModalHeaderElement.off('mousedown', onModalHeaderElementMouseDown);                        
                    });                                       


                    // Handle mouse down events
                    function onModalHeaderElementMouseDown(event) {
                        var rightClick = (event.which) ? (event.which === 3) : (event.button === 2);

                        if (rightClick || isMoving) {
                            return;
                        }

                        // Prevent the browser's default behavior so that cursor remains the move one.
                        event.preventDefault();

                        // Set moving flag
                        isMoving = true;
                        // Set start pos 
                        startX = event.pageX;
                        startY = event.pageY;

                        // Setup events on document
                        $document.on('mousemove', onDocumentMouseMoveThrottled);
                        $document.on('mouseup', onDocumentMouseUp);                       
                    }


                    // Handle mouse move events
                    function onDocumentMouseMove(event) {
                        var x, y, dialogOffset;

                        if (!isMoving) {
                            return;
                        }

                        dialogOffset = iModalDialogElement.offset();

                        // Calculate new modal left and top
                        x = dialogOffset.left + (event.pageX - startX);
                        y = dialogOffset.top + (event.pageY - startY);

                        // Reset the start pos
                        startX = event.pageX;
                        startY = event.pageY;                                                                                                                            
                            
                        iModalDialogElement.css({
                            'margin-left': x + 'px',
                            'margin-right': 0,
                            'margin-bottom': 0,
                            'margin-top': y + 'px'
                        });                        
                    }                    


                    // On mouse up remove the mouse move and mouse up handler
                    function onDocumentMouseUp() {
                        isMoving = false;
                        $document.off('mousemove', onDocumentMouseMoveThrottled);
                        $document.off('mouseup', onDocumentMouseUp);                        
                    }
                }
            };
        });
}());