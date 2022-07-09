using System.ComponentModel;
using System.Configuration;
using System.Runtime.CompilerServices;

namespace beta.Infrastructure.Services.Interfaces
{
    /// <summary>
    /// 
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public class SettingsService<T> : ISettingsService where T : ApplicationSettingsBase
    {
        protected readonly T Settings;
        public SettingsService(T settings) => Settings = settings;

        private bool _IsAutoSaveEnabled;

        /// <summary>
        /// 
        /// </summary>
        public bool IsAutoSaveEnabled
        {
            get => _IsAutoSaveEnabled;
            set
            {
                if (!_IsAutoSaveEnabled.Equals(value))
                {
                    _IsAutoSaveEnabled = value;
                    if (value)
                    {
                        Settings.PropertyChanged += Settings_PropertyChanged;
                    }
                    else
                    {
                        Settings.PropertyChanged -= Settings_PropertyChanged;
                    }
                }
            }
        }

        private void Settings_PropertyChanged(object sender, PropertyChangedEventArgs e) => Settings.Save();

        /// <summary>
        /// 
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="value"></param>
        /// <param name="propertyName"></param>
        /// <returns></returns>
        public bool Set<Type>(Type value, [CallerMemberName] string propertyName = null)
        {
            if (Settings[propertyName].Equals(value)) return false;
            Settings[propertyName] = value;
            return true;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="propertyName"></param>
        /// <returns></returns>
        public Type Get<Type>([CallerMemberName] string propertyName = null) =>
            (Type)Properties.MapsVMSettings.Default[propertyName];
        /// <summary>
        /// 
        /// </summary>
        public void Save() => Settings.Save();
    }

    /// <summary>
    /// 
    /// </summary>
    public interface ISettingsService
    {
        public bool IsAutoSaveEnabled { get; set; }
        /// <summary>
        /// 
        /// </summary>
        /// <typeparam name="Type"></typeparam>
        /// <param name="value"></param>
        /// <param name="propertyName"></param>
        /// <returns></returns>
        public bool Set<Type>(Type value, [CallerMemberName] string propertyName = null);
        /// <summary>
        /// 
        /// </summary>
        /// <typeparam name="Type"></typeparam>
        /// <param name="propertyName"></param>
        /// <returns></returns>
        public Type Get<Type>([CallerMemberName] string propertyName = null);
        /// <summary>
        /// 
        /// </summary>
        public void Save();
    }
}
