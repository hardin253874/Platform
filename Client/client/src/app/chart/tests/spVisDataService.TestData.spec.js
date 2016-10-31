// Copyright 2011-2016 Global Software Innovation Pty Ltd
var visDataTestData;
(function (visDataTestData) {
    visDataTestData.allFields =
    {
        "meta": {
            "modified": "2016-03-06T23:40:19.570Z",
            "hideheader": false,
            "hideact": false,
            "dfid": 15666,
            "typefmtstyle": {
                "String": [
                   "Highlight",
                   "Icon"
                ],
                "Bool": [
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
                "ChoiceRelationship": [
                   "Highlight",
                   "Icon"
                ],
                "InlineRelationship": [
                   "Highlight",
                   "Icon"
                ],
                "Date": [
                   "Highlight",
                   "Icon",
                   "ProgressBar"
                ],
                "DateTime": [
                   "Highlight",
                   "Icon",
                   "ProgressBar"
                ],
                "Time": [
                   "Highlight",
                   "Icon",
                   "ProgressBar"
                ]
            },
            "rcols": {
                "12779": {
                    "entityname": true,
                    "maxlen": 200,
                    "tid": 18470,
                    "fid": 2805,
                    "ord": 0,
                    "regexerr": "The value must not contain angled brackets.",
                    "regex": "^[^<>]+$",
                    "oprtype": "InlineRelationship",
                    "type": "String",
                    "title": "Name"
                },
                "18665": {
                    "fid": 17831,
                    "ord": 1,
                    "defval": "",
                    "oprtype": "String",
                    "type": "String",
                    "title": "Single Line"
                },
                "20479": {
                    "mline": true,
                    "fid": 15872,
                    "ord": 2,
                    "defval": "",
                    "oprtype": "String",
                    "type": "String",
                    "title": "Multiline"
                },
                "12086": {
                    "fid": 17787,
                    "ord": 3,
                    "defval": "",
                    "oprtype": "Bool",
                    "type": "Bool",
                    "title": "Boolean"
                },
                "16972": {
                    "fid": 11895,
                    "ord": 4,
                    "defval": "",
                    "oprtype": "Int32",
                    "type": "Int32",
                    "title": "Number"
                },
                "17680": {
                    "mindec": 2,
                    "fid": 15087,
                    "ord": 5,
                    "places": 3,
                    "defval": "",
                    "oprtype": "Decimal",
                    "type": "Decimal",
                    "title": "Decimal"
                },
                "20955": {
                    "fid": 19656,
                    "ord": 6,
                    "places": 2,
                    "defval": "",
                    "oprtype": "Currency",
                    "type": "Currency",
                    "title": "Currency"
                },
                "16931": {
                    "fid": 15110,
                    "ord": 7,
                    "anpat": "",
                    "defval": "",
                    "oprtype": "Int32",
                    "type": "Int32",
                    "title": "Autonumber"
                },
                "22142": {
                    "ord": 8,
                    "oprtype": "Decimal",
                    "type": "Decimal",
                    "title": "Calculation"
                },
                "17480": {
                    "maxlen": 200,
                    "tid": 14005,
                    "fid": 2805,
                    "ord": 9,
                    "regexerr": "The value must not contain angled brackets.",
                    "regex": "^[^<>]+$",
                    "oprtype": "ChoiceRelationship",
                    "type": "ChoiceRelationship",
                    "title": "Weekday"
                },
                "15001": {
                    "aggcol": true,
                    "tid": 17237,
                    "ord": 10,
                    "oprtype": "ChoiceRelationship",
                    "type": "ChoiceRelationship",
                    "title": "List : Condiments"
                },
                "20960": {
                    "maxlen": 200,
                    "tid": 11868,
                    "fid": 2805,
                    "ord": 11,
                    "card": "ManyToOne",
                    "regexerr": "The value must not contain angled brackets.",
                    "regex": "^[^<>]+$",
                    "oprtype": "InlineRelationship",
                    "type": "InlineRelationship",
                    "title": "AA_Employee"
                },
                "16820": {
                    "maxlen": 200,
                    "tid": 18183,
                    "fid": 2805,
                    "ord": 12,
                    "card": "OneToOne",
                    "regexerr": "The value must not contain angled brackets.",
                    "regex": "^[^<>]+$",
                    "oprtype": "InlineRelationship",
                    "type": "InlineRelationship",
                    "title": "AA_Drinks"
                },
                "16336": {
                    "maxlen": 200,
                    "fid": 2805,
                    "ord": 13,
                    "regexerr": "The value must not contain angled brackets.",
                    "regex": "^[^<>]+$",
                    "oprtype": "Image",
                    "type": "Image",
                    "title": "New Image Field"
                },
                "14477": {
                    "maxlen": 200,
                    "tid": 21455,
                    "fid": 2805,
                    "ord": 14,
                    "card": "ManyToOne",
                    "regexerr": "The value must not contain angled brackets.",
                    "regex": "^[^<>]+$",
                    "oprtype": "InlineRelationship",
                    "type": "InlineRelationship",
                    "title": "AA_ChocBars (Rev)"
                },
                "15286": {
                    "maxlen": 200,
                    "tid": 11795,
                    "fid": 2805,
                    "ord": 15,
                    "card": "OneToOne",
                    "regexerr": "The value must not contain angled brackets.",
                    "regex": "^[^<>]+$",
                    "oprtype": "InlineRelationship",
                    "type": "InlineRelationship",
                    "title": "Scientist (Rev)"
                },
                "17441": {
                    "maxlen": 200,
                    "tid": 15329,
                    "fid": 2805,
                    "ord": 16,
                    "card": "ManyToMany",
                    "regexerr": "The value must not contain angled brackets.",
                    "regex": "^[^<>]+$",
                    "oprtype": "InlineRelationship",
                    "type": "InlineRelationship",
                    "title": "AA_Herb"
                },
                "11788": {
                    "maxlen": 200,
                    "tid": 21805,
                    "fid": 2805,
                    "ord": 17,
                    "card": "ManyToMany",
                    "regexerr": "The value must not contain angled brackets.",
                    "regex": "^[^<>]+$",
                    "oprtype": "InlineRelationship",
                    "type": "InlineRelationship",
                    "title": "AA_Truck"
                },
                "21838": {
                    "maxlen": 200,
                    "tid": 18817,
                    "fid": 2805,
                    "ord": 18,
                    "card": "OneToMany",
                    "regexerr": "The value must not contain angled brackets.",
                    "regex": "^[^<>]+$",
                    "oprtype": "InlineRelationship",
                    "type": "InlineRelationship",
                    "title": "AA_DogBreeds (Rev)"
                },
                "13329": {
                    "maxlen": 200,
                    "tid": 18052,
                    "fid": 2805,
                    "ord": 19,
                    "card": "ManyToMany",
                    "regexerr": "The value must not contain angled brackets.",
                    "regex": "^[^<>]+$",
                    "oprtype": "InlineRelationship",
                    "type": "InlineRelationship",
                    "title": "AA_Snacks (Rev)"
                },
                "12743": {
                    "fid": 20963,
                    "ord": 20,
                    "defval": "",
                    "oprtype": "Date",
                    "type": "Date",
                    "title": "Date"
                },
                "17731": {
                    "fid": 21232,
                    "ord": 21,
                    "defval": "",
                    "oprtype": "DateTime",
                    "type": "DateTime",
                    "title": "DateTime"
                },
                "21003": {
                    "fid": 13781,
                    "ord": 22,
                    "defval": "",
                    "oprtype": "Time",
                    "type": "Time",
                    "title": "Time"
                }
            },
            "sort": [
               {
                   "order": "Ascending",
                   "colid": "12779"
               }
            ],
            "choice": {
                "14005": [
                   {
                       "id": 13973,
                       "name": "Monday"
                   },
                   {
                       "id": 17812,
                       "name": "Tuesday"
                   },
                   {
                       "id": 21571,
                       "name": "Wednesday"
                   },
                   {
                       "id": 21543,
                       "name": "Thursday"
                   },
                   {
                       "id": 11903,
                       "name": "Friday"
                   },
                   {
                       "id": 14198,
                       "name": "Saturday"
                   },
                   {
                       "id": 20325,
                       "name": "Sunday"
                   }
                ],
                "17237": [
                   {
                       "id": 13608,
                       "name": "Chili"
                   },
                   {
                       "id": 20083,
                       "name": "Chutney"
                   },
                   {
                       "id": 16740,
                       "name": "Ketchup"
                   },
                   {
                       "id": 12413,
                       "name": "Mustard"
                   },
                   {
                       "id": 20416,
                       "name": "Pepper"
                   },
                   {
                       "id": 19396,
                       "name": "Pickle"
                   },
                   {
                       "id": 16123,
                       "name": "Relish"
                   },
                   {
                       "id": 12625,
                       "name": "Salt"
                   },
                   {
                       "id": 15951,
                       "name": "Vinegar"
                   },
                   {
                       "id": 20251,
                       "name": "Wasabi"
                   }
                ]
            },
            "inline": {
                "18470": 4980,
                "11868": 4980,
                "18183": 4980,
                "21455": 4980,
                "11795": 4980,
                "15329": 4980,
                "21805": 4980,
                "18817": 4980,
                "18052": 4980,
                "2892": 4980
            },
            "anlcols": {
                "13146": {
                    "ord": 6,
                    "doper": "GreaterThan",
                    "oper": "Unspecified",
                    "anltype": "Currency",
                    "type": "Currency",
                    "title": "Currency"
                },
                "13365": {
                    "tid": 14005,
                    "ord": 9,
                    "doper": "AnyOf",
                    "oper": "AnyOf",
                    "anltype": "ChoiceRelationship",
                    "type": "ChoiceRelationship",
                    "title": "Weekday"
                },
                "14065": {
                    "ord": 20,
                    "doper": "GreaterThan",
                    "oper": "Unspecified",
                    "anltype": "Date",
                    "type": "Date",
                    "title": "Date"
                },
                "14074": {
                    "ord": 5,
                    "doper": "GreaterThan",
                    "oper": "Unspecified",
                    "anltype": "Decimal",
                    "type": "Decimal",
                    "title": "Decimal"
                },
                "14230": {
                    "tid": 2892,
                    "ord": 13,
                    "doper": "AnyOf",
                    "oper": "AnyOf",
                    "anltype": "InlineRelationship",
                    "type": "InlineRelationship",
                    "title": "New Image Field"
                },
                "14245": {
                    "tid": 18183,
                    "ord": 12,
                    "doper": "AnyOf",
                    "oper": "AnyOf",
                    "anltype": "InlineRelationship",
                    "type": "InlineRelationship",
                    "title": "AA_Drinks"
                },
                "14348": {
                    "ord": 4,
                    "doper": "GreaterThan",
                    "oper": "Unspecified",
                    "anltype": "Int32",
                    "type": "Int32",
                    "title": "Number"
                },
                "14385": {
                    "tid": 18470,
                    "ord": 0,
                    "doper": "Contains",
                    "oper": "Unspecified",
                    "anltype": "InlineRelationship",
                    "type": "String",
                    "title": "Name"
                },
                "14388": {
                    "tid": 18817,
                    "ord": 18,
                    "doper": "AnyOf",
                    "oper": "Unspecified",
                    "anltype": "InlineRelationship",
                    "type": "InlineRelationship",
                    "title": "AA_DogBreeds (Rev)"
                },
                "14492": {
                    "ord": 1,
                    "doper": "Contains",
                    "oper": "Unspecified",
                    "anltype": "String",
                    "type": "String",
                    "title": "Single Line"
                },
                "14507": {
                    "ord": 22,
                    "doper": "GreaterThan",
                    "oper": "Unspecified",
                    "anltype": "Time",
                    "type": "Time",
                    "title": "Time"
                },
                "14559": {
                    "tid": 15329,
                    "ord": 16,
                    "doper": "AnyOf",
                    "oper": "Unspecified",
                    "anltype": "InlineRelationship",
                    "type": "InlineRelationship",
                    "title": "AA_Herb"
                },
                "14698": {
                    "ord": 3,
                    "doper": "Unspecified",
                    "oper": "Unspecified",
                    "anltype": "Bool",
                    "type": "Bool",
                    "title": "Boolean"
                },
                "15235": {
                    "ord": 7,
                    "doper": "GreaterThan",
                    "oper": "Unspecified",
                    "anltype": "Int32",
                    "type": "Int32",
                    "title": "Autonumber"
                },
                "15360": {
                    "tid": 21805,
                    "ord": 17,
                    "doper": "AnyOf",
                    "oper": "Unspecified",
                    "anltype": "InlineRelationship",
                    "type": "InlineRelationship",
                    "title": "AA_Truck"
                },
                "16362": {
                    "ord": 21,
                    "doper": "GreaterThan",
                    "oper": "Unspecified",
                    "anltype": "DateTime",
                    "type": "DateTime",
                    "title": "DateTime"
                },
                "16554": {
                    "ord": 2,
                    "doper": "Contains",
                    "oper": "Unspecified",
                    "anltype": "String",
                    "type": "String",
                    "title": "Multiline"
                },
                "16556": {
                    "tid": 18052,
                    "ord": 19,
                    "doper": "AnyOf",
                    "oper": "Unspecified",
                    "anltype": "InlineRelationship",
                    "type": "InlineRelationship",
                    "title": "AA_Snacks (Rev)"
                },
                "18248": {
                    "tid": 17237,
                    "ord": 10,
                    "doper": "AnyOf",
                    "oper": "AnyOf",
                    "anltype": "ChoiceRelationship",
                    "type": "ChoiceRelationship",
                    "title": "List : Condiments"
                },
                "18432": {
                    "ord": 8,
                    "doper": "GreaterThan",
                    "oper": "Unspecified",
                    "anltype": "Decimal",
                    "type": "Decimal",
                    "title": "Calculation"
                },
                "20253": {
                    "tid": 21455,
                    "ord": 14,
                    "doper": "AnyOf",
                    "oper": "AnyOf",
                    "anltype": "InlineRelationship",
                    "type": "InlineRelationship",
                    "title": "AA_ChocBars (Rev)"
                },
                "21034": {
                    "tid": 11868,
                    "ord": 11,
                    "doper": "AnyOf",
                    "oper": "AnyOf",
                    "anltype": "InlineRelationship",
                    "type": "InlineRelationship",
                    "title": "AA_Employee"
                },
                "21072": {
                    "tid": 11795,
                    "ord": 15,
                    "doper": "AnyOf",
                    "oper": "AnyOf",
                    "anltype": "InlineRelationship",
                    "type": "InlineRelationship",
                    "title": "Scientist (Rev)"
                }
            },
            "valrules": {
                "17680": {
                    "places": 3
                },
                "20955": {
                    "places": 2
                },
                "22142": {

                },
                "16336": {
                    "imgw": 16,
                    "imgh": 16,
                    "sizeid": 4964,
                    "scaleid": 3525
                }
            },
            "invalid": {
                "nodes": null,
                "columns": null,
                "conditions": null
            },
            "style": "Default",
            "title": "AF_All Fields"
        },
        "gdata": [
           {
               "eid": 17021,
               "values": [
                  {
                      "val": "Test 01"
                  },
                  {
                      "val": "data 01"
                  },
                  {
                      "val": "multi \rtext \rfor \rTest 01"
                  },
                  {
                      "val": "False"
                  },
                  {
                      "val": "100"
                  },
                  {
                      "val": "100.111"
                  },
                  {
                      "val": "100.100"
                  },
                  {
                      "val": "50"
                  },
                  {
                      "val": "200.111"
                  },
                  {
                      "vals": {
                          "20325": "Sunday"
                      }
                  },
                  {
                      "vals": {
                          "13608": "Chili",
                          "20083": "Chutney"
                      }
                  },
                  {
                      "vals": {
                          "17166": "Tina Adlakha"
                      }
                  },
                  {
                      "vals": {
                          "17985": "Coke"
                      }
                  },
                  {
                      "vals": {
                          "21633": "TinaAdlakha"
                      }
                  },
                  {
                      "vals": {
                          "18508": "3 Musketeers"
                      }
                  },
                  {
                      "vals": {
                          "11779": "Ada Lovelace"
                      }
                  },
                  {
                      "vals": {
                          "11856": "Basil"
                      }
                  },
                  {
                      "vals": {
                          "14140": "Aces High"
                      }
                  },
                  {
                      "vals": {
                          "16002": "Afghan Hound"
                      }
                  },
                  {
                      "vals": {
                          "19828": "Almond Nut Choc Bar"
                      }
                  },
                  {
                      "val": "2013-06-01"
                  },
                  {
                      "val": "2013-06-01T03:00:00Z"
                  },
                  {
                      "val": "1753-01-01T13:00:00Z"
                  }
               ]
           }
        ]
    };

    // Report with conditional formatting, from "ANL Simple Choice Cond"
    visDataTestData.conditionalFormatting =
    {
        "meta": {
            "hideheader": false,
            "hideact": false,
            "dfid": 16029,
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
                "19861": {
                    "fid": 16396,
                    "ord": 0,
                    "oprtype": "String",
                    "type": "String",
                    "title": "Text Field"
                },
                "21375": {
                    "mline": true,
                    "fid": 20739,
                    "ord": 1,
                    "oprtype": "String",
                    "type": "String",
                    "title": "Multiline Text Field"
                },
                "19403": {
                    "fid": 14782,
                    "ord": 2,
                    "oprtype": "Int32",
                    "type": "Int32",
                    "title": "Number Field"
                },
                "21341": {
                    "fid": 20956,
                    "ord": 3,
                    "oprtype": "Int32",
                    "type": "Int32",
                    "title": "AutoNumber Field"
                },
                "12690": {
                    "mindec": 2,
                    "fid": 13328,
                    "ord": 4,
                    "places": 3,
                    "oprtype": "Decimal",
                    "type": "Decimal",
                    "title": "Decimal Field"
                },
                "12937": {
                    "fid": 21513,
                    "ord": 5,
                    "places": 2,
                    "oprtype": "Currency",
                    "type": "Currency",
                    "title": "Currency Field"
                },
                "16960": {
                    "fid": 16732,
                    "ord": 6,
                    "oprtype": "DateTime",
                    "type": "DateTime",
                    "title": "Date and Time Field"
                },
                "20849": {
                    "fid": 16118,
                    "ord": 7,
                    "oprtype": "Date",
                    "type": "Date",
                    "title": "Date only Field"
                },
                "16885": {
                    "fid": 17204,
                    "ord": 8,
                    "oprtype": "Time",
                    "type": "Time",
                    "title": "Time only Field"
                },
                "20438": {
                    "fid": 22082,
                    "ord": 9,
                    "oprtype": "Bool",
                    "type": "Bool",
                    "title": "Yes/No Field"
                },
                "13417": {
                    "maxlen": 200,
                    "tid": 16486,
                    "fid": 2816,
                    "ord": 10,
                    "regexerr": "The value must not contain angled brackets.",
                    "regex": "^[^<>]+$",
                    "oprtype": "ChoiceRelationship",
                    "type": "ChoiceRelationship",
                    "title": "Choice Field"
                }
            },
            "choice": {
                "16486": [
                    {
                        "id": 21716,
                        "name": "One"
                    },
                    {
                        "id": 15674,
                        "name": "Two"
                    },
                    {
                        "id": 15361,
                        "name": "Three"
                    },
                    {
                        "id": 14926,
                        "name": "Four"
                    },
                    {
                        "id": 18429,
                        "name": "Five"
                    },
                    {
                        "id": 16252,
                        "name": "Six"
                    },
                    {
                        "id": 18067,
                        "name": "Seven"
                    },
                    {
                        "id": 17473,
                        "name": "Eight"
                    },
                    {
                        "id": 12338,
                        "name": "Nine"
                    },
                    {
                        "id": 16977,
                        "name": "Alpha"
                    },
                    {
                        "id": 17508,
                        "name": "Bravo"
                    },
                    {
                        "id": 12295,
                        "name": "Charlie"
                    },
                    {
                        "id": 20291,
                        "name": "Delta"
                    },
                    {
                        "id": 15664,
                        "name": "Echo"
                    },
                    {
                        "id": 13358,
                        "name": "Foxtrot"
                    }
                ]
            },
            "anlcols": {
                "12070": {
                    "ord": 2,
                    "doper": "GreaterThan",
                    "oper": "Unspecified",
                    "anltype": "Time",
                    "type": "Time",
                    "title": "Time only Field"
                },
                "13102": {
                    "tid": 16486,
                    "ord": 0,
                    "doper": "AnyOf",
                    "oper": "AnyOf",
                    "anltype": "ChoiceRelationship",
                    "type": "ChoiceRelationship",
                    "title": "Choice Field"
                },
                "13503": {
                    "ord": 3,
                    "doper": "GreaterThan",
                    "oper": "Unspecified",
                    "anltype": "Date",
                    "type": "Date",
                    "title": "Date only Field"
                },
                "14753": {
                    "ord": 7,
                    "doper": "GreaterThan",
                    "oper": "Unspecified",
                    "anltype": "Int32",
                    "type": "Int32",
                    "title": "AutoNumber Field"
                },
                "14904": {
                    "ord": 5,
                    "doper": "GreaterThan",
                    "oper": "Unspecified",
                    "anltype": "Currency",
                    "type": "Currency",
                    "title": "Currency Field"
                },
                "15394": {
                    "ord": 1,
                    "doper": "Unspecified",
                    "oper": "Unspecified",
                    "anltype": "Bool",
                    "type": "Bool",
                    "title": "Yes/No Field"
                },
                "16622": {
                    "ord": 9,
                    "doper": "GreaterThan",
                    "oper": "Unspecified",
                    "anltype": "Int32",
                    "type": "Int32",
                    "title": "Number Field"
                },
                "16965": {
                    "ord": 6,
                    "doper": "GreaterThan",
                    "oper": "Unspecified",
                    "anltype": "Decimal",
                    "type": "Decimal",
                    "title": "Decimal Field"
                },
                "19296": {
                    "ord": 8,
                    "doper": "Contains",
                    "oper": "Unspecified",
                    "anltype": "String",
                    "type": "String",
                    "title": "Multiline Text Field"
                },
                "19773": {
                    "ord": 10,
                    "doper": "Contains",
                    "oper": "Unspecified",
                    "anltype": "String",
                    "type": "String",
                    "title": "Text Field"
                },
                "19982": {
                    "ord": 4,
                    "doper": "GreaterThan",
                    "oper": "Unspecified",
                    "anltype": "DateTime",
                    "type": "DateTime",
                    "title": "Date and Time Field"
                }
            },
            "cfrules": {
                "12690": {
                    "showval": true,
                    "rules": [
                        {
                            "bgcolor": {
                                "b": 206,
                                "g": 239,
                                "r": 198,
                                "a": 255
                            },
                            "bounds": {
                                "lower": 0,
                                "upper": 1000
                            },
                            "oper": "Unspecified"
                        }
                    ],
                    "style": "ProgressBar"
                },
                "12937": {
                    "showval": true,
                    "rules": [
                        {
                            "bgcolor": {
                                "b": 0,
                                "g": 128,
                                "r": 0,
                                "a": 255
                            },
                            "bounds": {
                                "lower": 0,
                                "upper": 100
                            },
                            "oper": "Unspecified"
                        }
                    ],
                    "style": "ProgressBar"
                },
                "19403": {
                    "showval": true,
                    "rules": [
                        {
                            "fgcolor": {
                                "b": 0,
                                "g": 0,
                                "r": 255,
                                "a": 255
                            },
                            "bgcolor": {
                                "b": 153,
                                "g": 153,
                                "r": 255,
                                "a": 255
                            },
                            "val": "4",
                            "oper": "LessThan"
                        },
                        {
                            "fgcolor": {
                                "b": 0,
                                "g": 0,
                                "r": 128,
                                "a": 255
                            },
                            "bgcolor": {
                                "b": 153,
                                "g": 255,
                                "r": 255,
                                "a": 255
                            },
                            "val": "6",
                            "oper": "Equal"
                        },
                        {
                            "fgcolor": {
                                "b": 0,
                                "g": 128,
                                "r": 0,
                                "a": 255
                            },
                            "bgcolor": {
                                "b": 153,
                                "g": 255,
                                "r": 153,
                                "a": 255
                            },
                            "val": "7",
                            "oper": "GreaterThanOrEqual"
                        },
                        {
                            "fgcolor": {
                                "b": 128,
                                "g": 0,
                                "r": 0,
                                "a": 255
                            },
                            "bgcolor": {
                                "b": 255,
                                "g": 204,
                                "r": 153,
                                "a": 255
                            },
                            "oper": "Unspecified"
                        }
                    ],
                    "style": "Highlight"
                },
                "19861": {
                    "showval": true,
                    "rules": [
                        {
                            "imgid": 3246,
                            "val": "T",
                            "oper": "StartsWith"
                        },
                        {
                            "imgid": 3155,
                            "val": "F",
                            "oper": "StartsWith"
                        },
                        {
                            "imgid": 3795,
                            "oper": "Unspecified"
                        }
                    ],
                    "style": "Icon"
                }
            },
            "valrules": {
                "19861": {
                    "imgw": 16,
                    "imgh": 16,
                    "sizeid": 4982,
                    "scaleid": 3541,
                    "places": 2,
                    "align": "Automatic"
                },
                "19403": {
                    "places": 2,
                    "align": "Automatic"
                },
                "12690": {
                    "places": 3,
                    "align": "Automatic"
                },
                "12937": {
                    "places": 2,
                    "align": "Automatic"
                },
                "13417": {
                    "places": 2,
                    "align": "Automatic"
                }
            },
            "invalid": {
                "nodes": null,
                "columns": null,
                "conditions": null
            },
            "style": "Default",
            "title": "ANL Simple Choice Cond"
        },
        "gdata": [
            {
                "eid": 11958,
                "values": [
                    {
                        "cfidx": 2,
                        "val": "Nine"
                    },
                    {
                        "val": "Nine"
                    },
                    {
                        "cfidx": 2,
                        "val": "9"
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
                            "12338": "Nine"
                        }
                    }
                ]
            },
            {
                "eid": 13891,
                "values": [
                    {
                        "cfidx": 1,
                        "val": "Four"
                    },
                    {
                        "val": "Four"
                    },
                    {
                        "cfidx": 3,
                        "val": "4"
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
                            "14926": "Four"
                        }
                    }
                ]
            },
            {
                "eid": 14056,
                "values": [
                    {
                        "cfidx": 2,
                        "val": "Eleven"
                    },
                    {
                        "val": "Eleven"
                    },
                    {
                        "cfidx": 2,
                        "val": "11"
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
                            "17508": "Bravo"
                        }
                    }
                ]
            },
            {
                "eid": 14565,
                "values": [
                    {
                        "cfidx": 1,
                        "val": "Fifteen"
                    },
                    {
                        "val": "Fifteen"
                    },
                    {
                        "cfidx": 2,
                        "val": "15"
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
                            "13358": "Foxtrot"
                        }
                    }
                ]
            },
            {
                "eid": 15027,
                "values": [
                    {
                        "cfidx": 2,
                        "val": "Seven"
                    },
                    {
                        "val": "Seven"
                    },
                    {
                        "cfidx": 2,
                        "val": "7"
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
                            "18067": "Seven"
                        }
                    }
                ]
            },
            {
                "eid": 15255,
                "values": [
                    {
                        "cfidx": 0,
                        "val": "Ten"
                    },
                    {
                        "val": "Ten"
                    },
                    {
                        "cfidx": 2,
                        "val": "10"
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
                            "16977": "Alpha"
                        }
                    }
                ]
            },
            {
                "eid": 16609,
                "values": [
                    {
                        "cfidx": 0,
                        "val": "Three"
                    },
                    {
                        "val": "Three"
                    },
                    {
                        "cfidx": 0,
                        "val": "3"
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
                            "15361": "Three"
                        }
                    }
                ]
            },
            {
                "eid": 16734,
                "values": [
                    {
                        "cfidx": 2,
                        "val": "One"
                    },
                    {
                        "val": "One"
                    },
                    {
                        "cfidx": 0,
                        "val": "1"
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
                            "21716": "One"
                        }
                    }
                ]
            },
            {
                "eid": 18621,
                "values": [
                    {
                        "cfidx": 0,
                        "val": "Thirteen"
                    },
                    {
                        "val": "Thirteen"
                    },
                    {
                        "cfidx": 2,
                        "val": "13"
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
                            "20291": "Delta"
                        }
                    }
                ]
            },
            {
                "eid": 19197,
                "values": [
                    {
                        "cfidx": 0,
                        "val": "Twelve"
                    },
                    {
                        "val": "Twelve"
                    },
                    {
                        "cfidx": 2,
                        "val": "12"
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
                            "12295": "Charlie"
                        }
                    }
                ]
            },
            {
                "eid": 19580,
                "values": [
                    {
                        "cfidx": 2,
                        "val": "Six"
                    },
                    {
                        "val": "Six"
                    },
                    {
                        "cfidx": 1,
                        "val": "6"
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
                            "16252": "Six"
                        }
                    }
                ]
            },
            {
                "eid": 20447,
                "values": [
                    {
                        "cfidx": 2,
                        "val": "Eight"
                    },
                    {
                        "val": "Eight"
                    },
                    {
                        "cfidx": 2,
                        "val": "8"
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
                            "17473": "Eight"
                        }
                    }
                ]
            },
            {
                "eid": 20605,
                "values": [
                    {
                        "cfidx": 0,
                        "val": "Two"
                    },
                    {
                        "val": "Two"
                    },
                    {
                        "cfidx": 0,
                        "val": "2"
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
                            "15674": "Two"
                        }
                    }
                ]
            },
            {
                "eid": 22039,
                "values": [
                    {
                        "cfidx": 1,
                        "val": "Five"
                    },
                    {
                        "val": "Five"
                    },
                    {
                        "cfidx": 3,
                        "val": "5"
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
                            "18429": "Five"
                        }
                    }
                ]
            },
            {
                "eid": 22127,
                "values": [
                    {
                        "cfidx": 1,
                        "val": "Fourteen"
                    },
                    {
                        "val": "Fourteen"
                    },
                    {
                        "cfidx": 2,
                        "val": "14"
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
                            "15664": "Echo"
                        }
                    }
                ]
            }
        ]
    };

    // Report with adhoc pivot
    // "All Fields" with primary="Weekday", color="Boolean" and Value = Sum "Number", and Text = Count
    visDataTestData.allFieldsPivoted =
    {
        "meta": {
            "modified": "2016-03-14T11:51:44.607Z",
            "hideheader": false,
            "hideact": false,
            "dfid": 15684,
            "typefmtstyle": {
                "String": [
                   "Highlight",
                   "Icon"
                ],
                "Bool": [
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
                "ChoiceRelationship": [
                   "Highlight",
                   "Icon"
                ],
                "InlineRelationship": [
                   "Highlight",
                   "Icon"
                ],
                "Date": [
                   "Highlight",
                   "Icon",
                   "ProgressBar"
                ],
                "DateTime": [
                   "Highlight",
                   "Icon",
                   "ProgressBar"
                ],
                "Time": [
                   "Highlight",
                   "Icon",
                   "ProgressBar"
                ]
            },
            "rcols": {
                "12797": {
                    "entityname": true,
                    "maxlen": 200,
                    "tid": 18488,
                    "fid": 2816,
                    "ord": 0,
                    "regexerr": "The value must not contain angled brackets.",
                    "regex": "^[^<>]+$",
                    "oprtype": "InlineRelationship",
                    "type": "String",
                    "title": "Name"
                },
                "18683": {
                    "fid": 17849,
                    "ord": 1,
                    "defval": "",
                    "oprtype": "String",
                    "type": "String",
                    "title": "Single Line"
                },
                "20497": {
                    "mline": true,
                    "fid": 15890,
                    "ord": 2,
                    "defval": "",
                    "oprtype": "String",
                    "type": "String",
                    "title": "Multiline"
                },
                "12104": {
                    "fid": 17805,
                    "ord": 3,
                    "defval": "",
                    "oprtype": "Bool",
                    "type": "Bool",
                    "title": "Boolean"
                },
                "16990": {
                    "fid": 11913,
                    "ord": 4,
                    "defval": "",
                    "oprtype": "Int32",
                    "type": "Int32",
                    "title": "Number"
                },
                "17698": {
                    "mindec": 2,
                    "fid": 15105,
                    "ord": 5,
                    "places": 3,
                    "defval": "",
                    "oprtype": "Decimal",
                    "type": "Decimal",
                    "title": "Decimal"
                },
                "20973": {
                    "fid": 19674,
                    "ord": 6,
                    "places": 2,
                    "defval": "",
                    "oprtype": "Currency",
                    "type": "Currency",
                    "title": "Currency"
                },
                "16949": {
                    "fid": 15128,
                    "ord": 7,
                    "anpat": "",
                    "defval": "",
                    "oprtype": "Int32",
                    "type": "Int32",
                    "title": "Autonumber"
                },
                "22160": {
                    "ord": 8,
                    "oprtype": "Decimal",
                    "type": "Decimal",
                    "title": "Calculation"
                },
                "17498": {
                    "maxlen": 200,
                    "tid": 14023,
                    "fid": 2816,
                    "ord": 9,
                    "regexerr": "The value must not contain angled brackets.",
                    "regex": "^[^<>]+$",
                    "oprtype": "ChoiceRelationship",
                    "type": "ChoiceRelationship",
                    "title": "Weekday"
                },
                "15019": {
                    "aggcol": true,
                    "tid": 17255,
                    "ord": 10,
                    "oprtype": "ChoiceRelationship",
                    "type": "ChoiceRelationship",
                    "title": "List : Condiments"
                },
                "20978": {
                    "maxlen": 200,
                    "tid": 11886,
                    "fid": 2816,
                    "ord": 11,
                    "card": "ManyToOne",
                    "regexerr": "The value must not contain angled brackets.",
                    "regex": "^[^<>]+$",
                    "oprtype": "InlineRelationship",
                    "type": "InlineRelationship",
                    "title": "AA_Employee"
                },
                "16838": {
                    "maxlen": 200,
                    "tid": 18201,
                    "fid": 2816,
                    "ord": 12,
                    "card": "OneToOne",
                    "regexerr": "The value must not contain angled brackets.",
                    "regex": "^[^<>]+$",
                    "oprtype": "InlineRelationship",
                    "type": "InlineRelationship",
                    "title": "AA_Drinks"
                },
                "16354": {
                    "maxlen": 200,
                    "fid": 2816,
                    "ord": 13,
                    "regexerr": "The value must not contain angled brackets.",
                    "regex": "^[^<>]+$",
                    "oprtype": "Image",
                    "type": "Image",
                    "title": "New Image Field"
                },
                "14495": {
                    "maxlen": 200,
                    "tid": 21473,
                    "fid": 2816,
                    "ord": 14,
                    "card": "ManyToOne",
                    "regexerr": "The value must not contain angled brackets.",
                    "regex": "^[^<>]+$",
                    "oprtype": "InlineRelationship",
                    "type": "InlineRelationship",
                    "title": "AA_ChocBars (Rev)"
                },
                "15304": {
                    "maxlen": 200,
                    "tid": 11813,
                    "fid": 2816,
                    "ord": 15,
                    "card": "OneToOne",
                    "regexerr": "The value must not contain angled brackets.",
                    "regex": "^[^<>]+$",
                    "oprtype": "InlineRelationship",
                    "type": "InlineRelationship",
                    "title": "Scientist (Rev)"
                },
                "17459": {
                    "maxlen": 200,
                    "tid": 15347,
                    "fid": 2816,
                    "ord": 16,
                    "card": "ManyToMany",
                    "regexerr": "The value must not contain angled brackets.",
                    "regex": "^[^<>]+$",
                    "oprtype": "InlineRelationship",
                    "type": "InlineRelationship",
                    "title": "AA_Herb"
                },
                "11806": {
                    "maxlen": 200,
                    "tid": 21823,
                    "fid": 2816,
                    "ord": 17,
                    "card": "ManyToMany",
                    "regexerr": "The value must not contain angled brackets.",
                    "regex": "^[^<>]+$",
                    "oprtype": "InlineRelationship",
                    "type": "InlineRelationship",
                    "title": "AA_Truck"
                },
                "21856": {
                    "maxlen": 200,
                    "tid": 18835,
                    "fid": 2816,
                    "ord": 18,
                    "card": "OneToMany",
                    "regexerr": "The value must not contain angled brackets.",
                    "regex": "^[^<>]+$",
                    "oprtype": "InlineRelationship",
                    "type": "InlineRelationship",
                    "title": "AA_DogBreeds (Rev)"
                },
                "13347": {
                    "maxlen": 200,
                    "tid": 18070,
                    "fid": 2816,
                    "ord": 19,
                    "card": "ManyToMany",
                    "regexerr": "The value must not contain angled brackets.",
                    "regex": "^[^<>]+$",
                    "oprtype": "InlineRelationship",
                    "type": "InlineRelationship",
                    "title": "AA_Snacks (Rev)"
                },
                "12761": {
                    "fid": 20981,
                    "ord": 20,
                    "defval": "",
                    "oprtype": "Date",
                    "type": "Date",
                    "title": "Date"
                },
                "17749": {
                    "fid": 21250,
                    "ord": 21,
                    "defval": "",
                    "oprtype": "DateTime",
                    "type": "DateTime",
                    "title": "DateTime"
                },
                "21021": {
                    "fid": 13799,
                    "ord": 22,
                    "defval": "",
                    "oprtype": "Time",
                    "type": "Time",
                    "title": "Time"
                }
            },
            "sort": [
               {
                   "order": "Ascending",
                   "colid": "12797"
               }
            ],
            "choice": {
                "14023": [
                   {
                       "id": 13991,
                       "name": "Monday"
                   },
                   {
                       "id": 17830,
                       "name": "Tuesday"
                   },
                   {
                       "id": 21589,
                       "name": "Wednesday"
                   },
                   {
                       "id": 21561,
                       "name": "Thursday"
                   },
                   {
                       "id": 11921,
                       "name": "Friday"
                   },
                   {
                       "id": 14216,
                       "name": "Saturday"
                   },
                   {
                       "id": 20343,
                       "name": "Sunday"
                   }
                ],
                "17255": [
                   {
                       "id": 13626,
                       "name": "Chili"
                   },
                   {
                       "id": 20101,
                       "name": "Chutney"
                   },
                   {
                       "id": 16758,
                       "name": "Ketchup"
                   },
                   {
                       "id": 12431,
                       "name": "Mustard"
                   },
                   {
                       "id": 20434,
                       "name": "Pepper"
                   },
                   {
                       "id": 19414,
                       "name": "Pickle"
                   },
                   {
                       "id": 16141,
                       "name": "Relish"
                   },
                   {
                       "id": 12643,
                       "name": "Salt"
                   },
                   {
                       "id": 15969,
                       "name": "Vinegar"
                   },
                   {
                       "id": 20269,
                       "name": "Wasabi"
                   }
                ]
            },
            "inline": {
                "18488": 4998,
                "11886": 4998,
                "18201": 4998,
                "21473": 4998,
                "11813": 4998,
                "15347": 4998,
                "21823": 4998,
                "18835": 4998,
                "18070": 4998,
                "2904": 4998
            },
            "anlcols": {
                "13164": {
                    "ord": 6,
                    "doper": "GreaterThan",
                    "oper": "Unspecified",
                    "anltype": "Currency",
                    "type": "Currency",
                    "title": "Currency"
                },
                "13383": {
                    "tid": 14023,
                    "ord": 9,
                    "doper": "AnyOf",
                    "oper": "AnyOf",
                    "anltype": "ChoiceRelationship",
                    "type": "ChoiceRelationship",
                    "title": "Weekday"
                },
                "14083": {
                    "ord": 20,
                    "doper": "GreaterThan",
                    "oper": "Unspecified",
                    "anltype": "Date",
                    "type": "Date",
                    "title": "Date"
                },
                "14092": {
                    "ord": 5,
                    "doper": "GreaterThan",
                    "oper": "Unspecified",
                    "anltype": "Decimal",
                    "type": "Decimal",
                    "title": "Decimal"
                },
                "14248": {
                    "tid": 2904,
                    "ord": 13,
                    "doper": "AnyOf",
                    "oper": "AnyOf",
                    "anltype": "InlineRelationship",
                    "type": "InlineRelationship",
                    "title": "New Image Field"
                },
                "14263": {
                    "tid": 18201,
                    "ord": 12,
                    "doper": "AnyOf",
                    "oper": "AnyOf",
                    "anltype": "InlineRelationship",
                    "type": "InlineRelationship",
                    "title": "AA_Drinks"
                },
                "14366": {
                    "ord": 4,
                    "doper": "GreaterThan",
                    "oper": "Unspecified",
                    "anltype": "Int32",
                    "type": "Int32",
                    "title": "Number"
                },
                "14403": {
                    "tid": 18488,
                    "ord": 0,
                    "doper": "Contains",
                    "oper": "Unspecified",
                    "anltype": "InlineRelationship",
                    "type": "String",
                    "title": "Name"
                },
                "14406": {
                    "tid": 18835,
                    "ord": 18,
                    "doper": "AnyOf",
                    "oper": "Unspecified",
                    "anltype": "InlineRelationship",
                    "type": "InlineRelationship",
                    "title": "AA_DogBreeds (Rev)"
                },
                "14510": {
                    "ord": 1,
                    "doper": "Contains",
                    "oper": "Unspecified",
                    "anltype": "String",
                    "type": "String",
                    "title": "Single Line"
                },
                "14525": {
                    "ord": 22,
                    "doper": "GreaterThan",
                    "oper": "Unspecified",
                    "anltype": "Time",
                    "type": "Time",
                    "title": "Time"
                },
                "14577": {
                    "tid": 15347,
                    "ord": 16,
                    "doper": "AnyOf",
                    "oper": "Unspecified",
                    "anltype": "InlineRelationship",
                    "type": "InlineRelationship",
                    "title": "AA_Herb"
                },
                "14716": {
                    "ord": 3,
                    "doper": "Unspecified",
                    "oper": "Unspecified",
                    "anltype": "Bool",
                    "type": "Bool",
                    "title": "Boolean"
                },
                "15253": {
                    "ord": 7,
                    "doper": "GreaterThan",
                    "oper": "Unspecified",
                    "anltype": "Int32",
                    "type": "Int32",
                    "title": "Autonumber"
                },
                "15378": {
                    "tid": 21823,
                    "ord": 17,
                    "doper": "AnyOf",
                    "oper": "Unspecified",
                    "anltype": "InlineRelationship",
                    "type": "InlineRelationship",
                    "title": "AA_Truck"
                },
                "16380": {
                    "ord": 21,
                    "doper": "GreaterThan",
                    "oper": "Unspecified",
                    "anltype": "DateTime",
                    "type": "DateTime",
                    "title": "DateTime"
                },
                "16572": {
                    "ord": 2,
                    "doper": "Contains",
                    "oper": "Unspecified",
                    "anltype": "String",
                    "type": "String",
                    "title": "Multiline"
                },
                "16574": {
                    "tid": 18070,
                    "ord": 19,
                    "doper": "AnyOf",
                    "oper": "Unspecified",
                    "anltype": "InlineRelationship",
                    "type": "InlineRelationship",
                    "title": "AA_Snacks (Rev)"
                },
                "18266": {
                    "tid": 17255,
                    "ord": 10,
                    "doper": "AnyOf",
                    "oper": "AnyOf",
                    "anltype": "ChoiceRelationship",
                    "type": "ChoiceRelationship",
                    "title": "List : Condiments"
                },
                "18450": {
                    "ord": 8,
                    "doper": "GreaterThan",
                    "oper": "Unspecified",
                    "anltype": "Decimal",
                    "type": "Decimal",
                    "title": "Calculation"
                },
                "20271": {
                    "tid": 21473,
                    "ord": 14,
                    "doper": "AnyOf",
                    "oper": "AnyOf",
                    "anltype": "InlineRelationship",
                    "type": "InlineRelationship",
                    "title": "AA_ChocBars (Rev)"
                },
                "21052": {
                    "tid": 11886,
                    "ord": 11,
                    "doper": "AnyOf",
                    "oper": "AnyOf",
                    "anltype": "InlineRelationship",
                    "type": "InlineRelationship",
                    "title": "AA_Employee"
                },
                "21090": {
                    "tid": 11813,
                    "ord": 15,
                    "doper": "AnyOf",
                    "oper": "AnyOf",
                    "anltype": "InlineRelationship",
                    "type": "InlineRelationship",
                    "title": "Scientist (Rev)"
                }
            },
            "valrules": {
                "17698": {
                    "places": 3
                },
                "20973": {
                    "places": 2
                },
                "22160": {

                },
                "16354": {
                    "imgw": 16,
                    "imgh": 16,
                    "sizeid": 4982,
                    "scaleid": 3541
                }
            },
            "rmeta": {
                "rollup": true,
                "showoplbl": false,
                "groups": [
                   {
                       "12104": {
                           "collapsed": false,
                           "value": "Boolean",
                           "style": "groupList"
                       }
                   },
                   {
                       "17498": {
                           "collapsed": false,
                           "value": "Weekday",
                           "style": "groupList"
                       }
                   }
                ],
                "aggs": {
                    "16990": [
                       {
                           "type": "Int32",
                           "style": "aggSum"
                       }
                    ]
                }
            },
            "rdata": [
               {
                   "map": 0,
                   "hdrs": [
                      {
                          "12104": {
                              "val": "False"
                          }
                      },
                      {
                          "17498": {

                          }
                      }
                   ],
                   "aggs": {
                       "16990": [
                          {
                              "value": "1"
                          }
                       ]
                   },
                   "total": 1
               },
               {
                   "map": 0,
                   "hdrs": [
                      {
                          "12104": {
                              "val": "False"
                          }
                      },
                      {
                          "17498": {
                              "vals": {
                                  "11921": "Friday"
                              }
                          }
                      }
                   ],
                   "aggs": {
                       "16990": [
                          {
                              "value": "24"
                          }
                       ]
                   },
                   "total": 24
               },
               {
                   "map": 0,
                   "hdrs": [
                      {
                          "12104": {
                              "val": "True"
                          }
                      },
                      {
                          "17498": {
                              "vals": {
                                  "11921": "Friday"
                              }
                          }
                      }
                   ],
                   "aggs": {
                       "16990": [
                          {
                              "value": "32"
                          }
                       ]
                   },
                   "total": 32
               },
               {
                   "map": 0,
                   "hdrs": [
                      {
                          "12104": {
                              "val": "False"
                          }
                      },
                      {
                          "17498": {
                              "vals": {
                                  "13991": "Monday"
                              }
                          }
                      }
                   ],
                   "aggs": {
                       "16990": [
                          {
                              "value": "24"
                          }
                       ]
                   },
                   "total": 24
               },
               {
                   "map": 0,
                   "hdrs": [
                      {
                          "12104": {
                              "val": "True"
                          }
                      },
                      {
                          "17498": {
                              "vals": {
                                  "13991": "Monday"
                              }
                          }
                      }
                   ],
                   "aggs": {
                       "16990": [
                          {
                              "value": "24"
                          }
                       ]
                   },
                   "total": 24
               },
               {
                   "map": 0,
                   "hdrs": [
                      {
                          "12104": {
                              "val": "False"
                          }
                      },
                      {
                          "17498": {
                              "vals": {
                                  "14216": "Saturday"
                              }
                          }
                      }
                   ],
                   "aggs": {
                       "16990": [
                          {
                              "value": "24"
                          }
                       ]
                   },
                   "total": 24
               },
               {
                   "map": 0,
                   "hdrs": [
                      {
                          "12104": {
                              "val": "True"
                          }
                      },
                      {
                          "17498": {
                              "vals": {
                                  "14216": "Saturday"
                              }
                          }
                      }
                   ],
                   "aggs": {
                       "16990": [
                          {
                              "value": "20"
                          }
                       ]
                   },
                   "total": 20
               },
               {
                   "map": 0,
                   "hdrs": [
                      {
                          "12104": {
                              "val": "False"
                          }
                      },
                      {
                          "17498": {
                              "vals": {
                                  "17830": "Tuesday"
                              }
                          }
                      }
                   ],
                   "aggs": {
                       "16990": [
                          {
                              "value": "28"
                          }
                       ]
                   },
                   "total": 28
               },
               {
                   "map": 0,
                   "hdrs": [
                      {
                          "12104": {
                              "val": "True"
                          }
                      },
                      {
                          "17498": {
                              "vals": {
                                  "17830": "Tuesday"
                              }
                          }
                      }
                   ],
                   "aggs": {
                       "16990": [
                          {
                              "value": "24"
                          }
                       ]
                   },
                   "total": 24
               },
               {
                   "map": 0,
                   "hdrs": [
                      {
                          "12104": {
                              "val": "False"
                          }
                      },
                      {
                          "17498": {
                              "vals": {
                                  "20343": "Sunday"
                              }
                          }
                      }
                   ],
                   "aggs": {
                       "16990": [
                          {
                              "value": "20"
                          }
                       ]
                   },
                   "total": 20
               },
               {
                   "map": 0,
                   "hdrs": [
                      {
                          "12104": {
                              "val": "True"
                          }
                      },
                      {
                          "17498": {
                              "vals": {
                                  "20343": "Sunday"
                              }
                          }
                      }
                   ],
                   "aggs": {
                       "16990": [
                          {
                              "value": "24"
                          }
                       ]
                   },
                   "total": 24
               },
               {
                   "map": 0,
                   "hdrs": [
                      {
                          "12104": {
                              "val": "False"
                          }
                      },
                      {
                          "17498": {
                              "vals": {
                                  "21561": "Thursday"
                              }
                          }
                      }
                   ],
                   "aggs": {
                       "16990": [
                          {
                              "value": "20"
                          }
                       ]
                   },
                   "total": 20
               },
               {
                   "map": 0,
                   "hdrs": [
                      {
                          "12104": {
                              "val": "True"
                          }
                      },
                      {
                          "17498": {
                              "vals": {
                                  "21561": "Thursday"
                              }
                          }
                      }
                   ],
                   "aggs": {
                       "16990": [
                          {
                              "value": "24"
                          }
                       ]
                   },
                   "total": 24
               },
               {
                   "map": 0,
                   "hdrs": [
                      {
                          "12104": {
                              "val": "False"
                          }
                      },
                      {
                          "17498": {
                              "vals": {
                                  "21589": "Wednesday"
                              }
                          }
                      }
                   ],
                   "aggs": {
                       "16990": [
                          {
                              "value": "24"
                          }
                       ]
                   },
                   "total": 24
               },
               {
                   "map": 0,
                   "hdrs": [
                      {
                          "12104": {
                              "val": "True"
                          }
                      },
                      {
                          "17498": {
                              "vals": {
                                  "21589": "Wednesday"
                              }
                          }
                      }
                   ],
                   "aggs": {
                       "16990": [
                          {
                              "value": "28"
                          }
                       ]
                   },
                   "total": 28
               }
            ],
            "invalid": {
                "nodes": null,
                "columns": null,
                "conditions": null
            },
            "style": "Default",
            "title": "AF_All Fields"
        }
    };

})(visDataTestData || (visDataTestData = {}));
