using BiitSocioApis.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace BiitSocioApis.HelpingClasses
{
    public class UserHelper
    {
      public  User user { get; set; }
        public bool isFriend { get; set; }
         public int countFriends { get; set; }
       public int postCount { get; set; }
    }
}