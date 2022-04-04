using System;
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
            
        }

        readonly DoubleAnimation animation = new DoubleAnimation
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

            media.BeginAnimation(UIElement.OpacityProperty, animation);

            media.Position = TimeSpan.Zero;
        }

        readonly DoubleAnimation ToOneOpacity = new DoubleAnimation
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

        private bool Initialized = false;
        private void UserControl_IsVisibleChanged(object sender, DependencyPropertyChangedEventArgs e)
        {
            if (Initialized && (bool)e.NewValue == false)
            {
                Player.Volume = 0;
            }
            if ((bool)e.NewValue == true)
            {
                Initialized = true;
            }
        }

        private void PasswordBox_PasswordChanged(object sender, RoutedEventArgs e) => 
            LoginButton.CommandParameter = ((PasswordBox)sender).Password;
    }
}
