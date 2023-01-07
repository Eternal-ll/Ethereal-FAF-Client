using Ethereal.FA.Vault;
using System;

namespace Ethereal.FAF.UI.Client.Models
{
    public sealed class LocalMap
    {
        public string FolderName { get; set; }
        public MapScenario Scenario { get; set; }
        public string Preview { get; set; }
        public DateTime Downloaded { get; set; }
    }
}
