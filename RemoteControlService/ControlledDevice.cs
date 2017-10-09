using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.ServiceModel;
using System.Threading;
using System.Threading.Tasks;
using System.Timers;
using ClientPCService;
using DeviceAppLibralies;
using RemoteWcfService;
using murrayju.ProcessExtensions;
using UserInteractiveApp;
using Timer = System.Timers.Timer;

namespace RemoteControlService
{
    [ServiceBehavior(InstanceContextMode = InstanceContextMode.Single)]
    public class ControlledDevice : IDeviceCallback, IUserinteractiveService
    {
        // intercommunication
        private ServiceHost _host;
        private IUserinteractiveServiceCallback Callback;


        //Connection properties
        private IDeviceControlService _deviceControlService;
        private IFileTransferService _fileTransferService;
        private DuplexChannelFactory<IDeviceControlService> _channelFactory;
        private ChannelFactory<IFileTransferService> _fileTransferChannelFactory;
        private readonly Timer _connectionTimer = new Timer(60000*5);

        //Program features properties
        private HwInfo _hwInfo = HwInfo.GetInstance();
        private readonly Timer _screenTimer = new Timer(200);
        private readonly Timer _usageTimer = new Timer(1000);
        private Dictionary<string, FileExplorer> _explorers = new Dictionary<string, FileExplorer>();
        private ScreenManager _screenManager = new ScreenManager();
        private ScreenInteractor _screenInteractor = new ScreenInteractor();
        private EventLog _eventLog;

        private string Id => _hwInfo.CpuSerialNumber;

        public ControlledDevice(EventLog el)
        {

            _eventLog = el;

            _channelFactory = new DuplexChannelFactory<IDeviceControlService>(new InstanceContext(this),
                "MyNetTcpEndpoint");
            _channelFactory.Credentials.UserName.UserName = "Device";
            _channelFactory.Credentials.UserName.Password = "heslo@123.sk";
            //TODO pripojit meno a heslo ktore ziskame z DLL 

            _fileTransferChannelFactory = new ChannelFactory<IFileTransferService>("FiletransferEndpoint");

            _connectionTimer.Elapsed += ConnectionTimerTimerOnElapsed;
            _screenTimer.Elapsed += ScreenTimerOnElapsed;
            _usageTimer.Elapsed += UsageTimerOnElapsed;

            try
            {
                _host = new ServiceHost(this, new Uri("net.pipe://localhost"));
                _host.AddServiceEndpoint(typeof(IUserinteractiveService),
                    new NetNamedPipeBinding() {MaxReceivedMessageSize = 655360000, MaxBufferSize = 655360000},
                    "RemoteUserinteractivePipe");
                _host.Open();
                //Star inivisble program for user interaction
                StartUserinteractionprogram();
            }
            catch (Exception e)
            {
                _eventLog.WriteEntry("Exception in Contructor while trying  to create net.pipe host: " + e.Message);
            }
        }

        private void StartUserinteractionprogram()
        {
            PcControl.KillProcess("UserInteractiveApp");
            // ProcessExtensions.StartProcessAsCurrentUser(@"C:\Users\DomikoX\Source\Repos\Bakalarka\ClientControll\UserInteractiveApp\bin\Debug\UserInteractiveApp.exe");
            string path = Path.Combine(System.AppDomain.CurrentDomain.BaseDirectory, "UserInteractiveApp.exe");
            ProcessExtensions.StartProcessAsCurrentUser(path);

            //C:\Program Files (x86)\Doxisko\Remote computer management system\Interactive App
        }


        private void Load()
        {
            _deviceControlService = _channelFactory.CreateChannel();
            // _userInteractiveService = _userInteractiveFactory.CreateChannel();
            // _fileTransferService = _fileTransferChannelFactory.CreateChannel();
        }


        private void ConnectionTimerTimerOnElapsed(object sender, ElapsedEventArgs elapsedEventArgs)
        {
            try
            {
                if (!_deviceControlService.Ping(Id))
                {
                    _deviceControlService.Register(Id, _hwInfo.ComputerName, String.Join(", ",_hwInfo.GetCpuInfo()), String.Join(", ",_hwInfo.GetOsInfo()), _hwInfo.MacAddress);
                }
            }
            catch (Exception )
            {
                _connectionTimer.Interval = 30000;
                Load();
                return;
            }

            _connectionTimer.Interval = 60000*5;
        }


        public void OnStart()
        {
            while (_channelFactory.State != CommunicationState.Opened)
            {
                Load();
                if (_channelFactory.State != CommunicationState.Opened) Thread.Sleep(5000);
            }
            _deviceControlService.Register(Id, _hwInfo.ComputerName, String.Join(", ", _hwInfo.GetCpuInfo()), String.Join(", ", _hwInfo.GetOsInfo()), _hwInfo.MacAddress);

            _connectionTimer.Start();
        }

        public void OnStop()
        {
            _host.Close();
            _deviceControlService.UnRegister(Id);
            _channelFactory.Close();
        }


        private void ScreenTimerOnElapsed(object sender, ElapsedEventArgs elapsedEventArgs)
        {
            try
            {
                //TODO potreba nastaviť spravne parametre pre GetScreen
                var bitmapBytes = Callback.GetScreen(2f);
                if (bitmapBytes != null)
                {
                    _deviceControlService.NewScreenUpdate(Id, bitmapBytes);
                }
                
            }
            catch (Exception)
            {
                StartUserinteractionprogram();
            }
        }

        private void UsageTimerOnElapsed(object sender, ElapsedEventArgs elapsedEventArgs)
        {
            _deviceControlService.NewUsageUpdate(Id, _hwInfo.GetCpUsage(), _hwInfo.GetRamUsage());
        }


        public void StartScreenStreaming()
        {
            if (_screenTimer.Enabled) return;
            _screenTimer.Start();
        }

        public void StopScreenStreaming()
        {
            _screenTimer.Stop();
        }

        public void StartUsageStreaming()
        {
            if (_usageTimer.Enabled) return;
            _usageTimer.Start();
        }

        public void StopUsageStreaming()
        {
            _usageTimer.Stop();
        }

        public Task<ProcessRecord[]> GetRunningProcesses()
        {
            try
            {
                return Callback.GetRunningProcesses();
            }
            catch (Exception e)
            {
                _eventLog.WriteEntry("GetRunningProcesses error :" + e.Message);

                Callback = null;
                StartUserinteractionprogram();
                while (Callback == null)
                {
                    Thread.Sleep(100);
                }
                return  GetRunningProcesses();
            }


           // return Task.Run(() => PcControl.GetRunningRecords());
        }

        public void KillProcess(int pid)
        {
            PcControl.KillProcess(pid);
        }

        public Task<ExplorerRecord[]> RunExplorer(string clientId)
        {
            try
            {
                return Callback.RunExplorer(clientId);
            }
            catch (Exception e)
            {
                _eventLog.WriteEntry("RunExplorer error :" + e.Message);

                Callback = null;
                StartUserinteractionprogram();
                while (Callback == null)
                {
                    Thread.Sleep(100);
                }
                return RunExplorer(clientId);
            }
        }

        public Task<ExplorerRecord[]> SelectDirectory(string clientId, int index)
        {
            try
            {
                return Callback.SelectDirectory(clientId,index);
            }
            catch (Exception e)
            {
                _eventLog.WriteEntry("SelectDirectory error :" + e.Message);

                Callback = null;
                StartUserinteractionprogram();
                while (Callback == null)
                {
                    Thread.Sleep(100);
                }
                return SelectDirectory(clientId,index);
            }
        }


        public void PickFile(string clientId, string fileName, bool toOpenFolder)
        {
            Task.Run(() =>
            {
                while (_fileTransferChannelFactory.State != CommunicationState.Opened)
                {
                    _fileTransferService = _fileTransferChannelFactory.CreateChannel();
                    Thread.Sleep(5000);
                }


                FileExplorer fe;
                if (!_explorers.TryGetValue(clientId, out fe))
                {
                    fe = new FileExplorer();
                    _explorers.Add(clientId, fe);
                }

                var downloadedFile = _fileTransferService.DownloadFile(fileName);
                fe.SaveFile(downloadedFile, fileName,toOpenFolder);
            });
        }

       

        public Task<string> UploadFileOnServer(string clientId, string fileName)
        {
            return Task.Run(() =>
            {
                while (_fileTransferChannelFactory.State != CommunicationState.Opened)
                {
                    _fileTransferService = _fileTransferChannelFactory.CreateChannel();
                    Thread.Sleep(5000);
                }


                FileExplorer fe;
                if (_explorers.TryGetValue(clientId, out fe))
                {
                    var f = fe.GeFileToTransfer(fileName);
                    if (f == null) return "FAIL";
                    _fileTransferService.UploadFile(new FileToTransfer() {Name = fileName, File = f});
                   // _deviceControlService.NotifyClientAboutFile(clientId, fileName);
                    return "OK";
                }
                return "FAIL";
            });
        }

        public void PerformOperation(Operation operation)
        {
            StopScreenStreaming();
            StopUsageStreaming();

            try
            {
                switch (operation)
                {
                    case Operation.Lock:
                        Callback.PerformOperation(operation);

                        break;
                    case Operation.Logout:
                        Callback.PerformOperation(operation);

                        break;
                    case Operation.Sleep:
                        Callback.PerformOperation(operation);

                        break;
                    case Operation.ShutDown:
                        PcControl.ShutDown();
                        break;
                    case Operation.Restart:
                        PcControl.Restart();
                        break;
                }
            }
            catch (Exception)
            {

                Callback = null;
                StartUserinteractionprogram();
                while (Callback == null)
                {
                    Thread.Sleep(100);
                }
                PerformOperation(operation);
            }
        }

        public void Block()
        {
            try
            {
                Callback.Block();
            }
            catch (Exception e )
            {
                _eventLog.WriteEntry("BLock error :" + e.Message);
                Callback = null;
                StartUserinteractionprogram();
                while (Callback == null)
                {
                    Thread.Sleep(100);
                }
                Block();
            }
        }

        public void UnBlock()
        {
            try
            {
                Callback.UnBlock();
            }
            catch (Exception e)
            {
                _eventLog.WriteEntry("Unblocks error :" + e.Message);

                Callback = null;
                StartUserinteractionprogram();
                while (Callback == null)
                {
                    Thread.Sleep(100);
                }
                UnBlock();
            }
        }

        public void SendMessage(string title, string messaqge)
        {
            try
            {
                Callback.SendMessage(title, messaqge);
            }
            catch (Exception e)
            {

                _eventLog.WriteEntry("SendMessage error :" + e.Message);
                Callback = null;
                StartUserinteractionprogram();
                while (Callback == null)
                {
                    Thread.Sleep(100);
                }
                SendMessage(title, messaqge);
            }
        }

        public void Connect()
        {
            Callback = OperationContext.Current.GetCallbackChannel<IUserinteractiveServiceCallback>();
        }
    }
}