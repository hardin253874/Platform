// Copyright 2011-2016 Global Software Innovation Pty Ltd
describe('Console|configDialogs|relationshipProperties|spec:|spRelationshipFiltersControl', function() {
	'use strict';
	// Load the modules    
	beforeEach(module('mod.app.configureDialog.relationshipProperties.spRelationshipFilters'));
	beforeEach(module('app-templates'));
	beforeEach(module('component-templates'));

	function setupTestFormControl() {
		// Setup the form control
		var formControl = spEntity.fromJSON({
			id: 11111
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

	it('should replace HTML element with appropriate content', inject(function($rootScope, $compile) {
		var scope = $rootScope,
			element;
		// Setup the form control
		scope.formControl = setupTestFormControl();
		element = angular.element('<sp-relationship-filters-control form-control="formControl"></sp-relationship-filters-control>');
		$compile(element)(scope);
		scope.$digest();
		// Verify that the html element has been replaced        
		expect(element.hasClass('spRelationshipFiltersControl')).toBe(true);
		expect(scope.$$childHead.model.displayString).toBe('FilterSource');
	}));

	it('should clear existing filters', inject(function($rootScope, $compile) {
		var scope = $rootScope,
			element;
		/// Setup the form control
		scope.formControl = setupTestFormControl();
		element = angular.element('<sp-relationship-filters-control form-control="formControl"></sp-relationship-filters-control>');
		$compile(element)(scope);
		scope.$digest();
		// Verify that the html element has been replaced        
		expect(element.hasClass('spRelationshipFiltersControl')).toBe(true);
		expect(scope.$$childHead.model.displayString).toBe('FilterSource');
		scope.$$childHead.clear();
		$compile(element)(scope);
		expect(scope.formControl.getRelationship('console:relationshipControlFilters').length).toBe(0);
	}));
});