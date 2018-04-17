using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using System.Threading;
using System.Threading.Tasks;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.UI.Core;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Media.Animation;
using Windows.UI.Xaml.Navigation;

// The Blank Page item template is documented at https://go.microsoft.com/fwlink/?LinkId=234238

namespace Universal.Edge.Views
{
    /// <summary>
    /// An empty page that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class ProgressBarPage : Page
    {
        private int _speed = 1000, _current, _max;

        public ProgressBarPage()
        {
            this.InitializeComponent();
        }

        private void StartButton_OnClick(object sender, RoutedEventArgs e)
        {
            int.TryParse(this.AnimationSpeed.Text, out _current);
            int.TryParse(this.CountTextBox.Text, out _max);
            var timer = new Timer(SpeedChanger, null, 0, (int) TimeSpan.FromSeconds(.7).TotalMilliseconds);
            AnimateProgressBarUnitInSconds(_current++, _max, _speed);
        }

        private void SpeedChanger(object state)
        {
            Dispatcher.RunAsync(CoreDispatcherPriority.Normal, () =>
            {

                var nv = 1;
                if (SpeedSlider.Value + nv < SpeedSlider.Maximum)
                {
                    SpeedSlider.Value += nv;
                }
            });
        }

        private async void StepButton_OnClick(object sender, RoutedEventArgs e)
        {
            AnimateProgressBarUnitInSconds(_current++, _max, _speed);
        }

        private CancellationTokenSource CancellationTokenSource;

        private async void AnimateProgressBarUnitInSconds(double current, double max, int seconds)
        {
            CancellationTokenSource?.Cancel();
            double c = current;
            //double m = max;
            double m = c + 1;
            var ts = TimeSpan.FromSeconds(seconds);
            CancellationTokenSource = new CancellationTokenSource(ts);
            var token = CancellationTokenSource.Token;
            double count;
            double delta = 0;
            for (double i = c; i < m && token.IsCancellationRequested == false; i += delta)
            {
                count = SpeedSlider.Value <= 0 ? 1 : SpeedSlider.Value;
                delta = (m - c) / count;

                SetProgress(i, max);
                await Task.Delay((int)(1 / delta));
            }

            if (token.IsCancellationRequested == false)
            {
                AnimateProgressBarUnitInSconds(_current++, _max, _speed);
            }
        }

        private async void SetProgress(double d, double d1)
        {
            await Task.Yield();
            Dispatcher.RunAsync(CoreDispatcherPriority.Normal, () =>
            {
                AnimatedProgressBar.Maximum = d1;
                AnimatedProgressBar.Value = d;
            });
        }


        private void CompleteButton_OnClick(object sender, RoutedEventArgs e)
        {

        }

    }
}
