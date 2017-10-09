using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Diagnostics;
using System.Linq;
using System.ServiceProcess;
using System.Text;
using System.Threading.Tasks;
using murrayju.ProcessExtensions;

namespace RemoteControlService
{
    public partial class RemoteControlWindowsService : ServiceBase
    {
        private ControlledDevice _device;
        public RemoteControlWindowsService()
        {
            InitializeComponent();
           _device = new ControlledDevice(eventLog1);
            AppDomain.CurrentDomain.UnhandledException += CurrentDomainOnUnhandledException;

        }

        private void CurrentDomainOnUnhandledException(object sender, UnhandledExceptionEventArgs unhandledExceptionEventArgs)
        {
            _device = new ControlledDevice(eventLog1);
            _device.OnStart();
            
        }


        protected override void OnStart(string[] args)
        {
            _device.OnStart();
        }

        protected override void OnStop()
        {
          _device.OnStop();
        }

        private void eventLog1_EntryWritten(object sender, EntryWrittenEventArgs e)
        {

        }
    }
}
