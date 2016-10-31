// Copyright 2011-2016 Global Software Innovation Pty Ltd
/*global _, console, angular, sp */


(function() {
    "use strict";

    angular.module("app.controls.spNavContainerPicker").component("spNavContainerPicker", {
        bindings: {
            containerId: "<",
            onContainerUpdated: "&"
        },
        controller: NavContainerPickerController,
        templateUrl: "controls/spNavContainerPicker/spNavContainerPicker.tpl.html"
    });

    // ReSharper disable once InconsistentNaming
    /* @ngInject */
    function NavContainerPickerController($scope, $element, $attrs, spNavContainerPickerDialog, spNavService, spAppSettings) {
        const ctrl = this;

        ctrl.update = function(containerId) {
            if (ctrl.containerId !== containerId) {
                ctrl.containerId = containerId;
                ctrl.onContainerUpdated({ containerId: containerId });
                updateSelectedContainerPath();
            }
        };

        ctrl.onClearClick = function() {
            ctrl.update(null);
        };

        ctrl.onBrowseClick = function() {
            const options = {
                selectedContainerId: ctrl.containerId,
                isSelfServeMode: spAppSettings.selfServeNonAdmin
            };

            spNavContainerPickerDialog.showModalDialog(options).then(function(result) {
                if (!result) {
                    return;
                }

                ctrl.update(result);
            });
        };

        ctrl.$onInit = function() {
            updateSelectedContainerPath();
        };

        ctrl.$onChanges = function() {
            updateSelectedContainerPath();
        };

        function updateSelectedContainerPath() {
            if (!ctrl.containerId) {
                ctrl.selectedContainerPath = "";
                return;
            }

            // calculate path up to root
            const nodeNames = [];
            const pathInReverse = [];
            const selectedNode = spNavService.findInTreeById(spNavService.getNavTree(), ctrl.containerId, pathInReverse);

            if (!selectedNode) {
                ctrl.selectedContainerPath = "";
                return;
            }

            nodeNames.push(selectedNode.item.name);

            _.forEach(pathInReverse, function(n) {
                if (n && n.item) {
                    nodeNames.push(n.item.name);
                }
            });

            ctrl.selectedContainerPath = _.join(_.reverse(nodeNames), "/");
        }
    }
}());