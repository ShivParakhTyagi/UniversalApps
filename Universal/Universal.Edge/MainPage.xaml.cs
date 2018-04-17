using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using System.Threading.Tasks;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.UI;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Navigation;
using Universal.Edge.Models;
using Universal.Edge.UserControls;

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


        private void Start_OnClick(object sender, RoutedEventArgs e)
        {

            var node = new NodeItem()
            {
                Text = $"Start {InputTextBox.Text}",
                Index = 1,
            };

            Node_Added(node);
            Display("Start");
            //Begin();
            Begin3();
            //Display("Start 2");
            //Begin2();
        }

        private void Begin3()
        {
            var s = DateTime.Now;
            Display($"Begin");
            IEnumerable<int> res = GetItems3();
            Display($"Fetched");
            Display($"ForEach Begin");
            res = res.Where(x => x % 3 == 0);
            //foreach (var val in res)
            //{
            //    Display($"ForEach:Value: {val}");
            //    if (val % 3 == 0)
            //    {
            //        Display($"Divisible by 3: {val}");
            //    }
            //}
            var enu = res.GetEnumerator();
            do
            {
                Display($"while: {enu.Current}");
            } while (enu.MoveNext());
            foreach (var val in res)
            {
                Display($"ForEach:Value: {val}");
                if (val % 3 == 0)
                {
                    Display($"Divisible by 3: {val}");
                }
            }

            //var items = res.Where(x => x % 4 == 0 || x % 3 == 0).Where(x => x > 1 && x < 50);
            //foreach (var item in items)
            //{
            //    Display($"{DateTime.Now.TimeOfDay} items:{item}");
            //}

            //var enumerator = items.GetEnumerator();
            //enumerator.Dispose();
            ////enumerator.Reset();
            //for (int i = 0; i < 10; i++)
            //{
            //    Display($"{DateTime.Now.TimeOfDay} Enumerator:{enumerator.Current}");
            //    var r = enumerator.MoveNext();

            //}
            //enumerator.Dispose();
            //var list = items.ToList();
            //foreach (var item in list)
            //{
            //    Display($"{DateTime.Now.TimeOfDay} list:{item}");
            //}
            Display($"ForEach End");
            var e = DateTime.Now;
            var diff = e - s;
            Display($"{diff}");
        }

        private IEnumerable<int> GetItems3()
        {
            Display($"GetItems Begin");
            for (int i = 0; i < 50; i++)
            {
                //Task.Delay(TimeSpan.FromSeconds(1));
                Display($"\nGetItems {i}");
                if (i % 2 == 0)
                {
                    Display($"Divisible by 2 : {i}");
                    yield return i;
                    Display($"After Divisible by 2 : {i}");

                }
            }

            Display($"GetItems End");
        }

        private void Begin()
        {
            var s = DateTime.Now;
            Display($"Begin");
            IEnumerable<string> res = GetItems();

            Display($"Fetched");
            Display($"ForEach Begin");
            foreach (var val in res)
            {
                Display($"ForEach:Value: {val}");
            }
            Display($"ForEach End");
            var e = DateTime.Now;
            var diff = e - s;
            Display($"{diff}");
        }

        private IEnumerable<string> GetItems()
        {
            Display($"GetItems Begin");
            for (int i = 0; i < 10; i++)
            {
                Display($"GetItems {i}");

                yield return $"Number : {i}";
            }
            Display($"GetItems End");
        }

        private void Begin2()
        {
            var s = DateTime.Now;
            Display($"Begin");
            IEnumerable<string> res = GetItems2();
            Display($"Fetched");
            Display($"ForEach Begin");
            foreach (var val in res)
            {
                Display($"ForEach:Value: {val}");
            }
            Display($"ForEach End");
            var e = DateTime.Now;
            var diff = e - s;
            Display($"{diff}");
        }

        private List<string> GetItems2()
        {
            List<string> list = new List<string>();
            Display($"GetItems Begin");
            for (int i = 0; i < 10; i++)
            {
                Display($"GetItems {i}");

                list.Add($"Number : {i}");
            }

            Display($"GetItems End");
            return list;
        }


        private void Display(string text = "")
        {
            OutputTextBox.Text = $"{OutputTextBox.Text}{text}\n";
        }

        private void Control_Remove(UserControl obj)
        {
            ContainerStackPanel?.Children?.Remove(obj);
        }

        private void Node_Added(NodeItem node)
        {
            if (node != null)
            {
                node.Added -= Node_Added;
                node.Added += Node_Added;
            }

            var control = new DisplayItemControl()
            {
                DataContext = node
            };
            control.Remove += Control_Remove;
            //control.Add += Node_Added;

            ContainerStackPanel.Children.Add(control);
        }

    }
}
