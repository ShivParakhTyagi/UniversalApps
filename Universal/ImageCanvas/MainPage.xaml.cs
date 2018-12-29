using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.Storage;
using Windows.Storage.Pickers;
using Windows.UI;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Media.Imaging;
using Windows.UI.Xaml.Navigation;
using Windows.UI.Xaml.Shapes;
using ImageCanvas.Models;

// The Blank Page item template is documented at https://go.microsoft.com/fwlink/?LinkId=402352&clcid=0x409

namespace ImageCanvas
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

        private async void BeginButton_OnClick(object sender, RoutedEventArgs e)
        {

            try
            {

                FileOpenPicker picker = new FileOpenPicker();
                picker.FileTypeFilter.Add(".jpg");
                picker.FileTypeFilter.Add(".jpeg");
                picker.FileTypeFilter.Add(".png");
                picker.FileTypeFilter.Add("*");
                var file = await picker.PickSingleFileAsync();
                if (file == null)
                {
                    return;
                }
                MyCanvas.Children.Clear();
                Image image = new Image();
                BitmapImage bitmapImage = new BitmapImage();
                using (var accessStream = await file.OpenAsync(FileAccessMode.Read))
                {
                    await bitmapImage.SetSourceAsync(accessStream);
                }

                image.Source = bitmapImage;
                /*MyCanvas.Width = bitmapImage.PixelWidth;
                MyCanvas.Height = bitmapImage.PixelHeight;*/
                MyCanvas.Children.Add(image);
                var data = MockData.GetCoordinatesMatrix(JsonTextBox.Text);
                if (data == null)
                {
                    return;
                }
                Polyline polyline = new Polyline();
                
                MyCanvas.Children.Add(polyline);

                polyline.StrokeThickness = 4;
                polyline.Stroke = new SolidColorBrush(Colors.Pink);
                polyline.FillRule = FillRule.EvenOdd;

                if (polyline.Points == null)
                {
                    polyline.Points = new PointCollection();
                }

                MyCanvas.Children.Add(data.TopLeft.CreatePoint());
                polyline.Points.Add(data.TopLeft.Point);

                MyCanvas.Children.Add(data.TopRight.CreatePoint());
                polyline.Points.Add(data.TopRight.Point);

                MyCanvas.Children.Add(data.BottomLeft.CreatePoint());
                polyline.Points.Add(data.BottomLeft.Point);

                MyCanvas.Children.Add(data.BottomRight.CreatePoint());
                polyline.Points.Add(data.BottomRight.Point);

                foreach (var coordinate in data.UpperTubesCoordinates)
                {
                    MyCanvas.Children.Add(coordinate.CreatePoint(Colors.GreenYellow));
                    polyline.Points.Add(coordinate.Point);

                }
                foreach (var coordinate in data.LowerTubesCoordinates)
                { 
                    MyCanvas.Children.Add(coordinate.CreatePoint(Colors.Yellow));
                    polyline.Points.Add(coordinate.Point);
                }
            }
            catch (Exception exception)
            {
                Debug.Write(exception);
            }
        }
    }
}
