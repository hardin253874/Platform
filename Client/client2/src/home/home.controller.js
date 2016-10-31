export default class HomeController {
    constructor(entityService) {
        "ngInject";

        this.entityService = entityService;
        this.name = 'World !!!!';
    }

    changeName() {
        this.name = 'angular-tips';
        this.entityService.getEntity(222, 'name')
            .then(entity => {
                this.name = JSON.stringify(entity);
            });
    }
}

