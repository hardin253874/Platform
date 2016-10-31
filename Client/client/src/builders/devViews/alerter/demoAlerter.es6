// Copyright 2011-2016 Global Software Innovation Pty Ltd
(function () {
    'use strict';

    angular.module('app.demoAlerter', ['mod.common.alerts'])
        .controller('demoAlerter', function ($scope, $timeout, spAlertsService) {
            $scope.message = 'Message to raise';
            $scope.canClose = true;
            $scope.expires = false;

            function opts(severity) {
                return {
                    severity: severity,
                    expires: $scope.expires,
                    canClose: $scope.canClose
                };
            }

            $scope.raiseError = message => {
                spAlertsService.addAlert(message, opts(spAlertsService.sev.Error));
            };

            $scope.raiseInfo = message =>{
                spAlertsService.addAlert(message, opts(spAlertsService.sev.Info));
            };

            $scope.raiseSuccess = message => {
                spAlertsService.addAlert(message, opts(spAlertsService.sev.Success));
            };

            $scope.raiseCountdown = () => {
                var message = "Starting";

                var myOpts = { severity: spAlertsService.sev.Info, expires: false, canClose: true };

                var alert = spAlertsService.addAlert(message, myOpts);
                var count = 5;

                var downFn = () => {
                    alert.message = "Count down " + count--;

                    if (count >= 0)
                        $timeout(downFn, 1000);
                    else {
                        alert.severity = spAlertsService.sev.Success;
                        $timeout(() => spAlertsService.removeAlert(alert), 2000);
                    }
                };

                downFn();
            };


        });
}());