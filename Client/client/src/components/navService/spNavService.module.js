// Copyright 2011-2016 Global Software Innovation Pty Ltd
/*global angular */

(function () {
    "use strict";

    angular.module('sp.navService',
        [
            'ui.router',
            'mod.common.spEntityService',
            'sp.common.loginService',
            'mod.common.spTenantSettings',
            'sp.app.settings',
            'sp.app.navigation',
            'sp.themeService',
            'mod.common.spMobile',
            'mod.common.spNgUtils',
            'mod.featureSwitch',
            'mod.app.spDocumentationService'
        ]);
})();
