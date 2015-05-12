using System.Diagnostics;
using System.Net;
using System.Text.RegularExpressions;
using System.Threading;
using System.Windows.Documents;
using MongoDB.Bson;
using MongoDB.Driver;
using MongoDB.Driver.Core.Clusters;
using Netgun.Model;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Netgun
{
    public class MongoConnection
    {
        private readonly MongoClient _client;
        private readonly MongoUrl _url;

        public MongoConnection(string connectionString)
        {
            _url = new MongoUrl(connectionString);
            _client = new MongoClient(_url);
        }

        public Server Server { get; set; }

        public Task<List<BsonDocument>> GetDocuments(string db, string collection)
        {
            return this._client.GetDatabase(db).GetCollection<BsonDocument>(collection).Find(b => true).ToListAsync();
        }

        public List<BsonDocument> Eval(string db, string js)
        {
            var shell = new Process();
            var output = new List<BsonDocument>();
            shell.StartInfo.FileName = "mongo.exe";
            shell.StartInfo.Arguments = string.Format("--quiet -u {0} -p {1} --authenticationDatabase {2} {3}/{4}", _url.Username, _url.Password, _url.AuthenticationSource, _url.Servers.First(), db);
            shell.StartInfo.UseShellExecute = false;
            shell.StartInfo.CreateNoWindow = true;
            shell.StartInfo.RedirectStandardOutput = true;
            shell.StartInfo.RedirectStandardInput = true;
            shell.OutputDataReceived += (sender, args) => {try{output.Add(BsonDocument.Parse(args.Data));}catch{}};
            shell.Start();
            shell.BeginOutputReadLine();
            shell.StandardInput.WriteLine("DBQuery.shellBatchSize = {0}", int.MaxValue);
            shell.StandardInput.WriteLine(Regex.Split(js, "\r\n|\r|\n")[0]);
            shell.StandardInput.WriteLine("exit");
            shell.WaitForExit();
            return output;
        }

        async public Task Populate()
        {
            Server = new Server();
            var dbCursor = await _client.ListDatabasesAsync();
            if (_client.Cluster.Description.Type == ClusterType.ReplicaSet) // TODO: Sharded/Unknown case.
            {
                Server.Name = _client.Cluster.Settings.ReplicaSetName;
            }
            else
            {
                var dns = _client.Cluster.Description.Servers.First().EndPoint as DnsEndPoint;
                Server.Name = dns != null ? string.Format("{0}:{1}", dns.Host, dns.Port) : _client.Cluster.Description.Servers.First().EndPoint.ToString();
            }

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
