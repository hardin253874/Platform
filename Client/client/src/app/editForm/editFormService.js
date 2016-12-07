// Copyright 2011-2016 Global Software Innovation Pty Ltd
/*global console, _, angular, sp, spEntity, spEntityUtils, spResource, Globalize */

/**
 * @ngdoc module
 * @name editForm.service:spEditForm
 *
 *  @description This service is used by any Page that used the custom edit forms.
 */

(function () {
    'use strict';

    angular.module('mod.app.editFormServices', [
        'ng',
        'sp.common.loginService',
        'mod.common.spEntityService',
        'sp.common.fieldValidator',
        'mod.app.editFormWebServices',
        'mod.app.editFormCache',
        'mod.common.spTenantSettings',
        'spApps.reportServices',
        'mod.ui.spReportFilters',
        'spApps.enumValueService'
    ]);

    /**
     * @ngdoc service
     * @name mod_app.editFormServices:editFormServices
     *
     *  @description This service is used by any Page that used the custom edit forms.
     */

    angular.module('mod.app.editFormServices')
        .factory('spEditForm', spEditForm);

    /* @ngInject */
    function spEditForm($q, spLoginService, spFieldValidator, spEntityService, editFormWebServices, editFormCache,
                        spTenantSettings, spReportService, $filter, spEnumValueService) {

        // The registered filter source controls, keyed off the form id.
        var filterSourceControlsPerForm = {};
        var reportDataAsEntities = $filter('reportDataAsEntities');
        var exports = {};

        /**
         ** The modes a form can be in.
         */
        exports.formModes = {edit: 'edit', view: 'view', design: 'design'};

        /**
         ** The relationship direction a form can be in.
         */
        exports.relationshpDirection = {forward: 'forward', reverse: 'reverse'};

        /**
         ** The logged-in account entity
         */
        exports.activeAccountEntity = undefined;

         /**
         * @ngdoc method
         * @name getFormDataAdvanced
         * @description Given an entity and formId gets the entity data inclduing visibility calculations
         *
         * @methodOf mod_app.editFormServices:editFormServices
         */
        exports.getFormDataAdvanced = function (id, requestStrings, formId) {
            var formRequestQuery = _.uniq(requestStrings).join(',');

            return editFormWebServices.getFormDataAdvanced(id, formRequestQuery, formId);
        };

        /**
         * @ngdoc method
         * @name getFormData
         * @description Given a formId, get the form.
         *
         * @methodOf mod_app.editFormServices:editFormServices
         */
        exports.getFormData = function (id, requestStrings) {            

            var logTimeKey = 'getFormData(' + id + ')';

            console.time(logTimeKey);

            var formRequestQuery = _.uniq(requestStrings).join(',');
            //console.log('formRequestQuery:	', formRequestQuery);

            return spEntityService.getEntity(id, formRequestQuery, {hint: 'getFormData', batch: false}).then(
                function (formData) {
                    //console.log('EditForm formData:', formData);
                    console.timeEnd(logTimeKey);
                    return formData;
                },
                function (error) {
                    console.error('editFormService.getFormData error:', error);
                    console.timeEnd(logTimeKey);
                    throw error;
                });


        };

        /**
         * Save the form data
         */
        exports.saveFormData = function (formData) {
            // mark access control virtual fields as pristine so that they are not sent to server
            exports.markAccessControlFieldsPristine(formData);

            return spEntityService.putEntity(formData).then(
                function (newId) {
                    formData.setId(newId);
                    formData.markAllUnchanged();
                },
                function (error) {
                    // expect error to have data and status members
                    var message = error.data.ExceptionMessage || error.data.Message || error.status;
                    throw new Error('The server returned an error: ' + message);
                });
        };


        /**
         * @ngdoc method
         * @name getFormDefinition
         * @description Given a formId, get the form.
         *
         * @methodOf mod_app.editFormServices:editFormServices
         */
        exports.getFormDefinition = function (selectedFormIdOrAlias, isInDesignMode, skipModificationCheck) {
            if (isInDesignMode) {
                return editFormWebServices.getFormDefinition(selectedFormIdOrAlias, isInDesignMode);
            } else {
                return editFormCache.getFormDefinition(selectedFormIdOrAlias, skipModificationCheck);
            }
        };


        /**
         * @ngdoc method
         * @name getFormForInstance
         * @description Given an entityId, get the form
         *
         * @methodOf mod_app.editFormServices:editFormServices
         */
        exports.getFormForInstance = function (selectedFormIdOrAlias, forceGenerate, skipModificationCheck) {
            if (forceGenerate) {
                return editFormWebServices.getFormForInstance(selectedFormIdOrAlias, forceGenerate);
            } else {
                return editFormCache.getFormForInstance(selectedFormIdOrAlias, forceGenerate, skipModificationCheck);
            }
        };


        /**
         * @ngdoc method
         * @name getFormForDefinition
         * @description Given an typeId, get the form
         *
         * @methodOf mod_app.editFormServices:editFormServices
         */
        exports.getFormForDefinition = function (selectedDefIdOrAlias, forceGenerate) {
            if (forceGenerate) {
                return editFormWebServices.getFormForDefinition(selectedDefIdOrAlias, forceGenerate);
            } else {
                return editFormCache.getFormForDefinition(selectedDefIdOrAlias);
            }
        };

        exports.getFormVisCalcDependencies = function(formId) {
            return editFormCache.getFormVisCalcDependencies(formId);
        };

        exports.clearAllFormCaches = function () {
            editFormCache.removeAll();
        };

        exports.clearServerFormsCache = function () {
            editFormCache.clearServerFormsCache();
        };


        // get an ID that can be used for a request
        exports.getIdForRequest = function (fieldOrRel) {
            //return fieldOrRel.alias() || '#' + fieldOrRel.id();
            return '#' + fieldOrRel.id();
        };

        // get an ID that can be used for a request
        exports.getRelIdForRequest = function (rel, isReverse) {
            return isReverse ? ('-#' + rel.id()) : ('#' + rel.id());
        };

        exports.updateRequestStringsForField = function (requestStrings, field) {
            var alias = exports.getIdForRequest(field);
            if (alias) {
                if (requestStrings && !_.includes(requestStrings, alias)) {
                    requestStrings.push(alias);
                }
            } else {
                console.error("Missing alias for field");
            }
        };

        /* note, use the reverse alias or forward alias accordingly */
        exports.updateRequestStringsForRelationship = function (requestStrings, relAlias, isMetaDataOnlyRequest) {
            if (relAlias) {
                if (isMetaDataOnlyRequest) {
                    requestStrings.push(relAlias + '.?');  // get rel metadata only
                }
                else {
                    requestStrings.push(relAlias + '.{alias, name, description}');
                }
            } else {
                console.error("Missing alias for relationship");
            }
        };

        //
        // build request strings
        //
        exports.buildRequestStrings = function (requestStrings, formControl) {
            requestStrings = requestStrings || [];

            if (!formControl) {
                return requestStrings;
            }

            // add Access Control Fields to request strings
            exports.addAccessControlFieldsToRequest(requestStrings);

            var flat = exports.getFormControls(formControl);
            _.map(flat, function (ctrl) {
                var field, relationship;

                // field
                field = ctrl.getLookup('console:fieldToRender');
                if (field) {
                    exports.updateRequestStringsForField(requestStrings, field);
                }

                // relationship
                relationship = ctrl.getLookup('console:relationshipToRender');
                if (relationship) {
                    var isReversed = (ctrl.hasOwnProperty('isReversed') && ctrl.getIsReversed()) ? ctrl.getIsReversed() : false;
                    var relAlias = exports.getRelIdForRequest(relationship, isReversed);
                    var isMetaDataOnlyRequest = exports.getIsMetaDataOnlyRequest(ctrl);

                    exports.updateRequestStringsForRelationship(requestStrings, relAlias, isMetaDataOnlyRequest);
                }

                // map control
                if (!field && !relationship && ctrl.firstTypeId().getAlias() === 'mapControl') {
                    var mapControlFields = ['shared:addressLine1', 'shared:addressLine2', 'shared:addressLine3', 'shared:city', 'shared:region', 'shared:country', 'shared:code'];
                    mapControlFields.forEach(function (mapField) {
                        requestStrings.push(mapField);
                    });
                }

                // fileNameUploadControl
                if (field && ctrl.firstTypeId().getAlias() === 'fileNameUploadControl') {
                    var fileNameUploadControlFields = ['core:name', 'core:fileDataHash', 'core:inFolder.id'];
                    fileNameUploadControlFields.forEach(function (field) {
                        requestStrings.push(field);
                    });
                }
            });

            return requestStrings;
        };

        //
        // add Access Control Fields to request strings
        //
        exports.addAccessControlFieldsToRequest = function (requestStrings) {
            if (requestStrings) {
                var canModifyFieldAlias = 'canModify';
                var canDeleteFieldAlias = 'canDelete';

                if (!_.includes(requestStrings, canModifyFieldAlias)) {
                    requestStrings.push(canModifyFieldAlias);
                }

                if (!_.includes(requestStrings, canDeleteFieldAlias)) {
                    requestStrings.push(canDeleteFieldAlias);
                }
            } else {
                console.error('null exception: requestStrings');
            }
        };

        //
        // mark access control fields as pristine in fromData
        //
        exports.markAccessControlFieldsPristine = function (formData) {
            if (formData) {
                var canModifyFieldContainer = formData.getFieldContainer('core:canModify');
                if (canModifyFieldContainer) {
                    canModifyFieldContainer.markAsPristine();
                }

                var canDeleteFieldContainer = formData.getFieldContainer('core:canDelete');
                if (canDeleteFieldContainer) {
                    canDeleteFieldContainer.markAsPristine();
                }
            }
        };


        //
        // checks if only metadata needs to be requested form server based on a render control type
        //
        exports.getIsMetaDataOnlyRequest = function (control) {
            if (control && control.firstTypeId) {
                return control.firstTypeId().getAlias() === 'tabRelationshipRenderControl';
            }
            return false;
        };


        // give a value on a field, format it for display as a string.
        // prefix is optional
        exports.formatNumberForDisplay = function (field, value, prefix) {
            var decimalPlaces, format;

            prefix = prefix || '';

            if (value === null || value === '') {
                return '';
            } else {

                decimalPlaces = field.getField('decimalPlaces');

                switch (field.firstTypeId().alias()) {
                    case 'core:decimalField':
                        if (decimalPlaces) {
                            format = 'n' + decimalPlaces;
                        } else {
                            format = 'n3';
                        }
                        break;
                    case 'core:currencyField':
                        if (decimalPlaces) {
                            format = 'n' + decimalPlaces;
                        } else {
                            format = 'n2';
                        }
                        break;
                    case 'core:intField':
                        format = 'n0';
                        break;
                    default:
                        console.error('formatNumberForDisplay: attempted to display a field that was not a number as a number.', field);
                        return 'NAN';
                }

                return prefix + Globalize.format(parseFloat(value), format); // #Localize
            }
        };


        /**
         * Given a form and the entity data, validate for preSave. This requires the control has spValidateControl set for it.
         *
         * @param {object} form - The top level form control.
         * @param {object} entity - The entity being edited.
         * @returns {bool} Are all the fields valid.
         */
        exports.validateForm = function (form, entity) {
            var flat = exports.getFormControls(form);
            return exports.validateFormControls(flat, entity);
        };

        exports.getFormControls = function (form) {
            return sp.walkGraph(
                function (e) {
                    var b = e.getRelationship('console:containedControlsOnForm');
                    return b || [];
                },
                form);
        };

        /**
         * Given a form and the entity data, validate for preSave. This requires the control has spValidateControl set for it.
         *
         * @param {object} formcontrols - An array of form controls to validate
         * @param {object} entity - The entity being edited.
         * @returns {bool} Are all the fields valid.
         */
        exports.validateFormControls = function (formControls, entity) {
            var errors = 0;

            _.map(formControls, function (control) {
                if (control.spValidateControl && !control.spValidateControl(entity)) {
                    errors++;
                    console.warn('Validation failed for control ' + control.idP + ' \'' + control.name + '\'');
                }
            });

            return errors === 0;
        };


        /**
         * Given a form control, return the field name for its template
         *
         * @param {object} formControl The form control entity..
         * @param {string} FormMode, what form mode are we in.
         * @param {Boolean} isReadOnly (optional) do we want the readonly version.
         * @returns {String} The filename including the partial path.
         */
        exports.getFormControlFile = function (formControl, formMode, isReadOnly) {
            var readMarker;
            var baseFileName;
            var designerControl;

            isReadOnly = isReadOnly || formMode !== exports.formModes.edit;
            baseFileName = formControl.firstTypeId().getAlias();

            designerControl = false;

            switch (baseFileName) {
                case 'structureControlOnForm':
                case 'verticalStackContainerControl':
                case 'horizontalStackContainerControl':
                    designerControl = true;
                    break;
            }

            if (formMode === exports.formModes.design && designerControl) {
                return 'editForm/partials/shared/designWrapper.tpl.html';

            } else {

                /////
                // TODO: Refactor this once all relevant controls have directives. Controls should be able to manage their read-only mode themselves.
                /////
                switch (baseFileName) {
                    // files without a readonly version
                    case 'structureControlOnForm':
                    case 'verticalStackContainerControl':
                    case 'horizontalStackContainerControl':
                    case 'headerColumnContainerControl':
                    case 'tabContainerControl':
                    case 'tabRelationshipRenderControl':
                    case 'reportRenderControl':
                    case 'chartRenderControl':
                    case 'formRenderControl':
                    case 'heroTextControl':
                    case 'customEditForm':
                    case 'multiLineTextControl':
                    case 'singleLineTextControl':
                    case 'inlineRelationshipRenderControl':
                    case 'imageRelationshipRenderControl':
                    case 'imageFileNameUploadControl':
                    case 'choiceRelationshipRenderControl':
                        readMarker = '';
                        break;
                    default:
                        readMarker = isReadOnly ? '_read' : '';
                }
            }

            switch (baseFileName) {
                // legacy controls without directives
                case 'calculatedRenderControl':
                case 'chartRenderControl':
                case 'choiceRelationshipRenderControl':
                case 'currencyKFieldRenderControl2':
                case 'customEditFormControl':
                case 'dateKFieldRenderControl2':
                case 'dateAndTimeKFieldRenderControl2':
                case 'decimalKFieldRenderControl2':
                case 'dropDownRelationshipRenderControl':
                case 'fieldControlOnForm':
                case 'fileNameUploadControl':
                case 'fileRevisionControl':
                case 'formRenderControl':
                case 'guidFieldRenderControl':
                case 'headerColumnContainerControl':
                case 'horizontalStackContainerControl':
                case 'imageControl':
                case 'imageRelationshipRenderControl':
                case 'imageFileNameUploadControl':
                case 'inlineRelationshipRenderControl':
                case 'largeTextBoxKFieldRenderControl':
                case 'mapControl':
                case 'multiChoiceRelationshipRenderControl':
                case 'numericKFieldRenderControl2':
                case 'multilineRelationshipRenderControl':
                case 'relationshipControlOnForm':
                case 'reportLinkControl':
                case 'reportRenderControl':
                case 'structureControlOnForm':
                case 'structureViewRelationshipControl':
                case 'switchKFieldRenderControl':
                case 'tabContainerControl':
                case 'textBoxKFieldRenderControl':
                case 'timeKFieldRenderControl2':
                case 'verticalStackContainerControl':
                case 'workflowButtonControl':
                case 'xmlFieldRenderControl':
                    return 'editForm/partials/' + baseFileName + readMarker + '.tpl.html';
                default:
                    return 'editForm/directives/spControlOnForm/spControlOnForm.tpl.html';
            }
        };

        /**
         * Given a form control, return the structure control template
         *
         * @param {object} formControl The form control entity..
         * @param {string} FormMode, what form mode are we in.
         * @param {Boolean} isReadOnly (optional) do we want the readonly version.
         * @returns {String} The filename including the partial path.
         */
        exports.getStructureControlFile = function (formControl, formMode) {

            var baseFileName;
            var designerControl;

            designerControl = exports.isStructureControl(formControl);

            if (formMode === exports.formModes.design && !designerControl) {
                return 'editForm/partials/shared/designStructureWrapper.tpl.html';

            } else {

                return 'editForm/partials/structureControlOnForm_repeater.tpl.html';
            }
        };

        exports.isStructureControl = function (formControl) {
            var baseFileName;
            var structureControl = false;

            if (formControl && formControl.firstTypeId) {
                baseFileName = formControl.firstTypeId().getAlias();

                switch (baseFileName) {
                    case 'structureControlOnForm':
                    case 'verticalStackContainerControl':
                    case 'horizontalStackContainerControl':
                    case 'headerColumnContainerControl':
                    case 'tabContainerControl':
                        structureControl = true;
                        break;
                }
            }

            return structureControl;
        };

        /**
         * does this form control have it's own title?
         *
         * @param {object} formControl The form control entity..
         * @returns {Boolean} Does it need to have a title.
         */
        exports.controlNeedsTitle = function (formControl) {
            return false;
            //switch (formControl.firstTypeId().getAlias()) {
            //    // files without a readonly version
            //    case 'customEditForm':
            //    case 'headerColumnContainerControl':
            //    case 'verticalStackContainerControl':
            //    case 'horizontalStackContainerControl':
            //    case 'tabContainerControl':
            //    case 'reportRenderControl':
            //    case 'chartRenderControl':
            //    case 'heroTextControl':
            //    case 'formRenderControl':
            //    case 'checkboxKFieldRenderControl':
            //        return false;

            //    default:
            //        return true;
            //}
        };

        /**
         * returns title of form control.
         *
         * @param {object} formControl The form control entity..
         * @returns {String} title.
         */
        exports.getControlTitle = function (formControl) {

            if (!formControl) {
                return '';
            }

            var title = formControl.getName();
            if (!title) {
                var isField = formControl.hasRelationship('console:fieldToRender') && formControl.getFieldToRender() !== null;
                if (isField) {
                    return formControl.getFieldToRender().getName();
                }

                var isRelationship = formControl.hasRelationship('console:relationshipToRender') && formControl.getRelationshipToRender() !== null;
                if (isRelationship) {
                    var relationship = formControl.getRelationshipToRender();

                    var isReversed;

                    if (formControl.getIsReversed) {
                        isReversed = formControl.getIsReversed();
                    }

                    title = relationship.getName();

                    if (isReversed) {
                        return relationship.fromName || title;
                    }
                    else {
                        return relationship.toName || title;
                    }
                }
            }
            return title;
        };

        /**
         * returns description of form control.
         *
         * @param {object} formControl The form control entity..
         * @returns {String} description.
         */
        exports.getControlDescription = function (formControl) {
            if (!formControl) {
                return '';
            }

            var desc;

            if (formControl.getDescription) {
                desc = formControl.getDescription();
            }

            if (!desc) {
                var isField = formControl.hasRelationship('console:fieldToRender') && formControl.getFieldToRender() !== null;

                if (isField) {
                    if (formControl.getFieldToRender().getDescription) {
                        return formControl.getFieldToRender().getDescription();
                    }
                }

                var isRelationship = formControl.hasRelationship('console:relationshipToRender') && formControl.getRelationshipToRender() !== null;

                if (isRelationship) {
                    if (formControl.getRelationshipToRender().getDescription) {
                        return formControl.getRelationshipToRender().getDescription();
                    }
                }
            }
            return desc;
        };

        /**
         * Take a URL which may not have a http:, https: or ftp: on the front and add a http:
         *
         * @param {String} url The form control entity..
         * @returns {String} The url with a http: added if necessary
         */
        exports.httperizeUrl = function (url) {

            if (url && url.search(/^(http:|https:|ftp:)/) === -1) {
                return 'http://' + url;
            } else {
                return url;
            }
        };


        /**
         * Given a new entity and a form set all the default values on the entity:
         *
         * @param {int} typeId entity The created entity.
         * @param {Entity} form The form containing the defaults
         * @returns {Entity} The created entity
         */
        exports.createEntityWithDefaults = function (typeId, form) {
            return exports.getAccountHolderEntity().then(function (accountEntity) {
                return getEntityWithDefaults(typeId, form, accountEntity);
            }, function () {
                return getEntityWithDefaults(typeId, form);
            });
        };

        /**
         * Given a form and an entity, set autoCardinality true for all relationshipson of the entity:
         *
         * @param {Entity} form The form.
         * @param {Entity} entity The instance being updated
         */
        exports.markAutoCardinalityOfAllRelationships = function (form, formData) {
            var flat = exports.getFormControls(form);

            _.map(flat, function (ctrl) {
                var isReverse;
                var relationship = ctrl.getLookup('console:relationshipToRender');
                if (relationship) {

                    isReverse = (ctrl.getIsReversed && ctrl.getIsReversed());

                    // get the relationship container
                    var relContainer = formData.getRelationshipContainer({
                        id: relationship.eid(),
                        isReverse: isReverse
                    });

                    if (relContainer) {
                        relContainer.autoCardinality(); // mark autoCardinality true for all relationships. (M:M relationships are ignored on server)
                    }
                }
            });
        };

        function getEntityWithDefaults(typeId, form, accountEntity) {
            var entity = spEntity.createEntityOfType(typeId);

            var flat = exports.getFormControls(form);

            _.map(flat, function (ctrl) {
                var field, relationship, defaultValue, nativeValue, dbType;
                var isReverse;

                field = ctrl.getLookup('console:fieldToRender');

                if (field) {
                    defaultValue = field.getField('defaultValue');

                    dbType = spEntityUtils.dataTypeForField(field);

                    var fieldContainer = entity.registerField(field.eid(), dbType);
                    fieldContainer.markAsPristine();

                    if (defaultValue) {
                        nativeValue = sp.convertDbStringToNative(dbType, defaultValue);
                        entity.setField(field.id(), nativeValue, dbType);
                    }
                }

                relationship = ctrl.getLookup('console:relationshipToRender');

                if (relationship) {

                    isReverse = ( ctrl.getIsReversed && ctrl.getIsReversed());
                    var isLookup = spResource.Relationship.toOne(relationship.cardinality.nsAlias, isReverse);
                    var relContainer;
                    if (isLookup) {
                        relContainer = entity.registerLookup({id: relationship.eid(), isReverse: isReverse});
                    } else {
                        relContainer = entity.registerRelationship({id: relationship.eid(), isReverse: isReverse});
                    }
                    relContainer.autoCardinality();     // mark autoCardinality true for all relationships. (M:M relationships are ignored on server)

                    var useCurrentUserField = (ctrl.getIsReversed && ctrl.getIsReversed()) ? relationship.getField('core:defaultFromUseCurrent') : relationship.getField('core:defaultToUseCurrent');
                    if (useCurrentUserField) {
                        if (accountEntity) {
                            var typeToMatch = (ctrl.getIsReversed && ctrl.getIsReversed()) ? relationship.fromType : relationship.toType;
                            var typeToMatchAlias = typeToMatch.getField('core:alias');

                            if (typeToMatchAlias === 'core:userAccount') {
                                entity.setRelationship(relationship.id(), accountEntity);
                            } else {
                                var accountHolder = exports.activeAccountEntity.getLookup('core:accountHolder');
                                if (accountHolder) {
                                    var isMatch = false;
                                    var type = accountHolder.getLookup('core:isOfType');
                                    var types = spResource.getAncestorsAndSelf(type);
                                    _.forEach(types, function (e) {
                                        if (typeToMatch.idP === e.idP) {
                                            isMatch = true;
                                        }
                                    });

                                    if (isMatch) {
                                        entity.setRelationship(relationship.id(), accountHolder);
                                    }
                                }
                            }
                        }
                    }
                    else {
                        defaultValue = (ctrl.getIsReversed && ctrl.getIsReversed()) ? relationship.getFromTypeDefaultValue() : relationship.getToTypeDefaultValue();

                        if (defaultValue) {
                            entity.setRelationship(relationship.id(), defaultValue);
                        }
                    }
                }
            });

            // owned by ('SecurityOwner') relationship
            if (exports.activeAccountEntity) {
                entity.setRelationship('core:securityOwner', exports.activeAccountEntity);
            }

            return entity;
        }

        exports.getAccountHolderEntity = function () {
            var deferred = $q.defer();

            if (spLoginService.accountId) {

                if (exports.activeAccountEntity && exports.activeAccountEntity.idP === spLoginService.accountId) {
                    deferred.resolve(exports.activeAccountEntity);
                }
                else {
                    spEntityService.getEntity(spLoginService.accountId, 'name, accountHolder.{ name, isOfType.inherits*.{ name } }', {
                        hint: 'efsAcctHolder',
                        batch: true
                    }).then(
                        function (accountEntity) {
                            if (accountEntity) {
                                exports.activeAccountEntity = accountEntity;
                            }
                            deferred.resolve(accountEntity);
                        },
                        function (error) {
                            console.error(error);
                            deferred.reject(error);
                        });
                }
            }
            else {
                console.error('account id is not set');
                deferred.reject('account id is not set');
            }

            return deferred.promise;
        };

        exports.compareByRenderingOrdinal = function (a, b) {
            var result = (a.renderingOrdinal || 0) - (b.renderingOrdinal || 0);
            return result;
        };


        var cleanReg = new RegExp("[\\W]|_", "g");

        /**
         * Clean up a provided testId
         */
        exports.cleanTestId = function (str) {

            return str && str.replace(cleanReg, "_");

        };


        //
        // Designer
        // THIS SHOULD BE MOVED

        // remove child from anywhere in the document
        exports.removeChild = function (parent, child) {
            var children = parent.getContainedControlsOnForm();

            if (children) {
                var index = children.indexOf(child);
                if (index > -1) {
                    console.log('child removed');
                    children.splice(index, 1);
                    parent.setContainedControlsOnForm(children);
                } else {
                    console.error('Failed to find child to remove');
                }
            }
            else {
                console.error('unable to find containedControlsOnForm relationship.', parent, child);
            }

        };

        // Add child to the end of the list
        exports.addChildToEnd = function (parent, child) {
            var children = parent.getContainedControlsOnForm().sort(exports.compareByRenderingOrdinal);

            if (children) {
                var lastChild = _.last(children);
                var lastChildOrdinal = 50;

                if (lastChild && lastChild.renderingOrdinal) {
                    lastChildOrdinal = lastChild.renderingOrdinal + 10;
                }

                child.setRenderingOrdinal(lastChildOrdinal);
                children.push(child);

                parent.setContainedControlsOnForm(children);
            }
            else {
                console.error('unable to find containedControlsOnForm relationship.', parent, child);
            }
        };


        // remove child from anywhere in the document
        exports.addChildAfter = function (parent, child, index) {
            var children = parent.getContainedControlsOnForm().sort(exports.compareByRenderingOrdinal);

            if (children) {
                var sibling = children[index];
                var siblingOrdinal;

                if (index === children.length) {
                    siblingOrdinal = sibling.renderingOrdinal + 10;
                } else {
                    siblingOrdinal = children[index + 1].renderingOrdinal;
                }

                child.renderingOrdinal = siblingOrdinal;

                children.splice(index + 1, 0, child);

                for (var i = index + 1; i < children.length; i++) {
                    children[i].renderingOrdinal = children[i].renderingOrdinal + 10;
                }

                parent.setContainedControlsOnForm(children);
            }
            else {
                console.error('unable to find containedControlsOnForm relationship.', parent, child);
            }
        };

        // Behavior that is common to all form controls
        exports.commonFormControlInit = function (scope, formControl) {

        };

        // Behavior that is common to all field controls
        exports.commonFieldControlInit = function (fieldToRender) {
            // nothing yet
        };

        // Behavior common for Relationship controls
        exports.disallowCreateRelatedEntityInNewMode = function(relTypeAlias, isReversed) {
            if (!relTypeAlias || !_.isBoolean(isReversed)) {
                return null;
            }
            var result = false;
            switch (relTypeAlias) {
                case 'core:relSingleComponent':
                    result = isReversed ? false : true;     // disallow in fwd rel direction
                    break;
                case 'core:relSingleComponentOf':
                    result = isReversed ? true : false;     // disallow in reverse rel direction
                    break;
                case 'core:relDependantOf':
                    result = isReversed ? true : false;     // disallow in reverse rel direction
                    break;
                case 'core:relDependants':
                    result = isReversed ? false : true;     // disallow in fwd rel direction
                    break;
                case 'core:relComponentOf':
                    result = isReversed ? true : false;     // disallow in reverse rel direction
                    break;
                case 'core:relComponents':
                    result = isReversed ? false : true;     // disallow in fwd rel direction
                    break;
                case 'core:relSharedDependantsOf':
                    result = isReversed ? true : false;     // disallow in reverse rel direction
                    break;
                case 'core:relSharedDependants':
                    result = isReversed ? false : true;     // disallow in fwd rel direction
                    break;
                case 'core:relLookup':
                    result = isReversed ? true : false;     // disallow in reverse rel direction (no ownership either end, but disallow the end that represents to 'many')
                    break;
                case 'core:relExclusiveCollection':
                    result = isReversed ? false : true;     // disallow in fwd rel direction (no ownership either end, but disallow the end that represents to 'many')
                    break;
                case 'core:relSingleLookup':
                    result = false;                         // allowed both ends. No ownership either end and both ends represent to 'one'
                    break;
                case 'core:relManyToMany':
                    result = true;                          // disallow both ends. No ownership either end but both ends represent to 'many'
                    break;
                case 'core:relChoiceField':
                case 'core:relMultiChoiceField':
                case 'core:relCustom':
                    result = false;     // n/a
                    break;
                default:
                    console.log('invalid relType provided');
                    break;
            }

            return result;
        };

        exports.getDisallowCreateRelatedEntityInNewModeMessage = function (relationship, isReversed) {
            if (!relationship || !_.isBoolean(isReversed)) {
                console.error('invalid relationship or direction provided');
                return null;
            }

            // assuming this function is called from the disallowed side of relationship and not the allowed side of relationship
            var parentTypeName, childTypeName;
            if (isReversed) {
                parentTypeName = sp.result(relationship, 'toType.name') || 'parent';
                childTypeName = sp.result(relationship, 'fromType.name') || 'related child';
            } else {
                parentTypeName = sp.result(relationship, 'fromType.name') || 'parent';
                childTypeName = sp.result(relationship, 'toType.name') || 'related child';
            }

            return 'You must save the ' + parentTypeName + ' before creating a ' + childTypeName;
        };
 
        /**
         * Initializes scope variables of a relationship form control.
         *
         * @param {object} scope object.
         */
        exports.commonRelFormControlInit = function (scope, options) {

            // allow either formControl or control as the control entity
            const formControl = scope.formControl || scope.control;

            scope.customValidationMessages = [];

            scope.relationshipToRender = formControl.getRelationshipToRender();
            scope.isReversed = (formControl.getIsReversed && formControl.getIsReversed()) ? formControl.getIsReversed() : false;
            if (scope.relationshipToRender) {
                scope.relTypeId = {id: scope.relationshipToRender.eidP, isReverse: scope.isReversed};
                scope.relAlias = exports.getRelIdForRequest(scope.relationshipToRender, scope.isReversed);
                var eType = scope.isReversed ?
                    scope.relationshipToRender.fromType ? scope.relationshipToRender.fromType : undefined :
                    scope.relationshipToRender.toType ? scope.relationshipToRender.toType : undefined;

                scope.entityType = eType ? eType.idP : undefined;
                scope.canCreateType = sp.result(eType, 'canCreateType');
                scope.createAccessDenied = scope.canCreateType === false;

                scope.testId = exports.cleanTestId(_.result(scope.relationshipToRender, 'getName'));

                if (options && options.areCreating) {
                    var relTypeAlias = sp.result(scope.relationshipToRender, 'relType.nsAlias');
                    if (relTypeAlias) {
                        scope.disallowCreateRelatedEntityInNewMode = exports.disallowCreateRelatedEntityInNewMode(scope.relationshipToRender.relType.nsAlias, scope.isReversed);
                    }
                }
            }

            scope.isMandatory = exports.mandatoryControlAndDefinition(formControl);
            scope.isReadOnly = scope.isReadOnly || formControl.readOnlyControl;

            scope.titleModel = exports.createTitleModel(formControl, scope.isInDesign);

            if (options && options.validator) {
                // provide a function to validate the model is correct. Used when saving
                formControl.spValidateControl = function (entity) {
                    options.validator();
                    return scope.customValidationMessages.length === 0;
                };
            } else {
                formControl.spValidateControl = null;         // make sure that no left over validator will be called.
            }
        };

        /**
         * Checks if a given form control is mandatory (this function checks for both mandatory on form as well as mandatory on the definition).
         *
         * @param {object} scope object.
         */
        exports.mandatoryControlAndDefinition = function (control) {

            if (!control)
                return false;

            var fieldToRender = control.fieldToRender;
            var relationshipToRender = control.relationshipToRender;
            var fieldMandatory = false;
            var relMandatory = false;

            if (fieldToRender) {
                fieldMandatory = fieldToRender.isRequired;
            }
            if (relationshipToRender) {
                relMandatory = control.isReversed ? relationshipToRender.revRelationshipIsMandatory : relationshipToRender.relationshipIsMandatory;
            }

            return fieldMandatory || relMandatory || control.mandatoryControl;
        };

        /**
         * Returns resourceViewerConsoleFormId or null.
         *
         * @param {object} scope object.
         */
        exports.getControlConsoleFormId = function (control) {
            if (control) {
                if (control.hasRelationship('resourceViewerConsoleForm') && control.getResourceViewerConsoleForm()) {
                    return control.getResourceViewerConsoleForm().id();
                }
            }

            return null;
        };

        /**
         * Validates the relationship form control.
         *
         * @param {object} scope object.
         * @param {object} relationships
         */
        exports.validateRelationshipControl = function (scope, relationships) {
            var errorMessages = spFieldValidator.validateFormRelationshipControl(scope.relationshipToRender, scope.isMandatory, relationships);
            if (errorMessages.length > 0) {
                spFieldValidator.raiseValidationErrors(scope, errorMessages);
            } else {
                spFieldValidator.clearValidationErrors(scope);
            }
        };

        exports.autoFillRelationshipData = function (navitem, formData, onSuccess, onFail) {

            //if (navitem && navitem.relationship && formData) {
            //    formData.setRelationship(navitem.relationship.id(), navitem.relatedEntity);
            //}
            if (navitem && navitem.relationshipId && navitem.relatedEntityId && formData) {
                // todo: use batching and clone the result object
                return spEntityService.getEntity(navitem.relatedEntityId, 'name', {
                    hint: 'efsAutoFill',
                    batch: false
                }).then(
                    function (entity) {
                        var rel;
                        var relationships;

                        if (entity) {
                            rel = {id: navitem.relationshipId, isReverse: navitem.isReverse};

                            if (!formData.hasRelationship(rel)) {
                                var relContainer;
                                // if form doesn't have the specified relationship, then register it.
                                if (navitem.isLookup) {
                                    relContainer = formData.registerLookup({
                                        id: navitem.relationshipId,
                                        isReverse: navitem.isReverse
                                    });
                                } else {
                                    relContainer = formData.registerRelationship({
                                        id: navitem.relationshipId,
                                        isReverse: navitem.isReverse
                                    });
                                }

                                relContainer.autoCardinality();     // mark autoCardinality true for all relationships. (M:M relationships are ignored on server)
                            }

                            relationships = formData.getRelationship(rel);
                            relationships.add(entity);
                        }

                        if (onSuccess) {
                            onSuccess();
                        }
                    },
                    function (error) {
                        console.error('editFormService.getFormData error:', error);
                        if (onFail) {
                            onFail(error);
                        }
                    });
            }
        };

        exports.addAutoFillRelationshipParams = function (navItem, relatedEntity, formControl) {

            if (navItem && formControl && formControl.hasRelationship('console:relationshipToRender')) {
                if (relatedEntity) {
                    navItem.relatedEntityId = relatedEntity.id();
                }
                var relationship = formControl.relationshipToRender;
                var isReversedInRelatedEntity = formControl.isReversed ? false : true;  // the direction is reversed in related entity to what it is in this entity.
                navItem.relationshipId = relationship.idP;
                navItem.isReverse = isReversedInRelatedEntity;
                navItem.isLookup = spResource.Relationship.toOne(relationship.cardinality.nsAlias, isReversedInRelatedEntity); // the direction is reversed in related entity to what it is in this entity.
            }
        };

        exports.setCreatedChildEntity = function (navItem, relatedEntity) {

            if (navItem && navItem.relationshipId && relatedEntity) {
                if (!sp.result(navItem, 'data.createdChildEntities')) {
                    navItem.data.createdChildEntities = [];
                }

                var relEntitiesDict = navItem.data.createdChildEntities;
                var foundRel = _.find(relEntitiesDict, function (relationship) {
                    return relationship.relId === navItem.relationshipId;
                });

                if (foundRel) {
                    foundRel.entities.push(relatedEntity);
                }
                else {
                    relEntitiesDict.push({
                        relId: navItem.relationshipId,
                        entities: [relatedEntity]
                    });
                }
            }
        };

        exports.getCreatedChildEntities = function (navItem, relationshipId) {

            if (navItem && sp.result(navItem, 'data.createdChildEntities')) {
                var relEntitiesDict = navItem.data.createdChildEntities;
                var foundRel = _.find(relEntitiesDict, function (relationship) {
                    return relationship.relId === relationshipId;
                });

                if (foundRel) {
                    return foundRel.entities;
                }

            }
            return undefined;
        };

        exports.removeCreatedChildEntity = function (navItem, relationshipId, entityId) {

            if (navItem && sp.result(navItem, 'data.createdChildEntities') && entityId) {
                var relEntitiesDict = navItem.data.createdChildEntities;
                var foundRel = _.find(relEntitiesDict, function (relationship) {
                    return relationship.relId === relationshipId;
                });

                if (foundRel && foundRel.entities) {
                    _.remove(foundRel.entities, function (createdEntity) {
                        return createdEntity.idP === entityId;
                    });
                }
            }
        };

        exports.setLookupOnReturnFromChildCreate = function (navItem, relationshipToRender, isReversed, formData) {
            if (navItem.data.createdChildEntities && navItem.data.createdChildEntities.length > 0) {
                var relatedEntities = exports.getCreatedChildEntities(navItem, relationshipToRender.idP);
                if (relatedEntities && relatedEntities.length > 0) {

                    // Set the look-up then mark it as unchanged. The create of the child shold have already set the value.
                    var relSelector = {
                        id: relationshipToRender.id(),
                        isReverse: isReversed
                    };

                    formData.setLookup(relSelector, relatedEntities[relatedEntities.length - 1]);

                    if (formData.dataState !== spEntity.DataStateEnum.Create) {
                        formData.getRelationshipContainer(relSelector).markUnchanged();
                    }
                }
            }
        };

        exports.setRelationshipOnReturnFromChildCreate = function (navItem, relationshipToRender, isReversed, formData) {
            if (navItem.data.createdChildEntities && navItem.data.createdChildEntities.length > 0) {
                var relatedEntities = exports.getCreatedChildEntities(navItem, relationshipToRender.idP);
                if (relatedEntities && relatedEntities.length > 0) {
                    var hasRelationship = formData.hasRelationship({
                        id: relationshipToRender.eid(),
                        isReverse: isReversed
                    });
                    if (hasRelationship) {
                        var rel = formData.getRelationship({id: relationshipToRender.eid(), isReverse: isReversed});
                        if (rel) {
                            rel.add(relatedEntities);
                        }
                    } else { // register and set the relationship
                        formData.setRelationship({
                            id: relationshipToRender.eid(),
                            isReverse: isReversed
                        }, relatedEntities);
                    }
                }
            }
        };

        exports.updateLookupNameOnReturnFromChildUpdate = function (navItem, relationshipToRender, isReversed, formData) {

            var updatedEntity = sp.result(navItem, 'data.updatedChildEntity');
            if (updatedEntity) {
                // update name of entity without altering its dataState
                var entities = formData.getRelationship({id: relationshipToRender.eid(), isReverse: isReversed});
                var lookupEntity = _.find(entities, function (entity) {
                    return entity.idP === updatedEntity.idP;
                });
                if (lookupEntity) {
                    var lookupEntityNameFieldContainer = lookupEntity.getFieldContainer('core:name');
                    if (lookupEntityNameFieldContainer) {
                        lookupEntityNameFieldContainer.setRawFieldValue(updatedEntity.name);
                    }
                }
            }
        };

        exports.getRelControlPickerReport = function (relControlOnForm) {
            if (relControlOnForm) {
                if (relControlOnForm.hasRelationship('console:pickerReport') && relControlOnForm.getPickerReport() !== null) {
                    //when the getPickerReport is template report, check defaultPickerReport of the type first. if not null, use the defaultPickerReport
                    var defaultPickerReport = getDefaultPickerReport(relControlOnForm);
                    if (relControlOnForm.getPickerReport().name === "Template" && defaultPickerReport !== null) {
                        return defaultPickerReport;
                    } else {
                        return relControlOnForm.getPickerReport();
                    }
                }
                else {
                    return getDefaultPickerReport(relControlOnForm);
                }
            }
            return null;
        };

        exports.getRelControlDisplayReport = function (relControlOnForm) {
            if (relControlOnForm) {
                if (relControlOnForm.hasRelationship('console:relationshipDisplayReport') && relControlOnForm.getRelationshipDisplayReport() !== null) {

                    //when the getRelationshipDisplayReport is template report, check defaultDisplayReport of the type first. if not null, use the defaultDisplayReport
                    var defaultDisplayReport = getDefaultDisplayReport(relControlOnForm);
                    if (relControlOnForm.getRelationshipDisplayReport().name === "Template" && defaultDisplayReport !== null) {
                        return defaultDisplayReport;
                    } else {
                        return relControlOnForm.getRelationshipDisplayReport();
                    }
                }
                else {
                    return getDefaultDisplayReport(relControlOnForm);
                }
            }
            return null;
        };

        function getDefaultDisplayReport(relControlOnForm) {
            var retVal = null;
            if (relControlOnForm && relControlOnForm.getRelationshipToRender() !== null) {
                var relation = relControlOnForm.getRelationshipToRender();
                var isReversed = relControlOnForm.getIsReversed ? relControlOnForm.getIsReversed() : false;
                var type = isReversed ? relation.getFromType() : relation.getToType();

                if (type && type.hasRelationship('defaultDisplayReport')) {
                    retVal = type.getDefaultDisplayReport();
                }
            }
            return retVal;
        }

        function getDefaultPickerReport(relControlOnForm) {
            var retVal = null;
            if (relControlOnForm && relControlOnForm.getRelationshipToRender() !== null) {
                var relation = relControlOnForm.getRelationshipToRender();
                var isReversed = relControlOnForm.getIsReversed ? relControlOnForm.getIsReversed() : false;
                var type = isReversed ? relation.getFromType() : relation.getToType();

                if (type && type.hasRelationship('defaultPickerReport')) {
                    retVal = type.getDefaultPickerReport();
                }
            }
            return retVal;
        }

        exports.resizeControl = function (control, width, height) {
            if (control) {
                if (control.setRenderingWidth) {
                    control.setRenderingWidth(width);
                }

                if (control.setRenderingHeight) {
                    control.setRenderingHeight(height);
                }
            }
        };

        exports.setControlBackgroundColor = function (control, color) {
            if (control) {
                if (control.setRenderingBackgroundColor) {
                    control.setRenderingBackgroundColor(color);
                }
            }
        };

        exports.getControlBackgroundColor = function (control) {
            if (control) {
                if (control.getRenderingBackgroundColor) {
                    return control.getRenderingBackgroundColor();
                }
            }

            return null;
        };

        exports.getTemplateReport = function (onSuccess, onFail) {
            return exports.getTemplateReportP().then(
                function (report) {
                    if (onSuccess) {
                        onSuccess(report);
                    }
                    return report;
                },
                function (error) {
                    console.error('Error occurred getTemplateReport:', error);
                    if (onFail) {
                        onFail(error);
                    }
                    return error;
                });
        };


        exports.getTemplateReportP = _.memoize(function () {         // we can memoize this because the template report never changes.
            return spEntityService.getEntity('templateReport', 'name, description', {hint: 'efsTemplRpt', batch: true});
        });

        exports.getNameFieldEntity = function () {
            return spTenantSettings.getNameFieldEntity();
        };

        exports.canHaveManyRelatedEntities = function (relCardinality, isReverseRelationship) {
            var retVal = false;
            if ((isReverseRelationship && relCardinality === 'core:manyToOne') || (!isReverseRelationship && relCardinality === 'core:oneToMany') || relCardinality === 'core:manyToMany') {
                retVal = true;
            }
            return retVal;
        };

        exports.getDisplayName = function (entity) {
            if (_.isArray(entity)) {
                return _.map(entity, exports.getDisplayName).join(', ');
            } else if (entity) {
                return entity.name || 'Unnamed resource';
            } else {
                return '';
            }
        };

        // get comma separated name of selected entities
        exports.getEntitiesDisplayName = function (entities) {
            var name = '';
            if (entities && entities.length > 0) {
                for (var i = 0; i < entities.length; i++) {
                    if (entities[i]) {
                        name = name + exports.getDisplayName(entities[i]);
                    }

                    if (i !== (entities.length - 1)) {
                        name = name + ', ';
                    }
                }
            }
            return name;
        };


        exports.createTitleModel = function (formControl, isInDesign) {

            var titleModel = {};

            if (formControl) {
                var fieldToRender = formControl.getLookup('console:fieldToRender');
                var relToRender = formControl.getLookup('console:relationshipToRender');

                titleModel.formControl = formControl;
                titleModel.fieldToRender = fieldToRender;
                titleModel.relToRender = relToRender;

                titleModel.readonly = !isInDesign;

                /////
                // Setup the title Models description.
                /////
                Object.defineProperty(titleModel, 'description', {
                    get: function () {

                        return this.formControl.description || (this.fieldToRender && this.fieldToRender.description) || (this.relToRender && this.relToRender.description);
                    },
                    enumerable: true,
                    configurable: true
                });

                /////
                // Setup the title Models name.
                /////
                Object.defineProperty(titleModel, 'name', {
                    get: function () {
                        if (this.fieldToRender) {
                            return this.formControl.name || this.fieldToRender.name;

                        } else if (this.relToRender) {
                            var reversed = this.formControl.isReversed;

                            if (this.formControl.name) {
                                return this.formControl.name;
                            } else if (!reversed && this.relToRender.toName) {
                                return this.relToRender.toName;
                            } else if (reversed && this.relToRender.fromName) {
                                return this.relToRender.fromName;
                            } else {
                                return this.relToRender.name;
                            }
                        } else {
                            return this.formControl.name;
                        }
                    },
                    set: function (value) {

                        this.formControl.name = value;
                    },
                    enumerable: true,
                    configurable: true
                });

                /////
                // Setup the title Models description.
                /////
                Object.defineProperty(titleModel, 'hasName', {
                    get: function () {

                        return !!this.name;
                    },
                    enumerable: true,
                    configurable: true
                });
            }

            return titleModel;
        };


        exports.getDerivedTypesContextMenu = function (scope, entityTypeId, canCreateDerivedTypes, createHandler) {
            var err = '';
            if (!entityTypeId || !(_.isNumber(entityTypeId) || _.isString(entityTypeId))) {
                console.log('editFormService.getDerivedTypesContextMenu', 'Invalid entityTypeId provided');
                throw new Error('Invalid entityTypeId provided');
            }

            if (!_.isBoolean(canCreateDerivedTypes) || !createHandler) {
                console.log('spDropdownRelationshipPicker.getDerivedTypesContextMenu', 'Invalid params provided');
                throw new Error('Invalid params provided');
            }

            var request = '';
            if (canCreateDerivedTypes === true) {
                request = 'name, alias, isAbstract, canCreateType, k:defaultEditForm.name, derivedTypes*.{ name, alias, isAbstract, canCreateType, k:defaultEditForm.name}';
            }
            else {
                request = 'name, isAbstract, canCreateType, k:defaultEditForm.name';
            }


            return spEntityService.getEntity(entityTypeId, request, {hint: 'dtCtx', batch: true})
                .then(function (entityType) {
                    buildDerivedTypesMenuAndDefaultFormsDict(scope, entityType, createHandler);
                });
        };

        function buildDerivedTypesMenuAndDefaultFormsDict(scope, entityType, createHandler) {

            var derivedTypes = spResource.getDerivedTypesAndSelf(entityType);
            var nonAbstractDerivedTypes = [];
            if (derivedTypes) {
                var menuItems = [];

                _.forEach(derivedTypes, function (derived) {
                    var canCreateType = sp.result(derived, 'canCreateType');
                    var isAbstract = sp.result(derived, 'isAbstract');

                    if (canCreateType && !isAbstract) { // skip the abstract types and the type that user do not have access to create instances of
                        var menuItem = {
                            text: derived.getName(),
                            type: 'click',
                            click: createHandler + '(' + derived.id() + ')'
                            //click: 'handleCreate(' + derived.id() + ')'
                        };

                        menuItems.push(menuItem);

                        nonAbstractDerivedTypes.push(derived);
                    }
                });

                scope.contextMenu = {
                    menuItems: menuItems
                };
            }

            // dict
            buildEntityDefaultFormDict(scope, nonAbstractDerivedTypes);

        }

        function buildEntityDefaultFormDict(scope, derivedTypes) {
            if (derivedTypes) {
                var dict = {};

                _.forEach(derivedTypes, function (derived) {

                    if (derived.hasRelationship('console:defaultEditForm')) {
                        var form = derived.getDefaultEditForm();
                        dict[derived.id()] = form !== null ? form.id() : null;
                    }
                });

                scope.entityDefaultFormDict = dict;
            }
        }


        exports.formatSaveErrorMessage = function (error) {
            var message = error.message;
            if (error.data) {
                message += ': ' + (error.data.ExceptionMessage || error.data.Message);
            }
            return message;
        };


        exports.getRelationshipFilterData = function (formControl, formData) {
            var filters, relationshipControlFilters, sortedRelationshipControlFilters;

            if (!formControl || !formControl.hasRelationship('console:relationshipControlFilters') || !formData) {
                return null;
            }

            // Get all the filters for the current control
            relationshipControlFilters = formControl.getRelationship('console:relationshipControlFilters');
            if (!relationshipControlFilters || !relationshipControlFilters.length) {
                return null;
            }

            // Order the filters
            sortedRelationshipControlFilters = _.sortBy(relationshipControlFilters, function (rf) {
                return rf.getField('console:relationshipControlFilterOrdinal') || 0;
            });

            filters = _.map(sortedRelationshipControlFilters, function (rf) {
                var relationshipControl,
                    relationshipToRender,
                    relationshipFilter,
                    relationshipFilterId,
                    relationshipFilterDirection,
                    relationshipFilterDirectionAlias,
                    relatedEntityIds = [],
                    relId,
                    isReverse = false;

                relationshipControl = rf.getLookup('console:relationshipControlFilter');
                if (relationshipControl) {
                    relationshipToRender = relationshipControl.relationshipToRender;
                    isReverse = !!relationshipControl.isReversed;
                }

                relationshipFilter = rf.getLookup('console:relationshipFilter');
                relationshipFilterDirection = rf.getLookup('console:relationshipDirectionFilter');

                relationshipFilterId = relationshipFilter ? relationshipFilter.id() : -1;
                relationshipFilterDirectionAlias = relationshipFilterDirection && relationshipFilterDirection._id ? relationshipFilterDirection._id._alias : null;                

                if (relationshipToRender) {
                    relId = {
                        id: relationshipToRender.id(),
                        isReverse: isReverse
                    };

                    var alias = relationshipToRender.nsAlias;
                    var relContainer = formData.getRelationshipContainer(relId);
                    if (!relContainer) {
                        relContainer = formData.getRelationshipContainer(alias);
                    }

                    if (relContainer) {
                        relatedEntityIds = _.map(relContainer.entities, function (e) {
                            return e.id();
                        });
                    }
                }

                var result = {
                    relid: relationshipFilterId,
                    dir: relationshipFilterDirectionAlias,
                    eids: relatedEntityIds
                };

                return result;
            });

            return filters;
        };


        // Get the filtered control ids that the specified source control filters
        exports.getFilteredControlIds = function (form, sourceControl) {
            var filterSourceControls, filteredSourceControl;

            if (!form || !sourceControl) {
                return [];
            }

            // Get all the filter sources for the current form
            filterSourceControls = filterSourceControlsPerForm[form.id()];

            if (!filterSourceControls) {
                return [];
            } else {
                // Get all the filtered controls for the current source control
                filteredSourceControl = filterSourceControls[sourceControl.id()];

                return filteredSourceControl.filteredControls;
            }
        };


        // Returns true if the specified control is a source relationship filter
        exports.isSourceRelationshipFilterControl = function (form, formControl) {
            var filterSourceControls;

            if (!form || !formControl) {
                return false;
            }

            // Get all the filter sources for the current form
            filterSourceControls = filterSourceControlsPerForm[form.id()];

            if (!filterSourceControls) {
                return false;
            } else {
                return _.has(filterSourceControls, formControl.id());
            }
        };


        // Registers any relationship control filters for this form.
        // Each source control is registered along with the
        // controls that it filters.
        exports.registerRelationshipControlFilters = function (form) {
            var sourceFilterControls = {};

            if (!form) {
                return;
            }

            _.forEach(exports.getFormControls(form), function (formControl) {
                var relationshipControlFilters;

                if (!formControl.hasRelationship('console:relationshipControlFilters')) {
                    return true;
                }

                relationshipControlFilters = formControl.getRelationship('console:relationshipControlFilters');

                _.forEach(relationshipControlFilters, function (rf) {
                    var sourceRelationshipControl = rf.getLookup('console:relationshipControlFilter');
                    var sourceFilterControl;

                    if (sourceRelationshipControl) {
                        sourceFilterControl = sourceFilterControls[sourceRelationshipControl.id()];

                        if (!sourceFilterControl) {
                            sourceFilterControl = {
                                filteredControls: []
                            };
                            sourceFilterControls[sourceRelationshipControl.id()] = sourceFilterControl;
                        }

                        sourceFilterControl.filteredControls.push(formControl.id());
                    }
                });

                return true;
            });

            filterSourceControlsPerForm[form.id()] = sourceFilterControls;
        };


        // Clear any filter source controls
        exports.clearRelationshipControlFilters = function (form) {
            if (!form) {
                return;
            }

            delete filterSourceControlsPerForm[form.id()];
        };


        // Run the report and get entities from the result
        exports.getReportDataAsEntities = function (reportId, options, withFilter) {
            if (!reportId || !options) {
                return $q.when();
            }

            return spTenantSettings.getTemplateReportIds().then(function (ids) {
                var isTemplateReport = _.has(ids, reportId);
                var reportOptions = _.clone(options);
                reportOptions.entityTypeId = isTemplateReport ? options.entityTypeId : 0;

                // Run the report and convert the result to entities
                if (reportId === "console:enumValuesReport" && !withFilter) {
                    //if run the enumValues Report, use spEnumValueService to retrieve enum Values
                    return spEnumValueService.getEnumValue(reportId, reportOptions).then(function (data) {
                        return reportDataAsEntities(data);
                    });
                } else {

                    return spReportService.getReportData(reportId, reportOptions).then(function (data) {
                        return reportDataAsEntities(data);
                    });
                }
            });
        };

        // Run the report and get the result
        exports.getReportData = function (reportId, options) {
            if (!reportId || !options) {
                return $q.when();
            }

            return spTenantSettings.getTemplateReportIds().then(ids => {
                const isTemplateReport = _.has(ids, reportId);
                const reportOptions = _.clone(options);

                reportOptions.entityTypeId = isTemplateReport ? options.entityTypeId : 0;
                return spReportService.getReportData(reportId, reportOptions);
            });
        };


        // Run the picker report and get entities from the result
        // Return the promise if any request is performed.
        exports.setPickerEntitiesFromReportData = function (reportOptions, pickerOptions) {
            if (!reportOptions || !pickerOptions) {
                return;
            }

            if (!reportOptions.reportId || !reportOptions.entityTypeId) {
                return;
            }

            var withFilter = reportOptions.relfilters && reportOptions.relfilters.length > 0;

            return exports.getReportDataAsEntities(reportOptions.reportId, reportOptions, withFilter).then(function (entities) {
                pickerOptions.entities = entities;
            });
        };


        // flatted the controls into a single list preserving order according to renderingOrdinal
        exports.flattenControls = function (formControl) {
            var orderedChildren = _.sortBy(formControl.containedControlsOnForm, 'renderingOrdinal');
            return _.reduce(orderedChildren, function (acc, value) {
                return acc.concat(exports.flattenControls(value));
            }, [formControl]);
        };

        exports.simplifyForm = simplifyForm;
        exports.dumpFormToConsole = dumpFormToConsole;        

        return exports;        

        //////////////////////////////////////////////////////////////////////////////

        /**
         * Return a modified form suitable for use on mobile.
         * @param formControl - is not modified, we return a modified copy
         * @returns {spEntity.Entity|*} the modified formControl
         */
        function simplifyForm(formControl) {

            console.time('makeMobileEditForm');

            // make a clone that we can modify and return
            // - beware however, it is slow (takes 500msec for a GRC form on a fastish dev box
            // ... need to check on mobile device
            console.time('makeMobileEditForm - clone');
            // console.profile("clone form");
            formControl = formControl.cloneDeep();
            // console.profileEnd();
            console.timeEnd('makeMobileEditForm - clone');

            replaceTabContainers(formControl);
            removeControlsToBeAPage(formControl);
            removeEmptyContainers(formControl);

            console.timeEnd('makeMobileEditForm');

            return formControl;
        }

        function replaceTabContainers(formControl) {
            var tabControlsAndParents = [];
            walkControlTree(formControl, function (c, parents) {
                var parent = _.last(parents);
                if (isTabContainerControl(c) && parent) {
                    tabControlsAndParents.push([c, parent]);
                }
            });
            _.forEach(tabControlsAndParents, function (p) {
                //promoteTabs(p[0], p[1], formControl);
                promoteTabs(p[0], p[1], null);
            });
        }

        function removeControlsToBeAPage(formControl) {
            var relControls = [];
            walkControlTree(formControl, function (c, parents) {
                var parent = _.last(parents);
                if (isPagedControl(c) && parent) {
                    relControls.push([c, parent]);
                }
            });
            _.forEach(relControls, function (p) {
                var c = p[0], parent = p[1];
                parent.containedControlsOnForm.remove(c);
            });
        }

        function removeEmptyContainers(formControl, recurCount) {
            var controlsToDelete = [];

            walkControlTree(formControl, function (c, parents) {
                var parent = _.last(parents);
                //about bug 27828, the containner header will be hided in mobile if it is ContainnerControl or empty control.
                //after discuss with Darren, if the containner header is not empty, do not remove it as empty containner
                if (isContainerControl(c) && _.isEmpty(c.containedControlsOnForm) && _.isEmpty(c.name) && parent) {
                    controlsToDelete.push([c, parent]);
                }
            });
            _.forEach(controlsToDelete, function (p) {
                var c = p[0], parent = p[1];
                parent.containedControlsOnForm.remove(c);
            });

            //todo - handle nested empty containers without recursion
            recurCount = recurCount || 0;
            if (!_.isEmpty(controlsToDelete) && recurCount < 10) {
                removeEmptyContainers(formControl, recurCount + 1);
            }
        }

        function promoteTabs(control, parent, formControl) {
            console.assert(control && parent);

            var newParent = formControl || parent;

            _.forEach(control.containedControlsOnForm, function (c) {
                newParent.containedControlsOnForm.add(c);
                c.renderingOrdinal += (control.renderingOrdinal || 0) * 100;
                if (isContainerControl(c)) {
                    c.hideLabel = false;
                }
            });
            control.containedControlsOnForm.clear();
            parent.containedControlsOnForm.remove(control);
        }

        function isTabContainerControl(c) {
            return c.firstTypeId().getAlias() === 'tabContainerControl';
        }

        function isPagedControl(c) {
            return (sp.result(c, 'isOfType.0.pagerSupportMobile') === true);
        }

        function isContainerControl(c) {
            return _.includes([
                'tabContainerControl',
                'horizontalStackContainerControl',
                'verticalStackContainerControl'
            ], c.firstTypeId().getAlias());
        }

        function dumpFormToConsole(formControl, controlToString) {
            walkControlTree(formControl, function (c, parents) {
                var depth = parents.length;
                console.log('+' + depth + _.repeat('--', depth || 0) +
                    (controlToString ? controlToString(c) : [c.debugString, c.renderingOrdinal].join()));
            });
        }

        function walkControlTree(container, f, parents) {
            parents = parents || [];
            f.call(null, container, parents);
            parents = parents.concat([container]);
            _(container.containedControlsOnForm)
                .sortBy('renderingOrdinal')
                .forEach(function (c) {
                    walkControlTree(c, f, parents);
                });
        }
    }

}());