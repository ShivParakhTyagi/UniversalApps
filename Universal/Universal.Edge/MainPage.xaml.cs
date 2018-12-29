using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using System.Threading.Tasks;
using Windows.Data.Pdf;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.Globalization;
using Windows.Graphics.Imaging;
using Windows.Media.Ocr;
using Windows.Storage.Pickers;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Media.Imaging;
using Windows.UI.Xaml.Navigation;

// The Blank Page item template is documented at https://go.microsoft.com/fwlink/?LinkId=402352&clcid=0x409

namespace Universal.Edge
{
    /// <summary>
    /// An empty page that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class MainPage : Page
    {
        public MainPage()
        {
            this.InitializeComponent();
        }

        private async void OcrKaro()
        {
            try
            {
                FileOpenPicker picker = new FileOpenPicker();
                picker.FileTypeFilter.Add(".pdf");
                var file = await picker.PickSingleFileAsync();
                if (file == null) return;
                var pdfDocument = await PdfDocument.LoadFromFileAsync(file);
                var page = pdfDocument.GetPage(0);
                //BitmapImage bitmapImage = new BitmapImage();
                SoftwareBitmap softwareBitmap;
                using (var ms = new MemoryStream())
                {
                    using (var randomAccessStream = ms.AsRandomAccessStream())
                    {
                        await page.RenderToStreamAsync(randomAccessStream);
                        await randomAccessStream.FlushAsync();
                        //await bitmapImage.SetSourceAsync(randomAccessStream);
                        randomAccessStream.Seek(0);
                        BitmapDecoder bitmapDecoder = await BitmapDecoder.CreateAsync(randomAccessStream);

                        softwareBitmap =
                            await bitmapDecoder.GetSoftwareBitmapAsync(BitmapPixelFormat.Bgra8,
                                BitmapAlphaMode.Premultiplied, new BitmapTransform(),
                                ExifOrientationMode.IgnoreExifOrientation,
                                ColorManagementMode.DoNotColorManage);

                        OcrEngine ocrEnglishEngine = OcrEngine.TryCreateFromUserProfileLanguages();
                        if (ocrEnglishEngine == null) return;
                        OcrEngine ocrJapaneseEngine = OcrEngine.TryCreateFromLanguage(new Language("ja-jp"));
                        if (ocrJapaneseEngine == null) return;

                        OcrResult ocrEnglishResult = await ocrEnglishEngine.RecognizeAsync(softwareBitmap);
                        OcrResult ocrJapaneseResult = await ocrJapaneseEngine.RecognizeAsync(softwareBitmap);

                        string extractedText = ocrEnglishResult.Text + ocrJapaneseResult.Text;
                        OcrResultTextBox.Text = extractedText;
                        JapLinesListView.ItemsSource = ocrJapaneseResult.Lines.ToArray();
                        EngLinesListView.ItemsSource = ocrEnglishResult.Lines.ToArray();
                    }
                }
            }
            catch (Exception ex)
            {

            }
        }

        private void StartOCRButton_OnClick(object sender, RoutedEventArgs e)
        {
            OcrKaro();
        }
    }
}
