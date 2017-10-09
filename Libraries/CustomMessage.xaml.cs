using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO.Packaging;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Forms.VisualStyles;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace ClientPCService
{
    /// <summary>
    /// Interaction logic for CustomMessage.xaml
    /// </summary>
    public partial class CustomMessage : Window
    {
       public bool CanClose { get; set; } = true;

        public CustomMessage(string ptitle, string pmessage)
        {
            InitializeComponent();
            this.Title = ptitle;
            MessageTitle.Content = ptitle;
            Text.Content = pmessage;
            Topmost = true;
        }

        private void CustomMessage_OnClosing(object sender, CancelEventArgs e)
        {
            e.Cancel = !CanClose;
        }
    }
}