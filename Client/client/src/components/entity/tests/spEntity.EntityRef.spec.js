// Copyright 2011-2016 Global Software Innovation Pty Ltd
/* globals spEntity */

describe('Entity Model|spEntity.EntityRef|spec:', function () {
    'use strict';

    beforeEach(function () {
        this.addMatchers(TestSupport.matchers);
    });

    describe('constructor', function () {
        it('should exist', function () {
            expect(spEntity.EntityRef).toBeTruthy();
        });

        it('accepts ID as number', function () {
            var er = new spEntity.EntityRef(123);
            expect(er.getId()).toBe(123);
            expect(er.getAlias()).toBeNull();
            expect(er.getNamespace()).toBeNull();
            expect(er.getNsAlias()).toBeNull();
        });

        it('accepts ID as string', function () {
            var er = new spEntity.EntityRef('123');
            expect(er.getId()).toBe(123);
            expect(er.getAlias()).toBeNull();
            expect(er.getNamespace()).toBeNull();
            expect(er.getNsAlias()).toBeNull();
        });

        it('accepts null', function () {
            var er = new spEntity.EntityRef(null);
            expect(er.getId()).toBe(0);
            expect(er.getAlias()).toBeNull();
            expect(er.getNamespace()).toBeNull();
            expect(er.getNsAlias()).toBeNull();
        });

        it('accepts ID of zero', function () {
            var er = new spEntity.EntityRef(0);
            expect(er.getId()).toBe(0);
            expect(er.getAlias()).toBeNull();
            expect(er.getNamespace()).toBeNull();
            expect(er.getNsAlias()).toBeNull();
        });

        it('accepts core alias', function () {
            var er = new spEntity.EntityRef('type');
            expect(er.getId()).toBe(0);
            expect(er.getAlias()).toBe('type');
            expect(er.getNamespace()).toBe('core');
            expect(er.getNsAlias()).toBe('core:type');
        });

        it('accepts alias with namespace', function () {
            var er = new spEntity.EntityRef('test:myAlias');
            expect(er.getId()).toBe(0);
            expect(er.getAlias()).toBe('myAlias');
            expect(er.getNamespace()).toBe('test');
            expect(er.getNsAlias()).toBe('test:myAlias');
        });

        it('accepts id and alias with namespace', function () {
            var er = new spEntity.EntityRef({ id: 123, nsAlias: 'test:myAlias' });
            expect(er.getId()).toBe(123);
            expect(er.getAlias()).toBe('myAlias');
            expect(er.getNamespace()).toBe('test');
            expect(er.getNsAlias()).toBe('test:myAlias');
        });

        it('accepts id and alias without namespace', function () {
            var er = new spEntity.EntityRef({id:123, nsAlias: 'myAlias'});
            expect(er.getId()).toBe(123);
            expect(er.getAlias()).toBe('myAlias');
            expect(er.getNamespace()).toBe('core');
            expect(er.getNsAlias()).toBe('core:myAlias');
        });

        it('accepts JSON structure', function () {
            var er = new spEntity.EntityRef({
                    id: 123,
                    ns: 'test',
                    alias: 'myAlias'
                });
            expect(er.getId()).toBe(123);
            expect(er.getAlias()).toBe('myAlias');
            expect(er.getNamespace()).toBe('test');
            expect(er.getNsAlias()).toBe('test:myAlias');
        });

        it('accepts another EntityRef', function () {
            var cloneMe = new spEntity.EntityRef({
                id: 123,
                ns: 'test',
                alias: 'myAlias'
            });
            var er = new spEntity.EntityRef(cloneMe);
            expect(er.getId()).toBe(123);
            expect(er.getAlias()).toBe('myAlias');
            expect(er.getNamespace()).toBe('test');
            expect(er.getNsAlias()).toBe('test:myAlias');
        });

    });

    describe('member', function () {
        it('getId works', function () {
            var er = new spEntity.EntityRef(123);
            expect(er.getId()).toBe(123);
        });

        it('idP property works', function () {
            var er = new spEntity.EntityRef(123);
            expect(er.idP).toBe(123);
        });

        it('getAlias works', function () {
            var er0 = new spEntity.EntityRef(123);
            var er1 = new spEntity.EntityRef('hello');
            var er2 = new spEntity.EntityRef('test:hello');
            expect(er0.getAlias()).toBe(null);
            expect(er1.getAlias()).toBe('hello');
            expect(er2.getAlias()).toBe('hello');
        });

        it('getNamespace works', function () {
            var er0 = new spEntity.EntityRef(123);
            var er1 = new spEntity.EntityRef('hello');
            var er2 = new spEntity.EntityRef('test:hello');
            expect(er0.getNamespace()).toBe(null);
            expect(er1.getNamespace()).toBe('core');
            expect(er2.getNamespace()).toBe('test');
        });

        it('getNsAlias works', function () {
            var er0 = new spEntity.EntityRef(123);
            var er1 = new spEntity.EntityRef('hello');
            var er2 = new spEntity.EntityRef('test:hello');
            expect(er0.getNsAlias()).toBe(null);
            expect(er1.getNsAlias()).toBe('core:hello');
            expect(er2.getNsAlias()).toBe('test:hello');
        });

        it('nsAlias property works', function () {
            var er0 = new spEntity.EntityRef(123);
            var er1 = new spEntity.EntityRef('hello');
            var er2 = new spEntity.EntityRef('test:hello');
            expect(er0.nsAlias).toBe(null);
            expect(er1.nsAlias).toBe('core:hello');
            expect(er2.nsAlias).toBe('test:hello');
        });

        it('matches function works', function () {
            var er1 = new spEntity.EntityRef({ id: 123, alias: 'type', ns: 'core' });

            expect(er1.matches(123)).toBeTruthy();
            expect(er1.matches('type')).toBeTruthy();
            expect(er1.matches({ id: 123, alias: 'type' })).toBeTruthy();
            expect(er1.matches(321)).toBeFalsy();
            expect(er1.matches('type2')).toBeFalsy();
            expect(er1.matches({ id: 123, alias: 'type2' })).toBeFalsy();
            expect(er1.matches({ id: 321, alias: 'type' })).toBeFalsy();
            expect(er1.matches({ id: 321, alias: 'type2' })).toBeFalsy();
        });

        it('asEntityRef static function accepts ID, alias, entityref or entity', function () {
            var er1 = spEntity.asEntityRef(123);
            var er2 = spEntity.asEntityRef(er1);
            var er3 = spEntity.asEntityRef('blah');
            var e = spEntity.fromId(456);
            var er4 = spEntity.asEntityRef(e);
            expect(er1.getId()).toBe(123);
            expect(er2).toBe(er1);
            expect(er3.getNsAlias()).toBe('core:blah');
            expect(er4).toBe(e.eid());
        });

        it('getAliasOrId static function works', function () {
            var id1 = spEntity.asAliasOrId(123);
            var id2 = spEntity.asAliasOrId('blah');
            var er1 = spEntity.asEntityRef(456);
            var id3 = spEntity.asAliasOrId(er1);
            expect(id1).toBe(123);
            expect(id2).toBe('core:blah');
            expect(id3).toBe(456);
            expect(function() { return spEntity.asAliasOrId([]); }).toThrow();
        });

        it('setNsAlias works for core', function () {
            var er0 = new spEntity.EntityRef(123);
            er0.setNsAlias('blah');
            expect(er0.nsAlias).toBe('core:blah');
        });

        it('setNsAlias works for console', function () {
            var er0 = new spEntity.EntityRef(123);
            er0.setNsAlias('k:blah');
            expect(er0.nsAlias).toBe('console:blah');
        });

    });

});
