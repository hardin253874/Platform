// Copyright 2011-2016 Global Software Innovation Pty Ltd
/*global _, angular, console, sp*/

(function () {
    'use strict';

    angular.module('mod.app.board')
        .config(boardViewStateConfiguration)
        .controller('rnBoardViewController', BoardViewController);

    /* @ngInject */
    function boardViewStateConfiguration($stateProvider) {

        var data = {
            showBreadcrumb: false,
            region: { 'content-header': { templateUrl: 'board/templates/rnBoardHeader.tpl.html' } }
        };

        $stateProvider.state('board', {
            url: '/{tenant}/{eid}/board?path&q',
            template: '<div class="board-view" ng-controller="rnBoardViewController as view">' +
            '   <rn-board board-id="view.boardId" analyser="view.analyserParams" icon-info="view.iconInfo"></rn-board>' +
            '</div>',
            data: data
        });
    }

    /* @ngInject */
    function BoardViewController(spState, titleService, spEntityService) {
        var vm = this;
        vm.boardId = sp.result(spState, 'navItem.id');
        vm.analyserParams = spState.params.q;
		vm.iconInfo = {};
    }

}());
