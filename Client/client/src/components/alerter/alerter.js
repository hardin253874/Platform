// Copyright 2011-2016 Global Software Innovation Pty Ltd
/*global _, angular */

(function () {
    'use strict';

    angular.module('mod.common.alerts', [])
        .factory('spAlertsService', function ($timeout) {
            var exports = {};
            var alerts = [];
            var alertTimer;

            function now() {
                return new Date().getTime();
            }

            function alertHasExpired(now, alert) {
                return alert.expires && (now - alert.__added) > alert.expires * 1000;
            }

            function expireAlerts() {
                alerts = _.reject(alerts, _.partial(alertHasExpired, now()));
                alertTimer = _.some(alerts, 'expires') ? $timeout(expireAlerts, 1000) : null;
            }

            /**
             * Get the alerts array. For read-only purposes. Don't modify.
             */
            exports.getAlerts = function () {
                return alerts;
            };

            exports.sev = { Error: 'error', Warning: 'warning', Info: 'info', Success: 'success' };

            /**
             * Add a new alert based on the given message.
             *
             * The second parameter is either a string representing the severity, default 'info'
             * or an options object with possible properties:
             * severity - affects the visuals,
             * expires - true, or number of seconds before the alert is automatically removed, 0 for never,
             * canClose - bool indicating where or not the user can remove the alert, default true.
             *
             * The caller can include other properties in the options and they'll be retained on the alert
             * and usable in such as removeAlertsWhere()
             *
             * @returns the alert object
             */
            exports.addAlert = function (message, options) {
                var alert;
                if (_.isString(options)) {
                    options = { severity: options };
                }
                alert = _.extend({ message: message, severity: 'info', expires: 0, canClose: true }, options);

                // replace a duplicate alert based on message and severity.
                exports.removeAlertsWhere({ message: alert.message, severity: alert.severity });
                
                alerts = alerts.concat([alert]);
                if (alert.expires) {
                    if (alert.expires === true) {
                        alert.expires = 5; // default to 10 seconds
                    }
                    alert.__added = now();
                    alertTimer = alertTimer || $timeout(expireAlerts, 1000);
                }
                return alert;
            };

            /**
             * Removes the given alert
             */
            exports.removeAlert = function (alert) {
                alerts = _.without(alerts, alert);
            };

            /**
             * Removed all alerts with properties matching the given object's properties.
             * For example if you have added one or more alerts with an options object including
             * a member such as groupId: 666, you could then remove all using
             *      removeAlertsWhere({ groupId: 666 })
             * and this would save you from tracking the added alerts yourself.
             */
            exports.removeAlertsWhere = function (obj) {
                alerts = _.reject(alerts, obj);
            };

            return exports;
        })
        .directive('spAlertsControl', function ($rootScope, spAlertsService) {
            return {
                restrict: 'EACM',
                scope: {},
                templateUrl: 'alerter/alertsControl.tpl.html',
                link: function (scope, el, attrs) {
                    scope.alerts = spAlertsService.getAlerts;
                    scope.remove = spAlertsService.removeAlert;

                    scope.$watch('alerts()', function () {
                        $rootScope.$broadcast('app.layout');
                    });
                }
            };
        });
}());