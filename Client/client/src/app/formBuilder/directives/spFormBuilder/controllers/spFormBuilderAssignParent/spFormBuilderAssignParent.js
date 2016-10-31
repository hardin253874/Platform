// Copyright 2011-2016 Global Software Innovation Pty Ltd


(function () {
    'use strict';

    /**
    * Module implementing a dialog to edit the properties of a context-receiver.
    * That is, to say what source or parent will be providing the resource context.
    *
    * @module spFormBuilderAssignParent
    */
    angular.module('mod.app.formBuilder.directives.spFormBuilderAssignParent', [
        'ui.bootstrap',
        'mod.common.spEntityService',
        'mod.common.ui.spDialogService',
        'mod.app.formBuilder.services.spFormBuilderService'
    ])
        .controller('spFormBuilderAssignParentController', function ($scope, $uibModalInstance, options, spFormBuilderService, spEntityService) {

            // schema info for context providers & receivers
            var schemaInfo = {
                formRenderControl: {
                    controlName: 'Form',
                    objectRel: 'formToRender',
                    typeRel: 'typeToEditWithForm',
                    kTypeRel: 'k:typeToEditWithForm',
                    allowReports: true,
                    allowCharts: true
                },
                reportRenderControl: {
                    controlName: 'Report',
                    objectRel: 'reportToRender',
                    typeRel: 'rootNode.resourceReportNodeType',
                    kTypeRel: 'rootNode.resourceReportNodeType',
                    allowReports: false,
                    allowCharts: true
                },
                chartRenderControl: {
                    controlName: 'Chart',
                    objectRel: 'chartToRender',
                    typeRel: 'chartReport.rootNode.resourceReportNodeType',
                    kTypeRel: 'chartReport.rootNode.resourceReportNodeType',
                    allowReports: false,
                    allowCharts: true
                },
                heroTextControl: {
                    controlName: 'Hero Text',
                    objectRel: '', // the control is the heroText (c.f. reportControl->report)
                    typeRel: 'heroTextReport.rootNode.resourceReportNodeType',
                    kTypeRel: 'heroTextReport.rootNode.resourceReportNodeType',
                    allowReports: false,
                    allowCharts: true
                }
            };

            // options: { targetName, targetTypeName, control }
            $scope.modalInstance = $uibModalInstance;
            $scope.model = {};
            $scope.model = _.extend($scope.model, options);

            $scope.model.control.registerLookup('k:receiveContextFrom');
            $scope.model.selectedParent = sp.result($scope.model.control, 'receiveContextFrom.id');

            var getSchema = function (ctrl) {
                var type = ctrl.getType().getAlias();
                return schemaInfo[type];
            };

            // Prepare batch
            var batch = new spEntityService.BatchRequest();

            // Load info for target
            var targetSchema = getSchema($scope.model.control);
            var query = 'name, ' + targetSchema.kTypeRel + '.name'; // objectRel and typeRel get substituted

            var targetId = getInstId($scope.model.control, targetSchema);
            spEntityService.getEntity(targetId, query, { batch: batch })
                .then(function (eTarget) {
                    $scope.model.targetName = eTarget.name;
                    $scope.model.targetType = sp.result(eTarget, targetSchema.typeRel); // eg the form
                    $scope.model.targetTypeName = $scope.model.targetType ? $scope.model.targetType.name : '';
                    checkInheritance();
                });

            // Load info for potential parents
            var candidates = findCandidates();
            var objectIds = _.map(candidates, 'objectId');

            var kTypeRels = _.map(_.values(schemaInfo), 'kTypeRel').join();
            var query2 = 'name,  {' + kTypeRels + '}.{inherits*, name}';
            if (objectIds.length) {
                spEntityService.getEntities(objectIds, query2, { batch: batch })
                    .then(function (eObjects) {
                        _.forEach(candidates, function (candidate) {
                            var eObject = _.find(eObjects, function (eo) { return eo.id() === candidate.objectId; });
                            candidate.object = eObject;
                            candidate.type = sp.result(eObject, candidate.schema.typeRel);
                            candidate.typeName = candidate.type.name;
                            candidate.text = eObject.name + ' (' + candidate.typeName + ' ' + candidate.schema.controlName + ')';
                        });
                        // Filter out candidates for which we couldn't find the object data, or for incorrect types
                        candidates = _.filter(candidates, 'object');
                        // Ignore the control itself
                        candidates = _.filter(candidates, function (cand) { return cand.control !== $scope.model.control; });
                        $scope.model.candidates = candidates;
                        checkInheritance();
                    });
            }

            // Run batch
            batch.runBatch();

            function checkInheritance() {
                if (!$scope.model.targetType || !$scope.model.candidates)
                    return;
                // After all indivuals run, filter out any that don't satisfy the type inheritance
                var targetTypeId = $scope.model.targetType.id();
                $scope.model.parents = _.filter($scope.model.candidates, function (candidate) {
                    var ancestors = spResource.getAncestorsAndSelf(candidate.type);
                    var res = _.some(ancestors, function (type) { return type.id() === targetTypeId; });
                    return res;
                });
            }

            // OK click handler
            $scope.ok = function () {
                if (!$scope.model.selectedParent) {
                    $scope.model.control.receiveContextFrom = null;
                } else {
                    var parentControl = _.find($scope.model.parents, function (p) {
                        return p.id === $scope.model.selectedParent;
                    });
                    $scope.model.control.receiveContextFrom = parentControl ? parentControl.control : null;
                }
                $scope.modalInstance.close(true);
            };

            // Cancel click handler
            $scope.cancel = function () {
                $scope.modalInstance.close(false);
            };

            $scope.getParentTypeName = function (selectedParent) {
                if (!selectedParent)
                    return '';
                var candidate = _.find(candidates, { id: selectedParent });
                return candidate.typeName + ' ' + candidate.schema.controlName;
            };

            $scope.clearParent = function () {
                $scope.model.selectedParent = null;
            };

            function findCandidates() {
                var contextProviders = {
                    reportRenderControl: targetSchema.allowReports ? 'Report' : null,
                    chartRenderControl: targetSchema.allowCharts ? 'Chart' : null
                };

                var form = spFormBuilderService.form;
                var controls = spUtils.walkGraph(function (control) { return control.containedControlsOnForm; }, form);
                var providers = _.map(_.filter(controls, function (control) {
                    return contextProviders[control.getType().getAlias()];
                }), function (control) {
                    var schema = getSchema(control);
                    return {
                        id: control.id(),
                        control: control,
                        schema: schema,
                        objectId: getInstId(control, schema),  // the report/form/chart/heroText being pointed to
                        text: schema.controlName
                    };
                });
                return providers;
            }
            
            function getInstId(control, schema) {
                if (schema.objectRel)
                    return control[schema.objectRel].id();
                else
                    return control.id();
            }
        })
        .service('spFormBuilderAssignParentService', function (spDialogService) {
            // setup the dialog
            var exports = {
                showDialog: function (options) {
                    var dialogOptions = {
                        title: 'Assign Parent',
                        templateUrl: 'formBuilder/directives/spFormBuilder/controllers/spFormBuilderAssignParent/spFormBuilderAssignParent.tpl.html',
                        controller: 'spFormBuilderAssignParentController',
                        resolve: {
                            options: function () {
                                return options;
                            }
                        }
                    };

                    return spDialogService.showModalDialog(dialogOptions);
                }
            };

            return exports;
        });
}());