// Copyright 2011-2016 Global Software Innovation Pty Ltd

(function () {

    class EditForm {
        constructor($scope, rnEditFormDndService) {
            'ngInject';

            this.$scope = $scope;
            this.rnEditFormDndService = rnEditFormDndService;

            $scope.$watch('$ctrl.form.entityChangeCounter', () => {
                // console.log('EntityForm $watch changeCounter', $scope.$id, (this.form || {}).debugString);
            });
        }

        $onInit() {
            // expect this to be here... don't render this component until it is ready
            console.assert(this.form);

            this.name = 'Entity Form';
            this.dropOptions = this.rnEditFormDndService.getDropOptions(this.form);
            this.childControlOptions = {noTitle: true};
        }

        onDragEnter(e, se, te, sd, td) {
            const targetControl = sp.result(angular.element(e.target), 'scope.$ctrl.control') || {};
            const sourceControl = sp.result(angular.element(se), 'scope.$ctrl.control') || {};
        }

        onDragLeave(e, se, te, sd, td) {
        }

        onDrop(e, se, te, sd, td) {
        }
    }

    angular.module('app.editFormComponents')
        .component('rnEditForm', {
            bindings: {
                resource: '<', form: '<',
                options: '<', formOptions: '<'
            },
            templateUrl: 'editForm/components/editForm/rnEditForm.tpl.html',
            controller: EditForm
        });

})();