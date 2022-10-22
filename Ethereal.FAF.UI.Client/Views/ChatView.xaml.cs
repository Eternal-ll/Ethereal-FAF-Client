using Ethereal.FAF.UI.Client.ViewModels;
using GEmojiSharp;
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
            Resources["OpenPrivateCommand"] = viewmodel.OpenPrivateCommand;
            InitializeComponent();
        }

        public ChatViewModel ViewModel { get; }

        private readonly List<string> History = new();
        private int HistoryIndex;

        private void TextBox_KeyDown(object sender, System.Windows.Input.KeyEventArgs e)
        {
            var textbox = (TextBox)sender;
            if (e.Key is System.Windows.Input.Key.Enter)
            {
                if (!ViewModel.SendMessageCommand.CanExecute(textbox.Text)) return;
                ViewModel.SendMessageCommand.Execute(textbox.Text);
                History.Add(textbox.Text);
                HistoryIndex = History.Count - 1;
                textbox.Text = null;
                return;
            }
        }
        (string channel, int users)[] Channels;
        bool waitingChannels;
        private void AutoSuggestBox_TextChanged(object sender, TextChangedEventArgs e)
        {
            if (waitingChannels) return;
            var box = (Wpf.Ui.Controls.AutoSuggestBox)sender;
            var text = box.Text;
            if (!string.IsNullOrWhiteSpace(text))
            {
                box.ItemsSource ??= ViewModel.SuggestList.AsObservable;
            }
            if (ViewModel.SuggestList.Count == 0 && !string.IsNullOrWhiteSpace(text))
            {
                waitingChannels = true;
                Task.Run(async () =>
                {
                    var channels = await ViewModel.GetChannelsAsync();
                    if (channels is null)
                    {
                        return;
                    }
                    ViewModel.SuggestList.AddRange(channels
                    .OrderByDescending(c => c.users)
                    .Select(c => $"{c.channel} ({c.users})"));
                }).ContinueWith(t =>
                {
                    waitingChannels = false;
                });
            }
            if (text == "-")
            {
                ViewModel.SuggestList.Clear();
                //Channels = null;
                box.ItemsSource = null;
                return;
            }
            //if (box.ItemsSource is null && Channels is not null)
            //{
            //    box.ItemsSource = Channels
            //        .OrderByDescending(c => c.users)
            //        .Select(c => $"{c.channel} ({c.users})");
            //}
            if (string.IsNullOrWhiteSpace(text) && e.Changes.Any(t => t.RemovedLength > 0))
            {
                box.ItemsSource = null;
                box.IsSuggestionListOpen = false;
            }
            //var founded = Channels.Where(c => c.channel.Contains(text, System.StringComparison.OrdinalIgnoreCase));
        }

        private void AutoSuggestBox_LostFocus(object sender, System.Windows.RoutedEventArgs e)
        {
            var box = (Wpf.Ui.Controls.AutoSuggestBox)sender;
            if (!string.IsNullOrWhiteSpace(box.Text)) return;
            box.IsSuggestionListOpen = false;
        }

        private void AutoSuggestBox_KeyDown(object sender, System.Windows.Input.KeyEventArgs e)
        {
            if (e.Key is not System.Windows.Input.Key.Enter) return;
            var box = (Wpf.Ui.Controls.AutoSuggestBox)sender;
            if (ViewModel.JoinChannelCommand.CanExecute(box.Text))
            {
                ViewModel.JoinChannelCommand.Execute(box.Text);
                box.Text = null;
            }
        }

        private void InputTextChanged(object sender, TextChangedEventArgs e)
        {
            var input = (Wpf.Ui.Controls.TextBox)sender;
            var text = input.Text;
            var splits = text.Split(':');
            if (splits.Length <= 2) return;
            if (!splits.Any(t => !t.Contains(' '))) return;
            var before = input.Text.Length;
            var selected = input.SelectionStart;
            input.Text = Emoji.Emojify(input.Text);
            if (input.Text.Length > before)
                selected -= input.Text.Length - before;
            if (input.SelectionStart != selected) input.SelectionStart = selected;
        }

        private void TextBox_PreviewKeyDown(object sender, System.Windows.Input.KeyEventArgs e)
        {
            var box = (Wpf.Ui.Controls.TextBox)sender;
            if (e.Key is System.Windows.Input.Key.Up)
            {
                string msg;
                if (HistoryIndex > 0)
                    msg = History[--HistoryIndex];
                else
                {
                    HistoryIndex = History.Count;
                    msg = History[--HistoryIndex];
                }
                box.Text = msg;
                box.SelectionStart = msg.Length;
            }
            if(e.Key is System.Windows.Input.Key.Down)
            {
                string msg;
                if (History.Count - 1 > HistoryIndex)
                    msg = History[++HistoryIndex];
                else
                {
                    HistoryIndex = -1;
                    msg = History[++HistoryIndex];
                }
                box.Text = msg;
                box.SelectionStart = msg.Length;
            }
        }
    }
}
