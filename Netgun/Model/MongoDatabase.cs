using System.Collections.Generic;

namespace Netgun.Model
{
    public class MongoDatabase
    {
        public string Name { get; set; }

        public List<MongoCollection> Collections { get; set; } 
    }
}