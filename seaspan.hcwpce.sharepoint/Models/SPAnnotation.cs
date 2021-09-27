using GemBox.Document;
using Microsoft.SharePoint.Client;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Media;

namespace seaspan.hcwpce.sharepoint.Models
{
    public class SPAnnotation
    {
        private string _objectid { get; set; }
        public List<Annotiation> Annotiations { get; set; }
        public SPAnnotation(string objectid)
        {
            _objectid = objectid;
            Annotiations = new List<Annotiation>();
        }

        public void LoadImages()
        {
            //seaspan.hcwpce.sharepoint.Models.SPList tl = new seaspan.hcwpce.sharepoint.Models.SPList("Condition Rating");
            //tl.LoadAllItemsById("79A90A395192EB11B1AC000D3AE8516D");
            var client = SPContext.CurrentInstance.SPClient;
            foreach (var lib in Constants.spot_libraries.Split(",".ToCharArray()))
            {
                var list = new SPList(lib);
                list.LoadAllItemsById(_objectid);
                if (list.Items.Count > 0)
                {
                    foreach (var itm in list.Items)
                    {
                        var ann = new Annotiation();
                        ann.LibraryName = lib;
                        var v = ann.Get(itm.FieldValues, "ID");
                        ann.annotationid = $"{v}";
                        ann.ID = int.Parse($"{v}");
                        v = ann.Get(itm.FieldValues, "Author");
                        ann.CreatedBy = ann.Get(itm.FieldValues, "Author") as FieldUserValue;
                        ann.CreatedOn = DateTime.Parse($"{ann.Get(itm.FieldValues, "Created")}");
                        ann.filesize = int.Parse($"{ann.Get(itm.FieldValues, "File_x0020_Size")}");
                        ann.url = $"{ann.Get(itm.FieldValues, "FileRef")}";
                        ann.subject = $"{ann.Get(itm.FieldValues, "Title")}";
                        ann.notetext = $"{ann.Get(itm.FieldValues, "Comments")}";
                        var t = $"{ann.Get(itm.FieldValues, "File_x0020_Type")}";
                        switch(t.ToLower())
                        {
                            case "wav":
                            case "amr":
                                t = $"audio/{t}";
                                break;
                            case "jpeg":
                            case "png":
                            case "jpg":
                                t = $"image/{t}";
                                break;
                            case "mov":
                            case "3pg":
                            case "quicktime":
                                t = $"video/{t}";
                                break;
                            case "pdf":
                            case "octet-stream":
                            case "vnd.openxmlformats-officedocument.wordprocessingml.document":
                                t = $"application/{t}";
                                break;                                
                        }
                        ann.mimetype = t;
                        ann.filename = $"{ann.Get(itm.FieldValues, "FileLeafRef")}";
                        Annotiations.Add(ann);
                    }
                    break;
                }
            }
        }

        public static KeyValuePair<string, byte[]> LoadImage(string libraryname, int Id)
        {
            var client = SPContext.CurrentInstance.SPClient;
            var list = SPContext.CurrentInstance.GetList(libraryname);
            var item = list.GetItemById(Id);
            client.Load(item);
            client.ExecuteQuery();

            var file = item.File;
            client.Load(file);
            client.ExecuteQuery();

            var fstream = file.OpenBinaryStream();
            client.ExecuteQuery();

            if (fstream != null && fstream.Value != null)
            {
                using (System.IO.MemoryStream memoryStream = new System.IO.MemoryStream())
                {
                    byte[] buffer = new byte[32768];
                    int bytesRead;
                    do
                    {
                        bytesRead = fstream.Value.Read(buffer, 0, buffer.Length);
                        memoryStream.Write(buffer, 0, bytesRead);
                    } while (bytesRead != 0);

                    return new KeyValuePair<string, byte[]>(file.Name, memoryStream.ToArray());
                }
            }
            return new KeyValuePair<string, byte[]>(file.Name, null);
        }
    }
}
