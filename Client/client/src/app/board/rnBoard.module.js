// Copyright 2011-2016 Global Software Innovation Pty Ltd
/*global _, angular, console, sp, spEntity */

(function () {
    'use strict';

    angular.module('mod.app.board',
        [
            'mod.common.spEntityService',
            'spApps.reportServices',
            'sp.navService',
            'mod.app.navigationProviders',
            'mod.common.ui.spActionsService',
            'mod.common.ui.spContextMenu',
            'mod.common.spMobile',
            'sp.common.directives',
            'mod.common.ui.spReportPicker',
            'mod.ui.spReportMetadataManager',
            'titleService',
            'ngLetterAvatar',
            'mod.common.alerts',
            'sp.common.filters'
        ]);
    
}());
