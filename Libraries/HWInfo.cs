using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.ComponentModel;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Runtime.CompilerServices;
using Microsoft.VisualBasic.Devices;
using System.Management;
using System.Net;
using System.Net.NetworkInformation;

namespace DeviceAppLibralies
{
    /// <summary>
    /// This class is Singleton and keep information about hardware information of this pc
    /// </summary>
    public class HwInfo
    {
        private static HwInfo _instatnce;

        private ComputerInfo _ci;
        private PerformanceCounter _totalCpu;
        private string[] _cpuInfo = null;
        public string CpuSerialNumber { get; private set; }
        public string ComputerName => Environment.MachineName;
        private string _mac;

        public string MacAddress
        {
            get
            {
                if (string.IsNullOrEmpty(_mac))
                {
                    _mac = NetworkInterface.GetAllNetworkInterfaces()                   //.Contains"irtual" kvoli virtualnym adapterom v virtualBoxu a pod.
                        .Where(nic => nic.OperationalStatus == OperationalStatus.Up && !nic.Description.Contains("irtual"))
                        .Select(nic => nic.GetPhysicalAddress().ToString())
                        .FirstOrDefault();
                }

                return _mac;
            }
        }

        private HwInfo()
        {
            this._ci = new ComputerInfo();
            this._totalCpu = new PerformanceCounter("Processor", "% Processor Time", "_Total");
            _totalCpu.NextValue();
           
            _cpuInfo = GetCpuInfo();
        }


        /// <summary>
        /// Get instance of HwInfo
        /// </summary>
        /// <returns>Singleton instance</returns>
        public static HwInfo GetInstance()
        {
            if (_instatnce == null)
            {
                _instatnce = new HwInfo();
            }

            return _instatnce;
        }


        /// <summary>
        /// Get info about drives on this computer in form of: Name AvailableFreeSpace/TotalSize Gb
        /// </summary>
        /// <returns>List of strings  with information about drives</returns>
        public List<string> GetDrivesInfo()
        {
            DriveInfo[] drives = DriveInfo.GetDrives();
            List<string> driveInfo = new List<string>();
            foreach (DriveInfo drive in drives)
            {
                if (drive.IsReady)
                    driveInfo.Add(
                        $"{drive.Name} {(double) drive.AvailableFreeSpace/(1024*1024*1024):0.00} of {(double) drive.TotalSize/(1024*1024*1024):0.00} GB");
            }
            return driveInfo;
        }


        /// <summary>
        /// Get actual percentual usage of cpu
        /// </summary>
        /// <returns>usage of cpu in %</returns>
        public float GetCpUsage()
        {
            PerformanceCounter totalCpu = new PerformanceCounter("Processor", "% Processor Time", "_Total");
            totalCpu.NextValue();
            System.Threading.Thread.Sleep(250);
            return totalCpu.NextValue();
        }

        /// <summary>
        /// Get actual usage of Ramin array
        /// </summary>
        /// <returns> float[0] = available Memory size in Gb; float[1]= Total memory size in Gb  </returns>
        public float[] GetRamUsage()
        {
            ulong totalMem = _ci.TotalPhysicalMemory;
            ulong avaiMem = _ci.AvailablePhysicalMemory;

            float[] mem = new[] {(float) avaiMem/(1024*1024), (float) totalMem/(1024*1024)};

            return mem;
        }

        /// <summary>
        /// Get information about windows operation system on this computer in string array
        /// </summary>
        /// <returns>string[0]=Os full name; string[1]= OS platform; string[2]= Os version    </returns>
        public string[] GetOsInfo()
        {
            return new[] {_ci.OSFullName, _ci.OSPlatform, _ci.OSVersion};
        }

        /// <summary>
        /// Get information about Spu on this computer in string array
        /// And set property CpuSerialNumber
        /// </summary>
        /// <returns>string[0]=Cpu name; string[1]= Number of cores</returns>
        public string[] GetCpuInfo()
        {
            if (this._cpuInfo != null)
            {
                return this._cpuInfo;
            }
            ManagementObjectSearcher mos =
                new ManagementObjectSearcher("Select ProcessorID, Name, NumberOfCores from Win32_Processor");

            string[] info = null;
            foreach (var nieco in mos.Get())
            {
                info = new[] {nieco["Name"].ToString(), nieco["NumberOfCores"].ToString()};
                CpuSerialNumber = nieco["ProcessorID"].ToString();
            }
            this._cpuInfo = info;
            return this._cpuInfo;
        }
    }
}