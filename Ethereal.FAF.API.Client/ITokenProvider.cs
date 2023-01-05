namespace Ethereal.FAF.API.Client
{
    public interface ITokenProvider
    {
        public string GetToken();
        public Task<string> GetTokenAsync(string host);
    }
}
