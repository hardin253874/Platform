// Copyright 2011-2016 Global Software Innovation Pty Ltd
/*global _, console, angular, spWorkflowConfiguration, spWorkflow, sp, spEntity */

(function () {
    'use strict';

    angular.module('sp.workflow.directives.parameterExpressionContextMenu', ['mod.common.ui.spContextMenu'])
        .directive('spParameterExpressionContextMenu', function ($q) {
            return {
                restrict: 'EA',
                replace: true,
                scope: true,
                template: '<span class="btn-icon" sp-context-menu="contextMenu" sp-context-menu-placement="alignleft" sp-context-menu-is-open="contextMenuIsOpen" ng-click="clickPicker()" >' +
                    '<img src="assets/images/icon_picker_open.png"/></span>',
                link: function (scope, elem, attrs) {

                    var aliasOrId = '';
                    var tools = [];
                    var toolLabels = [];

                    function refreshMenuItems() {
                        var items = [
                            {
                                id: 'typechooser',
                                text: 'Select Object',
                                type: 'click',
                                icon: 'assets/images/icon_picker_open_darken.png',
                                click: "open('typeChooser', '" + aliasOrId + "');"
                            },
                            {
                                id: 'fieldchooser',
                                text: 'Select Field',
                                type: 'click',
                                icon: 'assets/images/icon_picker_open_darken.png',
                                click: "open('fieldChooser', '" + aliasOrId + "');"
                            },
                            {
                                id: 'relchooser',
                                text: 'Select Relationship',
                                type: 'click',
                                icon: 'assets/images/icon_picker_open_darken.png',
                                click: "open('relChooser', '" + aliasOrId + "');"
                            },
                            {
                                id: 'resourcechooser',
                                text: 'Select Record',
                                type: 'click',
                                icon: 'assets/images/icon_picker_open_darken.png',
                                click: "open('resourceChooser', '" + aliasOrId + "');"
                            },
                            {
                                id: 'parameterchooser',
                                text: 'Select Parameter',
                                type: 'click',
                                icon: 'assets/images/icon_picker_open_darken.png',
                                click: "open('parameterChooser', '" + aliasOrId + "');"
                            },
                            {
                                id: 'expreditor',
                                text: 'Calculation',
                                type: 'click',
                                icon: 'assets/images/icon_picker_open_darken.png',
                                click: "open('exprEditor', '" + aliasOrId + "');"
                            },
                            {
                                id: 'reportchooser',
                                text: 'Select Report',
                                type: 'click',
                                icon: 'assets/images/icon_picker_open_darken.png',
                                click: "open('reportChooser', '" + aliasOrId + "');"
                            }
                        ];

                        // build menu items based on items order but included only if in tools spec
                        //console.log('wfcontextmenu: building menu', aliasOrId, tools, toolLabels);

                        var menuItems = _.filter(items, function (item) {
                            var index = _.indexOf(tools, item.id);
                            if (index < 0) {
                                return false;
                            }
                            item.text = index < toolLabels.length && toolLabels[index] || item.text;
                            return true;
                        });

                        scope.contextMenu = { menuItems: menuItems };
                    }

                    scope.contextMenu = { menuItems: [] };

                    scope.contextMenuIsOpen = false;

                    attrs.$observe('parameter', function (value) {
                        aliasOrId = value;
                        refreshMenuItems();
                    });

                    attrs.$observe('tools', function (value) {
                        tools = (value || '').toLowerCase().split(',').map(function (s) {
                            return s.trim();
                        });
                        refreshMenuItems();
                    });

                    attrs.$observe('toolLabels', function (value) {
                        toolLabels = (value || '').split(',').map(function (s) {
                            return s.trim();
                        });
                        refreshMenuItems();
                    });

                    scope.clickPicker = function () {
                        var menuItems = scope.contextMenu.menuItems;
                        if (menuItems.length == 1) {
                            scope.$eval(menuItems[0].click);
                        } else {
                            scope.contextMenuIsOpen = true;
                        }
                    };
                }
            };
        });

}());