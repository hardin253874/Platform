// Copyright 2011-2016 Global Software Innovation Pty Ltd
(function () {
    'use strict';

    angular.module('mod.common.ui.spEditSaveCancel', ['mod.common.spUserTask', 'mod.common.spMobile'])
    
  
    .directive('spEditSaveCancel', function ($rootScope, spUserTask, spMobileContext, spNavService) {

        var isMobile = spMobileContext.isMobile;
        var isTablet = spMobileContext.isTablet;

        var templateUrl;

        if (isMobile) {
            templateUrl = 'editSaveCancel/spEditSaveCancelMobile.tpl.html'; 
        } else if (isTablet) {
            templateUrl = 'editSaveCancel/spEditSaveCancelTablet.tpl.html';
        } else {
            templateUrl = 'editSaveCancel/spEditSaveCancel.tpl.html';
        }
        
        return {
            restrict: 'E',          // don't knwo why, but this can't be an element]
            templateUrl: templateUrl,
            replace: true,
            transclude: false,
            scope: {
                areEditing: '=',
                areCreating: '=',
                isDisabled: '=',
                editClick: '&',
                cancelClick: '&',
                saveClick: '&',
                savePlusClick: '&',
                backClick: '&',
                actionsClick: '&',
                formTitle: '=?',
                hasModifyAccess: '=?',
                hasDeleteAccess: '=?',
                hasCreateAccess: '=?',
                pagerOptions: '=?'

            },
            link: function (scope) {
                // we need to use a watch function to access the spNavService via a closure.
                scope.watchFunction = function () {
                    return sp.result(spNavService.getCurrentItem(), 'actionPanelOptions.formTitle');
                };
                scope.$watch('watchFunction()', function (value) {
                    scope.formTitle = value;
                });
            }
        };
    });

}());