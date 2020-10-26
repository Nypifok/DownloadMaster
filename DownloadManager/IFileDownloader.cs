using System;

namespace DownloadManager
{
    public interface IFileDownloader 
    {
        void SetDegreeOfParallelism(int degreeOfParallelism);
        void AddFileToDownloadingQueue(string fileId,string url,string pathToSave);
        event Action<string> OnDownloaded;
        event Action<string, Exception> OnFailed;
        event Action<string, int, int> OnFileProgress;
    }
}
