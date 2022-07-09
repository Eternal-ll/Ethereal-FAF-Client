using beta.Infrastructure.Commands.Base;
using beta.Infrastructure.Services.Interfaces;
using Microsoft.Extensions.DependencyInjection;

namespace beta.Infrastructure.Commands
{
    internal class AddFriendCommand : Command
    {
        private ISocialService SocialService;
        public AddFriendCommand()
        {
            //SocialService = App.Services.GetService<ISocialService>();
        }
        public override bool CanExecute(object parameter) => true;
        public override void Execute(object parameter)
        {
            if (int.TryParse(parameter.ToString(), out int id))
                SocialService.AddFriend(id);
        }
    }
    internal class RemoveFriendCommand : Command
    {
        private ISocialService SocialService;
        public RemoveFriendCommand()
        {
            //SocialService = App.Services.GetService<ISocialService>();
        }
        public override bool CanExecute(object parameter) => true;
        public override void Execute(object parameter)
        {
            if (int.TryParse(parameter.ToString(), out int id))
                SocialService.RemoveFriend(id);
        }
    }

    internal class AddFoeCommand : Command
    {
        private ISocialService SocialService;
        public AddFoeCommand()
        {
            //SocialService = App.Services.GetService<ISocialService>();
        }
        public override bool CanExecute(object parameter) => true;
        public override void Execute(object parameter)
        {
            if (int.TryParse(parameter.ToString(), out int id))
                SocialService.AddFoe(id);
        }
    }
    internal class RemoveFoeCommand : Command
    {
        private ISocialService SocialService;
        public RemoveFoeCommand()
        {
            //SocialService = App.Services.GetService<ISocialService>();
        }
        public override bool CanExecute(object parameter) => true;
        public override void Execute(object parameter)
        {
            if (int.TryParse(parameter.ToString(), out int id))
                SocialService.RemoveFoe(id);
        }
    }
}
