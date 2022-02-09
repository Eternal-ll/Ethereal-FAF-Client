using beta.Infrastructure.Services.Interfaces;
using beta.Models;
using beta.Resources.Controls;
using Microsoft.Extensions.DependencyInjection;
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

        private readonly ILobbySessionService LobbySessionService;
        public TextConverter()
        {
            LobbySessionService = App.Services.GetService<ILobbySessionService>();
        }

        private InlineUIContainer GetEmoji(string emoji)
        {
            emoji = emoji.Substring(1, emoji.Length - 2);
            InlineUIContainer ui = new();

            for (int i = 0; i < Cache.Count; i++)
            {
                var cache = Cache[i];
                if (cache.Name.Equals(emoji, StringComparison.OrdinalIgnoreCase))
                {
                    Image image;
                    if (cache is GIFEmojiCache gifCache)
                    {
                        image = new GifImage()
                        {
                            GifBitmapDecoder = gifCache.GifBitmapDecoder,
                            AutoStart = true,
                        };
                    }
                    else
                    {
                        var emojiCache = (BitmapEmojiCache)cache;
                        image = new Image()
                        {
                            Source = emojiCache.ImageSource
                        };
                    }

                    image.DataContext = image;
                    image.ToolTip = emoji;
                    image.Stretch = Stretch.Uniform;
                    image.MaxHeight = 24;
                    image.Margin = new(2, 0, 2, 0);

                    ui.Child = image;
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
                        Image image;
                        if (extension.ToString() == "gif")
                        {
                            GIFEmojiCache emojiCache = new()
                            {
                                Name = emoji,
                                GifBitmapDecoder = new GifBitmapDecoder(
                                    new Uri(file, UriKind.Absolute),
                                BitmapCreateOptions.PreservePixelFormat,
                                BitmapCacheOption.Default),
                            };
                            image = new GifImage
                            {
                                GifBitmapDecoder = emojiCache.GifBitmapDecoder,
                                AutoStart = true,
                            };
                            cache = emojiCache;
                        }
                        else
                        {
                            var source = new BitmapImage(new Uri(file));
                            source.DecodePixelHeight = 34;
                            source.Freeze();
                            BitmapEmojiCache emojiCache = new()
                            {
                                Name = emoji,
                                ImageSource = source,
                            };

                            image = new Image()
                            {
                                Source = emojiCache.ImageSource
                            };
                            cache = emojiCache;
                        }
                        image.DataContext = image;
                        image.ToolTip = emoji;
                        image.Stretch = Stretch.Uniform;
                        image.MaxHeight = 24;
                        image.Margin = new(2, 0, 2, 0);

                        ui.Child = image;
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

            StringBuilder textBuilder = new();

            StringBuilder sb = new();

            var len = text.Length;

            bool anyText = false;

            for (int i = 0; i < len; i++)
            {
                var letter = text[i];
                if (letter == ':')
                {
                    sb.Append(letter);
                    Char innerL = '0';
                    while (innerL != ':' && i < len - 1)
                    {
                        i++;
                        innerL = text[i];

                        if (innerL == ' ')
                        {
                            textBuilder.Append(sb);
                            sb.Clear();
                            break;
                        }
                        sb.Append(innerL);
                    }
                    if (sb.Length <= 2)
                    {
                        textBuilder.Append(sb);
                        sb.Clear();
                    }
                    if (textBuilder.Length > 0)
                    {
                        Inlines.Add(new Run()
                        {
                            Text = textBuilder.ToString()
                        });
                        textBuilder.Clear();
                        anyText = true;
                    }

                    if (sb.Length > 2)
                    {
                        Inlines.Add(GetEmoji(sb.ToString()));
                        sb.Clear();
                    }
                }
                else if (letter == '@')
                {
                    sb.Append(letter);
                    Char innerL = '0';
                    while (innerL != ' ' && i < len - 1)
                    {
                        i++;
                        innerL = text[i];
                        if (innerL == '@')
                        {
                            i--;
                            break;
                        }
                        sb.Append(innerL);
                    }

                    if (sb.Length <= 2)
                    {
                        textBuilder.Append(sb);
                        sb.Clear();
                    }

                    if (textBuilder.Length > 0)
                    {
                        Inlines.Add(new Run()
                        {
                            Text = textBuilder.ToString()
                        });
                        textBuilder.Clear();
                        anyText = true;
                    }

                    if (sb.Length > 2)
                    {
                        var login = sb.ToString().Substring(1).Replace(" ", "");
                        var player = LobbySessionService.GetPlayerInfo(login);
                        if (player != null)
                        {
                            Inlines.Add(new InlineUIContainer()
                            {
                                Child = new Button()
                                {
                                    DataContext = player
                                }
                            });
                            anyText = true;
                        }
                        else
                        {
                            Inlines.Add(new Run()
                            {
                                Text = sb.ToString()
                            });
                            anyText = true;
                        }
                        sb.Clear();
                    }
                }
                else textBuilder.Append(letter);
            }

            if (textBuilder.Length > 0)
            {
                Inlines.Add(new Run()
                {
                    Text = textBuilder.ToString()
                });
                textBuilder.Clear();
                anyText = true;
            }
            if (!anyText)
                for (int i = 0; i < Inlines.Count; i++)
                {
                    var inline = (InlineUIContainer)Inlines[i];
                    var image = (Image)inline.Child;
                    image.MaxHeight = 34;
                    image.Margin = new(2, 4, 2, -4);
                }
            return Inlines;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
