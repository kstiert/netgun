using System.Collections.Generic;

namespace Netgun.Model
{
    public class Database
    {
        public string Name { get; set; }

        public List<Collection> Collections { get; set; } 
    }
}