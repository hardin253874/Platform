// Copyright 2011-2016 Global Software Innovation Pty Ltd
/*global _, console, angular */

(function () {
    'use strict';

    /**
     * Module implementing the security query view.
     *
     * @module app.securityQuery.newQueryDialog
     */
    angular.module('app.securityQuery.newQueryDialog', ['mod.common.ui.spDialogService']);

    angular.module('app.securityQuery.newQueryDialog').controller("NewSecurityQueryDialogController", NewSecurityQueryDialogController);
    angular.module('app.securityQuery.newQueryDialog').factory("spNewSecurityQueryDialogFactory", spNewSecurityQueryDialogFactory);

    /* @ngInject */
    function NewSecurityQueryDialogController($scope, $uibModalInstance, options) {
        if (!angular.isArray(options.subjects)) {
            throw new Error("options.subjects must be an array");
        }
        if (!angular.isObject(options.selectedSubject)) {
            throw new Error("options.selectedSubject must be an object");
        }
        if (!angular.isArray(options.securableEntities)) {
            throw new Error("options.securableEntities must be an array");
        }
        if (!angular.isObject(options.selectedSecurableEntity)) {
            throw new Error("options.selectedSecurableEntity must be an object");
        }

        function filterSubjects(subjects, includeUsers) {
            return _.filter(subjects, function (subject) {
                return includeUsers ? true : (!subject.isOfType || !subject.isOfType.length || subject.isOfType[0].nsAlias === "core:role");
            });
        }

        function addDisplayName(subjects) {
            return _.forEach(subjects, function (subject) {
                subject.displayName = subject.name;
                if (subject.isOfType && subject.isOfType.length) {
                    subject.displayName += " (" + subject.isOfType[0].name + ")";
                }
                return subject;
            });
        }

        $scope.options = options;
        $scope.options.subjects = addDisplayName(options.subjects);
        $scope.options.includeUsers = false;
        $scope.options.applicationPickerOptions = {
            selectedEntityId: null,
            selectedEntity: null,
            selectedEntities: null,
            pickerReportId: options.ids.applicationPickerReportId ? options.ids.applicationPickerReportId.id() : options.ids.templateReportId.id(),
            entityTypeId: options.ids.solutionId.id(),
            multiSelect: false,
            isDisabled: false
        };

        $scope.subjects = filterSubjects(options.subjects, $scope.options.includeUsers);

        $scope.toggleIncludeUsers = function () {
            $scope.subjects = filterSubjects(options.subjects, $scope.options.includeUsers);
        };

        $scope.ok = function () {
            var result = {
                selectedSubject: $scope.options.selectedSubject,
                selectedSecurableEntity: $scope.options.selectedSecurableEntity,
            };

            if ($scope.options.applicationPickerOptions.selectedEntities && $scope.options.applicationPickerOptions.selectedEntities.length > 0) {
                result.selectedApplication = $scope.options.applicationPickerOptions.selectedEntities[0];
            }

            $uibModalInstance.close(result);
        };

        $scope.cancel = function () {
            $uibModalInstance.dismiss("cancel");
        };
    }

    /* @ngInject */
    function spNewSecurityQueryDialogFactory(spDialogService) {
        var exports = {};

        /**
         * Create the dialog prompting the user for new security query details.
         *
         * @param {Array} The subjects (users or roles) to display. These should be spEntity.Entity objects.
         * @param {Array} The securable entities (types) to display. These should be spEntity.Entity objects.
         * @param {String} The default (initially selected) subject (user or role). This should be an alias.
         * @param {String} The default (initially selected) securable entity (type). This should be an alias.
         * @returns {String} A promise containing the results of the dialog, specifically the "selectedSubject" and "selectedSecurityEntity" fields.
         *
         * @function showDialog
         */
        exports.showDialog = function (subjects, types, defaultSubjectAlias, defaultSecurableEntityAlias, ids) {
            if (!angular.isArray(subjects)) {
                throw new Error("subjects must be an array");
            }
            if (!angular.isArray(types)) {
                throw new Error("types must be an array");
            }
            if (!angular.isString(defaultSubjectAlias)) {
                throw new Error("defaultSubjectAlias must be a string");
            }
            if (!angular.isString(defaultSecurableEntityAlias)) {
                throw new Error("defaultSecurableEntityAlias must be a string");
            }

            var selectedSubject;
            if (subjects.length === 1) {
                selectedSubject = _.first(subjects);
            } else {
                selectedSubject = _.find(subjects, { nsAlias: defaultSubjectAlias });
            }

            var dialogOptions = {
                templateUrl: 'security/newSecurityQueryDialog.tpl.html',
                controller: 'NewSecurityQueryDialogController',
                resolve: {
                    options: function () {
                        return {
                            subjects: _.sortBy(subjects, "name"),
                            selectedSubject: selectedSubject,
                            securableEntities: _.sortBy(types, "name"),
                            securableEntitiesAnnotated: annotateNames(types),
                            selectedSecurableEntity: _.find(types, { nsAlias: defaultSecurableEntityAlias }),
                            ids: ids
                        };
                    }
                }
            };

            return spDialogService.showModalDialog(dialogOptions);

        };

        return exports;
    }

    // warning  - the given entity objects are modified to include a nameAnnotated property
    function annotateNames(entities) {

        // data structure to help find dups
        var byNameIndex = _.groupBy(entities, 'name');

        // add the nameAnnotated prop to each entity
        return _(entities)
            .map(function (e) {
                // default to the entity name
                e.nameAnnotated = e.name;
                // append the solution name if the name isn't unique
                if (byNameIndex[e.name].length > 1) {
                    var app = e.inSolution;
                    var appName = app && app.name;
                    if (appName) {
                        e.nameAnnotated += ' (' + appName + ')';
                    }
                }
                return e;
            })
            .sortBy('nameAnnotated')
            .value();
    }
})();
