// Copyright 2011-2016 Global Software Innovation Pty Ltd
/*global _, console, angular */

(function() {
    "use strict";

    angular.module("mod.app.spDocumentationService",
    [
        "mod.common.spFormSubmitService",
        "mod.common.spConsoleService"       
    ]);   

    angular.module("mod.app.spDocumentationService")
        .factory("spDocumentationService", spDocumentationService);

    function spDocumentationService($q, spFormSubmitService, spConsoleService) {
        "ngInject";        

        // The settings need to be cached.
        // If we try and navigate to the documentation url in a
        // promise callback, the browser will assume a popup is being displayed and block it,
        // as it was regard it as not originating from a trusted event (like a click).
        // Having the data cached means that the navigation to the doco url
        // happens in the context of the click and not in response to an ajax call.
        let cachedDocoSettings = null;

        return {
            initializeDocoSettings,
            navigateToDocumentation
        };

        //-------------------------------- Public Methods --------------------------------      

        /**
         * Initializes the doco settings.
         */
        function initializeDocoSettings() {
            getDocoSettings();
        }

        /**
         * Navigates to the documentation at the specified url.         
         * @param {string} urlName The field name of the url to load.
         */
        function navigateToDocumentation(urlName) {
            // Get the setting just in case
            getDocoSettings();            
                        
            if (!cachedDocoSettings || !urlName || urlName === "documentationUserName" || urlName === "documentationUserPassword") {
                return;
            }

            const url = sp.result(cachedDocoSettings, urlName);
            if (!url) {
                return;
            }

            const data = {
                
            };

            // Assume documentation is confluence at the moment and post the
            // credentials
            if (cachedDocoSettings.documentationUserName) {
                data.os_username = cachedDocoSettings.documentationUserName;
            }

            if (cachedDocoSettings.documentationUserPassword) {
                data.os_password = cachedDocoSettings.documentationUserPassword;
            }
            
            spFormSubmitService.sumbitFormData("post", url, data, "_blank");            
        }


        //-------------------------------- Private Methods --------------------------------                

        /**
         * Gets the documentation settings
         * @returns {object} The doco settings
         */
        function getDocoSettings() {
            if (cachedDocoSettings) {
                return $q.when(cachedDocoSettings);
            } else {
                return spConsoleService.getDocoSettings().then(settings => {
                    if (!settings) {
                        return null;
                    }

                    cachedDocoSettings = settings;
                    return cachedDocoSettings;
                });                
            }
        }
    }
}());