'use strict';

var app = angular.module('angularOchsApp', ["ngRoute","ngCookies"]);
//app.value('backendServerUrl', 'http://localhost/test/');
app.filter('timespan', function () {
    return function (seconds) {
        if (isNaN(parseFloat(seconds)) || !isFinite(seconds)) {
            seconds = 0.0;
        }
        var s = seconds % 60;
        var m = parseInt(seconds / 60);
        return (m<10?'0':'')+m + ':' + (s < 10 ? '0' : '') + parseInt(s);
    };
});

app.filter('timespanmili', function () {
    return function (seconds) {
        if (isNaN(parseFloat(seconds)) || !isFinite(seconds)) {
            seconds = 0.0;
        }
        var s = seconds % 60;
        var m = parseInt(seconds / 60);
        return m + ':' + (s < 10 ? '0' : '') + parseFloat(Math.round(s * 100) / 100).toFixed(2);
    };
});

app.filter('range', function () {
    return function (input, start, end) {
        var direction;
        start = parseInt(start);
        end = parseInt(end);
        if (start === end) { return [start]; }
        direction = (start <= end) ? 1 : -1;
        while (start != end) {
            input.push(start);
            if (direction < 0 && start === end + 1) {
                input.push(end);
            }
            if (direction > 0 && start === end - 1) {
                input.push(end);
            }
            start += direction;
        }
        return input;
    };
});

app.config(['$routeProvider', '$locationProvider',
    function ($routeProvider, $locationProvider) {
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
            when('/ShowOrganization/:organizationId', {
	            templateUrl: 'templates/ShowOrganization.html',
	            controller: 'OrganizationController'
            }).
            when('/ListCompetitions', {
                templateUrl: 'templates/ListCompetitions.html',
                controller: 'ListCompetitionsController'
            }).
            when('/ShowCompetition/:competitionId', {
                templateUrl: 'templates/ShowCompetition.html',
                controller: 'CompetitionController'
            }).
            when('/ExportHemaRatings/:competitionId', {
                templateUrl: 'templates/ExportHemaRatings.html',
                controller: 'CompetitionController'
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
            when('/ShowCompetitionMatches/:competitionId', {
	            templateUrl: 'templates/ShowCompetitionMatches.html',
	            controller: 'CompetitionController'
            }).
            when('/ShowOrganizationMatches/:organizationId', {
	            templateUrl: 'templates/ShowOrganizationMatches.html',
	            controller: 'OrganizationController'
            }).
            when('/ShowPhaseMatches/:phaseId', {
	            templateUrl: 'templates/ShowPhaseMatches.html',
	            controller: 'PhaseController'
            }).
            when('/ShowPoolMatches/:poolId', {
	            templateUrl: 'templates/ShowPoolMatches.html',
	            controller: 'PoolController'
            }).
            when('/ShowMatch/:matchId', {
                templateUrl: 'templates/ShowMatch.html',
                controller: 'MatchController'
            }).
            when('/ShowMatchOverlay/:matchId', {
                templateUrl: 'templates/ShowMatchOverlay.html',
                controller: 'MatchController'
            }).
            when('/ShowEmptyOverlay', {
                templateUrl: 'templates/ShowMatchOverlay.html',
                controller: 'MatchController'
            }).
            when('/ShowLocationOverlay/:location', {
                templateUrl: 'templates/ShowMatchOverlay.html',
                controller: 'MatchController'
            }).
            when('/ScoreKeeper/:matchId', {
                templateUrl: 'templates/ScoreKeeper.html',
                controller: 'MatchController'
            }).
            when('/EditMatch/:matchId', {
                templateUrl: 'templates/EditMatch.html',
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
            when('/ViewSettings', {
                templateUrl: 'templates/ViewSettings.html',
                controller: 'ViewSettingsController'
            }).
            when('/ShowLocation', {
                templateUrl: 'templates/ShowLocation.html',
                controller: 'MatchController'
            }).
            when('/EditPerson/:personId', {
                templateUrl: 'templates/EditPerson.html',
                controller: 'EditPersonController'
            }).
            when('/EditMatchRules', {
                templateUrl: 'templates/EditMatchRules.html',
                controller: 'EditMatchRulesController'
            }).
            when('/EditRankingRules', {
                templateUrl: 'templates/EditRankingRules.html',
                controller: 'EditRankingRulesController'
            }).
            when('/ListRules', {
                templateUrl: 'templates/ListRules.html',
                controller: 'ListRulesController'
            }).
            when('/EditMatchRules/:matchRulesId', {
                templateUrl: 'templates/EditMatchRules.html',
                controller: 'EditMatchRulesController'
            }).
            when('/EditRankingRules/:rankingRulesId', {
                templateUrl: 'templates/EditRankingRules.html',
                controller: 'EditRankingRulesController'
            }).
            otherwise({
                redirectTo: '/Welcome'
            });
    }]);
app.run(function() {
    
});