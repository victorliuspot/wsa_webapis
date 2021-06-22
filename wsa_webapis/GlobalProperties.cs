using MarkMpn.Sql4Cds.Engine;
using McTools.Xrm.Connection;
using Microsoft.Xrm.Tooling.Connector;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace wsa_webapis
{
    public class GlobalProperties
    {
        public static string DBConnectionString { get; set; }

        public static ConnectionDetail WSAConnectionDetail { get; set; }

        public static IDictionary<ConnectionDetail, AttributeMetadataCache> MetaData { get; set; }
        public static IDictionary<ConnectionDetail, TableSizeCache> TableSize { get; set; }
       
        private static CrmServiceClient _crmServiceClient { get; set; }
        public static CrmServiceClient CrmServiceClient { get
            {
                if (WSAConnectionDetail != null && _crmServiceClient == null)
                {
                    _crmServiceClient = WSAConnectionDetail.GetCrmServiceClient(false);
                    if (_crmServiceClient.IsReady)
                    {
                        MetaData = new Dictionary<ConnectionDetail, AttributeMetadataCache>();
                        MetaData[WSAConnectionDetail] = new AttributeMetadataCache(_crmServiceClient);
                        TableSize = new Dictionary<ConnectionDetail, TableSizeCache>();
                        TableSize[WSAConnectionDetail] = new TableSizeCache(_crmServiceClient, MetaData[WSAConnectionDetail]);
                    }
                    return _crmServiceClient;
                }
                return _crmServiceClient;
            } 
        }
    }
}