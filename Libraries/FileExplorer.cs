using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Threading.Tasks;
using murrayju.ProcessExtensions;
using RemoteWcfService;

namespace ClientPCService
{
    public class FileExplorer
    {
        public DriveInfo[] Drives => DriveInfo.GetDrives();
        public DirectoryInfo ActualDirectory { get; private set; }


        public ExplorerRecord[] Open(int index)
        {
            if (index < 0 && ActualDirectory == null) // Just open Explorer Not selected Drive
            {
                return PackDriveRecords();
            }

            if (ActualDirectory == null)
            {
                if (index > Drives.Length) return null;
                if (!Drives[index].IsReady) return null;
                ActualDirectory = Drives[index].RootDirectory;
                return PackDirectoryRecords();
            }

            if (index == 0)
            {
                if (ActualDirectory.Parent == null)
                {
                    ActualDirectory = null;
                    return PackDriveRecords();
                }
                ActualDirectory = ActualDirectory.Parent;
                return PackDirectoryRecords();
            }

            if (index - 1 > ActualDirectory.GetDirectories().Length)
            {//try to open file like photo or .exe and etc.
                try
                {
                    int fileIndex = index - ActualDirectory.GetDirectories().Length -1;
                    FileInfo fi = ActualDirectory.GetFiles()[fileIndex];
                    
                    Process.Start(fi.FullName);
                }
                catch (Exception)
                {
                }
                
                return null;
            }
            ActualDirectory = ActualDirectory.GetDirectories()[index - 1];
            return PackDirectoryRecords();
        }

        private ExplorerRecord[] PackDriveRecords()
        {
            List<ExplorerRecord> array = new List<ExplorerRecord>();
            for (int i = 0; i < Drives.Length; i++)
            {
                var er = new ExplorerRecord();
                er.Name = Drives[i].Name;
                er.Size = Drives[i].IsReady ? Drives[i].TotalSize : 0;
                array.Add(er);
            }

            return array.ToArray();
        }


        private ExplorerRecord[] PackDirectoryRecords()
        {
            List<ExplorerRecord> array = new List<ExplorerRecord>();
            array.Add(new ExplorerRecord() {Name = "...", Date = DateTime.Now});

            foreach (DirectoryInfo directoryInfo in ActualDirectory.GetDirectories())
            {
                array.Add(new ExplorerRecord() {Name = directoryInfo.Name, Date = directoryInfo.CreationTime});
            }
            foreach (FileInfo fileInfo in ActualDirectory.GetFiles())
            {
                array.Add(new ExplorerRecord()
                {
                    Name = fileInfo.Name,
                    Date = fileInfo.CreationTime,
                    Size = fileInfo.Length
                });
            }
            return array.ToArray();
        }


        public void SaveFile(Stream file, string fileName, bool toOpenFolder)
        {
           
            Task.Run(() =>
            {
                string path;
                if (toOpenFolder && ActualDirectory != null)
                {
                    path = ActualDirectory.FullName + "\\" + fileName;
                }
                else
                {
                    path = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.UserProfile), "Downloads") + "\\" + fileName;
                }

               
                if (File.Exists(path)) File.Delete(path);

                var fileStream = new FileStream(path, FileMode.Create, FileAccess.Write);
                file.CopyTo(fileStream);

                fileStream.Dispose();

            });
                
        }


       
        public DriveInfo[] GetDrives()
        {
            return DriveInfo.GetDrives();
        }

        public Stream GeFileToTransfer(string fileName)
        {
            if (ActualDirectory == null) return null;
            string path = ActualDirectory.FullName + "\\" + fileName;
            if (!File.Exists(path)) return null;

            return File.OpenRead(path);

        }
    }
}