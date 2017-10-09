using System.IO;
using System.ServiceModel;
using System.Threading.Tasks;

namespace RemoteWcfService
{
    public interface IDeviceCallback
    {
        [OperationContract(IsOneWay = true)]
        void StartScreenStreaming();

        [OperationContract(IsOneWay = true)]
        void StopScreenStreaming();

        [OperationContract(IsOneWay = true)]
        void StartUsageStreaming();

        [OperationContract(IsOneWay = true)]
        void StopUsageStreaming();

        [OperationContract]
        Task<ProcessRecord[]> GetRunningProcesses();

        [OperationContract(IsOneWay = true)]
        void KillProcess(int pid);

        [OperationContract]
        Task<ExplorerRecord[]> RunExplorer(string clientId);
        

        [OperationContract]
        Task<ExplorerRecord[]> SelectDirectory(string clientId, int index);

        [OperationContract(IsOneWay = true)]
        void PickFile(string clientId, string fileName, bool toOpenFolder);

        [OperationContract]
        Task<string> UploadFileOnServer(string clientId, string fileName);
        
        [OperationContract(IsOneWay = true)]
        void PerformOperation(Operation operation);

        [OperationContract(IsOneWay = true)]
        void Block();

        [OperationContract(IsOneWay = true)]
        void UnBlock();

        [OperationContract(IsOneWay = true)]
        void SendMessage(string title, string messaqge);

    }
}
