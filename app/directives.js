'use strict';

app.directive('matchesTable',
    function() {
        return {
            restrict: 'E',
            scope: {
                context: '=context'
            },
            templateUrl: 'templates/MatchesTable.html',
            link: function($scope, element, attrs) {
                $scope.checkAll = function () {
                    angular.forEach($scope.context.Matches, function (obj) {
                        obj.Selected = $scope.select;
                    });
                };
            }
        };
    });