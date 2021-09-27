using Microsoft.SharePoint.Client;
using Microsoft.SharePoint.Client.Utilities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace seaspan.hcwpce.sharepoint.Models
{
    public class SPDatasource
    {
        protected virtual string LibraryName { get; }
        protected virtual Dictionary<string, string> FieldMapping { get; }
        public int Id { get; set; }        
        public Dictionary<string, object> Data { get; set; }
        public Dictionary<string, object> FieldValues { get; set; }

        public SPDatasource(int id)
        {
            this.Id = id;
            Data = new Dictionary<string, object>();

            LoadData();
        }

        public SPDatasource(string name, int id)
        {
            this.Id = id;
            this.LibraryName = name;
            Data = new Dictionary<string, object>();

            LoadData();
        }

        protected void LoadData()
        {
            if (string.IsNullOrEmpty(LibraryName)) return;

            var cliet = SPContext.CurrentInstance.SPClient;
            var list = SPContext.CurrentInstance.GetList(LibraryName);
            var item = list.GetItemById(this.Id);            
            cliet.Load(item);
            cliet.ExecuteQuery();

            this.FieldValues = item.FieldValues;

            if (FieldMapping == null || FieldMapping.Count == 0) return;

            foreach(var f in FieldMapping)
            {
                var ff = f.Key.Split(',').ToList();
                string v = "";
                bool isobj = false;
                ff.ForEach(x =>
                {
                    var fv = x.Split(':');
                    var xv = fv[0];
                    var xf = fv.Length > 1 ? fv[1] : "";
                    if (item.FieldValues[xv] is FieldLookupValue || item.FieldValues[xv] is FieldUserValue)
                    {
                        isobj = true;
                        Data.Add(f.Value, item.FieldValues[xv]);
                    }
                    else
                    {
                        var formatedv = item.FieldValues[xv];
                        if (!string.IsNullOrEmpty(xf) && formatedv != null)
                        {
                            formatedv = string.Format("{0:" + xf + "}", formatedv);
                        }
                        if (string.IsNullOrEmpty(v))
                            v = $"{formatedv}";
                        else
                            v = $"{v} {formatedv}".Trim();                      
                    }
                });
                if (string.IsNullOrEmpty(v))
                    v = "N/A";

                if (!isobj)
                    Data.Add(f.Value, v);
            }
        }

    }

    public class Spec : SPDatasource
    {
        protected override string LibraryName => "Spec PMRS";
        protected override Dictionary<string, string> FieldMapping => 
            new Dictionary<string, string> {
                { "Title", "PMRS NUMBER" },
                { "SpecTitle", "TITLE" },
                { "Spec_x0020_Rev,RevDate:d-MMM-yyyy", "PMRS REV NUMBER & DATE" },
                { "SSMRS_x0020_REFERENCE_x0020_NUMBER", "SSMRS REFERENCE NUMBER" },
                { "SSMRS_x0020_REV_x0020_NUMBER,SSMRS_x0020_REV_x0020_DATE:d-MMM-yyyy", "SSMRS REV NUMBER & DATE" },
                { "EC_x0020_NUMBER", "EC NUMBER" },
                { "EC_x0020_REV_x0020_NUMBER,EC_x0020_REV_x0020_DATE:d-MMM-yyyy", "EC REV NUMBER & DATE" },
                { "Vessel", "Vessel"}
            };

        public Vessel Vessel { get; set; }
        public Spec(int id) : base(id) 
        { 
            if (Data.Keys.Contains("Vessel") && Data["Vessel"] != null)
            {
                Vessel = new Vessel(((FieldLookupValue)Data["Vessel"]).LookupId);
            }
        }
    }

    public class Vessel : SPDatasource
    {
        protected override string LibraryName => "Vessel";
        protected override Dictionary<string, string> FieldMapping => 
            new Dictionary<string, string>()
            {
                {"VesselNameAndType", "VESSEL" },
                {"Hull_x0020_Number", "HULL NUMBER" },
                {"PWGSCContractNo_x002e_", "CONTRACT NUMBER" }
            };
        public Vessel(int id) : base(id) { }
    }
}
