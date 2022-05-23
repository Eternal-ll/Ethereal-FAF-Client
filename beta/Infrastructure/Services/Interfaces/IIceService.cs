using System;

namespace beta.Infrastructure.Services.Interfaces
{
    /// <summary>
    /// Ice proxy service
    /// </summary>
    public interface IIceService
    {
        /// <summary>
        /// Ice servers for local faf ice adapter
        /// </summary>
        public event EventHandler<string> IceServersReceived;
        public string IceServers { get; }
    }
}
