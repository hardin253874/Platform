// Copyright 2011-2016 Global Software Innovation Pty Ltd
/* globals spEntity, spResource */

describe('Entity Model|spResource|spec:|Type', function () {

    var manyToOne = spEntity.fromId('core:manyToOne');

    beforeEach(module('ng'));
    beforeEach(module('mod.common.spResource'));

    beforeEach(function () {
        this.addMatchers(TestSupport.matchers);
    });

    describe('constructor', function () {

        it('should work on empty input', inject(function (spResource) {
            expect(new spResource.Type(null)).toBeTruthy();
        }));

        it('should work on a single type', inject(function (spResource) {
            var typeEntity = spEntity.fromId(123);
            var type = new spResource.Type(typeEntity);
            expect(type.getEntity()).toBe(typeEntity);
        }));

        it('should work on a multiple types', inject(function (spResource) {
            var types = [spEntity.fromId(123), spEntity.fromId(123)];
            var type = new spResource.Type(types);
            expect(type.getEntities()).toBe(types);
        }));
    });

    describe('getName', function () {
        it('returns the type name', function () {
            var typeEntity = spEntity.fromJSON({ name: 'abc' });
            var type = new spResource.Type(typeEntity);
            expect(type.getName()).toBe('abc');
        });
    });

    describe('getDescription', function () {
        it('returns the type description', function () {
            var typeEntity = spEntity.fromJSON({ description: 'abc' });
            var type = new spResource.Type(typeEntity);
            expect(type.getDescription()).toBe('abc');
        });
    });

    describe('getEntity', function () {
        it('returns the type entity', function () {
            var typeEntity = spEntity.fromId(123);
            var type = new spResource.Type(typeEntity);
            expect(type.getEntity()).toBe(typeEntity);
        });
    });

    describe('getEntities', function () {
        it('returns the original array of entities', function () {
            var types = [spEntity.fromId(123), spEntity.fromId(123)];
            var type = new spResource.Type(types);
            expect(type.getEntities()).toBe(types);
        });
    });


    describe('getAllEntities', function () {
        it('should work on empty input', inject(function (spResource) {
            var type = new spResource.Type(null);
            expect(type.getAllEntities()).toBeEmptyArray();
        }));

        it('should return inherited entities', inject(function (spResource) {
            var parentEntity = spEntity.fromId(123);
            var typeEntity = spEntity.fromJSON({
                inherits: [parentEntity]
            });
            var type = new spResource.Type(typeEntity);
            expect(type.getAllEntities()).toBeArray(2);
            expect(type.getAllEntities()[0]).toBe(typeEntity);
            expect(type.getAllEntities()[1]).toBe(parentEntity);
        }));

        it('should return inherited entities for multiple types', inject(function (spResource) {
            var parentEntity = spEntity.fromId(123);
            var typeEntity = spEntity.fromJSON({
                inherits: [parentEntity]
            });
            var type = new spResource.Type([parentEntity,typeEntity]);
            expect(type.getAllEntities()).toBeArray(2);
            expect(type.getAllEntities()[0]).toBe(typeEntity);
            expect(type.getAllEntities()[1]).toBe(parentEntity);
        }));
    });


    describe('getFields', function () {

        it('should work if nothing provided', inject(function (spResource) {
            var type = new spResource.Type(null);
            expect(type.getFields()).toBeEmptyArray();
        }));

        it('return basic fields', inject(function (spResource) {
            var ent = spEntity.fromJSON({
                fields: [
                    { name: 'First name' },
                    { name: 'Last name' }
                ]
            });

            var type = new spResource.Type(ent);
            var fields = type.getFields();
            expect(fields).toBeArray(2);
            expect(fields[0].getName()).toBe('First name');
            expect(fields[1].getName()).toBe('Last name');
        }));

        it('return inherited fields', inject(function (spResource) {
            var ent = spEntity.fromJSON({
                fields: [
                    { name: 'Employee number' }
                ],
                inherits: [
                    {
                        fields: [
                            { name: 'First name' },
                            { name: 'Last name' }
                        ]
                    }
                ]
            });

            var type = new spResource.Type(ent);
            var fields = type.getFields();
            expect(fields).toBeArray(3);
            expect(fields[0].getName()).toBe('Employee number');
            expect(fields[1].getName()).toBe('First name');
            expect(fields[2].getName()).toBe('Last name');
        }));

        it('excludes hidden fields by default', inject(function (spResource) {
            var ent = spEntity.fromJSON({
                fields: [
                    { name: 'First name' },
                    { name: 'Last name', hideField: true }
                ]
            });

            var type = new spResource.Type(ent);
            var fields = type.getFields();
            expect(fields).toBeArray(1);
            expect(fields[0].getName()).toBe('First name');
        }));

        it('shows hidden fields if requested', inject(function (spResource) {
            var ent = spEntity.fromJSON({
                fields: [
                    { name: 'First name' },
                    { name: 'Last name', hideField: true }
                ]
            });

            var type = new spResource.Type(ent);
            var fields = type.getFields({ showHidden: true });
            expect(fields).toBeArray(2);
        }));

        it('includes calculated by default', inject(function (spResource) {
            var ent = spEntity.fromJSON({
                fields: [
                    { name: 'First name' },
                    { name: 'Last name', isCalculatedField: true }
                ]
            });

            var type = new spResource.Type(ent);
            var fields = type.getFields();
            expect(fields).toBeArray(2);
            expect(fields[0].getName()).toBe('First name');
            expect(fields[1].getName()).toBe('Last name');
        }));

        it('includes autonumber fields by default', inject(function (spResource) {
            var ent = spEntity.fromJSON({
                fields: [
                    { name: 'First name' },
                    { name: 'PersonID', typeId: 'core:autoNumberField' }
                ]
            });

            var type = new spResource.Type(ent);
            var fields = type.getFields();
            expect(fields).toBeArray(2);
            expect(fields[0].getName()).toBe('First name');
            expect(fields[1].getName()).toBe('PersonID');
        }));

        it('excludes calculated fields when hiding non-writables', inject(function (spResource) {
            var ent = spEntity.fromJSON({
                fields: [
                    { name: 'First name' },
                    { name: 'Last name', isCalculatedField: true }
                ]
            });

            var type = new spResource.Type(ent);
            var fields = type.getFields({ hideNonWritable: true });
            expect(fields).toBeArray(1);
            expect(fields[0].getName()).toBe('First name');
        }));

        it('excludes autonumber fields when hiding non-writables', inject(function (spResource) {
            var ent = spEntity.fromJSON({
                fields: [
                    { name: 'First name' },
                    { name: 'PersonID', typeId: 'core:autoNumberField' }
                ]
            });

            var type = new spResource.Type(ent);
            var fields = type.getFields({ hideNonWritable: true });
            expect(fields).toBeArray(1);
            expect(fields[0].getName()).toBe('First name');
        }));
    });

    describe('getFieldGroups', function () {

        it('should work if nothing provided', inject(function (spResource) {
            var type = new spResource.Type(null);
            expect(type.getFieldGroups()).toBeEmptyArray();
        }));

        it('should get inherited field groups', inject(function (spResource) {
            var ent = spEntity.fromJSON({
                fieldGroups: [ { name: 'FG1' } ],
                inherits: [
                    {
                        fieldGroups: [{ name: 'FG2' }],
                        inherits: [ { fieldGroups: [{ name: 'FG3' }] } ]
                    },
                    { fieldGroups: [{ name: 'FG4' }] }
                ]
            });

            var type = new spResource.Type(ent);
            var fg = type.getFieldGroups();
            expect(fg).toBeArray(4);
        }));

        it('should field groups from members', inject(function (spResource) {

            var ent = spEntity.fromJSON({
                fields: [{ name: 'F1', fieldInGroup: { name: 'FG1' } }],
                relationships: [
                    {
                        name: 'R1', cardinality: manyToOne,
                        relationshipInFromTypeGroup: { name: 'FG2' }
                    }],
                reverseRelationships: [
                    {
                        name: 'RR1', cardinality: manyToOne,
                        relationshipInToTypeGroup: { name: 'FG3' }
                    }],
            });

            var type = new spResource.Type(ent);
            var fg = type.getFieldGroups();
            expect(fg).toBeArray(3);
        }));

        it('should manufacture a single \'Default\' group', inject(function (spResource) {

            var ent = spEntity.fromJSON({
                fields: [{ name: 'F1' }, { name: 'F2' }],
                relationships: [
                    { name: 'R1', cardinality: manyToOne },
                    { name: 'R2', cardinality: manyToOne }],
                reverseRelationships: [
                    { name: 'RR1', cardinality: manyToOne },
                    { name: 'RR2', cardinality: manyToOne }]
            });

            var type = new spResource.Type(ent);
            var fg = type.getFieldGroups();
            expect(fg).toBeArray(1);
            expect(fg[0].getName()).toBe('Default');
            expect(fg[0].getDescription()).toBeTruthy();
        }));

        it('should share \'Default\' group instance for implicit and explicit members', inject(function (spResource) {

            var ent = spEntity.fromJSON({
                fields: [{ name: 'F1', fieldInGroup: { id: 'core:default', name:'Default' } },{ name: 'F2' }]
            });

            var type = new spResource.Type(ent);
            var fg = type.getFieldGroups();
            expect(fg).toBeArray(1);
            expect(fg[0].getName()).toBe('Default');
        }));
    });

    describe('getAllRelationships', function () {

        it('should work if nothing provided', inject(function (spResource) {
            var type = new spResource.Type(null);
            expect(type.getAllRelationships()).toBeEmptyArray();
        }));

        it('return rels in both directions', inject(function (spResource) {
            var ent = spEntity.fromJSON({
                relationships: [
                    { name: 'A', cardinality: manyToOne },
                    { name: 'C', cardinality: manyToOne }],
                reverseRelationships: [
                    { name: 'B', cardinality: manyToOne },
                    { name: 'D', cardinality: manyToOne }]
            });

            var type = new spResource.Type(ent);
            var fields = type.getAllRelationships();
            expect(fields).toBeArray(4);
            expect(fields[0].getName()).toBe('A');
            expect(fields[1].getName()).toBe('B');
            expect(fields[2].getName()).toBe('C');
            expect(fields[3].getName()).toBe('D');
        }));

        it('return inherited rels', inject(function (spResource) {
            var ent = spEntity.fromJSON({
                relationships: [
                    { name: 'A', cardinality: manyToOne }],
                reverseRelationships: [
                    { name: 'C', cardinality: manyToOne }],
                inherits: [
                    {
                        relationships: [
                            { name: 'B', cardinality: manyToOne }],
                        reverseRelationships: [
                            { name: 'D', cardinality: manyToOne }]
                    }
                ]
            });

            var type = new spResource.Type(ent);
            var fields = type.getAllRelationships();
            expect(fields).toBeArray(4);
            expect(fields[0].getName()).toBe('A');
            expect(fields[1].getName()).toBe('B');
            expect(fields[2].getName()).toBe('C');
            expect(fields[3].getName()).toBe('D');
        }));
    });

});
