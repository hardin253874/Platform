// Copyright 2011-2016 Global Software Innovation Pty Ltd

/**
 * Helper module for supporting XSRF prevention.
 * @module spXsrf
 */
(function () {
    'use strict';

    angular.module('mod.common.spXsrf', ['ng'])
        .service('spXsrf', function($http) {
            var exports = {};

            var oldToken;                   // the old cookie is maintained when a valid cookie is fetched.

            function getXsrfCookieName(){
                return $http.defaults.xsrfCookieName;
            }

            function getXsrfHeaderName() {
                return $http.defaults.xsrfHeaderName;
            }

            function urlContainsParam(url, paramName) {
                if (url.indexOf('?') == -1) return false;
                var queryStringPairs = url.split("?")[1].split("&");
                return _.some(queryStringPairs, function(paramNameAndValue){
                    return paramNameAndValue.indexOf(paramName + "=") === 0;
                });
            }

            /**
             * Returns new URL based on the supplied URL with supplied query string parameter added as URI-encoded key/value pairs e.g. http://example.com/?my-value=123
             */
            exports.addQueryStringParam = function(url, paramName, paramValue) {
                if(!url || !paramName) return url;
                var param = encodeURIComponent(paramName) + '=' + encodeURIComponent(paramValue);
                if (urlContainsParam(url, paramName)) return url; //no change when duplicating query param is supplied
                var delimiter = url.indexOf('?') == -1 ? '?' : '&';
                return url + delimiter + param;
            };

            /**
             * Returns XSRF token from the XSRF-TOKEN cookie if available. This is primarily to be used to build query strings which include XSRF token.
             * Returns falsey if cookie is missing, cannot be accessed due to HttpOnly attributes, or cookie value itself is falsey.
             */
            exports.getXsrfToken = function() {
                var token = spUtils.getCookie(getXsrfCookieName());
                
                if (token) {
                    oldToken = token;
                }

                return token;
            };

            /**
             * Returns URI with added XSRF token to the supplied uri using query string parameters. Same URI is returned when no XSRF token is available.
             */
            exports.addXsrfTokenAsQueryString = function(uri) {
                if(!uri) return uri;
                var xsrfToken = exports.getXsrfToken();

                if (!xsrfToken) 
                    xsrfToken = oldToken;           // fall back to the old token to prevent icons from refreshing when timeouts occur

                if(!xsrfToken)
                    return uri;

                var xsrfHeaderName = getXsrfHeaderName();
                return exports.addQueryStringParam(uri, xsrfHeaderName, xsrfToken);
            };

            /**
             * Returns true if the URI already has the XSRF token added. False if not.
             */
            exports.uriContainsXsrfToken = function (uri) {
                if (!uri) return uri;
                return urlContainsParam(uri, getXsrfHeaderName());
            };
                
            return exports;
        });
}());