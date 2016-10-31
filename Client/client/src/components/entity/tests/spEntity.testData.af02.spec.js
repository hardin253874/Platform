// Copyright 2011-2016 Global Software Innovation Pty Ltd
var entityTestData;
(function (entityTestData) {
    // POST:
    /* https://syd1dev19.entdata.local/spapi/data/v2/entity
    {
        queries: [ 'alias,name,isOfType.{alias}, description, test:afNumber, test:afBoolean, test:afDecimal, test:afCurrency, test:afTime, test:afDate, test:afDateTime, test:afString, test:afMultiline, test:allFieldsEmployee.{name, description, test:employeeAllFields.name }, test:trucks.{name, test:truckAllFields.id }, test:herbs.{name, test:herbAllFields.id }, test:drinks.{name, test:drinkAllFields.id }' ],
        requests: [{ rq:0, get:'basic', ids:['test:af02'] }]
    }
    */
    // GET:
    // https://syd1dev19.entdata.local/spapi/data/v2/entity/test/af02?request=alias,name,isOfType.%7balias%7d,+description,+test:afNumber,+test:afBoolean,+test:afDecimal,+test:afCurrency,+test:afTime,+test:afDate,+test:afDateTime,+test:afString,+test:afMultiline,+test:allFieldsEmployee.%7Bname,+description,+test:employeeAllFields.name+%7D,+test:trucks.%7Bname,+test:truckAllFields.id+%7D,+test:herbs.%7Bname,+test:herbAllFields.id+%7D,+test:drinks.%7Bname,+test:drinkAllFields.id+%7D
    entityTestData.af02 =
    {
        "results": [
            {
                "code": 200,
                "ids": [
                    16545
                ]
            }
        ],
        "ids": [],
        "entities": {
            "15905": {
                "7765": "American Graffiti",
                "16637": {
                    "r": [
                        16545
                    ]
                }
            },
            "16321": {
                "7765": "Chives",
                "17633": {
                    "r": [
                        15893,
                        16045,
                        16211,
                        16545,
                        17189,
                        17282
                    ]
                }
            },
            "16383": {
                "7765": "Peter Aylett",
                "9446": null,
                "16517": {
                    "r": [
                        16545
                    ]
                }
            },
            "16545": {
                "7765": "Test 02",
                "8870": "test:af02",
                "9446": "Description of Test 2",
                "10965": {
                    "f": [
                        17016
                    ]
                },
                "15827": 200,
                "15970": {
                    "f": [
                        17593
                    ]
                },
                "16162": "1753-01-01T02:00:00",
                "16391": 200.222,
                "16517": {
                    "f": [
                        16383
                    ]
                },
                "16537": "multi \rtext \rfor \rTest 02",
                "16637": {
                    "f": [
                        15905,
                        17047,
                        17373,
                        17450
                    ]
                },
                "16868": true,
                "16883": "data 02",
                "17204": 200.2,
                "17429": "2013-06-02T00:00:00",
                "17473": "2013-06-01T16:00:00",
                "17633": {
                    "f": [
                        16321,
                        17091,
                        17190
                    ]
                }
            },
            "17016": {
                "8870": "test:allFields"
            },
            "17047": {
                "7765": "Airborne Ranger",
                "16637": {
                    "r": [
                        16545,
                        16736
                    ]
                }
            },
            "17091": {
                "7765": "Cardamon",
                "17633": {
                    "r": [
                        15893,
                        16545,
                        17189
                    ]
                }
            },
            "17190": {
                "7765": "Bay Leaf",
                "17633": {
                    "r": [
                        16108,
                        16545,
                        16736
                    ]
                }
            },
            "17373": {
                "7765": "AMSOIL Shock Therapy",
                "16637": {
                    "r": [
                        16032,
                        16545
                    ]
                }
            },
            "17450": {
                "7765": "American Guardian",
                "16637": {
                    "r": [
                        16545
                    ]
                }
            },
            "17593": {
                "7765": "Coke Zero",
                "15970": {
                    "r": [
                        16545
                    ]
                }
            }
        },
        "members": {
            "7765": {
                "alias": "core:name",
                "dt": "String"
            },
            "8870": {
                "alias": "core:alias",
                "dt": "String"
            },
            "9446": {
                "alias": "core:description",
                "dt": "String"
            },
            "10965": {
                "frel": {
                    "alias": "core:isOfType",
                    "isLookup": false
                }
            },
            "15827": {
                "alias": "test:afNumber",
                "dt": "Int32"
            },
            "15970": {
                "frel": {
                    "alias": "test:drinks",
                    "isLookup": true
                },
                "rrel": {
                    "alias": "test:drinkAllFields",
                    "isLookup": true
                }
            },
            "16162": {
                "alias": "test:afTime",
                "dt": "Time"
            },
            "16391": {
                "alias": "test:afDecimal",
                "dt": "Decimal"
            },
            "16517": {
                "frel": {
                    "alias": "test:allFieldsEmployee",
                    "isLookup": true
                },
                "rrel": {
                    "alias": "test:employeeAllFields",
                    "isLookup": false
                }
            },
            "16537": {
                "alias": "test:afMultiline",
                "dt": "String"
            },
            "16637": {
                "frel": {
                    "alias": "test:trucks",
                    "isLookup": false
                },
                "rrel": {
                    "alias": "test:truckAllFields",
                    "isLookup": true
                }
            },
            "16868": {
                "alias": "test:afBoolean",
                "dt": "Bool"
            },
            "16883": {
                "alias": "test:afString",
                "dt": "String"
            },
            "17204": {
                "alias": "test:afCurrency",
                "dt": "Currency"
            },
            "17429": {
                "alias": "test:afDate",
                "dt": "Date"
            },
            "17473": {
                "alias": "test:afDateTime",
                "dt": "DateTime"
            },
            "17633": {
                "frel": {
                    "alias": "test:herbs",
                    "isLookup": false
                },
                "rrel": {
                    "alias": "test:herbAllFields",
                    "isLookup": false
                }
            }
        }
    };


})(entityTestData || (entityTestData = {}));
