using System;
using System.Collections.Generic;
using System.Data;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Web;

namespace wsa_webapis.Models
{
    public class Annotation
    {
        public string annotationid { get; set; }
        public string createdbyname { get; set; }
        public DateTime createdon { get; set; }
        public string documentbody { get; set; }
        public string filename { get; set; }
        public int filesize { get; set; }
        public bool isdocument { get; set; }
        public string mimetype { get; set; }
        public string modifiedbyname { get; set; }
        public DateTime modifiedon { get; set; }
        public string notetext { get; set; }
        public string objectidtype { get; set; }
        public string subject { get; set; }

        public Annotation(DataRow drow)
        {
            annotationid = $"{drow["annotationid"]}";
            createdbyname = $"{drow["createdbyname"]}";
            createdon = DateTime.Parse($"{drow["createdon"]}");
            //documentbody = $"{drow["documentbody"]}";
            filename = $"{drow["filename"]}";
            filesize = int.Parse($"{drow["filesize"]}");
            isdocument = bool.Parse($"{drow["isdocument"]}");
            mimetype = $"{drow["mimetype"]}".Replace("Null", "").Replace("NULL", "").Replace("null", "").Trim();
            modifiedbyname = $"{drow["modifiedbyname"]}";
            modifiedon = DateTime.Parse($"{drow["modifiedon"]}");
            notetext = $"{drow["notetext"]}".Replace("Null", "").Replace("NULL", "").Replace("null", "").Trim();
            objectidtype = $"{drow["objectidtype"]}";
            subject = $"{drow["subject"]}".Replace("Null","").Replace("NULL","").Replace("null", "").Trim();
        }

        public string toString()
        {
            StringBuilder sb = new StringBuilder();
            sb.AppendLine($"<div class='panel panel-default'>");
            sb.AppendLine($"<div class='panel-heading'><span class='glyphicon glyphicon-user'></span> {createdbyname} &nbsp;&nbsp; <span class='glyphicon glyphicon-calendar'></span>{createdon: d-MMM-yyyy HH:mm}</div>");
            sb.AppendLine($"<div class='panel-body'>");
            if (!string.IsNullOrEmpty(subject))
                sb.AppendLine($"<div style='border-bottom:solid 1px #ccc;'><h4>{subject}</h4></div>");
            if (!string.IsNullOrEmpty(notetext))
                sb.AppendLine($"<div>{notetext}</div>");
            
            ///Display images
            if (mimetype.ToLower().StartsWith("image/"))
            {                
                var v = $"data:{mimetype};base64,{documentbody}";
                sb.AppendLine($"<img src='{v}' alt='{filename}' style='max-width:100%;'/>");
            }
            else if (mimetype.Length > 0)
            {
                var url = $"api/Download?id={annotationid}";
                sb.AppendLine($"<div><span class='glyphicon glyphicon-download-alt'></span><a href='{url}' alt='{filename}'>{filename}</a></ div>");
            }

            sb.AppendLine($"</div>");
            sb.AppendLine($"</div>");
            return sb.ToString();
        }
    }
}