// Copyright 2011-2016 Global Software Innovation Pty Ltd
/* globals rnEditForm */

(function () {

    angular.module('app.editFormComponents')
        .component('rnTabRelationshipRenderControl', {
            bindings: {
                control: '<', resource: '<', form: '<',
                options: '<', formOptions: '<'
            },
            template: `
<div class="rnTabRelationshipRenderControl">
    <sp-tab-relationship-render-control
        form-control="$ctrl.control" form-data="$ctrl.resource" is-read-only="$ctrl.isReadOnly">
    </sp-tab-relationship-render-control>
</div>
`,
            controller: TabRelationshipRenderControlController
        });

    function TabRelationshipRenderControlController(spEditForm, $state) {
        'ngInject';

        const $ctrl = this; // to match symbol used in template

        this.$onInit = () => {

        };

        this.$onChanges = changes => {

            $ctrl.isReadOnly = true;

        };
    }
}());