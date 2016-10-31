// Copyright 2011-2016 Global Software Innovation Pty Ltd
/*global _, angular, console, sp, spEntity */

angular.module('sp.common.spEntityHelper', ['sp.common.spDialog', 'mod.common.spEntityService', 'mod.common.alerts', 'mod.common.ui.spDeleteService']);

/**
 *  A set of client side services working against the entity webapi service.
 *  @module spReportService
 */

angular.module('sp.common.spEntityHelper').factory('spEntityHelper', function (spDialog, spEntityService, spAlertsService, spDeleteService) {
    'use strict';
    var exports = {};

    /**
    * Prompts the user to delete a resource.
    * Returns a promise. True if deleted, otherwise false.
    * @returns N/A
    */
    exports.promptDelete = function (options) {
        // Valid options:
        // .entity  (with name)

        var name = options.entity.name;
        var id = options.entity.idP;

        var btns = [
            { result: true, label: 'OK' },
            { result: false, label: 'Cancel' }
        ];

        var dlgOptions = {
            title: 'Confirm delete',
            message: 'Are you sure that you want to delete \'' + name + '\'?',
            ids: [id],
            btns: btns
        };

        return spDeleteService.showDialog(dlgOptions).then(function (result) {
            if (!result)
                return false;

            return spEntityService.deleteEntity(id).then(function() {
                spAlertsService.addAlert(name + ' deleted', { severity: spAlertsService.sev.Info, expires: true });
                return true;
            }, function (err) {
                var message = err.status === 403 ? 'You do not have permission to delete ' + name : name + ' failed to deleted';
                spAlertsService.addAlert(message, { severity: spAlertsService.sev.Error, expires: false });
                return false;
            });
        });
    };

    return exports;

});
