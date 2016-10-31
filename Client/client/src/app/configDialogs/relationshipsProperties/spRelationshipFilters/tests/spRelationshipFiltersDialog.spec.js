// Copyright 2011-2016 Global Software Innovation Pty Ltd
describe('Console|configDialogs|relationshipProperties|spec:|spRelationshipFiltersDialog', function() {
	'use strict';
	// Load the modules    
	beforeEach(module('mod.app.configureDialog.relationshipProperties.spRelationshipFiltersDialog'));
	beforeEach(module('app-templates'));
	beforeEach(module('component-templates'));
	beforeEach(module('mockedEntityService'));

	afterEach(inject(function($document) {
		var body = $document.find('body');
		body.find('div.modal').remove();
		body.find('div.modal-backdrop').remove();
		body.removeClass('modal-open');
	}));

	// Set the mocked data
	beforeEach(inject(function(spEntityService) {
		// Set the data we wish the mock to return
		spEntityService.mockGetEntity(spEntity.fromJSON({
			id: {
				id: 11113
			}
		}));
	}));

	function setupTestFormControl() {
		// Setup the form control
		var formControl = spEntity.fromJSON({
			id: 11111,
			'console:relationshipToRender': jsonLookup({
				id: 11112,
				toType: jsonLookup({
					id: 11113
				})
			})
		});
		var filters = [spEntity.fromJSON({
			id: 55555,
			'console:relationshipControlFilterOrdinal': 0,
			'console:relationshipFilter': jsonLookup({
				id: 44444
			}),
			'console:relationshipControlFilter': jsonLookup({
				id: 33333,
				'console:isReverse': false,
				'console:relationshipToRender': jsonLookup({
					id: 22222,
					toName: 'FilterSource'
				})
			}),
			'console:relationshipDirectionFilter': jsonLookup({
				alias: 'forward'
			})
		})];
		formControl.setRelationship('console:relationshipControlFilters', filters);
		return formControl;
	}

	it('create dialog, show and cancel', inject(function($rootScope, spRelationshipFiltersDialog) {
		var scope = $rootScope,
			formControl = setupTestFormControl(),
			dialogOptions = {
				formControl: formControl,
				form: spEntity.fromJSON({
					id: 91111,
					'console:containedControlsOnForm': [formControl, spEntity.fromJSON({
						id: 91112
					})]
				}),
			},
			done;

		// Setup dialog options columns           
		spRelationshipFiltersDialog.showModalDialog(dialogOptions).then(function(result) {
			expect(result).toBe(false);
		});

		scope.$digest();

		// Allow dialog to display
		runs(function() {
			done = false;
			scope.$digest();

			setTimeout(function() {

				expect(scope.$$childHead.$$childHead.model.form).toBe(dialogOptions.form);
				expect(scope.$$childHead.$$childHead.model.formControl).toBe(dialogOptions.formControl);

				scope.$$childHead.$$childHead.cancel();

				scope.$digest();

				done = true;
			}, 100);
		});

		waitsFor(function() {
			return done;
		}, "Create and cancel dialog done", 1000);
	}));
});