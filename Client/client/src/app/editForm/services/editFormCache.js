// Copyright 2011-2016 Global Software Innovation Pty Ltd


/**
* @ngdoc module
* @name editForm.service:editFormCache
* 
*  @description A form caching service.
*/

(function	()	{
	'use strict';

	angular.module('mod.app.editFormCache', ['ng', 'mod.common.spEntityService', 'angular-data.DSCacheFactory', 'mod.app.editFormWebServices']);


    
/**
* @ngdoc service
* @name mod_app.editFormCache:editFormCache
* 
*  @description This service is used by any Page that used the custom edit forms and should use cached forms. the cache 
*/
    
	angular.module('mod.app.editFormCache')
        .constant('editFormCacheSettings', {
            FormCapacity: 40,
            InstanceCapacity: 200,
            TypeFormCapacity: 200,
            FormVisCalcDependencyCapacity: 200,   
            FormRefreshCycleInSeconds: 60,
            MaxAgeInMinutes: 60
        })
        .factory('editFormCache', function ($q, $rootScope, spLoginService, spLocalStorage, spEntityService, editFormWebServices, DSCacheFactory, editFormCacheSettings) {

            var exports = {};
            
	        var defaultCacheSettings = {
	            maxAge:      editFormCacheSettings.MaxAgeInMinutes * 60 * 1000,
	            recycleFreq: 5 * 60 * 1000,                                     // how often stale entries are cleared out
	            deleteOnExpire: 'aggressive',
	            storageMode: 'localStorage' /*'memory'*/
	        };   // make it memory until the entity serialization is sorted out.

	        var formCacheSettings = _.defaults( { capacity: editFormCacheSettings.FormCapacity},   defaultCacheSettings);          
	        var instanceCacheSettings = _.defaults({  capacity: editFormCacheSettings.InstanceCapacity },    defaultCacheSettings);   
	        var typeFormCacheSettings = _.defaults({ capacity: editFormCacheSettings.TypeFormCapacity }, defaultCacheSettings);
            var formVisCalcDependencyCacheSettings = _.defaults({ capacity: editFormCacheSettings.FormVisCalcDependencyCapacity }, defaultCacheSettings);	        

	        var formCache = DSCacheFactory('formCache', formCacheSettings);
	        var instanceFormCache = DSCacheFactory('itemFormCache', instanceCacheSettings);
	        var typeFormCache = DSCacheFactory('typeFormCache', typeFormCacheSettings);
            var formVisCalcDependencyCache = DSCacheFactory('formVisCalcDependencyCache', formVisCalcDependencyCacheSettings);
	        var invalidatingEntityToFormCache = {};

	        var pendingFormRequests = {};
	        var pendingInstanceRequests = {};
	        var pendingDefRequests = {};
	        var pendingGenRequests = {};
            var pendingVisCalcRequests = {};

	        var deserialize = spEntity.deserialize;
	        var serialize = spEntity.serialize;

            // When will a form need to be refreshed
	        function getFormExpiration() {
	            return new Date((new Date()).getTime() + editFormCacheSettings.FormRefreshCycleInSeconds * 1000);
            }

            // Put a form in the cache
	        function putForm(key, form, isGenerated) {
	            var changeMarker = !form.cacheChangeMarker ? null : form.cacheChangeMarker;         // the service can come back with undefined.
	            var expiresAt = !isGenerated ? getFormExpiration() : null;                          // generated forms don't expire
	            var json = serialize(form);
	            try {
	                formCache.put(key, { cacheChangeMarker: changeMarker, isGenerated: isGenerated, expiresAt: expiresAt, form: json });
	            } catch (e) {
	                console.error('Failed to store form in cache', e);
	            }
	        }

            // Put form visibility calc dependencies in the cache
	        function putFormVisCalcDependencies(key, data) {
	            try {
	                formVisCalcDependencyCache.put(key, { data: data });
	            } catch (e) {
	                console.error('Failed to store calc dependency data in cache', e);
	            }
	        }

	        // Get a form from the cache
	        function getForm(key, skipModificationCheck) {

	            var cacheEntry = formCache.get(key);

	            if (!cacheEntry) {  // nothing cached
	                return $q.when(null);
	            }

	            var form = deserialize(cacheEntry.form);
	            if (!form) {        // could not deserialize (eg version mismatch)
	                return $q.when(null);
	            }

	            if ( cacheEntry.isGenerated ||                  // generated forms don't expire
                     new Date() < cacheEntry.expiresAt) {

	                // The form is still good, send it back
	                return $q.when(form);
	            }

                // The form may need to be updated.
	            return skipModificationCheck ? $q.when(form) : refreshForm(key, cacheEntry, form);
	        }
            
            // Check if the form has changed on the server. 
	        // If it has changed, remove it from the cache to force a refresh.
            // if it has not, update the expiry.
	        function refreshForm(key, cacheEntry, form) {
	            return spEntityService.getEntity(key, 'cacheChangeMarker', {hint: 'editFormCache-refreshEntry', batch: true})
                    .then(function(entity) {
                        if (cacheEntry.cacheChangeMarker === entity.cacheChangeMarker) {    // unchanged
                            cacheEntry.expiresAt = getFormExpiration();
                            return form;
                        } else {                                                            // changed
                            console.log('editFormCache: form changed on server ', key);
                            formCache.remove(key);
                            formVisCalcDependencyCache.remove(key);
                            return undefined;
                        }
                    });
	        }

            /**
            * @ngdoc method
            * @name clearServerFormsCache
            * @description remove given formIds from server side forms cache 
            *
            * @methodOf mod_app.editFormCache:editFormCache
            */
	        exports.clearServerFormsCache = function() {
	            var cachedFormIds = formCache.keys();

	            if (cachedFormIds && _.isArray(cachedFormIds)) {
	                // remove given formIds from server side forms cache
	                editFormWebServices.removeFormsFromServerFormCache(cachedFormIds);
	            }
	        };
	        
	    
            /**
              * @ngdoc method
              * @name getFormDefinition
              * @description Given a formId, get the form.
              *
            * @methodOf mod_app.editFormCache:editFormCache
              */
	        exports.getFormDefinition = function (selectedFormIdOrAlias, skipModificationCheck) {

	            // are we already asking for it?
                var pendingRequest = pendingFormRequests[selectedFormIdOrAlias];
        
	            if (pendingRequest) {
	                return pendingRequest;
	            }

	            // do we have it in the cache?
	            return getForm(selectedFormIdOrAlias, skipModificationCheck).then(function (cachedForm) {

	                if (cachedForm) {
	                    console.log('editFormCache: Using cached form ', selectedFormIdOrAlias);
	                    return $q.when(cachedForm);
	                }

	                else { // Better go get it.
	                    console.log('editFormCache: Fetching form ', selectedFormIdOrAlias);
	                    
	                    var request = editFormWebServices.getFormDefinition(selectedFormIdOrAlias, false)
                            .then(function (form) {
                                putForm(selectedFormIdOrAlias, form);
                                return form;
                            })
	                        .finally(function () {
	                            pendingFormRequests[selectedFormIdOrAlias] = undefined;
	                        });

	                    pendingFormRequests[selectedFormIdOrAlias] = request;

	                    return request;
	                }
	            });
            };



	        /**
            * @ngdoc method
            * @name getFormForInstance
            * @description Given an entityId, get the form 
            *
            * @methodOf mod_app.editFormCache:editFormCache
            */
	        exports.getFormForInstance = function (selectedFormIdOrAlias, skipModificationCheck) {

                // are we already asking for it?
                var pendingRequest = pendingInstanceRequests[selectedFormIdOrAlias];

                if (pendingRequest) {
                    return pendingRequest;
                }

                // do we have it in the cache?
                var cachedFormID = instanceFormCache.get(selectedFormIdOrAlias);

                if (cachedFormID) {
                    return exports.getFormDefinition(cachedFormID, skipModificationCheck);
                }

                else { // Better go get it.

                    var request = getDefaultFormInfo(selectedFormIdOrAlias)
                        .then(function (defaultFormInfo) {
                            var generatedTypeId;
                            var generateRequest;
                            var formType = getFormTypeForInstance(defaultFormInfo);

                            if (formType) {
                                var formId = getFormIdForInstance(defaultFormInfo);

                                typeFormCache.put(formType.id(), formId);
                                instanceFormCache.put(selectedFormIdOrAlias, formId);

                                return exports.getFormDefinition(formId, skipModificationCheck);
                            } else {
                                // we need a generated form
                                console.log('editFormCache: Generating form for instance ', selectedFormIdOrAlias);
                                generatedTypeId = defaultFormInfo.isOfType[0].id();
                                return generateForm(generatedTypeId).then(function (form) {
                                    instanceFormCache.put(selectedFormIdOrAlias, form.id());
                                    typeFormCache.put(generatedTypeId, form.id());
                                    return form;
                                });
                            }
                                
                        })
                        .finally(function () {
                            pendingInstanceRequests[selectedFormIdOrAlias] = undefined;
                        });

                    pendingInstanceRequests[selectedFormIdOrAlias] = request;

                    return request;
              
                }
            };
	    
            /**
            * @ngdoc method
            * @name invalidateFormsForEntity
            * @description Invalidates the forms assigned to the specified entity from the cache
            *
            * @methodOf mod_app.editFormCache:invalidateFormsForEntity
            */
	        exports.invalidateFormsForEntity = function (entityId) {
	            if (!entityId) {
	                return;
	            }

	            var formIds = invalidatingEntityToFormCache[entityId];
	            if (formIds) {
	                _.forOwn(formIds, function (value, formId) {
	                    exports.remove(formId);
	                });
	            }

	            delete invalidatingEntityToFormCache[entityId];
	        };

            /**
            * @ngdoc method
            * @name assignInvalidatingEntityToForm
            * @description Assigns the specified entity to the specified form.
            *
            * @methodOf mod_app.editFormCache:assignInvalidatingEntityToForm
            */
	        exports.assignInvalidatingEntityToForm = function (entityId, formId) {
	            if (!entityId || !formId) {
	                return;
	            }

	            var formIds = invalidatingEntityToFormCache[entityId];
	            if (!formIds) {
	                formIds = {};
	                invalidatingEntityToFormCache[entityId] = formIds;
	            }

	            if (!_.has(formIds, formId)) {
	                formIds[formId] = true;	                
	            }	            
	        };

            // Given an instance id or alias, return the default form for it.
	        function getDefaultFormInfo(idOrAlias) {
	            return spEntityService.getEntity(idOrAlias, 'isOfType.console:defaultEditForm.id', { hint: 'getFormIdForInstance', batch: true });
	        }

            // Get the frst type with a form
	        function getFormTypeForInstance(defaultFormInfo) {
	            var type = _.find(defaultFormInfo.isOfType, function (t) { return t.defaultEditForm; });
	            return type;
	        }

            // Given an instance id or alias, return the default form for it.
	        function getFormIdForInstance(defaultFormInfo) {
	            return getFormTypeForInstance(defaultFormInfo).defaultEditForm.id();
	        }



	        /**
            * @ngdoc method
            * @name getFormForDefinition
            * @description Given an typeId, get the form 
            *
            * @methodOf mod_app.editFormCache:editFormCache
            */
	        exports.getFormForDefinition = function (selectedDefIdOrAlias, skipModificationCheck) {

	            // are we already asking for it?
	            var pendingRequest = pendingDefRequests[selectedDefIdOrAlias];

	            if (pendingRequest) {
	                return pendingRequest;
	            }

	            // do we have it in the cache?
	            var cachedFormID = typeFormCache.get(selectedDefIdOrAlias);

	            if (cachedFormID) {
	                return exports.getFormDefinition(cachedFormID, skipModificationCheck);
	            }

	            else { // Better go get it.

	                var request = getFormIdForDefinition(selectedDefIdOrAlias)
                        .then(function (formId) {
                            if (formId) {
                                typeFormCache.put(selectedDefIdOrAlias, formId);
                                return exports.getFormDefinition(formId, skipModificationCheck);
                            } else {
                                return generateForm(selectedDefIdOrAlias);   // force it to generate
                            }
                        })
                        .finally(function () {
                            pendingDefRequests[selectedDefIdOrAlias] = undefined;
                        });

	                pendingDefRequests[selectedDefIdOrAlias] = request;

	                return request;

	            }
            };
	

            /**
            * @ngdoc method
            * @name remove
            * @description Remove the given entry from the cache. 
            *
            * @methodOf mod_app.editFormCache:editFormCache
            */
	        exports.remove = function (key) {
	            formCache.remove(key);      // might need to deal with id v/s a26
	            formVisCalcDependencyCache.remove(key);
	        };


            /**
            * @ngdoc method
            * @name remove
            * @description Flush the entire cache 
            *
            * @methodOf mod_app.editFormCache:editFormCache
            */
	        exports.removeAll = function () {
	            formCache.removeAll();      // might need to deal with id v/s a26
	            typeFormCache.removeAll();      // might need to deal with id v/s a26
	            instanceFormCache.removeAll();
	            formVisCalcDependencyCache.removeAll();
	            invalidatingEntityToFormCache = {};

	            pendingFormRequests = {};
	            pendingInstanceRequests = {};
	            pendingDefRequests = {};
	            pendingGenRequests = {};
	            pendingVisCalcRequests = {};
	        };

	        exports.getFormVisCalcDependencies = function (formId) {
                // are we already asking for it?
                var pendingRequest = pendingVisCalcRequests[formId];

                if (pendingRequest) {
                    return pendingRequest;
                }

                // do we have it in the cache
                var cacheEntry = formVisCalcDependencyCache.get(formId);
                if (cacheEntry && cacheEntry.data) {
                    return $q.when(cacheEntry.data);
                }

	            // Not in cache. Get it.                

	            var request = editFormWebServices.getFormVisCalcDependencies(formId).then(function(visibilityCalcDependencies) {
	                    putFormVisCalcDependencies(formId, visibilityCalcDependencies);
	                    return visibilityCalcDependencies;
	                })
	                .finally(function() {
	                    pendingVisCalcRequests[formId] = undefined;
	                });

	            pendingVisCalcRequests[formId] = request;

	            return request;
	        };

	        // Given an instance id or alias, return the default form for it.
	        function getFormIdForDefinition(idOrAlias) {
	            return spEntityService.getEntity(idOrAlias, 'console:defaultEditForm.id', { hint: 'getFormIdForDefinition', batch: true })
                .then(function (entity) {
                    return spUtils.result(entity, 'defaultEditForm.id');
                });
	        }

            // generate a form
	        function generateForm(typeId) {
	            var pendingRequest = pendingGenRequests[typeId];

	            if (pendingRequest) {
	                return pendingRequest;
	            }

	            pendingRequest = editFormWebServices.getFormForDefinition(typeId, true)
                    .then(function (form) {
                        var formId = form.id();
                        putForm(formId, form, true);
                        typeFormCache.put(typeId, formId);
                        return form;
                    })
	            .finally(function () {
	                pendingGenRequests[typeId] = undefined;
	            });

	            pendingGenRequests[typeId] = pendingRequest;

	            return pendingRequest;
	        }

            // clear cache if new user has logged in
	        function handleCacheReset() {
	            var previousAccountId = spLocalStorage.getItem('formCachePreviousAccountId');
	            var currentAccountId = '' + spLoginService.accountId; 

	           if (previousAccountId !== currentAccountId) {
	                spLocalStorage.setItem('formCachePreviousAccountId', currentAccountId); // add key to local storage
	                exports.removeAll();
	            }
	        }
	        
	        $rootScope.$on('signedin', function () {
	            handleCacheReset();
	        });

            // in some cases when login is complete and 'signedin' message is raised, editFormcache may not have been initialized. so do it when cache is first initialized.
	        handleCacheReset();

            return exports;
	    });



}());