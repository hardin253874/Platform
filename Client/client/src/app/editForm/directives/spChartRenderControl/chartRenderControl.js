// Copyright 2011-2016 Global Software Innovation Pty Ltd
//
// Represents a chart hosted on an edit form
//
/*global _, console, angular, sp */

(function () {
    'use strict';

    angular.module('app.editForm.chartRenderControl',
        [
            'mod.app.editForm',
            'spApps.reportServices',
            'mod.app.resourceScopeService',
            'sp.navService',
            'mod.common.spVisDataActionService',
            'mod.common.spVisDataService',
            'mod.common.spMobile'
        ]);

    angular.module('app.editForm.chartRenderControl')
        .controller('chartRenderControl', chartRenderControl);

    /* @ngInject */
    function chartRenderControl($scope, spResourceScope, spVisDataService, spVisDataActionService, spState, spMobileContext) {
        var chartId;
        var onScopeUpdateUnregisterListener;
        var condsChangedBySender = false;        

        $scope.model = {
            chartOptions: {
                chartId: 0,
                isEditMode: false,
                isInScreen: spState.name === 'screen'
            }
        };

        $scope.$watch('formControl', formControlChanged);
        $scope.$watch('model.chartOptions.selectedEntityId', selectedEntityIdChanged);
        $scope.$watch('model.chartOptions.drilldownConds', conditionsChanged);

        $scope.$on('$destroy', function () {
            if (onScopeUpdateUnregisterListener) {
                onScopeUpdateUnregisterListener();
                onScopeUpdateUnregisterListener = null;
            }
        });

        function formControlChanged(control) {
            if (control && control.getChartToRender()) {
                chartId = control.getChartToRender().eid();
                $scope.model.chartOptions.chartId = chartId.id();

                $scope.formTitle = control.name || control.getChartToRender().name;

                // check if we are getting our context from elsewhere
                var contextSender = control.getReceiveContextFrom();

                if (contextSender) {
                    var channelId = spResourceScope.getChannelIdFromReceiver(control);

                    if (onScopeUpdateUnregisterListener) {
                        onScopeUpdateUnregisterListener();
                    }

                    // receive context update.
                    onScopeUpdateUnregisterListener = spResourceScope.onScopeUpdate(channelId, function (actionData) {
                        if (actionData.drilldownConds) {
                            $scope.model.chartOptions.externalConds = actionData.drilldownConds;
                            condsChangedBySender = true;
                        }
                    });
                }
            }
        }

        function selectedEntityIdChanged(id) {
            if (id) {
                console.log('chartRenderControl clicked id:', id);
                updateScopeIfApplicable(id);
                executeClickAction(id, false);
            }
        }

        function conditionsChanged(newDrilldownConds, oldDrilldownConds) {
            //if the init of newDrilldownConds is set as empty array and oldDrilldownConds is null or undefined, the drilldownConds is not changed            
            if (!newDrilldownConds || (!oldDrilldownConds && newDrilldownConds.length === 0))
                return;        

            console.log('chartRenderControl clicked filter');
            var actionData = { drilldownConds: newDrilldownConds };

            // note - if on mobile we ignore on-screen linking and do the default drilldown

            if (spMobileContext.isMobile || !updateScopeIfApplicable(actionData)) {
                if (!spVisDataService.isEmptyConds(newDrilldownConds)) {
                    executeClickAction(actionData, true);
                }
            }
        }

        function executeClickAction(params, isPivotChart) {
            var reportEntity = sp.result($scope.formControl, 'chartToRender.chartReport');
            spVisDataActionService.executeClickAction(params, isPivotChart, reportEntity);
        }

        function updateScopeIfApplicable(params) {
            var changed = condsChangedBySender;
            var entity = $scope.formControl;

            // Reset change
            condsChangedBySender = false;

            if (entity.sendContextTo && entity.sendContextTo.length > 0) {
                var channelId = spResourceScope.getChannelIdFromSender(entity);
                spResourceScope.sendScopeUpdate(channelId, params);
                changed = true;
            }

            return changed;
        }
    }

})();