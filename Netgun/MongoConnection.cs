using MongoDB.Bson;
using MongoDB.Driver;
using Netgun.Model;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Netgun
{
    public class MongoConnection
    {
        private readonly MongoClient _client;

        public MongoConnection(string connectionString)
        {
            _client = new MongoClient(connectionString);
            Server = new Server();
        }

        public Server Server { get; set; }

        public Task<List<BsonDocument>> GetDocuments(string db, string collection)
        {
            return this._client.GetDatabase(db).GetCollection<BsonDocument>(collection).Find(b => true).ToListAsync();
        }

        async public Task Populate()
        {
            Server.Databases = new List<Database>();
            var dbCursor = await _client.ListDatabasesAsync();
            await dbCursor.ForEachAsync(async dbBson =>
            {
                var dbName = dbBson["name"].AsString;
                var db = new Database { Name = dbName, Collections = new List<Collection>() };
                Server.Databases.Add(db);

                var collectionCursor = await _client.GetDatabase(dbName).ListCollectionsAsync();
                await
                    collectionCursor.ForEachAsync(
                        collectionBson => db.Collections.Add(new Collection { Name = collectionBson["name"].AsString }));
            });
        }
    }
}
