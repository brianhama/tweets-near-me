using System;
using System.Collections.Generic;
using System.Html;
using jQueryApi;
using KnockoutApi;

namespace TweetsNearMe.Scripts
{
   public class TweetsViewModel
   {
      public ObservableArray<Tweet> CurrentTweets;

      private Int32 refreshTimer;

      public void ClearTweets()
      {
         while (CurrentTweets.GetItems().Count > 0)
            CurrentTweets.Shift();
         jQuery.Select("#loading-indicator").Show();
      }

      public void Refresh()
      {
         if (refreshTimer > -1)
            Window.ClearTimeout(refreshTimer);

         Int32 LastID = -1;
         if (CurrentTweets.GetItems().Count > 0)
         {
            LastID = CurrentTweets.GetItems()[0].Id;
         }
         String Url = String.Format("http://search.twitter.com/search.json?callback=?&result_type=recent&q=&geocode={0},{1},{2}mi&rpp=25{3}", LocationHelper.Latitude, LocationHelper.Longitude, GetMaxDistance(), (LastID > -1 ? ("&since_id=" + LastID) : ""));
         jQueryAjaxOptions ajaxOptions = new jQueryAjaxOptions();
         ajaxOptions.DataType = "jsonp";
         ajaxOptions.Success = new AjaxRequestCallback(delegate(object data, string textStatus, jQueryXmlHttpRequest request) {
            jQuery.Select("#loading-indicator").Hide();
            Array results = (Array)Type.GetField(data, "results");
            for (int i = 0; i < results.Length; i++)
            {
               Tweet tweet = new Tweet();
               tweet.Created = (String)Type.GetField(results[i], "created_at");
               tweet.Id = (Int32)Type.GetField(results[i], "id");
               tweet.ProfileImageUrl = (String)Type.GetField(results[i], "profileImageUrl");
               tweet.Text = (String)Type.GetField(results[i], "text");
               tweet.FromUser = (String)Type.GetField(results[i], "fromUser");
               CurrentTweets.Push(tweet);
            }
            refreshTimer = Window.SetTimeout(Refresh, 2500);
         });
         ajaxOptions.Error = new AjaxErrorCallback(delegate(jQueryXmlHttpRequest request, string error, Exception ex) {
            refreshTimer = Window.SetTimeout(Refresh, 5000);
         });
         jQuery.Ajax(Url, ajaxOptions);
      }

      public void InitViewModel()
      {
         CurrentTweets = Knockout.ObservableArray<Tweet>();
         refreshTimer = -1;

         String maxDistance = Utils.GetCookieValue("Max-Distance");
         if (!String.IsNullOrEmpty(maxDistance))
            jQuery.Select("#max-distance-field").Value(maxDistance);
      }

      private Int32 GetMaxDistance()
      {
         Int32 maxDistance = Int32.Parse(jQuery.Select("#max-distance-field").GetValue());
         Utils.WriteCookie("Max-Distance", maxDistance.ToString(), 7);
         return maxDistance;
      }
   }
}