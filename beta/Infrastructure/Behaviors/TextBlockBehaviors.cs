using System.Collections.Generic;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;

namespace beta.Infrastructure.Behaviors
{
    /// <summary>
    /// Bindable inlines for TextBlock
    /// </summary>
    public static class TextBlockExtensions
    {
        public static IEnumerable<Inline> GetBindableInlines(DependencyObject obj) =>
            (IEnumerable<Inline>)obj.GetValue(BindableInlinesProperty);

        public static void SetBindableInlines(DependencyObject obj, IEnumerable<Inline> value) => 
            obj.SetValue(BindableInlinesProperty, value);

        public static readonly DependencyProperty BindableInlinesProperty =
            DependencyProperty.RegisterAttached("BindableInlines", typeof(IEnumerable<Inline>), typeof(TextBlockExtensions), new PropertyMetadata(null, OnBindableInlinesChanged));

        private static void OnBindableInlinesChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            if (d is TextBlock Target)
            {
                Target.Inlines.Clear();
                if (e.NewValue is not null) Target.Inlines.AddRange((System.Collections.IEnumerable)e.NewValue);
            }
        }
    }
}
