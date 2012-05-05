using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;

namespace TweetsNearMe.Scripts
{
   public class Tweet
   {
      public long Id;

      public String FromUser;

      public String Text;
      [ScriptName("created_at")]

      public String Created;

      public String ProfileImageUrl;
   }
}
