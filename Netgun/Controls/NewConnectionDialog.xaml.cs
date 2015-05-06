using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;

namespace Netgun.Controls
{
    /// <summary>
    /// Interaction logic for NewConnectionDialog.xaml
    /// </summary>
    public partial class NewConnectionDialog : Window
    {
        public NewConnectionDialog()
        {
            InitializeComponent();
            this.DataContext = this;
        }

        public ActionCommand ConnectCommand { get { return new ActionCommand(this.Connect, Key.Enter); } }

        private void Connect()
        {
            this.DialogResult = true;
            this.Close();
        }
    }
}
