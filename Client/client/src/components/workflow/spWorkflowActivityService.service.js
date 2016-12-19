// Copyright 2011-2016 Global Software Innovation Pty Ltd
/*global _, angular, console, sp, spEntity, jsonLookup, jsonString, jsonBool, jsonInt, spWorkflowConfiguration, spWorkflow */

(function () {
    'use strict';

    angular.module('mod.services.workflowService')
        .factory('spWorkflowActivityService', spWorkflowActivityService);

    /* @ngInject */
    function spWorkflowActivityService($q, spWorkflowCacheService, activityTypesRequest) {

        var aliases = spWorkflowConfiguration.aliases;

        ///////////////////////////////////////////////////////////////////////
        // The interface
        //

        var exports = {
            // called after creation and before adding into the workflow
            // to do type specific initialisation
            activityCreatedFns: {
                'core:workflowProxy': workflowProxyActivityCreated,
                'core:switchActivity': switchActivityCreated,
                'core:createActivity': createActivityCreated,
                'core:assignToVariable': assignToVariableActivityCreated,
                'core:displayFormActivity': displayFormActivityCreated,
                'core:reviewSurveyActivity':reviewSurveyActivityCreated,
                'core:startSurveyActivity': startSurveyActivityCreated,
                'core:logActivity': logActivityCreated,
                'core:promptUserActivity': promptUserActivityCreated,
                'core:notifyActivity': notifyActivityCreated
            },
            // called when activityUpdated is called by anything that has modified an activity
            // to perform type specific updates of derived data for example
            activityUpdatedFns: {
                'core:workflowProxy': workflowProxyActivityUpdated
            },
            // called after any update to the activity as part of a whole workflow process operation to
            // perform any type specific validation or building non-model derived data
            activityProcessFns: {
                'core:getResourcesActivity': getResourcesActivityUpdated,
                'core:cloneActivity': cloneActivityUpdated,
                'core:forEachResource': forEachResourceActivityUpdated,
                'core:sendEmailActivity': sendEmailActivityUpdated,
                'core:logActivity': logActivityUpdated
            }
        };

        return exports;

        ///////////////////////////////////////////////////////////////////////
        // The implementation
        //

        function assignToVariableActivityCreated(workflow, activity) {
            activity.registerLookup(aliases.targetVariable);
            return workflow;
        }

        function switchActivityCreated(workflow, activity) {
            if (!activity.exitPoints || !activity.exitPoints.length) {
                // add a default conditional exit
                activity.exitPoints.add(spEntity.fromJSON({
                    typeId: spWorkflowConfiguration.aliases.exitPoint,
                    name: 'exit',
                    description: 'condition:',
                    isDefaultExitPoint: false,
                    exitPointOrdinal: 1
                }));
            }
            return workflow;
        }

        function createActivityCreated(workflow, activity) {
            return spWorkflowCacheService.getCacheableEntity('entity:core:name', 'core:name', 'name,isOfType.{name}')
                .then(function (nameFieldEntity) {
                    var key = spWorkflow.addSetFieldArgs(workflow, activity);
                    spWorkflow.updateParameterEntityExpressionByName(workflow, activity, aliases.inputArguments, key, nameFieldEntity);
                    spWorkflow.updateParameterExpressionByName(workflow, activity, aliases.inputArguments, key + '_value', "");
                    return workflow;
                });
        }

        function displayFormActivityCreated(workflow, activity) {
            addApprovalExitPoints(workflow, activity);
            spWorkflow.updateParameterExpressionByName(workflow, activity, aliases.inputArguments, 'For Person', '[Triggering Person]');
            return workflow;
        }

        function reviewSurveyActivityCreated(workflow, activity) {
            addApprovalExitPoints(workflow, activity);
            return workflow;
        }

        function addApprovalExitPoints(workflow, activity) {
            if (!activity.exitPoints || !activity.exitPoints.length) {
                // add a default exits
                activity.exitPoints.add(spEntity.fromJSON({
                    typeId: spWorkflowConfiguration.aliases.exitPoint,
                    name: 'Approve',
                    description: '',
                    isDefaultExitPoint: true,
                    exitPointOrdinal: 1
                }));
                activity.exitPoints.add(spEntity.fromJSON({
                    typeId: spWorkflowConfiguration.aliases.exitPoint,
                    name: 'Reject',
                    description: '',
                    isDefaultExitPoint: false,
                    exitPointOrdinal: 2
                }));


            }
        }

        function startSurveyActivityCreated(workflow, activity) {
            return workflow;
        }

        // *grrr* took me quite a while to find these hooks.
        function promptUserActivityCreated(workflow, activity) {
            activity.registerRelationship(aliases.promptForArguments);
            spWorkflow.updateParameterExpressionByName(workflow, activity, aliases.inputArguments, 'For Person', '[Triggering Person]');
            return workflow;
        }

        function workflowProxyActivityCreated(workflow, activity) {
            activity.registerLookup(aliases.workflowToProxy);
            return workflow;
        }

        function cloneActivityUpdated(workflow, activity) {

            // if the newDefinitionCloneArgument is defined and is a single known entity expression then use that
            // else if the resourceToCloneArgument is defined and is a single known entity expression then use its type

            var act = spWorkflow.activityData(workflow, activity);
            var typeParameter = act.parameters['core:newDefinitionCloneArgument'];
            var resourceParameter = act.parameters['core:resourceToCloneArgument'];

            var typeEntity = spWorkflow.getAsSingleKnownEntity(workflow, typeParameter.expression) ||
                sp.result(resourceParameter, 'compileResult.entityTypeId') ||
                sp.result(spWorkflow.getAsSingleKnownEntity(workflow, resourceParameter.expression), 'isOfType.0');

            if (typeEntity) {
                spWorkflow.setArgumentInstanceConformsToType(workflow, activity, 'core:clonedResourceArgument', typeEntity);
            }
        }

        function forEachResourceActivityUpdated(workflow, activity) {

            // Set the output type based on the input

            var act = spWorkflow.activityData(workflow, activity);
            var listParameter = act.parameters['core:foreachList'];
            var entityTypeId = listParameter.compileResult.entityTypeId;

            if (entityTypeId) {
                spWorkflow.setArgumentInstanceConformsToType(workflow, activity, 'core:foreachSelectedResource', entityTypeId);
            }
        }

        function getResourcesActivityUpdated(workflow, activity) {

            // set the output type based on the input

            var act = spWorkflow.activityData(workflow, activity);
            var typeParameter = act.parameters['core:getResourcesResourceType'];
            var typeEntity = spWorkflow.getAsSingleKnownEntity(workflow, typeParameter.expression);

            if (typeEntity) {
                spWorkflow.setArgumentInstanceConformsToType(workflow, activity, 'core:getResourcesFirst', typeEntity);
                spWorkflow.setArgumentInstanceConformsToType(workflow, activity, 'core:getResourcesList', typeEntity);
            }

            // set some parameter configuration .. used in the chooser UIs

            act.parameters['core:getResourcesReport'].resourceType = 'core:report';
            act.parameters['core:getResourcesReport'].actionLabelMap = {'resourceChooser': 'Select Report'};
        }

        function sendEmailActivityUpdated(workflow, activity) {

            // set some parameter configuration .. used in the chooser UIs

            var act = spWorkflow.activityData(workflow, activity);
            var recipientsParam = act.parameters['core:sendEmailRecipientList'];

            if (_.get(recipientsParam, 'compileResult')) {

                act.parameters['core:sendEmailRecipientField'].resourceType =
                    recipientsParam.compileResult.entityTypeId ||
                    sp.result(spWorkflow.getAsSingleKnownEntity(workflow, recipientsParam.expression), 'type.idP');

                act.parameters['core:sendEmailRecipientCCField'].resourceType =
                    recipientsParam.compileResult.entityTypeId ||
                    sp.result(spWorkflow.getAsSingleKnownEntity(workflow, recipientsParam.expression), 'type.idP');

                act.parameters['core:sendEmailRecipientBCCField'].resourceType =
                    recipientsParam.compileResult.entityTypeId ||
                    sp.result(spWorkflow.getAsSingleKnownEntity(workflow, recipientsParam.expression), 'type.idP');
            }


            act.parameters['core:sendEmailFromInbox'].resourceType = 'core:inbox';
        }

        function logActivityCreated(workflow, activity) {
            activity._firstCreatedAndNeedsMessage = true;
            return workflow;
        }

        function logActivityUpdated(workflow, activity) {
            if (activity._firstCreatedAndNeedsMessage && activity.name) {
                activity._firstCreatedAndNeedsMessage = undefined;
                var message = '\'' + activity.name + '\'';
                spWorkflow.updateParameterExpressionByName(workflow, activity, aliases.inputArguments, 'Message', message);
            }
        }


        function notifyActivityCreated(workflow, activity) {
            activity.registerRelationship(aliases.nReplyMap);
        }

        /**
         * Get a promise for a Workflow that includes all Activity Type meta-data
         */
        function getWorkflowAsActivityType(id, refresh) {
            return spWorkflowCacheService.getCacheableEntity('workflow:' + id, id, activityTypesRequest, refresh);
        }

        function copyArgument(arg) {
            var copiedArg = spEntity.fromJSON({
                typeId: arg.getType(),
                name: jsonString(arg.getName()),
                description: jsonString(arg.getDescription()),
                defaultExpression: jsonString(arg.getDefaultExpression()),
                argumentIsMandatory: jsonBool(arg.getArgumentIsMandatory())
            });
            if (spWorkflow.isResourceArgument(arg)) {
                copiedArg.setLookup(spWorkflowConfiguration.aliases.conformsToType, arg.getConformsToType());
            }
            return copiedArg;
        }

        function copyExitPoint(ep) {
            var copiedEp = spEntity.fromJSON({
                typeId: ep.getType(),
                name: jsonString(ep.getName()),
                description: jsonString(ep.getDescription()),
                isDefaultExitPoint: jsonBool(ep.isDefaultExitPoint),
                exitPointOrdinal: jsonInt(ep.exitPointOrdinal)
            });
            return copiedEp;
        }

        function updateSequencesForActivityExits(workflow, activity) {

            var exits = spWorkflow.getExitPoints(workflow, activity);
            var freeExits = _.filter(exits, _.partial(spWorkflow.isFreeExit, workflow, activity));
            var fromId = activity.id();

            _(workflow.entity.transitions).concat(workflow.entity.terminations)
                .filter(function (t) {
                    // find seqs out of this activity but not on one of the new exits
                    console.log('updateSequencesForActivityExits: checking %s %o', t.debugString, spEntity.toJSON(t));
                    return fromId === t.getLookup(spWorkflowConfiguration.aliases.fromActivity).id() && !spEntity.findByEid(exits, t.getLookup(spWorkflowConfiguration.aliases.fromExitPoint).eid());
                })
                .each(function (t) {
                    // attach to a free exit or remove it
                    if (freeExits.length > 0) {
                        t.setLookup(spWorkflowConfiguration.aliases.fromExitPoint, freeExits.shift().aliasOrId());
                    } else {
                        spWorkflow.removeSequence(workflow, t);
                    }
                });

            spWorkflow.setBookmark(workflow);
            return workflow;
        }

        function workflowProxyActivityUpdated(workflow, activity, hint) {
            
            console.assert(workflow, 'missing expected argument: workflow');
            console.assert(activity, 'missing expected argument: activity');

            // Only run this update code if the update involved the workflowToProxy lookup. (#26845)
            if (hint !== 'workflowToProxy')
                return $q.when().then(function() {
                    return workflow;
                });

            var targetWorkflowProp = activity.getLookup(spWorkflowConfiguration.aliases.workflowToProxy);

            return $q.when(targetWorkflowProp ? getWorkflowAsActivityType(targetWorkflowProp.id()) : null)
                .then(function (targetWorkflow) {

                    // We are about to replace all inputs and outputs so drop any expressions targeting them
                    // - and really that means all expressions. Also remove any existing inputs and outputs.

                    activity.expressionMap.deleteEntity(_.identity);
                    activity.inputArguments.deleteEntity(_.identity);
                    activity.outputArguments.deleteEntity(_.identity);
                    activity.exitPoints.deleteEntity(_.identity);

                    if (targetWorkflow) {

                        // Add new inputs and outputs... need to be copies due to cardinality of the relationships.
                        activity.inputArguments.add(_.map(targetWorkflow.inputArguments, copyArgument));
                        activity.outputArguments.add(_.map(targetWorkflow.outputArguments, copyArgument));

                        // Copy exits
                        activity.exitPoints.add(_.map(targetWorkflow.exitPoints, copyExitPoint));
                    }

                    // Update sequences that may have been connected to old exits
                    return updateSequencesForActivityExits(workflow, activity);
                });
        }
    }

})();


