using Microsoft.Xaml.Behaviors;
using System;
using System.Windows.Controls.Primitives;

namespace beta.Infrastructure.Behaviors
{
    public class CalculateColumns : Behavior<UniformGrid>
    {
        protected override void OnAttached()
        {
            AssociatedObject.SizeChanged += OnDataGridSizeChanged;
        }

        private void OnDataGridSizeChanged(object sender, System.Windows.SizeChangedEventArgs e)
        {
            if (e.NewSize.Width < 600) AssociatedObject.Columns = 1;
            else AssociatedObject.Columns = Convert.ToInt32((e.NewSize.Width - 60) / (330));
        }

        protected override void OnDetaching()
        {
            AssociatedObject.SizeChanged -= OnDataGridSizeChanged;
        }
    }
}
