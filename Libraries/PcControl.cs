using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.Runtime.InteropServices;
using RemoteWcfService;
using murrayju.ProcessExtensions;

namespace DeviceAppLibralies
{
    public class PcControl
    {
        
        /// <summary>
        /// Restar this computer
        /// </summary>
        public static void Restart()
        {
            Process.Start("shutdown.exe", "-r -f -t 5");
        }
        /// <summary>
        /// Shut down this computer
        /// </summary>
        public static void ShutDown()
        {
            Process.Start("shutdown.exe", "-s -f -t 5");
        }

        /// <summary>
        /// Log out current user
        /// </summary>
        public static void LogOut()
        {
            Process.Start("shutdown.exe", "-l -f -t 5 ");
        }

        /// <summary>
        /// Lock computer
        /// </summary>
        public static void Lock()
        {
            Process.Start("Rundll32.exe", "User32.dll, LockWorkStation");
        }

        /// <summary>
        /// Put this computer into Sleep
        /// </summary>
        public static void Sleep()
        {
            Process.Start("Rundll32.exe", "powrprof.dll, SetSuspendState 0,1,0");
        }


        /// <summary>
        /// Get the list of all running processes
        /// </summary>
        /// <returns>List of running processes</returns>
        public static ProcessRecord[] GetRunningRecords()
        {
            List<ProcessRecord> array = new List<ProcessRecord>();

            foreach (Process process in Process.GetProcesses())
            {
                array.Add(new ProcessRecord()
                {
                    Title = process.MainWindowTitle,
                    Name = process.ProcessName,
                    Pid = process.Id
                });
            }
            array.Sort((a, b) => String.Compare(b.Title, a.Title, StringComparison.Ordinal));

            return array.ToArray();
        }

        /// <summary>
        /// Kill running process by its id
        /// </summary>
        /// <param name="pid"> id of process to be killed </param>
        public static void KillProcess(int pid)
        {
            Process.GetProcessById(pid).Kill();
        }

        /// <summary>
        /// Kill al processes with selected name
        /// </summary>
        /// <param name="name">the name of the procces to be killed</param>
        public static void KillProcess(string name)
        {
            foreach (Process process in Process.GetProcessesByName(name))
            {
                process.Kill();
             }
        }
    }
}