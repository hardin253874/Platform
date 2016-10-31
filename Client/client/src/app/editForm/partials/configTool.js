// Copyright 2011-2016 Global Software Innovation Pty Ltd
 

function configToolController($scope) {
    
    // given an form control instance, return its configuration for (if one exists), 
    function getConfigForm(formControlInstance) {

        var type, form, editFormRels;

        type = formControlInstance.getIsOfType()[0];
        editFormRels = type.getRelationship('console:defaultEditForm');
        form = editFormRels.length > 0 ? editFormRels[0] : null;
        return form;
    }
    
        
    var configForm;

    $scope.configForm = getConfigForm($scope.formControl);
    $scope.showTool = $scope.isInDesigner && $scope.configForm;

    $scope.modalInfo = {isOpen:false};
    
    $scope.openModal = function () {
        $scope.modalInfo.formData = $scope.formControl;
        $scope.modalInfo.formControl = $scope.configForm;
        $scope.modalInfo.isOpen = true;
    };

    $scope.closeModal = function () {
        $scope.formControl = $scope.modalInfo.formData;

        $scope.modalInfo.isOpen = false;
        $scope.modalInfo.formControl = null;
        $scope.modalInfo.formData = null;
    };
}
