using System;
using System.Collections.Generic;
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
using Universal.Edge.Models;

// The User Control item template is documented at https://go.microsoft.com/fwlink/?LinkId=234236

namespace Universal.Edge.UserControls
{
    public sealed partial class DisplayItemControl : UserControl
    {
        public DisplayItemControl()
        {
            this.InitializeComponent();
            this.DataContextChanged += DisplayItemControl_DataContextChanged;
        }

        private void DisplayItemControl_DataContextChanged(FrameworkElement sender, DataContextChangedEventArgs args)
        {
            if (ViewModel != null)
            {
                ViewModel.Closed -= ViewModelOnClosed;
                ViewModel.Closed += ViewModelOnClosed;
            }
        }

        private void ViewModelOnClosed(NodeItem nodeItem)
        {
            OnRemove(this);
        }

        public event Action<UserControl> Remove;
        //public event Action<NodeItem> Add;

        public NodeItem ViewModel
        {
            get { return this.DataContext as NodeItem; }
        }
        private void Close_OnClick(object sender, RoutedEventArgs e)
        {
            ViewModel?.Close();
        }

        private void OnRemove(UserControl obj)
        {
            Remove?.Invoke(obj);
        }

        private void Add_OnClick(object sender, RoutedEventArgs e)
        {
            var node = ViewModel.AddItem();
            //OnAdd(node);
        }

        //private void OnAdd(NodeItem obj)
        //{
        //    Add?.Invoke(obj);
        //}
    }
}
