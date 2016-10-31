// Copyright 2011-2016 Global Software Innovation Pty Ltd
/*global angular, _, sp */

(function () {
'use strict';

    angular.module('mod.featureSwitch', ['sp.app.settings']);

    angular.module('mod.featureSwitch')
        .factory('rnFeatureSwitch', rnFeatureSwitch);

    /* @ngInject */
    function rnFeatureSwitch(spAppSettings) {

        return {
            isFeatureOn: spAppSettings.isFeatureOn,
			isAliasHidden: isAliasHidden
        };

		
		/*
		** If the given alias is hidden due to feature switching
		*/
		function isAliasHidden(alias) {
		    return ((alias === 'console:applicationConfigurationStaticPage' && !spAppSettings.isFeatureOn('applicationDependency')) || (alias === 'console:tenantRollbackStaticPage' && !spAppSettings.isFeatureOn('tenantRollback')));
		}
    }


})();

