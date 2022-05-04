using beta.Models.IRC;
using beta.Models.IRC.Base;
using beta.ViewModels.Base;
using System.Collections.Generic;

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

        public List<string> Users { get; } = new();
        public List<IrcMessage> History { get; } = new();

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
            return true;
        }
        public bool RemoveUser(string login) => Users.Remove(login);

        public IrcMessage AddMessage(IrcMessage msg)
        {
            if (msg is IrcChannelMessage userMsg)
            {
                userMsg.IsSame = History.Count > 1 &&
                    History[^2] is IrcChannelMessage lastMsg &&
                    lastMsg.From == userMsg.From;
            }
            History.Add(msg);
            return msg;
        }
    }
}
