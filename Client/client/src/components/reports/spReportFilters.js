// Copyright 2011-2016 Global Software Innovation Pty Ltd
(function() {
    'use strict';

    /**
    * Module containing report filters.
    * It contains the following filters:
    * <ul>
    *   <li>relatedResource - flattens an object's property values to a comma separated list</li>    
    * </ul>
    *
    * @module spReportFilters    
    */
    angular.module('mod.ui.spReportFilters', [])
        .filter('relatedResource', function() {
            return function(relatedResources) {
                var valuesArr = _.filter(_.values(relatedResources), function(v) {
                    return !_.isEmpty(v);
                });
                if (valuesArr) {
                    valuesArr.sort();
                    return valuesArr.join(', ');
                } else {
                    return '';
                }
            };
        })
        .filter('structureLevels', function () {
            function structureLevelsPathFromString(data) {
                var paths, distinctPaths = [], resultPaths = {};

                if (!data || !_.isString(data)) {
                    return '';
                }
                
                paths = _.uniq(data.split('\u0003'));                

                _.forEach(paths, function (path) {
                    var filteredPaths = _.filter(paths, function (p) {
                        return p.indexOf(path) >= 0;
                    });

                    if (filteredPaths.length === 1) {
                        distinctPaths.push(path);
                    }
                });

                _.forEach(distinctPaths, function (path) {
                    var structureLevels, secLast, last, levelPath;

                    // Strip entity ids
                    structureLevels = _.map(path.split('\u0002'), function(p) {
                        var re = /\d+:(.*)/g;
                        var matches = re.exec(p);

                        if (!matches) {
                            return '?';
                        } else {
                            return matches[1];
                        }
                    });

                    if (structureLevels.length > 2) {
                        secLast = structureLevels[structureLevels.length - 2];
                        last = structureLevels[structureLevels.length - 1];
                        levelPath = secLast + ' > ' + last;                        
                    } else {
                        levelPath = structureLevels.join(' > ');                        
                    }

                    // As we are only showing immediate parents
                    // prevent duplicates from being shown
                    if (!_.has(resultPaths, levelPath)) {
                        resultPaths[levelPath] = true;                    
                    }
                });                

                return _.keys(resultPaths).sort().join(', ');
            }

            function structureLevelsPathFromDict(dict) {
                // Format each value of the object
                var valuesArr = _.map(_.filter(_.values(dict), function(v) {
                    return !_.isEmpty(v);
                }), function(value) {
                    return structureLevelsPathFromString(value);
                });

                // Combine the values
                if (valuesArr) {                    
                    return valuesArr.join(', ');
                } else {
                    return '';
                }
            }

            return function (data) {                
                if (!data) {
                    return '';
                }

                // Handle strings and dictionaries of strings
                if (_.isString(data)) {
                    return structureLevelsPathFromString(data);
                } else if (_.isObject(data)) {
                    return structureLevelsPathFromDict(data);
                } else {
                    return '';
                }                
            };
        })
        .filter('reportDataAsEntities', function() {
            return function(data) {
                var nameColumn,
                    nameColumnIndex = 0;

                if (!data || !data.meta || !data.gdata) {
                    return [];
                }

                // Find the name column
                nameColumn = _.find(_.values(spUtils.result(data, 'meta.rcols')), function(rc) {
                    return rc.entityname;
                });

                if (nameColumn) {
                    nameColumnIndex = nameColumn.ord;
                }

                return _.map(data.gdata, function(row) {
                    var id = row.eid,
                        name = '',
                        rowValue,
                        entity;

                    if (row.values) {
                        rowValue = row.values[nameColumnIndex];
                        if (rowValue) {
                            name = rowValue.val;
                        }
                    }

                    entity = spEntity.fromJSON({
                        id: id,
                        name: name
                    });

                    entity.setDataState(spEntity.DataStateEnum.Unchanged);

                    return entity;
                });
            };
        });
}());