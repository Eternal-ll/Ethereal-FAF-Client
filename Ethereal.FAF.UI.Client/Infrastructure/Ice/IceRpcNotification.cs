namespace Ethereal.FAF.UI.Client.Infrastructure.Ice
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
