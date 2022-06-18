using beta.Infrastructure.Services.Interfaces;
using beta.Models.Server;
using beta.Models.Server.Base;
using System;
using System.Threading;

namespace beta.Infrastructure.Services
{
    internal class IceService : IIceService
    {
        public event EventHandler<string> IceServersReceived;
            
        private readonly ISessionService SessionService;

        /// <summary>
        /// credentials need to be valid for 10h, usual ttlfor each request is 24h
        /// </summary>
        private static int MinimumValidSeconds = 36000;

        private DateTime? IceServersValidUntil;

        private TimerCallback TimerCallback;
        private Timer IceServersValidityTimer;

        public string IceServers { get; private set; }

        public IceService(ISessionService sessionService)
        {
            SessionService = sessionService;
            SessionService.Authorized += SessionService_Authorized;
            SessionService.IceServersDataReceived += SessionService_IceServersDataReceived;

            TimerCallback = new(CheckIceServersValidity);
        }

        private bool IsSent = false;
        private void CheckIceServersValidity(object obj)
        {
            if (!SessionService.IsAuthorized) return;

            if (IceServersValidUntil.HasValue)
            {
                if ((IceServersValidUntil - DateTime.Now).Value.Seconds < MinimumValidSeconds) 
                    return;
            }

            SessionService.Send(ServerCommands.RequestIceServers);
        }

        private void SessionService_IceServersDataReceived(object sender, IceServersData e)
        {
            IsSent = false;
            if (!Equals(IceServers, e.ice_servers))
            {
                IceServersValidUntil = DateTime.Now.AddSeconds(e.ttl);
                IceServers = e.ice_servers;
                OnIceServersReceived(e.ice_servers);
            }
        }

        private void SessionService_Authorized(object sender, bool e)
        {
            if (e)
            {
                if (IceServersValidityTimer is null)
                    IceServersValidityTimer = new(TimerCallback, null, 0, 10000);
            }
        }


        private void OnIceServersReceived(string e) => IceServersReceived?.Invoke(this, e);

    }
}
