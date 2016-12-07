// Copyright 2011-2016 Global Software Innovation Pty Ltd
/* globals rnEditForm */

(function () {


    angular.module('app.editFormComponents')
        .component('rnExpanderContainerControl', {
            bindings: {
                control: '<', resource: '<', form: '<',
                options: '<', formOptions: '<'
            },
            templateUrl: 'editForm/components/layoutControls/rnExpanderContainerControl.tpl.html',
            controller: ExpanderContainerController
        });

    function ExpanderContainerController($scope, rnEditFormDndService, spEditForm) {
        'ngInject';

        const $ctrl = this;

        $ctrl.childControlOptions = {};

        $ctrl.isSelected = c => {
            return c === $ctrl.selectedControl;
        };

        $ctrl.isRendered = c => {
            return _.indexOf($ctrl.visitedControls, c) >= 0;
        };

        $ctrl.select = c => {
            $ctrl.selectedControl = c;
            $ctrl.visitedControls = _.uniq(_.concat($ctrl.visitedControls || [], c));
        };

        this.$onInit = () => {
            $scope.$watchCollection('$ctrl.control.containedControlsOnForm', (x, y) => x === y || updateControls());
            $scope.$watch(() => rnEditFormDndService.getTransform(), (x, y) => x === y || updateControls());
        };

        this.$onChanges = changes => {
            // console.log('ExpanderContainerController', _.keys(changes));
            updateControls();
        };

        function updateControls() {
            const options = _.defaults({}, $ctrl.options, $ctrl.formOptions);

            $ctrl.controls = rnEditForm.getTransformedControls($ctrl.control, rnEditFormDndService.getTransform());
            _.forEach($ctrl.controls, c => {
                c.id = c.control.idP;
                c.name = spEditForm.getControlTitle(c.control);
                c.tooltip = c.control.description;
                c.options = {noTitle: true};

                c.cssClass = `rn-${c.control.renderingVerticalResizeMode.aliasOnly}`;
                if (c.state) c.cssClass += ` ${c.state && 'rn-control-state-' + c.state || ''}`;
                if (c.state) c.cssClass += ` ${c.dragging && 'rn-control-dragging' || ''}`;
            });

            $ctrl.hideContainerLabel = options.noTitle || $ctrl.control.hideLabel;

            $ctrl.visitedControls = [];
            $ctrl.select($ctrl.selectedControl && _.find($ctrl.controls, {id: $ctrl.selectedControl.idP}) || _.first($ctrl.controls));
        }
    }

})();