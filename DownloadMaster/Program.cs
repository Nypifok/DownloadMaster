using DownloadManager;
using System;
using System.IO;
using System.Text;

namespace DownloadMaster
{
    class Program
    {
        static void Main(string[] args)
        {
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
            //downloader.OnFileProgress += ProgressOutput;
            downloader.OnFailed += ExceptionOutput;
            downloader.OnDownloaded += DownloadedOutput;
            foreach (string str in raws)
            {
                downloader.AddFileToDownloadingQueue(Guid.NewGuid().ToString(), str, @"C:\Users\Nypifok\Desktop\TestDownloads");
            }

            Console.ReadKey();
        }
        static void ProgressOutput(string id,int size,int downloaded)
        {
            Console.WriteLine($"File: {id}   |  Size: {(double)size/1024/1024}MB     |  Downloaded: {(double)downloaded / 1024 / 1024}MB");
        }
        static async void ExceptionOutput(string id, Exception exception)
        {
            string input = ($"\r\n\r\nFileId:{id}\r\nExceptionMessage:{exception.Message}\r\nStackTrace:{exception.StackTrace}");
            using (StreamWriter sw = new StreamWriter(@"C:\Users\Nypifok\Desktop\TestDownloads\log.txt", true, Encoding.Default))
            {
                await sw.WriteLineAsync(input);
            }
        }
        static void DownloadedOutput(string id)
        {
            Console.WriteLine($"File: {id}  downloaded");
        }
    }
}
