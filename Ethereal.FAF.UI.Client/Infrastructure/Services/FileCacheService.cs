using Ethereal.FAF.UI.Client.Infrastructure.Services.Interfaces;
using Microsoft.Extensions.Configuration;
using System;
using System.Collections.Generic;
using System.IO;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;

namespace Ethereal.FAF.UI.Client.Infrastructure.Services
{
	internal class FileCacheService : IFileCacheService
	{
		private readonly IConfiguration _configuration;
		private readonly IHttpClientFactory _httpClientFactory;

		public FileCacheService(IConfiguration configuration, IHttpClientFactory httpClientFactory)
		{
			_configuration = configuration;
			_httpClientFactory = httpClientFactory;
		}
		private string GetCacheDirectory() => _configuration.GetValue<string>("Client:Cache") ?? Path.GetTempPath();

		public async Task<string> Cache(string url, CancellationToken cancellationToken)
		{
			var ub = new UriBuilder(url);
			var cache = GetCacheDirectory()[..^1];
			var segments = new List<string>()
			{
				cache
			};
			segments.AddRange(ub.Uri.Segments[1..]);
			var cacheFile = Path.Combine(segments.ToArray());
			if (!Directory.Exists(Path.GetDirectoryName(cacheFile)))
			{
				Directory.CreateDirectory(Path.GetDirectoryName(cacheFile));
			}
			if (File.Exists(cacheFile))
			{
				return cacheFile;
			}
			using var client = _httpClientFactory.CreateClient();
			using var rsp = await client.GetAsync(url, cancellationToken);
			if (rsp.IsSuccessStatusCode)
			{
				using var fs = new FileStream(cacheFile, FileMode.Create);
				await rsp.Content.CopyToAsync(fs, cancellationToken);
				return cacheFile;
			}
			return null;
		}
	}
}
