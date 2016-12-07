// Copyright 2011-2016 Global Software Innovation Pty Ltd
/* globals rnEditForm */

(function () {

    angular.module('app.editFormComponents')
        .component('rnTabContainerControl', {
            bindings: {
                control: '<', resource: '<', form: '<',
                options: '<', formOptions: '<'
            },
            templateUrl: 'editForm/components/layoutControls/rnTabContainerControl.tpl.html',
            controller: TabContainerController
        });

    /* @ngInject */
    function TabContainerController($scope, spEditForm, rnEditFormDndService) {

        //todo
        // - track selected container in app/nav state so if return to the form we can re-select
        // the tab .... or should the top level form do this?
        // - track visited tabs and only render (using ng-if) if we have visited
        // - may need a new unique id for 'tabs' to handle creating new tabs in design mode
        // - maybe a whole lotta stuff about design mode, dnd etc...

        const $ctrl = this;

        $ctrl.childControlOptions = {};

        $ctrl.dragOptions = {
            onDragStart(e, d) {
                // console.log('onDragStart event', e);
                // console.log('onDragStart data', d);
            },
        };

        $ctrl.isSelected = c => {
            return c === $ctrl.selectedControl;
        };

        $ctrl.isRendered = c => {
            return _.indexOf($ctrl.visitedControls, c) >= 0;
        };

        $ctrl.select = c => {
            console.log('selected control', c.control.debugString);
            $ctrl.selectedControl = c;
            $ctrl.visitedControls = _.uniq(_.concat($ctrl.visitedControls || [], c));
        };

        this.$onInit = () => {
            $scope.$watchCollection('$ctrl.control.containedControlsOnForm', (x, y) => x === y || updateControls());
            $scope.$watch(() => rnEditFormDndService.getTransform(), (x, y) => x === y || updateControls());
        };

        this.$onChanges = changes => {
            // console.log('ContainerController', _.keys(changes));
            updateControls();
        };

        function updateControls() {
            const options = _.defaults({}, $ctrl.options, $ctrl.formOptions);

            $ctrl.hideContainerLabel = options.noTitle || $ctrl.control.hideLabel;

            $ctrl.containerType = $ctrl.control.getType().getAlias();

            $ctrl.containerCssClass = `rnContainerControl--${$ctrl.control.getType().getAlias()}`;

            $ctrl.controls = rnEditForm.getTransformedControls($ctrl.control, rnEditFormDndService.getTransform());

            _.forEach($ctrl.controls, c => {
                c.id = c.control.idP;
                c.name = spEditForm.getControlTitle(c.control);
                c.tooltip = c.control.description;
                c.options = {};

                c.cssClass = `rn-${c.control.renderingVerticalResizeMode.aliasOnly}`;
                if (c.state) c.cssClass += ` ${c.state && 'rn-control-state-' + c.state || ''}`;
                if (c.state) c.cssClass += ` ${c.dragging && 'rn-control-dragging' || ''}`;

                if ($ctrl.containerType === 'tabContainerControl') {
                    c.options.noTitle = true;
                }
            });

            $ctrl.visitedControls = [];
            $ctrl.select($ctrl.selectedControl && _.find($ctrl.controls, {id: $ctrl.selectedControl.idP}) || _.first($ctrl.controls));

            console.log('ContainerController', $ctrl.containerType);

        }
    }

})();