// Copyright 2011-2016 Global Software Innovation Pty Ltd
(function () {
    'use strict';

    /**
     *  A set of client side services working against the console webapi service.
     *  @module spConsoleService
     */

    angular.module('mod.common.spConsoleService',
        ['mod.common.spWebService'])
        .factory('spConsoleService', function ($http, spWebService) {
            var exports = {};

            /**
             * Gets the session info.     
             * @returns {promise} The session info data.
             */
            exports.getSessionInfo = function () {
                return $http({
                    method: 'GET',
                    url: spWebService.getWebApiRoot() + '/spapi/data/v1/console/sessioninfo',
                    headers: spWebService.getHeaders()
                }).then(function (response) {
                    return response.data;
                });
            };

            /**
             * Gets the documentation settings
             * @returns {promise} The doco settings
             */
            exports.getDocoSettings = function () {
                return $http({
                    method: 'GET',
                    url: spWebService.getWebApiRoot() + '/spapi/data/v1/console/getDocoSettings',
                    headers: spWebService.getHeaders()
                }).then(function (response) {
                    return response.data;
                });
            };

            return exports;
        });
}());