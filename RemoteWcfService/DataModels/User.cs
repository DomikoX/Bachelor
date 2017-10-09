using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.ServiceModel.Security;
using System.Text;
using System.Threading.Tasks;
using System.Web.Caching;
using System.Xml.Serialization;

namespace RemoteWcfService.DataModels
{
    
    public class User
    {
        public int Id { get; set; }
        public string Username { get; set; }
        public string PasswordHash { get; set; }
        public string Email { get; set; }
        public Roles Role { get; set; }
        
        public virtual List<Device> AssignedDevices { get; set; }


    }


    public enum Roles
    {
        User,
        Admin,
    }
}
