namespace Ethereal.FAF.API.Client
{
    public static partial class BuilderExtensions
    {
        public interface ITokenProvider
        {
            public string GetToken();
        }
    }
}
