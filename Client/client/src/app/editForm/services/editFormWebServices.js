// Copyright 2011-2016 Global Software Innovation Pty Ltd


/**
* @ngdoc module
* @name editForm.service:spEditForm
* 
*  @description The custom web services used by the edit form.
*/

(function	()	{
	'use strict';

	angular.module('mod.app.editFormWebServices', ['ng', 'mod.common.spEntityService' ]);


    
/**
* @ngdoc service
* @name mod_app.editFormServices:editFormServices
* 
*  @description This service is used by any Page that used the custom edit forms.
*/
    
	angular.module('mod.app.editFormWebServices').factory('editFormWebServices', function ($http, $q, spEntityService, spWebService) {


	var exports = {};



    function callFormService(url) {

        var args = { headers: spWebService.getHeaders() };
        return $http.get(url, args).then(
            function (result) {
                return spEntityService.extractNamedQueriesFromBatch(result).formEntity;
            });
    }

   
	    
    /**
      * @ngdoc method
      * @name getFormDefinition
      * @description Given a formId, get the form.
      *
      * @methodOf mod_app.editFormServices:editFormServices
      */
    exports.getFormDefinition = function (selectedFormIdOrAlias, isInDesignMode) {
        var url = spWebService.getWebApiRoot() + '/spapi/data/v1/form/' + spUtils.aliasOrIdUri(selectedFormIdOrAlias);

        if (isInDesignMode) {
            url += '?designmode=true';
        }

        return callFormService(url);
    };

	/**
     * @ngdoc method
     * @name removeFormsFromServerFormCache
     * @description Given a formId, get the form.
     *
     * @methodOf mod_app.editFormServices:editFormServices
     */
   exports.removeFormsFromServerFormCache = function (formIds) {
        return $q.when()
            .then(function() {
                return $http({
                    method: 'POST',
                    url: spWebService.getWebApiRoot() + '/spapi/data/v1/form/forcerefresh',
                    data: formIds,
                    headers: spWebService.getHeaders()
                });
            });
    };
    

	/**
    * @ngdoc method
    * @name getFormForInstance
    * @description Given an entityId, get the form 
    *
    * @methodOf mod_app.editFormServices:editFormServices
    */
    exports.getFormForInstance = function (selectedFormIdOrAlias, forceGenerate) {

        var url = spWebService.getWebApiRoot() + '/spapi/data/v1/instance/' + spUtils.aliasOrIdUri(selectedFormIdOrAlias);

        if (forceGenerate) {
            url += '?forceGenerate=true';
        }
        
        return callFormService(url);
    };
	    


	/**
    * @ngdoc method
    * @name getFormForDefinition
    * @description Given an typeId, get the form 
    *
    * @methodOf mod_app.editFormServices:editFormServices
    */
    exports.getFormForDefinition = function (selectedDefIdOrAlias, forceGenerate) {

        var url = spWebService.getWebApiRoot() + '/spapi/data/v1/type/' + spUtils.aliasOrIdUri(selectedDefIdOrAlias) + '/defaultForm';
        
        if (forceGenerate) {
            url += '?forceGenerate=true';
        }
        
        return callFormService(url);
    };

	exports.getFormDataAdvanced = function(eid, request, formId) {
	    var formRequestData = {
	        entityId: eid.toString(),
	        formId: formId.toString(),
	        query: request,
	        hint: "getFormDataAdvanced"
	    };

        var logTimeKey = 'getFormDataAdvanced(' + eid + ')';

        console.time(logTimeKey);

	    var url = spWebService.getWebApiRoot() + '/spapi/data/v1/form/data';

	    var args = { headers: spWebService.getHeaders() };
	    return $http.post(url, formRequestData, args).then(
	        function(response) {	                
	            var json = response.data;
	            var formDataEntity = _.first(spEntity.entityDataVer2ToEntities(json.formDataEntity));
                console.timeEnd(logTimeKey);
	            return {
	                formDataEntity: formDataEntity,
	                initiallyHiddenControls: json.initiallyHiddenControls	                
	            };
	        },
	        function (error) {
                console.timeEnd(logTimeKey);
                console.error('editFormWebServices.getFormDataAdvanced error: ' + (sp.result(error, 'status') || error));
                throw error;
	        });
	};

    exports.getFormVisCalcDependencies = function(formId) {
        var logTimeKey = 'getFormVisCalcDependencies(' + formId + ')';

        console.time(logTimeKey);

	    var url = spWebService.getWebApiRoot() + '/spapi/data/v1/form/visCalcDependencies/' + spUtils.aliasOrIdUri(formId);

	    var args = { headers: spWebService.getHeaders() };
	    return $http.get(url, args).then(
	        function(response) {	            
	            console.timeEnd(logTimeKey);
                if (response.data) {
                    return response.data.visibilityCalcDependencies;
                }

                return null;                
	        },
	        function (error) {
                console.timeEnd(logTimeKey);
                console.error('editFormWebServices.getFormVisCalcDependencies error: ' + (sp.result(error, 'status') || error));
                throw error;
	        });
    };

    return exports;
	});



}());