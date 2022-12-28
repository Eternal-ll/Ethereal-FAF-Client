namespace Ethereal.FAF.UI.Client.Infrastructure.Commands
{
    /// <summary>
    /// https://fafscore.nl/{login}
    /// </summary>
    internal class FafScoreCommand : NagivateUriCommand
    {
        public override void Execute(object parameter) => 
            base.Execute($"https://fafscore.nl/{parameter}");
    }
}