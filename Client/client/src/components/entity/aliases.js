// Copyright 2011-2016 Global Software Innovation Pty Ltd

/*
 * The intent is to have a set of 'constants' rather than strings that can be used in
 * code when we need to use aliases.
 * The structure below allows access in plain ol' js code as well as in AngularJS
 * contexts where we can use DI.
 * I've only added those used in new code.
 */

var spEntity;


var EdcEntity;  // legacy
(function (spEntity) {

spEntity.aliases = {
    name: 'name',

    instancesOfType: 'instancesOfType',
    isOfType: 'isOfType',

    isTopMenuVisible: 'console:isTopMenuVisible',
    showApplicationTabs: 'console:showApplicationTabs',
    consoleOrder: 'console:consoleOrder',
    folderContents: 'console:folderContents',
    resourceInFolder: 'console:resourceInFolder',
    resourceConsoleBehavior: 'console:resourceConsoleBehavior',
    typeConsoleBehavior: 'console:typeConsoleBehavior'
};

angular.module('sp.entityAliases', []).value('entityAliases', spEntity.aliases);

})(spEntity || (spEntity = EdcEntity = {}));
