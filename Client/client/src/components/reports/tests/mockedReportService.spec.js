// Copyright 2011-2016 Global Software Innovation Pty Ltd
angular.module('mockedReportService', ['ng'])
    .factory('spReportService', function ($q) {
        /**
         *  A set of client side services working against the report webapi service.
         *  @module spReportService
         */
        var exports = {};    
    
        var getReportDataMocks = {};  


        /* This method sets the report data to be returned by the getReportData method
        whenever a report by that id is requested. */
        exports.mockGetReportData = function (id, reportData) {
            getReportDataMocks[id] = reportData;
            return this;
        };


        /* Mocked implementation of the getReportData method */
        exports.getReportData = function (reportId, options) {
            var data = getReportDataMocks[reportId];
            if (!data)
                throw new Error('No mock data was provided for ' + reportId);        

            var deferred = $q.defer();
            deferred.resolve(data);

            return deferred.promise;
        };

        /* Mocked implementation of the getColumnFormatFunc method */
        exports.getColumnFormatFunc = function () {
            return _.identity;
        };

        return exports;
    });

