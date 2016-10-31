// Copyright 2011-2016 Global Software Innovation Pty Ltd
/*global _, angular, console, spEntity */

/// <reference path="../lib/angular/angular.js" />

(function () {
    'use strict';

    angular.module('app.workflowDiagram', ['ui.router', 'titleService'])
        .config(function ($stateProvider) {
            $stateProvider.state('workflowDiagram', {
                url: '/{tenant}/{eid}/workflowDiagram?path',
                templateUrl: 'devViews/workflowDiagram/workflowDiagram.tpl.html',
                controller: 'WorkflowDiagramTestController'
            });

            // register a nav item for this page (most fields are defaulted, but you can provide your own viewType, iconUrl, href and order)
            window.testNavItems = window.testNavItems || {};
            window.testNavItems.workflowDiagram = { name: 'workflow Diagram Test Page' };
        })
        .controller('WorkflowDiagramTestController', function ($scope) {

            function handler() {
                console.log.apply(console, ['event %o'].concat(arguments));
            }

            $scope.diagram = {
                elements: [],
                connections: [],
                tools: [],
                elementClicked: _.partial(handler, 'clicked'),
                elementMoved: _.partial(handler, 'moved'),
                toolClicked: _.partial(handler, 'tool clicked'),
                portClicked: _.partial(handler, 'port clicked'),
                connectionChanged: _.partial(handler, 'conn changed'),
                newConnection: _.partial(handler, 'new conn'),
                clearSelection: _.partial(handler, 'clear selected'),
                addToSelection: _.partial(handler, 'selected')
            };

            $scope.diagram.elements.push({
                type: 'event',
                id: 1000,
                name: 'start',
                template: 'startTemplate',
                x: 50,
                y: 50,
                movable: true,
                inPorts: [],
                outPorts: [
                    { id: 0, name: '' }
                ],
                tools: []
            });
            $scope.diagram.elements.push({
                type: 'event',
                id: 9000,
                name: 'end',
                template: 'endTemplate',
                x: 500,
                y: 50,
                movable: true,
                outPorts: [],
                tools: []
            });

            _.each(_.range(20), function (n) {
                $scope.diagram.elements.push({
                    type: 'activity',
                    id: 5000 + n,
                    name: 'Activity ' + (n + 1),
                    help: '',
                    outPorts: [
                        { id: 10000 + n, name: '', ordinal: 0 }
                    ],
                    template: 'activityTemplate',
                    imageSrc: 'assets/images/activities/cloneActivity.svg',
                    x: 50 + Math.floor(n / 4) * 75,
                    y: 150 + (n % 4) * 75,
                    movable: true,
                    tools: []
                });
                if (n > 0) {
                    $scope.diagram.connections.push({
                        type: 'trans',
                        id: 100000 + n,
                        from: 5000 + n - 1,
                        to: 5000 + n
                    });
                }
            });


            $scope.diagram.connections.push({
                type: 'first',
                id: 1100,
                from: 1000,
                to: 5000
            });
            $scope.diagram.connections.push({
                type: 'term',
                id: 1101,
                from: 5019,
                to: 9000
            });

        });
}());
