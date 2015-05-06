using System.Collections.Generic;
using System.Windows.Controls;
using System.Windows.Data;
using Netgun.Model;

namespace Netgun.Controls
{
    /// <summary>
    /// Interaction logic for DocumentsTab.xaml
    /// </summary>
    public partial class DocumentsTab : TabItem
    {
        private CollectionViewSource _documentSource;

        public DocumentsTab(string tabName, List<Document> source)
        {
            InitializeComponent();
            this.TabName = tabName;
            _documentSource = (CollectionViewSource)this.FindResource("DocumentCollectionViewSource");
            _documentSource.Source = source;
            this.DataContext = this;
        }

        public string TabName { get; set; }

        private void CloseTab_Click(object sender, System.Windows.RoutedEventArgs e)
        {
            var tabs = (TabControl)this.Parent;
            tabs.Items.Remove(this);
        }
    }
}
