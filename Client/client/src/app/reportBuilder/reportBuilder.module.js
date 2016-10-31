// Copyright 2011-2016 Global Software Innovation Pty Ltd
/*global _, angular, console, sp, spEntity, spReportEntityQueryManager, spReportEntity, jsonLookup,
  ReportEntityQueryManager, spReportPropertyDialog */

(function () {
    'use strict';

    angular.module('app.reportBuilder', [
        'ui.router', 'spApps.reportServices', 'mod.common.ui.spReportBuilder',
        'mod.common.ui.spEntityComboPicker', 'mod.common.spEntityService',
        'sp.navService', 'mod.common.spResource', 'mod.common.ui.spReport',
        'mod.ui.spReportModelManager', 'mod.app.reportProperty', 'sp.common.spDialog',
        'mod.common.alerts', 'mod.app.reportSaveAs', 'mod.common.ui.spBusyIndicator',
        'sp.themeService']);
}());
