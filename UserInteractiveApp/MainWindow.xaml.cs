using System;
using System.Collections.Generic;
using System.Linq;
using System.ServiceModel;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace UserInteractiveApp
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            UserInteractiveService uis = new UserInteractiveService();
            DuplexChannelFactory<IUserinteractiveService> pipeFactory =
                new DuplexChannelFactory<IUserinteractiveService>(uis,
                    new NetNamedPipeBinding() {MaxReceivedMessageSize = 655360000, MaxBufferSize = 655360000},
                    new EndpointAddress("net.pipe://localhost/RemoteUserinteractivePipe"));

            var channel = pipeFactory.CreateChannel();
            channel.Connect();


            this.Visibility = Visibility.Hidden;
        }

        private void MainWindow_OnClosed(object sender, EventArgs e)
        {
        }
    }
}