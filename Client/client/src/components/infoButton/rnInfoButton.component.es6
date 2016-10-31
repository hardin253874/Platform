// Copyright 2011-2016 Global Software Innovation Pty Ltd
/*global _, angular, console, sp */

(function () {
	'use strict';

	class InfoButtonController {
		constructor(spDialogService) {
			"ngInject";
			this.spDialogService = spDialogService;

		}
		
		openDetail() {

			if (this.disabled)
				return;

			var that = this;

			var modalInstanceCtrl = (scope, $uibModalInstance, fieldTitle, fieldValue) => {
				scope.model = {};
				scope.model.fieldTitle = fieldTitle;
				scope.model.fieldValue = fieldValue;

				scope.closeDetail = function () {
					$uibModalInstance.close(scope.model);
				};
			};

			var defaults = {
				template: `
					<div modal="isOpen" close="closeDetail()" options="modalOpts" class ="rn-info-button-popup">
							<div class="modal-header">
								<h6><img src="assets/images/help.svg">{{model.fieldTitle}}</h6>
								<button ng-click="closeDetail()"><img src="assets/images/icon_picker_close.png"></button>
							</div>

							<div class="modal-body">
								<textarea class="modal-text" readonly name="input">{{model.fieldValue}}</textarea>
							</div>

						</div>
					`,
				controller: ['$scope', '$uibModalInstance', 'fieldTitle', 'fieldValue', modalInstanceCtrl],
				resolve: {
					fieldTitle: function () {
						return that.heading ? that.heading : '';
					},
					fieldValue: function () {
						return that.text ? that.text : '';
					}
				}
			};

			this.spDialogService.showDialog(defaults);
		}
	}

	// Note - this angular component does not conform to our rules of
	// only one way bindings and callbacks for changes. Here though to explore 
	// some new techniques.

	angular.module('mod.common.ui.rnInfoButton')
        .component('rnInfoButton', {
        	bindings: {
				heading: '@',
				text: '@',
				disabled: '<',
        	},
        	controller: InfoButtonController,
        	template: `
				<img class="rn-info-button"
					src="assets/images/help.svg"
					onclick="event.preventDefault()"
					ng-click="$ctrl.openDetail()"
				/>
            `
        });
}());
