// Copyright 2011-2016 Global Software Innovation Pty Ltd

// import template from './rnReportRenderControl.html';
// import './rnReportRenderControl.scss';

class ReportRenderControl {
    constructor() {
        'ngInject';
    }
}

angular.module('app.editFormComponents')
    .component('rnReportRenderControl', {
        bindings: {control: '<', form: '<', formOptions: '<', options: '<'},
        templateUrl: 'editForm/components/screenElements/rnReportRenderControl.tpl.html',
        controller: ReportRenderControl
    });