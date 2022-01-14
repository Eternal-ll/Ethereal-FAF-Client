using beta.Infrastructure.Services.Interfaces;
using System.Security.Cryptography;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using TechLifeForum;

namespace beta.Views
{
    /// <summary>
    /// Interaction logic for ChatView.xaml
    /// </summary>
    public partial class ChatView : UserControl
    {
        //private readonly Irc Client; 
        private readonly IIRCService IRCService;
        private readonly IrcClient IrcClient;
        
        public ChatView()
        {
            InitializeComponent();
            //IRCService = App.Services.GetRequiredService<IIRCService>();
            //IRCService.Message += IRCService_Message;
            // username = player id
            // nick = player name
            //Client = new("irc.faforever.com", 6667);
            IrcClient = new IrcClient("116.202.155.226", 6697);
            
            string nick = Properties.Settings.Default.PlayerNick;
            string id = Properties.Settings.Default.PlayerId.ToString();
            string password = Properties.Settings.Default.irc_password;
            IrcClient.Nick = nick;
            IrcClient.ServerPass = password;
            IrcClient.Connect();
            //IrcClient.JoinChannel("#aeolus");
            IrcClient.ServerMessage += IrcClient_ServerMessage;
            IrcClient.ChannelMessage += IrcClient_ChannelMessage;
        }

        private void IrcClient_ChannelMessage(object sender, ChannelMessageEventArgs e)
        {
            e.Message = e.Message.Replace('\n', ' ');
            e.From = e.From.Replace('\n', ' ');
            AeolusControl.Items.Add(e.From + ": " + e.Message);
        }

        private void IrcClient_ServerMessage(object sender, StringEventArgs e)
        {
            e.Result = e.Result.Replace('\n', ' ');
            ServerControl.Items.Add(e.Result);
        }

        private bool s = false;
        private void ButtonBase_OnClick(object sender, RoutedEventArgs e)
        {
            if (!s)
            {
                IrcClient.JoinChannel("#aeolus");
                s = true;
                return;
            }

            if (string.IsNullOrWhiteSpace(TextBox.Text)) return;
            string nick = Properties.Settings.Default.PlayerNick;
            IrcClient.SendMessage("#aeolus", TextBox.Text);
            AeolusControl.Items.Add(nick + ": " + TextBox.Text);
            TextBox.Clear();
        }

        private void MenuItem_OnClick(object sender, RoutedEventArgs e)
        {
        }
    }
}
