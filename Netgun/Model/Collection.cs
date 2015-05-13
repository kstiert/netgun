using MongoDB.Driver.Core.Clusters;

namespace Netgun.Model
{
    public class Collection
    {
        public string Name { get; set; }

        public string DatabaseName { get; set; }

        public string ConnectionId { get; set; }
    }
}