// Copyright 2011-2016 Global Software Innovation Pty Ltd
/* globals rnEditForm */

(function () {


    angular.module('app.editFormComponents')
        .component('rnContainerControl', {
            bindings: {
                control: '<', resource: '<', form: '<',
                options: '<', formOptions: '<'
            },
            template: `
<div class="rnContainerControl {{$ctrl.containerCssClass}}">
    <div ng-if="!$ctrl.hideContainerLabel" class="rnContainerControl__title">
        {{$ctrl.control.name}}
    </div>
    <div ng-switch="$ctrl.containerType" class="rnContainerControl__content">
        <div ng-switch-when="tabContainerControl">
            <uib-tabset>
                <uib-tab ng-repeat="c in $ctrl.controls" index="$index" heading="{{c.name}}">
                    <rn-form-control class="rnTabContainerControl__control rn-{{c.control.renderingVerticalResizeMode.aliasOnly}}"
                                     control="c.control" resource="$ctrl.resource" form="$ctrl.form"
                                     options="c.options" form-options="$ctrl.formOptions"></rn-form-control>
                </uib-tab>
            </uib-tabset>            
        </div>
        <div ng-switch-when="pageContainerControl" class="rnPageContainerControl">
            <ul class="rnPageContainerControl__tabs">
                <li ng-repeat="c in $ctrl.controls" ng-click="$ctrl.selectTab(c)">{{c.name}}</li>
            </ul>            
            <ul class="rnPageContainerControl__pages">
                <li ng-repeat="c in $ctrl.controls"
                    ng-if="c == $ctrl.selectedTab" class="rnPageContainerControl__page">
                    <rn-form-control class="rnTabContainerControl__control rn-{{c.control.renderingVerticalResizeMode.aliasOnly}}"
                                     control="c.control" resource="$ctrl.resource" form="$ctrl.form"
                                     options="c.options" form-options="$ctrl.formOptions"></rn-form-control>
                </li>
            </ul>    
        </div>
        <div ng-switch-when="notebookContainerControl" class="rnNotebookContainerControl">
            <ul class="rnNotebookContainerControl__tabs">
                <li ng-repeat="c in $ctrl.controls" ng-click="$ctrl.selectTab(c)"
                    ng-style="c.titleStyle">{{c.name}}</li>
            </ul>            
            <ul class="rnNotebookContainerControl__pages">
                <li ng-repeat="c in $ctrl.controls" 
                    ng-if="c == $ctrl.selectedTab" class="rnNotebookContainerControl__page">
                    <rn-form-control class="rnTabContainerControl__control rn-{{c.control.renderingVerticalResizeMode.aliasOnly}}"
                                     control="c.control" resource="$ctrl.resource" form="$ctrl.form"
                                     options="c.options" form-options="$ctrl.formOptions"></rn-form-control>
                </li>
            </ul>
        </div>
        <div ng-switch-default class="{{$ctrl.containerType}}">
            <rn-form-control
                    ng-repeat="c in $ctrl.controls"
                    class="rnContainerControl__control {{c.cssClass}}"
                    control="c.control" resource="$ctrl.resource" form="$ctrl.form"
                    options="c.options" form-options="$ctrl.formOptions"></rn-form-control>
        </div>
    </div>
</div>
`,
            controller: ContainerController
        });

    function ContainerController($scope, spEditForm, rnEditFormDndService) {
        'ngInject';

        const $ctrl = this;

        $ctrl.childControlOptions = {};

        $ctrl.dragOptions = {
            onDragStart(e, d) {
                // console.log('onDragStart event', e);
                // console.log('onDragStart data', d);
            },
        };

        $ctrl.selectTab = tab => {
            console.log('selected tab', tab.control.debugString);
            $ctrl.selectedControl = tab;
        };
        $ctrl.selectedControl = null;

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

            if ($ctrl.containerType === 'tabContainerControl') {
                $ctrl.containerType = 'pageContainerControl';
            }

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

                // mucking around with stuff here...

                if ($ctrl.containerType === 'notebookContainerControl') {
                    c.titleStyle = {'font-weight': 'bold'};
                }
                if ($ctrl.containerType === 'tabContainerControl') {
                    c.titleStyle = {'font-weight': 'bold'};
                    c.options.noTitle = true;
                }
            });

            $ctrl.selectedControl = $ctrl.selectedControl && _.find($ctrl.controls, {id: $ctrl.selectedControl.idP}) || _.first($ctrl.controls);

            console.log('ContainerController', $ctrl.containerType);

        }
    }

})();