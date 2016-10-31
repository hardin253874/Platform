// Copyright 2011-2016 Global Software Innovation Pty Ltd
/* globals spEntity */

describe('Entity Model|spEntity|spec:', function () {
    'use strict';


    describe('Decode network response v2|entityDataVer2ToEntities', function () {

        // Introduction Tests

        it('should exist', function () {
            expect(spEntity.entityDataVer2ToEntities).toBeTruthy();
        });

        it('should parse simple data', function () {
            var data = { "ids":["16553"], "entities": { "16553": { "7820": "AA_Manager" } }, "members": { "7820": { "alias": "core:name", "dt": "String" } } };
            var res = spEntity.entityDataVer2ToEntities(data);
            expect(res).toBeTruthy();
        });

        it('should parse complex data', function () {
            var dataAf02 = { "ids": ["16523"], "entities": { "16523": { "7820": "Test 02", "9476": "Description of Test 2", "15805": 200, "16846": true, "16369": 200.2220000000, "17182": 200.2000000000, "16140": "1753-01-01T02:00:00", "17407": "2013-06-02T00:00:00", "17451": "2013-06-01T16:00:00", "16861": "data 02", "16515": "multi \rtext \rfor \rTest 02", "16495": { "f": [16361] }, "16615": { "f": [15883, 17025, 17351, 17428] }, "17611": { "f": [16299, 17069, 17168] }, "15948": { "f": [17571] } }, "16361": { "7820": "Peter Aylett", "9476": "", "16495": { "r": [16523] } }, "15883": { "7820": "American Graffiti", "16615": { "r": [16523] } }, "17025": { "7820": "Airborne Ranger", "16615": { "r": [16523, 16714] } }, "17351": { "7820": "AMSOIL Shock Therapy", "16615": { "r": [16010, 16523] } }, "17428": { "7820": "American Guardian", "16615": { "r": [16523] } }, "16299": { "7820": "Chives", "17611": { "r": [15871, 16023, 16189, 16523, 17167, 17260] } }, "17069": { "7820": "Cardamon", "17611": { "r": [15871, 16523, 17167] } }, "17168": { "7820": "Bay Leaf", "17611": { "r": [16086, 16523, 16714] } }, "17571": { "7820": "Coke Zero", "15948": { "r": [16523] } } }, "members": { "7820": { "alias": "core:name", "dt": "String" }, "9476": { "alias": "core:description", "dt": "String" }, "15805": { "alias": "test:afNumber", "dt": "Int32" }, "16846": { "alias": "test:afBoolean", "dt": "Bool" }, "16369": { "alias": "test:afDecimal", "dt": "Decimal" }, "17182": { "alias": "test:afCurrency", "dt": "Currency" }, "16140": { "alias": "test:afTime", "dt": "Time" }, "17407": { "alias": "test:afDate", "dt": "Date" }, "17451": { "alias": "test:afDateTime", "dt": "DateTime" }, "16861": { "alias": "test:afString", "dt": "String" }, "16515": { "alias": "test:afMultiline", "dt": "String" }, "16495": { "frel": { "alias": "test:allFieldsEmployee", "isLookup": true }, "rrel": { "alias": "test:employeeAllFields", "isLookup": false } }, "16615": { "frel": { "alias": "test:trucks", "isLookup": false }, "rrel": { "alias": "test:truckAllFields", "isLookup": true } }, "17611": { "frel": { "alias": "test:herbs", "isLookup": false }, "rrel": { "alias": "test:herbAllFields", "isLookup": false } }, "15948": { "frel": { "alias": "test:drinks", "isLookup": true }, "rrel": { "alias": "test:drinkAllFields", "isLookup": true } } } };
            var res = spEntity.entityDataVer2ToEntities(dataAf02);
            expect(res).toBeTruthy();
        });

        it('should parse complex data with nulls and empties', function () {
            var dataAf30 = { "ids": ["17595"], "entities": { "17595": { "7820": "Test 30", "9476": "", "15805": null, "16846": false, "16369": null, "17182": null, "16140": null, "17407": null, "17451": null, "16861": "", "16515": "", "16495": { "f": [] }, "16615": { "f": [] }, "17611": { "f": [] }, "15948": { "f": [] } } }, "members": { "7820": { "alias": "core:name", "dt": "String" }, "9476": { "alias": "core:description", "dt": "String" }, "15805": { "alias": "test:afNumber", "dt": "Int32" }, "16846": { "alias": "test:afBoolean", "dt": "Bool" }, "16369": { "alias": "test:afDecimal", "dt": "Decimal" }, "17182": { "alias": "test:afCurrency", "dt": "Currency" }, "16140": { "alias": "test:afTime", "dt": "Time" }, "17407": { "alias": "test:afDate", "dt": "Date" }, "17451": { "alias": "test:afDateTime", "dt": "DateTime" }, "16861": { "alias": "test:afString", "dt": "String" }, "16515": { "alias": "test:afMultiline", "dt": "String" }, "16495": { "frel": { "alias": "test:allFieldsEmployee", "isLookup": true } }, "16615": { "frel": { "alias": "test:trucks", "isLookup": false } }, "17611": { "frel": { "alias": "test:herbs", "isLookup": false } }, "15948": { "frel": { "alias": "test:drinks", "isLookup": true } } } };
            var res = spEntity.entityDataVer2ToEntities(dataAf30);
            expect(res).toBeTruthy();
        });

        it('should parse complex data2', function () {
            var dataAf02 = { "ids": ["16523"], "entities": { "16523": { "7820": "Test 02", "9476": "Description of Test 2" } }, "members": { "7820": { "alias": "core:name", "dt": "String" }, "9476": { "alias": "core:description", "dt": "String" } } };
            var entities = spEntity.entityDataVer2ToEntities(dataAf02);
            expect(entities[0].getField('name')).toEqual('Test 02');
        });

        
    });
});