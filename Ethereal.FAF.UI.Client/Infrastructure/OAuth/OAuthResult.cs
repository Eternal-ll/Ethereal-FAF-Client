namespace Ethereal.FAF.UI.Client.Infrastructure.OAuth
{
    public class OAuthResult
    {
        public TokenBearer TokenBearer { get; set; }
        public bool IsError { get; set; } = true;
        public string Error { get; set; }
        public string ErrorDescription { get; set; }
    }
}
