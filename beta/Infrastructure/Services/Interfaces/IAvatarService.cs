using System;
using System.Windows.Media.Imaging;

namespace beta.Infrastructure.Services.Interfaces
{
    public interface IAvatarService
    {
        public BitmapImage GetAvatar(Uri uri);

        public void SetAvatar();
    }
}
