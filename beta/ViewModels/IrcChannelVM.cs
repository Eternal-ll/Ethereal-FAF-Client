using beta.Models.IRC;
using beta.Models.IRC.Base;
using beta.ViewModels.Base;
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

        public ObservableCollection<string> Users { get; } = new();
        public ObservableCollection<IrcMessage> History { get; } = new();
        public IrcChannelVM(string name) => Name = name;
    }
}
