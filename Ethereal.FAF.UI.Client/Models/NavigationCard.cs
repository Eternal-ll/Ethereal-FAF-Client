using System;
using Wpf.Ui.Controls;

namespace Ethereal.FAF.UI.Client.Models
{
    public record NavigationCard
    {
        public string Name { get; init; }

        public FontIcon Icon { get; init; }

        public string Description { get; init; }

        public Type PageType { get; init; }
    }
}
