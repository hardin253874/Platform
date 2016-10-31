// Copyright 2011-2016 Global Software Innovation Pty Ltd
/* globals spEntity, jsonString, jsonInt, jsonLookup, jsonRelationship, jsonDecimal, jsonBool,
 jsonCurrency, jsonDate, jsonDateTime, jsonTime */

describe('Entity Model|spEntity|spec:', function () {
    'use strict';

    beforeEach(function () {
        this.addMatchers(TestSupport.matchers);
    });


    describe('history', function () {

        it('should be able to get history', function () {
            var json = {};
            var e = spEntity.fromJSON(json);
            var g = e.graph;
            expect(g).toBeTruthy();
            expect(g.history).toBeTruthy();
        });

        it('should be able to undo a field set', function () {
            var json = { name: 'hello' };
            var e = spEntity.fromJSON(json).markAllUnchanged();
            expect(e.name).toBe('hello');
            expect(e.getDataState()).toBe(spEntity.DataStateEnum.Unchanged);

            e.name = 'hello2';
            expect(e.name).toBe('hello2');
            expect(e.getDataState()).toBe(spEntity.DataStateEnum.Update);

            var h = e.graph.history;
            h.undo();
            expect(e.name).toBe('hello');
            expect(e.getDataState()).toBe(spEntity.DataStateEnum.Unchanged);
        });

        it('should be able to redo a field set', function () {
            var json = { name: 'hello' };
            var e = spEntity.fromJSON(json).markAllUnchanged();
            expect(e.name).toBe('hello');
            expect(e.getDataState()).toBe(spEntity.DataStateEnum.Unchanged);

            e.name = 'hello2';
            expect(e.name).toBe('hello2');
            expect(e.getDataState()).toBe(spEntity.DataStateEnum.Update);

            var h = e.graph.history;
            h.undo();
            expect(e.name).toBe('hello');
            expect(e.getDataState()).toBe(spEntity.DataStateEnum.Unchanged);

            h.redo();
            expect(e.name).toBe('hello2');
            expect(e.getDataState()).toBe(spEntity.DataStateEnum.Update);
        });

        it('should be able to undo a child field set', function () {
            var json = {
                name: 'hello',
                child: {
                    name: 'child'
                }
            };
            var e = spEntity.fromJSON(json);
            expect(e.child.name).toBe('child');

            e.child.name = 'child2';
            expect(e.child.name).toBe('child2');

            var h = e.graph.history;
            h.undo();
            expect(e.child.name).toBe('child');
        });

        it('should be able to undo a lookup value change', function () {
            var json = { lookup: jsonLookup(123) };
            var e = spEntity.fromJSON(json);
            expect(e.lookup.id()).toBe(123);

            e.lookup = spEntity.fromId(456);
            expect(e.lookup.id()).toBe(456);

            var h = e.graph.history;
            h.undo();
            expect(e.lookup.id()).toBe(123);
        });

        it('should be able to undo a lookup value change from null', function () {
            var json = { lookup: jsonLookup() };
            var e = spEntity.fromJSON(json);
            expect(e.lookup).toBeNull();

            e.lookup = spEntity.fromId(456);
            expect(e.lookup.id()).toBe(456);

            var h = e.graph.history;
            h.undo();
            expect(e.lookup).toBeNull();
        });

        it('should be able to undo a lookup value change to null', function () {
            var json = { lookup: jsonLookup(123) };
            var e = spEntity.fromJSON(json);
            expect(e.lookup.id()).toBe(123);

            e.lookup = null;
            expect(e.lookup).toBeNull();

            var h = e.graph.history;
            h.undo();
            expect(e.lookup.id()).toBe(123);
        });

        it('should be able to undo a relationship add', function () {
            var json = { rel: [123] };
            var e = spEntity.fromJSON(json).markAllUnchanged();
            expect(e.rel.length).toBe(1);

            e.rel.add(456);
            expect(e.rel.length).toBe(2);
            expect(e.rel[1].id()).toBe(456);

            var h = e.graph.history;
            h.undo();
            expect(e.rel.length).toBe(1);
            expect(e.rel[0].id()).toBe(123);
        });

        it('should be able to undo a relationship remove', function () {
            var json = { rel: [123,456] };
            var e = spEntity.fromJSON(json).markAllUnchanged();
            expect(e.rel.length).toBe(2);

            e.rel.remove(456);
            expect(e.rel.length).toBe(1);
            expect(e.rel[0].id()).toBe(123);

            var h = e.graph.history;
            h.undo();
            expect(e.rel.length).toBe(2);
            expect(e.rel[0].id()).toBe(123);
            expect(e.rel[1].id()).toBe(456);
        });

        it('should be able to undo a relationship clear', function () {
            var json = { rel: [123, 456] };
            var e = spEntity.fromJSON(json).markAllUnchanged();
            expect(e.rel.length).toBe(2);
            expect(e.rel.removeExisting).toBeFalsy();

            e.rel.clear();
            expect(e.rel.length).toBe(0);
            expect(e.rel.getRelationshipContainer().removeExisting).toBeTruthy();

            var h = e.graph.history;
            h.undo();
            expect(e.rel.length).toBe(2);
            expect(e.rel[0].id()).toBe(123);
            expect(e.rel[1].id()).toBe(456);
            expect(e.rel.removeExisting).toBeFalsy();
        });

        it('should be able to undo to a bookmark via history object', function () {
            var json = { f1: 'f1', f2: 'f2', f3: 'f3' };

            var e = spEntity.fromJSON(json).markAllUnchanged();
            var h = e.graph.history;

            e.f1 = 'f1b';
            var b = h.addBookmark();
            e.f2 = 'f2b';
            e.f3 = 'f3b';
            h.undoBookmark(b);

            expect(e.f1).toBe('f1b');
            expect(e.f2).toBe('f2');
            expect(e.f3).toBe('f3');
        });

        it('should be able to undo to a bookmark directly', function () {
            var json = { f1: 'f1', f2: 'f2', f3: 'f3' };

            var e = spEntity.fromJSON(json).markAllUnchanged();
            var h = e.graph.history;

            e.f1 = 'f1b';
            var b = h.addBookmark();
            e.f2 = 'f2b';
            e.f3 = 'f3b';
            b.undo();

            expect(e.f1).toBe('f1b');
            expect(e.f2).toBe('f2');
            expect(e.f3).toBe('f3');
        });

        it('should be able to undo to a nested bookmark', function () {
            var json = { f1: 'f1', f2: 'f2', f3: 'f3' };

            var e = spEntity.fromJSON(json).markAllUnchanged();
            var h = e.graph.history;

            e.f1 = 'f1b';
            var b1 = h.addBookmark();
            e.f2 = 'f2b';
            h.addBookmark();
            e.f3 = 'f3b';

            h.undoBookmark(b1);

            expect(e.f1).toBe('f1b');
            expect(e.f2).toBe('f2');
            expect(e.f3).toBe('f3');
        });

        it('should be able to redo a bookmark via history object', function () {
            var json = { f1: 'f1', f2: 'f2', f3: 'f3', f4: 'f4' };

            var e = spEntity.fromJSON(json).markAllUnchanged();
            var h = e.graph.history;

            e.f1 = 'f1b';
            var b = h.addBookmark();
            e.f2 = 'f2b';
            e.f3 = 'f3b';
            b.endBookmark();
            e.f4 = 'f3b';
            h.undoBookmark(b); // undo everything to the start of the bookmark
            h.redoBookmark(b); // then redo everything to the end of the bookmark

            expect(e.f1).toBe('f1b');
            expect(e.f2).toBe('f2b');
            expect(e.f3).toBe('f3b');
            expect(e.f4).toBe('f4');
        });

        it('should be able to redo a bookmark directly', function () {
            var json = { f1: 'f1', f2: 'f2', f3: 'f3', f4: 'f4' };

            var e = spEntity.fromJSON(json).markAllUnchanged();
            var h = e.graph.history;

            e.f1 = 'f1b';
            var b = h.addBookmark();
            e.f2 = 'f2b';
            e.f3 = 'f3b';
            b.endBookmark();
            e.f4 = 'f3b';
            b.undo(); // undo everything to the start of the bookmark
            b.redo(); // then redo everything to the end of the bookmark

            expect(e.f1).toBe('f1b');
            expect(e.f2).toBe('f2b');
            expect(e.f3).toBe('f3b');
            expect(e.f4).toBe('f4');
        });

    });

});
