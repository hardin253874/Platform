/*global angular */

var IntgTestSupport;

(function (IntgTestSupport) {
    'use strict';

    // The following is a replacement for the angular mock module that is always loaded
    // prior to each test. For integration tests we want to be able to use their e2e version
    // of the mocked httpBackend as this one allows us to use passThrough().
    // Note - simply trying to use module('ngMockE2E') doesn't work as that decorates the already
    // mocked backend with itself and so passthrough only goes to a mocked backend anyway.

    // This declaration must come after the one from angular-mocks.js.

    angular.module('ngMock', ['ng'])
        .provider({
            $browser: angular.mock.$BrowserProvider,
            $exceptionHandler: angular.mock.$ExceptionHandlerProvider,
            $log: angular.mock.$LogProvider,
            $rootElement: angular.mock.$RootElementProvider
        })
        .config(function ($provide) {
            $provide.decorator('$timeout', angular.mock.$TimeoutDecorator);
            $provide.decorator('$httpBackend', angular.mock.e2e.$httpBackendDecorator);
        });

})(IntgTestSupport || (IntgTestSupport = {}));
