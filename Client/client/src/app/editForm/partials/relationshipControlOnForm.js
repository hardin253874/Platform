// Copyright 2011-2016 Global Software Innovation Pty Ltd
   
angular.module('app.editForm.relationshipControlOnForm', ['mod.app.editForm'])
    .controller('relationshipControlOnForm',
        function ($scope, spEditForm) {
            // $scope.selectedResourceName = 'Other thing';
            
            // init
            spEditForm.commonRelFormControlInit($scope);

            $scope.selectedResourceName = '';
            // MAY NEED TO DELETE THIS CONTROL IF IT IS NOT BEEN USED
        }
    );
