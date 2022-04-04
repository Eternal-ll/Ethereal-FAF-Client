using beta.Infrastructure.Utils;
using System;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media.Animation;

namespace beta.Views
{
    /// <summary>
    /// Interaction logic for AuthorizationView.xaml
    /// </summary>
    public partial class AuthorizationView : UserControl
    {
        public AuthorizationView()
        {
            InitializeComponent();

            Player.MediaOpened += Player_MediaOpened;
            Player.MediaEnded += OnTrailerEnded;
            Task.Run(() =>
            {
                var trailer = Tools.GetTrailerFileInfo();
                Dispatcher.Invoke(() => Player.Source = new(trailer.FullName));
            });
            Player.Play();
        }

        private readonly DoubleAnimation ToZeroOpacity = new()
        {
            From = 1,
            To = 0,
            Duration = TimeSpan.FromSeconds(2),
            FillBehavior = FillBehavior.HoldEnd,
            AutoReverse = true
        };

        private void OnTrailerEnded(object sender, RoutedEventArgs e)
        {
            var media = (MediaElement)sender;

            media.BeginAnimation(UIElement.OpacityProperty, ToZeroOpacity);

            media.Position = TimeSpan.Zero;
        }

        private readonly DoubleAnimation ToOneOpacity = new()
        {
            From = 0,
            To = 1,
            Duration = TimeSpan.FromSeconds(2),
            FillBehavior = FillBehavior.HoldEnd
        };

        private void Player_MediaOpened(object sender, RoutedEventArgs e)
        {
            var media = (MediaElement)sender;
            media.BeginAnimation(UIElement.OpacityProperty, ToOneOpacity);

        }
        private bool IsControlInitialized = false;
        private void UserControl_IsVisibleChanged(object sender, DependencyPropertyChangedEventArgs e)
        {
            if (IsControlInitialized && (bool)e.NewValue == false)
            {
                Player.Close();
            }
            if ((bool)e.NewValue == true)
            {
                IsControlInitialized = true;
            }
        }

        private void PasswordBox_PasswordChanged(object sender, RoutedEventArgs e) => 
            LoginButton.CommandParameter = ((PasswordBox)sender).Password;
    }
}
