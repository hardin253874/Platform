// Copyright 2011-2016 Global Software Innovation Pty Ltd
/* globals spEntity */

describe('Entity Model|spEntity|spec:', function () {
    'use strict';

    beforeEach(function () {
        this.addMatchers(TestSupport.matchers);
    });


    describe('Relationship arrays', function () {

        // Introduction Tests

        it('getRelationshipContainer returns the container', function () {
            var e = spEntity.fromJSON({ myRelationship: [] });
            var c1 = e.getRelationshipContainer('myRelationship');
            var arr = e.getMyRelationship();
            var c2 = arr.getRelationshipContainer(); // test subject
            expect(c1).toBe(c2);
        });

        it('getInstances returns instances', function () {
            var e = spEntity.fromJSON({ myRelationship: [123, 456] });
            var arr = e.getMyRelationship();
            var inst = arr.getInstances(); // test subject
            expect(inst).toBeArray(2);
        });

        it('getInstances returns empty array if empty', function () {
            var e = spEntity.fromJSON({ myRelationship: [] });
            var arr = e.getMyRelationship();
            var inst = arr.getInstances(); // test subject
            expect(inst).toBeEmptyArray();
        });

        it('can add entity', function () {
            var e = spEntity.fromJSON({ myRelationship: [] });
            var mr = e.getMyRelationship();
            var relatedE = spEntity.fromId(123);
            mr.add(relatedE); // test subject
            expect(mr[0].id()).toBe(123);

            var inst = mr.getInstances();
            expect(inst[0].entity).toBe(mr[0]);
            expect(inst[0].getDataState()).toBe(spEntity.DataStateEnum.Create);
            expect(relatedE._graph).toBe(e._graph);
        });

        it('can add entity by Id', function () {
            var e = spEntity.fromJSON({ myRelationship: [] });
            var mr = e.getMyRelationship();
            mr.add(123); // test subject
            expect(mr[0].id()).toBe(123);

            var inst = mr.getInstances();
            expect(inst[0].entity).toBe(mr[0]);
            expect(inst[0].getDataState()).toBe(spEntity.DataStateEnum.Create);
        });

        it('can add entity by alias', function () {
            var e = spEntity.fromJSON({ myRelationship: [] });
            var mr = e.getMyRelationship();
            mr.add('hello'); // test subject
            expect(mr[0].eid().getAlias()).toBe('hello');
        });

        it('can add entity array', function () {
            var e = spEntity.fromJSON({ myRelationship: [] });
            var mr = e.getMyRelationship();
            mr.add([123, 456]); // test subject
            expect(mr[0].id()).toBe(123);
            expect(mr[1].id()).toBe(456);
        });

        it('can add entity to existing', function () {
            var e = spEntity.fromJSON({ myRelationship: [123] }).markAllUnchanged();
            var mr = e.getMyRelationship();
            mr.add(456); // test subject
            expect(mr[0].id()).toBe(123);
            expect(mr[1].id()).toBe(456);

            var inst = mr.getInstances();
            expect(inst[0].getDataState()).toBe(spEntity.DataStateEnum.Unchanged);
            expect(inst[1].getDataState()).toBe(spEntity.DataStateEnum.Create);
        });

        it('can clear', function () {
            var e = spEntity.fromJSON({ myRelationship: [123] }).markAllUnchanged();
            var mr = e.getMyRelationship();
            mr.clear(); // test subject

            var rc = e.getMyRelationship().getRelationshipContainer();
            expect(e.getMyRelationship()).toBeEmptyArray();
            expect(rc.getInstances()).toBeEmptyArray();
            expect(rc.removeExisting).toBeTruthy();
            expect(e.getDataState()).toBe(spEntity.DataStateEnum.Update);
        });

        it('can clear even if already empty', function () {
            var e = spEntity.fromJSON({ myRelationship: [] }).markAllUnchanged();
            var mr = e.getMyRelationship();
            mr.clear(); // test subject

            var rc = e.getMyRelationship().getRelationshipContainer();
            expect(e.getMyRelationship()).toBeEmptyArray();
            expect(rc.getInstances()).toBeEmptyArray();
            expect(rc.removeExisting).toBeTruthy();
            expect(rc._deleteExisting).toBeFalsy();
            expect(e.getDataState()).toBe(spEntity.DataStateEnum.Update);
        });

        it('can delete all existing', function () {
            var e = spEntity.fromJSON({ myRelationship: [123] }).markAllUnchanged();
            var mr = e.getMyRelationship();
            mr.deleteExisting(); // test subject

            var rc = e.getMyRelationship().getRelationshipContainer();
            expect(e.getMyRelationship()).toBeEmptyArray();
            expect(rc.getInstances()).toBeEmptyArray();
            expect(rc.removeExisting).toBeTruthy();
            expect(rc._deleteExisting).toBeTruthy();
            expect(e.getDataState()).toBe(spEntity.DataStateEnum.Update);
        });

        it('can add then remove entity', function () {
            var e = spEntity.fromJSON({ id: 1, myRelationship: [123, 456] }).markAllUnchanged();
            var mr = e.getMyRelationship();

            var newE = spEntity.fromJSON({ type: 'abc' });
            mr.add(newE);
            expect(mr.getInstances()).toBeArray(3);

            mr.remove(newE); // test subject

            expect(mr).toBeArray(2);
            var inst = mr.getInstances();
            var c = mr.getRelationshipContainer();
            expect(inst).toBeArray(2);
            //expect(e.getDataState()).toBe(spEntity.DataStateEnum.Update);
            expect(c.findInstance(123).getDataState()).toBe(spEntity.DataStateEnum.Unchanged);
            expect(c.findInstance(456).getDataState()).toBe(spEntity.DataStateEnum.Unchanged);
        });

        it('can remove entity', function () {
            var e = spEntity.fromJSON({ id: 1, myRelationship: [123, 456] }).markAllUnchanged();
            var mr = e.getMyRelationship();
            var relatedE = spEntity.fromId(123);
            mr.remove(relatedE); // test subject

            expect(mr).toBeArray(1);
            expect(mr[0].id()).toBe(456);
            var inst = mr.getInstances();
            var c = mr.getRelationshipContainer();
            expect(inst).toBeArray(2);
            expect(e.getDataState()).toBe(spEntity.DataStateEnum.Update);
            expect(c.findInstance(123).getDataState()).toBe(spEntity.DataStateEnum.Delete);
            expect(c.findInstance(456).getDataState()).toBe(spEntity.DataStateEnum.Unchanged);
        });

        it('can remove entity by ID', function () {
            var e = spEntity.fromJSON({ id: 1, myRelationship: [123, 456] }).markAllUnchanged();
            var mr = e.getMyRelationship();
            mr.remove(123); // test subject

            expect(mr).toBeArray(1);
            expect(mr[0].id()).toBe(456);
            var inst = mr.getInstances();
            var c = mr.getRelationshipContainer();
            expect(inst).toBeArray(2);
            expect(e.getDataState()).toBe(spEntity.DataStateEnum.Update);
            expect(c.findInstance(123).getDataState()).toBe(spEntity.DataStateEnum.Delete);
            expect(c.findInstance(456).getDataState()).toBe(spEntity.DataStateEnum.Unchanged);
        });

        it('can remove entity by alias', function () {
            var e = spEntity.fromJSON({ id: 1, myRelationship: ['blah:hello', 456] }).markAllUnchanged();
            var mr = e.getMyRelationship();
            mr.remove('blah:hello'); // test subject

            expect(mr).toBeArray(1);
            expect(mr[0].id()).toBe(456);
            var inst = mr.getInstances();
            var c = mr.getRelationshipContainer();
            expect(inst).toBeArray(2);
            expect(e.getDataState()).toBe(spEntity.DataStateEnum.Update);
            expect(c.findInstance('blah:hello').getDataState()).toBe(spEntity.DataStateEnum.Delete);
            expect(c.findInstance(456).getDataState()).toBe(spEntity.DataStateEnum.Unchanged);
        });

        it('can remove entities by array', function () {
            var e = spEntity.fromJSON({ id: 1, myRelationship: [22, 33, 'test:a', 'test:b'] }).markAllUnchanged();
            var mr = e.getMyRelationship();
            mr.remove([33, 'test:b']); // test subject

            expect(mr).toBeArray(2);
            expect(mr[0].id()).toBe(22);
            expect(mr[1].eid().getNsAlias()).toBe('test:a');
            var inst = mr.getInstances();
            var c = mr.getRelationshipContainer();
            expect(inst).toBeArray(4);
            expect(e.getDataState()).toBe(spEntity.DataStateEnum.Update);
            expect(c.findInstance(22).getDataState()).toBe(spEntity.DataStateEnum.Unchanged);
            expect(c.findInstance(33).getDataState()).toBe(spEntity.DataStateEnum.Delete);
            expect(c.findInstance('test:a').getDataState()).toBe(spEntity.DataStateEnum.Unchanged);
            expect(c.findInstance('test:b').getDataState()).toBe(spEntity.DataStateEnum.Delete);
        });

        it('can remove entity that isn\'t present', function () {
            var e = spEntity.fromJSON({ id: 1, myRelationship: [123] }).markAllUnchanged();
            var mr = e.getMyRelationship();
            mr.remove(456); // test subject

            expect(mr).toBeArray(1);
            expect(mr[0].id()).toBe(123);
            var inst = mr.getInstances();
            var c = mr.getRelationshipContainer();
            expect(inst).toBeArray(2);
            expect(e.getDataState()).toBe(spEntity.DataStateEnum.Update);
            expect(c.findInstance(123).getDataState()).toBe(spEntity.DataStateEnum.Unchanged);
            expect(c.findInstance(456).getDataState()).toBe(spEntity.DataStateEnum.Delete);
        });

        it('can mark clear as unchanged', function () {
            var e = spEntity.fromJSON({ myRelationship: [123] }).markAllUnchanged();
            var mr = e.getMyRelationship();
            mr.clear(); // test subject

            var rc = mr.getRelationshipContainer();
            
            rc.markUnchanged();

            expect(rc.removeExisting).toBeFalsy();
        });

    });

    describe('Relationship interface', function () {

        it('allows the get and set of relationship instance entities', function() {
            var e = spEntity.fromJSON({ id: 1, myRelationship: [123] });
            expect(e.getRelationship('myRelationship').length).toBe(1);

            expect(e.relationshipInstances('myRelationship').length).toBe(1);
            expect(_.first(e.relationshipInstances('myRelationship')).entity).toBe(_.first(e.getRelationship('myRelationship')));
            expect(_.first(e.relationshipInstances('myRelationship')).relEntity).toBeNull();

            e.relationshipInstances('anotherRelationship', [{
                entity: spEntity.fromJSON({ id: 123, name: 'test' }),
                relEntity: spEntity.fromJSON({ id: 456, value: 'testValue' })
            }]);
            expect(e.relationshipInstances('anotherRelationship').length).toBe(1);
            expect(_.first(e.relationshipInstances('anotherRelationship')).relEntity).not.toBeNull();
            expect(_.first(e.relationshipInstances('anotherRelationship')).entity.id()).toBe(123);
            expect(_.first(e.relationshipInstances('anotherRelationship')).relEntity.id()).toBe(456);
            expect(_.first(e.relationshipInstances('anotherRelationship')).entity._graph).toBe(e._graph);
            expect(_.first(e.relationshipInstances('anotherRelationship')).relEntity._graph).toBe(e._graph);
        });

        it('allows the get and set of relationship instance entities via ids', function() {
            var e = spEntity.fromJSON({ id: 1, myRelationship: [123] });
            expect(e.getRelationship('myRelationship').length).toBe(1);

            expect(e.relationshipInstances('myRelationship').length).toBe(1);
            expect(_.first(e.relationshipInstances('myRelationship')).entity).toBe(_.first(e.getRelationship('myRelationship')));
            expect(_.first(e.relationshipInstances('myRelationship')).relEntity).toBeNull();

            e.relationshipInstances('anotherRelationship', [new spEntity.RelationshipInstance(
                spEntity.fromId(123),
                spEntity.fromJSON({ id: 456, value: 'testValue' })
            )]);
            expect(e.relationshipInstances('anotherRelationship').length).toBe(1);
            expect(_.first(e.relationshipInstances('anotherRelationship')).relEntity).not.toBeNull();
            expect(_.first(e.relationshipInstances('anotherRelationship')).entity.id()).toBe(123);
            expect(_.first(e.relationshipInstances('anotherRelationship')).relEntity.id()).toBe(456);
        });

        it('allows a set of relationship instance entities and then prep for post', function() {
            var e = spEntity.fromJSON({ id: 1, myRelationship: [123] });

            e.relationshipInstances('anotherRelationship', [new spEntity.RelationshipInstance(
                spEntity.fromJSON({ id: 123, name: 'test' }),
                spEntity.fromJSON({ id: 456, value: 'testValue' })
            )]);

            expect(spEntity.entitiesToEntityData([e])).toBeTruthy();
        });

        it('allows a set of relationship instance entities using id and then prep for post', function() {
            var e = spEntity.fromJSON({ id: 1, myRelationship: [123] });

            e.relationshipInstances('anotherRelationship', [new spEntity.RelationshipInstance(
                spEntity.fromId(123),
                spEntity.fromJSON({ id: 456, value: 'testValue' })
            )]);

            expect(spEntity.entitiesToEntityData([e])).toBeTruthy();
        });

    });

});