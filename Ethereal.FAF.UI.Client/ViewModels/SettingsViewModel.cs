using Ethereal.FAF.UI.Client.Infrastructure.Commands;
using Ethereal.FAF.UI.Client.Infrastructure.Ice;
using Ethereal.FAF.UI.Client.Models;
using Microsoft.Extensions.Configuration;
using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Effects;
using Wpf.Ui.Appearance;
using Wpf.Ui.Controls;
using Wpf.Ui.Mvvm.Contracts;

namespace Ethereal.FAF.UI.Client.ViewModels
{
    public abstract class JsonSettingsViewModel : Base.ViewModel
    {
        protected bool Set<T>(ref T field, T value, string path, bool asString = true, [CallerMemberName] string PropertyName = null)
        {
            if (Set(ref field, value, PropertyName: PropertyName))
            {
                AppSettings.Update(path, asString ? value.ToString() : value);
                return true;
            }
            return false;
        }
    }
    public sealed class BackgroundViewModel: JsonSettingsViewModel
    {
        public BackgroundViewModel(IConfiguration configuration)
        {
            var effect = configuration.GetSection("UI:BackgroundImage:BlurEffect").Get<BlurEffect>();
            _ImageBlur = effect.Radius;
            _ImageKernelType = effect.KernelType;
            _ImageRenderingBias = effect.RenderingBias;
            _ImageStretch = configuration.GetValue<Stretch>("UI:BackgroundImage:Stretch");
            _ImageOpacity = configuration.GetValue<double>("UI:BackgroundImage:Opacity");
            _ImageUrl = configuration.GetValue<string>("UI:BackgroundImage:Url");

            SelectImageCommand = new LambdaCommand(arg =>
            {
                var dialog = new OpenFileDialog
                {
                    Filter = "Image Files|*.jpg;*.jpeg;*.png;*.gif;*.tif;..."
                };
                dialog.ShowDialog();
                if (!string.IsNullOrWhiteSpace(dialog.FileName))
                {
                    ImageUrl = dialog.FileName;
                }
            });
        }

        public ICommand SelectImageCommand { get; }

        #region ImageUrl
        private string _ImageUrl;
        public string ImageUrl { get => _ImageUrl; set => Set(ref _ImageUrl, value, "UI:BackgroundImage:Url"); }
        #endregion

        #region ImageOpacity
        private double _ImageOpacity;
        public double ImageOpacity { get => _ImageOpacity; set => Set(ref _ImageOpacity, value, "UI:BackgroundImage:Opacity"); }
        #endregion

        #region ImageBlur
        private double _ImageBlur;
        public double ImageBlur { get => _ImageBlur; set => Set(ref _ImageBlur, value, "UI:BackgroundImage:BlurEffect:Radius"); }
        #endregion

        #region ImageRenderingBias
        public RenderingBias[] RenderingBiasSource { get; } = Enum.GetValues<RenderingBias>();
        private RenderingBias _ImageRenderingBias;
        public RenderingBias ImageRenderingBias
        {
            get => _ImageRenderingBias;
            set => Set(ref _ImageRenderingBias, value, "UI:BackgroundImage:BlurEffect:RenderingBias");
        }

        #endregion

        #region ImageKernelType
        public KernelType[] KernelTypeSource { get; } = Enum.GetValues<KernelType>();
        private KernelType _ImageKernelType;
        public KernelType ImageKernelType
        {
            get => _ImageKernelType;
            set => Set(ref _ImageKernelType, value, "UI:BackgroundImage:BlurEffect:KernelType");
        }
        #endregion

        #region ImageStretch
        public Stretch[] StretchSource { get; } = Enum.GetValues<Stretch>();
        private Stretch _ImageStretch;
        public Stretch ImageStretch { get => _ImageStretch; set => Set(ref _ImageStretch, value, "UI:BackgroundImage:Stretch"); }
        #endregion
    }
    public class SettingsViewModel : JsonSettingsViewModel
    {
        private readonly IThemeService ThemeService;
        private readonly INavigationWindow NavigationWindow;

        public BackgroundViewModel BackgroundViewModel { get; }

        public SettingsViewModel(IThemeService themeService,
            INavigationWindow navigationWindow,
            BackgroundViewModel backgroundViewModel,
            IConfiguration configuration,
            IceManager iceManager)
        {
            ThemeService = themeService;
            NavigationWindow = navigationWindow;
            BackgroundViewModel = backgroundViewModel;

            _IsIceRelayForced = configuration.GetValue<bool>("IceAdapter:IsRelayForced");
            _IsIceDebugEnabled = configuration.GetValue<bool>("IceAdapter:IsDebugEnabled");
            _IsIceInfoEnabled = configuration.GetValue<bool>("IceAdapter:IsIceInfoEnabled");
            //_IsIceLogsEnabled = configuration.GetValue<bool>("IceAdapter:IsLogsEnabled");
            _PathToIceAdapter = configuration.GetValue<string>("IceAdapter:Executable");
            _PathToIceAdapterLogs = configuration.GetValue<string>("IceAdapter:Logs");

            _MapGenLogsFolder = configuration.GetValue<string>("MapGenerator:Logs");
            _MapGenVersionsFolder = configuration.GetValue<string>("MapGenerator:Versions");
            _IsMapGenLogsEnabled = configuration.GetValue<bool>("MapGenerator:IsLogsEnabled");
            _MapGenLogsFolder = configuration.GetValue<string>("MapGenerator:Logs");

            _PathToUidGenerator = configuration.GetValue<string>("Paths:UidGenerator");
            _PathToJavaRuntime = configuration.GetValue<string>("Paths:JavaRuntime");


            _BackgroundType = configuration.GetValue<BackgroundType>("UI:BackgroundType");
            _AccentColor = (Color)ColorConverter.ConvertFromString(configuration.GetValue<string>("UI:AccentColor"));
            _ThemeType = configuration.GetValue<ThemeType>("UI:ThemeType");

            Task.Run(() =>
            {
                IceHelper = iceManager.GetIceHelpMessage();

            });

            SetSystemAccentColorCommand = new LambdaCommand(arg => AccentColor = Accent.GetColorizationColor());
            SelectDirectoryCommand = new LambdaCommand(arg =>
            {
                var target = arg.ToString();
                var dialog = new OpenFileDialog
                {
                    Title = "Select any file, parent directory will be chosen",
                    InitialDirectory = target switch
                    {
                        "ice.logs" => PathToIceAdapterLogs,
                        "mapgen.logs" => MapGenLogsFolder,
                        "mapgen.versions" => MapGenVersionsFolder,
                    }
                };
                dialog.ShowDialog();
                if (!string.IsNullOrWhiteSpace(dialog.FileName))
                {
                    var path = Directory.GetDirectoryRoot(dialog.FileName);
                    switch (target)
                    {
                        case "ice.logs": PathToIceAdapterLogs = path; break;
                        case "mapgen.logs": MapGenLogsFolder = path; break;
                        case "mapgen.versions": MapGenVersionsFolder = path; break;
                    }
                }
            });
            SelectFileCommand = new LambdaCommand(arg =>
            {
                var target = arg.ToString();
                var file = target switch
                {
                    "java.runtime" => "Java Runtime",
                    "ice.file" => "FAF ICE Adapter",
                    "uid.file" => "FAF UID Generator"
                };
                var dialog = new OpenFileDialog
                {
                    Title = $"Select {file} file",
                    Filter = target switch
                    {
                        "java.runtime" => "Java Runtime|java.exe",
                        "ice.file" => "FAF ICE Adapter|*.jar",
                        "uid.file" => "FAF UID Generator|*.exe"
                    },
                    InitialDirectory = target switch
                    {
                        "java.runtime" => PathToJavaRuntime,
                        "ice.file" => PathToIceAdapter,
                        "uid.file" => PathToUidGenerator
                    }
                };
                dialog.ShowDialog();
                if (!string.IsNullOrWhiteSpace(dialog.FileName))
                {
                    switch (target)
                    {
                        case "java.runtime": PathToJavaRuntime = dialog.FileName; break;
                        case "ice.file": PathToIceAdapter = dialog.FileName; break;
                        case "uid.file": PathToUidGenerator = dialog.FileName; break;
                    }
                }
            });
        }
        public ICommand SetSystemAccentColorCommand { get; }
        public ICommand SelectDirectoryCommand { get; }
        public ICommand SelectFileCommand { get; }


        public static IEnumerable<Brush> AssentColors { get; } = typeof(Brushes)
            .GetProperties(BindingFlags.Static | BindingFlags.Public)
            .Select(p => p.GetValue(null))
            .OfType<SolidColorBrush>()
            .OrderBy(b => b.Color.R)
            .ThenBy(b => b.Color.G)
            .ThenBy(b => b.Color.B)
            .Where(b => b.Color.ToString() != Colors.Transparent.ToString())
            .Cast<Brush>();

        #region ThemeType
        public static ThemeType[] ThemeTypeSource { get; } = Enum.GetValues<ThemeType>(); //new ThemeType[] { ThemeType.Light, ThemeType.Dark }; 
        private ThemeType _ThemeType;
        public ThemeType ThemeType
        {
            get => _ThemeType;
            set
            {
                if (Set(ref _ThemeType, value))
                {
                    AppSettings.Update("UI:ThemeType", value.ToString());
                    ThemeService.SetTheme(value);
                }
            }
        }
        #endregion

        #region BackgroundType
        public static BackgroundType[] BackgroundTypeSource { get; } = Enum.GetValues<BackgroundType>();
        private BackgroundType _BackgroundType;
        public BackgroundType BackgroundType
        {
            get => _BackgroundType;
            set
            {
                if (Set(ref _BackgroundType, value))
                {
                    AppSettings.Update("UI:BackgroundType", value.ToString());
                    if (NavigationWindow is UiWindow window)
                    {
                        window.WindowBackdropType = value;
                    }
                }
            }
        }
        #endregion

        #region AccentColor
        private Color _AccentColor;
        public Color AccentColor
        {
            get => _AccentColor;
            set
            {
                if (Set(ref _AccentColor, value))
                {
                    AppSettings.Update("UI:AccentColor", value.ToString());
                    ThemeService.SetAccent(value);
                }
            }
        }
        #endregion

        #region FAF ICE Adapter

        #region IceHelper
        private string _IceHelper;
        public string IceHelper
        {
            get => _IceHelper;
            set => Set(ref _IceHelper, value);
        }
        #endregion

        #region IsIceRelayForced
        private bool _IsIceRelayForced;
        public bool IsIceRelayForced
        {
            get => _IsIceRelayForced;
            set => Set(ref _IsIceRelayForced, value);
        }
        #endregion
        #region IsIceDebugEnabled
        private bool _IsIceDebugEnabled;
        public bool IsIceDebugEnabled
        {
            get => _IsIceDebugEnabled;
            set => Set(ref _IsIceDebugEnabled, value);
        }
        #endregion
        #region IsIceInfoEnabled
        private bool _IsIceInfoEnabled;
        public bool IsIceInfoEnabled
        {
            get => _IsIceInfoEnabled;
            set => Set(ref _IsIceInfoEnabled, value);
        }
        #endregion
        #region IsIceLogsEnabled
        // Not supported
        //private bool _IsIceLogsEnabled;
        //public bool IsIceLogsEnabled
        //{
        //    get => _IsIceLogsEnabled;
        //    set => Set(ref _IsIceLogsEnabled, value);
        //}
        #endregion
        #region PathToIceAdapterLogs
        private string _PathToIceAdapterLogs;
        public string PathToIceAdapterLogs
        {
            get => _PathToIceAdapterLogs;
            set => Set(ref _PathToIceAdapterLogs, value);
        }
        #endregion

        #region PathToIceAdapter
        private string _PathToIceAdapter;
        public string PathToIceAdapter
        {
            get => _PathToIceAdapter;
            set => Set(ref _PathToIceAdapter, value);
        }
        #endregion

        #endregion

        #region Neroxis Map Generator

        #region IsMapGenLogsEnabled
        private bool _IsMapGenLogsEnabled;
        public bool IsMapGenLogsEnabled
        {
            get => _IsMapGenLogsEnabled;
            set => Set(ref _IsMapGenLogsEnabled, value);
        }
        #endregion

        #region MapGenLogsFolder
        private string _MapGenLogsFolder;
        public string MapGenLogsFolder
        {
            get => _MapGenLogsFolder;
            set => Set(ref _MapGenLogsFolder, value);
        }
        #endregion

        #region MapGenVersionsFolder
        private string _MapGenVersionsFolder;
        public string MapGenVersionsFolder
        {
            get => _MapGenVersionsFolder;
            set => Set(ref _MapGenVersionsFolder, value);
        }
        #endregion

        #endregion

        #region PathToUidGenerator
        private string _PathToUidGenerator;
        public string PathToUidGenerator
        {
            get => _PathToUidGenerator;
            set => Set(ref _PathToUidGenerator, value, "Paths:UidGenerator");
        }
        #endregion

        #region PathToJavaRuntime
        private string _PathToJavaRuntime;
        public string PathToJavaRuntime
        {
            get => _PathToJavaRuntime;
            set => Set(ref _PathToJavaRuntime, value);
        }
        #endregion
    }
}
