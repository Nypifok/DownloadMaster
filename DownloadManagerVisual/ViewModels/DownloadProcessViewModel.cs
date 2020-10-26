using DownloadManager;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Windows.Threading;
using System.Threading.Tasks;

namespace DownloadManagerVisual.ViewModels
{
    class DownloadProcessViewModel : INotifyPropertyChanged
    {
        private DownloadProcess selectedDownload;
        public delegate void NextPrimeDelegate();
        private string folder;
        public string Folder
        {
            get { return folder; }
            set
            {
                folder = value;
                OnPropertyChanged("Folder");
            }
        }
        private double folderSize;
        public double FolderSize
        {
            get
            {
                folderSize = ReturnDownloadsSize();
                return folderSize;
            }
            set
            {
                folderSize = value;
                OnPropertyChanged("FolderSize");
            }
        }
        public DownloadProcess SelectedDownload
        {
            get { return selectedDownload; }
            set
            {
                selectedDownload = value;
                OnPropertyChanged("SelectedDownload");
            }
        }
        public DownloadProcessViewModel(FileDownloader fileDownloader)
        {
            Folder = "CHIKIRAY";
            Downloads = new ObservableCollection<DownloadProcess>();
            fileDownloader.OnFileProgress += UpdatedDownloadHandler;
            fileDownloader.OnDownloaded += DownloadedHandler;
            fileDownloader.OnFailed += DownloadExceptionHandler;
        }

        public ObservableCollection<DownloadProcess> Downloads { get; set; }


        public event PropertyChangedEventHandler PropertyChanged;
        public void OnPropertyChanged([CallerMemberName] string prop = "")
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(prop));
        }
        private double ReturnDownloadsSize()
        {
            return (double)Downloads.Sum(d => d.Size);
        }
        public void UpdatedDownloadHandler(string id, int totalBytes, int downloadedBytes)
        {
            App.Current.Dispatcher.BeginInvoke(new Action(() =>
            {
                var download = Downloads.FirstOrDefault(d => d.Id == id);
                if (download != null)
                {
                    download.AlreadyDownloaded = Math.Round((double)downloadedBytes / 1024 / 1024, 3);
                    download.Size = Math.Round((double)totalBytes / 1024 / 1024, 3);
                    download.Status = download.DownloadedPercent + "%";
                }
                else
                {
                    var size = Math.Round((double)totalBytes / 1024 / 1024, 3);
                    var downloaded = Math.Round((double)downloadedBytes / 1024 / 1024, 3);
                    var tmpDownload = new DownloadProcess { Id = id, Size = size, AlreadyDownloaded = downloaded };
                    FolderSize = ReturnDownloadsSize();
                    tmpDownload.Status = tmpDownload.DownloadedPercent + "%";
                    Downloads.Add(tmpDownload);

                }
            }));


        }
        public void DownloadedHandler(string id)
        {
            App.Current.Dispatcher.BeginInvoke(new Action(() =>
            {
                var download = Downloads.FirstOrDefault(d => d.Id == id);
                download.AlreadyDownloaded = download.Size;
                download.Status = 100 + "%";
            }));


        }
        public void DownloadExceptionHandler(string id, Exception exception)
        {
            App.Current.Dispatcher.BeginInvoke(new Action(() =>
            {
                var download = Downloads.FirstOrDefault(d => d.Id == id);
                if(download!=null)
                download.Status = exception.Message;
            }));
        }
    }
}
