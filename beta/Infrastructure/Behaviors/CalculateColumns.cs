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
        /// <summary>
        /// 
        /// </summary>
        public int Width { get; set; } = 100;
        /// <summary>
        /// 
        /// </summary>
        public int WidthOffset { get; set; } = 0;
        
        protected override void OnAttached() =>
            AssociatedObject.SizeChanged += OnDataGridSizeChanged;
        protected override void OnDetaching() =>
            AssociatedObject.SizeChanged -= OnDataGridSizeChanged;
        private void OnDataGridSizeChanged(object sender, System.Windows.SizeChangedEventArgs e)
        {
            if (e.NewSize.Width > 10000) return;
            AssociatedObject.Columns = e.NewSize.Width <= Width ? 1 : Convert.ToInt32((e.NewSize.Width - WidthOffset) / Width);
        }
    }
}
