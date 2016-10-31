// Copyright 2011-2016 Global Software Innovation Pty Ltd
/*global _, angular, console, sp */

(function () {
    'use strict';

    /**
     * Module implementing a report view.
     *
     * It contains a report directive and is integrated with the navigation service.
     * This means that navigating back to this page from a child page will initialise the report from that prior state.
     * When this page is navigated to it checks with the navigation service to see if it has prior state.
     * If it does, it initialises the reportModel field of the report with the report model stored by the navigation service.
     * The page then registers the latest model with the navigation service with an object returned from the
     * spReportModelManager.getNavServiceData() method.
     *
     * @module report
     */
    angular.module('app.report', ['ui.router', 'titleService', 'mod.common.spEntityService', 'mod.common.spNgUtils',
        'spApps.reportServices', 'sp.navService', 'sp.spNavHelper', 'mod.common.spResource', 'mod.common.ui.spReport',
        'mod.ui.spReportModelManager', 'mod.app.reportProperty', 'mod.common.ui.spContextMenu', 'mod.app.navigationProviders',
        'sp.themeService', 'mod.common.spMobile', 'mod.app.editFormCache', 'sp.consoleIconService']);

    angular.module('app.report')
        .config(reportViewStateConfiguration)
        .controller('ReportController', ReportController)
        .controller('reportContentHeaderController', ReportContentHeaderPanelController);

    /* @ngInject */
    function reportViewStateConfiguration($stateProvider) {

        var data = {
            showBreadcrumb: false,
            region: {'content-header': {templateUrl: 'report/reportContentHeader.tpl.html'}}
        };

        $stateProvider.state('report', {
            url: '/{tenant}/{eid}/report?path',
            templateUrl: 'report/report.tpl.html',
            data: data
        });
        $stateProvider.state('drilldown', {
            url: '/{tenant}/{eid}/drilldown?path&typeIdFilter',
            templateUrl: 'report/report.tpl.html',
            data: data
        });
    }

    /* @ngInject */
    function ReportController($scope, $stateParams, $timeout, titleService, spEntityService, spNgUtils, spState,
                              spNavService, spReportModelManager, spNavHelper, spNavigationBuilderProvider,
                              spThemeService, spMobileContext, editFormCache, consoleIconService) {

        var existingReportModel;
        var currentReportModel;
        var existingAggregateRows;
        var navigationBuilderProvider = spNavigationBuilderProvider($scope);

        //sp.logTime('ReportController created');

        $scope.eid = $stateParams.eid;
        $scope.entity = {};
        $scope.entityInfoRequest = 'alias,name,description,isOfType.{alias,name, ' +
            'k:typeConsoleBehavior.{ k:treeIcon.{ name, imageBackgroundColor}}}, isPrivatelyOwned, ' +
            'k:navigationElementIcon.{ alias, name, imageBackgroundColor}, ' +
            'reportUsesDefinition.{ name,  k:typeConsoleBehavior.{ k:treeIcon.{ name, imageBackgroundColor}} }';
        $scope.isMobile = spMobileContext.isMobile;
        $scope.isTablet = spMobileContext.isTablet;
        $scope.nav = spNavService;
        $scope.consoleThemeModel = {consoleTheme: null, titleStyle: {}};

        $scope.model = {
            reportOptions: {
                reportId: 0,
                reportModel: null,
                reportObject: null,
                reportEntity: null,
                isEditMode: false,
                isMobile: spMobileContext.isMobile,
                multiSelect: true,
                fastRun: true
            },
            configContextMenu: {}
        };

        $scope.reportbuilderClick = function () {
            spNavService.navigateToChildState('reportBuilder', $stateParams.eid, $stateParams);
        };

        $scope.createReport = function () {
            var folder = _.last($stateParams.path.split(','));
            var reportPropertyOptions = {folder: folder};
            spNavHelper.createReport(reportPropertyOptions);
        };

        // Navigate to builder to modify the report.
        $scope.configMenuModifyEntity = function () {
            if (!spNavService.isSelfServeEditMode) {
                return;
            }

            spNavService.navigateToChildState('reportBuilder', $stateParams.eid, $stateParams);
        };

        // Configure the report's properties
        $scope.configMenuUpdateEntityProperties = function () {
            if (!spNavService.isSelfServeEditMode) {
                return;
            }

            navigationBuilderProvider.configureNavItem($scope.entity, null, 'core:report');
        };

        // Delete the report and remove nav element
        $scope.configMenuDeleteEntity = function () {
            if (!spNavService.isSelfServeEditMode) {
                return;
            }

            navigationBuilderProvider.removeNavItem($scope.entity, true);
            editFormCache.invalidateFormsForEntity($scope.entity.idP);
        };

        // Remove nav element only, do not delete the report
        $scope.configMenuRemoveEntity = function () {
            if (!spNavService.isSelfServeEditMode) {
                return;
            }

            navigationBuilderProvider.removeNavItem($scope.entity, false);
        };

        $scope.$watch('nav.getThemes()', function (getThemesCompleted) {
            if (getThemesCompleted === true) {
                $scope.consoleThemeModel.titleStyle = spThemeService.getTitleStyle();
            }
        });

        $scope.$on('spReportEventModelReady', function (event, model) {
            onModelReady(model);
        });

        syncNavItem();
        if (!maybeNavigateNow()) {
            initPage();
        }

        function initPage() {

            titleService.setTitle('Report');
            setFocusOnFirst();

            // Attached existing report model to the report options
            // The report control will load from the existing model instead of the id
            $scope.model.reportOptions.reportModel = existingReportModel;

            var idOrAlias = $scope.eid;
            if (!idOrAlias) {
                return;
            }

            idOrAlias = sp.coerseToNumberOrLeaveAlone(idOrAlias);

            if (_.isNumber(idOrAlias)) {
                $scope.model.reportOptions.reportId = idOrAlias; // set asap so we can get the request to the server more quickly
            }

            spEntityService.getEntity(idOrAlias, $scope.entityInfoRequest, {
                hint: 'refreshReport',
                batch: true
            }).then(function (entity) {
                if (!entity) {
                    return;
                }

                $scope.entity = entity;
                $scope.model.reportOptions.title = entity.name;
                $scope.model.reportOptions.reportId = entity.idP;

                $scope.model.configContextMenu = navigationBuilderProvider.buildConfigureContextMenu(entity);

                // set header icon and style
                setHeaderIconAndStyle();
            }, function (error) {
                console.error('Error getting report: ' + error);
            });

        }

        function syncNavItem() {

            var state = spState.getPageState();

            if (state.reportModelManager) {

                existingAggregateRows = state.reportModelManager.getAggregateRows();
                $scope.model.reportOptions.existingAggregateRows = existingAggregateRows;

                // The updatedReport flag is used to detect if this report is modified by report builder
                // if we are navigated to report builder and then back.

                if (!state.updatedReport && state.reportModelManager.getModel().reportId > 0) {
                    existingReportModel = state.reportModelManager.getModel();
                }

                // We need to set the updatedReport property on our nav item, as report builder will only
                // set it true if the property exists (so it doesn't pollute the nav item of non-report items
                // in other navigation scenarios) - (this is my theory on this code - sg)

                state.updatedReport = false;
            }
        }

        function maybeNavigateNow() {

            // Check for instructions in the parent nav item data that indicate
            // we are to immediately navigation elsewhere. Clear the instructions
            // so we only do it once.

            var parentState = spState.getParentPageState() || {};

            if (parentState.createNewReport) {
                parentState.createNewReport = false;
                spNavService.navigateToChildState('reportBuilder', $stateParams.eid, $stateParams);
                return true;
            }

            if (parentState.createFromScreenBuilder) {
                parentState.createFromScreenBuilder = false;
                spNavService.navigateToParent();
                return true;
            }

            return false;
        }

        function onModelReady(model) {

            // The model is ready, register it with the nav service

            currentReportModel = model;

            var state = spState.getPageState();

            _.assign(state, spReportModelManager(model).getNavServiceData());
            state.quickSearch = model.quickSearch;

            if (existingAggregateRows) {
                state.reportModelManager.setAggregateRows(existingAggregateRows);
            }

            // We use nav's "dataObject" mechanism to receive external analyser conditions
            var conds = sp.result(spNavService, 'getCurrentItem.dataObject.conds');
            if (!_.isEmpty(conds)) {
                model.externalConds = conds;
                state.reportModelManager.mergeFilterConditions();
            }

            var currentTheme = spNavService.getCurrentTheme();
            if (currentTheme) {
                $scope.consoleThemeModel.consoleTheme = currentTheme;
            }
        }

        function setFocusOnFirst() {
            // set focus on first visible input in the document if its not the mobile and tablet device
            if (!$scope.isMobile && !$scope.isTablet) {
                $timeout(function () {
                    spNgUtils.setFocusOnFirstVisibleInput();
                }, 0);
            }
        }

        function setHeaderIconAndStyle() {
            var iconInfo = consoleIconService.getNavItemIconUrlAndBackColor($scope.entity);
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

        //sp.logTime('ReportController ready');
    }

    /* @ngInject */
    function ReportContentHeaderPanelController($scope, spNavService, spMobileContext) {
        // This controller is only used on mobile

        $scope.spMobileContext = spMobileContext;

        $scope.toggleNavigation = function () {
            $scope.navData.userShowNav = !$scope.navData.userShowNav;
            $scope.navData.showNav = $scope.navData.userShowNav;
        };


        $scope.navigateToParent = function () {
            spNavService.navigateToParent();
        };

        // This panel is not recreated so we have to watch for navigation changes.
        $scope.$watch(
            function () {
                return spNavService.getCurrentItem();
            },
            function (currentItem) {
                $scope.currentNavItem = currentItem;
                $scope.haveParent = !spNavService.isParentNavItem();
            });

    }
}());
