using beta.Models.Server;
using System;

namespace beta.Infrastructure.Services.Interfaces
{
    public interface IIceService
    {
        public event EventHandler<string> IceServersReceived;
        public string IceServers { get; }
    }
}
