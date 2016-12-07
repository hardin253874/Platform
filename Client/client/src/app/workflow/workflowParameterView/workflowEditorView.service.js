// Copyright 2011-2016 Global Software Innovation Pty Ltd
/*global _, console, angular, spWorkflowConfiguration, spWorkflow, sp, spEntity */

// TODO: have an issue with promises and the apply/cancel functions being callable multiple times
// TODO: don't use the name "apply" for the callback.... is confusing given function's apply function

(function () {
    'use strict';

    angular.module('sp.workflow.parameterViewServices')
        .factory('spWorkflowEditorViewService', spWorkflowEditorViewService);
    
    function spWorkflowEditorViewService($q, spViewRegionService, spEntityService, spWorkflowService, spExpressionEditorService) {

        var exports = {
            getReportChooserView: getReportChooserView,
            chooseResource: chooseResource,
            chooseReport: chooseReport,
            openSingleKnownEntityChooser: openSingleKnownEntityChooser,
            openSingleParameterChooser: openSingleParameterChooser,
            openExprEditor: openExprEditor
        };

        function getFilteredRows(view, columns) {
            return _.filter(view.rows, function(row) {
                var search = _.toLower(view.quickSearch.value || '');
                return _.some(_.map(columns, 'field'), function(column) {
                    return _.includes(_.toLower(_.get(row, column)), search);
                });
            });
        }

        function getMetaData(resourceType) {
            let req = 'name, description, isOfType.name, inherits*, { fields, relationships, reverseRelationships }.{ alias, name, toName, fromName, cardinality.alias, description, { fromType, toType }.{ name } }';
            let resourceTypeRef = spEntity.asEntityRef(resourceType);
            let id = resourceTypeRef.nsAliasOrId;
            return spWorkflowService.getCacheableEntity('meta:' + id, id, req);
        }

        function getFields(resourceType) {
            if (!resourceType) {
                return $q.when(null);
            }

            return getMetaData(resourceType).then(function (type) {
                var data = [];

                if (type) {
                    var t = new spResource.Type(type);
                    var fields = t.getFields();
                    var rels = _.filter(t.getAllRelationships(), function (r) { return r.isLookup() || r.isChoiceField(); });

                    var dataFields = _.map(fields, function (f) {
                        return {
                            name: f.getName(),
                            description: f.getDescription(),
                            isReverse: false,
                            entity: f.getEntity()
                        };
                    });

                    var dataRels = _.map(rels, function (r) {
                        return {
                            name: r.getName(),
                            description: r.getDescription(),
                            isReverse: !!r.isReverse(),
                            entity: r.getEntity()
                        };
                    });

                    data = dataFields.concat(dataRels);
                }

                return data;
            });
        }

        function getRelationships(resourceType) {
            if (!resourceType) {
                return $q.when(null);
            }

            return getMetaData(resourceType).then(function (type) {
                var data = [];
                if (type) {
                    data = _.filter(new spResource.Type(type).getAllRelationships(), function (r) { return !r.isLookup(); });
                }
                return _.map(data, function (d) {
                    return {
                        name: d.getName(),
                        description: d.getDescription(),
                        direction: d.isReverse() ? 'reverse' : 'forward',
                        isReverse: !!d.isReverse(),
                        entity: d.getEntity()
                    };
                });
            });
        }

        function getProperties(resourceType) {
            if (!resourceType) {
                return $q.when(null);
            }

            return getMetaData(resourceType).then(function (type) {
                var data = [];
                if (type) {
                    data = new spResource.Type(type).getAllMembers();
                }
                return _.map(data, function (d) {
                    return {
                        name: d.getName(),
                        description: d.getDescription(),
                        entity: d.getEntity()
                    };
                });
            });
        }

        /////
        //
        // getReportChooserView
        //
        /////
        function getReportChooserView(chooserType, resourceType, reportEntity, selection, onApplyCallback, onCancelCallback) {
            var resourceTypeId = sp.result(resourceType, 'idP');
            var reportId = resourceTypeId ? sp.result(reportEntity, 'idP') : null;
            var quickSearch = {
                value: '',
                onSearchValueChanged: function () { }
            };
            return {
                templateUrl: 'workflow/workflowParameterView/chooserReportView.tpl.html',
                reportOptions: {
                    reportId: reportId,
                    entityTypeId: resourceTypeId,
                    selectedItems: selection,
                    multiSelect: false,
                    isEditMode: false,
                    newButtonInfo: {},
                    isInPicker: true,
                    isMobile: false,
                    fastRun: true,
                    quickSearch: {
                        quickSearch: quickSearch
                    }
                },
                viewName: chooserType,
                resourceType: resourceTypeId,
                quickSearch: quickSearch,
                allowPickerSwitch: true,
                apply: function () {
                    if (!onApplyCallback) return;
                    var selectedId = sp.result(this, 'reportOptions.selectedItems.0.eid');
                    if (selectedId) {
                        var result = null;
                        var rq = 'name, description, isOfType.name';
                        if (chooserType === 'typeChooser') {
                            rq = rq + ', inherits*.{id, alias}';
                        }
                        spWorkflowService.getCacheableEntity('entity:' + selectedId, selectedId, rq).then(function (e) {
                            if (e) {
                                result = { id: e.idP, name: e.name, entity: e };
                            }
                        }).catch(function () {
                            console.error('error requesting entity for ', selectedId);
                        }).finally(function () {
                            onApplyCallback(result);
                        });
                    } else {
                        onApplyCallback(null);
                    }
                },
                cancel: onCancelCallback || function () { }
            };
        }

        /////
        //
        // getResourceQueryChooserView
        //
        /////
        function getResourceQueryChooserView(chooserType, resourceType, resourceQuery, selection, onPrepareResultCallback, onApplyCallback, onCancelCallback) {
            var resourceTypeId = sp.result(resourceType, 'idP');

            if (!onPrepareResultCallback) {
                onPrepareResultCallback = function() {
                    return [];
                };
            }

            return spWorkflowService.getTemplateReport().then(function(templateReport) {
                var view = getReportChooserView(chooserType, resourceType, templateReport, selection, onApplyCallback, onCancelCallback);
                view.allowPickerSwitch = false;
                view.customDataProvider = function (data) {

                    return spWorkflowService.getCacheableEntity(chooserType + ':' + resourceTypeId, resourceTypeId, resourceQuery).then(function (result) {
                        var selectedId = _.map(selection, 'id');

                        var resultData = onPrepareResultCallback(result);
                        
                        var rows = _.map(resultData, function (r) {
                            return {
                                eid: r.idP,
                                obj: r,
                                values: [{ val: r.name }, { val: r.description }],
                                selected: _.includes(selectedId, r.idP)
                            };
                        });

                        // Quick Search
                        if (data.quickSearch && data.quickSearch.value) {
                            var quickSearch = data.quickSearch.value.toLowerCase();
                            var filtered = _.filter(rows, function (dataRow) {
                                var name = _.get(dataRow, 'values[0].val');
                                return name && _.includes(name.toLowerCase(), quickSearch);
                            });
                            rows = filtered;
                        }

                        // Sorting
                        if (data.meta) {
                            if (!data.meta.rcols) {
                                _.each(data.columnDefinitions, function(col, idx) {
                                    _.set(data.meta, 'rcols.' + col.columnId, { ord: idx });
                                });
                            }
                            var sorting = _.map(data.meta.sort, function (s) {
                                var ord = _.get(data.meta.rcols, s.colid + '.ord');
                                return {
                                    val: 'values[' + ord + '].val',
                                    dir: s.order === 'Ascending' ? 'asc' : 'desc'
                                };
                            });

                            if (sorting && sorting.length) {
                                rows = _.orderBy(rows, _.map(sorting, 'val'), _.map(sorting, 'dir'));
                            }
                        }
                        
                        // Selection
                        _.each(rows, function(r, rowIndex) { _.set(r, 'rowIndex', rowIndex); });

                        view.reportOptions.selectedItems = _.filter(rows, 'selected');
                        
                        return rows;
                    });
                };

                return view;
            });
        }
        
        /////
        //
        // getChooserView
        //
        /////
        function getChooserView(chooserType, resourceType, columns, selection, onLoadCallback, onApplyCallback, onCancelCallback) {
            var sortInfo = {};
            var firstField = sp.result(columns, '0.field');
            if (firstField) {
                sortInfo = { fields: [firstField], directions: ['asc'] };
            }
            var view = {
                templateUrl: 'workflow/workflowParameterView/chooserReportView.tpl.html',
                viewName: chooserType,
                rows: [],
                filteredRows: [],
                quickSearch: {
                    value: '',
                    onSearchValueChanged: function () {
                        view.filteredRows = getFilteredRows(view, columns);
                    }
                },
                gridOptions: {
                        data: 'view.filteredRows',
                        targetvirtualizationThreshold: 10000,
                        multiSelect: false,
                        enableSorting: true,
                        sortInfo: sortInfo,
                        enableColumnResize: false,
                        selectedItems: [],
                        columnDefs: _.map(columns, function (col) {
                            return {
                                field: col.field,
                                displayName: col.displayName,
                                sortable: true,
                                groupable: false,
                                enableCellEdit: false
                            };
                        })
                },
                load: onLoadCallback,
                apply: function () {
                    var selected = _.first(this.gridOptions.selectedItems);
                    onApplyCallback(selected);
                },
                cancel: onCancelCallback || function () { }
            };
            return view;
        }

        /////
        //
        // chooseResource - return a promise for a resource selected via a chooser UI
        //
        /////
        function chooseResource(resourceType, currentSelections, chooserType) {

            var defer = $q.defer();

            var selection = _.castArray(currentSelections || []);

            var onApply = function (selected) {
                defer.resolve(selected);
            };
            
            chooserType = chooserType || 'resourceChooser';
            
            if (!resourceType && chooserType === 'typeChooser') {
                resourceType = 'core:managedType';
            }

            spWorkflowService.getTemplateReport().then(function (templateReport) {
                spWorkflowService.getCacheableType(resourceType).then(function (typeEntity) {
                    var report = sp.result(typeEntity, 'defaultPickerReport') || templateReport;
                    var view = getReportChooserView(chooserType, typeEntity, report, selection, onApply);

                    spViewRegionService.pushView('workflow-properties-sidepanel', view);
                });
            });

            return defer.promise;
        }

        /////
        //
        // chooseReport - special case. see alex.
        //
        /////
        function chooseReport(resourceType, currentSelections) {

            var defer = $q.defer();

            var selection = _.castArray(currentSelections || []);
            var onApply = function (selected) {
                defer.resolve(selected);
            };
            var onPrepareResult = function (result) {
                var reportsForType = [];
                var derivedTypes = spResource.getDerivedTypesAndSelf(result);
                _.forEach(derivedTypes, function (derivedType) {
                    reportsForType = reportsForType.concat(derivedType.definitionUsedByReport);
                });
                return reportsForType;
            };

            spWorkflowService.getCacheableType(resourceType).then(function (typeEntity) {
                var rq = 'name, isOfType.id, definitionUsedByReport.{ name, alias, description }, derivedTypes*.{ id, alias, definitionUsedByReport.{ name, alias, description }}';

                getResourceQueryChooserView('reportChooser', typeEntity, rq, selection, onPrepareResult, onApply).then(function (view) {
                    spViewRegionService.pushView('workflow-properties-sidepanel', view);
                });
            });

            return defer.promise;
        }

        /////
        //
        // openEntityChooser
        //
        /////
        function openEntityChooser(context) {
            var workflow = context.workflow;
            var activity = context.activity || context.entity;
            var parameter = context.parameter;
            var resourceType = context.resourceType || parameter.resourceType;

            console.assert(workflow && activity && parameter && context.chooserType);

            var singleKnownEntity = spWorkflow.getAsSingleKnownEntity(workflow, parameter.expression);

            switch (context.chooserType) {
                case 'parameterChooser':
                    return openParameterChooser(context);
                case 'fieldChooser':
                case 'relChooser':
                    return openPropertyChooser(context);
            }

            return chooseResource(resourceType, singleKnownEntity, context.chooserType).then(function (selected) {
                if (context.apply) {
                    context.apply(selected);
                }
                return selected && selected.name;
            });
        }

        /////
        //
        // openExprResourceChooser
        //
        /////
        function openExprResourceChooser(context) {
            return openEntityChooser(_.extend({}, context, {
                chooserType: 'resourceChooser',
                apply: function (selected) {
                    if (selected) {
                        var parameterName = selected.name;

                        //todo - we need to deal with duplicates in name, whether same entity or different, and taking into account workflow params
                        this.parentView.knownEntitiesParameters = this.parentView.knownEntitiesParameters.concat({ name: selected.name, entity: selected.entity });
                        this.parentView.params = this.parentView.params.concat({
                            name: parameterName,
                            description: selected.entity.description,
                            typeName: 'Entity',
                            entityTypeId: sp.result(selected.entity, 'isOfType.0.idP') || 'core:resource'
                        });
                    }
                }
            }));
        }
        
        /////
        //
        // openSingleKnownEntityChooser
        //
        /////
        function openSingleKnownEntityChooser(context) {
            var callersApply = context.apply;
            context.apply = function (selected) {
                spWorkflow.setSingleKnownEntityExpression(context.workflow, context.activity, context.parameter.argument, selected.entity, selected.name);
                if (callersApply) {
                    callersApply(selected);
                }
            };

            return openEntityChooser(context);
        }
        
        /////
        //
        // openParameterChooser
        //
        /////
        function openParameterChooser(context) {

            var workflow = context.workflow;
            var activity = context.activity || context.entity;
            var parameter = context.parameter;
            var resourceType = context.resourceType || parameter.resourceType;
            var sourceFilter = context.sourceFilter;

            console.assert(workflow && activity && parameter);

            var defer = $q.defer();

            var selection = _(spWorkflow.getWorkflowExpressionParameters(workflow))
                .filter(function (p) {
                    return parameter.expression.expressionString === '[' + p.name + ']';
                })
                .map(function (p) {
                    return _.pick(p, 'id', 'name');
                })
                .value();

            var columns = [
                { field: 'name', displayName: 'Name' },
                { field: 'description', displayName: 'Description' }
            ];

            var onLoad = function () {
                return $q.when().then(function () {
                    return spWorkflow.getExpressionParameters(workflow, activity, sourceFilter);
                });
            };

            var onApply = context.apply || function (selected) {
                if (selected && context.apply) {
                    context.apply(selected);
                }
                defer.resolve(selected && selected.name);
            };

            var view = getChooserView('parameterChooser', resourceType, columns, selection, onLoad, onApply);

            spViewRegionService.pushView('workflow-properties-sidepanel', view);

            return defer.promise;
        }

        /////
        //
        // openSingleParameterChooser
        //
        /////
        function openSingleParameterChooser(context) {

            var callersApply = context.apply;
            context.apply = function (selected) {
                spWorkflow.updateExpressionForParameter(context.workflow, context.activity, context.parameter.argument, '[' + selected.name + ']');
                if (callersApply) {
                    callersApply(selected);
                }
            };
            context.sourceFilter = function (param) {
                var paramType = {
                    baseTypeAlias: param.argumentInstanceArgument.type.nsAlias,
                    entityTypeId: sp.result(param, 'instanceConformsToType.idP') || sp.result(param, 'argumentInstanceArgument.conformsToType.idP')
                };
                var targetType = {
                    baseTypeAlias: context.parameter.argument.type.nsAlias,
                    entityTypeId: sp.result(context.parameter.argument, 'conformsToType.idP')
                };

                //                    console.log('parameterChooser - to filter %o for target type %o', paramType, targetType);

                //todo - check entityTypeId compatibility
                var result =
                    targetType.baseTypeAlias === 'core:objectArgument' ||
                    (paramType.baseTypeAlias === 'core:resourceArgument' && targetType.baseTypeAlias === 'core:resourceListArgument') ||    // You can put a single item into a list argument
                    paramType.baseTypeAlias === targetType.baseTypeAlias;

                return result;
            };

            return openParameterChooser(context);
        }
        
        /////
        //
        // openFunctionChooser
        //
        /////
        function openFunctionChooser() {

            var defer = $q.defer();

            var columns = [
                { field: 'name', displayName: 'Name' },
                { field: 'signature', displayName: 'Signature' },
                { field: 'description', displayName: 'Description' }
            ];

            var onLoad = function() {
                return $q.when().then(function() {
                    return _.filter(spExpressionEditorService.functionList, 'signature');
                });
            };

            var onApply = function (selected) {
                defer.resolve(_.get(selected, 'name'));
            };

            var view = getChooserView('functionChooser', null, columns, [], onLoad, onApply);

            spViewRegionService.pushView('workflow-properties-sidepanel', view);

            return defer.promise;
        }
        
        /////
        //
        // openPropertyChooser
        //
        /////
        function openPropertyChooser(context) {

            //var workflow = context.workflow;
            //var activity = context.activity || context.entity;
            var resourceType = context.resourceType;

            var chooserType = context.chooserType || 'propertyChooser';

            var defer = $q.defer();

            var fn = getProperties;
            var columns = [
                { field: 'name', displayName: 'Name' },
                { field: 'description', displayName: 'Description' }
            ];

            if (chooserType === 'relChooser') {
                columns.push({ field: 'direction', displayName: 'Direction' });
                fn = getRelationships;
            }

            if (chooserType === 'fieldChooser') {
                fn = getFields;
            }

            var onLoad = function () {
                return $q.when().then(function () { return fn(resourceType); });
            };

            var onApply = function (selected) {
                if (selected && context.apply) {
                    context.apply(selected);
                }
                defer.resolve(_.get(selected, 'name'));
            };

            var view = getChooserView(chooserType, resourceType, columns, [], onLoad, onApply);

            spViewRegionService.pushView('workflow-properties-sidepanel', view);

            return defer.promise;
        }
        
        /////
        //
        // openExprPropertyChooser
        //
        /////
        function openExprPropertyChooser(context, resourceTypeId) {
            return openPropertyChooser({
                workflow: context.workflow,
                activity: context.activity,
                parameter: context.parameter,
                resourceType: resourceTypeId
            });
        }

        /////
        //
        // openExprEditor
        //
        /////
        function openExprEditor(context) {
            var workflow = context.workflow;
            var activity = context.activity || context.entity;
            var parameter = context.parameter;
            var resourceType = context.resourceType || parameter.resourceType;

            var defer = $q.defer();

            // we are creating the object then extending it so we can have a ref to it within
            var exprView = {};

            var chooserContext = _.defaults({ resourceType: null, parentView: exprView }, context);
            var choosers = {
                functionChooser: {
                    label: 'fx',
                    iconUrl: null,
                    chooserFn: openFunctionChooser
                },
                propertyChooser: {
                    label: 'property',
                    iconUrl: null,
                    chooserFn: _.partial(openExprPropertyChooser, chooserContext)
                },
                parameterChooser: {
                    label: 'parameter',
                    iconUrl: null,
                    chooserFn: _.partial(openParameterChooser, chooserContext)
                }
            };
            if (!parameter.disableResourceChooser) {
                choosers.resourceChooser = {
                    label: 'record',
                    iconUrl: null,
                    chooserFn: _.partial(openExprResourceChooser, chooserContext)
                };
            }

            exprView = _.extend(exprView, {
                templateUrl: 'workflow/workflowParameterView/expressionEditorView.tpl.html',
                workflow: workflow,
                activity: activity,
                model: parameter.expression.expressionString,
                label: '',//parameter.argument.name,
                expressionType: parameter.expression.isTemplate ? 'template' : '',
                params: parameter.exprParamHints,
                knownEntitiesParameters: [],
                resourceType: resourceType,
                contextId: null,
                expressionInputOptions: {
                    useHintLinks: true,
                    choosers: choosers
                },
                apply: function () {
                    parameter.expression.expressionString = this.model;
                    _.forEach(this.knownEntitiesParameters, function (p) {
                        spWorkflow.addExpressionKnownEntity(parameter.expression, p.entity, p.name);
                    });
                    this.knownEntitiesParameters = [];

                    defer.resolve(this.model);
                },
                cancel: function () {
                    this.model = parameter.expression.expressionString;
                    this.params = parameter.exprParamHints;
                    this.knownEntitiesParameters = [];

                    defer.resolve(null);
                },
                onPopped: function () {
                }
            });

            spViewRegionService.pushView('workflow-properties-sidepanel', exprView);

            return defer.promise;
        }
        
        return exports;
    }
})();