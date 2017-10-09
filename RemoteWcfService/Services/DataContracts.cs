using System;
using System.Runtime.Serialization;

namespace RemoteWcfService
{



    [DataContract]
    public struct DeviceUsage
    {
        [DataMember]
        public float CpuUsage { get; set; }
        [DataMember]
        public float[] RamUsage { get; set; }
    }

    [DataContract]
    public struct DeviceInfo
    {
        [DataMember]
        public string Id;
        [DataMember]
        public string Name;
        [DataMember]
        public string OsInfo;
        [DataMember]
        public string CpuInfo;
        [DataMember]
        public string MacAddress;
        [DataMember]
        public bool Online;
        [DataMember]
        public bool Mark;

       

        public DeviceInfo(DataModels.Device device, bool isOnline)
        {
            Id = device.Id;
            Name = device.Name;
            OsInfo = device.OsInfo;
            CpuInfo = device.CpuInfo;
            MacAddress = device.MacAddress;
            Online = isOnline;
            Mark = Online;
        }
        public DeviceInfo(DataModels.Device device) :this(device,false)
        {
        }
    }


    [DataContract]
    public enum Operation
    {
        [EnumMember]
        Restart,
        [EnumMember]
        ShutDown,
        [EnumMember]
        Logout,
        [EnumMember]
        Lock,
        [EnumMember]
        Sleep
    };



    [DataContract]
    public struct ProcessRecord
    {
        [DataMember] public string Title { get; set; }
        [DataMember] public string Name { get; set; }
        [DataMember] public int Pid { get; set; }
       
    }

    [DataContract]
    public struct ExplorerRecord
    {
        [DataMember]
        public string Name { get; set; }

        [DataMember(IsRequired = false, EmitDefaultValue = false)]
        public DateTime Date { get; set; }

        [DataMember]
        public long Size { get; set; }
    }
}