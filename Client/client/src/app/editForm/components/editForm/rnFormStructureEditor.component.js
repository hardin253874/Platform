// Copyright 2011-2016 Global Software Innovation Pty Ltd

// import _ from 'lodash';
// import './rnFormStructureEditor.scss';
// import template from './rnFormStructureEditor.html';

(function () {

    angular.module('app.editFormComponents')
        .component('rnFormStructureEditor', {
            controller: FormStructureEditorController,
            bindings: {formEntity: '='},
            templateUrl: 'editForm/components/editForm/rnFormStructureEditor.tpl.html'
        });

    function FormStructureEditorController($scope, rnEditFormDndService) {
        'ngInject';

        const $ctrl = this; // match the implied var used in the template

        this.$onInit = () => {
        };

        this.delete = item => {
            if (item.parent) {
                item.parent.containedControlsOnForm.remove(item.control);
                onFormEntityUpdated();
            }
        };

        this.log = item => {
            console.log('form control => ', item);
        };

        this.toggleHorzResize = ({control}) => {
            //todo toggle it
            control.renderingHorizontalResizeMode = getNextResizeMode(control.renderingHorizontalResizeMode.nsAlias);
            onFormEntityUpdated();
        };

        this.toggleVertResize = ({control}) => {
            //todo toggle it
            control.renderingVerticalResizeMode = getNextResizeMode(control.renderingVerticalResizeMode.nsAlias);
            onFormEntityUpdated();
        };

        this.clearTransform = () => rnEditFormDndService.clearTransform();

        this.getTransform = () => rnEditFormDndService.getTransform();

        $scope.$watch('$ctrl.formEntity', onFormEntityUpdated);

        $scope.$watch('$ctrl.formEntity.graph.history.getChangeCounter()', onFormEntityUpdated);

        // $scope.$watch(
        //     () => rnEditFormDndService.getTransform(),
        //     x => {
        //         this.transform = x;
        //     }
        // );

        function onFormEntityUpdated() {

            $ctrl.items = $ctrl.formEntity ? flattenControls(0, null, $ctrl.formEntity) : [];
            $ctrl.items = _.map($ctrl.items, item => {
                // do any data massaging here
                return item;
            });

            $scope.$emit('doLayout');
        }

        function getNextResizeMode(mode) {
            const resizeModes = ['console:resizeSpring', 'console:resizeAutomatic',
                'console:resizeTwentyFive', 'console:resizeThirtyThree', 'console:resizeFifty',
                'console:resizeSixtySix', 'console:resizeSeventyFive', 'console:resizeHundred'];

            const index = _.indexOf(resizeModes, mode);
            return resizeModes[(index + 1) % resizeModes.length];
        }
    }

    function flattenControls(depth, parent, control) {
        const childItems = _.map(getOrderedContainedControls(control), _.partial(flattenControls, depth + 1, control));
        return _.flatten([{depth, parent, control}].concat(childItems));
    }

    function getOrderedContainedControls(container) {
        return _.sortBy(container.containedControlsOnForm, getRenderingOrdinal);
    }

    function getRenderingOrdinal(control) {
        return control.renderingOrdinal || 0;
    }

})();