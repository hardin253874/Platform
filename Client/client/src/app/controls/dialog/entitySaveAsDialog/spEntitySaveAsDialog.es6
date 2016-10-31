// Copyright 2011-2016 Global Software Innovation Pty Ltd
/*global _, console, angular, sp */


(function () {
    'use strict';
    
    angular.module('app.controls.dialog.spEntitySaveAsDialog')
        .factory('spEntitySaveAsDialog', spEntitySaveAsDialog);

    /* @ngInject */
    function spEntitySaveAsDialog(spDialogService) {

        let exports = {
            showModalDialog
        };

        return exports;

        /**
        * Show the entity save as dialog
        */
        function showModalDialog(options, defaultOverrides) {
            const dialogDefaults = {
                title: 'Entity Save As',
                keyboard: true,
                backdropClick: true,
                windowClass: 'modal entitySaveAsDialog',
                templateUrl: 'controls/dialog/entitySaveAsDialog/spEntitySaveAsDialog.tpl.html',
                controller: 'spEntitySaveAsDialogController',
                resolve: {
                    options: function() {
                        return options;
                    }
                }
            };

            if (defaultOverrides) {
                angular.extend(dialogDefaults, defaultOverrides);
            }

            return spDialogService.showModalDialog(dialogDefaults);
        }
    }    
}());

