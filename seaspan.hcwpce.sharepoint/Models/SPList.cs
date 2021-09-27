using Microsoft.SharePoint.Client;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace seaspan.hcwpce.sharepoint.Models
{
    public class SPList
    {
        private List _list { get; set; }
        protected string Title { get; set; }
        public List<ListItem> Items { get; set; }

        public SPList(string title)
        {
            this.Title = title;
            Items = new List<ListItem>();
            _list = SPContext.CurrentInstance.GetList(title);
        }


        public void LoadAllItems(string fieldName, string fieldType, string value, int limit = 1000)
        {
            if (fieldType.ToLower().Equals("lookup"))
                LoadAllItems(fieldName, int.Parse(value), limit);
            else
                LoadAllItems(fieldName, value, limit);
        }
        
        public void LoadAllItems(string fieldName, string value, int limit = 1000)
        {
            CamlQuery camlQuery = new CamlQuery();
            camlQuery.ViewXml = $"<View><Query><Where><Eq><FieldRef Name='{fieldName}' /><Value Type='Text'>{value}</Value></Eq></Where></Query><RowLimit>{limit}</RowLimit></View>";

            LoadAllItems(camlQuery);
        }

        /// <summary>
        /// Load all items associated to a lookup field by Id
        /// </summary>
        /// <param name="fieldName"></param>
        /// <param name="lookupId"></param>
        /// <param name="limit"></param>
        public void LoadAllItems(string fieldName, int lookupId, int limit = 1000)
        {
            CamlQuery camlQuery = new CamlQuery();
            camlQuery.ViewXml = $"<View><Query><Where><Eq><FieldRef Name='{fieldName}' LookupId='TRUE' /><Value Type='Lookup'>{lookupId}</Value></Eq></Where></Query><RowLimit>{limit}</RowLimit></View>";

            LoadAllItems(camlQuery);
        }        

        public void LoadAllItems(CamlQuery query)
        {       
            if (query == null)
                query = new CamlQuery();

            var client = SPContext.CurrentInstance.SPClient;
            var collListItem = _list.GetItems(query);
            client.Load(collListItem);
            client.ExecuteQuery();
            
            Items.AddRange(collListItem);            
            var position = collListItem.ListItemCollectionPosition;
            do
            {
                if (position == null) break;
                query.ListItemCollectionPosition = position;
                collListItem = _list.GetItems(query);

                client.Load(collListItem);
                client.ExecuteQuery();

                Items.AddRange(collListItem);
                position = collListItem.ListItemCollectionPosition;

            } while (position != null);
        }

        public void LoadAllItems()
        {
            LoadAllItems(null);            
        }

        public void LoadAllItemsById(string Id)
        {
            Id = $"{Id}".Replace("-", "").ToLower();
            if (!string.IsNullOrEmpty(Id))
            {
                Id = $"_{Id}";
                CamlQuery query = new CamlQuery();
                query.ViewXml = $"<View><Query><Where><Contains><FieldRef Name='FileRef' /><Value Type='Text'>{Id}</Value></Contains></Where></Query><RowLimit>100</RowLimit></View>";                
                LoadAllItems(query);

                var fls = new List<ListItem>(Items);
                Items = new List<ListItem>();
                foreach(var itm in fls)
                {
                    query = new CamlQuery();
                    query.FolderServerRelativeUrl = $"{itm["FileRef"]}";
                    LoadAllItems(query);
                }
            }
            else
            {
                LoadAllItems();
            }
        }

        private T Convert<T>(Dictionary<string, object> keyValues)
        {
            T v = Activator.CreateInstance<T>();
            foreach(var p in v.GetType().GetProperties().Where(x => x.CanWrite))
            {
                var attrs = p.GetCustomAttributes(typeof(SPFieldAttribute), false);
                if (attrs.Length == 0) continue;
                var attr = attrs[0] as SPFieldAttribute;

                var value = keyValues.Keys.Contains(attr.Name) ? keyValues[attr.Name] : null;
                if (value == null) continue;
                switch(attr.EntityType.ToLower())
                {
                    case "lookup":
                        p.SetValue(v, value);
                        break;
                    case "datetime":
                        p.SetValue(v, DateTime.Parse($"{value}"));
                        break;
                    case "number":
                        p.SetValue(v, int.Parse($"{value}")) ;
                        break;
                    default:
                        p.SetValue(v, $"{value}");
                        break;
                }
            }

            return v;
        }

        public List<T> Convert<T>()
        {
            var results = new List<T>();

            this.Items.ForEach(x =>
            {
                results.Add(Convert<T>(x.FieldValues));
            });
            return results;
        }
    }
}
