using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Reflection.Metadata.Ecma335;
using System.Runtime.CompilerServices;
using System.Text;
using System.Windows.Controls;

namespace DownloadManagerVisual
{
    public class DownloadProcess : INotifyPropertyChanged
    {
        private string id;
        private string path;
        private string status;
        private string name;
        private double size;
        private double alreadyDownloaded;
        private int downloadedPercent;

        public string Id
        {
            get { return id; }
            set
            {
                id = value;
                OnPropertyChanged("Id");
            }
        }
        public string Path
        {
            get { return path; }
            set
            {
                path = value;
                OnPropertyChanged("Path");
            }
        }
        public string Status
        {
            get { return status; }
            set
            {
                status = value;
                OnPropertyChanged("Status");
            }
        }
        public string Name
        {
            get { return name; }
            set
            {
                name = value;
                OnPropertyChanged("Name");
            }
        }
        public double Size
        {
            get { return size; }
            set
            {
                size = value;
                OnPropertyChanged("Size");
            }
        }
        public double AlreadyDownloaded
        {
            get { return alreadyDownloaded; }
            set
            {
                alreadyDownloaded = value;
                OnPropertyChanged("AlreadyDownloaded");
                OnPropertyChanged("DownloadedPercent");
            }
        }
        public int DownloadedPercent
        {
            get
            {
                try
                {
                    var tmp = Convert.ToInt32(Math.Round((AlreadyDownloaded / Size) * 100, 0));
                    downloadedPercent = tmp;
                }
                catch (OverflowException)
                {
                    downloadedPercent = 0;
                }
                return downloadedPercent;
            }
            set
            {
                downloadedPercent = value;
                OnPropertyChanged("DownloadedPercent");
            }
        }


        public event PropertyChangedEventHandler PropertyChanged;
        public void OnPropertyChanged([CallerMemberName] string prop = "")
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(prop));
        }
    }
}
