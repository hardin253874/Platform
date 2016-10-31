// Copyright 2011-2016 Global Software Innovation Pty Ltd
var reportTestData;
(function (reportTestData) {    
    // GET:
    // https://syd1dev20.entdata.local/spapi/data/v1/report/4190?metadata=full&page=0,1000
    reportTestData.anlSimpleChoiceCond = {
        "meta": {
            "title": "ANL Simple Choice Cond",
            "dfid": 5387,
            "typefmtstyle": {
                "String": [
                    "Highlight",
                    "Icon"
                ],
                "Int32": [
                    "Highlight",
                    "Icon",
                    "ProgressBar"
                ],
                "Decimal": [
                    "Highlight",
                    "Icon",
                    "ProgressBar"
                ],
                "Currency": [
                    "Highlight",
                    "Icon",
                    "ProgressBar"
                ],
                "DateTime": [
                    "Highlight",
                    "Icon",
                    "ProgressBar"
                ],
                "Date": [
                    "Highlight",
                    "Icon",
                    "ProgressBar"
                ],
                "Time": [
                    "Highlight",
                    "Icon",
                    "ProgressBar"
                ],
                "Bool": [
                    "Highlight",
                    "Icon"
                ],
                "ChoiceRelationship": [
                    "Highlight",
                    "Icon"
                ]
            },
            "rcols": {
                "8ae9ee00-6f78-4d8d-afbf-fbaef9d65d63": {
                    "ord": 0,
                    "title": "Text Field",
                    "type": "String",
                    "fid": 5495
                },
                "1f68c48a-9e17-4b2f-a67d-028fcaeff4f7": {
                    "ord": 1,
                    "title": "Multiline Text Field",
                    "type": "String",
                    "fid": 6866,
                    "mline": true
                },
                "c0f145dc-97a5-4a6e-ac5a-5723c530821a": {
                    "ord": 2,
                    "title": "Number Field",
                    "type": "Int32",
                    "fid": 4993,
                    "ro": true
                },
                "ee536d56-2b40-40c7-9433-03b9519983db": {
                    "ord": 3,
                    "title": "AutoNumber Field",
                    "type": "Int32",
                    "fid": 6940,
                    "ro": true
                },
                "31340dea-cda9-4ca6-b9e6-e9d0f5da9c1f": {
                    "ord": 4,
                    "title": "Decimal Field",
                    "type": "Decimal",
                    "fid": 4515
                },
                "636c2d31-f59c-42f6-a946-bface6543d00": {
                    "ord": 5,
                    "title": "Currency Field",
                    "type": "Currency",
                    "fid": 7096
                },
                "73a9a773-f0a8-43ca-b5aa-7d86d562233a": {
                    "ord": 6,
                    "title": "Date and Time Field",
                    "type": "DateTime",
                    "fid": 5595
                },
                "cacab7fa-87f1-43bd-b1a3-4a5f1af7b3e5": {
                    "ord": 7,
                    "title": "Date only Field",
                    "type": "Date",
                    "fid": 5412
                },
                "ff902379-8a0e-4567-85f6-35242751d211": {
                    "ord": 8,
                    "title": "Time only Field",
                    "type": "Time",
                    "fid": 5750
                },
                "4b2e7c23-990d-4e08-aad1-5d588ed8b6ea": {
                    "ord": 9,
                    "title": "Yes/No Field",
                    "type": "Bool",
                    "fid": 7277,
                    "ro": true
                },
                "a5b20dde-e782-4454-a29d-bd58506f2d64": {
                    "ord": 10,
                    "title": "Choice Field",
                    "type": "ChoiceRelationship",
                    "fid": 1946,
                    "tid": 5524,
                    "maxlen": 200,
                    "regex": "^[^<>]+$",
                    "regexerr": "The field cannot contain angled brackets."
                }
            },
            "choice": {
                "5524": [
                    {
                        "id": 7155,
                        "name": "One"
                    },
                    {
                        "id": 5267,
                        "name": "Two"
                    },
                    {
                        "id": 5167,
                        "name": "Three"
                    },
                    {
                        "id": 5034,
                        "name": "Four"
                    },
                    {
                        "id": 6132,
                        "name": "Five"
                    },
                    {
                        "id": 5450,
                        "name": "Six"
                    },
                    {
                        "id": 6022,
                        "name": "Seven"
                    },
                    {
                        "id": 5835,
                        "name": "Eight"
                    },
                    {
                        "id": 4195,
                        "name": "Nine"
                    },
                    {
                        "id": 5671,
                        "name": "Alpha"
                    },
                    {
                        "id": 5847,
                        "name": "Bravo"
                    },
                    {
                        "id": 4181,
                        "name": "Charlie"
                    },
                    {
                        "id": 6709,
                        "name": "Delta"
                    },
                    {
                        "id": 5264,
                        "name": "Echo"
                    },
                    {
                        "id": 4522,
                        "name": "Foxtrot"
                    }
                ]
            },
            "anlcols": {
                "958bbd35-7792-4e2f-9fc4-97289ccf2a71": {
                    "ord": 0,
                    "title": "Choice Field",
                    "type": "ChoiceRelationship",
                    "tid": 5524,
                    "doper": "AnyOf"
                },
                "68e8ce36-7d9e-44f3-a7d3-bbde7bc2121d": {
                    "ord": 1,
                    "title": "Yes/No Field",
                    "type": "Bool",
                    "doper": "Equal"
                },
                "3702f89b-70fe-40e9-b10f-71817aa3b07f": {
                    "ord": 2,
                    "title": "Time only Field",
                    "type": "Time",
                    "doper": "GreaterThan"
                },
                "24e4b1a1-252c-4d23-9b5b-91282700ef55": {
                    "ord": 3,
                    "title": "Date only Field",
                    "type": "Date",
                    "doper": "GreaterThan"
                },
                "5463afb9-6b27-4179-aec5-56750c3ae2e1": {
                    "ord": 4,
                    "title": "Date and Time Field",
                    "type": "DateTime",
                    "doper": "GreaterThan"
                },
                "ed6d51ea-cefc-4be3-a8e2-d6826ea98bd5": {
                    "ord": 5,
                    "title": "Currency Field",
                    "type": "Currency",
                    "doper": "GreaterThan"
                },
                "48250f35-467e-4c7a-aba3-1046a31ee609": {
                    "ord": 6,
                    "title": "Decimal Field",
                    "type": "Decimal",
                    "doper": "GreaterThan"
                },
                "f6e1c9e8-55f4-4f46-b320-d4828f9d8e6e": {
                    "ord": 7,
                    "title": "AutoNumber Field",
                    "type": "Int32",
                    "doper": "GreaterThan"
                },
                "c6e42413-ac55-4387-9276-5020a44379e9": {
                    "ord": 8,
                    "title": "Multiline Text Field",
                    "type": "String",
                    "doper": "Contains"
                },
                "6ee7e899-38bf-41d3-b8e3-48f37a4a57fa": {
                    "ord": 9,
                    "title": "Number Field",
                    "type": "Int32",
                    "doper": "GreaterThan"
                },
                "0687109e-6ba5-4a0f-9a6e-2ba893c1f451": {
                    "ord": 10,
                    "title": "Text Field",
                    "type": "String",
                    "doper": "Contains"
                }
            },
            "imgscale": {
                "8ae9ee00-6f78-4d8d-afbf-fbaef9d65d63": {
                    "sizeid": 3415,
                    "scaleid": 2360,
                    "height": 16,
                    "width": 16
                }
            },
            "cfrules": {
                "c0f145dc-97a5-4a6e-ac5a-5723c530821a": {
                    "style": "Highlight",
                    "showval": false,
                    "rules": [
                        {
                            "oper": "LessThan",
                            "val": "4",
                            "fgcolor": {
                                "a": 255,
                                "r": 255,
                                "g": 0,
                                "b": 0
                            },
                            "bgcolor": {
                                "a": 255,
                                "r": 255,
                                "g": 153,
                                "b": 153
                            }
                        },
                        {
                            "oper": "Equal",
                            "val": "6",
                            "fgcolor": {
                                "a": 255,
                                "r": 128,
                                "g": 0,
                                "b": 0
                            },
                            "bgcolor": {
                                "a": 255,
                                "r": 255,
                                "g": 255,
                                "b": 153
                            }
                        },
                        {
                            "oper": "GreaterThanOrEqual",
                            "val": "7",
                            "fgcolor": {
                                "a": 255,
                                "r": 0,
                                "g": 128,
                                "b": 0
                            },
                            "bgcolor": {
                                "a": 255,
                                "r": 153,
                                "g": 255,
                                "b": 153
                            }
                        },
                        {
                            "oper": "Unspecified",
                            "fgcolor": {
                                "a": 255,
                                "r": 0,
                                "g": 0,
                                "b": 128
                            },
                            "bgcolor": {
                                "a": 255,
                                "r": 153,
                                "g": 204,
                                "b": 255
                            }
                        }
                    ]
                },
                "31340dea-cda9-4ca6-b9e6-e9d0f5da9c1f": {
                    "style": "ProgressBar",
                    "showval": false,
                    "rules": [
                        {
                            "oper": "Unspecified",
                            "bgcolor": {
                                "a": 255,
                                "r": 198,
                                "g": 239,
                                "b": 206
                            },
                            "bounds": {
                                "lower": 0,
                                "upper": 1000
                            }
                        }
                    ]
                },
                "636c2d31-f59c-42f6-a946-bface6543d00": {
                    "style": "ProgressBar",
                    "showval": false,
                    "rules": [
                        {
                            "oper": "Unspecified",
                            "bgcolor": {
                                "a": 255,
                                "r": 0,
                                "g": 128,
                                "b": 0
                            },
                            "bounds": {
                                "lower": 0,
                                "upper": 100
                            }
                        }
                    ]
                },
                "8ae9ee00-6f78-4d8d-afbf-fbaef9d65d63": {
                    "style": "Icon",
                    "showval": true,
                    "rules": [
                        {
                            "oper": "StartsWith",
                            "val": "T",
                            "imgid": 2189
                        },
                        {
                            "oper": "StartsWith",
                            "val": "F",
                            "imgid": 2132
                        },
                        {
                            "oper": "Unspecified",
                            "imgid": 2497
                        }
                    ]
                },
                "a5b20dde-e782-4454-a29d-bd58506f2d64": {
                    "style": "Highlight",
                    "showval": true,
                    "rules": [
                        {
                            "oper": "AnyOf",
                            "vals": {
                                "7155": "One"
                            },
                            "fgcolor": {
                                "a": 255,
                                "r": 255,
                                "g": 0,
                                "b": 0
                            },
                            "bgcolor": {
                                "a": 255,
                                "r": 255,
                                "g": 153,
                                "b": 153
                            }
                        },
                        {
                            "oper": "AnyOf",
                            "vals": {
                                "5267": "Two"
                            },
                            "fgcolor": {
                                "a": 255,
                                "r": 128,
                                "g": 0,
                                "b": 0
                            },
                            "bgcolor": {
                                "a": 255,
                                "r": 255,
                                "g": 255,
                                "b": 153
                            }
                        },
                        {
                            "oper": "AnyOf",
                            "vals": {
                                "5167": "Three"
                            },
                            "fgcolor": {
                                "a": 255,
                                "r": 0,
                                "g": 128,
                                "b": 0
                            },
                            "bgcolor": {
                                "a": 255,
                                "r": 153,
                                "g": 255,
                                "b": 153
                            }
                        },
                        {
                            "oper": "Unspecified",
                            "fgcolor": {
                                "a": 255,
                                "r": 0,
                                "g": 0,
                                "b": 128
                            },
                            "bgcolor": {
                                "a": 255,
                                "r": 153,
                                "g": 204,
                                "b": 255
                            }
                        }
                    ]
                }
            }
        },
        "gdata": [
            {
                "eid": 4088,
                "values": [
                    {
                        "val": "Nine",
                        "cfidx": 2
                    },
                    {
                        "val": "Nine"
                    },
                    {
                        "val": "9",
                        "cfidx": 2
                    },
                    {
                        "val": "9"
                    },
                    {
                        "val": "999.999"
                    },
                    {
                        "val": "99.990"
                    },
                    {
                        "val": "2013-10-08T22:00:00Z"
                    },
                    {
                        "val": "2013-10-09"
                    },
                    {
                        "val": "1753-01-01T09:00:00Z"
                    },
                    {
                        "val": "False"
                    },
                    {
                        "vals": {
                            "4195": "Nine"
                        }
                    }
                ]
            },
            {
                "eid": 4697,
                "values": [
                    {
                        "val": "Four",
                        "cfidx": 1
                    },
                    {
                        "val": "Four"
                    },
                    {
                        "val": "4",
                        "cfidx": 3
                    },
                    {
                        "val": "4"
                    },
                    {
                        "val": "444.444"
                    },
                    {
                        "val": "44.440"
                    },
                    {
                        "val": "2013-10-03T18:00:00Z"
                    },
                    {
                        "val": "2013-10-04"
                    },
                    {
                        "val": "1753-01-01T04:00:00Z"
                    },
                    {
                        "val": "False"
                    },
                    {
                        "vals": {
                            "5034": "Four"
                        }
                    }
                ]
            },
            {
                "eid": 4753,
                "values": [
                    {
                        "val": "Eleven",
                        "cfidx": 2
                    },
                    {
                        "val": "Eleven"
                    },
                    {
                        "val": "11",
                        "cfidx": 2
                    },
                    {
                        "val": "11"
                    },
                    {
                        "val": "11.011"
                    },
                    {
                        "val": "11.110"
                    },
                    {
                        "val": "2013-10-11T00:00:00Z"
                    },
                    {
                        "val": "2013-10-11"
                    },
                    {
                        "val": "1753-01-01T11:00:00Z"
                    },
                    {
                        "val": "False"
                    },
                    {
                        "vals": {
                            "5847": "Bravo"
                        }
                    }
                ]
            },
            {
                "eid": 4913,
                "values": [
                    {
                        "val": "Fifteen",
                        "cfidx": 1
                    },
                    {
                        "val": "Fifteen"
                    },
                    {
                        "val": "15",
                        "cfidx": 2
                    },
                    {
                        "val": "15"
                    },
                    {
                        "val": "15.015"
                    },
                    {
                        "val": "15.150"
                    },
                    {
                        "val": "2013-10-15T04:00:00Z"
                    },
                    {
                        "val": "2013-10-15"
                    },
                    {
                        "val": "1753-01-01T15:00:00Z"
                    },
                    {
                        "val": "False"
                    },
                    {
                        "vals": {
                            "4522": "Foxtrot"
                        }
                    }
                ]
            },
            {
                "eid": 5061,
                "values": [
                    {
                        "val": "Seven",
                        "cfidx": 2
                    },
                    {
                        "val": "Seven"
                    },
                    {
                        "val": "7",
                        "cfidx": 2
                    },
                    {
                        "val": "7"
                    },
                    {
                        "val": "777.777"
                    },
                    {
                        "val": "77.770"
                    },
                    {
                        "val": "2013-10-06T20:00:00Z"
                    },
                    {
                        "val": "2013-10-07"
                    },
                    {
                        "val": "1753-01-01T07:00:00Z"
                    },
                    {
                        "val": "False"
                    },
                    {
                        "vals": {
                            "6022": "Seven"
                        }
                    }
                ]
            },
            {
                "eid": 5131,
                "values": [
                    {
                        "val": "Ten",
                        "cfidx": 0
                    },
                    {
                        "val": "Ten"
                    },
                    {
                        "val": "10",
                        "cfidx": 2
                    },
                    {
                        "val": "10"
                    },
                    {
                        "val": "10.010"
                    },
                    {
                        "val": "10.100"
                    },
                    {
                        "val": "2013-10-09T23:00:00Z"
                    },
                    {
                        "val": "2013-10-10"
                    },
                    {
                        "val": "1753-01-01T10:00:00Z"
                    },
                    {
                        "val": "False"
                    },
                    {
                        "vals": {
                            "5671": "Alpha"
                        }
                    }
                ]
            },
            {
                "eid": 5558,
                "values": [
                    {
                        "val": "Three",
                        "cfidx": 0
                    },
                    {
                        "val": "Three"
                    },
                    {
                        "val": "3",
                        "cfidx": 0
                    },
                    {
                        "val": "3"
                    },
                    {
                        "val": "333.333"
                    },
                    {
                        "val": "33.330"
                    },
                    {
                        "val": "2013-10-02T17:00:00Z"
                    },
                    {
                        "val": "2013-10-03"
                    },
                    {
                        "val": "1753-01-01T03:00:00Z"
                    },
                    {
                        "val": "False"
                    },
                    {
                        "vals": {
                            "5167": "Three"
                        }
                    }
                ]
            },
            {
                "eid": 5596,
                "values": [
                    {
                        "val": "One",
                        "cfidx": 2
                    },
                    {
                        "val": "One"
                    },
                    {
                        "val": "1",
                        "cfidx": 0
                    },
                    {
                        "val": "1"
                    },
                    {
                        "val": "111.111"
                    },
                    {
                        "val": "11.110"
                    },
                    {
                        "val": "2013-09-30T15:00:00Z"
                    },
                    {
                        "val": "2013-10-01"
                    },
                    {
                        "val": "1753-01-01T01:00:00Z"
                    },
                    {
                        "val": "False"
                    },
                    {
                        "vals": {
                            "7155": "One"
                        }
                    }
                ]
            },
            {
                "eid": 6188,
                "values": [
                    {
                        "val": "Thirteen",
                        "cfidx": 0
                    },
                    {
                        "val": "Thirteen"
                    },
                    {
                        "val": "13",
                        "cfidx": 2
                    },
                    {
                        "val": "13"
                    },
                    {
                        "val": "13.013"
                    },
                    {
                        "val": "13.130"
                    },
                    {
                        "val": "2013-10-13T02:00:00Z"
                    },
                    {
                        "val": "2013-10-13"
                    },
                    {
                        "val": "1753-01-01T13:00:00Z"
                    },
                    {
                        "val": "False"
                    },
                    {
                        "vals": {
                            "6709": "Delta"
                        }
                    }
                ]
            },
            {
                "eid": 6358,
                "values": [
                    {
                        "val": "Twelve",
                        "cfidx": 0
                    },
                    {
                        "val": "Twelve"
                    },
                    {
                        "val": "12",
                        "cfidx": 2
                    },
                    {
                        "val": "12"
                    },
                    {
                        "val": "12.012"
                    },
                    {
                        "val": "12.120"
                    },
                    {
                        "val": "2013-10-12T01:00:00Z"
                    },
                    {
                        "val": "2013-10-12"
                    },
                    {
                        "val": "1753-01-01T12:00:00Z"
                    },
                    {
                        "val": "False"
                    },
                    {
                        "vals": {
                            "4181": "Charlie"
                        }
                    }
                ]
            },
            {
                "eid": 6469,
                "values": [
                    {
                        "val": "Six",
                        "cfidx": 2
                    },
                    {
                        "val": "Six"
                    },
                    {
                        "val": "6",
                        "cfidx": 1
                    },
                    {
                        "val": "6"
                    },
                    {
                        "val": "666.666"
                    },
                    {
                        "val": "66.660"
                    },
                    {
                        "val": "2013-10-05T19:00:00Z"
                    },
                    {
                        "val": "2013-10-06"
                    },
                    {
                        "val": "1753-01-01T06:00:00Z"
                    },
                    {
                        "val": "False"
                    },
                    {
                        "vals": {
                            "5450": "Six"
                        }
                    }
                ]
            },
            {
                "eid": 6765,
                "values": [
                    {
                        "val": "Eight",
                        "cfidx": 2
                    },
                    {
                        "val": "Eight"
                    },
                    {
                        "val": "8",
                        "cfidx": 2
                    },
                    {
                        "val": "8"
                    },
                    {
                        "val": "888.888"
                    },
                    {
                        "val": "88.880"
                    },
                    {
                        "val": "2013-10-07T21:00:00Z"
                    },
                    {
                        "val": "2013-10-08"
                    },
                    {
                        "val": "1753-01-01T08:00:00Z"
                    },
                    {
                        "val": "False"
                    },
                    {
                        "vals": {
                            "5835": "Eight"
                        }
                    }
                ]
            },
            {
                "eid": 6818,
                "values": [
                    {
                        "val": "Two",
                        "cfidx": 0
                    },
                    {
                        "val": "Two"
                    },
                    {
                        "val": "2",
                        "cfidx": 0
                    },
                    {
                        "val": "2"
                    },
                    {
                        "val": "222.222"
                    },
                    {
                        "val": "22.220"
                    },
                    {
                        "val": "2013-10-01T16:00:00Z"
                    },
                    {
                        "val": "2013-10-02"
                    },
                    {
                        "val": "1753-01-01T02:00:00Z"
                    },
                    {
                        "val": "False"
                    },
                    {
                        "vals": {
                            "5267": "Two"
                        }
                    }
                ]
            },
            {
                "eid": 7264,
                "values": [
                    {
                        "val": "Five",
                        "cfidx": 1
                    },
                    {
                        "val": "Five"
                    },
                    {
                        "val": "5",
                        "cfidx": 3
                    },
                    {
                        "val": "5"
                    },
                    {
                        "val": "555.555"
                    },
                    {
                        "val": "55.550"
                    },
                    {
                        "val": "2013-10-04T19:00:00Z"
                    },
                    {
                        "val": "2013-10-05"
                    },
                    {
                        "val": "1753-01-01T05:00:00Z"
                    },
                    {
                        "val": "False"
                    },
                    {
                        "vals": {
                            "6132": "Five"
                        }
                    }
                ]
            },
            {
                "eid": 7291,
                "values": [
                    {
                        "val": "Fourteen",
                        "cfidx": 1
                    },
                    {
                        "val": "Fourteen"
                    },
                    {
                        "val": "14",
                        "cfidx": 2
                    },
                    {
                        "val": "14"
                    },
                    {
                        "val": "14.014"
                    },
                    {
                        "val": "14.140"
                    },
                    {
                        "val": "2013-10-14T03:00:00Z"
                    },
                    {
                        "val": "2013-10-14"
                    },
                    {
                        "val": "1753-01-01T14:00:00Z"
                    },
                    {
                        "val": "False"
                    },
                    {
                        "vals": {
                            "5264": "Echo"
                        }
                    }
                ]
            }
        ]
    };
})(reportTestData || (reportTestData = {}));