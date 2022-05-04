using beta.Infrastructure.Services.Interfaces;
using beta.Models;
using beta.Resources.Controls;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Text;
using System.Text.RegularExpressions;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;

namespace beta.Infrastructure.Converters
{
    public abstract class EmojiCache
    {
        public string Name { get; set; }
        public Uri PathToFile { get; set; }
    }
    public class GIFEmojiCache : EmojiCache
    {
        public GifBitmapDecoder GifBitmapDecoder;
    }
    public class BitmapEmojiCache : EmojiCache
    {
        public ImageSource ImageSource;
    }
    // TODO: Move to Service!!! This logic should be in some ChatService
    internal class TextConverter : IValueConverter
    {
        private readonly List<EmojiCache> Cache = new()
        {
            new BitmapEmojiCache()
            {
                Name = "unknown",
                ImageSource = App.Current.Resources["QuestionIcon"] as ImageSource
            }
        };

        private readonly IPlayersService PlayersService;
        public TextConverter()
        {
            PlayersService = App.Services.GetService<IPlayersService>();
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
                        EmojiCache cache;
                        Image image;
                        if (extension.ToString() == "gif")
                        {
                            Uri url = new(file, UriKind.Absolute);
                            GIFEmojiCache emojiCache = new()
                            {
                                Name = emoji,
                                PathToFile = url,
                                GifBitmapDecoder = new GifBitmapDecoder(url,
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
                            Uri url = new(file, UriKind.Absolute);
                            BitmapImage source = new();
                            source.BeginInit();
                            source.UriSource = url;
                            source.CacheOption = BitmapCacheOption.OnLoad;
                            source.EndInit();
                            source.Freeze();
                            BitmapEmojiCache emojiCache = new()
                            {
                                Name = emoji,
                                PathToFile = url,
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
            if (value is not string text) return null;
            return ParseText(text);
        }

        private Inline[] ParseText(string text)
        {
            List<Inline> inlines = new();

            bool isOnlyUIContainers = false;
            bool isOnlyImages = false;

            string[] words = text.Split();

            Run run = null;
            InlineUIContainer inlineUIContainer = null;
            bool isText = false;
            for (int i = 0; i < words.Length; i++)
            {
                var word = words[i];
                if (!isText)
                {
                    if (run is not null)
                    {
                        if (i == 0 && run.Text.Trim().Length == 0) continue;
                        if (i > 0)
                        {
                            run.Text = run.Text.Insert(0, " ");
                        }
                        inlines.Add(run);
                    }
                    run = new();
                }
                if (inlineUIContainer is not null)
                {
                    inlines.Add(inlineUIContainer);
                    inlineUIContainer = null;
                }
                // Is URL
                if ((word.StartsWith("https:") || word.StartsWith("http")) && Uri.IsWellFormedUriString(word, UriKind.Absolute)
                    && Uri.TryCreate(word, UriKind.Absolute, out var url))
                {
                    //TODO Add regex for images/GIFs
                    inlineUIContainer = new InlineUIContainer(new Button()
                    {
                        Content = word,
                        Command = App.Current.Resources.MergedDictionaries[2]["NavigateUriCommand"] as ICommand,
                        CommandParameter = url,
                        Style = App.Current.Resources["ButtonLinkStyle"] as Style
                    });
                    isText = false;
                    continue;
                }
                // Is local directory
                if (Regex.IsMatch(word, @"^(?:[a-zA-Z]\:|\\\\[\w\.]+\\[\w.$]+)\\(?:[\w]+\\)*\w([\w.])+$"))
                {
                    inlineUIContainer = new InlineUIContainer(new Button()
                    {
                        Content = word,
                        Command = App.Current.Resources.MergedDictionaries[2]["NavigateExplorerCommand"] as ICommand,
                        CommandParameter = word,
                        Style = App.Current.Resources["ButtonExplorerStyle"] as Style
                    });
                    isText = false;
                    continue;
                }
                // Is player
                if (PlayersService.TryGetPlayer(word, out var player))
                {
                    inlineUIContainer = new InlineUIContainer(new Button()
                    {
                        Content = player,
                        Style = App.Current.Resources["ButtonPlayerStyle"] as Style
                    });
                    isText = false;
                    continue;
                }
                // Is emoji
                if (Regex.IsMatch(word, @"\:.*?\:"))
                {
                    inlineUIContainer = GetEmoji(text);
                    isText = false;
                    continue;
                }
                isText = true;
                run.Text += word + " ";
            }

            if (run is not null)
            {
                inlines.Add(run);
            }
            if (inlineUIContainer is not null)
            {
                inlines.Add(inlineUIContainer);
            }

            return inlines.ToArray();
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
