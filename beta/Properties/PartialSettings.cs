namespace beta.Properties
{
    internal sealed partial class Settings : System.Configuration.ApplicationSettingsBase
    {
        [global::System.Configuration.UserScopedSettingAttribute()]
        [global::System.Diagnostics.DebuggerNonUserCodeAttribute()]
        [global::System.Configuration.DefaultSettingValueAttribute("1024, 768")]
        public global::System.Windows.Size WindowSize
        {
            get
            {
                return ((global::System.Windows.Size)(this["WindowSize"]));
            }
            set
            {
                this["WindowSize"] = value;
            }
        }

        [global::System.Configuration.UserScopedSettingAttribute()]
        [global::System.Diagnostics.DebuggerNonUserCodeAttribute()]
        [global::System.Configuration.DefaultSettingValueAttribute("0, 0")]
        public global::System.Windows.Point WindowLocation
        {
            get
            {
                return ((global::System.Windows.Point)(this["WindowLocation"]));
            }
            set
            {
                this["WindowLocation"] = value;
            }
        }
        [global::System.Configuration.UserScopedSettingAttribute()]
        [global::System.Diagnostics.DebuggerNonUserCodeAttribute()]
        [global::System.Configuration.DefaultSettingValueAttribute("0")]
        public global::ModernWpf.Controls.NavigationViewPaneDisplayMode NavigationViewPaneDisplayMode
        {
            get
            {
                return ((global::ModernWpf.Controls.NavigationViewPaneDisplayMode)(this["NavigationViewPaneDisplayMode"]));
            }
            set
            {
                this["NavigationViewPaneDisplayMode"] = value;
            }
        }
    }
}
