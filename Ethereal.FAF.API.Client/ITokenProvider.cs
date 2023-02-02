namespace Ethereal.FAF.API.Client
{
    public interface ITokenProvider
    {
        public Task<string> GetAccessTokenAsync(CancellationToken cancellationToken = default);
    }
}
