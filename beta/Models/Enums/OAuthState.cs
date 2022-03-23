namespace beta.Models.Enums
{
    public enum OAuthState : byte
    {
        EMPTY_FIELDS,
        NO_CONNECTION,
        NO_TOKEN,
        INVALID,
        TIMED_OUT,
        AUTHORIZED,
    }
}