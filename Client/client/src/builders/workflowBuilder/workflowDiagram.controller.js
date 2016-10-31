// Copyright 2011-2016 Global Software Innovation Pty Ltd
/*global _, console, angular, sp, spWorkflowConfiguration, spWorkflow, spEntity */

(function () {
    'use strict';

    /**
     * This child controller (of the workflow controller) watches the open workflow entity model
     * and maintains a diagram data model suitable for the diagram view logic
     * (handled by the {@link module:diagramWidget})
     *
     * @namespace diagramController
     */
    angular.module('sp.workflow.builder')
        .controller('workflowDiagramController', function ($scope, spState, $q, elementTemplates, activityTypeConfig, spWorkflowService) {

            // convenience aliases
            var findByEid = spEntity.findByEid;
            var aliases = spWorkflowConfiguration.aliases;
            var ws = spWorkflowService;

            function clearDiagram() {
                $scope.diagram = {
                    elements: [],
                    connections: [],
                    tools: [],
                    elementClicked: elementClicked,
                    elementMoved: elementMoved,
                    toolClicked: toolClicked,
                    portClicked: portClicked,
                    connectionChanged: connectionChanged,
                    newConnection: newConnection,
                    clearSelection: clearSelection,
                    addToSelection: addToSelection,
                };

            }

            function getSwimlaneElements(workflow) {
                var elements = [],
                    element;

                var swimlanes = workflow.entity.getSwimlanes();

                if (swimlanes.length === 0) {

                    element = {
                        type: 'swimlane',
                        id: 0,
                        name: '',
                        template: 'backgroundTemplate',
                        x: 0,
                        y: 0,
                        grid: { dx: 20, dy: 20 },
                        inPorts: [],
                        outPorts: []
                    };
                    elements.push(element);

                } else {

                    swimlanes.forEach(function (swimlane, i) {
                        element = {
                            type: 'swimlane',
                            id: swimlane.id(),
                            name: swimlane.getName(),
                            template: 'swimlaneTemplate',
                            x: 0,
                            y: i * 160,
                            grid: { dx: 20, dy: 20 },
                            inPorts: [],
                            outPorts: []
                        };
                        _.extend(element, _.pick(spWorkflow.getExtendedProperties(swimlane), 'x', 'y'));
                        elements.push(element);
                    });
                }

                return elements;
            }

            function getEventElements(workflow) {

                // A Start event and End events - note the id we use for each...

                var elements = [],
                    element;

                element = {
                    type: 'event',
                    id: workflow.entity.idP,
                    name: 'start',
                    template: 'startTemplate',
                    x: 170,
                    y: 66,
                    movable: true,
                    inPorts: [], // means none
                    outPorts: [
                        { id: 'core:defaultExitPoint', name: '', isStartPort: true }
                    ],
                    tools: []
                };
                _.extend(element, _.pick(sp.result(spWorkflow.getExtendedProperties(workflow.entity), 'startEvent'), 'x', 'y'));
                elements.push(element);

                workflow.entity.exitPoints.forEach(function (exitPoint, i) {
                    element = {
                        type: 'event',
                        id: exitPoint.nsAliasOrId,
                        name: (!exitPoint.name || exitPoint.name === 'Default Exit Point') ? 'end' : exitPoint.name,
                        template: 'endTemplate',
                        movable: true,
                        outPorts: [] // means none
                    };
                    _.extend(element, _.pick(sp.result(spWorkflow.getExtendedProperties(workflow.entity), 'endEvents.' + element.id), 'x', 'y'));
                    elements.push(element);
                });

                return elements;
            }

            function getActivityElement(workflow, activity) {

                var config = ws.getActivityTypeConfig(activity.type.nsAlias);
                var exitPoints = spWorkflow.getExitPoints(workflow, activity);
                var errors = spWorkflow.getActivityValidationMessages(workflow, activity);

                var element = {
                    type: 'activity',
                    id: activity.idP,
                    name: activity.name,// + (errors.length ? ' (errors)' : ''),
                    help: activity.type.nsAlias, //todo - change to get the activity type name, not its alias....
                    outPorts: exitPoints.map(function (e) {
                        var name = e.name !== 'Default Exit Point' ? e.name : '';
// we were thinking of putting the condition expression in the exit label, but no longer doing it...
//                            var label = e.description && e.description.indexOf('condition:') === 0 &&
//                                name + ' (' + e.description.substring(10) + ')';
                        var label = name;

                        return { id: e.nsAliasOrId, name: name, ordinal: e.exitPointOrdinal || 0, label: label };
                    }),
                    template: config.elementTemplate,
                    imageSrc: config.imagePath + config.image,
                    movable: true,
                    tools: [],
                    errors: errors
                };
                _.extend(element, _.pick(spWorkflow.getExtendedProperties(activity), 'x', 'y'));

                return element;
            }

            function getActivityElements(workflow) {

                // The activities and gateways

                return _.map(workflow.entity.containedActivities, function (activity) {
                    return getActivityElement(workflow, activity);
                });
            }

            function getSequenceConnections(workflow) {

                // The sequences

                var connections = [], connection;

                var firstActivity = workflow.entity.getFirstActivity();
                if (firstActivity) {
                    // we use the firstActivity relationship id for the first sequence id,
                    // as there is only one per workflow
                    connection = {
                        type: 'first',
                        id: aliases.firstActivity,
                        from: workflow.entity.id(),
                        to: firstActivity.id()
                    };
                    connections.push(connection);
                }

                workflow.entity.getTransitions().forEach(function (t) {

                    var fromActivity = t.getFromActivity();
                    var fromExitPoint = t.getFromExitPoint();
                    var toActivity = t.getToActivity();

                    if (fromActivity && fromExitPoint && toActivity) {
                        connection = {
                            type: 'trans',
                            id: t.id(),
                            from: fromActivity.id(),
                            fromPort: fromExitPoint.aliasOrId(),
                            to: toActivity.id()
                        };
                        connections.push(connection);
                    } else {
                        console.error('transition %d missing expected data (fromAct, fromExit, to)', t.id(), fromActivity, fromExitPoint, toActivity);
                    }
                });

                workflow.entity.getTerminations().forEach(function (t) {
                    var fromActivity = t.getFromActivity();
                    var fromExitPoint = t.getFromExitPoint();
                    var toExitPoint = t.getWorkflowExitPoint();

                    if (fromActivity && fromExitPoint && toExitPoint) {
                        connection = {
                            type: 'term',
                            id: t.id(),
                            from: fromActivity.id(),
                            fromPort: fromExitPoint.aliasOrId(),
                            to: toExitPoint.aliasOrId()
                        };
                        connections.push(connection);
                    } else {
                        console.error('termination %d missing expected data (fromAct, fromExit, to)', t.id(), fromActivity, fromExitPoint, toExitPoint);
                    }
                });

                return connections;
            }

            function sortElements(elements, connections, startEventId) {

                // walk the connections building a list of element ids

                var elementIds;

                // swimlanes first

                elementIds = _.map(_.filter(elements, { type: 'swimlane' }), 'id');

                // then follow the connections from the start event

                (function walkConnections(fromId, cons, elIds) {
                    if (!cons || !cons.length) {
                        return;
                    }
                    var consOut = _.filter(cons, { from: fromId });
                    _.each(consOut, function (c) {
                        elIds.push(c.from);
                        if (c.type !== 'term') {
                            elIds.push(c.to);
                        }
                        walkConnections(c.to, _.without(cons, c), elIds);
                    });
                })(startEventId, _.cloneDeep(connections), elementIds);

                // rebuild the elements list based on the "sorted" elementIds

                var sortedElements = [];

                _.each(elementIds, function (id, i) {
                    var el = sp.findByKey(elements, 'id', id);
                    if (el) {
                        sortedElements.push(el);
                    } else {
                        console.error('warning - connection with unknown end point', id);
                    }
                });

                // return the sorted ones followed by any left over

                return _.union(sortedElements, elements);
            }

            function layoutDiagramElements() {

                // This is very rudimentary.... need to employ some kind of collision detection or spatial awareness..
                // The concept is to build a grid with each cell able to hold a single activity. User positioned elements
                // may appear in the same cell, but any we auto layout will not.

                //todo - don't crash if we go over the end of the layout grid....!


                //console.groupCollapsed('layout diagram');

                function createArray(length) {
                    var arr = new Array(length || 0),
                        i = length,
                        args;

                    if (arguments.length > 1) {
                        args = Array.prototype.slice.call(arguments, 1);
                        while ((i -= 1) > 0) {
                            arr[length - 1 - i] = createArray.apply(null, args);
                        }
                    }

                    return arr;
                }

                var workflow = $scope.workflow;
                var elements = $scope.diagram.elements;
                var connections = $scope.diagram.connections;

                var grid = createArray(100, 100),
                    x0 = 66, y0 = 22,
                    dx = 110, dy = 75,
                    cell, row, col, free;


                function getCellRowCol(x, y) {
                    return {
                        row: Math.max(0, Math.floor((y - y0) / dy)),
                        col: Math.max(0, Math.floor((x - x0) / dx))
                    };
                }

                function hasDefinedPosition(el) {
                    return !_.isUndefined(el.x) && !_.isUndefined(el.y);
                }

                // First pass - mark all cells as occupied for any activities or events with known positions.

                elements.forEach(function (el, i) {
                    if (el.type === 'activity' || el.type === 'event') {
                        if (hasDefinedPosition(el)) {
                            cell = getCellRowCol(el.x, el.y);
                            if (cell.row >= grid.length || cell.col >= grid[0].length) {
                                console.log('ooops, outside our layout grid', cell.row, cell.col, el.x, el.y, grid.length, grid[0].length);
                            } else {
                                //console.log('already positioned "%s" at %s %s (%s, %s)', el.name, cell.row, cell.col, el.x, el.y);
                                grid[cell.row][cell.col] = el.id; // could be overwriting another, that's ok
                            }
                        }
                    }
                });

                // Second pass is to flow any activities that do not have a saved position.
                // If no position then find previous activity, find its cell, and then find the next free cell and mark that as occupied.

                //console.log('debug: positioning elements', _.cloneDeep(elements));

                elements.forEach(function (el, i) {
                    // bail out if not an activity or event with missing location info
                    if (!(el.type === 'activity' || el.type === 'event')) {
                        return;
                    }
                    if (hasDefinedPosition(el)) {
                        if (el.x < 0)
                            el.x = 40;

                        if (el.y < 0)
                            el.y = 50;

                        return;
                    }

                    // by default start looking for a free cell from the start
                    var prevX = x0 + dx / 2, prevY = y0 + dy / 2, exitIndex = 0;
                    // if there is an earlier activity then find its position and work from there
                    var conTo = sp.findByKey(connections, 'to', el.id);
                    if (conTo) {
                        var prevEl = sp.findByKey(elements, 'id', conTo.from);
                        if (prevEl) {
                            prevX = prevEl.x;
                            prevY = prevEl.y;
                            if (prevEl.outPorts && conTo.fromPort) {
                                exitIndex = prevEl.outPorts.map(function (p) {
                                    return p.id;
                                }).indexOf(conTo.fromPort);
                                if (exitIndex < 0) {
                                    exitIndex = 0;
                                }
                            }
                            if (_.isUndefined(prevX) || _.isUndefined(prevY)) {
                                console.warn('unexpected issue with workflow diagram layout, prev el %o for %o is not positioned', prevEl, el);
                                prevX = prevY = 100;
                            }
                        }
                    }
                    // "events" start a little further away than activities
                    if (el.type === 'event' && prevX < (x0 + 4 * dx)) {
                        prevX = x0 + 4 * dx;
                    }
                    //todo - this 'safety' stuff is lame...
                    var safety = 20;
                    // next position to try is a cell's width to the right and either same level or lower depending on exit point
                    //todo - the logic around the exit points is a little out of date... not up with that we can now have many exit points
                    prevY += dy * exitIndex;
                    
                    do {
                        prevX += dx;
                        el.x = prevX;
                        el.y = prevY;
                        // what cell is it?
                        cell = getCellRowCol(el.x, el.y);
                        row = cell.row;
                        col = cell.col;
                        // is it free?
                        free = grid[row] && !grid[row][col];    // guard so we don't drop off the bottom.
                        //console.log('%s: %s %s for %s = %s', safety, row, col, el.name, free ? 'free' : 'occupied', exitIndex);
                    } while (!free && --safety >= 0);

                    // occupy it
                    if (grid[row])
                        grid[row][col] = el.id;

                    //console.log('newly positioned "%s" at %d %d', el.name, row, col, el.x, el.y);

                    //put the following in if we wish to 'lock in' the position of an element. At the moment choosing not to.
                    //NOTE - this commented out code will need to be updated if reintroduced.. to use spWorkflow.mergeExtendedProperties
                    //if (el.type === 'activity') {
                    //    var entity = spWorkflow.findWorkflowComponentEntity(workflow, el.id);
                    //    if (entity) {
                    //        entity.extendedProperties.x = el.x;
                    //        entity.extendedProperties.y = el.y;
                    //    }
                    //}
                });

                // third pass ... shift the default "end" event to the next free cell to the right

                //elements.forEach(function(el, i) {

                //    if (!(el.type === 'event' && el.id !== workflow.id())) return;
                //    if (_.isUndefined(el.x) || _.isUndefined(el.y)) return;

                //    console.log('checking event "%s" %s at %d %d', el.name, el.id, el.x, el.y);
                //    while (true) {
                //        var cell = getCellRowCol(el.x, el.y),
                //            elInCell = grid[cell.row][cell.col];

                //        console.log('checking event "%s" at %d %d -> %o', el.name, el.x, el.y, elInCell);

                //        if (!elInCell || elInCell === el.id) {
                //            break;
                //        }
                //        console.log('shifting event "%s" by %d at %d %d', el.name, dx, el.x, el.y);
                //        el.x += dx;
                //    }
                //});

                //console.groupEnd();
            }

            // adjust the size of the diagram so that it fits all of the elements.
            function resizeDiagram() {
                var minWidth = 1920, minHeight = 1200,  // if this is too small then blank spaces appear around the grid.
                    activitySize = 80;

                var maxWidth = activitySize + _.maxBy($scope.diagram.elements, 'x').x;
                $scope.diagramWidth = maxWidth;

                if ($scope.diagramWidth < minWidth)
                    $scope.diagramWidth = minWidth;

                var maxHeight = activitySize + _.maxBy($scope.diagram.elements, 'y').y;
                $scope.diagramHeight = maxHeight;

                if ($scope.diagramHeight < minHeight)
                    $scope.diagramHeight = minHeight;
            }

            function updateDiagramModel() {

                clearDiagram();

                if (!$scope.diagramWidth) {
                    $scope.diagramWidth = 1000;
                    $scope.diagramHeight = 1000;
                }

                var workflow = $scope.workflow;
                if (!workflow || !workflow.entity) {
                    return;
                }

                console.time('updateDiagramModel');

                // TODO - update the existing model rather than rebuild every time....
                // or maybe not.... should profile

                $scope.diagram.elements = $scope.diagram.elements.concat(
                    getSwimlaneElements(workflow),
                    getEventElements(workflow),
                    getActivityElements(workflow));

                $scope.diagram.connections = $scope.diagram.connections.concat(
                    getSequenceConnections(workflow));

                _.each($scope.diagram.elements, function (e) {
                    e.selected = _.includes($scope.selectedItems, e.id);
                });

                _.each($scope.diagram.connections, function (c) {
                    c.selected = _.includes($scope.selectedItems, c.id);
                });

                // Now arrange somewhat

                $scope.diagram.elements = sortElements($scope.diagram.elements, $scope.diagram.connections, workflow.entity.id());

                // Set default layout for elements that don't have a saved position

                layoutDiagramElements();

                resizeDiagram();

                // add a function on the model that prepares an element that is being dragged

                $scope.diagram.getElementForType = function (typeId) {

                    //var activityType = spEntity.findByEid(workflow.activityTypes, typeId);
                    var activity = spEntity.fromJSON({
                        typeId: typeId,
                        name: jsonString(''),
                        description: jsonString(''),
                        designerData: jsonString(''),
                        exitPoints: [],
                        inputArguments: [],
                        outputArguments: [],
                        expressionMap: []
                    });
                    return activity && getActivityElement(workflow, activity);
                };

                console.timeEnd('updateDiagramModel');
            }

            function elementClicked(id, cmd, x, y) {
//                console.log('element clicked', id, cmd, x, y);
                //$scope.setSelectedEntity(id);
            }

            function elementMoved(id, x, y) {
//                console.log('element moved', id, x, y);

                var workflow = $scope.workflow;

                // Save the element position in the extended properties on the corresponding entity.

                if (id === workflow.entity.id()) {
                    spWorkflow.mergeExtendedProperties(workflow.entity, { startEvent: { x: x, y: y } });
                    ws.activityUpdated(workflow, workflow.entity);

                } else if (id === aliases.defaultExitPoint || sp.findByKey(workflow.entity.getExitPoints(), 'id', id)) {
                    var extProps = { endEvents: {} };
                    extProps.endEvents[id] = { x: x, y: y };
                    spWorkflow.mergeExtendedProperties(workflow.entity, extProps);
                    ws.activityUpdated(workflow, workflow.entity);

                } else {
                    var entity = sp.findByKey(workflow.entity.getContainedActivities(), 'id', id);
                    if (entity) {
                        spWorkflow.mergeExtendedProperties(entity, { x: x, y: y });
                        ws.activityUpdated(workflow, entity);
                    }
                }
            }

            function toolClicked(toolId, eleId, x, y) {
//                console.log('tool clicked', toolId, eleId, x, y, $scope.$$phase);
//                if (toolId === 'delete') {
//                    $scope.removeActivity(eleId);
//                    $scope.removeSequence(eleId);
//                } else if (toolId === 'add') {
//                    $scope.showPortMenu(eleId, 0, x, y);
//                }
            }

            function portClicked(portId, eleId, x, y) {
//                console.log('port clicked', portId, eleId, x, y, $scope.$$phase);
//                if ($scope.showPortMenu) {
//                    $scope.showPortMenu(eleId, portId, x, y);
//                }
            }

            function connectionChanged(id, fromId, fromExitId, toId) {
//                console.log('connection updated', id, fromId, fromExitId, toId);

                var workflow = $scope.workflow;
                if (workflow) {
                    var seq = spWorkflow.findWorkflowComponentEntity(workflow, id);
                    var from = spWorkflow.findWorkflowComponentEntity(workflow, fromId);
                    var fromExit = from && findByEid(spWorkflow.getExitPoints(workflow, from), fromExitId);
                    var to = spWorkflow.findWorkflowComponentEntity(workflow, toId);

                    ws.updateSequence(workflow, seq, from, fromExit, to);
                }
            }

            function newConnection(fromId, fromExitId, toId) {
//                console.log('new connection', fromId, fromExitId, toId);

                var workflow = $scope.workflow;
                if (workflow) {
                    var from = spWorkflow.findWorkflowComponentEntity(workflow, fromId);
                    var fromExit = from && findByEid(spWorkflow.getExitPoints(workflow, from), fromExitId);
                    var to = spWorkflow.findWorkflowComponentEntity(workflow, toId);

                    ws.addSequence(workflow, from, fromExit, to);
                }
            }

            function clearSelection() {
                //console.log('workflowDiagramController: clearSelection');
                var workflow = $scope.workflow;
                if (workflow) {
                    workflow.selectedEntity = null;
                }
            }

            function addToSelection(ids) {
//                console.log('workflowDiagramController: addToSelection', ids);
                // only setting the selected entity to the first id
                var workflow = $scope.workflow;
                if (workflow) {
                    var id = _.isArray(ids) ? _.first(ids) : ids;
                    workflow.selectedEntity = spWorkflow.findWorkflowComponentEntity(workflow, id);
                }
            }

            function updateSelectedEntity(entity, previous) {
                //console.log('workflowDiagramController: updateSelectedEntity %o => %o', previous, entity);
                $scope.selectedItems = _.compact([sp.result($scope.workflow, 'selectedEntity.nsAliasOrId')]);
                updateDiagramModel();
            }

            $scope.$watch('workflow.processState.count', updateDiagramModel);
            $scope.$watch('workflow.selectedEntity', updateSelectedEntity);

            spState.registerAction('addSequence', function (opts) {
                // we tend to work with entity names rather than ids or aliases as it is
                // only the names that the user sees in the UI.

                console.log('diagram.addSequence: ' + JSON.stringify(opts));

                var workflow = $scope.workflow;
                if (workflow) {
                    var from = _.find(workflow.entity.containedActivities, {name: opts.fromName});
                    var fromExit = from && _.find(spWorkflow.getExitPoints(workflow, from), {name: opts.exitName});
                    var to = _.find(workflow.entity.containedActivities, {name: opts.toName});
                    if (!from || !to) console.log('addSequence FAILED to find from or to activities');
                    console.log(['addSequence', opts.fromName, sp.result(from, 'id'), opts.exitName, opts.toName, sp.result(to, 'id')].join());
                    ws.addSequence(workflow, from, fromExit, to);
                }
                return workflow;
            });
        });
}());
