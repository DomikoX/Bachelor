using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Runtime.InteropServices.ComTypes;
using System.ServiceModel;
using System.ServiceModel.Web;
using NLog;
using RemoteWcfService.DataModels;
using Roles = System.Web.Security.Roles;

namespace RemoteWcfService
{
    [GlobalErrorBehaviorAttribute(typeof(GlobalErrorHandler))]
    [ServiceBehavior(InstanceContextMode = InstanceContextMode.Single)]
    public partial class DeviceControlService : IDeviceControlService
    {
        private static readonly Logger Logger = LogManager.GetCurrentClassLogger();

        IDeviceCallback DeviceCallback => OperationContext.Current.GetCallbackChannel<IDeviceCallback>();
        
        private readonly Dictionary<string, Device> _registeredDevices = new Dictionary<string, Device>();


        public void Register(string id, string name, string cpuInfo, string osInfo, string macAddress)
        {
            using (var db = new RemConDB())
            {
                var query = db.Devices.Where(d => d.Id == id);

                DataModels.Device device = query.DefaultIfEmpty(null).SingleOrDefault();
                if (device == null)
                {
                    device = new DataModels.Device()
                    {
                        Id = id,
                        Name = name,
                        OsInfo = osInfo,
                        CpuInfo = cpuInfo,
                        MacAddress = macAddress
                    };
                    db.Devices.Add(device);
                   var admins =  db.Users.Where(user => user.Role == DataModels.Roles.Admin);

                    foreach (User admin in admins)
                    {
                        admin.AssignedDevices.Add(device);
                    }


                }
                else
                {
                    device.Name = name;
                    device.CpuInfo = cpuInfo;
                    device.OsInfo = osInfo;
                    device.MacAddress = macAddress;
                }

                
                db.SaveChanges();
            }
            

            Logger.Info($"Registred device with id: {id}, {name}, {cpuInfo}, {osInfo}, {macAddress}");


            if (_registeredDevices.ContainsKey(id))
            {
                _registeredDevices.Remove(id);
            }
            _registeredDevices.Add(id, new Device(id, DeviceCallback));

          
        }

        public bool Ping(string id)
        {
            Logger.Info($"Ping From  {id}");
            return _registeredDevices.ContainsKey(id);
        }

        public void UnRegister(string id)
        {
            Logger.Info($"UnRegistred device with id: {id}");

            _registeredDevices.Remove(id);
        }

        public void NewScreenUpdate(string deviceId, byte[] bitmap)
        {
            if (bitmap == null)
            {
                Logger.Info($"new EMPTY screen update from Device: {deviceId} ");
                return;
            }

            Logger.Info($"new screen update from Device: {deviceId} of size {bitmap.Length}");

            Device device;
            if (_registeredDevices.TryGetValue(deviceId, out device))
            {
                device.LastScreenUpdate = bitmap;
                return;
            }
            DeviceCallback.StopScreenStreaming();
        }

        public void NewUsageUpdate(string deviceId, float cpu, float[] ram)
        {
            Logger.Info($"new Usage update from Device: {deviceId}");

            Device device;
            if (_registeredDevices.TryGetValue(deviceId, out device))
            {
                device.LastCpuUsage = cpu;
                device.LastRamUsage = ram;
                return;
            }
            DeviceCallback.StopUsageStreaming();
        }

       
    }
}