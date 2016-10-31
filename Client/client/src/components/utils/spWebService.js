// Copyright 2011-2016 Global Software Innovation Pty Ltd
/*global angular, console, document, spEntity, jstz */

angular.module('mod.common.spWebService', []);

/**
 *  A set of client side services working against the entity webapi service.
 *  @module spWebService
 */

angular.module('mod.common.spWebService', ['mod.common.spXsrf'])
    .service('spWebService', function ($rootScope, spXsrf) {
    'use strict';

    /**
     *  A set of client side services working against the SoftwarePlatform Web API.
     *  @module spWebService
     */
    var exports = {};
    var headers = { Tz: jstz.determine().name() };
    var webApiRoot = '';

    /** Todo: remove me
     * @private
     */
    exports.getWebApiRoot = function () {
        return webApiRoot;
    };

    /**
     * Set the root of the webapi service. This is required if the client is being not being hosted from the same
     * location as the spapi web service.
     *
     * @param {string} path the path to the root of the webapi web. May or may not include a trailing /
     *
     * @example
     *
     * If the spapi web is https://somemachine.local/spapi then call this using
     *
     <pre>
     spWebService.setWebApiRoot('https://somemachine.local/')
     </pre>
     */
    exports.setWebApiRoot = function (path) {
        console.log('spWebService: setting webapi root to "' + path + '"');
        webApiRoot = path.replace(/\/+$/, '');
    };

    /** internal to assist testing
     * @private
     */
    exports.getHeaders = function () {
        return headers;
    };

    /** internal to assist testing
     * @private
     */
    exports.setHeader = function (key, value) {
        headers[key] = value;
        return exports;
    };

    /** internal to assist testing
     * @private
     */
    exports.haveBasicAuthHeader = function () {
        return !!headers.Authorization;
    };

    // for debugging
    $rootScope.spWebServerHeaders = headers;
    $rootScope.$watch('spWebServerHeaders', function (value, prev) {
        if (value && value !== prev) {
            console.log('spWebService.headers has changed:', value, prev);
        }
    }, true);
    // end debugging


    exports.getImageApiUrl = function (imageId) {
        return spXsrf.addXsrfTokenAsQueryString(webApiRoot + '/spapi/data/v1/image/' + imageId);
    };


    exports.getImageApiIconUrl = function (imageId) {
        return spXsrf.addXsrfTokenAsQueryString(webApiRoot + '/spapi/data/v1/image/thumbnail/' + imageId + '/console-iconThumbnailSize/core-scaleImageProportionally');
    };


    exports.getImageApiSmallUrl = function (imageId) {
        return spXsrf.addXsrfTokenAsQueryString(webApiRoot + '/spapi/data/v1/image/thumbnail/' + imageId + '/console-smallThumbnail/core-scaleImageProportionally');
    };

    return exports;

});
