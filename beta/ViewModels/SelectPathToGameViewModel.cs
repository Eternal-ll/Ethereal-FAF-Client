﻿using beta.Infrastructure.Commands;
using beta.Infrastructure.Services.Interfaces;
using beta.Properties;
using Microsoft.Extensions.DependencyInjection;
using ModernWpf.Controls;
using System.IO;
using System.Windows.Input;

namespace beta.ViewModels
{
    internal class SelectPathToGameViewModel : Base.ViewModel
    {
        private readonly ContentDialog ContentDialog;
        public SelectPathToGameViewModel()
        {
            ContentDialog = App.Services.GetService<INotificationService>().ContentDialog;

            var settings = Settings.Default.PathToGame;

            if (string.IsNullOrWhiteSpace(settings))
            {
                var steam = @"C:\Program Files (x86)\Steam\SteamApps\Supreme Commander Forged Alliance";
                ConfirmPath(steam);
                if (IsConfirmed)
                {
                    Path = steam;
                }
            }
            else
            {
                Path = settings;
            }
        }

        private void ConfirmPath(string path)
        {
            if (Directory.Exists(path))
            {
                if (!path.EndsWith('\\')) path += '\\';
                if (File.Exists(path + "bin\\SupremeCommander.exe"))
                {
                    IsConfirmed = true;
                    return;
                }
            }
            IsConfirmed = false;
        }

        #region Path
        private string _Path;
        public string Path
        {
            get => _Path;
            set
            {
                if (Set(ref _Path, value))
                {
                    ConfirmPath(value);
                }
            }
        }
        #endregion

        #region IsConfirmed
        private bool _IsConfirmed;
        public bool IsConfirmed
        {   
            get => _IsConfirmed;
            set
            {
                if (Set(ref _IsConfirmed, value))
                {
                    ContentDialog.IsPrimaryButtonEnabled = value;
                }
            }
        }
        #endregion

        #region ConfirmCommand
        private ICommand _ConfirmCommand;
        public ICommand ConfirmCommand => _ConfirmCommand ??= new LambdaCommand(OnConfirmCommand, CanConfirmCommand);
        private bool CanConfirmCommand(object parameter) => IsConfirmed;
        private void OnConfirmCommand(object parameter) {}
        #endregion
    }
}
