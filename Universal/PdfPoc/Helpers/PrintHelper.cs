using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using Windows.ApplicationModel.Core;
using Windows.Graphics.Printing;
using Windows.Graphics.Printing.OptionDetails;
using Windows.Storage;
using Windows.Storage.Pickers;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Printing;

namespace PdfPoc.Helpers
{
    class PrintHelper
    {
        protected PrintDocument printDocument;

        protected IPrintDocumentSource printDocumentSource;


        internal List<UIElement> printPreviewPages;


        protected event EventHandler PreviewPagesCreated;

        protected FrameworkElement firstPage;

        public event Action<bool, string, int, int> PrintingProgress;


        public PrintHelper()
        {
            printPreviewPages = new List<UIElement>();
        }



        public virtual void RegisterForPrinting()
        {
            printDocument = new PrintDocument();
            printDocumentSource = printDocument.DocumentSource;
            printDocument.Paginate += CreatePrintPreviewPages;
            printDocument.GetPreviewPage += GetPrintPreviewPage;
            printDocument.AddPages += AddPrintPages;

            PrintManager printMan = PrintManager.GetForCurrentView();
            printMan.PrintTaskRequested -= PrintTaskRequested;
            printMan.PrintTaskRequested += PrintTaskRequested;
        }

        public virtual void UnregisterForPrinting()

        {

            if (printDocument == null)

            {
                return;
            }

            printDocument.Paginate -= CreatePrintPreviewPages;
            printDocument.GetPreviewPage -= GetPrintPreviewPage;
            printDocument.AddPages -= AddPrintPages;

            // Remove the handler for printing initialization.
            PrintManager printMan = PrintManager.GetForCurrentView();
            printMan.PrintTaskRequested -= PrintTaskRequested;
        }

        public virtual void CreatePrintPreviewPages(object sender, PaginateEventArgs e)
        {
            PrintDocument printDoc = (PrintDocument)sender;
            int requestCount = 0;
            // A new "session" starts with each paginate event.
            Interlocked.Increment(ref requestCount);

            //PageDescription pageDescription = new PageDescription();

            // Get printer's page description.
            PrintTaskOptionDetails printDetailedOptions = PrintTaskOptionDetails.GetFromPrintTaskOptions(e.PrintTaskOptions);
            PrintPageDescription printPageDescription = e.PrintTaskOptions.GetPageDescription(0);

            // Reset the error state
            //printDetailedOptions.Options["photoSize"].ErrorText = string.Empty;

            // Compute the printing page description (page size & center printable area)
            //pageDescription.PageSize = printPageDescription.PageSize;

            //pageDescription.Margin.Width = Math.Max(
            //    printPageDescription.ImageableRect.Left,
            //    printPageDescription.ImageableRect.Right - printPageDescription.PageSize.Width);

            //pageDescription.Margin.Height = Math.Max(
            //    printPageDescription.ImageableRect.Top,
            //    printPageDescription.ImageableRect.Bottom - printPageDescription.PageSize.Height);

            //pageDescription.ViewablePageSize.Width = printPageDescription.PageSize.Width - pageDescription.Margin.Width * 2;
            //pageDescription.ViewablePageSize.Height = printPageDescription.PageSize.Height - pageDescription.Margin.Height * 2;

            // Compute print photo area.
            //switch (photoSize)
            //{
            //    case PhotoSize.Size4x6:
            //        pageDescription.PictureViewSize.Width = 4 * DPI96;
            //        pageDescription.PictureViewSize.Height = 6 * DPI96;
            //        break;
            //    case PhotoSize.Size5x7:
            //        pageDescription.PictureViewSize.Width = 5 * DPI96;
            //        pageDescription.PictureViewSize.Height = 7 * DPI96;
            //        break;
            //    case PhotoSize.Size8x10:
            //        pageDescription.PictureViewSize.Width = 8 * DPI96;
            //        pageDescription.PictureViewSize.Height = 10 * DPI96;
            //        break;
            //    case PhotoSize.SizeFullPage:
            //        pageDescription.PictureViewSize.Width = pageDescription.ViewablePageSize.Width;
            //        pageDescription.PictureViewSize.Height = pageDescription.ViewablePageSize.Height;
            //        break;
            //}

            // Try to maximize photo-size based on it's aspect-ratio
            //if ((pageDescription.ViewablePageSize.Width > pageDescription.ViewablePageSize.Height) && (photoSize != PhotoSize.SizeFullPage))
            //{
            //    var swap = pageDescription.PictureViewSize.Width;
            //    pageDescription.PictureViewSize.Width = pageDescription.PictureViewSize.Height;
            //    pageDescription.PictureViewSize.Height = swap;
            //}

            //pageDescription.IsContentCropped = photoScale == Scaling.Crop;

            // Recreate content only when :
            // - there is no current page description
            // - the current page description doesn't match the new one
            //if (currentPageDescription == null || !currentPageDescription.Equals(pageDescription))
            //{
            //    ClearPageCollection();

            //    if (pageDescription.PictureViewSize.Width > pageDescription.ViewablePageSize.Width ||
            //        pageDescription.PictureViewSize.Height > pageDescription.ViewablePageSize.Height)
            //    {
            //printDetailedOptions.Options["photoSize"].ErrorText = StringConstant.PritingStringConstants.PhotoDoesNotFit;

            //// Inform preview that it has only 1 page to show.
            //printDoc.SetPreviewPageCount(1, PreviewPageCountType.Intermediate);

            //        // Add a custom "preview" unavailable page
            //        lock (printSync)
            //        {
            //            pageCollection[0] = new PreviewUnavailable(pageDescription.PageSize, pageDescription.ViewablePageSize);
            //        }
            //    }
            //    else
            //    {
            //        // Inform preview that is has #NumberOfPhotos pages to show.
            //        var count = TotalPages;
            //        printDoc.SetPreviewPageCount(count, PreviewPageCountType.Intermediate);
            //    }

            //    currentPageDescription = pageDescription;
            //}
            
            printDoc.SetPreviewPageCount(1, PreviewPageCountType.Final);
            printDoc.SetPreviewPage(1, printPreviewPages.First());
        }

        public async Task ShowPrintUIAsync()

        {

            // Catch and print out any errors reported

            try

            {

                await PrintManager.ShowPrintUIAsync();

            }

            catch (Exception e)

            {

                Debug.WriteLine(e, "Error occured");
                //await DeviceServices.DeviceService.GetMessageHelper()
                //    .ShowToastMessageAsync($"{StringConstant.PritingStringConstants.PrintFailed}: " + e.Message + ", hr=" + e.HResult);

            }

        }



        protected virtual void PrintTaskRequested(PrintManager sender, PrintTaskRequestedEventArgs e)

        {

            PrintTask printTask = null;

            printTask = e.Request.CreatePrintTask("IO CHECK TOOL", sourceRequested =>

            {
                // Print Task event handler is invoked when the print job is completed.

                printTask.Completed += async (s, args) =>

                {

                    // Notify the user when the print operation fails.

                    if (args.Completion == PrintTaskCompletion.Failed)

                    {
                        var dispatcher = CoreApplication.Views.First().Dispatcher;

                        await dispatcher.RunAsync(Windows.UI.Core.CoreDispatcherPriority.Normal, async () =>

                        {
                            Debug.WriteLine("Printing Failed");
                            //await DeviceServices.DeviceService.GetMessageHelper()
                            //    .ShowToastMessageAsync(StringConstant.PritingStringConstants.PrintFailed);

                        });

                    }

                };



                sourceRequested.SetSource(printDocumentSource);

            });
            printTask.Progressing += PrintTask_Progressing;
            printTask.Previewing += PrintTask_Previewing;
            printTask.Submitting += PrintTask_Submitting;

        }

        private void PrintTask_Submitting(PrintTask sender, object args)
        {
            Debug.WriteLine(args);
        }

        private void PrintTask_Previewing(PrintTask sender, object args)
        {
            Debug.WriteLine(args);

        }

        private void PrintTask_Progressing(PrintTask sender, PrintTaskProgressingEventArgs args)
        {
            Debug.WriteLine(args);
        }

        protected virtual void GetPrintPreviewPage(object sender, GetPreviewPageEventArgs e)

        {

            PrintDocument printDoc = (PrintDocument) sender;

            printDoc.SetPreviewPage(e.PageNumber, printPreviewPages[e.PageNumber - 1]);

        }



        protected virtual void AddPrintPages(object sender, AddPagesEventArgs e)

        {

            // Loop over all of the preview pages and add each one to  add each page to be printied

            for (int i = 0; i < printPreviewPages.Count; i++)

            {

                // We should have all pages ready at this point...

                printDocument.AddPage(printPreviewPages[i]);

            }



            PrintDocument printDoc = (PrintDocument) sender;



            // Indicate that all of the print pages have been provided

            printDoc.AddPagesComplete();

        }

        private static readonly SemaphoreSlim _progressMutex = new SemaphoreSlim(1);

        protected virtual void OnPrintingProgress(bool arg1, string arg2, int arg3, int arg4)
        {
            try
            {
                _progressMutex.Wait();
                PrintingProgress?.Invoke(arg1, arg2, arg3, arg4);
            }
            finally
            {
                _progressMutex.Release();
            }
        }

        protected bool IsDisposed;

        public void Dispose()
        {
            IsDisposed = true;
        }
    }


}