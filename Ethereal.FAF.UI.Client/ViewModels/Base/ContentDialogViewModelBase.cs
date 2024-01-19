using CommunityToolkit.Mvvm.ComponentModel;
using System;
using System.Windows;
using Wpf.Ui.Controls;

namespace Ethereal.FAF.UI.Client.ViewModels.Base
{
    public class ContentDialogViewModelBase : ViewModel
    {
        public virtual string? Title { get; set; }

        // Events for button clicks
        public event EventHandler<ContentDialogResult> PrimaryButtonClick;
        public event EventHandler<ContentDialogResult> SecondaryButtonClick;
        public event EventHandler<ContentDialogResult> CloseButtonClick;

        public virtual void OnPrimaryButtonClick()
        {
            PrimaryButtonClick?.Invoke(this, ContentDialogResult.Primary);
        }

        public virtual void OnSecondaryButtonClick()
        {
            SecondaryButtonClick?.Invoke(this, ContentDialogResult.Secondary);
        }

        public virtual void OnCloseButtonClick()
        {
            CloseButtonClick?.Invoke(this, ContentDialogResult.None);
        }

        /// <summary>
        /// Return a <see cref="BetterContentDialog"/> that uses this view model as its content
        /// </summary>
        public virtual ContentDialog GetDialog()
        {
            Application.Current.Dispatcher.VerifyAccess();

            var dialog = new ContentDialog(new()
            {

            })
            { Title = Title, Content = this,
            
            };

            return dialog;
        }
    }
}
