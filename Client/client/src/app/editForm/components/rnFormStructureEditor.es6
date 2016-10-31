// Copyright 2011-2016 Global Software Innovation Pty Ltd
/*global _, angular, console, sp, spEntity */

(function () {
    'use strict';

    angular.module('app.editFormComponents')
        .component('rnFormStructureEditor', {
            controller: FormStructureEditorController,
            bindings: {formEntity: '='},
            template: `
<table style="width: 100%">
    <thead>
    <tr>
        <th>depth</th>
        <th>name</th>
        <th>field or rel</th>
        <th>type</th>
        <th>ordinal</th>
        <th>horz</th>
        <th>vert</th>
    </tr>
    </thead>
    <tbody>
    <tr ng-repeat="item in $ctrl.items">
        <td>{{item.depth}}</td>
        <td title="{{item.control.idP}}">{{item.control.name}}</td>
        <td>{{(item.control.fieldToRender || item.control.relationshipToRender || {}).name}}</td>
        <td>{{item.control.type.getAlias()}}</td>
        <td>{{item.control.renderingOrdinal}}</td>
        <td><button ng-click="$ctrl.toggleHorzResize(item)" class="btn-link">{{item.control.renderingHorizontalResizeMode.aliasOnly}}</button></td>
        <td><button ng-click="$ctrl.toggleVertResize(item)" class="btn-link">{{item.control.renderingVerticalResizeMode.aliasOnly}}</button></td>
        <td><button ng-click="$ctrl.delete(item)" class="btn-link">delete</button></td>
    </tr>
    </tbody>
</table>
`
        });

    function FormStructureEditorController($scope, $element, ControlWrapper) {
        'ngInject';

        const $ctrl = this; // match the implied var used in the template

        this.$onInit = () => {
        };

        this.delete = item => {
            if (item.parent) {
                new ControlWrapper(item.parent).remove(item.control);
                onFormEntityUpdated();
            }
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

        $scope.$watch('$ctrl.formEntity', onFormEntityUpdated);

        $scope.$watch('$ctrl.formEntity.graph.history._changeCounter', onFormEntityUpdated);

        function onFormEntityUpdated() {

            $ctrl.items = $ctrl.formEntity ? flattenControls(0, null, $ctrl.formEntity) : [];
            $ctrl.items = _.map($ctrl.items, item => {
                // do any data massaging here
                return item;
            });


            $scope.$root.$broadcast('doLayout');
        }

        function getNextResizeMode(mode) {
            const resizeModes = ['console:resizeSpring','console:resizeAutomatic',
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
}());
