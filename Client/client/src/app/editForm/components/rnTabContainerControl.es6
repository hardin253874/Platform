// Copyright 2011-2016 Global Software Innovation Pty Ltd
/*global _, angular, console, sp */

(function () {
    'use strict';

    angular.module('app.editFormComponents')
        .component('rnTabContainerControl', {
            bindings: {formControl: '=', formData: '=?'},
            template: `
<div class="rn-flex-column rn-flex-item" random-bgc>

    <ul class="nav nav-tabs">
        <li ng-repeat="tab in $ctrl.tabs"
            ng-class="{active: $ctrl.selectedTab.id === tab.id}"
            ng-click="$ctrl.selectTab(tab)">
            <a href="#" onclick="event.preventDefault()">{{tab.name}}</a>
        </li>
    </ul>

    <!--TODO put back in the ng-if on whether previously visited to delay load tabs-->
    <div ng-repeat="tab in $ctrl.tabs" ng-show="tab === $ctrl.selectedTab"
         class="rn-flex-item rn-flex-column">
        <rn-form-control form-control="tab.formControl" form-data="$ctrl.formData"></rn-form-control>
    </div>

</div>`,
            controller: TabContainerController
        });

    /* @ngInject */
    function TabContainerController($scope, $element, spEditForm) {

        //todo
        // - track selected container in app/nav state so if return to the form we can re-select
        // the tab .... or should the top level form do this?
        // - track visited tabs and only render (using ng-if) if we have visited
        // - may need a new unique id for 'tabs' to handle creating new tabs in design mode
        // - maybe a whole lotta stuff about design mode, dnd etc...

        this.selectTab = tab => {
            console.log('selected tab', tab.formControl.debugString);
            this.selectedTab = tab;
        };
        this.selectedTab = null;

        $scope.$watch('$ctrl.formControl', formControl => {
            // console.log('ContainerControlController $watch', $scope.$id, (formControl || {}).debugString);

            // prepare some data for the 'tabs'
            this.tabs = _((formControl || {}).containedControlsOnForm)
                .sortBy('renderingOrdinal')
                .map(control => {
                    return {
                        id: control.idP,
                        formControl: control,
                        name: spEditForm.getControlTitle(control),
                        tooltip: spEditForm.getControlDescription(control)
                    };
                })
                .value();

            this.selectedTab =
                (this.selectedTab && (_.some(this.tabs, {id: this.selectedTab.idP}))) ||
                _.first(this.tabs);
        });

        $element.addClass('rn-flex-row');
        $element.addClass('rn-flex-item');
    }

}());
