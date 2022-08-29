namespace Ethereal.FAF.UI.Client.Infrastructure.Ice
{
    public class GpgNetMessage
    {
        public GpgNetMessage(string command, string args)
        {
            Command = command;
            Args = args;
        }

        public string Command { get; }
        public string Args { get; }

    }
}
