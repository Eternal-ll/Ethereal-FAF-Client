using Ethereal.FAF.UI.Client.Infrastructure.Services;
using Ethereal.FAF.UI.Client.ViewModels;
using Ethereal.FAF.UI.Client.ViewModels.Base;
using System.Collections.Generic;
using System.Linq;

namespace Ethereal.FAF.UI.Client.Models.IRC
{
    public interface IServerIrcChannel
    {
        public string Name { get; }
        public ServerManager ServerManager { get; }
    }
    public abstract class IrcMessage
    {
        public IrcMessage(string text)
        {
            Text = text;
            Created = System.DateTime.Now;
        }
        public System.DateTime Created { get; set; }
        public string Text { get; }
    }
    public class IrcUserMessage : IrcMessage
    {
        public IrcUserMessage(string text, IrcUser user) : base(text)
        {
            User = user;
        }

        public IrcUser User { get; set; }
    }
    public class IrcUser : ViewModel
    {
        public IrcUser(string name) => Name = name;
        public IrcUser(string name, Player player) : this(name) => SetPlayer(player);
        #region Name
        private string _Name;
        public string Name
        {
            get => _Name;
            set
            {
                if (Set(ref _Name, value))
                {
                    if (_Player is not null) _Player.IrcUsername = value;
                    OnPropertyChanged(nameof(DisplayedName));
                }
            }
        }
        #endregion

        public string DisplayedName => Player is null ?
            Name :
            string.IsNullOrWhiteSpace(Player.Clan) ?
                Name :
                $"[{Player.Clan}] {Name}";

        #region Player
        private Player _Player;
        public Player Player { get => _Player; set => Set(ref _Player, value); }
        #endregion

        public void SetPlayer(Player player)
        {
            Player = player;
            player.IrcUsername = Name;
        }
    }
    public abstract class IrcChannel : ViewModel
    {
        private string _Name;
        public string Name { get => _Name; set => Set(ref _Name, value); }
        public abstract string Group { get; }
        public bool IsSelected { get; set; }
        public ServerManager ServerManager {get; }
        protected IrcChannel(string name, ServerManager serverManager)
        {
            Name = name;
            ServerManager = serverManager;
        }
        public List<IrcMessage> History { get; } = new();
    }
    public sealed class GroupChannel : IrcChannel
    {
        public GroupChannel(string name, ServerManager serverManager, string group = "Channel") : base(name, serverManager)
        {
            Group = group;
        }
        public override string Group { get; }

        #region Title
        private string _Title;
        public string Title
        {
            get => _Title;
            set => Set(ref _Title, value);
        }
        #endregion

        #region TopicChangedBy
        private string _TopicChangedBy;
        public string TopicChangedBy
        {
            get => _TopicChangedBy;
            set => Set(ref _TopicChangedBy, value);
        }
        #endregion

        #region TopicChangedAt
        private long _TopicChangedAt;
        public long TopicChangedAt
        {
            get => _TopicChangedAt;
            set => Set(ref _TopicChangedAt, value);
        }
        #endregion

        public List<IrcUser> Users { get; } = new();

        public void AddUser(IrcUser user) => Users.Add(user);
        public void RemoveUser(string user)
        {
            for (int i = 0; i < Users.Count; i++)
            {
                if (Users[i].Name == user)
                {
                    Users.RemoveAt(i);
                    break;
                }
            }
        }
        public void ReplaceUser(string from, string to)
        {
            for (int i = 0; i < Users.Count; i++)
            {
                if (Users[i].Name == from)
                {
                    Users[i].Name = to;
                    break;
                }
            }
        }
        public IrcUser GetUser(string user) => Users.FirstOrDefault(u => u.Name == user);
        public bool TryGetUser(string user, out IrcUser irc)
        {
            irc = GetUser(user);
            return irc is not null;
        }
    }
    public sealed class DialogueChannel : IrcChannel
    {
        public DialogueChannel(string name, ServerManager serverManager) : base(name, serverManager) => Receiver = new(name);
        public DialogueChannel(string name, Player player, ServerManager serverManager) : this(name, serverManager) => Receiver.SetPlayer(player);
        public IrcUser Receiver { get; set; }

        public override string Group { get; } = "Direct";
    }
}
