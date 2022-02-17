using System;
using beta.Models;

namespace beta.Infrastructure.Services.Interfaces
{
    public interface IIRCService
    {
        public event EventHandler<EventArgs<string>> Message;
        //public SSLClient SslClient { get; }
    }
}