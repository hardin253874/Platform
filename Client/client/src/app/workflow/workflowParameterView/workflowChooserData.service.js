// Copyright 2011-2016 Global Software Innovation Pty Ltd
/*global _, console, angular, spWorkflowConfiguration, spWorkflow, sp, spEntity */

// TODO: have an issue with promises and the apply/cancel functions being callable multiple times
// TODO: don't use the name "apply" for the callback.... is confusing given function's apply function

(function () {
    'use strict';

    angular.module('sp.workflow.parameterViewServices')
        .factory('spWorkflowChooserDataService', spWorkflowChooserDataService);

    function spWorkflowChooserDataService($q, spEntityService, spReportService, spWorkflowService, spExpressionEditorService) {
        var pageSize = 100;
        var maxColumns = 3;

        var exports = {
            getFields : getFields,
            getRelationships: getRelationships,
            getTypes: getTypes,
            getEntityProperties: getEntityProperties,
            getResources: getResources,
            getParameters: getParameters,
            getFunctions: getFunctions,
            getReports: getReports,
        };



        function isLookup(r) {
            var cardinality = (r.cardinality || '').toLowerCase();
            return !r.isReverse && cardinality.indexOf('toone') >= 0 || r.isReverse && cardinality.indexOf('oneto') >= 0;
        }

        function isChoice(r) {
            return r.toTypeType === 'core:enumType';
        }

        function getFields(resourceType, filterText) {

            if (!resourceType) {
                return $q.when(null);
            }

            return spEntityService.getTypeMetadata(resourceType)
                .then(function (type) {

                var data = [], fields, rels, result;

                if (type) {
                    fields = type.allFields();
                    rels = _.filter(type.allRelationships(), function (r) {
                        return isLookup(r) || isChoice(r);
                    });
                    data = _.map(_.sortBy(fields.concat(rels), 'name'), function (item) {
                        return {
                            id: item.id,
                            item: [
                                { value: item.name },
                                { value: item.description }
                            ],
                            name: item.name,
                            isReverse: item.isReverse
                        };
                    });
                }

                return filterAndTrimResults({
                    results: {
                        cols: [
                            { title: 'Name', type: 'String' },
                            { title: 'Description', type: 'String' }
                        ],
                        data: data
                    }
                }, filterText);

            });
        }


        function getRelationships(resourceType, filterText) {

            if (!resourceType) {
                return $q.when(null);
            }

            return spEntityService.getTypeMetadata(resourceType).then(function (type) {

                var data = [], rels;

                if (type) {
                    rels = _.filter(type.allRelationships(), function (r) {
                        return !isLookup(r) && !isChoice(r);
                    });
                    data = _.map(_.sortBy(rels, 'name'), function (item) {

                        return {
                            id: item.id,
                            item: [
                                { value: item.name },
                                { value: item.isReverse ? 'reverse' : 'forward' },
                                { value: item.description }
                            ],
                            name: item.name,
                            isReverse: item.isReverse
                        };
                    });
                }

                return filterAndTrimResults({
                    results: {
                        cols: [
                            { title: 'Name', type: 'String' },
                            { title: 'Direction', type: 'String' },
                            { title: 'Description', type: 'String' }
                        ],
                        data: data
                    }
                }, filterText);
            });
        }


        function getTypes(filterText) {

            return $q.all([
                spWorkflowService.getCacheableEntity('allDefinition', 'definition', 'instancesOfType.{alias,name,description}'),
                spWorkflowService.getCacheableEntity('allEnumTypes', 'enumType', 'instancesOfType.{alias,name,description}')
            ])
                .then(function (entities) {

                    var data = _(entities[0].instancesOfType.concat(entities[1].instancesOfType))
                                .sortBy('name')
                                .map(function (e) {
                                    var description = e.description;
                                    return {
                                        id: e.id(),
                                        item: [
                                            { value: e.name },
                                            { value: description }
                                        ],
                                        name: e.name
                                    };
                                })
                                .value();

                    return filterAndTrimResults({
                        results:
                        {
                            cols: [
                                { title: 'Name', type: 'String' },
                                { title: 'Description', type: 'String' }
                            ],
                            data: data,
                        },
                    }, filterText);
                });
        }


        function getEntityProperties(resourceType, filterText) {

            if (!resourceType) {
                return $q.when(null);
            }

            return $q.all({ fields: getFields(resourceType), rels: getRelationships(resourceType) })
                .then(function (r) {

                    var data = r.fields.results.data.concat(_.map(r.rels.results.data, function (row) {
                        // keep the row object but replace the item array with name and desc fields only
                        return _.extend({}, row, { item: [row.item[0], row.item[2]]});
                    }));

                    // here we are assuming fields has name,description cols and rels has name,direction,description
                    return filterAndTrimResults({
                        results: {
                            cols: r.fields.results.cols,
                            data: data,
                        }
                    }, filterText);
                });
        }

        

        function getResources(resourceType, filterText) {

            if (!resourceType) {
                return $q.when(null);
            }

            var options = reportOptions(filterText);

            return spReportService.runDefaultReportForType(resourceType, options).then(function (results) {

                return {
                    results: {
                        cols: results.cols,
                        data: results.data,
                        moreAvailable:  sp.result(results, 'data.length') >= pageSize
                    }
                };
            });
        }


        function getParameters(context, filterText) {

            var parameters = spWorkflow.getExpressionParameters(context.workflow, context.activity, context.sourceFilter);

            var data = _(parameters)
                        .map(function (p) {
                            return { id: p.idP, item: [
                                {value: p.name},
                                {value: p.description}
                            ], name: p.name };
                        })
                        .sortBy('name')
                        .value();

            return $q.when(filterAndTrimResults({
                results: {
                    cols: [
                        { title: 'Name', type: 'String' },
                        { title: 'Description', type: 'String' }
                    ],
                    data: data
                }
            }, filterText));
        }


        function getFunctions(filterText) {
            var data =  _(spExpressionEditorService.functionList)
                        .map(function (p) {
                            return { id: p.name, item: [
                                {value: p.name},
                                {value: p.signature},
                                {value: p.description}
                            ], name: p.name };
                        })
                        .value();

            return $q.when(filterAndTrimResults(
                {
                    results: {
                        cols: [
                            { title: 'Name', type: 'String' },
                            { title: 'Signature', type: 'String' },
                            { title: 'Description', type: 'String' }
                        ],
                        data: data,
                    }
                },
                filterText));
        }


        function getReports(resourceType, filterText) {
            if (resourceType) {
                var query = 'name, isOfType.id, definitionUsedByReport.{ name, alias, description }, derivedTypes*.{ id, alias, definitionUsedByReport.{ name, alias, description }}';
                return spEntityService.getEntity(resourceType, query, { hint: 'wf-rpt', batch: true }).then(function (result) {
                    var reportsForType = [];
                    var derivedTypes = spResource.getDerivedTypesAndSelf(result);
                    _.forEach(derivedTypes, function (derivedType) {
                        reportsForType = reportsForType.concat(derivedType.definitionUsedByReport);
                    });

                    var data = _.map(reportsForType, function(r) {
                        return {
                            id: r.idP,
                            item: [{ value: r.name }, { value: r.description }],
                            name: r.name
                        };
                    });

                    var reportResult = {
                        results: {
                            cols: [{ title: 'Name', type: 'String' }, { title: 'Description', type: 'String' }],
                            data: data,
                        }
                    };

                    return filterAndTrimResults(reportResult, filterText);
                });
            }

            return $q.when(null);
        }


        function reportOptions(filterText) {
            var result = {
                pageSize: pageSize,
                maxCols: maxColumns
            };

            if (filterText)
                result.quickSearch = filterText;

            return result;
        }


        function filterAndTrimResults(report, filterText) {
            
            var results = sp.result(report, 'results');

            if (!results) {
                throw "report missing results";
            }

            results.data = _.take(sortedFilter(results.data, filterText), pageSize);
            results.moreAvailable = results.data.length > pageSize;

            return report;
        }

        // return a filtered list of items ordered with better matches first
        function sortedFilter(data, filterText) {

            var exactMatches, startsWithMatches, anyMatches;

            if (!data || !filterText) return data;

            filterText = filterText.toLowerCase();

            exactMatches = data.filter(function (datum) {
                return _.find(datum.item, function (item) {
                    var valueString = _.isString(item.value) ? item.value : item.value && item.value.toString() || '';
                    return valueString.toLowerCase() === filterText;
                });
            });
            startsWithMatches = data.filter(function (datum) {
                return _.find(datum.item, function (item) {
                    return _.isString(item.value) && item.value.toLowerCase().indexOf(filterText) === 0;
                });
            });
            anyMatches = data.filter(function (datum) {
                return _.find(datum.item, function (item) {
                    return _.isString(item.value) && item.value.toLowerCase().match(filterText);
                });
            });
            return _.take(_.union(exactMatches, startsWithMatches, anyMatches), pageSize);
        }


        return exports;
    }
}());