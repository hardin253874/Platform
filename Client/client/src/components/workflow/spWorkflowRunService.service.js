// Copyright 2011-2016 Global Software Innovation Pty Ltd
/*global _, angular, console, sp, spEntity, jsonLookup, jsonString, jsonBool, jsonInt, spWorkflowConfiguration, spWorkflow */

/**
 * Client side service to handle everything workflow. Layers on top of the Entity modules and services to
 * allow managing workflows and their underlying entity model.
 *
 * @module spWorkflowRunService
 */

(function () {
    'use strict';

    angular.module('mod.services.workflowRunService')
        .factory('spWorkflowRunService', spWorkflowRunService);

    /* @ngInject */
    function spWorkflowRunService( $http, spEntityService, spWebService, spPromiseService) {


        //`
        // Constants (These should probably be obtained from the server)
        //
        var WaitForWorkflowStop_Retries = 5 * 60 / 2;      // Wait 5 minutes maximum
        var WaitForWorkflowStop_Pause = 500;        // Initial pause between polls
        var WaitForWorkflowStop_Backoff = 1.2;        // The multiplier to increase backoff after each response.

        ///////////////////////////////////////////////////////////////////////
        // The interface
        //

        var exports = {
           
            runWorkflow: runWorkflow,
            getWorkflowRunResults: getWorkflowRunResults,
            waitForRunToStop: waitForRunToStop,
            waitForRunToStopWithThrow: waitForRunToStopWithThrow,
            cancelRun: cancelRun,
            waitForRunTimeoutMsg: "The workflow is taking longer than expected to complete, it has been marked as long running."
        };

        return exports;

        ///////////////////////////////////////////////////////////////////////
        // The implementation
        //

       
        /**
         * Run the workflow with the given id and using the given parameters.
         * @returns {promise} promise for the workflow run id
         */
        function runWorkflow(id, options, trace, timeoutOptions) {
            console.log('spWorkflowService: runWorkflow', id, options);

            var traceString = trace ? '?trace=true' : '';

            return callWorkflowUrl('run', id + traceString, 'POST', options).then(_.partialRight(pollTillHaveRunId, timeoutOptions));
        }



        /**
         * Request the latest status of the given report run. If tracing is enabled include trace information.
         * @returns {promise} promise for the workflow run result as a map with following entries:
         *   { date, results, status }, where the results are the raw report results and status is the
         *   extracted status string: 'Queued', 'Completed' etc.
         */
        function getWorkflowRunResults(id, includeTrace) {
            var query = 'name, runCompletedAt, workflowRunStatus.name, runStepCounter, errorLogEntry.{name, description}';

            if (includeTrace)
                query += ', runLog.{logEventTime, workflowRunTraceStep, name, description}';

            return spEntityService.getEntity(id, query, { batch: 'true', hint: 'getWorkflowRunResults' });
         }


       
        /**
         * Wait for the  workflowRun to stop.
         * @param workflowRun The workflow run to check
         * @param {object} options - options are cancel (default false), timeoutFn
         * cancel: If options is provided and cancel becomes true the polling will stop.
         * timeoutFn: a replacement for $timeout
         * @return a promise returning true if we did not time-out.
         */
        function waitForRunToStop(workflowRun, options) {
            var workflowRunId = spEntity.isEntity(workflowRun) ? workflowRun.id() : workflowRun;
            return pollTillRunStopped(workflowRunId, options);
        }

        /**
        * Wait for the  workflowRun to stop. Throw an error on timeout
        * @param workflowRun The workflow run to check
        * @param {object} options - members cancel (default false).
         * If options is provided and cancel becomes true the polling will stop.
        * @return a promise returning the workflowRun. Throws an error on timeout.
        */
        function waitForRunToStopWithThrow(workflowRun, options) {
            return  waitForRunToStop(workflowRun, options).then(function (result) {
                if (!result)
                    throw new Error(exports.waitForRunTimeoutMsg);

                return result;
            });
        }

        /*
        ** poll until the given taskId has been converted into a runId
        */
        function pollTillHaveRunId(taskId, options) {
            return pollTillTruthy(_.partial(getRunidfromTaskid, taskId), options);
        }

        /*
        ** Poll until the given run has stopped
        */
        function pollTillRunStopped(id, options) {
            return pollTillTruthy(_.partial(hasRunStopped, id), options);
        }

        function pollTillTruthy(fn, options) {
            var timeoutFn = null || (options && options.timeoutFn);
            return spPromiseService.poll(fn,
                function (r) {
                    return r || (options && options.cancel);
                },
                WaitForWorkflowStop_Retries,
                WaitForWorkflowStop_Pause,
                WaitForWorkflowStop_Backoff,
                timeoutFn);
        }

        function hasRunStopped(id) {
            return callWorkflowUrl('hasrunstopped', id, 'GET');
        }

             
        /*
        ** Cancel the given workflow run
        */
        function cancelRun(id) {
            return callWorkflowUrl('cancelRun', id, 'GET');

        }

        /*
        ** Get the runId from the taskId
        */
        function getRunidfromTaskid(taskId) {
            return callWorkflowUrl('runIdFromTaskId', taskId, 'GET'); 
        }

        /*
        ** Call the named workflow service
        ** data is optional
        */
        function callWorkflowUrl(name, id, method, data) {
            return $http({
                method: method,
                url: getWorkflowUrl(name, id),
                data: data,
                headers: spWebService.getHeaders()
            }).then(function (response) {
                return response.data;
            });
        }

        /*
        ** Get the url for the workflow service call
        */
        function getWorkflowUrl(name, id) {
            return spWebService.getWebApiRoot() + '/spapi/data/v1/workflow/' + name + '/' + id;
        }
    }

})();


