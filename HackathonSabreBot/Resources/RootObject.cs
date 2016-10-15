using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Web;

namespace HackathonSabreBot.Resources
{
    public class RootObject
    {
        public List<Airport> airports { get; set; }
    }
}