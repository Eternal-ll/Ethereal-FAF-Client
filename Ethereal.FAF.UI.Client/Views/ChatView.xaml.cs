using Ethereal.FAF.UI.Client.ViewModels;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Controls;
using Wpf.Ui.Common.Interfaces;

namespace Ethereal.FAF.UI.Client.Views
{
    /// <summary>
    /// Interaction logic for ChatView.xaml
    /// </summary>
    public partial class ChatView : INavigableView<ChatViewModel>
    {
        public ChatView(ChatViewModel viewmodel)
        {
            ViewModel = viewmodel;
            Resources["LeaveChannelCommand"] = viewmodel.LeaveChannelCommand;
            Resources["SendMessageCommand"] = viewmodel.SendMessageCommand;
            InitializeComponent();
        }

        public ChatViewModel ViewModel { get; }

        private void TextBox_KeyDown(object sender, System.Windows.Input.KeyEventArgs e)
        {
            if (e.Key is not System.Windows.Input.Key.Enter) return;
            var textbox = (TextBox)sender;
            if (!ViewModel.SendMessageCommand.CanExecute(textbox.Text)) return;
            ViewModel.SendMessageCommand.Execute(textbox.Text);
            textbox.Text = null;
        }
        (string channel, int users)[] Channels;
        bool waitingChannels;
        private void AutoSuggestBox_TextChanged(object sender, TextChangedEventArgs e)
        {
            if (waitingChannels) return;
            var box = (Wpf.Ui.Controls.AutoSuggestBox)sender;
            var text = box.Text;
            if (Channels is null && !string.IsNullOrWhiteSpace(text))
            {
                waitingChannels = true;
                Task.Run(async () =>
                {
                    Channels = await ViewModel.GetChannelsAsync();
                    waitingChannels = false;
                });
            }
            if (text == "-")
            {
                Channels = null;
                box.ItemsSource = null;
                return;
            }
            if (box.ItemsSource is null && Channels is not null)
            {
                box.ItemsSource = Channels
                    .OrderByDescending(c => c.users)
                    .Select(c => $"{c.channel} ({c.users})");
            }
            //var founded = Channels.Where(c => c.channel.Contains(text, System.StringComparison.OrdinalIgnoreCase));
        }

        private void AutoSuggestBox_LostFocus(object sender, System.Windows.RoutedEventArgs e)
        {
            var box = (Wpf.Ui.Controls.AutoSuggestBox)sender;
            if (!string.IsNullOrWhiteSpace(box.Text)) return;
            box.IsSuggestionListOpen = false;
        }
    }
}
