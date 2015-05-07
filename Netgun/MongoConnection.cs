using MongoDB.Bson;
using MongoDB.Bson.IO;
using MongoDB.Driver;
using MongoDB.Driver.Core.Bindings;
using MongoDB.Driver.Core.Operations;
using MongoDB.Driver.Core.WireProtocol.Messages.Encoders;
using Netgun.Model;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace Netgun
{
    public class MongoConnection
    {
        private readonly MongoClient _client;

        public MongoConnection(string connectionString)
        {
            _client = new MongoClient(connectionString);
        }

        public Server Server { get; set; }

        public Task<List<BsonDocument>> GetDocuments(string db, string collection)
        {
            return this._client.GetDatabase(db).GetCollection<BsonDocument>(collection).Find(b => true).ToListAsync();
        }

        public Task<BsonValue> Eval(string db, string js)
        {
            var dbNamespace = new DatabaseNamespace(db);
            var encodeSettings = new MessageEncoderSettings
            {
                { MessageEncoderSettingsName.GuidRepresentation, GuidRepresentation.Standard},
                { MessageEncoderSettingsName.ReadEncoding, Utf8Encodings.Strict },
                { MessageEncoderSettingsName.WriteEncoding, Utf8Encodings.Strict }
            };
            var op = new EvalOperation(dbNamespace, new BsonJavaScript(js), encodeSettings);

            using(var binding = new WritableServerBinding(this._client.Cluster))
            {
                return  op.ExecuteAsync(binding, CancellationToken.None);
            }
        }

        async public Task Populate()
        {
            Server = new Server();
            var dbCursor = await _client.ListDatabasesAsync();
            Server.Name = _client.Cluster.Description.Servers.First().EndPoint.ToString();
            await dbCursor.ForEachAsync(async dbBson =>
            {
                var dbName = dbBson["name"].AsString;
                var db = new Database { Name = dbName, Collections = new List<Collection>() };
                Server.Databases.Add(db);
                var collectionCursor = await _client.GetDatabase(dbName).ListCollectionsAsync();
                await
                    collectionCursor.ForEachAsync(
                        collectionBson => db.Collections.Add(new Collection { Name = collectionBson["name"].AsString, DatabaseName = dbName, ConnectionName = Server.Name}));
            });
        }
    }
}
