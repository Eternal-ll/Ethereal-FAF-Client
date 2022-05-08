using beta.Infrastructure.Utils;
using beta.ViewModels;
using System.Windows;
using System.Windows.Controls;

namespace beta.Views
{
    /// <summary>
    /// Interaction logic for ChatView.xaml
    /// </summary>
    public partial class ChatView : UserControl
    {

        public ScrollViewer ChannelUsersScrollViewer { get; }
        public ScrollViewer HistoryListBoxScrollViewer { get; }

        public ChatView()
        {
            InitializeComponent();
            ChannelUsersScrollViewer = Tools.FindChild<ScrollViewer>(ChannelUsers);
            HistoryListBoxScrollViewer = Tools.FindChild<ScrollViewer>(HistoryListBox);

            HistoryListBoxScrollViewer.ScrollChanged += HistoryListBoxScrollViewer_ScrollChanged;
            DataContextChanged += ChatView_DataContextChanged;

            MouseDown += ChatView_MouseDown;
            FocusableChanged += ChatView_FocusableChanged;
        }

        bool AtTheBottom = false;
        private void HistoryListBoxScrollViewer_ScrollChanged(object sender, ScrollChangedEventArgs e)
        {
            var scroll = (ScrollViewer)sender;
            if (e.ExtentHeightChange > 0 && AtTheBottom)
            {
                scroll.ScrollToBottom();
            }
            AtTheBottom = scroll.ScrollableHeight == e.VerticalOffset;
        }

        private void ChatView_DataContextChanged(object sender, DependencyPropertyChangedEventArgs e)
        {
            if (e.NewValue is ChatViewModel vm)
            {
                vm.TestInputControl = TestInputControl;
            }
        }
        private void OnShowJoinToChannelClick(object sender, RoutedEventArgs e) => JoinChannelInput.Focus();

        bool IsSettingsOpen = false;
        private void ToggleButton_Checked(object sender, RoutedEventArgs e)
        {
            IsSettingsOpen = true;
        }

        private void ChatView_MouseDown(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            //if (IsSettingsOpen)
            //{
            //    if (!(AdditionalSettingsBtn.IsMouseOver || SettingsPopup.IsMouseOver))
            //    {
            //        AdditionalSettingsBtn.IsChecked = false;
            //    }
            //}
        }
        private void ChatView_FocusableChanged(object sender, DependencyPropertyChangedEventArgs e)
        {
            //if (IsSettingsOpen)
            //{
            //    if (!(AdditionalSettingsBtn.IsMouseOver || SettingsPopup.IsMouseOver))
            //    {
            //        AdditionalSettingsBtn.IsChecked = false;
            //    }
            //}
        }
    }
}
