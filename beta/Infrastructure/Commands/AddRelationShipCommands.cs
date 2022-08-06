using beta.Infrastructure.Commands.Base;
using beta.Infrastructure.Services.Interfaces;
using Microsoft.Extensions.DependencyInjection;

namespace beta.Infrastructure.Commands
{
    internal class AddFriendCommand : Command
    {
        private ISocialService SocialService;
        public override bool CanExecute(object parameter) => true;
        public override void Execute(object parameter)
        {
            var socialService = SocialService ??= ServiceProvider.GetService<ISocialService>();
            if (int.TryParse(parameter.ToString(), out int id))
                socialService.AddFriend(id);
        }
    }
    internal class RemoveFriendCommand : Command
    {
        private ISocialService SocialService;
        public override bool CanExecute(object parameter) => true;
        public override void Execute(object parameter)
        {
            var socialService = SocialService ??= ServiceProvider.GetService<ISocialService>();
            if (int.TryParse(parameter.ToString(), out int id))
                socialService.RemoveFriend(id);
        }
    }

    internal class AddFoeCommand : Command
    {
        private ISocialService SocialService;
        public override bool CanExecute(object parameter) => true;
        public override void Execute(object parameter)
        {
            var socialService = SocialService ??= ServiceProvider.GetService<ISocialService>();
            if (int.TryParse(parameter.ToString(), out int id))
                socialService.AddFoe(id);
        }
    }
    internal class RemoveFoeCommand : Command
    {
        private ISocialService SocialService;
        public override bool CanExecute(object parameter) => true;
        public override void Execute(object parameter)
        {
            var socialService = SocialService ??= ServiceProvider.GetService<ISocialService>();
            if (int.TryParse(parameter.ToString(), out int id))
                socialService.RemoveFoe(id);
        }
    }
}
