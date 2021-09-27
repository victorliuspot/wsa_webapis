using Microsoft.SharePoint.Client;
using OfficeDevPnP.Core;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Security;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace seaspan.hcwpce.sharepoint
{
    public class SPContext
    {        
        private static SPContext _instance { get; set; }
        public static SPContext CurrentInstance { 
            get
            {
                if (_instance == null)
                    _instance = new SPContext();
                return _instance;
            }
            set
            {
                _instance = value;
            }
        }
        private string _siteUrl { get; set; }
        private string _clientId { get; set; }
        private string _tenant { get; set; }

        private string _username { get
            {
                return $"{ConfigurationManager.AppSettings["sp:username"]}";
            } }

        private string _password
        {
            get
            {
                string pwd = $"{ConfigurationManager.AppSettings["sp:password"]}";
                if (!string.IsNullOrEmpty(pwd))
                {
                    byte[] data = Convert.FromBase64String(pwd);
                    return Encoding.UTF8.GetString(data);
                }
                return string.Empty;
            }
        }

        private string _certificatePath { get; set; }
        private string _certificatePassword { get; set; }

        private ClientContext _ctx { get; set; }
        public ClientContext SPClient {
            get
            {
                if (_ctx == null)
                {
                    if (!string.IsNullOrEmpty(_clientId))
                    {
                        _ctx = new AuthenticationManager().GetAzureADAppOnlyAuthenticatedContext(_siteUrl, _clientId, _tenant, _certificatePath, _certificatePassword);
                    }
                    else if (!string.IsNullOrEmpty(_username))
                    {
                        _ctx = new ClientContext(_siteUrl);
                        SecureString securePassword = new SecureString();
                        foreach (char c in _password.ToCharArray()) securePassword.AppendChar(c);
                        _ctx.Credentials = new SharePointOnlineCredentials(_username, securePassword);
                    }
                }
                
                return _ctx;
            } 
        }

        public SPContext()
        {
            _siteUrl = ConfigurationManager.AppSettings["sp:siteurl"];
            _clientId = ConfigurationManager.AppSettings["sp:clientid"];
            _tenant = ConfigurationManager.AppSettings["sp:tenant"];
            
            if (!string.IsNullOrEmpty(_clientId))
            {
                //To get the location the assembly normally resides on disk or the install directory
                string path = System.Reflection.Assembly.GetExecutingAssembly().CodeBase;

                //once you have the path you get the directory with:
                _certificatePath = $"{System.IO.Path.GetDirectoryName(path).TrimEnd('/')}\\Config\\spotsolutions.pfx".Replace(@"file:\", "");
                var pwd = ConfigurationManager.AppSettings["certificatePassword"];
                if (!string.IsNullOrEmpty(pwd))
                {
                    byte[] data = Convert.FromBase64String(pwd);
                    _certificatePassword = Encoding.UTF8.GetString(data);
                }
            }
        }

        public Web Web
        {
            get
            {
                Web web = SPClient.Web;
                SPClient.Load(web);
                return web;
            }
        }

        public List GetList(string title)
        {
            var list = this.Web.Lists.GetByTitle(title);
            SPClient.Load(list);            
            return list;
        }

        public void ExecuteNonQuery()
        {
            SPClient.ExecuteQuery();
        }       
    }
}
