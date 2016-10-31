// Copyright 2011-2016 Global Software Innovation Pty Ltd
/*global angular, console, spEntity */


/**
 * the utcDummyService module
 * @class
 * @name utcDummyService
 *
 * @classdesc 
 * spApps.utcDummyService is a set of client side services working against the entity webapi service.
 */
angular.module('spApps.utcDummyService', ['mod.common.spWebService']);

angular.module('spApps.utcDummyService').factory('utcDummyService', function ($http, spWebService) {
    'use strict';
    
    /**
     * @ngdoc service
     * @name sp_common.service:utcDummyService
     * @description A set of client side services working against the eventHandler webapi service.
     */

    var utcDummy = {};

    var logonAlert, webApiRoot = '';

    /** internal to assist testing */
    utcDummy.getHeaders = function () {
        return spWebService.getHeaders();
    };


    /** set to a warning if issues with authentication **/
    utcDummy.logonAlert = function () {
        return logonAlert;
    };


    utcDummy.setWebApiRoot = function (path) {
        console.log('utcDummyService: setting webapi root to "' + path + '"');
        webApiRoot = path.replace(/\/+$/, '');
    };


    function getMsTimeZoneNameFromUriUrl() {
        var url = webApiRoot + '/spapi/data/v1/test/detecttimezonetest/mstzuri?tz=';
        return url;
    }

    function getMsTimeZoneNameFromHeaderUrl() {
        var url = webApiRoot + '/spapi/data/v1/test/detecttimezonetest/mstzheader';
        return url;
    }
    
    
    /**
    * passes the Olson timezone name in a custom header 'Tz' and gets the display name of corresponding windows time zone name.
    *
    * @function
    * @name utcDummyService#getMsTzFromHeader
    */
    utcDummy.getMsTzFromHeader = function () {
        return $http({
            method: 'GET',
            url: getMsTimeZoneNameFromHeaderUrl(),
            headers: spWebService.getHeaders()
        });
    };
    
    /**
    * passes the Olson timezone name in a query string and gets the display name of corresponding windows time zone name.
    *
    * @function    
    * @param {string} Olson timezone name.
    *
    * @function
    * @name utcDummyService#getMsTzFromUrl
    */
    utcDummy.getMsTzFromUrl = function (tz) {
        return $http({
            method: 'GET',
            url: getMsTimeZoneNameFromUriUrl() + encodeURIComponent(tz),
            headers: spWebService.getHeaders()
        });
    };


    return utcDummy;
});
