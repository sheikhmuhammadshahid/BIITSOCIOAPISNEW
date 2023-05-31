using BiitSocioApis.Models;
using Microsoft.Ajax.Utilities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace BiitSocioApis.HelpingClasses
{
    public class DropDownClass
    {
        public string category { get; set; }
        public List<string> data { get; set; }
        public List<User> users { get; set; }
        public bool isString = false;
    }
}