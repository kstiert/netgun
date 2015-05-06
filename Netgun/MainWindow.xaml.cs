using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using MongoDB.Bson.Serialization.Conventions;
using Netgun.Model;
using System.Windows.Input;
using MongoDB.Bson;
using Netgun.Controls;

namespace Netgun
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        public List<MongoConnection> Connections { get; set; } 

        public  MainWindow()
        {
            InitializeComponent();
            Connections = new List<MongoConnection>();
            this.ConnectionTree.DataContext = this;
        }

        public void RefreshTree()
        {
            this.ConnectionTree.Items.Refresh();
        }

        async private void CollectionDoubleClick(object sender, MouseButtonEventArgs e)
        {
            var collection = (Collection) (sender as ItemsControl).Tag;
            var documents = await this.Connections.Single(c => c.Server.Name == collection.ConnectionName).GetDocuments(collection.DatabaseName, collection.Name);
            var newTab = new DocumentsTab(string.Format("{0}.{1}", collection.DatabaseName, collection.Name), documents.Select(d => new Document { Raw = d.ToJson() }).ToList());
            MainTab.Items.Add(newTab);
            MainTab.SelectedItem = newTab;
        }

        private void MenuExit_Click(object sender, RoutedEventArgs e)
        {
            this.Close();
        }

        async private void MenuNewConnection_Click(object sender, RoutedEventArgs e)
        {
            var newConnection = new NewConnectionDialog();
            if(newConnection.ShowDialog() ?? false)
            {
                var conn = new MongoConnection(newConnection.ConnectionString.Text);
                await conn.Populate();
                this.Connections.Add(conn);
                RefreshTree();
            }
        }
    }
}
