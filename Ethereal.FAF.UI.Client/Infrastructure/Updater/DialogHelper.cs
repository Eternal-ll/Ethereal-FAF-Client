using ICSharpCode.AvalonEdit;
using MdXaml;
using Microsoft.Extensions.Logging;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Threading;
using Wpf.Ui.Controls;

namespace Ethereal.FAF.UI.Client.Infrastructure.Updater
{
    public enum TextEditorPreset
    {

    }
    internal class DialogHelper
    {

        /// <summary>
        /// Create a generic dialog for showing a markdown document
        /// </summary>
        public static ContentDialog CreateMarkdownDialog(
            string markdown,
            string? title = null,
            TextEditorPreset editorPreset = default
        )
        {
            Application.Current.Dispatcher.VerifyAccess();

            var viewer = new MarkdownScrollViewer { Markdown = markdown };

            // Apply syntax highlighting to code blocks if preset is provided
            //if (editorPreset != default)
            //{
            //    using var _ = CodeTimer.StartDebug();

            //    var appliedCount = 0;

            //    if (
            //        viewer.GetLogicalDescendants().FirstOrDefault()?.GetLogicalDescendants() is
            //        { } stackDescendants
            //    )
            //    {
            //        foreach (var editor in stackDescendants.OfType<TextEditor>())
            //        {
            //            TextEditorConfigs.Configure(editor, editorPreset);

            //            editor.FontFamily = "Cascadia Code,Consolas,Menlo,Monospace";
            //            editor.Margin = new Thickness(0);
            //            editor.Padding = new Thickness(4);
            //            editor.IsEnabled = false;

            //            if (editor.GetLogicalParent() is Border border)
            //            {
            //                border.BorderThickness = new Thickness(0);
            //                border.CornerRadius = new CornerRadius(4);
            //            }

            //            appliedCount++;
            //        }
            //    }

            //    Logger.Log(
            //        appliedCount > 0 ? LogLevel.Trace : LogLevel.Warn,
            //        $"Applied syntax highlighting to {appliedCount} code blocks"
            //    );
            //}

            return new ContentDialog(new())
            {
                Title = title,
                Content = viewer,
                CloseButtonText = "Close",
                IsPrimaryButtonEnabled = false,
            };
        }
    }
}
