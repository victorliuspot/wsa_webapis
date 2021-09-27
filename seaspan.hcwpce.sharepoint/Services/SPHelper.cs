using Microsoft.SharePoint.Client;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace seaspan.hcwpce.sharepoint.Services
{
    public class SPHelper
    {
        public static void WithRetry(Action x, string description = "")
        {
            for (int i = 0; i <= 6; i++) //7 attempts, last attempt will sleep for (3^6)*100 = 72900ms = 72.9s = 1.2min
            {
                try
                {
                    x();
                    return;
                }
                catch (Exception e)
                {
                    int sleep = (int)(Math.Pow(2, i) * 100);
                    System.Threading.Thread.Sleep(sleep);
                }
            }
            throw new Exception(string.Format("Max retries reached for ({0})", description));
        }

        public T ConvertTo<T>(Dictionary<string, object> keyValues)
        {
            T v = Activator.CreateInstance<T>();
            foreach(var p in v.GetType().GetRuntimeProperties().Where(x => x.CanWrite))
            {
                //var kv = keyValues.TryGetValue(p.)
            }

            return v;
        }

        public File Upload(string folderUrl, System.IO.Stream stream, string fileName, Dictionary<string, object> properties)
        {
            if (string.IsNullOrEmpty(folderUrl))
                throw new ArgumentNullException("folderUrl");
            if (string.IsNullOrEmpty(fileName))
                throw new ArgumentNullException("fileName");

            var web = SPContext.CurrentInstance.Web;
            if (!web.IsPropertyAvailable("ServerRelativeUrl"))
            {
                web.Context.Load(web, w => w.ServerRelativeUrl);
                web.Context.ExecuteQuery();
            }
            var folder = web.GetFolderByServerRelativeUrl(web.ServerRelativeUrl + folderUrl);
            web.Context.Load(folder);
            web.Context.ExecuteQuery();            
            if (folder == null)
                throw new Exception(string.Format("Folder, {0}, not found", folderUrl));

            SPContext.CurrentInstance.SPClient.RequestTimeout = 60000 * 20; //20 mins
            if (stream.CanSeek)
                stream.Position = 0;
            return UploadFileSlicePerSlice(SPContext.CurrentInstance.SPClient, stream, folder, fileName, properties);
        }

        public Microsoft.SharePoint.Client.File UploadFileSlicePerSlice(ClientContext ctx, System.IO.Stream stream, Folder folder, string fileName, Dictionary<string, object> properties, int fileChunkSizeInMB = 2)
        {
            // Each sliced upload requires a unique ID.
            Guid uploadId = Guid.NewGuid();

            File uploadFile;

            // Calculate block size in bytes.
            int blockSize = fileChunkSizeInMB * 1024 * 1024;

            // Get the size of the file.
            long fileSize = stream.Length;

            if (fileSize <= blockSize)
            {
                // Use regular approach.
                using (System.IO.MemoryStream ms = new System.IO.MemoryStream())
                {
                    stream.CopyTo(ms);
                    ms.Position = 0;
                    FileCreationInformation fileInfo = new FileCreationInformation();                    
                    fileInfo.ContentStream = ms;
                    fileInfo.Url = fileName;
                    fileInfo.Overwrite = true;
                    uploadFile = folder.Files.Add(fileInfo);                    
                    ctx.Load(uploadFile);
                    ctx.ExecuteQuery();
                    var item = uploadFile.ListItemAllFields;
                    foreach(var kk in properties.Keys)
                    {
                        item[kk] = properties[kk];
                    }
                    item.Update();
                    WithRetry(() =>
                    {
                        ctx.ExecuteQuery();
                    });

                    // Return the file object for the uploaded file.
                    return uploadFile;
                }
            }
            else
            {
                // Use large file upload approach.
                ClientResult<long> bytesUploaded = null;

                System.IO.Stream fs = stream;
                try
                {
                    System.IO.BinaryReader br = new System.IO.BinaryReader(fs);
                    byte[] buffer = new byte[blockSize];
                    Byte[] lastBuffer = null;
                    long fileoffset = 0;
                    long totalBytesRead = 0;
                    int bytesRead;
                    bool first = true;
                    bool last = false;

                    // Read data from file system in blocks. 
                    while ((bytesRead = br.Read(buffer, 0, buffer.Length)) > 0)
                    {
                        totalBytesRead = totalBytesRead + bytesRead;

                        // You've reached the end of the file.
                        if (totalBytesRead == fileSize)
                        {
                            last = true;
                            // Copy to a new buffer that has the correct size.
                            lastBuffer = new byte[bytesRead];
                            Array.Copy(buffer, 0, lastBuffer, 0, bytesRead);
                        }

                        if (first)
                        {
                            using (System.IO.MemoryStream contentStream = new System.IO.MemoryStream())
                            {
                                // Add an empty file.
                                FileCreationInformation fileInfo = new FileCreationInformation();
                                fileInfo.ContentStream = contentStream;
                                fileInfo.Url = fileName;
                                fileInfo.Overwrite = true;
                                uploadFile = folder.Files.Add(fileInfo);
                                ctx.Load(uploadFile);
                                ctx.ExecuteQuery();
                                var item = uploadFile.ListItemAllFields;
                                foreach (var kk in properties.Keys)
                                {
                                    item[kk] = properties[kk];
                                }
                                item.Update();
                                ctx.ExecuteQuery();
                                WithRetry(() =>
                                {
                                    // Start upload by uploading the first slice. 
                                    using (System.IO.MemoryStream s = new System.IO.MemoryStream(buffer))
                                    {
                                        bytesUploaded = uploadFile.StartUpload(uploadId, s);
                                        ctx.ExecuteQuery();

                                        // fileoffset is the pointer where the next slice will be added.
                                        fileoffset = bytesUploaded.Value;
                                    }
                                });
                                // You can only start the upload once.
                                first = false;
                            }
                        }
                        else
                        {
                            // Get a reference to your file.
                            uploadFile = ctx.Web.GetFileByServerRelativeUrl(folder.ServerRelativeUrl + System.IO.Path.AltDirectorySeparatorChar + fileName);

                            if (last)
                            {                                
                                WithRetry(() =>
                                {
                                    // Is this the last slice of data?
                                    using (System.IO.MemoryStream s = new System.IO.MemoryStream(lastBuffer))
                                    {
                                        // End sliced upload by calling FinishUpload.
                                        uploadFile = uploadFile.FinishUpload(uploadId, fileoffset, s);
                                        ctx.ExecuteQuery();                                        
                                    }                                    
                                });
                                // Return the file object for the uploaded file.
                                return uploadFile;
                            }
                            else
                            {
                                WithRetry(() =>
                                {
                                    using (System.IO.MemoryStream s = new System.IO.MemoryStream(buffer))
                                    {
                                        bytesUploaded = uploadFile.ContinueUpload(uploadId, fileoffset, s);
                                        ctx.ExecuteQuery();

                                        // Update fileoffset for the next slice.
                                        fileoffset = bytesUploaded.Value;
                                    }
                                });
                            }
                        }

                    } // while ((bytesRead = br.Read(buffer, 0, buffer.Length)) > 0)
                }
                finally
                {                    
                }
            }

            return null;
        }
    }
}
