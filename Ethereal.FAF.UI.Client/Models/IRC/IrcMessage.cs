using Ethereal.FAF.UI.Client.ViewModels;
using Ethereal.FAF.UI.Client.ViewModels.Base;
using Humanizer;
using System;
using System.CodeDom;
using System.Collections.Generic;
using System.ComponentModel;
using System.Windows.Documents;

namespace Ethereal.FAF.UI.Client.Models.IRC
{
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
        public IrcUserMessage(string text, string user) : base(text)
        {
            User = user;
        }

        public string User { get; set; }
    }
    public sealed class IrcPlayerMessage : IrcUserMessage
    {
        public IrcPlayerMessage(string text, string user, Player player) : base(text, user)
        {
            Player = player;
        }

        public Player Player { get; set; }
    }
    public abstract class IrcChannel
    {
        public string Name { get; set; }
        public abstract string Group { get; }
        public bool IsSelected { get; set; }
        protected IrcChannel(string name)
        {
            Name = name;
        }
        public List<IrcMessage> History { get; } = new();
    }
    public sealed class GroupChannel : IrcChannel, INotifyPropertyChanged
    {
        public GroupChannel(string name, string group = "Channel") : base(name)
        {
            Group = group;
        }
        public override string Group { get; }

        #region Title
        private string _Title;
        public string Title
        {
            get => _Title;
            set
            {
                _Title = value;
                PropertyChanged?.Invoke(this, new(nameof(Title)));
            }
        }
        #endregion

        #region TopicChangedBy
        private string _TopicChangedBy;
        public string TopicChangedBy
        {
            get => _TopicChangedBy;
            set
            {
                _TopicChangedBy = value;
                PropertyChanged?.Invoke(this, new(nameof(TopicChangedBy)));
            }
        }
        #endregion
        #region TopicChangedAt
        private long _TopicChangedAt;
        public long TopicChangedAt
        {
            get => _TopicChangedAt;
            set
            {
                _TopicChangedAt = value;
                PropertyChanged?.Invoke(this, new(nameof(TopicChangedAt)));
            }
        }
        #endregion



        public List<string> Users { get; } = new();

        public event PropertyChangedEventHandler PropertyChanged;

        public void AddUser(string user) => Users.Add(user);
        public void RemoveUser(string user)
        {
            for (int i = 0; i < Users.Count; i++)
            {
                if (Users[i] == user)
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
                if (Users[i] == from)
                {
                    Users[i] = to;
                    break;
                }
            }
        }
    }
    public sealed class DialogueChannel : IrcChannel
    {
        public DialogueChannel(string name, Player receiver) : base(name)
            => Receiver = receiver;
        public Player Receiver { get; set; }

        public override string Group { get; } = "Direct";
    }
}
