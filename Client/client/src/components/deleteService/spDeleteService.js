// Copyright 2011-2016 Global Software Innovation Pty Ltd


(function () {
    'use strict';

    /**
    * Module implementing a dialog for deleting entities.
    *
    * @module mod.common.ui.spDeleteService
    */
    angular.module('mod.common.ui.spDeleteService', [
        'mod.common.spWebService'
    ])
        .controller('spDeleteController', ['$scope', '$uibModalInstance', '$timeout', '$http', 'options', 'spDeleteService', 'spWebService', function ($scope, $uibModalInstance, $timeout, $http, options, spDeleteService, spWebService) {

            /////
            // Setup the dialog model
            /////
            $scope.model = {
                title: options.title,
                message: options.message,
                okText: options.btns[0].label,
                cancelText: options.btns[1].label,
                busyIndicator: {
                    type: 'spinner',
                    text: 'Loading...',
                    placement: 'element',
                    isBusy: false
                },
                dependentString: 'Dependent resources - calculating...',
                relatedString: 'Related resources - calculating...'
            };

            $scope.$on('signedout', function () {
                $scope.model.cancel();
            });

            $scope.model.close = function(result) {
                $uibModalInstance.close(result);
            };

            $scope.model.loadDeleteDetails = function () {
                var url = spWebService.getWebApiRoot() + '/spapi/data/v2/entity/getDeleteDetails';

                return $http({
                        method: 'POST',
                        url: url,
                        headers: spWebService.getHeaders(),
                        data: options.ids
                    })
                    .then(function (response) {

                        if (response && response.status === 200) {
                            if (response.data) {
                                var count = 0;

                                if (response.data.dependents) {
                                    count = response.data.dependents.length;

                                    $scope.model.dependents = _.groupBy(response.data.dependents, function (item) { return item.typeName; });
                                }
                                $scope.model.dependentsDisabled = count <= 0;
                                $scope.model.dependentString = 'Resources - (' + count + ')';

                                count = 0;

                                if (response.data.related) {
                                    count = response.data.related.length;

                                    $scope.model.related = _.groupBy(response.data.related, function (item) { return item.typeName; });
                                }

                                $scope.model.relatedDisabled = count <= 0;
                                $scope.model.relatedString = 'Related resources - (' + count + ')';
                            }
                        }
                    });
            };

            $scope.model.loadDeleteDetails();
        }])
        .service('spDeleteService', ['spDialogService', function (spDialogService) {
            // setup the dialog
            var exports = {
                showDialog: function (options) {
                    var dialogOptions = {
                        templateUrl: 'deleteService/spDeleteService.tpl.html',
                        controller: 'spDeleteController',
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
        }]);
}());