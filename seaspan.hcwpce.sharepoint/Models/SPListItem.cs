using Microsoft.SharePoint.Client;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace seaspan.hcwpce.sharepoint.Models
{
    public class SPListItem
    {
        protected virtual string ListName { get; }
        [SPField(Name = "ID", EntityType = "Number")]
        public int ID { get; set; }
        [SPField(Name = "Title", EntityType = "Text")]
        public string Title { get; set; }

        [SPField(Name = "Created", EntityType = "Datetime")]
        public DateTime CreatedOn { get; set; }
        [SPField(Name = "Author", EntityType = "Lookup")]
        public FieldUserValue CreatedBy { get; set; }

        [SPField(Name = "Modified", EntityType = "Datetime")]
        public DateTime ModifiedOn { get; set; }
        [SPField(Name = "Editor", EntityType = "Lookup")]
        public FieldUserValue ModifiedBy { get; set; }

        public SPListItem()
        {
            CreatedOn = DateTime.UtcNow;
            ModifiedOn = DateTime.UtcNow;
        }

        public object Get(Dictionary<string, object> fieldvalues, string key)
        {
            if (fieldvalues.Keys.Contains(key) && fieldvalues[key] != null)
            {
                return fieldvalues[key];
            }

            return null;                
        }

        public void Create()
        {
            var list = SPContext.CurrentInstance.GetList(this.ListName);
            ListItemCreationInformation itemCreateInfo = new ListItemCreationInformation();
            ListItem oListItem = list.AddItem(itemCreateInfo);

            var properties = this.GetType().GetProperties().Where(x => x.CanWrite && x.CanRead);
            foreach(var p in properties)
            {
                var attrs = p.GetCustomAttributes(typeof(SPFieldAttribute), false);
                if (attrs.Length == 0) continue;                
                var attr = attrs[0] as SPFieldAttribute;
                switch(attr.Name.ToLower())
                {
                    case "id":
                    case "created":
                    case "modified":
                        continue;
                    default:
                        var v = p.GetValue(this);
                        if (v == null) continue;
                        oListItem[attr.Name] = v;
                        break;
                }
                //if (attr.Name.Equals("ID")) continue;
                
            }

            oListItem.Update();
            SPContext.CurrentInstance.SPClient.ExecuteQuery();
        }        
    }

    public class Annotiation : SPListItem
    {
        public string annotationid { get; set; }
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
        public string url { get; set; }
        public Microsoft.SharePoint.Client.File File { get; set; }
        public string LibraryName { get; set; }
    }
}
