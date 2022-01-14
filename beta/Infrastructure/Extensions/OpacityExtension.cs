using System;
using System.Windows.Markup;
using System.Windows.Media;

namespace beta.Infrastructure.Extensions
{
    public class OpacityExtension : MarkupExtension
    {
        private readonly Color color;

        public byte Opacity { get; set; } // defaults to 0, so you don't have 
        // to set it for the color to be transparent

        public OpacityExtension(Color color)
        {
            this.color = color;
        }

        public override object ProvideValue(IServiceProvider serviceProvider)
        {
            return Color.FromArgb(Opacity, color.R, color.G, color.B);
        }
    }
}
