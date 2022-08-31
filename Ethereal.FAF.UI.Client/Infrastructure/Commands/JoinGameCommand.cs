using AsyncAwaitBestPractices;
using System;
using System.Threading.Tasks;

namespace Ethereal.FAF.UI.Client.Infrastructure.Commands
{
    //public abstract class AsyncCommand : Base.Command
    //{
    //    Action<Exception> OnException;
    //    bool ContinueOnCapturedContex;

    //    protected AsyncCommand(Action<Exception> onException = null, bool continueOnCapturedContex = false)
    //    {
    //        OnException = onException;
    //        ContinueOnCapturedContex = continueOnCapturedContex;
    //    }

    //    public override void Execute(object parameter)
    //    {
    //        Work(parameter).SafeFireAndForget(OnException, ContinueOnCapturedContex);
    //    }
    //    public abstract Task Work(object parameter);
    //}
    //public abstract class ServiceProviderCommand : AsyncCommand
    //{
    //    public IServiceProvider ServiceProvider { get; set; }
    //}
    //public class JoinGameCommand : ServiceProviderCommand
    //{
    //    public override bool CanExecute(object parameter) => true;

    //    public override async Task Work(object parameter)
    //    {

    //    }
    //}
}
