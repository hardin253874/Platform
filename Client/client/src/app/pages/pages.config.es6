// Copyright 2011-2016 Global Software Innovation Pty Ltd
/*global _, angular, console, sp */

(function () {
    'use strict';

    angular.module('mod.app.pages').config(pagesConfig);

    function pagesConfig($stateProvider) {
        'ngInject';

        $stateProvider.state('pages', {
            url: '/{tenant}/{eid}/pages?path&formId',
            template: `<rn-pages></rn-pages>`,
            data: {}
        });
    }

}());
