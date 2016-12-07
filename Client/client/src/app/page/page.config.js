// Copyright 2011-2016 Global Software Innovation Pty Ltd
/*global _, angular, console, sp */

(function () {
    'use strict';

    angular.module('mod.app.page').config(pagesConfig);

    function pagesConfig($stateProvider) {
        'ngInject';

        $stateProvider.state('page', {
            url: '/{tenant}/{eid}/page?path&formId',
            template: `<rn-page></rn-page>`,
            data: {}
        });
    }

}());
