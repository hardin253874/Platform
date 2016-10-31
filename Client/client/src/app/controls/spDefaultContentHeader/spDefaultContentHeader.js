// Copyright 2011-2016 Global Software Innovation Pty Ltd


(function () {
    'use strict';


    /////
    // The default content header for a page
    /////
    angular.module('app.controls.spDefaultContentHeader', ['sp.navService', 'mod.common.spMobile', 'sp.themeService'])
        .directive('spDefaultContentHeader', function(spEntityService, spNavService, spPromiseService, spMobileContext) {


            /////
            // Directive structure.
            /////
            return {
                restrict: 'E',
                templateUrl: 'controls/spDefaultContentHeader/spDefaultContentHeader.tpl.html',
            };
        })

     .controller('defaultContentHeaderController',
       function defaultContentHeaderPanelController($scope, $rootScope, spNavService, spMobileContext, spThemeService) {

           $scope.toggleNavigation = function () {
               $scope.navData.userShowNav = !$scope.navData.userShowNav;
               $scope.navData.showNav = $scope.navData.userShowNav;
           };
           
           $scope.isMobile = spMobileContext.isMobile;
           $scope.isTablet = spMobileContext.isTablet;
           $scope.nav = spNavService;
           $scope.currentNavItem = spNavService.getCurrentItem();
           $scope.navBarStyle = spThemeService.getNavBarStyle();
           $scope.titleStyle = spThemeService.getTitleStyle();
        
           $scope.$watch('nav.getThemes()', function (getThemesCompleted) {
               if (getThemesCompleted === true) {
                   $scope.navBarStyle = spThemeService.getNavBarStyle();
                   $scope.titleStyle = spThemeService.getTitleStyle();
               }
           });

         
           $scope.$on('$stateChangeSuccess', function () {
               $scope.currentNavItem = spNavService.getCurrentItem();     
           });
       });


}());