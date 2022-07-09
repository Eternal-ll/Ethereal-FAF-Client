using beta.Infrastructure.Services.Interfaces;
using beta.Models.Enums;
using beta.ViewModels.Base;

namespace beta.ViewModels
{
    /// <summary>
    /// 
    /// </summary>
    public class IrcViewModel : ViewModel
    {
        private readonly IIrcService IRCService;

        public IrcViewModel(IIrcService iRCService)
        {
            IRCService = iRCService;
            IRCService.StateChanged += IRCService_StateChanged;
        }

        #region Status
        private IrcState _Status;
        public IrcState Status
        {
            get => _Status;
            set => Set(ref _Status, value);
        }
        #endregion

        private void IRCService_StateChanged(object sender, IrcState e)
        {
            Status = e;
        }

        protected override void Dispose(bool disposing)
        {
            IRCService.StateChanged -= IRCService_StateChanged;
        }

        #region Commands

        #endregion
    }
}
