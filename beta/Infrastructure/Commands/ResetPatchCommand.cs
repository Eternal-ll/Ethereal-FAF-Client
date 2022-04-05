using beta.Infrastructure.Services.Interfaces;
using Microsoft.Extensions.DependencyInjection;
using System.Threading.Tasks;

namespace beta.Infrastructure.Commands
{
    internal class ResetPatchCommand : Base.Command
    {
        private readonly IGameSessionService GamesSessionService;
        public ResetPatchCommand() => GamesSessionService = App.Services.GetService<IGameSessionService>();

        private Task Task;

        public override bool CanExecute(object parameter)
        {
            if (Task is null) return true;
            return Task.IsCompleted;
        }
        public override void Execute(object parameter) => Task = Task.Run(() => GamesSessionService.ResetPatch());
    }
}
