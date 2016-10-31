// Copyright 2011-2016 Global Software Innovation Pty Ltd
var reportTestData;
(function (reportTestData) {    
    // GET:
    // https://syd1dev20.entdata.local/spapi/data/v1/report/4190?metadata=full&page=0,1000
    // WARNING .. this data is no longer representative of the current format
    reportTestData.allFields =
    {
        "meta": {
            "title": "All Fields",
            "rcols": {
                "016904e2-ba8b-4ec8-8264-6ee0da2136ee": {
                    "ord": 0,
                    "title": "All Fields",
                    "type": "String",
                    "fid": 1697,
                    "maxlen": 200,
                    "regex": "^[^<>]+$",
                    "regexerr": "The field cannot contain angled brackets."
                },
                "58870551-0c28-499f-9196-863e89abf3a1": {
                    "ord": 1,
                    "title": "Currency",
                    "type": "Currency",
                    "fid": 4744,
                    "places": 3
                },
                "21b3d645-b9dc-4e50-b288-07cccbe7a442": {
                    "ord": 2,
                    "title": "Number",
                    "type": "Int32",
                    "fid": 3578,
                    "ro": true
                },
                "9456e2ff-c91a-488f-b772-496ce85f0b24": {
                    "ord": 3,
                    "title": "Decimal",
                    "type": "Decimal",
                    "fid": 4743,
                    "places": 3
                },
                "b4678851-906c-4e54-ac0d-3c36385e6748": {
                    "ord": 4,
                    "title": "Date",
                    "type": "Date",
                    "fid": 4209
                },
                "f8a60330-d8f0-47c4-8fa4-398171963c7d": {
                    "ord": 5,
                    "title": "Time",
                    "type": "Time",
                    "fid": 3995
                },
                "dca7fdb9-47cb-46f4-8144-0898dac9bae2": {
                    "ord": 6,
                    "title": "Date Time",
                    "type": "DateTime",
                    "fid": 3832
                },
                "fcabce13-f425-4b14-a414-0411bba78875": {
                    "ord": 7,
                    "title": "Single Text",
                    "type": "String",
                    "fid": 4761
                },
                "757bcb55-6933-4136-8038-589311d21c5f": {
                    "ord": 8,
                    "title": "Multi Text",
                    "type": "String",
                    "fid": 4525,
                    "mline": true
                },
                "b0687f2b-c0e3-4e26-99b5-1afa220f5a04": {
                    "ord": 9,
                    "title": "Yes No",
                    "type": "Bool",
                    "fid": 4564,
                    "ro": true
                },
                "87f274c7-f65e-4225-86fc-ef011364e734": {
                    "ord": 10,
                    "title": "Priority - Choice",
                    "type": "RelatedResource",
                    "fid": 1697,
                    "maxlen": 200,
                    "regex": "^[^<>]+$",
                    "regexerr": "The field cannot contain angled brackets."
                },
                "517e4030-fc95-4c5d-af97-930f19a8de92": {
                    "ord": 11,
                    "title": "Person",
                    "type": "RelatedResource",
                    "fid": 1697,
                    "maxlen": 200,
                    "regex": "^[^<>]+$",
                    "regexerr": "The field cannot contain angled brackets."
                }
            },
            "sort": [
                {
                    "colid": "016904e2-ba8b-4ec8-8264-6ee0da2136ee",
                    "order": "Ascending"
                }
            ],
            "anlcols": {
                "bd1a1b7f-4c5f-47a7-be7f-54d06aa42ace": {
                    "ord": 0,
                    "title": "All Fields",
                    "type": "String",
                    "doper": "Contains"                    
                },
                "edfda0bb-60d2-4ab2-9afd-bb316463aa91": {
                    "ord": 1,
                    "title": "Currency",
                    "type": "Currency",
                    "doper": "GreaterThan"                    
                },
                "3098a000-ee57-4bae-b19f-12ae971cacb4": {
                    "ord": 2,
                    "title": "Number",
                    "type": "Int32",
                    "doper": "GreaterThan"
                },
                "e674bdf0-29d4-4a34-a715-c0ea257dd098": {
                    "ord": 3,
                    "title": "Decimal",
                    "type": "Decimal",
                    "doper": "GreaterThan"                    
                },
                "ecdeb8d4-948b-4903-95c5-aea32d7aebff": {
                    "ord": 4,
                    "title": "Date",
                    "type": "Date",
                    "doper": "GreaterThan"
                },
                "52e8e062-8319-4e31-9412-beef4a1e405d": {
                    "ord": 5,
                    "title": "Time",
                    "type": "Time",
                    "doper": "GreaterThan"                    
                },
                "ccb44106-fc37-434d-aed8-2100a47a1501": {
                    "ord": 6,
                    "title": "Date Time",
                    "type": "DateTime",
                    "doper": "GreaterThan"                    
                },
                "eb8c33b1-d510-4e7f-973f-99f2a828f867": {
                    "ord": 7,
                    "title": "Single Text",
                    "type": "String",
                    "doper": "Contains"                    
                },
                "3568512f-1dca-4878-8d96-fb946b2483a3": {
                    "ord": 8,
                    "title": "Multi Text",
                    "type": "String",
                    "doper": "Contains"                    
                },
                "36e31d74-a0b3-4681-9120-fc94bc3ccc8c": {
                    "ord": 9,
                    "title": "Yes No",
                    "type": "Bool",
                    "doper": "Equal"                    
                },
                "258d6ba8-0d6d-4f43-a699-a34f4ce0547c": {
                    "ord": 10,
                    "title": "Priority - Choice",
                    "type": "ChoiceRelationship",
                    "doper": "AnyOf",                    
                },
                "d32bb32b-7ccf-43a4-9534-ca971a9cd658": {
                    "ord": 11,
                    "title": "Person",
                    "type": "InlineRelationship",
                    "doper": "AnyOf",                    
                    "rpt": 2981,
                    "rpttype": 1840
                }
            }
        },
        "gdata": [
            {
                "eid": 4279,
                "values": [
                    {
                        "val": "AF 1"
                    },
                    {
                        "val": "100.100"
                    },
                    {
                        "val": "100"
                    },
                    {
                        "val": "100.110"
                    },
                    {
                        "val": "2012-12-01T00:00:00.0000000"
                    },
                    {
                        "val": "1753-01-01T13:00:00.0000000"
                    },
                    {
                        "val": "2012-12-01T02:00:00.0000000"
                    },
                    {
                        "val": "single odd"
                    },
                    {
                        "val": "\n      multi\n      text\n      here\n    "
                    },
                    {
                        "val": "False"
                    },
                    {
                        "vals": {
                            "3493": "Low"
                        }
                    },
                    {
                        "vals": {
                            "4566": "Tina Ballarina"
                        }
                    }
                ]
            },
            {
                "eid": 4266,
                "values": [
                    {
                        "val": "AF 2"
                    },
                    {
                        "val": "200.200"
                    },
                    {
                        "val": "200"
                    },
                    {
                        "val": "200.220"
                    },
                    {
                        "val": "2012-12-02T00:00:00.0000000"
                    },
                    {
                        "val": "1753-01-01T02:00:00.0000000"
                    },
                    {
                        "val": "2012-12-01T15:00:00.0000000"
                    },
                    {
                        "val": "single even"
                    },
                    {
                        "val": "\n      multi\n      text\n      here\n    "
                    },
                    {
                        "val": "True"
                    },
                    {
                        "vals": {
                            "3493": "Low"
                        }
                    },
                    {
                        "vals": {
                            "3639": "Peter Aylett"
                        }
                    }
                ]
            },
            {
                "eid": 3998,
                "values": [
                    {
                        "val": "AF 3"
                    },
                    {
                        "val": "300.300"
                    },
                    {
                        "val": "300"
                    },
                    {
                        "val": "300.330"
                    },
                    {
                        "val": "2012-12-03T00:00:00.0000000"
                    },
                    {
                        "val": "1753-01-01T15:00:00.0000000"
                    },
                    {
                        "val": "2012-12-03T04:00:00.0000000"
                    },
                    {
                        "val": "single odd"
                    },
                    {
                        "val": "\n      multi\n      text\n      here\n    "
                    },
                    {
                        "val": "False"
                    },
                    {
                        "vals": {
                            "3171": "Medium"
                        }
                    },
                    {
                        "vals": {
                            "4696": "Nino Carabella"
                        }
                    }
                ]
            },
            {
                "eid": 3879,
                "values": [
                    {
                        "val": "AF 4"
                    },
                    {
                        "val": "400.400"
                    },
                    {
                        "val": "400"
                    },
                    {
                        "val": "400.440"
                    },
                    {
                        "val": "2012-12-04T00:00:00.0000000"
                    },
                    {
                        "val": "1753-01-01T04:00:00.0000000"
                    },
                    {
                        "val": "2012-12-03T17:00:00.0000000"
                    },
                    {
                        "val": "single even"
                    },
                    {
                        "val": "\n      multi\n      text\n      here\n    "
                    },
                    {
                        "val": "True"
                    },
                    {
                        "vals": {
                            "3335": "High"
                        }
                    },
                    {
                        "vals": {
                            "4407": "Conrad Black"
                        }
                    }
                ]
            },
            {
                "eid": 4726,
                "values": [
                    {
                        "val": "AF 5"
                    },
                    {
                        "val": "500.500"
                    },
                    {
                        "val": "500"
                    },
                    {
                        "val": "500.560"
                    },
                    {
                        "val": "2012-12-05T00:00:00.0000000"
                    },
                    {
                        "val": "1753-01-01T17:00:00.0000000"
                    },
                    {
                        "val": "2012-12-05T06:00:00.0000000"
                    },
                    {
                        "val": "single odd"
                    },
                    {
                        "val": "\n      multi\n      text\n      here\n    "
                    },
                    {
                        "val": "False"
                    },
                    {
                        "vals": {
                            "3259": "Critical"
                        }
                    },
                    {
                        "vals": {
                            "3871": "Kenny Siu"
                        }
                    }
                ]
            }
        ]
    };
})(reportTestData || (reportTestData = {}));