using beta.Resources.Controls;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Text;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Media;
using System.Windows.Media.Imaging;

namespace beta.Infrastructure.Converters
{
    public interface IEmojiCache
    {
        public string Name { get; set; }
    }
    public struct GIFEmojiCache : IEmojiCache
    {
        public string Name { get; set; }
        public GifBitmapDecoder GifBitmapDecoder;
    }
    public struct BitmapEmojiCache : IEmojiCache
    {
        public string Name { get; set; }
        public ImageSource ImageSource;
    }
    internal class TextConverter : IValueConverter
    {

        private readonly IList<IEmojiCache> Cache = new List<IEmojiCache>()
        {
            new BitmapEmojiCache()
            {
                Name = "unknown",
                ImageSource = App.Current.Resources["QuestionIcon"] as ImageSource
            }
        };
        private Image GetImage(ImageSource imageSource) => new Image
        {
            Source = imageSource
        };
        private InlineUIContainer GetEmoji(string emoji)
        {
            emoji = emoji.Substring(1, emoji.Length - 2);
            InlineUIContainer ui = new();

            for (int i = 0; i < Cache.Count; i++)
            {
                var cache = Cache[i];
                if (cache.Name.Equals(emoji, StringComparison.OrdinalIgnoreCase))
                {
                    if (cache is GIFEmojiCache gifCache)
                    {
                        ui.Child = new GifImage()
                        {
                            GifBitmapDecoder = gifCache.GifBitmapDecoder,
                            ToolTip = emoji,
                            AutoStart = true
                        };
                    }
                    else if (cache is BitmapEmojiCache emojiCache)
                    {
                        ui.Child = new Image()
                        {
                            Source = emojiCache.ImageSource,
                            ToolTip = emoji
                        };
                    }
                    return ui;
                }
            }

            string emojiFolderPath = App.GetPathToFolder(Folder.Emoji);
            if (Directory.Exists(emojiFolderPath))
            {
                var files = Directory.GetFiles(emojiFolderPath);
                for (int i = 0; i < files.Length; i++)
                {
                    var file = files[i];
                    var name = file.Split('\\')[^1];
                    StringBuilder sb = new();
                    StringBuilder extension = new();
                    var len = name.Length;
                    for (int j = 0; j < len; j++)
                    {
                        var letter = name[j];
                        int nextL = name[j + 1];
                        if (Char.IsDigit(letter))
                        {
                            if (nextL == '-' || nextL == '_')
                            {
                                continue;
                            }
                        }
                        else
                        {
                            if (letter != '.')
                                sb.Append(letter);
                            else
                            {
                                for (int k = j; k < len; k++)
                                {
                                    extension.Append(name[k]);
                                }
                                break;
                            }
                        }
                    }
                    extension.Remove(0, 1);

                    if (sb[0] == '-' || sb[0] == '_')
                        sb.Remove(0, 1);
                    if (sb.ToString().Equals(emoji, StringComparison.OrdinalIgnoreCase))
                    {

                        IEmojiCache cache;

                        if (extension.ToString() == "gif")
                        {
                            GIFEmojiCache emojiCache = new()
                            {
                                Name = emoji,
                                GifBitmapDecoder = new GifBitmapDecoder(
                                    new Uri(file, UriKind.Absolute),
                                BitmapCreateOptions.PreservePixelFormat,
                                BitmapCacheOption.Default)
                            };
                            ui.Child = new GifImage
                            {
                                GifBitmapDecoder = emojiCache.GifBitmapDecoder,
                                ToolTip = emoji,
                                AutoStart = true
                            };
                            cache = emojiCache;
                        }
                        else
                        {
                            var source = new BitmapImage(new Uri(file))
                            {
                                DecodePixelHeight = 16,
                            };
                            source.Freeze();
                            BitmapEmojiCache emojiCache = new()
                            {
                                Name = emoji,
                                ImageSource = source,
                            };
                            ui.Child = new Image
                            {
                                Source = emojiCache.ImageSource,
                                ToolTip = emoji
                            };
                            cache = emojiCache;
                        }
                        Cache.Add(cache);
                        return ui;
                    }
                }
            }
            ui.Child = new Image
            {
                Source = App.Current.Resources["QuestionIcon"] as ImageSource,
                ToolTip = emoji
            };
            return ui;
        }
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value == null) return null;
            var text = value as string;
            IList<Inline> Inlines = new List<Inline>();
            string textEmoji = string.Empty;
            StringBuilder emojiBuilder = new();

            StringBuilder textBuilder = new();

            int start;
            int inlineIndex = 0;
            for (int i = 0; i < text.Length; i++)
            {
                if (emojiBuilder.Length > 0)
                {
                    if (text[i] == ' ')
                    {
                        emojiBuilder.Clear();
                        continue;
                    }

                    emojiBuilder.Append(text[i]);
                    if (text[i] == ':')
                    {
                        Inlines.Add(GetEmoji(emojiBuilder.ToString()));
                        inlineIndex++;
                        emojiBuilder.Clear();
                    }
                    continue;
                }

                if (text[i] == ':' && text[i + 1] != ' ')
                {
                    Inlines.Add(new Run()
                    {
                        Text = textBuilder.ToString()
                    });
                    inlineIndex++;
                    textBuilder.Clear();

                    emojiBuilder.Append(text[i]);
                }
                else textBuilder.Append(text[i]);
            }
            return Inlines;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
