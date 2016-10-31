// Copyright 2011-2016 Global Software Innovation Pty Ltd

(function() {
    'use strict';

    angular.module('sp.workflow.activities')
        .controller('promptUserActivityController', function ($q, $scope, spWorkflowService, spWorkflowEditorViewService) {
            var allAvailable = [];

            $scope.variables = [];
            $scope.noMoreVariables = false;

            $scope.$watch('activityParameters', function () {

                $scope.activityParameters['core:inPromptUserForPerson'].resourceType = 'core:person';

            });

            $scope.$watch('entity', function (entity) {

                if (!entity) {
                    return;
                }

                // all the variables
                allAvailable = _.sortBy(_.map(_.union($scope.workflow.entity.inputArguments, $scope.workflow.entity.variables), function (a) {
                    return {
                        id: a.idP,
                        name: a.name,
                        description: a.field('description'),
                        resourceType: a.conformsToType
                    };
                }), 'name');

                // the variables already chosen
                $scope.variables = _.filter(_.map(entity.promptForArguments, function (c) {
                    var arg = c.activityPromptArgument;
                    var rpt = c.activityPromptArgumentPickerReport;
                    if (arg) {
                        var name = arg.name;
                        var description = arg.field('description');
                        var order = c.activityPromptOrdinal;
                        var available = _.find(allAvailable, { 'id': arg.idP });
                        if (available) {
                            name = available.name;
                            description = available.description;
                        }
                        
                        var v = {
                            id: arg.idP,
                            name: name,
                            description: description,
                            order: order
                        };

                        if (rpt) {
                            v.report = { id: rpt.idP, name: rpt.name };
                        }

                        return v;
                    }
                    return null;
                }), function (m) { return m !== null; });
                
                updateNoMoreVariables();
            });

            $scope.updatePromptForArguments = function (newArgument, oldArgumentId) {
                if (newArgument && (newArgument.id !== oldArgumentId)) {

                    // I don't know what to do with ordering here. I may have to recreate the entire collection.
                    var o = _.find(allAvailable, function (v) { return v.id === newArgument.id; });
                    var e = _.find($scope.variables, function (v) { return v.id === newArgument.id; });

                    // update the selected variables details after the id change
                    if (o) {
                        //e.order = o.order; // order remains (handy)
                        e.name = o.name;
                        e.description = o.description;
                        e.report = null; // reset!
                    }

                    // add
                    $scope.entity.promptForArguments.add(spEntity.fromJSON({
                        typeId: 'core:activityPrompt',
                        activityPromptOrdinal: e.order,
                        activityPromptArgument: spEntity.fromId(e.id),
                        activityPromptArgumentPickerReport: e.report ? spEntity.fromId(e.report.id) : jsonLookup('core:report')
                    }));

                    // remove
                    var r = _.find($scope.entity.promptForArguments, function (a) { return a.activityPromptArgument.idP === oldArgumentId; });
                    if (r) {
                        $scope.entity.promptForArguments.deleteEntity(r);
                    }

                    updateNoMoreVariables();
                }
            };

            $scope.addVariable = function () {
                var ids = _.map($scope.variables, 'id');
                var v = _.find(allAvailable, function(a) {
                    return ids.indexOf(a.id) < 0;
                });
                if (v) {
                    var order = getMaxOrder() + 1;
                    $scope.variables.push({
                        id: v.id,
                        name: v.name,
                        description: v.description,
                        order: order
                    });
                    $scope.entity.promptForArguments.add(spEntity.fromJSON({
                        typeId: 'core:activityPrompt',
                        activityPromptOrdinal: order,
                        activityPromptArgument: spEntity.fromId(v.id)
                    }));
                    updateNoMoreVariables();
                }
            };

            $scope.removeVariable = function (v) {
                var i = $scope.variables.indexOf(v);
                if (i >= 0) {
                    $scope.variables.splice(i, 1);

                    var e = _.find($scope.entity.promptForArguments, function (a) { return a.activityPromptArgument.idP === v.id; });
                    if (e) {
                        $scope.entity.promptForArguments.deleteEntity(e);
                    }

                    updateNoMoreVariables();
                }
            };

            $scope.getAllVariables = function (v) {
                // the variables to choose from
                var ids = _.map($scope.variables, 'id');
                return _.filter(allAvailable, function (a) {
                    return ((a.id === v.id) || (ids.indexOf(a.id) < 0));
                });
            };

            $scope.showReportsForType = function(v) {
                var info = _.find(allAvailable, function (a) { return a.id === v.id; });

                var show = info && info.resourceType && info.resourceType.idP > 0;
                if (show) {
                    show = _.map(spResource.getAncestorsAndSelf(info.resourceType), 'nsAlias').indexOf('core:enumValue') < 0;
                }

                //console.log('promptUserActivity.showReportsForType: ' + show, info);

                return show;
            };

            function updateNoMoreVariables() {
                $scope.noMoreVariables = allAvailable.length === $scope.variables.length;
            }

            function getMaxOrder() {
                if (!$scope.variables || $scope.variables.length === 0)
                    return 0;

                return _.maxBy(_.map($scope.variables, 'order'));
            }

            $scope.open = function (actionName, parameter) {
                var a = _.find(allAvailable, function (p) { return parameter === ('' + p.id); });
                var v = _.find($scope.variables, function (p) { return parameter === ('' + p.id); });
                if (a && v) {
                    spWorkflowEditorViewService.chooseReport(a.resourceType.idP, null).then(function (resource) {
                        var arg = _.find($scope.entity.promptForArguments, function (p) { return p.activityPromptArgument.idP === v.id; });
                        if (resource && resource.id > 0) {
                            v.report = { id: resource.id, name: resource.name };
                            if (arg) {
                                arg.activityPromptArgumentPickerReport = spEntity.fromJSON({ id: resource.id, name: resource.name });
                            }
                        } else {
                            v.report = null;
                            if (arg) {
                                arg.activityPromptArgumentPickerReport = null;
                            }
                        }
                    });
                }
            };
    });
}());