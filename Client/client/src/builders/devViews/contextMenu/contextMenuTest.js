// Copyright 2011-2016 Global Software Innovation Pty Ltd
(function () {
    'use strict';

    angular.module('app.contextMenuTest', ['mod.common.ui.spContextMenu'])
        .controller('contextMenuTestController', ['$scope', function ($scope) {
            $scope.placements = ['alignright', 'alignleft', 'mouse'];
            $scope.placement = $scope.placements[0];
            $scope.triggers = ['leftclick', 'rightclick'];
            $scope.trigger = $scope.triggers[0];

            $scope.disableMenuItems = false;

            $scope.menuItemClick = function (value) {
                window.alert('Menu clicked ' + value);
            };

            $scope.contextMenu = {
                menuItems: [
                    {
                        text: 'MenuItem1',
                        icon: 'assets/images/16x16/Copy.png',
                        type: 'click',
                        click: 'menuItemClick(\'MenuItem1\')'
                    },
                    {
                        text: 'MenuItem2',
                        icon: 'assets/images/16x16/Delete.png',
                        type: 'click',
                        click: 'menuItemClick(\'MenuItem2\')'
                    },                    
                    {
                        type: 'divider'
                    },
                    {
                        text: 'SubMenu',
                        submenu: [
                            {
                                text: 'SubMenuItem1',
                                icon: 'assets/images/16x16/Cut.png',
                                type: 'click',
                                click: 'menuItemClick(\'SubMenuItem1\')'
                            },
                            {
                                text: 'SubMenuItem2',
                                icon: 'assets/images/16x16/Documents.png',
                                type: 'click',
                                click: 'menuItemClick(\'SubMenuItem2\')'
                            }
                        ]
                    }
                ]
            };

            $scope.$watch('disableMenuItems', function (value) {
                $scope.contextMenu.menuItems[0].disabled = value;
                $scope.contextMenu.menuItems[1].disabled = value;
            });
        }]);
}());