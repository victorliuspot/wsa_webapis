using MarkMpn.Sql4Cds.Engine;
using MarkMpn.Sql4Cds.Engine.ExecutionPlan;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Linq;
using System.Web;

namespace wsa_webapis.Sql4Cds
{
    class ExecuteParams
    {
        public string Sql { get; set; }
        public bool Execute { get; set; }
        public bool IncludeFetchXml { get; set; }
        public int Offset { get; set; }
    }

    class QueryException : ApplicationException
    {
        public QueryException(IRootExecutionPlanNode query, Exception innerException) : base(innerException.Message, innerException)
        {
            Query = query;
        }

        public IRootExecutionPlanNode Query { get; }
    }

    public class SqlQuery
    {        
        public static object Execute(string sql)
        {           
            var worker = new System.ComponentModel.BackgroundWorker();
            worker.ProgressChanged += new ProgressChangedEventHandler(backgroundWorker_ProgressChanged);
            worker.RunWorkerCompleted += new RunWorkerCompletedEventHandler(backgroundWorker_RunWorkerCompleted);

            var args = new ExecuteParams { Sql = sql, Execute = true, IncludeFetchXml = false, Offset = 0 };
            var options = new QueryExecutionOptions(GlobalProperties.WSAConnectionDetail, GlobalProperties.CrmServiceClient, worker, null);
            var converter = new ExecutionPlanBuilder(GlobalProperties.MetaData[GlobalProperties.WSAConnectionDetail], GlobalProperties.TableSize[GlobalProperties.WSAConnectionDetail], options);
            if (Settings.Instance.UseTSQLEndpoint &&
                args.Execute &&
                !String.IsNullOrEmpty(GlobalProperties.CrmServiceClient.CurrentAccessToken))
                converter.TDSEndpointAvailable = true;

            var queries = converter.Build(args.Sql);

            foreach (var query in queries)
            {
                try
                {
                    if (query is IDataSetExecutionPlanNode dataQuery)
                    {
                        return dataQuery.Execute(GlobalProperties.CrmServiceClient, GlobalProperties.MetaData[GlobalProperties.WSAConnectionDetail], options, null, null);
                    }
                    else if (query is IDmlQueryExecutionPlanNode dmlQuery)
                    {
                        return dmlQuery.Execute(GlobalProperties.CrmServiceClient, GlobalProperties.MetaData[GlobalProperties.WSAConnectionDetail], options, null, null);
                    }
                }
                catch (Exception ex)
                {
                    throw new QueryException(query, ex);
                }
            }
            return null;
        }

        private static void backgroundWorker_ProgressChanged(object sender, ProgressChangedEventArgs e)
        {
        }

        private static void backgroundWorker_RunWorkerCompleted(object sender, RunWorkerCompletedEventArgs e)
        {
        }
        }
}