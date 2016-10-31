// Copyright 2011-2016 Global Software Innovation Pty Ltd

/**
 * Module for mobile related general stuff.
 *
 * @module spMobile
 */
angular.module('mod.common.spMobile', ['ng'])


    // What mobile context are we in - set by the app
    .value('spMobileContext', {
        isMobile: false,
        isTablet: false,

        setContextFromWindow: function (window) {
            var body = angular.element(window.document.body);

            var isMobile = window.navigator.userAgent.match(/iPhone/i) ||
                window.navigator.userAgent.match(/Windows Phone/i) ||
                (window.navigator.userAgent.match(/Android/i) && window.navigator.userAgent.match(/Mobile/i));

            var isTablet = window.navigator.userAgent.match(/iPad/i) ||
                (window.navigator.userAgent.match(/Android/i) && !window.navigator.userAgent.match(/Mobile/i));

            this.isMobile = !!isMobile;
            this.isTablet = !!isTablet;
        },

        setContextFromLocation: function (location) {

            if (!_.isUndefined(location.search().isMobile)) {
                this.isMobile = true;
            }

            if (!_.isUndefined(location.search().isTablet)) {
                this.isMobile = true;
                this.isTablet = true;
            }

            if (!_.isUndefined(location.search().isDesktop)) {
                this.isMobile = false;
                this.isTablet = false;
            }
        }
         
    })

    // Inject mobile context into the current scope
    .directive('spMobileContext', function (spMobileContext) {
        return function (scope, element, attrs) {            if (!scope.spMobileContext) {
                scope.spMobileContext = spMobileContext;
            }
        };
    });

        

