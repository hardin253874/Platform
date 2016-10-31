/*global console, angular, _*/

(function () {
    'use strict';

    var mappedTemplateUrls = {};

    angular.module('testPagesAppHelper', [])
        .config(function ($provide, $httpProvider) {
            $httpProvider.interceptors.push(function () {
                return {
                    'request': function (config) {
                        //console.log('http request', config.url);
                        if (config.url.match(/\.tpl\.html/) && mappedTemplateUrls[config.url]) {
                            config.url = mappedTemplateUrls[config.url];
                            console.log('MAPPED to', config.url);
                        }
                        return config;
                    }
                };
            });
        })
        .factory('testPagesAppHelper', function ($templateRequest, $templateCache, $q) {
            return {
                basePath: '../../',
                appTemplate: function (templateUrl) {
                    var id = templateUrl.substring('src/app/'.length);
                    mappedTemplateUrls[id] = this.basePath + templateUrl;
                },
                componentTemplate: function (templateUrl) {
                    var id = templateUrl.substring('src/components/'.length);
                    mappedTemplateUrls[id] = this.basePath + templateUrl;
                },
                getWebApiRoot: getWebApiRoot,
                cacheTemplate: cacheTemplate,
                requestTemplates: requestTemplates
            };

            function requestTemplates() {
                return $q.all(_.map(_.keys(mappedTemplateUrls), cacheTemplate));
            }

            function cacheTemplate(id, src) {
                return $templateRequest(src || id).then(function (template) {
                    $templateCache.put(id, template);
                    return template;
                });
            }

            function getWebApiRoot() {
                // We can't base on the current window location as it may not be same machine, or it
                // might be a test runner on localhost....
                // Maybe to pull from a js file that is created/updated when the test is run
                // and that can be overridden by a url parameter to this page.

                // window.spapiBasePath is typically defined by a test runner and pulled from the environment
                //   (using this one for ENTDATA.LOCAL domain based)
                // window.spapiBasePathLocal exists so we can define one for a given machine
                //   (using this one for non-domain machines)

                var webApiRoot = '';

                if (window.spapiBasePathLocal) {
                    webApiRoot = window.spapiBasePathLocal;
                } else if (window.spapiBasePath) {
                    webApiRoot = window.spapiBasePath;
                }

                if (!webApiRoot) console.warning('webapiroot for test is empty');

                return webApiRoot.replace(/\/+$/, '');
            }
        });
})();
