using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using System.IO;
using System.Reflection;

namespace NavigationMockUp
{
    /// <summary>
    /// MainWindow.xaml에 대한 상호 작용 논리
    /// </summary>
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();
        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            try
            {
                string exePath = System.Reflection.Assembly.GetExecutingAssembly().Location;
                string exeDir = System.IO.Path.GetDirectoryName(exePath);
                string videoPath = System.IO.Path.Combine(exeDir, "Resources", "Background.mp4");

                if (System.IO.File.Exists(videoPath))
                {
                    Uri videoUri = new Uri(videoPath, UriKind.Absolute);
                    backgroundVideo.Source = videoUri;
                    backgroundVideo.MediaOpened += BackgroundVideo_MediaOpened;
                    backgroundVideo.MediaFailed += BackgroundVideo_MediaFailed;
                    backgroundVideo.MediaEnded += BackgroundVideo_MediaEnded;
                    backgroundVideo.Stretch = Stretch.UniformToFill;
                    
                    // 비디오 품질 향상을 위한 설정
                    backgroundVideo.SetValue(RenderOptions.BitmapScalingModeProperty, BitmapScalingMode.HighQuality);
                    RenderOptions.SetEdgeMode(backgroundVideo, EdgeMode.Aliased);

                    backgroundVideo.Play();
                }
                else
                {
                    MessageBox.Show($"Video file not found at: {videoPath}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error loading video: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void BackgroundVideo_MediaOpened(object sender, RoutedEventArgs e)
        {
            Console.WriteLine("Background video loaded successfully");
            // You can also use MessageBox.Show for a visible confirmation
            // MessageBox.Show("Background video loaded successfully", "Success", MessageBoxButton.OK, MessageBoxImage.Information);
        }

        private void BackgroundVideo_MediaFailed(object sender, ExceptionRoutedEventArgs e)
        {
            MessageBox.Show($"Failed to load background video: {e.ErrorException.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
        }

        private void BackgroundVideo_MediaEnded(object sender, RoutedEventArgs e)
        {
            backgroundVideo.Position = TimeSpan.Zero;
            backgroundVideo.Play();
        }

        private void buttonExit_Click(object sender, RoutedEventArgs e)
        {
            this.Close();
        }
    }

    public class MultiplyByConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is double doubleValue && parameter is string stringParameter)
            {
                if (double.TryParse(stringParameter, out double multiplier))
                {
                    return doubleValue * multiplier;
                }
            }
            return value;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
