// Copyright 2011-2016 Global Software Innovation Pty Ltd
/* globals spEntity */

var entityTestData;
(function (entityTestData) {    
    entityTestData.thumbnailSizesTestData =
    {
        "ids": [
            12198
        ],
        "entities": [
            {
                "id": 12198,
                "typeIds": [
                    7688
                ],
                "fields": [],
                "relationships": [
                    {
                        "relTypeId": {
                            "id": 10763,
                            "alias": "instancesOfType",
                            "ns": "core"
                        },
                        "instances": [
                            {
                                "entity": 12160,
                                "relEntity": 12734,
                                "dataState": 0
                            },
                            {
                                "entity": 12522,
                                "relEntity": 11350,
                                "dataState": 0
                            },
                            {
                                "entity": 13732,
                                "relEntity": 13822,
                                "dataState": 0
                            }
                        ],
                        "isReverse": false,
                        "removeExisting": false
                    }
                ],
                "dataState": 0
            },
            {
                "id": 12160,
                "typeIds": [
                    12198
                ],
                "fields": [
                    {
                        "fieldId": 8728,
                        "value": "console:smallThumbnail",
                        "typeName": "String"
                    },
                    {
                        "fieldId": 7629,
                        "value": "Small",
                        "typeName": "String"
                    },
                    {
                        "fieldId": 9303,
                        "value": "150 x 150 (pixels)",
                        "typeName": "String"
                    },
                    {
                        "fieldId": 8631,
                        "value": 1,
                        "typeName": "Int32"
                    }
                ],
                "relationships": [],
                "dataState": 0
            },
            {
                "id": 12734,
                "typeIds": [],
                "fields": [],
                "relationships": [],
                "dataState": 0
            },
            {
                "id": 12522,
                "typeIds": [
                    12198
                ],
                "fields": [
                    {
                        "fieldId": 8728,
                        "value": "console:largeThumbnail",
                        "typeName": "String"
                    },
                    {
                        "fieldId": 7629,
                        "value": "Large",
                        "typeName": "String"
                    },
                    {
                        "fieldId": 9303,
                        "value": "300 x 300 (pixels)",
                        "typeName": "String"
                    },
                    {
                        "fieldId": 8631,
                        "value": 2,
                        "typeName": "Int32"
                    }
                ],
                "relationships": [],
                "dataState": 0
            },
            {
                "id": 11350,
                "typeIds": [],
                "fields": [],
                "relationships": [],
                "dataState": 0
            },
            {
                "id": 13732,
                "typeIds": [
                    12198
                ],
                "fields": [
                    {
                        "fieldId": 8728,
                        "value": "console:iconThumbnailSize",
                        "typeName": "String"
                    },
                    {
                        "fieldId": 7629,
                        "value": "Icon",
                        "typeName": "String"
                    },
                    {
                        "fieldId": 9303,
                        "value": "16 x 16 (pixels)",
                        "typeName": "String"
                    },
                    {
                        "fieldId": 8631,
                        "value": 3,
                        "typeName": "Int32"
                    }
                ],
                "relationships": [],
                "dataState": 0
            },
            {
                "id": 13822,
                "typeIds": [],
                "fields": [],
                "relationships": [],
                "dataState": 0
            }
        ],
        "entityRefs": [
            {
                "id": 12198,
                "alias": "thumbnailSizeEnum",
                "ns": "console"
            },
            {
                "id": 7688,
                "alias": "enumType",
                "ns": "core"
            },
            {
                "id": 12160,
                "alias": "smallThumbnail",
                "ns": "console"
            },
            {
                "id": 8728,
                "alias": "alias",
                "ns": "core"
            },
            {
                "id": 7629,
                "alias": "name",
                "ns": "core"
            },
            {
                "id": 9303,
                "alias": "description",
                "ns": "core"
            },
            {
                "id": 8631,
                "alias": "enumOrder",
                "ns": "core"
            },
            {
                "id": 12734,
                "alias": null,
                "ns": null
            },
            {
                "id": 12522,
                "alias": "largeThumbnail",
                "ns": "console"
            },
            {
                "id": 11350,
                "alias": null,
                "ns": null
            },
            {
                "id": 13732,
                "alias": "iconThumbnailSize",
                "ns": "console"
            },
            {
                "id": 13822,
                "alias": null,
                "ns": null
            }
        ]
    };
    entityTestData.thumbnailSizesTestEnumData = spEntity.fromJSON(
        [{
            dataState: 0,
            id: { id: 12160, ns: 'console', alias: 'smallThumbnail' },
            typeId:'console:thumbnailSizeEnum',
            name: 'Small',
            description:'150 x 150 (pixels)',
            enumOrder:1
        },
        {
            dataState: 0,
            id: { id: 12522, ns: 'console', alias: 'largeThumbnail' },
            typeId: 'console:thumbnailSizeEnum',
            name: 'Large',
            description: '300 x 300 (pixels)',
            enumOrder:2
        },
        {
            dataState: 0,
            id: { id: 13732, ns: 'console', alias: 'iconThumbnailSize' },
            typeId: 'console:thumbnailSizeEnum',
            name: 'Icon',
            description: '16 x 16 (pixels)',
            enumOrder:3

        }]);
    
})(entityTestData || (entityTestData = {}));