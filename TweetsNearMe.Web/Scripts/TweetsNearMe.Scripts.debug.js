//! TweetsNearMe.Scripts.debug.js
//

(function($) {

Type.registerNamespace('TweetsNearMe.Scripts');

////////////////////////////////////////////////////////////////////////////////
// TweetsNearMe.Scripts.LocationHelper

TweetsNearMe.Scripts.LocationHelper = function TweetsNearMe_Scripts_LocationHelper() {
    /// <field name="_longitude" type="String" static="true">
    /// </field>
    /// <field name="_latitude" type="String" static="true">
    /// </field>
}
TweetsNearMe.Scripts.LocationHelper._getLocation = function TweetsNearMe_Scripts_LocationHelper$_getLocation() {
    var locationCookie = TweetsNearMe.Scripts.Utils.getCookieValue('Location');
    if (String.isNullOrEmpty(locationCookie)) {
        TweetsNearMe.Scripts.LocationHelper._performLocationRequest();
        TweetsNearMe.Scripts.LocationHelper._longitude = '0';
        TweetsNearMe.Scripts.LocationHelper._latitude = '0';
    }
    else {
        var coords = locationCookie.split('|');
        TweetsNearMe.Scripts.LocationHelper._latitude = coords[0];
        TweetsNearMe.Scripts.LocationHelper._longitude = coords[1];
        if (TweetsNearMe.Scripts.LocationHelper._latitude === '0' || TweetsNearMe.Scripts.LocationHelper._longitude === '0') {
            TweetsNearMe.Scripts.LocationHelper._performLocationRequest();
        }
    }
}
TweetsNearMe.Scripts.LocationHelper._performLocationRequest = function TweetsNearMe_Scripts_LocationHelper$_performLocationRequest() {
    try {
        window.navigator.geolocation.getCurrentPosition(function(data) {
            TweetsNearMe.Scripts.Utils.writeCookie('Location', data.coords.latitude.toString() + '|' + data.coords.longitude.toString(), 1);
            TweetsNearMe.Scripts.LocationHelper._latitude = data.coords.latitude.toString();
            TweetsNearMe.Scripts.LocationHelper._longitude = data.coords.longitude.toString();
        });
    }
    catch ($e1) {
    }
}
TweetsNearMe.Scripts.LocationHelper.get_longitude = function TweetsNearMe_Scripts_LocationHelper$get_longitude() {
    /// <value type="String"></value>
    if (String.isNullOrEmpty(TweetsNearMe.Scripts.LocationHelper._longitude)) {
        TweetsNearMe.Scripts.LocationHelper._getLocation();
    }
    return TweetsNearMe.Scripts.LocationHelper._longitude;
}
TweetsNearMe.Scripts.LocationHelper.get_latitude = function TweetsNearMe_Scripts_LocationHelper$get_latitude() {
    /// <value type="String"></value>
    if (String.isNullOrEmpty(TweetsNearMe.Scripts.LocationHelper._latitude)) {
        TweetsNearMe.Scripts.LocationHelper._getLocation();
    }
    return TweetsNearMe.Scripts.LocationHelper._latitude;
}


////////////////////////////////////////////////////////////////////////////////
// TweetsNearMe.Scripts.Utils

TweetsNearMe.Scripts.Utils = function TweetsNearMe_Scripts_Utils() {
}
TweetsNearMe.Scripts.Utils.getCookieValue = function TweetsNearMe_Scripts_Utils$getCookieValue(cookieName) {
    /// <param name="cookieName" type="String">
    /// </param>
    /// <returns type="String"></returns>
    var cookies = document.cookie.split(';');
    var $enum1 = ss.IEnumerator.getEnumerator(cookies);
    while ($enum1.moveNext()) {
        var cookie = $enum1.current;
        if (cookie.substring(0, cookie.indexOf('=')).trim() === cookieName) {
            return cookie.substr(cookie.indexOf('=') + 1).trim();
        }
    }
    return '';
}
TweetsNearMe.Scripts.Utils.writeCookie = function TweetsNearMe_Scripts_Utils$writeCookie(name, value, days) {
    /// <param name="name" type="String">
    /// </param>
    /// <param name="value" type="String">
    /// </param>
    /// <param name="days" type="Number" integer="true">
    /// </param>
    var expires = '';
    if (days > 0) {
        var date = new Date();
        date.setTime(date.getTime() + (days * 24 * 60 * 60 * 1000));
        expires = '; expires=' + date.toUTCString();
    }
    document.cookie = name + '=' + value + expires + '; path=/';
}
TweetsNearMe.Scripts.Utils.getLanguage = function TweetsNearMe_Scripts_Utils$getLanguage() {
    /// <returns type="String"></returns>
    try {
        if (!ss.isNullOrUndefined(window.navigator.browserLanguage)) {
            return window.navigator.browserLanguage.substr(0, 2).toLowerCase();
        }
    }
    catch ($e1) {
    }
    try {
        if (!ss.isNullOrUndefined(window.navigator.systemLanguage)) {
            return window.navigator.systemLanguage.substr(0, 2).toLowerCase();
        }
    }
    catch ($e2) {
    }
    try {
        if (!ss.isNullOrUndefined(window.navigator.userLanguage)) {
            return window.navigator.userLanguage.substr(0, 2).toLowerCase();
        }
    }
    catch ($e3) {
    }
    return 'en';
}


////////////////////////////////////////////////////////////////////////////////
// TweetsNearMe.Scripts.Tweet

TweetsNearMe.Scripts.Tweet = function TweetsNearMe_Scripts_Tweet() {
    /// <field name="id" type="Number" integer="true">
    /// </field>
    /// <field name="fromUser" type="String">
    /// </field>
    /// <field name="text" type="String">
    /// </field>
    /// <field name="created_at" type="String">
    /// </field>
    /// <field name="profileImageUrl" type="String">
    /// </field>
    /// <field name="shareUrl" type="String">
    /// </field>
}
TweetsNearMe.Scripts.Tweet.prototype = {
    id: 0,
    fromUser: null,
    text: null,
    created_at: null,
    profileImageUrl: null,
    shareUrl: null,
    
    shareTweet: function TweetsNearMe_Scripts_Tweet$shareTweet() {
        window.location.assign(this.shareUrl);
    }
}


////////////////////////////////////////////////////////////////////////////////
// TweetsNearMe.Scripts.TweetsViewModel

TweetsNearMe.Scripts.TweetsViewModel = function TweetsNearMe_Scripts_TweetsViewModel() {
    /// <field name="currentTweets" type="ObservableArray`1">
    /// </field>
    /// <field name="_refreshTimer" type="Number" integer="true">
    /// </field>
    /// <field name="_lastID" type="Number" integer="true">
    /// </field>
}
TweetsNearMe.Scripts.TweetsViewModel.prototype = {
    currentTweets: null,
    _refreshTimer: 0,
    _lastID: 0,
    
    clearTweets: function TweetsNearMe_Scripts_TweetsViewModel$clearTweets() {
        while (this.currentTweets().length > 0) {
            this.currentTweets.shift();
        }
        $('#loading-indicator').show();
        this._lastID = -1;
        this.refresh();
    },
    
    refresh: function TweetsNearMe_Scripts_TweetsViewModel$refresh() {
        if (this._refreshTimer > -1) {
            window.clearTimeout(this._refreshTimer);
        }
        var Url = String.format('http://search.twitter.com/search.json?callback=?&result_type=recent&lang={0}&q=&geocode={1},{2},{3}mi&rpp=25{4}', TweetsNearMe.Scripts.Utils.getLanguage(), TweetsNearMe.Scripts.LocationHelper.get_latitude(), TweetsNearMe.Scripts.LocationHelper.get_longitude(), this._getMaxDistance(), ((this._lastID > -1) ? ('&since_id=' + this._lastID) : ''));
        var ajaxOptions = {};
        ajaxOptions.dataType = 'jsonp';
        ajaxOptions.success = ss.Delegate.create(this, function(data, textStatus, request) {
            $('#loading-indicator').hide();
            var results = data.results;
            results.reverse();
            for (var i = 0; i < results.length; i++) {
                var tweet = new TweetsNearMe.Scripts.Tweet();
                tweet.created_at = (results[i].created_at).replaceAll(' +0000', '');
                tweet.id = results[i].id;
                if (tweet.id > this._lastID) {
                    this._lastID = tweet.id;
                }
                tweet.profileImageUrl = results[i].profile_image_url;
                tweet.text = results[i].text;
                tweet.fromUser = results[i].from_user;
                var tweetUrl = 'https://twitter.com/' + tweet.fromUser + '/status/' + tweet.id;
                var tweetTitle = tweet.fromUser + "'s Tweet";
                tweet.shareUrl = 'http://www.facebook.com/sharer.php?u=' + encodeURIComponent(tweetUrl) + '&t=' + encodeURIComponent(tweetTitle);
                if (this.currentTweets().length > 0) {
                    if (this.currentTweets()[0].id === tweet.id) {
                        continue;
                    }
                }
                this.currentTweets.unshift(tweet);
            }
            this._refreshTimer = window.setTimeout(ss.Delegate.create(this, this.refresh), 2500);
        });
        ajaxOptions.error = ss.Delegate.create(this, function(request, error, ex) {
            this._refreshTimer = window.setTimeout(ss.Delegate.create(this, this.refresh), 5000);
        });
        $.ajax(Url, ajaxOptions);
    },
    
    initViewModel: function TweetsNearMe_Scripts_TweetsViewModel$initViewModel() {
        this.currentTweets = ko.observableArray();
        this._refreshTimer = -1;
        this._lastID = -1;
        var maxDistance = TweetsNearMe.Scripts.Utils.getCookieValue('Max-Distance');
        if (!String.isNullOrEmpty(maxDistance)) {
            $('#max-distance-field').val(maxDistance);
            $('#max-distance-field').trigger('refresh');
        }
        $('#max-distance-field').change(ss.Delegate.create(this, this._distanceChangeHandler));
    },
    
    _distanceChangeHandler: function TweetsNearMe_Scripts_TweetsViewModel$_distanceChangeHandler(args) {
        /// <param name="args" type="jQueryEvent">
        /// </param>
        this.clearTweets();
    },
    
    _getMaxDistance: function TweetsNearMe_Scripts_TweetsViewModel$_getMaxDistance() {
        /// <returns type="Number" integer="true"></returns>
        var maxDistance = parseInt($('#max-distance-field').val());
        TweetsNearMe.Scripts.Utils.writeCookie('Max-Distance', maxDistance.toString(), 7);
        return maxDistance;
    }
}


TweetsNearMe.Scripts.LocationHelper.registerClass('TweetsNearMe.Scripts.LocationHelper');
TweetsNearMe.Scripts.Utils.registerClass('TweetsNearMe.Scripts.Utils');
TweetsNearMe.Scripts.Tweet.registerClass('TweetsNearMe.Scripts.Tweet');
TweetsNearMe.Scripts.TweetsViewModel.registerClass('TweetsNearMe.Scripts.TweetsViewModel');
TweetsNearMe.Scripts.LocationHelper._longitude = '';
TweetsNearMe.Scripts.LocationHelper._latitude = '';
})(jQuery);

//! This script was generated using Script# v0.7.4.0
