// Copyright 2011-2016 Global Software Innovation Pty Ltd
/*global _, console, angular, sp */


(function() {
    "use strict";

    angular.module("mod.app.configureDialog.spVisibilityCalculationControl").component("spVisibilityCalculationControl", {
        bindings: {
            script: "<",
            typeId: "<",
            onScriptCompiled: "&",
            onScriptChanged: "&"
        },
        controller: VisibilityCalculationControlController,
        templateUrl: "configDialogs/visibilityCalculationControl/spVisibilityCalculationControl.tpl.html"
    });

    // ReSharper disable once InconsistentNaming
    /* @ngInject */
    function VisibilityCalculationControlController() {
        const ctrl = this;        

        ctrl.$onInit = function() {
            ctrl.host = "Any";
            ctrl.options = {
                expectedResultType: {
                    dataType: spEntity.DataType.Bool
                },
                onCompile: onCompile,
                onScriptChanged: onScriptChanged
            };            
        };        

        function onScriptChanged(script) {
            if (!_.isEmpty(_.trim(script)) &&
                ctrl.onScriptChanged) {
                ctrl.onScriptChanged({ script: script });
            }
        }

        function onCompile(res) {
            update(res.result);
        }

        function update(result) {
            if (!result) {
                return;
            }

            if (ctrl.onScriptCompiled) {
                // Have a valid script call result callback                
                ctrl.onScriptCompiled({ script: result.expression, error: result.error });
            }
        }
    }
}());