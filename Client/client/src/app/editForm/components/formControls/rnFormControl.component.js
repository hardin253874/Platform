// Copyright 2011-2016 Global Software Innovation Pty Ltd

(function () {

    angular.module('app.editFormComponents')
        .component('rnFormControl', {
            bindings: {
                control: '<', resource: '<', form: '<',
                options: '<', formOptions: '<'
            },
            template: `<div>should be replaced</div>`,
            controller: rnFormControlController
        });

    function rnFormControlController($scope, $element, $compile, $injector, rnEditFormDndService) {
        'ngInject';

        const $ctrl = this;
        let renderedTemplateHtml = '';

        console.assert($ctrl.control);

        this.$onInit = () => {

        };

        this.$onChanges = changes => {
            renderControl();
        };

        function renderControl() {
            const control = $ctrl.control;
            const options = _.defaults({}, $ctrl.options, $ctrl.formOptions);

            $ctrl.dragOptions = rnEditFormDndService.getDragOptions(control);

            const controlTypeAlias = control.getType().getAlias();
            const [elementName] = getElementNameForControl($injector, control, options);
            const template = `
<div ng-if="$ctrl.formOptions.designing" class="rnFormControl__overlay" 
     sp-draggable="$ctrl.dragOptions" sp-draggable-data="$ctrl.control"
     data-formControlId="{{$ctrl.control.idP}}"></div>
    
<${elementName} ng-if="!!$ctrl.control" class="rnFormControl rnFormControl-${controlTypeAlias}"  
    form="$ctrl.form" control="$ctrl.control" resource="$ctrl.resource"
    options="$ctrl.options" form-options="$ctrl.formOptions" 
    title="{{$ctrl.control.debugString}}"></${elementName}>
`;
            if (template !== renderedTemplateHtml) {
                $compile($element.html(template).contents())($scope);
                renderedTemplateHtml = template;
            }
        }
    }

    /**
     * Get the HTML element name and the directive name to use for the given form control
     * @param injector
     * @param {Entity} c - the control
     * @returns {Array}
     */
    function getElementNameForControl(injector, c, options) {
        const alias = getAlias(c, options) || '##ERROR-NO-TYPE-ALIAS##';
        const directiveName = alias.charAt(0).toUpperCase() + alias.slice(1);
        const baseElementName = alias.replace(/(.*?)([A-Z])/g, '$1-$2').toLowerCase();

        if (injector.has('rn' + directiveName + 'Directive')) {
            return ['rn-' + baseElementName, 'rn' + directiveName];
        }
        // if ($injector.has('sp' + directiveName + 'Directive')) {
        //     return ['sp-' + baseElementName, 'sp' + directiveName];
        // }
        return ['rn-default-field-control', 'rnDefaultFieldControl'];
    }

    function getAlias(c, {mobile}) {

        const aliasToComponent = {
            // 'verticalStackContainerControl': 'ContainerControl',
            // 'horizontalStackContainerControl': 'ContainerControl',
            // 'tabContainerControl': 'ContainerControl',
            'headerColumnContainerControl': 'verticalStackContainerControl'
        };
        const mobileAliasToComponent = {
            'horizontalStackContainerControl': 'expanderContainerControl'
        };

        let alias = sp.result(c, ['typesP', 0, 'getAlias']);
        if (aliasToComponent[alias]) {
            alias = aliasToComponent[alias];
        }
        if (mobile && mobileAliasToComponent[alias]) {
            alias = mobileAliasToComponent[alias];
        }
        return alias;
    }

})();
