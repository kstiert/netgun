using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Input;
using Netgun.Model;
using System.Windows.Media;

namespace Netgun.Controls
{
    /// <summary>
    /// Interaction logic for DocumentsTab.xaml
    /// </summary>
    public partial class DocumentsTab : TabItem, INotifyPropertyChanged
    {
        private CollectionViewSource _documentSource;
        private string _db;
        private MongoConnection _connection;
        private List<Document> _source;
        private int _page, _pageCount;

        public event PropertyChangedEventHandler PropertyChanged;

        public DocumentsTab(string db, string collection, MongoConnection conn, List<Document> source)
        {
            _page = 0;
            _pageCount = 50;
            _source = source;
            InitializeComponent();
            this.TabName = string.Format("{0}.{1}", db, collection);
            _documentSource = (CollectionViewSource)this.FindResource("DocumentCollectionViewSource");
            _db = db;
            _connection = conn;
            this.DataContext = this;
            StatusLabel.Content = string.Format("{0} Results", _source.Count);
            StatusLabel.Background = Brushes.LightBlue;
            Refresh();
        }

        public string TabName { get; set; }

        public bool LeftDisabled { get { return _page == 0; } }

        public bool RightDisabled { get { return _page + 1 == TotalPages; } }

        public int TotalPages 
        { 
            get
            {
                return (_source.Count / _pageCount) + (_source.Count % _pageCount != 0 ? 1 : 0);
            }
        }

        public Terminal Terminal { get { return (Terminal) this.WinFormsHost.Child; } }

        public ActionCommand RunCommand { get { return new ActionCommand(this.Run); } }
        public ActionCommand RightCommand { get { return new ActionCommand(this.PageRight); } }
        public ActionCommand LeftCommand { get { return new ActionCommand(this.PageLeft); } }

        public void Run()
        {
            try
            {
                _source = this._connection.Eval(this._db, this.Terminal.Text).Select(Document.FromBsonDocument).ToList();
                StatusLabel.Content = string.Format("{0} Results", _source.Count);
                StatusLabel.Background = Brushes.LightBlue;
            }
            catch(MongoConsoleException e)
            {
                _source = new List<Document>();
                StatusLabel.Content = e.Message;
                StatusLabel.Background = Brushes.OrangeRed;
            }
            
            Refresh();
        }

        private void Refresh()
        {

            _documentSource.Source = _source.Skip(_page * _pageCount).Take(_pageCount).ToList();
            _documentSource.View.Refresh();
            this.PagingLabel.Content = string.Format("{0} of {1}", _page + 1, TotalPages);
        }

        private void PageRight()
        {
            if(_page + 1 < TotalPages)
            {
                _page++;
                OnPropertyChanged("LeftDisabled");
                OnPropertyChanged("RightDisabled");
                Refresh();
            }
        }

        private void PageLeft()
        {
            if(_page > 0)
            {
                _page--;
                OnPropertyChanged("LeftDisabled");
                OnPropertyChanged("RightDisabled");
                Refresh();
            }
        }

        private void CloseTab_Click(object sender, System.Windows.RoutedEventArgs e)
        {
            var tabs = (TabControl)this.Parent;
            tabs.Items.Remove(this);
        }

        private void MainGrid_OnPreviewMouseWheel(object sender, MouseWheelEventArgs e)
        {
            // Always use mouse wheel to scroll
            var scroll = MainGrid.GetScrollbar();
            scroll.ScrollToVerticalOffset(scroll.VerticalOffset - e.Delta);
        }

        protected virtual void OnPropertyChanged(string propertyName)
        {
            PropertyChangedEventHandler handler = PropertyChanged;
            if (handler != null) handler(this, new PropertyChangedEventArgs(propertyName));
        }

        private void TabHeader_OnMouseDown(object sender, MouseButtonEventArgs e)
        {
            if (e.ChangedButton != MouseButton.Middle) return;
            var tabs = (TabControl)this.Parent;
            tabs.Items.Remove(this);
        }
    }
}
