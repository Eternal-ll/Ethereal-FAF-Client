using Ethereal.FAF.UI.Client.Infrastructure.Commands;
using Ethereal.FAF.UI.Client.Infrastructure.Ice;
using Ethereal.FAF.UI.Client.Models;
using Microsoft.Extensions.Configuration;
using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.NetworkInformation;
using System.Reflection;
using System.Threading;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Effects;
using Wpf.Ui;
using Wpf.Ui.Appearance;
using Wpf.Ui.Controls;

namespace Ethereal.FAF.UI.Client.ViewModels
{
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
        private readonly BackgroundWorker _backgroundWorker;

		public SettingsViewModel(IThemeService themeService,
			INavigationWindow navigationWindow,
			BackgroundViewModel backgroundViewModel,
			IConfiguration configuration,
			IceManager iceManager)
		{
			ThemeService = themeService;
			NavigationWindow = navigationWindow;
			BackgroundViewModel = backgroundViewModel;

			_IsIceRelayForced = configuration.GetValue<bool>("IceAdapter:ForceRelay", false);
			_IsIceDebugEnabled = configuration.GetValue<bool>("IceAdapter:IsDebugEnabled");
			_IsIceInfoEnabled = configuration.GetValue<bool>("IceAdapter:IsIceInfoEnabled");
			_UseIceTelemetryUI = configuration.GetValue<bool>("IceAdapter:UseTelemetryUI", true);
			//_IsIceLogsEnabled = configuration.GetValue<bool>("IceAdapter:IsLogsEnabled");
			_PathToIceAdapter = configuration.GetValue<string>("IceAdapter:Executable");
			_PathToIceAdapterLogs = configuration.GetValue<string>("IceAdapter:Logs");

			_MapGenLogsFolder = configuration.GetValue<string>("MapGenerator:Logs");
			_MapGenVersionsFolder = configuration.GetValue<string>("MapGenerator:Versions");
			_IsMapGenLogsEnabled = configuration.GetValue<bool>("MapGenerator:IsLogsEnabled");
			_MapGenLogsFolder = configuration.GetValue<string>("MapGenerator:Logs");

			_PathToUidGenerator = configuration.GetValue<string>("Paths:UidGenerator");
			_PathToJavaRuntime = configuration.GetValue<string>("Paths:JavaRuntime");


			_BackgroundType = configuration.GetValue<WindowBackdropType>("UI:BackgroundType");
			_AccentColor = (Color)ColorConverter.ConvertFromString(configuration.GetValue<string>("UI:AccentColor"));
			_ThemeType = configuration.GetValue<ApplicationTheme>("UI:ThemeType");

			//Task.Run(async () =>
			//{
			//	IceHelper = iceManager.GetIceHelpMessage();
   //             CoturnServers = await _mediator.Send(new GetCoturnServersQuery());
			//});

			SetSystemAccentColorCommand = new LambdaCommand(arg => throw new NotImplementedException());
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
						_ => null,
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
            _backgroundWorker = new()
            {
				WorkerSupportsCancellation = true
			};
			_backgroundWorker.DoWork += _backgroundWorker_DoWork;
            _backgroundWorker.RunWorkerAsync();
		}

		private void _backgroundWorker_DoWork(object sender, DoWorkEventArgs e)
		{
            while (!_backgroundWorker.CancellationPending)
            {
                Thread.Sleep(1000);
                CalculateCoturnServersPing();
			}
		}
        private void CalculateCoturnServersPing()
		{
            if (CoturnServers == null) return;
			using var pingSender = new Ping();
            foreach (var coturnServer in CoturnServers)
			{
				var host = coturnServer.Urls?.FirstOrDefault();
				if (string.IsNullOrWhiteSpace(host))
				{
					coturnServer.RoundtripTime = 0;
					return;
				}
				var ub = new UriBuilder(host);
				IPAddress address = Dns.GetHostEntry(ub.Host).AddressList.FirstOrDefault();
				if (address == null)
				{
					coturnServer.RoundtripTime = 0;
					return;
				}
				PingReply reply = pingSender.Send(address);

				if (reply.Status == IPStatus.Success)
				{
					coturnServer.RoundtripTime = reply.RoundtripTime;
				}
				else
				{
					coturnServer.RoundtripTime = 0;
				}
			}
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
        public static ApplicationTheme[] ThemeTypeSource { get; } = Enum.GetValues<ApplicationTheme>(); //new ThemeType[] { ThemeType.Light, ThemeType.Dark }; 
        private ApplicationTheme _ThemeType;
        public ApplicationTheme ThemeType
        {
            get => _ThemeType;
            set
            {
                if (Set(ref _ThemeType, value))
                {
                    UserSettings.Update("UI:ThemeType", value.ToString());
                    ThemeService.SetTheme(value);
                }
            }
        }
        #endregion

        #region BackgroundType
        public static WindowBackdropType[] BackgroundTypeSource { get; } = Enum.GetValues<WindowBackdropType>();
        private WindowBackdropType _BackgroundType;
        public WindowBackdropType BackgroundType
        {
            get => _BackgroundType;
            set
            {
                if (Set(ref _BackgroundType, value))
                {
                    UserSettings.Update("UI:BackgroundType", value.ToString());
                    if (NavigationWindow is FluentWindow window)
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
                    UserSettings.Update("UI:AccentColor", value.ToString());
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
            set => Set(ref _IsIceRelayForced, value, "IceAdapter:ForceRelay");
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
        #region UseIceTelemetryUI
        private bool _UseIceTelemetryUI;
        public bool UseIceTelemetryUI
        {
            get => _UseIceTelemetryUI;
            set => Set(ref _UseIceTelemetryUI, value, "IceAdapter:UseTelemetryUI");
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

        #region CoturnServers
        private CoturnServer[] _CoturnServers;
        public CoturnServer[] CoturnServers
        { get => _CoturnServers; set => Set(ref _CoturnServers, value); }
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

		protected override void Dispose(bool disposing)
		{
            if (disposing)
            {
                _backgroundWorker.CancelAsync();
				_backgroundWorker.Dispose();

			}
			base.Dispose(disposing);
		}
	}
}
