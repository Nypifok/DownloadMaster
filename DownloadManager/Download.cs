using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace DownloadManager
{
    public class Download
    {
        public string Id { get; }

        public string Url { get; }

        public string PathToSave { get; }
        public Task DownloadTask { get; set; }
        
        public Download(string id,string url,string path,Task downloadTask)
        {
            Id = id;
            Url = url;
            PathToSave = path;
            DownloadTask = downloadTask;
        }
    }
}
