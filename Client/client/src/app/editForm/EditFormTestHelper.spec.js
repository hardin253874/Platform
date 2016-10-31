// Copyright 2011-2016 Global Software Innovation Pty Ltd
var EditFormTestHelper;

(function (module) {
    module.DummyControlEntity = spEntity.fromJSON(
                    {
                        alias: 'myControlEntity',
                        
                        fieldToRender: {
                            id: 123,
                            alias: 'myField',
                            isRequired: false,
                            isOfType: [{alias: 'stringField'}]
                        }
                    });
    
    module.DummyRelControlEntity = spEntity.fromJSON(
                {
                    alias: 'myControlEntity',
                    isReversed: false,

                    relationshipToRender: {
                        id: 123,
                        alias: 'myRel',
                        cardinality: jsonLookup('core:manyToOne'),
                        toType: {alias:'toType'}
                    }
                });
    
    module.DummyStructControlEntity = spEntity.fromJSON(
            {
                alias: 'myControlEntity',
                isReversed: false,

                containedControlsOnForm: [
                     {
                         alias: 'myControlEntity',
                        
                         fieldToRender: {
                             id: 123,
                             alias: 'myField',
                             isRequired: false,
                             isOfType: [{alias: 'stringField'}]
                         }
                     }]
                
            });

})(EditFormTestHelper || (EditFormTestHelper = {}));

