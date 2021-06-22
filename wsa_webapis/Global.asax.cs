using McTools.Xrm.Connection;
using Sql4Cds;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.IO;
using System.Linq;
using System.Web;
using System.Web.Http;
using System.Web.Mvc;
using System.Web.Optimization;
using System.Web.Routing;
using wsa_webapis.Sql4Cds;

namespace wsa_webapis
{
    public class WebApiApplication : System.Web.HttpApplication
    {
        protected void Application_Start()
        {
            AreaRegistration.RegisterAllAreas();
            GlobalConfiguration.Configure(WebApiConfig.Register);
            FilterConfig.RegisterGlobalFilters(GlobalFilters.Filters);
            RouteConfig.RegisterRoutes(RouteTable.Routes);
            BundleConfig.RegisterBundles(BundleTable.Bundles);

            GlobalProperties.DBConnectionString = ConfigurationManager.ConnectionStrings["wsainspectionform"].ConnectionString;

            Settings.Instance = new Settings()
            {
                QuotedIdentifiers = true,
                SelectLimit = 0, 
                UseTSQLEndpoint = false,
                UseRetrieveTotalRecordCount = true                
            };
            
            LoadConnectionDetail();
        }

        private void LoadConnectionDetail()
        {
            var filename = Server.MapPath("~/Resources/ConnectionsList.Default.xml");
            if (!File.Exists(filename)) return;
            using(var reader = new StreamReader(filename))
            {
                var s = reader.ReadToEnd();
                GlobalProperties.WSAConnectionDetail = XmlSerializerHelper.Deserialize(s, typeof(ConnectionDetail)) as ConnectionDetail;
            }
        }
    }
}
