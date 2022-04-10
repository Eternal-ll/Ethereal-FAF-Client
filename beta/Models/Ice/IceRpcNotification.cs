namespace beta.Models.Ice
{
    public enum IceRpcNotification : byte
    {
        onConnectionStateChanged,
        onGpgNetMessageReceived,
        onIceMsg,
        onIceConnectionStateChanged,
        onConnected
    }
}
