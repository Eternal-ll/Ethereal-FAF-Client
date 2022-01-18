using beta.Infrastructure.Services.Interfaces;
using beta.Models;
using beta.Models.Server;
using beta.Views.Windows;
using ModernWpf;
using ModernWpf.Controls;
using System;
using System.Collections.Generic;
using System.Net.Sockets;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Shapes;
using System.Windows.Threading;
using Button = System.Windows.Controls.Button;

namespace beta.Views
{
    /// <summary>
    /// Interaction logic for MainView.xaml
    /// </summary>
    public partial class MainView : INavigationAware
    {
        private INavigationManager NavigationManager;
        private readonly TcpClient Client;
        private readonly ConnectionHandler ConnectionHandler;
        private readonly IOAuthService OAuthService;

        private readonly Dictionary<Type, UserControl> Views = new();
        
        public MainView()
        {
            InitializeComponent();

            var window = (MainWindow)Application.Current.MainWindow;
            window.TitleProfileSpace.Visibility = Visibility.Visible;

            var lob = new LobbiesView();
            //MainFrame.Content = lob;
            Views.Add(typeof(LobbiesView), new LobbiesView());
            Views.Add(typeof(ChatView), new ChatView());
        }
        public interface IServerMessage
        {
            public string command { get; set; }
        }
        public class NoticeMessage : IServerMessage
        {
            public string command { get; set; }
            public string style { get; set; }
            public string text { get; set; }

        }
        public class SessionMessage : IServerMessage
        {
            public string command { get; set; }
            public long session { get; set; }
        }
        //public class GameInfoMessage: IServerMessage
        //{
        //    public string command { get; set; }
            
        //    public GameInfoMessage[] games { get; set; }

        //    public string visibility { get; set; }
        //    public bool password_protected { get; set; }
        //    public long uid { get; set; }
            
        //    public string title { get; set; }
        //    public string state { get; set; }
        //    public string game_type { get; set; }
        //    public string featured_mod { get; set; }
        //    public Dictionary<string, string> sim_mods { get; set; }
        //    public string map_name { get; set; }
        //    public string map_file_path { get; set; }
        //    public string host { get; set; }
        //    public int num_players { get; set; }
        //    public int max_players { get; set; }
        //    public double? launched_at { get; set; }
        //    public string rating_type { get; set; }
            
        //    public double? rating_min { get; set; }

        //    public double? rating_max { get; set; }
        //    public bool enforce_rating_range { get; set; }
        //    public Dictionary<int, string[]> teams { get; set; }
        //}
        public class GamesMessage : IServerMessage
        {
            public string command { get; set; }
            public GameInfoMessage[] games { get; set; }
        }
        public class QueueMessage : IServerMessage
        {
            public string command { get; set; }
            public QueueMessage[] queues { get; set; }

            public string queue_name { get; set; }
            public string queue_pop_time { get; set; }
            public double queue_pop_time_delta { get; set; }
            public int num_players { get; set; }
            public int[][] boundary_80s { get; set; }
            public int[][] boundary_75s { get; set; }
        }
        public class WelcomeMessage : IServerMessage
        {
            public PlayerInfoMessage me { get; set; }
            public int id { get; set; }
            public string login { get; set; }
            public string command { get; set; }
        }
        public class IRCPasswordMessage : IServerMessage
        {
            public string command { get; set; }
            public string password { get; set; }
        }
        public class SocialMessage : IServerMessage
        {
            public string[] autojoin { get; set; }
            public string[] channels { get; set; }
            public long[] friends { get; set; }
            public long[] foes { get; set; }
            public int power { get; set; }
            public string command { get; set; }
        }
        private class ServerMessage : IServerMessage
        {
            public string command { get; set; }

            #region notice
            public string style { get; set; }
            public string text { get; set; }
            #endregion

            #region session
            public long session { get; set; }
            #endregion

            #region irc_password
            public string password { get; set; }
            #endregion

            #region welcome
            public PlayerInfoMessage me { get; set; }
            public int id { get; set; }
            public string login { get; set; }
            //public string country { get; set; }
            //public string clan { get; set; }
            #endregion

            #region social
            public string[] autojoin { get; set; }
            public string[] channels { get; set; }
            public long[] friends { get; set; }
            public long[] foes { get; set; }
            public int power { get; set; }
            #endregion

            #region player_info
            public string country { get; set; }
            public string clan { get; set; }
            public Dictionary<string, Rating> ratings { get; set; }
            public double[] global_rating { get; set; }
            public double[] ladder_rating { get; set; }
            public int number_of_games { get; set; }
            #endregion

            #region player_info //players
            public PlayerInfoMessage[] players { get; set; }
            #endregion
            
            #region game_info
            public string visibility { get; set; }
            public bool password_protected { get; set; }
            public int uid { get; set; }
            
            public string title { get; set; }
            public string state { get; set; }
            public string game_type { get; set; }
            public string featured_mod { get; set; }
            public Dictionary<string, string> sim_mods { get; set; }
            public string map_name { get; set; }
            public string map_file_path { get; set; }
            public string host { get; set; }
            public int num_players { get; set; }
            public int max_players { get; set; }
            public double? launched_at { get; set; }
            public string rating_type { get; set; }
            
            public double? rating_min { get; set; }

            public double? rating_max { get; set; }
            public bool enforce_rating_range { get; set; }
            public Dictionary<int, string[]> teams { get; set; }

            #endregion

            #region game_info //games
            public GameInfoMessage[] games { get; set; }
            #endregion

            #region matchmaker_info
            public string queue_name { get; set; }
            public string queue_pop_time { get; set; }
            public double queue_pop_time_delta { get; set; }
            //public int num_players { get; set; }
            public int[][] boundary_80s { get; set; }
            public int[][] boundary_75s { get; set; }
            #endregion

            #region matchmaker_info //queues
            public QueueMessage[] queues { get; set; }
            #endregion
        }

        private Dictionary<string, IServerMessage> dict = new();
        private string mesJson = string.Empty;
        //private void Client_DataReceived(object sender, Message e)
        //{
        //    var client = (SimpleTcpClient)sender;

        //    var payloads = e.MessageString.Split("\n");

        //    bool partJsonIsOk = mesJson.Length > 0 && (mesJson[0] == '{' && mesJson[^1] == '}');

        //    for (var index = 0; index < payloads.Length; index++)
        //    {
        //        var json = payloads[index];
        //        if (json.Length == 0) continue;

        //        #region If json is messed up
        //        if (json[0] != '{' || json[^1] != '}')
        //        {
        //            mesJson += json;

        //            if (partJsonIsOk)
        //            {
        //                json = mesJson;

        //                mesJson = string.Empty;
        //            }
        //        } 
        //        #endregion

        //        //LOG(json.Replace(",", ",\n").Replace("{","{\n").Replace("}","\n}"));

        //        StringBuilder builder = new();

        //        #region Get command of payload (VERY FAST KAPPA)
        //        int count = 0;
        //        for (int i = 0; i < 30; i++)
        //        {
        //            if (count > 0)
        //                if (json[i + 1] != '\"') builder.Append(json[i + 1]);
        //                else break;

        //            if (json[i] != ':') continue;
        //            count++;
        //        } 
        //        #endregion

        //        #region Variant One //Disabled
        //        ////IServerMessage message = null;
        //        //try
        //        //{
        //        //    //message = builder.ToString() switch
        //        //    //{
        //        //    //    "notice" => JsonSerializer.Deserialize<NoticeMessage>(json),
        //        //    //    "session" => JsonSerializer.Deserialize<SessionMessage>(json),

        //        //    //    _ => JsonSerializer.Deserialize<ServerMessage>(json)
        //        //    //};
        //        //}
        //        //catch (Exception exception)
        //        //{
        //        //    // ignored
        //        //} 
        //        #endregion

        //        #region Variant Two //Activated
                
        //        switch (builder.ToString())
        //        {
        //            #region Notice
        //            case "notice":

        //                break;
        //            #endregion

        //            #region Session
        //            case "session":
        //                var sessionMessage = JsonSerializer.Deserialize<SessionMessage>(json);

        //                var session = sessionMessage.session.ToString();

        //                // TODO: ASYNC!!!!
        //                var uid = OAuthService.GenerateUID(session).Result;
        //                if (string.IsNullOrWhiteSpace(uid))
        //                {
        //                    //MessageBox.Show("Failed to calculate UID");
        //                    return;
        //                }

        //                Properties.Settings.Default.session = session;
        //                Dictionary<string, string> auth = new()
        //                {
        //                    { "command", "auth" },
        //                    { "token", Properties.Settings.Default.access_token },
        //                    { "unique_id", uid },
        //                    { "session", session }
        //                };
        //                client.Write(Encoding.UTF8.GetBytes(JsonSerializer.Serialize(auth) + "\n"));
        //                break; 
        //            #endregion

        //            #region IRC password
        //            case "irc_password":
        //                var ircPasswordMessage = JsonSerializer.Deserialize<IRCPasswordMessage>(json);
        //                break;
        //            #endregion

        //            #region Welcome
        //            case "welcome":
        //                var welcomeMessage = JsonSerializer.Deserialize<WelcomeMessage>(json);
        //                break;
        //            #endregion
                        
        //            #region Social
        //            case "social":
        //                var socialMessage = JsonSerializer.Deserialize<SocialMessage>(json);
        //                break;
        //            #endregion

        //            #region PlayerInfo
        //            case "player_info":
        //                var playerInfoMessage = JsonSerializer.Deserialize<PlayerInfoMessage>(json);
        //                if (playerInfoMessage.players.Length > 0)
        //                {
        //                    // payload with players
        //                }
        //                break;
        //            #endregion

        //            #region GameInfo
        //            case "game_info":
        //                var gameInfoMessage = JsonSerializer.Deserialize<GameInfoMessage>(json);
        //                if (gameInfoMessage.games.Length > 0)
        //                {
        //                    // payload with lobbies
        //                }

        //                break;
        //            #endregion

        //            #region MatchmakerInfo
        //            case "matchmaker_info":
        //                var queueMessage = JsonSerializer.Deserialize<QueueMessage>(json);
        //                if (queueMessage.queues.Length > 0)
        //                {
        //                    // payload with queues
        //                }
        //                break;
        //            #endregion

        //            #region Ping
        //            case "ping":
        //                return;
        //                client.Write(new byte[]
        //                {
        //                    // command=pong\n
        //                    99, 111, 109, 109, 97, 110, 100, 61, 112, 111, 110, 103, 10
        //                });
        //                break;
        //            #endregion

        //            #region Pong
        //            case "pong":
        //                return;
        //                client.Write(new byte[]
        //                {
        //                    // command=ping\n
        //                    99, 111, 109, 109, 97, 110, 100, 61, 112, 105, 110, 103, 10,
        //                });
        //                break; 
        //            #endregion
        //        } 
        //        #endregion
        //    }
        //}

        public async Task OnViewChanged(INavigationManager navigationManager) => NavigationManager = navigationManager;

        private void OnUserSearchInputTextChange(AutoSuggestBox sender, AutoSuggestBoxTextChangedEventArgs args)
        {
            if (args.Reason == AutoSuggestionBoxTextChangeReason.UserInput)
            {
                sender.ItemsSource = new int[] { 1, 2, 3, 4, 5, 6, 7, 8, 9, 10 };
            }
        }

        Button lastBtn;
        private void ItemsControl_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            var btn = (Button)e.Source;
            var brush = new SolidColorBrush();
            brush.Color = Color.FromRgb(69,69,69);
            btn.Background = brush;

            brush.Color = Colors.Transparent;
            if (lastBtn != null) lastBtn.Background = brush;
            lastBtn=btn;
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            var btn = (Button)sender;
            
            Grid content = (Grid)btn.Content;
            var title = (TextBlock)content.Children[1];
            if(title.Text=="Theme")return;

            btn.Background = App.Current.Resources["ButtonBackgroundDisabled"] as SolidColorBrush;
            if(lastBtn != null ? lastBtn.Equals(btn):false)return;

            DoubleAnimation animation = new DoubleAnimation();
            animation.From = title.ActualHeight;
            animation.To = 0;
            animation.Duration = TimeSpan.FromSeconds(.1);
            title.BeginAnimation(HeightProperty, animation);
            btn.ToolTip = title.Text;

            var path = (Path)content.Children[0];
            path.Fill = App.Current.Resources["SystemControlBackgroundBaseMediumHighBrush"] as SolidColorBrush;
            path.Stroke = null;

            if(!lastBtn?.Equals(btn) ?? false)
            {
                content = (Grid)lastBtn.Content;
                var titlex = (TextBlock)content.Children[1];

                animation.From = 0;
                animation.To = title.ActualHeight;
                animation.Duration = TimeSpan.FromSeconds(.1);
                titlex.BeginAnimation(HeightProperty, animation);
                lastBtn.Background = null;
                lastBtn.ToolTip = null;
                path = (Path)content.Children[0];
                path.Fill=null;
                path.Fill = new SolidColorBrush(){Color=Color.FromRgb(154, 154, 154)};
            }
            else
            {
            }
            lastBtn = btn;
        }

        private void Button_Click_1(object sender, RoutedEventArgs e)
        {
            ClearValue(ThemeManager.RequestedThemeProperty);

            Application.Current.Dispatcher.BeginInvoke(() =>
            {
                var tm = ThemeManager.Current;
                if (tm.ActualApplicationTheme == ApplicationTheme.Dark)
                {
                    tm.ApplicationTheme = ApplicationTheme.Light;
                }
                else
                {
                    tm.ApplicationTheme = ApplicationTheme.Dark;
                }
            });
        }
        private void MenuItem_OnClick(object sender, RoutedEventArgs e)
        {
            //var t = ((ListBoxItem)Test.SelectedItem).Content.ToString();
            //Clipboard.SetText(t);
        }
        private void NavigateTo(Type viewtype)
        {
            if (Views.TryGetValue(viewtype, out var view))
            {
                MainFrame.Content = view;
            }
            else
            {
                var instance = (UserControl)Activator.CreateInstance(viewtype);
                Views.Add(viewtype, view);
                MainFrame.Content = view;
            }
        }
        private void SwitchView(object sender, RoutedEventArgs e)
        {
            var btn = (Button)sender;
            switch (btn.Name)
            {
                case "Lobbies":
                    NavigateTo(typeof(LobbiesView));
                    break;
                case "Chat":
                    NavigateTo(typeof(ChatView));
                    break;
            }
        }
    }
}
