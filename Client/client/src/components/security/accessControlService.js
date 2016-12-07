// Copyright 2011-2016 Global Software Innovation Pty Ltd
(function() {
    'use strict';

    angular.module('mod.app.accessControl.service', ['mod.app.accessControl.repository', 'mod.services.promiseService', 'app.securityQuery.newQueryDialog'])
        /**
        * Module for maintaining the list of security queries in the browser.
        */
        .service('spAccessControlService', function (spAccessControlRepository, $q, spPromiseService, spNewSecurityQueryDialogFactory) {
            var exports = {};
            var loading = false;
            
            /**
             * A query shown as a row in the security queries table. Wraps an Access Rule entity.
             *
             * @param {spEntity.Entity} The access rule entity to wrap.
             * @param {Boolean} True if the SecurityQueryRow should be dirty, false if not.
             * @returns A SecurityQueryRow object used as a model by the grid.
             */
            function SecurityQueryRow(accessRule, defaultDirty) {
                if (!spEntity.isEntity(accessRule)) {
                    throw new Error("accessRule must be an AccessRule entity");
                }
                if (!_.isBoolean(defaultDirty)) {
                    throw new Error("defaultDirty must be a boolean");
                }

                var subjectName;
                var subjectType = "";
                if (!accessRule.allowAccessBy) {
                    subjectName = "(Deleted)";
                    subjectType = "Unknown Type";
                } else {
                    subjectName = accessRule.allowAccessBy.name;
                    if (accessRule.allowAccessBy.isOfType && accessRule.allowAccessBy.isOfType.length) {
                        subjectType = accessRule.allowAccessBy.isOfType[0].name;
                    }
                }

                var query = {
                    roleOrUserAccount: subjectName ? subjectName + " (" + subjectType + ")" : "",
                    accessRule: accessRule,
                    dirty: defaultDirty
                };

                Object.defineProperty(query, "permissions",
                    {
                        get: function() {
                            return _.map(query.accessRule.permissionAccess, "nsAlias").sort().join(",");
                        },
                        set: function (value) {
                            // Remove existing permissions
                            var toRemove = _.clone(query.accessRule.permissionAccess);
                            _.forEach(toRemove, function (permissionToRemove) {
                                query.accessRule.permissionAccess.remove(permissionToRemove);
                            });

                            // Add the new permissions
                            if (value) {
                                _.forEach(value.split(","), function(permissionAlias) {
                                    var permissionToAdd = _.find(exports.allPermissions, { nsAlias: permissionAlias });
                                    query.accessRule.permissionAccess.add(permissionToAdd);
                                });
                            }
                            
                            query.dirty = true;
                        },
                        enumerable: true
                    });

                Object.defineProperty(query, "queryName",
                    {
                        get: function() {
                            return query.accessRule.accessRuleReport ? query.accessRule.accessRuleReport.name : '';
                        },
                        set: function (value) {
                            if (query.accessRule.accessRuleReport) {
                                query.accessRule.accessRuleReport.name = value;
                                query.dirty = true;
                            }
                        },
                        enumerable: true
                    });

                Object.defineProperty(query, "enabled",
                    {
                        get: function () {
                            return query.accessRule.accessRuleEnabled;
                        },
                        set: function (value) {
                            query.accessRule.accessRuleEnabled = value;
                            query.dirty = true;
                        },
                        enumerable: true
                    });

                return query;
            }

            function handleAccessRuleResults(results) {
                if (results) {
                    exports.accessRules = results.accessRules;
                    exports.allPermissions = results.permissions;
                }
                exports.queries = _.map(exports.accessRules, function (accessRule) { return new SecurityQueryRow(accessRule, false); });
                exports.deletedQueries = [];
            }

            function createAccessRule(subject, securable) {
                var readPermission = spEntity.fromJSON({
                    alias: 'read',
                    typeId: 'permission'
                });

                var recordArgument = spEntity.fromJSON({
                    typeId: 'resourceArgument'
                });

                var idExpression = spEntity.fromJSON({
                    typeId: 'idExpression',
                    sourceNode: jsonLookup(),
                    reportExpressionResultType: jsonLookup(recordArgument)
                });

                var stringArgument = spEntity.fromJSON({
                    typeId: 'stringArgument'
                });

                var nameExpression = spEntity.fromJSON({
                    typeId: 'fieldExpression',
                    sourceNode: jsonLookup(),
                    fieldExpressionField: jsonLookup('name'),
                    reportExpressionResultType: jsonLookup(stringArgument)
                });

                var reportColumns = [
                    spEntity.fromJSON({
                        typeId: 'reportColumn',
                        name: jsonString('Id'),
                        columnDisplayOrder: 0,
                        columnIsHidden: true,
                        columnExpression: jsonLookup(idExpression)
                    }),
                    spEntity.fromJSON({
                        typeId: 'reportColumn',
                        name: jsonString('Name'),
                        columnDisplayOrder: 1,
                        columnIsHidden: false,
                        columnExpression: jsonLookup(nameExpression)
                    })
                ];

                var f = '' + spUtils.newGuid() + ' ' + new Date().toLocaleTimeString();
                var reportFolder = spEntity.fromJSON({
                    typeId: 'folder',
                    name: jsonString(f)
                });

                var r = sp.result(securable, 'name') || ('Query ' + securable.idP);
                var accessReport = spEntity.fromJSON({
                    typeId: 'report',
                    name: jsonString(r),
                    description: jsonString(),
                    'console:resourceInFolder': jsonRelationship([reportFolder]),
                    reportColumns: jsonRelationship(reportColumns),
                    reportUsesDefinition: jsonLookup(securable),
                    rootNode: {
                        typeId: 'resourceReportNode',
                        resourceReportNodeType: jsonLookup(securable),
                        exactType: false,
                        targetMustExist: true,
                        nodeUsedByExpression: jsonRelationship([idExpression, nameExpression])
                    }
                });

                var accessRule = spEntity.fromJSON({
                    typeId: 'accessRule',
                    accessRuleEnabled: false,
                    accessRuleHidden: false,
                    allowAccessBy: jsonLookup(subject),
                    controlAccess: jsonLookup(securable),
                    permissionAccess: jsonRelationship([readPermission]),
                    accessRuleReport: jsonLookup(accessReport)
                });

                return accessRule;
            }

            // Used for testing only
            exports._newSecurityQueryRow = SecurityQueryRow;

            /**
             * Load data into the queries array.
             *
             * @returns {promise} Completes when the data is loaded.
             *
             * @function
             * @name spAccessControlService#loadQueries
             */
            exports.loadQueries = function (options) {
                var opts = _.defaults(options || {}, { includeHidden: false });

                if (loading) {
                    // there is a call under way. wait for it to complete, instead of initiating another.
                    return spPromiseService.poll(function () { return $q.when(loading); }, function (result) { return !result; }, 100, 200).finally(function () {
                        if (loading) {
                            console.warn('spAccessControlService.loadQueries: took too long to load.');
                        }
                    });
                }

                console.log('spAccessControlService.loadQueries');

                loading = true;

                return spAccessControlRepository.getAccessRules(opts)
                    .then(handleAccessRuleResults)
                    .finally(function() {
                        loading = false;
                    });
            };

            /**
             * Save any changes.
             *
             * @returns {bool} True if there are changes, false otherwise.
             *
             * @function
             * @name spAccessControlService#isDirty
             */
            exports.isDirty = function() {
                return _.some(exports.queries, { dirty: true }) || exports.deletedQueries.length > 0;
            };

            /**
             * Save any changes.
             *
             * @returns {promise} A promise that completes when the data is saved. Consider handing the reject case to handle errors.
             *
             * @function
             * @name spAccessControlService#commit
             */
            exports.commit = function (doUpdates, doDeletes) {
                return $q.all({
                    saveAuth: !doUpdates ? $q.when() : spAccessControlRepository.putAccessRules(_.map(_.filter(exports.queries, { dirty: true }), "accessRule")),
                    deleteAuth: !doDeletes ? $q.when() : spAccessControlRepository.deleteAccessRules(_.map(exports.deletedQueries, "accessRule"))
                }).then( function() {
                    _.forEach(exports.queries, function (query) {
                        query.dirty = false;
                    });
                    exports.deletedQueries = [];
                }); 
            };

            /**
             * Create a new query. Prompt the user for the subject (user or role) and securable entity (type). Assign read 
             * permission only initially.
             *
             * @param {Function} Function to show a waiting message.
             * @param {Function} Function to hide a waiting message.
             * @returns {Object} A promise that completes when the query is created. Consider handing the reject case to handle errors.
             *
             * @function
             * @name spAccessControlService#createQuery
             */
            exports.createQuery = function (showWait, hideWait, options) {
                if (!angular.isFunction(showWait)) {
                    throw new Error("showWait must be a function");
                }
                if (!angular.isFunction(hideWait)) {
                    throw new Error("hideWait must be a function");
                }

                showWait();
                
                return spAccessControlRepository.getSubjectsAndTypes().then(function (result) {
                    hideWait();
                    var subs = result.subjects;
                    if (options.subject) {
                        subs = [options.subject];
                    }

                    var ids = {
                        templateReportId: result.ids[0],
                        solutionId: result.ids[1],
                        applicationPickerReportId: result.ids[2]
                    };
                    return spNewSecurityQueryDialogFactory.showDialog(subs, result.types, "core:everyoneRole", "core:person", ids );
                }).then(function (results) {
                    showWait();

                    var selectedApplicationId = null;

                    if (results.selectedApplication) {
                        selectedApplicationId = results.selectedApplication.idP;
                    }
                    if (results.selectedSubject.dataState !== spEntity.DataStateEnum.Create) {
                        return spAccessControlRepository.createAccessRule(results.selectedSubject.idP, results.selectedSecurableEntity.idP, _.filter(exports.permissions, { nsAlias: "core:read" }), selectedApplicationId);
                    }

                    var accessRule = createAccessRule(results.selectedSubject, results.selectedSecurableEntity);
                    results.selectedSubject.getRelationship('core:allowAccess').add(accessRule);
                    
                    return $q.when(accessRule);
                }).then(function (newAccessRule) {
                    
                    exports.queries.push(new SecurityQueryRow(newAccessRule, newAccessRule.dataState !== spEntity.DataStateEnum.Unchanged));
                    hideWait();

                    return newAccessRule;
                });
            };
            
            /**
             * Delete the given query.
             *
             * @param {spEntity.Entity} The query to delete.
             * @returns {promise} A promise that completes when the query is created. Consider handing the reject case to handle errors.
             *
             * @function
             * @name spAccessControlService#deleteQuery
             */
            exports.deleteQuery = function (query) {
                if (!angular.isObject(query)) {
                    throw new Error("query must be an object");
                }

                _.remove(exports.queries, query);
                exports.deletedQueries.push(query);
            };
            
            /**
             * Modes for the UI.
             */
            exports.modes = {
                view: 'view',
                edit: 'edit'
            };
            
            /**
             * The current view/edit mode.
             */
            exports.viewMode = exports.modes.view;

            exports.accessRules = [];
            exports.permissions = [];
            exports.queries = [];
            exports.deletedQueries = [];

            return exports;
        });
})();