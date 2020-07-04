'use strict';

app.directive('matchesTable',
    function () {
        return {
            restrict: 'E',
            scope: {
                context: '=context'
            },
            templateUrl: 'templates/MatchesTable.html',
            link: function ($scope, element, attrs) {
	            $scope.location = $scope.$parent.$parent.location;
                $scope.checkAll = function () {
                    angular.forEach($scope.context.Matches, function (obj) {
                        obj.Selected = $scope.select;
                    });
                };
                $scope.updateLocation = function () {
                    var matchIds = [];
                    angular.forEach($scope.context.Matches,
                        function (obj) {
                            if (obj.Selected) {
                                matchIds.push(obj.Id);
                            }
                        });
                    if (matchIds.length > 0) {
                        $scope.$parent.$parent.ochsHub.invoke("UpdateMatchLocation", $scope.location, matchIds);
                    }
                };
            }
        };
    });