// Copyright 2011-2016 Global Software Innovation Pty Ltd
(function () {
    'use strict';

    /**
    * Module relationship properties helper functions.       
    * 
    * @module spRelationshipPropertiesHelper    
    */
    angular.module('mod.app.configureDialog.relationshipProperties.spRelationshipPropertiesHelper', ['mod.common.alerts'])
        .factory('spRelationshipPropertiesHelper', function (spAlertsService) {
            var exports = {};
            
            exports.getLookupTemplateFn = function () {
                return function () {
                    return spEntity.fromJSON({
                        name: jsonString(''),
                        description: jsonString(''),
                        toName: jsonString(''),
                        fromName: jsonString(''),
                        toType: jsonLookup(),
                        fromType: jsonLookup(),
                        toScriptName: jsonString(''),
                        fromScriptName: jsonString(''),
                        toTypeDefaultValue: jsonLookup(),
                        fromTypeDefaultValue: jsonLookup(),
                        relationshipIsMandatory: false,
                        revRelationshipIsMandatory: false,
                        isRelationshipReadOnly: false,
                        cardinality: jsonLookup('core:manyToOne'),
                        relType: jsonLookup('core:relLookup'),
                        cascadeDelete: false,
                        cascadeDeleteTo: false,
                        cloneAction: { alias: 'cloneReferences' },
                        reverseCloneAction: { alias: 'drop' },
                        implicitInSolution: false,
                        reverseImplicitInSolution: false
                    });
                };
            };

            exports.getRelationshipTemplateFn = function () {
                return function () {
                    return spEntity.fromJSON({
                        name: jsonString(''),
                        description: jsonString(''),
                        toName: jsonString(''),
                        fromName: jsonString(''),
                        toType: jsonLookup(),
                        fromType: jsonLookup(),
                        toScriptName: jsonString(''),
                        fromScriptName: jsonString(''),
                        toTypeDefaultValue: jsonLookup(),
                        fromTypeDefaultValue: jsonLookup(),
                        relationshipIsMandatory: false,
                        revRelationshipIsMandatory: false,
                        isRelationshipReadOnly: false,
                        cardinality: jsonLookup('core:oneToMany'),
                        relType: jsonLookup('core:relExclusiveCollection'),
                        cascadeDelete: false,
                        cascadeDeleteTo: false,
                        cloneAction: { alias: 'drop' },
                        reverseCloneAction: { alias: 'cloneReferences' },
                        implicitInSolution: false,
                        reverseImplicitInSolution: false
                    });
                };
            };
            
            exports.getLookupControlTemplateFn = function() {
                return function () {
                    return spEntity.fromJSON({
                        //id: 'myRelForm',
                        //typeId: 'console:inlineRelationshipRenderControl',
                        //isOfType: [{
                        //    id: 'console:inlineRelationshipRenderControl',
                        //    alias: 'console:inlineRelationshipRenderControl'
                        //}]
                        //'console:mandatoryControl': false,
                        //'console:readOnlyControl': false,
                        //'console:isReversed': false,
                        canCreate: true,
                        canCreateDerivedTypes: true,
                        //'console:pickerReport': jsonLookup()
                    });
                };
            };

            // localize these strings
            exports.relDisplayAsComboItemInline = 'Inline';
            exports.relDisplayAsComboItemDropdown = 'Dropdown';
            exports.relDisplayAsComboItemReport = 'Report';
            exports.resourceViewerComboItemNo = 'No';
            exports.resourceViewerComboItemYes = 'Yes, only this type';
            exports.resourceViewerComboItemYesIncDerivedTypes = 'Yes, including Derived Types';


            exports.relDisplayAsRelationshipComboOptions = [
                { name: exports.relDisplayAsComboItemReport, value: exports.relDisplayAsComboItemReport },
                { name: exports.relDisplayAsComboItemInline, value: exports.relDisplayAsComboItemInline }
            ];

            exports.relDisplayAsLookupComboOptions = [
                { name: exports.relDisplayAsComboItemInline, value: exports.relDisplayAsComboItemInline },
                { name: exports.relDisplayAsComboItemDropdown, value: exports.relDisplayAsComboItemDropdown }
            ];

            exports.relCanCreateComboOptions = [
                { name: exports.resourceViewerComboItemYesIncDerivedTypes, value: exports.resourceViewerComboItemYesIncDerivedTypes },
                { name: exports.resourceViewerComboItemYes, value: exports.resourceViewerComboItemYes },
                { name: exports.resourceViewerComboItemNo, value: exports.resourceViewerComboItemNo }
            ];

            exports.getMissingActualRelType = function (relationship) {

                // If opening existing relationship that has no relType, then calculate an actual relType using cardinality + ownership=none
                // because we are using the actual cardinality of the relationship then we can pre-assume the relationship direction as fwd

                var actualRelType;

                if (relationship.cardinality.alias() === 'core:manyToOne') {
                    actualRelType = exports.getRelType('core:relLookup');
                }
                else if (relationship.cardinality.alias() === 'core:oneToMany') {
                    actualRelType = exports.getRelType('core:relExclusiveCollection');
                }
                else if (relationship.cardinality.alias() === 'core:oneToOne') {     // no ownership, so its same relType in both rel directions
                    actualRelType = exports.getRelType('core:relSingleLookup');
                }
                else if (relationship.cardinality.alias() === 'core:manyToMany') {   // no ownership, so its same relType in both rel directions
                    actualRelType = exports.getRelType('core:relManyToMany');
                }

                return actualRelType;
            };

            exports.getEffectiveRelTypeByActualRelType = function (actualRelType) {
                var effectiveRelType;
                var relType;
                var relTypeAlias = actualRelType.alias();
                if (relTypeAlias.indexOf(':') > 0) {
                    relType = relTypeAlias.substr(relTypeAlias.indexOf(':') + 1);
                }

                //relType = actualRelType.substr(actualRelType.indexOf(':') + 1);
                if (relType) {
                    switch (relType) {
                        case 'relLookup':
                            effectiveRelType = exports.getRelType('core:relExclusiveCollection');
                            break;
                        case 'relDependantOf':
                            effectiveRelType = exports.getRelType('core:relDependants');
                            break;
                        case 'relComponentOf':
                            effectiveRelType = exports.getRelType('core:relComponents');
                            break;
                        case 'relChoiceField':
                            raiseErrorAlert('getEffectiveRelTypeByActualRelType: choice fields not implemented yet');
                            effectiveRelType = ''; // todo: 
                            break;
                        case 'relSingleLookup':
                            effectiveRelType = exports.getRelType('core:relSingleLookup');
                            break;
                        case 'relSingleComponentOf':
                            effectiveRelType = exports.getRelType('core:relSingleComponent');
                            break;
                        case 'relSingleComponent':
                            effectiveRelType = exports.getRelType('core:relSingleComponentOf');
                            break;
                        case 'relExclusiveCollection':
                            effectiveRelType = exports.getRelType('core:relLookup');
                            break;
                        case 'relDependants':
                            effectiveRelType = exports.getRelType('core:relDependantOf');
                            break;
                        case 'relComponents':
                            effectiveRelType = exports.getRelType('core:relComponentOf');
                            break;
                        case 'relManyToMany':
                            effectiveRelType = exports.getRelType('core:relManyToMany');
                            break;
                        case 'relMultiChoiceField':
                            raiseErrorAlert('getEffectiveRelTypeByActualRelType: multi choice fields not implemented yet');
                            effectiveRelType = ''; // todo: 
                            break;
                        case 'relSharedDependantsOf':
                            effectiveRelType = exports.getRelType('core:relSharedDependants');
                            break;
                        case 'relSharedDependants':
                            effectiveRelType = exports.getRelType('core:relSharedDependantsOf');
                            break;
                        case 'relManyToManyFwd':
                            effectiveRelType = exports.getRelType('core:relManyToManyFwd');
                            break;
                        case 'relManyToManyRev':
                            effectiveRelType = exports.getRelType('core:relManyToManyRev');
                            break;
                        case 'relCustom':
                            effectiveRelType = exports.getRelType('core:relCustom');
                            break;
                        default:
                            console.error('getEffectiveRelTypeByActualRelType: invalid relType provided.');
                            effectiveRelType = '';
                            break;
                    }
                }

                return effectiveRelType;
            };

            exports.setRelationshipInternalValues = function (actualRelType, relationship) {
                // set other properties based on relType
                var relType;
                var relTypeAlias = actualRelType.alias();
                if (relTypeAlias.indexOf(':') > 0) {
                    relType = relTypeAlias.substr(relTypeAlias.indexOf(':') + 1);
                }
                if (relType) {
                    switch (relType) {
                        case 'relLookup':
                            //effectiveRelType = 'core:relExclusiveCollection';
                            relationship.setRelType('relLookup');
                            relationship.setCardinality('manyToOne');
                            //relationship.setRelationshipIsMandatory(true);      // optional
                            //relationship.setRevRelationshipIsMandatory(true);   // optional
                            relationship.setCascadeDelete(false);
                            relationship.setCascadeDeleteTo(false);
                            relationship.setCloneAction('cloneReferences');
                            relationship.setReverseCloneAction('drop');
                            relationship.setImplicitInSolution(false);          // ref only
                            relationship.setReverseImplicitInSolution(false);   // ref only

                            break;
                        case 'relDependantOf':
                            relationship.setRelType('relDependantOf');
                            relationship.setCardinality('manyToOne');
                            relationship.setRelationshipIsMandatory(true);
                            //relationship.setRevRelationshipIsMandatory(true);   // optional
                            relationship.setCascadeDelete(true);
                            relationship.setCascadeDeleteTo(false);
                            relationship.setCloneAction('cloneReferences');
                            relationship.setReverseCloneAction('drop');
                            relationship.setImplicitInSolution(false);          // ref only
                            relationship.setReverseImplicitInSolution(false);   // ref only

                            break;
                        case 'relComponentOf':
                            relationship.setRelType('relComponentOf');
                            relationship.setCardinality('manyToOne');
                            relationship.setRelationshipIsMandatory(true);
                            //relationship.setRevRelationshipIsMandatory(true);   // optional
                            relationship.setCascadeDelete(true);
                            relationship.setCascadeDeleteTo(false);
                            relationship.setCloneAction('cloneReferences');
                            relationship.setReverseCloneAction('cloneEntities');
                            relationship.setImplicitInSolution(false);          // ref only
                            relationship.setReverseImplicitInSolution(true);

                            break;
                        case 'relChoiceField':
                            relationship.setRelType('relChoiceField');
                            relationship.setCardinality('manyToOne');
                            //relationship.setRelationshipIsMandatory(true);    // optional
                            //relationship.setRevRelationshipIsMandatory(true);   // optional
                            relationship.setCascadeDelete(false);
                            relationship.setCascadeDeleteTo(false);
                            relationship.setCloneAction('cloneReferences');
                            relationship.setReverseCloneAction('drop');
                            relationship.setImplicitInSolution(false);          // ref only
                            relationship.setReverseImplicitInSolution(false);

                            break;
                        case 'relSingleLookup':
                            relationship.setRelType('relSingleLookup');
                            relationship.setCardinality('oneToOne');
                            relationship.setRelationshipIsMandatory(false);    // we don't mark relationships mandatory. Only the controls that display them can be marked mandatoy on a specific form
                            relationship.setRevRelationshipIsMandatory(false);   // optional
                            relationship.setCascadeDelete(false);
                            relationship.setCascadeDeleteTo(false);
                            relationship.setCloneAction('drop');
                            relationship.setReverseCloneAction('drop');
                            relationship.setImplicitInSolution(false);          // ref only
                            relationship.setReverseImplicitInSolution(false);   // ref only

                            break;
                        case 'relSingleComponentOf':
                            relationship.setRelType('relSingleComponentOf');
                            relationship.setCardinality('oneToOne');
                            relationship.setRelationshipIsMandatory(true);
                            relationship.setRevRelationshipIsMandatory(false);   // optional
                            relationship.setCascadeDelete(true);
                            relationship.setCascadeDeleteTo(false);
                            relationship.setCloneAction('drop');
                            relationship.setReverseCloneAction('cloneEntities');
                            relationship.setImplicitInSolution(false);          // ref only
                            relationship.setReverseImplicitInSolution(true);

                            break;
                        case 'relSingleComponent':
                            relationship.setRelType('relSingleComponent');
                            relationship.setCardinality('oneToOne');
                            //relationship.setRelationshipIsMandatory(true);        // optional
                            //relationship.setRevRelationshipIsMandatory(true);     // optional
                            relationship.setCascadeDelete(false);
                            relationship.setCascadeDeleteTo(true);
                            relationship.setCloneAction('cloneEntities');
                            relationship.setReverseCloneAction('drop');
                            relationship.setImplicitInSolution(true);
                            relationship.setReverseImplicitInSolution(false);       // ref only

                            break;
                        case 'relExclusiveCollection':
                            relationship.setRelType('relExclusiveCollection');
                            relationship.setCardinality('oneToMany');
                            //relationship.setRelationshipIsMandatory(true);        // optional
                            //relationship.setRevRelationshipIsMandatory(true);     // optional
                            relationship.setCascadeDelete(false);
                            relationship.setCascadeDeleteTo(false);
                            relationship.setCloneAction('drop');
                            relationship.setReverseCloneAction('cloneReferences');
                            relationship.setImplicitInSolution(false);              // ref only
                            relationship.setReverseImplicitInSolution(false);       // ref only

                            break;
                        case 'relDependants':
                            relationship.setRelType('relDependants');
                            relationship.setCardinality('oneToMany');
                            //relationship.setRelationshipIsMandatory(true);        // optional
                            relationship.setRevRelationshipIsMandatory(true);
                            relationship.setCascadeDelete(false);
                            relationship.setCascadeDeleteTo(true);
                            relationship.setCloneAction('drop');
                            relationship.setReverseCloneAction('cloneReferences');
                            relationship.setImplicitInSolution(false);              // ref only
                            relationship.setReverseImplicitInSolution(false);       // ref only

                            break;
                        case 'relComponents':
                            relationship.setRelType('relComponents');
                            relationship.setCardinality('oneToMany');
                            //relationship.setRelationshipIsMandatory(true);        // optional
                            relationship.setRevRelationshipIsMandatory(true);
                            relationship.setCascadeDelete(false);
                            relationship.setCascadeDeleteTo(true);
                            relationship.setCloneAction('cloneEntities');
                            relationship.setReverseCloneAction('cloneReferences');
                            relationship.setImplicitInSolution(true);
                            relationship.setReverseImplicitInSolution(false);       // ref only

                            break;
                        case 'relManyToMany':
                            relationship.setRelType('relManyToMany');
                            relationship.setCardinality('manyToMany');
                            //relationship.setRelationshipIsMandatory(true);        // optional
                            //relationship.setRevRelationshipIsMandatory(true);     // optional
                            relationship.setCascadeDelete(false);
                            relationship.setCascadeDeleteTo(false);
                            relationship.setCloneAction('cloneReferences');
                            relationship.setReverseCloneAction('cloneReferences');
                            relationship.setImplicitInSolution(false);              // ref only
                            relationship.setReverseImplicitInSolution(false);       // ref only

                            break;
                        case 'relManyToManyFwd':
                            relationship.setRelType('relManyToManyFwd');
                            relationship.setCardinality('manyToMany');
                            //relationship.setRelationshipIsMandatory(true);        // optional
                            //relationship.setRevRelationshipIsMandatory(true);     // optional
                            relationship.setCascadeDelete(false);
                            relationship.setCascadeDeleteTo(false);
                            relationship.setCloneAction('cloneReferences');
                            relationship.setReverseCloneAction('drop');
                            relationship.setImplicitInSolution(false);              // ref only
                            relationship.setReverseImplicitInSolution(false);       // ref only

                            break;
                        case 'relManyToManyRev':
                            relationship.setRelType('relManyToManyRev');
                            relationship.setCardinality('manyToMany');
                            //relationship.setRelationshipIsMandatory(true);        // optional
                            //relationship.setRevRelationshipIsMandatory(true);     // optional
                            relationship.setCascadeDelete(false);
                            relationship.setCascadeDeleteTo(false);
                            relationship.setCloneAction('drop');
                            relationship.setReverseCloneAction('cloneReferences');
                            relationship.setImplicitInSolution(false);              // ref only
                            relationship.setReverseImplicitInSolution(false);       // ref only

                            break;
                        case 'relMultiChoiceField':
                            relationship.setRelType('relMultiChoiceField');
                            relationship.setCardinality('manyToMany');
                            //relationship.setRelationshipIsMandatory(true);        // optional
                            //relationship.setRevRelationshipIsMandatory(true);     // optional
                            relationship.setCascadeDelete(false);
                            relationship.setCascadeDeleteTo(false);
                            relationship.setCloneAction('cloneReferences');
                            relationship.setReverseCloneAction('drop');
                            relationship.setImplicitInSolution(false);              // ref only
                            relationship.setReverseImplicitInSolution(false);

                            break;
                        case 'relSharedDependantsOf':
                            relationship.setRelType('relSharedDependantsOf');
                            relationship.setCardinality('manyToMany');
                            relationship.setRelationshipIsMandatory(true);
                            //relationship.setRevRelationshipIsMandatory(true);     // optional
                            relationship.setCascadeDelete(true);
                            relationship.setCascadeDeleteTo(false);
                            relationship.setCloneAction('cloneReferences');
                            relationship.setReverseCloneAction('cloneReferences');
                            relationship.setImplicitInSolution(false);              // ref only
                            relationship.setReverseImplicitInSolution(false);       // ref only
                            break;
                        case 'relSharedDependants':
                            relationship.setRelType('relSharedDependants');
                            relationship.setCardinality('manyToMany');
                            //relationship.setRelationshipIsMandatory(true);        // optional
                            relationship.setRevRelationshipIsMandatory(true);       // optional
                            relationship.setCascadeDelete(false);
                            relationship.setCascadeDeleteTo(true);
                            relationship.setCloneAction('cloneReferences');
                            relationship.setReverseCloneAction('cloneReferences');
                            relationship.setImplicitInSolution(false);              // ref only
                            relationship.setReverseImplicitInSolution(false);       // ref only
                            break;
                        case 'relCustom':
                            console.log('setRelationshipInternalValues: viewing a custom relationship - ' + relationship.name);
                            break;
                        default:
                            console.log('invalid relType provided');
                            break;
                    }
                }
            };

            exports.getRelType = function (alias) {
                var retVal = _.find(exports.relTypes, { 'nsAlias': alias });
                if (!retVal) {
                    console.error('getRelType: invalid relType alias provided: ' + alias);
                }
                return retVal;
            };

            exports.relTypes = spEntity.fromJSON(
                 [
                     {
                         id: 'core:relLookup'
                     },
                     {
                         id: 'core:relDependantOf'
                     },
                     {
                         id: 'core:relComponentOf'
                     },
                     {
                         id: 'core:relChoiceField'
                     },
                     {
                         id: 'core:relSingleLookup'
                     },
                     {
                         id: 'core:relSingleComponent'
                     },
                     {
                         id: 'core:relSingleComponentOf'
                     },
                     {
                         id: 'core:relDependants'
                     },
                     {
                         id: 'core:relExclusiveCollection'
                     },
                     {
                         id: 'core:relComponents'
                     },
                     {
                         id: 'core:relManyToMany'
                     },
                     {
                         id: 'core:relMultiChoiceField'
                     },
                     {
                         id: 'core:relSharedDependantsOf'
                     },
                     {
                         id: 'core:relSharedDependants'
                     },
                     {
                         id: 'core:relManyToManyFwd'
                     },
                     {
                         id: 'core:relManyToManyRev'
                     },
                     {
                         id: 'core:relCustom'
                     }
                 ]);

            exports.getSvgSettingsByCardinality = function (cardinality, leftText, rightText, showLineMarkersLeft, showLineMarkersRight) {
                var svgSettings;
                if (cardinality) {
                    switch (cardinality) {
                        case 'manyToOne':
                            svgSettings = [{
                                leftRects: {
                                    numOfRects: 1,
                                    text: leftText,
                                    rectStyle: 'normal',
                                    textStyle: 'normal',
                                    connectorStyle: 'normal',
                                    showLineMarkers: showLineMarkersLeft
                                },
                                rightRects: {
                                    numOfRects: 3,
                                    text: rightText,
                                    rectStyle: 'normal',
                                    textStyle: 'normal',
                                    hasOffsetDown: false,
                                    showLineMarkers: showLineMarkersRight
                                }
                            }];
                            break;
                        case 'oneToOne':
                            svgSettings = [{
                                leftRects: {
                                    numOfRects: 1,
                                    text: leftText,
                                    rectStyle: 'normal',
                                    textStyle: 'normal',
                                    connectorStyle: 'normal',
                                    showLineMarkers: showLineMarkersLeft
                                },
                                rightRects: {
                                    numOfRects: 1,
                                    text: rightText,
                                    rectStyle: 'normal',
                                    textStyle: 'normal',
                                    hasOffsetDown: false,
                                    showLineMarkers: showLineMarkersRight
                                }
                            }];
                            break;
                        case 'oneToMany':
                            svgSettings = [{
                                leftRects: {
                                    numOfRects: 1,
                                    text: leftText,
                                    rectStyle: 'normal',
                                    textStyle: 'normal',
                                    connectorStyle: 'normal',
                                    showLineMarkers: showLineMarkersLeft
                                },
                                rightRects: {
                                    numOfRects: 3,
                                    text: rightText,
                                    rectStyle: 'normal',
                                    textStyle: 'normal',
                                    hasOffsetDown: false,
                                    showLineMarkers: showLineMarkersRight
                                }
                            }];
                            break;
                        case 'manyToMany':
                            svgSettings = [{
                                leftRects: {
                                    numOfRects: 2,
                                    text: leftText,
                                    rectStyle: 'normal',
                                    textStyle: 'normal',
                                    connectorStyle: 'normal',
                                    showLineMarkers: showLineMarkersLeft
                                },
                                rightRects: {
                                    numOfRects: 3,
                                    text: rightText,
                                    rectStyle: 'normal',
                                    textStyle: 'normal',
                                    hasOffsetDown: false,
                                    showLineMarkers: showLineMarkersRight
                                }
                            }];
                            break;
                        default:
                            console.error('getSvgSettingsByCardinality: invalid cardinality provided');
                            break;
                    }
                }
                return svgSettings;
            };

            exports.getDeletedSvgSettingsByCardinalityAndOwnership = function (uiCardinality, uiOwnership, leftText, rightText) {
                var svgSettings;
                if (uiCardinality) {
                    switch (uiCardinality) {
                        case 'manyToOne':
                            if (uiOwnership === 'none') {
                                svgSettings = {
                                    svgs: [{
                                        leftRects: {
                                            numOfRects: 1,
                                            text: leftText,
                                            rectStyle: 'dotted',
                                            textStyle: 'normal',
                                            connectorStyle: 'dotted'
                                        },
                                        rightRects: {
                                            numOfRects: 3,
                                            text: rightText,
                                            rectStyle: 'normal',
                                            textStyle: 'normal',
                                            hasOffsetDown: false
                                        }
                                    }]
                                };
                            }
                            else if (uiOwnership === 'part' || uiOwnership === 'full') {
                                svgSettings = {
                                    svgs: [{
                                        leftRects: {
                                            numOfRects: 1,
                                            text: leftText,
                                            rectStyle: 'dotted',
                                            textStyle: 'normal',
                                            connectorStyle: 'dotted'
                                        },
                                        rightRects: {
                                            numOfRects: 3,
                                            text: rightText,
                                            rectStyle: 'dotted',
                                            textStyle: 'normal',
                                            hasOffsetDown: false
                                        }
                                    }]
                                };
                            }
                            break;
                        case 'oneToOne':
                            if (uiOwnership === 'none') {
                                svgSettings = {
                                    svgs: [{
                                        leftRects: {
                                            numOfRects: 1,
                                            text: leftText,
                                            rectStyle: 'normal',
                                            textStyle: 'normal',
                                            connectorStyle: 'dotted'
                                        },
                                        rightRects: {
                                            numOfRects: 1,
                                            text: rightText,
                                            rectStyle: 'normal',
                                            textStyle: 'normal',
                                            hasOffsetDown: false
                                        }
                                    }]
                                };
                            }
                            else if (uiOwnership === 'full') {      // uiOwnership === 'part' is not supported. 
                                svgSettings = {
                                    svgs: [{
                                        leftRects: {
                                            numOfRects: 1,
                                            text: leftText,
                                            rectStyle: 'normal',
                                            textStyle: 'normal',
                                            connectorStyle: 'dotted'
                                        },
                                        rightRects: {
                                            numOfRects: 1,
                                            text: rightText,
                                            rectStyle: 'dotted',
                                            textStyle: 'normal',
                                            hasOffsetDown: true
                                        }
                                    }]
                                };
                            }
                            break;
                        case 'oneToMany':
                            if (uiOwnership === 'none') {
                                svgSettings = {
                                    svgs: [{
                                        leftRects: {
                                            numOfRects: 1,
                                            text: rightText,            // in this case left and right text are switched
                                            rectStyle: 'dotted',
                                            textStyle: 'normal',
                                            connectorStyle: 'dotted'
                                        },
                                        rightRects: {
                                            numOfRects: 3,
                                            text: leftText,             // in this case left and right text are switched
                                            rectStyle: 'normal',
                                            textStyle: 'normal',
                                            hasOffsetDown: false
                                        }
                                    }]
                                };
                            }
                            else if (uiOwnership === 'part' || uiOwnership === 'full') {
                                svgSettings = {
                                    svgs: [{
                                        leftRects: {
                                            numOfRects: 1,
                                            text: rightText,            // in this case left and right text are switched
                                            rectStyle: 'dotted',
                                            textStyle: 'normal',
                                            connectorStyle: 'dotted'
                                        },
                                        rightRects: {
                                            numOfRects: 3,
                                            text: leftText,             // in this case left and right text are switched
                                            rectStyle: 'dotted',
                                            textStyle: 'normal',
                                            hasOffsetDown: false
                                        }
                                    }]
                                };
                            }
                            break;
                        case 'manyToMany':
                            // not supported in UI
                            break;
                        default:
                            console.error('getSvgSettingsByCardinality: invalid cardinality provided');
                            break;
                    }
                }
                return svgSettings;
            };

            exports.getDuplicatedSvgSettingsByCardinalityAndOwnership = function (uiCardinality, uiOwnership, leftText, rightText) {
                var svgSettings;
                if (uiCardinality) {
                    switch (uiCardinality) {
                        case 'manyToOne':
                            if (uiOwnership === 'none' || uiOwnership === 'part') {
                                svgSettings = {
                                    svgs: [{
                                        leftRects: {
                                            numOfRects: 1,
                                            text: leftText,
                                            rectStyle: 'light',
                                            textStyle: 'light',
                                            connectorStyle: 'light'
                                        },
                                        rightRects: {
                                            numOfRects: 3,
                                            text: rightText,
                                            rectStyle: 'light',
                                            textStyle: 'light',
                                            hasOffsetDown: false
                                        }
                                    },
                                        {
                                            leftRects: {
                                                numOfRects: 1,
                                                text: leftText,
                                                rectStyle: 'strong',
                                                textStyle: 'normal',
                                                connectorStyle: 'normal'
                                            },
                                            rightRects: {
                                                numOfRects: 0,
                                                text: rightText,
                                                rectStyle: 'strong',
                                                textStyle: 'normal',
                                                hasOffsetDown: false
                                            }
                                        }
                                    ]
                                };
                            }
                            else if (uiOwnership === 'full') {
                                svgSettings = {
                                    svgs: [{
                                        leftRects: {
                                            numOfRects: 1,
                                            text: leftText,
                                            rectStyle: 'light',
                                            textStyle: 'light',
                                            connectorStyle: 'light'
                                        },
                                        rightRects: {
                                            numOfRects: 3,
                                            text: rightText,
                                            rectStyle: 'light',
                                            textStyle: 'light',
                                            hasOffsetDown: false
                                        }
                                    },
                                        {
                                            leftRects: {
                                                numOfRects: 1,
                                                text: leftText,
                                                rectStyle: 'strong',
                                                textStyle: 'normal',
                                                connectorStyle: 'strong'
                                            },
                                            rightRects: {
                                                numOfRects: 3,
                                                text: rightText,
                                                rectStyle: 'strong',
                                                textStyle: 'normal',
                                                hasOffsetDown: false
                                            }
                                        }
                                    ]
                                };
                            }
                            break;
                        case 'oneToOne':
                            if (uiOwnership === 'none') {
                                svgSettings = {
                                    svgs: [{
                                        leftRects: {
                                            numOfRects: 1,
                                            text: leftText,
                                            rectStyle: 'light',
                                            textStyle: 'light',
                                            connectorStyle: 'dotted'
                                        },
                                        rightRects: {
                                            numOfRects: 1,
                                            text: rightText,
                                            rectStyle: 'light',
                                            textStyle: 'light',
                                            hasOffsetDown: false
                                        }
                                    }]
                                };
                            }
                            else if (uiOwnership === 'full') {      // uiOwnership === 'part' is not supported. 
                                svgSettings = {
                                    svgs: [{
                                        leftRects: {
                                            numOfRects: 1,
                                            text: leftText,
                                            rectStyle: 'light',
                                            textStyle: 'light',
                                            connectorStyle: 'light'
                                        },
                                        rightRects: {
                                            numOfRects: 1,
                                            text: rightText,
                                            rectStyle: 'light',
                                            textStyle: 'light',
                                            hasOffsetDown: true
                                        }
                                    },
                                        {
                                            leftRects: {
                                                numOfRects: 1,
                                                text: leftText,
                                                rectStyle: 'strong',
                                                textStyle: 'normal',
                                                connectorStyle: 'strong'
                                            },
                                            rightRects: {
                                                numOfRects: 1,
                                                text: rightText,
                                                rectStyle: 'strong',
                                                textStyle: 'normal',
                                                hasOffsetDown: true
                                            }
                                        }
                                    ]
                                };
                            }
                            break;
                        case 'oneToMany':
                            if (uiOwnership === 'none' || uiOwnership === 'part') {
                                svgSettings = {
                                    svgs: [{
                                        leftRects: {
                                            numOfRects: 1,
                                            text: rightText,            // in this case left and right text are switched
                                            rectStyle: 'light',
                                            textStyle: 'light',
                                            connectorStyle: 'light'
                                        },
                                        rightRects: {
                                            numOfRects: 3,
                                            text: leftText,             // in this case left and right text are switched
                                            rectStyle: 'light',
                                            textStyle: 'light',
                                            hasOffsetDown: false
                                        }
                                    },
                                        {
                                            leftRects: {
                                                numOfRects: 1,
                                                text: rightText,            // in this case left and right text are switched
                                                rectStyle: 'strong',
                                                textStyle: 'normal',
                                                connectorStyle: 'strong'
                                            },
                                            rightRects: {
                                                numOfRects: 0,
                                                text: leftText,             // in this case left and right text are switched
                                                rectStyle: 'strong',
                                                textStyle: 'normal',
                                                hasOffsetDown: false
                                            }
                                        }
                                    ]
                                };
                            }
                            else if (uiOwnership === 'full') {
                                svgSettings = {
                                    svgs: [{
                                        leftRects: {
                                            numOfRects: 1,
                                            text: rightText,            // in this case left and right text are switched
                                            rectStyle: 'light',
                                            textStyle: 'light',
                                            connectorStyle: 'light'
                                        },
                                        rightRects: {
                                            numOfRects: 3,
                                            text: leftText,             // in this case left and right text are switched
                                            rectStyle: 'light',
                                            textStyle: 'light',
                                            hasOffsetDown: false
                                        }
                                    },
                                        {
                                            leftRects: {
                                                numOfRects: 1,
                                                text: rightText,            // in this case left and right text are switched
                                                rectStyle: 'strong',
                                                textStyle: 'normal',
                                                connectorStyle: 'strong'
                                            },
                                            rightRects: {
                                                numOfRects: 3,
                                                text: leftText,             // in this case left and right text are switched
                                                rectStyle: 'strong',
                                                textStyle: 'normal',
                                                hasOffsetDown: false
                                            }
                                        }
                                    ]
                                };
                            }
                            break;
                        case 'manyToMany':
                            // not supported in UI
                            break;
                        default:
                            console.error('getSvgSettingsByCardinality: invalid cardinality provided');
                            break;
                    }
                }
                return svgSettings;
            };
            return exports;

            function raiseErrorAlert(message) {
                spAlertsService.addAlert(message, {severity: spAlertsService.sev.Error});
            }

        });
}());