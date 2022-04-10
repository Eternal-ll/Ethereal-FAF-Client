using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media.Animation;
using System.Windows.Media.Imaging;

namespace beta.Resources.Controls
{
    class GifImage : Image
    {
        private bool _isInitialized;
        private Int32Animation _animation;

        public int FrameIndex
        {
            get { return (int)GetValue(FrameIndexProperty); }
            set { SetValue(FrameIndexProperty, value); }
        }

        private void Initialize()
        {
            if (!string.IsNullOrEmpty(GifSource))
            {
                Uri uri = GifSource.StartsWith("http") ? new(GifSource) : new("pack://application:,,," + GifSource);
                if (GifSource[1] == ':')
                    uri = new Uri(GifSource, UriKind.Absolute);
                GifBitmapDecoder = new GifBitmapDecoder(uri, BitmapCreateOptions.PreservePixelFormat, BitmapCacheOption.Default);
            }
            _animation = new Int32Animation(0, GifBitmapDecoder.Frames.Count - 1, new Duration(new TimeSpan(0, 0, 0, GifBitmapDecoder.Frames.Count / 10, (int)((GifBitmapDecoder.Frames.Count / 10.0 - GifBitmapDecoder.Frames.Count / 10) * 1000))));
            _animation.RepeatBehavior = RepeatBehavior.Forever;
            Source = GifBitmapDecoder.Frames[0];

            _isInitialized = true;
        }

        static GifImage()
        {
            VisibilityProperty.OverrideMetadata(typeof(GifImage),
                new FrameworkPropertyMetadata(VisibilityPropertyChanged));
        }

        private static void VisibilityPropertyChanged(DependencyObject sender, DependencyPropertyChangedEventArgs e)
        {
            if ((Visibility)e.NewValue == Visibility.Visible)
            {
                ((GifImage)sender).StartAnimation();
            }
            else
            {
                ((GifImage)sender).StopAnimation();
            }
        }

        public static readonly DependencyProperty FrameIndexProperty =
            DependencyProperty.Register("FrameIndex", typeof(int), typeof(GifImage), new UIPropertyMetadata(0, new PropertyChangedCallback(ChangingFrameIndex)));

        static void ChangingFrameIndex(DependencyObject obj, DependencyPropertyChangedEventArgs ev)
        {
            var gifImage = obj as GifImage;
            gifImage.Source = gifImage.GifBitmapDecoder.Frames[(int)ev.NewValue];
        }

        /// <summary>
        /// Defines whether the animation starts on it's own
        /// </summary>
        public bool AutoStart
        {
            get { return (bool)GetValue(AutoStartProperty); }
            set { SetValue(AutoStartProperty, value); }
        }

        public static readonly DependencyProperty AutoStartProperty =
            DependencyProperty.Register("AutoStart", typeof(bool), typeof(GifImage), new UIPropertyMetadata(false, AutoStartPropertyChanged));

        private static void AutoStartPropertyChanged(DependencyObject sender, DependencyPropertyChangedEventArgs e)
        {
            if ((bool)e.NewValue)
                (sender as GifImage).StartAnimation();
        }

        public string GifSource
        {
            get { return (string)GetValue(GifSourceProperty); }
            set { SetValue(GifSourceProperty, value); }
        }

        public static readonly DependencyProperty GifSourceProperty =
            DependencyProperty.Register("GifSource", typeof(string), typeof(GifImage), new UIPropertyMetadata(string.Empty, GifSourcePropertyChanged));

        private static void GifSourcePropertyChanged(DependencyObject sender, DependencyPropertyChangedEventArgs e)
        {
            (sender as GifImage).Initialize();
        }

        public GifBitmapDecoder GifBitmapDecoder
        {
            get { return (GifBitmapDecoder)GetValue(GifBitmapDecoderProperty); }
            set
            {
                SetValue(GifBitmapDecoderProperty, value);
                _animation = new Int32Animation(
                    0,
                    value.Frames.Count - 1,
                    new Duration(new TimeSpan(0, 0, 0, value.Frames.Count / 10, (int)((value.Frames.Count / 10.0 - value.Frames.Count / 10) * 1000))));
                _animation.RepeatBehavior = RepeatBehavior.Forever;
                Source = value.Frames[0];
            }
        }

        public static readonly DependencyProperty GifBitmapDecoderProperty =
            DependencyProperty.Register("GifBitmapDecoder", typeof(GifBitmapDecoder), typeof(GifImage), new UIPropertyMetadata(null));

        /// <summary>
        /// Starts the animation
        /// </summary>
        public void StartAnimation()
        {
            if (!_isInitialized)
                Initialize();

            BeginAnimation(FrameIndexProperty, _animation);
        }

        /// <summary>
        /// Stops the animation
        /// </summary>
        public void StopAnimation()
        {
            BeginAnimation(FrameIndexProperty, null);
        }
    }
}
