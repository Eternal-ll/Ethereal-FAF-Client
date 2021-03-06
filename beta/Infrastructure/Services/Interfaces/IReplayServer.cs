using beta.Models;
using System;

namespace beta.Infrastructure.Services.Interfaces
{
    /// <summary>
    /// Replay service for saving and stream replays
    /// </summary>
    public interface IReplayServerService
    {
        public event EventHandler<ReplayRecorder> ReplayRecorderCreated;

        /// <summary>
        /// Starts local listening server that FA can send its replay data to.
        /// </summary>
        /// <returns>Port of started listening server</returns>
        public int StartReplayServer();
        /// <summary>
        /// Stops local listening server
        /// </summary>
        /// <returns></returns>
        public void StopReplayServer();
    }
}
