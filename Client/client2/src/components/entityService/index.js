import angular from 'angular';
import EntityService from './entity.service';

export default angular.module('components.entityService', [])
    .service('entityService', EntityService)
    .name;
