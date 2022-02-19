using System.ComponentModel;

namespace beta.Infrastructure.Services.Interfaces
{
    public interface IIrcService
    {
        public ICollectionView ChannelUsersView { get; }
    }
}