// Copyright 2011-2016 Global Software Innovation Pty Ltd
(function () {
    'use strict';

    angular.module('mod.app.connector.spResourceEndpointService', [
        'ui.router',
        'mod.common.spEntityService',
        'mod.common.spResource',
        'mod.common.spWebService'
    ])
    .service('spResourceEndpointService', ResourceEndpointService);

    function ResourceEndpointService($q, spEntityService, spResource, spWebService, $stateParams, $sce) {

        function createEmptyModel(devMode) {
            return {
                endpoint: null, // entity
                endpointId: 0,
                typeMembers: [],    // all available members on selected type
                displayMode: 'edit',
                addressPrefix: '',
                apiEndpointAddress: '',
                objectPickerOptions: {
                    selectedEntityId: null,
                    selectedEntity: null,
                    selectedEntities: [],
                    pickerReportId: 'console:userTypesReport',
                    entityTypeId: devMode ? 'core:type' : 'core:definition',
                    multiSelect: false,
                    isDisabled: false,
                    reportOptions: {}
                },
                sample: {
                    verb: 'POST'
                }
            };
        }

        function createModel(endpointId, parentApiId, devMode) {
            var model = createEmptyModel(devMode);
            model.endpointId = endpointId;

            var promise;
            if (endpointId === 0) {
                if (parentApiId) {
                    promise = createEndpoint(model, parentApiId);
                } else {
                    promise = $q.when();
                }
            } else {
                promise = loadEndpoint(model, endpointId);
            }
            return promise.then(function () {
                var type = sp.result(model, 'endpoint.endpointResourceMapping.mappedType');
                model.objectPickerOptions.selectedEntities = type ? [type] : [];
                return model;
            });
        }        

        function createEndpoint(model, parentApiId) {
            var json = {
                name: 'New Endpoint',
                typeId: 'apiResourceEndpoint',
                endpointForApi: jsonLookup(parentApiId),
                endpointResourceMapping: jsonLookup(),
                mappedType: jsonLookup(),
                apiEndpointAddress: jsonString(),
                apiEndpointEnabled: false,
                endpointCanCreate: true,
                endpointCanUpdate: true,
                endpointCanDelete: true
            };

            var endpoint = spEntity.fromJSON(json);
            verifyStructure(endpoint);

            model.endpoint = endpoint;

            return spEntityService.getEntity(parentApiId, 'apiAddress').then(function (api)
            {
                model.addressPrefix = makeAddressPrefix(api);
                updateSampleVerb(model);
            });
        }

        function loadEndpoint(model, endpointId) {
            var rq = 'let @MEMBERMAPPING = { name, isOfType.alias, mappedField.id, mappedRelationship.id, mapRelationshipInReverse, mappedRelationshipLookupField.id } ' +
                     'let @RESOURCEMAPPING = { mappedType.name, mappingSuppressWorkflows, resourceMemberMappings.@MEMBERMAPPING } ' +
                     'let @API = { apiAddress } ' +
                     'let @ENDPOINT = { name, apiEndpointAddress, apiEndpointEnabled, endpointCanCreate, endpointCanUpdate, endpointCanDelete, endpointResourceMapping.@RESOURCEMAPPING, endpointForApi.@API } ' +
                     '@ENDPOINT';

            return spEntityService.getEntity(endpointId, rq, { hint: 'resourceEndpoint' }).then(function (endpoint) {
                verifyStructure(endpoint);
                model.endpoint = endpoint;
                model.loadedTypeId = sp.result(endpoint, 'endpointResourceMapping.mappedType.id');

                if (endpoint) {
                    model.addressPrefix = makeAddressPrefix(endpoint.endpointForApi);
                }
            });
        }

        function verifyStructure(endpoint) {
            if (!endpoint.endpointResourceMapping) {
                endpoint.endpointResourceMapping = spEntity.fromJSON({
                    typeId: 'apiResourceMapping',
                    mappingSuppressWorkflows: false,
                    resourceMemberMappings: []
                });
            }
        }

        function makeAddressPrefix(api) {
            var apiAddress = (api || {}).apiAddress;
            if (!apiAddress)
                return '';

            var host = spWebService.getWebApiRoot();
            var fullHost = host || ('https://' + window.location.host);
            var tenant = $stateParams.tenant || 'TENANT';

            return fullHost + '/spapi/api/' + tenant + '/' + apiAddress;
        }

        // Make a JSON member name - remove non alphanumeric and spaces
        function makeJsonName(name) {
            if (!name)
                return null;
            var res = name.replace(/[^a-z0-9]/gi, "").toLowerCase();    // global (all), case insensitive
            return res;
        }

        // Make a JSON member name - remove non alphanumeric and spaces
        function makeSafeName(name) {
            if (!name)
                return null;
            var res = name.replace(/[^a-z0-9]/gi, "");    // global (all), case insensitive
            return res;
        }

        function setMemberMappingEntity(member) {
            var jsonId = makeJsonName(member.name);
            member.mappingEntity = createMemberMappingEntity(member.memberInfo, jsonId);

            var memberId = member.memberInfo.getEntity().idP;
            var json;
            if (member.memberInfo.isField()) {
                json = {
                    name: name,
                    typeId: 'apiFieldMapping',
                    mappedField: jsonLookup(memberId)
                };
            } else {
                json = {
                    name: name,
                    typeId: 'apiRelationshipMapping',
                    mapRelationshipInReverse: member.memberInfo.isReverse(),
                    mappedRelationship: jsonLookup(memberId),
                    mappedRelationshipLookupField: jsonLookup()
                };
            }
            member.mappingEntity = spEntity.fromJSON(json);
        }

        // This is shared by Excel import
        // reference - the remote field reference (e.g. jsonId or column reference)
        // memberInfo - the field or relationship as wrapped by spResource.
        function createMemberMappingEntity(memberInfo, reference) {
            if (!memberInfo) return null; // assert false
            if (!reference) return null;  // assert false
            var memberId = memberInfo.getEntity().idP;
            var json;
            if (memberInfo.isField()) {
                json = {
                    name: reference,
                    typeId: 'apiFieldMapping',
                    mappedField: jsonLookup(memberId)
                };
            } else {
                json = {
                    name: reference,
                    typeId: 'apiRelationshipMapping',
                    mapRelationshipInReverse: memberInfo.isReverse(),
                    mappedRelationship: jsonLookup(memberId),
                    mappedRelationshipLookupField: jsonLookup() 
                };
            }
            var mappingEntity = spEntity.fromJSON(json);
            return mappingEntity;
        }

        // Returns true if a particular mapping entity refers to a particular member info
        // This is shared by Excel import
        function mappingRefersToMember(mappingEntity, memberInfo) {
            if (!mappingEntity) return false;  // assert false
            if (!memberInfo) return false;     // assert false
            var memberId = memberInfo.getEntity().idP;
            var isMatch = sp.result(mappingEntity, 'mappedField.idP') === memberId;
            if (!isMatch) {
                var isRelMatch = sp.result(mappingEntity, 'mappedRelationship.idP') === memberId;
                var isDirMatch = memberInfo.isReverse && mappingEntity.mapRelationshipInReverse === memberInfo.isReverse();
                isMatch = isRelMatch && isDirMatch;
            }
            return isMatch;
        }

        function memberChanged(member, endpoint) {
            if (member.selected) {
                // Tick on
                if (!member.mappingEntity) {
                    setMemberMappingEntity(member);
                }
                endpoint.endpointResourceMapping.resourceMemberMappings.add(member.mappingEntity);
                
            } else {
                // Tick off
                if (member.mappingEntity) {
                    endpoint.endpointResourceMapping.resourceMemberMappings.remove(member.mappingEntity);
                }
            }
        }

        // get the JSON member name
        function getJsonName(member) {
            if (!member.selected)
                return '';
            return sp.result(member, 'mappingEntity.name');
        }

        function typeChanged(model, newTypeId) {
            if (!newTypeId) {
                model.typeMembers = [];
                return;
            }
            getTypeMembers(model, newTypeId);
        }

        function getTypeMembers(model, typeId) {
            var endpoint = model.endpoint;
            var typeChanged = model.loadedTypeId !== typeId;
            var rq = spResource.makeTypeRequest({ fieldGroups: false, scriptInfo: true });

            return spEntityService.getEntity(typeId, rq, { hint: 'resourceEndpoint fields' }).then(function (typeEntity) {
                var typeInfo = new spResource.Type(typeEntity);
                var membersInfo = typeInfo.getAllMembers({ hideNonWritable: true });
                model.typeInfo = typeInfo;

                if (typeChanged) {
                    model.endpoint.apiEndpointAddress = makeJsonName(typeEntity.name);
                }

                var currentMappings = endpoint.endpointResourceMapping.resourceMemberMappings;

                var members = _.filter(_.map(membersInfo, function (mi) {
                    if (mi.isReadOnly()) // hmm.. maybe we want this on create
                        return null;
                    var memberMapping = _.find(currentMappings, function (memberMapping) {
                        return mappingRefersToMember(memberMapping, mi);
                    });

                    var member = {
                        selected: !!memberMapping,
                        name: mi.getName(),
                        memberType: mi.memberTypeDesc(),
                        memberInfo: mi,
                        mappingEntity: memberMapping
                    };
                    member.change = function () {
                        return memberChanged(member, endpoint);
                    };
                    member.getJsonName = function () {
                        return getJsonName(member);
                    };

                    // Select mandatory fields by default
                    if (mi.isRequired() && typeChanged) {
                        member.selected = true;
                        member.change(member, endpoint);
                    }
                    if (mi.isRequired() && member.selected) {
                        member.disableCheck = true;
                    }
                    return member;
                }));
                model.typeMembers = members;
            });
        }

        function save(endpoint) {
            endpoint.name = sp.result(endpoint, 'mappedType.name') + ' Endpoint';
            return spEntityService.putEntity(endpoint);
        }

        function sampleReady(model) {
            return model.endpoint && model.typeMembers && model.endpoint.apiEndpointAddress && model.sample.verb;
        }

        function getSampleAddress(model) {
            if (!model.endpoint)
                return;
            var res = model.sample.verb + ' ' + model.addressPrefix + '/' + model.endpoint.apiEndpointAddress;
            if (model.sample.verb !== 'POST') {
                res += '/<span class="val">' + getSampleIdentifier(model) + '</span>';
            }
            res += '?key=<span class="val">APIKey</span>';
            return $sce.trustAsHtml(res);
        }

        function getSamplePayload(model) {
            if (model.sample.verb === 'DELETE') {
                return $sce.trustAsHtml('Not applicable');
            }
            var sample = '{';
            var first = true;
            _.forEach(model.typeMembers, function (member) {
                if (!member.selected)
                    return;
                if (!first)
                    sample += ',';
                var name = member.getJsonName();
                var value = getSampleValue(member);
                sample += '\n    "' + name + '":<span class="val">' + value + '</span>';
                first = false;
            });
            sample += '\n}';
            return $sce.trustAsHtml(sample);
        }

        function getSampleValue(member) {
            switch (member.memberType) {
                case 'Int Field': return '1001';
                case 'Boolean Field': return 'true';
                case 'Currency Field': return '1001.12';
                case 'Decimal Field': return '1001.123';
                case 'Date Field': return '"2000-12-31"';
                case 'Time Field': return '"23:59:59"';
                case 'DateTime Field': return '"2000-12-31T23:59:59Z"';
                case 'String Field': return '"text"';
                case 'Lookup': return '"Name"';
                case 'Choice Field': return '"Choice Value"';
                case 'Multi Choice': return '["Choice 1", "Choice 2"]';
                case 'Relationship': return '["Name 1", "Name 2"]';
            }
            return null;
        }

        function getSampleIdentifier(model) {
            return 'ResourceName';
        }

        function objectName(model) {
            return model.typeInfo ? makeSafeName(model.typeInfo.getName()) : 'Name';
        }

        function updateSampleVerb(model) {
            var verb;
            if (!model.endpoint)
                verb = '';
            else if (model.endpoint.endpointCanCreate)
                verb = 'POST';
            else if (model.endpoint.endpointCanUpdate)
                verb = 'PUT';
            else if (model.endpoint.endpointCanDelete)
                verb = 'DELETE';
            else
                verb = '';
            model.sample.verb = verb;
        }

        var exports = {
            createEmptyModel: createEmptyModel,
            createModel: createModel,
            makeJsonName: makeJsonName,
            typeChanged: typeChanged,
            save: save,
            getSampleAddress: getSampleAddress,
            getSamplePayload: getSamplePayload,
            sampleReady: sampleReady,
            updateSampleVerb: updateSampleVerb,
            createMemberMappingEntity: createMemberMappingEntity,    // used by Excel import
            mappingRefersToMember: mappingRefersToMember             // used by Excel import
        };
        return exports;
    }

})();
