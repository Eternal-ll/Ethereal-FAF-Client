using beta.Models;
using beta.Models.API;
using beta.Models.API.Base;
using System;
using System.Threading.Tasks;

namespace beta.ViewModels
{
    internal class AvatarsViewModel : ApiPlayerViewModel
    {
        public EventHandler<AvatarModel> AvatarChanged;
        public int[] AvatarsIds { get; private set; }

        public AvatarsViewModel(int playerId, params int[] avatarsIds) : base(playerId)
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

        protected override async Task RequestTask()
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
                    ExpiresAt = result1.Data.ExpiresAt,
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
