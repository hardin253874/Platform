// Copyright 2011-2016 Global Software Innovation Pty Ltd
/*global _, angular, console, sp */


(function () {
    'use strict';

    angular.module('sp.common.clientUpdateService', ['ngCookies'])

        .factory('spClientUpdateService', function ($window, $cookies, $timeout) {
            var updatingClientKey = 'updatingClient';   // Key for cookie keeping track if an client update is in progress
            var blockUpdatingClient = false;            // if true then do not attempt updating the client - used after an update failure

            return {
                attemptClientRefresh: attemptClientRefresh,
                checkMinClientVersionAndRefresh: checkMinClientVersionAndRefresh,
                areClientUpdatesBlocked: areClientUpdatesBlocked
            };


            /**
            * Check that the client versions meets the minimum. If not a refresh might be required
            */
            function checkMinClientVersionAndRefresh(clientVersion, requiredClientVersion) {
                console.log("Checking client version");

                // We can't do anything unless we have both the server and client info.
                if (requiredClientVersion && clientVersion) {

                    if (clientVersion === '0.0.0') { // Debug client build, don't do anything
                        console.log("Dev client");
                        return true;
                    }

                    if (spUtils.compareVersionStrings(clientVersion, requiredClientVersion) < 0) {
                        console.log("Refreshing client");
                        return !attemptClientRefresh();
                    } else {
                        console.log("Client version OK");
                        $cookies.remove(updatingClientKey);
                        return true;
                    }
                }

                return true;
            }
            /*
            ** Attempt to refresh the client ensuring that we don't end up in a refresh loop
            */
            function attemptClientRefresh() {
                if (blockUpdatingClient) {
                    console.log("Client refresh blocked");
                    return false;
                }

                // Refresh the client after ensuring that we are not in a loop
                if ($cookies.get(updatingClientKey)) {
                    console.log("Refresh failed to update client. Blocking further automatic refreshes.");
                    blockUpdatingClient = true;
                    $cookies.remove(updatingClientKey);                     
                    return false;
                } else {
                    $cookies.put(updatingClientKey, 'true');

                    $timeout(function () { $window.location.reload(true); }, 0);      // Why? If we don't add a small delay the reload becomes unreliable. 

                    return true;                                            
                }
            }

            /*
            ** Are we blocking the update of clients
            */
            function areClientUpdatesBlocked() {
                return blockUpdatingClient;
            }
        });

}());

