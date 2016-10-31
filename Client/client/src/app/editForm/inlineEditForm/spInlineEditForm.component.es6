// Copyright 2011-2016 Global Software Innovation Pty Ltd
/*global _, angular, console, sp */

(function () {
    'use strict';

    class InlineEditFormController {
        constructor() {
            "ngInject";

            // bindables required by spControlOnForm
            this.isReadOnly = false;
            this.isInlineEditing = true;

            this.validationMessages = [];
        }

        $onChanges(changes) {

            if (changes.formControl) this.formControl = changes.formControl.currentValue;
            if (changes.formData) this.formData = changes.formData.currentValue;
            if (changes.formMode) this.formMode = changes.formMode.currentValue;

            if (!this.formControl || !this.formData) return;

            if (this.formControl.fieldToRender) {
                this.value = this.formData.getField(this.formControl.fieldToRender.idP);
            }
            if (this.formControl.relationshipToRender) {
                this.value = this.formControl.relationshipToRender.name;
            }

            this.validationCssClass = sp.result(this.formControl, 'getType.getAlias');
            this.isReadOnly = this.formMode === 'view';
        }

        addValidationMessage(control, message) {
            if (control === this.formControl) {
                this.validationMessages.push(message);
                this._onUpdatedValidationMessages();
            }
        }

        clearValidationMessages(control) {
            if (control === this.formControl) {
                this.validationMessages = [];
                this._onUpdatedValidationMessages();
            }
        }

        setValidationMessages(control, messages) {
            if (control === this.formControl) {
                this.validationMessages = (messages || []).slice(0); // copy array
                this._onUpdatedValidationMessages();
            }
        }

        _onUpdatedValidationMessages() {
            if (this.onUpdatedValidationMessages) {
                this.onUpdatedValidationMessages()(this.validationMessages);
            }
        }
    }

    // Note - this angular component does not conform to our rules of
    // only one way bindings and callbacks for changes. Here though to explore 
    // some new techniques.

    angular.module('app.inlineEditForm')
        .component('spInlineEditForm', {
            bindings: {
                formControl: '<',
                formData: '<',
                formMode: '<',
                onUpdatedValidationMessages: '&?'
            },
            controller: InlineEditFormController,
            template: `
                <sp-control-on-form 
                    form-Control="$ctrl.formControl" 
                    form-data="$ctrl.formData"
                    form-mode="$ctrl.formMode"
                    is-read-only="$ctrl.isReadOnly"
                    is-inline-editing="$ctrl.isInlineEditing"></sp-control-on-form>
                <sp-custom-validation-message messages="$ctrl.validationMessages"
                    class="{{$ctrl.validationCssClass}}"></sp-custom-validation-message>
            `
        });
}());
