using System.Collections.Generic;
using System.Linq;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Input;
using Netgun.Model;

namespace Netgun.Controls
{
    /// <summary>
    /// Interaction logic for DocumentsTab.xaml
    /// </summary>
    public partial class DocumentsTab : TabItem
    {
        private CollectionViewSource _documentSource;
        private string _db;
        private MongoConnection _connection;

        public DocumentsTab(string db, string collection, MongoConnection conn, List<Document> source)
        {
            InitializeComponent();
            this.TabName = string.Format("{0}.{1}", db, collection);
            _documentSource = (CollectionViewSource)this.FindResource("DocumentCollectionViewSource");
            _documentSource.Source = source;
            _db = db;
            _connection = conn;
            this.DataContext = this;
        }

        public string TabName { get; set; }

        public Terminal Terminal { get { return (Terminal) this.WinFormsHost.Child; } }

        public ICommand RunCommand { get { return new ActionCommand(this.Run, Key.F5);} }

        private void CloseTab_Click(object sender, System.Windows.RoutedEventArgs e)
        {
            var tabs = (TabControl)this.Parent;
            tabs.Items.Remove(this);
        }

        private void Run()
        {
            _documentSource.Source = this._connection.Eval(this._db, this.Terminal.Text).Select(Document.FromBsonDocument).ToList();
            _documentSource.View.Refresh();
        }
    }
}
