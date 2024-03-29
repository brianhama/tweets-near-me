using System;
using System.Collections.Generic;
using System.Html;

namespace TweetsNearMe.Scripts
{
   public static class Utils
   {
      public static string GetCookieValue(String cookieName)
      {
         String[] cookies = Document.Cookie.Split(';');
         foreach (String cookie in cookies)
         {
            if (cookie.Substring(0, cookie.IndexOf('=')).Trim() == cookieName)
               return cookie.Substr(cookie.IndexOf('=') + 1).Trim();
         }
         return "";
      }

      public static void WriteCookie(String name, String value, Int32 days)
      {
         string expires = "";
         if (days > 0)
         {
            Date date = new Date();
            date.SetTime(date.GetTime() + (days * 24 * 60 * 60 * 1000));
            expires = "; expires=" + date.ToUTCString();
         }
         Document.Cookie = name + "=" + value + expires + "; path=/";
      }

      public static string GetLanguage()
      {
         try
         {
            if (!Script.IsNullOrUndefined(Window.Navigator.BrowserLanguage))
               return Window.Navigator.BrowserLanguage.Substr(0, 2).ToLowerCase();
         }
         catch { }

         try
         {
            if (!Script.IsNullOrUndefined(Window.Navigator.SystemLanguage))
               return Window.Navigator.SystemLanguage.Substr(0, 2).ToLowerCase();
         }
         catch { }

         try
         {
            if (!Script.IsNullOrUndefined(Window.Navigator.UserLanguage))
               return Window.Navigator.UserLanguage.Substr(0, 2).ToLowerCase();
         }
         catch { }

         return "en";
      }
   }
}
