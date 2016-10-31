// Copyright 2011-2016 Global Software Innovation Pty Ltd
/*global angular, console */

/**
 * the spDialog control module
 * @class
 * @name spDialog
 *
 * @classdesc 
 * common.service:spDialog
 */
angular.module('sp.common.spDialog', ['mod.common.ui.spDialogService', 'spApps.eventHandlerService'])
    .factory('spDialog', ['spDialogService', 'eventHandlerService', function (spDialogService, eventHandlerService) {
        "use strict";

        var spDialog = {            
            /**
             * Show the user error to dialog message box.
             *
             * @function
             * @param {string} title - the error title of dialog  
             * @param {string} message - the error message of dialog         
             * @param {bool} [optional] posttoservice - post the error to service 
             *
             * @function
             * @name spDialog#showUserError
             */
            showUserError: function (title, msg, posttoservice) {

                if (posttoservice === undefined) {
                    posttoservice = false;
                }

                var btns = [{ result: 'ok', label: 'OK' }];
                if (posttoservice === true) {
                    eventHandlerService.postEvent(msg).then(function(response) {

                        if (response === true) {
                            msg = 'message \"' + msg + '\" posted back to server successed';
                        } else {
                            msg = 'message \"' + msg + '\" posted back to server failed';
                        }


                        spDialogService.showMessageBox(title, msg, btns);
                    });
                } else {
                    spDialogService.showMessageBox(title, msg, btns);
                }
            },
            /**
             * Show the internal error to dialog message box.
             *
             * @function             
             * @param {string} message - the error message of dialog               
             *
             * @function
             * @name spDialog#showInternalError
             */
            showInternalError: function (msg) {
                var title = 'Internal Error';
                var btns = [
                    { result: 'ok', label: 'OK' }
                ];

                eventHandlerService.postEvent(msg).then(function (response) {

                    spDialogService.showMessageBox(title, msg, btns);
                });
            },


           /**
           * Show the confirm message to dialog message box.
           *
           * @function             
           * @param {string} message - the error message of dialog               
           *
           * @function
           * @name spDialog#confirmDialog
           */           
            confirmDialog: function (title, msg) {
                var btns = [                   
                    { result: true, label: 'OK' },
                     { result: false, label: 'Cancel' }
                ];

                return spDialogService.showMessageBox(title, msg, btns);
            },
            
            /**
            * Show the confirm message to dialog message box.
            *
            * @function             
            * @param {string} message - the error message of dialog               
            *
            * @function
            * @name spDialog#confirmDialog
            */           
            messageBox: function (title, msg) {
                var btns = [                   
                    { result: 'ok', label: 'OK' }
                ];

                return spDialogService.showMessageBox(title, msg, btns);
            }
        };

        return spDialog;
    }]);
