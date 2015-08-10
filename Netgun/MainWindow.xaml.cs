using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using Netgun.Model;
using System.Windows.Input;
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
            this.DataContext = this;
        }

        public ICommand RunCommand 
        {
            get { return new ActionCommand(this.RunSelectedTab); }
        }

        public void RefreshTree()
        {
            this.ConnectionTree.Items.Refresh();
        }

        private void RunSelectedTab()
        {
            var tab = this.MainTab.SelectedItem as DocumentsTab;

            if (tab != null)
            {
                tab.Run();
            }
        }

        async private void CollectionDoubleClick(object sender, MouseButtonEventArgs e)
        {
            var collection = (Collection) (sender as ItemsControl).Tag;
            var connection = this.Connections.Single(c => c.ConnectionId == collection.ConnectionId);
            var documents = await connection.GetDocuments(collection.DatabaseName, collection.Name);
            var newTab = new DocumentsTab(collection.DatabaseName, collection.Name, connection, documents.Select(Document.FromBsonDocument).ToList());
            newTab.Terminal.Text = string.Format("db.{0}.find()", collection.Name);
            MainTab.Items.Add(newTab);
            MainTab.SelectedItem = newTab;
        }

        private void MenuExit_Click(object sender, RoutedEventArgs e)
        {
            this.Close();
        }

        async private void MenuNewConnection_Click(object sender, RoutedEventArgs args)
        {
            var newConnection = new NewConnectionDialog();
            if(newConnection.ShowDialog() ?? false)
            {
                MongoConnection conn = null;
                try
                {
                    conn = new MongoConnection(newConnection.ConnectionString.Text);
                    this.Connections.Add(conn);
                    RefreshTree();
                    await conn.Populate();
                    RefreshTree();
                }
                catch (Exception e)
                {
                    if(conn != null && this.Connections.Contains(conn))
                    {
                        this.Connections.Remove(conn);
                        RefreshTree();
                    }
                    MessageBox.Show(e.Message, "Connection Failed", MessageBoxButton.OK, MessageBoxImage.Error);
                }

            }
        }

        async private void RefreshServer(object sender, RoutedEventArgs e)
        {
            var item = e.Source as MenuItem;
            var connId = item.CommandParameter as string;
            var conn = this.Connections.First(c => c.ConnectionId ==  connId);
            conn.Clear();
            this.RefreshTree();
            await conn.Populate();
            this.RefreshTree();

        }
    }
}
