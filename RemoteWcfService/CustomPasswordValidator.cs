using System;
using System.IdentityModel.Selectors;
using System.ServiceModel;

namespace RemoteWcfService
{
    public class CustomPasswordValidator : UserNamePasswordValidator
    {
        public override void Validate(string userName, string password)
        {
            if (null == userName || null == password)
            {
                throw new ArgumentNullException();
            }
           

            //TODO spravne validovat pomocou DLL 

            if (!(userName == "Device" && password == "heslo@123.sk") )
            {
                // This throws an informative fault to the client.
                throw new FaultException("Unknown Username or Incorrect Password!");
                // When you do not want to throw an infomative fault to the client,
                // throw the following exception.
                // throw new SecurityTokenException("Unknown Username or Incorrect Password");
            }
        }
    }
}