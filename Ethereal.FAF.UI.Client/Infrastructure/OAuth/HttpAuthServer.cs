using System;
using System.Net;
using System.Threading.Tasks;
using System.Windows;

namespace Ethereal.FAF.UI.Client.Infrastructure.OAuth
{
    internal sealed class HttpAuthServer
    {
        public event EventHandler<(string code, string state)> CodeReceived;

        public bool IsListening { get; set; }

        private HttpListener httpListener;

        /// <summary>
        /// Launches http listener and returns port
        /// </summary>
        /// <returns></returns>
        /// <exception cref="ArgumentOutOfRangeException"></exception>
        public int StartListener(int[] ports)
        {
            if (!TryFindOpenPort(ports, out var port))
            {
                throw new InvalidOperationException($"Cant find open port in given range.");
            }
            httpListener = new();
            httpListener.Prefixes.Add($"http://localhost:{port}/");
            httpListener.Start();

            IsListening = true;

            Task.Run(async () =>
            {
                while (IsListening)
                {
                    try
                    {
                        var context = await httpListener.GetContextAsync();
                        await HandleContext(context);
                    }
                    catch (HttpListenerException ex)
                    {
                        if (ex.ErrorCode != 995)
                        {
                            return;
                        }
                        throw;
                    }
                    catch (Exception ex)
                    {
                        throw;
                    }
                }
            });
            return port;
        }
        /// <summary>
        /// Stop http listener
        /// </summary>
        public void StopListener()
        {
            httpListener?.Stop();
            httpListener?.Close();
            httpListener = null;
            IsListening = false;
        }
        private async Task HandleContext(HttpListenerContext context)
        {
            if (context.Request.RawUrl == "/favicon.ico")
            {
                context.Response.StatusCode = 404;
                return;
            }
            if (context.Request.RawUrl.Contains("close"))
            {
                context.Response.StatusCode = 200;
                await Application.ResourceAssembly
                    .GetManifestResourceStream("Ethereal.FAF.UI.Client.Resources.Close.html")
                    .CopyToAsync(context.Response.OutputStream);
                context.Response.Close();
                return;
            }
            var code = context.Request.QueryString["code"];
            var state = context.Request.QueryString["state"];
            var error = context.Request.QueryString["error"];
            var errorDescription = context.Request.QueryString["error_description"];
            context.Response.StatusCode = 200;
            await Application.ResourceAssembly
                .GetManifestResourceStream("Ethereal.FAF.UI.Client.Resources.Result.html")
                .CopyToAsync(context.Response.OutputStream);
            context.Response.Close();
            if (code != null)
            {
                CodeReceived?.Invoke(this, (code, state));
            }
            else
            {

            }
        }

        private static bool TryFindOpenPort(int[] ports, out int openPort)
        {
            openPort = 0;
            using var httpListener = new HttpListener();
            foreach (var port in ports)
            {
                try
                {
                    httpListener.Prefixes.Clear();
                    httpListener.Prefixes.Add($"http://localhost:{port}/");
                    httpListener.Start();
                    httpListener.Close();
                    openPort = port;
                    return true;
                }
                catch
                {
                    httpListener.Close();
                }
            }
            return false;
        }
    }
}
