using Microsoft;
using StreamJsonRpc;
using StreamJsonRpc.Protocol;
using StreamJsonRpc.Reflection;
using System;
using System.Buffers;
using System.Collections.Generic;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.Reflection;
using System.Text;
using System.Text.Json;

namespace Ethereal.FAF.UI.Client.Infrastructure.Lobby
{
    public class FafLobbySystemTextJsonFormatter : FormatterBase, IJsonRpcMessageFormatter, IJsonRpcMessageTextFormatter, IJsonRpcInstanceContainer, IJsonRpcMessageFactory, IJsonRpcFormatterTracingCallbacks
    {
        private static readonly JsonWriterOptions WriterOptions = new() { };

        private static readonly JsonDocumentOptions DocumentOptions = new() { };
        /// <summary>
        /// UTF-8 encoding without a preamble.
        /// </summary>
        private static readonly Encoding DefaultEncoding = new UTF8Encoding(encoderShouldEmitUTF8Identifier: false);
        private readonly ToStringHelper serializationToStringHelper = new ToStringHelper();

        public JsonSerializerOptions massagedUserDataSerializerOptions;
        public JsonRpc PublicJsonRpc => JsonRpc;
        public DeserializationTracking PublicTrackDeserialization(JsonRpcMessage message, ReadOnlySpan<ParameterInfo> parameters = default(ReadOnlySpan<ParameterInfo>))
            => TrackDeserialization(message, parameters);

        /// <summary>
        /// Retains the message currently being deserialized so that it can be disposed when we're done with it.
        /// </summary>
        private JsonDocument? deserializingDocument;
        /// <inheritdoc/>
        public Encoding Encoding
        {
            get => DefaultEncoding;
            set => throw new NotSupportedException();
        }
        public JsonRpcMessage Deserialize(ReadOnlySequence<byte> contentBuffer) => Deserialize(contentBuffer, Encoding);

        /// <inheritdoc/>
        public JsonRpcMessage Deserialize(ReadOnlySequence<byte> contentBuffer, Encoding encoding)
        {
            if (encoding is not UTF8Encoding)
            {
                throw new NotSupportedException("Only our default encoding is supported.");
            }

            JsonDocument document = deserializingDocument = JsonDocument.Parse(contentBuffer, DocumentOptions);
            if (document.RootElement.ValueKind != JsonValueKind.Object)
            {
                throw new JsonException("Expected a JSON object at the root of the message.");
            }

            JsonRpcMessage message;
            if (document.RootElement.TryGetProperty(LobbyUtf8Strings.command, out JsonElement jsonElement))
            {
                var command = jsonElement.GetString();
                if (command == LobbyUtf8Strings.game_info &&
                    document.RootElement.TryGetProperty(LobbyUtf8Strings.gamesProperty, out _))
                {
                    command = LobbyUtf8Strings.games_info;
                }
                else if (command == LobbyUtf8Strings.player_info &&
                    document.RootElement.TryGetProperty(LobbyUtf8Strings.playersProperty, out _))
                {
                    command = LobbyUtf8Strings.players_info;
                }
                LobbyJsonRpcRequest request = new(this)
                {
                    RequestId = RequestId.NotSpecified,
                    Method = command,
                    JsonArguments = document.RootElement,
                    TraceParent = null,
                    TraceState = null,
                };
                message = request;
            }
            else
            {
                throw new JsonException("Expected a command message.");
            }

            //if (message is IMessageWithTopLevelPropertyBag messageWithTopLevelPropertyBag)
            //{
            //    messageWithTopLevelPropertyBag.TopLevelPropertyBag = new TopLevelPropertyBag(document, this.massagedUserDataSerializerOptions);
            //}

            IJsonRpcTracingCallbacks? tracingCallbacks = JsonRpc;
            tracingCallbacks?.OnMessageDeserialized(message, document.RootElement);

            return message;
        }
        /// <inheritdoc/>
        public object GetJsonText(JsonRpcMessage message) => throw new NotSupportedException();

        public void Serialize(IBufferWriter<byte> bufferWriter, JsonRpcMessage message)
        {
            Requires.NotNull(message);

            using (TrackSerialization(message))
            {
                try
                {
                    //using MemoryStream stream = new();
                    //using Utf8JsonWriter writer = new(stream, WriterOptions);
                    using Utf8JsonWriter writer = new(bufferWriter, WriterOptions);
                    writer.WriteStartObject();
                    switch (message)
                    {
                        case LobbyJsonRpcRequest request:
                            request.RequestId = RequestId.NotSpecified;
                            writer.WriteString(LobbyUtf8Strings.command, request.Method);
                            WriteArguments(request);
                            request.PublicReleaseBuffers();
                            break;
                        default:
                            throw new ArgumentException("Unknown message type: " + message.GetType().Name, nameof(message));
                    }

                    writer.WriteEndObject();
                    //writer.Flush();
                    //string json = Encoding.UTF8.GetString(stream.ToArray());
                    //Console.WriteLine(json);

                    void WriteArguments(JsonRpcRequest request)
                    {
                        //if (request.ArgumentsList is not null)
                        //{
                        //    //writer.WriteStartArray(Utf8Strings.@params);
                        //    for (int i = 0; i < request.ArgumentsList.Count; i++)
                        //    {
                        //        writer.WritePropertyName(argument.Key);
                        //        var type = request.ArgumentListDeclaredTypes?[i];
                        //        var value = request.ArgumentsList[i];
                        //        WriteUserData(value, type);
                        //    }

                        //    //writer.WriteEndArray();
                        //}
                        //else if (request.NamedArguments is not null)
                        //{
                        //    //writer.WriteStartObject(Utf8Strings.@params);
                        //    foreach (KeyValuePair<string, object?> argument in request.NamedArguments)
                        //    {
                        //        writer.WritePropertyName(argument.Key);
                        //        WriteUserData(argument.Value, request.NamedArgumentDeclaredTypes?[argument.Key]);
                        //    }

                        //    //writer.WriteEndObject();
                        //}
                        if (request.Arguments is not null)
                        {
                            var type = request.Arguments.GetType();
                            if (type.Attributes == TypeAttributes.AnsiClass)
                            {
                                // proxy generated
                                var properties = type.GetFields(
                                    BindingFlags.Instance | BindingFlags.Static | BindingFlags.Public | BindingFlags.NonPublic);
                                foreach (var property in properties)
                                {
                                    writer.WritePropertyName(property.Name);
                                    WriteUserData(property.GetValue(request.Arguments), property.FieldType);
                                }
                            }
                            else if (type.Attributes ==( TypeAttributes.Sealed | TypeAttributes.BeforeFieldInit))
                            {
                                // anonymous
                                var properties = type.GetProperties(
                                    BindingFlags.Instance | BindingFlags.Static | BindingFlags.Public | BindingFlags.NonPublic);
                                foreach (var property in properties)
                                {
                                    writer.WritePropertyName(property.Name);
                                    WriteUserData(property.GetValue(request.Arguments), property.PropertyType);
                                }
                            }
                            
                            // This is a custom named arguments object, so we'll just serialize it as-is.
                            //writer.WritePropertyName(Utf8Strings.@params);
                            //WriteUserData(request.Arguments, declaredType: null);
                        }
                    }

                    void WriteUserData(object? value, Type? declaredType)
                    {
                        if (declaredType is not null && value is not null)
                        {
                            JsonSerializer.Serialize(writer, value, declaredType, massagedUserDataSerializerOptions);
                        }
                        else
                        {
                            JsonSerializer.Serialize(writer, value, massagedUserDataSerializerOptions);
                        }
                    }
                }
                catch (Exception ex)
                {
                    throw new JsonException("Failed to write", ex);
                }
            }
        }

        JsonRpcError IJsonRpcMessageFactory.CreateErrorMessage() => new JsonRpcError();

        JsonRpcRequest IJsonRpcMessageFactory.CreateRequestMessage() => new LobbyJsonRpcRequest(this)
        {
            RequestId = RequestId.NotSpecified
        };
        

        JsonRpcResult IJsonRpcMessageFactory.CreateResultMessage() => new JsonRpcResult();

        void IJsonRpcFormatterTracingCallbacks.OnSerializationComplete(JsonRpcMessage message, ReadOnlySequence<byte> encodedMessage)
        {
            IJsonRpcTracingCallbacks? tracingCallbacks = JsonRpc;
            serializationToStringHelper.Activate(encodedMessage);
            try
            {
                tracingCallbacks?.OnMessageSerialized(message, serializationToStringHelper);
            }
            finally
            {
                serializationToStringHelper.Deactivate();
            }
        }

        private static class LobbyUtf8Strings
        {
#pragma warning disable SA1300 // Element should begin with upper-case letter
            internal static ReadOnlySpan<byte> command => "command"u8;
            internal static string game_info = "game_info";
            internal static string gamesProperty = "games";
            internal static string games_info = "games_info";
            internal static string player_info = "player_info";
            internal static string playersProperty = "players";
            internal static string players_info = "players_info";
#pragma warning restore SA1300 // Element should begin with upper-case letter
        }
        private class LobbyJsonRpcRequest : JsonRpcRequestBase
        {
            private readonly FafLobbySystemTextJsonFormatter formatter;

            private int? _argumentCount;
            private JsonElement? _jsonArguments;

            internal LobbyJsonRpcRequest(FafLobbySystemTextJsonFormatter formatter)
            {
                this.formatter = formatter;
            }

            public override int ArgumentCount => _argumentCount ?? base.ArgumentCount;

            internal JsonElement? JsonArguments
            {
                get => _jsonArguments;
                init
                {
                    _jsonArguments = value;
                    if (value.HasValue)
                    {
                        _argumentCount = CountArguments(value.Value);
                    }
                }
            }

            public override ArgumentMatchResult TryGetTypedArguments(ReadOnlySpan<ParameterInfo> parameters, Span<object?> typedArguments)
            {
                using (formatter.PublicTrackDeserialization(this, parameters))
                {
                    // single object deserialization
                    if (parameters.Length == 1 &&
                        formatter.ApplicableMethodAttributeOnDeserializingMethod?
                            .UseSingleObjectParameterDeserialization is true)
                    {
                        var type = parameters[0].ParameterType;
                        typedArguments[0] = JsonSerializer.Deserialize(JsonArguments.Value, type, formatter.massagedUserDataSerializerOptions);
                        return ArgumentMatchResult.Success;
                    }

                    // We ignore "command" argument in count
                    if (parameters.Length < ArgumentCount - 1)
                    {
                        return ArgumentMatchResult.ParameterArgumentCountMismatch;
                    }

                    if (parameters.Length == 0)
                    {
                        return ArgumentMatchResult.Success;
                    }

                    for (int i = 0; i < parameters.Length; i++)
                    {
                        ParameterInfo parameter = parameters[i];
                        if (TryGetArgumentByNameOrIndex(parameter.Name, i, parameter.ParameterType, out object? argument))
                        {
                            if (argument is null)
                            {
                                if (parameter.ParameterType.GetTypeInfo().IsValueType && Nullable.GetUnderlyingType(parameter.ParameterType) is null)
                                {
                                    // We cannot pass a null value to a value type parameter.
                                    return ArgumentMatchResult.ParameterArgumentTypeMismatch;
                                }
                            }
                            else if (!parameter.ParameterType.GetTypeInfo().IsAssignableFrom(argument.GetType()))
                            {
                                return ArgumentMatchResult.ParameterArgumentTypeMismatch;
                            }

                            typedArguments[i] = argument;
                        }
                        else if (parameter.HasDefaultValue)
                        {
                            // The client did not supply an argument, but we have a default value to use, courtesy of the parameter itself.
                            typedArguments[i] = parameter.DefaultValue;
                        }
                        else
                        {
                            return ArgumentMatchResult.MissingArgument;
                        }
                    }

                    return ArgumentMatchResult.Success;
                }
            }

            public override bool TryGetArgumentByNameOrIndex(string? name, int position, Type? typeHint, out object? value)
            {
                if (JsonArguments is null)
                {
                    value = null;
                    return false;
                }

                JsonElement? valueElement = null;
                switch (JsonArguments?.ValueKind)
                {
                    case JsonValueKind.Object when name is not null:
                        if (JsonArguments.Value.TryGetProperty(name, out JsonElement propertyValue))
                        {
                            valueElement = propertyValue;
                        }

                        break;
                    case JsonValueKind.Array:
                        int elementIndex = 0;
                        foreach (JsonElement arrayElement in JsonArguments.Value.EnumerateArray())
                        {
                            if (elementIndex++ == position)
                            {
                                valueElement = arrayElement;
                                break;
                            }
                        }

                        break;
                    default:
                        throw new JsonException("Unexpected value kind for arguments: " + (JsonArguments?.ValueKind.ToString() ?? "null"));
                }

                try
                {
                    using (formatter.PublicTrackDeserialization(this))
                    {
                        try
                        {
                            value = valueElement?.Deserialize(typeHint ?? typeof(object), formatter.massagedUserDataSerializerOptions);
                        }
                        catch (Exception ex)
                        {
                            if (formatter.PublicJsonRpc?.TraceSource.Switch.ShouldTrace(TraceEventType.Warning) ?? false)
                            {
                                formatter.PublicJsonRpc.TraceSource.TraceEvent(TraceEventType.Warning, (int)JsonRpc.TraceEvents.MethodArgumentDeserializationFailure, "Deserializing JSON-RPC argument with name \"{0}\" and position {1} to type \"{2}\" failed: {3}", name, position, typeHint, ex);
                            }

                            throw new RpcArgumentDeserializationException(name, position, typeHint, ex);
                        }
                    }
                }
                catch (JsonException ex)
                {
                    throw new RpcArgumentDeserializationException(name, position, typeHint, ex);
                }

                return valueElement.HasValue;
            }
            private static int CountArguments(JsonElement arguments)
            {
                int count;
                switch (arguments.ValueKind)
                {
                    case JsonValueKind.Array:
                        count = arguments.GetArrayLength();

                        break;
                    case JsonValueKind.Object:
                        count = 0;
                        foreach (JsonProperty property in arguments.EnumerateObject())
                        {
                            count++;
                        }

                        break;
                    default:
                        throw new InvalidOperationException("Unexpected value kind: " + arguments.ValueKind);
                }

                return count;
            }
            protected override TopLevelPropertyBagBase? CreateTopLevelPropertyBag() => new TopLevelPropertyBag(formatter.massagedUserDataSerializerOptions);

            protected override void ReleaseBuffers()
            {
                base.ReleaseBuffers();
                _jsonArguments = null;
                formatter.deserializingDocument?.Dispose();
                formatter.deserializingDocument = null;
            }
            public void PublicReleaseBuffers() => ReleaseBuffers();
        }
        private class TopLevelPropertyBag : TopLevelPropertyBagBase
        {
            private readonly JsonDocument? incomingMessage;
            private readonly JsonSerializerOptions jsonSerializerOptions;

            /// <summary>
            /// Initializes a new instance of the <see cref="TopLevelPropertyBag"/> class
            /// for use with an incoming message.
            /// </summary>
            /// <param name="incomingMessage">The incoming message.</param>
            /// <param name="jsonSerializerOptions">The serializer options to use.</param>
            internal TopLevelPropertyBag(JsonDocument incomingMessage, JsonSerializerOptions jsonSerializerOptions)
                : base(isOutbound: false)
            {
                this.incomingMessage = incomingMessage;
                this.jsonSerializerOptions = jsonSerializerOptions;
            }

            /// <summary>
            /// Initializes a new instance of the <see cref="TopLevelPropertyBag"/> class
            /// for use with an outcoming message.
            /// </summary>
            /// <param name="jsonSerializerOptions">The serializer options to use.</param>
            internal TopLevelPropertyBag(JsonSerializerOptions jsonSerializerOptions)
                : base(isOutbound: true)
            {
                this.jsonSerializerOptions = jsonSerializerOptions;
            }

            internal void WriteProperties(Utf8JsonWriter writer)
            {
                if (incomingMessage is not null)
                {
                    // We're actually re-transmitting an incoming message (remote target feature).
                    // We need to copy all the properties that were in the original message.
                    // Don't implement this without enabling the tests for the scenario found in JsonRpcRemoteTargetSystemTextJsonFormatterTests.cs.
                    // The tests fail for reasons even without this support, so there's work to do beyond just implementing this.
                    throw new NotImplementedException();
                }
                else
                {
                    foreach (KeyValuePair<string, (Type DeclaredType, object? Value)> property in OutboundProperties)
                    {
                        writer.WritePropertyName(property.Key);
                        JsonSerializer.Serialize(writer, property.Value.Value, jsonSerializerOptions);
                    }
                }
            }
            protected override bool TryGetTopLevelProperty<T>(string name, [MaybeNull] out T value)
            {
                if (incomingMessage?.RootElement.TryGetProperty(name, out JsonElement serializedValue) is true)
                {
                    value = serializedValue.Deserialize<T>(jsonSerializerOptions);
                    return true;
                }

                value = default;
                return false;
            }
        }

        /// <inheritdoc cref="MessagePackFormatter.ToStringHelper"/>
        private class ToStringHelper
        {
            private ReadOnlySequence<byte>? encodedMessage;
            private string? jsonString;

            public override string ToString()
            {
                Verify.Operation(encodedMessage.HasValue, "This object has not been activated. It may have already been recycled.");

                using JsonDocument doc = JsonDocument.Parse(encodedMessage.Value);
                return jsonString ??= doc.RootElement.ToString();
            }

            /// <summary>
            /// Initializes this object to represent a message.
            /// </summary>
            internal void Activate(ReadOnlySequence<byte> encodedMessage)
            {
                this.encodedMessage = encodedMessage;
            }

            /// <summary>
            /// Cleans out this object to release memory and ensure <see cref="ToString"/> throws if someone uses it after deactivation.
            /// </summary>
            internal void Deactivate()
            {
                encodedMessage = null;
                jsonString = null;
            }
        }
    }
}
