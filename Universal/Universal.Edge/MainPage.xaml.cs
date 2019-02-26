using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
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
using Universal.Edge.Repos;

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

        private void ButtonBase_OnClick(object sender, RoutedEventArgs e)
        {
            var t1 = new tempfile()
            {
                Id = 1,
                DT = DateTime.UtcNow,
            };
            var t2 = new tempfile()
            {
                Id = 2,
                DT = DateTime.Now,
            };
            FileStorageRepo repo = new FileStorageRepo();
            var list1 = repo.Save(t1).ToList();
            var list2 = repo.Save(t2).ToList();
            var list = new[] {t1, t2};
            var join = (from tempfile in list
                join tempfile1 in list2 on tempfile.Id equals tempfile1.Id
                select new
                {
                    Orig = tempfile,
                    Local = tempfile1
                }).ToList();
            foreach (var item in join)
            {
                Debug.WriteLine(item.Local.DT.Kind, "Local kind");
                Debug.WriteLine(item.Orig.DT.Kind, "Original kind");
                Debug.WriteLine(item.Local.DT == item.Orig.DT, "Local Original equal");
            }
        }
    }
}
