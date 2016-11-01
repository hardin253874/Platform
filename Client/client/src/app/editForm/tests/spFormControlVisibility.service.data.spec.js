// Copyright 2011-2016 Global Software Innovation Pty Ltd
/* globals spEntity */

var spFormControlVisibilityServiceTestData;
(function (spFormControlVisibilityServiceTestData) {
    spFormControlVisibilityServiceTestData.nameFieldId = 1000;
    spFormControlVisibilityServiceTestData.descriptionFieldId = 2000;
    spFormControlVisibilityServiceTestData.lookupFieldId = 3000;
    spFormControlVisibilityServiceTestData.nameControlId = 5000;
    spFormControlVisibilityServiceTestData.descriptionControlId = 6000;

    spFormControlVisibilityServiceTestData.formData = spEntity.fromJSON({
        "id": 50290,
        "_dataState": "unchanged",
        "1000": "TestObj",
        "2000": "TestObj Description",
        "3000": jsonLookup(10000)
    });

    spFormControlVisibilityServiceTestData.formWithNameDescInContainer = spEntity.fromJSON({
        "id": 30428,
        "_dataState": "unchanged",
        "name": "TestObj Form",
        "cacheChangeMarker": "cb563f13-a741-4be7-9373-6e5b5730dd9c",
        "console:showFormHelpText": false,
        "isOfType": [
            {
                "id": "console:customEditForm",
                "_dataState": "unchanged",
                "name": "Custom Edit Form",
                "alias": "console:customEditForm",
                "console:typeConsoleBehavior": {
                    "id": 4995,
                    "_dataState": "unchanged",
                    "console:treeIcon": {
                        "id": 7560,
                        "_dataState": "unchanged",
                        "name": "form_default.svg",
                        "imageBackgroundColor": "ff0173c7"
                    }
                }
            }
        ],
        "console:typeToEditWithForm": {
            "id": 30413,
            "_dataState": "unchanged",
            "name": "ShowHideObj",
            "canCreateType": true,
            "console:typeConsoleBehavior": {
                "id": 30412,
                "_dataState": "unchanged"
            }
        },
        "console:containedControlsOnForm": [
            {
                "id": 30427,
                "_dataState": "unchanged",
                "name": "New Container",
                "console:renderingBackgroundColor": "white",
                "console:hideLabel": false,
                "console:renderingHeight": 50,
                "console:renderingWidth": 100,
                "console:renderingOrdinal": 0,
                "isOfType": [
                    {
                        "id": "console:verticalStackContainerControl",
                        "_dataState": "unchanged",
                        "name": "Vertical Stack Container",
                        "alias": "console:verticalStackContainerControl",
                        "console:minWidthMobile": 300,
                        "console:minWidth": 370,
                        "console:minHeightTablet": 50,
                        "console:minHeightMobile": 50,
                        "console:minWidthTablet": 370,
                        "console:minHeight": 50
                    }
                ],
                "console:relationshipControlFilters": [],
                "console:containedControlsOnForm": [
                    {
                        "id": 6000,
                        "_dataState": "unchanged",
                        "console:renderingBackgroundColor": "white",                        
                        "console:renderingOrdinal": 1,
                        "isOfType": [
                            {
                                "id": "console:singleLineTextControl",
                                "_dataState": "unchanged",
                                "name": "Text box",
                                "alias": "console:singleLineTextControl",                                
                                "console:minWidthMobile": 280,
                                "console:minWidth": 340,
                                "console:minHeightTablet": 71,
                                "console:minHeightMobile": 71,
                                "console:minWidthTablet": 340,
                                "console:minHeight": 30
                            }
                        ],
                        "console:relationshipControlFilters": [],
                        "console:fieldToRender": {
                            "id": "core:description",
                            "_dataState": "unchanged",                            
                            "name": "Description",
                            "isFieldReadOnly": false,
                            "alias": "core:description",                            
                            "description": "The 'description' field of every resource.",
                            "isRequired": false,
                            "allowMultiLines": true,                            
                            "minLength": 0,                            
                            "maxLength": 10000,
                            "isOfType": [
                                {
                                    "id": "core:stringField",
                                    "_dataState": "unchanged",
                                    "alias": "core:stringField"
                                }
                            ]
                        },
                        "console:containedControlsOnForm": [],
                        "console:sendContextTo": [],
                        "console:resourceInFolder": [],
                        "console:controlRelatedEntityDataPathNodes": [],
                        "console:renderingVerticalResizeMode": {
                            "id": "console:resizeAutomatic",
                            "_dataState": "unchanged",
                            "alias": "console:resizeAutomatic"
                        },
                        "console:renderingHorizontalResizeMode": {
                            "id": "console:resizeAutomatic",
                            "_dataState": "unchanged"                            
                        }
                    },
                    {
                        "id": 5000,
                        "_dataState": "unchanged",                        
                        "console:renderingBackgroundColor": "white",                        
                        "console:renderingOrdinal": 0,
                        "isOfType": [
                            {
                                "id": "console:singleLineTextControl",
                                "_dataState": "unchanged"                                
                            }
                        ],
                        "console:relationshipControlFilters": [],
                        "console:fieldToRender": {
                            "id": "core:name",
                            "_dataState": "unchanged",                            
                            "name": "Name",
                            "isFieldReadOnly": false,
                            "alias": "core:name",                            
                            "description": "The 'name' field of every resource.",
                            "isRequired": false,                            
                            "minLength": 0,                            
                            "maxLength": 200,
                            "isOfType": [
                                {
                                    "id": "core:stringField",
                                    "_dataState": "unchanged"                                    
                                }
                            ],
                            "pattern": {
                                "id": 4159,
                                "_dataState": "unchanged",
                                "regexDescription": "The value must not contain angled brackets.",
                                "regex": "^[^<>]+$"
                            }
                        },
                        "console:containedControlsOnForm": [],
                        "console:sendContextTo": [],
                        "console:resourceInFolder": [],
                        "console:controlRelatedEntityDataPathNodes": [],
                        "console:renderingVerticalResizeMode": {
                            "id": "console:resizeAutomatic",
                            "_dataState": "unchanged"                            
                        },
                        "console:renderingHorizontalResizeMode": {
                            "id": "console:resizeAutomatic",
                            "_dataState": "unchanged"                            
                        }
                    }
                ],
                "console:sendContextTo": [],
                "console:resourceInFolder": [],
                "console:controlRelatedEntityDataPathNodes": [],
                "console:renderingVerticalResizeMode": {
                    "id": "console:resizeFifty",
                    "_dataState": "unchanged",
                    "alias": "console:resizeFifty"
                },
                "console:renderingHorizontalResizeMode": {
                    "id": "console:resizeSpring",
                    "_dataState": "unchanged",
                    "alias": "console:resizeSpring"
                }
            }
        ],
        "console:sendContextTo": [],
        "console:resourceInFolder": [],
        "console:renderingVerticalResizeMode": {
            "id": "console:resizeSpring",
            "_dataState": "unchanged"            
        },
        "console:renderingHorizontalResizeMode": {
            "id": "console:resizeSpring",
            "_dataState": "unchanged"            
        }
    });

    spFormControlVisibilityServiceTestData.formWithCalculations = spEntity.fromJSON({
        "id": 30290,
        "_dataState": "unchanged",
        "name": "TestObj Form",        
        "cacheChangeMarker": "3d7e114c-9539-4fd7-bfd0-102b64c32c98",                
        "console:showFormHelpText": false,                
        "inSolution": {
            "id": 15635,
            "_dataState": "unchanged",
            "name": "Test Solution"
        },
        "isOfType": [
            {
                "id": "console:customEditForm",
                "_dataState": "unchanged",
                "name": "Custom Edit Form",
                "alias": "console:customEditForm",
                "console:typeConsoleBehavior": {
                    "id": 4995,
                    "_dataState": "unchanged",
                    "console:treeIcon": {
                        "id": 7560,
                        "_dataState": "unchanged",
                        "name": "form_default.svg",
                        "imageBackgroundColor": "ff0173c7"
                    }
                }
            }
        ],
        "console:typeToEditWithForm": {
            "id": 30289,
            "_dataState": "unchanged",
            "name": "TestObj",
            "canCreateType": true,
            "console:typeConsoleBehavior": {
                "id": 30288,
                "_dataState": "unchanged"
            }
        },
        "console:containedControlsOnForm": [
            {
                "id": 6000,
                "_dataState": "unchanged",                
                "console:renderingBackgroundColor": "rgba(255,255,255,1)",                
                "console:visibilityCalculation": "[Name] = 'name test'",
                "console:showControlHelpText": false,
                "console:readOnlyControl": false,                
                "console:mandatoryControl": false,                
                "console:renderingOrdinal": 1,
                "isOfType": [
                    {
                        "id": "console:singleLineTextControl",
                        "_dataState": "unchanged",
                        "name": "Text box",
                        "alias": "console:singleLineTextControl",                        
                        "console:minWidthMobile": 280,
                        "console:minWidth": 340,
                        "console:minHeightTablet": 71,
                        "console:minHeightMobile": 71,
                        "console:minWidthTablet": 340,
                        "console:minHeight": 30
                    }
                ],
                "console:relationshipControlFilters": [],
                "console:fieldToRender": {
                    "id": "core:description",
                    "_dataState": "unchanged",                    
                    "name": "Description",
                    "isFieldReadOnly": false,
                    "alias": "core:description",
                    "description": "The 'description' field of every resource.",
                    "isRequired": false,
                    "allowMultiLines": true,
                    "minLength": 0,                                        
                    "maxLength": 10000,
                    "isOfType": [
                        {
                            "id": "core:stringField",
                            "_dataState": "unchanged",
                            "alias": "core:stringField"
                        }
                    ]
                },
                "console:containedControlsOnForm": [],
                "console:sendContextTo": [],
                "console:resourceInFolder": [],
                "console:controlRelatedEntityDataPathNodes": [],
                "console:renderingVerticalResizeMode": {
                    "id": "console:resizeAutomatic",
                    "_dataState": "unchanged",
                    "alias": "console:resizeAutomatic"
                },
                "console:renderingHorizontalResizeMode": {
                    "id": "console:resizeAutomatic",
                    "_dataState": "unchanged"                    
                }
            },
            {
                "id": 5000,
                "_dataState": "unchanged",                
                "console:renderingBackgroundColor": "rgba(255,255,255,1)",                
                "console:visibilityCalculation": "[Description] = 'desc test'",
                "console:showControlHelpText": false,
                "console:readOnlyControl": false,                
                "console:mandatoryControl": false,                
                "console:renderingOrdinal": 0,
                "isOfType": [
                    {
                        "id": "console:singleLineTextControl",
                        "_dataState": "unchanged"                        
                    }
                ],
                "console:relationshipControlFilters": [],
                "console:fieldToRender": {
                    "id": "core:name",
                    "_dataState": "unchanged",                    
                    "name": "Name",
                    "isFieldReadOnly": false,
                    "alias": "core:name",
                    "description": "The 'name' field of every resource.",
                    "isRequired": false,                    
                    "minLength": 0,                    
                    "maxLength": 200,
                    "isOfType": [
                        {
                            "id": "core:stringField",
                            "_dataState": "unchanged"                            
                        }
                    ],
                    "pattern": {
                        "id": 4159,
                        "_dataState": "unchanged",
                        "regexDescription": "The value must not contain angled brackets.",
                        "regex": "^[^<>]+$"
                    }
                },
                "console:containedControlsOnForm": [],
                "console:sendContextTo": [],
                "console:resourceInFolder": [],
                "console:controlRelatedEntityDataPathNodes": [],
                "console:renderingVerticalResizeMode": {
                    "id": "console:resizeAutomatic",
                    "_dataState": "unchanged"                    
                },
                "console:renderingHorizontalResizeMode": {
                    "id": "console:resizeAutomatic",
                    "_dataState": "unchanged"                    
                }
            }
        ],
        "console:sendContextTo": [],
        "console:resourceInFolder": [],
        "console:renderingVerticalResizeMode": {
            "id": "console:resizeSpring",
            "_dataState": "unchanged",
            "alias": "console:resizeSpring"
        },
        "console:renderingHorizontalResizeMode": {
            "id": "console:resizeSpring",
            "_dataState": "unchanged"            
        }
    });

    spFormControlVisibilityServiceTestData.formWithNoCalculations = spEntity.fromJSON({
        "id": 30290,
        "_dataState": "unchanged",
        "name": "TestObj Form",        
        "cacheChangeMarker": "3d7e114c-9539-4fd7-bfd0-102b64c32c98",                
        "console:showFormHelpText": false,                
        "inSolution": {
            "id": 15635,
            "_dataState": "unchanged",
            "name": "Test Solution"
        },
        "isOfType": [
            {
                "id": "console:customEditForm",
                "_dataState": "unchanged",
                "name": "Custom Edit Form",
                "alias": "console:customEditForm",
                "console:typeConsoleBehavior": {
                    "id": 4995,
                    "_dataState": "unchanged",
                    "console:treeIcon": {
                        "id": 7560,
                        "_dataState": "unchanged",
                        "name": "form_default.svg",
                        "imageBackgroundColor": "ff0173c7"
                    }
                }
            }
        ],
        "console:typeToEditWithForm": {
            "id": 30289,
            "_dataState": "unchanged",
            "name": "TestObj",
            "canCreateType": true,
            "console:typeConsoleBehavior": {
                "id": 30288,
                "_dataState": "unchanged"
            }
        },
        "console:containedControlsOnForm": [
            {
                "id": 6000,
                "_dataState": "unchanged",                
                "console:renderingBackgroundColor": "rgba(255,255,255,1)",                                
                "console:showControlHelpText": false,
                "console:readOnlyControl": false,                
                "console:mandatoryControl": false,                
                "console:renderingOrdinal": 1,
                "isOfType": [
                    {
                        "id": "console:singleLineTextControl",
                        "_dataState": "unchanged",
                        "name": "Text box",
                        "alias": "console:singleLineTextControl",                        
                        "console:minWidthMobile": 280,
                        "console:minWidth": 340,
                        "console:minHeightTablet": 71,
                        "console:minHeightMobile": 71,
                        "console:minWidthTablet": 340,
                        "console:minHeight": 30
                    }
                ],
                "console:relationshipControlFilters": [],
                "console:fieldToRender": {
                    "id": "core:description",
                    "_dataState": "unchanged",                    
                    "name": "Description",
                    "isFieldReadOnly": false,
                    "alias": "core:description",
                    "description": "The 'description' field of every resource.",
                    "isRequired": false,
                    "allowMultiLines": true,
                    "minLength": 0,                                        
                    "maxLength": 10000,
                    "isOfType": [
                        {
                            "id": "core:stringField",
                            "_dataState": "unchanged",
                            "alias": "core:stringField"
                        }
                    ]
                },
                "console:containedControlsOnForm": [],
                "console:sendContextTo": [],
                "console:resourceInFolder": [],
                "console:controlRelatedEntityDataPathNodes": [],
                "console:renderingVerticalResizeMode": {
                    "id": "console:resizeAutomatic",
                    "_dataState": "unchanged",
                    "alias": "console:resizeAutomatic"
                },
                "console:renderingHorizontalResizeMode": {
                    "id": "console:resizeAutomatic",
                    "_dataState": "unchanged"                    
                }
            },
            {
                "id": 5000,
                "_dataState": "unchanged",                
                "console:renderingBackgroundColor": "rgba(255,255,255,1)",                                
                "console:showControlHelpText": false,
                "console:readOnlyControl": false,                
                "console:mandatoryControl": false,                
                "console:renderingOrdinal": 0,
                "isOfType": [
                    {
                        "id": "console:singleLineTextControl",
                        "_dataState": "unchanged"                        
                    }
                ],
                "console:relationshipControlFilters": [],
                "console:fieldToRender": {
                    "id": "core:name",
                    "_dataState": "unchanged",                    
                    "name": "Name",
                    "isFieldReadOnly": false,
                    "alias": "core:name",
                    "description": "The 'name' field of every resource.",
                    "isRequired": false,                    
                    "minLength": 0,                    
                    "maxLength": 200,
                    "isOfType": [
                        {
                            "id": "core:stringField",
                            "_dataState": "unchanged"                            
                        }
                    ],
                    "pattern": {
                        "id": 4159,
                        "_dataState": "unchanged",
                        "regexDescription": "The value must not contain angled brackets.",
                        "regex": "^[^<>]+$"
                    }
                },
                "console:containedControlsOnForm": [],
                "console:sendContextTo": [],
                "console:resourceInFolder": [],
                "console:controlRelatedEntityDataPathNodes": [],
                "console:renderingVerticalResizeMode": {
                    "id": "console:resizeAutomatic",
                    "_dataState": "unchanged"                    
                },
                "console:renderingHorizontalResizeMode": {
                    "id": "console:resizeAutomatic",
                    "_dataState": "unchanged"                    
                }
            }
        ],
        "console:sendContextTo": [],
        "console:resourceInFolder": [],
        "console:renderingVerticalResizeMode": {
            "id": "console:resizeSpring",
            "_dataState": "unchanged",
            "alias": "console:resizeSpring"
        },
        "console:renderingHorizontalResizeMode": {
            "id": "console:resizeSpring",
            "_dataState": "unchanged"            
        }
    });

})(spFormControlVisibilityServiceTestData || (spFormControlVisibilityServiceTestData = {}));