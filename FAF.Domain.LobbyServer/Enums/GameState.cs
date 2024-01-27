namespace FAF.Domain.LobbyServer.Enums
{
    /// <summary>
    /// Game state
    /// </summary>
    public enum GameState : byte
    {
        /// <summary>
        /// Game open, free to join
        /// </summary>
        Open = 1,
        /// <summary>
        /// Game launched, only for live replay
        /// </summary>
        Playing = 2,
        /// <summary>
        /// Game is initializing or closing
        /// </summary>
        Closed = 3
    }
}
