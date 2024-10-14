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
using System.Windows.Threading;
using System.Windows.Media.Animation;
using System.Media;

namespace NavigationMockUp
{
    /// <summary>
    /// MainWindow.xaml에 대한 상호 작용 논리
    /// </summary>
    public partial class MainWindow : Window
    {
        private DispatcherTimer timer;
        private bool isMapVisible = false;
        private SoundPlayer clickSound;
        private bool isHandled = false;

        public MainWindow()
        {
            InitializeComponent();
            InitializeTimer();
            InitializeClickSound();
        }

        private void InitializeTimer()
        {
            timer = new DispatcherTimer();
            timer.Interval = TimeSpan.FromSeconds(1);
            timer.Tick += Timer_Tick;
            timer.Start();
        }

        private void InitializeClickSound()
        {
            string exePath = System.Reflection.Assembly.GetExecutingAssembly().Location;
            string exeDir = System.IO.Path.GetDirectoryName(exePath);
            string soundPath = System.IO.Path.Combine(exeDir, "Resources", "click.wav");

            if (System.IO.File.Exists(soundPath))
            {
                clickSound = new SoundPlayer(soundPath);
            }
            else
            {
                MessageBox.Show($"Click sound file not found at: {soundPath}", "Warning", MessageBoxButton.OK, MessageBoxImage.Warning);
            }
        }

        private void Timer_Tick(object sender, EventArgs e)
        {
            UpdateDateTime();
        }

        private void UpdateDateTime()
        {
            DateTime now = DateTime.Now;
            CultureInfo enUS = new CultureInfo("en-US");
            
            label_date.Content = now.ToString("MM.dd");
            label_time.Content = now.ToString("hh:mm tt", enUS);
        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            try
            {
                string exePath = System.Reflection.Assembly.GetExecutingAssembly().Location;
                string exeDir = System.IO.Path.GetDirectoryName(exePath);
                string backgroundVideoPath = System.IO.Path.Combine(exeDir, "Resources", "Background.mp4");
                string mapVideoPath = System.IO.Path.Combine(exeDir, "Resources", "MapVideo.mp4");

                if (System.IO.File.Exists(backgroundVideoPath))
                {
                    Uri backgroundVideoUri = new Uri(backgroundVideoPath, UriKind.Absolute);
                    backgroundVideo.Source = backgroundVideoUri;
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
                    MessageBox.Show($"Background video file not found at: {backgroundVideoPath}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                }

                if (System.IO.File.Exists(mapVideoPath))
                {
                    Uri mapVideoUri = new Uri(mapVideoPath, UriKind.Absolute);
                    mapVideo.Source = mapVideoUri;
                    mapVideo.MediaOpened += MapVideo_MediaOpened;
                    mapVideo.MediaFailed += MapVideo_MediaFailed;
                    mapVideo.MediaEnded += MapVideo_MediaEnded;
                    mapVideo.Stretch = Stretch.UniformToFill;
                    
                    // 비디오 품질 향상을 위한 설정
                    mapVideo.SetValue(RenderOptions.BitmapScalingModeProperty, BitmapScalingMode.HighQuality);
                    RenderOptions.SetEdgeMode(mapVideo, EdgeMode.Aliased);

                    // 맵 비디오는 초기에 재생하지 않습니다.
                }
                else
                {
                    MessageBox.Show($"Map video file not found at: {mapVideoPath}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error loading videos: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
            UpdateDateTime(); // 초기 로드 시 시간 업데이트
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

        private void MapVideo_MediaOpened(object sender, RoutedEventArgs e)
        {
            Console.WriteLine("Map video loaded successfully");
        }

        private void MapVideo_MediaFailed(object sender, ExceptionRoutedEventArgs e)
        {
            MessageBox.Show($"Failed to load map video: {e.ErrorException.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
        }

        private void MapVideo_MediaEnded(object sender, RoutedEventArgs e)
        {
            mapVideo.Position = TimeSpan.Zero;
            mapVideo.Play();
        }

        private void buttonExit_Click(object sender, RoutedEventArgs e)
        {
            this.Close();
        }

        private void Window_PreviewMouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            PlayClickSound();
            isHandled = false;
        }

        private void PlayClickSound()
        {
            clickSound?.Play();
        }

        private void Grid_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            if (isHandled) return;

            // 버튼이나 다른 컨트롤이 아닌 경우에만 맵 토글
            if (e.OriginalSource is Grid || e.OriginalSource is Border)
            {
                ToggleMapVisibility();
                isHandled = true;
                e.Handled = true; // 이벤트가 더 이상 전파되지 않도록 함
            }
        }

        private void ToggleMapVisibility()
        {
            if (isMapVisible)
            {
                FadeOut(mapBorder);
                mapVideo.Pause();
            }
            else
            {
                mapBorder.Visibility = Visibility.Visible;
                FadeIn(mapBorder);
                mapVideo.Play();
            }

            isMapVisible = !isMapVisible;
        }

        private void FadeIn(UIElement element)
        {
            DoubleAnimation fadeIn = new DoubleAnimation
            {
                From = 0.0,
                To = 1.0,
                Duration = TimeSpan.FromSeconds(0.5)
            };
            element.BeginAnimation(UIElement.OpacityProperty, fadeIn);
        }

        private void FadeOut(UIElement element)
        {
            DoubleAnimation fadeOut = new DoubleAnimation
            {
                From = 1.0,
                To = 0.0,
                Duration = TimeSpan.FromSeconds(0.5)
            };
            fadeOut.Completed += (s, _) => element.Visibility = Visibility.Collapsed;
            element.BeginAnimation(UIElement.OpacityProperty, fadeOut);
        }
    }

    public class MultiplyByConverter : IValueConverter
    {
        public double Multiplier { get; set; } = 1.0; // 기본값 1.0으로 설정

        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is double doubleValue)
            {
                return doubleValue * Multiplier;
            }
            return value;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }

    public class HeightToFontSizeConverter : IValueConverter
    {
        public double Ratio { get; set; }
        public double BaseSize { get; set; } = 22; // 기본 폰트 크기 (16.5pt에 해당하는 DIUs)

        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is double height)
            {
                return Math.Max(BaseSize, height * Ratio);
            }
            return BaseSize;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }

    public class HeightToMarginConverter : IValueConverter
    {
        public double Ratio { get; set; }

        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is double height)
            {
                double margin = height * Ratio;
                return new Thickness(margin, 0, 0, 0);
            }
            return new Thickness(30, 0, 0, 0); // 기본 마진
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
