// Copyright 2011-2016 Global Software Innovation Pty Ltd
/*global _, console, angular, sp */

(function() {
    "use strict";

    angular.module("mod.common.spFormSubmitService",
    [        
    ]);

    angular.module("mod.common.spFormSubmitService")
        .factory("spFormSubmitService", spFormSubmitService);

    function spFormSubmitService($document) {
        "ngInject";

        return {
            sumbitFormData            
        };

        //-------------------------------- Public Methods --------------------------------

        /**
         * Submits the data using a html form. This gets around any cross domain security issues.
         * @param {string} method post or get
         * @param {string} action The url to submit data to
         * @param {object} data Object containing key value pairs of data to submit         
         * @param {string} target Target of the form. Can be _self, _blank, _parent, _top
         */
        function sumbitFormData(method, action, data, target) {
            method = _.toLower(method);

            if (method !== "post" && method !== "get") {
                throw new Error("method must either be a post or a get");
            }

            if (!action) {
                return;
            }

            const document = $document[0];

            const form = document.createElement("form");

            form.action = action;
            form.method = method;
            form.style.display = "none";
            if (target) {
                form.target = target;   
            }            

            if (data) {
                _.forOwn(data, function(value, name) {
                    const input = document.createElement("input");
                    input.name = name;
                    input.value = value.toString();
                    form.appendChild(input);
                });
            }

            try {
                document.body.appendChild(form);
                form.submit();
            } finally {                
                document.body.removeChild(form);
            }
        }
    }
}());