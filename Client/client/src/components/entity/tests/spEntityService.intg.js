// Copyright 2011-2016 Global Software Innovation Pty Ltd
/* globals spEntity, jsonString, jsonInt, jsonLookup, jsonRelationship, jsonDecimal, jsonBool,
 jsonCurrency, jsonDate, jsonDateTime, jsonTime */

describe('Entity Model|spEntityService|intg:', function () {
    'use strict';

    var $injector, $http, $rootScope, $q,
        spWebService, webApiRoot, spEntityService, headers,
        testEntityIds = [], testEntityId, testEntity;

    beforeEach(module('sp.common.loginService'));
    beforeEach(module('mod.common.spEntityService'));

    beforeEach(inject(function ($injector) {
        TestSupport.setupIntgTests(this, $injector);
    }));

    beforeEach(inject(function ($injector) {

        $http = $injector.get('$http');
        $rootScope = $injector.get('$rootScope');
        $q = $injector.get('$q');
        spWebService = $injector.get('spWebService');
        spEntityService = $injector.get('spEntityService');

        this.addMatchers(TestSupport.matchers);

        TestSupport.configureWebApi($injector);

        console.log('window.location', window.location);

        webApiRoot = spWebService.getWebApiRoot();
        headers = spWebService.getHeaders();

    }));

    describe('type reporting service', function () {

        it('should get entities for a known type', function () {
            
            var result1 = {};
            TestSupport.waitCheckReturn($rootScope,
                spEntityService.getEntitiesOfType('core:chart'),
                result1);

            runs(function () {
                var entities = result1.value;
                expect(entities).toBeTruthy();
                expect(entities.length).toBeGreaterThan(0);
                var item = entities[0];
                expect(item.getField('name')).toBeTruthy();
            });
        });

        it('should get entities for a known type, requesting additional fields', function () {
            TestSupport.setWaitTimeout(20000);
            
            var getEntities = spEntityService.getEntitiesOfType('core:activityType', 'alias,name,description');

            var result1 = {};
            TestSupport.wait(getEntities.then(function(entities) {
                expect(entities).toBeTruthy();
                expect(entities.length).toBeGreaterThan(0);
                var item = entities[0];
                expect(item.getName()).toBeTruthy();
            }));
        });

        it('should get instances for a known type', function () {
            var done = false,
                data;

            var fetch = spEntityService.getInstancesOfType('core:chart')
                .then(function (results) {
                    //console.log('then ok', results);
                    done = true;
                    data = results;
                }, function (error) {
                    console.log('then rejected', error);
                    done = true;
                    expect('the request failed with error ' + error).toBeFalsy();
                });

            TestSupport.wait(fetch.then(function () {
                expect(data).toBeTruthy();
                console.log('results are ', data);

                expect(data.length).toBeGreaterThan(0);
                var item = data[0];
                expect(item.name).toBeTruthy();
                expect(item.entity).toBeTruthy();
            }));
        });

        it('should get instances for a known type, requesting additional fields', function () {
            var done = false,
                data;

            var fetch = spEntityService.getInstancesOfType('core:folder', 'instancesOfType.{alias,name,description}')
                .then(function (results) {
                    //console.log('then ok', results);
                    done = true;
                    data = results;
                }, function (error) {
                    console.log('then rejected', error);
                    done = true;
                });

            TestSupport.wait(fetch.then(function () {
                expect(data).toBeTruthy();
                expect(data.length).toBeGreaterThan(0);
                var item = data[0];
                expect(item.name).toBeTruthy();
                expect(item.entity).toBeTruthy();
            }));
        });
    });

    describe('simultaneous requests support in entity service', function () {

        it('should see one request complete in timely manner', function () {

            var doneCount = 0, errCount = 0, requestCount = 1;

            spEntityService.getInstancesOfType('core:type').then(function () { doneCount++; }, function () { errCount++; });

            $rootScope.$apply();

            waitsFor(function () {
                return doneCount + errCount >= requestCount;
            }, 60000);

            runs(function () {
                expect(doneCount + errCount).toBe(requestCount);
                expect(doneCount).toBe(requestCount);
            });
        });

        it('should see two requests complete in timely manner', function () {

            var doneCount = 0, errCount = 0, requestCount = 2;

            spEntityService.getInstancesOfType('core:definition').then(function () { doneCount++; }, function () { errCount++; });
            spEntityService.getInstancesOfType('core:chart').then(function () { doneCount++; }, function () { errCount++; });

            $rootScope.$apply();

            waitsFor(function () {
                return doneCount + errCount >= requestCount;
            }, 60000);

            runs(function () {
                expect(doneCount + errCount).toBe(requestCount);
                expect(doneCount).toBe(requestCount);
            });
        });

        xit('should see three requests complete in timely manner', function () {

            var doneCount = 0, errCount = 0, requestCount = 3;

            spEntityService.getInstancesOfType('core:activityType').then(function () { doneCount++; }, function () { errCount++; });
            spEntityService.getInstancesOfType('core:chart').then(function () { doneCount++; }, function () { errCount++; });
            spEntityService.getInstancesOfType('core:workflow').then(function () { doneCount++; }, function () { errCount++; });

            $rootScope.$apply();

            waitsFor(function () {
                return doneCount + errCount >= requestCount;
            }, 60000);

            runs(function () {
                expect(doneCount + errCount).toBe(requestCount);
                expect(doneCount).toBe(requestCount);
            });
        });
    });

    describe('the entity info service GET', function () {

        it('will see a newly created entity instance appear in an instanceOfType request', function () {

            // 1. get a list of some type
            // 2. create a new entity of that type
            // 3. re-get the list and check the new entity has been added

            var done, error, originalList, newList, entity, newEntityId;

            spEntityService.getEntity('workflow', 'instancesOfType.id')
                .then(function (e) {
                    originalList = e.relationship('instancesOfType');

                    entity = spEntity.createEntityOfType('core:workflow').
                        setName(TestSupport.getTestEntityName('Workflow', 'tbd'));
                    return spEntityService.putEntity(entity);
                })
                .then(function (id) {
                    newEntityId = parseInt(id, 10);
                    testEntityIds.push(newEntityId);

                    return spEntityService.getEntity('workflow', 'instancesOfType.id');
                })
                .then(function (e) {
                    newList = e.getInstancesOfType();
                    done = true;
                }, function (e) {
                    done = true;
                    console.log('ERROR', e);
                    error = e;
                });

            $rootScope.$apply();

            waitsFor(function () {
                return done;
            }, 20000);

            runs(function () {
                expect(error).toBeFalsy();
                expect(originalList).toBeTruthy();
                expect(newList).toBeTruthy();
                expect(newList.length).toBe(originalList.length + 1);
                expect(originalList.map(function (item) { return item.id(); })).not.toContain(newEntityId);
                expect(newList.map(function (item) { return item.id(); })).toContain(newEntityId);
            });
        });
    });

    describe('when call get', function () {
        it('should return a list of workflow instances', function () {
            var asyncDone = false;
            var data;
            spEntityService._getEntityData('core:workflow', 'alias,name,instancesOfType.{alias,name}')
                .then(function (result) {
                    data = result;
                    asyncDone = true;
                }, function (err) {
                    asyncDone = true;
                });
            $rootScope.$apply();
            waitsFor(function () {
                return asyncDone;
            });
            runs(function () {
                var entities = spEntity.entityDataToEntities(data);
                expect(entities).toBeTruthy();
                expect(entities.length).toBeGreaterThan(0);

                var entity = entities[0];
                expect(entity).toBeDefined();
                expect(entity.field).toBeDefined();

                var name = entity.field('name');
                expect(name).toBeDefined();
                expect(name).toBe('Workflow');

                var rel = entity.relationship('instancesOfType');
                expect(rel).toBeTruthy();
                rel.forEach(function (e) {
                    expect(e).toBeDefined();
                    expect(e.field).toBeDefined();
                    console.log('workflow', e.id(), e.field('name'), e);
                    if (e.field('name') && e.field('name').match(/^Unit Tests:/)) {
                        testEntityIds.push(e.id());
                    }
                });
            });
        });
        it('should return a list of all fields', function () {
            var asyncDone = false;
            var data;
            spEntityService._getEntityData('core:stringField', 'alias,name,instancesOfType.{alias,name}')
                .then(function (result) {
                    data = result;
                    asyncDone = true;
                }, function (err) {
                    asyncDone = true;
                });
            $rootScope.$apply();
            waitsFor(function () {
                return asyncDone;
            });
            runs(function () {
                var entities = spEntity.entityDataToEntities(data);
                var entity = entities[0];

                expect(entity.field('name')).toBe('String Field');

                var rel = entity.relationship('instancesOfType');
                rel.forEach(function (e) {
                    //console.log(e.id(), e.field('name'));
                });
            });
        });
    });

    describe('a sequence on workflow entities', function () {  
        it('call putEntity to create an entity', function () {

            var workflowEntity = spEntity.fromJSON({
                typeId:'core:workflow',
                name:'Unit Tests: Workflow 1'
            });

            TestSupport.waitCheckReturn($rootScope,
                spEntityService.putEntity(workflowEntity)
                    .then(function (id) {
                        testEntityId = id;
                    }),
                null);
        });
        it('call getEntity to retrieve the entity', function () {  

            var id = testEntityId;

            var result = {};
            TestSupport.waitCheckReturn($rootScope,
                spEntityService.getEntity(id, 'name,exitPoints.alias'),
                result);

            runs(function () {
                expect(result.value).toBeDefined();
                expect(result.value.name).toBe('Unit Tests: Workflow 1');
                console.log('GET ok');
            });
        });
        it('call deleteEntity to delete the entity', function () {

            var id = testEntityId;
            TestSupport.waitCheckReturn($rootScope, spEntityService.deleteEntity(testEntityId), null);

        });
        it('call getEntity to verify it is gone', function () {

            var id = testEntityId;

            var asyncDone = false, asyncError = false, abort = false;
            var data;

            runs(function () {
                console.log('NOTICE - expecting a failed GET for id', id);
                spEntityService._getEntityData(id, 'name')
                    .then(function (result) {
                        data = result;
                        asyncDone = true;
                    }, function (err) {
                        asyncError = true;
                    });
                $rootScope.$apply();
            });
            waitsFor(function () {
                return abort || asyncDone || asyncError;
            });
            runs(function () {
                if (asyncError) {
                    console.log('GET failed as expected');
                    abort = true;
                    return;
                }
                if (abort) return;
                expect('GET returned ok - NOT expected').toBeFalsy();
            });
        });
    });

    describe('using the entity services with the Employee and related types', function () {
        var employeeIds = [],
            age,
            newEmployeeId;

        it('it should get a list of the employee instances', function () {

            var instances, asyncDone = false, asyncError = false;

            spEntityService.getEntity('oldshared:employee', 'alias,name,instancesOfType.{alias,name}').then(function (entity) {
                asyncDone = true;

                expect(entity).toBeEntity();
                instances = entity.relationship('core:instancesOfType');
                expect(instances).toBeTruthy();

            }, function (error) {
                asyncError = true;
                expect(error.data.ExceptionMessage || error.data.Message || error).toBeFalsy();
            });

            $rootScope.$apply();

            waitsFor(function () {
                return asyncDone || asyncError;
            });

            runs(function () {
                if (!instances) return;

                instances.forEach(function (e) {
                    console.log(e.id(), e.alias(), e.field('name'));
                    employeeIds.push(e.id());

                    if (e.field('name') && e.field('name').match(/^Unit Tests:/)) {
                        testEntityIds.push(e.id());
                    }
                });
            });
        });

        it('it should get an known employee instance', function () {

            var asyncDone = false, asyncError = false;

            spEntityService.getEntity(employeeIds[0], 'alias,name,*').then(function (entity) {
                asyncDone = true;

                age = entity.field('oldshared:age');
                console.log('name', entity.getName());
                console.log('email', entity.field('oldshared:email'));
                console.log('age', entity.field('oldshared:age'));
                console.log('startDate', entity.field('oldshared:employeeStartDate'));

            }, function (error) {
                asyncError = true;
                expect(error.data.ExceptionMessage || error.data.Message || error).toBeFalsy();
            });

            $rootScope.$apply();
            waitsFor(function () {
                return asyncDone || asyncError;
            });
        });

        it('it should update an employee\'s field', function () {
            var asyncDone = false, asyncError = false,
                entity = spEntity.fromId(employeeIds[0]);
            entity.registerField('oldshared:age', spEntity.DataType.Int32);
            entity.setAge(age + 1);

            spEntityService.putEntity(entity).then(function (id) {
                asyncDone = true;
                expect(+id).toBe(employeeIds[0]);
            }, function (error) {
                asyncError = true;
                expect(error.data.ExceptionMessage || error.data.Message || error).toBeFalsy();
            });

            $rootScope.$apply();
            waitsFor(function () {
                return asyncDone || asyncError;
            });
        });

        it('it should get that employee and check the updated field was saved', function () {
            var asyncDone = false, asyncError = false;
            spEntityService.getEntity(employeeIds[0], 'alias,name,*').then(function (entity) {
                asyncDone = true;
                expect(entity.getAge()).toBe(age + 1);
            }, function (error) {
                asyncError = true;
                expect(error.data.ExceptionMessage || error.data.Message || error).toBeFalsy();
            });

            $rootScope.$apply();
            waitsFor(function () {
                return asyncDone || asyncError;
            });
        });
        it('it should create and save a new employee', function () {
            var asyncDone = false, asyncError = false,
                entity = spEntity.createEntityOfType('oldshared:employee');
            entity.registerField('oldshared:firstName', spEntity.DataType.Int32);
            entity.registerField('oldshared:age', spEntity.DataType.Int32);
            entity
                .setName('Joel Gibbon')
                .setFirstName('Joel')
                .setAge(age = 6);

            spEntityService.putEntity(entity).then(function (id) {
                asyncDone = true;
                expect(+id).toBeGreaterThan(0);

                newEmployeeId = +id;
                testEntityIds.push(id);

            }, function (error) {
                asyncError = true;
                expect(error.data.ExceptionMessage || error.data.Message || error).toBeFalsy();
            });

            $rootScope.$apply();
            waitsFor(function () {
                return asyncDone || asyncError;
            });
        });
        it('it should get that employee and verify the field', function () {
            var asyncDone = false, asyncError = false;

            spEntityService.getEntity(newEmployeeId, 'alias,name,*').then(function (entity) {
                asyncDone = true;
                expect(entity.field('oldshared:age')).toBe(age);

            }, function (error) {
                asyncError = true;
                expect(error.data.ExceptionMessage || error.data.Message || error).toBeFalsy();
            });

            $rootScope.$apply();

            waitsFor(function () {
                return asyncDone || asyncError;
            });
        });

        // Disabled due to changes with shared
        xit('it should update that employee with a new related location', function () {
            // hmmm I think I was testing for something specific here...., but it looks like
            // I was trying to add a related entity to an existing known entity...

            var asyncDone = false, asyncError = false;

            var address = spEntity.createEntityOfType('oldshared:location');
            address.registerField('oldshared:addressLine1', spEntity.DataType.String);
            address.registerField('oldshared:city', spEntity.DataType.String);
            address
                .setName('home address')
                .setAddressLine1('129 Artarmon Rd')
                .setCity('Artarmon');

            var entity = spEntity.fromId(newEmployeeId);
            entity.relationship('oldshared:personHasAddress', address);

            spEntityService.putEntity(entity).then(function (id) {
                asyncDone = true;
                expect(+id).toBe(+newEmployeeId);
            }, function (error) {
                asyncError = true;
                expect(error.data.ExceptionMessage || error.data.Message || error).toBeFalsy();
            });

            $rootScope.$apply();

            waitsFor(function () {
                return asyncDone || asyncError;
            });
        });
    });

    // this one is just here to clean up. probably not good in unit test theory, but...
    describe('when call delete on all our test entities', function () {
        it('it should delete all of them', function () {

            testEntityIds = _.uniq(testEntityIds); // in case some have been added multiple times
            var todelete = testEntityIds.slice(0, -10); // all but last few as we are inspecting them post test run

            todelete.forEach(function (id) {
                console.log('deleting', id);
            });

            spEntityService.deleteEntities(todelete);
        });
    });

    describe('getEntityByNameAndTypeName', function () {
        it('should load an entity', function () {

            var result = {};
            var entity;

            TestSupport.waitCheckReturn($rootScope,
                spEntityService.getEntityByNameAndTypeName('Pizza','Definition', 'name'),
                result);

            runs(function () {
                entity = result.value;
                expect(entity).toBeEntity();
                expect(entity.getName()).toBe('Pizza');
            });
        });
        it('should handle wildcard fields', function () {

            var result = {};
            var entity;

            TestSupport.waitCheckReturn($rootScope,
                spEntityService.getEntityByNameAndTypeName('Pizza', 'Definition', 'name,instancesOfType.*'),
                result);

            runs(function () {
                entity = result.value;
                expect(entity).toBeEntity();
                expect(entity.getName()).toBe('Pizza');
                expect(entity.getInstancesOfType()).toBeArray();
            });
        });
    });

    describe('using putEntity', function () {
        it('a field can be set to null', function () {

            var e0 = spEntity.fromJSON({
                typeId: 'test:drink',
                name: 'test1',
                description: 'desc1'
            });

            var results = {};

            TestSupport.waitCheckReturn($rootScope,
                spEntityService.putEntity(e0)
                .then(function (id) {
                    results.id = id;
                    return spEntityService.getEntity(id, 'name, description');
                })
                .then(function (e1) {
                    results.name1 = e1.getName();
                    results.desc1 = e1.getDescription();
                    e1.setDescription(null);
                    return spEntityService.putEntity(e1);
                })
                .then(function () {
                    return spEntityService.getEntity(results.id, 'name, description');
                })
                .then(function (e2) {
                    results.name2 = e2.getName();
                    results.desc2 = e2.getDescription();
                }), null);


            runs(function () {
                expect(results.id).toBeGreaterThan(0);
                expect(results.name1).toBe('test1');
                expect(results.desc1).toBe('desc1');
                expect(results.name2).toBe('test1');
                expect(results.desc2).toBeNull();
            });
        });


        it('getting a reverse relationship by ID is flagged with isReverse', function () {

            var results = {};
            
            TestSupport.waitCheckReturn($rootScope,
                spEntityService.getEntity('test:af02', 'test:drinks.id')
                .then(function (af) {
                    results.res1 = af;
                    results.allFields = af;
                    results.drink = af.getDrinks(); //one-to-one
                    var rc = af.getRelationshipContainer('test:drinks');
                    results.relId = rc.id.getId();
                    results.drinkId = results.drink.id();
                    var query = '-#' + results.relId + '.id';
                    return spEntityService.getEntity(results.drinkId, query);
                })
                .then(function (drink) {
                    results.res2 = drink;
                }), null);


            runs(function () {
                expect(results.res2.id()).toBe(results.drinkId);
                var rc = results.res2.getRelationshipContainer({ id: results.relId, isReverse: true });
                expect(rc.isReverse).toBe(true);
                var af = results.res2.getLookup({ id: results.relId, isReverse: true });
                var afId = af.eid().getId();
                expect(afId).toBe(results.allFields.id());
            });
        });

    });
    
    describe('getEntitiesOfType filtering', function () {

        it('should get entities for a known type filtered', function () {

            TestSupport.wait(
                spEntityService.getEntitiesOfType('core:report', 'name', { filter: '[Name]=\'Inboxes\'' })
                .then(function (entities) {
                    expect(entities).toBeTruthy();
                    expect(entities.length).toBe(1);
					if (entities.length > 1) {
						var item = entities[0];
						expect(item.getField('name')).toBe('Inboxes');
					}
                }));
        });

        it('should get reports filtered to a specific solution', function () {

            TestSupport.wait(
                spEntityService.getEntity('core:testSolution', 'id')
                .then(function (e) {
                    return spEntityService.getEntitiesOfType('core:report', 'name', { filter: 'id([Resource in application])=' + e.idP });
                })
                .then(function (reports) {
                    expect(reports).toBeTruthy();
                    expect(reports.length).toBeGreaterThan(0);
                }));
        });
    });

});
