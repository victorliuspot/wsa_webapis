using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using System.Web.Http;

namespace wsa_webapis.Controllers
{
    public class DownloadController : ApiController
    {
        public HttpResponseMessage Get(string id)
        {
            HttpResponseMessage result = null;
            try
            {
                var sql = $"select documentbody,filename,filesize,isdocument,mimetype from annotation where annotationid='{id}' ";
                var docs = wsa_webapis.Sql4Cds.SqlQuery.Execute(sql) as System.Data.DataTable;

                if (docs == null || docs.Rows.Count == 0)
                {
                    result = Request.CreateResponse(HttpStatusCode.Gone);
                    return result;
                }

                var doc = docs.Rows[0];
                var mimetype = $"{doc["mimetype"]}";
                var filename = $"{doc["filename"]}";
                var documentbody = $"{doc["documentbody"]}";
                if (string.IsNullOrEmpty(documentbody))
                {
                    result = Request.CreateResponse(HttpStatusCode.Gone);
                    return result;
                }
                else
                {
                    var bytes = Convert.FromBase64String(documentbody);
                    result = Request.CreateResponse(HttpStatusCode.OK);
                    result.Content = new ByteArrayContent(bytes);
                    result.Content.Headers.ContentType = new System.Net.Http.Headers.MediaTypeHeaderValue(mimetype);
                    result.Content.Headers.ContentDisposition = new System.Net.Http.Headers.ContentDispositionHeaderValue("attachment");
                    result.Content.Headers.ContentDisposition.FileName = filename;
                }

                return result;
            }
            catch (Exception ex)
            {
                return Request.CreateResponse(HttpStatusCode.Gone);
            }
        }
    }
}