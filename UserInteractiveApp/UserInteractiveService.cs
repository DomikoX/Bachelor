using System;
using System.Collections.Generic;
using System.Linq;
using System.ServiceModel;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Media;
using System.Windows.Media.Effects;
using ClientPCService;
using DeviceAppLibralies;
using murrayju.ProcessExtensions;
using RemoteWcfService;

namespace UserInteractiveApp
{

    [ServiceContract(CallbackContract = typeof(IUserinteractiveServiceCallback), SessionMode = SessionMode.Required)]
    public interface IUserinteractiveService
    {
        [OperationContract(IsOneWay = true)]
        void Connect();

    }


    [ServiceContract]
    public interface IUserinteractiveServiceCallback
    {
        [OperationContract]
        byte[] GetScreen(params object[] objects);

        [OperationContract(IsOneWay = true)]
        void PerformOperation(Operation operation);

        [OperationContract(IsOneWay = true)]
        void Block();

        [OperationContract(IsOneWay = true)]
        void UnBlock();

        [OperationContract(IsOneWay = true)]
        void SendMessage(string title, string messaqge);

        [OperationContract]
        Task<ProcessRecord[]> GetRunningProcesses();

        [OperationContract]
        Task<ExplorerRecord[]> RunExplorer(string clientId);
        [OperationContract]
        Task<ExplorerRecord[]> SelectDirectory(string clientId, int index);


    }





    public class UserInteractiveService : IUserinteractiveServiceCallback
    {
        private ScreenInteractor _screenInteractor = new ScreenInteractor();
        private ScreenManager _screenManager = new ScreenManager();
        private Dictionary<string,FileExplorer> _explorers = new Dictionary<string, FileExplorer>();


        public Task<ExplorerRecord[]> RunExplorer(string clientId)
        {
            return Task.Run(() =>
            {
                _explorers.Remove(clientId);
                FileExplorer fe = new FileExplorer();
                _explorers.Add(clientId, fe);
                return fe.Open(-1);
            });
        }

        public Task<ExplorerRecord[]> SelectDirectory(string clientId, int index)
        {
            return Task.Run(() =>
            {
                FileExplorer fe;
                if (_explorers.TryGetValue(clientId, out fe))
                {
                    return fe.Open(index);
                }
                return null;
            });
        }



        public byte[] GetScreen(params object[] objects)
        {
            
            return _screenManager.GetNextScreen(objects);
        }

        public void PerformOperation(Operation operation)
        {
            switch (operation)
            {
                case Operation.Lock:
                    PcControl.Lock();
                    break;
                case Operation.Logout:
                    PcControl.LogOut();
                    break;
                case Operation.Sleep:
                    PcControl.Sleep();
                    break;
                case Operation.ShutDown:
                    PcControl.ShutDown();
                    break;
                case Operation.Restart:
                    PcControl.Restart();
                    break;
            }
        }

        public void Block()
        {
            _screenInteractor.Block();
        }

        public void UnBlock()
        {
            _screenInteractor.Unblock();
        }

        public void SendMessage(string title, string messaqge)
        {
            _screenInteractor.ShowMessage(title, messaqge);
        }

        public Task<ProcessRecord[]> GetRunningProcesses()
        {
            return Task.Run(() => PcControl.GetRunningRecords());
        }
    }
}