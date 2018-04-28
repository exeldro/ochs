'use strict';

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
    //$scope.newFightPlanned = Date();
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
            $scope.$apply();
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
            $scope.$apply();
        }
    });
    $scope.createCompetitionPhase = function () {
        $scope.$parent.ochsHub.invoke("CreateCompetitionPhase", $scope.competitionId, $scope.newPhaseName, $scope.newPhaseLocationName);
    };
    $scope.competitionAddFighter = function () {
        $scope.$parent.ochsHub.invoke("CompetitionAddFighter", $scope.competitionId, $scope.newFighterFirstName, $scope.newFighterLastNamePrefix, $scope.newFighterLastName, $scope.newFighterOrganization, $scope.newFighterCountry);
    };
    $scope.competitionAddFight = function () {
        $scope.$parent.ochsHub.invoke("CompetitionAddFight", $scope.competitionId, $scope.newFightName, $scope.newFightPhaseName, $scope.newFightPoolName, $scope.newFightPlanned, $scope.newFightBlueFighterId, $scope.newFightRedFighterId);
    };
    $scope.uploadFighters = function(fighterFile) {
        $http.post("api/Competition/UploadFighters/"+$scope.competitionId, fighterFile)
            .then(function(response) {
                $scope.currentCompetition = response.data;
            });
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
            $scope.$apply();
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
            $scope.$apply();
        }
    });
    $scope.createPhasePool = function () {
        $scope.$parent.ochsHub.invoke("CreatePhasePool", $scope.phaseId, $scope.newPoolName, $scope.newPoolLocationName);
    };
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
            $scope.$apply();
        }
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
                $location.path("ListCompetitions");
            }
        });
    };
});

app.controller("MatchController", function ($scope, $http, $routeParams, $interval, $location) {
    $scope.matchId = $routeParams.matchId;
    $http.get("api/Match/Get/" + $routeParams.matchId).then(function (response) {
        $scope.currentMatch = response.data;
        $scope.matchResult = response.data.Result;
    });
    $scope.$on('updateMatch', function (event, args) {
        if ($scope.matchId === args.Id) {
            if (!$scope.currentMatch.Finished && args.Finished) {
                /*if (args.PoolId) {
                    $location.path("ShowPool/" + args.PoolId);
                } else */if (args.PhaseId) {
                    $location.path("ShowPhase/" + args.PhaseId);
                } else if (args.CompetitionId) {
                    $location.path("ShowCompetition/"+args.CompetitionId);
                } else {
                    $location.path("ListCompetitions");
                }
            }
            $scope.currentMatch = args;
        }
    });
    $interval(function () {
        if ($scope.currentMatch && $scope.currentMatch.TimeRunning) {
            $scope.currentMatch.LiveTime += 0.01;
        }
    }, 10);
    $scope.addMatchEvent = function($pointsBlue,$pointsRed,$eventtype) {
        $scope.$parent.ochsHub.invoke("AddMatchEvent", $scope.matchId, $pointsBlue, $pointsRed, $eventtype);
    }
    $scope.undoLastMatchEvent = function() {
        $scope.$parent.ochsHub.invoke("UndoLastMatchEvent", $scope.matchId);
    }
    $scope.startTime = function() {
        $scope.$parent.ochsHub.invoke("StartTime", $scope.matchId);
    }
    $scope.stopTime = function() {
        $scope.$parent.ochsHub.invoke("StopTime", $scope.matchId);
    }
    $scope.setMatchResult = function() {
        if ($scope.matchResult)
            $scope.$parent.ochsHub.invoke("SetMatchResult", $scope.matchId, $scope.matchResult);
    }


});