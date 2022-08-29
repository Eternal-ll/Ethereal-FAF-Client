using System;
using System.Buffers;
using System.IO;
using System.IO.Pipelines;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace beta.Models
{
    public class LobbyClient : PipeTcpClient
    {
        protected override Task ProcessMessageAsync(string message)
        {
            throw new NotImplementedException();
        }
    }
    public abstract class PipeTcpClient
    {
        protected TcpClient TcpClient = new();
        protected NetworkStream NetworkStream;
        protected async Task<bool> ConnectAsync(string host, int port) => await TcpClient
            .ConnectAsync(host, port)
            .ContinueWith(t =>
            {
                if (t.IsFaulted) return false;
                NetworkStream = new NetworkStream(TcpClient.Client);
                Task.Run(async () =>
                {
                    await ProcessMessagesAsync(PipeReader.Create(TcpClient.GetStream()));
                });
                return TcpClient.Connected;
            });

        protected abstract Task ProcessMessageAsync(string message);

        protected async Task WriteMessagesAsync(string message) =>
            await NetworkStream.WriteAsync(Encoding.UTF8.GetBytes(message));

        private async Task ProcessMessagesAsync(PipeReader reader, CancellationToken cancellationToken = default)
        {
            try
            {
                while (true)
                {
                    ReadResult result = await reader.ReadAsync(cancellationToken);
                    ReadOnlySequence<byte> buffer = result.Buffer;

                    try
                    {
                        // Process all messages from the buffer, modifying the input buffer on each
                        // iteration.
                        while (TryParseLines(ref buffer, out string message))
                        {
                            await ProcessMessageAsync(message);
                        }

                        // There's no more data to be processed.
                        if (result.IsCompleted)
                        {
                            if (buffer.Length > 0)
                            {
                                // The message is incomplete and there's no more data to process.
                                throw new InvalidDataException("Incomplete message.");
                            }
                            break;
                        }
                    }
                    finally
                    {
                        // Since all messages in the buffer are being processed, you can use the
                        // remaining buffer's Start and End position to determine consumed and examined.
                        reader.AdvanceTo(buffer.Start, buffer.End);
                    }
                }
            }
            finally
            {
                await reader.CompleteAsync();
            }
        }

        private static bool TryParseLines(ref ReadOnlySequence<byte> buffer, out string message)
        {
            SequencePosition? position;
            StringBuilder outputMessage = new();
            while (true)
            {
                position = buffer.PositionOf((byte)'\n');
                if (!position.HasValue)
                    break;
                outputMessage
                    .Append(Encoding.UTF8.GetString(buffer.Slice(buffer.Start, position.Value)))
                    .AppendLine();
                buffer = buffer.Slice(buffer.GetPosition(1, position.Value));
            };
            message = outputMessage.ToString();
            return message.Length != 0;
        }
    }
}