// Copyright 2011-2016 Global Software Innovation Pty Ltd
/*global angular, console, spEntity, jstz */


/**
 * the eventHandlerService module
 * @class
 * @name eventHandlerService
 *
 * @classdesc 
 * spApps.eventHandlerService is used for post the client side event handler back to server by webapi
 */
angular.module('spApps.eventHandlerService', []);

angular.module('spApps.eventHandlerService').factory('eventHandlerService', ['$http', function ($http) {
    'use strict';

    /**
     * @ngdoc service
     * @name sp_common.service:eventHandlerService
     * @description A set of client side services working against the eventHandler webapi service.
     */

    var exports = {};

    var headers, logonAlert, webApiRoot = '';
    


    /** internal to assist testing */
    exports.getHeaders = function () {
        return headers;
    };
   

    /** set to a warning if issues with authentication **/
    exports.logonAlert = function () {
        return logonAlert;
    };

    exports.setWebApiRoot = function (path) {
        console.log('eventHandlerService: setting webapi root to "' + path + '"');
        webApiRoot = path.replace(/\/+$/, '');
    };


    /** Get the post event post url**/
    function getEventHandlerPostUrl() {

        var url = webApiRoot + '/spapi/data/v1/eventhandler/postevent';
        return url;
    }

   
    /**
    * post the event back to server
    *
    * @function    
    * @param {string} message - the error message of dialog         
    *
    * @function
    * @name eventHandlerService#postEvent
    */
    exports.postEvent = function(message) {
        console.log('postMessage: post ' + message);
        
        var jsonmessage = { "message": message };
       

        return $http({
            method: 'POST',
            url: getEventHandlerPostUrl(),
            data: jsonmessage,
            headers: headers
        }).then(function (webResponse) {
            
            console.log('response status: ' + webResponse.status);
            if (webResponse.status == 201) {
                return true;
            } else {
                return false;
            }
        });
    };


    headers = { Tz: jstz.determine().name() };
    
    return exports;
    

}]);