using BiitSocioApis.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace BiitSocioApis.HelpingClasses
{
    public class PostHelper
    {
       public Post post { get; set; }
    public    bool isLiked { get; set; }

        public bool isFriend { get; set; }
        public bool isPinned { get; set; }


    }
}