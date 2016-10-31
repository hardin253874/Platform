// Copyright 2011-2016 Global Software Innovation Pty Ltd
(function () {
    'use strict';

    /////
    // Value Editor test controller.
    /////
    angular.module('app.valueEditorTest', ['ui.bootstrap', 'mod.common.alerts'])
        .controller('valueEditorTestController', function ($scope, valueEditorTestDialog) {

            /////
            // spNumberControl model
            /////
            $scope.model = {
                type: null,
                value: "123",
                types: [
                    {
                        name: 'String',
                        value: spEntity.DataType.String
                    },
                    {
                        name: 'Int32',
                        value: spEntity.DataType.Int32
                    },
                    {
                        name: 'Decimal',
                        value: spEntity.DataType.Decimal
                    },
                    {
                        name: 'Currency',
                        value: spEntity.DataType.Currency
                    },
                    {
                        name: 'Date',
                        value: spEntity.DataType.Date
                    },
                    {
                        name: 'Time',
                        value: spEntity.DataType.Time
                    },
                    {
                        name: 'DateTime',
                        value: spEntity.DataType.DateTime
                    },
                    {
                        name: 'Bool',
                        value: spEntity.DataType.Bool
                    },
                    {
                        name: 'Guid',
                        value: spEntity.DataType.Guid
                    },
                    {
                        name: 'None',
                        value: spEntity.DataType.None
                    }
                ]
            };

            $scope.model.type = $scope.model.types[1];

            $scope.open = function() {
                valueEditorTestDialog.showModalDialog(null).then(function(result) {

                });
            };            
        })
        .factory('valueEditorTestDialog', function (spDialogService) {
            // setup the dialog
            var exports = {

                showModalDialog: function (options, defaultOverrides) {
                    var dialogDefaults = {
                        title: 'Value Editor',
                        backdrop: true,
                        keyboard: true,
                        backdropClick: true,
                        windowClass: 'modal',
                        templateUrl: 'devViews/valueEditor/valueEditorTestDialog.tpl.html',
                        controller: 'valueEditorTestController',
                        resolve: {
                            options: function () {
                                return options;
                            }
                        }
                    };

                    if (defaultOverrides) {
                        angular.extend(dialogDefaults, defaultOverrides);
                    }

                    return spDialogService.showModalDialog(dialogDefaults);
                }

            };

            return exports;
        });
}());