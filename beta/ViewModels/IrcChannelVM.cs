using beta.Models;
using beta.ViewModels.Base;
using System.Collections.ObjectModel;

namespace beta.ViewModels
{
    public class IrcChannelVM : ViewModel
    {
        #region Name
        private string _Name;
        public string Name
        {
            get => _Name;
            set
            {
                if (Set(ref _Name, value))
                    OnPropertyChanged(nameof(FormattedName));
            }
        }
        public string FormattedName => _Name.Substring(1);
        #endregion

        #region Topic
        private string _Topic;
        public string Topic
        {
            get => _Topic;
            set => Set(ref _Topic, value);
        }
        #endregion

        #region History
        public ObservableCollection<ChannelMessage> History { get; } = new();
        #endregion

        #region Users
        public ObservableCollection<string> Users { get; } = new();
        #endregion
    }
}
