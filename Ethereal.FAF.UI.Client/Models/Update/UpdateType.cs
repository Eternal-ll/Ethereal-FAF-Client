using System;

namespace Ethereal.FAF.UI.Client.Models.Update
{
    [Flags]
    public enum UpdateType
    {
        /// <summary>
        /// Usual update
        /// </summary>
        Normal = 1 << 0,
        /// <summary>
        /// Critical update
        /// </summary>
        Critical = 1 << 1,
        /// <summary>
        /// Mandatory update
        /// </summary>
        Mandatory = 1 << 2,
    }
}
