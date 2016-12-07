// Copyright 2011-2016 Global Software Innovation Pty Ltd
/*global console, _, angular */

(function () {
    'use strict';
    
    angular.module("mod.app.editForm.designerDirectives.spReportWatermarkControl", []);

    angular.module("mod.app.editForm.designerDirectives.spReportWatermarkControl")
        .directive("spReportWatermarkControl", spReportWatermarkControl);

    /* @ngInject */
    function spReportWatermarkControl() {
        return {
            template: '<div class="div_div_reportwatermark">REPORT DATA NOT AVAILABLE IN BUILDER MODE</div>',
            restrict: "E"
        };        
    }
}());