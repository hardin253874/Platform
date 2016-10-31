// Copyright 2011-2016 Global Software Innovation Pty Ltd
/*global angular, console, _ */
(function () {
    'use strict';

    angular.module('app.demoDialog', ['ui.router', 'sp.common.spDialog'])


        .constant('DialogState', {
            name: 'demoDialog',
            url: '/controls/dialog',
            views: {
                'content@': {
                    controller: 'dialogController',
                    templateUrl: 'controls/dialog/demoDialog.tpl.html'
                }

            }
        })

     .controller('dialogController', function ($scope, spDialog) {

                 
         $scope.openConfirmMessageBox = function (title, msg) {

             spDialog.confirmDialog(title, msg).then(function(result) {
                 window.alert(result);
             });
         };
         
         $scope.openMessageBoxPostServer = function (title, msg) {
             spDialog.showUserError(title, msg, true);
         };
         
         $scope.openMessageBox = function (title, msg) {
             spDialog.showUserError(title, msg);
         };
       
     });

}());

