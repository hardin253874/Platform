/*global _, angular, console, expect, waitsFor, runs, spEntity, sp, spEntityUtils, jasmine, it, describe */

var TestSupport;

(function (TestSupport) {
    'use strict';

    // Monkey patch the xit and xdescribe routines so we can get an indication of ignored tests.
    // Unfortunately they'll show up as passed tests.... outstanding is to have them show up
    // as ignored.... but that needs investigating.

    jasmine.Env.prototype.xit = function (description, func) {
        it('IGNORED:' + description, function () {
            console.log('IGNORING SPEC: ' + description);
        });
    };

    jasmine.Env.prototype.xdescribe = function(description, specDefinitions) {
        describe('IGNORED:' + description, function () {
            console.log('IGNORING DESCRIBE: ' + description);
            it('IGNORED: entire describe() suite');
        });
    };

    function checkResults(results, checkHasData) {
        return results.cols && _.isArray(results.cols) && (!checkHasData || results.cols.length >= 0) &&
            results.data && _.isArray(results.data) && (!checkHasData || results.data.length >= 0);
    }

    var scope; // set using setScope()
    var waitTimeout = 50000; // set using setWaitTimeout
    var firstWaitTimeout = 120000; //120000
    var testToken;

    function clearAuthenticationTicket() {
        localStorage.removeItem('userIdentity');
    }

    /**
     * Use the following in a beforeEach function to perform the typical setup configuration
     * for unit (spec) tests.
     *
     * This can only be called after your modules have been loaded or you'll get errors about
     * injector already created.
     */
    TestSupport.setupUnitTests = function (context, $injector) {
        clearAuthenticationTicket();
        context.addMatchers(TestSupport.matchers);
        TestSupport.setScope($injector.get('$rootScope'));

    };

    /**
     * Use the following in a beforeEach function to perform the typical setup configuration
     * for integration tests.
     *
     * Optionally specify whether you want full passthrough on any $http.
     * If you choose not to, then you can configure the $httpBackend how you like - it is the
     * mock httpbackend with passthrough enabled - discussed in the Angular doc under E2e backend.
     *
     * This can only be called after your modules have been loaded or you'll get errors about
     * injector already created.
     */
    TestSupport.setupIntgTests = function (context, $injector, options) {

        var defaults = { enablePassthrough: true, skipLogin: false };
        var spWebService = $injector.get('spWebService');

        options = options || {};

        options = _.defaults(options, defaults);

        context.addMatchers(TestSupport.matchers);
        TestSupport.setScope($injector.get('$rootScope'));

        TestSupport.configureWebApi($injector);


        if (options.enablePassthrough) {
            var $httpBackend = $injector.get('$httpBackend');
            $httpBackend.whenGET(/.*/).passThrough();
            $httpBackend.whenPOST(/.*/).passThrough();
            $httpBackend.whenPUT(/.*/).passThrough();
            $httpBackend.whenDELETE(/.*/).passThrough();
            

            if (!options.skipLogin) {
                TestSupport.login($injector);
            }
        }

    };

    // add these in a beforeEach using this.addMatchers(EntityMatchers.matchers)

    TestSupport.matchers = {
        toBeArray: function (length) {
            if (!_.isArray(this.actual)) {
                return false;
            }
            if (!_.isUndefined(length) && this.actual.length !== length) {
                return false;
            }
            return true;
        },
        toBeEmptyArray: function () {
            return _.isArray(this.actual) && this.actual.length === 0;
        },
        toBeReportResults: function () {
            var results = this.actual;
            return results && checkResults(results);
        },
        toBeReportResultsWithData: function () {
            var results = this.actual;
            return results && checkResults(results, true);
        },
        toBePickerReportResults: function () {
            var actual = this.actual;
            return actual && actual.results && checkResults(actual.results);
        },
        toBePickerReportResultsWithData: function () {
            var actual = this.actual;
            return actual && actual.results && checkResults(actual.results, true);
        },
        toBeEntity: function (message) {
            var entity = this.actual;
            message = message ? (message + ': ') : '';
            expect(entity).toBeTruthy(message + ' Expected entity to be truthy ');
            expect(entity instanceof spEntity._Entity).toBeTruthy(message + ' Expected instanceof spEntity._Entity ');
            return entity instanceof spEntity._Entity;
        },
        toBeEntityData: function () {
            var entity = this.actual;
            expect(entity).toBeTruthy();
            expect(entity.ids).toBeTruthy();
            expect(entity.ids.length).toBeGreaterThan(0);
            expect(entity.entityRefs).toBeTruthy();
            expect(entity.entityRefs.length).toBeGreaterThan(0);
            expect(entity.entities).toBeTruthy();
            expect(entity.entities.length).toBeGreaterThan(0);
            return true;
        },
        toBePromise: function () {
            return _.isFunction(this.actual.then);
        },
        toStartWith: function(s) {
            return _.isString(this.actual) && this.actual.indexOf(s) === 0;
        }
    };

    TestSupport.configureWebApi = function ($injector) {
        var spWebService, webApiRoot, headers;

        spWebService = $injector.get('spWebService');

        webApiRoot = TestSupport.getWebApiRoot();
        expect(webApiRoot).toBeTruthy('configureWebApi: webApiRoot');

        spWebService.setWebApiRoot(webApiRoot);
        console.log('webApiRoot', webApiRoot);

        headers = spWebService.getHeaders();
        expect(headers).toBeTruthy('configureWebApi: headers');

        console.log('TESTSUPPORT: jasmine spec=' + sp.result(jasmine, 'getEnv.currentSpec.description'));

    }

    var loginResult;
    var loginData;


    TestSupport.login = function ($injector) {
        var spLoginService = $injector.get('spLoginService');
        var spWebService = $injector.get('spWebService');
      
        if (!loginResult) {
            loginResult = spLoginService.readiNowLogin('EDC', 'Administrator', 'tacoT0wn', false, true);

            TestSupport.wait(loginResult.then(function (result) {
                loginData = result;
                testToken = result.testToken;

                expect(testToken).toBeTruthy('TestToken missing, you need to configure your server to be in integrationTest mode. Use "PlatformConfigure -intgModeOn" ')

                return result;
            }));
        } else  {
            spLoginService.testSupportReadiNowLogin(loginData);
        }

       
    };

    TestSupport.getWebApiRoot = function () {
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

        return webApiRoot.replace(/\/+$/, '');
    };

    TestSupport.getTestEntityName = function (name, param) {
        if (!param) {
            param = Math.floor((Math.random() * 10000) + 1);
        }
        return 'Unit Tests: ' + name + ' {{' + param + '}}';
    };

    TestSupport.getUpdatedTestEntityName = function (name, param, paramValue) {
        return name.replace('{{' + param + '}}', '(' + paramValue + ')');
    };

    TestSupport.wait = function (promise, options) {
        var opts = options || {};
        TestSupport.waitCheckReturn(promise, {}, opts.customFlush);
    };

    var _isFirstWait = true;
    TestSupport.waitCheckReturn = function (a, b, c, d, customWaitMs) {

        // Executes workerReturnsPromise, storing the result in resultContainer.value.
        var done, error, scopeToUse, workerPromise, resultContainer, customFlush;

        // sort out the arguments... can be either
        //  scope, workerPromise, resultContainer
        // or
        //  workerPromise, resultContainer

        if (arguments.length < 2) {
            throw new Error('invalid arguments to waitCheckReturn');
        }
        if (!a.$apply && a.then) {  // TestSupport.wait takes this path
            // scope not passed
            if (!scope) {
                throw new Error('TestSupport.setScope has not been called before waitCheckReturn and no scope passed');
            }
            scopeToUse = scope;
            workerPromise = a;
            resultContainer = b;
            customFlush = c;
        } else {
            // scope passed
            scopeToUse = a;
            workerPromise = b;
            resultContainer = c;
            customFlush = d;
        }

        // now get to it...

        workerPromise.then(function (result) {
            done = true;
            if (resultContainer) {
                resultContainer.value = result;
            }

        }, function (err) {
            done = true;
            error = err;
        });

        // Flush any async events.
        scopeToUse.$apply();

        if (customFlush) {
            customFlush();
        }

        // wait up to 60 secs on the first test.. (longer than the default 5 secs as a first test can take a while
        // if it needs to spin up an idle server)
        var waitTime = customWaitMs ? customWaitMs : (_isFirstWait ? Math.max(firstWaitTimeout, waitTimeout) : waitTimeout);
        _isFirstWait = false;

        waitsFor(function () {
            return done;
        }, waitTime);

        runs(function () {
            if (error) {
                var msg = error.ExceptionMessage || error.status || error.toString();
                console.error('%s - ERROR', this.description, error, msg);
                expect('the worker failed with error ' + msg).toBeFalsy();
            }
        });
    };

    TestSupport.setScope = function (value) {
        scope = value;
    };

    TestSupport.setWaitTimeout = function (value) {
        waitTimeout = value;
    };

    TestSupport.validateEntity = function (entity) {
        var all = spEntityUtils.walkEntities(entity);
        for (var i = 0; i < all.length; i++) {
            var e = all[i];
            if ((e._typeIds === null || _.isUndefined(e._typeIds) || e._typeIds.length === 0) && e._dataState === spEntity.DataStateEnum.Create) {
                throw new Error('Found a \'create\' entity without a typeId');
            }
            if (e._typeIds.length === 1 && _.isFunction(e._typeIds[0]._id)) {
                throw new Error('Hey, the ID for this entity is actually a function. Oops.');
            }
        }
    };

})(TestSupport || (TestSupport = {}));
