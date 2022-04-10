using beta.Models.Enums;
using System;

namespace beta.Models
{
    public class OAuthEventArgs : EventArgs
    {
        public OAuthState State { get; }
        public string Message { get; }
        public string Trace { get; }
        public OAuthEventArgs(OAuthState state, string message, string trace = null)
        {
            State = state;
            Message = message;
            Trace = trace;
        }
    }
}