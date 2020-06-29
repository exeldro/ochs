'use strict';

app.controller('OchsController',
    function ($scope, $http, $location, $cookies) {
        console.log('trying to connect to service');
        $scope.viewer = ($cookies.get('viewer') === 'true');
        $scope.highcontrast = ($cookies.get('highcontrast') === 'true');
        $scope.mirror = ($cookies.get('mirror') === 'true');
        $scope.autoscroll = ($cookies.get('autoscroll') === 'true');
        if ($scope.autoscroll) {
            $scope.scrollingUp = false;
            $scope.scrollFunction = function (duration) {
                setTimeout(function () {
                    var scrollHeight = $(document).height() - $(window).height();
                    if (scrollHeight <= 0) {
                        $scope.scrollFunction(1000);
                        return;
                    }
                    var scrollDuration = scrollHeight * 25;
                    if ($scope.scrollingUp) {
                        $('html, body').animate({ scrollTop: 0 }, scrollDuration);
                    } else {
                        $("html, body").animate({ scrollTop: scrollHeight }, scrollDuration);
                    }
                    $scope.scrollingUp = !$scope.scrollingUp;
                    $scope.scrollFunction(scrollDuration + 2000);
                }, duration);
            };
            $scope.scrollFunction(2000);
        }
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
        $scope.ochsHub.client.updateRankings = function (data) {
            $scope.$broadcast('updateRankings', data);
        };
        $scope.ochsHub.client.showMatchOnLocation = function (matchId, location) {
	        if ($cookies.get('location') && location === $cookies.get('location') && $cookies.get('viewer') === 'true') {
		        $location.path("ShowMatch/" + matchId);
		        $scope.$apply();
	        }
        };
        $scope.ochsHub.client.showPoolRankingOnLocation = function (poolId, location) {
	        if ($cookies.get('location') && location === $cookies.get('location') && $cookies.get('viewer') === 'true'){
		        $location.path("ShowPoolRanking/" + poolId);
		        $scope.$apply();
	        }
        };
        $scope.ochsHub.client.showPhaseRankingOnLocation = function (phaseId, location) {
	        if ($cookies.get('location') && location === $cookies.get('location') && $cookies.get('viewer') === 'true'){
		        $location.path("ShowPhaseRanking/" + phaseId);
		        $scope.$apply();
	        }
        };
        $scope.ochsHub.client.showPoolEliminationOnLocation = function (poolId, location) {
	        if ($cookies.get('location') && location === $cookies.get('location') && $cookies.get('viewer') === 'true'){
		        $location.path("ShowPoolElimination/" + poolId);
		        $scope.$apply();
	        }
        };
        $scope.ochsHub.client.showPhaseEliminationOnLocation = function (phaseId, location) {
	        if ($cookies.get('location') && location === $cookies.get('location') && $cookies.get('viewer') === 'true'){
		        $location.path("ShowPhaseElimination/" + phaseId);
		        $scope.$apply();
	        }
        };
        $scope.ochsHub.client.showPoolMatchesOnLocation = function (poolId, location) {
	        if ($cookies.get('location') && location === $cookies.get('location') && $cookies.get('viewer') === 'true'){
		        $location.path("ShowPoolMatches/" + poolId);
		        $scope.$apply();
	        }
        };
        $scope.ochsHub.client.showPhaseMatchesOnLocation = function (phaseId, location) {
	        if ($cookies.get('location') && location === $cookies.get('location') && $cookies.get('viewer') === 'true'){
		        $location.path("ShowPhaseMatches/" + phaseId);
		        $scope.$apply();
	        }
        };
        $scope.ochsHub.client.showCompetitionMatchesOnLocation = function (competitionId, location) {
	        if ($cookies.get('location') && location === $cookies.get('location') && $cookies.get('viewer') === 'true'){
		        $location.path("ShowCompetitionMatches/" + competitionId);
		        $scope.$apply();
	        }
        };
        $scope.ochsHub.client.authorizationException = function (gotsession) {
            //$scope.
            $('#authorizationExceptionModal').modal('show');
        };
        $scope.ochsHub.client.displayMessage = function (message, messageType) {
            $('#messagePopupMessage').text(message);
            var popup = $('#messagePopup');
            popup.removeClass('alert-success');
            popup.removeClass('alert-warning');
            popup.removeClass('alert-danger');
            popup.removeClass('alert-info');
            popup.addClass('alert-' + messageType);
            // add the message element to the body, fadein, wait 3secs, fadeout
            popup.fadeIn(300).delay(3000).fadeOut(500);
        };
        $.connection.hub.start()
            .done(function () { console.log('Now connected, connection ID=' + $.connection.hub.id); })
            .fail(function () { console.log('Could not Connect!'); });
        console.log('connected to service');
        $.connection.hub.error(function (error) {
            console.log('SignalR error: ' + error);
        });
        $scope.handleHttpResponse = function (response) {
            if (response) {
                if (response.data) {
                    if (response.data.Message) {
                        $scope.ochsHub.client.displayMessage("Error: " + response.data.Message, "danger");
                    } else {
                        $scope.ochsHub.client.displayMessage("Error: " + response.data, "danger");
                    }
                } else if (response.statusText) {
                    $scope.ochsHub.client.displayMessage("Error: " + response.statusText, "danger");
                } else {
                    $scope.ochsHub.client.displayMessage("Unknown error from server", "danger");
                }
            } else {
                $scope.ochsHub.client.displayMessage("Unknown error from server", "danger");
            }
        };
        $http.get("api/Auth/CurrentUser").then(function (response) {
            $scope.username = response.data;
        }, $scope.handleHttpResponse);
        $http.get("api/Auth/Version").then(function (response) {
            $scope.version = response.data;
        }, $scope.handleHttpResponse);
        $scope.reconnectSignalR = function () {
            $.connection.hub.stop();
            $.connection.hub.start().done(function () {
                console.log('Now connected, connection ID=' + $.connection.hub.id);
            })
                .fail(function () { console.log('Could not Connect!'); });
        };
    }
);

app.controller("WelcomeController", function ($scope, $http) {

});

app.controller("ViewSettingsController", function ($scope, $cookies) {
    $scope.location = $cookies.get('location');
    $scope.viewer = ($cookies.get('viewer') === 'true');
    $scope.highcontrast = ($cookies.get('highcontrast') === 'true');
    $scope.mirror = ($cookies.get('mirror') === 'true');
    $scope.apply = function () {
        $cookies.put('location', $scope.location);
        $cookies.put('viewer', $scope.viewer ? 'true' : 'false');
        $cookies.put('highcontrast', $scope.highcontrast ? 'true' : 'false');
        $scope.$parent.highcontrast = $scope.highcontrast;
        $cookies.put('mirror', $scope.mirror ? 'true' : 'false');
        $scope.$parent.mirror = $scope.mirror;
        $cookies.put('autoscroll', $scope.autoscroll ? 'true' : 'false');
        $scope.$parent.autoscroll = $scope.autoscroll;
    };
    $scope.fullscreen = function () {
        if (screenfull.isEnabled) {
            screenfull.toggle();
        }
    };
});

app.controller("ListUsersController", function ($scope, $http) {
    $http.get("api/Auth/All")
        .then(function (response) { $scope.users = response.data; }, $scope.$parent.handleHttpResponse);
    $scope.createUser = function () {
        $http.post("api/Auth/Create", { Username: $scope.newUsername, Password: $scope.newPassword })
            .then(function (response) { $scope.users.push(response.data); }, $scope.$parent.handleHttpResponse);
    };
});

app.controller("EditUserController", function ($scope, $http, $routeParams) {
    $scope.userId = $routeParams.userId;
    $http.get("api/Auth/Get/" + $routeParams.userId).then(function (response) {
        $scope.currentUser = response.data;
    }, $scope.$parent.handleHttpResponse);
    $http.get("api/Organization/All").then(function (response) {
        $scope.organizations = response.data;
    }, $scope.$parent.handleHttpResponse);
    $scope.addUserRole = function () {
        $http.post("api/Auth/AddRole", { UserId: $scope.userId, Role: $scope.newRole, OrganizationName: $scope.newOrganization })
            .then(function (response) { $scope.currentUser.Roles.push(response.data); }, $scope.$parent.handleHttpResponse);
    };
    $scope.deleteUserRole = function ($role) {
        $http.post("api/Auth/DeleteRole", { RoleId: $role.Id })
            .then(function (response) {
                $scope.currentUser.Roles.splice($scope.currentUser.Roles.indexOf($role), 1);
            }, $scope.$parent.handleHttpResponse);
    };
});

app.controller("EditPersonController", function ($scope, $http, $routeParams) {
    $scope.personId = $routeParams.personId;
    $http.get("api/Person/Get/" + $routeParams.personId).then(function (response) {
        $scope.currentPerson = response.data;
    }, $scope.$parent.handleHttpResponse);
    $http.get("api/Organization/All").then(function (response) {
        $scope.organizations = response.data;
    }, $scope.$parent.handleHttpResponse);
    $http.get("api/Country/All").then(function (response) {
        $scope.countries = response.data;
    }, $scope.$parent.handleHttpResponse);
    $scope.savePerson = function () {
        $http.post("api/Person/Save", $scope.currentPerson)
            .then(function (response) {
                $scope.currentPerson = response.data;
            }, $scope.$parent.handleHttpResponse);
    };
    $scope.addOrganization = function () {
        $http.post("api/Person/AddOrganization", { PersonId: $scope.currentPerson.Id, Organization: $scope.personOrganization })
            .then(function (response) {
                $scope.currentPerson = response.data;
            }, $scope.$parent.handleHttpResponse);
    };
    $scope.removeOrganization = function () {
        $http.post("api/Person/RemoveOrganization", { PersonId: $scope.currentPerson.Id, Organization: $scope.personOrganization })
            .then(function (response) {
                $scope.currentPerson = response.data;
            }, $scope.$parent.handleHttpResponse);
    };
});

app.controller("ListOrganizationsController", function ($scope, $http) {
    $http.get("api/Organization/AllWithDetails").then(function (response) {
        $scope.organizations = response.data;
    }, $scope.$parent.handleHttpResponse);
    $http.get("api/Country/All").then(function (response) {
        $scope.countries = response.data;
    }, $scope.$parent.handleHttpResponse);
    $scope.uploadOrganizations = function (organizationFile) {
        $http.post("api/Organization/UploadOrganizations", organizationFile)
            .then(function (response) {
                $scope.organizations = response.data;
            }, $scope.$parent.handleHttpResponse);
    };
    $scope.mergeOrganizations = function () {
        if (!$scope.mergeOrganizationToId || !$scope.mergeOrganizationFromId || $scope.mergeOrganizationToId === $scope.mergeOrganizationFromId)
            return;
        $http.post("api/Organization/MergeOrganizations", { FromId: $scope.mergeOrganizationFromId, ToId: $scope.mergeOrganizationToId })
            .then(function (response) {
                $scope.organizations = response.data;
            }, $scope.$parent.handleHttpResponse);
    };
    $scope.updateCountry = function () {
        var organizationIds = [];
        angular.forEach($scope.organizations,
            function (obj) {
                if (obj.Selected) {
                    organizationIds.push(obj.Id);
                }
            });
        if (organizationIds.length > 0) {
            $http.post("api/Organization/ChangeCountry", { OrganizationIds: organizationIds, CountryCode: $scope.countryCode })
                .then(function (response) {
                    $scope.organizations = response.data;
                }, $scope.$parent.handleHttpResponse);
        }
    };
});

app.controller("ListCompetitionsController", function ($scope, $http) {
    $http.get("api/Competition/All")
        .then(function (response) { $scope.competitions = response.data; }, $scope.$parent.handleHttpResponse);
    $http.get("api/Organization/All").then(function (response) {
        $scope.organizations = response.data;
    }, $scope.$parent.handleHttpResponse);
    $scope.createCompetition = function () {
        $http.post("api/Competition/Create", { CompetitionName: $scope.newCompetitionName, OrganizationName: $scope.newOrganizationName })
            .then(function (response) { $scope.competitions.push(response.data); }, $scope.$parent.handleHttpResponse);
    };
});
app.controller("CompetitionController", function ($scope, $http, $routeParams, $cookies) {
    $scope.competitionId = $routeParams.competitionId;
    $http.get("api/Competition/Get/" + $routeParams.competitionId).then(function (response) {
        $scope.currentCompetition = response.data;
        if ($scope.rights) {
            $scope.currentCompetition.Rights = $scope.rights;
        }
        $scope.currentCompetition.viewer = ($cookies.get('viewer') === 'true');
    }, $scope.$parent.handleHttpResponse);
    $http.get("api/Auth/CompetitionRights/" + $routeParams.competitionId).then(function (response) {
        $scope.rights = response.data;
        if ($scope.currentCompetition) {
            $scope.currentCompetition.Rights = response.data;
        }
    }, $scope.$parent.handleHttpResponse);
    $http.get("api/Organization/All").then(function (response) {
        $scope.organizations = response.data;
    });
    $http.get("api/Country/All").then(function (response) {
        $scope.countries = response.data;
    }, $scope.$parent.handleHttpResponse);
    $http.get("api/MatchRules/All").then(function (response) {
        $scope.matchRules = response.data;
    }, $scope.$parent.handleHttpResponse);
    $http.get("api/RankingRules/All").then(function (response) {
        $scope.rankingRules = response.data;
    }, $scope.$parent.handleHttpResponse);
    $scope.$on('updateCompetition', function (event, args) {
        if ($scope.competitionId === args.Id) {
            $scope.currentCompetition = args;
            if ($scope.rights) {
                $scope.currentCompetition.Rights = $scope.rights;
            }
            $scope.currentCompetition.viewer = ($cookies.get('viewer') === 'true');
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
        $scope.$parent.ochsHub.invoke("CompetitionAddFighter", $scope.competitionId, $scope.newFighterFirstName, $scope.newFighterLastNamePrefix, $scope.newFighterLastName, $scope.newFighterOrganization, $scope.newFighterCountry, $scope.newFighterSeed);
    };
    $scope.competitionAddFight = function () {
        $scope.$parent.ochsHub.invoke("CompetitionAddFight", $scope.competitionId, $scope.newFightName, $scope.newFightPlanned, $scope.newFightBlueFighterId, $scope.newFightRedFighterId);
    };
    $scope.uploadFighters = function (fighterFile) {
        $http.post("api/Competition/UploadFighters/" + $scope.competitionId, fighterFile)
            .then(function (response) {
                $scope.currentCompetition = response.data;
            }, $scope.$parent.handleHttpResponse);
    };
    $scope.checkAll = function () {
        angular.forEach($scope.currentCompetition.Fighters, function (obj) {
            obj.Selected = $scope.select;
        });
    };
    $scope.competitionRemoveFighters = function () {
        var fighterIds = [];
        angular.forEach($scope.currentCompetition.Fighters,
            function (fighter) {
                if (fighter.Selected && fighter.MatchesTotal === 0) {
                    fighterIds.push(fighter.Id);
                }
            });
        if (fighterIds.length > 0) {
            $scope.$parent.ochsHub.invoke("CompetitionRemoveFighters", $scope.competitionId, fighterIds);
        }
    };
    $scope.phaseAddFighters = function () {
        if ($scope.addFightersPhase) {
            var fighterIds = [];
            angular.forEach($scope.currentCompetition.Fighters,
                function (fighter) {
                    if (fighter.Selected) {
                        fighterIds.push(fighter.Id);
                    }
                });
            if (fighterIds.length > 0) {
                $scope.$parent.ochsHub.invoke("PhaseAddFighters", $scope.addFightersPhase, fighterIds);
            }
        }
    };
    $scope.competitionSaveMatchRules = function () {
        if ($scope.newMatchRules) {
            $scope.$parent.ochsHub.invoke("CompetitionSetMatchRules", $scope.competitionId, $scope.newMatchRules);
        }
    };
    $scope.competitionSaveRankingRules = function () {
        if ($scope.newRankingRules) {
            $scope.$parent.ochsHub.invoke("CompetitionSetRankingRules", $scope.competitionId, $scope.newRankingRules);
        }
    };
    $scope.showMatchesOnLocation = function () {
	    if (!$cookies.get('location'))
		    return;
	    $scope.$parent.ochsHub.invoke("ShowCompetitionMatchesOnLocation",
		    $routeParams.competitionId,
		    $cookies.get('location'));
    };
});

app.controller("PhaseController", function ($scope, $http, $routeParams, $cookies) {
    $scope.phaseId = $routeParams.phaseId;
    $http.get("api/Phase/Get/" + $routeParams.phaseId).then(function (response) {
        $scope.currentPhase = response.data;
        $http.get("api/Auth/CompetitionRights/" + $scope.currentPhase.CompetitionId).then(function (response) {
            $scope.rights = response.data;
            $scope.currentPhase.Rights = $scope.rights;
            $scope.currentPhase.viewer = ($cookies.get('viewer') === 'true');
        });
    }, $scope.$parent.handleHttpResponse);
    $http.get("api/MatchRules/All").then(function (response) {
        $scope.matchRules = response.data;
    }, $scope.$parent.handleHttpResponse);
    $scope.newMatchRules = '00000000-0000-0000-0000-000000000000';
    $scope.$on('updatePhase', function (event, args) {
        if ($scope.phaseId === args.Id) {
            $scope.currentPhase = args;
            if ($scope.rights) {
                $scope.currentPhase.Rights = $scope.rights;
            }
            $scope.currentPhase.viewer = ($cookies.get('viewer') === 'true');
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
    $scope.addAllFighters = function () {
        $scope.$parent.ochsHub.invoke("PhaseAddAllFighters", $scope.phaseId);
    };
    $scope.addTopFighters = function () {
        $scope.$parent.ochsHub.invoke("PhaseAddTopFighters", $scope.phaseId, $scope.topfighters);
    };
    $scope.distributeFighters = function () {
        $scope.$parent.ochsHub.invoke("PhaseDistributeFighters", $scope.phaseId);
    };
    $scope.generateMatches = function () {
        $scope.$parent.ochsHub.invoke("PhaseGenerateMatches", $scope.phaseId);
    };
    $scope.checkAll = function () {
        angular.forEach($scope.currentPhase.Fighters, function (obj) {
            obj.Selected = $scope.select;
        });
    };
    $scope.phaseRemoveFighters = function () {
        var fighterIds = [];
        angular.forEach($scope.currentPhase.Fighters,
            function (fighter) {
                if (fighter.Selected && fighter.MatchesTotal === 0) {
                    fighterIds.push(fighter.Id);
                }
            });
        if (fighterIds.length > 0) {
            $scope.$parent.ochsHub.invoke("PhaseRemoveFighters", $scope.phaseId, fighterIds);
        }
    };
    $scope.poolAddFighters = function () {
        if ($scope.addFightersPool) {
            var fighterIds = [];
            angular.forEach($scope.currentPhase.Fighters,
                function (fighter) {
                    if (fighter.Selected && fighter.MatchesTotal === 0) {
                        fighterIds.push(fighter.Id);
                    }
                });
            if (fighterIds.length > 0) {
                $scope.$parent.ochsHub.invoke("PoolAddFighters", $scope.addFightersPool, fighterIds);
            }
        }
    };
    $scope.phaseSaveMatchRules = function () {
        $scope.$parent.ochsHub.invoke("PhaseSetMatchRules", $scope.phaseId, $scope.newMatchRules ? $scope.newMatchRules : '00000000-0000-0000-0000-000000000000');
    };
    $scope.showMatchesOnLocation = function () {
	    if (!$cookies.get('location'))
		    return;
	    $scope.$parent.ochsHub.invoke("ShowPhaseMatchesOnLocation",
		    $routeParams.phaseId,
		    $cookies.get('location'));
    };
});


app.controller("PoolController", function ($scope, $http, $routeParams, $cookies) {
    $scope.poolId = $routeParams.poolId;
    $http.get("api/Pool/Get/" + $routeParams.poolId).then(function (response) {
        $scope.currentPool = response.data;
        $http.get("api/Auth/CompetitionRights/" + $scope.currentPool.CompetitionId).then(function (response) {
            $scope.rights = response.data;
            $scope.currentPool.Rights = $scope.rights;
            $scope.currentPool.viewer = ($cookies.get('viewer') === 'true');
        });
    }, $scope.$parent.handleHttpResponse);
    $scope.$on('updatePool', function (event, args) {
        if ($scope.poolId === args.Id) {
            $scope.currentPool = args;
            if ($scope.rights) {
                $scope.currentPool.Rights = $scope.rights;
            }
            $scope.currentPool.viewer = ($cookies.get('viewer') === 'true');
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
    $scope.generateMatches = function () {
        $scope.$parent.ochsHub.invoke("PoolGenerateMatches", $scope.poolId);
    }
    $scope.checkAll = function () {
        angular.forEach($scope.currentPool.Fighters, function (obj) {
            obj.Selected = $scope.select;
        });
    };
    $scope.poolRemoveFighters = function () {
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
    $scope.showMatchesOnLocation = function () {
	    if (!$cookies.get('location'))
		    return;
	    $scope.$parent.ochsHub.invoke("ShowPoolMatchesOnLocation",
		    $routeParams.poolId,
		    $cookies.get('location'));

    };
});

app.controller("EliminationController", function ($scope, $http, $routeParams, $interval, $location, $cookies) {
    var eliminationUrl;
    var url;
    if ($routeParams.poolId) {
        eliminationUrl = "api/Pool/GetElimination/" + $routeParams.poolId;
        url = "api/Pool/Get/" + $routeParams.poolId;
    } else if ($routeParams.phaseId) {
        eliminationUrl = "api/Phase/GetElimination/" + $routeParams.phaseId;
        url = "api/Phase/Get/" + $routeParams.phaseId;
    } else {
        return;
    }
    $http.get(url).then(function (response) {
        $scope.current = response.data;
    }, $scope.$parent.handleHttpResponse);
    $http.get(eliminationUrl).then(function (response) {
        $scope.currentElimination = response.data;
        var bracketdata = {
            teams: [],
            results: [[]]
        };
        for (var i = 0; i < response.data.Fighters.length; i += 2) {
            bracketdata.teams.push([response.data.Fighters[i], response.data.Fighters[i + 1]]);
        }
        for (var i = 0; i < response.data.Matches.length; i++) {
            bracketdata.results[0].push([]);
            for (var j = 0; j < response.data.Matches[i].length; j++) {
                if (response.data.Matches[i][j]) {
                    if (response.data.Matches[i][j].Finished) {
                        if (response.data.Matches[i][j].Result === 'ForfeitBlue' || response.data.Matches[i][j].Result === 'DisqualificationBlue') {
                            bracketdata.results[0][i].push([
                                -1, 0,
                                response.data.Matches[i][j]
                            ]);
                        } else if (response.data.Matches[i][j].Result === 'ForfeitRed' || response.data.Matches[i][j].Result === 'DisqualificationRed') {
                            bracketdata.results[0][i].push([
                                0, -1,
                                response.data.Matches[i][j]
                            ]);
                        } else if (response.data.Matches[i][j].RoundsBlue > 0 || response.data.Matches[i][j].RoundsRed > 0) {
                            bracketdata.results[0][i].push([
                                response.data.Matches[i][j].RoundsBlue, response.data.Matches[i][j].RoundsRed,
                                response.data.Matches[i][j]
                            ]);
                        } else {
                            bracketdata.results[0][i].push([
                                response.data.Matches[i][j].ScoreBlue, response.data.Matches[i][j].ScoreRed,
                                response.data.Matches[i][j]
                            ]);
                        }
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
            onMatchClick: function (data) {
                if (data) {
                    if (data.Finished) {
                        $location.path("ShowMatch/" + data.Id);
                    } else {
                        $location.path("ScoreKeeper/" + data.Id);
                    }
                    $scope.$apply();
                }
            },
            decorator: {
                edit: function () { }, render: function (container, data, score, state) {
                    switch (state) {
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
                },
                renderMatch: function (container, data) {
                    if (data && !data.Finished && data.Location) {
                        container.append("<div class='matchLocation'>" + data.Location + "</div>");
                    }
                }
            },
            teamWidth: 200
        });
    }, $scope.$parent.handleHttpResponse);
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
    $scope.showOnLocation = function () {
        if (!$cookies.get('location'))
            return;
        if ($routeParams.poolId) {
            $scope.$parent.ochsHub.invoke("ShowPoolEliminationOnLocation",
                $routeParams.poolId,
                $cookies.get('location'));
        } else if ($routeParams.phaseId) {
            $scope.$parent.ochsHub.invoke("ShowPhaseEliminationOnLocation",
                $routeParams.phaseId,
                $cookies.get('location'));
        }
    };
});
app.controller("RankingController", function ($scope, $http, $routeParams, $location, $cookies) {
    var url;
    var rulesUrl;
    if ($routeParams.poolId) {
        url = "api/Pool/GetRanking/" + $routeParams.poolId;
        rulesUrl = "api/Pool/GetRules/" + $routeParams.poolId;
    } else if ($routeParams.phaseId) {
        url = "api/Phase/GetRanking/" + $routeParams.phaseId;
        rulesUrl = "api/Phase/GetRules/" + $routeParams.phaseId;
    } else {
        return;
    }
    $http.get(url).then(function (response) {
        $scope.rankings = response.data;
    }, $scope.$parent.handleHttpResponse);
    $http.get(rulesUrl).then(function (response) {
        $scope.rules = response.data;
    }, $scope.$parent.handleHttpResponse);
    $scope.$on('updateRankings', function (event, args) {
        if (args && (args === $routeParams.poolId || args === $routeParams.phaseId)) {
            $http.get(url).then(function (response) {
                $scope.totalMatches = 0;
                $scope.totalWins = 0;
                $scope.totalDraws = 0;
                $scope.totalLosses = 0;
                $scope.totalMatchPoints = 0;
                $scope.totalDoubleHits = 0;
                $scope.totalPenalties = 0;
                $scope.totalWarnings = 0;
                $scope.totalExchanges = 0;
                $scope.totalHitsGiven = 0;
                $scope.totalHitsReceived = 0;
                $scope.totalSportsmanshipPoints = 0;
                $scope.totalNotes = 0;
                $scope.rankings = response.data;
            });
        }
    });
    $scope.updateRankings = function () {
        if ($routeParams.poolId) {
            $scope.$parent.ochsHub.invoke("UpdatePoolRankings", $routeParams.poolId);
        } else if ($routeParams.phaseId) {
            $scope.$parent.ochsHub.invoke("UpdatePhaseRankings", $routeParams.phaseId);
        }
    };
    $scope.showOnLocation = function () {
	    if (!$cookies.get('location'))
		    return;
	    if ($routeParams.poolId) {
		    $scope.$parent.ochsHub.invoke("ShowPoolRankingOnLocation",
			    $routeParams.poolId,
			    $cookies.get('location'));
	    } else if ($routeParams.phaseId) {
		    $scope.$parent.ochsHub.invoke("ShowPhaseRankingOnLocation",
			    $routeParams.phaseId,
			    $cookies.get('location'));
	    }
    };
});

app.controller("ListMatchesController",
	function($scope, $http, $cookies) {
		$http.get("api/Match/All")
			.then(function(response) {
					$scope.currentMatches = {
						Matches: response.data,
                        viewer: ($cookies.get('viewer') === 'true')
					};
				},
				$scope.$parent.handleHttpResponse);
	});

app.controller("LoginController", function ($scope, $location, $http) {
    $scope.login = function () {
        $http.post("api/Auth/Login", { Username: $scope.username, Password: $scope.password }).then(function (result) {
            if (result) {
                $scope.$parent.username = $scope.username;
                $scope.$parent.reconnectSignalR();
                $location.path("Welcome");
            }
        }, $scope.$parent.handleHttpResponse);
    };
});

app.controller("MatchController", function ($scope, $http, $routeParams, $interval, $location, $cookies) {
    $scope.matchId = $routeParams.matchId;
    $scope.addEventPointBlue = 0;
    $scope.addEventPointRed = 0;
    $scope.location = $cookies.get('location');
    if ($routeParams.matchId) {

        $http.get("api/Match/Get/" + $routeParams.matchId).then(function (response) {
            $scope.currentMatch = response.data;
            $scope.matchResult = response.data.Result;
            if (!$scope.currentMatch.Started && $scope.rules && $scope.rules.CountDownScore) {
                $scope.currentMatch.ScoreRed = $scope.rules.PointsMax;
                $scope.currentMatch.ScoreBlue = $scope.rules.PointsMax;
            }
            $scope.round = response.data.Round;
            $scope.editTime = new Date(1970, 1, 1, 0, 0, response.data.LiveTime);
        }, $scope.$parent.handleHttpResponse);
        $http.get("api/Match/GetRules/" + $routeParams.matchId).then(function (response) {
            $scope.rules = response.data;
            if ($scope.currentMatch && !$scope.currentMatch.Started && $scope.rules.CountDownScore) {
                $scope.currentMatch.ScoreRed = $scope.rules.PointsMax;
                $scope.currentMatch.ScoreBlue = $scope.rules.PointsMax;
            }
            $scope.newMatchRules = $scope.rules.Id;
        }, $scope.$parent.handleHttpResponse);
        $http.get("api/Match/GetNext/" + $routeParams.matchId).then(function (response) {
            $scope.nextMatch = response.data;
        }, $scope.$parent.handleHttpResponse);
    }
    if ($location.path().substring(0, 10) === '/EditMatch') {
        $http.get("api/MatchRules/All").then(function (response) {
            $scope.matchRules = response.data;
        }, $scope.$parent.handleHttpResponse);
    }
    $scope.$on('updateMatch', function (event, args) {
        if ($scope.matchId && $scope.matchId === args.Id) {
            if ($scope.currentMatch && !$scope.currentMatch.Finished && args.Finished) {
                if ($scope.nextMatch && ($location.path().length < 10 || $location.path().substring(0, 10) !== '/EditMatch')) {
                    if ($location.path().length >= 12 && $location.path().substring(0, 12) === '/ScoreKeeper') {
                        $location.path("ScoreKeeper/" + $scope.nextMatch.Id);
                    } else {
                        $location.path("ShowMatch/" + $scope.nextMatch.Id);
                    }
                } else if ($cookies.get('location') && $location.path().length >= 10 && $location.path().substring(0, 10) === '/ShowMatch') {
                    $location.path("ShowLocation");
                } else if (args.PoolId) {
                    $location.path("ShowPool/" + args.PoolId);
                } else if (args.PhaseId) {
                    $location.path("ShowPhase/" + args.PhaseId);
                } else if (args.CompetitionId) {
                    $location.path("ShowCompetition/" + args.CompetitionId);
                } else {
                    $location.path("ListCompetitions");
                }
            }
            $scope.currentMatch = args;
            if (!$scope.currentMatch.Started && $scope.rules && $scope.rules.CountDownScore) {
                $scope.currentMatch.ScoreRed = $scope.rules.PointsMax;
                $scope.currentMatch.ScoreBlue = $scope.rules.PointsMax;
            }
            $scope.editTime = new Date(1970, 1, 1, 0, 0, args.LiveTime);
        } else if ((!$scope.currentMatch || !$scope.currentMatch.Started || $scope.currentMatch.Finished) && args.Started && !args.Finished && $cookies.get('location') && $cookies.get('location') === args.Location && $location.path().length >= 12 && $location.path().substring(0, 12) !== '/ScoreKeeper') {
            $location.path("ShowMatch/" + args.Id);
        } else if ($scope.nextMatch && $scope.nextMatch.Id === args.Id) {
            $http.get("api/Match/GetNext/" + $scope.matchId).then(function (response) {
                $scope.nextMatch = response.data;
            }, $scope.$parent.handleHttpResponse);
        }
    });
    if ($location.path().length < 12 || $location.path().substring(0, 12) !== '/ScoreKeeper') {
        $scope.$watch('currentMatch.TimeOutRunning', function () {
            var popup = $('#timeOutPopup');
            if (popup) {
                if ($scope.currentMatch && $scope.currentMatch.TimeOutRunning) {
                    popup.fadeIn(800);
                } else {
                    popup.fadeOut(0);
                }
            }
        }, true);
    }
    $interval(function () {
        if ($scope.currentMatch && $scope.currentMatch.TimeRunning) {
            if ($scope.rules && $scope.rules.TimeMaxSeconds > 0 &&
                $location.path().length >= 12 && $location.path().substring(0, 12) === '/ScoreKeeper' &&
                $scope.currentMatch.LiveTime < $scope.rules.TimeMaxSeconds &&
                $scope.currentMatch.LiveTime + 0.01 >= $scope.rules.TimeMaxSeconds) {
                $scope.$parent.ochsHub.client.displayMessage('Maximum time reached', 'warning');
            }
            $scope.currentMatch.LiveTime += 0.01;
            $scope.editingTime = false;
        }
        if ($scope.currentMatch && $scope.currentMatch.TimeOutRunning) {
            $scope.currentMatch.LiveTimeOut += 0.01;
        }
    }, 10);
    $scope.addMatchEvent = function ($pointsBlue, $pointsRed, $eventType, $note) {
        $scope.addEventPointBlue = 0;
        $scope.addEventPointRed = 0;
        $scope.note = "";
        $scope.$parent.ochsHub.invoke("AddMatchEvent", $scope.matchId, $pointsBlue, $pointsRed, $eventType, $note);
    };
    $scope.setScoreBlue = function ($points) {
        if ($scope.rules && $scope.rules.ConfirmScores) {
            $scope.addEventPointBlue = $points;
        } else {
            $scope.addMatchEvent($points, 0, 'Score');
        }
    };
    $scope.setScoreRed = function ($points) {
        if ($scope.rules && $scope.rules.ConfirmScores) {
            $scope.addEventPointRed = $points;
        } else {
            $scope.addMatchEvent(0, $points, 'Score');
        }
    };
    $scope.undoLastMatchEvent = function () {
        $scope.$parent.ochsHub.invoke("UndoLastMatchEvent", $scope.matchId);
    };
    $scope.updateEventNote = function ($eventId, $note) {
        $scope.$parent.ochsHub.invoke("UpdateMatchEventNote", $scope.matchId, $eventId, $note);
    };
    $scope.deleteMatchEvent = function ($eventId) {
        $scope.$parent.ochsHub.invoke("DeleteMatchEvent", $scope.matchId, $eventId);
    };
    $scope.startTime = function () {
        if ($scope.editingTime) {
            $scope.changeEditTime();
        }
        $scope.$parent.ochsHub.invoke("StartTime", $scope.matchId);
    };
    $scope.stopTime = function () {
        $scope.$parent.ochsHub.invoke("StopTime", $scope.matchId);
    };
    $scope.changeEditTime = function () {
        if ($scope.editingTime) {
            $scope.$parent.ochsHub.invoke("SetTimeMilliSeconds",
                $scope.matchId,
                $scope.editTime - new Date(1970, 1, 1, 0, 0, 0));
            $scope.editingTime = false;
        } else if (!$scope.currentMatch.TimeRunning) {
            $scope.editingTime = true;
        }
    };
    $scope.setMatchResult = function () {
        if ($scope.matchResult)
            $scope.$parent.ochsHub.invoke("SetMatchResult", $scope.matchId, $scope.matchResult);
    };
    $scope.matchSaveMatchRules = function () {
        $scope.$parent.ochsHub.invoke("MatchSetMatchRules", $scope.matchId, $scope.newMatchRules ? $scope.newMatchRules : '00000000-0000-0000-0000-000000000000');
    };
    $scope.range = function (min, max, step) {
        step = step || 1;
        var input = [];
        for (var i = min; i <= max; i += step) {
            input.push(i);
        }
        return input;
    };
    $scope.updateRound = function () {
        if ($scope.round)
            $scope.$parent.ochsHub.invoke("UpdateMatchRound", $scope.matchId, $scope.round);
    };
    $scope.showMatchOnLocation = function () {
        if ($scope.matchId && $cookies.get('location'))
            $scope.$parent.ochsHub.invoke("ShowMatchOnLocation", $scope.matchId, $cookies.get('location'));
    };
});

app.controller("ListRulesController", function ($scope, $http) {
    $http.get("api/MatchRules/All").then(function (response) {
        $scope.matchRules = response.data;
    }, $scope.$parent.handleHttpResponse);
    $http.get("api/RankingRules/All").then(function (response) {
        $scope.rankingRules = response.data;
    }, $scope.$parent.handleHttpResponse);
});

app.controller("EditMatchRulesController", function ($scope, $http, $routeParams, $location) {
    $scope.matchRulesId = $routeParams.matchRulesId;

    $http.get("api/MatchRules/Get" + ($scope.matchRulesId ? "/" + $routeParams.matchRulesId : "")).then(function (response) {
        $scope.rules = response.data;
        delete $scope.rules.TimeMax;
        $scope.editTimeMax = new Date(1970, 1, 1, 0, 0, response.data.TimeMaxSeconds);
    }, $scope.$parent.handleHttpResponse);
    $scope.saveMatchRules = function () {
        $scope.rules.TimeMaxSeconds = ($scope.editTimeMax - new Date(1970, 1, 1, 0, 0, 0)) / 1000.0;
        $http.post("api/MatchRules/Save", $scope.rules)
            .then(function (response) {
                $scope.$parent.ochsHub.client.displayMessage("Saved", "success");
                $scope.rules = response.data;
                delete $scope.rules.TimeMax;
                $scope.editTimeMax = new Date(1970, 1, 1, 0, 0, response.data.TimeMaxSeconds);
                if (!$scope.matchRulesId) {
                    $location.path("EditMatchRules/" + response.data.Id);
                }
            }, $scope.$parent.handleHttpResponse);
    };
});

app.controller("EditRankingRulesController", function ($scope, $http, $routeParams, $location) {
    $scope.rankingRulesId = $routeParams.rankingRulesId;
    $scope.rankingStats = ['Match Points', 'Hit Ratio', 'Double Hits', 'Win Ratio', 'Penalties', 'Warnings'];
    $http.get("api/RankingRules/Get" + ($scope.rankingRulesId ? "/" + $routeParams.rankingRulesId : "")).then(function (response) {
        $scope.rules = response.data;
    }, $scope.$parent.handleHttpResponse);
    $scope.saveRankingRules = function () {
        $http.post("api/RankingRules/Save", $scope.rules)
            .then(function (response) {
                $scope.$parent.ochsHub.client.displayMessage("Saved", "success");
                $scope.rules = response.data;
                if (!$scope.rankingRulesId) {
                    $location.path("EditRankingRules/" + response.data.Id);
                }
            }, $scope.$parent.handleHttpResponse);
    };
    $scope.moveUp = function (index) {
        var old = $scope.rules.Sorting[index];
        $scope.rules.Sorting[index] = $scope.rules.Sorting[index - 1];
        $scope.rules.Sorting[index - 1] = old;
    };
    $scope.moveDown = function (index) {
        var old = $scope.rules.Sorting[index];
        $scope.rules.Sorting[index] = $scope.rules.Sorting[index + 1];
        $scope.rules.Sorting[index + 1] = old;
    };
});