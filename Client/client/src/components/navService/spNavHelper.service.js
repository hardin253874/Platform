// Copyright 2011-2016 Global Software Innovation Pty Ltd
/*global _, angular, console, sp, spEntity */

(function () {
    "use strict";

    angular.module('sp.spNavHelper', [
        'spApps.reportServices',
        'mod.app.reportProperty',
        'sp.navService',
        'mod.app.chartBuilder.controllers.spNewChartDialog'
    ]);

    /**
     *  A set of client side services working against the entity webapi service.
     *  @module spReportService
     */

    angular.module('sp.spNavHelper')
        .factory('spNavHelper', spNavHelper);

    /* @ngInject */
    function spNavHelper(spReportPropertyDialog, spNavService, spNewChartDialog) {

        var exports = {
            createReport: createReport,
            createChart: createChart
        };
        return exports;

        /**
         * create report by report property dialog window, then navigate to report builder
         * @returns N/A
         */
        function createReport(options, create, createFromScreenBuilder) {
            // Valid options:
            // .folder
            // .typeId

            spReportPropertyDialog.showModalDialog(options).then(function (result) {
                if (!result)
                    return;

                if (result.reportId && result.reportId > 0) {

                    //var currentNavItem = spNavService.getCurrentItem();
                    var parentNavItem = spNavService.getParentItem();

                    if (parentNavItem) {
                        if (parentNavItem.data && !sp.isNullOrUndefined(parentNavItem.data.createNewReport)) {
                            parentNavItem.data.createNewReport = true;

                            if (createFromScreenBuilder) {
                                if (!sp.isNullOrUndefined(parentNavItem.data.createFromScreenBuilder)) {
                                    parentNavItem.data.createFromScreenBuilder = true;
                                } else {
                                    parentNavItem.data = _.extend(
                                        parentNavItem.data || {},
                                        {
                                            createFromScreenBuilder: true
                                        });
                                }
                            }

                        } else {
                            parentNavItem.data = _.extend(
                                parentNavItem.data || {},
                                {
                                    createNewReport: true,
                                    createFromScreenBuilder: createFromScreenBuilder ? true : false
                                });
                        }
                    }

                    if (create) {
                        spNavService.navigateToChildState(
                            'reportBuilder',
                            result.reportId,
                            {returnOnCompletion: true});
                    } else {
                        /////
                        // TODO: Why was this done??
                        /////

                        //spNavService.navigateToChildState(
                        //           'report',
                        //           result.reportId,
                        //           { returnOnCompletion: true });
                        spNavService.navigateToSibling('report', result.reportId);
                    }
                }
                else if (result.report) {
                    /////
                    // TODO: Where is this method?
                    /////
                    exports.updateReportModel(-1, result.report).then(function (reportModelResponse) {
                        if (reportModelResponse) {
                            spNavService.navigateToChildState(
                                'reportBuilder',
                                reportModelResponse,
                                {returnOnCompletion: true});
                        }
                    });
                }
            });
        }

        /**
         * create chart then navigate to the chart builder
         * @returns N/A
         */
        function createChart(options) {
            // Valid options:
            // .folder (not implemented)
            // .typeId (not implemented)
            // .reportId
            spNewChartDialog.showDialog(options);
        }
    }

})();
