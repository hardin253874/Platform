// Copyright 2011-2016 Global Software Innovation Pty Ltd

// import template from './rnChartRenderControl.html';
// import './rnChartRenderControl.scss';

class ChartRenderControl {
    constructor() {
        'ngInject';
    }
}

angular.module('app.editFormComponents')
    .component('rnChartRenderControl', {
        bindings: {control: '<', form: '<', formOptions: '<', options: '<'},
        templateUrl: 'editForm/components/screenElements/rnChartRenderControl.tpl.html',
        controller: ChartRenderControl
    });