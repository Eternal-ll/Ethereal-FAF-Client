using System;
using System.Windows.Media.Imaging;

namespace beta.Infrastructure.Services.Interfaces
{
    public interface IMapService
    {
        public BitmapImage GetMap(Uri uri);
    }
}
