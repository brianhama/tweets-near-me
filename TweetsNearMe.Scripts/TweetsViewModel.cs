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
      private long LastID;

      public void ClearTweets()
      {
         while (CurrentTweets.GetItems().Count > 0)
            CurrentTweets.Shift();

         jQuery.Select("#loading-indicator").Show();
         LastID = -1;

         Refresh();
      }

      public void Refresh()
      {
         if (refreshTimer > -1)
            Window.ClearTimeout(refreshTimer);

         String Url = String.Format("http://search.twitter.com/search.json?callback=?&result_type=recent&lang={0}&q=&geocode={1},{2},{3}mi&rpp=5{4}", Utils.GetLanguage(), LocationHelper.Latitude, LocationHelper.Longitude, GetMaxDistance(), (LastID > -1 ? ("&since_id=" + LastID) : ""));
         jQueryAjaxOptions ajaxOptions = new jQueryAjaxOptions();
         ajaxOptions.DataType = "jsonp";
         ajaxOptions.Success = new AjaxRequestCallback(delegate(object data, string textStatus, jQueryXmlHttpRequest request) {
            jQuery.Select("#loading-indicator").Hide();
            Array results = (Array)Type.GetField(data, "results");
            results.Reverse();
            for (int i = 0; i < results.Length; i++)
            {
               Tweet tweet = new Tweet();
               tweet.Created = ((String)Type.GetField(results[i], "created_at")).Replace(" +0000", "");
               tweet.Id = (long)Type.GetField(results[i], "id");
               if (tweet.Id > LastID)
                  LastID = tweet.Id;
               tweet.ProfileImageUrl = (String)Type.GetField(results[i], "profile_image_url");
               tweet.Text = (String)Type.GetField(results[i], "text");
               tweet.FromUser = (String)Type.GetField(results[i], "from_user");

               String tweetUrl = "https://twitter.com/" + tweet.FromUser + "/status/" + tweet.Id;
               String tweetTitle = tweet.FromUser + "'s Tweet";

               tweet.ShareUrl = "http://www.facebook.com/sharer.php?u=" + tweetUrl.EncodeUriComponent() + "&t=" + tweetTitle.EncodeUriComponent();

               if (CurrentTweets.GetItems().Count > 0)
               {
                  if (CurrentTweets.GetItems()[0].Id == tweet.Id)
                     continue;
               }
               CurrentTweets.Unshift(tweet);
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
         LastID = -1;

         String maxDistance = Utils.GetCookieValue("Max-Distance");
         if (!String.IsNullOrEmpty(maxDistance))
         {
            jQuery.Select("#max-distance-field").Value(maxDistance);
            Script.Literal("$('#max-distance-field').trigger('refresh')");
         }
         jQuery.Select("#max-distance-field").Change(new jQueryEventHandler(DistanceChangeHandler));
      }

      private void DistanceChangeHandler(jQueryEvent args)
      {
         ClearTweets();
      }

      private Int32 GetMaxDistance()
      {
         Int32 maxDistance = Int32.Parse(jQuery.Select("#max-distance-field").GetValue());
         Utils.WriteCookie("Max-Distance", maxDistance.ToString(), 7);
         return maxDistance;
      }
   }
}