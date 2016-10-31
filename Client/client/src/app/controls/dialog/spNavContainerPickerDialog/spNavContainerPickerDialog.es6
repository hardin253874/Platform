// Copyright 2011-2016 Global Software Innovation Pty Ltd
/*global _, console, angular, sp */


(function() {
    "use strict";

    angular.module("app.controls.dialog.spNavContainerPickerDialog")
        .factory("spNavContainerPickerDialog", spNavContainerPickerDialog);

    /* @ngInject */
    function spNavContainerPickerDialog(spDialogService) {

        const exports = {
            showModalDialog
        };

        return exports;

        /**
        * Show the nav section picker dialog
        */
        function showModalDialog(options, defaultOverrides) {
            const dialogDefaults = {
                title: "Select Section",
                keyboard: true,
                backdropClick: true,
                windowClass: "modal spNavContainerPickerDialogWindow",
                templateUrl: "controls/dialog/spNavContainerPickerDialog/spNavContainerPickerDialog.tpl.html",
                controller: "spNavContainerPickerDialogController",
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