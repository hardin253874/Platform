// Copyright 2011-2016 Global Software Innovation Pty Ltd
/* globals spEntity, spResource, jsonString, jsonInt, jsonLookup, jsonRelationship, jsonDecimal, jsonBool,
 jsonCurrency, jsonDate, jsonDateTime, jsonTime */

describe('Entity Model|spResource|spec:|Field', function () {

    beforeEach(module('ng'));
    beforeEach(module('mod.common.spResource'));

    beforeEach(function () {
        this.addMatchers(TestSupport.matchers);
    });

    describe('getName', function () {
        it('returns the field name', function () {
            var typeEntity = spEntity.fromJSON({ fields: [{ name: 'abc' }] });
            var type = new spResource.Type(typeEntity);
            var field = type.getFields()[0];
            expect(field.getName()).toBe('abc');
        });

        it('respects a simple field override', function () {
            var jField = spEntity.fromJSON({ name: 'f1', fieldOverriddenBy: jsonLookup() });
            var jOverride = spEntity.fromJSON({ name: 'o1', fieldOverrides: jField, fieldOverrideForType: jsonLookup() });
            var typeEntity = spEntity.fromJSON(
                {
                    inherits: [{
                        fields: [jField],
                    }],
                    fieldOverridesForType: [jOverride]
                });
            jField.setFieldOverriddenBy(jOverride);
            jOverride.setFieldOverrideForType(typeEntity);

            var type = new spResource.Type(typeEntity);
            var field = type.getFields()[0];
            expect(field.getName()).toBe('o1');
        });

        it('respects a twice overridden field', function () {
            var jField = spEntity.fromJSON({ name: 'f1', fieldOverriddenBy: jsonLookup() });
            var jOverride = spEntity.fromJSON({ name: 'o1', fieldOverrides: jField, fieldOverrideForType: jsonLookup() });
            var jOverride2 = spEntity.fromJSON({ name: 'o2', fieldOverrides: jField, fieldOverrideForType: jsonLookup() });
            var typeEntity = spEntity.fromJSON(
                {
                    inherits: [{
                        inherits: [{
                            fields: [jField],
                        }],
                        fieldOverridesForType: [jOverride]
                    }],
                    fieldOverridesForType: [jOverride2]
                });
            jField.setFieldOverriddenBy([jOverride, jOverride2]);
            jOverride.setFieldOverrideForType(typeEntity.getInherits()[0]);
            jOverride2.setFieldOverrideForType(typeEntity);

            var type = new spResource.Type(typeEntity);
            var field = type.getFields()[0];
            expect(field.getName()).toBe('o2');
        });
    });

    describe('getDescription', function () {
        it('returns the field description', function () {
            var typeEntity = spEntity.fromJSON({ fields: [{ description: 'abc' }] });
            var type = new spResource.Type(typeEntity);
            var field = type.getFields()[0];
            expect(field.getDescription()).toBe('abc');
        });
    });

    describe('getScriptName', function () {
        it('returns the field script name if specified', function () {
            var typeEntity = spEntity.fromJSON({ fields: [{ name: 'myname', fieldScriptName: 'myscriptname' }] });
            var type = new spResource.Type(typeEntity);
            var field = type.getFields()[0];
            expect(field.getScriptName()).toBe('myscriptname');
        });

        it('returns the field name if there is no script name', function () {
            var typeEntity = spEntity.fromJSON({ fields: [{ name: 'myname' }] });
            var type = new spResource.Type(typeEntity);
            var field = type.getFields()[0];
            expect(field.getScriptName()).toBe('myname');
        });
    });

    describe('getEntity', function () {
        it('returns the field entity', function () {
            var fEntity = spEntity.fromJSON({ name: 'f1' });
            var typeEntity = spEntity.fromJSON({ fields: [fEntity] });
            var type = new spResource.Type(typeEntity);
            var field = type.getFields()[0];
            expect(field.getEntity()).toBe(fEntity);
        });
    });

    describe('isHidden', function () {
        it('returns true if the field is hidden', function () {
            var typeEntity = spEntity.fromJSON({ fields: [{ name: 'f1', hideField: true }] });
            var type = new spResource.Type(typeEntity);
            var field = type.getFields({ showHidden: true })[0];
            expect(field.isHidden()).toBe(true);
        });

        it('returns false if the hidden status is unknown', function () {
            var typeEntity = spEntity.fromJSON({ fields: [{ name: 'f1' }] });
            var type = new spResource.Type(typeEntity);
            var field = type.getFields()[0];
            expect(field.isHidden()).toBe(false);
        });
    });

    describe('getFieldOverrides', function () {
        it('returns empty array if there are no overrides', function () {
            var typeEntity = spEntity.fromJSON({ fields: [{ name: 'f1' }] });
            var type = new spResource.Type(typeEntity);
            var field = type.getFields()[0];
            var fo = field.getFieldOverrides();
            expect(fo).toBeEmptyArray();
        });

        it('returns a simple override on a derived type', function () {
            var typeEntity = spEntity.fromJSON(
                {
                    id:'t1',
                    inherits: [{
                        fields: [{ id: 'f1', fieldOverriddenBy: ['o1'] }],
                    }],
                    fieldOverridesForType: [{ id: 'o1', fieldOverrides: jsonLookup('f1'), fieldOverrideForType: jsonLookup('t1') }]
                });
            var jOverride = spEntity.findInGraph(typeEntity, 'o1');

            var type = new spResource.Type(typeEntity);
            var field = type.getFields()[0];
            var fo = field.getFieldOverrides();
            expect(fo).toBeArray(1);
            expect(fo[0]).toBe(jOverride);
        });

        it('returns twice overridden field', function () {
            var typeEntity = spEntity.fromJSON(
                {
                    id: 't1',
                    inherits: [{
                        id: 't2',
                        inherits: [{
                            id: 't3',
                            fields: [{ id: 'f1', fieldOverriddenBy: ['o1', 'o2'] }],
                        }],
                        fieldOverridesForType: [{ id: 'o1', fieldOverrides: jsonLookup('f1'), fieldOverrideForType: jsonLookup('t2') }]
                    }],
                    fieldOverridesForType: [{ id: 'o2', fieldOverrides: jsonLookup('f1'), fieldOverrideForType: jsonLookup('t1') }]
                });
            var jOverride = spEntity.findInGraph(typeEntity, 'o1');
            var jOverride2 = spEntity.findInGraph(typeEntity, 'o2');

            var type = new spResource.Type(typeEntity);
            var field = type.getFields()[0];
            var fo = field.getFieldOverrides();
            expect(fo).toBeArray(2);
            expect(fo[0]).toBe(jOverride2);
            expect(fo[1]).toBe(jOverride);
        });

    });

    describe('getFieldGroupEntity', function () {
        it('can be overridden', function () {
            var typeEntity = spEntity.fromJSON(
                {
                    id: 't1',
                    fieldGroups: [{ id: 'fg1' },{ id: 'fg2' }],
                    inherits: [{
                        fields: [{ id: 'f1', fieldOverriddenBy: ['o1'], fieldInGroup: jsonLookup('fg1') }],
                    }],
                    fieldOverridesForType: [{ id: 'o1', fieldOverrides: jsonLookup('f1'), fieldOverrideForType: jsonLookup('t1'), fieldInGroup: jsonLookup('fg2') }]
                });
            var o1 = spEntity.findInGraph(typeEntity, 'o1');
            var fg2 = spEntity.findInGraph(typeEntity, 'fg2');

            var type = new spResource.Type(typeEntity);
            var field = type.getFields()[0];
            var fg = field.getFieldGroupEntity();
            expect(fg).toBe(fg2);
        });
    });

});
