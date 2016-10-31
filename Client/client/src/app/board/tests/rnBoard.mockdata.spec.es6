/*global _, spEntity, jsonString, jsonLookup, jsonBool */

var rnBoardTestData;
(function (rnBoardTestData) {
    "use strict";

    rnBoardTestData.boardEntity = spEntity.fromJSON({
        "id": 39515,
        "name": "Story Board",
        "boardCardTemplateName": "story - brief",
        "boardCardCustomTemplate": "<div>hello</div>",
        "boardSwimlaneDimension": {
            "id": 28227,
            "boardDimensionShowUndefined": false,
            "boardDimensionValue": [{"id": 29859, "name": "Ship It"}, {"id": 50092, "name": "Platform"}, {
                "id": 59728,
                "name": "GRC"
            }, {"id": 62307, "name": "Operations"}]
        },
        "boardChildReport": {
            "id": 72756,
            "name": "Tasks on Story Board Report",
            "rootNode": {
                "id": 27426,
                "resourceReportNodeType": {
                    "id": 35600,
                    "name": "Task Item",
                    "relationships": [{
                        "id": 56088,
                        "toTypeDefaultValue": {"id": 58121}
                    }, {"id": 63406}, {"id": 74404}, {"id": 85020, "toTypeDefaultValue": {"id": 81767}}],
                    "console:defaultEditForm": {"id": 82868}
                }
            }
        },
        "boardReport": {
            "id": 50835,
            "name": "Story Board Report",
            "rootNode": {
                "id": 27378,
                "resourceReportNodeType": {
                    "id": 33454,
                    "name": "Backlog Item",
                    "relationships": [{"id": 26899}, {"id": 29016}, {"id": 34352}, {"id": 35299}, {"id": 45634}, {
                        "id": 50789,
                        "toTypeDefaultValue": {
                            "id": 58121,
                            "_warnings": "already visited id:58121 \"null\" name: \"null\""
                        }
                    }, {"id": 56972}, {
                        "id": 64457,
                        "toTypeDefaultValue": {"id": 30093, "name": "Customer experience"}
                    }, {
                        "id": 75896,
                        "toTypeDefaultValue": {
                            "id": 81767,
                            "_warnings": "already visited id:81767 \"null\" name: \"null\""
                        }
                    }, {"id": 80308, "toTypeDefaultValue": {"id": 73617}}],
                    "console:defaultEditForm": {"id": 33981}
                }
            }
        },
        "boardStyleDimension": {
            "id": 54319,
            "boardDimensionShowUndefined": false,
            "boardDimensionReportColumn": {"id": 68183, "name": "Category"},
            "boardDimensionValue": [{
                "id": 30093,
                "_warnings": "already visited id:30093 \"null\" name: \"Customer experience\""
            }, {"id": 30734, "name": "Operations"}, {"id": 67058, "name": "Technical"}]
        },
        "boardColumnDimension": {
            "id": 75704,
            "boardDimensionShowUndefined": false,
            "boardDimensionReportColumn": {"id": 41982, "name": "Kanban State"},
            "boardDimensionValue": [{"id": 31916, "name": "Test & fix"}, {
                "id": 35175,
                "name": "Development done"
            }, {"id": 37939, "name": "Analysis"}, {"id": 43891, "name": "Shipped"}, {
                "id": 49711,
                "name": "Next"
            }, {"id": 58280, "name": "Test & fix done"}, {"id": 60621, "name": "Acceptance done"}, {
                "id": 66887,
                "name": "Development"
            }, {"id": 81339, "name": "Acceptance"}, {"id": 82748, "name": "Analysis done"}]
        },
        "drilldownTargetBoard": {"id": 48182, "name": "Task Board"}
    });

    rnBoardTestData.testEntity = spEntity.fromJSON({
        id: 1000,
        type: 'person',
        name: 'test person',
        otherHalf: {
            id: 1100, type: 'person', name: 'better person'
        }
    });

    rnBoardTestData.kanbanStateEntity = spEntity.fromJSON({
        "id": 76997,
        "defaultDisplayReport": {"id": 81579}
    });

    rnBoardTestData.teamMemberEntity = spEntity.fromJSON({
        "id": 43300,
        "defaultDisplayReport": {"id": 38726}
    });

    rnBoardTestData.projectEntity = spEntity.fromJSON({
        "id": 78100,
        "defaultDisplayReport": {"id": 29643}
    });

    rnBoardTestData.accountEntity = spEntity.fromJSON({
        "id": 64298
    });

    rnBoardTestData.kanbanStateColumn = spEntity.fromJSON({
        "id": 41982,
        "name": "Kanban State",
        "columnExpression": {
            "id": 38303,
            "sourceNode": {
                "id": 34979,
                "resourceReportNodeType": {
                    "id": 76997,
                    "name": "Kanban State",
                    "alias": null,
                    "isOfType": [{"id": "core:definition", "alias": "core:definition"}]
                },
                "followRelationship": {
                    "id": 35299,
                    "name": "Kanban State",
                    "alias": null,
                    "isOfType": [{"id": "core:relationship", "alias": "core:relationship"}]
                }
            }
        }
    });

    rnBoardTestData.itemTypeColumn = spEntity.fromJSON({
        "id": 63282,
        "name": "Type",
        "columnExpression": {
            "id": 42034,
            "sourceNode": {
                "id": 52349,
                "resourceReportNodeType": {
                    "id": 66035,
                    "name": "Type",
                    "alias": null,
                    "isOfType": [{"id": "core:enumType", "alias": "core:enumType"}]
                },
                "followRelationship": {
                    "id": 80308,
                    "name": "Type",
                    "alias": null,
                    "isOfType": [{"id": "core:relationship", "alias": "core:relationship"}]
                }
            }
        }
    });

    rnBoardTestData.priorityColumn = spEntity.fromJSON({
        "id": 68864,
        "name": "Priority",
        "columnExpression": {
            "id": 62628,
            "sourceNode": {
                "id": 41994,
                "resourceReportNodeType": {
                    "id": 79567,
                    "name": "Priority",
                    "alias": null,
                    "isOfType": [{"id": "core:enumType", "alias": "core:enumType"}]
                },
                "followRelationship": {
                    "id": 50789,
                    "name": "Priority",
                    "alias": null,
                    "isOfType": [{"id": "core:relationship", "alias": "core:relationship"}]
                }
            }
        }
    });

    rnBoardTestData.categoryColumn = spEntity.fromJSON({
        "id": 68183,
        "name": "Category",
        "columnExpression": {
            "id": 42566,
            "sourceNode": {
                "id": 30290,
                "resourceReportNodeType": {
                    "id": 79866,
                    "name": "Category",
                    "alias": null,
                    "isOfType": [{"id": "core:enumType", "alias": "core:enumType"}]
                },
                "followRelationship": {
                    "id": 64457,
                    "name": "Category",
                    "alias": null,
                    "isOfType": [{"id": "core:relationship", "alias": "core:relationship"}]
                }
            }
        }
    });

    rnBoardTestData.createdByColumn = spEntity.fromJSON({
        "id": 72516,
        "name": "Created by",
        "columnExpression": {
            "id": 58814,
            "sourceNode": {
                "id": 52278,
                "resourceReportNodeType": {
                    "id": "core:userAccount",
                    "name": "User Account",
                    "alias": "core:userAccount",
                    "isOfType": [{"id": "core:managedType", "alias": "core:managedType"}]
                },
                "followRelationship": {
                    "id": "core:createdBy",
                    "name": "Created by",
                    "alias": "core:createdBy",
                    "isOfType": [{"id": "core:relationship", "alias": "core:relationship"}]
                }
            }
        }
    });

    rnBoardTestData.lastModifiedByColumn = spEntity.fromJSON({
        "id": 57186,
        "name": "Last modified by",
        "columnExpression": {
            "id": 76987,
            "sourceNode": {
                "id": 27332,
                "resourceReportNodeType": {
                    "id": "core:userAccount",
                    "name": "User Account",
                    "alias": "core:userAccount",
                    "isOfType": [{"id": "core:managedType", "alias": "core:managedType"}]
                },
                "followRelationship": {
                    "id": "core:lastModifiedBy",
                    "name": "Modified by",
                    "alias": "core:lastModifiedBy",
                    "isOfType": [{"id": "core:relationship", "alias": "core:relationship"}]
                }
            }
        }
    });

    rnBoardTestData.ownerColumn = spEntity.fromJSON({
        "id": 83640,
        "name": "Owner",
        "columnExpression": {
            "id": 51737,
            "sourceNode": {
                "id": 79659,
                "resourceReportNodeType": {
                    "id": 43300,
                    "name": "Team Member",
                    "alias": null,
                    "isOfType": [{"id": "core:definition", "alias": "core:definition"}]
                },
                "followRelationship": {
                    "id": 34352,
                    "name": "Backlog Item - Owner",
                    "alias": null,
                    "isOfType": [{"id": "core:relationship", "alias": "core:relationship"}]
                }
            }
        }
    });

    rnBoardTestData.projectColumn = spEntity.fromJSON({
        "id": 69499,
        "name": "Project",
        "columnExpression": {
            "id": 67301,
            "sourceNode": {
                "id": 53808,
                "resourceReportNodeType": {
                    "id": 78100,
                    "name": "Project",
                    "alias": null,
                    "isOfType": [{"id": "core:definition", "alias": "core:definition"}]
                },
                "followRelationship": {
                    "id": 57437,
                    "name": "Work Item - Project",
                    "alias": null,
                    "isOfType": [{"id": "core:relationship", "alias": "core:relationship"}]
                }
            }
        }
    });

    rnBoardTestData.storiesResults = {
        "meta": {
            "modified": "2016-05-02T20:21:23.807Z",
            "hideheader": false,
            "hideact": false,
            "dfid": 33981,
            "typefmtstyle": {
                "Int32": ["Highlight", "Icon", "ProgressBar"],
                "String": ["Highlight", "Icon"],
                "InlineRelationship": ["Highlight", "Icon"],
                "ChoiceRelationship": ["Highlight", "Icon"],
                "Decimal": ["Highlight", "Icon", "ProgressBar"]
            },
            "rcols": {
                "59519": {"fid": 85868, "ord": 0, "oprtype": "Int32", "type": "Int32", "title": "ID"},
                "70760": {"fid": 76593, "ord": 1, "oprtype": "String", "type": "String", "title": "Summary"},
                "41982": {
                    "maxlen": 200,
                    "tid": 76997,
                    "fid": 33222,
                    "rid": 35299,
                    "ord": 2,
                    "card": "ManyToOne",
                    "regexerr": "The value must not contain angled brackets.",
                    "regex": "^[^<>]+$",
                    "oprtype": "InlineRelationship",
                    "type": "InlineRelationship",
                    "title": "Kanban State"
                },
                "68864": {
                    "maxlen": 200,
                    "tid": 79567,
                    "fid": 33222,
                    "rid": 50789,
                    "ord": 3,
                    "regexerr": "The value must not contain angled brackets.",
                    "regex": "^[^<>]+$",
                    "oprtype": "ChoiceRelationship",
                    "type": "ChoiceRelationship",
                    "title": "Priority"
                },
                "63282": {
                    "maxlen": 200,
                    "tid": 66035,
                    "fid": 33222,
                    "rid": 80308,
                    "ord": 4,
                    "regexerr": "The value must not contain angled brackets.",
                    "regex": "^[^<>]+$",
                    "oprtype": "ChoiceRelationship",
                    "type": "ChoiceRelationship",
                    "title": "Type"
                },
                "50138": {
                    "mindec": 2,
                    "fid": 74091,
                    "ord": 5,
                    "places": 9,
                    "defval": "0",
                    "oprtype": "Decimal",
                    "type": "Decimal",
                    "title": "Rank"
                },
                "53593": {
                    "mindec": 2,
                    "ro": true,
                    "fid": 74367,
                    "ord": 6,
                    "places": 3,
                    "oprtype": "Decimal",
                    "type": "Decimal",
                    "title": "Days old"
                },
                "69138": {"ord": 7, "oprtype": "String", "type": "String", "title": "State name"},
                "68183": {
                    "maxlen": 200,
                    "tid": 79866,
                    "fid": 33222,
                    "rid": 64457,
                    "ord": 8,
                    "regexerr": "The value must not contain angled brackets.",
                    "regex": "^[^<>]+$",
                    "oprtype": "ChoiceRelationship",
                    "type": "ChoiceRelationship",
                    "title": "Category"
                },
                "51750": {"ord": 9, "oprtype": "String", "type": "String", "title": "Tasks"},
                "83640": {
                    "maxlen": 200,
                    "tid": 43300,
                    "fid": 33222,
                    "rid": 34352,
                    "ord": 10,
                    "card": "ManyToOne",
                    "regexerr": "The value must not contain angled brackets.",
                    "regex": "^[^<>]+$",
                    "oprtype": "UserInlineRelationship",
                    "type": "InlineRelationship",
                    "title": "Owner"
                },
                "43041": {"ord": 11, "oprtype": "Decimal", "type": "Decimal", "title": "Days in state"},
                "48901": {
                    "mline": true,
                    "maxlen": 10000,
                    "fid": 57784,
                    "ord": 12,
                    "oprtype": "String",
                    "type": "String",
                    "title": "Description"
                },
                "69499": {
                    "maxlen": 200,
                    "tid": 78100,
                    "fid": 33222,
                    "rid": 57437,
                    "ord": 13,
                    "card": "ManyToOne",
                    "regexerr": "The value must not contain angled brackets.",
                    "regex": "^[^<>]+$",
                    "oprtype": "InlineRelationship",
                    "type": "InlineRelationship",
                    "title": "Project"
                }
            },
            "sort": [{"order": "Ascending", "colid": "50138"}],
            "choice": {
                "79567": [{"id": 45648, "name": "ASAP"}, {"id": 61160, "name": "High"}, {
                    "id": 58121,
                    "name": "Normal"
                }],
                "66035": [{"id": 73617, "name": "Story"}, {"id": 73360, "name": "Feature"}, {
                    "id": 46981,
                    "name": "Epic"
                }],
                "79866": [{"id": 30093, "name": "Customer experience"}, {"id": 67058, "name": "Technical"}, {
                    "id": 30734,
                    "name": "Operations"
                }]
            },
            "inline": {"76997": 81579, "43300": 38726, "78100": 29643},
            "anlcols": {
                "32305": {
                    "rcolid": 51750,
                    "ord": 15,
                    "doper": "Contains",
                    "oper": "Unspecified",
                    "anltype": "String",
                    "type": "String",
                    "title": "Task Count"
                },
                "37150": {
                    "tid": 66035,
                    "ord": 2,
                    "doper": "AnyOf",
                    "oper": "Unspecified",
                    "anltype": "ChoiceRelationship",
                    "type": "ChoiceRelationship",
                    "title": "Type"
                },
                "39376": {
                    "ord": 19,
                    "doper": "Unspecified",
                    "oper": "IsFalse",
                    "anltype": "Bool",
                    "type": "Bool",
                    "title": "Removed"
                },
                "39716": {
                    "tid": 79866,
                    "ord": 13,
                    "doper": "AnyOf",
                    "oper": "Unspecified",
                    "anltype": "ChoiceRelationship",
                    "type": "ChoiceRelationship",
                    "title": "Category"
                },
                "43103": {
                    "ord": 14,
                    "doper": "GreaterThan",
                    "oper": "Unspecified",
                    "anltype": "Int32",
                    "type": "Int32",
                    "title": "ID"
                },
                "50553": {
                    "tid": 43300,
                    "ord": 16,
                    "doper": "AnyOf",
                    "oper": "Unspecified",
                    "anltype": "UserInlineRelationship",
                    "type": "InlineRelationship",
                    "title": "Owner"
                },
                "54266": {
                    "tid": 76997,
                    "ord": 0,
                    "doper": "AnyOf",
                    "oper": "IsNotNull",
                    "anltype": "InlineRelationship",
                    "type": "InlineRelationship",
                    "title": "Kanban State"
                },
                "58649": {
                    "ord": 12,
                    "doper": "Contains",
                    "oper": "Unspecified",
                    "anltype": "String",
                    "type": "String",
                    "title": "Summary"
                },
                "60972": {
                    "tid": 79567,
                    "ord": 1,
                    "doper": "AnyOf",
                    "oper": "Unspecified",
                    "anltype": "ChoiceRelationship",
                    "type": "ChoiceRelationship",
                    "title": "Priority"
                },
                "62736": {
                    "rcolid": 43041,
                    "ord": 17,
                    "doper": "GreaterThan",
                    "oper": "Unspecified",
                    "anltype": "Decimal",
                    "type": "Decimal",
                    "title": "Days in state"
                },
                "64534": {
                    "ord": 9,
                    "doper": "GreaterThan",
                    "oper": "Unspecified",
                    "anltype": "Decimal",
                    "type": "Decimal",
                    "title": "Days old"
                },
                "69533": {
                    "ord": 18,
                    "doper": "Contains",
                    "oper": "Unspecified",
                    "anltype": "String",
                    "type": "String",
                    "title": "Description"
                },
                "72468": {
                    "tid": 78100,
                    "ord": 20,
                    "doper": "AnyOf",
                    "oper": "Unspecified",
                    "anltype": "InlineRelationship",
                    "type": "InlineRelationship",
                    "title": "Project"
                },
                "74742": {
                    "ord": 8,
                    "doper": "GreaterThan",
                    "oper": "Unspecified",
                    "anltype": "Decimal",
                    "type": "Decimal",
                    "title": "Rank"
                },
                "80282": {
                    "rcolid": 69138,
                    "ord": 10,
                    "doper": "Contains",
                    "oper": "Unspecified",
                    "anltype": "String",
                    "type": "String",
                    "title": "State name"
                }
            },
            "valrules": {"50138": {"places": 9}, "53593": {"places": 3}, "43041": {}},
            "rmeta": {"rollup": true, "showoplbl": true},
            "invalid": {"nodes": null, "columns": null, "conditions": null},
            "style": "Default",
            "title": "Story Board Report"
        },
        "gdata": [{
            "eid": 27305,
            "values": [{"val": "635"}, {"val": "New Admin Roles"}, {"vals": {"84277": "P1"}}, {"vals": {"58121": "Normal"}}, {"vals": {"73617": "Story"}}, {"val": ""}, {"val": "26.250000"}, {"val": "-1-P1"}, {"vals": {"30093": "Customer experience"}}, {"val": "0/0"}, {}, {"val": "22.291666"}, {"val": "[see wiki](http://spwiki.sp.local/display/DEV/Application+Admin+Roles?focusedCommentId=190119971#comment-190119971)"}, {"vals": {"50092": "Platform"}}]
        }, {
            "eid": 27531,
            "values": [{"val": "690"}, {"val": "Ability to move client customised Apps from sandbox to production"}, {"vals": {"84277": "P1"}}, {"vals": {"58121": "Normal"}}, {"vals": {"73617": "Story"}}, {"val": ""}, {"val": "18.250000"}, {"val": "-1-P1"}, {"vals": {"30093": "Customer experience"}}, {"val": "0/0"}, {}, {"val": "12.291666"}, {"val": ""}, {"vals": {"50092": "Platform"}}]
        }, {
            "eid": 29375,
            "values": [{"val": "638"}, {"val": "Ability for Task Delegation"}, {"vals": {"64636": "P3"}}, {"vals": {"58121": "Normal"}}, {"vals": {"73617": "Story"}}, {"val": ""}, {"val": "26.250000"}, {"val": "-3-P3"}, {"vals": {"30093": "Customer experience"}}, {"val": "0/0"}, {}, {"val": "26.166666"}, {"val": ""}, {"vals": {"50092": "Platform"}}]
        }, {
            "eid": 29548,
            "values": [{"val": "689"}, {"val": "Conditionally hide fields on forms"}, {"vals": {"64636": "P3"}}, {"vals": {"58121": "Normal"}}, {"vals": {"73617": "Story"}}, {"val": ""}, {"val": "18.250000"}, {"val": "-3-P3"}, {"vals": {"30093": "Customer experience"}}, {"val": "0/0"}, {}, {"val": "18.125000"}, {"val": ""}, {"vals": {"50092": "Platform"}}]
        }, {
            "eid": 29950,
            "values": [{"val": "688"}, {"val": "Resource Governor - per tenant"}, {"vals": {"64636": "P3"}}, {"vals": {"58121": "Normal"}}, {"vals": {"73617": "Story"}}, {"val": ""}, {"val": "18.250000"}, {"val": "-3-P3"}, {"vals": {"30093": "Customer experience"}}, {"val": "0/0"}, {}, {"val": "18.125000"}, {"val": ""}, {"vals": {"50092": "Platform"}}]
        }, {
            "eid": 30455,
            "values": [{"val": "494"}, {"val": "Structure Views"}, {"vals": {"31916": "Test & fix"}}, {"vals": {"58121": "Normal"}}, {"vals": {"73617": "Story"}}, {"val": ""}, {"val": "46.250000"}, {"val": "5-Test & fix"}, {"vals": {"30093": "Customer experience"}}, {"val": "4/6"}, {"vals": {"83192": "Darren Jacobs"}}, {"val": "36.208333"}, {"val": ""}, {"vals": {"50092": "Platform"}}]
        }, {
            "eid": 31595,
            "values": [{"val": "700"}, {"val": "Data transformation on import"}, {"vals": {"64636": "P3"}}, {"vals": {"58121": "Normal"}}, {"vals": {"73617": "Story"}}, {"val": ""}, {"val": "13.250000"}, {"val": "-3-P3"}, {"vals": {"30093": "Customer experience"}}, {"val": "0/0"}, {}, {"val": "13.208333"}, {"val": ""}, {"vals": {"50092": "Platform"}}]
        }, {
            "eid": 33189,
            "values": [{"val": "649"}, {"val": "Mobile: Ability to show/hide fields or containers on form on mobile"}, {"vals": {"64636": "P3"}}, {"vals": {"58121": "Normal"}}, {"vals": {"73617": "Story"}}, {"val": ""}, {"val": "22.250000"}, {"val": "-3-P3"}, {"vals": {"30093": "Customer experience"}}, {"val": "0/0"}, {}, {"val": "22.375000"}, {"val": ""}, {"vals": {"50092": "Platform"}}]
        }, {
            "eid": 36833,
            "values": [{"val": "726"}, {"val": "Custom quick links on home page"}, {"vals": {"64636": "P3"}}, {"vals": {"58121": "Normal"}}, {"vals": {"73617": "Story"}}, {"val": ""}, {"val": "7.250000"}, {"val": "-3-P3"}, {"vals": {"30093": "Customer experience"}}, {"val": "0/0"}, {}, {"val": "7.375000"}, {"val": ""}, {"vals": {"50092": "Platform"}}]
        }, {
            "eid": 40723,
            "values": [{"val": "687"}, {"val": "Reporting for concurrent users"}, {"vals": {"75156": "P2"}}, {"vals": {"58121": "Normal"}}, {"vals": {"73617": "Story"}}, {"val": ""}, {"val": "18.250000"}, {"val": "-2-P2"}, {"vals": {"30093": "Customer experience"}}, {"val": "0/0"}, {}, {"val": "18.125000"}, {"val": ""}, {"vals": {"50092": "Platform"}}]
        }, {
            "eid": 41234,
            "values": [{"val": "724"}, {"val": "Email Listner"}, {"vals": {"64636": "P3"}}, {"vals": {"58121": "Normal"}}, {"vals": {"73617": "Story"}}, {"val": ""}, {"val": "7.250000"}, {"val": "-3-P3"}, {"vals": {"30093": "Customer experience"}}, {"val": "0/0"}, {}, {"val": "7.375000"}, {"val": ""}, {"vals": {"50092": "Platform"}}]
        }, {
            "eid": 47220,
            "values": [{"val": "591"}, {"val": "Implement SQL failover"}, {"vals": {"49711": "Next"}}, {"vals": {"58121": "Normal"}}, {"vals": {"73617": "Story"}}, {"val": ""}, {"val": "34.250000"}, {"val": "0-Next"}, {"vals": {"30734": "Operations"}}, {"val": "0/2"}, {"vals": {"74150": "Stephen Gibbon"}}, {"val": "13.166666"}, {"val": ""}, {"vals": {"62307": "Operations"}}]
        }, {
            "eid": 47535,
            "values": [{"val": "717"}, {"val": "Scalability to 8000 users"}, {"vals": {"49711": "Next"}}, {"vals": {"58121": "Normal"}}, {"vals": {"73617": "Story"}}, {"val": ""}, {"val": "12.250000"}, {"val": "0-Next"}, {"vals": {"30093": "Customer experience"}}, {"val": "0/0"}, {"vals": {"84630": "Brad Stevens"}}, {"val": "12.333333"}, {"val": ""}, {"vals": {"62307": "Operations"}}]
        }, {
            "eid": 49463,
            "values": [{"val": "735"}, {"val": "Provide instructions for having syslog server in dev and test envs"}, {"vals": {"84277": "P1"}}, {"vals": {"58121": "Normal"}}, {"vals": {"73617": "Story"}}, {"val": ""}, {"val": "6.250000"}, {"val": "-1-P1"}, {"vals": {"30093": "Customer experience"}}, {"val": "0/0"}, {"vals": {"74150": "Stephen Gibbon"}}, {"val": "5.791666"}, {"val": "Dev and test want to have a test syslog server AND be able to run private dev syslog servers. Using same tech as in prod."}, {"vals": {"62307": "Operations"}}]
        }, {
            "eid": 50697,
            "values": [{"val": "701"}, {"val": "BIG button solution"}, {"vals": {"64636": "P3"}}, {"vals": {"58121": "Normal"}}, {"vals": {"73617": "Story"}}, {"val": ""}, {"val": "13.250000"}, {"val": "-3-P3"}, {"vals": {"30093": "Customer experience"}}, {"val": "0/0"}, {}, {"val": "13.166666"}, {"val": ""}, {"vals": {"50092": "Platform"}}]
        }, {
            "eid": 54498,
            "values": [{"val": "625"}, {"val": "update lodash lb in the client"}, {"vals": {"43891": "Shipped"}}, {"vals": {"58121": "Normal"}}, {"vals": {"73617": "Story"}}, {"val": ""}, {"val": "28.250000"}, {"val": "9-Shipped"}, {"vals": {"67058": "Technical"}}, {"val": "1/2"}, {"vals": {"74150": "Stephen Gibbon"}}, {"val": "6.250000"}, {"val": ""}, {"vals": {"50092": "Platform"}}]
        }, {
            "eid": 55478,
            "values": [{"val": "661"}, {"val": "Separation of Tenant Performance"}, {"vals": {"75156": "P2"}}, {"vals": {"58121": "Normal"}}, {"vals": {"73617": "Story"}}, {"val": ""}, {"val": "22.250000"}, {"val": "-2-P2"}, {"vals": {"30093": "Customer experience"}}, {"val": "0/0"}, {}, {"val": "22.291666"}, {"val": ""}, {"vals": {"50092": "Platform"}}]
        }, {
            "eid": 59562,
            "values": [{"val": "685"}, {"val": "Add RT tests cases to cover issues identified in bugs"}, {"vals": {"31916": "Test & fix"}}, {"vals": {"58121": "Normal"}}, {"vals": {"73617": "Story"}}, {"val": ""}, {"val": ""}, {"val": "5-Test & fix"}, {"vals": {"67058": "Technical"}}, {"val": "0/1"}, {"vals": {"72599": "Shaofen Ning"}}, {"val": ""}, {"val": "#27601, #27603, #27606\n(these are very basic stuff in our platform, and we need automatic tests for them)"}, {"vals": {"50092": "Platform"}}]
        }, {
            "eid": 61644,
            "values": [{"val": "734"}, {"val": "Upgrade Angular and related libs to latest 1.x"}, {"vals": {"84277": "P1"}}, {"vals": {"58121": "Normal"}}, {"vals": {"73617": "Story"}}, {"val": ""}, {"val": "6.250000"}, {"val": "-1-P1"}, {"vals": {"67058": "Technical"}}, {"val": "0/0"}, {}, {"val": "5.791666"}, {"val": ""}, {"vals": {"50092": "Platform"}}]
        }, {
            "eid": 61937,
            "values": [{"val": "716"}, {"val": "Scalability  of 2000 Tenants"}, {"vals": {"49711": "Next"}}, {"vals": {"58121": "Normal"}}, {"vals": {"73617": "Story"}}, {"val": ""}, {"val": "12.250000"}, {"val": "0-Next"}, {"vals": {"30093": "Customer experience"}}, {"val": "0/0"}, {"vals": {"84630": "Brad Stevens"}}, {"val": "12.333333"}, {"val": ""}, {"vals": {"62307": "Operations"}}]
        }, {
            "eid": 63118,
            "values": [{"val": "548"}, {"val": "Data Connector: Look up by field rather than name"}, {"vals": {"84277": "P1"}}, {"vals": {"58121": "Normal"}}, {"vals": {"73617": "Story"}}, {"val": ""}, {"val": "35.250000"}, {"val": "-1-P1"}, {"vals": {"30093": "Customer experience"}}, {"val": "0/0"}, {"vals": {"46126": "Diana Walker"}}, {"val": "22.291666"}, {"val": ""}, {"vals": {"50092": "Platform"}}]
        }, {
            "eid": 64590,
            "values": [{"val": "528"}, {"val": "Connector - CSV/Excel import via API"}, {"vals": {"66887": "Development"}}, {"vals": {"58121": "Normal"}}, {"vals": {"73617": "Story"}}, {"val": ""}, {"val": "43.250000"}, {"val": "3-Development"}, {"vals": {"30093": "Customer experience"}}, {"val": "37/58"}, {"vals": {"46126": "Diana Walker"}}, {"val": "42.041666"}, {"val": ""}, {"vals": {"50092": "Platform"}}]
        }, {
            "eid": 64617,
            "values": [{"val": "725"}, {"val": "Add Icon/picture to screen with action"}, {"vals": {"64636": "P3"}}, {"vals": {"58121": "Normal"}}, {"vals": {"73617": "Story"}}, {"val": ""}, {"val": "7.250000"}, {"val": "-3-P3"}, {"vals": {"30093": "Customer experience"}}, {"val": "0/0"}, {}, {"val": "7.375000"}, {"val": "Add Icon/picture to screen with action (e.g. navigate, create new, run workflow, generate document)"}, {"vals": {"50092": "Platform"}}]
        }, {
            "eid": 65512,
            "values": [{"val": "531"}, {"val": "Single sign-on - oauth"}, {"vals": {"66887": "Development"}}, {"vals": {"58121": "Normal"}}, {"vals": {"73617": "Story"}}, {"val": ""}, {"val": "43.250000"}, {"val": "3-Development"}, {"vals": {"30093": "Customer experience"}}, {"val": "0/1"}, {"vals": {"46126": "Diana Walker"}}, {"val": "6.333333"}, {"val": "See [Wiki](http://spwiki.sp.local/display/DEV/SAML+Authentication+Requirements)"}, {"vals": {"50092": "Platform"}}]
        }, {
            "eid": 67843,
            "values": [{"val": "662"}, {"val": "NAB UI Feedback"}, {"vals": {"75156": "P2"}}, {"vals": {"58121": "Normal"}}, {"vals": {"73617": "Story"}}, {"val": ""}, {"val": "22.250000"}, {"val": "-2-P2"}, {"vals": {"30093": "Customer experience"}}, {"val": "0/0"}, {}, {"val": "22.250000"}, {"val": ""}, {"vals": {"50092": "Platform"}}]
        }, {
            "eid": 69394,
            "values": [{"val": "636"}, {"val": "Protect \"out of the box\" application"}, {"vals": {"49711": "Next"}}, {"vals": {"58121": "Normal"}}, {"vals": {"73617": "Story"}}, {"val": ""}, {"val": "26.250000"}, {"val": "0-Next"}, {"vals": {"30093": "Customer experience"}}, {"val": "0/5"}, {}, {"val": "22.291666"}, {"val": "[see wiki](http://spwiki.sp.local/display/DEV/Protecting+%27Out+of+the+Box%27+Applications)"}, {"vals": {"50092": "Platform"}}]
        }, {
            "eid": 71972,
            "values": [{"val": "650"}, {"val": "Mobile: Ability to show/hide columns on a report on mobile"}, {"vals": {"64636": "P3"}}, {"vals": {"58121": "Normal"}}, {"vals": {"73617": "Story"}}, {"val": ""}, {"val": "22.250000"}, {"val": "-3-P3"}, {"vals": {"30093": "Customer experience"}}, {"val": "0/0"}, {}, {"val": "22.375000"}, {"val": ""}, {"vals": {"50092": "Platform"}}]
        }, {
            "eid": 75388,
            "values": [{"val": "732"}, {"val": "boardView - make it production ready"}, {"vals": {"49711": "Next"}}, {"vals": {"58121": "Normal"}}, {"vals": {"73617": "Story"}}, {"val": ""}, {"val": "6.250000"}, {"val": "0-Next"}, {"vals": {"30093": "Customer experience"}}, {"val": "0/0"}, {"vals": {"74150": "Stephen Gibbon"}}, {"val": "6.291666"}, {"val": ""}, {"vals": {"50092": "Platform"}}]
        }, {
            "eid": 75506,
            "values": [{"val": "565"}, {"val": "GCR scalability with many tenants"}, {"vals": {"31916": "Test & fix"}}, {"vals": {"58121": "Normal"}}, {"vals": {"73617": "Story"}}, {"val": ""}, {"val": "34.250000"}, {"val": "5-Test & fix"}, {"vals": {"30093": "Customer experience"}}, {"val": "1/2"}, {"vals": {"74150": "Stephen Gibbon"}}, {"val": "6.250000"}, {"val": ""}, {"vals": {"50092": "Platform"}}]
        }, {
            "eid": 80153,
            "values": [{"val": "692"}, {"val": "Cleanup Person email workflows in shared"}, {"vals": {"82748": "Analysis done"}}, {"vals": {"58121": "Normal"}}, {"vals": {"73617": "Story"}}, {"val": ""}, {"val": "15.250000"}, {"val": "2-Analysis done"}, {"vals": {"30093": "Customer experience"}}, {"val": "0/0"}, {"vals": {"46126": "Diana Walker"}}, {"val": "6.291666"}, {"val": "Need to delete the following Triggers & Workflows in shared:\n1.\tPerson Business Email trigger & workflow\n2.\tPerson Other Email trigger & workflow\n3.\tPerson Personal Email trigger & workflow"}, {"vals": {"50092": "Platform"}}]
        }, {
            "eid": 84235,
            "values": [{"val": "723"}, {"val": "No-Context Actions on reports"}, {"vals": {"64636": "P3"}}, {"vals": {"58121": "Normal"}}, {"vals": {"73617": "Story"}}, {"val": ""}, {"val": "7.250000"}, {"val": "-3-P3"}, {"vals": {"30093": "Customer experience"}}, {"val": "0/0"}, {}, {"val": "7.375000"}, {"val": ""}, {"vals": {"50092": "Platform"}}]
        }, {
            "eid": 84983,
            "values": [{"val": "465"}, {"val": "Data Connector: Schedule import from CSV"}, {"vals": {"84277": "P1"}}, {"vals": {"58121": "Normal"}}, {}, {"val": "-1.000000000"}, {"val": "50.250000"}, {"val": "-1-P1"}, {"vals": {"30093": "Customer experience"}}, {"val": "0/0"}, {"vals": {"46126": "Diana Walker"}}, {"val": "22.291666"}, {"val": "PM's to confirm use case\n\nOptions:\n \n- Import UCF data\n \nwha tool are we using? e.g. mulesoft, cloud or on-prem?"}, {"vals": {"50092": "Platform"}}]
        }, {
            "eid": 59779,
            "values": [{"val": "450"}, {"val": "Record change auditing"}, {"vals": {"66887": "Development"}}, {"vals": {"58121": "Normal"}}, {}, {"val": "-0.375000000"}, {"val": "54.250000"}, {"val": "3-Development"}, {"vals": {"30093": "Customer experience"}}, {"val": "20/32"}, {"vals": {"46126": "Diana Walker"}}, {"val": "43.083333"}, {"val": "See [wiki](http://spwiki.sp.local/display/DEV/Resource+change+auditing)"}, {"vals": {"50092": "Platform"}}]
        }, {
            "eid": 63172,
            "values": [{"val": "448"}, {"val": "Run database health checks as a regular process in Team City via ReadiMon"}, {"vals": {"43891": "Shipped"}}, {"vals": {"58121": "Normal"}}, {}, {"val": "-0.187500000"}, {"val": "54.250000"}, {"val": "9-Shipped"}, {"vals": {"30734": "Operations"}}, {"val": "2/2"}, {"vals": {"74150": "Stephen Gibbon"}}, {"val": "6.333333"}, {"val": "Have the ability to run this as part of our Team City builds\n\nMaybe command line options to readimon to run queries and generate reports that can be checked.\nCould even emit TC messages to product test result like reporting. But that's optional."}, {"vals": {"50092": "Platform"}}]
        }, {
            "eid": 31975,
            "values": [{"val": "457"}, {"val": "Action Menu on Form"}, {"vals": {"75156": "P2"}}, {"vals": {"58121": "Normal"}}, {}, {"val": "-0.094000000"}, {"val": "54.250000"}, {"val": "-2-P2"}, {"vals": {"30093": "Customer experience"}}, {"val": "0/0"}, {}, {"val": "22.291666"}, {"val": "[wiki](http://spwiki.sp.local/display/DEV/Action+Menu+on+Form)\n \nAs a user I would like the ability to run workflow from a form\nAs a user I would like the ability to generate documents from a form\nAs a user I would like the ability to delete a resource from its own form.\n \nWhere to go after deleting a resource? Back to whatever you'd get by clicking on the cross.\n \nAlso, ensure that while this BL is being implimented, we also need to make sure workflow triggers are not displayed when not relevent (BL27253)"}, {"vals": {"50092": "Platform"}}]
        }, {
            "eid": 70272,
            "values": [{"val": "449"}, {"val": "Survey: Reconfigure Campaign Behaviour"}, {"vals": {"66887": "Development"}}, {"vals": {"58121": "Normal"}}, {}, {"val": "-0.094000000"}, {"val": "54.250000"}, {"val": "3-Development"}, {"vals": {"30093": "Customer experience"}}, {"val": "5/9"}, {"vals": {"51451": "Tina Gosain"}}, {"val": "29.333333"}, {"val": "[see wiki](http://spwiki.sp.local/display/DEV/Audit-Survey+-+Additional+Stories)"}, {"vals": {"50092": "Platform"}}]
        }, {
            "eid": 35540,
            "values": [{"val": "667"}, {"val": "SG misc tasks"}, {"vals": {"66887": "Development"}}, {"vals": {"58121": "Normal"}}, {"vals": {"73617": "Story"}}, {"val": "-0.023500000"}, {"val": "22.250000"}, {"val": "3-Development"}, {"vals": {"30093": "Customer experience"}}, {"val": "3/9"}, {"vals": {"74150": "Stephen Gibbon"}}, {"val": "22.083333"}, {"val": ""}, {"vals": {"50092": "Platform"}}]
        }, {
            "eid": 27335,
            "values": [{"val": "651"}, {"val": "Scalability: Add 'entity type' column to data tables to increase scalability"}, {"vals": {"64636": "P3"}}, {"vals": {"58121": "Normal"}}, {}, {"val": "0.000000000"}, {"val": "22.250000"}, {"val": "-3-P3"}, {"vals": {"67058": "Technical"}}, {"val": "0/0"}, {}, {"val": "22.291666"}, {"val": ""}, {"vals": {"50092": "Platform"}}]
        }, {
            "eid": 28842,
            "values": [{"val": "474"}, {"val": "Charts: axis groupings"}, {"vals": {"31916": "Test & fix"}}, {"vals": {"58121": "Normal"}}, {"vals": {"73360": "Feature"}}, {"val": "0.000000000"}, {"val": "48.250000"}, {"val": "5-Test & fix"}, {"vals": {"30093": "Customer experience"}}, {"val": "2/3"}, {"vals": {"46126": "Diana Walker"}}, {"val": "6.250000"}, {"val": ""}, {"vals": {"50092": "Platform"}}]
        }, {
            "eid": 29235,
            "values": [{"val": "657"}, {"val": "Cleanup home or add 'my tasks' report to landing page"}, {"vals": {"75156": "P2"}}, {"vals": {"58121": "Normal"}}, {}, {"val": "0.000000000"}, {"val": "22.250000"}, {"val": "-2-P2"}, {"vals": {"30093": "Customer experience"}}, {"val": "0/0"}, {}, {"val": "22.291666"}, {"val": ""}, {"vals": {"50092": "Platform"}}]
        }, {
            "eid": 30675,
            "values": [{"val": "447"}, {"val": "Inline relationship that can pick multiple values"}, {"vals": {"31916": "Test & fix"}}, {"vals": {"58121": "Normal"}}, {}, {"val": "0.000000000"}, {"val": "55.250000"}, {"val": "5-Test & fix"}, {"vals": {"30093": "Customer experience"}}, {"val": "3/6"}, {"vals": {"83192": "Darren Jacobs"}}, {"val": "42.250000"}, {"val": "[wiki](http://spwiki.sp.local/display/DEV/Multi-Inline+Picker)\n[tfs](http://sptfs.sp.local:8080/tfs/EntData/Inventus/_workitems#_a=edit&id=14439&triage=true)\n\n\nSometimes a relationship is a many-to-many, although in practice it will usually only have one or two values. In this case it is often preferable to only show an inline relationship control. Need a way to specify that a many-to-many relationship should be rendered as a multi-select-inline relationship picker control. Note: this will take extra time if the customer can configure this, as we need infrastructure to allow selection of different controls pointing to the same fields.\n \n \nSee wiki page: http://spwiki.sp.local/display/DEV/Multi-Inline+Picker\n \nAS: Listing of things implemented and require QA:\n1. For a relationship, 'Report' and 'Inline' display options are presented.\n2. By default, 'Report' display option is selected.\n3. Switching between 'Report' and 'Inline' options show/hide 'Display Report' detail.\n4. Switching between 'Report' and 'Inline' options should update horizontal/vertical modes under 'Format' tab.\n5. Switching between 'Report' and 'Inline' options should enable/disable vertical mode selection under 'Format' tab.\n6. In form view mode, each selected record name appears inline as a comma separated list, with each name beig a hyper-link so the user can navigate to that record.\n7. if name/s of all selected entity/es are not visible in view mode, show a clickable icon that opens a modal that lists all the selected entities names."}, {"vals": {"50092": "Platform"}}]
        }, {
            "eid": 33997,
            "values": [{"val": "637"}, {"val": "Change mobile UI around tabs..."}, {"vals": {"31916": "Test & fix"}}, {"vals": {"58121": "Normal"}}, {}, {"val": "0.000000000"}, {"val": "26.250000"}, {"val": "5-Test & fix"}, {"vals": {"30093": "Customer experience"}}, {"val": "2/3"}, {"vals": {"83192": "Darren Jacobs"}}, {"val": "6.250000"}, {"val": "Change mobile UI around tabs... \n\n[wiki](http://spwiki.sp.local/pages/viewpage.action?pageId=120291335#MobileStylingandUsabilityImprovements(Apr15)-bug24383)\n\nMobile & Tablet: Replace tabbed numbers with 3 menus: Detail, More, Actions. Also ensure fields in tabs work\n\nSee item in"}, {"vals": {"50092": "Platform"}}]
        }, {
            "eid": 40563,
            "values": [{"val": "620"}, {"val": "Survey: Modify Campaign on workflow actions"}, {"vals": {"64636": "P3"}}, {"vals": {"58121": "Normal"}}, {"vals": {"73617": "Story"}}, {"val": "0.000000000"}, {"val": "26.250000"}, {"val": "-3-P3"}, {"vals": {"30093": "Customer experience"}}, {"val": "0/0"}, {}, {"val": "26.166666"}, {"val": "[see wiki](http://spwiki.sp.local/display/DEV/Audit-Survey+-+Additional+Stories)"}, {"vals": {"50092": "Platform"}}]
        }, {
            "eid": 41442,
            "values": [{"val": "658"}, {"val": "Filter on screen"}, {"vals": {"75156": "P2"}}, {"vals": {"58121": "Normal"}}, {}, {"val": "0.000000000"}, {"val": "22.250000"}, {"val": "-2-P2"}, {"vals": {"30093": "Customer experience"}}, {"val": "0/0"}, {}, {"val": "22.291666"}, {"val": ""}, {"vals": {"50092": "Platform"}}]
        }, {
            "eid": 47037,
            "values": [{"val": "617"}, {"val": "Survey: Custom Survey design UI"}, {"vals": {"49711": "Next"}}, {"vals": {"58121": "Normal"}}, {"vals": {"73617": "Story"}}, {"val": "0.000000000"}, {"val": "26.250000"}, {"val": "0-Next"}, {"vals": {"30093": "Customer experience"}}, {"val": "0/0"}, {"vals": {"83192": "Darren Jacobs"}}, {"val": "7.375000"}, {"val": "[see wiki](http://spwiki.sp.local/display/DEV/Audit-Survey+-+Additional+Stories)"}, {"vals": {"50092": "Platform"}}]
        }, {
            "eid": 47043,
            "values": [{"val": "485"}, {"val": "SMS integration"}, {"vals": {"75156": "P2"}}, {"vals": {"58121": "Normal"}}, {"vals": {"73617": "Story"}}, {"val": "0.000000000"}, {"val": "26.250000"}, {"val": "-2-P2"}, {"vals": {"30093": "Customer experience"}}, {"val": "0/0"}, {}, {"val": "22.291666"}, {"val": ""}, {"vals": {"50092": "Platform"}}]
        }, {
            "eid": 53754,
            "values": [{"val": "473"}, {"val": "Charts: enhancements to axes/scales"}, {"vals": {"31916": "Test & fix"}}, {"vals": {"58121": "Normal"}}, {"vals": {"73360": "Feature"}}, {"val": "0.000000000"}, {"val": "48.250000"}, {"val": "5-Test & fix"}, {"vals": {"30093": "Customer experience"}}, {"val": "9/11"}, {"vals": {"46126": "Diana Walker"}}, {"val": "15.250000"}, {"val": "A grab-bag of improvements to scales.\n\nThe ability to specify whether a scale acts as a linear or category scale, if/when applicable.\nThe introduction of the 'log' scale type.\n"}, {"vals": {"50092": "Platform"}}]
        }, {
            "eid": 54499,
            "values": [{"val": "443"}, {"val": "Remove SQL filestream"}, {"vals": {"81339": "Acceptance"}}, {"vals": {"58121": "Normal"}}, {}, {"val": "0.000000000"}, {"val": "55.250000"}, {"val": "7-Acceptance"}, {"vals": {"67058": "Technical"}}, {"val": "9/9"}, {"vals": {"74150": "Stephen Gibbon"}}, {"val": "6.250000"}, {"val": "see [Moving off filestream](http://spwiki.sp.local/display/DEV/Story+-+Moving+Off+Filestream)\nsee http://spwiki.sp.local/pages/viewpage.action?pageId=190906404 for info on dev changes"}, {"vals": {"50092": "Platform"}}]
        }, {
            "eid": 63648,
            "values": [{"val": "513"}, {"val": "Work Required for On-Premise Deployment"}, {"vals": {"64636": "P3"}}, {"vals": {"58121": "Normal"}}, {"vals": {"73617": "Story"}}, {"val": "0.000000000"}, {"val": "46.250000"}, {"val": "-3-P3"}, {"vals": {"30093": "Customer experience"}}, {"val": "0/0"}, {}, {"val": "26.333333"}, {"val": "who owns this? what is it exactly?"}, {"vals": {"50092": "Platform"}}]
        }, {
            "eid": 64239,
            "values": [{"val": "440"}, {"val": "Record stats for Licencing Purposes and import to CAST"}, {"vals": {"64636": "P3"}}, {"vals": {"58121": "Normal"}}, {}, {"val": "0.000000000"}, {"val": "55.250000"}, {"val": "-3-P3"}, {"vals": {"30093": "Customer experience"}}, {"val": "0/0"}, {"vals": {"83192": "Darren Jacobs"}}, {"val": "26.333333"}, {"val": ""}, {"vals": {"50092": "Platform"}}]
        }, {
            "eid": 71201,
            "values": [{"val": "654"}, {"val": "Some way to refresh data on form when workflow updates"}, {"vals": {"75156": "P2"}}, {"vals": {"58121": "Normal"}}, {}, {"val": "0.000000000"}, {"val": "22.250000"}, {"val": "-2-P2"}, {"vals": {"30093": "Customer experience"}}, {"val": "0/0"}, {}, {"val": "22.291666"}, {"val": ""}, {"vals": {"50092": "Platform"}}]
        }, {
            "eid": 75043,
            "values": [{"val": "445"}, {"val": "Get CAST working on Production"}, {"vals": {"31916": "Test & fix"}}, {"vals": {"58121": "Normal"}}, {}, {"val": "0.000000000"}, {"val": "55.250000"}, {"val": "5-Test & fix"}, {"vals": {"30093": "Customer experience"}}, {"val": "7/13"}, {"vals": {"83192": "Darren Jacobs"}}, {"val": "6.250000"}, {"val": ""}, {"vals": {"50092": "Platform"}}]
        }, {
            "eid": 75564,
            "values": [{"val": "514"}, {"val": "Licence Feature (ability to licence modules/use cases)"}, {"vals": {"49711": "Next"}}, {"vals": {"58121": "Normal"}}, {}, {"val": "0.000000000"}, {"val": "46.250000"}, {"val": "0-Next"}, {"vals": {"30093": "Customer experience"}}, {"val": "0/0"}, {"vals": {"83192": "Darren Jacobs"}}, {"val": "22.291666"}, {"val": "[see wiki](http://spwiki.sp.local/display/DEV/Licensing+Capability)"}, {"vals": {"50092": "Platform"}}]
        }, {
            "eid": 75636,
            "values": [{"val": "546"}, {"val": "Text capitalization"}, {"vals": {"84277": "P1"}}, {"vals": {"58121": "Normal"}}, {}, {"val": "0.000000000"}, {"val": "40.250000"}, {"val": "-1-P1"}, {"vals": {"30093": "Customer experience"}}, {"val": "0/0"}, {"vals": {"46126": "Diana Walker"}}, {"val": "19.958333"}, {"val": "Implement new capitalization standards as per:\n[wiki](http://spwiki.sp.local/pages/viewpage.action?pageId=7536693)\n \nThis requires:\n- creating a javascript function to convert sentence case to title case\n- applying that function in places that should be title case (e.g. headings, report columns)\n- creating a tool, query, or similar that we can use to convert existing schema names to sentence case"}, {"vals": {"50092": "Platform"}}]
        }, {
            "eid": 76074,
            "values": [{"val": "458"}, {"val": "Business Process: Conditional worklflow actions"}, {"vals": {"64636": "P3"}}, {"vals": {"58121": "Normal"}}, {}, {"val": "0.000000000"}, {"val": "54.250000"}, {"val": "-3-P3"}, {"vals": {"30093": "Customer experience"}}, {"val": "0/0"}, {}, {"val": "22.291666"}, {"val": ""}, {"vals": {"50092": "Platform"}}]
        }, {
            "eid": 77090,
            "values": [{"val": "619"}, {"val": "Survey: Add Person/Target After survey has been sent"}, {"vals": {"64636": "P3"}}, {"vals": {"58121": "Normal"}}, {"vals": {"73617": "Story"}}, {"val": "0.000000000"}, {"val": "26.250000"}, {"val": "-3-P3"}, {"vals": {"30093": "Customer experience"}}, {"val": "0/0"}, {}, {"val": "26.333333"}, {"val": "[see wiki](http://spwiki.sp.local/display/DEV/Audit-Survey+-+Additional+Stories)"}, {"vals": {"50092": "Platform"}}]
        }, {
            "eid": 79936,
            "values": [{"val": "547"}, {"val": "ReadiGRC0316 Testing Bugs Focus"}, {"vals": {"43891": "Shipped"}}, {"vals": {"58121": "Normal"}}, {}, {"val": "0.000000000"}, {"val": "35.250000"}, {"val": "9-Shipped"}, {}, {"val": "2/2"}, {"vals": {"46126": "Diana Walker"}}, {"val": "6.333333"}, {"val": ""}, {"vals": {"50092": "Platform"}}]
        }, {
            "eid": 80485,
            "values": [{"val": "655"}, {"val": "Workflow: push long running workflows out of IIS"}, {"vals": {"75156": "P2"}}, {"vals": {"58121": "Normal"}}, {}, {"val": "0.000000000"}, {"val": "22.250000"}, {"val": "-2-P2"}, {"vals": {"67058": "Technical"}}, {"val": "0/0"}, {}, {"val": "22.291666"}, {"val": ""}, {"vals": {"50092": "Platform"}}]
        }, {
            "eid": 84898,
            "values": [{"val": "618"}, {"val": "Survey: Survey Paging"}, {"vals": {"64636": "P3"}}, {"vals": {"58121": "Normal"}}, {"vals": {"73617": "Story"}}, {"val": "0.000000000"}, {"val": "22.250000"}, {"val": "-3-P3"}, {"vals": {"30093": "Customer experience"}}, {"val": "0/0"}, {}, {"val": "22.291666"}, {"val": "[see wiki](http://spwiki.sp.local/display/DEV/Audit-Survey+-+Additional+Stories)"}, {"vals": {"50092": "Platform"}}]
        }, {
            "eid": 86280,
            "values": [{"val": "446"}, {"val": "Business Process Auditing"}, {"vals": {"81339": "Acceptance"}}, {"vals": {"58121": "Normal"}}, {}, {"val": "0.000000000"}, {"val": "54.250000"}, {"val": "7-Acceptance"}, {"vals": {"30093": "Customer experience"}}, {"val": "10/11"}, {"vals": {"46126": "Diana Walker"}}, {"val": "6.250000"}, {"val": "[wiki](http://spwiki.sp.local/display/DEV/Approval+Task+History+Requirements)"}, {"vals": {"50092": "Platform"}}]
        }]
    };

    rnBoardTestData.kanbanStatesResults = {
        "meta": {
            "modified": "2016-03-28T16:01:10Z",
            "hideheader": false,
            "hideact": false,
            "dfid": 59373,
            "typefmtstyle": {"String": ["Highlight", "Icon"], "Int32": ["Highlight", "Icon", "ProgressBar"]},
            "rcols": {
                "39996": {
                    "entityname": true,
                    "maxlen": 200,
                    "tid": 76997,
                    "fid": 33222,
                    "rid": 35299,
                    "ord": 0,
                    "regexerr": "The value must not contain angled brackets.",
                    "regex": "^[^<>]+$",
                    "oprtype": "InlineRelationship",
                    "type": "String",
                    "title": "Kanban State"
                },
                "36342": {"fid": 66834, "ord": 1, "oprtype": "Int32", "type": "Int32", "title": "WIP Limit"},
                "79380": {"fid": 58563, "ord": 2, "oprtype": "Int32", "type": "Int32", "title": "Ord"},
                "47263": {
                    "mline": true,
                    "maxlen": 10000,
                    "fid": 57784,
                    "ord": 3,
                    "oprtype": "String",
                    "type": "String",
                    "title": "Description"
                }
            },
            "sort": [{"order": "Ascending", "colid": "79380"}],
            "inline": {"76997": 81579},
            "anlcols": {
                "62182": {
                    "tid": 76997,
                    "ord": -1,
                    "doper": "Contains",
                    "oper": "Unspecified",
                    "anltype": "InlineRelationship",
                    "type": "String",
                    "title": "Kanban State"
                },
                "68708": {
                    "ord": 1,
                    "doper": "Contains",
                    "oper": "Unspecified",
                    "anltype": "String",
                    "type": "String",
                    "title": "Description"
                },
                "77619": {
                    "ord": 0,
                    "doper": "GreaterThan",
                    "oper": "Unspecified",
                    "anltype": "Int32",
                    "type": "Int32",
                    "title": "WIP Limit"
                }
            },
            "rmeta": {"rollup": true, "showoplbl": true},
            "invalid": {"nodes": null, "columns": null, "conditions": null},
            "style": "Default",
            "title": "Kanban State Report"
        },
        "gdata": [{
            "eid": 64636,
            "values": [{"val": "P3"}, {"val": "8"}, {"val": "-3"}, {"val": "Items ready for consideration to move to P2"}]
        }, {
            "eid": 75156,
            "values": [{"val": "P2"}, {"val": "4"}, {"val": "-2"}, {"val": "Items for consideration to move to P1 state"}]
        }, {
            "eid": 84277,
            "values": [{"val": "P1"}, {"val": "2"}, {"val": "-1"}, {"val": "Items ready to be considered for the Next state, i.e. on the board"}]
        }, {"eid": 49711, "values": [{"val": "Next"}, {"val": ""}, {"val": "0"}, {"val": ""}]}, {
            "eid": 37939,
            "values": [{"val": "Analysis"}, {"val": "3"}, {"val": "1"}, {"val": "Definition of Done\n* goal is clear\n* first tasks defined\n* story is split if needed"}]
        }, {"eid": 82748, "values": [{"val": "Analysis done"}, {"val": ""}, {"val": "2"}, {"val": ""}]}, {
            "eid": 66887,
            "values": [{"val": "Development"}, {"val": "10"}, {"val": "3"}, {"val": "Definition of done:\n* code clean and committed to trunk\n* unit and intg test written\n* RT drivers written\n* automated testing passing on trunk\n"}]
        }, {
            "eid": 35175,
            "values": [{"val": "Development done"}, {"val": ""}, {"val": "4"}, {"val": ""}]
        }, {
            "eid": 31916,
            "values": [{"val": "Test & fix"}, {"val": "7"}, {"val": "5"}, {"val": "Definition of done:\n* all manual verification done\n* on UAC system\n* to be completed"}]
        }, {
            "eid": 58280,
            "values": [{"val": "Test & fix done"}, {"val": ""}, {"val": "6"}, {"val": ""}]
        }, {
            "eid": 81339,
            "values": [{"val": "Acceptance"}, {"val": "3"}, {"val": "7"}, {"val": "Definition of done:\n* \"Customer\" accepted\n* Ready for production deployment"}]
        }, {
            "eid": 60621,
            "values": [{"val": "Acceptance done"}, {"val": ""}, {"val": "8"}, {"val": ""}]
        }, {"eid": 43891, "values": [{"val": "Shipped"}, {"val": ""}, {"val": "9"}, {"val": ""}]}]
    };

    rnBoardTestData.projectResults = {
        "meta": {
            "modified": "2016-04-06T20:06:44Z",
            "hideheader": false,
            "hideact": false,
            "dfid": 49709,
            "typefmtstyle": {"String": ["Highlight", "Icon"], "Int32": ["Highlight", "Icon", "ProgressBar"]},
            "rcols": {
                "45875": {"fid": 68924, "ord": 2, "oprtype": "Int32", "type": "Int32", "title": "Ordinal"},
                "46117": {
                    "mline": true,
                    "maxlen": 10000,
                    "fid": 57784,
                    "ord": 1,
                    "oprtype": "String",
                    "type": "String",
                    "title": "Description"
                },
                "72563": {
                    "entityname": true,
                    "maxlen": 200,
                    "tid": 78100,
                    "fid": 33222,
                    "ord": 0,
                    "regexerr": "The value must not contain angled brackets.",
                    "regex": "^[^<>]+$",
                    "oprtype": "InlineRelationship",
                    "type": "String",
                    "title": "Project"
                }
            },
            "sort": [{"order": "Ascending", "colid": "45875"}],
            "inline": {"78100": 29643},
            "anlcols": {
                "27453": {
                    "tid": 78100,
                    "ord": -1,
                    "doper": "Contains",
                    "oper": "Unspecified",
                    "anltype": "InlineRelationship",
                    "type": "String",
                    "title": "Project"
                },
                "29220": {
                    "ord": 1,
                    "doper": "GreaterThan",
                    "oper": "Unspecified",
                    "anltype": "Int32",
                    "type": "Int32",
                    "title": "Ordinal"
                },
                "31509": {
                    "ord": 0,
                    "doper": "Contains",
                    "oper": "Unspecified",
                    "anltype": "String",
                    "type": "String",
                    "title": "Description"
                }
            },
            "rmeta": {"rollup": true, "showoplbl": true},
            "invalid": {"nodes": null, "columns": null, "conditions": null},
            "style": "Default",
            "title": "Project Report"
        },
        "gdata": [{
            "eid": 50092,
            "values": [{"val": "Platform"}, {"val": "The ReadiNow Platform"}, {"val": "1"}]
        }, {
            "eid": 62307,
            "values": [{"val": "Operations"}, {"val": "Operations and support items not directly tied to stories for the Platform and apps"}, {"val": "2"}]
        }, {"eid": 59728, "values": [{"val": "GRC"}, {"val": "The GRC application"}, {"val": "3"}]}, {
            "eid": 29859,
            "values": [{"val": "Ship It"}, {"val": "The Ship It application"}, {"val": "4"}]
        }]
    };

    rnBoardTestData.teamMemberResults = {
        "meta": {
            "modified": "2016-03-16T19:33:23Z",
            "hideheader": false,
            "hideact": false,
            "dfid": 80696,
            "typefmtstyle": {"String": ["Highlight", "Icon"], "ChoiceRelationship": ["Highlight", "Icon"]},
            "rcols": {
                "43266": {
                    "maxlen": 200,
                    "tid": 42143,
                    "fid": 33222,
                    "ord": 1,
                    "regexerr": "The value must not contain angled brackets.",
                    "regex": "^[^<>]+$",
                    "oprtype": "ChoiceRelationship",
                    "type": "ChoiceRelationship",
                    "title": "Team Role"
                },
                "51079": {
                    "entityname": true,
                    "maxlen": 200,
                    "tid": 43300,
                    "fid": 33222,
                    "ord": 0,
                    "regexerr": "The value must not contain angled brackets.",
                    "regex": "^[^<>]+$",
                    "oprtype": "UserString",
                    "type": "String",
                    "title": "Team Member"
                }
            },
            "choice": {
                "42143": [{"id": 71325, "name": "Dev"}, {"id": 45912, "name": "QA"}, {
                    "id": 64597,
                    "name": "PM"
                }, {"id": 39689, "name": "Dev Mgr"}, {"id": 40083, "name": "Architect"}, {
                    "id": 72887,
                    "name": "Dev Ops"
                }, {"id": 27313, "name": "UI"}]
            },
            "inline": {"43300": 38726},
            "anlcols": {
                "58689": {
                    "ord": 0,
                    "doper": "Contains",
                    "oper": "Unspecified",
                    "anltype": "UserString",
                    "type": "String",
                    "title": "Team Member"
                },
                "65619": {
                    "tid": 42143,
                    "ord": 1,
                    "values": {"40083": "Architect", "45912": "QA", "64597": "PM", "71325": "Dev", "72887": "Dev Ops"},
                    "doper": "AnyOf",
                    "oper": "AnyOf",
                    "anltype": "ChoiceRelationship",
                    "type": "ChoiceRelationship",
                    "title": "Team Role"
                }
            },
            "rmeta": {"rollup": true, "showoplbl": true},
            "invalid": {"nodes": null, "columns": null, "conditions": null},
            "style": "Default",
            "title": "Active Team Member"
        },
        "gdata": [{"eid": 36042, "values": [{"val": "Scott Hopwood"}, {"vals": {"40083": "Architect"}}]}, {
            "eid": 42503,
            "values": [{"val": "Abida Begum"}, {"vals": {"45912": "QA"}}]
        }, {"eid": 46126, "values": [{"val": "Diana Walker"}, {"vals": {"64597": "PM"}}]}, {
            "eid": 49754,
            "values": [{"val": "Peter Aylett"}, {"vals": {"40083": "Architect"}}]
        }, {"eid": 51451, "values": [{"val": "Tina Gosain"}, {"vals": {"45912": "QA"}}]}, {
            "eid": 53443,
            "values": [{"val": "Anurag Sharma"}, {"vals": {"71325": "Dev"}}]
        }, {"eid": 55205, "values": [{"val": "Nino Carabella"}, {"vals": {"64597": "PM"}}]}, {
            "eid": 56745,
            "values": [{"val": "Alex Engelhardt"}, {"vals": {"71325": "Dev"}}]
        }, {"eid": 58343, "values": [{"val": "Kun Dai"}, {"vals": {"71325": "Dev"}}]}, {
            "eid": 59079,
            "values": [{"val": "Joe User"}, {"vals": {"72887": "Dev Ops"}}]
        }, {"eid": 64540, "values": [{"val": "Greg Margossian"}, {"vals": {"64597": "PM"}}]}, {
            "eid": 72599,
            "values": [{"val": "Shaofen Ning"}, {"vals": {"45912": "QA"}}]
        }, {"eid": 74150, "values": [{"val": "Stephen Gibbon"}, {"vals": {"40083": "Architect"}}]}, {
            "eid": 81545,
            "values": [{"val": "Karen Jones"}, {"vals": {"45912": "QA"}}]
        }, {"eid": 82449, "values": [{"val": "David Quint"}, {"vals": {"71325": "Dev"}}]}, {
            "eid": 83192,
            "values": [{"val": "Darren Jacobs"}, {"vals": {"64597": "PM"}}]
        }, {"eid": 84314, "values": [{"val": "Con Christou"}, {"vals": {"71325": "Dev"}}]}, {
            "eid": 84630,
            "values": [{"val": "Brad Stevens"}, {"vals": {"72887": "Dev Ops"}}]
        }]
    };

    rnBoardTestData.tasksReport = {
        "meta": {
            "modified": "2016-04-06T20:03:40Z",
            "hideheader": false,
            "hideact": false,
            "dfid": 82868,
            "typefmtstyle": {
                "Int32": ["Highlight", "Icon", "ProgressBar"],
                "String": ["Highlight", "Icon"],
                "ChoiceRelationship": ["Highlight", "Icon"],
                "InlineRelationship": ["Highlight", "Icon"]
            },
            "rcols": {
                "81870": {"fid": 85868, "ord": 0, "oprtype": "Int32", "type": "Int32", "title": "ID"},
                "44398": {"fid": 76593, "ord": 1, "oprtype": "String", "type": "String", "title": "Summary"},
                "29309": {
                    "maxlen": 200,
                    "tid": 78166,
                    "fid": 33222,
                    "ord": 2,
                    "regexerr": "The value must not contain angled brackets.",
                    "regex": "^[^<>]+$",
                    "oprtype": "ChoiceRelationship",
                    "type": "ChoiceRelationship",
                    "title": "State"
                },
                "73418": {
                    "maxlen": 200,
                    "tid": 79567,
                    "fid": 33222,
                    "ord": 3,
                    "regexerr": "The value must not contain angled brackets.",
                    "regex": "^[^<>]+$",
                    "oprtype": "ChoiceRelationship",
                    "type": "ChoiceRelationship",
                    "title": "Priority"
                },
                "78537": {
                    "maxlen": 200,
                    "tid": 33454,
                    "fid": 33222,
                    "ord": 4,
                    "card": "ManyToOne",
                    "regexerr": "The value must not contain angled brackets.",
                    "regex": "^[^<>]+$",
                    "oprtype": "InlineRelationship",
                    "type": "InlineRelationship",
                    "title": "Story"
                },
                "27955": {
                    "maxlen": 200,
                    "tid": 43300,
                    "fid": 33222,
                    "ord": 5,
                    "card": "ManyToOne",
                    "regexerr": "The value must not contain angled brackets.",
                    "regex": "^[^<>]+$",
                    "oprtype": "UserInlineRelationship",
                    "type": "InlineRelationship",
                    "title": "Assigned"
                },
                "84814": {
                    "maxlen": 200,
                    "fid": 33222,
                    "ord": 6,
                    "regexerr": "The value must not contain angled brackets.",
                    "regex": "^[^<>]+$",
                    "oprtype": "Image",
                    "type": "Image",
                    "title": "Avatar"
                }
            },
            "choice": {
                "78166": [{"id": 81767, "name": "Not Started"}, {"id": 83984, "name": "In Progress"}, {
                    "id": 77304,
                    "name": "Done"
                }, {"id": 33275, "name": "Blocked"}],
                "79567": [{"id": 45648, "name": "ASAP"}, {"id": 61160, "name": "High"}, {"id": 58121, "name": "Normal"}]
            },
            "inline": {"33454": 76524, "43300": 38726},
            "anlcols": {
                "36270": {
                    "ord": 0,
                    "doper": "Contains",
                    "oper": "Unspecified",
                    "anltype": "String",
                    "type": "String",
                    "title": "Summary"
                },
                "42762": {
                    "tid": 79567,
                    "ord": 4,
                    "doper": "AnyOf",
                    "oper": "Unspecified",
                    "anltype": "ChoiceRelationship",
                    "type": "ChoiceRelationship",
                    "title": "Priority"
                },
                "47447": {
                    "ord": -1,
                    "doper": "GreaterThan",
                    "oper": "Unspecified",
                    "anltype": "Int32",
                    "type": "Int32",
                    "title": "ID"
                },
                "56854": {
                    "tid": 43300,
                    "ord": 1,
                    "doper": "AnyOf",
                    "oper": "Unspecified",
                    "anltype": "UserInlineRelationship",
                    "type": "InlineRelationship",
                    "title": "Assigned"
                },
                "76375": {
                    "tid": 78166,
                    "ord": 3,
                    "values": {"77304": "Done"},
                    "doper": "AnyOf",
                    "oper": "AnyExcept",
                    "anltype": "ChoiceRelationship",
                    "type": "ChoiceRelationship",
                    "title": "State"
                },
                "85739": {
                    "tid": 33454,
                    "ord": 2,
                    "doper": "AnyOf",
                    "oper": "Unspecified",
                    "anltype": "InlineRelationship",
                    "type": "InlineRelationship",
                    "title": "Story"
                }
            },
            "valrules": {"84814": {"imgw": 16, "imgh": 16, "sizeid": 74479, "scaleid": 57593}},
            "rmeta": {"rollup": true, "showoplbl": true},
            "invalid": {"nodes": null, "columns": null, "conditions": null},
            "style": "Default",
            "title": "Tasks on Story Board Report"
        },
        "gdata": [{
            "eid": 27041,
            "values": [{"val": "594"}, {"val": "Unit test coverage for structure views"}, {}, {"vals": {"58121": "Normal"}}, {"vals": {"30455": "494 - Structure Views"}}, {"vals": {"53443": "Anurag Sharma"}}, {}]
        }, {
            "eid": 28179,
            "values": [{"val": "730"}, {"val": "Dev: implement errors or warnings"}, {"vals": {"81767": "Not Started"}}, {"vals": {"58121": "Normal"}}, {"vals": {"64590": "528 - Connector - CSV/Excel import via API"}}, {"vals": {"49754": "Peter Aylett"}}, {}]
        }, {
            "eid": 28459,
            "values": [{"val": "527"}, {"val": "Outline what is needed operationally for SQL HA"}, {"vals": {"81767": "Not Started"}}, {"vals": {"58121": "Normal"}}, {"vals": {"47220": "591 - Implement SQL failover"}}, {"vals": {"84630": "Brad Stevens"}}, {}]
        }, {
            "eid": 28510,
            "values": [{"val": "581"}, {"val": "Dev: Support multiple Excel tabs"}, {"vals": {"33275": "Blocked"}}, {"vals": {"45648": "ASAP"}}, {"vals": {"64590": "528 - Connector - CSV/Excel import via API"}}, {"vals": {"49754": "Peter Aylett"}}, {}]
        }, {
            "eid": 28736,
            "values": [{"val": "621"}, {"val": "Compression and partitioning of message packages"}, {"vals": {"81767": "Not Started"}}, {"vals": {"58121": "Normal"}}, {"vals": {"75043": "445 - Get CAST working on Production"}}, {"vals": {"56745": "Alex Engelhardt"}}, {}]
        }, {
            "eid": 28838,
            "values": [{"val": "553"}, {"val": "RT drivers for Questionaire"}, {"vals": {"81767": "Not Started"}}, {"vals": {"58121": "Normal"}}, {"vals": {"81456": "452 - Questionaire Tools"}}, {"vals": {"36042": "Scott Hopwood"}}, {}]
        }, {
            "eid": 29829,
            "values": [{"val": "554"}, {"val": "RT tests for Questionaire"}, {"vals": {"81767": "Not Started"}}, {"vals": {"58121": "Normal"}}, {"vals": {"81456": "452 - Questionaire Tools"}}, {}, {}]
        }, {
            "eid": 30081,
            "values": [{"val": "489"}, {"val": "Setup CAST Server on AWS"}, {"vals": {"81767": "Not Started"}}, {"vals": {"58121": "Normal"}}, {"vals": {"75043": "445 - Get CAST working on Production"}}, {"vals": {"84630": "Brad Stevens"}}, {}]
        }, {
            "eid": 30512,
            "values": [{"val": "694"}, {"val": "looking at create-update-scenario RT test"}, {"vals": {"83984": "In Progress"}}, {"vals": {"58121": "Normal"}}, {"vals": {"35540": "667 - SG misc tasks"}}, {"vals": {"74150": "Stephen Gibbon"}}, {}]
        }, {
            "eid": 31561,
            "values": [{"val": "512"}, {"val": "QA on Survey"}, {"vals": {"81767": "Not Started"}}, {"vals": {"58121": "Normal"}}, {"vals": {"81456": "452 - Questionaire Tools"}}, {}, {}]
        }, {
            "eid": 33250,
            "values": [{"val": "491"}, {"val": "SSL Support RabbitMQ"}, {"vals": {"81767": "Not Started"}}, {"vals": {"58121": "Normal"}}, {"vals": {"75043": "445 - Get CAST working on Production"}}, {"vals": {"56745": "Alex Engelhardt"}}, {}]
        }, {
            "eid": 34392,
            "values": [{"val": "738"}, {"val": "Unit tests: For record merging"}, {"vals": {"81767": "Not Started"}}, {"vals": {"58121": "Normal"}}, {"vals": {"64590": "528 - Connector - CSV/Excel import via API"}}, {"vals": {"49754": "Peter Aylett"}}, {}]
        }, {
            "eid": 37888,
            "values": [{"val": "600"}, {"val": "Have a design meeting to brainstorm ideas"}, {"vals": {"81767": "Not Started"}}, {"vals": {"58121": "Normal"}}, {"vals": {"27851": "456 - Move application .db over to a text-based format"}}, {}, {}]
        }, {
            "eid": 38025,
            "values": [{"val": "699"}, {"val": "Working on SAML prototype"}, {"vals": {"83984": "In Progress"}}, {"vals": {"58121": "Normal"}}, {"vals": {"65512": "531 - Single sign-on - oauth"}}, {"vals": {"84314": "Con Christou"}}, {}]
        }, {
            "eid": 39121,
            "values": [{"val": "480"}, {"val": "#27452 - Charts: if a gantt-style range chart has rows that start/end on the same day then the line is skinny"}, {"vals": {"33275": "Blocked"}}, {"vals": {"61160": "High"}}, {"vals": {"53754": "473 - Charts: enhancements to axes/scales"}}, {}, {}]
        }, {
            "eid": 39471,
            "values": [{"val": "608"}, {"val": "Define Test Cases"}, {"vals": {"81767": "Not Started"}}, {"vals": {"58121": "Normal"}}, {"vals": {"70272": "449 - Survey: Reconfigure Campaign Behaviour"}}, {"vals": {"51451": "Tina Gosain"}}, {}]
        }, {
            "eid": 40190,
            "values": [{"val": "702"}, {"val": "Dev: make configuration editable without doing actual import"}, {"vals": {"81767": "Not Started"}}, {"vals": {"58121": "Normal"}}, {"vals": {"64590": "528 - Connector - CSV/Excel import via API"}}, {"vals": {"49754": "Peter Aylett"}}, {}]
        }, {
            "eid": 40931,
            "values": [{"val": "567"}, {"val": "Test"}, {"vals": {"81767": "Not Started"}}, {"vals": {"58121": "Normal"}}, {"vals": {"75506": "565 - GCR scalability with many tenants"}}, {"vals": {"84630": "Brad Stevens"}}, {}]
        }, {
            "eid": 41188,
            "values": [{"val": "737"}, {"val": "User Action Log Entry: Make the description field 100% horizontally in User Action Log Entry default form"}, {"vals": {"81767": "Not Started"}}, {"vals": {"58121": "Normal"}}, {"vals": {"86280": "446 - Business Process Auditing"}}, {"vals": {"58343": "Kun Dai"}}, {}]
        }, {
            "eid": 41284,
            "values": [{"val": "729"}, {"val": "Dev: add configuration [last row number for importing]"}, {"vals": {"33275": "Blocked"}}, {"vals": {"58121": "Normal"}}, {"vals": {"64590": "528 - Connector - CSV/Excel import via API"}}, {"vals": {"49754": "Peter Aylett"}}, {}]
        }, {
            "eid": 42297,
            "values": [{"val": "696"}, {"val": "Dev: Support time zones"}, {"vals": {"81767": "Not Started"}}, {"vals": {"58121": "Normal"}}, {"vals": {"64590": "528 - Connector - CSV/Excel import via API"}}, {"vals": {"49754": "Peter Aylett"}}, {}]
        }, {
            "eid": 43493,
            "values": [{"val": "610"}, {"val": "Create UI for selecting relationship for target campaign"}, {"vals": {"83984": "In Progress"}}, {"vals": {"58121": "Normal"}}, {"vals": {"70272": "449 - Survey: Reconfigure Campaign Behaviour"}}, {"vals": {"56745": "Alex Engelhardt"}}, {}]
        }, {
            "eid": 46766,
            "values": [{"val": "680"}, {"val": "Fallout of AWS workflow bug"}, {"vals": {"81767": "Not Started"}}, {"vals": {"58121": "Normal"}}, {}, {}, {}]
        }, {
            "eid": 47151,
            "values": [{"val": "545"}, {"val": "Verify a RNServer built with VS 2015 and with a .NET 4.6.1 dependency can install on our servers"}, {"vals": {"81767": "Not Started"}}, {"vals": {"58121": "Normal"}}, {"vals": {"84629": "441 - Migrate to .Net 4.6.1 and VS2015"}}, {"vals": {"81545": "Karen Jones"}}, {}]
        }, {
            "eid": 47916,
            "values": [{"val": "709"}, {"val": "QA: testing importing csv"}, {"vals": {"81767": "Not Started"}}, {"vals": {"58121": "Normal"}}, {"vals": {"64590": "528 - Connector - CSV/Excel import via API"}}, {"vals": {"72599": "Shaofen Ning"}}, {}]
        }, {
            "eid": 48423,
            "values": [{"val": "540"}, {"val": "RT tests for inline relationship"}, {"vals": {"81767": "Not Started"}}, {"vals": {"58121": "Normal"}}, {"vals": {"30675": "447 - Inline relationship that can pick multiple values"}}, {"vals": {"72599": "Shaofen Ning"}}, {}]
        }, {
            "eid": 49669,
            "values": [{"val": "488"}, {"val": "QA on New CAST System"}, {"vals": {"81767": "Not Started"}}, {"vals": {"58121": "Normal"}}, {"vals": {"75043": "445 - Get CAST working on Production"}}, {}, {}]
        }, {
            "eid": 49710,
            "values": [{"val": "714"}, {"val": "get ship it moved to production"}, {"vals": {"81767": "Not Started"}}, {"vals": {"58121": "Normal"}}, {"vals": {"35540": "667 - SG misc tasks"}}, {"vals": {"74150": "Stephen Gibbon"}}, {}]
        }, {
            "eid": 49814,
            "values": [{"val": "504"}, {"val": "QA on Axis groupings"}, {"vals": {"83984": "In Progress"}}, {"vals": {"58121": "Normal"}}, {"vals": {"28842": "474 - Charts: axis groupings"}}, {"vals": {"81545": "Karen Jones"}}, {}]
        }, {
            "eid": 50169,
            "values": [{"val": "508"}, {"val": "Readitest Tests for Structure VIews"}, {"vals": {"83984": "In Progress"}}, {"vals": {"58121": "Normal"}}, {"vals": {"30455": "494 - Structure Views"}}, {"vals": {"42503": "Abida Begum"}}, {}]
        }, {
            "eid": 50357,
            "values": [{"val": "708"}, {"val": "QA: testing importing excel"}, {"vals": {"83984": "In Progress"}}, {"vals": {"58121": "Normal"}}, {"vals": {"64590": "528 - Connector - CSV/Excel import via API"}}, {"vals": {"72599": "Shaofen Ning"}}, {}]
        }, {
            "eid": 50908,
            "values": [{"val": "526"}, {"val": "Figure out the cost of auditing"}, {"vals": {"81767": "Not Started"}}, {"vals": {"58121": "Normal"}}, {"vals": {"59779": "450 - Record change auditing"}}, {"vals": {"36042": "Scott Hopwood"}}, {}]
        }, {
            "eid": 51069,
            "values": [{"val": "455"}, {"val": "add a CFD for high priority items"}, {"vals": {"81767": "Not Started"}}, {"vals": {"58121": "Normal"}}, {"vals": {"35877": "454 - Replace TFS for non-bug items"}}, {"vals": {"74150": "Stephen Gibbon"}}, {}]
        }, {
            "eid": 51314,
            "values": [{"val": "634"}, {"val": "Add policy for audit policies"}, {"vals": {"81767": "Not Started"}}, {"vals": {"58121": "Normal"}}, {"vals": {"59779": "450 - Record change auditing"}}, {}, {}]
        }, {
            "eid": 53261,
            "values": [{"val": "704"}, {"val": "Dev: make configuration name field visible on UI so that user can pick up a name by self"}, {"vals": {"81767": "Not Started"}}, {"vals": {"58121": "Normal"}}, {"vals": {"64590": "528 - Connector - CSV/Excel import via API"}}, {"vals": {"49754": "Peter Aylett"}}, {}]
        }, {
            "eid": 53621,
            "values": [{"val": "539"}, {"val": "RT drivers for inline relationship"}, {"vals": {"81767": "Not Started"}}, {"vals": {"58121": "Normal"}}, {"vals": {"30675": "447 - Inline relationship that can pick multiple values"}}, {"vals": {"53443": "Anurag Sharma"}}, {}]
        }, {
            "eid": 53698,
            "values": [{"val": "490"}, {"val": "Install and Setup CAST on AWS"}, {"vals": {"81767": "Not Started"}}, {"vals": {"58121": "Normal"}}, {"vals": {"75043": "445 - Get CAST working on Production"}}, {"vals": {"84630": "Brad Stevens"}}, {}]
        }, {
            "eid": 54199,
            "values": [{"val": "633"}, {"val": "Add Policy report"}, {"vals": {"81767": "Not Started"}}, {"vals": {"58121": "Normal"}}, {"vals": {"59779": "450 - Record change auditing"}}, {}, {}]
        }, {
            "eid": 57156,
            "values": [{"val": "645"}, {"val": "QA: functional testing for non-definition types"}, {"vals": {"81767": "Not Started"}}, {"vals": {"58121": "Normal"}}, {"vals": {"59779": "450 - Record change auditing"}}, {"vals": {"72599": "Shaofen Ning"}}, {}]
        }, {
            "eid": 57828,
            "values": [{"val": "583"}, {"val": "Dev: Support zip files?"}, {"vals": {"33275": "Blocked"}}, {"vals": {"58121": "Normal"}}, {"vals": {"64590": "528 - Connector - CSV/Excel import via API"}}, {"vals": {"49754": "Peter Aylett"}}, {}]
        }, {
            "eid": 60126,
            "values": [{"val": "573"}, {"val": "Dev: Integrate into WebAPIs"}, {"vals": {"33275": "Blocked"}}, {"vals": {"58121": "Normal"}}, {"vals": {"64590": "528 - Connector - CSV/Excel import via API"}}, {"vals": {"49754": "Peter Aylett"}}, {}]
        }, {
            "eid": 60242,
            "values": [{"val": "712"}, {"val": "Bug: Some runs has 0 succeed, and 0 fails, which doesn't make sense."}, {"vals": {"33275": "Blocked"}}, {"vals": {"58121": "Normal"}}, {"vals": {"64590": "528 - Connector - CSV/Excel import via API"}}, {"vals": {"49754": "Peter Aylett"}}, {}]
        }, {
            "eid": 60888,
            "values": [{"val": "492"}, {"val": "Write Unit Tests"}, {"vals": {"83984": "In Progress"}}, {"vals": {"58121": "Normal"}}, {"vals": {"75043": "445 - Get CAST working on Production"}}, {"vals": {"56745": "Alex Engelhardt"}}, {}]
        }, {
            "eid": 61116,
            "values": [{"val": "632"}, {"val": "Add Report of log entries"}, {"vals": {"81767": "Not Started"}}, {"vals": {"58121": "Normal"}}, {"vals": {"59779": "450 - Record change auditing"}}, {}, {}]
        }, {
            "eid": 61839,
            "values": [{"val": "722"}, {"val": "RT Drivers"}, {"vals": {"81767": "Not Started"}}, {"vals": {"58121": "Normal"}}, {"vals": {"70272": "449 - Survey: Reconfigure Campaign Behaviour"}}, {}, {}]
        }, {
            "eid": 62249,
            "values": [{"val": "646"}, {"val": "QA: testing purging and archiving of audit logs"}, {"vals": {"81767": "Not Started"}}, {"vals": {"58121": "Normal"}}, {"vals": {"59779": "450 - Record change auditing"}}, {"vals": {"72599": "Shaofen Ning"}}, {}]
        }, {
            "eid": 62572,
            "values": [{"val": "643"}, {"val": "R2.2.3 Audit Identifier"}, {"vals": {"81767": "Not Started"}}, {"vals": {"58121": "Normal"}}, {"vals": {"59779": "450 - Record change auditing"}}, {}, {}]
        }, {
            "eid": 62862,
            "values": [{"val": "559"}, {"val": "RT tests"}, {"vals": {"81767": "Not Started"}}, {"vals": {"58121": "Normal"}}, {"vals": {"59779": "450 - Record change auditing"}}, {}, {}]
        }, {
            "eid": 63306,
            "values": [{"val": "691"}, {"val": "QA Check product for new red in the browser console logs"}, {"vals": {"81767": "Not Started"}}, {"vals": {"58121": "Normal"}}, {"vals": {"54498": "625 - update lodash lb in the client"}}, {}, {}]
        }, {
            "eid": 63885,
            "values": [{"val": "686"}, {"val": "Three RT to be done, one for each bug."}, {"vals": {"81767": "Not Started"}}, {"vals": {"58121": "Normal"}}, {}, {"vals": {"72599": "Shaofen Ning"}}, {}]
        }, {
            "eid": 64757,
            "values": [{"val": "630"}, {"val": "QA: check unit test in other modules"}, {"vals": {"81767": "Not Started"}}, {"vals": {"58121": "Normal"}}, {"vals": {"64590": "528 - Connector - CSV/Excel import via API"}}, {"vals": {"72599": "Shaofen Ning"}}, {}]
        }, {
            "eid": 66635,
            "values": [{"val": "713"}, {"val": "Bug: Investigate issues importing relationships"}, {"vals": {"81767": "Not Started"}}, {"vals": {"58121": "Normal"}}, {"vals": {"64590": "528 - Connector - CSV/Excel import via API"}}, {"vals": {"49754": "Peter Aylett"}}, {}]
        }, {
            "eid": 67431,
            "values": [{"val": "710"}, {"val": "QA: testing import configuration"}, {"vals": {"83984": "In Progress"}}, {"vals": {"58121": "Normal"}}, {"vals": {"64590": "528 - Connector - CSV/Excel import via API"}}, {"vals": {"72599": "Shaofen Ning"}}, {}]
        }, {
            "eid": 67984,
            "values": [{"val": "585"}, {"val": "QA: Determine tests for CSV format"}, {"vals": {"83984": "In Progress"}}, {"vals": {"58121": "Normal"}}, {"vals": {"64590": "528 - Connector - CSV/Excel import via API"}}, {"vals": {"72599": "Shaofen Ning"}}, {}]
        }, {
            "eid": 69351,
            "values": [{"val": "542"}, {"val": "Fix bugs related to this"}, {"vals": {"81767": "Not Started"}}, {"vals": {"58121": "Normal"}}, {"vals": {"30675": "447 - Inline relationship that can pick multiple values"}}, {}, {}]
        }, {
            "eid": 69675,
            "values": [{"val": "718"}, {"val": "Investigate Failover"}, {"vals": {"81767": "Not Started"}}, {"vals": {"58121": "Normal"}}, {"vals": {"47220": "591 - Implement SQL failover"}}, {"vals": {"84630": "Brad Stevens"}}, {}]
        }, {
            "eid": 72356,
            "values": [{"val": "697"}, {"val": "Dev: Allow configurations to be renamed"}, {"vals": {"33275": "Blocked"}}, {"vals": {"58121": "Normal"}}, {"vals": {"64590": "528 - Connector - CSV/Excel import via API"}}, {"vals": {"49754": "Peter Aylett"}}, {}]
        }, {
            "eid": 74161,
            "values": [{"val": "648"}, {"val": "QA: Usability - testing configuration UI"}, {"vals": {"81767": "Not Started"}}, {"vals": {"58121": "Normal"}}, {"vals": {"59779": "450 - Record change auditing"}}, {"vals": {"72599": "Shaofen Ning"}}, {}]
        }, {
            "eid": 74900,
            "values": [{"val": "666"}, {"val": "test the changes"}, {"vals": {"81767": "Not Started"}}, {"vals": {"58121": "Normal"}}, {"vals": {"33997": "637 - Change mobile UI around tabs..."}}, {}, {}]
        }, {
            "eid": 76147,
            "values": [{"val": "698"}, {"val": "Dev: Remove legacy code"}, {"vals": {"81767": "Not Started"}}, {"vals": {"58121": "Normal"}}, {"vals": {"64590": "528 - Connector - CSV/Excel import via API"}}, {"vals": {"49754": "Peter Aylett"}}, {}]
        }, {
            "eid": 77767,
            "values": [{"val": "731"}, {"val": "Dev: Fix link on admin toolbox page"}, {"vals": {"81767": "Not Started"}}, {"vals": {"58121": "Normal"}}, {}, {"vals": {"49754": "Peter Aylett"}}, {}]
        }, {
            "eid": 79698,
            "values": [{"val": "588"}, {"val": "QA: Write/update RT tests"}, {"vals": {"81767": "Not Started"}}, {"vals": {"58121": "Normal"}}, {"vals": {"64590": "528 - Connector - CSV/Excel import via API"}}, {"vals": {"72599": "Shaofen Ning"}}, {}]
        }, {
            "eid": 80240,
            "values": [{"val": "584"}, {"val": "RT: Create new RT drivers"}, {"vals": {"81767": "Not Started"}}, {"vals": {"58121": "Normal"}}, {"vals": {"64590": "528 - Connector - CSV/Excel import via API"}}, {"vals": {"49754": "Peter Aylett"}}, {}]
        }, {
            "eid": 81253,
            "values": [{"val": "502"}, {"val": "QA on Charts: enhancements to axes/scales"}, {"vals": {"83984": "In Progress"}}, {"vals": {"58121": "Normal"}}, {"vals": {"53754": "473 - Charts: enhancements to axes/scales"}}, {"vals": {"81545": "Karen Jones"}}, {}]
        }, {
            "eid": 81606,
            "values": [{"val": "530"}, {"val": "Automated tests for it"}, {"vals": {"83984": "In Progress"}}, {"vals": {"58121": "Normal"}}, {"vals": {"64590": "528 - Connector - CSV/Excel import via API"}}, {"vals": {"49754": "Peter Aylett"}}, {}]
        }, {
            "eid": 83049,
            "values": [{"val": "721"}, {"val": "QA on Survey"}, {"vals": {"81767": "Not Started"}}, {"vals": {"58121": "Normal"}}, {"vals": {"70272": "449 - Survey: Reconfigure Campaign Behaviour"}}, {}, {}]
        }, {
            "eid": 83700,
            "values": [{"val": "715"}, {"val": "do most pressing ship it/task board tweaks"}, {"vals": {"83984": "In Progress"}}, {"vals": {"58121": "Normal"}}, {"vals": {"35540": "667 - SG misc tasks"}}, {"vals": {"74150": "Stephen Gibbon"}}, {}]
        }, {
            "eid": 84010,
            "values": [{"val": "682"}, {"val": "Dev: make Definition visible on Field picker report so it would be easier for user to pick up a specific field"}, {"vals": {"81767": "Not Started"}}, {"vals": {"58121": "Normal"}}, {"vals": {"59779": "450 - Record change auditing"}}, {"vals": {"36042": "Scott Hopwood"}}, {}]
        }, {
            "eid": 84709,
            "values": [{"val": "683"}, {"val": "Dev: add a new picker report for \"Log entries for object\" and hide unrelated buttons"}, {"vals": {"81767": "Not Started"}}, {"vals": {"58121": "Normal"}}, {"vals": {"59779": "450 - Record change auditing"}}, {"vals": {"36042": "Scott Hopwood"}}, {}]
        }, {
            "eid": 85507,
            "values": [{"val": "558"}, {"val": "RT drivers"}, {"vals": {"81767": "Not Started"}}, {"vals": {"58121": "Normal"}}, {"vals": {"59779": "450 - Record change auditing"}}, {}, {}]
        }, {
            "eid": 86349,
            "values": [{"val": "739"}, {"val": "task 3"}, {"vals": {"81767": "Not Started"}}, {"vals": {"58121": "Normal"}}, {"vals": {"35540": "667 - SG misc tasks"}}, {"vals": {"74150": "Stephen Gibbon"}}, {}]
        }, {
            "eid": 86351,
            "values": [{"val": "740"}, {"val": "another item"}, {"vals": {"81767": "Not Started"}}, {"vals": {"58121": "Normal"}}, {"vals": {"35540": "667 - SG misc tasks"}}, {"vals": {"74150": "Stephen Gibbon"}}, {}]
        }, {
            "eid": 86353,
            "values": [{"val": "741"}, {"val": "and oteher"}, {"vals": {"81767": "Not Started"}}, {"vals": {"58121": "Normal"}}, {"vals": {"35540": "667 - SG misc tasks"}}, {"vals": {"74150": "Stephen Gibbon"}}, {}]
        }, {
            "eid": 86355,
            "values": [{"val": "742"}, {"val": "hlkahds"}, {"vals": {"81767": "Not Started"}}, {"vals": {"58121": "Normal"}}, {"vals": {"69394": "636 - Protect \"out of the box\" application"}}, {}, {}]
        }, {
            "eid": 86357,
            "values": [{"val": "743"}, {"val": "sadfdsafjhlkg"}, {"vals": {"81767": "Not Started"}}, {"vals": {"58121": "Normal"}}, {"vals": {"69394": "636 - Protect \"out of the box\" application"}}, {}, {}]
        }, {
            "eid": 86359,
            "values": [{"val": "744"}, {"val": "lkhsadfl"}, {"vals": {"81767": "Not Started"}}, {"vals": {"58121": "Normal"}}, {"vals": {"69394": "636 - Protect \"out of the box\" application"}}, {}, {}]
        }, {
            "eid": 86361,
            "values": [{"val": "745"}, {"val": "dfgdfgdf"}, {"vals": {"81767": "Not Started"}}, {"vals": {"58121": "Normal"}}, {"vals": {"69394": "636 - Protect \"out of the box\" application"}}, {}, {}]
        }, {
            "eid": 86363,
            "values": [{"val": "746"}, {"val": "klhsdlkfhsda"}, {"vals": {"83984": "In Progress"}}, {"vals": {"58121": "Normal"}}, {"vals": {"69394": "636 - Protect \"out of the box\" application"}}, {}, {}]
        }]
    };

})(rnBoardTestData || (rnBoardTestData = {}));
