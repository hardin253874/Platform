// Copyright 2011-2016 Global Software Innovation Pty Ltd
/*global angular */

(function () {
    'use strict';

    angular.module('mod.services.workflowService', ['mod.services.promiseService',
        'mod.common.spWebService',
        'mod.common.spEntityService',
        'spApps.reportServices',
        'sp.app.settings',
        'mod.services.workflowConfiguration',
        'sp.common.spCalcEngineService',
        'sp.common.fieldValidator',
        'mod.featureSwitch',
        'app.controls.dialog.spEntitySaveAsDialog']);
})();

