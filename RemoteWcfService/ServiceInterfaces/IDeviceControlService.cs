using System.ServiceModel;

namespace RemoteWcfService
{
    // NOTE: You can use the "Rename" command on the "Refactor" menu to change the interface name "IService1" in both code and config file together.
    [ServiceContract(CallbackContract = typeof(IDeviceCallback), SessionMode = SessionMode.Required)]
    public interface IDeviceControlService
    {
        [OperationContract(IsOneWay = true)]
        void Register(string id, string name, string cpuInfo, string osInfo, string macAddress);

        [OperationContract]
        bool Ping(string id);

        [OperationContract(IsOneWay = true)]
        void UnRegister(string id);

        [OperationContract(IsOneWay = true)]
        void NewScreenUpdate(string deviceId, byte[] bitmap);

        [OperationContract(IsOneWay = true)]
        void NewUsageUpdate(string deviceId, float cpu, float[] ram);
        

    }
}
