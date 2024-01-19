using System;

namespace Ethereal.FAF.UI.Client.Models.Settings
{
    public class FafAuthToken
    {
        public string AuthProvider { get; set; }
        public string AccessToken { get; set; }
        public string IdToken { get; set; }
        public string RefreshToken { get; set; }
    }
}
