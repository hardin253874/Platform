// Copyright 2011-2016 Global Software Innovation Pty Ltd
/*global _, angular, console, sp, spEntity, jsonLookup, jsonString, jsonBool, spWorkflowConfiguration */

/**
 * Much of this service is about working on a workflow "model" object which wraps a
 * workflow entity object graph plus other support config and state tracking data.
 *
 * The model object has the following properties (at least):

 *  entity - the root entity of the workflow entity graph that includes all related entities.

 *  selectedEntity - the "selected entity" that may be the workflow, an activity, or
 *      any other related entity to the workflow

 *  updateCount - a counter that is zero for an unchanged workflow and incremented with each modification.
 *      This may be used in angular watch expressions to monitor for changes to the workflow.
 */

// todo - double check that removed activities are being deleted....

// todo - automatically add (not in seq) activities to the workflow when creating them
// todo - name the async routines (promise returning) something to indicate that fact
// todo - consider adding some "ById" variations on the functions so we can pass in activity and seq ids rather than having the caller do the look ups all the time
// todo - create an index for activityTypes on the alias

// todo - refactoring - rename updateExpressionForParameter to updateArgumentExpression and have it support taking id or alias or eid or entity for the argument
// todo - refactoring - remove updateParameterExpression

var spWorkflow;
(function (spWorkflow) {
    'use strict';

    // aliases for commonly used utility functions
    var forEach = _.forEach;
    var assert = console.assert.bind(console);

    //
    // The interface
    //

    spWorkflow.undo = undo;
    spWorkflow.redo = redo;
    spWorkflow.initialiseHistory = initialiseHistory;
    spWorkflow.setBookmark = setBookmark;
    spWorkflow.resetHistory = resetHistory;
    spWorkflow.makeIdOrAlias = makeIdOrAlias;
    spWorkflow.matchingEids = matchingEids;
    spWorkflow.hasEid = hasEid;
    spWorkflow.isResourceArgument = isResourceArgument;
    spWorkflow.findWorkflowComponentEntity = findWorkflowComponentEntity;
    spWorkflow.getUniqueName = getUniqueName;
    spWorkflow.getExitPoints = getExitPoints;
    spWorkflow.getSequenceUsingExit = getSequenceUsingExit;
    spWorkflow.isFreeExit = isFreeExit;
    spWorkflow.getExtendedProperties = getExtendedProperties;
    spWorkflow.setExtendedProperties = setExtendedProperties;
    spWorkflow.mergeExtendedProperties = mergeExtendedProperties;
    spWorkflow.addActivityArgument = addActivityArgument;
    spWorkflow.addSetMemberArg = addSetMemberArg;
    spWorkflow.addSetFieldArgs = addSetFieldArgs;
    spWorkflow.addSetRelArgs = addSetRelArgs;
    spWorkflow.addSetRelValueArg = addSetRelValueArg;
    spWorkflow.getActivityArguments = getActivityArguments;
    spWorkflow.getExpression = getExpression;
    spWorkflow.getActivityParameters = getActivityParameters;
    spWorkflow.addExpressionKnownEntity = addExpressionKnownEntity;
    spWorkflow.addSingleExpressionKnownEntity = addSingleExpressionKnownEntity;
    spWorkflow.getAsSingleKnownEntity = getAsSingleKnownEntity;
    spWorkflow.removeUnusedExpressionKnownEntities = removeUnusedExpressionKnownEntities;
    spWorkflow.updateExpressionForParameter = updateExpressionForParameter;
    spWorkflow.updateParameterExpression = updateParameterExpression;
    spWorkflow.updateParameterExpressionByName = updateParameterExpressionByName;
    spWorkflow.updateEntityExpressionForParameter = updateEntityExpressionForParameter;
    spWorkflow.setSingleKnownEntityExpression = setSingleKnownEntityExpression;
    spWorkflow.updateParameterEntityExpression = updateParameterEntityExpression;
    spWorkflow.updateParameterEntityExpressionByName = updateParameterEntityExpressionByName;
    spWorkflow.updateArgument = updateArgument;
    spWorkflow.findWorkflowExpressionParameter = findWorkflowExpressionParameter;
    spWorkflow.setArgumentInstanceConformsToType = setArgumentInstanceConformsToType;
    spWorkflow.getParameterAndExpression = getParameterAndExpression;
    spWorkflow.getPriorActivities = getPriorActivities;
    spWorkflow.getExpressionParameters = getExpressionParameters;
    spWorkflow.updateExpressionParameters = updateExpressionParameters;
    spWorkflow.updateExpressionParameter = updateExpressionParameter;
    spWorkflow.getActivityValidationMessages = getActivityValidationMessages;
    spWorkflow.cleanExpressions = cleanExpressions;
    spWorkflow.setDefaultActivityNames = setDefaultActivityNames;
    spWorkflow.activityTypeEntity = activityTypeEntity;
    spWorkflow.activityData = activityData;
    spWorkflow.removeSequence = removeSequence;

    //@deprecated
    spWorkflow.getParameterViewModel = getParameterViewModel;
    spWorkflow.getActivityParameterMap = getActivityParameterMap;
    spWorkflow.updateActivityParameters = updateActivityParameters;
    spWorkflow.getWorkflowExpressionParameters = getWorkflowExpressionParameters;

    //
    // The functions
    //

    function activityData(workflowEntity, activity) {
        return _.defaults(workflowEntity.activities[activity.idP] || (workflowEntity.activities[activity.idP] = {}),
            {parameters: {}, exprParamHints: [], debugString: activity.debugString});
    }

    function removeSequence(workflow, seq) {

        assert(seq && _.isObject(seq));

        var e = workflow.entity;

        if (seq.alias() === spWorkflowConfiguration.aliases.firstActivity) {
            // This sequence represents the firstActivity - so remove the relationship
            e.setFirstActivity(null);

        } else {

            if (!_.includes(e.getTransitions(), seq) && !_.includes(e.getTerminations(), seq)) {
                // quietly do nothing
                return workflow;
            }

            e.transitions.deleteEntity(seq);
            e.terminations.deleteEntity(seq);

            if (seq.dataState !== spEntity.DataStateEnum.Create) {
                seq.dataState = spEntity.DataStateEnum.Delete;
            } else {
                // there is no good way of deleting a freshly created entity
                seq.fromActivity = null;
                seq.fromExitPoint = null;
                seq.toExitPoint = null;
            }
        }

        spWorkflow.setBookmark(workflow);
        return workflow;
    }

    function bookmarks(workflow) {
        return _.filter(workflow.entity.graph.history._undoList, 'isBookmark');
    }

    function changeCount(workflow) {
        //the following is using some "internals" of the entity graph and history to work this out
        //so we'll change this once there is a public interface for change count
        var history = workflow.entity.graph.history;
        var bookmark = _.first(bookmarks(workflow));

        //        if (_.isNaN(bookmark && (history._undoList.length - _.indexOf(history._undoList, bookmark) - 1) || 0)) {
        //            debugger;
        //        }

        return bookmark && (history._undoList.length - _.indexOf(history._undoList, bookmark) - 1) || 0;
    }

    function logChangeCounts(workflow) {
        var history = workflow.entity.graph.history;

        console.log('workflow.setBookmark: wf=%s, updateCount=%s, bookmarks=%s, changeCount=%s, undo=%s, redo=%s',
            workflow.entity.name, workflow.updateCount, bookmarks(workflow).length,
            changeCount(workflow), history._undoList.length, history._redoList.length);

        console.log('workflow.setBookmark: bookmarks=',
            _.reduce(bookmarks(workflow), function (s, bm, index) {
                return s + ', ' + index + '=' + bm.changedSinceBookmark();
            }, ''));

        //        console.trace();
    }

    function initialiseHistory(workflow) {

        console.assert(bookmarks(workflow).length === 0);
        //        if (bookmarks(workflow).length !== 0) {
        //            debugger;
        //        }

        workflow.entity.graph.history.addBookmark();

        if (_.isUndefined(workflow.updateCount)) {
            Object.defineProperties(workflow, {
                'updateCount': {
                    get: function () {
                        return changeCount(this);
                    },
                    enumerable: true
                }
            });
        }

        //        logChangeCounts(workflow);

        return workflow;
    }

    function resetHistory(workflow) {
        workflow.entity.graph.history.clear();
        setBookmark(workflow);
        return workflow;
    }

    function setBookmark(workflow) {
        var history = workflow.entity.graph.history;

        if (bookmarks(workflow).length === 0) {
            initialiseHistory(workflow);
        }

        var lastBookmark = _.last(bookmarks(workflow));

        if (!lastBookmark._end && lastBookmark.changedSinceBookmark()) {
            lastBookmark.endBookmark();
        }
        if (lastBookmark._end) {
            history.addBookmark();
        }

        //        logChangeCounts(workflow);

        return workflow;
    }

    function undo(workflow) {
        var history = workflow.entity.graph.history;
        var lastBookmark = _.findLast(bookmarks(workflow), function (bm) {
            return bm.changedSinceBookmark();
        });
        if (lastBookmark) {
            history.undoBookmark(lastBookmark);
            history.redo(); // redo a single step should be the bookmark we just undid
            // - but just the start of it... the theory anyway
            // note we cannot add a new bookmark as that clears the redo list
            logChangeCounts(workflow);
        }
    }

    function redo(workflow) {
        var history = workflow.entity.graph.history;
        if (history.canRedo()) {
            history.redoBookmark();
            logChangeCounts(workflow);
        }
    }

    function makeIdOrAlias(idOrAlias) {
        var temp;
        if (_.isString(idOrAlias) && !isNaN((temp = parseInt(idOrAlias, 10)))) {
            return temp;
        }
        return idOrAlias;
    }

    /**
     * Returns truthy if both are entities with matching eid.
     */
    function matchingEids(e1, e2) {
        return e1 && e2 && e1.eidP.matches(e2.eidP);
    }

    /**
     * Returns truthy if the given entity has eid matching the given eid.
     * The order of the arguments is handy for use in partials.
     */
    function hasEid(eid, e) {
        return e && e.eidP.matches(eid);
    }

    function isResourceArgument(argEntity) {
        var typeAlias = argEntity.getType().getNsAlias();
        return typeAlias === spWorkflowConfiguration.aliases.resourceArgument || typeAlias === spWorkflowConfiguration.aliases.resourceListArgument;
    }

    function activityTypeEntity(workflow, activity) {
        return spEntity.findByEid(workflow.activityTypes, activity.getType());
    }

    function findWorkflowComponentEntity(workflow, idOrAlias) {

        if (idOrAlias === spWorkflowConfiguration.aliases.firstActivity) {
            // the first activity is only a relationship.... let's just use
            // an unattached (i.e. outside our graph) entity
            var dummy = spEntity.fromJSON({ id: idOrAlias, typeId: 'core:dummy', name: '', description: '' });
            dummy.dataState = spEntity.DataStateEnum.Unchanged;
            return dummy;
        }

        // Using the general graph walker, but might profile and see how it performs as we could just
        // search out the known relationships.
        return idOrAlias && spEntity.findInGraph(workflow.entity, idOrAlias);
    }

    function getUniqueName(existingNames, baseName) {
        var counter = 1;
        if (!_.includes(existingNames, baseName)) {
            return baseName;
        }
        while (_.includes(existingNames, baseName + counter)) {
            counter += 1;
        }
        return baseName + counter;
    }

    function isDefaultExit(exitPoint) {
        return exitPoint.getIsDefaultExitPoint();
    }

    function getExitPoints(workflow, activity) {

        if (!activity) {
            return [];
        }

        assert(activity && activity.getType, 'bad activity arg', activity);

        var activityType = activityTypeEntity(workflow, activity);
        var exitPoints = _(activityType.getExitPoints())
            .concat(activity.getExitPoints())
            .uniqBy(function (e) {
                return e.aliasOrId();
            })
            .value();
        var defaultExitPoints = _.filter(exitPoints, isDefaultExit);

        exitPoints = _(exitPoints)
            .reject(function (e) {
                return _.includes(defaultExitPoints, e);
            })
            .sortBy(exitPoints, function (e) {
                return e.getExitPointOrdinal();
            })
            .value();

        return defaultExitPoints.concat(exitPoints);
    }

    function getSequenceUsingExit(workflow, activity, exitPoint) {
        var seqs = workflow.entity.getTransitions().concat(workflow.entity.getTerminations());
        return _.find(seqs, function (seq) {
            //console.log('getSequenceUsingExit: checking ', spEntity.toJSON(seq), activity.eid(), exitPoint.eid());
            return activity.eid().matches(sp.result(seq, 'getFromActivity.eid')) &&
                exitPoint.eid().matches(sp.result(seq, 'getFromExitPoint.eid'));
        });
    }

    function isFreeExit(workflow, activity, exitPoint) {
        return !getSequenceUsingExit(workflow, activity, exitPoint);
    }

    function getExtendedProperties(entity) {
        return JSON.parse(entity.getField(spWorkflowConfiguration.aliases.designerData) || '{}');
    }

    function setExtendedProperties(entity, propsObj) {
        entity.setField(spWorkflowConfiguration.aliases.designerData, JSON.stringify(propsObj || {}, null, 0), 'String');
    }

    function mergeExtendedProperties(entity, propsObj) {
        setExtendedProperties(entity, _.merge(getExtendedProperties(entity), propsObj));
    }

    function getNextSetMemberBaseName(workflow, activity) {
        var argsByIndex = _.map(activity.inputArguments, function (e) {
            var parts = e.name.split('_');
            var index = parseInt(_.first(parts), 10);
            return { index: !_.isNaN(index) ? index : 0, argEntity: e };
        });

        return (1 + Math.max.apply(null, [0].concat(_.map(argsByIndex, 'index')))).toString();
    }

    function addActivityArgument(workflow, activity, relAlias, typeAlias, name) {
        var argEntity = spEntity.fromJSON({
            typeId: typeAlias,
            name: name || jsonString(''),
            description: jsonString(''),
            defaultExpression: jsonString(null),
            argumentIsMandatory: false,
            // The following 'registers' the lookup for the client side code, but nothing goes to server if never set
            // Just saying as this lookup is only applic for resource and resourceList type args
            conformsToType: jsonLookup(null)
        });
        activity.getRelationship(relAlias).add(argEntity);
        updateExpressionForParameter(workflow, activity, argEntity, '', false);
        setBookmark(workflow);
        return argEntity;
    }

    function addInputArgument(workflow, activity, typeAlias, name) {
        return addActivityArgument(workflow, activity, spWorkflowConfiguration.aliases.inputArguments, typeAlias, name);
    }

    /**
     * Add a new activity argument for setting an entity member, automatically determining the base name
     * to be used for the group of arguments.
     * This is used in multi-member setting activities such as createActivity and updateActivity.
     * Once the type of the member (field, versus rel etc) has been determined you then use addSetMemberArg on the base name
     * to add the arguments required to fill out the information for the member type.
     */
    function addSetMemberArg(workflow, activity) {
        var baseName = getNextSetMemberBaseName(workflow, activity);
        addInputArgument(workflow, activity, spWorkflowConfiguration.aliases.resourceArgument, baseName);
        return baseName;
    }

    function addSetFieldArgs(workflow, activity) {

        var baseName = getNextSetMemberBaseName(workflow, activity);

        addInputArgument(workflow, activity, spWorkflowConfiguration.aliases.resourceArgument, baseName);
        addInputArgument(workflow, activity, spWorkflowConfiguration.aliases.objectArgument, baseName + '_value');

        return baseName;
    }

    function addSetRelArgs(workflow, activity) {

        var baseName = getNextSetMemberBaseName(workflow, activity);

        addInputArgument(workflow, activity, spWorkflowConfiguration.aliases.resourceArgument, baseName);
        addInputArgument(workflow, activity, spWorkflowConfiguration.aliases.boolArgument, baseName + '_reverse');
        addInputArgument(workflow, activity, spWorkflowConfiguration.aliases.boolArgument, baseName + '_replace');

        return baseName;
    }

    function addSetRelValueArg(workflow, activity, baseName, valueArgType) {
        var valueArgName = baseName + '_value';
        var existingArgIndexes = _(activity.inputArguments)
            .filter(function (e) {
                return e.name.indexOf(valueArgName) === 0;
            })
            .map(function (e) {
                var parts = e.name.split('_');
                return parts.length > 2 ? parseInt(parts[2], 10) : 0;
            })
            .value();
        var nextIndex = 1 + Math.max.apply(null, [0].concat(existingArgIndexes));
        valueArgName = valueArgName + '_' + nextIndex;
        addInputArgument(workflow, activity, valueArgType || spWorkflowConfiguration.aliases.resourceArgument, valueArgName);
        return valueArgName;
    }

    /**
     * Return all activityArguments for the activity for the given relationship (default to InputArguments)
     * for the activity instance and its type.
     */
    function getActivityArguments(workflow, activity, relAliasOrId) {
        relAliasOrId = relAliasOrId || spWorkflowConfiguration.aliases.inputArguments;
        return activity.getRelationship(relAliasOrId).concat(activityTypeEntity(workflow, activity).getRelationship(relAliasOrId));
    }

    /**
     * Find and return the expression that applies to the given activity and activityArgument.
     * If an expression entity doesn't already exist then create an empty one. Logically this hasn't modified
     * the workflow so we DON'T set dirty.
     */
    function getExpression(workflow, activityEntity, argEntity) {

        var exprEntity = _.find(activityEntity.expressionMap, function (exp) {
            return matchingEids(exp.argumentToPopulate, argEntity);
        });

        if (!exprEntity) {
            exprEntity = spEntity.fromJSON({
                typeId: spWorkflowConfiguration.aliases.expression,
                argumentToPopulate: jsonLookup(argEntity),
                expressionString: jsonString(null),
                isTemplateString: false,
                wfExpressionKnownEntities: []
            });
            activityEntity.expressionMap.add(exprEntity);
        }

        return exprEntity;
    }

    /**
     * Return the set of {argument, expression} pairs for each argument for the given
     * activity that match the given "where" filter, where "where" is
     * either undefined -> all arguments and their expressions
     * or an object with either aliasOrId or name property to match.
     */
    function getActivityParameters(workflow, activity, relAliasOrId, where) {
        return _(getActivityArguments(workflow, activity, relAliasOrId))
            .filter(function (e) {
                return (!where ||
                    (where.aliasOrId && e.aliasOrId() === sp.coerseToNumberOrLeaveAlone(where.aliasOrId)) ||
                    (where.name && e.name === where.name));
            })
            .map(function (e) {
                return {
                    argument: e,
                    expression: getExpression(workflow, activity, e)
                };
            })
            .value();
    }

    function createNamedReference(entity, name) {
        var namedReference = spEntity.createEntityOfType('namedReference');
        namedReference.setName(name || entity.name || 'unnamed');
        namedReference.setLookup('referencedEntity', entity);

        return namedReference;
    }

    function addExpressionKnownEntity(expression, entity, name) {
        var namedReference = createNamedReference(entity, name);
        expression.wfExpressionKnownEntities.add(namedReference);
    }

    function addSingleExpressionKnownEntity(expression, entity, name) {
        var knownEntities = expression.wfExpressionKnownEntities;
        knownEntities.clear();
        addExpressionKnownEntity(expression, entity, name);
    }

    /**
     * Return the related entity if this is a "single known entity" expression, otherwise return null.
     */
    function getAsSingleKnownEntity(workflow, exprEntity) {
        if (exprEntity.wfExpressionKnownEntities.length !== 1) return null;

        var namedReference = exprEntity.wfExpressionKnownEntities[0];

        return namedReference.name && exprEntity.expressionString.trim() === '[' + namedReference.name + ']' ? namedReference.referencedEntity : null;
    }

    function removeUnusedExpressionKnownEntities(workflow) {
        forEach(workflow.entity.containedActivities, function (a) {
            forEach(a.expressionMap, function (e) {

                e.wfExpressionKnownEntities.remove(function (ke) {
                    if ((!ke.name || ke.name.length === 0) ||
                        (e.expressionString || '').indexOf('[' + ke.name + ']') < 0) {      // no name, no known entity
                        console.log('dropping unused expression known entity %s from expression %s', ke.name, e.expressionString, spEntity.toJSON(e));
                        return true;
                    }
                    return false;
                });
            });
        });
    }

    /**
     * Convert a "template" style expression to the normal formula style expression string.
     *
     * WARNING: this function was whipped up to support the intg tests that used template
     * expressions as the backend is dropping support for them. However, this function
     * needs more tests and thinking through boundary conditions, embedded delimiters and all that.
     *
     * @param templateString
     * @returns {string}
     */
    function exprFromTemplate(templateString) {
        var expr = "'" + templateString.replace(/{{(.*?)}}/g, "' + $1 + '") + "'";
        //        console.log('Converted template expression "%s" to "%s"', templateString, expr);
        return expr;
    }

    /**
     * Update the expression with the given value.
     */
    function updateExpressionForParameter(workflow, activity, argEntity, value, isTemplate) {
        var expression = getExpression(workflow, activity, argEntity);

        // we no longer support isTemplate in the backend
        if (isTemplate) {
            value = exprFromTemplate(value);
            isTemplate = false;
        }

        var changed = expression.expressionString !== value || expression.isTemplateString !== isTemplate;

        expression.expressionString = value;
        expression.isTemplateString = isTemplate;

        if (changed) setBookmark(workflow);
        return expression;
    }

    function updateParameterExpression(workflow, activity, relId, parameterAlias, value, isTemplate) {
        var argEntity = _.find(getActivityArguments(workflow, activity, relId), function (p) {
            return p.eid().matches(parameterAlias);
        });
        if (!argEntity) {
            console.error('updateParameterExpression: parameter %s doesn\'t exist', parameterAlias);
            throw 'activity ' + activity.getName() + ' doesn\'t have parameter ' + parameterAlias;
        }
        return updateExpressionForParameter(workflow, activity, argEntity, value, isTemplate);
    }

    function updateParameterExpressionByName(workflow, activity, relId, parameterName, value, isTemplate) {
        var argEntity = _.find(getActivityArguments(workflow, activity, relId), function (p) {
            return p.name === parameterName;
        });
        if (!argEntity) {
            console.error('updateParameterExpression: parameter %s doesn\'t exist', parameterName);
            throw 'activity ' + activity.name + ' doesn\'t have parameter ' + parameterName;
        }
        return updateExpressionForParameter(workflow, activity, argEntity, value, isTemplate);
    }

    /**
     * Update the expression, creating one if necessary, for the activity parameter
     * with the given idOrAlias.
     */
    function updateEntityExpressionForParameter(workflow, activity, argEntity, other, replace) {

        // we used to use a different expression type that was a single relationship to the other entity
        // but now we use the standard expression and add the entity as a "known entity".

        // urg
        var substName = other.name || 'unnamed';
        var expression = updateExpressionForParameter(workflow, activity, argEntity, '[' + substName + ']', false);
        addExpressionKnownEntity(expression, other, substName);
        return expression;
    }

    function setSingleKnownEntityExpression(workflow, activity, argEntity, other, name) {

        var expression = updateExpressionForParameter(workflow, activity, argEntity, '[' + (name || 'unnamed') + ']', false);
        addSingleExpressionKnownEntity(expression, other, name);
        return expression;
    }

    function updateParameterEntityExpression(workflow, activity, relId, parameterAlias, other) {
        var argEntity = _.find(getActivityArguments(workflow, activity, relId), function (p) {
            return p.eid().matches(parameterAlias);
        });
        if (!argEntity) {
            console.error('updateParameterEntityExpression: parameter %s doesn\'t exist', parameterAlias);
            throw 'activity ' + activity.getName() + ' doesn\'t have parameter ' + parameterAlias;
        }
        return updateEntityExpressionForParameter(workflow, activity, argEntity, other);
    }

    function updateParameterEntityExpressionByName(workflow, activity, relId, parameterName, other) {
        var argEntity = _.find(getActivityArguments(workflow, activity, relId), function (p) {
            return p.name === parameterName;
        });
        if (!argEntity) {
            console.error('updateParameterEntityExpression: parameter %s doesn\'t exist', parameterName);
            throw 'activity ' + activity.name + ' doesn\'t have parameter ' + parameterName;
        }
        return updateEntityExpressionForParameter(workflow, activity, argEntity, other);
    }

    function updateArgument(workflow, activity, argEntity, details) {
        argEntity.name = details.name;
        argEntity.description = details.description;
        if (details.conformsToType && details.conformsToType.id) {
            argEntity.conformsToType = details.conformsToType.id;
        }
        if (details.isConstant) {
            var other = spEntity.fromJSON({ id: details.value, name: details.text });
            updateEntityExpressionForParameter(workflow, activity, argEntity, other);
        } else {
            updateExpressionForParameter(workflow, activity, argEntity, details.text, details.isTemplate);
        }
        return argEntity;
    }

    function findWorkflowExpressionParameter(workflow, activity, argAliasOrId) {

        var argInst = _.find(workflow.entity.expressionParameters, function (p) {
            if (!p || !p.argumentInstanceArgument || !p.argumentInstanceActivity)
                return false;

            return p.argumentInstanceArgument.eidP.matches(argAliasOrId) &&
                p.argumentInstanceActivity.eidP.matches(activity.eidP);
        });

        if (argInst) return argInst;

        // otherwise see if one of the runtime parameters and simulate the argInst

        if (activity !== workflow.entity) return null; // only have runtime params against the workflow entity

        var workflowType = sp.findByKey(workflow.activityTypes, 'alias', spWorkflowConfiguration.aliases.workflow);
        var argEntity = _.find(workflowType && workflowType.runtimeProperties || [], function (argEntity) {
            return argEntity.eidP.matches(argAliasOrId);
        });
        return argEntity && spEntity.fromJSON({
            typeId: spWorkflowConfiguration.aliases.argumentInstance,
            name: jsonString(argEntity.name),
            description: jsonString(argEntity.description),
            argumentInstanceArgument: jsonLookup(argEntity),
            instanceConformsToType: jsonLookup(null),

            // keep this out of the workflow's entity graph so use id only
            argumentInstanceActivity: jsonLookup(workflow.entity.idP)
        });
    }

    function setArgumentInstanceConformsToType(workflow, activity, argAliasOrId, typeId) {
        var p = findWorkflowExpressionParameter(workflow, activity, argAliasOrId);
        if (p && !hasEid(typeId, p.instanceConformsToType)) {
            p.instanceConformsToType = typeId;
            setBookmark(workflow);
        }
    }

    //@deprecated
    function getParameterViewModel(argEntity, expression) {

        function getIdAndName(e) {
            return e ? { id: e.id(), name: e.getName() } : {};
        }

        var parameterViewModel = {
            id: argEntity.aliasOrId(),
            name: argEntity.getName(),
            text: argEntity.getDefaultExpression(),
            type: argEntity.getType().alias(),
            description: argEntity.getDescription() || 'no description',
            conformsToType: getIdAndName(argEntity.getLookup(spWorkflowConfiguration.aliases.conformsToType))
        };

        if (expression) {
            if (expression.getType().alias() === spWorkflowConfiguration.aliases.entityExpression) {
                console.error('ERROR - not using entityExpressions any more??!!');
                throw new Error('ERROR - not using entityExpressions any more??!!');
                //                var other = expression.getOtherEntity();
                //                parameterViewModel.text = other && other.getName() || '';
                //                parameterViewModel.value = other && other.id() || 0;
                //                parameterViewModel.isConstant = true;
            } else {
                parameterViewModel.text = expression.getExpressionString();
                parameterViewModel.isTemplate = expression.getIsTemplateString();
            }
        }

        return parameterViewModel;
    }

    function getParameterAndExpression(workflow, activity, relAlias, alias) {
        return _(getActivityArguments(workflow, activity, relAlias))
            .filter(_.partial(hasEid, alias))
            .map(function (a) {
                return getParameterViewModel(a, getExpression(workflow, activity, a));
            })
            .first();
    }

    //@deprecated
    function getActivityParameterMap(workflow, activity, relAlias) {

        return _(getActivityArguments(workflow, activity, relAlias))
            .map(function (a) {
                return getParameterViewModel(a, getExpression(workflow, activity, a));
            })
            .keyBy(function (p) {
                return p.id;
            })
            .value();
    }

    //@deprecated
    function updateActivityParameters(workflow, activity, parameters) {

        _.forEach(parameters, function (p) {
            if (p.isConstant) {
                var other = spEntity.fromJSON({ id: p.value, name: p.text });
                updateParameterEntityExpression(workflow, activity, spWorkflowConfiguration.aliases.inputArguments, p.id, other);
            } else {
                updateParameterExpression(workflow, activity, spWorkflowConfiguration.aliases.inputArguments, p.id, p.text, p.isTemplate);
            }
        });
    }

    /**
     * Return the list of activities in the workflow that are before the given
     * activity. This is useful if you wish to determine the workflow parameters
     * based on activity outputs that are available for input to a given activity.
     */
    function getPriorActivities(workflow, activity, priorActivities) {
        // return a list of all the activities prior to the given

        // the list to build into, only passed in the recursive call
        priorActivities = priorActivities || [];

        var id = activity.idP;

        // find any transition ending at this activity and add the previous act to the list
        // and recursively add its priors.

        forEach(workflow.entity.transitions, function (t) {
            var to = t.toActivity;
            if (to && to.idP === id) {
                var from = t.fromActivity;
                if (from && priorActivities.indexOf(from) < 0) {
                    priorActivities.push(from);
                    getPriorActivities(workflow, from, priorActivities);
                }
            }
        });

        return priorActivities;
    }

    /**
     * Get the expression parameters for the workflow. These are mainly the argumentInstance entities
     * on the workflow representing the input, vars and act outs for the workflow.
     * We do add a couple of 'pretend' argument instances for the runtime parameters that the workflow
     * runtime will automatically add.
     *
     * We can do some optional filtering based on whether we include only outputs from prior activities
     * or we have a general filter function, typically used for filtering by type.
     *
     * @param workflow
     * @param priorTo
     * @param filter
     */
    function getExpressionParameters(workflow, priorTo, filter) {

        var argInstances = workflow.entity.expressionParameters;

        // add the workflow built-in properties to use in expressions

        var workflowType = sp.findByKey(workflow.activityTypes, 'alias', spWorkflowConfiguration.aliases.workflow);

        argInstances = argInstances.concat(_.map(workflowType && workflowType.runtimeProperties || [], function (argEntity) {
            // return argumentInstance-like object, enough to stuff using the parameter lists
            // we don't want a real entity finding its way into the entity graph of the workflow
            return {
                id: argEntity.idP,
                idP: argEntity.idP,
                nsAlias: argEntity.nsAlias,
                name: argEntity.name,
                description: argEntity.description,
                argumentInstanceActivity: workflow.entity,
                argumentInstanceArgument: argEntity
            };
        }));

        // If an activity is given then filter parameters for act outs to only those
        // that are available prior to the given activity

        if (priorTo) {
            var priorActivities = priorTo && spWorkflow.getPriorActivities(workflow, priorTo) || [];
            argInstances = _.filter(argInstances, function (e) {
                return matchingEids(e.argumentInstanceActivity, workflow.entity) ||
                    spEntity.findByEid(priorActivities, e.argumentInstanceActivity.eidP);
            });
        }

        // optional filter...

        if (filter) {
            argInstances = _.filter(argInstances, filter);
        }

        return argInstances;
    }

    /*
     * The following gets a 'view model' on the workflow parameters ... the ArgumentInstance entities.
     * This is kinda legacy.
     */
    //deprecated
    function getWorkflowExpressionParameters(workflow) {

        //don't do this... it ups the updateCount
        //spWorkflowService.activityUpdated(workflow, workflow.entity); // forces the parameters to be recalculated

        var workflowParameters = _.map(workflow.entity.getExpressionParameters(), function (argInstance) {

            var argEntity = argInstance.getArgumentInstanceArgument();
            var srcEntity = argInstance.getArgumentInstanceActivity();
            var conformsToType = argEntity.getLookup(spWorkflowConfiguration.aliases.conformsToType);

            return {
                id: argInstance.id(),
                name: argInstance.getName(),
                arg: argEntity,
                source: srcEntity,
                description: argInstance.getDescription() || 'no description',
                type: argEntity.getType().alias(),
                conformsToType: conformsToType && conformsToType.id() ||
                    argInstance.instanceConformsToType && argInstance.instanceConformsToType.id()
            };
        });

        // add the workflow built-in properties to use in expressions

        var workflowType = sp.findByKey(workflow.activityTypes, 'alias', spWorkflowConfiguration.aliases.workflow);

        _.forEach(workflowType && workflowType.getRuntimeProperties() || [], function (argEntity) {
            var parameter = {
                id: argEntity.id(),
                name: argEntity.name,
                arg: argEntity,
                source: workflow.entity,
                description: argEntity.getDescription(),
                type: argEntity.getType().alias()
            };
            if (argEntity.eid().alias() === 'core:triggeringUserAccount' || argEntity.eid().alias() === 'core:workflowOwnerAccount') {
                parameter.conformsToType = 'core:userAccount';
            }
            workflowParameters.push(parameter);
        });

        return workflowParameters;
    }

    function cleanExpressions(activity) {
        if (activity) {
            activity.expressionMap.deleteEntity(_.filter(activity.getExpressionMap(), function (e) {
                return !e.getArgumentToPopulate();
            }));
        }
    }

    function setDefaultActivityNames(workflow) {
        var containedActivities = workflow.entity.getContainedActivities();
        forEach(containedActivities, function (activity) {
            if (!activity.getName()) {
                activity.setName(spWorkflow.getUniqueName(_.map(containedActivities, 'name'), activityTypeEntity(workflow, activity).getName()));
            }
        });
    }

    function replaceInExpression(fromName, toName, expression) {

        function reEscape(s) {
            return s.replace(/([.*+?^$|(){}\[\]])/mg, "\\$1");
        }

        function replace(text, from, to) {
            return text.replace(new RegExp(reEscape(from), 'gm'), to);
        }

        // exit now if not the correct expression type
        if (expression.getType().alias() !== spWorkflowConfiguration.aliases.expression) {
            return;
        }

        var value = expression.getExpressionString();
        if (!value) {
            // i think this can happen when an activity is renamed while it still has uninitialised arguments
            console.warn('unexpected missing expressionString for ', expression.debugString);
            return;
        }

        //todo - is the logic of the replacement ok? maybe to better handle whitespace inside the delimiters
        var isTemplate = expression.getIsTemplateString();
        var newValue;

        assert(typeof isTemplate === 'boolean');

        if (isTemplate) {
            newValue = replace(value, '{{[' + fromName + ']}}', '{{[' + toName + ']}}');
        } else {
            newValue = replace(value, '[' + fromName + ']', '[' + toName + ']');
        }

        //console.log('%s expression "%s" => "%s"', newValue !== value ? 'updated' : 'no update for', value, newValue);
        expression.setExpressionString(newValue || '');
    }

    function renameExpressionParameter(workflow, argInstance, toName) {

        // capture the update details in a partial we can apply to expression collections

        var updateExpression = _.partial(replaceInExpression, argInstance.name, toName);

        // rename it

        argInstance.setName(toName);

        // rename in any expressions, in the workflow or for any activities

        forEach(workflow.entity.getExpressionMap(), updateExpression);
        forEach(workflow.entity.getContainedActivities(), function (activity) {
            forEach(activity.getExpressionMap(), updateExpression);
        });
    }

    function getArgDesc(act, actType, arg) {
        var prefix = actType && (actType.name + ' - ') || '';
        return prefix + (arg.description || 'no description');
    }

    function updateExpressionParameter(workflow, activity, parameter) {

        //console.log('updateExpressionParameter:', spEntity.toJSON(activity), spEntity.toJSON(parameter));

        var activityId = activity.idP;
        var parameterId = parameter.idP;

        var name = activityId === workflow.entity.idP ? parameter.name : activity.name + '.' + parameter.name;
        var description = getArgDesc(activity, activityTypeEntity(workflow, activity), parameter);

        var argumentInstance = _.find(workflow.entity.expressionParameters, function (e) {
            return sp.result(e, 'argumentInstanceArgument.idP') === parameterId &&
                sp.result(e, 'argumentInstanceActivity.idP') === activityId;
        });

        if (!argumentInstance) {
            argumentInstance = spEntity.fromJSON({
                typeId: spWorkflowConfiguration.aliases.argumentInstance,
                name: jsonString(name),
                description: jsonString(description),
                argumentInstanceArgument: jsonLookup(parameter),
                argumentInstanceActivity: jsonLookup(activity),
                instanceConformsToType: jsonLookup(null)
            });
            workflow.entity.expressionParameters.add(argumentInstance);

        } else {
            if (argumentInstance.name !== name) {
                renameExpressionParameter(workflow, argumentInstance, name);
            }
            argumentInstance.description = description;
        }

        return argumentInstance;
    }

    function updateExpressionParameters(workflow) {

        var existingEntities = workflow.entity.expressionParameters;
        var newParams = [];

        forEach(workflow.entity.inputArguments, function (e) {
            newParams.push(updateExpressionParameter(workflow, workflow.entity, e));
        });
        forEach(workflow.entity.variables, function (e) {
            newParams.push(updateExpressionParameter(workflow, workflow.entity, e));
        });
        forEach(workflow.entity.containedActivities, function (activity) {
            forEach(activityTypeEntity(workflow, activity).outputArguments, function (e) {
                newParams.push(updateExpressionParameter(workflow, activity, e));
            });
            forEach(activity.outputArguments, function (e) {
                newParams.push(updateExpressionParameter(workflow, activity, e));
            });
        });

        var toRemove = _.difference(existingEntities, newParams);
        if (toRemove.length) {
            workflow.entity.expressionParameters.deleteEntity(toRemove);
        }
    }

    function getActivityValidationMessages(workflow, activity) {
        return sp.result(workflow, ['activities', activity.idP, 'validationMessages']) || [];
    }

}(spWorkflow || (spWorkflow = {})));
