using System;
using System.Collections.Generic;
using System.Linq;
using System.Management.Automation;
using System.Management.Automation.Runspaces;
using System.Security;


namespace WSManDemo1
{
    public static class WSMan
    {
        private static void OpenRunSpace(WSManConnectionInfo connectionInfo, ref Runspace remoteRunspace)
        {
            remoteRunspace = RunspaceFactory.CreateRunspace((RunspaceConnectionInfo)connectionInfo);
            remoteRunspace.Open();
        }

        private static void OpenRunSpace(string uri, string schema, string username, string pass, ref Runspace remoteRunspace)
        {
            try
            {
                var password = new SecureString();
                foreach (char c in pass.ToCharArray())
                {
                    password.AppendChar(c);
                }
                var credential = new PSCredential(username, password);
                WSManConnectionInfo manConnectionInfo = new WSManConnectionInfo(new Uri(uri), schema, credential);
                manConnectionInfo.AuthenticationMechanism = AuthenticationMechanism.Negotiate;
                manConnectionInfo.ProxyAuthentication = AuthenticationMechanism.Negotiate;
                remoteRunspace = RunspaceFactory.CreateRunspace((RunspaceConnectionInfo)manConnectionInfo);
                remoteRunspace.Open();

            }
            catch
            {
                
            }
        }

        public static IEnumerable<PSObject> RunScript(string scriptTxt, string serverName, string username, string password, int port)
        {
            Runspace remoteRunspace = null;
            if(!string.IsNullOrEmpty(username))
            {
                OpenRunSpace($"http://{serverName}:{port}/wsman", "http://schemas.microsoft.com/powershell/Microsoft.PowerShell", username, password,ref remoteRunspace);
            }

            try
            {
                using (var powerShell = PowerShell.Create())
                {
                    powerShell.Runspace = remoteRunspace;
                    powerShell.AddScript(scriptTxt);
                    var collection = powerShell.Invoke();
                    remoteRunspace.Close();
                    return collection;
                }
            }
            catch
            {
                return Enumerable.Empty<PSObject>();
            }
        }
    }
}
