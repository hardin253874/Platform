// Copyright 2011-2016 Global Software Innovation Pty Ltd
/*global _, console, angular */

(function () {
    'use strict';
    /**
     * @ngdoc service
     * @name mod.common.spVisDataActionService:spVisDataActionService
     *
     *  @description This service can be used anywhere to excute click action on chart or hero text.
     */

    angular.module('mod.common.spVisDataActionService', ['ng', 'mod.app.resourceScopeService', 'sp.navService'])
        .factory('spVisDataActionService', function ($stateParams, spResourceScope, spNavService) {
            var exports = {};

            //Execute click action on a chart segment or data point.
            function executeClickAction(params, isPivotChart, reportEntity) {
                if (isPivotChart) {
                    if (params && params.drilldownConds) {
                        var reportId = reportEntity.idP;
                        if (reportId)
                            spNavService.navigateToChildState('report', reportId, $stateParams, { conds: params.drilldownConds });
                    }
                } else {
                    if (params > 0) {
                        spNavService.navigateToChildState('viewForm', params, $stateParams);
                    }
                }
            }

            exports.executeClickAction = executeClickAction;

            return exports;
        });
}());