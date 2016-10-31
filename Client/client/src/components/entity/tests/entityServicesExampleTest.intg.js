// Copyright 2011-2016 Global Software Innovation Pty Ltd
/*global _, angular, console, module, describe, beforeEach, it, inject, expect, runs, waitsFor, TestSupport, sp, spEntity */

describe('Internal|Sample Test|intg:', function () {
    'use strict';

    beforeEach(module('sp.common.loginService'));

    describe('async integration test example with real $http', function () {

        var $injector, $http, $rootScope, $q,
            spWebService, spEntityService,
            testEntityIds = [], testEntityId;


        beforeEach(inject(function ($injector) {

            // load a DI table with the modules needed
            //
            // Don't try to use the "inject" wrapper around your test/"it" functions as you'll typically see
            // in the AngularJS doc. That does do injection but it'll use a mock http backend. Fine for unit
            // tests but not what we want for "integration" tests.
            //

            // this is how you get the real $http service if you wanted to doing anything with it
            $http = $injector.get('$http');

            // grab the $rootScope as we need to do $apply after any async work to flush out any promises
            $rootScope = $injector.get('$rootScope');

            // grab the spWebService and spEntityService
            spWebService = $injector.get('spWebService');
            spEntityService = $injector.get('spEntityService');

            TestSupport.setupIntgTests(this, $injector);

        }));

        // Don't wrap your "it" function with angular-mock's inject function to get things injected
        // or else you'll get the mocked http backend. Use the $injector we loaded earlier.
        it('the spEntityService can get a known entity (oldshared:employee)', function () {

            var done, error, data;

            // Note - don't do "expect"s in promise (then) functions... they'll be quietly ignored
            // Do them in the "runs" function later

            spEntityService.getEntity('oldshared:employee', 'name')
                .then(function (result) {
                    done = true;
                    data = result;

                }, function (err) {
                    // All async calls need to catch the failed case and set the flag that we will "waitFor"
                    // otherwise when errors occur things just time out rather than fail immediately with a message.
                    done = true;
                    error = err;
                });

            // Flush any async events.
            $rootScope.$apply();

            waitsFor(function () {
                return done;

            }, 5000); // wait up to 5 secs (this is the default)

            // Any logic that occurs after a waitsFor must be placed in a function and registered using "runs".
            // Note - logic before the waitsFor *may* be put in a "runs" function.
            runs(function () {

                if (error) {
                    // this is ugly - haven't decided on a way to best handle this
                    var msg = error.ExceptionMessage || error.status || error.toString();
                    console.log('%s - ERROR', this.description, error, msg);
                    expect('the http request failed with error ' + msg).toBeFalsy();

                } else {
                    console.log('getEntity returned: ', data);
                    expect(data).toBeTruthy();
                    expect(data).toBeEntity(); // this is from our TestSupport.matchers... see earlier
                }
            });
        });

        it('the spEntityService can be used to create a new entity type (definition)', function () {

            var done1, done2, done3, error, entity,
                entities = [
                    spEntity.createEntityOfType('definition').setName('AAATestType'),
                    spEntity.createEntityOfType('stringField').setName('AAAStringField'),
                    spEntity.createEntityOfType('stringField').setName('AAAStringField2')
                ];

            // note - using "runs" before the "wait" or "waitFor" calls is optional (I believe)

            runs(function () {
                entities[0].relationship('fieldIsOnType', [entities[1], entities[2]]);

                spEntityService.putEntity(entities[0])
                    .then(function (id) {
                        console.log('PUT success', id);
                        return spEntityService.getEntity(id, 'name,fieldIsOnType.name');
                    })
                    .then(function (results) {
                        console.log('GET success', results);
                        done1 = true;
                        entity = results;

                    }, function (err) {
                        done1 = true;
                        error = err;
                    });

                // When using "runs" then ensure the call to $apply is inside.
                $rootScope.$apply();
            });

            waitsFor(function () {
                return done1;
            }, 10000);

            runs(function () {

                var msg;

                if (error) {
                    msg = error.ExceptionMessage || error.status || error.toString();
                    console.log('%s - ERROR', this.description, error, msg);
                    expect('the http request failed with error ' + msg).toBeFalsy();

                } else {
                    console.log('GET entity', entity);
                    expect(entity).toBeEntity();
                    expect(entity.name).toBe('AAATestType');
                    expect(entity.relationship('fieldIsOnType').length).toBe(2);                    
                }

                done2 = true;
                $rootScope.$apply();
            });

            waitsFor(function () {
                return done2;
            }, 10000);

            runs(function () {
                var eId = entity._id._id;

                spEntityService.deleteEntity(eId).then(function () {
                    console.log('DELETED');
                    done3 = true;
                }, function (err) {
                    console.log(err);
                    done3 = true;
                });

                // When using "runs" then ensure the call to $apply is inside.
                $rootScope.$apply();
            });

            waitsFor(function () {
                return done3;
            }, 10000);
        });

        it('direct request via $http angular service to our webapi', function () {
            var done, error, data;

            runs(function () {

                $http({
                    method: 'GET',
                    url: spWebService.getWebApiRoot() + '/spapi/data/v1/entity/test/steveGibbon',
                    params: { request: 'alias,name' },
                    headers: spWebService.getHeaders()

                }).then(function (results) {
                        data = results.data;
                        done = true;

                    }, function (err) {
                        error = err;
                        done = true;
                    });

                $rootScope.$apply();
            });

            waitsFor(function () {
                return done;
            });

            runs(function () {
                var msg, entities, entity;

                if (error) {
                    msg = error.ExceptionMessage || error.status || error.toString();
                    console.log('%s - ERROR', this.description, error, msg);
                    console.log('', JSON.stringify(error, null, ' ' ));
                    expect('the http request failed with error ' + msg).toBeFalsy();

                } else {
                    expect(data).toBeEntityData();

                    entities = spEntity.entityDataToEntities(data);
                    expect(entities).toBeTruthy();
                    expect(entities.length).toBeGreaterThan(0);

                    entity = entities[0];
                    expect(entity).toBeEntity();
                    expect(entity.name).toContain('ibbon');
                }
            });
        });

    });

    describe('sample tests using new intg test preamble', function () {

        beforeEach(module('mod.common.spEntityService'));
        beforeEach(module('sp.common.loginService'));


        beforeEach(inject(function ($injector) {
            // The following does all your typically need to use the angular mocks with passthrough enabled.
            // If you wand to any or all of the http then pass false as the last arg and configure $httpBackend
            // as per angular doc on the topic.
            TestSupport.setupIntgTests(this, $injector);
        }));

        it('the spEntityService can get a known entity (oldshared:employee)', inject(function ($rootScope, spEntityService) {

            var done, error, data;

            // Note - don't do "expect"s in promise (then) functions... they'll be quietly ignored
            // Do them in the "runs" function later

            spEntityService.getEntity('oldshared:employee', 'name')
                .then(function (result) {
                    done = true;
                    data = result;

                }, function (err) {
                    // All async calls need to catch the failed case and set the flag that we will "waitFor"
                    // otherwise when errors occur things just time out rather than fail immediately with a message.
                    done = true;
                    error = err;
                });

            // Flush any async events.
            $rootScope.$apply();

            waitsFor(function () {
                return done;

            }, 5000); // wait up to 5 secs (this is the default)

            // Any logic that occurs after a waitsFor must be placed in a function and registered using "runs".
            // Note - logic before the waitsFor *may* be put in a "runs" function.
            runs(function () {

                if (error) {
                    // this is ugly - haven't decided on a way to best handle this
                    var msg = error.ExceptionMessage || error.status || error.toString();
                    console.log('%s - ERROR', this.description, error, msg);
                    expect('the http request failed with error ' + msg).toBeFalsy();

                } else {
                    console.log('getEntity returned: ', data);
                    expect(data).toBeTruthy();
                    expect(data).toBeEntity(); // this is from our TestSupport.matchers... see earlier
                }
            });
        }));

        it('the spEntityService can be used to create a new entity type (definition)', inject(function ($rootScope, spEntityService) {

            var done1, done2, done3, error, entity,
                entities = [
                    spEntity.createEntityOfType('definition').setName('AAATestType'),
                    spEntity.createEntityOfType('stringField').setName('AAAStringField'),
                    spEntity.createEntityOfType('stringField').setName('AAAStringField2')
                ];

            // note - using "runs" before the "wait" or "waitFor" calls is optional (I believe)

            runs(function () {
                entities[0].relationship('fieldIsOnType', [entities[1], entities[2]]);

                spEntityService.putEntity(entities[0])
                    .then(function (id) {
                        console.log('PUT success', id);
                        return spEntityService.getEntity(id, 'name,fieldIsOnType.name');
                    })
                    .then(function (results) {
                        console.log('GET success', results);
                        done1 = true;
                        entity = results;

                    }, function (err) {
                        done1 = true;
                        error = err;
                    });

                // When using "runs" then ensure the call to $apply is inside.
                $rootScope.$apply();
            });

            waitsFor(function () {
                return done1;
            }, 10000);

            runs(function () {

                var msg;

                if (error) {
                    msg = error.ExceptionMessage || error.status || error.toString();
                    console.log('%s - ERROR', this.description, error, msg);
                    expect('the http request failed with error ' + msg).toBeFalsy();

                } else {
                    console.log('GET entity', entity);
                    expect(entity).toBeEntity();
                    expect(entity.name).toBe('AAATestType');
                    expect(entity.relationship('fieldIsOnType').length).toBe(2);
                }

                done2 = true;

                $rootScope.$apply();

            });

            waitsFor(function () {
                return done2;
            }, 10000);

            runs(function () {
                var eId = entity._id._id;

                spEntityService.deleteEntity(eId).then(function () {
                    console.log('DELETED');
                    done3 = true;
                }, function (err) {
                    console.log(err);
                    done3 = true;
                });

                // When using "runs" then ensure the call to $apply is inside.
                $rootScope.$apply();
            });

            waitsFor(function () {
                return done3;
            }, 10000);
        }));

        it('direct request via $http angular service to our webapi', inject(function ($rootScope, $http, spWebService) {
            var done, error, data;

            runs(function () {

                $http({
                    method: 'GET',
                    url: spWebService.getWebApiRoot() + '/spapi/data/v1/entity/test/steveGibbon',
                    params: { request: 'alias,name' },
                    headers: spWebService.getHeaders()

                }).then(function (results) {
                        data = results.data;
                        done = true;

                    }, function (err) {
                        error = err;
                        done = true;
                    });

                $rootScope.$apply();
            });

            waitsFor(function () {
                return done;
            });

            runs(function () {
                var msg, entities, entity;

                if (error) {
                    msg = error.ExceptionMessage || error.status || error.toString();
                    console.log('%s - ERROR', this.description, error, msg);
                    console.log('', JSON.stringify(error, null, ' ' ));
                    expect('the http request failed with error ' + msg).toBeFalsy();

                } else {
                    expect(data).toBeEntityData();

                    entities = spEntity.entityDataToEntities(data);
                    expect(entities).toBeTruthy();
                    expect(entities.length).toBeGreaterThan(0);

                    entity = entities[0];
                    expect(entity).toBeEntity();
                    expect(entity.name).toContain('ibbon');
                }
            });
        }));

    });

});
