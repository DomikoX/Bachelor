using System;
using System.IO;
using System.Net;
using System.ServiceModel.Web;
using NLog;

namespace RemoteWcfService
{
    [GlobalErrorBehaviorAttribute(typeof(GlobalErrorHandler))]
    public class FileTransferService : IFileTransferService, IWebFileTransferService
    {
        private string _tempDir = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "temp_files");
        private static readonly Logger Logger = LogManager.GetCurrentClassLogger();


        public Stream DownloadFile(string fileName)
        {
            string filePath = Path.Combine(_tempDir, fileName);
            if (!File.Exists(filePath)) return null;
            Stream r;
            try
            {
                r = File.OpenRead(filePath);
            }
            catch (Exception e)
            {
                Logger.Info("Exception " + e.Message);
                return null;
            }


            return r;
        }


        public void UploadFile(FileToTransfer file)
        {
            if (file == null) return;

            string filePath = Path.Combine(_tempDir, file.Name);

            if (File.Exists(filePath)) File.Delete(filePath);

            var fileStream = new FileStream(filePath, FileMode.Create, FileAccess.Write);
            file.File.CopyTo(fileStream);

            fileStream.Dispose();
        }


        public void Upload(Stream uploading)
        {
            WebHeaderCollection header;
            try
            {
                header = WebOperationContext.Current.IncomingRequest.Headers;
            }
            catch (Exception)
            {
                Logger.Info("Do not find: WebOperationContext.Current.IncomingRequest.Headers");
                return;
            }


            string fileName = header.Get("file-name");

            if (fileName == null || fileName.Equals(String.Empty))
            {
                throw new WebFaultException<Exception>(
                    new Exception("fileName not found!  you must define file name int http header file-name"),
                    HttpStatusCode.BadRequest);
            }


            Logger.Info("Upload");

            string filePath = Path.Combine(_tempDir, fileName);

            if (File.Exists(filePath)) File.Delete(filePath);

            var fileStream = new FileStream(filePath, FileMode.Create, FileAccess.Write);
            uploading.CopyTo(fileStream);
            fileStream.Dispose();
        }

        public Stream Download(string fileName)
        {
            string filePath = Path.Combine(_tempDir, fileName);
            if (!File.Exists(filePath))
            {
                throw new WebFaultException<Exception>(
                    new Exception("file not found!"),
                    HttpStatusCode.BadRequest);
            }
            Stream r;
            try
            {
                r = File.OpenRead(filePath);
            }
            catch (Exception e)
            {
                Logger.Info("Exception " + e.Message);
                throw new WebFaultException<Exception>(
                    new Exception("Error while opening File"),
                    HttpStatusCode.BadRequest);
            }

            String headerInfo = "attachment; filename=" + fileName;
            
            try
            {
                WebOperationContext.Current.OutgoingResponse.Headers["Content-Disposition"] = headerInfo;
                WebOperationContext.Current.OutgoingResponse.ContentType = "application/octet-stream";
            }
            catch (Exception)
            {
                Logger.Info("Do not find: WebOperationContext.Current.OutgoingResponse.Headers");
                return null;
            }


            return r;
        }
    }
}