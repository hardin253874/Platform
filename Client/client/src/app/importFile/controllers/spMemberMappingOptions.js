// Copyright 2011-2016 Global Software Innovation Pty Ltd
(function() {
    'use strict';

    /**
    * Module implementing the member mapping options dialog.
    * 
    * @module spMemberMappingOptions
    */
    angular.module(
            'mod.app.importFile.controllers.spMemberMappingOptions', [
                'ui.bootstrap',
                'mod.common.alerts',
                'mod.common.spEntityService',
                'mod.app.importFile.services.spImportFileService'
            ])
        .service("spMemberMappingOptions", ['$q', 'spImportFileService', 'spEntityService', MemberMappingOptionsService])
        .controller("spMemberMappingOptionsController", ['$scope', '$uibModalInstance', 'mappingRow', 'spMemberMappingOptions', MemberMappingOptionsController]);

    function MemberMappingOptionsController($scope, $uibModalInstance, mappingRow, spMemberMappingOptions) {

        // model defines .series
        $scope.model = spMemberMappingOptions.createModel(mappingRow);

        // Handle OK
        $scope.ok = function () {
            spMemberMappingOptions.applyChanges($scope.model);
            $uibModalInstance.close(true);
        };

        // Handle Cancel
        $scope.cancel = function() {
            $uibModalInstance.close(false);
        };

        spMemberMappingOptions.loadModelData($scope.model);
    }

    function MemberMappingOptionsService($q, spImportFileService, spEntityService) {

        function createModel(mappingRow) {
            return {
                mapping: mappingRow.mapping,
                defaultLookupField: null,
                selectedLookupField: null,
                lookupByMembers: []
            };
        }

        function loadModelData(model) {

            var mapping = model.mapping;
            var rel = sp.result(mapping, 'mappedRelationship');
            if (!rel) return $q.when();

            var isRev = mapping.mapRelationshipInReverse;

            var fieldTypeCanBeUsed = {
                'String Field': true,
                'Int Field': true,
                'Number Field': true
            };

            var selectedFieldId = sp.result(mapping, 'mappedRelationshipLookupField.idP');

            return loadFields(rel.idP, isRev).then(function (relEntity) {
                var typeEntity = isRev ? relEntity.fromType : relEntity.toType;
                var type = new spResource.Type(typeEntity);
                var fieldOptions = spImportFileService.getAvailableTargetMembers(type);
                model.lookupByMembers = _.filter(fieldOptions, function (entry) {
                    var fieldEntity = entry.memberInfo.getEntity();
                    if (fieldEntity.nsAlias === 'core:name')
                        model.defaultLookupField = entry;
                    if (fieldEntity.idP === selectedFieldId)
                        model.selectedLookupField = entry;
                    return fieldTypeCanBeUsed[entry.memberTypeDesc];
                });
                if (!model.selectedLookupField)
                    model.selectedLookupField = model.defaultLookupField;

                return model;
            });
        }

        function loadFields(relId, isRev) {
            // Create a request to load schema info
            var opts = {
                fields: true,
                relationships: false,
                fieldGroups: true,
                ignoreInheritance: false,
                ignoreOverrides: false,
                derivedTypes: false
            };
            var rq = spResource.makeTypeRequest(opts);
            var rq2 = (isRev ? 'fromType' : 'toType') + '.{' + rq + '}';
            return spEntityService.getEntity(relId, rq2, { hint: 'schema for import lookup' });
        }

        function applyChanges(model) {
            var mapping = model.mapping;
            if (model.defaultLookupField === model.selectedLookupField) {
                mapping.mappedRelationshipLookupField = null;
            } else {
                var selectedField = model.selectedLookupField.memberInfo;

                mapping.mappedRelationshipLookupField = spEntity.fromJSON({
                    id: selectedField.getEntity().idP,
                    name: jsonString(selectedField.getName())
                });
            }
        }

        return {
            createModel: createModel,
            loadModelData: loadModelData,
            applyChanges: applyChanges            
        };

    }

}());