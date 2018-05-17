﻿'use strict';

app.controller('OchsController', 
    function ($scope, $http) {
        console.log('trying to connect to service');
        $scope.ochsHub = $.connection.ochsHub;
        $scope.ochsHub.client.updateMatch = function (data) {
            $scope.$broadcast('updateMatch', data);
        };
        $scope.ochsHub.client.addMatch = function (data) {
            $scope.$broadcast('addMatch', data);
        };
        $scope.ochsHub.client.removeMatch = function (data) {
            $scope.$broadcast('removeMatch', data);
        };
        $scope.ochsHub.client.updateCompetition = function (data) {
            $scope.$broadcast('updateCompetition', data);
        };
        $scope.ochsHub.client.updatePhase = function (data) {
            $scope.$broadcast('updatePhase', data);
        };
        $scope.ochsHub.client.addPhase = function (data) {
            $scope.$broadcast('addPhase', data);
        };
        $scope.ochsHub.client.updatePool = function (data) {
            $scope.$broadcast('updatePool', data);
        };
        $scope.ochsHub.client.addPool = function (data) {
            $scope.$broadcast('addPool', data);
        };
        $scope.ochsHub.client.authorizationException = function (gotsession) {
            //$scope.
            $('#authorizationExceptionModal').modal('show');
        }
        $scope.ochsHub.client.displayMessage = function(message, messageType) {
            $('#messagePopupMessage').text(message);
            var popup = $('#messagePopup');
            popup.removeClass('alert-success');
            popup.removeClass('alert-warning');
            popup.removeClass('alert-danger');
            popup.removeClass('alert-info');
            popup.addClass('alert-' + messageType);
            // add the message element to the body, fadein, wait 3secs, fadeout
            popup.fadeIn(300).delay(3000).fadeOut(500);
        }
        $.connection.hub.start()
            .done(function () { console.log('Now connected, connection ID=' + $.connection.hub.id); })
            .fail(function () { console.log('Could not Connect!'); });
        console.log('connected to service');
        $.connection.hub.error(function (error) {
            console.log('SignalR error: ' + error);
        });
        $http.get("api/Auth/CurrentUser").then(function(response) {
            $scope.username = response.data;
        });
        $scope.reconnectSignalR = function() {
            $.connection.hub.stop();
            $.connection.hub.start().done(function () { console.log('Now connected, connection ID=' + $.connection.hub.id); })
                .fail(function () { console.log('Could not Connect!'); });
        }
    }
);

app.controller("WelcomeController", function ($scope, $http) {
});

app.controller("ListUsersController", function ($scope, $http) {
    $http.get("api/Auth/All")
        .then(function (response) { $scope.users = response.data; });
    $scope.createUser = function() {
        $http.post("api/Auth/Create", { Username: $scope.newUsername, Password: $scope.newPassword })
            .then(function (response) { $scope.users.push(response.data); });
    };
});

app.controller("EditUserController", function ($scope, $http, $routeParams) {
    $scope.userId = $routeParams.userId;
    $http.get("api/Auth/Get/" + $routeParams.userId).then(function (response) {
        $scope.currentUser = response.data;
    });
    $http.get("api/Organization/All").then(function (response) {
        $scope.organizations = response.data;
    });
    $scope.addUserRole = function() {
        $http.post("api/Auth/AddRole", { UserId: $scope.userId, Role: $scope.newRole, OrganizationName: $scope.newOrganization })
            .then(function (response) { $scope.currentUser.Roles.push(response.data); });
    };
    $scope.deleteUserRole = function($role) {
        $http.post("api/Auth/DeleteRole", { RoleId: $role.Id })
            .then(function(response) {
                 $scope.currentUser.Roles.splice($scope.currentUser.Roles.indexOf($role),1);
            });
    };
});

app.controller("ListOrganizationsController", function ($scope, $http) {
    $http.get("api/Organization/AllWithDetails").then(function (response) {
        $scope.organizations = response.data;
    });
    $scope.uploadOrganizations = function(organizationFile) {
        $http.post("api/Organization/UploadOrganizations", organizationFile)
            .then(function(response) {
                $scope.organizations = response.data;
            });
    }
});

app.controller("ListCompetitionsController", function ($scope, $http) {
    $http.get("api/Competition/All")
        .then(function (response) { $scope.competitions = response.data; });
    $http.get("api/Organization/All").then(function (response) {
        $scope.organizations = response.data;
    });
    $scope.createCompetition = function() {
        $http.post("api/Competition/Create", { CompetitionName: $scope.newCompetitionName, OrganizationName: $scope.newOrganizationName })
            .then(function (response) { $scope.competitions.push(response.data); });
    };
});
app.controller("CompetitionController", function ($scope, $http, $routeParams, $interval) {
    $scope.competitionId = $routeParams.competitionId;
    $http.get("api/Competition/Get/" + $routeParams.competitionId).then(function (response) {
        $scope.currentCompetition = response.data;
    });
    $http.get("api/Organization/All").then(function (response) {
        $scope.organizations = response.data;
    });
    $http.get("api/Country/All").then(function (response) {
        $scope.countries = response.data;
    });
    $scope.$on('updateCompetition', function (event, args) {
        if ($scope.competitionId === args.Id) {
            $scope.currentCompetition = args;
            $scope.$apply();
        }
    });
    $scope.$on('updateMatch', function (event, args) {
        if ($scope.currentCompetition && $scope.currentCompetition.Matches) {
            for (var i = 0; i < $scope.currentCompetition.Matches.length; i++) {
                if ($scope.currentCompetition.Matches[i].Id === args.Id) {
                    $scope.currentCompetition.Matches[i] = args;
                    $scope.$apply();
                }
            }
        }
    });
    $scope.$on('addMatch', function (event, args) {
        if ($scope.currentCompetition && $scope.currentCompetition.Id === args.CompetitionId) {
            $scope.currentCompetition.Matches.push(args);
            $scope.currentCompetition.MatchesTotal++;
            $scope.$apply();
        }
    });
    $scope.$on('removeMatch', function (event, args) {
        if ($scope.currentCompetition && $scope.currentCompetition.Matches) {
            for (var i = 0; i < $scope.currentCompetition.Matches.length; i++) {
                if ($scope.currentCompetition.Matches[i].Id === args) {
                    $scope.currentCompetition.Matches.splice(i, 1);
                    $scope.currentCompetition.MatchesTotal--;
                    $scope.$apply();
                }
            }
        }
    });
    $scope.$on('updatePhase', function (event, args) {
        if ($scope.currentCompetition && $scope.currentCompetition.Phases) {
            for (var i = 0; i < $scope.currentCompetition.Phases.length; i++) {
                if ($scope.currentCompetition.Phases[i].Id === args.Id) {
                    $scope.currentCompetition.Phases[i] = args;
                    $scope.$apply();
                }
            }
        }
    });
    $scope.$on('addPhase', function (event, args) {
        if ($scope.currentCompetition && $scope.currentCompetition.Id === args.CompetitionId) {
            $scope.currentCompetition.Phases.push(args);
            $scope.currentCompetition.PhasesTotal++;
            $scope.$apply();
        }
    });
    $scope.createCompetitionPhase = function () {
        $scope.$parent.ochsHub.invoke("CreateCompetitionPhase", $scope.competitionId, $scope.newPhaseName, $scope.newPhaseType, $scope.newPhaseLocationName);
    };
    $scope.competitionAddFighter = function () {
        $scope.$parent.ochsHub.invoke("CompetitionAddFighter", $scope.competitionId, $scope.newFighterFirstName, $scope.newFighterLastNamePrefix, $scope.newFighterLastName, $scope.newFighterOrganization, $scope.newFighterCountry);
    };
    $scope.competitionAddFight = function () {
        $scope.$parent.ochsHub.invoke("CompetitionAddFight", $scope.competitionId, $scope.newFightName, $scope.newFightPlanned, $scope.newFightBlueFighterId, $scope.newFightRedFighterId);
    };
    $scope.uploadFighters = function(fighterFile) {
        $http.post("api/Competition/UploadFighters/"+$scope.competitionId, fighterFile)
            .then(function(response) {
                $scope.currentCompetition = response.data;
            });
    }
    $scope.checkAll = function () {
        angular.forEach($scope.currentCompetition.Fighters, function (obj) {
            obj.Selected = $scope.select;
        });
    };
    $scope.competitionRemoveFighters = function() {
        var fighterIds = [];
        angular.forEach($scope.currentCompetition.Fighters, function (fighter) {
            if (fighter.Selected && fighter.MatchesTotal === 0) {
                fighterIds.push(fighter.Id);
            }
        });
        if (fighterIds.length > 0) {
            $scope.$parent.ochsHub.invoke("CompetitionRemoveFighters", $scope.competitionId, fighterIds);
        }
    }
    $scope.phaseAddFighters = function() {
        if ($scope.addFightersPhase) {
            var fighterIds = [];
            angular.forEach($scope.currentCompetition.Fighters, function (fighter) {
                if (fighter.Selected && fighter.MatchesTotal === 0) {
                    fighterIds.push(fighter.Id);
                }
            });
            if (fighterIds.length > 0) {
                $scope.$parent.ochsHub.invoke("PhaseAddFighters", $scope.addFightersPhase, fighterIds);
            }
        }
    }
});

app.controller("CompetitionRulesController", function ($scope, $http, $routeParams, $interval) {
    $scope.competitionId = $routeParams.competitionId;
    $http.get("api/Competition/GetRules/" + $routeParams.competitionId).then(function (response) {
        $scope.currentCompetition = response.data;
    });
});

app.controller("PhaseController", function ($scope, $http, $routeParams, $interval) {
    $scope.phaseId = $routeParams.phaseId;
    $http.get("api/Phase/Get/" + $routeParams.phaseId).then(function (response) {
        $scope.currentPhase = response.data;
    });
    $scope.$on('updatePhase', function (event, args) {
        if ($scope.phaseId === args.Id) {
            $scope.currentPhase = args;
            $scope.$apply();
        }
    });
    $scope.$on('updateMatch', function (event, args) {
        if ($scope.currentPhase && $scope.currentPhase.Matches) {
            for (var i = 0; i < $scope.currentPhase.Matches.length; i++) {
                if ($scope.currentPhase.Matches[i].Id === args.Id) {
                    $scope.currentPhase.Matches[i] = args;
                    $scope.$apply();
                }
            }
        }
    });
    $scope.$on('addMatch', function (event, args) {
        if ($scope.currentPhase && $scope.currentPhase.Id === args.PhaseId) {
            $scope.currentPhase.Matches.push(args);
            $scope.currentPhase.MatchesTotal++;
            $scope.$apply();
        }
    });
    $scope.$on('removeMatch', function (event, args) {
        if ($scope.currentPhase && $scope.currentPhase.Matches) {
            for (var i = 0; i < $scope.currentPhase.Matches.length; i++) {
                if ($scope.currentPhase.Matches[i].Id === args) {
                    $scope.currentPhase.Matches.splice(i, 1);
                    $scope.currentPhase.MatchesTotal--;
                    $scope.$apply();
                }
            }
        }
    });
    $scope.$on('updatePool', function (event, args) {
        if ($scope.currentPhase && $scope.currentPhase.Pools) {
            for (var i = 0; i < $scope.currentPhase.Pools.length; i++) {
                if ($scope.currentPhase.Pools[i].Id === args.Id) {
                    $scope.currentPhase.Pools[i] = args;
                    $scope.$apply();
                }
            }
        }
    });
    $scope.$on('addPool', function (event, args) {
        if ($scope.currentPhase && $scope.currentPhase.Id === args.PhaseId) {
            $scope.currentPhase.Pools.push(args);
            $scope.currentPhase.PoolsTotal++;
            $scope.$apply();
        }
    });
    $scope.createPhasePool = function () {
        $scope.$parent.ochsHub.invoke("CreatePhasePool", $scope.phaseId, $scope.newPoolName, $scope.newPoolLocationName, $scope.newPoolPlanned);
    };
    $scope.addAllFighters = function() {
        $scope.$parent.ochsHub.invoke("PhaseAddAllFighters", $scope.phaseId);
    }
    $scope.addTopFighters = function() {
        $scope.$parent.ochsHub.invoke("PhaseAddTopFighters", $scope.phaseId, $scope.topfighters);
    }
    $scope.distributeFighters = function() {
        $scope.$parent.ochsHub.invoke("PhaseDistributeFighters", $scope.phaseId);
    }
    $scope.generateMatches = function() {
        $scope.$parent.ochsHub.invoke("PhaseGenerateMatches", $scope.phaseId);
    }
    $scope.checkAll = function () {
        angular.forEach($scope.currentPhase.Fighters, function (obj) {
            obj.Selected = $scope.select;
        });
    };
    $scope.phaseRemoveFighters = function() {
        var fighterIds = [];
        angular.forEach($scope.currentPhase.Fighters, function (fighter) {
            if (fighter.Selected && fighter.MatchesTotal === 0) {
                fighterIds.push(fighter.Id);
            }
        });
        if (fighterIds.length > 0) {
            $scope.$parent.ochsHub.invoke("PhaseRemoveFighters", $scope.phaseId, fighterIds);
        }
    }
    $scope.poolAddFighters = function() {
        if ($scope.addFightersPool) {
            var fighterIds = [];
            angular.forEach($scope.currentPhase.Fighters, function (fighter) {
                if (fighter.Selected && fighter.MatchesTotal === 0) {
                    fighterIds.push(fighter.Id);
                }
            });
            if (fighterIds.length > 0) {
                $scope.$parent.ochsHub.invoke("PoolAddFighters", $scope.addFightersPool, fighterIds);
            }
        }
    }
});


app.controller("PoolController", function ($scope, $http, $routeParams, $interval) {
    $scope.poolId = $routeParams.poolId;
    $http.get("api/Pool/Get/" + $routeParams.poolId).then(function (response) {
        $scope.currentPool = response.data;
    });
    $scope.$on('updatePool', function (event, args) {
        if ($scope.poolId === args.Id) {
            $scope.currentPool = args;
            $scope.$apply();
        }
    });
    $scope.$on('updateMatch', function (event, args) {
        if ($scope.currentPool && $scope.currentPool.Matches) {
            for (var i = 0; i < $scope.currentPool.Matches.length; i++) {
                if ($scope.currentPool.Matches[i].Id === args.Id) {
                    $scope.currentPool.Matches[i] = args;
                    $scope.$apply();
                }
            }
        }
    });
    $scope.$on('addMatch', function (event, args) {
        if ($scope.currentPool && $scope.currentPool.Id === args.PoolId) {
            $scope.currentPool.Matches.push(args);
            $scope.currentPool.MatchesTotal++;
            $scope.$apply();
        }
    });
    $scope.$on('removeMatch', function (event, args) {
        if ($scope.currentPool && $scope.currentPool.Matches) {
            for (var i = 0; i < $scope.currentPool.Matches.length; i++) {
                if ($scope.currentPool.Matches[i].Id === args) {
                    $scope.currentPool.Matches.splice(i, 1);
                    $scope.currentPool.MatchesTotal--;
                    $scope.$apply();
                }
            }
        }
    });
    $scope.generateMatches = function() {
        $scope.$parent.ochsHub.invoke("PoolGenerateMatches", $scope.poolId);
    }
    $scope.checkAll = function () {
        angular.forEach($scope.currentPool.Fighters, function (obj) {
            obj.Selected = $scope.select;
        });
    };
    $scope.poolRemoveFighters = function() {
        var fighterIds = [];
        angular.forEach($scope.currentPool.Fighters, function (fighter) {
            if (fighter.Selected && fighter.MatchesTotal === 0) {
                fighterIds.push(fighter.Id);
            }
        });
        if (fighterIds.length > 0) {
            $scope.$parent.ochsHub.invoke("PoolRemoveFighters", $scope.poolId, fighterIds);
        }
    }
});

app.controller("EliminationController", function ($scope, $http, $routeParams, $interval, $location) {
    var eliminationUrl = "";
    var url = "";
    if ($routeParams.poolId) {
        eliminationUrl = "api/Pool/GetElimination/" + $routeParams.poolId;
        url = "api/Pool/Get/" + $routeParams.poolId;
    }else if ($routeParams.phaseId) {
        eliminationUrl = "api/Phase/GetElimination/" + $routeParams.phaseId;
        url = "api/Phase/Get/" + $routeParams.phaseId;
    } else {
        return;
    }
    $http.get(url).then(function(response) {
        $scope.current = response.data;
    });
    $http.get(eliminationUrl).then(function (response) {
        $scope.currentElimination = response.data;
        var bracketdata = {
            teams: [],
            results: [[]]
        };
        for (var i = 0; i < response.data.Fighters.length; i += 2) {
            bracketdata.teams.push([response.data.Fighters[i], response.data.Fighters[i+1]]);
        }
        for (var i = 0; i < response.data.Matches.length; i++) {
            bracketdata.results[0].push([]);
            for (var j = 0; j < response.data.Matches[i].length; j++) {
                if (response.data.Matches[i][j]){
                    if (response.data.Matches[i][j].Finished) {
                        bracketdata.results[0][i].push([
                            response.data.Matches[i][j].ScoreBlue, response.data.Matches[i][j].ScoreRed,
                            response.data.Matches[i][j]
                        ]);
                    } else {
                        bracketdata.results[0][i].push([
                            null, null,
                            response.data.Matches[i][j]
                        ]);
                    }
                } else {
                    bracketdata.results[0][i].push([
                        null, null, null
                    ]);
                }
            }
        }
        $('#bracket').bracket({
            init: bracketdata, 
            onMatchClick: function(data) {
                if (data) {
                    if (data.Finished) {

                    } else {
                        $location.path("ScoreKeeper/"+data.Id);
                    }
                    $scope.$apply();
                } else {

                }
            },
            decorator: {
                edit: function () { }, render: function(container, data, score, state) {
                    switch(state) {
                    case "empty-bye":
                        container.append("No fighter");
                        container.parent().addClass("bye");
                        return;
                    case "empty-tbd":
                        container.append("Upcoming");
                        container.parent().addClass("tbd");
                        return;
 
                    case "entry-default-win":
                        container.parent().addClass("bye");
                    case "entry-no-score":
                    case "entry-complete":
                        if (data.CountryCode) {
                            container.append('<img src="Content/flags/' + data.CountryCode + '.svg" width="27" height="18" /> ');
                        } else {
                            container.append('<img src="Content/flags/none.svg" width="27" height="18" /> ');
                        }
                        container.append(data.DisplayName);
                        //container.append(data);
                        return;
                    }
                }},
            teamWidth: 200
        });
    });
    $scope.$on('updateMatch', function (event, args) {
        if ($scope.currentPhase && $scope.currentPhase.Matches) {
            for (var i = 0; i < $scope.currentPhase.Matches.length; i++) {
                if ($scope.currentPhase.Matches[i].Id === args.Id) {
                    $scope.currentPhase.Matches[i] = args;
                    $scope.$apply();
                }
            }
        }
    });
});
app.controller("RankingController",
    function($scope, $http, $routeParams, $interval) {
        var url = "";
        if ($routeParams.poolId) {
            url = "api/Pool/GetRanking/" + $routeParams.poolId;
        } else if ($routeParams.phaseId) {
            url = "api/Phase/GetRanking/" + $routeParams.phaseId;
        } else {
            return;
        }
        $http.get(url).then(function(response) {
            $scope.rankings = response.data;
        });
    });

app.controller("ListMatchesController", function ($scope, $http) {
    $http.get("api/Match/All")
        .then(function (response) { $scope.matches = response.data; });
});

app.controller("LoginController", function ($scope, $location, $http) {
    $scope.login = function (e) {
        $http.post("api/Auth/Login", { Username: $scope.username, Password: $scope.password }).then(function(result) {
            if (result) {
                $scope.$parent.username = $scope.username;
                $scope.$parent.reconnectSignalR();
                $location.path("Welcome");
            }
        });
    };
});

app.controller("MatchController", function ($scope, $http, $routeParams, $interval, $location) {
    $scope.matchId = $routeParams.matchId;
    $http.get("api/Match/Get/" + $routeParams.matchId).then(function (response) {
        $scope.currentMatch = response.data;
        $scope.matchResult = response.data.Result;
        $scope.editTime = new Date(1970, 1, 1, 0, 0, response.data.LiveTime);
    });
    $scope.$on('updateMatch', function (event, args) {
        if ($scope.matchId === args.Id) {
            if (!$scope.currentMatch.Finished && args.Finished) {
                if (args.PoolId) {
                    $location.path("ShowPool/" + args.PoolId);
                } else if (args.PhaseId) {
                    $location.path("ShowPhase/" + args.PhaseId);
                } else if (args.CompetitionId) {
                    $location.path("ShowCompetition/"+args.CompetitionId);
                } else {
                    $location.path("ListCompetitions");
                }
            }
            $scope.currentMatch = args;
            $scope.editTime = new Date(1970, 1, 1, 0, 0, args.LiveTime);
        }
    });
    $interval(function () {
        if ($scope.currentMatch && $scope.currentMatch.TimeRunning) {
            $scope.currentMatch.LiveTime += 0.01;
            $scope.editingTime = false;
        }
    }, 10);
    $scope.addMatchEvent = function($pointsBlue,$pointsRed,$eventtype) {
        $scope.$parent.ochsHub.invoke("AddMatchEvent", $scope.matchId, $pointsBlue, $pointsRed, $eventtype);
    }
    $scope.undoLastMatchEvent = function() {
        $scope.$parent.ochsHub.invoke("UndoLastMatchEvent", $scope.matchId);
    }
    $scope.startTime = function() {
        if ($scope.editingTime) {
            $scope.changeEditTime();
        }
        $scope.$parent.ochsHub.invoke("StartTime", $scope.matchId);
    }
    $scope.stopTime = function() {
        $scope.$parent.ochsHub.invoke("StopTime", $scope.matchId);
    }
    $scope.changeEditTime = function() {
        if ($scope.editingTime) {
            $scope.$parent.ochsHub.invoke("SetTimeMilliSeconds", $scope.matchId,  $scope.editTime - new Date(1970, 1, 1, 0, 0, 0));
            $scope.editingTime = false;
        } else if(!$scope.currentMatch.TimeRunning){
            $scope.editingTime = true;
        }
    }
    $scope.setMatchResult = function() {
        if ($scope.matchResult)
            $scope.$parent.ochsHub.invoke("SetMatchResult", $scope.matchId, $scope.matchResult);
    }


});