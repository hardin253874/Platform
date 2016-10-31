// Copyright 2011-2016 Global Software Innovation Pty Ltd
var spreadsheetInfoTestData;

(function (spreadsheetInfoTestData) {
    spreadsheetInfoTestData.spreadsheetData =
    {
        "sheetCollection": [
           {
               "sheetName": "Sheet1",
               "sheetId": "Sheet1"
           },
           {
               "sheetName": "Department",
               "sheetId": "Department"
           }
        ],
        "initialSampleTable": {
            "columns": [
               {
                   "colId": "A",
                   "colName": "Name"
               },
               {
                   "colId": "B",
                   "colName": "Age"
               }
            ],
            "rows": [
               {
                   "vals": [
                      "Adam",
                      "35"
                   ]
               },
               {
                   "vals": [
                      "Betty",
                      "23"
                   ]
               },
               {
                   "vals": [
                      "Charles",
                      "25"
                   ]
               }
            ]
        },
        "importFileFormat": "excel",
        "fileName": null,
        "error": null
    };

    spreadsheetInfoTestData.sampleData =
    {
        "columns": [
            {
                "colId": "A",
                "colName": "Sales"
            }
        ],
        "rows": [
            {
                "vals": [
                    "Marketing"
                ]
            },
            {
                "vals": [
                    "Development"
                ]
            }
        ]
    };

    spreadsheetInfoTestData.allFieldType =
    {
        "results": [
            {
                "code": 200,
                "ids": [
                    18751
                ],
                "hint": "schema for import-18751",
                "name": null
            }
        ],
        "ids": [
        ],
        "entities": {
            "18751": {
                "3344": "test:allFields",
                "2854": "AA_All Fields",
                "3604": null,
                "3176": {
                    "r": [
                        12176,
                        14062,
                        15368,
                        15391,
                        16153,
                        18068,
                        18112,
                        19937,
                        21244,
                        21513
                    ]
                },
                "3194": {
                    "r": [
                    ]
                },
                "3218": {
                    "r": [
                        12985,
                        16059,
                        16750,
                        17078,
                        20367,
                        21368,
                        22332
                    ]
                },
                "3924": {
                    "r": [
                        13642,
                        15797,
                        15913,
                        18761,
                        18882,
                        19413
                    ]
                },
                "3557": {
                    "f": [
                        3929
                    ]
                },
                "3172": {
                    "r": [
                        12249,
                        14576,
                        19534,
                        20960,
                        21393
                    ]
                },
                "4110": {
                    "r": [
                    ]
                }
            },
            "12176": {
                "3344": "test:afNumber",
                "2854": "Number",
                "3514": null,
                "4390": null,
                "3604": null,
                "2685": false,
                "3783": false,
                "2915": false,
                "4337": {
                    "f": [
                        2852
                    ]
                },
                "4263": {
                    "f": [
                        21393
                    ]
                }
            },
            "2852": {
                "3344": "core:intField",
                "2854": "Int Field"
            },
            "21393": {
                "3344": null,
                "2854": "Numeric Fields",
                "3604": null
            },
            "14062": {
                "3344": "test:afTime",
                "2854": "Time",
                "3514": null,
                "4390": null,
                "3604": null,
                "2685": false,
                "3783": false,
                "2915": false,
                "4337": {
                    "f": [
                        2895
                    ]
                },
                "4263": {
                    "f": [
                        20960
                    ]
                }
            },
            "2895": {
                "3344": "core:timeField",
                "2854": "Time Field"
            },
            "20960": {
                "3344": null,
                "2854": "Date Fields",
                "3604": null
            },
            "15368": {
                "3344": "test:afDecimal",
                "2854": "Decimal",
                "3514": null,
                "4390": null,
                "3604": null,
                "2685": false,
                "3783": false,
                "2915": false,
                "4337": {
                    "f": [
                        3943
                    ]
                },
                "4263": {
                    "f": [
                        21393
                    ]
                }
            },
            "3943": {
                "3344": "core:decimalField",
                "2854": "Decimal Field"
            },
            "15391": {
                "3344": "test:afAutonumber",
                "2854": "Autonumber",
                "3514": null,
                "4390": null,
                "3604": null,
                "2685": false,
                "3783": false,
                "2915": false,
                "4337": {
                    "f": [
                        3742
                    ]
                },
                "4263": {
                    "f": [
                        21393
                    ]
                }
            },
            "3742": {
                "3344": "core:autoNumberField",
                "2854": "AutoNumber Field"
            },
            "16153": {
                "3344": "test:afMultiline",
                "2854": "Multiline",
                "3514": null,
                "4390": null,
                "3604": null,
                "2685": false,
                "3783": false,
                "2915": false,
                "4337": {
                    "f": [
                        3233
                    ]
                },
                "4263": {
                    "f": [
                        14576
                    ]
                }
            },
            "3233": {
                "3344": "core:stringField",
                "2854": "String Field"
            },
            "14576": {
                "3344": null,
                "2854": "Text Fields",
                "3604": null
            },
            "18068": {
                "3344": "test:afBoolean",
                "2854": "Boolean",
                "3514": null,
                "4390": null,
                "3604": null,
                "2685": false,
                "3783": false,
                "2915": false,
                "4337": {
                    "f": [
                        3704
                    ]
                },
                "4263": {
                    "f": [
                        14576
                    ]
                }
            },
            "3704": {
                "3344": "core:boolField",
                "2854": "Boolean Field"
            },
            "18112": {
                "3344": "test:afString",
                "2854": "Single Line",
                "3514": null,
                "4390": null,
                "3604": null,
                "2685": false,
                "3783": false,
                "2915": false,
                "4337": {
                    "f": [
                        3233
                    ]
                },
                "4263": {
                    "f": [
                        14576
                    ]
                }
            },
            "19937": {
                "3344": "test:afCurrency",
                "2854": "Currency",
                "3514": null,
                "4390": null,
                "3604": null,
                "2685": false,
                "3783": false,
                "2915": false,
                "4337": {
                    "f": [
                        3057
                    ]
                },
                "4263": {
                    "f": [
                        21393
                    ]
                }
            },
            "3057": {
                "3344": "core:currencyField",
                "2854": "Currency Field"
            },
            "21244": {
                "3344": "test:afDate",
                "2854": "Date",
                "3514": null,
                "4390": null,
                "3604": null,
                "2685": false,
                "3783": false,
                "2915": false,
                "4337": {
                    "f": [
                        3915
                    ]
                },
                "4263": {
                    "f": [
                        20960
                    ]
                }
            },
            "3915": {
                "3344": "core:dateField",
                "2854": "Date Field"
            },
            "21513": {
                "3344": "test:afDateTime",
                "2854": "DateTime",
                "3514": null,
                "4390": null,
                "3604": null,
                "2685": false,
                "3783": false,
                "2915": false,
                "4337": {
                    "f": [
                        4297
                    ]
                },
                "4263": {
                    "f": [
                        20960
                    ]
                }
            },
            "4297": {
                "3344": "core:dateTimeField",
                "2854": "DateTime Field"
            },
            "12985": {
                "3344": "test:drinks",
                "2854": "AA_Drinks",
                "2763": "AA_All Fields",
                "3771": "AA_Drinks",
                "3615": null,
                "3275": null,
                "3604": "All Fields to Drink relationship",
                "2641": false,
                "3581": null,
                "3799": false,
                "3269": false,
                "4223": {
                    "f": [
                        3553
                    ]
                },
                "3065": {
                    "f": [
                        14123
                    ]
                },
                "3218": {
                    "f": [
                        18751
                    ]
                },
                "3924": {
                    "f": [
                        18464
                    ]
                },
                "4228": {
                    "f": [
                    ]
                },
                "2709": {
                    "f": [
                        12249
                    ]
                }
            },
            "3553": {
                "3344": "core:oneToOne"
            },
            "14123": {
                "3344": "core:testSolution",
                "2854": "Test Solution"
            },
            "18464": {
                "2854": "AA_Drink",
                "3344": "test:drink",
                "3557": {
                    "f": [
                        3929
                    ]
                }
            },
            "3929": {
                "3344": "core:userResource",
                "2854": "Editable Resource",
                "3604": "This is the parent type for all business resources, including both application resources and user-created resources.",
                "3176": {
                    "r": [
                    ]
                },
                "3194": {
                    "r": [
                    ]
                },
                "3218": {
                    "r": [
                    ]
                },
                "3924": {
                    "r": [
                        3096,
                        3119,
                        3193
                    ]
                },
                "3557": {
                    "f": [
                        3505
                    ]
                },
                "3172": {
                    "r": [
                    ]
                },
                "4110": {
                    "r": [
                    ]
                }
            },
            "3096": {
                "3344": "core:recordToPresent",
                "2854": "Record",
                "2763": "Task",
                "3771": "Record",
                "3615": null,
                "3275": null,
                "3604": "The record that is related to this task. This field is optional.",
                "2641": false,
                "3581": false,
                "3799": false,
                "3269": true,
                "4223": {
                    "f": [
                        3922
                    ]
                },
                "3065": {
                    "f": [
                        3852
                    ]
                },
                "3218": {
                    "f": [
                        3406
                    ]
                },
                "3924": {
                    "f": [
                        3929
                    ]
                },
                "4228": {
                    "f": [
                    ]
                },
                "2709": {
                    "f": [
                        3913
                    ]
                }
            },
            "3922": {
                "3344": "core:manyToOne"
            },
            "3852": {
                "3344": "core:coreSolution",
                "2854": "ReadiNow Core"
            },
            "3406": {
                "2854": "Approval Task",
                "3344": "core:displayFormUserTask",
                "3557": {
                    "f": [
                        4292,
                        4442
                    ]
                }
            },
            "4292": {
                "3344": "core:baseUserTask"
            },
            "4442": {
                "3344": "core:transitionTask"
            },
            "3913": {
                "3344": "core:displayFormUserTaskDetails",
                "2854": "Approval Task Details",
                "3604": null
            },
            "3119": {
                "3344": "core:objectReferencedInLog",
                "2854": "Referenced object",
                "2763": "Log entries for object",
                "3771": "Object",
                "3615": null,
                "3275": null,
                "3604": "The object that is the subject of the log message.",
                "2641": false,
                "3581": false,
                "3799": false,
                "3269": false,
                "4223": {
                    "f": [
                        3922
                    ]
                },
                "3065": {
                    "f": [
                        3852
                    ]
                },
                "3218": {
                    "f": [
                        3963
                    ]
                },
                "3924": {
                    "f": [
                        3929
                    ]
                },
                "4228": {
                    "f": [
                        2761
                    ]
                },
                "2709": {
                    "f": [
                        2761
                    ]
                }
            },
            "3963": {
                "2854": "Managed Object Log Entry",
                "3344": "core:managedObjectLogEntry",
                "3557": {
                    "f": [
                        3505
                    ]
                }
            },
            "3505": {
                "3344": "core:resource",
                "2854": "Resource",
                "3604": "The root resource type from which all other types inherit.",
                "3176": {
                    "r": [
                        2689,
                        2854,
                        3344,
                        3604,
                        4134,
                        4251,
                        4299
                    ]
                },
                "3194": {
                    "r": [
                    ]
                },
                "3218": {
                    "r": [
                        3065,
                        3248,
                        3542,
                        3968,
                        3982,
                        4275,
                        4337,
                        4359,
                        4495,
                        4565,
                        4608,
                        4819,
                        4935,
                        17255
                    ]
                },
                "3924": {
                    "r": [
                        2749,
                        3085,
                        3145,
                        3345,
                        3430,
                        4015,
                        4150,
                        4279,
                        4476,
                        4816,
                        4906,
                        4930,
                        14348
                    ]
                },
                "3557": {
                    "f": [
                    ]
                },
                "3172": {
                    "r": [
                        3572,
                        4506
                    ]
                },
                "4110": {
                    "r": [
                    ]
                }
            },
            "2689": {
                "3344": "core:canModify",
                "2854": "Can modify access",
                "3514": null,
                "4390": null,
                "3604": "If the field is set to true, then the context user has modify access for a given entity.",
                "2685": true,
                "3783": false,
                "2915": false,
                "4337": {
                    "f": [
                        3704
                    ]
                },
                "4263": {
                    "f": [
                        3572
                    ]
                }
            },
            "3572": {
                "3344": "core:system",
                "2854": "System",
                "3604": "Contains System Fields"
            },
            "2854": {
                "3344": "core:name",
                "2854": "Name",
                "3514": null,
                "4390": null,
                "3604": "The 'name' field of every resource.",
                "2685": false,
                "3783": false,
                "2915": false,
                "4337": {
                    "f": [
                        3233
                    ]
                },
                "4263": {
                    "f": [
                        4506
                    ]
                }
            },
            "4506": {
                "3344": "core:default",
                "2854": "Default",
                "3604": "Contains Name and Description Fields"
            },
            "3344": {
                "3344": "core:alias",
                "2854": "Alias",
                "3514": null,
                "4390": null,
                "3604": "The 'alias' field of every resource.",
                "2685": true,
                "3783": false,
                "2915": false,
                "4337": {
                    "f": [
                        3835
                    ]
                },
                "4263": {
                    "f": [
                    ]
                }
            },
            "3835": {
                "3344": "core:aliasField",
                "2854": "Alias Field"
            },
            "3604": {
                "3344": "core:description",
                "2854": "Description",
                "3514": null,
                "4390": null,
                "3604": "The 'description' field of every resource.",
                "2685": false,
                "3783": false,
                "2915": false,
                "4337": {
                    "f": [
                        3233
                    ]
                },
                "4263": {
                    "f": [
                        4506
                    ]
                }
            },
            "4134": {
                "3344": "core:canDelete",
                "2854": "Can delete access",
                "3514": null,
                "4390": null,
                "3604": "If the field is set to true, then context user has delete access for a given entity.",
                "2685": true,
                "3783": false,
                "2915": false,
                "4337": {
                    "f": [
                        3704
                    ]
                },
                "4263": {
                    "f": [
                        3572
                    ]
                }
            },
            "4251": {
                "3344": "core:createdDate",
                "2854": "Created date",
                "3514": null,
                "4390": null,
                "3604": "The date/time when this resource was created or imported.",
                "2685": false,
                "3783": false,
                "2915": true,
                "4337": {
                    "f": [
                        4297
                    ]
                },
                "4263": {
                    "f": [
                        3572
                    ]
                }
            },
            "4299": {
                "3344": "core:modifiedDate",
                "2854": "Modified date",
                "3514": null,
                "4390": null,
                "3604": "The date/time when this resource was last modified.",
                "2685": false,
                "3783": false,
                "2915": true,
                "4337": {
                    "f": [
                        4297
                    ]
                },
                "4263": {
                    "f": [
                        3572
                    ]
                }
            },
            "3065": {
                "3344": "core:inSolution",
                "2854": "Resource in application",
                "2763": null,
                "3771": null,
                "3615": null,
                "3275": null,
                "3604": "Indicates what application a resource belongs to.",
                "2641": false,
                "3581": false,
                "3799": true,
                "3269": false,
                "4223": {
                    "f": [
                        4221
                    ]
                },
                "3065": {
                    "f": [
                        3852
                    ]
                },
                "3218": {
                    "f": [
                        3505
                    ]
                },
                "3924": {
                    "f": [
                        3317
                    ]
                },
                "4228": {
                    "f": [
                        2758
                    ]
                },
                "2709": {
                    "f": [
                        2758
                    ]
                }
            },
            "4221": {
                "3344": "core:manyToMany"
            },
            "3317": {
                "2854": "Application",
                "3344": "core:solution",
                "3557": {
                    "f": [
                        2845
                    ]
                }
            },
            "2845": {
                "3344": "core:visualController"
            },
            "2758": {
                "3344": "core:solutionDetails",
                "2854": "Application Details",
                "3604": null
            },
            "3248": {
                "3344": "core:securityOwner",
                "2854": "Owner",
                "2763": "Owns resource",
                "3771": "Owned by",
                "3615": null,
                "3275": null,
                "3604": "The owner of a resources. Resource owners always have permission to see their resource, irrespective of other security rules. Users are automatically made the owner of any resource that they create.",
                "2641": false,
                "3581": false,
                "3799": false,
                "3269": true,
                "4223": {
                    "f": [
                        3922
                    ]
                },
                "3065": {
                    "f": [
                        3852
                    ]
                },
                "3218": {
                    "f": [
                        3505
                    ]
                },
                "3924": {
                    "f": [
                        3830
                    ]
                },
                "4228": {
                    "f": [
                    ]
                },
                "2709": {
                    "f": [
                        3572
                    ]
                }
            },
            "3830": {
                "2854": "User Account",
                "3344": "core:userAccount",
                "3557": {
                    "f": [
                        4282
                    ]
                }
            },
            "4282": {
                "3344": "core:subject",
                "2854": "Subject",
                "3557": {
                    "f": [
                        3505
                    ]
                }
            },
            "3542": {
                "3344": "core:isRootForStructureView",
                "2854": "Structure view root resource",
                "2763": null,
                "3771": null,
                "3615": null,
                "3275": null,
                "3604": null,
                "2641": false,
                "3581": false,
                "3799": true,
                "3269": false,
                "4223": {
                    "f": [
                        4221
                    ]
                },
                "3065": {
                    "f": [
                        3852
                    ]
                },
                "3218": {
                    "f": [
                        3505
                    ]
                },
                "3924": {
                    "f": [
                        3720
                    ]
                },
                "4228": {
                    "f": [
                        4363
                    ]
                },
                "2709": {
                    "f": [
                    ]
                }
            },
            "3720": {
                "2854": "Hierarchy",
                "3344": "core:structureView",
                "3557": {
                    "f": [
                        3247
                    ]
                }
            },
            "3247": {
                "3344": "core:resourcePicker"
            },
            "4363": {
                "3344": "core:structureViewDetails",
                "2854": "Structure View Details",
                "3604": null
            },
            "3968": {
                "3344": "core:inStructureLevel",
                "2854": "Structure level members",
                "2763": null,
                "3771": "Structure levels",
                "3615": null,
                "3275": null,
                "3604": null,
                "2641": false,
                "3581": false,
                "3799": true,
                "3269": false,
                "4223": {
                    "f": [
                        4221
                    ]
                },
                "3065": {
                    "f": [
                        3852
                    ]
                },
                "3218": {
                    "f": [
                        3505
                    ]
                },
                "3924": {
                    "f": [
                        3752
                    ]
                },
                "4228": {
                    "f": [
                        4261
                    ]
                },
                "2709": {
                    "f": [
                    ]
                }
            },
            "3752": {
                "2854": "Structure Level",
                "3344": "core:structureLevel",
                "3557": {
                    "f": [
                        3505
                    ]
                }
            },
            "4261": {
                "3344": "core:structureLevelDetails",
                "2854": "Structure Level Details",
                "3604": null
            },
            "3982": {
                "3344": "core:resourceHasResourceKeyDataHashes",
                "2854": "Resource has resource key data hash",
                "2763": null,
                "3771": null,
                "3615": null,
                "3275": null,
                "3604": "Associates a resource key data hash with a resource.",
                "2641": false,
                "3581": false,
                "3799": true,
                "3269": false,
                "4223": {
                    "f": [
                        3312
                    ]
                },
                "3065": {
                    "f": [
                        3852
                    ]
                },
                "3218": {
                    "f": [
                        3505
                    ]
                },
                "3924": {
                    "f": [
                        3543
                    ]
                },
                "4228": {
                    "f": [
                        4079
                    ]
                },
                "2709": {
                    "f": [
                    ]
                }
            },
            "3312": {
                "3344": "core:oneToMany"
            },
            "3543": {
                "2854": "Resource Key Data Hash",
                "3344": "core:resourceKeyDataHash",
                "3557": {
                    "f": [
                        3505
                    ]
                }
            },
            "4079": {
                "3344": "core:resourceKeyDataHashDetails",
                "2854": "Resource Key Data Hash Details",
                "3604": null
            },
            "4275": {
                "3344": "core:flags",
                "2854": "Resource flags",
                "2763": null,
                "3771": null,
                "3615": null,
                "3275": null,
                "3604": null,
                "2641": false,
                "3581": false,
                "3799": true,
                "3269": true,
                "4223": {
                    "f": [
                        4221
                    ]
                },
                "3065": {
                    "f": [
                        3852
                    ]
                },
                "3218": {
                    "f": [
                        3505
                    ]
                },
                "3924": {
                    "f": [
                        3857
                    ]
                },
                "4228": {
                    "f": [
                    ]
                },
                "2709": {
                    "f": [
                    ]
                }
            },
            "3857": {
                "2854": "Resource Flags",
                "3344": "core:resourceFlagsEnum",
                "3557": {
                    "f": [
                        4411
                    ]
                }
            },
            "4411": {
                "3344": "core:enumValue"
            },
            "4337": {
                "3344": "core:isOfType",
                "2854": "Resource is of type",
                "2763": "Instances",
                "3771": "Resource type",
                "3615": null,
                "3275": null,
                "3604": null,
                "2641": false,
                "3581": false,
                "3799": false,
                "3269": false,
                "4223": {
                    "f": [
                        4221
                    ]
                },
                "3065": {
                    "f": [
                        3852
                    ]
                },
                "3218": {
                    "f": [
                        3505
                    ]
                },
                "3924": {
                    "f": [
                        2820
                    ]
                },
                "4228": {
                    "f": [
                        3303
                    ]
                },
                "2709": {
                    "f": [
                        3572
                    ]
                }
            },
            "2820": {
                "2854": "Type",
                "3344": "core:type",
                "3557": {
                    "f": [
                        3075,
                        4246
                    ]
                }
            },
            "3075": {
                "3344": "core:schema"
            },
            "4246": {
                "3344": "core:securableEntity"
            },
            "3303": {
                "3344": "core:typeDetails",
                "2854": "Type Details",
                "3604": null
            },
            "4359": {
                "3344": "core:createdBy",
                "2854": "Created by",
                "2763": "Created",
                "3771": "Created by",
                "3615": null,
                "3275": null,
                "3604": null,
                "2641": false,
                "3581": false,
                "3799": false,
                "3269": true,
                "4223": {
                    "f": [
                        3922
                    ]
                },
                "3065": {
                    "f": [
                        3852
                    ]
                },
                "3218": {
                    "f": [
                        3505
                    ]
                },
                "3924": {
                    "f": [
                        3830
                    ]
                },
                "4228": {
                    "f": [
                    ]
                },
                "2709": {
                    "f": [
                        3572
                    ]
                }
            },
            "4495": {
                "3344": "core:lastModifiedBy",
                "2854": "Modified by",
                "2763": "Last to modify",
                "3771": "Last modified by",
                "3615": null,
                "3275": null,
                "3604": null,
                "2641": false,
                "3581": false,
                "3799": false,
                "3269": true,
                "4223": {
                    "f": [
                        3922
                    ]
                },
                "3065": {
                    "f": [
                        3852
                    ]
                },
                "3218": {
                    "f": [
                        3505
                    ]
                },
                "3924": {
                    "f": [
                        3830
                    ]
                },
                "4228": {
                    "f": [
                    ]
                },
                "2709": {
                    "f": [
                        3572
                    ]
                }
            },
            "4565": {
                "3344": "console:resourceConsoleBehavior",
                "2854": "Resource console behavior",
                "2763": null,
                "3771": null,
                "3615": null,
                "3275": null,
                "3604": "Links an individual resource to a description of how it should behave in the console.",
                "2641": false,
                "3581": false,
                "3799": true,
                "3269": false,
                "4223": {
                    "f": [
                        3553
                    ]
                },
                "3065": {
                    "f": [
                        4671
                    ]
                },
                "3218": {
                    "f": [
                        3505
                    ]
                },
                "3924": {
                    "f": [
                        4742
                    ]
                },
                "4228": {
                    "f": [
                        4856
                    ]
                },
                "2709": {
                    "f": [
                    ]
                }
            },
            "4671": {
                "3344": "core:consoleSolution",
                "2854": "ReadiNow Console"
            },
            "4742": {
                "2854": "Console Behavior",
                "3344": "console:consoleBehavior",
                "3557": {
                    "f": [
                        3505
                    ]
                }
            },
            "4856": {
                "3344": "console:consoleBehaviorDetails",
                "2854": "Console Behavior Details",
                "3604": null
            },
            "4608": {
                "3344": "console:selectionBehavior",
                "2854": "Selection behavior",
                "2763": null,
                "3771": null,
                "3615": null,
                "3275": null,
                "3604": "Links a report, chart, or relationship control to the way that its selected items should show their context menu.",
                "2641": false,
                "3581": false,
                "3799": true,
                "3269": false,
                "4223": {
                    "f": [
                        3553
                    ]
                },
                "3065": {
                    "f": [
                        4671
                    ]
                },
                "3218": {
                    "f": [
                        3505
                    ]
                },
                "3924": {
                    "f": [
                        4742
                    ]
                },
                "4228": {
                    "f": [
                        4856
                    ]
                },
                "2709": {
                    "f": [
                    ]
                }
            },
            "4819": {
                "3344": "console:shortcutInFolder",
                "2854": "Shortcut in folder",
                "2763": null,
                "3771": null,
                "3615": null,
                "3275": null,
                "3604": "This relationship indicates that a resource should appear in the console under the specified folder as a shortcut.",
                "2641": false,
                "3581": false,
                "3799": true,
                "3269": false,
                "4223": {
                    "f": [
                        4221
                    ]
                },
                "3065": {
                    "f": [
                        4671
                    ]
                },
                "3218": {
                    "f": [
                        3505
                    ]
                },
                "3924": {
                    "f": [
                        4569
                    ]
                },
                "4228": {
                    "f": [
                        5087
                    ]
                },
                "2709": {
                    "f": [
                    ]
                }
            },
            "4569": {
                "2854": "Navigation Container",
                "3344": "console:navContainer",
                "3557": {
                    "f": [
                        2845,
                        5161
                    ]
                }
            },
            "5161": {
                "3344": "console:navigationElement"
            },
            "5087": {
                "3344": "console:navContainerDetails",
                "2854": "Navigation Container Details",
                "3604": null
            },
            "4935": {
                "3344": "console:resourceInFolder",
                "2854": "Resource in folder",
                "2763": null,
                "3771": null,
                "3615": null,
                "3275": null,
                "3604": "This relationship indicates that a resource should appear in the console under the specified folder.",
                "2641": false,
                "3581": false,
                "3799": true,
                "3269": false,
                "4223": {
                    "f": [
                        4221
                    ]
                },
                "3065": {
                    "f": [
                        4671
                    ]
                },
                "3218": {
                    "f": [
                        3505
                    ]
                },
                "3924": {
                    "f": [
                        4569
                    ]
                },
                "4228": {
                    "f": [
                        5087
                    ]
                },
                "2709": {
                    "f": [
                    ]
                }
            },
            "17255": {
                "3344": "core:resourceInSecurityGroup",
                "2854": "Resource in security group",
                "2763": "Contains Resources",
                "3771": "In security groups",
                "3615": null,
                "3275": null,
                "3604": "Roles that a user is a member of.",
                "2641": false,
                "3581": null,
                "3799": true,
                "3269": false,
                "4223": {
                    "f": [
                        4221
                    ]
                },
                "3065": {
                    "f": [
                        14123
                    ]
                },
                "3218": {
                    "f": [
                        3505
                    ]
                },
                "3924": {
                    "f": [
                    ]
                },
                "4228": {
                    "f": [
                    ]
                },
                "2709": {
                    "f": [
                    ]
                }
            },
            "2749": {
                "3344": "core:defaultPointTo",
                "2854": "Default value",
                "2763": null,
                "3771": null,
                "3615": null,
                "3275": null,
                "3604": "Describes the default value for a relationship or choice field.",
                "2641": false,
                "3581": false,
                "3799": true,
                "3269": true,
                "4223": {
                    "f": [
                        3922
                    ]
                },
                "3065": {
                    "f": [
                        3852
                    ]
                },
                "3218": {
                    "f": [
                        3008
                    ]
                },
                "3924": {
                    "f": [
                        3505
                    ]
                },
                "4228": {
                    "f": [
                    ]
                },
                "2709": {
                    "f": [
                    ]
                }
            },
            "3008": {
                "2854": "Relationship",
                "3344": "core:relationship",
                "3557": {
                    "f": [
                        3075
                    ]
                }
            },
            "3085": {
                "3344": "core:allowAccessInternal",
                "2854": "Allow access internal",
                "2763": "Subjects (internal)",
                "3771": "Allowed resources (internal)",
                "3615": null,
                "3275": null,
                "3604": "List of resources that are allowed to be accessed.",
                "2641": false,
                "3581": false,
                "3799": true,
                "3269": true,
                "4223": {
                    "f": [
                        4221
                    ]
                },
                "3065": {
                    "f": [
                        3852
                    ]
                },
                "3218": {
                    "f": [
                        4282
                    ]
                },
                "3924": {
                    "f": [
                        3505
                    ]
                },
                "4228": {
                    "f": [
                    ]
                },
                "2709": {
                    "f": [
                        2859
                    ]
                }
            },
            "2859": {
                "3344": "core:subjectDetails",
                "2854": "Subject Details",
                "3604": null
            },
            "3145": {
                "3344": "core:resourceParameterValue",
                "2854": "Record parameter value",
                "2763": null,
                "3771": null,
                "3615": null,
                "3275": null,
                "3604": "The value being passed to a record parameter.",
                "2641": false,
                "3581": false,
                "3799": false,
                "3269": true,
                "4223": {
                    "f": [
                        3922
                    ]
                },
                "3065": {
                    "f": [
                        3852
                    ]
                },
                "3218": {
                    "f": [
                        3353
                    ]
                },
                "3924": {
                    "f": [
                        3505
                    ]
                },
                "4228": {
                    "f": [
                    ]
                },
                "2709": {
                    "f": [
                        3985
                    ]
                }
            },
            "3353": {
                "2854": "Record Argument",
                "3344": "core:resourceArgument",
                "3557": {
                    "f": [
                        3478,
                        4395
                    ]
                }
            },
            "3478": {
                "3344": "core:activityArgument"
            },
            "4395": {
                "3344": "core:typedArgument"
            },
            "3985": {
                "3344": "core:resourceArgumentDetails",
                "2854": "Record Argument Details",
                "3604": null
            },
            "3345": {
                "3344": "core:toTypeDefaultValue",
                "2854": "To type default value",
                "2763": null,
                "3771": null,
                "3615": null,
                "3275": null,
                "3604": null,
                "2641": false,
                "3581": false,
                "3799": true,
                "3269": true,
                "4223": {
                    "f": [
                        3922
                    ]
                },
                "3065": {
                    "f": [
                        3852
                    ]
                },
                "3218": {
                    "f": [
                        3008
                    ]
                },
                "3924": {
                    "f": [
                        3505
                    ]
                },
                "4228": {
                    "f": [
                    ]
                },
                "2709": {
                    "f": [
                    ]
                }
            },
            "3430": {
                "3344": "core:resourceArgumentValue",
                "2854": "Resource argument value",
                "2763": null,
                "3771": null,
                "3615": null,
                "3275": null,
                "3604": null,
                "2641": false,
                "3581": false,
                "3799": false,
                "3269": true,
                "4223": {
                    "f": [
                        3922
                    ]
                },
                "3065": {
                    "f": [
                        3852
                    ]
                },
                "3218": {
                    "f": [
                        4143
                    ]
                },
                "3924": {
                    "f": [
                        3505
                    ]
                },
                "4228": {
                    "f": [
                    ]
                },
                "2709": {
                    "f": [
                        4387
                    ]
                }
            },
            "4143": {
                "2854": "Argument Value",
                "3344": "core:argumentValue",
                "3557": {
                    "f": [
                        3505
                    ]
                }
            },
            "4387": {
                "3344": "core:argumentValueDetails",
                "2854": "Argument Value Details",
                "3604": null
            },
            "4015": {
                "3344": "core:referencedEntity",
                "2854": "Referenced entity",
                "2763": null,
                "3771": null,
                "3615": null,
                "3275": null,
                "3604": "The entity that has been named in the expression.",
                "2641": false,
                "3581": false,
                "3799": false,
                "3269": true,
                "4223": {
                    "f": [
                        3922
                    ]
                },
                "3065": {
                    "f": [
                        3852
                    ]
                },
                "3218": {
                    "f": [
                        3490
                    ]
                },
                "3924": {
                    "f": [
                        3505
                    ]
                },
                "4228": {
                    "f": [
                        3692
                    ]
                },
                "2709": {
                    "f": [
                        3348
                    ]
                }
            },
            "3490": {
                "2854": "Expression Entity",
                "3344": "core:namedReference",
                "3557": {
                    "f": [
                        3505
                    ]
                }
            },
            "3692": {
                "3344": "core:activityArgumentDetails",
                "2854": "Argument Details",
                "3604": null
            },
            "3348": {
                "3344": "core:wfExpressionDetails",
                "2854": "Workflow Expression Details",
                "3604": null
            },
            "4150": {
                "3344": "core:fromTypeDefaultValue",
                "2854": "From type default value",
                "2763": null,
                "3771": null,
                "3615": null,
                "3275": null,
                "3604": null,
                "2641": false,
                "3581": false,
                "3799": true,
                "3269": true,
                "4223": {
                    "f": [
                        3922
                    ]
                },
                "3065": {
                    "f": [
                        3852
                    ]
                },
                "3218": {
                    "f": [
                        3008
                    ]
                },
                "3924": {
                    "f": [
                        3505
                    ]
                },
                "4228": {
                    "f": [
                    ]
                },
                "2709": {
                    "f": [
                    ]
                }
            },
            "4279": {
                "3344": "core:resourceListParameterValues",
                "2854": "Record list parameter values",
                "2763": null,
                "3771": null,
                "3615": null,
                "3275": null,
                "3604": "The values being passed to a record list parameter.",
                "2641": false,
                "3581": false,
                "3799": false,
                "3269": true,
                "4223": {
                    "f": [
                        4221
                    ]
                },
                "3065": {
                    "f": [
                        3852
                    ]
                },
                "3218": {
                    "f": [
                        2657
                    ]
                },
                "3924": {
                    "f": [
                        3505
                    ]
                },
                "4228": {
                    "f": [
                    ]
                },
                "2709": {
                    "f": [
                        2823
                    ]
                }
            },
            "2657": {
                "2854": "Record List",
                "3344": "core:resourceListArgument",
                "3557": {
                    "f": [
                        3478,
                        4395
                    ]
                }
            },
            "2823": {
                "3344": "core:resourceListArgumentDetails",
                "2854": "Record List Details",
                "3604": null
            },
            "4476": {
                "3344": "core:boardDimensionValue",
                "2854": "Board dimension value",
                "2763": null,
                "3771": "Dimension value",
                "3615": null,
                "3275": null,
                "3604": "A dimension value to show on this board.",
                "2641": false,
                "3581": false,
                "3799": false,
                "3269": true,
                "4223": {
                    "f": [
                        4221
                    ]
                },
                "3065": {
                    "f": [
                        3852
                    ]
                },
                "3218": {
                    "f": [
                        4318
                    ]
                },
                "3924": {
                    "f": [
                        3505
                    ]
                },
                "4228": {
                    "f": [
                    ]
                },
                "2709": {
                    "f": [
                        4056
                    ]
                }
            },
            "4318": {
                "2854": "Board Dimension",
                "3344": "core:boardDimension",
                "3557": {
                    "f": [
                        3505
                    ]
                }
            },
            "4056": {
                "3344": "core:boardDimensionDetails",
                "2854": "Board Dimension Details",
                "3604": null
            },
            "4816": {
                "3344": "console:wbcDisableControlBasedOnResources",
                "2854": "Disable control based on resources",
                "2763": null,
                "3771": null,
                "3615": null,
                "3275": null,
                "3604": "The resources that determine if this control is to be disabled.",
                "2641": false,
                "3581": false,
                "3799": true,
                "3269": true,
                "4223": {
                    "f": [
                        3312
                    ]
                },
                "3065": {
                    "f": [
                        4671
                    ]
                },
                "3218": {
                    "f": [
                        5035
                    ]
                },
                "3924": {
                    "f": [
                        3505
                    ]
                },
                "4228": {
                    "f": [
                    ]
                },
                "2709": {
                    "f": [
                        5058
                    ]
                }
            },
            "5035": {
                "2854": "Workflow Button",
                "3344": "console:workflowButtonControl",
                "3557": {
                    "f": [
                        4607
                    ]
                }
            },
            "4607": {
                "3344": "console:controlOnForm"
            },
            "5058": {
                "3344": "console:workflowButtonControlDetails",
                "2854": "Workflow Button Details",
                "3604": null
            },
            "4906": {
                "3344": "console:relationshipDefaultValue",
                "2854": "Default value",
                "2763": null,
                "3771": null,
                "3615": null,
                "3275": null,
                "3604": "The default value of relationship field control",
                "2641": false,
                "3581": false,
                "3799": true,
                "3269": true,
                "4223": {
                    "f": [
                        3922
                    ]
                },
                "3065": {
                    "f": [
                        4671
                    ]
                },
                "3218": {
                    "f": [
                        4728
                    ]
                },
                "3924": {
                    "f": [
                        3505
                    ]
                },
                "4228": {
                    "f": [
                    ]
                },
                "2709": {
                    "f": [
                        4727
                    ]
                }
            },
            "4728": {
                "2854": "RelationshipControlOnForm",
                "3344": "console:relationshipControlOnForm",
                "3557": {
                    "f": [
                        3316,
                        4607,
                        4811
                    ]
                }
            },
            "3316": {
                "3344": "core:resourceViewer"
            },
            "4811": {
                "3344": "console:contextReceiver"
            },
            "4727": {
                "3344": "console:relationshipControlOnFormDetails",
                "2854": "RelationshipControlOnForm Details",
                "3604": null
            },
            "4930": {
                "3344": "core:inspectSecurityChecksOnResource",
                "2854": "Inspect security checks on resource",
                "2763": null,
                "3771": null,
                "3615": null,
                "3275": null,
                "3604": null,
                "2641": false,
                "3581": false,
                "3799": false,
                "3269": true,
                "4223": {
                    "f": [
                        3312
                    ]
                },
                "3065": {
                    "f": [
                        4671
                    ]
                },
                "3218": {
                    "f": [
                        5032
                    ]
                },
                "3924": {
                    "f": [
                        3505
                    ]
                },
                "4228": {
                    "f": [
                        4506
                    ]
                },
                "2709": {
                    "f": [
                        4716
                    ]
                }
            },
            "5032": {
                "2854": "Event Log Settings",
                "3344": "core:eventLogSettings",
                "3557": {
                    "f": [
                        3505
                    ]
                }
            },
            "4716": {
                "3344": "core:eventLogSettingsDetails",
                "2854": "Event Log Settings Details",
                "3604": null
            },
            "14348": {
                "3344": "core:resourceToApprove",
                "2854": "Item to Approve",
                "2763": null,
                "3771": null,
                "3615": null,
                "3275": null,
                "3604": "When set, contains the item that needs to be approved.",
                "2641": false,
                "3581": null,
                "3799": false,
                "3269": true,
                "4223": {
                    "f": [
                        3922
                    ]
                },
                "3065": {
                    "f": [
                        14123
                    ]
                },
                "3218": {
                    "f": [
                    ]
                },
                "3924": {
                    "f": [
                        3505
                    ]
                },
                "4228": {
                    "f": [
                    ]
                },
                "2709": {
                    "f": [
                    ]
                }
            },
            "2761": {
                "3344": "core:tenantLogEntryDetails",
                "2854": "Log Entry Details",
                "3604": null
            },
            "3193": {
                "3344": "core:surveyTarget",
                "2854": "Survey target",
                "2763": null,
                "3771": null,
                "3615": null,
                "3275": null,
                "3604": "The target of the survey.",
                "2641": false,
                "3581": false,
                "3799": false,
                "3269": false,
                "4223": {
                    "f": [
                        3922
                    ]
                },
                "3065": {
                    "f": [
                        3852
                    ]
                },
                "3218": {
                    "f": [
                        3153
                    ]
                },
                "3924": {
                    "f": [
                        3929
                    ]
                },
                "4228": {
                    "f": [
                        3738
                    ]
                },
                "2709": {
                    "f": [
                        3738
                    ]
                }
            },
            "3153": {
                "2854": "Survey Result",
                "3344": "core:surveyResponse",
                "3557": {
                    "f": [
                        3929
                    ]
                }
            },
            "3738": {
                "3344": "core:surveyFieldGroup",
                "2854": "Survey Fields",
                "3604": null
            },
            "12249": {
                "3344": null,
                "2854": "Special Fields",
                "3604": null
            },
            "16059": {
                "3344": "test:allFieldsEmployee",
                "2854": "AA_Employee",
                "2763": "AA_All Fields",
                "3771": "AA_Employee",
                "3615": null,
                "3275": null,
                "3604": "All Fields to Employee relationship",
                "2641": false,
                "3581": null,
                "3799": false,
                "3269": false,
                "4223": {
                    "f": [
                        3922
                    ]
                },
                "3065": {
                    "f": [
                        14123
                    ]
                },
                "3218": {
                    "f": [
                        18751
                    ]
                },
                "3924": {
                    "f": [
                        12149
                    ]
                },
                "4228": {
                    "f": [
                    ]
                },
                "2709": {
                    "f": [
                        12249
                    ]
                }
            },
            "12149": {
                "2854": "AA_Employee",
                "3344": "test:employee",
                "3557": {
                    "f": [
                        21120
                    ]
                }
            },
            "21120": {
                "3344": "test:person"
            },
            "16750": {
                "3344": "test:trucks",
                "2854": "AA_Trucks",
                "2763": "AA_All Fields",
                "3771": "AA_Truck",
                "3615": null,
                "3275": null,
                "3604": "All Fields to Truck relationship",
                "2641": false,
                "3581": null,
                "3799": false,
                "3269": false,
                "4223": {
                    "f": [
                        4221
                    ]
                },
                "3065": {
                    "f": [
                        14123
                    ]
                },
                "3218": {
                    "f": [
                        18751
                    ]
                },
                "3924": {
                    "f": [
                        22086
                    ]
                },
                "4228": {
                    "f": [
                    ]
                },
                "2709": {
                    "f": [
                        4506
                    ]
                }
            },
            "22086": {
                "2854": "AA_Truck",
                "3344": "test:truck",
                "3557": {
                    "f": [
                        3929
                    ]
                }
            },
            "17078": {
                "3344": "test:afImage",
                "2854": "New Image Field",
                "2763": "AA_All Fields",
                "3771": "New Image Field",
                "3615": null,
                "3275": null,
                "3604": null,
                "2641": false,
                "3581": null,
                "3799": false,
                "3269": true,
                "4223": {
                    "f": [
                        3922
                    ]
                },
                "3065": {
                    "f": [
                        14123
                    ]
                },
                "3218": {
                    "f": [
                        18751
                    ]
                },
                "3924": {
                    "f": [
                        2943
                    ]
                },
                "4228": {
                    "f": [
                    ]
                },
                "2709": {
                    "f": [
                        19534
                    ]
                }
            },
            "2943": {
                "2854": "Photo",
                "3344": "core:photoFileType",
                "3557": {
                    "f": [
                        3271
                    ]
                }
            },
            "3271": {
                "3344": "core:imageFileType"
            },
            "19534": {
                "3344": null,
                "2854": "Image Field",
                "3604": null
            },
            "20367": {
                "3344": "test:afWeekday",
                "2854": "Weekday",
                "2763": "AA_All Fields",
                "3771": "Weekday",
                "3615": null,
                "3275": null,
                "3604": "All Fields to Weekday relationship",
                "2641": false,
                "3581": null,
                "3799": false,
                "3269": true,
                "4223": {
                    "f": [
                        3922
                    ]
                },
                "3065": {
                    "f": [
                        14123
                    ]
                },
                "3218": {
                    "f": [
                        18751
                    ]
                },
                "3924": {
                    "f": [
                        14286
                    ]
                },
                "4228": {
                    "f": [
                    ]
                },
                "2709": {
                    "f": [
                        12249
                    ]
                }
            },
            "14286": {
                "2854": "AA_Weekday",
                "3344": "test:weekdayEnum",
                "3557": {
                    "f": [
                        4411
                    ]
                }
            },
            "21368": {
                "3344": "test:afCondiments",
                "2854": "Condiments",
                "2763": "AA_All Fields",
                "3771": "Condiments",
                "3615": null,
                "3275": null,
                "3604": "All Fields to Condiments relationship",
                "2641": false,
                "3581": null,
                "3799": false,
                "3269": true,
                "4223": {
                    "f": [
                        4221
                    ]
                },
                "3065": {
                    "f": [
                        14123
                    ]
                },
                "3218": {
                    "f": [
                        18751
                    ]
                },
                "3924": {
                    "f": [
                        17518
                    ]
                },
                "4228": {
                    "f": [
                    ]
                },
                "2709": {
                    "f": [
                        12249
                    ]
                }
            },
            "17518": {
                "2854": "AA_Condiments",
                "3344": "test:condimentsEnum",
                "3557": {
                    "f": [
                        4411
                    ]
                }
            },
            "22332": {
                "3344": "test:herbs",
                "2854": "AA_Herbs",
                "2763": "AA_All Fields",
                "3771": "AA_Herb",
                "3615": null,
                "3275": null,
                "3604": "All Fields to Herb relationship",
                "2641": false,
                "3581": null,
                "3799": false,
                "3269": false,
                "4223": {
                    "f": [
                        4221
                    ]
                },
                "3065": {
                    "f": [
                        14123
                    ]
                },
                "3218": {
                    "f": [
                        18751
                    ]
                },
                "3924": {
                    "f": [
                        15610
                    ]
                },
                "4228": {
                    "f": [
                    ]
                },
                "2709": {
                    "f": [
                        4506
                    ]
                }
            },
            "15610": {
                "2854": "AA_Herb",
                "3344": "test:herb",
                "3557": {
                    "f": [
                        3929
                    ]
                }
            },
            "13642": {
                "3344": null,
                "2854": "AA_All Fields",
                "2763": "AA_Snacks (Rev)",
                "3771": "AA_All Fields",
                "3615": null,
                "3275": null,
                "3604": null,
                "2641": false,
                "3581": null,
                "3799": false,
                "3269": false,
                "4223": {
                    "f": [
                        4221
                    ]
                },
                "3065": {
                    "f": [
                        14123
                    ]
                },
                "3218": {
                    "f": [
                        18333
                    ]
                },
                "3924": {
                    "f": [
                        18751
                    ]
                },
                "4228": {
                    "f": [
                    ]
                },
                "2709": {
                    "f": [
                        4506
                    ]
                }
            },
            "18333": {
                "2854": "AA_Snacks",
                "3344": "test:snacks",
                "3557": {
                    "f": [
                        3929
                    ]
                }
            },
            "15797": {
                "3344": null,
                "2854": "Lookup",
                "2763": "EDT_FRM_MandatoryOnForm",
                "3771": "Lookup",
                "3615": null,
                "3275": null,
                "3604": null,
                "2641": false,
                "3581": null,
                "3799": false,
                "3269": true,
                "4223": {
                    "f": [
                        3922
                    ]
                },
                "3065": {
                    "f": [
                        14123
                    ]
                },
                "3218": {
                    "f": [
                        16409
                    ]
                },
                "3924": {
                    "f": [
                        18751
                    ]
                },
                "4228": {
                    "f": [
                    ]
                },
                "2709": {
                    "f": [
                        19944
                    ]
                }
            },
            "16409": {
                "2854": "EDT_FRM_MandatoryOnForm",
                "3344": "test:edtFrmMandatoryOnForm",
                "3557": {
                    "f": [
                        3929
                    ]
                }
            },
            "19944": {
                "3344": null,
                "2854": "Special",
                "3604": null
            },
            "15913": {
                "3344": null,
                "2854": "AA_Calculations - Norm Lookup",
                "2763": "AA_Calculations",
                "3771": "Norm Lookup",
                "3615": null,
                "3275": null,
                "3604": null,
                "2641": null,
                "3581": null,
                "3799": false,
                "3269": false,
                "4223": {
                    "f": [
                        3922
                    ]
                },
                "3065": {
                    "f": [
                        14123
                    ]
                },
                "3218": {
                    "f": [
                        13420
                    ]
                },
                "3924": {
                    "f": [
                        18751
                    ]
                },
                "4228": {
                    "f": [
                    ]
                },
                "2709": {
                    "f": [
                        4506
                    ]
                }
            },
            "13420": {
                "2854": "AA_Calculations",
                "3344": null,
                "3557": {
                    "f": [
                        3929
                    ]
                }
            },
            "18761": {
                "3344": null,
                "2854": "AA-All Fields",
                "2763": "Scientist (Rev)",
                "3771": "AA-All Fields",
                "3615": null,
                "3275": null,
                "3604": null,
                "2641": false,
                "3581": null,
                "3799": false,
                "3269": false,
                "4223": {
                    "f": [
                        3553
                    ]
                },
                "3065": {
                    "f": [
                        14123
                    ]
                },
                "3218": {
                    "f": [
                        12076
                    ]
                },
                "3924": {
                    "f": [
                        18751
                    ]
                },
                "4228": {
                    "f": [
                    ]
                },
                "2709": {
                    "f": [
                        19142
                    ]
                }
            },
            "12076": {
                "2854": "Scientist",
                "3344": null,
                "3557": {
                    "f": [
                        3929
                    ]
                }
            },
            "19142": {
                "3344": null,
                "2854": "Details",
                "3604": null
            },
            "18882": {
                "3344": null,
                "2854": "AA_All Fields",
                "2763": "AA_DogBreeds (Rev)",
                "3771": "AA_All Fields",
                "3615": null,
                "3275": null,
                "3604": null,
                "2641": false,
                "3581": null,
                "3799": false,
                "3269": false,
                "4223": {
                    "f": [
                        3922
                    ]
                },
                "3065": {
                    "f": [
                        14123
                    ]
                },
                "3218": {
                    "f": [
                        19098
                    ]
                },
                "3924": {
                    "f": [
                        18751
                    ]
                },
                "4228": {
                    "f": [
                    ]
                },
                "2709": {
                    "f": [
                        12270
                    ]
                }
            },
            "19098": {
                "2854": "AA_DogBreeds",
                "3344": null,
                "3557": {
                    "f": [
                        3929
                    ]
                }
            },
            "12270": {
                "3344": null,
                "2854": "New Field Group",
                "3604": null
            },
            "19413": {
                "3344": null,
                "2854": "AA_All  Fields",
                "2763": "AA_ChocBars (Rev)",
                "3771": "AA_All  Fields",
                "3615": null,
                "3275": null,
                "3604": null,
                "2641": false,
                "3581": null,
                "3799": false,
                "3269": false,
                "4223": {
                    "f": [
                        3312
                    ]
                },
                "3065": {
                    "f": [
                        14123
                    ]
                },
                "3218": {
                    "f": [
                        21736
                    ]
                },
                "3924": {
                    "f": [
                        18751
                    ]
                },
                "4228": {
                    "f": [
                    ]
                },
                "2709": {
                    "f": [
                        4506
                    ]
                }
            },
            "21736": {
                "2854": "AA_ChocBars",
                "3344": null,
                "3557": {
                    "f": [
                        3929
                    ]
                }
            }
        },
        "members": {
            "3344": {
                "dt": "String",
                "alias": "core:alias"
            },
            "2854": {
                "dt": "String",
                "alias": "core:name"
            },
            "3604": {
                "dt": "String",
                "alias": "core:description"
            },
            "3176": {
                "rrel": {
                    "isLookup": false,
                    "alias": "core:fields"
                }
            },
            "3514": {
                "dt": "String",
                "alias": "core:fieldScriptName"
            },
            "4390": {
                "dt": "Bool",
                "alias": "core:isCalculatedField"
            },
            "2685": {
                "dt": "Bool",
                "alias": "core:hideField"
            },
            "3783": {
                "dt": "Bool",
                "alias": "core:isRequired"
            },
            "2915": {
                "dt": "Bool",
                "alias": "core:isFieldReadOnly"
            },
            "4337": {
                "frel": {
                    "isLookup": false,
                    "alias": "core:isOfType"
                }
            },
            "4263": {
                "frel": {
                    "isLookup": true,
                    "alias": "core:fieldInGroup"
                }
            },
            "3194": {
                "rrel": {
                    "isLookup": false,
                    "alias": "core:fieldOverridesForType"
                }
            },
            "3218": {
                "frel": {
                    "isLookup": true,
                    "alias": "core:fromType"
                },
                "rrel": {
                    "isLookup": false,
                    "alias": "core:relationships"
                }
            },
            "2763": {
                "dt": "String",
                "alias": "core:fromName"
            },
            "3771": {
                "dt": "String",
                "alias": "core:toName"
            },
            "3615": {
                "dt": "String",
                "alias": "core:fromScriptName"
            },
            "3275": {
                "dt": "String",
                "alias": "core:toScriptName"
            },
            "2641": {
                "dt": "Bool",
                "alias": "core:relationshipIsMandatory"
            },
            "3581": {
                "dt": "Bool",
                "alias": "core:revRelationshipIsMandatory"
            },
            "3799": {
                "dt": "Bool",
                "alias": "core:hideOnFromType"
            },
            "3269": {
                "dt": "Bool",
                "alias": "core:hideOnToType"
            },
            "4223": {
                "frel": {
                    "isLookup": true,
                    "alias": "core:cardinality"
                }
            },
            "3065": {
                "frel": {
                    "isLookup": false,
                    "alias": "core:inSolution"
                }
            },
            "3924": {
                "frel": {
                    "isLookup": true,
                    "alias": "core:toType"
                },
                "rrel": {
                    "isLookup": false,
                    "alias": "core:reverseRelationships"
                }
            },
            "3557": {
                "frel": {
                    "isLookup": false,
                    "alias": "core:inherits"
                }
            },
            "4228": {
                "frel": {
                    "isLookup": true,
                    "alias": "core:relationshipInToTypeGroup"
                }
            },
            "2709": {
                "frel": {
                    "isLookup": true,
                    "alias": "core:relationshipInFromTypeGroup"
                }
            },
            "3172": {
                "rrel": {
                    "isLookup": false,
                    "alias": "core:fieldGroups"
                }
            },
            "4110": {
                "rrel": {
                    "isLookup": false,
                    "alias": "core:resourceKeys"
                }
            }
        }
    };

    spreadsheetInfoTestData.getAllFieldTypeEntity = function getAllFieldTypeEntity() {
        return spEntity.entityDataVer2ToEntities(spreadsheetInfoTestData.allFieldType)[0];
    };

})(spreadsheetInfoTestData || (spreadsheetInfoTestData = {}));