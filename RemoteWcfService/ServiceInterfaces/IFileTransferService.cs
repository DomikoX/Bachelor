using System;
using System.IO;
using System.ServiceModel;
using System.ServiceModel.Web;

namespace RemoteWcfService
{
    [ServiceContract]
    public interface IFileTransferService
    {
        [OperationContract]
        Stream DownloadFile(string fileName);

        [OperationContract(IsOneWay = true)]
        void UploadFile(FileToTransfer file);
    }

    [ServiceContract]
    public interface IWebFileTransferService
    {
        [OperationContract]
        [DataContractFormat]
        [WebInvoke(Method = "POST",BodyStyle = WebMessageBodyStyle.Bare,ResponseFormat = WebMessageFormat.Json)]
        void Upload(Stream uploading);

        [OperationContract]
        [WebGet(UriTemplate = "Download/{fileName}")]
        Stream Download(string fileName);

    }


    [MessageContract]
    public class FileToTransfer
    {
        [MessageHeader]
        public string Name { get; set; }

        [MessageBodyMember]
        public Stream File { get; set; }
    }
}