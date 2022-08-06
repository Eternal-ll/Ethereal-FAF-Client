using beta.Infrastructure.Commands;
using beta.Infrastructure.Services;
using beta.Properties;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text.Json.Serialization;
using System.Windows.Input;

namespace beta.ViewModels
{
    public class Server
    {
        public string Name { get; set; }
        public string Host { get; set; }
        public int Port { get; set; }
    }
    public class ClientEnvironment : Base.ViewModel
    {
        #region Name
        private string _Name;
        public string Name
        {
            get => _Name;
            set => Set(ref _Name, value);
        }
        #endregion
        #region Description
        private string _Description;
        public string Description
        {
            get => _Description;
            set => Set(ref _Description, value);
        }
        #endregion
        #region API
        private string _API;
        public string API
        {
            get => _API;
            set => Set(ref _API, value);
        }
        #endregion
        public ObservableCollection<Server> Servers { get; set; } = new();

        #region _IsEditModeEnabled
        private bool _IsEditModeEnabled;
        [JsonIgnore]
        public bool IsEditModeEnabled
        {
            get => _IsEditModeEnabled;
            set => Set(ref _IsEditModeEnabled, value);
        }
        #endregion

        private ICommand _AddServerCommand;
        public ICommand AddServerCommand => _AddServerCommand??= new LambdaCommand(OnAddServerCommand);
        private void OnAddServerCommand(object p) => Servers.Add(new Server());
        private ICommand _RemoveServerCommand;
        public ICommand RemoveServerCommand => _RemoveServerCommand??= new LambdaCommand(OnRemoveServerCommand, CanRemoveServerCommand);
        private bool CanRemoveServerCommand(object p) => Servers.Any();
        private void OnRemoveServerCommand(object p) => Servers.Remove((Server)p);
    }
    public class ServersViewModel : Base.ViewModel
    {
        private readonly NavigationService NavigationService;

        public ServersViewModel(NavigationService navigationService)
        {
            ClientEnvironments = new();
            Restore();
            NavigationService = navigationService;
        }
        public ObservableCollection<ClientEnvironment> ClientEnvironments { get; set; }

        #region SaveCommand
        private ICommand _SaveCommand;
        public ICommand SaveCommand => _SaveCommand ??= new LambdaCommand(OnSaveCommand);
        private void OnSaveCommand(object parameter)
        {
            Servers.Default.Environments = ClientEnvironments.ToArray();
            Servers.Default.Save();
        }
        #endregion

        #region BackCommand
        private ICommand _BackCommand;
        public ICommand BackCommand => _BackCommand ??= new LambdaCommand(OnBackCommand);
        private void OnBackCommand(object parameter) => NavigationService.GoBack();
        #endregion

        #region AddOptionCommand
        private ICommand _AddOptionCommand;
        public ICommand AddOptionCommand => _AddOptionCommand ??= new LambdaCommand(OnAddOptionCommand);
        private void OnAddOptionCommand(object p) => ClientEnvironments.Add(new ClientEnvironment()
        {
            IsEditModeEnabled = true
        });
        #endregion
        #region RemoveOptionCommand
        private ICommand _RemoveOptionCommand;
        public ICommand RemoveOptionCommand => _RemoveOptionCommand ??= new LambdaCommand(OnRemoveOptionCommand);
        private void OnRemoveOptionCommand(object p) => ClientEnvironments.Remove((ClientEnvironment)p);
        #endregion

        #region RestoreCommand
        private ICommand _RestoreCommand;
        public ICommand RestoreCommand => _RestoreCommand ??= new LambdaCommand(OnRestoreCommand);
        private void OnRestoreCommand(object p) => Restore();
        #endregion

        private void Restore()
        {
            ClientEnvironments.Clear();
            ClientEnvironments.Add(new ClientEnvironment()
            {
                Name = "Main FAForever",
                Description = "Main Forged Alliance Forever server",
                Servers = new ObservableCollection<Server>()
                    {
                        new Server()
                        {
                            Name = "Lobby",
                            Host = "lobby.faforever.com",
                            Port = 8002
                        },
                        new Server()
                        {
                            Name = "Replay",
                            Host = "lobby.faforever.com",
                            Port = 15000
                        },
                        new Server()
                        {
                            Name = "Relay",
                            Host = "lobby.faforever.com",
                            Port = 8000
                        },
                        new Server()
                        {
                            Name = "IRC",
                            Host = "irc.faforever.com",
                            Port = 6697
                        },
                    },
                API = "https://api.faforever.com/data/"
            });
            ClientEnvironments.Add(new ClientEnvironment()
            {
                Name = "Test FAForever",
                Description = "Test Forged Alliance Forever server",
                Servers = new ObservableCollection<Server>()
                    {
                        new Server()
                        {
                            Name = "Lobby",
                            Host = "lobby.faforever.com",
                            Port = 8002
                        },
                        new Server()
                        {
                            Name = "Replay",
                            Host = "lobby.faforever.com",
                            Port = 15000
                        },
                        new Server()
                        {
                            Name = "Relay",
                            Host = "lobby.faforever.com",
                            Port = 8000
                        },
                        new Server()
                        {
                            Name = "IRC",
                            Host = "irc.faforever.com",
                            Port = 6697
                        },
                    },
                API = "https://api.faforever.com/data/"
            });
        }
    }
}
