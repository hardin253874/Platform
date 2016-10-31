// Copyright 2011-2016 Global Software Innovation Pty Ltd
/*global console, _, spEntity, sp */

/**
 * This is the stuff that used to be in spUtils that depends on spEntity.
 *
 *  @namespace spEntityUtils
 */

var spEntityUtils = spEntityUtils || {};

(function (spEntityUtils) {
    'use strict';

    // 
    // Create a map between the data type and the field type alias. Note that this is not a one to one mapping.
    var dataTypeToFieldTypeMap = {};

    dataTypeToFieldTypeMap[spEntity.DataType.String] = 'stringField';
    dataTypeToFieldTypeMap[spEntity.DataType.Int32] = 'intField';
    dataTypeToFieldTypeMap[spEntity.DataType.Decimal] = 'decimalField';
    dataTypeToFieldTypeMap[spEntity.DataType.Currency] = 'currencyField';
    dataTypeToFieldTypeMap[spEntity.DataType.Date] = 'dateField';
    dataTypeToFieldTypeMap[spEntity.DataType.Time] = 'timeField';
    dataTypeToFieldTypeMap[spEntity.DataType.DateTime] = 'dateTimeField';
    dataTypeToFieldTypeMap[spEntity.DataType.Bool] = 'boolField';
    dataTypeToFieldTypeMap[spEntity.DataType.Guid] = 'guidField';

    var fieldTypeToDataTypeMap = _.invert(dataTypeToFieldTypeMap);
    fieldTypeToDataTypeMap['aliasField'] = spEntity.DataType.String;                // add the one I can't get from the inversion

    // add the 'other' mappings
    fieldTypeToDataTypeMap.autoNumberField = spEntity.DataType.String;

    /**
     * Given a field (with the isOfType relationship populated) return the dataType string
     *
     * @param {Entity} field The field, with a populated sOfType relationship.
     * @returns {String} A dbType string
     */
    spEntityUtils.dataTypeForField = function (field) {
        return spEntityUtils.dataTypeForFieldTypeAlias(field.getIsOfType()[0].getAlias());
    };


    /**
     * Given a field type alias, return the dataType string
     *
     * @param {String} fieldTypeAlias The alias of the fieldType.
     * @returns {String} A dbType string
     */
    spEntityUtils.dataTypeForFieldTypeAlias = function (fieldTypeAlias) {
        if (!fieldTypeAlias)
            return fieldTypeAlias;
        var trimmedAlias = fieldTypeAlias.replace(/^core:/, '');
        var result = fieldTypeToDataTypeMap[trimmedAlias];

        if (!result) {
            throw new Error("fieldTypeAlias does not correspond to a valid dataType: " + fieldTypeAlias);
        }

        return result;
    };


    /**
     * Given a dateType, return the fieldType Alias, null if there is no match
     *
     * @param {String} dataType The alias of the fieldType.
     * @returns {String} A dbType string
     */
    spEntityUtils.fieldTypeAliasForDataType = function (dataType) {

        return dataTypeToFieldTypeMap[dataType];
    };

    /**
     * Flattens a graph of entities to an array of enties.
     *
     * @param {spEntity.Entity} entity The entity or array of entities to start with.
     * @param {Object} options The options to use when walking entities.
     *  - includeDeleted {bool} - True to include deleted instances, false otherwise
        - ignoreRelationships {function} - to check the relationship should be ignore or not during walk.
     * @returns {Array.<spEntity.Entity>} All entities.
     */
    spEntityUtils.walkEntities = function (entity, options) {
        var assertIsEntity = function (e) {
            if ((e && e._id && e._fields && e._relationships)) {
                console.assert(e && e._id && e._fields && e._relationships);
            }
            //if (!(e && e._id && e._fields && e._relationships)) debugger;
        };

        // This has been added to support entity cloning
        var includeDeleted = options && options.includeDeleted;
        var ignoreRelationships = options && options.ignoreRelationships;
        var mapRelationshipInstance = function (ri) {
            return ri.entity;
        };

        var allRelated = function (e) {
            var result = [];
            if (e && e._relationships) {
                for (var i = 0; i < e._relationships.length; i++) {

                    var ignoreRel = (ignoreRelationships && ignoreRelationships(e._relationships[i])) || false;
                    if (ignoreRel === true || ignoreRel === 'cloneref' || ignoreRel === 'ignore')
                        continue;

                    var relId = e._relationships[i].id;
                    var related = e.getRelationship({id: relId, isReverse: e._relationships[i].isReverse}); //e.getRelationship(relId);

                    if (includeDeleted) {
                        result = _.union(result, _.map(related.getInstances(), mapRelationshipInstance));
                    } else {
                        _.forEach(related, assertIsEntity);

                        result = _.union(result, related);
                    }
                }
            }
            return result;
        };
        var result = sp.walkGraph(allRelated, entity);
        return result;
    };

})(spEntityUtils);

