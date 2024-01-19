using Ethereal.FAF.UI.Client.Models.Update;
using System;
using System.Collections.Generic;

namespace Ethereal.FAF.UI.Client.Infrastructure.Updater
{
    public class UpdateStatusChangedEventArgs
    {
        public UpdateInfo? LatestUpdate { get; init; }

        public IReadOnlyDictionary<UpdateChannel, UpdateInfo> UpdateChannels { get; init; } =
            new Dictionary<UpdateChannel, UpdateInfo>();

        public DateTimeOffset CheckedAt { get; init; } = DateTimeOffset.UtcNow;
    }
}
