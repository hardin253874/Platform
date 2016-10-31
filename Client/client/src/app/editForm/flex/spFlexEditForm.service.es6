// Copyright 2011-2016 Global Software Innovation Pty Ltd
/*global _, angular, console, sp */

(function () {
    'use strict';

    angular.module('app.flexEditForm')
        .service('spFlexEditFormService', spFlexEditFormService);

    /* @ngInject */
    function spFlexEditFormService($injector, spFormBuilderService) {
        return {
            getDirectiveNameForControl
        };

        function getDirectiveNameForControl(c) {
            let alias = sp.result(c, ['typesP', 0, 'getAlias']) || '##ERROR-NO-TYPE-ALIAS##';
            let directiveName = alias.charAt(0).toUpperCase() + alias.slice(1) + 'Directive';
            let elementName = alias.replace(/(.*?)([A-Z])/g, '$1-$2').toLowerCase();

            return $injector.has('rn' + directiveName) ? 'rn-' + elementName :
                $injector.has('sp' + directiveName) ? 'sp-' + elementName :
                    'rn-default-field-control';
        }

        function isContainer(alias) {
            let [, a] = alias.split(':');
            return spFormBuilderService.isContainer(a || alias);
        }
    }

}());
