// Copyright 2011-2016 Global Software Innovation Pty Ltd
/*global _, angular, console, sp */

/**
 * A set of AngularJS services related to downloading files.
 * @module common
 */

(function () {
    'use strict';

    angular.module('mod.common.spFileDownloadService', ['ng', 'mod.common.spBrowserService'])
        .factory('spFileDownloadService', spFileDownloadService);

    /* @ngInject */
    function spFileDownloadService($window, $document, spBrowserService) {

        var service = {
            downloadFile: downloadFile
        };

        return service;

        // Downloads a file from the specified url by
        // creating an anchor tag and using the HTML5 download attribute.        
        function downloadFile(url, fileName) {
            if (!url) {
                return;
            }

            var body = $document.find('body'),
                link;

            if (spBrowserService.isSafari) {
                // Safari doesn't support download attribute                
                $window.location.href = url;
            } else {
                link = angular.element('<a></a>');
                link.attr('href', url);
                // Content-disposition filename header from server overrides this value
                link.attr('download', fileName || '');

                try {
                    body.append(link);
                    link[0].click();
                } finally {
                    link.remove();
                }                                            
            }
        }
    }
})();

