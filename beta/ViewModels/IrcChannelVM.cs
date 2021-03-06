using beta.Models;
using beta.Models.IRC;
using beta.Models.IRC.Base;
using beta.ViewModels.Base;
using System.Collections.Generic;
using System.Collections.ObjectModel;

namespace beta.ViewModels
{
    public class IrcChannelVM : ViewModel
    {
        public string Name { get; }

        #region Topic
        private string _Topic;
        public string Topic
        {
            get => _Topic;
            set => Set(ref _Topic, value);
        }
        #endregion

        #region TopicChangeBy
        private IrcChannelTopicChangedBy _IrcChannelTopicChangedBy;
        public IrcChannelTopicChangedBy TopicChangedBy
        {
            get => _IrcChannelTopicChangedBy;
            set=> Set(ref _IrcChannelTopicChangedBy, value);
        }

        #endregion

        public bool IsSelected { get; set; }

        public List<string> Users { get; set; }
        public ObservableCollection<IrcMessage> History { get; } = new();
        public ObservableCollection<IPlayer> Players { get; set; } = new();

        public IrcChannelVM(string name) => Name = name;

        public bool AddUser(string login)
        {
            if (Users.Contains(login)) return false;
            Users.Add(login);
            return true;
        }
        public bool UpdateUser(string from, string to)
        {
            var index = Users.IndexOf(from);
            if (index == -1) return false;
            Users[index] = to;
            return false;
        }
        public bool UpdatePlayer(IPlayer player, string from)
        {
            var players = Players;
            for (int i = 0; i < players.Count; i++)
            {
                if (players[i].login == from)
                {
                    players[i] = player;
                    return true;
                }
            }
            return false;
        }
        public bool RemoveUser(string login)
        {
            Users.Remove(login);

            if (login.StartsWith('@'))
            {
                login = login[1..];
            }

            var players = Players;
            for (int i = 0; i < players.Count; i++)
            {
                if (players[i].login == login)
                {
                    players.RemoveAt(i);
                    return true;
                }
            }

            return false;
        }

        public IrcMessage AddMessage(IrcMessage msg)
        {
            if (msg is IrcChannelMessage userMsg)
            {
                userMsg.IsSame = History.Count >= 1 &&
                    History[^1] is IrcChannelMessage lastMsg &&
                    lastMsg.From == userMsg.From;
            }
            History.Add(msg);
            return msg;
        }
    }
}
