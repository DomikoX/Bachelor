using System;
using System.DirectoryServices.AccountManagement;
using System.IdentityModel.Selectors;
using System.ServiceModel;

namespace RemoteWcfService
{
    public class CustomUserPasswordValidator : UserNamePasswordValidator
    {
        public override void Validate(string userName, string password)
        {
            if (null == userName || null == password)
            {
                throw new ArgumentNullException();
            }
            //Meno a priezvisko;Reason1;Rease2
            //Stefan Toth;456.50;0;12;
            //using (PrincipalContext pc = new PrincipalContext(ContextType.Domain, "fri.uniza.sk"))
            //{
            //    if (!pc.ValidateCredentials(userName, password))
            //        throw new FaultException("Unknown Username or Incorrect Password");
            //}

            if (!(userName == "a" && password == "a"))
            {
                // This throws an informative fault to the client.
                throw new FaultException("Unknown Username or Incorrect Password");
                // When you do not want to throw an infomative fault to the client,
                // throw the following exception.
                // throw new SecurityTokenException("Unknown Username or Incorrect Password");
            }
        }
    }
}