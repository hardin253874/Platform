// Copyright 2011-2016 Global Software Innovation Pty Ltd
// JavaScript source code
//https://syd1dev16.entdata.local/spapi/data/v1/entity?id%5B%5D=core%3Aname&id%5B%5D=core%3Adescription&request%5B%5D=isOfType.id%2C+isRequired%2C+allowMultiLines%2C+pattern.%7Bregex%2C+regexDescription%7D%2C+minLength%2C+maxLength
var fieldTestData;
(function (fieldTestData) {
    // This resource contains all field information
    fieldTestData.fields =
{
    "ids": [7805, 9451],
    "entities": [
        {
            "id": 7805,
            "typeIds": [8637],
            "fields": [
                {
                    "fieldId": 9837,
                    "value": false,
                    "typeName": "Bool"
                },
                {
                    "fieldId": 9882,
                    "value": null,
                    "typeName": "Bool"
                },
                {
                    "fieldId": 10237,
                    "value": 0,
                    "typeName": "Int32"
                },
                {
                    "fieldId": 11186,
                    "value": 200,
                    "typeName": "Int32"
                }],
            "relationships": [
                {
                    "relTypeId": {
                        "id": 10922,
                        "alias": "isOfType",
                        "ns": "core"
                    },
                    "instances": [
                        {
                            "entity": 8637,
                            "relEntity": 0,
                            "dataState": 0
                        }],
                    "isReverse": false,
                    "isLookup": false,
                    "removeExisting": false
                },
                {
                    "relTypeId": {
                        "id": 11121,
                        "alias": "pattern",
                        "ns": "core"
                    },
                    "instances": [
                        {
                            "entity": 9938,
                            "relEntity": 8550,
                            "dataState": 0
                        }],
                    "isReverse": false,
                    "isLookup": true,
                    "removeExisting": false
                }],
            "dataState": 0
        },
        {
            "id": 8637,
            "typeIds": [7798],
            "fields": [],
            "relationships": [],
            "dataState": 0
        },
        {
            "id": 9938,
            "typeIds": [9871],
            "fields": [
                {
                    "fieldId": 9710,
                    "value": "^[^<>]+$",
                    "typeName": "String"
                },
                {
                    "fieldId": 9683,
                    "value": "The field cannot contain angled brackets.",
                    "typeName": "String"
                }],
            "relationships": [],
            "dataState": 0
        },
        {
            "id": 8550,
            "typeIds": [],
            "fields": [],
            "relationships": [],
            "dataState": 0
        },
        {
            "id": 9451,
            "typeIds": [8637],
            "fields": [
                {
                    "fieldId": 9837,
                    "value": false,
                    "typeName": "Bool"
                },
                {
                    "fieldId": 9882,
                    "value": true,
                    "typeName": "Bool"
                },
                {
                    "fieldId": 10237,
                    "value": 0,
                    "typeName": "Int32"
                },
                {
                    "fieldId": 11186,
                    "value": 10000,
                    "typeName": "Int32"
                }],
            "relationships": [
                {
                    "relTypeId": {
                        "id": 10922,
                        "alias": "isOfType",
                        "ns": "core"
                    },
                    "instances": [
                        {
                            "entity": 8637,
                            "relEntity": 0,
                            "dataState": 0
                        }],
                    "isReverse": false,
                    "isLookup": false,
                    "removeExisting": false
                },
                {
                    "relTypeId": {
                        "id": 11121,
                        "alias": "pattern",
                        "ns": "core"
                    },
                    "instances": [],
                    "isReverse": false,
                    "isLookup": true,
                    "removeExisting": false
                }],
            "dataState": 0
        }],
    "entityRefs": [
        {
            "id": 7805,
            "alias": "name",
            "ns": "core"
        },
        {
            "id": 8637,
            "alias": "stringField",
            "ns": "core"
        },
        {
            "id": 9837,
            "alias": "isRequired",
            "ns": "core"
        },
        {
            "id": 9882,
            "alias": "allowMultiLines",
            "ns": "core"
        },
        {
            "id": 10237,
            "alias": "minLength",
            "ns": "core"
        },
        {
            "id": 11186,
            "alias": "maxLength",
            "ns": "core"
        },
        {
            "id": 7798,
            "alias": "fieldType",
            "ns": "core"
        },
        {
            "id": 9938,
            "alias": "namePattern",
            "ns": "core"
        },
        {
            "id": 9871,
            "alias": "stringPattern",
            "ns": "core"
        },
        {
            "id": 9710,
            "alias": "regex",
            "ns": "core"
        },
        {
            "id": 9683,
            "alias": "regexDescription",
            "ns": "core"
        },
        {
            "id": 8550,
            "alias": null,
            "ns": null
        },
        {
            "id": 9451,
            "alias": "description",
            "ns": "core"
        }],
    "extra": "15",
    "extra2": "3"
};
})(fieldTestData || (fieldTestData = {}));