using Ethereal.FAF.UI.Client.ViewModels;
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
            InitializeComponent();
        }

        public ChatViewModel ViewModel { get; }
    }
}
