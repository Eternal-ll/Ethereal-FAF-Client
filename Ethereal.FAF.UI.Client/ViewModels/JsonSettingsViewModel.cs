using Ethereal.FAF.UI.Client.Models;
using System.Runtime.CompilerServices;

namespace Ethereal.FAF.UI.Client.ViewModels
{
    public abstract class JsonSettingsViewModel : Base.ViewModel
    {
        protected bool Set<T>(ref T field, T value, string path, bool asString = true, [CallerMemberName] string PropertyName = null)
        {
            if (Set(ref field, value, PropertyName: PropertyName))
            {
                UserSettings.Update(path, asString ? value.ToString() : value);
                return true;
            }
            return false;
        }
    }
}
