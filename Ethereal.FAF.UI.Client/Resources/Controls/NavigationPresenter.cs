using CommunityToolkit.Mvvm.Input;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Windows;
using Wpf.Ui;

namespace Ethereal.FAF.UI.Client.Resources.Controls
{
    public class NavigationPresenter : System.Windows.Controls.Control
    {
        /// <summary>
        /// Property for <see cref="ItemsSource"/>.
        /// </summary>
        public static readonly DependencyProperty ItemsSourceProperty = DependencyProperty.Register(
            nameof(ItemsSource),
            typeof(object),
            typeof(NavigationPresenter),
            new PropertyMetadata(null)
        );

        /// <summary>
        /// Property for <see cref="TemplateButtonCommand"/>.
        /// </summary>
        public static readonly DependencyProperty TemplateButtonCommandProperty = DependencyProperty.Register(
            nameof(TemplateButtonCommand),
            typeof(IRelayCommand),
            typeof(NavigationPresenter),
            new PropertyMetadata(null)
        );

        public object? ItemsSource
        {
            get => GetValue(ItemsSourceProperty);
            set => SetValue(ItemsSourceProperty, value);
        }

        /// <summary>
        /// Gets the command triggered after clicking the titlebar button.
        /// </summary>
        public IRelayCommand TemplateButtonCommand => (IRelayCommand)GetValue(TemplateButtonCommandProperty);

        /// <summary>
        /// Initializes a new instance of the <see cref="NavigationPresenter"/> class.
        /// Creates a new instance of the class and sets the default <see cref="FrameworkElement.Loaded"/> event.
        /// </summary>
        public NavigationPresenter()
        {
            SetValue(TemplateButtonCommandProperty, new RelayCommand<Type>(o => OnTemplateButtonClick(o)));
        }

        private void OnTemplateButtonClick(Type? pageType)
        {
            if (pageType is not null)
            {
                INavigationService navigationService = App.Hosting.Services.GetRequiredService<INavigationService>();
                navigationService.Navigate(pageType);
            }
        }
    }
}
