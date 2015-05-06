using System.Collections.Generic;

namespace Netgun.Model
{
    public class Server
    {
        public Server()
        {
            Databases = new List<Database>();
        }

        public string Name { get; set; }

        public List<Database> Databases { get; set; }
    }
}