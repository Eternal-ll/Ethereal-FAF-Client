using beta.Infrastructure.Commands;
using beta.Models.API;
using beta.Models.API.Base;
using System;
using System.Threading.Tasks;
using System.Windows.Input;
using System.Windows.Media.Imaging;

namespace beta.ViewModels
{
    public abstract class ApiPlayerViewModel : Base.ViewModel
    {
        #region IsPendingRequest
        private bool _IsPendingRequest;
        public bool IsPendingRequest
        {
            get => _IsPendingRequest;
            set => Set(ref _IsPendingRequest, value);
        }
        #endregion

        public abstract void RunRequest();

        #region RefreshCommand
        private ICommand _RefreshCommand;
        public ICommand RefreshCommand => _RefreshCommand ??= new LambdaCommand(OnRefreshCommand, CanRefreshCommand);
        private bool CanRefreshCommand(object parameter) => !IsPendingRequest;
        private void OnRefreshCommand(object parameter) => RunRequest(); 
        #endregion
    }
    internal class AvatarModel
    {
        public string ToolTip { get; set; }
        public Uri Url { get; set; }
        private BitmapImage _Preview;
        public BitmapImage Preview
        {
            get
            {
                if (_Preview is not null) return _Preview;
                BitmapImage img = new();
                img.BeginInit();
                img.DecodePixelHeight = 20;
                img.DecodePixelWidth = 40;
                img.CacheOption = BitmapCacheOption.OnLoad;
                img.UriSource = Url;
                img.EndInit();
                _Preview = img;
                return _Preview;
            }
        }

        public string Filename { get; set; }
        public DateTime AssignedAt { get; set; }
        public DateTime UpdatedAt { get; set; }
        public DateTime? ExpiresAT { get; set; }
        public bool IsSelected { get; set; }
    }
    internal class AvatarsViewModel : ApiPlayerViewModel
    {
        public EventHandler<AvatarModel> AvatarChanged;
        public int[] AvatarsIds { get; private set; }

        public AvatarsViewModel(params int[] avatarsIds)
        {
            AvatarsIds = avatarsIds;
            RunRequest();
        }

        #region Avatars
        private AvatarModel[] _Avatars;
        public AvatarModel[] Avatars
        {
            get => _Avatars;
            set => Set(ref _Avatars, value);
        }
        #endregion

        #region SelectedAvatar
        private AvatarModel _SelectedAvatar;
        public AvatarModel SelectedAvatar
        {
            get => _SelectedAvatar;
            set
            {
                if (Set(ref _SelectedAvatar, value))
                {
                    AvatarChanged?.Invoke(this, value);
                }
            }
        }
        #endregion

        public override void RunRequest()
        {
            Task.Run(async () =>
            {
                IsPendingRequest = true;
                await Request();
                IsPendingRequest = false;
            });
        }
        private async Task Request()
        {
            var ids = AvatarsIds;
            var avatars = new AvatarModel[ids.Length];
            for (int i = 0; i < AvatarsIds.Length; i++)
            {
                var id = AvatarsIds[i];
                var result1 = await ApiRequest<ApiUniversalResult<ApiAvatarAssignment>>.RequestWithId("https://api.faforever.com/data/avatarAssignment/",
                    id);
                if (result1 is null) continue;
                AvatarModel model = new()
                {
                    AssignedAt = result1.Data.CreateTime,
                    UpdatedAt = result1.Data.UpdateTime,
                    ExpiresAT = result1.Data.ExpiresAt,
                    IsSelected = result1.Data.IsSelected
                };
                var avatarId = result1.Data.Relations["avatar"].Data[0].Id;

                var result2 = await ApiRequest<ApiUniversalResult<ApiUniversalWithAttributes>>.RequestWithId("https://api.faforever.com/data/avatar/", avatarId,
                    "?fields[avatar]=filename,tooltip,url");

                model.Filename = result2.Data.Attributes["filename"];
                model.ToolTip = result2.Data.Attributes["tooltip"];
                model.Url = new(result2.Data.Attributes["url"]);

                avatars[i] = model;

                if (model.IsSelected) SelectedAvatar = model;
            }
            Avatars = avatars;
        }
    }
}
