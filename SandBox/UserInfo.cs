using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using Newtonsoft.Json;

namespace SandBox
{
    public class UserInfo
    {
        public string Name { get; set; } = "Liu";
        public string Email { get; set; } = "thodsaphon@gmail.com";
        public List<string> FavoriteFood { get; set; } = new List<string>() { "shochi","Pie","Rice"};

        
    }
}