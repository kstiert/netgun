using System;
using System.Diagnostics;
using System.Net;
using System.Text.RegularExpressions;
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
            Server = new Server();
            this.ConnectionId = Guid.NewGuid().ToString();
        }

        public string ConnectionId { get; private set; }

        public Server Server { get; set; }

        public Task<List<BsonDocument>> GetDocuments(string db, string collection)
        {
            return this._client.GetDatabase(db).GetCollection<BsonDocument>(collection).Find(b => true).ToListAsync();
        }

        public List<BsonDocument> Eval(string db, string js)
        {
            // TODO: factor this out into something more flexible
            var shell = new Process();
            var output = new List<BsonDocument>();
            shell.StartInfo.FileName = "mongo.exe";
            shell.StartInfo.Arguments = "--quiet ";
            shell.StartInfo.Arguments += _url.Username != null ? string.Format("-u {0} ", _url.Username) : string.Empty;
            shell.StartInfo.Arguments += _url.Password!= null ? string.Format("-p {0} ", _url.Password) : string.Empty;
            shell.StartInfo.Arguments += _url.AuthenticationSource != null ? string.Format("--authenticationDatabase {0} ", _url.AuthenticationSource) : string.Empty;
            shell.StartInfo.Arguments += string.Format("{0}/{1}", _client.GetServer().Primary.Address, db);
            shell.StartInfo.UseShellExecute = false;
            shell.StartInfo.CreateNoWindow = true;
            shell.StartInfo.RedirectStandardOutput = true;
            shell.StartInfo.RedirectStandardInput = true;
            shell.StartInfo.RedirectStandardError = true;
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
            Server.Loading = true;
            Server.Name = "Loading...";
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
                        collectionBson => db.Collections.Add(new Collection { Name = collectionBson["name"].AsString, DatabaseName = dbName, ConnectionId = this.ConnectionId}));
                db.Collections = db.Collections.OrderBy(c => c.Name).ToList();
            });
            Server.Databases = Server.Databases.OrderBy(db => db.Name).ToList();
            Server.Loading = false;
        }
    }
}
