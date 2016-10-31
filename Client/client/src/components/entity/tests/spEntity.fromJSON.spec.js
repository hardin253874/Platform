// Copyright 2011-2016 Global Software Innovation Pty Ltd
/* globals spEntity, jsonString, jsonInt, jsonLookup, jsonRelationship, jsonDecimal, jsonBool,
 jsonCurrency, jsonDate, jsonDateTime, jsonTime */

describe('Entity Model|spEntity|spec:', function () {
    'use strict';

    beforeEach(function () {
        this.addMatchers(TestSupport.matchers);
    });


    describe('fromJSON', function () {

        it('should work empty', function () {
            var json = {};
            var e = spEntity.fromJSON(json);
            expect(e).toBeEntity();

            expect(spEntity.fromJSON(null)).toBeNull();
            expect(spEntity.fromJSON()).toBeUndefined();
        });

        it('should work by ID', function () {
            var json = {id: 123};
            var e = spEntity.fromJSON(json);
            expect(e).toBeEntity();
            expect(e.id()).toBe(123);
            expect(e.getDataState()).toBe(spEntity.DataStateEnum.Unchanged);
        });

        it('should work by alias', function () {
            var json = {id: 'test'};
            var e = spEntity.fromJSON(json);
            expect(e).toBeEntity();
            expect(e.eid().getNsAlias()).toBe('core:test');
            expect(e.getDataState()).toBe(spEntity.DataStateEnum.Unchanged);
        });

        it('should work by id and alias', function () {
            var json = {id: 6781, alias: 'testZ'};
            var e = spEntity.fromJSON(json);
            expect(e).toBeEntity();
            expect(e.idP).toBe(6781);
            expect(e.eidP.getNsAlias()).toBe('core:testZ');
            expect(e.alias()).toBe('core:testZ');
            expect(e.getDataState()).toBe(spEntity.DataStateEnum.Unchanged);
        });

        it('should work by id and core alias', function () {
            var json = {id: 6781, alias: 'core:testZ'};
            var e = spEntity.fromJSON(json);
            expect(e).toBeEntity();
            expect(e.idP).toBe(6781);
            expect(e.eidP.getNsAlias()).toBe('core:testZ');
            expect(e.alias()).toBe('core:testZ');
            expect(e.getDataState()).toBe(spEntity.DataStateEnum.Unchanged);
        });

        it('should work by id and console alias', function () {
            var json = {id: 6781, alias: 'console:testZ'};
            var e = spEntity.fromJSON(json);
            expect(e).toBeEntity();
            expect(e.idP).toBe(6781);
            expect(e.eidP.getNsAlias()).toBe('console:testZ');
            expect(e.alias()).toBe('console:testZ');
            expect(e.getDataState()).toBe(spEntity.DataStateEnum.Unchanged);
        });

        it('should work for typeId alias', function () {
            var json = {typeId: 'folder'};
            var e = spEntity.fromJSON(json);
            expect(e).toBeEntity();
            expect(e.getType().getNsAlias()).toBe('core:folder');
            expect(e.getDataState()).toBe(spEntity.DataStateEnum.Create);
        });

        it('should work for typeId id', function () {
            var json = {typeId: 123};
            var e = spEntity.fromJSON(json);
            expect(e).toBeEntity();
            expect(e.getType().getId()).toBe(123);
            expect(e.getDataState()).toBe(spEntity.DataStateEnum.Create);
        });

        it('should work for typeId and id combo', function () {
            var json = {id: 1, typeId: 123};
            var e = spEntity.fromJSON(json);
            expect(e).toBeEntity();
            expect(e.id()).toBe(1);
            expect(e.getType().getId()).toBe(123);
            expect(e.getDataState()).toBe(spEntity.DataStateEnum.Unchanged);
        });

        it('will pass back any entities', function () {
            var e1 = spEntity.fromId(123);
            e1.setDataState(spEntity.DataStateEnum.Update);
            var e = spEntity.fromJSON(e1);
            expect(e).toBeEntity();
            expect(e).toBe(e1, 'e1');
            expect(e.getDataState()).toBe(spEntity.DataStateEnum.Update);  // make sure our data state didn't change
        });

        it('can set string fields', function () {
            var json = {firstName: 'Peter', emptyName: ''};
            var e = spEntity.fromJSON(json);
            expect(e.getFirstName()).toBe('Peter');
            expect(e.getEmptyName()).toBe('');
            expect(e.getDataState()).toBe(spEntity.DataStateEnum.Create);
        });

        it('can set string fields on existing ID', function () {
            var json = {id: 1, firstName: 'Peter', emptyName: ''};
            var e = spEntity.fromJSON(json);
            expect(e.getFirstName()).toBe('Peter');
            expect(e.getEmptyName()).toBe('');
            expect(e.getDataState()).toBe(spEntity.DataStateEnum.Update);
        });

        it('can set int fields', function () {
            var json = {age: 123};
            var e = spEntity.fromJSON(json);
            expect(e.getAge()).toBe(123);
            expect(e.getDataState()).toBe(spEntity.DataStateEnum.Create);
        });

        it('can set bool fields', function () {
            var json = {yes: true, no: false};
            var e = spEntity.fromJSON(json);
            expect(e.getYes()).toBe(true);
            expect(e.getNo()).toBe(false);
            expect(e.getDataState()).toBe(spEntity.DataStateEnum.Create);
        });

        it('accepts arrays', function () {
            var json = [{id: 1}, {id: 2}];
            var eArr = spEntity.fromJSON(json);
            expect(eArr).toBeArray(2);
            expect(eArr[0].eid().id()).toBe(1);
            expect(eArr[1].eid().id()).toBe(2);
            expect(eArr[0].getDataState()).toBe(spEntity.DataStateEnum.Unchanged);
            expect(eArr[1].getDataState()).toBe(spEntity.DataStateEnum.Unchanged);
        });

        it('can set lookups', function () {
            var json = {
                id: 123,
                resourceInFolder: {id: 456}
            };
            var e = spEntity.fromJSON(json);
            var res = e.getResourceInFolder();
            var rc = e.getRelationshipContainer('resourceInFolder');
            var relInst = rc.getInstances()[0];

            expect(res).toBeEntity();
            expect(res.id()).toBe(456);
            expect(e.getDataState()).toBe(spEntity.DataStateEnum.Update);
            expect(res.getDataState()).toBe(spEntity.DataStateEnum.Unchanged);
            expect(rc.removeExisting).toBeTruthy('removeExisting');
            expect(rc.getInstances()[0].getDataState()).toBe(spEntity.DataStateEnum.Create);
            expect(relInst.getDataState()).toBe(spEntity.DataStateEnum.Create);
        });

        it('can set relationships', function () {
            var json = {
                id: 123,
                inherits: [{id: 456}, {id: 789}]
            };
            var e = spEntity.fromJSON(json);
            var res = e.getInherits();
            var rc = res.getRelationshipContainer();
            var relInst = res.getInstances();
            expect(res).toBeArray(2);
            expect(res[0].id()).toBe(456);
            expect(res[1].id()).toBe(789);
            expect(e.getDataState()).toBe(spEntity.DataStateEnum.Update);
            expect(res[0].getDataState()).toBe(spEntity.DataStateEnum.Unchanged);
            expect(res[1].getDataState()).toBe(spEntity.DataStateEnum.Unchanged);
            expect(relInst[0].getDataState()).toBe(spEntity.DataStateEnum.Create);
            expect(relInst[1].getDataState()).toBe(spEntity.DataStateEnum.Create);
            expect(rc.removeExisting).toBeTruthy('removeExisting');
            expect(rc.getInstances()[0].getDataState()).toBe(spEntity.DataStateEnum.Create);
            expect(rc.getInstances()[1].getDataState()).toBe(spEntity.DataStateEnum.Create);
        });

        it('jsonString works', function () {
            var json = {blah: jsonString(null)};
            var e = spEntity.fromJSON(json);
            expect(e.getBlah()).toBe(null);
            expect(e.getFieldContainer('core:blah')._dataType).toBe(spEntity.DataType.String);
        });

        it('jsonInt works', function () {
            var json = {blah: jsonInt(null)};
            var e = spEntity.fromJSON(json);
            expect(e.getBlah()).toBe(null);
            expect(e.getFieldContainer('core:blah')._dataType).toBe(spEntity.DataType.Int32);
        });

        it('jsonDecimal works', function () {
            var json = {blah: jsonDecimal(null), blah2: jsonDecimal(2)};
            var e = spEntity.fromJSON(json);
            expect(e.getBlah()).toBe(null);
            expect(e.getBlah2()).toBe(2);
            expect(e.getFieldContainer('core:blah')._dataType).toBe(spEntity.DataType.Decimal);
            expect(e.getFieldContainer('core:blah2')._dataType).toBe(spEntity.DataType.Decimal);
        });

        it('jsonLookup works', function () {
            var json = {
                a: jsonLookup(null),
                b: jsonLookup(1),
                c: jsonLookup('type'),
                d: jsonLookup(spEntity.fromId(123)),
                e: jsonLookup({someJson: 123}),
            };
            var e = spEntity.fromJSON(json);

            expect(e.getA()).toBe(null);
            expect(e.getRelationshipContainer('core:a')).toBeTruthy();
            expect(e.getB().eid().getId()).toBe(1);
            expect(e.getC().eid().getNsAlias()).toBe('core:type');
            expect(e.getD().eid().getId()).toBe(123);
            expect(e.getE().getSomeJson()).toBe(123);
        });

        it('jsonRelationship works', function () {
            var json = {
                a: jsonRelationship(null),
                b: jsonRelationship([1, 2]),
                c: jsonRelationship(['type', 'something']),
                d: jsonRelationship([spEntity.fromId(123)]),
                e: jsonRelationship([{someJson: 123}]),
            };
            var e = spEntity.fromJSON(json);
            expect(e.getA()).toBeEmptyArray();
            expect(e.getRelationshipContainer('core:a')).toBeTruthy();
            expect(e.getB()).toBeArray(2);
            expect(e.getB()[0].eid().id()).toBe(1);
            expect(e.getC()[0].eid().getNsAlias()).toBe('core:type');
            expect(e.getD()[0].eid().getId()).toBe(123);
            expect(e.getE()[0].getSomeJson()).toBe(123);
        });

        it('everything at once', function () {
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
                    {name: 'Acer'},     // embedded entities can also be JSON
                    spEntity.fromId(456) // or they can be pre-existing entities, wherever you got them from
                ],
                'console:toolTip': 'abc'  // fully qualified aliases can also be used, if surrounded in quotes.
            };

            var e = spEntity.fromJSON(json);

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

        it('connects cycles', function () {
            var json = {
                id: 123,
                myLookup: {
                    parent: jsonLookup(123)
                }
            };
            var e = spEntity.fromJSON(json);
            expect(e.getMyLookup().getParent()).toBe(e);
        });

        it('connects in same instance', function () {
            var json = {
                id: 123,
                myLookup: jsonLookup(123)
            };
            var e = spEntity.fromJSON(json);
            expect(e.getMyLookup()).toBe(e);
        });

        it('connects ID-only relationships with no defined target object', function () {
            var json = {
                myLookup1: jsonLookup(123),
                myLookup2: jsonLookup(123)
            };
            var e = spEntity.fromJSON(json);
            expect(e.getMyLookup1()).toBe(e.getMyLookup2());
        });

        it('connects in same instance regardless of order of appearance', function () {
            var json = {
                myLookup1: jsonLookup(123),
                myLookup2: {id: 123, name: 'abc'},
                myLookup3: jsonLookup(123)
            };
            var e = spEntity.fromJSON(json);
            expect(e.getMyLookup1()).toBe(e.getMyLookup2());
            expect(e.getMyLookup2()).toBe(e.getMyLookup3());
            expect(e.getMyLookup2().getName()).toBe('abc');
        });

        it('connects aliases', function () {
            var json = {
                id: 'test',
                myLookup: {
                    parent: jsonLookup('core:test')
                }
            };
            var e = spEntity.fromJSON(json);
            expect(e.getMyLookup().getParent()).toBe(e);
        });

        it('connects to existing entities', function () {
            var tmp = spEntity.fromId(123);
            var json = {
                myLookup1: tmp,
                myLookup2: jsonLookup(123)
            };
            var e = spEntity.fromJSON(json);
            expect(e.getMyLookup1()).toBe(e.getMyLookup2());
        });

        it('connects relationships', function () {
            var json = {
                myRel1: [123],
                myRel2: [123, {name: 'blah'}]
            };
            var e = spEntity.fromJSON(json);
            expect(e.getMyRel1()[0]).toBe(e.getMyRel2()[0]);
        });

        it('connects relationships in array without namespace', function () {
            var json =
            {
                id: 't1',
                myRel1: [{myRel2: jsonLookup('t1')}]
            };
            var e = spEntity.fromJSON(json);
            expect(e.getMyRel1()[0].getMyRel2()).toBe(e);
        });

        it('should reuse existing graph if an entity is assigned in via lookup', function () {
            var e1 = spEntity.fromJSON({a: 1});
            var graph = e1.graph;
            var e2 = spEntity.fromJSON({b: jsonLookup(e1)});

            expect(e1.graph).toBe(graph);
            expect(e2.graph).toBe(graph);
        });

        it('should reuse existing graph if an entity is assigned in via relationship', function () {
            var e1 = spEntity.fromJSON({a: 1});
            var graph = e1.graph;
            var e2 = spEntity.fromJSON({b: [e1]});

            expect(e1.graph).toBe(graph);
            expect(e2.graph).toBe(graph);
        });
    });


    describe('toJSON', function () {

        it('should handle entity without ID', function () {
            var json = {
                name: 'X'
            };

            var e = spEntity.fromJSON(json);
            var json2 = spEntity.toJSON(e);

            expect(json2.name).toBe('X');
        });

        it('should work', function () {
            var json = {
                id: 123,                 // optional: number or alias or absent of resource id
                firstName: 'Peter',       // string values get registered as string fields
                daysSinceLogin: 3,        // number values get registered as Int32 fields
                yesNo: true,              // string values get registered as string fields
                manager: {                // non-array objects get registered as lookups
                    id: 'judeJacobs'      // aliases can be used for IDs
                },
                monitors: [               // arrays get registered as to-many relationships
                    {name: 'Acer'},     // embedded entities can also be JSON
                    spEntity.fromId(456) // or they can be pre-existing entities, wherever you got them from
                ],
                'console:toolTip': 'abc'  // fully qualified aliases can also be used, if surrounded in quotes.
            };

            var e = spEntity.fromJSON(json);
            var json2 = spEntity.toJSON(e);

            expect(json2.id).toBe(123);
            expect(json2.firstName).toBe('Peter');
            expect(json2.daysSinceLogin).toBe(3);
            expect(json2.yesNo).toBe(true);
            expect(json2.manager.id).toBe('core:judeJacobs');
            expect(json2.monitors[0].name).toBe('Acer');
            expect(json2.monitors[1].id).toBe(456);
            expect(json2['console:toolTip']).toBe('abc');

            var graph = e._graph;
            expect(graph).toBeTruthy();
            expect(e.manager._graph).toBe(graph);
            expect(e.monitors[0]._graph).toBe(graph);
            expect(e.monitors[1]._graph).toBe(graph);
        });

        it('should pass on empty values', function () {
            expect(spEntity.toJSON()).toBeUndefined();
            expect(spEntity.toJSON(null)).toBeNull();
        });

        it('should flag cycles as errors', function () {
            var json = {
                id: 1,
                parent: {
                    id: 2,
                    children: []
                }
            };

            var e = spEntity.fromJSON(json);
            e.getParent().setChildren([1]);

            var json2 = spEntity.toJSON(e);
            expect(json2.parent.children[0]._warnings).toBeTruthy();
        });
    });

});
