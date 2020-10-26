using DownloadManager;
using DownloadManagerVisual.ViewModels;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace DownloadManagerVisual
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();
            string input;

            using (var stream = File.OpenRead(@"C:\Users\Nypifok\Desktop\TestDownloads\input.txt"))
            {
                byte[] array = new byte[stream.Length];
                stream.Read(array, 0, array.Length);
                string textFromFile = Encoding.Default.GetString(array);
                input = textFromFile;
            }


            var raws = input.Split("\r\n");
            var downloader = new FileDownloader();
            DataContext = new DownloadProcessViewModel(downloader);
            foreach (string str in raws)
            {
                Task.Factory.StartNew(() => downloader.AddFileToDownloadingQueue(Guid.NewGuid().ToString(), str, @"C:\Users\Nypifok\Desktop\TestDownloads"));
            }

        }
    }
}
