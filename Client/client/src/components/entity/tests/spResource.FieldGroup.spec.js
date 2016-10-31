// Copyright 2011-2016 Global Software Innovation Pty Ltd
/* globals spEntity, spResource, jsonString, jsonInt, jsonLookup, jsonRelationship, jsonDecimal, jsonBool,
 jsonCurrency, jsonDate, jsonDateTime, jsonTime */

describe('Entity Model|spResource|spec:|FieldGroup', function () {

    var oneToOne = spEntity.fromId('oneToOne');
    var oneToMany = spEntity.fromId('oneToMany');
    var manyToOne = spEntity.fromId('manyToOne');
    var manyToMany = spEntity.fromId('manyToMany');

    beforeEach(module('ng'));
    beforeEach(module('mod.common.spResource'));

    beforeEach(function () {
        this.addMatchers(TestSupport.matchers);
    });

    describe('getName and getDescription', function () {

        it('returns for a normal field group', function () {
            var type = new spResource.Type(spEntity.fromJSON({
                fieldGroups: [{ name: 'FG1', description: 'Desc' }]
            }));
            var fg = type.getFieldGroups()[0];
            expect(fg.getName()).toBe('FG1');
            expect(fg.getDescription()).toBe('Desc');
        });

        it('returns \'Default\' for the ungrouped group', function () {
            var type = new spResource.Type(spEntity.fromJSON({
                fields: [{ name: 'Field1' }]
            }));
            var fg = type.getFieldGroups()[0];
            expect(fg.getName()).toBe('Default');
            expect(fg.getDescription()).toBeTruthy();
        });

    });

    describe('getEntity', function () {

        it('returns the entity', function () {
            var fgEntity = spEntity.fromId(123);
            var type = new spResource.Type(spEntity.fromJSON({
                fieldGroups: [fgEntity]
            }));
            var fg = type.getFieldGroups()[0];
            expect(fg.getEntity()).toBe(fgEntity);
        });

        it('returns mock entity for the ungrouped group', function () {
            var type = new spResource.Type(spEntity.fromJSON({
                fields: [{ name: 'Field1' }]
            }));
            var fg = type.getFieldGroups()[0];
            expect(fg.getEntity().eid().getNsAlias()).toBe('core:default');
        });

    });

    describe('getFields', function () {

        it('returns fields in the current group', function () {
            var fgEntity = spEntity.fromId(123);
            fgEntity.name = 'AAA Ardvaark'; // sorts in front of the default
            var type = new spResource.Type(spEntity.fromJSON({
                fieldGroups: [fgEntity],
                fields: [
                    { name: 'F1', fieldInGroup: fgEntity },
                    { name: 'F2', fieldInGroup: fgEntity },
                    { name: 'F3' }]
            }));

            // the default group is included
            expect(type.getFieldGroups()).toBeArray(2);

            var fg = type.getFieldGroups()[0];
            var fields = fg.getFields();
            expect(fields).toBeArray(2);
            expect(fields[0].getName()).toBe('F1');
            expect(fields[1].getName()).toBe('F2');
        });

        it('returns unassigned fields in the default group', function () {
            var fgEntity = spEntity.fromId(123);
            fgEntity.name = 'AAA Ardvaark'; // sorts in front of the default

            var type = new spResource.Type(spEntity.fromJSON({
                fieldGroups: [fgEntity],
                fields: [
                    { name: 'F1', fieldInGroup: fgEntity },
                    { name: 'F2' },
                    { name: 'F3' }]
            }));
            // the default group is included
            expect(type.getFieldGroups()).toBeArray(2);

            var fg = type.getFieldGroups()[1];
            var fields = fg.getFields();
            expect(fields).toBeArray(2);
            expect(fields[0].getName()).toBe('F2');
            expect(fields[1].getName()).toBe('F3');
        });

    });

    describe('relationships', function () {

        var getType = function() {
            var fgEntity = spEntity.fromId(123);
            fgEntity.name = 'AAA Ardvaark'; // sorts in front of the default

            return new spResource.Type(spEntity.fromJSON({
                fieldGroups: [fgEntity],
                relationships: [
                    { name: 'R1', cardinality: manyToOne, relationshipInFromTypeGroup: fgEntity },
                    { name: 'R2', cardinality: oneToMany, relationshipInFromTypeGroup: fgEntity },
                    { name: 'R3', cardinality: manyToMany },
                    { name: 'R4', cardinality: oneToOne }],
                reverseRelationships: [
                    { name: 'R5', cardinality: oneToOne, relationshipInToTypeGroup: fgEntity },
                    { name: 'R6', cardinality: manyToMany, relationshipInToTypeGroup: fgEntity },
                    { name: 'R7', cardinality: oneToOne, relationshipInFromTypeGroup: fgEntity },
                    { name: 'R8', cardinality: manyToMany, relationshipInFromTypeGroup: fgEntity }]
            }));
        };

        it('getLookups returns lookups in the current group', function () {
            var type = getType();
            var fg = type.getFieldGroups()[0];
            var rels = fg.getLookups();
            expect(rels).toBeArray(2);
            expect(rels[0].getName()).toBe('R1');
            expect(rels[1].getName()).toBe('R5');
        });

        it('getLookups returns ungrouped lookups', function () {
            var type = getType();
            var fg = type.getFieldGroups()[1];
            var rels = fg.getLookups();
            expect(fg.getName()).toBe('Default');
            expect(rels).toBeArray(2);
            expect(rels[0].getName()).toBe('R4');
            expect(rels[1].getName()).toBe('R7');
        });

        it('getRelationships returns relationships in the current group', function () {
            var type = getType();
            var fg = type.getFieldGroups()[0];
            var rels = fg.getRelationships();
            expect(rels).toBeArray(2);
            expect(rels[0].getName()).toBe('R2');
            expect(rels[1].getName()).toBe('R6');
        });

        it('getRelationships returns ungrouped relationships', function () {
            var type = getType();
            var fg = type.getFieldGroups()[1];
            var rels = fg.getRelationships();
            expect(fg.getName()).toBe('Default');
            expect(rels).toBeArray(2);
            expect(rels[0].getName()).toBe('R3');
            expect(rels[1].getName()).toBe('R8');
        });

        it('getAllRelationships returns relationships in the current group', function () {
            var type = getType();
            var fg = type.getFieldGroups()[0];
            var rels = fg.getAllRelationships();
            expect(rels).toBeArray(4);
            expect(rels[0].getName()).toBe('R1');
            expect(rels[1].getName()).toBe('R2');
            expect(rels[2].getName()).toBe('R5');
            expect(rels[3].getName()).toBe('R6');
        });

        it('getAllRelationships returns ungrouped relationships', function () {
            var type = getType();
            var fg = type.getFieldGroups()[1];
            var rels = fg.getAllRelationships();
            expect(fg.getName()).toBe('Default');
            expect(rels).toBeArray(4);
            expect(rels[0].getName()).toBe('R3');
            expect(rels[1].getName()).toBe('R4');
            expect(rels[2].getName()).toBe('R7');
            expect(rels[3].getName()).toBe('R8');
        });

    });

});
