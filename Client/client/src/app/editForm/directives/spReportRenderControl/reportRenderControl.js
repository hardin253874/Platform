// Copyright 2011-2016 Global Software Innovation Pty Ltd
/*global _, console, angular, sp */

(function () {
    'use strict';

    angular.module('app.editForm.reportRenderControl', [
        'mod.app.editForm',
        'spApps.reportServices',
        'mod.ui.spReportModelManager',
        'mod.app.resourceScopeService',
        'sp.navService',
        'mod.common.spMobile'])
        .controller('reportRenderControl', ReportRenderControlController);

    /* @ngInject */
    function ReportRenderControlController($scope, $stateParams, spResourceScope, spNavService, spMobileContext, spState, spReportModelManager) {

        var reportId;
        var onScopeUpdateUnregisterListener;
        var isMobile = spMobileContext.isMobile;
        var stateName = spState.name;
        var model = {
            reportOptions: {
                reportId: 0,
                isEditMode: false,
                multiSelect: true,
                externalConds: null,
                isMobile: isMobile,
                isInScreen: stateName === 'screen'
            }
        };

        $scope.isMobile = isMobile;
        $scope.model = model;

        $scope.viewFullReport = function () {
            if (reportId) {
                if ($scope.model.reportOptions.externalConds &&
                    $scope.model.reportOptions.externalConds.length) {
                    spNavService.navigateToChildState('report', reportId, $stateParams, { conds: $scope.model.reportOptions.externalConds });
                } else {
                    spNavService.navigateToChildState('report', reportId);
                }                
            }
        };

        $scope.$watch('formControl', formControlChanged);

        $scope.$watch('model.reportOptions.selectedItems[0]', selectedItemChanged);

        $scope.$on('spReportEventReportRanAtleastOnce', function (event, recordCount) {
            // after report has run and if recordCount is 0 then send selected item id '-1' to
            // indicate that there is no item available in report.
            if (!recordCount) {
                sendScopeUpdate(-1);
            }
        });

        $scope.$on('$destroy', function () {
            if (onScopeUpdateUnregisterListener) {
                onScopeUpdateUnregisterListener();
                onScopeUpdateUnregisterListener = null;
            }
        });

        $scope.$on('spReportEventModelReady', function (event, model) {
            getControlNavState().reportState = spReportModelManager(model).getNavServiceData();
        });

        function formControlChanged(control) {

            if (!control) return;

            reportId = sp.result(control, 'reportToRender.eidP.idP');

            $scope.formTitle = control.name || control.reportToRender.name;

            model.reportOptions.reportId = reportId;
            model.reportOptions.formControlEntity = control;
            model.reportControl = control;

            restoreReportState();
            registerParentScreenElementScopeChangeListener();
        }

        function selectedItemChanged(item) {
            if (!item) return;
            if (_.isNumber(item.eid) && item.eid > 0) {
                sendScopeUpdate(item.eid);
            }
        }

        function registerParentScreenElementScopeChangeListener() {
            if ($scope.formControl.receiveContextFrom) {
                var channelId = spResourceScope.getChannelIdFromReceiver($scope.formControl);

                if (onScopeUpdateUnregisterListener) {
                    onScopeUpdateUnregisterListener();
                }

                onScopeUpdateUnregisterListener = spResourceScope.onScopeUpdate(channelId, function (actionData) {
                    if (actionData.drilldownConds) {
                        model.reportOptions.externalConds = actionData.drilldownConds;
                    }
                });
            }
        }

        function sendScopeUpdate(id) {
            var channelId = spResourceScope.getChannelIdFromSender($scope.formControl);
            if (channelId) {
                spResourceScope.sendScopeUpdate(channelId, id);
            }
        }

        function restoreReportState() {
            var existingModel = sp.result(getControlNavState(), 'reportState.reportModelManager.getModel');
            if (existingModel) {
                model.reportOptions.reportModel = existingModel;
            }
        }

        function getControlNavState() {
            return spState.getComponentState(getComponentKey());
        }

        function getComponentKey() {
            return 'id-' + sp.result($scope, 'model.reportControl.idP');
        }
    }
})();

