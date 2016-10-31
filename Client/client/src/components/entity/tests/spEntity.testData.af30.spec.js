// Copyright 2011-2016 Global Software Innovation Pty Ltd
var entityTestData;
(function (entityTestData) {
    // https://syd1dev19.entdata.local/spapi/data/v2/entity/test/af30?request=alias,name,isOfType.%7balias%7d,+description,+test:afNumber,+test:afBoolean,+test:afDecimal,+test:afCurrency,+test:afTime,+test:afDate,+test:afDateTime,+test:afString,+test:afMultiline,+test:allFieldsEmployee.%7Bname,+description,+test:employeeAllFields.name+%7D,+test:trucks.%7Bname,+test:truckAllFields.id+%7D,+test:herbs.%7Bname,+test:herbAllFields.id+%7D,+test:drinks.%7Bname,+test:drinkAllFields.id+%7D
    // This resource contains all empty data
    entityTestData.af30 =
    {
        "ids": [
          17595
        ],
        "entities": {
            "17595": {
                "8947": "test:af30",
                "7820": "Test 30",
                "9476": null,
                "15805": null,
                "16846": false,
                "16369": null,
                "17182": null,
                "16140": null,
                "17407": null,
                "17451": null,
                "16861": null,
                "16515": null,
                "10957": {
                    "f": [
                      16994
                    ]
                },
                "16495": {
                    "f": [

                    ]
                },
                "16615": {
                    "f": [

                    ]
                },
                "17611": {
                    "f": [

                    ]
                },
                "15948": {
                    "f": [

                    ]
                }
            },
            "16994": {
                "8947": "test:allFields"
            }
        },
        "members": {
            "8947": {
                "alias": "core:alias",
                "dt": "String"
            },
            "7820": {
                "alias": "core:name",
                "dt": "String"
            },
            "9476": {
                "alias": "core:description",
                "dt": "String"
            },
            "15805": {
                "alias": "test:afNumber",
                "dt": "Int32"
            },
            "16846": {
                "alias": "test:afBoolean",
                "dt": "Bool"
            },
            "16369": {
                "alias": "test:afDecimal",
                "dt": "Decimal"
            },
            "17182": {
                "alias": "test:afCurrency",
                "dt": "Currency"
            },
            "16140": {
                "alias": "test:afTime",
                "dt": "Time"
            },
            "17407": {
                "alias": "test:afDate",
                "dt": "Date"
            },
            "17451": {
                "alias": "test:afDateTime",
                "dt": "DateTime"
            },
            "16861": {
                "alias": "test:afString",
                "dt": "String"
            },
            "16515": {
                "alias": "test:afMultiline",
                "dt": "String"
            },
            "10957": {
                "frel": {
                    "alias": "core:isOfType",
                    "isLookup": false
                }
            },
            "16495": {
                "frel": {
                    "alias": "test:allFieldsEmployee",
                    "isLookup": true
                }
            },
            "16615": {
                "frel": {
                    "alias": "test:trucks",
                    "isLookup": false
                }
            },
            "17611": {
                "frel": {
                    "alias": "test:herbs",
                    "isLookup": false
                }
            },
            "15948": {
                "frel": {
                    "alias": "test:drinks",
                    "isLookup": true
                }
            }
        }
    };


})(entityTestData || (entityTestData = {}));
