// Copyright 2011-2016 Global Software Innovation Pty Ltd
/* globals spEntity, jsonString, jsonInt, jsonLookup, jsonRelationship, jsonDecimal, jsonBool,
 jsonCurrency, jsonDate, jsonDateTime, jsonTime, entityTestData */

describe('Entity Model|spEntity|spec:', function () {
    'use strict';

    // Use 'entity' for any read-only tests
    var entity, emptyEntity, allFieldsReport;
    // Use var e = getTestEntity() for any mutable tests
    var getTestEntity = function (testEntityData) {
        if (!testEntityData)
            testEntityData = entityTestData.af02;
        var testEntity = spEntity.entityDataVer2ToEntities(testEntityData)[0];
        return testEntity;
    };

    var getWritableEntity = function () {
        return getTestEntity();
    };

    beforeEach(function () {
        this.addMatchers(TestSupport.matchers);
        if (!entity) {
            entity = getTestEntity(entityTestData.af02);
            emptyEntity = getTestEntity(entityTestData.af30);
            allFieldsReport = getTestEntity(entityTestData.allFieldsReport);
        }
    });


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


    describe('id and alias', function () {

        it('can get id() function [deprecated]', function () {
            var e = spEntity.fromJSON(123);
            expect(e.id()).toBe(123);
        });

        it('can get idP property', function () {
            var e = spEntity.fromJSON(123);
            expect(e.idP).toBe(123);
        });

        it('can get nsAlias property', function () {
            var e1 = spEntity.fromJSON('test:abc');
            var e2 = spEntity.fromJSON('type');
            expect(e1.nsAlias).toBe('test:abc');
            expect(e2.nsAlias).toBe('core:type');
        });

        it('can get alias() function [deprecated]', function () {
            var e1 = spEntity.fromJSON('test:abc');
            var e2 = spEntity.fromJSON('type');
            expect(e1.alias()).toBe('test:abc');
            expect(e2.alias()).toBe('core:type');
        });

        it('can get eid() function [deprecated]', function () {
            var e = spEntity.fromJSON(123);
            expect(e.eid().getId()).toBe(123);
        });

        it('can get eidP property', function () {
            var e = spEntity.fromJSON(123);
            expect(e.eidP.getId()).toBe(123);
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

        it('flag dirty if there has been a string change', function () {
            var e = spEntity.fromJSON({ myfield: 'abc' }).markAllUnchanged();
            expect(e.getDataState()).toBe(spEntity.DataStateEnum.Unchanged);
            e.setMyfield('def');
            expect(e.getDataState()).toBe(spEntity.DataStateEnum.Update);
        });

        it('flag dirty if there has been an int change', function () {
            var e = spEntity.fromJSON({ myfield: 123 }).markAllUnchanged();
            expect(e.getDataState()).toBe(spEntity.DataStateEnum.Unchanged);
            e.setMyfield(456);
            expect(e.getDataState()).toBe(spEntity.DataStateEnum.Update);
        });

        it('flag dirty if there has been a bool change', function () {
            var e = spEntity.fromJSON({ myfield: false }).markAllUnchanged();
            expect(e.getDataState()).toBe(spEntity.DataStateEnum.Unchanged);
            e.setMyfield(true);
            expect(e.getDataState()).toBe(spEntity.DataStateEnum.Update);
        });

        it('flag dirty if there has been a change to null', function () {
            var e = spEntity.fromJSON({ myfield: 'abc' }).markAllUnchanged();
            expect(e.getDataState()).toBe(spEntity.DataStateEnum.Unchanged);
            e.setMyfield(null);
            expect(e.getDataState()).toBe(spEntity.DataStateEnum.Update);
        });

        it('flag dirty if there has been a change from null', function () {
            var e = spEntity.fromJSON({ myfield: jsonString() }).markAllUnchanged();
            expect(e.getDataState()).toBe(spEntity.DataStateEnum.Unchanged);
            e.setMyfield('def');
            expect(e.getDataState()).toBe(spEntity.DataStateEnum.Update);
        });

        it('flag dirty is not set if there has been no change', function () {
            var e = spEntity.fromJSON({ myfield: 'abc' }).markAllUnchanged();
            expect(e.getDataState()).toBe(spEntity.DataStateEnum.Unchanged);
            e.setMyfield('abc');
            expect(e.getDataState()).toBe(spEntity.DataStateEnum.Unchanged);
        });

        it('flag dirty is not set if there has been no change to null', function () {
            var e = spEntity.fromJSON({ myfield: jsonString() }).markAllUnchanged();
            expect(e.getDataState()).toBe(spEntity.DataStateEnum.Unchanged);
            e.setMyfield(null);
            expect(e.getDataState()).toBe(spEntity.DataStateEnum.Unchanged);
        });

        it('flag dirty detects null and 0 int as non-identical', function () {
            var e = spEntity.fromJSON({ myfield: jsonInt() }).markAllUnchanged();
            expect(e.getDataState()).toBe(spEntity.DataStateEnum.Unchanged);
            e.setMyfield(0);
            expect(e.getDataState()).toBe(spEntity.DataStateEnum.Update);
        });

        it('flag dirty detects null and blank string as identical', function () {
            var e = spEntity.fromJSON({ myfield: jsonString() }).markAllUnchanged();
            expect(e.getDataState()).toBe(spEntity.DataStateEnum.Unchanged);
            e.setMyfield('');
            expect(e.getDataState()).toBe(spEntity.DataStateEnum.Unchanged);
        });

        it('flag dirty detects null and false bool as identical', function () {
            var e = spEntity.fromJSON({ myfield: false }).markAllUnchanged();
            expect(e.getDataState()).toBe(spEntity.DataStateEnum.Unchanged);
            e.setMyfield(null);
            expect(e.getDataState()).toBe(spEntity.DataStateEnum.Unchanged);
        });

        it('can be accessed via the property getter', function () {
            var e = spEntity.fromJSON({ myfield: false }).markAllUnchanged();
            expect(e.getDataState()).toBe(spEntity.DataStateEnum.Unchanged);
            expect(e.myfield).toBe(false);
            e.myfield = true;
            expect(e.myfield).toBe(true);
            expect(e.getDataState()).toBe(spEntity.DataStateEnum.Update);
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
            expect(entity.getField('test:afMultiline')).toEqual('multi \rtext \rfor \rTest 02');
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

        it('can access an XML field', function () {
            expect(allFieldsReport.getField('core:queryXml').substring(0, 6)).toEqual('<Query');
            expect(typeof allFieldsReport.getField('core:queryXml')).toEqual('string');
        });

        it('can access a GUID field', function () {
            expect(allFieldsReport.getField('core:defaultDataViewId')).toEqual('00000000-0000-0000-0000-000000000000');
            expect(typeof allFieldsReport.getField('core:defaultDataViewId')).toEqual('string');
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

        it('getter should work for returned null fields', function () {
            expect(emptyEntity.getAfNumber()).toBeNull();
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

        it('property style accessors work like the accessor methods', function () {
            var e = getWritableEntity();
            expect(e.setAfNumber(9876)).toEqual(e);
            expect(e.afNumber).toBe(9876);

            e.afNumber = 1002;
            expect(e.afNumber).toBe(1002);
            expect(e.getAfNumber()).toEqual(1002);

            console.log(e);
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
            var e = getWritableEntity().markAllUnchanged();
            expect(e.setLookup('test:allFieldsEmployee', null)).toEqual(e);
            expect(e.getLookup('test:allFieldsEmployee')).toBeNull();
            expect(e.getDataState()).toBe(spEntity.DataStateEnum.Update);
        });

        it('can be updated by entity', function () {
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

        it('can be updated by ID', function () {
            var newValue = 123;
            var e = getWritableEntity().markAllUnchanged();
            expect(e.setLookup('test:allFieldsEmployee', newValue)).toEqual(e);
            expect(e.getLookup('test:allFieldsEmployee').id()).toEqual(123);
            expect(e.getDataState()).toBe(spEntity.DataStateEnum.Update);
        });

        it('can be updated by alias', function () {
            var newValue = 'core:whatever';
            var e = getWritableEntity().markAllUnchanged();
            expect(e.setLookup('test:allFieldsEmployee', newValue)).toEqual(e);
            expect(e.getLookup('test:allFieldsEmployee').eid().getNsAlias()).toEqual('core:whatever');
            expect(e.getDataState()).toBe(spEntity.DataStateEnum.Update);
        });

        it('flag dirty if a different entity instance is set', function () {
            var newValue = spEntity.fromId(123);
            var newValue2 = spEntity.fromId(123);
            var e = getWritableEntity();
            expect(e.setLookup('test:allFieldsEmployee', newValue)).toEqual(e);
            e.markAllUnchanged();
            expect(e.getDataState()).toBe(spEntity.DataStateEnum.Unchanged);
            expect(e.setLookup('test:allFieldsEmployee2', newValue2)).toEqual(e);
            expect(e.getDataState()).toBe(spEntity.DataStateEnum.Update);
        });

        it('does not flag dirty if the same entity instance is set', function () {
            var newValue = spEntity.fromId(123);
            var e = getWritableEntity();
            expect(e.setLookup('test:allFieldsEmployee', newValue)).toEqual(e);
            e.markAllUnchanged();
            expect(e.getDataState()).toBe(spEntity.DataStateEnum.Unchanged);
            expect(e.setLookup('test:allFieldsEmployee', newValue)).toEqual(e);
            expect(e.getDataState()).toBe(spEntity.DataStateEnum.Unchanged);
        });

        it('does not flag dirty if the same entity ID is set', function () {
            var newValue = 123;
            var e = getWritableEntity();
            expect(e.setLookup('test:allFieldsEmployee', newValue)).toEqual(e);
            e.markAllUnchanged();
            expect(e.getDataState()).toBe(spEntity.DataStateEnum.Unchanged);
            expect(e.setLookup('test:allFieldsEmployee', newValue)).toEqual(e);
            expect(e.getDataState()).toBe(spEntity.DataStateEnum.Unchanged);
        });

        it('does not flag dirty if the same null is set again', function () {
            var e = getWritableEntity();
            expect(e.setLookup('test:allFieldsEmployee', null)).toEqual(e);
            e.markAllUnchanged();
            expect(e.getDataState()).toBe(spEntity.DataStateEnum.Unchanged);
            expect(e.setLookup('test:allFieldsEmployee', null)).toEqual(e);
            expect(e.getDataState()).toBe(spEntity.DataStateEnum.Unchanged);
        });
    });


    describe('lookup containers', function () {

        it('can be registered via registerLookup', function () {
            var e = getWritableEntity();
            var e2 = spEntity.fromId(123);
            expect(e.registerLookup('test:blah') instanceof spEntity.Relationship).toBeTruthy();
            expect(e.getRelationshipContainer('test:blah') instanceof spEntity.Relationship).toBeTruthy();
            expect(e.getRelationshipContainer('test:blah').isLookup).toBeTruthy();
            expect(e.getRelationshipContainer('test:blah').isReverse).toBeFalsy();
            expect(e.getBlah).toBeDefined();
            expect(e.getBlah()).toBeNull();
            expect(e.setBlah).toBeDefined();
            expect(e.setBlah(e2)).toBe(e);
            expect(e.getBlah()).toBe(e2);
        });

        it('can be registered via registerLookup in reverse', function () {
            var e = getWritableEntity();
            expect(e.registerLookup({id:'test:blah', isReverse: true }) instanceof spEntity.Relationship).toBeTruthy();
            expect(e.getRelationshipContainer('test:blah')).toBeUndefined();
            expect(e.getRelationshipContainer({ id: 'test:blah', isReverse: true }) instanceof spEntity.Relationship).toBeTruthy();
            expect(e.getRelationshipContainer({ id: 'test:blah', isReverse: true }).isReverse).toBeTruthy();
        });

        it('can be registered via registerLookup in both directions', function () {
            var e = getWritableEntity();
            expect(e.registerLookup({ id: 'test:blah', isReverse: false }) instanceof spEntity.Relationship).toBeTruthy();
            expect(e.registerLookup({ id: 'test:blah', isReverse: true }) instanceof spEntity.Relationship).toBeTruthy();
            expect(e.getRelationshipContainer({ id: 'test:blah', isReverse: false }).isReverse).toBeFalsy();
            expect(e.getRelationshipContainer({ id: 'test:blah', isReverse: true }).isReverse).toBeTruthy();
        });

        it('can be registered by id via registerLookup in both directions', function () {
            var e = getWritableEntity();
            expect(e.registerLookup({ id: 123, isReverse: false }) instanceof spEntity.Relationship).toBeTruthy();
            expect(e.registerLookup({ id:123, isReverse: true }) instanceof spEntity.Relationship).toBeTruthy();
            expect(e.getRelationshipContainer({ id: 123, isReverse: false }).isReverse).toBeFalsy();
            expect(e.getRelationshipContainer({ id: 123, isReverse: true }).isReverse).toBeTruthy();
        });

        it('can be registered automatically setLookup', function () {
            var e = getWritableEntity();
            var e2 = spEntity.fromId(123);
            expect(e.setLookup('test:blah', e2)).toBe(e);
            expect(e.getRelationshipContainer('test:blah') instanceof spEntity.Relationship).toBeTruthy();
            expect(e.getRelationshipContainer('test:blah').isLookup).toBeTruthy();
            expect(e.setBlah).toBeDefined();
            expect(e.getBlah).toBeDefined();
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
            expect(halfWay.getTruckAllFields().getName()).toEqual('Test 02');
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

        it('property style accessors work like the methods', function () {
            var e = getWritableEntity();

            expect(e.setAllFieldsEmployee(spEntity.fromId(456))).toEqual(e);
            expect(e.allFieldsEmployee.id()).toEqual(456);

            e.allFieldsEmployee = spEntity.fromId(457);
            expect(e.allFieldsEmployee.id()).toEqual(457);
            expect(e.getAllFieldsEmployee().id()).toEqual(457);
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
            expect(e.getRelationshipContainer('test:blah').isReverse).toBeFalsy();
            expect(e.getBlah).toBeDefined();
            expect(e.getBlah()).toBeEmptyArray();   //TODO . fix this
            expect(e.setBlah).toBeDefined();
            expect(e.setBlah([e2])).toBe(e);
            var res = e.getBlah();
            expect(res).toBeArray(1);
            expect(res[0]).toBe(e2);
        });

        it('can be registered via registerRelationship in reverse', function () {
            var e = getWritableEntity();
            expect(e.registerRelationship({ id: 'test:blah', isReverse: true }) instanceof spEntity.Relationship).toBeTruthy();
            expect(e.getRelationshipContainer('test:blah')).toBeUndefined();
            expect(e.getRelationshipContainer({ id: 'test:blah', isReverse: true }) instanceof spEntity.Relationship).toBeTruthy();
            expect(e.getRelationshipContainer({ id: 'test:blah', isReverse: true }).isReverse).toBeTruthy();
        });

        it('can be registered via registerRelationship in both directions', function () {
            var e = getWritableEntity();
            expect(e.registerRelationship({ id: 'test:blah', isReverse: false }) instanceof spEntity.Relationship).toBeTruthy();
            expect(e.registerRelationship({ id: 'test:blah', isReverse: true }) instanceof spEntity.Relationship).toBeTruthy();
            expect(e.getRelationshipContainer({ id: 'test:blah', isReverse: false }).isReverse).toBeFalsy();
            expect(e.getRelationshipContainer({ id: 'test:blah', isReverse: true }).isReverse).toBeTruthy();
        });

        it('can be registered by id via registerRelationship in both directions', function () {
            var e = getWritableEntity();
            expect(e.registerRelationship({ id: 456, isReverse: false }) instanceof spEntity.Relationship).toBeTruthy();
            expect(e.registerRelationship({ id: 456, isReverse: true }) instanceof spEntity.Relationship).toBeTruthy();
            expect(e.getRelationshipContainer({ id: 456, isReverse: false }).isReverse).toBeFalsy();
            expect(e.getRelationshipContainer({ id: 456, isReverse: true }).isReverse).toBeTruthy();
        });

        it('are initially clean', function () {
            var rel = entity.getRelationshipContainer('test:trucks');
            expect(rel.removeExisting).toBeFalsy();
            expect(rel._deleteExisting).toBeFalsy();
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
            expect(entity.hasRelationship({ id: 'test:trucks', isReverse: false })).toBeTruthy();
            expect(entity.hasRelationship({ id: 'test:trucks', isReverse: true })).toBeFalsy();
            expect(entity.hasRelationship('test:whatever')).toBeFalsy();
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


        it('return an empty array when the relationship was not requested', function () {
            expect(entity.getRelationship('test:blahBlahRelationship')).toBeEmptyArray();
        });

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

        it('return the same array instance if no change has been made', function () {
            var rel1 = entity.getTrucks();
            var rel2 = entity.getTrucks();
            expect(rel1).toBe(rel2);
        });
    });


    describe('type info', function () {

        it('ID gets stored into entity getType() if isOfType was requested', function () {
            var idViaRelationship = entity.getIsOfType()[0].eid().getId();
            expect(entity.getType().getId()).toBe(idViaRelationship);
        });

        it('alias gets stored into entity getType() if isOfType was requested', function () {
            expect(entity.getType().getNsAlias()).toBe('test:allFields');
        });

        it('type can be accessed via type property', function () {
            expect(entity.type.nsAlias).toBe('test:allFields');
        });

        it('type array can be accessed via typesP property', function () {
            expect(entity.typesP[0].nsAlias).toBe('test:allFields'); 
        });
    });


    describe('augment', function () {

        it('handles null augment', function () {
            var e = spEntity.fromJSON({
                fEOnly: 123
            });
            spEntity.augment(e, null);
            expect(e.getFEOnly(), 123);
        });

        it('handles null primary', function () {
            var a = spEntity.fromJSON({
                fAOnly: true
            });
            spEntity.augment(null, a);
        });

        it('augments fields', function () {
            var e = spEntity.fromJSON({
                fEOnly: 123
            });
            var a = spEntity.fromJSON({
                fAOnly: true
            });
            spEntity.augment(e, a);
            expect(e.getFEOnly(), 123);
            expect(e.getFAOnly(), true);
        });

        it('augments relationships', function () {
            var e = spEntity.fromJSON({
                lEOnly: jsonLookup(22)
            });
            var a = spEntity.fromJSON({
                lAOnly: jsonLookup(44)
            });
            spEntity.augment(e, a);
            expect(e.getLEOnly().eid().getId(), 22);
            expect(e.getLAOnly().eid().getId(), 44);
        });

        it('prefers primary entity for fields', function () {
            var e = spEntity.fromJSON({
                fBoth: 'abc'
            });
            var a = spEntity.fromJSON({
                fBoth: 'def'
            });
            spEntity.augment(e, a);
            expect(e.getFBoth(), 'abc');
            expect(a.getFBoth(), 'def'); // and leave additional unadjusted
        });

        it('prefers primary entity for relationships', function () {
            var e = spEntity.fromJSON({
                lBoth: jsonLookup(11)
            });
            var a = spEntity.fromJSON({
                lBoth: jsonLookup(33)
            });
            spEntity.augment(e, a);
            expect(e.getLBoth().eid().getId(), 11);
            expect(a.getLBoth().eid().getId(), 33); // and leave additional unadjusted
        });

        it('can augment related entities', function () {
            var e = spEntity.fromJSON({
                computers: [
                    {
                        id: 123,
                        name: 'mypc',
                        form: 'desktop'
                    }
                ]
            });
            var a = spEntity.fromJSON({
                computers: [
                    {
                        id: 123,
                        name: 'mypc2',
                        os: 'windows'
                    }
                ]
            });
            spEntity.augment(e, a);
            var comp = e.getComputers()[0];
            expect(comp.getName(), 'mypc');
            expect(comp.getOs(), 'windows');
            expect(comp.getForm(), 'desktop');
        });

        it('children of augments get included', function () {
            var e = spEntity.fromJSON({
                computers: [
                    {
                        id: 123,
                        name: 'mypc'
                    }
                ]
            });
            var a = spEntity.fromJSON({
                computers: [
                    {
                        id: 123,
                        monitors: [11, 22]
                    }
                ]
            });
            spEntity.augment(e, a);
            var comp = e.getComputers()[0];
            expect(comp.getName(), 'mypc');
            var monitors = comp.getMonitors();
            expect(monitors.length, 2);
            expect(monitors[0].eid().getId(), 11);
        });

        it('can augment root from a template callback', function () {
            var e = spEntity.fromJSON({
                typeId: 'lamington',
                hasJam: false
            });
            var templateFn = function (type) {
                if (type === 'core:lamington') {
                    return spEntity.fromJSON({
                        hasJam: true,
                        hasCream: true
                    });
                }
                return null;
            };

            spEntity.augment(e, null, templateFn);
            expect(e.getHasJam(), false);
            expect(e.getHasCream(), true);
        });

        it('can augment related from a template callback', function () {
            var e = spEntity.fromJSON({
                desserts: [{
                    typeId: 'lamington',
                    hasJam: false
                }]
            });
            var templateFn = function (type) {
                if (type === 'core:lamington') {
                    return spEntity.fromJSON({
                        hasJam: true,
                        hasCream: true
                    });
                }
                return null;
            };

            spEntity.augment(e, null, templateFn);
            expect(e.getDesserts()[0].getHasJam(), false);
            expect(e.getDesserts()[0].getHasCream(), true);
        });

        it('typical use-case', function () {
            var report = spEntity.fromJSON({
                id: 1,
                columns: [{
                    id: 2,
                    typeId: 'reportColumn',
                    name: 'Name1',
                    columnFormat: { name: 'rightAlign' }
                }, {
                    id: 3,
                    typeId: 'reportColumn',
                    name: 'Desc'
                }, {
                    typeId: 'reportColumn',
                    name: 'New Column'
                }]
            });
            var additional = spEntity.fromJSON({
                createdBy: jsonLookup(),
                columns: [{
                    id: 2,
                    typeId: 'reportColumn',
                    name: 'Name',
                    width: 40
                }, {
                    id: 3,
                    typeId: 'reportColumn',
                    name: 'Desc',
                    width: 50
                }]
            });
            var templateFn = function(type) {
                switch (type) {
                case 'core:reportColumn':
                    return spEntity.fromJSON({
                        name: jsonString(),
                        width: 60,
                        columnFormat: { name: 'leftAlign' }
                    });
                }
                return null;
            };

            spEntity.augment(report, additional, templateFn);
            expect(report.getCreatedBy(), null);
            expect(report.getColumns()[0].getName(), 'Name1');
            expect(report.getColumns()[0].getWidth(), 40);
            expect(report.getColumns()[0].getColumnFormat().getName(), 'rightAlign');
            expect(report.getColumns()[1].getName(), 'Desc');
            expect(report.getColumns()[1].getWidth(), 50);
            expect(report.getColumns()[0].getColumnFormat().getName(), 'leftAlign');
            expect(report.getColumns()[2].getName(), 'New Column');
            expect(report.getColumns()[2].getWidth(), 60);
            expect(report.getColumns()[0].getColumnFormat().getName(), 'leftAlign');
        });

        it('typical use-case for field properties', function () {
            var fieldDefaultObject = {
                name: jsonString(''),
                description: jsonString(''),
                typeId: 'stringField',
                decimalPlaces: 3,
                isRequired: false,
                allowMultiLines: false,
                minLength: 0,
                maxLength: 250,
                isOfType: {
                    name: 'String Field',
                    id: 'stringField',
                    alias: 'core:stringField',
                    'k:fieldDisplayName': { name: jsonString('Text') }
                }
            };
            var formControlDefaultObject = {
                name: jsonString(''),
                description: 'template description',
                typeId: 'console:singleLineTextControl',
                isOfType: { id: 'k:singleLineTextControl' },
                'k:renderingBackgroundColor': { a: 255, r: 255, g: 255, b: 255 },
                'k:mandatoryControl': false,
                'k:readOnlyControl': false,
                'k:fieldToRender': fieldDefaultObject
            };
            var templateFn = function (type) {
                if (type === 'console:singleLineTextControl')
                    return spEntity.fromJSON(formControlDefaultObject);
                return null;
            };

            var formControl = spEntity.fromJSON({
                name: 'new control',
                description: jsonString(''),
                typeId: 'console:singleLineTextControl',
                isOfType: { id: 'console:singleLineTextControl' },
                'k:fieldToRender': {
                    name: 'new field',
                    description: 'new field description',
                    typeId: 'stringField',
                    isOfType: {
                        name: 'String Field',
                        id: 'stringField',
                        alias: 'core:stringField'
                    }
                }
            });

            spEntity.augment(formControl, null, templateFn);
            expect(formControl.fieldToRender).toBeTruthy();
            expect(formControl.fieldToRender.minLength).toBe(0);
        });

    });

    describe('serialization', function () {

        it('can serialize/deserialize nulls', function () {
            var json = spEntity.serialize(null);
            var e = spEntity.deserialize(json);
            expect(e).toBeNull();
        });

        it('can serialize/deserialize empty arrays', function () {
            var json = spEntity.serialize([]);
            var e = spEntity.deserialize(json);
            expect(e).toBeArray(0);
        });

        it('can serialize/deserialize single instance arrays', function () {
            var json = { myfield: 'Hello' };
            var eTmp = spEntity.fromJSON(json);
            var json2 = spEntity.serialize([eTmp]);
            var e = spEntity.deserialize(json2);
            expect(e[0].myfield).toBe('Hello');
        });

        it('can serialize/deserialize a simple entity', function () {
            var json = {
                name: 'Scott'
            };
            var eTmp = spEntity.fromJSON(json);
            var json2 = spEntity.serialize(eTmp);
            var e = spEntity.deserialize(json2);
            expect(e.getName()).toBe('Scott');
        });

        it('can serialize/deserialize an alias', function () {
            var json = {
                alias: 'hello:world'
            };
            var eTmp = spEntity.fromJSON(json);
            var json2 = spEntity.serialize(eTmp);
            var e = spEntity.deserialize(json2);
            expect(e.eidP.nsAlias).toBe('hello:world');
        });

        it('can serialize/deserialize a type', function () {
            var json = {
                typeId: 'test:person'
            };
            var eTmp = spEntity.fromJSON(json);
            var json2 = spEntity.serialize(eTmp);
            var e = spEntity.deserialize(json2);
            expect(e.getType().getNsAlias()).toBe('test:person');
        });

        it('can serialize/deserialize connected graphs', function () {
            var json = {
                myLookup1: jsonLookup(123),
                myLookup2: { id: 123, name: 'abc' },
                myLookup3: jsonLookup(123)
            };
            var eTmp = spEntity.fromJSON(json);
            var json2 = spEntity.serialize(eTmp);
            var e = spEntity.deserialize(json2);
            
            expect(e.getMyLookup1()).toBe(e.getMyLookup2());
            expect(e.getMyLookup2()).toBe(e.getMyLookup3());
            expect(e.getMyLookup2().getName()).toBe('abc');
        });

        it('can serialize/deserialize connected graphs without delete column', function () {
            var json = {
                myLookup1: { id: 123, name: 'abc' },
                myLookup2: { id: 456, name: 'edf' }
            };
            var eTmp = spEntity.fromJSON(json);
            var json2 = spEntity.serialize(eTmp);
            var e = spEntity.deserialize(json2);

            //set myLookup1's instance state is delete
            eTmp._relationships[0].instances[0].setDataState(spEntity.DataStateEnum.Delete);
            e = eTmp.cloneDeep({ includeDeleted: false });
            expect(e.getMyLookup1()).toBe(null);
            expect(e.getMyLookup2()).toNotBe(null);
        });

        it('can serialize/deserialize complex graphs', function () {
            var json = {
                id: 123,                 // optional: number or alias or absent of resource id
                typeId: 'test:person',   // optional: number or alias or absent of typeid
                firstName: 'Peter',       // string values get registered as string fields
                daysSinceLogin: 3,        // number values get registered as Int32 fields
                yesNo: true,              // string values get registered as string fields
                manager: {                // non-array objects get registered as lookups
                    id: 'judeJacobs'      // aliases can be used for IDs
                },
                monitors: [               // arrays get registered as to-many relationships
                    { name: 'Acer' },     // embedded entities can also be JSON
                    spEntity.fromId(456) // or they can be pre-existing entities, wherever you got them from
                ],
                'console:toolTip': 'abc'  // fully qualified aliases can also be used, if surrounded in quotes.
            };

            var eTmp = spEntity.fromJSON(json);
            var json2 = spEntity.serialize(eTmp);
            var e = spEntity.deserialize(json2);

            expect(e.id()).toBe(123);
            expect(e.getType().getNsAlias()).toBe('test:person');
            expect(e.getFirstName()).toBe('Peter');
            expect(e.getDaysSinceLogin()).toBe(3);
            expect(e.getYesNo()).toBe(true);
            expect(e.getManager().eid().getAlias()).toBe('judeJacobs');
            expect(e.getMonitors()[0].getName()).toBe('Acer');
            expect(e.getMonitors()[1].id()).toBe(456);
            expect(e.getField('console:toolTip')).toBe('abc');
        });
    });

});