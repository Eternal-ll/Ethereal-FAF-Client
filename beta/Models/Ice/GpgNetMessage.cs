namespace beta.Models.Ice
{
    internal class GpgNetMessage
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
