using System.Collections.Generic;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;

namespace beta.Infrastructure.Behaviors
{
    public static class TextBlockExtensions
    {
        public static IEnumerable<Inline> GetBindableInlines(DependencyObject obj)
        {
            return (IEnumerable<Inline>)obj.GetValue(BindableInlinesProperty);
        }

        public static void SetBindableInlines(DependencyObject obj, IEnumerable<Inline> value)
        {
            obj.SetValue(BindableInlinesProperty, value);
        }

        public static readonly DependencyProperty BindableInlinesProperty =
            DependencyProperty.RegisterAttached("BindableInlines", typeof(IEnumerable<Inline>), typeof(TextBlockExtensions), new PropertyMetadata(null, OnBindableInlinesChanged));

        private static void OnBindableInlinesChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            var Target = d as TextBlock;

            if (Target is not null)
            {
                Target.Inlines.Clear();
                Target.Inlines.AddRange((System.Collections.IEnumerable)e.NewValue);
            }
        }
    }
}
