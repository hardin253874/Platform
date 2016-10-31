// Copyright 2011-2016 Global Software Innovation Pty Ltd
angular.module('mockedEntityInfo', [])
    .value('entityInfoResponses', [
        {
            request: /spapi\/data\/v1\/entity\/core\/definition[\w\W]*alias/,
            response: {
                ids: [1000],
                entities: [
                    {
                        id: 1000, typeIds: [100],
                        fields: [
                            { fieldId: 8610, typeName: "string", value: 'definition' }
                        ],
                        relationships: []
                    }
                ],
                entityRefs: [
                    { id: 100, ns: 'core', alias: 'type' },
                    { id: 1000, ns: 'core', alias: 'activityType' },
                    { id: 8610, ns: "core", alias: "alias" }
                ]
            }
        }
    ]);
