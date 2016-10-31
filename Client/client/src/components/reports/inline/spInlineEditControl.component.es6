// Copyright 2011-2016 Global Software Innovation Pty Ltd
/*global _, console, angular, sp */

(function () {
    'use strict';

    class InlineEditControlController {
        constructor($scope) {
            "ngInject";
            this.$scope = $scope;
            this.canEdit = false;
            this.formMode = 'edit';
            this.cellEditClass = '';
            this.cellViewStyle = {};

            // The onUpdatedValidationMessages exists if we choose to do something
            // at the report level.
            // And note - the following cannot be a class method as it will be called as a
            // plain function without a 'this'.
            this.onUpdatedValidationMessages = messages => {
                // if (this.spReport) {
                //     this.spReport.setValidationMessages(this.row, this.col, messages);
                // }
                if (this.editingState) {
                    this.editingState.validationMessages = this.editingState.validationMessages || [];
                    this.editingState.validationMessages[this.col] = messages;
                }
            };
        }

        $onInit() {
            // console.log('InlineEditControlController: $onInit, spReport=' + !!this.spReport);

            // console.log('styles=', this.cellStyle, this.cellText);

            if (!this.spReport) return; // ignore when used outside of a report
            
            this.editingState = this.spReport.getResourceEditingState(this.row, this.col);

            if (this.editingState) {

                this.$scope.$watch(
                    () => this.spReport.getSelectedRowIndex(),
                    rowIndex => this._setFormMode(null, rowIndex)
                );
                this.$scope.$watch(
                    () => this.spReport.getInlineEditingState(this.row),
                    state => this._setFormMode(state, null)
                );

                this.editingState.formPromise.then(() => this._onStateLoaded());
            }

            // The following is needed to prevent these events due to any report reloads that
            // may happen for report like form controls. If these are allowed to propagate
            // up ($emit is used) then it can muck up the linking when we are a inline form
            // on a report on a screen.
            this.$scope.$on('spReportEventReportRanAtleastOnce', event => {
                if (event.stopPropagation) {
                    event.stopPropagation();
                }
            });
        }

        $onChanges(changes) {
            if (changes.cellStyle) {
                const style = changes.cellStyle.currentValue;

                // make a style for viewing, adjusting as required for our context
                // and so it works on the div containing the value
                this.cellViewStyle = _.assign({}, style);
                this.cellViewStyle['line-height'] = 'inherit';

                // make a style for editing, adjusting as required knowing that
                // this is used only on the container of the form control and
                // whatever might be used there
                this.cellEditClass = style['text-align'] ? 'align-' + style['text-align'] : '';
            }
        }

        _setFormMode(rowState, selectedRowIndex) {
            rowState = rowState || this.spReport.getInlineEditingState(this.row);
            selectedRowIndex = selectedRowIndex || this.spReport.getSelectedRowIndex();            

            this.formMode = rowState === 'edit' ||
            rowState === 'error' && this.row === selectedRowIndex ?
                'edit' : 'view';

            if (this.formMode === 'edit') {
                this.spReport.focusInlineEditCellElement();
            }
        }

        _onStateLoaded() {
            this.controlEntity = this.spReport.getFormControl(this.editingState, this.col);
            this.canEdit = !!this.controlEntity;
                            
            // not finished on this yet...
            //this._formatControl();
        }

        _formatControl() {
            if (!this.controlEntity) return;

            // Form control don't seem to have formatting settings corresponding to the
            // formatting settings available in a report column. So here we are
            // looking for the main formats that 'look bad if different' and making
            // them happen whatever it takes, typically with CSS override for now.

            let {rcols, valrules} = this.spReport.getMetadata();
            if (!rcols || !valrules) return;

            // rcol values don't include the col id which is a pain, so add it
            rcols = _.map(rcols, (c, eid) => _.assign({}, c, {eid: sp.coerseToNumberOrLeaveAlone(eid)}));

            // find the valrule if any that corresponds to our col
            const rcol = _.find(rcols, c => c.ord === this.col);
            const valrule = rcol && valrules[rcol.eid];

            console.log('format', rcol, valrule);
        }
    }

    angular.module('mod.common.ui.spReport').component('spInlineEditControl', {
        bindings: {
            row: '<',
            col: '<',
            cellStyle: '<',
            cellText: '<'
        },
        template: `
          <div ng-if="!$ctrl.canEdit" class="rn-inline-edit-value" ng-style="$ctrl.cellViewStyle">
            {{$ctrl.cellText}}</div>
          <sp-inline-edit-form ng-if="$ctrl.canEdit" class="{{$ctrl.cellEditClass}}"
            form-control="$ctrl.controlEntity"
            form-data="$ctrl.editingState.resourceEntity"
            form-mode="$ctrl.formMode"
            on-updated-validation-messages="$ctrl.onUpdatedValidationMessages"></sp-inline-edit-form>
        `,
        controller: InlineEditControlController,
        require: {'spReport': '^^?spReport'}
    });

})();