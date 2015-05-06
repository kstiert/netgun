using System.Collections.Generic;

namespace Netgun.Model
{
    public class Server
    {
        public string Name { get; set; }

        public List<Database> Databases { get; set; }
    }
}