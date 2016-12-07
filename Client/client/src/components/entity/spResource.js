// Copyright 2011-2016 Global Software Innovation Pty Ltd
/**
 * Module for manipulating Software Platform resource, relationships, types, etc.
 * @namespace spResource
 */
var spResource;

(function (spResource) {

    /**
     * Makes a request string for querying various type schema information.
     *
     * @param {object} options An object of options for what to include and exclude.
     *
     * @example
     * <pre>
     * var opts = {  // these are the defaults
     *      fields: true,
     *      relationships: true,
     *      fieldGroups: true,
     *      ignoreInheritance: false,
     *      ignoreOverrides: false
     *  };
     * var rqString = spResource.makeTypeRequest(opts);
     * </pre>
     *
     * @function
     * @name spResource.makeTypeRequest
     */
    spResource.makeTypeRequest = function makeRequest(options) {

        var opts = _.defaults(options || {}, {
            fields: true,
            relationships: true,
            fieldGroups: true,
            ignoreInheritance: false,
            ignoreOverrides: false,
            derivedTypes: false,
            resourceKeys: false,
            scriptInfo: false,
            additional: ''
        });

        var rq = '';
        var rqDerived = '';
        var fieldGroups = 'fieldGroups';
        var fieldName = 'name' + (opts.scriptInfo ? ', fieldScriptName, isCalculatedField' : '');
        var relNames = 'name, fromName, toName' + (opts.scriptInfo ? ', fromScriptName, toScriptName' : '');

        if (opts.fields) {
            rq += '{fields,fieldOverridesForType}.{alias, ' + fieldName + ', description, hideField, isOfType.{alias,name}, isRequired, isFieldReadOnly }';
            if (!opts.ignoreOverrides) {
                // go from the type to the override. Then from the override back to the type, as well as from the override to the original field, and back.
                rq += ',fieldOverridesForType.{fieldOverrideForType.id, fieldOverrides.fieldOverriddenBy.id}';
                fieldGroups += ',fieldOverridesForType.fieldInGroup';
            }

            fieldGroups += ',fields.fieldInGroup';
        }

        if (opts.relationships) {
            rq +=
                ',{ relationships, reverseRelationships }.{ ' +
                '    alias, ' + relNames + ', description, relationshipIsMandatory, revRelationshipIsMandatory, cardinality.alias, relType.alias, hideOnFromType, hideOnToType, defaultFromUseCurrent,defaultToUseCurrent, inSolution.{alias,name}, ' +
                '    {fromType, toType}.{name,alias,inherits.alias} ' +
                '}';
            
            fieldGroups += ',relationships.relationshipInToTypeGroup, relationships.relationshipInFromTypeGroup, reverseRelationships.relationshipInToTypeGroup, reverseRelationships.relationshipInFromTypeGroup';
        }

        if (opts.fieldGroups) {
            rq +=
                ',{' + fieldGroups + '}.{alias, name, description}';
        }
        
        if (opts.derivedTypes) {
            rqDerived = ', derivedTypes.{alias,name,description, inSolution.{alias,name}}';
        }
        
        if (opts.resourceKeys) {
            rq +=
               ', resourceKeys.{alias,name,mergeDuplicates}';
        }

        if (opts.additional) {
            rq += ', ' + opts.additional;
        }

        if (!rq)
            rq = 'name';    // whatever
        else if (rq[0] === ',')
            rq = rq.substr(1);

        var res = '';
        var base = 'alias, name, description';
        if (opts.ignoreInheritance) {
            res = '{' + base + ', {' + rq + rqDerived + '}}';
        } else {
            if (opts.derivedTypes) {
                // mustn't include derived types request as a peer to the recursive inherits request
                res = '{' + base + rqDerived + ',' + rq + ', inherits.{ inherits*, ' + rq + ' } }';
            } else {
                res = '{' + base + ', {inherits*,' + rq + '}}';
            }
        }
        return res;
    };


    /**
     * Given an entity type resource, returns an array of all entity types that it inherits, including the resource itself.
     * Requires that the 'core:inherits*' relationship is loaded on each entity type.
     *
     * @param {spEntity.Entity} typeEntity An entity that contains type data.
     * @returns {Array.<spEntity.Entity>} An array of type entities, including the current entity.
     *
     * @example
       <pre>var arr = spResource.getAncestorsAndSelf(personDefinition);</pre>    
     *
     * @function
     * @name spResource.getAncestorsAndSelf
     */
    spResource.getAncestorsAndSelf = function getAncestorsAndSelf(typeEntity) {
        if (!typeEntity)
            return [];
        var getRelated = function (e) {
            var inh = !(e.getInherits) ? [] : e.getInherits();
            return inh;
        };
        var res = spUtils.walkGraphSorted(getRelated, typeEntity);
        return res;
    };


    /**
     * Given an entity type resource, returns an array of all derived types that derive from the entity, including the resource itself.
     * Requires that the 'core:derivedTypes*' relationship is loaded on each entity type.
     *
     * @param {spEntity.Entity} typeEntity An entity that contains type data.
     * @returns {Array.<spEntity.Entity>} An array of type entities, including the current entity.
     *
     * @example
       <pre>var arr = spResource.getDerivedTypesAndSelf(personDefinition);</pre>    
     *
     * @function
     * @name spResource.getDerivedTypesAndSelf
     */
    spResource.getDerivedTypesAndSelf = function getDerivedTypesAndSelf(typeEntity) {
        if (!typeEntity)
            return [];
        var getRelated = function (e) {
            var der = !(e.getDerivedTypes) ? [] : e.getDerivedTypes();
            return der;
        };
        var res = spUtils.walkGraphSorted(getRelated, typeEntity);
        return res;
    };

})(spResource || (spResource = {}));



angular.module('mod.common.spResource', ['ng', 'mod.common.spEntityService']);
angular.module('mod.common.spResource').factory('spResource', function () {
    'use strict';

    /**
     *  A set of APIs for manipulating resource schema data.
     *  @module spResource
     */
    var exports = _.clone(spResource);
    return exports;

});
