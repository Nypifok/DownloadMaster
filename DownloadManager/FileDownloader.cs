using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;

namespace DownloadManager
{
    public class FileDownloader : IFileDownloader
    {
        private int degreeOfParallelism = 4;

        public event Action<string> OnDownloaded;
        public event Action<string, Exception> OnFailed;
        public event Action<Exception> OnDownloaderException;
        public event Action<string, int, int> OnFileProgress;

        public int DegreeOfParallelism
        {
            get
            {
                return degreeOfParallelism;
            }
            private set
            {
                if (DownloadQueue.Count == 0)
                {
                    degreeOfParallelism = value;
                }
                else
                {
                    throw new Exception("Degree of parallelism is immutable now.");
                }
            }
        }

        public ConcurrentQueue<string> DownloadQueue { get; }
        public ConcurrentBag<string> DownloadingNow { get; private set; }
        public ConcurrentDictionary<string, Download> AllDownloads { get; private set; }


        public FileDownloader()
        {
            DownloadQueue = new ConcurrentQueue<string>();
            DownloadingNow = new ConcurrentBag<string>();
            AllDownloads = new ConcurrentDictionary<string, Download>();
        }
        public void AddFileToDownloadingQueue(string fileId, string url, string pathToSave)
        {
            try
            {
                DownloadQueue.Enqueue(fileId);
                var download = new Download(fileId, url, pathToSave, CreateDownload(url, pathToSave, fileId));

                if (!AllDownloads.TryAdd(fileId, download))
                {
                    throw new Exception($"An exception while adding {fileId}");
                }
                Task.Factory.StartNew(() => OnFileProgress?.Invoke(fileId, 0, 0)).Wait();

            }
            catch (Exception ex)
            {
                OnDownloaderException?.Invoke(ex);
            }
            ManageDownloads();
        }

        public void SetDegreeOfParallelism(int degreeOfParallelism)
        {
            try
            {
                DegreeOfParallelism = degreeOfParallelism;
            }
            catch (Exception ex)
            {
                OnDownloaderException?.Invoke(ex);
            }
        }

        private async Task CreateDownload(string url, string pathToSave, string id)
        {
            try
            {
                int fileSize = await GetFileSize(url);
                var finalPathToSave = pathToSave + "\\" + Path.GetFileName(url);
                using (var httpClient = new HttpClient())
                {
                    using (var request = new HttpRequestMessage(HttpMethod.Get, url))
                    {
                        using (
                            Stream contentStream = await (await httpClient.SendAsync(request)).Content.ReadAsStreamAsync(),
                            stream = new FileStream(finalPathToSave, FileMode.Create))
                        {
                            var buffer = new byte[4096];
                            var downloaded = 0;
                            while (true)
                            {
                                var length = contentStream.Read(buffer, 0, buffer.Length);
                                if (length <= 0)
                                {
                                    break;
                                }

                                await stream.WriteAsync(buffer, 0, length);
                                downloaded += length;
                                if(downloaded%102400==0)
                                Task.Factory.StartNew(()=>OnFileProgress?.Invoke(id, fileSize, downloaded)).Wait();
                            }
                        }
                    }
                }
                OnDownloaded?.Invoke(id);
            }
            catch (Exception ex)
            {
                OnFailed?.Invoke(id, ex);
            }
            ManageDownloads();
        }

        private async Task ManageDownloads()
        {
            try
            {
                if (DownloadingNow.Count < DegreeOfParallelism && DownloadQueue.Count > 0)
                {
                    for (int i = 0; i < degreeOfParallelism - DownloadingNow.Count; i++)
                    {
                        string download;
                        if (DownloadQueue.TryDequeue(out download))
                        {
                            DownloadingNow.Add(download);
                        }
                    }
                }
                if (DownloadingNow.Count > 0)
                {
                    var downloads = DownloadingNow.ToArray();
                    foreach (string id in downloads)
                    {
                        var download = GetFromAllDownloads(id);

                        if (download.DownloadTask.Status == TaskStatus.RanToCompletion || download.DownloadTask.Status == TaskStatus.Faulted)
                        {
                            RemoveFromDownloadingNow(id);
                        }
                        if (download.DownloadTask.Status == TaskStatus.WaitingForActivation)
                        {
                            RunDownloadTask(download.DownloadTask);
                        }
                    }

                }
            }
            catch (Exception ex)
            {
                OnDownloaderException?.Invoke(ex);
            }
        }

        private void RemoveFromDownloadingNow(string id)
        {
            string result = id;
            if (!DownloadingNow.TryTake(out result))
            {
                throw new Exception("An exception while removing from DownloadingNow");
            }
        }
        private Download GetFromAllDownloads(string id)
        {
            Download download;
            if (AllDownloads.TryGetValue(id, out download))
            {
                return download;
            }
            else
            {
                throw new Exception("An exception while managing");
            }
        }
        private async Task RunDownloadTask(Task downloadTask)
        {
            await downloadTask;
        }

        private async Task<int> GetFileSize(string url)
        {
            try
            {
                int result = 0;

                System.Net.WebRequest req = System.Net.WebRequest.Create(url);
                req.Method = "HEAD";
                using (System.Net.WebResponse resp = await req.GetResponseAsync())
                {
                    if (long.TryParse(resp.Headers.Get("Content-Length"), out long ContentLength))
                    {
                        result = Convert.ToInt32(ContentLength);
                    }
                }

                return result;
            }
            catch
            {
                return 0;
            }
        }
        
    }
}
