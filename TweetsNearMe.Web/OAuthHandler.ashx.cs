using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Net;
using System.IO;
using System.Xml;
using System.Text;
using System.Web.Script.Serialization;

namespace TweetsNearMe.Web
{
   /// <summary>
   /// Summary description for OAuthHandler
   /// </summary>
   public class OAuthHandler : IHttpHandler
   {

      public void ProcessRequest(HttpContext context)
      {
         try
         {
            if (context.Request.QueryString["code"] != null && context.Request.QueryString["t"] != null)
            {
               String authCode = context.Request.QueryString["code"];
               String mobileNumber = context.Request.QueryString["t"];
               String accessToken = "";

               WebRequest accessTokenRequest = System.Net.HttpWebRequest.Create("https://api.att.com/oauth/token");
               accessTokenRequest.Method = "POST";
               string oauthParameters = "client_id=95578ca832fd253acdf012133ec4f2b9&client_secret=6e85fce5b679d15a&grant_type=authorization_code&code=" + authCode;
               accessTokenRequest.ContentType = "application/x-www-form-urlencoded";
               UTF8Encoding encoding = new UTF8Encoding();
               byte[] postBytes = encoding.GetBytes(oauthParameters);
               accessTokenRequest.ContentLength = postBytes.Length;
               Stream postStream = accessTokenRequest.GetRequestStream();
               postStream.Write(postBytes, 0, postBytes.Length);
               postStream.Close();

               WebResponse accessTokenResponse = accessTokenRequest.GetResponse();
               using (StreamReader accessTokenResponseStream = new StreamReader(accessTokenResponse.GetResponseStream()))
               {
                  string access_token_json = accessTokenResponseStream.ReadToEnd().ToString();
                  JavaScriptSerializer deserializeJsonObject = new JavaScriptSerializer();
                  AccessTokenResponse deserializedJsonObj = (AccessTokenResponse)deserializeJsonObject.Deserialize(access_token_json, typeof(AccessTokenResponse));
                  if (deserializedJsonObj.access_token != null)
                  {
                     accessToken = deserializedJsonObj.access_token.ToString();
                     accessTokenResponseStream.Close();
                     GetLocation(accessToken, mobileNumber);
                  }
               }
            }
         }
         catch { }

         context.Response.Redirect("/", false);
      }

      private void GetLocation(String accessToken, String mobileNumber)
      {
         string deviceId = mobileNumber.ToString().Replace("tel:+1", "");
         deviceId = deviceId.ToString().Replace("tel:+", "");
         deviceId = deviceId.ToString().Replace("tel:1", "");
         deviceId = deviceId.ToString().Replace("tel:", "");
         deviceId = deviceId.ToString().Replace("tel:", "");
         deviceId = deviceId.ToString().Replace("+1", "");
         deviceId = deviceId.ToString().Replace("-", "");
         deviceId = deviceId.ToString().Replace("(", "");
         deviceId = deviceId.ToString().Replace(")", "");
         deviceId = deviceId.ToString().Replace(" ", "");
         if (deviceId.Length == 11)
         {
            deviceId = deviceId.Remove(0, 1);
         }
         String strResult;
         int[] definedReqAccuracy = new int[3] { 100, 1000, 10000 };
         string[] definedTolerance = new string[3] { "NoDelay", "LowDelay", "DelayTolerant" };
         int requestedAccuracy, acceptableAccuracy;
         string tolerance = "NoDelay";
         acceptableAccuracy = 100;
         requestedAccuracy = 100;

         HttpWebRequest tlRequestObject = (HttpWebRequest)System.Net.WebRequest.Create("https://api.att.com/1/devices/tel:" + deviceId.ToString() + "/location?access_token=" + accessToken.ToString() + "&requestedAccuracy=" + requestedAccuracy.ToString() + "&acceptableAccuracy=" + acceptableAccuracy.ToString() + "&tolerance=" + tolerance.ToString());
         tlRequestObject.Method = "GET";
         DateTime msgSentTime = DateTime.UtcNow.ToLocalTime();
         HttpWebResponse tlResponseObject = (HttpWebResponse)tlRequestObject.GetResponse();
         DateTime msgReceivedTime = DateTime.UtcNow.ToLocalTime();
         TimeSpan tokenSpan = msgReceivedTime.Subtract(msgSentTime);
         using (StreamReader tlResponseStream = new StreamReader(tlResponseObject.GetResponseStream()))
         {
            strResult = tlResponseStream.ReadToEnd();
            JavaScriptSerializer deserializeJsonObject = new JavaScriptSerializer();
            tlResponse deserializedJsonObj = (tlResponse)deserializeJsonObject.Deserialize(strResult, typeof(tlResponse));

            HttpCookie locationCookie = new HttpCookie("Location");
            if (HttpContext.Current.Request.Cookies["Location"] != null)
               locationCookie = HttpContext.Current.Request.Cookies["Location"];

            locationCookie.Value = deserializedJsonObj.latitude.ToString() + "|" + deserializedJsonObj.longitude.ToString();
            locationCookie.Expires = DateTime.UtcNow.AddDays(365);
            HttpContext.Current.Response.Cookies.Add(locationCookie);
            tlResponseStream.Close();
         }
      }

      public bool IsReusable
      {
         get
         {
            return true;
         }
      }
   }

   public class AccessTokenResponse
   {
      public string access_token;
      public string refresh_token;
      public string expires_in;

   }

   public class tlResponse
   {
      public string accuracy { get; set; }
      public double altitude { get; set; }
      public double latitude { get; set; }
      public string longitude { get; set; }
      public DateTime timestamp { get; set; }
   }
}