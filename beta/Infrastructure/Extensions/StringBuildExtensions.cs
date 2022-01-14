using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;

namespace beta.Infrastructure.Extensions
{
    public static class StringBuildExtensions
    {
        public static ByteArrayContent GetQueryByteArrayContent(this StringBuilder builder)
        {
            ByteArrayContent bytes = new(Encoding.UTF8.GetBytes(builder.ToString()));
            bytes.Headers.ContentType = new MediaTypeHeaderValue("application/x-www-form-urlencoded");
            return bytes;
        }

        public static byte[] GetBytes(this StringBuilder builder) => Encoding.UTF8.GetBytes(builder.ToString());
    }
}
