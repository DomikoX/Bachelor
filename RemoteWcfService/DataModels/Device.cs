using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Serialization;

namespace RemoteWcfService.DataModels
{
    public  class Device
    {
        [Key]
        public string Id { get; set; }
        public string Name { get; set; }
        public string CpuInfo { get; set; }
        public string OsInfo { get; set; }
        public string MacAddress { get; set; }
        [IgnoreDataMember]
        [XmlIgnore]
        public virtual List<User> AllowedUsers { get; set; }
        
    }
}
