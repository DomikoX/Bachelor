using System;
using System.Collections.Generic;
using System.Runtime.Serialization;
using System.Timers;
using NLog;

namespace RemoteWcfService
{
    public class Device
    {
        //check for unactive clients
        private readonly Timer _checkTimer = new Timer(1000 *60 *30);
        public string Id { get; }
        public IDeviceCallback DeviceCallback { get; }

        private DateTime _lastScreenUpdateTime;
        private byte[] _lastScreenUpdate = null;
        public byte[] LastScreenUpdate
        {
            set
            {
                if (_screenCallbacks.Count == 0)
                {
                    DeviceCallback.StopScreenStreaming();
                }

                _lastScreenUpdateTime =  DateTime.Now;
                _lastScreenUpdate = value;

            }
        }

        private float _lastCpuUsage = 0;

        public float LastCpuUsage
        {
            set
            {
                if (_usageCallbacks.Count == 0)
                {
                    DeviceCallback.StopUsageStreaming();
                }
                _lastCpuUsage = value;
            }
        }

        private float[] _lastRamusage =  {0,0};

        public float[] LastRamUsage
        {
            set
            {
                if (_usageCallbacks.Count == 0)
                {
                    DeviceCallback.StopUsageStreaming();
                }
                _lastRamusage = value;
            }
        }

        private static readonly Logger Logger = LogManager.GetCurrentClassLogger();

        private readonly Dictionary<string, DateTime> _screenCallbacks = new Dictionary<string, DateTime>();
        private readonly Dictionary<string, DateTime> _usageCallbacks = new Dictionary<string, DateTime>();

        public Device(string id, IDeviceCallback deviceCallback)
        {
            Id = id;
            DeviceCallback = deviceCallback;
            _checkTimer.Elapsed += CheckCallbacks;
            _checkTimer.Start();
        }

       

        private void CheckCallbacks(object sender, ElapsedEventArgs elapsedEventArgs)
        {
            //check if there is client whom last request was before more than 30 minues
            foreach (var client in _screenCallbacks)
            {
                if (DateTime.Compare(client.Value, DateTime.Now) > 60*30)
                {
                    RemoveScreenCallback(client.Key);
                }
                
            }
            foreach (var client in _usageCallbacks)
            {
                if (DateTime.Compare(client.Value, DateTime.Now) > 60 * 30)
                {
                    RemoveUsageCallback(client.Key);
                }

            }




        }


        protected bool Equals(Device other)
        {
            return Id == other.Id || DeviceCallback.Equals(other.DeviceCallback);
        }


        public void AddScreenCallback(string clientId)
        {
            _screenCallbacks.Remove(clientId);
            _screenCallbacks.Add(clientId, DateTime.Now);
            DeviceCallback.StartScreenStreaming();
        }

        public void RemoveScreenCallback(string clientId)
        {
            _screenCallbacks.Remove(clientId);
        }


        public byte[] GetLastScreenUpdate(string clientId)
        {
            //last time client get screeen
            DateTime v;
            if (!_screenCallbacks.TryGetValue(clientId, out v))
            {
                v = DateTime.Now;
                _screenCallbacks.Add(clientId, v);
            }


            // if there is newest screen return it 
            if (DateTime.Compare(v, _lastScreenUpdateTime) < 0)
            {
                Logger.Info("GetLastScreenUpdate INSIDE CONDITION: " + v.ToString("0:MM/dd/yy H:mm:ss zzz"));
                _screenCallbacks[clientId] = DateTime.Now;
                return _lastScreenUpdate;
            }
            Logger.Info("GetLastScreenUpdate OUTSIDE CONDITION: " +v.ToString("0:MM/dd/yy H:mm:ss zzz"));
            // or return null
            return null;

        }


        public void AddUsageCallback(string clientId)
        {
            _usageCallbacks.Remove(clientId);
            _usageCallbacks.Add(clientId, DateTime.Now);
            DeviceCallback.StartUsageStreaming();
        }


        public void RemoveUsageCallback(string clientId)
        {
            _usageCallbacks.Remove(clientId);
        }


        public float GetLastCpuUsage(string clientId)
        {
            DateTime v;
            if (!_usageCallbacks.TryGetValue(clientId, out v))
            {
                v = DateTime.Now;
                _usageCallbacks.Add(clientId, v);
            }

            _usageCallbacks[clientId] = DateTime.Now;
            return _lastCpuUsage;
            
        }

        public float[] GetLastRamUsage(string clientId)
        {
            return _lastRamusage;
        }


        public override bool Equals(object obj)
        {
            if (ReferenceEquals(null, obj)) return false;
            if (ReferenceEquals(this, obj)) return true;
            if (obj.GetType() != this.GetType()) return false;
            return Equals((Device) obj);
        }

        public override int GetHashCode()
        {
            return Id.GetHashCode();
        }
    }
}