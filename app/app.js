'use strict';

var app = angular.module('angularOchsApp', ["ngRoute"]);
//app.value('backendServerUrl', 'http://localhost/test/');
app.filter('timespan', function () {
    return function (seconds) {
        if (isNaN(parseFloat(seconds)) || !isFinite(seconds)) {
            seconds = 0.0;
        }
        var s = seconds % 60;
        var m = parseInt(seconds / 60);
        return m + ':' + (s < 10 ? '0' : '') + parseFloat(Math.round(s * 100) / 100).toFixed(2);
    };
});
app.config(['$routeProvider',
    function ($routeProvider) {
        $routeProvider.
            when('/Welcome', {
                templateUrl: 'templates/Welcome.html',
                controller: 'WelcomeController'
            }).
            when('/Login', {
                templateUrl: 'templates/Login.html',
                controller: 'LoginController'
            }).
            when('/ListUsers', {
                templateUrl: 'templates/ListUsers.html',
                controller: 'ListUsersController'
            }).
            when('/EditUser/:userId', {
                templateUrl: 'templates/EditUser.html',
                controller: 'EditUserController'
            }).
            when('/ListOrganizations', {
                templateUrl: 'templates/ListOrganizations.html',
                controller: 'ListOrganizationsController'
            }).
            when('/ListCompetitions', {
                templateUrl: 'templates/ListCompetitions.html',
                controller: 'ListCompetitionsController'
            }).
            when('/ShowCompetition/:competitionId', {
                templateUrl: 'templates/ShowCompetition.html',
                controller: 'CompetitionController'
            }).
            when('/ShowCompetitionRules/:competitionId', {
                templateUrl: 'templates/ShowCompetitionRules.html',
                controller: 'CompetitionRulesController'
            }).
            when('/ShowPhase/:phaseId', {
                templateUrl: 'templates/ShowPhase.html',
                controller: 'PhaseController'
            }).
            when('/ShowPool/:poolId', {
                templateUrl: 'templates/ShowPool.html',
                controller: 'PoolController'
            }).
            when('/ListMatches', {
                templateUrl: 'templates/ListMatches.html',
                controller: 'ListMatchesController'
            }).
            when('/ShowMatch/:matchId', {
                templateUrl: 'templates/ShowMatch.html',
                controller: 'MatchController'
            }).
            when('/ScoreKeeper/:matchId', {
                templateUrl: 'templates/ScoreKeeper.html',
                controller: 'MatchController'
            }).
            when('/ShowPhaseElimination/:phaseId', {
                templateUrl: 'templates/ShowElimination.html',
                controller: 'EliminationController'
            }).
            when('/ShowPoolElimination/:poolId', {
                templateUrl: 'templates/ShowElimination.html',
                controller: 'EliminationController'
            }).
            when('/ShowPhaseRanking/:phaseId', {
                templateUrl: 'templates/ShowRanking.html',
                controller: 'RankingController'
            }).
            when('/ShowPoolRanking/:poolId', {
                templateUrl: 'templates/ShowRanking.html',
                controller: 'RankingController'
            }).
            otherwise({
                redirectTo: '/Welcome'
            });
    }]);
app.run(function() {
    
});