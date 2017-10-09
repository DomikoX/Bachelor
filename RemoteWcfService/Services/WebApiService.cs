using System;
using System.Collections.Generic;
using System.Globalization;
using System.IdentityModel.Tokens.Jwt;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Security.Claims;
using System.Security.Cryptography;
using System.ServiceModel.Web;
using System.Text;
using System.Threading.Tasks;
using Microsoft.IdentityModel.Tokens;
using RemoteWcfService.DataModels;

namespace RemoteWcfService
{
    public partial class DeviceControlService : IWebApiService
    {
        private static string _secure = System.Configuration.ConfigurationManager.AppSettings["token_key"];
        private SymmetricSecurityKey _secureKey = new SymmetricSecurityKey(Base64UrlEncoder.DecodeBytes(_secure));


        private ClaimsPrincipal CheckTokenFromHeader()
        {
            var header = WebOperationContext.Current.IncomingRequest.Headers;

            string jwt = header.Get("Authorization");

            var handler = new JwtSecurityTokenHandler();

            SecurityToken token;
            ClaimsPrincipal principal;
            try
            {
                principal = handler.ValidateToken(jwt, new TokenValidationParameters
                {
                    ValidateAudience = false,
                    ValidateIssuer = false,
                    IssuerSigningKey = _secureKey,
                }, out token);
                return principal;
            }
            catch (Exception e)
            {
                throw new WebFaultException<Exception>(new Exception(e.Message), HttpStatusCode.Unauthorized);
            }
        }

        private ClaimsPrincipal CheckTokenFromHeader(string role)
        {
            var principals = CheckTokenFromHeader();
            if (!principals.IsInRole(role))
            {
                throw new WebFaultException<Exception>(
                    new Exception("You cannot perform this operation with you role: " + role),
                    HttpStatusCode.Unauthorized);
            }
            return principals;
        }


        private string GenerateToken(string username, string role)
        {
            SecurityTokenHandler tokenHandler = new JwtSecurityTokenHandler();

            var claimList = new List<Claim>()
            {
                new Claim(ClaimTypes.Name, username),
                new Claim(ClaimTypes.Role, role)
            };


            var tokenDescriptor = new SecurityTokenDescriptor
            {
                Subject = new ClaimsIdentity(claimList),
                SigningCredentials = new SigningCredentials(
                    _secureKey,
                    Microsoft.IdentityModel.Tokens.SecurityAlgorithms.HmacSha256Signature)
            };

            var securityToken = tokenHandler.CreateToken(tokenDescriptor);
            return tokenHandler.WriteToken(securityToken);
        }

        private string Hash(string password)
        {
            var bytes = new UTF8Encoding().GetBytes(password);
            var hashBytes = SHA1.Create().ComputeHash(bytes);
            return Convert.ToBase64String(hashBytes);
        }


        private static void WakeOnLan(string mac)
        {

            byte[] bytes = new byte[6];
            int i = 0;
            for (int z = 0; z < 6; z++)
            {
                bytes[z] =
                    byte.Parse(mac.Substring(i, 2), NumberStyles.HexNumber);
                i += 2;
            }
            WakeOnLan(bytes);
        }

        private static void WakeOnLan(byte[] mac)
        {
            // WOL packet is sent over UDP 255.255.255.0:40000.
            UdpClient client = new UdpClient();
            client.Connect(IPAddress.Broadcast, 40000);

            // WOL packet contains a 6-bytes trailer and 16 times a 6-bytes sequence containing the MAC address.
            byte[] packet = new byte[17*6];

            // Trailer of 6 times 0xFF.
            for (int i = 0; i < 6; i++)
                packet[i] = 0xFF;

            // Body of magic packet contains 16 times the MAC address.
            for (int i = 1; i <= 16; i++)
                for (int j = 0; j < 6; j++)
                    packet[i*6 + j] = mac[j];

            // Send WOL packet.
            client.Send(packet, packet.Length);
        }


        //Neodskusana metoda.. 
        public void TryWakeDevice(string deviceId)
        {
            using (var db = new RemConDB())
            {
                var d = db.Devices.FirstOrDefault(dev => dev.Id.ToString() == deviceId);
                if (d == null) return;

               WakeOnLan(d.MacAddress);
            }
        }

        public string Login(string username, string password)
        {
            using (var db = new RemConDB())
            {
                var pass = Hash(password);
                var query = db.Users.Where(u => u.Username == username && u.PasswordHash == pass);


                try
                {
                    User user = query.Single();
                    return GenerateToken(user.Username, user.Role.ToString());
                }
                catch (Exception)
                {
                    throw new WebFaultException<Exception>(
                        new Exception("bad username or password"), HttpStatusCode.Forbidden);
                }
            }
        }

        public string Register(string username, string password, string passwordAgain, string email)
        {
            if (password != passwordAgain)
            {
                throw new WebFaultException<Exception>(
                    new Exception("Passwords didnot match"), HttpStatusCode.BadRequest);
            }


            using (var db = new RemConDB())
            {
                string passHash = Hash(password);
                User newUser = new User()
                {
                    Username = username,
                    PasswordHash = passHash,
                    Email = email,
                    Role = Roles.User
                };
                db.Users.Add(newUser);
                db.SaveChanges();
            }
            var token = this.GenerateToken(username, "user");
            return token;
        }

        public void AddAsUsageClient(string deviceId, string clientId)
        {
            CheckTokenFromHeader();

            Device dev;
            if (_registeredDevices.TryGetValue(deviceId, out dev))
            {
                try
                {
                    dev.AddUsageCallback(clientId);
                }
                catch (Exception)
                {
                    _registeredDevices.Remove(deviceId);
                }
            }
        }

        public void RemoveFromUsageClients(string deviceId, string clientId)
        {
            CheckTokenFromHeader();

            Device dev;
            if (_registeredDevices.TryGetValue(deviceId, out dev))
            {
                try
                {
                    dev.RemoveUsageCallback(clientId);
                }
                catch (Exception)
                {
                    _registeredDevices.Remove(deviceId);
                }
            }
        }


        public DeviceUsage GetLastUsageUpdate(string deviceId, string clientId)
        {
            CheckTokenFromHeader();

            Device dev;
            if (_registeredDevices.TryGetValue(deviceId, out dev))
            {
                try
                {
                    return new DeviceUsage()
                    {
                        CpuUsage = dev.GetLastCpuUsage(clientId),
                        RamUsage = dev.GetLastRamUsage(clientId)
                    };
                }
                catch (Exception)
                {
                    _registeredDevices.Remove(deviceId);
                }
            }
            return new DeviceUsage() {CpuUsage = 0, RamUsage = new float[] {0, 0}};
        }

        public void AddAsScreenClient(string deviceId, string clientId)
        {
            CheckTokenFromHeader();

            Device dev;
            if (_registeredDevices.TryGetValue(deviceId, out dev))
            {
                try
                {
                    dev.AddScreenCallback(clientId);
                }
                catch (Exception)
                {
                    _registeredDevices.Remove(deviceId);
                }
            }
        }

        public void RemoveFromScreenClients(string deviceId, string clientId)
        {
            CheckTokenFromHeader();

            Device dev;
            if (_registeredDevices.TryGetValue(deviceId, out dev))
            {
                try
                {
                    dev.RemoveScreenCallback(clientId);
                }
                catch (Exception)
                {
                    _registeredDevices.Remove(deviceId);
                }
            }
        }

        public string GetLastScreenUpdate(string deviceId, string clientId)
        {
            CheckTokenFromHeader();

            Device dev;
            if (_registeredDevices.TryGetValue(deviceId, out dev))
            {
                try
                {
                    byte[] arr = dev.GetLastScreenUpdate(clientId);
                    string rs = "nic";
                    if (arr != null)
                    {
                        rs = Convert.ToBase64String(arr);
                    }

                    return rs;
                }
                catch (Exception e)
                {
                    Logger.Error("GetLastScrenUpdate Error: " + e.Message);
                    _registeredDevices.Remove(deviceId);
                }
            }
            return null;
        }

        public void PickFile(string deviceId, string fileName, string clientId, bool saveToOpenFolder)
        {
            CheckTokenFromHeader();

            Device dev;
            if (_registeredDevices.TryGetValue(deviceId, out dev))
            {
                try
                {
                    dev.DeviceCallback.PickFile(clientId, fileName, saveToOpenFolder);
                }
                catch (Exception)
                {
                    _registeredDevices.Remove(deviceId);
                }
            }
        }


        public void DoOperation(string deviceId, string operation)
        {
            CheckTokenFromHeader();

            Logger.Info($"DoOperation metode to Device: {deviceId} ({operation}) ");

            Device dev;
            if (_registeredDevices.TryGetValue(deviceId, out dev))
            {
                try
                {
                    Operation op = (Operation) Enum.Parse(typeof(Operation), operation);
                    dev.DeviceCallback.PerformOperation(op);
                }
                catch (ArgumentException)
                {
                    throw new WebFaultException<Exception>(
                        new Exception("Bad operation type:" + operation), HttpStatusCode.BadRequest);
                }
                catch (Exception)
                {
                    _registeredDevices.Remove(deviceId);
                }
            }
        }


        public async Task<ProcessRecord[]> GetRunningProcesses(string deviceId)
        {
            CheckTokenFromHeader();

            Device dev;
            if (_registeredDevices.TryGetValue(deviceId, out dev))
            {
                try
                {
                    return await dev.DeviceCallback.GetRunningProcesses();
                }
                catch (Exception)
                {
                    _registeredDevices.Remove(deviceId);
                    return null;
                }
            }
            return null;
        }

        public void KillProcess(string deviceId, int pid)
        {
            CheckTokenFromHeader();

            Device dev;
            if (_registeredDevices.TryGetValue(deviceId, out dev))
            {
                try
                {
                    dev.DeviceCallback.KillProcess(pid);
                }
                catch (Exception)
                {
                    _registeredDevices.Remove(deviceId);
                }
            }
        }


        public async Task<ExplorerRecord[]> RunExplorer(string deviceId, string clientId)
        {
            CheckTokenFromHeader();

            Logger.Info($"RunExplorer metode to Device: {deviceId} from {clientId} ");
            Device dev;
            if (_registeredDevices.TryGetValue(deviceId, out dev))
            {
                try
                {
                    return await dev.DeviceCallback.RunExplorer(clientId);
                }
                catch (Exception)
                {
                    _registeredDevices.Remove(deviceId);
                    return null;
                }
            }
            return null;
        }

        public async Task<ExplorerRecord[]> OpenDirectory(string deviceId, string clientId, int index)
        {
            CheckTokenFromHeader();

            Logger.Info($"OpenDirectory metode to Device: {deviceId} from {clientId}  index: {index}");


            Device dev;
            if (_registeredDevices.TryGetValue(deviceId, out dev))
            {
                try
                {
                    return await dev.DeviceCallback.SelectDirectory(clientId, index);
                }
                catch (Exception e)
                {
                    _registeredDevices.Remove(deviceId);
                    Logger.Info($"OpenDirectory Error {e.Message}");
                    return null;
                }
            }
            return null;
        }

        public void UploadFile(string deviceId, string clientId, string fileName)
        {
            CheckTokenFromHeader();

            Device dev;

            if (_registeredDevices.TryGetValue(deviceId, out dev))
            {
                try
                {
                    dev.DeviceCallback.PickFile(clientId, fileName, true);
                }
                catch (Exception)
                {
                    _registeredDevices.Remove(deviceId);
                }
            }
        }


        public async Task<string> PrepareFileOnServer(string deviceId, string clientId, string fileName)
        {
            CheckTokenFromHeader();


            Device dev;

            if (_registeredDevices.TryGetValue(deviceId, out dev))
            {
                try
                {
                    return await dev.DeviceCallback.UploadFileOnServer(clientId, fileName);
                }
                catch (Exception)
                {
                    _registeredDevices.Remove(deviceId);
                    return null;
                }
            }
            return "FAIL";
        }


        public void SendMessage(string deviceId, string title, string message)
        {
            CheckTokenFromHeader();

            Logger.Info($"SendMessage metode to Device: {deviceId}");


            Device dev;
            if (_registeredDevices.TryGetValue(deviceId, out dev))
            {
                try
                {
                    dev.DeviceCallback.SendMessage(title, message);
                }
                catch (Exception)
                {
                    _registeredDevices.Remove(deviceId);
                }
            }
        }

        public void Block(string deviceId)
        {
            CheckTokenFromHeader();

            Logger.Info($"Block metode to Device: {deviceId}");


            Device dev;
            if (_registeredDevices.TryGetValue(deviceId, out dev))
            {
                try
                {
                    dev.DeviceCallback.Block();
                }
                catch (Exception)
                {
                    _registeredDevices.Remove(deviceId);
                }
            }
        }

        public void UnBlock(string deviceId)
        {
            CheckTokenFromHeader();

            Logger.Info($"Unblock metode to Device: {deviceId}");

            Device dev;
            if (_registeredDevices.TryGetValue(deviceId, out dev))
            {
                try
                {
                    dev.DeviceCallback.UnBlock();
                }
                catch (Exception)
                {
                    _registeredDevices.Remove(deviceId);
                }
            }
        }


        public List<DeviceInfo> GetOnlineDeviceList()
        {
            var username = CheckTokenFromHeader().Claims.First(c => c.Type == ClaimTypes.Name).Value;
            Logger.Info($"GetOnlineDeviceList for client: {username}");
            var list = new List<DeviceInfo>();
            using (var db = new RemConDB())
            {
                User user = db.Users.First(u => u.Username == username);
                foreach (DataModels.Device device in user.AssignedDevices)
                {
                    bool isOnline = _registeredDevices.ContainsKey(device.Id);
                    list.Add(new DeviceInfo(device, isOnline));
                }
            }

            return list;
        }

        public List<string[]> GetListOfUsers()
        {
            CheckTokenFromHeader(Roles.Admin.ToString());
            List<string[]> list = new List<string[]>();
            using (var db = new RemConDB())
            {
                foreach (var user in db.Users)
                {
                    list.Add(new string[] {user.Username, user.Id.ToString()});
                }
            }
            return list;
        }

        public List<DeviceInfo> GetDevicesByUser(string username)
        {
            CheckTokenFromHeader(Roles.Admin.ToString());
            var list = new List<DeviceInfo>();
            using (var db = new RemConDB())
            {
                User user = db.Users.First(u => u.Username == username);

                foreach (DataModels.Device device in db.Devices)
                {
                    var di = new DeviceInfo(device) {Mark = user.AssignedDevices.Contains(device)};
                    list.Add(di);
                }
            }

            return list;

        }




        public void AssignDeviceToUser(string deviceId, string userId)
        {
            CheckTokenFromHeader(Roles.Admin.ToString());

            using (var db = new RemConDB())
            {
               
              db.Database.ExecuteSqlCommand("INSERT INTO UserDevices (User_Id, Device_Id) VALUES ({0},{1})",userId,deviceId);
            }
        }

        public void RemoveDeviceFromUser(string deviceId, string userId)
        {
            CheckTokenFromHeader(Roles.Admin.ToString());
            using (var db = new RemConDB())
            {
               
                db.Database.ExecuteSqlCommand("DELETE from  UserDevices WHERE User_Id = {0} AND  Device_Id = {1}", userId, deviceId);
            }
        }
    }
}