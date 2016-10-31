// Copyright 2011-2016 Global Software Innovation Pty Ltd
(function () {
    'use strict';

    angular.module('mod.ui.spTreeviewManager', ['mod.common.spEntityService'])
    .service('spTreeviewManager', function (spEntityService) {
     
                /**
                * Module implementing the treeview manager service.                        
                * @module spTreeviewManager                    
                */
                var exports = {};


                exports.newNode = function (nodeId, name, queryEntity, parentNodeId, parentQueryEntity, parentAggregateEntity, entitytypeId, relationshipId, relationshipType, fromEntityTypeId, toEntityTypeId, cols, followInReverse) {
                    return {
                        "nid": nodeId,
                        "name": name,
                        "qe": queryEntity,
                        "pnid": parentNodeId,
                        "pe": parentQueryEntity,
                        "pae": parentAggregateEntity,
                        "etid": entitytypeId,
                        "relid": relationshipId,
                        "reltype": relationshipType,
                        "ftid": fromEntityTypeId,
                        "ttid": toEntityTypeId,
                        "children": [],                        
                        "cols": cols,
                        "followInReverse": followInReverse
                    };
                };

                exports.newField = function (fieldId, fieldName, fieldType, isRequired, groupName, strutureViewId, relationshipId, entityId, fromTypeId, toTypeId, aggregateMethod, inReport, inAnalyzer, displayName, fieldOrder, isReverse) {
                    return {
                        "id": spUtils.newGuid(),
                        "fid": fieldId,
                        "fname": fieldName,
                        "ftype": fieldType,
                        "isreq": isRequired,
                        "group": groupName,
                        "svid":  strutureViewId,
                        "relid": relationshipId,
                        "etid": entityId,
                        "ftid": fromTypeId,
                        "ttid": toTypeId,
                        "aggm": aggregateMethod,
                        "inrep": inReport,
                        "inanal": inAnalyzer,
                        "dname": displayName,
                        "fieldOrder": fieldOrder,
                        "isReverse": isReverse
                        
                    };
                };

                
                /**
                * Add Node to Treeview
                */
                exports.addNode = function (newNode, parentNodeId, treeNode) {
                    if (treeNode === undefined || treeNode === null) {
                        return null;
                    }

                    if (treeNode.nid == parentNodeId) {
                        //treeNode.qe.rentity.push(newNode.qe);
                        treeNode.children.push(newNode);

                    } else {
                        for (var i = 0; i < treeNode.children.length; i++) {
                            treeNode.children[i] = exports.addNode(newNode, parentNodeId, treeNode.children[i]);
                        }

                    }
                    return treeNode;
                };

                exports.getNode = function (nodeId, treeNode)
                {
                    if (treeNode === undefined || treeNode === null) {
                        return null;
                    }

                    if (treeNode.nid == nodeId) {
                        return treeNode;
                    } else {
                        for (var i = 0; i < treeNode.children.length; i++) {
                            var childNode = exports.getNode(nodeId, treeNode.children[i]);
                            if (childNode ) {
                                return childNode;
                            }
                        }
                    }
                    return null;
                };

                exports.getNodeByRelationshipId = function (relationshipId, treeNode) {

                    if (treeNode === undefined || treeNode === null) {
                        return null;
                    }

                    var node = null;

                    if (treeNode.relid === relationshipId) {
                        node = treeNode;
                        return node;
                    } else {
                        if (treeNode.children && treeNode.children.length > 0) {
                            _.each(treeNode.children, function (childNode) {
                                var currentChildNode = exports.getNodeByRelationshipId(relationshipId, childNode);
                                if (currentChildNode) {
                                    node = currentChildNode;
                                }
                            });
                            return node;

                        } else {
                            return null;
                        }
                    }

                };

                exports.getNodeByRelationship = function(selectNode, relationship) {
                    var existRelationships = _.filter(selectNode.children, function (node) {
                        return (
                                   node.relid === relationship.rid
                               );
                    });


                    if (existRelationships && existRelationships.length > 0) {
                        return existRelationships[existRelationships.length - 1];
                    }

                    return null;
                };

                exports.removeNode = function (node, treeNode) {
                    if (node === undefined || node === null) {
                        return treeNode;
                    }

                    if (treeNode.nid != node.nid) {
                        var removeIndex = -1;
                        for (var i = 0; i < treeNode.children.length; i++) {
                            var childNode = exports.removeNode(node, treeNode.children[i]);
                            if (childNode === null) {
                                removeIndex = i;
                                break;
                            }
                        }
                        if (removeIndex > -1) {
                            treeNode.children.splice(removeIndex, 1);
                        }

                        return treeNode;
                    } else {
                        return null;
                    }
                };


                exports.updateNode = function (nodeid, fieldId, updateInRep, updateInAnal, treeNode) {

                    if (treeNode.pnid !== undefined && treeNode.pnid !== null && treeNode.pnid === nodeid) {
                        if (treeNode.cols !== undefined && treeNode.cols !== null && treeNode.cols.length > 0) {
                            for (var i = 0; i < treeNode.cols.length; i++) {
                                if (treeNode.cols[i].fid === fieldId && treeNode.cols[i].relid === 0 && treeNode.cols[i].svid === 0) {
                                    if (updateInRep !== null) {
                                        treeNode.cols[i].inrep = updateInRep;
                                    }
                                    if (updateInAnal !== null) {
                                        treeNode.cols[i].inanal = updateInAnal;
                                    }
                                }
                            }
                        }

                    } else {
                        if (treeNode.children !== undefined && treeNode.children !== null && treeNode.children.length > 0) {
                            for (var j = 0; j < treeNode.children.length; j++) {
                                treeNode.children[j] = exports.updateNode(nodeid, fieldId, updateInRep, updateInAnal, treeNode.children[j]);
                            }
                        }
                    }

                    return treeNode;
                };

                exports.updateTreeNodeEntity = function (nodeId, queryEntity, treeNode) {                   
                    if (treeNode.nid === nodeId) {

                        treeNode.qe = queryEntity;
                        return treeNode;
                    } else {
                        for (var i = 0; i < treeNode.children.length; i++) {
                            treeNode.children[i] = exports.updateTreeNodeEntity(nodeId, queryEntity, treeNode.children[i]);
                        }
                    }
                    return treeNode;

                };


                exports.updateTreeNodeColumns = function (entityId, fields, treeNode, selectedNodeId) {

                    if (treeNode.nid === selectedNodeId) {
                        for (var i = 0; i < treeNode.children.length; i++) {
                            if (treeNode.children[i].etid === entityId) {
                                treeNode.children[i].cols = fields;
                                break;
                            }
                        }
                    } else {
                        for (var j = 0; j < treeNode.children.length; j++) {
                            treeNode.children[j] = exports.updateTreeNodeColumns(entityId, fields, treeNode.children[j], selectedNodeId);
                        }
                    }
                    return treeNode;
                };


                exports.updateTreeNodeColumn = function (treeNode, nodeId, fieldId, relId, inrep, inanal) {
                    if (treeNode.nid === nodeId) {
                        for (var i = 0; i < treeNode.cols.length; i++) {
                            if (treeNode.cols[i].fid === fieldId && treeNode.cols[i].relid === relId) {
                                if (inrep !== null) {
                                    treeNode.cols[i].inrep = inrep;
                                }
                                if (inanal !== null) {
                                    treeNode.cols[i].inanal = inanal;
                                }
                                break;
                            }
                        }
                    } else {
                        for (var j = 0; j < treeNode.children.length; j++) {
                            treeNode.children[j] = exports.updateTreeNodeColumn(treeNode.children[j], nodeId, fieldId, relId, inrep, inanal);
                        }
                    }
                    return treeNode;
                };

                exports.getNodeColumns = function (entityId, nameFieldId, promise) {

                    spEntityService.getEntity(entityId, spResource.makeTypeRequest()).then
                    (
                          function (typeEntity) {
                              exports.getFieldTypeResourcePromise(new spResource.Type(typeEntity), nameFieldId, promise);
                          }
                    );
                };

                //Get ToType Resource Promise to build fields collection of current type
                exports.getFieldTypeResourcePromise = function (resource, nameFieldId, promise) {
                    if (!resource) {
                        return;
                    }

                    var fields = [];
                    var fieldOrder = 0;
                    resource.getFields().forEach(
                        function (field) {
                            if (!field.isHidden()) {
                                fieldOrder++;
                                var colJson = exports.getFieldJson(field, resource, fieldOrder);
                                fields.push(colJson);
                            }
                        }
                    );

                    resource.getLookups().forEach(
                         function (relationship) {
                             if (!relationship.isHidden()) {
                                 fieldOrder++;
                                 var colJson = exports.getLookupJson(relationship, resource, fieldOrder, nameFieldId);
                                 fields.push(colJson);
                             }
                         }
                    );

                    resource.getChoiceFields().forEach(
                         function (relationship) {
                             if (!relationship.isHidden()) {
                                 fieldOrder++;
                                 var colJson = exports.getLookupJson(relationship, resource, fieldOrder, nameFieldId);
                                 fields.push(colJson);
                             }
                         }
                    );

                    promise(fields);
                };

                exports.getFieldJson = function (field, resource, fieldOrder) {
                    var fid, fname, ftype, group, displayName, isReadOnly;
                    var typeId = 0;
                    fid = field.getEntity().eid().id();
                    fname = field.getName();
                    isReadOnly = field.getEntity().isFieldReadOnly;

                    //fix the sort order of default fields
                    if (fname === "Name") {
                        displayName = resource.getName();
                        typeId = resource.getEntity().id();
                        fieldOrder = 1;
                        isReadOnly = true;
                    } else if (fname === "Description") {
                        fieldOrder = 2;
                        isReadOnly = true;
                    } else if (fname === "Created Date") {
                        fieldOrder = 3;
                    } else if (fname === "Modified Date") {
                        fieldOrder = 4;
                    }

                    ftype = exports.getDatabaseTypeDisplayName(field.getTypes()[0]);

                    group = field.getFieldGroupEntity() ? field.getFieldGroupEntity().getName() : 'Unallocated';

                    var colJson = exports.newField(fid, fname, ftype, field.isRequired(), group, 0, 0, resource.getEntity().idP, 0, typeId, '', false, false, displayName, fieldOrder);
                    return colJson;
                };

                exports.getLookupJson = function (relationship, resource, fieldOrder, nameFieldId) {
                    var fromType = relationship.getEntity().getFromType();
                    var toType = relationship.getEntity().getToType();
                    var fromTypeId = fromType ? fromType.eid().id() : 0;
                    var toTypeId = toType ? toType.eid().id() : 0;
                    var group = relationship.getFieldGroupEntity() ? relationship.getFieldGroupEntity().getName() : 'Unallocated'; //Relationship Fields
                    var name = relationship.getName() ? relationship.getName() : toType.name;
                    var isReverse = relationship.isReverse();
                    
                    var fieldType = relationship.isChoiceField() ? 'ChoiceRelationship' : 'InlineRelationship';
                    if (toType && toType.nsAlias === 'core:photoFileType') {
                        fieldType = 'Image';
                    }
                    var colJson = exports.newField(nameFieldId, name, fieldType, false, group, 0, relationship.getEntity().idP, resource.getEntity().idP, fromTypeId, toTypeId, '', false, false, null, fieldOrder, isReverse);
                    return colJson;
                };

                //get field type's database displayname
                exports.getDatabaseTypeDisplayName = function (fieldType) {

                    switch (fieldType.alias()) {
                        case 'core:stringField':
                            return "String";
                        case 'core:intField':
                            return "Int32";
                        case 'core:dateField':
                            return "Date";
                        case 'core:timeField':
                            return "Time";
                        case 'core:dateTimeField':
                            return "DateTime";
                        case 'core:decimalField':
                            return "Decimal";
                        case 'core:currencyField':
                            return "Currency";
                        case 'core:boolField':
                            return "Bool";
                        case 'core:autoNumberField':
                            return "AutoNumber";
                        default:
                            // return empty string
                            return "";
                    }
                };

                return exports;
           
    });
}());
    