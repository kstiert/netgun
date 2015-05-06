using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using MongoDB.Driver;
using Netgun.Model;
using System.Windows.Input;
using MongoDB.Bson;
using MongoDB.Bson.IO;
using Netgun.Controls;

namespace Netgun
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        private MongoConnection _connection;

        public  MainWindow()
        {
            InitializeComponent();
        }

        async public void Refresh()
        {
            await _connection.Populate();
            TreeRoot.Items.Clear();
            foreach (var db in _connection.Server.Databases)
            {
                var dbItem = new TreeViewItem { Header = db.Name };
                TreeRoot.Items.Add(dbItem);
                foreach (var collection in db.Collections)
                {
                    var collectionItem = new TreeViewItem { Header = collection.Name };
                    collectionItem.MouseDoubleClick += CollectionDoubleClick;
                    dbItem.Items.Add(collectionItem);
                }
            }
        }

        async private void CollectionDoubleClick(object sender, MouseButtonEventArgs e)
        {
            var item = (TreeViewItem)sender;
            var parent = (TreeViewItem)item.Parent;
            var documents = await this._connection.GetDocuments(parent.Header.ToString(), item.Header.ToString());
            MainGrid.ItemsSource = documents.Select(d => new { Document = d.ToJson() });
        }

        private void MenuExit_Click(object sender, RoutedEventArgs e)
        {
            this.Close();
        }

        private void MenuNewConnection_Click(object sender, RoutedEventArgs e)
        {
            var newConnection = new NewConnectionDialog();
            if(newConnection.ShowDialog() ?? false)
            {
                this._connection = new MongoConnection(newConnection.ConnectionString.Text);
                Refresh();
            }
        }
    }
}
