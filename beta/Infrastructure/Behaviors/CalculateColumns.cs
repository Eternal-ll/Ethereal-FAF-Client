using Microsoft.Xaml.Behaviors;
using System;
using System.Windows.Controls.Primitives;

namespace beta.Infrastructure.Behaviors
{
    /// <summary>
    /// Dynamis column calculator for <see cref="UniformGrid"/>
    /// </summary>
    public class CalculateColumns : Behavior<UniformGrid>
    {
        public int Width { get; set; } = 100;
        public int WidthOffset { get; set; } = 0;
        protected override void OnAttached()
        {
            AssociatedObject.SizeChanged += OnDataGridSizeChanged;
        }

        private void OnDataGridSizeChanged(object sender, System.Windows.SizeChangedEventArgs e)
        {
            if (e.NewSize.Width < Width) AssociatedObject.Columns = 1;
            else AssociatedObject.Columns = Convert.ToInt32((e.NewSize.Width - WidthOffset) / Width);
        }

        protected override void OnDetaching()
        {
            AssociatedObject.SizeChanged -= OnDataGridSizeChanged;
        }
    }
}
