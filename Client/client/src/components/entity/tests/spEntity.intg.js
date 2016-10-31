// Copyright 2011-2016 Global Software Innovation Pty Ltd
/* globals spEntity */

describe('Entity Model|spEntity|intg:', function () {
    'use strict';

    beforeEach(module('sp.common.loginService'));

    beforeEach(inject(function ($injector) {
        TestSupport.setupIntgTests(this, $injector);
    }));
    
    var getTestEntity, getEmptyEntity;

    beforeEach(inject(function (spEntityService) {
        getTestEntity = function (testEntityAlias) {
                var query =
                    'alias, name, description, isOfType.{alias}, ' +
                        'test:afNumber, test:afBoolean, test:afDecimal, test:afCurrency, ' +
                        'test:afTime, test:afDate, test:afDateTime, ' +
                        'test:afString, test:afMultiline, ' +
                        'test:allFieldsEmployee.{name, description, test:employeeAllFields.name }, ' + // many to one
                        'test:trucks.{name, test:truckAllFields.id }, ' + // one to many
                        'test:herbs.{name, test:herbAllFields.id }, ' +        // many to many
                        'test:drinks.{name, test:drinkAllFields.id }';         // one to one (despite what the name implies)

                return spEntityService.getEntity(testEntityAlias || 'test:af02', query);
            };
        getEmptyEntity = function () {
            return getTestEntity('test:af30');
        };

    }));
    // Use var e = getTestEntity() for any mutable tests
    

    var $injector, $http, $rootScope, $q,
        spWebService, webApiRoot, spEntityService;

    var entity, emptyEntity;

    var getWritableEntity = function() {
        return entity;
    };



    beforeEach(inject(function ($injector) {
        TestSupport.setupIntgTests(this, $injector);
        TestSupport.setWaitTimeout(20000);

        // load a DI table with the modules needed
        $http = $injector.get('$http');
        $rootScope = $injector.get('$rootScope');
        spWebService = $injector.get('spWebService');
        spEntityService = $injector.get('spEntityService');


        // Grabbing the headers as defined in the spEntityService, only needed if you want to do some
        // direct $http calls.
        webApiRoot = spWebService.getWebApiRoot();

        
        var result1 = {};
        TestSupport.waitCheckReturn($rootScope, getTestEntity(), result1);
        runs(function () {
            entity = result1.value;
        });

        if (!emptyEntity) {
            var result2 = {};
            TestSupport.waitCheckReturn($rootScope, getEmptyEntity(), result2);
            runs(function () {
                emptyEntity = result2.value;
            });
        }
    }));


    describe('spEntity', function () {

        // Introduction Tests

        it('should exist', function () {
            expect(spEntity).toBeTruthy();
        });

        it('test data ready', function () {
            expect(entity).toBeTruthy();
            expect(emptyEntity).toBeTruthy();
            expect(getWritableEntity()).toBeTruthy();
        });
    });

    describe('fields', function () {

        // Field Lookup Tests

        it('can access a core field without namespace', function () {
            expect(entity.getField('name')).toEqual('Test 02');
        });

        it('can access a field with namespace', function () {
            expect(entity.getField('core:name')).toEqual('Test 02');
        });

        it('can access a field by ID', function () {
            // first get the field id
            var fc = entity.getFieldContainer('name');
            var fieldIdNumber = fc.id.getId();
            expect(fieldIdNumber).toBeGreaterThan(0);
            // actual test
            expect(entity.getField(fieldIdNumber)).toEqual('Test 02');
        });

        it('should return null for non-existent field name', function () {
            expect(entity.getField('blahBlahBlah')).toBeNull();
        });

        it('should return null for non-existent field ID', function () {
            expect(entity.getField(999999)).toBeNull();
        });

        it('should return null for empty string fields', function () {
            expect(emptyEntity.getField('test:afString')).toBeNull();
        });

        it('setter should work by ID', function () {
            var e = getWritableEntity();
            // first get the field id
            var fc = e.getFieldContainer('test:afNumber');
            var fieldIdNumber = fc.id.getId();
            expect(fieldIdNumber).toBeGreaterThan(0);
            // actual test
            expect(e.setField(fieldIdNumber, 25000)).toEqual(e);
            expect(e.getField('test:afNumber')).toEqual(25000);
        });

        it('setter should work by alias', function () {
            var e = getWritableEntity();
            // first get the field id
            var fc = e.getFieldContainer('test:afNumber');
            var fieldIdNumber = fc.id.getId();
            expect(fieldIdNumber).toBeGreaterThan(0);
            // actual test
            expect(e.setField('test:afNumber', 9876)).toEqual(e);
            expect(e.getField(fieldIdNumber)).toEqual(9876);
            expect(e._dataState).toEqual(spEntity.DataStateEnum.Update);
        });

        it('can set null value', function () {
            var e = getWritableEntity();
            expect(e.setField('test:afNumber', null)).toEqual(e);
            expect(e.getField('test:afNumber')).toBeNull();
            expect(e._dataState).toEqual(spEntity.DataStateEnum.Update);
        });

        it('can set zero value', function () {
            var e = getWritableEntity();
            expect(e.setField('test:afNumber', 0)).toEqual(e);
            expect(e.getField('test:afNumber')).toBe(0);
            expect(e._dataState).toEqual(spEntity.DataStateEnum.Update);
        });

        it('can set empty string', function () {
            var e = getWritableEntity();
            expect(e.setField('test:afMultiline', '')).toEqual(e);
            expect(e.getField('test:afMultiline')).toBe('');
            expect(e._dataState).toEqual(spEntity.DataStateEnum.Update);
        });
    });

    describe('field containers', function () {

        // Field Lookup Tests

        it('can be accessed via getFieldContainer', function () {
            expect(entity.getFieldContainer('name') instanceof spEntity.Field).toBeTruthy();
        });

        it('can be registered via registerField', function () {
            var e = getWritableEntity();
            expect(e.registerField('test:blah', spEntity.DataType.Int32) instanceof spEntity.Field).toBeTruthy();
            expect(e.getFieldContainer('test:blah') instanceof spEntity.Field).toBeTruthy();
            expect(e.getBlah).toBeDefined();
            //expect(e.getBlah()).toBeNull();   //TODO . fix this
            expect(e.setBlah).toBeDefined();
            expect(e.setBlah(123)).toBe(e);
            expect(e.getBlah()).toBe(123);
        });

        it('can be tested with hasField', function () {
            expect(entity.hasField('core:name')).toBeTruthy();
            expect(entity.hasField('test:whatever')).toBeFalsy();
        });
    });


    describe('field types', function () {

        // Field Type Tests

        it('can access a string field', function () {
            expect(entity.getField('test:afString')).toEqual('data 02');
            expect(typeof entity.getField('test:afString')).toEqual('string');
        });

        it('can access a multiline string field', function () {
            expect(entity.getField('test:afMultiline')).toEqual('multi \ntext \nfor \nTest 02');
            expect(typeof entity.getField('test:afMultiline')).toEqual('string');
        });

        it('can access an int field', function () {
            expect(entity.getField('test:afNumber')).toEqual(200);
            expect(typeof entity.getField('test:afNumber')).toEqual('number');
        });

        it('can access a bool field', function () {
            expect(entity.getField('test:afBoolean')).toEqual(true);
            expect(typeof entity.getField('test:afBoolean')).toEqual('boolean');
        });

        it('can access a decimal field', function () {
            expect(entity.getField('test:afDecimal')).toEqual(200.222);
            expect(typeof entity.getField('test:afDecimal')).toEqual('number');
        });

        it('can access a date field', function () {
            expect(entity.getField('test:afDate')).toEqual(new Date('2013-06-02T00:00:00Z'));
            expect(typeof entity.getField('test:afDate')).toEqual('object');
        });

        it('can access a time field', function () {
            expect(entity.getField('test:afTime')).toEqual(new Date('1753-01-01T02:00:00Z'));
            expect(typeof entity.getField('test:afTime')).toEqual('object');
        });

        it('can access a date-time field', function () {
            expect(entity.getField('test:afDateTime')).toEqual(new Date('2013-06-01T16:00:00Z'));
            expect(typeof entity.getField('test:afDateTime')).toEqual('object');
        });

        it('can access an alias field if it was requested', function () {
            expect(entity.getField('core:alias')).toEqual('test:af02');
            expect(entity.getAlias()).toEqual('test:af02');
        });

        it('can access alias on the ID if it was requested', function () {
            expect(entity.eid().getNamespace()).toEqual('test');
            expect(entity.eid().getAlias()).toEqual('af02');
        });
    });


    describe('dynamic fields', function () {

        it('getter should exist for returned fields', function () {
            expect(entity.getAfNumber).toBeTruthy();
        });

        it('setter should exist for returned fields', function () {
            expect(entity.setAfNumber).toBeTruthy();
        });

        it('getter should work for returned fields', function () {
            expect(entity.getAfNumber()).toEqual(200);
        });

        it('setter should work for returned fields', function () {
            var e = getWritableEntity();
            expect(e.setAfNumber(9876)).toEqual(e);
            expect(e.getAfNumber()).toEqual(9876);
            expect(e._dataState).toEqual(spEntity.DataStateEnum.Update);
        });

        it('accessors get reused (on setter)', function () {
            // Note: getters got further optimised at the expense of reuse
            expect(entity.getLookup('test:allFieldsEmployee').setDescription).toEqual(entity.setDescription);
        });
    });


    describe('lookups', function () {

        it('return the entity when there is data (many to one, forward)', function () {
            expect(entity.getLookup('test:allFieldsEmployee').getName()).toEqual('Peter Aylett');
        });

        it('return the entity when there is data (one to one, forward)', function () {
            expect(entity.getLookup('test:drinks').getName()).toEqual('Coke Zero');
        });

        it('return the entity when there is data (one to one, reverse)', function () {
            expect(entity.getName()).toEqual('Test 02');
            var halfWay = entity.getLookup('test:drinks');
            expect(halfWay.getName()).toEqual('Coke Zero');
            expect(halfWay.getLookup('test:drinkAllFields').getName()).toEqual('Test 02');
        });

        it('return the entity when there is data (one to many, reverse)', function () {
            expect(entity.getName()).toEqual('Test 02');
            var halfWay = entity.getRelationship('test:trucks')[0];
            expect(halfWay).toBeTruthy();
            expect(halfWay.getLookup('test:truckAllFields').getName()).toEqual('Test 02');
        });

        it('return same entity object for same resource', function () {
            var halfWay = entity.getRelationship('test:trucks')[0];
            expect(halfWay).toBeTruthy();
            expect(halfWay.getLookup('test:truckAllFields')).toBe(entity);
        });

        it('return null when the lookups was not requested', function () {
            expect(entity.getLookup('test:thisDoesNotExist')).toBeNull();
        });

        it('return null when the lookups was requested but there was no data', function () {
            expect(emptyEntity.getLookup('test:allFieldsEmployee')).toBeNull();
        });

        it('can be cleared', function () {
            var e = getWritableEntity();
            expect(e.setLookup('test:allFieldsEmployee', null)).toEqual(e);
            expect(e.getLookup('test:allFieldsEmployee')).toBeNull();
        });

        it('can be updated', function () {
            var newValue = spEntity.fromId(123);
            var e = getWritableEntity();
            expect(e.setLookup('test:allFieldsEmployee', newValue)).toEqual(e);
            expect(e.getLookup('test:allFieldsEmployee').id()).toEqual(123);
            expect(e._dataState).toEqual(spEntity.DataStateEnum.Update);        // do we need this one?
            var r = e.getRelationshipContainer('test:allFieldsEmployee');
            expect(r.removeExisting).toBeTruthy();                              // we are replacing the existing relationship
            expect(r.instances[0]._dataState).toEqual(spEntity.DataStateEnum.Create);   // and establishing a new relationship instance
            expect(r.instances[0].entity._dataState).toEqual(spEntity.DataStateEnum.Unchanged);   // but not a new entity
        });
    });


    describe('lookup containers', function () {

        it('can be registered via registerLookup', function () {
            var e = getWritableEntity();
            var e2 = spEntity.fromId(123);
            expect(e.registerLookup('test:blah') instanceof spEntity.Relationship).toBeTruthy();
            expect(e.getRelationshipContainer('test:blah') instanceof spEntity.Relationship).toBeTruthy();
            expect(e.getRelationshipContainer('test:blah').isLookup).toBeTruthy();
            expect(e.getBlah).toBeDefined();
            expect(e.getBlah()).toBeNull();
            expect(e.setBlah).toBeDefined();
            expect(e.setBlah(e2)).toBe(e);
            expect(e.getBlah()).toBe(e2);
        });
    });


    describe('lookup accessors', function () {

        it('return the entity when there is data (many to one, forward)', function () {
            expect(entity.getAllFieldsEmployee().getName()).toEqual('Peter Aylett');
        });

        it('return the entity when there is data (one to one, forward)', function () {
            expect(entity.getDrinks().getName()).toEqual('Coke Zero');
        });

        it('return the entity when there is data (one to one, reverse)', function () {
            expect(entity.getName()).toEqual('Test 02');
            var halfWay = entity.getDrinks();
            expect(halfWay.getName()).toEqual('Coke Zero');
            expect(halfWay.getDrinkAllFields().getName()).toEqual('Test 02');
        });

        it('return the entity when there is data (one to many, reverse)', function () {
            expect(entity.getName()).toEqual('Test 02');
            var halfWay = entity.getTrucks()[0];
            expect(halfWay).toBeTruthy();
            expect(halfWay.getTruckAllFields()[0].getName()).toEqual('Test 02');
        });

        it('are undefined when the lookups was not requested', function () {
            expect(entity.getThisDoesNotExist).toBeUndefined();
        });

        it('return null when the lookup was requested but there was no data', function () {
            expect(emptyEntity.getAllFieldsEmployee()).toBeNull();
        });

        it('can be cleared', function () {
            var e = getWritableEntity();
            expect(e.setAllFieldsEmployee(null)).toEqual(e);
            expect(e.getAllFieldsEmployee()).toBeNull();
        });

        it('can be updated', function () {
            var newValue = spEntity.fromId(456);
            var e = getWritableEntity();
            expect(e.setAllFieldsEmployee(newValue)).toEqual(e);
            expect(e.getAllFieldsEmployee().id()).toEqual(456);
        });
    });


    describe('relationship containers', function () {

        it('can be accessed', function () {
            expect(entity.getRelationshipContainer).toBeDefined();
            var rel = entity.getRelationshipContainer('test:trucks');
            expect(rel).toBeDefined();
            expect(rel instanceof spEntity.Relationship).toBeTruthy();
        });

        it('can be registered via registerRelationship', function () {
            var e = getWritableEntity();
            var e2 = spEntity.fromId(123);
            expect(e.registerRelationship('test:blah') instanceof spEntity.Relationship).toBeTruthy();
            expect(e.getRelationshipContainer('test:blah') instanceof spEntity.Relationship).toBeTruthy();
            expect(e.getRelationshipContainer('test:blah').isLookup).toBeFalsy();
            expect(e.getBlah).toBeDefined();
            expect(e.getBlah()).toBeEmptyArray();   //TODO . fix this
            expect(e.setBlah).toBeDefined();
            expect(e.setBlah([e2])).toBe(e);
            var res = e.getBlah();
            expect(res).toBeArray(1);
            expect(res[0]).toBe(e2);
        });

        it('are initially clean', function () {
            var rel = entity.getRelationshipContainer('test:trucks');
            expect(rel.removeExisting).toBeFalsy();
        });

        it('have a direction', function () {
            var rel = entity.getRelationshipContainer('test:trucks');
            expect(rel.isReverse).toBeFalsy();
        });

        it('know whether they are a lookup (false)', function () {
            var rel = entity.getRelationshipContainer('test:trucks');
            expect(rel.isLookup).toBeFalsy();
        });

        it('know whether they are a lookup (true)', function () {
            var rel = entity.getRelationshipContainer('test:allFieldsEmployee');
            expect(rel.isLookup).toBeTruthy();
        });

        it('can be tested with hasRelationship', function () {
            expect(entity.hasRelationship('test:trucks')).toBeTruthy();
            expect(entity.hasRelationship('test:whatever')).toBeFalsy();
        });

        it('get isReverse set from server response', function () {
            var drinks = entity.getDrinks();
            var rc = drinks.getRelationshipContainer('test:drinkAllFields');
            expect(rc.isReverse).toBe(false);
        });

    });


    describe('relationships', function () {

        it('return the entities when there is data (one to many, forward)', function () {
            var rel = entity.getRelationship('test:trucks');
            expect(rel).toBeArray(4);
            expect(rel[0]).toBeEntity();
        });

        it('return the entities when there is data (many to many, forward)', function () {
            var rel = entity.getRelationship('test:herbs');
            expect(rel).toBeArray(3);
            expect(rel[0]).toBeEntity();
        });

        it('return the entities when there is data (many to many, reverse)', function () {
            var halfway = entity.getRelationship('test:herbs')[0];
            expect(halfway).toBeEntity();
            var res = halfway.getRelationship('test:herbAllFields');
            expect(res).toBeArray();
            expect(res[0]).toBeEntity();
        });

        it('return the entities when there is data (many to one, reverse)', function () {
            var halfway = entity.getLookup('test:allFieldsEmployee');
            expect(halfway).toBeEntity();
            var res = halfway.getRelationship('test:employeeAllFields');
            expect(res).toBeArray();
            expect(res[0]).toBeEntity();
        });

        //it('return null when the relationship was not requested', function () {
        //    expect(entity.getRelationship('test:thisDoesNotExist')).toBeNull();      // returns empty array.. hmm.
        //});

        it('return an empty array when the relationship was requested but there was no data', function () {
            expect(entity.getRelationship('test:emptyRelationship')).toBeEmptyArray();
        });

        it('can be cleared', function () {
            var e = getWritableEntity();
            expect(e.setRelationship('test:trucks', [])).toEqual(e);
            expect(e.getRelationship('test:trucks')).toBeEmptyArray();
        });

        it('can be updated', function () {
            var newValue1 = spEntity.fromId(123);
            var newValue2 = spEntity.fromId(456);
            var newValue3 = spEntity.fromId(789);
            var e = getWritableEntity();
            expect(e.setRelationship('test:trucks', [newValue1, newValue2, newValue3])).toEqual(e);
            expect(e.getRelationship('test:trucks').length).toEqual(3);
        });

        it('can be updated with id or alias', function () {
            var newValue1 = 123;
            var newValue2 = 'test:trucks123';
            var newValue3 = spEntity.fromId(789);
            var e = getWritableEntity();
            expect(e.setRelationship('test:trucks', [newValue1, newValue2, newValue3])).toEqual(e);
            expect(e.getRelationship('test:trucks').length).toEqual(3);
            expect(e.getRelationship('test:trucks')[0].id()).toBe(newValue1);
            expect(e.getRelationship('test:trucks')[1].alias()).toBe(newValue2);
            expect(e.getRelationship('test:trucks')[2].id()).toBe(newValue3.id());
        });

        it('return the same array instance if no change has been made', function () {
            var rel1 = entity.getRelationship('test:trucks');
            var rel2 = entity.getRelationship('test:trucks');
            expect(rel1).toBe(rel2);
        });

        it('relationship instances are flagged as unchanged when first loaded from server', function () {
            var rel = entity.getRelationship('test:trucks');
            var inst = rel.getInstances()[0];
            expect(inst.getDataState()).toBe(spEntity.DataStateEnum.Unchanged);
        });
    });


    describe('relationship accessors', function () {

        it('return the entities when there is data (one to many, forward)', function () {
            var rel = entity.getTrucks();
            expect(rel).toBeArray(4);
            expect(rel[0]).toBeEntity();
        });

        it('return the entities when there is data (many to many, forward)', function () {
            var rel = entity.getRelationship('test:herbs');
            expect(rel).toBeArray(3);
            expect(rel[0]).toBeEntity();
        });

        it('return the entities when there is data (many to many, reverse)', function () {
            var halfway = entity.getRelationship('test:herbs')[0];
            expect(halfway).toBeEntity();
            var res = halfway.getHerbAllFields();
            expect(res).toBeArray();
            expect(res[0]).toBeEntity();
        });

        it('return the entities when there is data (many to one, reverse)', function () {
            var halfway = entity.getLookup('test:allFieldsEmployee');
            expect(halfway).toBeEntity();
            var res = halfway.getEmployeeAllFields();
            expect(res).toBeArray();
            expect(res[0]).toBeEntity();
        });

        it('return undefined when the relationship was not requested', function () {
            expect(entity.getThisDoesNotExist).toBeUndefined();
        });

        //it('return an empty array when the relationship was requested but there was no data', function () {
        //    expect(entity.getEmptyRelationship()).toBeEmptyArray();                                            // uh oh currently returns null
        //});

        it('can be cleared', function () {
            var e = getWritableEntity();
            expect(e.setTrucks([])).toEqual(e);
            expect(e.getTrucks()).toBeEmptyArray();
        });

        it('can be updated', function () {
            var newValue1 = spEntity.fromId(123);
            var newValue2 = spEntity.fromId(456);
            var newValue3 = spEntity.fromId(789);
            var e = getWritableEntity();
            expect(e.setTrucks([newValue1, newValue2, newValue3])).toEqual(e);
            expect(e.getTrucks().length).toEqual(3);
        });
    });


    describe('type info', function () {

        it('gets stored into entity getType() if isOfType was requested', function () {
            var idViaRelationship = entity.getIsOfType()[0].eid().getId();
            expect(entity.getType().getId()).toBe(idViaRelationship);
        });

        it('alias gets stored into entity getType() if isOfType was requested', function () {
            expect(entity.getType().getNsAlias()).toBe('test:allFields');
        });
    });


    describe('packageEntityNugget', function () {

        it('can be called with an entity', function () {
            var e = spEntity.fromJSON({
                name: 'Test Entity',
                typeId: 'core:report'
            });
            var nugget = spEntityService.packageEntityNugget(e);
            expect(nugget.v1).toBeTruthy();
        });

        it('can be called with null', function () {
            var e = spEntity.fromJSON(null);
            var nugget = spEntityService.packageEntityNugget(e);
            expect(nugget.v1).toBeNull();
        });

        it('can be decoded by the server', function () {
            // This test-case represents the typical use-case
            var e = spEntity.fromJSON({
                name: 'Test Entity',
                typeId: 'core:report'
            });

            var postData = {
                myOtherData: 'abc',
                myEntityData: spEntityService.packageEntityNugget(e)
            };

            var worker = $http({
                method: 'POST',
                url: spWebService.getWebApiRoot() + '/spapi/data/v1/test/entitytest',
                data: postData,
                headers: spWebService.getHeaders()
            }).then(function (response) {
                var data = response.data;
                return data;
            });

            var result = {};
            TestSupport.waitCheckReturn($rootScope, worker, result);
            runs(function () {
                var data = result.value;
                expect(data).toBeTruthy();
                expect(data.myOtherData).toBe('abc');
                expect(data.success).toBeTruthy();
            });

        });

        it('can pass nulls to the server', function () {
            var postData = {
                myOtherData: 'abc',
                myEntityData: spEntityService.packageEntityNugget(null)
            };

            var worker = $http({
                method: 'POST',
                url: spWebService.getWebApiRoot() + '/spapi/data/v1/test/entitytest',
                data: postData,
                headers: spWebService.getHeaders()
            }).then(function(response) {
                var data = response.data;
                return data;
            });

            var result = {};
            TestSupport.waitCheckReturn($rootScope, worker, result);
            runs(function () {
                var data = result.value;
                expect(data).toBeTruthy();
                expect(data.myOtherData).toBe('abc');
                expect(data.success).toBeFalsy();
            });

        });
    });


});