// Copyright 2011-2016 Global Software Innovation Pty Ltd
/*global angular */

var spWorkflowConfiguration = {};

(function (spWorkflowConfiguration) {
    'use strict';

    spWorkflowConfiguration.aliases = {
        // types
        resource: 'core:resource',
        wfActivity: 'core:wfActivity',
        workflow: 'core:workflow',
        exitPoint: 'core:exitPoint',
        activityType: 'core:activityType',
        transition: 'core:transition',
        termination: 'core:termination',
        expression: 'core:wfExpression',
        argumentInstance: 'core:wfArgumentInstance',

        argumentType: 'core:argumentType',
        stringArgument: 'core:stringArgument',
        resourceArgument: 'core:resourceArgument',
        resourceListArgument: 'core:resourceListArgument',
        integerArgument: 'core:integerArgument',
        boolArgument: 'core:boolArgument',
        objectArgument: 'core:objectArgument',

        gatewayType: 'core:decisionActivity',
        switchActivity: 'core:switchActivity',
        workflowProxy: 'core:workflowProxy',
        approvalActivity: 'core:approvalActivity',
        assignToVariable: 'core:assignToVariable',
        swimlane: 'core:swimlane',
        approvalEnum: 'core:approvalEnum',
        replyMapEntry: 'replyMapEntry',

        //fields
        name: 'core:name',
        description: 'core:description',
        designerData: 'core:designerData',
        expressionString: 'core:expressionString',
        defaultExpression: 'core:defaultExpression',
        isTemplateString: 'core:isTemplateString',
        isDefaultExitPoint: 'isDefaultExitPoint',
        exitPointOrdinal: 'core:exitPointOrdinal',
        enumOrder: 'core:enumOrder',
        stringArgumentValue: 'core:stringArgumentValue',
        resourceArgumentValue: 'core:resourceArgumentValue',

        // relationships
        isOfType: 'core:isOfType',
        instancesOfType: 'core:instancesOfType',
        swimlanes: 'core:swimlanes',
        firstActivity: 'core:firstActivity',
        containedActivities: 'core:containedActivities',
        transitions: 'core:transitions',
        terminations: 'core:terminations',
        argumentToPopulate: 'core:argumentToPopulate',
        expressionKnownEntities: 'core:wfExpressionKnownEntities',
        expressionMap: 'core:expressionMap',
        expressionParameters: 'core:expressionParameters',
        argumentInstanceActivity: 'core:argumentInstanceActivity',
        argumentInstanceArgument: 'core:argumentInstanceArgument',
        instanceConformsToType: 'core:instanceConformsToType',
        fromActivity: 'core:fromActivity',
        fromExitPoint: 'core:fromExitPoint',
        toActivity: 'core:toActivity',
        workflowExitPoint: 'core:workflowExitPoint',
        exitPoints: 'core:exitPoints',
        inputArguments: 'core:inputArguments',
        outputArguments: 'core:outputArguments',
        runtimeProperties: 'core:runtimeProperties',
        variables: 'core:variables',
        conformsToType: 'core:conformsToType',
        inputArgumentForAction: 'core:inputArgumentForAction',
        inputArgumentForRelatedResource: 'core:inputArgumentForRelatedResource',
        nReplyMap: 'core:nReplyMap',

        workflowToProxy: 'core:workflowToProxy',
        targetVariable: 'core:targetVariable',
        promptForArguments: 'core:promptForArguments',

        timeoutExitPoint: 'core:timeoutExitPoint',
        approvedApprovalEnum: 'core:approvedApprovalEnum'
    };

    angular.module('mod.services.workflowConfiguration', [])
        .value('workflowAliases', spWorkflowConfiguration.aliases)
        .value('workflowEntityRequest', ' ' +
            'alias, isOfType.alias, name, description,inSolution.name, designerData, workflowRunAsOwner, securityOwner.name, ' +
            'swimlanes.{alias, isOfType.alias, name, description, designerData}, ' +
            'containedActivities.{ ' +
            '  alias, isOfType.alias, name, description, designerData, ' +
            '  {inputArguments, outputArguments}.{alias, isOfType.{alias,name}, name, description, argumentIsMandatory, defaultExpression, conformsToType.{alias,isOfType.alias, name}}, ' +
            '  exitPoints.{alias, isOfType.alias, name, description, isDefaultExitPoint, exitPointOrdinal, exitPointActionSummary}, ' +
            '  expressionMap.wfExpressionKnownEntities.{name, referencedEntity.{name, alias, isOfType.alias}}, ' +
            '  expressionMap.{alias,isOfType.alias,expressionString,isTemplateString, argumentToPopulate.{alias,isOfType.alias,name}} ' +
            '  {{activityTypeRequest}} ' +
            '}, ' +
            'firstActivity.{name, alias, isOfType.{alias,name}}, ' +
            '{transitions, terminations}.{alias, isOfType.alias, ' +
            '  {fromActivity, fromExitPoint, toActivity, workflowExitPoint}.{name, alias, isOfType.alias} ' +
            '}, ' +
            '{inputArguments, outputArguments, variables}.{alias, isOfType.{alias,name}, name, description, argumentIsMandatory, defaultExpression, conformsToType.{alias, name, isOfType.alias, inherits*.{id, alias}}}, ' +
            'expressionMap.{alias, isOfType.alias, expressionString, isTemplateString, argumentToPopulate.{alias, isOfType.alias, name, description}}, ' +
            'expressionMap.wfExpressionKnownEntities.{name, referencedEntity.{name, alias, isOfType.alias}}, ' +
            'expressionParameters.{alias,isOfType.alias,name,description, {argumentInstanceActivity,argumentInstanceArgument,instanceConformsToType}.{alias, isOfType.alias,name,description}}, ' +
            'exitPoints.{alias, isOfType.alias, name, description, isDefaultExitPoint, exitPointOrdinal},' +
            'inputArgumentForAction.{alias, isOfType.alias}, ' +
            'inputArgumentForRelatedResource.{alias, isOfType.alias}, ' +
            'workflowHasErrors,' +
            'wfNewerVersion.id, workflowVersion, canModify, canDelete'

        )
        .value('activityTypesRequest',
            'alias, isOfType.alias, name, description, activityTypeOrder,' +
                '  {inputArguments, outputArguments, runtimeProperties}.{alias, isOfType.{alias,name}, name, description, argumentIsMandatory, defaultExpression, conformsToType.{alias, isOfType.alias,name}}, ' +
                '  {fields, relationships}.{alias, isOfType.alias, name, description}, ' +
                '  wfActivityCategory.{alias, isOfType.alias, name, description, enumOrder}, ' +
                '  exitPoints.{alias, isOfType.alias, name, description, isDefaultExitPoint, exitPointOrdinal} '
        )
        .value('activityTypeConfig', {
            'default': {
                request: '', // activity type specific fields and relationships to include in the entity info service query
                imagePath: 'assets/images/activities/',
                elementTemplate: 'activityTemplate'

                // the following may be overridden, but default to the activity type alias
                //image: 'workflow.svg',            // looks in imagePath
                //menuImage: 'workflow.svg',        // looks in imagePath + 'menu/'
            },
            'core:assignToVariable': {
                request: 'targetVariable.{id,alias, isOfType.alias}'
            },
            'core:promptUserActivity': {
                request: 'promptForArguments.{activityPromptOrdinal, activityPromptArgumentPickerReport.{id, name}, activityPromptArgument.{id, name, isOfType.{alias}, conformsToType.{id, defaultPickerReport.id, inherits*.{id, alias}}}}'
            },
            'core:workflowProxy': {
                request: 'workflowToProxy.{name, id,alias, isOfType.alias}'
            },
            'core:notifyActivity': {
                request: 'nReplyMap.{name, rmeOrder, rmeWorkflow.name}'
            }
        })
        .value('elementTemplates', {
            startTemplate: [
                {
                    metaProperty: 'bounding',
                    type: 'circle',
                    cx: 0,
                    cy: 0,
                    r: 16,
                    stroke: 'black',
                    fill: '#5cbc5e',
                    'stroke-opacity': 0
                },
                {
                    type: 'path',
                    path: 'M -4 -6 L 6 0 -4 6 z',
                    stroke: 'gray',
                    'stroke-width': 0,
                    fill: 'white'
                },
                {
                    type: 'text',
                    x: 0,
                    y: -35,
                    text: "start",
                    align: "start",
                    'font-family': "Segoe UI, My Segoe UI, -apple-system",
                    'font-size': "11px",
                    'font-weight': "bold",
                    fill: '#333333'
                }
            ],
            endTemplate: [
                {
                    metaProperty: 'bounding',
                    type: 'circle',
                    cx: 0,
                    cy: 0,
                    r: 16,
                    stroke: 'black',
                    fill: '#ff4848',
                    'stroke-opacity': 0
                },
                {
                    type: 'path',
                    path: 'M -4 -4 L 4 -4 4 4 -4 4 z',
                    stroke: 'gray',
                    'stroke-width': 0,
                    fill: 'white'
                },
                {
                    type: 'text',
                    x: 0,
                    y: -35,
                    text: "{{label}}",
                    align: "start",
                    'font-family': "Segoe UI, My Segoe UI, -apple-system",
                    'font-size': "11px",
                    'font-weight': "bold",
                    fill: '#333333'
                }
            ],
            textTemplate: [
                {
                    type: 'rect',
                    x: 0,
                    y: 0,
                    width: 100,
                    height: 10,
                    stroke: '#fff',
                    fill: '#fff'
                },
                {
                    type: 'text',
                    x: 0,
                    y: 10,
                    text: "{{label}}",
                    align: "start",
                    'font-family': "Segoe UI, My Segoe UI, -apple-system",
                    'font-size': "16pt",
                    'font-weight': "normal",
                    fill: 'black'
                }
            ],
            activityTemplate: [
                {
                    metaProperty: 'bounding',
                    type: 'image',
                    x: -30,
                    y: -25,
                    width: 60,
                    height: 50,
                    src: '{{imagesrc}}'
                },
                {
                    type: 'text',
                    x: 0,
                    y: -42,
                    text: "{{label}}",
                    align: "middle",
                    'font-family': "Segoe UI, My Segoe UI, -apple-system",
                    'font-size': "12px",
                    'font-weight': "bold",
                    fill: '#333333'
                }
            ],
            swimlaneTemplate: [
                {
                    metaProperty: 'bounding',
                    type: 'rect',
                    x: 25,
                    y: 0,
                    width: 3000,
                    height: 150,
                    stroke: 'black',
                    fill: '#ededed',
                    'stroke-dasharray': '',
                    'fill-opacity': 0.7,
                    'stroke-opacity': 0.2
                },
                {
                    type: 'rect',
                    x: 0,
                    y: 0,
                    width: 25,
                    height: 150,
                    'stroke-width': 0,
                    fill: '#A5A5A5'
                },
                {
                    type: 'text',
                    x: 12,
                    y: 75,
                    text: "{{label}}",
                    'font-family': "Segoe UI, My Segoe UI, -apple-system",
                    'font-size': "13px",
                    'font-weight': "normal",
                    fill: 'white',
                    transform: 't-3,0r-90'
                },
                {
                    metaProperty: 'selectable',
                    type: 'rect',
                    x: 0,
                    y: 0,
                    width: 25,
                    height: 150,
                    'stroke-width': 0,
                    fill: '#fff',
                    'fill-opacity': 0
                }
            ],

            backgroundTemplate: [
                {
                    metaProperty: 'bounding',
                    type: 'rect',
                    x: 0,
                    y: 0,
                    width: 3000,
                    height: 2000,
                    stroke: 'black',
                    fill: 'none',
                    'stroke-dasharray': '',
                    'fill-opacity': 0.7,
                    'stroke-opacity': 0.2
                }
            ],

            infoTemplate: [
                { type: 'image', x: -10, y: -10, width: 20, height: 20, src: '{{basePath}}info.svg' }
            ],
            plusTemplate: [
                { type: 'image', x: -10, y: -10, width: 20, height: 20, src: '{{basePath}}plus.svg' }
            ],
            deleteTemplate: [
                { type: 'image', x: -10, y: -10, width: 20, height: 20, src: '{{basePath}}delete.svg' }
            ]
        });
}(spWorkflowConfiguration));