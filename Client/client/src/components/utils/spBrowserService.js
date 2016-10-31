// Copyright 2011-2016 Global Software Innovation Pty Ltd
/*global _, angular, console, sp */

/**
 * A set of AngularJS services related to browser version detection.
 * @module common
 */

(function () {
    'use strict';

    angular.module('mod.common.spBrowserService', ['ng'])
        .factory('spBrowserService', spBrowserService);

    /* @ngInject */
    function spBrowserService($window) {

        var model = {
            isChrome: $window.navigator.userAgent.indexOf('Chrome') > -1,
            isSafari: $window.navigator.userAgent.indexOf('Safari') > -1
        };

        if ((model.isChrome) && (model.isSafari)) { model.isSafari = false; }

        var service = {            
        };

        Object.defineProperties(service, {
            'isSafari': {
                get: function () {
                    return model.isSafari;
                },
                enumerable: true
            }
        });
        
        return service;       
    }
})();

