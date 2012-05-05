using System;
using System.Collections.Generic;
using System.Html;

namespace TweetsNearMe.Scripts
{
   public static class LocationHelper
   {
      private static void GetLocation()
      {
         String locationCookie = Utils.GetCookieValue("Location");
         if (String.IsNullOrEmpty(locationCookie))
         {
            PerformLocationRequest();
            _longitude = "0";
            _latitude = "0";
         }
         else
         {
            String[] coords = locationCookie.Split('|');
            _latitude = coords[0];
            _longitude = coords[1];
            if (_latitude == "0" || _longitude == "0")
               PerformLocationRequest();
         }
      }

      private static void PerformLocationRequest()
      {
         try
         {
            Window.Navigator.Geolocation.GetCurrentPosition(new System.Html.Services.GeolocationSuccessCallback(delegate(System.Html.Services.Geolocation data)
            {
               Utils.WriteCookie("Location", data.Coordinates.Latitude.ToString() + "|" + data.Coordinates.Longitude.ToString(), 1);
               _latitude = data.Coordinates.Latitude.ToString();
               _longitude = data.Coordinates.Longitude.ToString();
            }));
         }
         catch { }
      }

      private static String _longitude = "";
      public static String Longitude
      {
         get
         {
            if (String.IsNullOrEmpty(_longitude))
               GetLocation();
            return _longitude;
         }
      }

      private static String _latitude = "";
      public static String Latitude
      {
         get
         {
            if (String.IsNullOrEmpty(_latitude))
               GetLocation();
            return _latitude;
         }
      }
   }
}
