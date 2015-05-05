using System.Collections.Generic;
using System.Windows;
using System.Windows.Controls;
using MongoDB.Driver;
using Netgun.Model;

namespace Netgun
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        private readonly MongoClient _client;

        private readonly MongoServer _server;

        public  MainWindow()
        {
            InitializeComponent();
            _client = new MongoClient();
            _server = new MongoServer();
            this.Populate();
        }

        public void Refresh()
        {
            TreeRoot.Items.Clear();
            foreach (var db in _server.Databases)
            {
                var dbItem = new TreeViewItem { Header = db.Name };
                TreeRoot.Items.Add(dbItem);
                foreach (var collection in db.Collections)
                {
                    dbItem.Items.Add(new TreeViewItem {Header = collection.Name});
                }
            }
        }

        async public void Populate()
        {
            _server.Databases = new List<MongoDatabase>();
            var dbCursor = await _client.ListDatabasesAsync();
            await dbCursor.ForEachAsync(async dbBson =>
            {
                var dbName = dbBson["name"].AsString;
                var db = new MongoDatabase {Name = dbName, Collections = new List<MongoCollection>()};
                _server.Databases.Add(db);

                var collectionCursor = await _client.GetDatabase(dbName).ListCollectionsAsync();
                await
                    collectionCursor.ForEachAsync(
                        collectionBson => db.Collections.Add(new MongoCollection {Name = collectionBson["name"].AsString}));
            });
            Dispatcher.Invoke(Refresh);
        }
    }
}
