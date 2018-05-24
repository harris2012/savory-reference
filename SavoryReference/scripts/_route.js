var route = function ($stateProvider, $urlRouterProvider) {

    $urlRouterProvider.when('', '/welcome').when('/', '/welcome');

    $stateProvider.state('app', {
        url: '/'
    });

    $stateProvider.state('app.welcome', {
        url: 'welcome',
        templateUrl: 'scripts/view/view_welcome.html?v=' + window.version,
        controller: WelcomeController
    });
}