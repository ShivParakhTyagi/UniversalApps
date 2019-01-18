using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices.WindowsRuntime;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Navigation;
using Newtonsoft.Json;

// The Blank Page item template is documented at https://go.microsoft.com/fwlink/?LinkId=234238

namespace Universal.Edge
{
    /// <summary>
    /// An empty page that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class ReferenceHandlingPage : Page
    {
        ObservableCollection<ReferenceHandlingPageData> tempList;

        public ReferenceHandlingPage()
        {
            this.InitializeComponent();
            tempList = new ObservableCollection<ReferenceHandlingPageData>();
        }

        private ReferenceHandlingPageData Item = new ReferenceHandlingPageData()
        {
            Data = "Sample Data"
        };
        private void ButtonBase_OnClick(object sender, RoutedEventArgs e)
        {
            this.SampleListView.ItemsSource = tempList;
            var data= new[]
            {
                Item,
                Item,
                Item,
            };
            var rli = JsonConvert.SerializeObject(data, new JsonSerializerSettings()
            {
                PreserveReferencesHandling = PreserveReferencesHandling.All
            });
            var li = JsonConvert.SerializeObject(data);
            var rl = JsonConvert.DeserializeObject<List<ReferenceHandlingPageData>>(rli, new JsonSerializerSettings()
            {
                PreserveReferencesHandling = PreserveReferencesHandling.None
            });
            var l = JsonConvert.DeserializeObject<List<ReferenceHandlingPageData>>(li);
            tempList.Clear();
            tempList.Add(Item);
            foreach (var referenceHandlingPageData in rl)
            {
                tempList.Add(referenceHandlingPageData);
            }
            foreach (var referenceHandlingPageData in l)
            {
                tempList.Add(referenceHandlingPageData);
            }
        }

        public class ReferenceHandlingPageData : INotifyPropertyChanged
        {
            private string _data;

            public string Data
            {
                get { return _data; }
                set
                {
                    _data = value;
                    OnPropertyChanged(nameof(Data));
                }
            }

            public event PropertyChangedEventHandler PropertyChanged;

            protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
            {
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
            }
        }
    }

}
