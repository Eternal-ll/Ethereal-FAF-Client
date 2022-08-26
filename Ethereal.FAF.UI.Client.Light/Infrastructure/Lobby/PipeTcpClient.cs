﻿using Microsoft.Extensions.Logging;
using System;
using System.Buffers;
using System.IO;
using System.IO.Pipelines;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Ethereal.FAF.UI.Client.Light.Infrastructure.Lobby
{
    public abstract class PipeTcpClient
    {
        protected readonly ILogger Logger;

        private TcpClient TcpClient;

        private PipeWriter Writer;

        public PipeTcpClient(ILogger logger)
        {
            Logger = logger;
        }

        public Task<bool> ConnectAsync(string host, int port)
        {
            var client = new TcpClient(host, port);
            var reader = PipeReader.Create(client.GetStream());
            Writer = PipeWriter.Create(client.GetStream(), new StreamPipeWriterOptions(leaveOpen: true));
            Task.Run(async () =>
            {
                CancellationTokenSource cancellationTokenSource = new();
                while (client.Connected)
                {
                    await ProcessMessagesAsync(reader);
                    await Task.Delay(50);
                }
            });
            return Task.FromResult(client.Connected);
        }

        protected abstract Task HandleMessage(string message);

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
                            await HandleMessage(message);
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

        private bool TryParseLines(ref ReadOnlySequence<byte> buffer, out string message)
        {
            SequencePosition? position;
            StringBuilder outputMessage = new();

            while (true)
            {
                position = buffer.PositionOf((byte)'\n');

                if (!position.HasValue)
                    break;

                outputMessage.Append(Encoding.UTF8.GetString(buffer.Slice(buffer.Start, position.Value)))
                            .AppendLine();

                buffer = buffer.Slice(buffer.GetPosition(1, position.Value));
            };

            message = outputMessage.ToString();
            return message.Length != 0;
        }

        public Task WriteMessagesAsync(string message)
        {
            WriteMessagesAsync(Writer, message);
            return Task.CompletedTask;
        }
        private ValueTask<FlushResult> WriteMessagesAsync(PipeWriter writer, string message) =>
            writer.WriteAsync(Encoding.UTF8.GetBytes(message));
    }
}
