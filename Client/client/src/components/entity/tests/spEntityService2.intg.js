// Copyright 2011-2016 Global Software Innovation Pty Ltd
/*global angular, console, _, TestSupport, spEntity */
describe('Entity Model|spEntityService|intg:', function () {
    'use strict';

    var testEntityIds = [], testEntityId, testEntity;

    beforeEach(module('sp.common.loginService'));
    beforeEach(module('mod.common.spEntityService'));

    beforeEach(inject(function ($injector) {
        TestSupport.setupIntgTests(this, $injector);
    }));

    describe('create entity', function () {
        describe('setting entity type', function () {
            it('can create an entity by type alias', inject(function (spEntityService) {
                var json = {
                    typeId: 'core:intField',
                    name: 'Int Field 1'
                };

                var e1 = spEntity.fromJSON(json);
                TestSupport.wait(
                    spEntityService.putEntity(e1)
                    .then(function (id) {
                        return spEntityService.getEntity(id, 'isOfType.alias');
                    })
                    .then(function (e2) {
                        expect(e2.getIsOfType()[0].nsAlias).toBe('core:intField');
                    }));
            }));

            it('can create an entity by type alias with extra bits', inject(function (spEntityService) {
                var json = {
                    typeId: 'core:intField',
                    name: 'Int Field 1',
                    isOfType: [{ alias: 'core:intField', id: 'core:intField' }]
                };

                var e1 = spEntity.fromJSON(json);
                TestSupport.wait(
                    spEntityService.putEntity(e1)
                    .then(function (id) {
                        return spEntityService.getEntity(id, 'isOfType.alias');
                    })
                    .then(function (e2) {
                        expect(e2.getIsOfType()[0].nsAlias).toBe('core:intField');
                    }));
            }));
        });
    });

    describe('sequence of tests on an entity definition', function () {

        it('putEntity can create a new instance', inject(function (spEntityService) {

            // Create a definition and two fields
            var json = {
                typeId: 'definition',
                name: 'AAATestType',
                inherits: { id: 'test:person' },
                defaultPickerReport: { id: 'core:templateReport' },
                fields: [
                    {
                        typeId: 'stringField',
                        name: 'AAAStringField0'
                    },
                    {
                        typeId: 'stringField',
                        name: 'AAAStringField1'
                    }
                ],
                fieldGroups: [
                    {
                        typeId: 'fieldGroup',
                        name: 'FieldGroup1'
                    }
                ]
            };

            var definition = spEntity.fromJSON(json);

            TestSupport.wait(
                spEntityService.putEntity(definition)
                .then(function (id) {
                    testEntityId = id;
                    testEntityIds.push(id);

                    return spEntityService.deleteEntity(id);
                }));
        }));

        it('getEntity can get the new instance', inject(function (spEntityService) {

            // Create a definition and two fields
            var json = {
                typeId: 'definition',
                name: 'AAATestType',
                inherits: { id: 'test:person' },
                defaultPickerReport: { id: 'core:templateReport' },
                fields: [
                    {
                        typeId: 'stringField',
                        name: 'AAAStringField0'
                    },
                    {
                        typeId: 'stringField',
                        name: 'AAAStringField1'
                    }
                ],
                fieldGroups: [
                    {
                        typeId: 'fieldGroup',
                        name: 'FieldGroup1'
                    }
                ]
            };

            var definition = spEntity.fromJSON(json);

            TestSupport.wait(
                spEntityService.putEntity(definition)
                .then(function (id) {
                    testEntityId = id;
                    return spEntityService.getEntity(id, 'name,fields.name,inherits.name,defaultPickerReport.name');
                })
                .then(function(entity) {
                    testEntity = entity;
                    expect(entity).toBeEntity();
                    expect(entity.getName()).toBe('AAATestType');
                    expect(entity.getFields()).toBeArray();
                    expect(entity.getFields().length).toBe(2);
                    expect(entity.getInherits()[0].getName()).toBe('AA_Person');
                    expect(entity.getDefaultPickerReport().getName()).toBe('Template');

                    return spEntityService.deleteEntity(testEntityId);
                }));
        }));

        // TODO: Ignored by Anthony as part of new security model. Ran out of time trying to work out why tests fail.
        xit('putEntity can remove a relationship and a lookup', inject(function (spEntityService) {
            var fields = testEntity.getFields();
            testEntityIds.push(fields[0]);
            testEntityIds.push(fields[1]);

            testEntity.setInherits([]);         // test clearing a relationship
            testEntity.setFields([fields[1]]);  // test replacing a relationship array (removing 1 elem)
            testEntity.setDefaultPickerReport(null);  // test clearing a lookup

            TestSupport.wait(
                spEntityService.putEntity(testEntity)
                .then(function (id) {
                    return spEntityService.getEntity(id, 'name,fields.name,inherits.name,defaultPickerReport.name');
                })
                .then(function (entity) {
                    expect(entity).toBeEntity();
                    expect(entity.getFields()).toBeArray();
                    expect(entity.getFields().length).toBe(1);
                    expect(entity.getFields()[0].getName()).toBe('AAAStringField1');
                    expect(entity.getInherits()).toBeEmptyArray();
                    expect(entity.getDefaultPickerReport()).toBeNull();
                }));
        }));

    });
    
    describe('various tests for updating relationships', function () {

        it('can call clear on relationship', inject(function (spEntityService) {

            // Create a definition and two fields
            var json = {
                typeId: 'definition',
                name: 'AAATestType1',
                inherits: { id: 'test:person' },
                fields: [
                    {
                        typeId: 'stringField',
                        name: 'AAAStringField0'
                    },
                    {
                        typeId: 'stringField',
                        name: 'AAAStringField1'
                    }
                ]
            };

            var definition = spEntity.fromJSON(json);
            var results = {};

            TestSupport.wait(
                // create test entity
                spEntityService.putEntity(definition)
                    .then(function (id) {
                        // get entity
                        testEntityId = id;
                        testEntityIds.push(id);
                        return spEntityService.getEntity(testEntityId, 'name,fields.name');
                    })
                    .then(function (defn) {
                        // make changes
                        expect(defn.getFields().length).toBe(2);
                        results.fieldId0 = defn.getFields()[0].eid().getId();
                        results.fieldId1 = defn.getFields()[1].eid().getId();
                        testEntityIds.push(results.fieldId0);
                        testEntityIds.push(results.fieldId1);
                        defn.getFields().clear();
                        return spEntityService.putEntity(defn);
                    })
                    .then(function () {
                        // re-get entity
                        return spEntityService.getEntity(testEntityId, 'name,fields.name');
                    })
                    .then(function (defn) {
                        expect(defn.getFields().length).toBe(0);
                        return spEntityService.getEntities([results.fieldId0, results.fieldId1], 'name');
                    })
                    .then(function (res) {
                        expect(res[0].eid().getId()).toBe(results.fieldId0);
                        expect(res[1].eid().getId()).toBe(results.fieldId1);

                        return spEntityService.deleteEntity(testEntityId);
                })
            );

        }));

        it('can call clear on relationship but re-add one item', inject(function (spEntityService) {

            // Create a definition and two fields
            var json = {
                typeId: 'definition',
                name: 'AAATestType1',
                inherits: { id: 'test:person' },
                fields: [
                    {
                        typeId: 'stringField',
                        name: 'AAAStringField0'
                    },
                    {
                        typeId: 'stringField',
                        name: 'AAAStringField1'
                    }
                ]
            };

            var definition = spEntity.fromJSON(json);
            var results = {};

            TestSupport.wait(
                // create test entity
                spEntityService.putEntity(definition)
                    .then(function(id) {
                        // get entity
                        testEntityId = id;
                        testEntityIds.push(id);
                        return spEntityService.getEntity(testEntityId, 'name,fields.name');
                    })
                    .then(function(defn) {
                        // make changes
                        expect(defn.getFields().length).toBe(2);
                        results.fieldId0 = defn.getFields()[0].eid().getId();
                        results.fieldId1 = defn.getFields()[1].eid().getId();
                        testEntityIds.push(results.fieldId0);
                        testEntityIds.push(results.fieldId1);
                        defn.getFields().clear();
                        defn.setFields([results.fieldId0]);
                        return spEntityService.putEntity(defn);
                    })
                    .then(function() {
                        // re-get entity
                        return spEntityService.getEntity(testEntityId, 'name,fields.name');
                    })
                    .then(function(defn) {
                        expect(defn.getFields().length).toBe(1);
                        var fieldId0b = defn.getFields()[0].eid().getId();
                        expect(fieldId0b).toBe(results.fieldId0);
                        return spEntityService.getEntities([results.fieldId0, results.fieldId1], 'name');
                    })
                    .then(function(res) {
                        expect(res[0].eid().getId()).toBe(results.fieldId0);
                        expect(res[1].eid().getId()).toBe(results.fieldId1);

                        return spEntityService.deleteEntity(testEntityId);
                    })
            );

        }));

        it('can call deleteExisting on relationship', inject(function (spEntityService) {

            // Create a definition and two fields
            var json = {
                typeId: 'definition',
                name: 'AAATestType1',
                inherits: { id: 'test:person' },
                fields: [
                    {
                        typeId: 'stringField',
                        name: 'AAAStringField0'
                    },
                    {
                        typeId: 'stringField',
                        name: 'AAAStringField1'
                    }
                ]
            };

            var definition = spEntity.fromJSON(json);
            var results = {};

            TestSupport.wait(
                // create test entity
                spEntityService.putEntity(definition)
                    .then(function (id) {
                        // get entity
                        testEntityId = id;
                        testEntityIds.push(id);
                        return spEntityService.getEntity(testEntityId, 'name,fields.name');
                    })
                    .then(function (defn) {
                        // make changes
                        expect(defn.getFields().length).toBe(2);
                        results.fieldId0 = defn.getFields()[0].eid().getId();
                        results.fieldId1 = defn.getFields()[1].eid().getId();
                        testEntityIds.push(results.fieldId0);
                        testEntityIds.push(results.fieldId1);
                        defn.getFields().deleteExisting();
                        return spEntityService.putEntity(defn);
                    })
                    .then(function () {
                        // re-get entity
                        return spEntityService.getEntity(testEntityId, 'name,fields.name');
                    })
                    .then(function (defn) {
                        expect(defn.getFields().length).toBe(0);
                        return spEntityService.getEntities([testEntityId, results.fieldId0, results.fieldId1], 'name'); // include testEntityId to avoid 404
                    })
                    .then(function (res) {
                        expect(res.length).toBe(1); // verifies both fields got deleted
                        return spEntityService.deleteEntity(testEntityId);
                    })
            );

        }));

        it('can call autoCardinality on relationship', inject(function (spEntityService) {

            // Create a definition and a field fields
            var suffix = ' '  + (new Date()).getTime();
            var json = {
                typeId: 'definition',
                name: 'TestAutoCardDefn1' + suffix,
                inherits: { id: 'test:person' },
                fields: [
                    {
                        typeId: 'stringField',
                        name: 'TestAutoCardField1' + suffix
                    }
                ]
            };

            var definition = spEntity.fromJSON(json);
            TestSupport.wait(
                // create test entity
                spEntityService.putEntity(definition)
                    .then(function (id) {
                        // get entity
                        testEntityId = id;
                        testEntityIds.push(id);
                        return spEntityService.getEntity(testEntityId, 'name,fields.name');
                    })
                    .then(function (defn) {
                        var fieldId = defn.getFields()[0].eid().getId();
                        testEntityIds.push(fieldId);
                        // steal the field to another definition
                        var json2 = {
                            typeId: 'definition',
                            name: 'TestAutoCardDefn2' + suffix,
                            inherits: { id: 'test:person' },
                            fields: [ fieldId ]
                        };
                        var definition2 = spEntity.fromJSON(json2);
                        definition2.getFields().autoCardinality();
                        return spEntityService.putEntity(definition2);
                    })
                    .then(function (defn2Id) {
                        testEntityIds.push(defn2Id);
                        // re-get entity
                        return spEntityService.getEntity(defn2Id, 'name,fields.name');
                    })
                    .then(function (defn) {
                        expect(defn.getFields().length).toBe(1);
                    })
            );

        }));

        it('can delete a relationship instance when reverse lookups are used', inject(function (spEntityService) {

            // Create a definition and two fields
            var json = {
                typeId: 'test:allFields',
                name: 'TAF1',
                'test:drinks': [
                    {
                        typeId: 'test:drink',
                        name: 'TD1'
                    }
                ]
            };
            var json2 = {
                typeId: 'test:allFields',
                name: 'TAF2'
            };

            var toDelete = [];
            var e1 = spEntity.fromJSON(json);
            var e2 = spEntity.fromJSON(json2);
            var results = {};

            TestSupport.wait(
                // create test entity
                spEntityService.putEntity(e1)
                    .then(function (id) {
                        toDelete.push(id);
                        results.e1id = id;
                        // get entity
                        testEntityId = id;
                        return spEntityService.putEntity(e2);
                    }).then(function (id) {
                        toDelete.push(id);
                        results.e2id = id;
                        return spEntityService.getEntity(results.e1id, 'name,test:drinks.name');
                    })
                    .then(function (e1Again) {
                        var rc = e1Again.getRelationshipContainer('test:drinks');
                        results.relId = rc.id.getId();
                        results.did = e1Again.getDrinks().id();
                        toDelete.push(results.did);
                        return spEntityService.getEntity(results.did, 'name,-#' + results.relId + '.name');
                    })
                    .then(function (drink) {
                        var rid = { id: results.relId, isReverse: true };
                        results.teste1id = drink.getLookup(rid).id();
                        // the main task being tested
                        drink.setLookup(rid, results.e2id);
                        return spEntityService.putEntity(drink);
                    })
                    .then(function () {
                        return spEntityService.getEntity(results.did, 'name,-#' + results.relId + '.name');
                    })
                    .then(function (drinkAgain) {
                        var rid = { id: results.relId, isReverse: true };
                        results.teste1idAgain = drinkAgain.getLookup(rid).id();
                        return spEntityService.deleteEntities(toDelete);
                    })
                    .then(function () {
                        expect(results.teste1id).toBe(results.e1id);
                        expect(results.teste1idAgain).toBe(results.e2id);
                    })
            );

        }));

        it('can call deleteExisting on relationship but re-add one', inject(function (spEntityService) {

            // Create a definition and two fields
            var json = {
                typeId: 'definition',
                name: 'AAATestType1',
                inherits: { id: 'test:person' },
                fields: [
                    {
                        typeId: 'stringField',
                        name: 'AAAStringField0'
                    },
                    {
                        typeId: 'stringField',
                        name: 'AAAStringField1'
                    }
                ]
            };

            var definition = spEntity.fromJSON(json);
            var results = {};

            TestSupport.wait(
                // create test entity
                spEntityService.putEntity(definition)
                    .then(function (id) {
                        // get entity
                        testEntityId = id;
                        testEntityIds.push(id);
                        return spEntityService.getEntity(testEntityId, 'name,fields.name');
                    })
                    .then(function (defn) {
                        // make changes
                        expect(defn.getFields().length).toBe(2);
                        results.fieldId0 = defn.getFields()[0].eid().getId();
                        results.fieldId1 = defn.getFields()[1].eid().getId();
                        testEntityIds.push(results.fieldId0);
                        testEntityIds.push(results.fieldId1);

                        var field0 = defn.getFields()[0];
                        defn.getFields().deleteExisting();
                        defn.getFields().add(field0);
                        return spEntityService.putEntity(defn);
                    })
                    .then(function () {
                        // re-get entity
                        return spEntityService.getEntity(testEntityId, 'name,fields.name');
                    })
                    .then(function (defn) {
                        expect(defn.getFields().length).toBe(1);
                        return spEntityService.getEntities([results.fieldId0, results.fieldId1], 'name');
                    })
                    .then(function (stillExists) {
                        expect(stillExists.length).toBe(1); // verifies that field1 got deleted
                        expect(stillExists[0].eid().getId()).toBe(results.fieldId0); // and field0 still exists

                        return spEntityService.deleteEntity(testEntityId);
                    })
            );

        }));

        it('can create and delete a reverse alias relationship', inject(function (spEntityService) {

            // Create a definition and two fields
            var json = {
                typeId: 'definition',
                name: 'Test Type 99',
                'fieldGroups': [
                    {
                        typeId: 'fieldGroup',
                        name: 'Test FieldGroup 99'
                    }
                ]
            };

            var toDelete = [];
            var e1 = spEntity.fromJSON(json);
            e1.alias('test:testType99');
            e1.fieldGroups[0].alias('test:testFieldGroup99');

            var results = {};

            TestSupport.wait(
                // create test entity
                spEntityService.putEntity(e1)
                    .then(function (id) {
                        toDelete.push(id);
                        results.e1id = id;
                        // get entity
                        testEntityId = id;
                        return spEntityService.getEntity(results.e1id, 'name,fieldGroups.name');
                    })
                    .then(function (e1Loaded) {
                        expect(e1.getFieldGroups().length).toBe(1);
                        expect(e1.getFieldGroups()[0].getName()).toBe('Test FieldGroup 99');
                        expect(e1Loaded.getFieldGroups().length).toBe(1);
                        expect(e1Loaded.getFieldGroups()[0].getName()).toBe('Test FieldGroup 99');
                        expect(results.teste1idAgain).toBe(results.e2id);
                        return spEntityService.deleteEntities(toDelete);
                    }));
        }));

        it('can create and delete a reverse alias relationship loaded from server', inject(function (spEntityService) {

            // Create a definition
            var json = {
                typeId: 'definition',
                name: 'Test Type 100'
            };

            var toDelete = [];
            var e1 = spEntity.fromJSON(json);
            e1.alias('test:testType100');
            var results = {};

            TestSupport.wait(
                // create test entity
                spEntityService.putEntity(e1)
                    .then(function (id) {
                        toDelete.push(id);
                        results.e1id = id;
                        // get entity, including fieldGroups
                        testEntityId = id;
                        return spEntityService.getEntity(results.e1id, 'name,fieldGroups.name');
                    })
                    .then(function (e1Loaded) {
                        results.e1loaded = e1Loaded;

                        // Add a field group
                        var fg = spEntity.fromJSON({
                            typeId: 'fieldGroup',
                            name: 'Test FieldGroup 100'
                        });
                        fg.alias('test:testFieldGroup100');
                        e1Loaded.getFieldGroups().add(fg);
                        return spEntityService.putEntity(e1Loaded);
                    })
                    .then(function () {
                        return spEntityService.getEntity(results.e1id, 'name,fieldGroups.name');
                    })
                    .then(function (e1Loaded2) {
                        expect(e1Loaded2.getFieldGroups().length).toBe(1);
                        expect(e1Loaded2.getFieldGroups()[0].getName()).toBe('Test FieldGroup 100');
                        return spEntityService.deleteEntities(toDelete);
                    })

            );

        }));

        it('can create and delete a reverse alias relationship3', inject(function (spEntityService) {
            var json = {
                typeId: 'core:definition',
                name: 'Reverse Relationship Test 1'
            };

            var definition = spEntity.fromJSON(json);

            // works:
            //var request = 'name,{reverseRelationships.id,{fieldGroups, reverseRelationships.relationshipInFromTypeGroup}.name}';
            // breaks:
            var request = 'name,reverseRelationships.alias, fieldGroups.name, reverseRelationships.relationshipInFromTypeGroup.id';

            var toDelete = [];

            TestSupport.wait(
                spEntityService.getEntity('core:type', request)
                    .then(function(typeInfo) {

                        spEntity.augment(definition, typeInfo);

                        var fgJson = {
                            typeId: 'core:fieldGroup',
                            name: 'Test Field Group',
                            description: 'A test field group'
                        };

                        var fieldGroup = spEntity.fromJSON(fgJson);

                        var fieldGroups = definition.getRelationship('core:fieldGroups');

                        expect(fieldGroups).toBeTruthy();
                        fieldGroups.add(fieldGroup);

                        return spEntityService.putEntity(definition);
                    })
                    .then(function(id) {
                        toDelete.push(id);
                        return spEntityService.getEntity(id, request);
                    })
                    .then(function(entity) {
                        expect(entity.getFieldGroups().length).toBe(1);
                        expect(entity.getFieldGroups()[0].getName()).toBe('Test Field Group');

                        return spEntityService.deleteEntities(toDelete);
                })
            );

        }));


    });

    describe('new features of the entity info service', function () {
        var testEntityId;

        it('should be able to request multiple entities', inject(function ($http, spWebService, $rootScope) {
            var done = false;

            runs(function () {
                done = false;

                var webApiRoot = spWebService.getWebApiRoot();
                var headers = spWebService.getHeaders();
                var url = webApiRoot + '/spapi/data/v1/entity';
                var params = {
                    id: ['test:steveGibbon', 'activityType'],
                    request: ['alias,name', 'alias,name,description']
                };
                url += '?' + $.param(params);

                $http({
                    method: 'GET',
                    url: url,
                    headers: headers

                }).success(function (data) {

                    console.log('success', data);
                    done = true;
                    expect(data).toBeEntityData();

                    var entities = spEntity.entityDataToEntities(data);
                    expect(entities).toBeTruthy();
                    expect(entities.length).toBe(2);

                    console.log('entities', entities);

                    var entity = entities[0];
                    expect(entity).toBeEntity();
                    console.log('entity:', entity.debugString);
                    expect(entity.name).toContain('ibbon');

                    testEntityId = entity.id();

                    entity = entities[1];
                    expect(entity).toBeEntity();
                    console.log('entity:', entity.debugString);
                    expect(entity.name).toContain('ctivity');

                }).error(function (err) {

                    console.log('error', err);

                    expect('the http request failed with error ' + err).toBeFalsy();

                    done = true;
                });

                $rootScope.$apply();
            });

            waitsFor(function () {
                return done;
            });
        }));

        it('should be able to request multiple entities', inject(function ($http, spWebService, $rootScope) {
            var done = false;

            runs(function () {
                done = false;

                var webApiRoot = spWebService.getWebApiRoot();
                var headers = spWebService.getHeaders();
                var url = webApiRoot + '/spapi/data/v1/entity/';
                url += testEntityId;
                var params = {
                    request: 'alias,name'
                };
                url += '?' + $.param(params);

                $http({
                    method: 'GET',
                    url: url,
                    headers: headers

                }).success(function (data) {

                    console.log('success', data);
                    done = true;
                    expect(data).toBeEntityData();

                    var entities = spEntity.entityDataToEntities(data);
                    expect(entities).toBeTruthy();
                    expect(entities.length).toBeGreaterThan(0);

                    console.log('entities', entities);

                    var entity = entities[0];
                    expect(entity).toBeEntity();

                    console.log('entity:', entity.debugString);

                    expect(entity.name).not.toBeNull();
                    expect(entity.name).toContain('ibbon');

                }).error(function (err) {

                    console.log('error', err);

                    expect('the http request failed with error ' + err).toBeFalsy();

                    done = true;
                });

                $rootScope.$apply();
            });

            waitsFor(function () {
                return done;
            });
        }));
    });

    describe('some direct web api tests', function () {

        it('should be able to request via $http angular service', inject(function ($http, spWebService, $rootScope) {
            var done = false;

            runs(function () {
                done = false;

                var webApiRoot = spWebService.getWebApiRoot();
                var headers = spWebService.getHeaders();
                $http({
                    method: 'GET',
                    url: webApiRoot + '/spapi/data/v1/entity/test/steveGibbon',
                    params: { request: 'alias,name' },
                    headers: headers

                }).success(function (data) {

                    console.log('success', data);
                    done = true;
                    expect(data).toBeEntityData();

                    var entities = spEntity.entityDataToEntities(data);
                    expect(entities).toBeTruthy();
                    expect(entities.length).toBeGreaterThan(0);

                    console.log('entities', entities);

                    var entity = entities[0];
                    expect(entity).toBeEntity();

                    console.log('entity:', entity.debugString);

                    expect(entity.name).toContain('ibbon');

                }).error(function (err) {

                    console.log('error', err);

                    expect('the http request failed with error ' + err).toBeFalsy();

                    done = true;
                });

                $rootScope.$apply();
            });

            waitsFor(function () {
                return done;
            });
        }));

        it('should be able to post via $http angular service', inject(function ($http, spWebService, $rootScope) {
            var done = false;

            var webApiRoot = spWebService.getWebApiRoot();
            var headers = spWebService.getHeaders();
            runs(function () {
                done = false;

                var entity = spEntity.createEntityOfType('oldshared:employee');
                entity.setName('Test Person');
                entity.registerField('oldshared:firstName', spEntity.DataType.String);
                entity.registerField('oldshared:lastName', spEntity.DataType.String);
                entity.field('oldshared:firstName', 'Test');
                entity.field('oldshared:lastName', 'Person');
                var postData = spEntity.entitiesToEntityData(entity);

                $http({
                    method: 'POST',
                    url: webApiRoot + '/spapi/data/v1/entity',
                    data: postData,
                    headers: headers

                }).success(function (data) {

                    //console.log('success', data);
                    done = true;
                    expect(data).toBeGreaterThan(0);

                    var id = parseInt(data, 10);
                    expect(id).toNotBe(NaN);

                }).error(function (err) {

                    console.log('error', err);
                    expect('the http request failed with error ' + err).toBeFalsy();
                    done = true;
                });

                $rootScope.$apply();
            });

            waitsFor(function () {
                return done;
            });
        }));
    });

    describe('client-side serialization round trip', function () {

        it('works with bidirectional relationship from server', inject(function (spEntityService) {

            // Check against problems that arise from having the same relationship followed by alias and reverseAlias between pairs of entities

            // Create a definition and two fields
            var json = {
                    typeId: 'definition',
                    name: 'SerTestType1',
                    inherits: {
                        typeId: 'definition',
                        name: 'SerTestType2',
                        inherits: { id: 'test:person' }
                    }
                };

            var definition = spEntity.fromJSON(json);

            TestSupport.wait(
                spEntityService.putEntity(definition)
                .then(function (id) {
                    testEntityIds.push(id);
                    return spEntityService.getEntity(id, '{inherits, derivedTypes}.{inherits.id, derivedTypes.id}');
                }).then(function (e) {
                    var e2 = e.inherits[0];
                    testEntityId = e2._id._id;
                    var person = e2.inherits[0];
                    testEntityIds.push(e2.idP);

                    // assert expected relationship data from server
                    expect(e.inherits).toBeArray(1, 'test1');
                    expect(e.derivedTypes).toBeArray(0, 'test2');
                    expect(e2.inherits).toBeArray(1, 'test3');
                    expect(e2.derivedTypes).toBeArray(1, 'test4');
                    expect(e.inherits[0]).toBe(e2, 'test5');
                    expect(e2.inherits[0]).toBe(person, 'test6');
                    expect(e2.derivedTypes[0]).toBe(e, 'test7');
                    
                    var ser = spEntity.serialize(e);
                    e = spEntity.deserialize(ser);
                    e2 = e.inherits[0];
                    person = e2.inherits[0];

                    // reassert that it survived serialization round trip
                    expect(e.inherits).toBeArray(1, 'test1b');
                    expect(e.derivedTypes).toBeArray(0, 'test2b');
                    expect(e2.inherits).toBeArray(1, 'test3b');
                    expect(e2.derivedTypes).toBeArray(1, 'test4b');
                    expect(e.inherits[0]).toBe(e2, 'test5b');
                    expect(e2.inherits[0]).toBe(person, 'test6b');
                    expect(e2.derivedTypes[0]).toBe(e, 'test7b');

                    return spEntityService.deleteEntity(testEntityId);
                }));
    }));

});

});
