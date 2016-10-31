// Copyright 2011-2016 Global Software Innovation Pty Ltd
/* globals spEntity, spResource, jsonString, jsonInt, jsonLookup, jsonRelationship, jsonDecimal, jsonBool,
 jsonCurrency, jsonDate, jsonDateTime, jsonTime */

describe('Entity Model|spResource|spec:|Relationship', function () {

	var manyToOne = spEntity.fromId('core:manyToOne');
	var manyToMany = spEntity.fromId('core:manyToMany');
	var oneToMany = spEntity.fromId('core:oneToMany');
	var oneToOne = spEntity.fromId('core:oneToOne');

	beforeEach(module('ng'));
	beforeEach(module('mod.common.spResource'));

	beforeEach(function () {
		this.addMatchers(TestSupport.matchers);
	});

	describe('getName', function () {

	    var rels = function (index) {
			return new spResource.Type(spEntity.fromJSON({
				relationships: [
                    { name: '0full name', toName: '0forward name', cardinality: manyToOne },
                    { name: '1full name', cardinality: manyToOne }
				],
				reverseRelationships: [
                    { name: '2full name', fromName: '2rev name', cardinality: manyToOne },
                    { name: '3full name', cardinality: manyToOne }
				],
			})).getAllRelationships()[index];
		};

		it('returns toName in forward direction', function () {
			expect(rels(0).getName()).toBe('0forward name');
		});

		it('falls back to full name in forward direction', function () {
			expect(rels(1).getName()).toBe('1full name');
		});

		it('returns toName in reverse direction', function () {
			expect(rels(2).getName()).toBe('2rev name');
		});

		it('falls back to full name in reverse direction', function () {
			expect(rels(3).getName()).toBe('3full name');
		});

	});

	describe('getScriptName', function () {

	    var rels = function (index) {
	        return new spResource.Type(spEntity.fromJSON({
	            relationships: [
                    { toScriptName:'0forward script', toName: '0forward name', cardinality: manyToOne },
                    { toName: '1forward name', cardinality: manyToOne },
                    { name: '2full name', cardinality: manyToOne }
	            ],
	            reverseRelationships: [
                    { fromScriptName:'3rev script', name: '3full name', fromName: '3rev name', cardinality: manyToOne },
                    { name: '4full name', fromName: '4rev name', cardinality: manyToOne },
                    { name: '5full name', cardinality: manyToOne }
	            ],
	        })).getAllRelationships()[index];
	    };

	    it('returns toScriptName in forward direction', function () {
	        expect(rels(0).getScriptName()).toBe('0forward script');
	    });

	    it('falls back to toName in forward direction', function () {
	        expect(rels(1).getScriptName()).toBe('1forward name');
	    });

	    it('finally falls back to name in forward direction', function () {
	        expect(rels(2).getScriptName()).toBe('2full name');
	    });

	    it('returns fromScriptName in reverse direction', function () {
	        expect(rels(3).getScriptName()).toBe('3rev script');
	    });

	    it('falls back to fromName in reverse direction', function () {
	        expect(rels(4).getScriptName()).toBe('4rev name');
	    });

	    it('finally falls back to name in reverse direction', function () {
	        expect(rels(5).getScriptName()).toBe('5full name');
	    });

	});

	describe('getEntity', function () {
		it('returns the relationship entity', function () {
			var relEntity = spEntity.fromJSON({ name: 'name', cardinality: manyToOne });
			var typeEntity = spEntity.fromJSON({ relationships: [relEntity] });
			var type = new spResource.Type(typeEntity);
		    var rel = type.getAllRelationships()[0];
		    expect(rel.getEntity()).toBe(relEntity);
		});
	});

	describe('isReverse', function () {

		var rels = function (index) {
			return new spResource.Type(spEntity.fromJSON({
				relationships: [
                    { name: '0', cardinality: manyToOne }
				],
				reverseRelationships: [
                    { name: '1', cardinality: manyToOne }
				],
			})).getAllRelationships()[index];
		};

		it('returns false in forward direction', function () {
			expect(rels(0).isReverse()).toBe(false);
		});

		it('returns true in reverse direction', function () {
			expect(rels(1).isReverse()).toBe(true);
		});
	

		describe('isToOne', function () {

			var rels = function (index) {
				return new spResource.Type(spEntity.fromJSON({
					relationships: [
						{ name: 'f0', cardinality: manyToOne },
						{ name: 'f1', cardinality: manyToMany },
						{ name: 'f2', cardinality: oneToMany },
						{ name: 'f3', cardinality: oneToOne }
					],
					reverseRelationships: [
						{ name: 'r0', cardinality: manyToOne },
						{ name: 'r1', cardinality: manyToMany },
						{ name: 'r2', cardinality: oneToMany },
						{ name: 'r3', cardinality: oneToOne }
					],
				})).getAllRelationships()[index];
			};

			it('returns true for forward manyToOne', function () {
				expect(rels(0).isToOne()).toBe(true);
			});

			it('returns false for forward manyToMany', function () {
				expect(rels(1).isToOne()).toBe(false);
			});

			it('returns false for forward oneToMany', function () {
				expect(rels(2).isToOne()).toBe(false);
			});

			it('returns true for forward oneToOne', function () {
				expect(rels(3).isToOne()).toBe(true);
			});

			it('returns false for reverse manyToOne', function () {
				expect(rels(4).isToOne()).toBe(false);
			});

			it('returns false for reverse manyToMany', function () {
				expect(rels(5).isToOne()).toBe(false);
			});

			it('returns true for reverse oneToMany', function () {
				expect(rels(6).isToOne()).toBe(true);
			});

			it('returns true for reverse oneToOne', function () {
				expect(rels(7).isToOne()).toBe(true);
			});
		});
	});

	describe('isToMany', function () {

		var rels = function (index) {
			return new spResource.Type(spEntity.fromJSON({
				relationships: [
                    { name: 'f0', cardinality: manyToOne },
                    { name: 'f1', cardinality: manyToMany },
                    { name: 'f2', cardinality: oneToMany },
                    { name: 'f3', cardinality: oneToOne }
				],
				reverseRelationships: [
                    { name: 'r0', cardinality: manyToOne },
                    { name: 'r1', cardinality: manyToMany },
                    { name: 'r2', cardinality: oneToMany },
                    { name: 'r3', cardinality: oneToOne }
				],
			})).getAllRelationships()[index];
		};

		it('returns false for forward manyToOne', function () {
			expect(rels(0).isToMany()).toBe(false);
		});

		it('returns true for forward manyToMany', function () {
			expect(rels(1).isToMany()).toBe(true);
		});

		it('returns true for forward oneToMany', function () {
			expect(rels(2).isToMany()).toBe(true);
		});

		it('returns false for forward oneToOne', function () {
			expect(rels(3).isToMany()).toBe(false);
		});

		it('returns true for reverse manyToOne', function () {
			expect(rels(4).isToMany()).toBe(true);
		});

		it('returns true for reverse manyToMany', function () {
			expect(rels(5).isToMany()).toBe(true);
		});

		it('returns false for reverse oneToMany', function () {
			expect(rels(6).isToMany()).toBe(false);
		});

		it('returns false for reverse oneToOne', function () {
			expect(rels(7).isToMany()).toBe(false);
		});
	});

	describe('isLookup', function () {

		var rels = function (index) {
			return new spResource.Type(spEntity.fromJSON({
				relationships: [
                    { name: 'f0', cardinality: manyToOne },
                    { name: 'f1', cardinality: manyToMany },
                    { name: 'f2', cardinality: oneToMany },
                    { name: 'f3', cardinality: oneToOne },
					{ name: 'f4', cardinality: oneToOne, toType: { inherits: [{ id: 'enumValue' }] } }
				],
				reverseRelationships: [
                    { name: 'r0', cardinality: manyToOne },
                    { name: 'r1', cardinality: manyToMany },
                    { name: 'r2', cardinality: oneToMany },
                    { name: 'r3', cardinality: oneToOne },
					{ name: 'r4', cardinality: oneToOne, toType: { inherits: [{ id: 'enumValue' }] } }
				],
			})).getAllRelationships()[index];
		};

		it('returns true for forward manyToOne', function () {
			expect(rels(0).isLookup()).toBe(true);
		});

		it('returns false for forward manyToMany', function () {
			expect(rels(1).isLookup()).toBe(false);
		});

		it('returns false for forward oneToMany', function () {
			expect(rels(2).isLookup()).toBe(false);
		});

		it('returns true for forward oneToOne', function () {
			expect(rels(3).isLookup()).toBe(true);
		});

		it('returns false for choice fields in the forward direction', function () {
			expect(rels(4).isLookup()).toBe(false);
		});

		it('returns false for reverse manyToOne', function () {
			expect(rels(5).isLookup()).toBe(false);
		});

		it('returns false for reverse manyToMany', function () {
			expect(rels(6).isLookup()).toBe(false);
		});

		it('returns true for reverse oneToMany', function () {
			expect(rels(7).isLookup()).toBe(true);
		});

		it('returns true for reverse oneToOne', function () {
			expect(rels(8).isLookup()).toBe(true);
		});

		it('returns true even for choice fields in the reverse direction', function () {
			expect(rels(9).isLookup()).toBe(true);
		});
	});

	describe('isChoiceField', function () {

		var rels = function (index) {
			return new spResource.Type(spEntity.fromJSON({
				relationships: [
                    { name: 'f0', cardinality: oneToOne, toType: { inherits: [{ id: 'whatever' }] } },
					{ name: 'f1', cardinality: oneToOne, toType: { inherits: [{ id: 'enumValue' }] } }
				],
				reverseRelationships: [
                    { name: 'r0', cardinality: oneToOne },
					{ name: 'r1', cardinality: oneToOne, toType: { inherits: [{ id: 'enumValue' }] } }
				],
			})).getAllRelationships()[index];
		};

		it('returns false if toType is not an enumValue in the forward direction', function () {
			expect(rels(0).isChoiceField()).toBe(false);
		});

		it('returns true if toType is an enumValue in the forward direction', function () {
			expect(rels(1).isChoiceField()).toBe(true);
		});

		it('handles the absense of metadata', function () {
			expect(rels(2).isChoiceField()).toBe(false);
		});

		it('returns false in the reverse direction', function () {
			expect(rels(3).isChoiceField()).toBe(false);
		});
	});

	describe('isHidden', function () {

		var rels = function (index) {
			return new spResource.Type(spEntity.fromJSON({
				relationships: [
                    { name: 'f0', cardinality: oneToOne, hideOnToType: false, hideOnFromType: true },
					{ name: 'f1', cardinality: oneToOne, hideOnToType: true, hideOnFromType: false }
				],
				reverseRelationships: [
                    { name: 'r0', cardinality: oneToOne, hideOnToType: true, hideOnFromType: false },
					{ name: 'r1', cardinality: oneToOne, hideOnToType: false, hideOnFromType: true }
				],
			})).getAllRelationships({ showHidden: true })[index];   // note: showHidden:true
		};

		it('checks hiddenOnFromType for forward relationships', function () {
			expect(rels(0).isHidden()).toBe(true);
			expect(rels(1).isHidden()).toBe(false);
		});

		it('checks hiddenOnToType for reverse relationships', function () {
			expect(rels(2).isHidden()).toBe(true);
			expect(rels(3).isHidden()).toBe(false);
		});
	});

	describe('cardinality helpers', function () {

	    it('toOne works', function () {
	        expect(spResource.Relationship.toOne('core:oneToOne', false)).toBe(true, '1');
	        expect(spResource.Relationship.toOne('core:manyToOne', false)).toBe(true, '2');
	        expect(spResource.Relationship.toOne('core:oneToMany', false)).toBe(false, '3');
	        expect(spResource.Relationship.toOne('core:manyToMany', false)).toBe(false, '4');
	        expect(spResource.Relationship.toOne('core:oneToOne', true)).toBe(true, '5');
	        expect(spResource.Relationship.toOne('core:manyToOne', true)).toBe(false, '6');
	        expect(spResource.Relationship.toOne('core:oneToMany', true)).toBe(true, '7');
	        expect(spResource.Relationship.toOne('core:manyToMany', true)).toBe(false, '8');
	    });

	    it('fromOne works', function () {
	        expect(spResource.Relationship.fromOne('core:oneToOne', false)).toBe(true);
	        expect(spResource.Relationship.fromOne('core:manyToOne', false)).toBe(false);
	        expect(spResource.Relationship.fromOne('core:oneToMany', false)).toBe(true);
	        expect(spResource.Relationship.fromOne('core:manyToMany', false)).toBe(false);
	        expect(spResource.Relationship.fromOne('core:oneToOne', true)).toBe(true);
	        expect(spResource.Relationship.fromOne('core:manyToOne', true)).toBe(true);
	        expect(spResource.Relationship.fromOne('core:oneToMany', true)).toBe(false);
	        expect(spResource.Relationship.fromOne('core:manyToMany', true)).toBe(false);
	    });
	});

});
