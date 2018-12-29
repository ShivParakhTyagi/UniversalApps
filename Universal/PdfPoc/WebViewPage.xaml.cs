using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.Storage;
using Windows.Storage.Pickers;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Navigation;
using Windows.Web.Http;

// The Blank Page item template is documented at https://go.microsoft.com/fwlink/?LinkId=234238

namespace PdfPoc
{
    /// <summary>
    /// An empty page that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class WebViewPage : Page
    {
        public WebViewPage()
        {
            this.InitializeComponent();
        }
        private async void ReportWebView_OnNewWindowRequested(WebView sender, WebViewNewWindowRequestedEventArgs args)
        {
            try
            {
                bool showPrint = false;
                args.Handled = true;
                using (HttpClient client = new HttpClient())
                {
                    using (HttpRequestMessage requestMessage = new HttpRequestMessage(HttpMethod.Get, args.Uri))
                    {
                        using (var response = await client.SendRequestAsync(requestMessage))
                        {
                            if (response.IsSuccessStatusCode == false)
                            {
                                return;
                            }

                            var queryDictionary = args.Uri.Query.TrimStart('?').Split('&')
                                .ToDictionary(x => x.Split('=').FirstOrDefault(), x => x.Split('=').LastOrDefault());

                            if (queryDictionary.Keys.Any(x => x == "rc:PrintOnOpen"))
                            {
                                bool.TryParse(queryDictionary["rc:PrintOnOpen"], out showPrint);
                            }

                            var fileFormat = queryDictionary["Format"];
                            var buffer = await response.Content.ReadAsBufferAsync();
                            StorageFile file;

                            if (showPrint)
                            {
                                file = await ApplicationData.Current.LocalCacheFolder.CreateFileAsync(
                                    $"{queryDictionary["ExecutionID"]}_{queryDictionary["FileName"]}.pdf",
                                    CreationCollisionOption.GenerateUniqueName);
                                using (var stream = await file.OpenStreamForWriteAsync())
                                {
                                    using (var bufferStream = buffer.AsStream())
                                    {
                                        await bufferStream.CopyToAsync(stream);
                                        await bufferStream.FlushAsync();
                                    }
                                }
                            }
                            else
                            {
                                var fileName = $"{queryDictionary["ExecutionID"]}_{queryDictionary["FileName"]}";
                                var savePicker = new FileSavePicker();
                                savePicker.SuggestedStartLocation =
                                    PickerLocationId.DocumentsLibrary;
                                switch (fileFormat)
                                {
                                    case "WORDOPENXML":
                                        {
                                            // Dropdown of file types the user can save the file as
                                            savePicker.FileTypeChoices.Add("Word Document", new List<string>() { ".docx" });
                                            // Default file name if the user does not type one in or select a file to replace
                                            savePicker.SuggestedFileName = fileName + ".docx";
                                        }
                                        break;
                                    case "EXCELOPENXML":
                                        {
                                            // Dropdown of file types the user can save the file as
                                            savePicker.FileTypeChoices.Add("Excel", new List<string>() { ".xlsx" });
                                            // Default file name if the user does not type one in or select a file to replace
                                            savePicker.SuggestedFileName = fileName + ".xlsx";
                                        }
                                        break;
                                    case "PPTX":
                                        {
                                            // Dropdown of file types the user can save the file as
                                            savePicker.FileTypeChoices.Add("PPT", new List<string>() { ".pptx" });
                                            // Default file name if the user does not type one in or select a file to replace
                                            savePicker.SuggestedFileName = fileName + ".pptx";
                                        }
                                        break;
                                    case "PDF":
                                        {
                                            // Dropdown of file types the user can save the file as
                                            savePicker.FileTypeChoices.Add("PDF", new List<string>() { ".pdf" });
                                            // Default file name if the user does not type one in or select a file to replace
                                            savePicker.SuggestedFileName = fileName + ".pdf";
                                        }
                                        break;
                                    case "IMAGE":
                                        {
                                            // Dropdown of file types the user can save the file as
                                            savePicker.FileTypeChoices.Add("IMAGE - TIFF", new List<string>() { ".tiff" });
                                            // Default file name if the user does not type one in or select a file to replace
                                            savePicker.SuggestedFileName = fileName + ".tiff";
                                        }
                                        break;
                                    case "MHTML":
                                        {
                                            // Dropdown of file types the user can save the file as
                                            savePicker.FileTypeChoices.Add("MHTML", new List<string>() { ".mhtml" });
                                            // Default file name if the user does not type one in or select a file to replace
                                            savePicker.SuggestedFileName = fileName + ".mhtml";
                                        }
                                        break;
                                    case "CSV":
                                        {
                                            // Dropdown of file types the user can save the file as
                                            savePicker.FileTypeChoices.Add("CSV", new List<string>() { ".csv" });
                                            // Default file name if the user does not type one in or select a file to replace
                                            savePicker.SuggestedFileName = fileName;
                                        }
                                        break;
                                    case "XML":
                                        {
                                            // Dropdown of file types the user can save the file as
                                            savePicker.FileTypeChoices.Add("XML - XML file with Report Data",
                                                new List<string>() { ".xml" });
                                            // Default file name if the user does not type one in or select a file to replace
                                            savePicker.SuggestedFileName = fileName + ".xml";
                                        }
                                        break;
                                    case "ATOM":
                                        {
                                            // Dropdown of file types the user can save the file as
                                            savePicker.FileTypeChoices.Add("ATOM - Data Feed",
                                                new List<string>() { ".atom" });
                                            // Default file name if the user does not type one in or select a file to replace
                                            savePicker.SuggestedFileName = fileName + ".atom";
                                        }
                                        break;
                                    default:
                                        {
                                            // Dropdown of file types the user can save the file as
                                            savePicker.FileTypeChoices.Add("Plain Text", new List<string>() { ".txt" });
                                            // Default file name if the user does not type one in or select a file to replace
                                            savePicker.SuggestedFileName = fileName + ".txt";
                                        }
                                        break;
                                }


                                file = await savePicker.PickSaveFileAsync();
                                //FolderPicker picker = new FolderPicker();
                                //picker.FileTypeFilter.Add("*");
                                //var folder = await picker.PickSingleFolderAsync();

                                //StorageFile destinationFile = await folder.CreateFileAsync(filename,
                                //    CreationCollisionOption.GenerateUniqueName);

                                using (var stream = await file.OpenStreamForWriteAsync())
                                {
                                    using (var bufferStream = buffer.AsStream())
                                    {
                                        await bufferStream.CopyToAsync(stream);
                                        await bufferStream.FlushAsync();
                                    }
                                }

                            }

                            Windows.System.Launcher.LaunchFileAsync(file);
                            //BackgroundDownloader downloader = new BackgroundDownloader();
                            //downloader.SetRequestHeader("Authorization",
                            //    $"Bearer {Mvx.Resolve<ILoginConfig>().TokenDataResult?.AccessToken}");
                            //DownloadOperation download = downloader.CreateDownload(args.Uri, destinationFile);

                            //// Attach progress and completion handlers.
                            ////HandleDownloadAsync(download, true);
                            //var t = await download.StartAsync();
                        }
                    }
                }
            }
            catch (Exception ex)
            {
            }
        }
    }
}
