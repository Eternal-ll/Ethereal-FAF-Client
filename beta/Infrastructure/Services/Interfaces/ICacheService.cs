using beta.Models;
using System;
using System.Windows.Media.Imaging;

namespace beta.Infrastructure.Services.Interfaces
{
    public interface ICacheService
    {
        public BitmapImage GetImage(Uri uri, Folder folder);
    }
}
