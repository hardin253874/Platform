// Copyright 2011-2016 Global Software Innovation Pty Ltd
/* globals spEntity, jsonString, jsonInt, jsonLookup, jsonRelationship, jsonDecimal, jsonBool,
 jsonCurrency, jsonDate, jsonDateTime, jsonTime, entityTestData */

describe('Entity Model|spEntity|spec:', function () {
    'use strict';

    function getEntityDataField(entityData, fieldId) {
        // extract value out of the entityData network packet (keep this in test only)
        var eid = entityData.ids[0];
        var entity = _.find(entityData.entities, { id: eid });
        var field = _.find(entity.fields, { fieldId: fieldId });
        if (!field)
            return undefined;
        var res = field.value;
        return res;
    }

    beforeEach(function () {
        this.addMatchers(TestSupport.matchers);
    });


    describe('spEntity', function () {
        it('should exist', function () {
            expect(spEntity).toBeTruthy();
        });
    });


    describe('Entity', function () {

        it('can get/set name without registration', function () {
            var e = spEntity.createEntityOfType(123);
            expect(e.setName('Hello')).toEqual(e);
            expect(e.getName()).toBe('Hello');
            expect(e._dataState).toEqual(spEntity.DataStateEnum.Create);
        });

        it('has zero typeId if none was specified', function () {
            var e = new spEntity._Entity();
            expect(_.isArray(e.getTypes())).toBeTruthy();
            expect(e.firstTypeId().id()).toEqual(0);
        });

        it('can be created given a typeId number', function () {
            var e = new spEntity._Entity({ typeId: 123 });
            expect(e).toBeTruthy();
            expect(e.firstTypeId().id()).toEqual(123);
            expect(e._dataState).toEqual(spEntity.DataStateEnum.Create);  // do we want this behavior?? (it may change)
        });

        it('can be created given a typeId alias', function () {
            var e = new spEntity._Entity({ typeId: 'resource' });
            expect(e).toBeTruthy();
            expect(e.firstTypeId().alias()).toEqual('core:resource');
            expect(e._dataState).toEqual(spEntity.DataStateEnum.Create);  // do we want this?? (it may change)
        });

        it('can be created given a typeId ns:alias', function () {
            var e = new spEntity._Entity({ typeId: 'core:resource' });
            expect(e).toBeTruthy();
            expect(e.firstTypeId().alias()).toEqual('core:resource');
            expect(e._dataState).toEqual(spEntity.DataStateEnum.Create);  // do we want this?? (it may change)
        });
    });


    describe('.entityDataToEntities', function () {

        // Introduction Tests

        it('should exist', function () {
            expect(spEntity.entityDataToEntities).toBeTruthy();
        });
    });


    describe('fromId function', function () {

        it('should exist', function () {
            expect(spEntity.fromId).toBeTruthy();
        });

        it('should have ID set', function () {
            var e = spEntity.fromId(123);
            expect(e.eid().getId()).toBe(123);
            expect(e.eid().getNsAlias()).toBeNull();
        });

        it('should have a zero typeId', function () {
            var e = spEntity.fromId(123);
            expect(e.getType().getId()).toBe(0);
        });

        it('should have alias set', function () {
            var e = spEntity.fromId('myAlias');
            expect(e.eid().getId()).toBe(0);
            expect(e.eid().getNsAlias()).toBe('core:myAlias');
        });

        it('should be flagged as unchanged', function () {
            var e = spEntity.fromId(123);
            expect(e.getDataState()).toBe(spEntity.DataStateEnum.Unchanged);
        });
    });


    describe('createEntityOfType function', function () {

        it('should exist', function () {
            expect(spEntity.createEntityOfType).toBeTruthy();
        });

        it('should return an entity', function () {
            var e = spEntity.createEntityOfType(123);
            expect(e).toBeEntity();
        });

        it('should have a generated id', function () {
            var e = spEntity.createEntityOfType(123);
            expect(e.id()).toBeGreaterThan(0);
        });

        it('should have type ID set', function () {
            var e = spEntity.createEntityOfType(123);
            expect(e.getType().getId()).toBe(123);
        });

        it('should have type alias set', function () {
            var e = spEntity.createEntityOfType('myType');
            expect(e.getType().getNsAlias()).toBe('core:myType');
        });

        it('should be flagged as create', function () {
            var e = spEntity.createEntityOfType(123);
            expect(e._dataState).toBe(spEntity.DataStateEnum.Create);
        });

        it('should be assigned a dummy ID', function () {
            var e = spEntity.createEntityOfType(123);
            expect(e.eid().getId()).toBeGreaterThan(0);
        });

        it('should set name if provided', function () {
            var e = spEntity.createEntityOfType(123, 'blah');
            expect(e.eid().getId()).toBeGreaterThan(0);
            expect(e.getName()).toBe('blah');
        });

        it('should set description if provided', function () {
            var e = spEntity.createEntityOfType(123, 'blah', 'mydesc');
            expect(e.eid().getId()).toBeGreaterThan(0);
            expect(e.getName()).toBe('blah');
            expect(e.getDescription()).toBe('mydesc');
        });
    });


    describe('spEntity.DataType', function () {

        it('should exist', function () {
            expect(spEntity.DataType).toBeTruthy();
            expect(spEntity.DataType.None).toBe('None');
        });
    });


    describe('isEntity', function () {

        it('should work', function () {
            var entity = spEntity.createEntityOfType(123);
            expect(spEntity.isEntity(entity)).toEqual(true);
            expect(spEntity.isEntity(123)).toEqual(false);
            expect(spEntity.isEntity(null)).toEqual(false);
            expect(spEntity.isEntity()).toEqual(false);
        });
    });


    describe('entitiesToEntityData', function () {

        it('should return all fields if changesOnly is false', function () {
            var testEntity = spEntity.entityDataVer2ToEntities(entityTestData.af02)[0];
            var testEntityData2 = spEntity.entitiesToEntityData(testEntity, { changesOnly: false });
            expect(testEntityData2).toBeTruthy(false);
            var field = getEntityDataField(testEntityData2, 7765);
            expect(field).toEqual('Test 02');
        });

        it('should return changed fields if changesOnly is true', function () {
            var testEntity = spEntity.entityDataVer2ToEntities(entityTestData.af02)[0];
            testEntity.name = 'Blah';
            var testEntityData2 = spEntity.entitiesToEntityData(testEntity, { changesOnly: false });
            expect(testEntityData2).toBeTruthy(false);
            var field = getEntityDataField(testEntityData2, 7765);
            expect(field).toEqual('Blah');
        });

        it('should not return unchanged fields if changesOnly is true', function () {
            var testEntity = spEntity.entityDataVer2ToEntities(entityTestData.af02)[0];
            var testEntityData2 = spEntity.entitiesToEntityData(testEntity, { changesOnly: true });
            expect(testEntityData2).toBeTruthy(false);
            var field = getEntityDataField(testEntityData2, 7765);
            expect(field).toBeUndefined();
        });

        it('should not return fields set to initial value if changesOnly is true', function () {
            var testEntity = spEntity.entityDataVer2ToEntities(entityTestData.af02)[0];
            testEntity.name = 'Test 02';
            var testEntityData2 = spEntity.entitiesToEntityData(testEntity, { changesOnly: true });
            expect(testEntityData2).toBeTruthy(false);
            var field = getEntityDataField(testEntityData2, 7765);
            expect(field).toBeUndefined();
        });

        it('should not return fields reverted to initial value if changesOnly is true', function () {
            var testEntity = spEntity.entityDataVer2ToEntities(entityTestData.af02)[0];
            testEntity.name = 'Blah';
            testEntity.name = 'Test 02';
            var testEntityData2 = spEntity.entitiesToEntityData(testEntity, { changesOnly: true });
            expect(testEntityData2).toBeTruthy(false);
            var field = getEntityDataField(testEntityData2, 7765);
            expect(field).toBeUndefined();
        });

        it('when cloned should return changed fields if changesOnly is true', function () {
            var testEntity = spEntity.entityDataVer2ToEntities(entityTestData.af02)[0];
            testEntity.name = 'Blah';
            var testEntityData2 = spEntity.entitiesToEntityData(testEntity.cloneDeep(), { changesOnly: false });
            expect(testEntityData2).toBeTruthy(false);
            var field = getEntityDataField(testEntityData2, 7765);
            expect(field).toEqual('Blah');
        });

        it('when cloned should not return unchanged fields if changesOnly is true', function () {
            var testEntity = spEntity.entityDataVer2ToEntities(entityTestData.af02)[0];
            var testEntityData2 = spEntity.entitiesToEntityData(testEntity.cloneDeep(), { changesOnly: true });
            expect(testEntityData2).toBeTruthy(false);
            var field = getEntityDataField(testEntityData2, 7765);
            expect(field).toBeUndefined();
        });

        it('when cloned should not return fields set to initial value if changesOnly is true', function () {
            var testEntity = spEntity.entityDataVer2ToEntities(entityTestData.af02)[0];
            testEntity.name = 'Test 02';
            var testEntityData2 = spEntity.entitiesToEntityData(testEntity.cloneDeep(), { changesOnly: true });
            expect(testEntityData2).toBeTruthy(false);
            var field = getEntityDataField(testEntityData2, 7765);
            expect(field).toBeUndefined();
        });

        it('when cloned should not return fields reverted to initial value if changesOnly is true', function () {
            var testEntity = spEntity.entityDataVer2ToEntities(entityTestData.af02)[0];
            testEntity.name = 'Blah';
            testEntity.name = 'Test 02';
            var testEntityData2 = spEntity.entitiesToEntityData(testEntity.cloneDeep(), { changesOnly: true });
            expect(testEntityData2).toBeTruthy(false);
            var field = getEntityDataField(testEntityData2, 7765);
            expect(field).toBeUndefined();
        });
    });


    describe('hasChangesRecursive', function () {

        it('can detect changes', function () {
            var e = spEntity.fromJSON({
                id: 123,
                myfield: 'value',
                myrel: { something: 123 }
            });

            expect(e.hasChangesRecursive()).toBeTruthy();
        });

        it('can detect zero changes', function () {
            var e = spEntity.fromJSON({
                id: 123,
                myfield: 'value',
                myrel: { something:123 }
            }).markAllUnchanged();

            expect(e.hasChangesRecursive()).toBeFalsy();
        });
    });


    describe('findInGraph', function () {

        it('should find by alias', function () {
            var e = spEntity.fromJSON(
                {
                    fieldOverridesForType: [{ id: 'o1' }]
                });
            var required = e.getFieldOverridesForType()[0];
            var actual = spEntity.findInGraph(e, 'o1');
            expect(actual).toBe(required);
        });

        it('should find by ID', function () {
            var e = spEntity.fromJSON(
                {
                    fieldOverridesForType: [ { id: 123 }]
                });
            var required = e.getFieldOverridesForType()[0];
            var actual = spEntity.findInGraph(e, 123);
            expect(actual).toBe(required);
        });
    });


    describe('cloneDeep', function () {

        it('can clone connected graphs', function () {
            var json = {
                myLookup1: jsonLookup(123),
                myLookup2: { id: 123, name: 'abc' },
                myLookup3: jsonLookup(123)
            };
            var eTmp = spEntity.fromJSON(json);
            var e = eTmp.cloneDeep();
            expect(e.getMyLookup1()).toBe(e.getMyLookup2());
            expect(e.getMyLookup2()).toBe(e.getMyLookup3());
            expect(e.getMyLookup2().getName()).toBe('abc');
        });

        it('can clone connected graphs without delete column', function () {
            var json = {
                myLookup1: { id: 123, name: 'abc' },
                myLookup2: { id: 456, name: 'edf' }
            };
            var eTmp = spEntity.fromJSON(json);

            //set myLookup1's instance state is delete
            eTmp._relationships[0].instances[0].setDataState(spEntity.DataStateEnum.Delete);
            var e = eTmp.cloneDeep({ includeDeleted: false });
            expect(e.getMyLookup1()).toBe(null);
            expect(e.getMyLookup2()).toNotBe(null);
        });

        it('can clone complex graphs', function () {
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
            var e = eTmp.cloneDeep();

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

        it('can clone graph with multiple instances of same entity', function () {            
            var json = {
                myLookup1: {
                    id: 123,
                    lookupX: {
                        id: 999,
                        name: 'ABC'
                    }
                },
                myLookup2: {
                    id: 567,
                    lookupX: {
                        id: 999,
                        description: 'ABC description'
                    }
                }               
            };

            // The original entity has two distinct entities for the lookupX relationship
            // whereas the clone has once instance with merged fields.
            var eTmp = spEntity.fromJSON(json);
            var e = eTmp.cloneDeep();
            expect(e.getMyLookup1().getLookupX()).toBe(e.getMyLookup2().getLookupX());

            expect(e.getMyLookup1().getLookupX().getName()).toBe('ABC');
            expect(e.getMyLookup1().getLookupX().getDescription()).toBe('ABC description');

            expect(e.getMyLookup2().getLookupX().getName()).toBe('ABC');
            expect(e.getMyLookup2().getLookupX().getDescription()).toBe('ABC description');            
        });
    });

});
