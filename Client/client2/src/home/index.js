import angular from 'angular';
import uirouter from 'angular-ui-router';
import entityService from '../components/entityService';

import routing from './home.routes';
import HomeController from './home.controller';

export default angular.module('app.home', [uirouter, entityService])
    .config(routing)
    .controller('HomeController', HomeController)
    .name;
