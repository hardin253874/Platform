// Copyright 2011-2016 Global Software Innovation Pty Ltd

// import template from './rnFormRenderControl.html';
// import './rnFormRenderControl.scss';

class FormRenderControl {
    constructor() {
        'ngInject';
    }
}

angular.module('app.editFormComponents')
    .component('rnFormRenderControl', {
        bindings: {control: '<', form: '<', formOptions: '<', options: '<'},
        templateUrl: 'editForm/components/screenElements/rnFormRenderControl.tpl.html',
        controller: FormRenderControl
    });