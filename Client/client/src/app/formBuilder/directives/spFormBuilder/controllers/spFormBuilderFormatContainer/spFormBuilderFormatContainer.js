// Copyright 2011-2016 Global Software Innovation Pty Ltd
(function () {
    'use strict';

    /**
    * Module implementing a dialog to edit the formatting of a custom form container.
    *
    * @module spFormBuilderFormatContainer
    */
    angular.module('mod.app.formBuilder.directives.spFormBuilderFormatContainer', ['ui.bootstrap', 'mod.common.ui.spDialogService'])
        .controller('spFormBuilderFormatContainerController', function ($scope, $uibModalInstance, options) {

            $scope.options = options;
            $scope.modalInstance = $uibModalInstance;

            // OK click handler
            $scope.ok = function () {
                $scope.modalInstance.close(true);
            };

            // Cancel click handler
            $scope.cancel = function () {
                $scope.modalInstance.close(false);
            };
        })
        .service('spFormBuilderFormatContainerService', function (spDialogService) {
            // setup the dialog
            var exports = {
                showDialog: function (options) {
                    var dialogOptions = {
                        title: 'Format Container',
                        templateUrl: 'formBuilder/directives/spFormBuilder/controllers/spFormBuilderFormatContainer/spFormBuilderFormatContainer.tpl.html',
                        controller: 'spFormBuilderFormatContainerController',
                        resolve: {
                            options: function () {
                                return options;
                            }
                        }
                    };

                    return spDialogService.showModalDialog(dialogOptions);
                }
            };

            return exports;
        });
}());