// Copyright 2011-2016 Global Software Innovation Pty Ltd
/*global _, console, angular, sp, spEntity */

(function () {
    'use strict';

    //todo - investigate why so many dependencies here!?

    angular.module('mod.app.screen', [
        'ui.router',
        'ui.bootstrap',
        'titleService',
        'mod.common.spEntityService',
        'ngGrid',
        'mod.app.editFormServices',
        'mod.app.editForm.designerDirectives',
        'mod.app.editForm.designerDirectives',
        'mod.common.editForm.editFormDirectives',
        'sp.navService',
        'sp.common.filters',
        'mod.common.alerts',
        'mod.app.resourceScopeService',
        'app.editForm.formRenderControl',
        'mod.app.navigationProviders',
        'mod.common.ui.spContextMenu',
        'mod.common.ui.spMeasureArrange',
        'sp.themeService',
        'mod.common.spMobile',
        'mod.featureSwitch']);

    angular.module('mod.app.screen')
        .config(screenStateConfiguration)
        .controller('screenController', ScreenController);

    /* @ngInject */
    function screenStateConfiguration($stateProvider, $compileProvider) {

        $stateProvider.state('screen', {
            url: '/{tenant}/{eid}/screen?path',
            templateUrl: 'editForm/screen.tpl.html',
            data: {
                showBreadcrumb: false,
                region: {'content-header': {templateUrl: 'editForm/screenContentHeader.tpl.html'}}
            }
        });

        // Allow the standard set of links
        $compileProvider.aHrefSanitizationWhitelist(/^\s*(https?|ftp|mailto|file|tel):/);
    }

    /* @ngInject */
    function ScreenController($scope, $element, $stateParams, titleService, $timeout, spEditForm, spNavService,
                              spAlertsService, spNavigationBuilderProvider, spMeasureArrangeService, spThemeService,
                              spMobileContext, rnFeatureSwitch, consoleIconService) {

        var navigationBuilderProvider = spNavigationBuilderProvider($scope);

        $scope.allowFlexForm = rnFeatureSwitch.isFeatureOn('flexEditForm');
        $scope.useFlexForm = false;
        $scope.toggleFlexForm = function () {
            if (!$scope.allowFlexForm) return;
            $scope.useFlexForm = !$scope.useFlexForm;
            doLayout();
        };
        $scope.showStructureEditor = false;//$scope.allowFlexForm;
        $scope.toggleFormStructureEditor = function () {
            if (!$scope.allowFlexForm) return;
            $scope.showStructureEditor = !$scope.showStructureEditor;
            doLayout();
        };

        $scope.$on('doLayout', function (event) {
            doLayout();
        });

        $scope.isMobile = spMobileContext.isMobile;
        $scope.isTablet = spMobileContext.isTablet;
        $scope.busyIndicator = {
            type: 'spinner',
            text: 'Loading...',
            placement: 'element',
            isBusy: true
        };

        $scope.measureArrangeOptions = {
            id: 'screen'
        };

        $scope.item = spNavService.getCurrentItem();

        if (!$scope.item) {
            // If this happens, it's going to be very bad.
            console.error('screenController: no current nav item');
            return;
        }
        if (!$scope.item.data) {
            $scope.item.data = {};
        }

        $scope.navService = spNavService;

        $scope.model = {
            formId: $stateParams.eid ? $stateParams.eid : 0,
            areEditingChildForms: false,
            isInTestMode: $stateParams.test,
            configContextMenu: {}
        };

        $scope.item.data = _.extend($scope.item.data, $scope.model);
        $scope.consoleThemeModel = {
            consoleTheme: null,
            titleStyle: {}
        };

        // Navigate to builder to modify the screen.
        $scope.configMenuModifyEntity = function () {
            if (!spNavService.isSelfServeEditMode) {
                return;
            }

            //
            // To the designer as a child
            //
            $scope.item.data = null; // clear the form and data so it will be refreshed on the return
            spNavService.navigateToChildState('screenBuilder', $scope.model.formControl.id());
        };


        // Configure the screen's properties
        $scope.configMenuUpdateEntityProperties = function () {
            if (!spNavService.isSelfServeEditMode) {
                return;
            }

            navigationBuilderProvider.configureNavItem($scope.model.formControl);
        };


        // Delete the screen
        $scope.configMenuDeleteEntity = function () {
            if (!spNavService.isSelfServeEditMode) {
                return;
            }

            navigationBuilderProvider.removeNavItem($scope.model.formControl);
        };

        //
        // Get and set the theme of screen
        //
        $scope.$watch('navService.getThemes()', function (getThemesCompleted) {
            if (getThemesCompleted === true) {
                $scope.consoleThemeModel.titleStyle = spThemeService.getTitleStyle();
            }
        });

        $scope.$on('calcTabsLayout', function (event, callback) {
            event.stopPropagation();
            callback($scope.measureArrangeOptions.id);
        });

        $scope.$on('measureArrangeComplete', function (event) {

            if ($scope && $scope.model && $scope.model.formControl) {

                if ($element) {

                    var titleElement = $element.find('> .screen-title');
                    if (titleElement) {

                        var titleHeight = titleElement.outerHeight(true);
                        if (titleHeight && titleHeight > 0) {

                            var contentElement = $element.find('> sp-custom-edit-form');
                            if (contentElement) {
                                contentElement.css('height', 'calc(100% - ' + titleHeight + 'px)');
                            }
                        }
                    }
                }
            }
        });

        activate();

        function activate() {

            titleService.setTitle('Screen');

            //
            // Error out if there is no formId (screen id)
            // NOTE, this should probably be changed to look for the default form.
            //
            if (!$scope.model.formId) {
                spAlertsService.addAlert('No form Id specified.', spAlertsService.sev.Error);
                return;
            }

            //
            // Get the definition of the form we are displaying from the server
            //
            //console.log('DEBUG: screen: getting formDefinition ', $scope.$id, $scope.model.formId);
            spEditForm.getFormDefinition($scope.model.formId).then(
                function (form) {
                    $scope.model.formControl = form;
                    $scope.model.configContextMenu = navigationBuilderProvider.buildConfigureContextMenu(form);

                    doLayout();
                    setHeaderIconAndStyle();
                },
                function (error) {
                    spAlertsService.addAlert('An error occurred getting the screen: ' + sp.result(error, 'data.Message'), spAlertsService.sev.Error);
                });
        }

        // Update the form header icon and icon background
        function setHeaderIconAndStyle() {
            if (!$scope.model.formControl) {
                return;
            }

            var iconInfo = consoleIconService.getNavItemIconUrlAndBackColor($scope.model.formControl);
            if (iconInfo) {
                var iconUrl = iconInfo.iconUrl;
                if (iconUrl) {
                    $scope.model.headerIconUrl = iconUrl;

                    $scope.model.headerIconStyle = {};
                    if (iconInfo.iconBackgroundColor) {
                        $scope.model.headerIconStyle['background-color'] = sp.getCssColorFromARGBString(iconInfo.iconBackgroundColor);
                    }
                }
            }
        }

        function doLayout() {
            if (!$scope.useFlexForm) {
                $timeout(function () {
                    spMeasureArrangeService.performLayout($scope.measureArrangeOptions.id);
                    $scope.busyIndicator.isBusy = false;
                });
            }
        }
    }

}());