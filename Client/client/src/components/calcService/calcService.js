// Copyright 2011-2016 Global Software Innovation Pty Ltd
/*global _, angular, console, sp, spResource, spEntity */

//todo - need to change the compile webapi to support parameters with exprs rather than values and types,
// like what we've done for eval
//todo - be consistent with determining the webapi server url
//todo - consolidate compile and compileExpression
//todo - get the webapi compile handle aliases for context and param entityTypeIds


(function () {
    "use strict";

    angular.module('sp.common.spCalcEngineService', ['mod.common.spWebService', 'mod.common.spEntityService', 'sp.app.settings'])
//        .factory('spCacheService', function ($timeout) {
//            // for the moment a simple absolute expiry based system
//
//            var cache = {};
//
//            return {
//                add: function (key, value, options) {
//                    cache[key] = { value: value, options: options };
//                },
//
//                get: function (key, defaultValue) {
//                    var cacheValue = cache[key];
//                    return cacheValue ? cacheValue.value : defaultValue;
//                }
//            };
//        })
        .factory('spCalcEngineService', function ($http, $q, $interval, spWebService, spAppSettings, spEntityService) {

            var exports = {};

            /**
             * Given a list of aliases or ids, return a promise for a map of the given item to the id.
             */
            function getIdMap(eids) {
                var idMap = _.keyBy(_.map(eids, spEntity.asAliasOrId), _.identity);
                var aliases = _.filter(idMap, function (v) {
                    return _.isString(v);
                });
                return !aliases.length ? $q.when(idMap) :
                    spEntityService.getEntities(aliases, 'id').then(function (entities) {
                        _.forEach(entities, function (e, index) {
                            idMap[aliases[index]] = e.id();
                        });
                        return idMap;
                    });
            }

            function entityToMetaObj(entity) {
                var inherited = spResource.getAncestorsAndSelf(entity);
                var fields = _.flatten(_.invokeMap(inherited, 'getRelationship', 'fields'));
                var relationships = _.flatten(_.invokeMap(inherited, 'getRelationship', 'relationships'));
                var revRelationships = _.flatten(_.invokeMap(inherited, 'getRelationship', 'reverseRelationships'));

                return {
                    entityTypeId: entity.id(),
                    fields: _.invokeMap(fields, 'getName'),
                    relationships: _.compact(_.map(relationships, function (e) {
                        if (e.hideOnFromType) return null;
                        return e.getToName() || sp.result(e, 'e.getName') || null;
                    })),
                    revRelationships: _.compact(_.map(revRelationships, function (e) {
                        if (e.hideOnToType) return null;
                        return e.getFromName() || null;
                    }))
                };
            }

            var compileCache = [];
            var expiryTimer;

            function expireCache() {

                var timeNow = new Date().getTime();
                var oldLength = compileCache.length;
                compileCache = _.reject(compileCache, function (o) {return (timeNow - o.touchedTime) > 30000;});

                if (compileCache.length > 0) {
                    expiryTimer = expiryTimer || $interval(expireCache, 10000);
                } else { // no items remaining
                    if (expiryTimer) {
                        $interval.cancel(expiryTimer);
                        expiryTimer = null;
                    }
                }
            }

            var debouncedExpireCache = _.debounce(expireCache, 10000);

            function normaliseParams(params) {
                // normalise the params for the purpose of caching (we value test)
                return _.map(params, function (p) { return _.pick(p, 'name', 'typeName', 'isList', 'entityTypeId'); });
            }

            function normaliseType(type) {
                if (!type) return '';
                return type.dataType + '|' + type.isList + '|' + type.entityTypeId;
            }
            
            function findInCache(expression, options) {
                var params = normaliseParams(options.params);
                var expected = normaliseType(options.expectedResultType);
                var context = options.context || '';
                var item = _.find(compileCache, function (o) { return o.expression === expression && o.context === context && o.expected === expected && _.isEqual(o.params, params); });
                if (item) {
                    item.touchedTime = new Date().getTime();
                }
                return item;
            }

            function addToCache(expression, options, result) {
                var params = normaliseParams(options.params);
                var expected = normaliseType(options.expectedResultType);
                var context = options.context || '';
                compileCache.push({ expression: expression, params: params, expected: expected, context: context, result: result, touchedTime: new Date().getTime() });
                debouncedExpireCache();
            }

            /**
             * Returns a promise....
             *
             * options.params is an array of { name, typeName, isList, and optional entityTypeId }
             *
             * @param expression
             */
            function compile(expression, options) {

                var cachedResult = findInCache(expression, options);
                if (cachedResult && cachedResult.result) {
                    return $q.when(cachedResult.result);
                }

                if (!expression) {
                    return $q.when({
                        expression: expression,
                        error: 'empty expression'
                    });
                }

                // legacy thing here... caller passing in "typeName" but service wants "type".
                var params = _.map(options.params, function (p) {
                    return { name: p.name, type: p.typeName, isList: p.isList, entityTypeId: p.entityTypeId ? p.entityTypeId.toString() : p.entityTypeId, expr: p.expr };
                });

                return compileExpression(expression, options.context, options.host, params, options.expectedResultType)
                    .then(function (result) {
                        result = {
                            expression: expression,
                            resultType: result.data.resultType,
                            entityTypeId: result.data.entityTypeId,
                            isList: result.data.isList,
                            error: result.data.error
                        };

                        addToCache(expression, options, result);
                        return result;

                    })
                    .catch(function (result) {
                        result = {
                            expression: expression,
                            error: result.status
                        };

                        addToCache(expression, options, result);
                        return result;
                    });
            }

            exports.compile = compile;

            /**
             * Compiles each of the named expressions, using the given parameters.
             *
             * @param namedExpressions - array of objects, expecting name and expression, skips those without
             * @param typedParameters - an array of objects with name, typeName, entityTypeId, isList
             * @returns {promise} a promise for a compile result object with possible fields:
             * expression, resultType, entityTypeId, errors
             */
            function compileParameterExpressions(namedExpressions, typedParameters) {

                function innerCompile(namedExpressions, parameterMap, retries) {

                    if (!namedExpressions.length) {
                        return $q.when([]);
                    }

                    var namedExpression = _.first(namedExpressions);

                    return compile(namedExpression.expression, { params: _.values(parameterMap) })
                        .then(function (firstResult) {

                            // compile the rest of the expressions, plus adding the just compiled named expression as a parameter

                            if (namedExpression.name && !firstResult.error && firstResult.resultType && firstResult.resultType !== 'None') {

                                parameterMap[namedExpression.name] = _.extend(parameterMap[namedExpression.name] || {}, {
                                    name: namedExpression.name,
                                    typeName: firstResult.resultType,
                                    entityTypeId: firstResult.entityTypeId
                                });
                            }

                            return innerCompile(_.tail(namedExpressions), parameterMap, 1).then(function (restResults) {

                                return [firstResult].concat(restResults);
                            });
                        })
                        .then(function (results) {

                            if (retries <= 0) {
                                return results;
                            }

                            // set up to retry any failed compiles, in case some parameters are now available
                            // to make the failed compiles work

                            var expressions = _(namedExpressions)
                                .filter(function (e, index) {
                                    var result = results[index];
                                    return result.error;
                                })
                                .values();

                            if (!expressions.length) {
                                return results;
                            }

                            return innerCompile(expressions, parameterMap, retries - 1).then(function (retryResults) {

                                _.forEach(results, function (result) {
                                    result = _.extend(result, _.find(retryResults, { expression: result.expression }));
                                });

                                return results;
                            });
                        });
                }

                var parameterMap = _(typedParameters)
                    .filter(function (p) {
                        return p.typeName;
                    })
                    .keyBy('name')
                    .value();

                var sortedExpressions = _(namedExpressions)
                    .filter(function (e) {
                        return e.name && e.expression;
                    })
                    .sort(function (a, b) {
                        // a kinda sort... lots of holes but this in combo with the multiple passes should get us there
                        return b.expression.toLowerCase().indexOf(a.name.toLowerCase()) >= 0 ? -1 :
                            a.expression.toLowerCase().indexOf(b.name.toLowerCase()) >= 0 ? +1 : 0;
                    })
                    .value();

//                console.log('compileParameterExpressions',
//                    _.map(namedExpressions, 'name'),
//                    namedExpressions,
//                    typedParameters !== namedExpressions && _.map(typedParameters, 'name'),
//                    typedParameters !== namedExpressions && typedParameters);

                return innerCompile(sortedExpressions, parameterMap, sortedExpressions.length)
                    .then(function (results) {
                        return _.map(namedExpressions, function (e) {
                            var index = _.findIndex(sortedExpressions, { name: e.name });
                            return index >= 0 ? results[index] : { error: 'no result' };
                        });
                    });
            }
            exports.compileParameterExpressions = compileParameterExpressions;

            /**
             * Returns a promise...
             * @param params
             */
            function getParameterTypeMetaData(params) {
                var entityIds = _(params).filter({ typeName: 'Entity'}).map('entityType').value();
                return spEntityService.getEntities(entityIds).then(_.partialRight(_.map, entityToMetaObj));
            }
            exports.getParameterTypeMetaData = getParameterTypeMetaData;

            /**
             * Get an array of auto-complete suggestions.
             * Returns a promise...
             * @param params
             */
            function getEntityTypeHints(typeId, scriptHost) {
                var rq = spResource.makeTypeRequest({ scriptInfo: true });
                return spEntityService.getEntity(typeId, rq, {hint:'calc hint'}).then( function (typeEntity) {
                    var type = new spResource.Type(typeEntity);
                    var opts = { hideCalculatedFields: scriptHost === 'Any' || scriptHost === 'Report' };
                    var membs = type.getAllMembers(opts);
                    var hints = _.map(membs, function (m) { return m.getScriptName(); });
                    var sorted = _.sortBy(hints);
                    return sorted;
                });
            }
            exports.getEntityTypeHints = getEntityTypeHints;


            /**
             * Return a promise for the evaluated expression. This calls a webapi service to perform the evaluation.
             *
             * @param expr - the expression text to evaluate
             * @param context - optional id of the root context for the expression
             * @param host - a host indicator... (not sure of the effect of this!)
             * @param params - an array of  name, type, optional entityTypeId, and either value or expr .
             * @returns {promise} for an eval results object containing either the result value and type or an error
             *
             * @note on params - if a parameter provide an expr rather than a value then it will first be evaluated
             * using the set of params that do have values.
             */
            function evalExpression(expr, context, host, params) {
                return $http({
                    method: 'POST',
                    url: spWebService.getWebApiRoot() + '/spapi/data/v1/calceditor/eval',
                    data: {
                        expr: expr,
                        context: context ? context + '' : context,
                        host: host,
                        params: params
                    },
                    headers: spWebService.getHeaders()
                });
            }
            exports.evalExpression = evalExpression;

            /**
             * See evalExpression
             *
             * @returns {promise} for compile results object containing either the result type or an error
             */

            function compileExpression(expr, context, host, params, expectedResultType) {
                return $http({
                    method: 'POST',
                    url: spWebService.getWebApiRoot() + '/spapi/data/v1/calceditor/compile',
                    data: {
                        expr: expr,                                     // eg. '1+2'
                        context: context ? context + '' : context,      // eg. 12345   (typeID of context for resolving field names)
                        host: host,                                     // optional. eg. 'Report','Evaluate','Any'  (support rules for this calc engine)
                        params: params,                                 // optional. See ExpressionParameter data contract
                        expectedResultType: expectedResultType          // optional. eg. {'dataType':'String','isList':'false','entityTypeId':1234} See ExpressionType data contract
                    },
                    headers: spWebService.getHeaders()
                });
            }
            exports.compileExpression = compileExpression;

            
             /**
             * Evaluates the specified expressions against the specified context entity.
             *
             * @param contextEntity - entity, the context entity
             * @param expressions - object, a dictionary of expressions
             * @returns {promise} a promise for a dictionary of evaluation results
             */
            function evaluateExpressions(contextEntity, expressions) {
                if (!contextEntity || !expressions) {
                    return $q.when({});
                }

                var entityData = spEntityService.packageEntityNugget(contextEntity);

                return $http({
                    method: 'POST',
                    url: spWebService.getWebApiRoot() + '/spapi/data/v1/calcEngine/evalExpressions',
                    data: {
                        contextEntity: entityData, 
                        expressions: expressions
                    },
                    headers: spWebService.getHeaders()
                }).catch(function(error) {                    
                    console.error('spCalcEngineService.evaluateExpressions error: ' + (sp.result(error, 'status') || error));
                    throw error;
                });
            }
            exports.evaluateExpressions = evaluateExpressions;

            return exports;
        });
})();
