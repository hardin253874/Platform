// Copyright 2011-2016 Global Software Innovation Pty Ltd
(function() {
    'use strict';

    angular.module('mod.app.accessControl.repository', ['mod.common.spEntityService', 'mod.common.spWebService'])
        /**
        * Module wrapping access control operations, such as editing security queries.
        */
        .service('spAccessControlRepository', function (spEntityService, $q, $http, spWebService) {
            var exports = {};
            var accessRuleRequestString = "accessRuleEnabled, accessRuleHidden, allowAccessBy.{name}, allowAccessBy.isOfType.{name}, controlAccess.{name}, permissionAccess.{alias}, accessRuleReport.{name, canModify}, canDelete";

            /**
             * Create a new access rule.
             *
             * @param {Number} The subject (user or role) ID.
             * @param {Number} The securable entity (type) ID.
             * @param {Array} The permissions to associate with the rule.
             * @returns {Object} A promise that returns the new access rule object.
             *
             * @function
             * @name spAccessControlRepository#createAccessRule
             */
            exports.createAccessRule = function (subjectId, securableEntityId, permissions, solutionId) {
                if (!angular.isNumber(subjectId) || subjectId <= 0) {
                    throw new Error("subject must be a positive integer");
                }
                if (!angular.isNumber(securableEntityId) || securableEntityId < 0) {
                    throw new Error("securableEntityId must be a positive integer");
                }
                if (!angular.isArray(permissions)) {
                    throw new Error("permissions must be an array");
                }

                var url;
                var data;

                url = spWebService.getWebApiRoot() + '/spapi/data/v1/accessrule';
                data = {
                    securableEntityId: securableEntityId,
                    subjectId: subjectId
                };

                if (solutionId) {
                    data.solutionId = solutionId;
                }

                return $http({
                    method: 'POST',
                    url: url,
                    data: data,
                    headers: spWebService.getHeaders()
                }).then(function(result) {
                    return spEntityService.getEntity(result.data, accessRuleRequestString);
                });
            };
            
            /**
             * Return a promise that returns a list of the access rules and 
             * all permissions (for the dropdowns).
             *
             * @returns {Object} A promise returning an object with the following fields:
             *     - accessRules: An array of access rule entities.
             *     - permissions: An array of permissions defined for this tenant.
             *
             * @function
             * @name spAccessControlRepository#getAccessRules
             */
            exports.getAccessRules = function (options) {
                var batch = new spEntityService.BatchRequest();
                var opts = _.defaults(options || {}, { includeHidden: false });
                var filter = opts.includeHidden ? null : '[Hidden]=\'No\'';
                var authPromise = spEntityService.getEntitiesOfType("core:accessRule", accessRuleRequestString, { batch: batch, filter: filter });
                var permissionsPromise = spEntityService.getEntitiesOfType("core:permission", "alias", { batch: batch });

                var promise = $q.all({
                    accessRules: authPromise,
                    permissions: permissionsPromise
                });
 
                batch.runBatch();

                return promise;
            };

            /**
             * Return a promise that returns a list of types and subjects, used when 
             * creating a new access rule.
             *
             * @returns {Object} A promise returning an object with the following fields:
             *     - subjects: An array of subject (user account or role) entities.
             *     - types: An array of entity types defined for this tenant.
             *
             * @function
             * @name spAccessControlRepository#getSubjectsAndTypes
             */
            exports.getSubjectsAndTypes = function () {
                var request = "name, alias, description, isOfType.name, inSolution.name";
                var typesPromiseBatch = new spEntityService.BatchRequest();
                var typesPromise = $q.all({
                    definitions: spEntityService.getEntitiesOfType("core:definition", request, { batch: typesPromiseBatch }),
                    others: spEntityService.getEntities(["console:customEditForm", "core:workflow", "core:reportTemplate", "core:fileType", "core:documentType", "core:imageFileType"], request, { batch: typesPromiseBatch })
                    // Add request(s) for new type or types here. Remember to change the code in AccessRulesController, too.
                }).then(function (promises) {
                    return _.flatten(_.values(promises));
                });
                typesPromiseBatch.runBatch();

                var batch = new spEntityService.BatchRequest();
                var subjectsPromise = spEntityService.getEntitiesOfType("core:subject", "name, alias, isOfType.{name, alias}", { batch: batch, filter: 'true' });
                var idsPromise = spEntityService.getEntities(['core:templateReport', 'core:solution', 'core:applicationsPickerReport'], 'id', { batch: batch });
                var promise = $q.all({
                    subjects: subjectsPromise,
                    types: typesPromise,
                    ids: idsPromise
                });
                batch.runBatch();

                return promise;
            };

            /**
             * Save the given access rules.
             *
             * @param {Array} The access rules to save.
             * @returns {promise} A promise that resolves with the ids of the newly saved access rules.
             *
             * @function
             * @name spAccessControlRepository#putAccessRules
             */
            exports.putAccessRules = function (accessRules) {
                if (!angular.isArray(accessRules)) {
                    throw new Error("accessRules must be an array");
                }

                var promises = [];

                _.forEach(accessRules, function (accessRule) {
                    promises.push(spEntityService.putEntity(accessRule));
                });

                return $q.all(promises);
            };

            /**
             * Delete the given access rules, including any reports 
             *
             * @param {Array} The access rules to delete.
             * @returns {promise} A promise that resolves when the entities are deleted.
             *
             * @function
             * @name spAccessControlRepository#deleteAccessRules
             */
            exports.deleteAccessRules = function (accessRules) {
                if (!angular.isArray(accessRules)) {
                    throw new Error("accessRules must be an array");
                }

                var promise;

                if (accessRules.length > 0) {
                    promise = spEntityService.deleteEntities(_.map(accessRules, "idP"));
                } else {
                    promise = $q.when();
                }

                return promise;
            };

            /**
             * Return a promise that returns a complete list of types that a subject can access.
             *
             * @returns {Object} A promise returning an array of objects with the following fields:
             *     - typename: the name of the type for which there is some access
             *     - perms: a csv representation of the permissions granted
             *     - subname: the name of the subject that the grant actually applies to (either the requested subject, or an included role)
             *     - reason: a textual description of the reason for the grant
             *
             * @function
             * @name spAccessControlRepository#getTypeAccessReport
             */
            exports.getTypeAccessReport = function (subjectId, showAdvanced) {
                if (!subjectId) {
                    return $q.reject('no subjectId');
                }

                var url = spWebService.getWebApiRoot() + '/spapi/data/v1/accesscontrol/typeaccessreport/' + encodeURIComponent(subjectId);
                if (showAdvanced)
                    url += "?advanced=true";

                return $http({
                    method: 'GET',
                    url: url,
                    headers: spWebService.getHeaders()
                }).then(function (response) {
                    return response.status === 200 ? response.data : null;
                }, function (response) {
                    return $q.reject(response.statusText);
                });
            };

            return exports;
        });
}());