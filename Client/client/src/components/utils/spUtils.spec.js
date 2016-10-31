// Copyright 2011-2016 Global Software Innovation Pty Ltd
/*global sp, _, describe, it, beforeEach, TestSupport, expect */

describe('Internal|spUtils library|spec:', function () {
    'use strict';

    beforeEach(function () {
        this.addMatchers(TestSupport.matchers);
    });

    describe('sp.pushArray', function () {
        it('should work', function () {
            var arr1 = [1, 2];
            var arr2 = [3, 4];
            var arrX = arr1;
            sp.pushArray(arr1, arr2);
            expect(arr1).toBeArray(4);
            expect(arr1).toBe(arrX);
        });
        it('should handle nulls, etc', function () {
            var arr1 = [1, 2];
            sp.pushArray(arr1);
            sp.pushArray(arr1, null);
            expect(arr1).toBeArray(2);
        });
    });

    describe('findByKey', function () {
        it('should exist', function () {
            expect(sp.findByKey).toBeTruthy();
        });
        it('should find values', function () {
            var data = [{a: 1}, {a: 2}, {a: 3}];
            var found = sp.findByKey(data, 'a', 2);
            expect(found.a).toBe(2);
        });
        it('should return undefined if not found', function () {
            var data = [{a: 1}, {a: 2}, {a: 3}];
            var found = sp.findByKey(data, 'a', 4);
            expect(found).toBeUndefined();
        });
        it('should still work if key is missing on some objects', function () {
            var data = [{x: 1}, {a: 2}, {a: 3}];
            var found = sp.findByKey(data, 'a', 2);
            expect(found.a).toBe(2);
        });
    });

    describe('stringToBoolean', function () {
        it('should exist', function () {
            expect(sp.stringToBoolean).toBeTruthy();
        });
        it('should accept true values', function () {
            expect(sp.stringToBoolean('true')).toBeTruthy();
            expect(sp.stringToBoolean('yes')).toBeTruthy();
            expect(sp.stringToBoolean('1')).toBeTruthy();
        });
        it('should accept false values', function () {
            expect(sp.stringToBoolean('false')).toBeFalsy();
            expect(sp.stringToBoolean('no')).toBeFalsy();
            expect(sp.stringToBoolean('0')).toBeFalsy();
            expect(sp.stringToBoolean(null)).toBeFalsy();
        });
        it('should fall back to default', function () {
            // But don't rely on it.
            expect(sp.stringToBoolean('Y')).toBeTruthy();
            expect(sp.stringToBoolean('N')).toBeTruthy();
            expect(sp.stringToBoolean('')).toBeFalsy();
        });
    });

    describe('doWhile', function () {
        it('should exist', function () {
            expect(sp.doWhile).toBeTruthy();
        });
        it('should run', function () {
            var value = 0;
            sp.doWhile(function () {
                value++;
                return value < 3;
            });
            expect(value).toBe(3);
        });
    });

    describe('capitaliseFirstLetter', function () {
        it('should exist', function () {
            expect(sp.capitaliseFirstLetter).toBeTruthy();
        });
        it('should capitalize the first letter', function () {
            expect(sp.capitaliseFirstLetter('abcDef')).toEqual('AbcDef');
        });
        it('should pass on nulls', function () {
            expect(sp.capitaliseFirstLetter(null)).toBeNull();
        });
        it('can handle empty', function () {
            expect(sp.capitaliseFirstLetter('')).toEqual('');
        });
    });

    describe('stringIsNumber', function () {
        it('should exist', function () {
            expect(sp.stringIsNumber).toBeTruthy();
        });
        it('should detect number string', function () {
            expect(sp.stringIsNumber('123')).toBeTruthy();
        });
        it('should detect zero string', function () {
            expect(sp.stringIsNumber('0')).toBeTruthy();
        });
        it('should not detect null', function () {
            expect(sp.stringIsNumber(null)).toBeFalsy();
        });
        it('should not detect empty', function () {
            expect(sp.stringIsNumber('')).toBeFalsy();
        });
        it('should not detect undefined', function () {
            expect(sp.stringIsNumber()).toBeFalsy();
        });
        it('should not detect text', function () {
            expect(sp.stringIsNumber('abc')).toBeFalsy();
        });
    });

    describe('walkGraph', function () {

        var graph;
        var walkRels = function (node) {
            return _.map(node.to, function (id) {
                return _.find(graph, function (n) {
                    return n.id === id;
                });
            });
        };
        var format = function (res) {
            return _.map(res, function (n) {
                return n.id;
            }).sort().join(',');
        };

        it('should exist', function () {
            expect(sp.walkGraph).toBeTruthy();
        });
        it('should handle empty', function () {
            graph = [];
            var res = sp.walkGraph(walkRels, []);
            expect(res).toBeEmptyArray();
        });
        it('should walk basic graph using single root', function () {
            graph = [
                {id: 1, to: [2]},
                {id: 2, to: [3]},
                {id: 3, to: []},
                {id: 4, to: [1]}
            ];
            var res = sp.walkGraph(walkRels, graph[0]);
            expect(format(res)).toBe('1,2,3');
        });
        it('should walk basic graph with cycle', function () {
            graph = [
                {id: 1, to: [2]},
                {id: 2, to: [3]},
                {id: 3, to: [1]}
            ];
            var res = sp.walkGraph(walkRels, graph[0]);
            expect(format(res)).toBe('1,2,3');
        });
        it('should walk graph with multiple paths', function () {
            graph = [
                {id: 1, to: [2, 3]},
                {id: 2, to: [3, 4]},
                {id: 3, to: [1, 2, 5]},
                {id: 4, to: [5]},
                {id: 5, to: [1]}
            ];
            var res = sp.walkGraph(walkRels, graph[0]);
            expect(format(res)).toBe('1,2,3,4,5');
        });
        it('should return empty array if startNodes is null', function () {
            expect(sp.walkGraph(walkRels, null)).toBeEmptyArray();
        });
        it('should throw if fnGetRelated argument is null', function () {
            graph = [
                {id: 1, to: [1]}
            ];
            expect(function () {
                sp.walkGraph(null, graph[0]);
            }).toThrow(new Error('fnGetRelated is null'));
        });
    });

    describe('walkGraphSorted', function () {

        var graph;
        var walkRels = function (node) {
            return _.map(node.to, function (id) {
                return _.find(graph, function (n) {
                    return n.id === id;
                });
            });
        };
        var format = function (res) {
            return _.map(res, function (n) {
                return n.id;
            }).sort().join(',');
        };

        it('should exist', function () {
            expect(sp.walkGraphSorted).toBeTruthy();
        });
        it('should handle empty', function () {
            graph = [];
            var res = sp.walkGraphSorted(walkRels, []);
            expect(res).toBeEmptyArray();
        });
        it('should walk basic graph using single root', function () {
            graph = [
                {id: 1, to: [2]},
                {id: 2, to: [3]},
                {id: 3, to: []},
                {id: 4, to: [1]}
            ];
            var res = sp.walkGraphSorted(walkRels, graph[0]);
            expect(format(res)).toBe('1,2,3');
        });
        it('should walk basic graph with cycle', function () {
            graph = [
                {id: 1, to: [2]},
                {id: 2, to: [3]},
                {id: 3, to: [1]}
            ];
            var res = sp.walkGraphSorted(walkRels, graph[0]);
            expect(format(res)).toBe('1,2,3');
        });
        it('should walk graph with multiple paths', function () {
            graph = [
                {id: 1, to: [2, 3]},
                {id: 2, to: [3, 4]},
                {id: 3, to: [1, 2, 5]},
                {id: 4, to: [5]},
                {id: 5, to: [1]}
            ];
            var res = sp.walkGraphSorted(walkRels, graph[0]);
            expect(format(res)).toBe('1,2,3,4,5');
        });
        it('should always put root last test1', function () {
            graph = [
                {id: 1, to: [2, 3]},
                {id: 2, to: [3]},
                {id: 3}
            ];
            var res = sp.walkGraphSorted(walkRels, graph[0]);
            expect(format(res)).toBe('1,2,3');
        });
        it('should always put root last test2', function () {
            graph = [
                {id: 1, to: [3, 2]},
                {id: 2, to: [3]},
                {id: 3}
            ];
            var res = sp.walkGraphSorted(walkRels, graph[0]);
            expect(format(res)).toBe('1,2,3');
        });
        it('should return empty array if startNodes is null', function () {
            expect(sp.walkGraphSorted(walkRels, null)).toBeEmptyArray();
        });
        it('should throw if fnGetRelated argument is null', function () {
            graph = [
                {id: 1, to: [1]}
            ];
            expect(function () {
                sp.walkGraphSorted(null, graph[0]);
            }).toThrow(new Error('fnGetRelated is null'));
        });
    });

    describe('intersect', function () {

        it('should exist', function () {
            expect(sp.except).toBeTruthy();
        });

        it('should run', function () {
            var arr1 = [{id: 1}, {id: 2}, {id: 3}, {id: 4}];
            var arr2 = [2, 3];
            var res = sp.intersect(arr1, arr2, function (x, y) {
                return x.id === y;
            });
            expect(res).toBeArray(2);
            expect(res[0].id).toBe(2);
            expect(res[1].id).toBe(3);
        });
    });

    describe('except', function () {

        it('should exist', function () {
            expect(sp.except).toBeTruthy();
        });

        it('should run', function () {
            var arr1 = [{id: 1}, {id: 2}, {id: 3}, {id: 4}];
            var arr2 = [2, 3];
            var res = sp.except(arr1, arr2, function (x, y) {
                return x.id === y;
            });
            expect(res).toBeArray(2);
            expect(res[0].id).toBe(1);
            expect(res[1].id).toBe(4);
        });
    });

    describe('extended "result" function', function () {
        var test = {
            a: 33,
            b: {
                c: 'baby',
                d: function () {
                    return 'hey';
                },
                arr: ['a', 'b', 'c']
            },
            o: {
                '99': 'hello',
                'core:aaa': 'entity alias'
            }
        };
        it('works as expected to retrieve values', function () {
            expect(sp.result(test, ['a'])).toBe(33);
            expect(sp.result(test, 'a')).toBe(33);
            expect(sp.result(test, 'b.c')).toBe('baby');
            expect(sp.result(test, ['b', 'd'])).toBe('hey');
        });
        it('doesn\'t crash on nulls', function () {
            expect(sp.result(test, ['z'])).toBeUndefined();
            expect(sp.result(test, 'b.not_here.c')).toBeUndefined();
            expect(sp.result(test)).toBeUndefined();
            expect(sp.result()).toBeUndefined();
        });
        it('can handle array indexes', function () {
            expect(sp.result(test, 'b.arr.1')).toBe('b');
        });
        it('can handle other styles of object prop refs', function () {
            expect(sp.result(test, 'o.99')).toBe('hello');
            expect(sp.result(test, 'o.core:aaa')).toBe('entity alias');
        });
    });

    describe('extended "result" and "pluckresult" function', function () {
        var arr = [
            {
                b: {
                    c: 'baby',
                    d: function () {
                        return 'hey';
                    },
                    a: ['a', 'b', 'c']
                }
            },
            {
                b: {
                    c: 'mama',
                    d: function () {
                        return 'there';
                    },
                    a: ['x', 'y', 'z']
                }
            }
        ];
        it('works as expected to retrieve values', function () {
            expect(sp.pluckResult(arr, 'b.a.0')).toBeArray(2);
            expect(sp.pluckResult(arr, 'b.a.0')[0]).toBe('a');
            expect(sp.pluckResult(arr, 'b.a.0')[1]).toBe('x');
            expect(sp.pluckResult(arr, 'b.d')[1]).toBe('there');
        });
    });

    describe('asProp function', function () {

        it('works with no additional arguments', function () {
            var val = 0;
            var g = false;
            var s = false;
            var getter = function () {
                g = true;
                return val;
            };
            var setter = function (newValue) {
                s = true;
                val = newValue;
            };
            var obj = sp.asProp(getter, setter);
            expect(obj.value).toBe(0);
            expect(g).toBeTruthy();
            obj.value = 1;
            expect(s).toBeTruthy();
            expect(obj.value).toBe(1);
        });

        it('works with additional arguments', function () {
            var val = 0;
            var g = false;
            var s = false;
            var getter = function (arg1, arg2) {
                if (arg1 === 'x' && arg2 === 'y') {
                    g = true;
                    return val;
                }
                return -1;
            };
            var setter = function (newValue, arg1, arg2) {
                if (arg1 === 'x' && arg2 === 'y') {
                    s = true;
                    val = newValue;
                }
            };
            var obj = sp.asProp(getter, setter, 'x', 'y');
            expect(obj.value).toBe(0);
            expect(g).toBeTruthy();
            obj.value = 1;
            expect(s).toBeTruthy();
            expect(obj.value).toBe(1);
        });


    });

    describe('compareTime function', function () {
        var now = new Date(2017, 11, 1, 10, 32, 11);
        var nowNextYear = new Date(2018, 11, 1, 10, 32, 11);

        var notNow = new Date(2017, 11, 17);


        it('works with one null', function () {
            expect(sp.compareTimeOnly(null, now)).toBeFalsy();
        });

        it('works with two nulls', function () {
            expect(sp.compareTimeOnly(null, null)).toBeTruthy();
        });
        it('works with two different values', function () {
            expect(sp.compareTimeOnly(now, notNow)).toBeFalsy();
        });
        it('works with two times a year apart', function () {
            expect(sp.compareTimeOnly(now, nowNextYear)).toBeTruthy();
        });


    });

    describe('naturalCompare', function () {

        var same = function (s1, s2) {
            var res = sp.naturalCompare(s1, s2);
            expect(res).toBe(0, s1 + ' vs ' + s2);
        };
        var beforeAfter = function (s1, s2) {
            var res1 = sp.naturalCompare(s1, s2);
            expect(res1).toBe(-1, s1 + ' vs ' + s2);
            var res2 = sp.naturalCompare(s2, s1);
            expect(res2).toBe(1, s2 + ' vs ' + s1);
        };

        it('works with nulls and empty', function () {
            same(null, null);
            same(null, '');
            beforeAfter('', 'a');
        });
        it('works with just numbers', function () {
            same('0', '0');
            same('23', '23');
            beforeAfter('4', '213');
            beforeAfter('45', '213');
        });
        it('works with letters', function () {
            same('a', 'a');
            same('ab', 'ab');
            same('Ab', 'aB');
            beforeAfter('abc', 'abd');
            beforeAfter('abc', 'abcd');
        });
        it('works with number/letter combos', function () {
            same('a0', 'a0');
            expect(sp.naturalCompare('abc123', 'abc45')).toBe(1);
            same('abc1', 'abc1');
            same('abc12', 'abc12');
            same('abc1.', 'abc1.');
            beforeAfter('abc3.', 'abc12.');
            beforeAfter('12.9', '12.37');
            beforeAfter('12 9', '12 37');
        });
        it('works characters and spaces', function () {
            same('a@', 'A@');
            beforeAfter('@.', '@_');
            beforeAfter('test one', 'test@');
            beforeAfter('test one', 'test_one');
            beforeAfter('a_test', 'aa_test');
            beforeAfter('a3_test', 'a12_test');
            beforeAfter('test1.1', 'test2');
            beforeAfter('test1.1', 'test10');
            beforeAfter('test1', 'test1.1');
            beforeAfter('test1x', 'test11x');
            beforeAfter('test_one', 'test1.1');
            beforeAfter('test one', 'test1.1');
        });

    });

    describe('naturalSort', function () {

        it('works', function () {
            var arr = ['test1.1', 'test', 'test_one', 'test one', 'test1', 'test10', 'test2'];
            var res = sp.naturalSort(arr);
            expect(res).not.toBe(arr);
            expect(res.join()).toBe('test,test one,test_one,test1,test1.1,test2,test10');
        });

        it('works with pluck accessor', function () {
            var arr = ['test1.1', 'test', 'test_one', 'test one', 'test1', 'test10', 'test2'];
            var arr2 = _.map(arr, function (s) {
                return {v: s};
            });
            var res = sp.naturalSort(arr2, 'v');
            var res2 = _.map(res, 'v');
            expect(res2.join()).toBe('test,test one,test_one,test1,test1.1,test2,test10');
        });

        it('works with function accessor', function () {
            var arr = ['test1.1', 'test', 'test_one', 'test one', 'test1', 'test10', 'test2'];
            var arr2 = _.map(arr, function (s) {
                return {v: s};
            });
            var res = sp.naturalSort(arr2, function (o) {
                return o.v;
            });
            var res2 = _.map(res, 'v');
            expect(res2.join()).toBe('test,test one,test_one,test1,test1.1,test2,test10');
        });

    });

    describe('toTitleCase', function () {
        it('works', function () {

            var tests = [
                ["Begining a an and as at but by en for if in nor of on or per the to vs. via End", "Begining a an and as at but by en for if in nor of on or per the to vs. via End"],
                ["iPad", "iPad"],
                ["This is very good", "This is Very Good"],
                ["last word is via", "Last Word is Via"],    // small words capitalised at the end of a title https://en.wikipedia.org/wiki/Letter_case#Headings_and_publication_titles " ...but the first word (always) and last word (in many styles) are also capped, regardless of part of speech..." 
                ["What about O'Brian", "What about O'Brian"],
                ["Now let's also check apostrophies", "Now Let's Also Check Apostrophies"],
                ["October the 5th", "October the 5th"],

            ];

            _.map(tests, function (a) {
                var converted = spUtils.toTitleCase(a[0]);
                expect(converted).toBe(a[1]);
            });
        });
    });

    describe('compareVersionStrings', function () {
        it('works', function () {

            var tests = [
                ["1.0.0", "1.0.0", 0],
                ["2.0.0", "1.0.0", 1],
                ["1.0.0", "2.0.0", -1],

                ["0.1.0", "0.1.0", 0],
                ["0.2.0", "0.1.0", 1],
                ["0.1.0", "0.2.0", -1],

                ["0.0.1", "0.0.1", 0],
                ["0.0.2", "0.0.1", 1],
                ["0.0.1", "0.0.2", -1],

                ["2.0.0", "11.0.0", -9],
                ["11.0.0", "2.0.0", 9],

                ["1", "1.0", 0],
                ["1", "1.1", -1],
                ["1.1", "1", 1],

            ];

            _.map(tests, function (a) {
                var converted = spUtils.compareVersionStrings(a[0], a[1]);
                expect(converted).toBe(a[2]);
            });
        });
    });
});
