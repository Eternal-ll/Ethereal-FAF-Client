using Ethereal.FAF.UI.Client.Infrastructure.Updater;
using System;

namespace Ethereal.FAF.UI.Client.Infrastructure.Helper
{
    internal class EventManager
    {
        public static EventManager Instance = new();
        private EventManager() { }


        public event EventHandler<UpdateInfo> UpdateAvailable;


        public void OnUpdateAvailable(UpdateInfo args) => UpdateAvailable?.Invoke(this, args);
    }
}
