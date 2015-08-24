'use strict';

var myApp = angular.module('RSSFeedApp', []);

/**
 * @typedef {object} Feed
 * @property {string} author The author of the feed.
 * @property {FeedItem[]} entries An Arry of all feed items.  
 */

/**
 * @typedef {object} FeedItem
 * @property {string} author The author of the feed item
 * @property {string[]} categories An Arry of all categories
 * @property {string} title The title of the feed item
 * @property {string} content The content of the feed item.
 * @property {date} publishedDate The date when this item was published.
 */

myApp.factory('FeedService', ['$http', '$q', function ($http, $q) {
  var parsedFeed = {
    entries: []
  };

  return {


    /**
     * @description updates the internal feed items list
     * @param {String} url - The url of the requested feed.
     * @returns {HttpPromise<Feed>} The a promise for the requested feed. 
     */
    updateFeed: function (url) {
      var defer = $q.defer();

      $http.jsonp('//ajax.googleapis.com/ajax/services/feed/load?v=1.0&num=50&callback=JSON_CALLBACK&q=' + encodeURIComponent(url)).then(function (res) {
        parsedFeed = res.data.responseData.feed;
        defer.resolve(parsedFeed);
      });
      return defer.promise;
    }
  }
}]);

myApp.controller("FeedCtrl", ['$scope', 'FeedService', function ($scope, feedService) { 
    feedService.updateFeed("http://www.dotnetautor.de/GetRssFeed").then(function (res) {
      $scope.feeds = res.entries;
    });
}]);

