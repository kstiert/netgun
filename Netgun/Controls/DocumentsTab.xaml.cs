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

        public DocumentsTab(string header, List<Document> source)
        {
            InitializeComponent();
            this.Header = header;
            _documentSource = (CollectionViewSource)this.FindResource("DocumentCollectionViewSource");
            _documentSource.Source = source;
        }
    }
}
