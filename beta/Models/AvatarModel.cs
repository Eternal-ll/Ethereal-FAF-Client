using System;
using System.Windows.Media.Imaging;

namespace beta.Models
{
    internal class AvatarModel
    {
        public string ToolTip { get; set; }
        public string UrlSource { get; set; }
        public string Filename { get; set; }
        public DateTime AssignedAt { get; set; }
        public DateTime UpdatedAt { get; set; }
        public DateTime? ExpiresAt { get; set; }
        public bool IsSelected { get; set; }
    }
}
