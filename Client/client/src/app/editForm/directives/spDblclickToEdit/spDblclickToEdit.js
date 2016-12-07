// Copyright 2011-2016 Global Software Innovation Pty Ltd
/*global console, _, angular */

(function () {
    'use strict';
    
    angular.module("mod.app.editForm.spDblclickToEdit", []);

    angular.module("mod.app.editForm.spDblclickToEdit")
        .directive("spDblclickToEdit", spDblclickToEdit);

    /* @ngInject */
    function spDblclickToEdit() {
        return {            
            restrict: "A",
            link: link
        };

        function link(scope, element) {
            function onDblclick() {
                if (scope.formMode === "edit" || scope.isInDesign || scope.isInlineEditing) {
                    return;
                }

                scope.$emit("enterEditModeFromDblclick");
            }

            element.on("dblclick", onDblclick);

            scope.$on("destroy", () => element.off("dblclick", onDblclick));
        }
    }
}());