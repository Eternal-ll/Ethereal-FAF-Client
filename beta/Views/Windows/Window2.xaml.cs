using beta.Infrastructure.Commands;
using beta.Models;
using System;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.IO;
using System.Net;
using System.Text;
using System.Threading;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

namespace beta.Views.Windows
{
    /// <summary>
    /// Interaction logic for Window2.xaml
    /// </summary>
    public partial class Window2 : Window, INotifyPropertyChanged
    {
        //NEWS format
        //https://direct.faforever.com/wp-json/wp/v2/posts?per_page=1&page=1&_embed=author,wp:featuredmedia&_fields[]=title&_fields[]=content&_fields[]=newshub_externalLinkUrl&_fields[]=_links&_fields[]=_links
        StreamReader g;
        public Window2()
        {
            InitializeComponent();

            new Thread(() =>
            {
                WebRequest web = WebRequest.Create("https://direct.faforever.com/wp-json/wp/v2/posts?per_page=20&page=1&_embed=author,wp:featuredmedia&_fields[]=title&_fields[]=content&_fields[]=newshub_externalLinkUrl&_fields[]=_links&_fields[]=_links");
                var result = web.GetResponse();
                g = new StreamReader(result.GetResponseStream());
                Test();
            }).Start();
            DataContext = this;
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            Test();
        }
        public ObservableCollection<NewsItem> News { get; set; } = new();
        private void Test()
        {
            //Dispatcher.Invoke(() => ListBox.Items.Clear());
            char[] buffer = new char[1];
            g.Read(buffer, 0, 1);
            StringBuilder main = new();
            StringBuilder word = new();
            bool found = false;
            string type = "";
            int i = 0;
            var news = new NewsItem[200];
            Uri previous = null;
            while (g.Read(buffer, 0, 1) != 0)
            {
                if (word.Length > 0)
                {
                    if (buffer[0] == '"')
                    {
                        var text = word.ToString().Substring(1);
                        if (found && text.Length > 0)
                        {
                            if (text == "embeddable" || text == "caption" || text == "orientation" || text == "date")
                            {
                                found = false;
                                word.Clear();
                                continue;
                            }
                            if (text == "rendered" || text == "author" || text == "name" || text == "id")
                            {
                                word.Clear();
                                continue;
                            }
                            word.Clear();
                            switch (type)
                            {
                                case "title":
                                    if (news[i] == null)
                                    news[i] = new()
                                    {
                                        Title = text
                                    };
                                    break;
                                case "content"://\\n<p>   <\/p>\n
                                    if(text.Length > 14)
                                    news[i].Description = text.Substring(5, text.Length -13).Trim();
                                    break;
                                case "newshub_externalLinkUrl": news[i].DestinationUri = new(text.Replace("\\/", "/"));
                                    break;
                                case "_embedded": news[i].Author = text;
                                    break;
                                case "source_url":
                                    if (news[i] == null)
                                    {
                                        if (previous == null)
                                        {
                                            previous = new(text.Replace("\\/", "/"));
                                        }
                                        if (previous != null)
                                        {
                                            news[i - 1].MediaUri = previous;
                                            previous = new(text.Replace("\\/", "/"));
                                            found = false;
                                            previous = null;
                                            continue;
                                        }
                                    }
                                    else news[i].MediaUri = new(text.Replace("\\/", "/"));
                                    i++;
                                    break;
                            }
                            found = false;
                        }

                        if (text == "title" || text == "content" || text == "newshub_externalLinkUrl" || text == "_embedded" || text == "wp:featuredmedia" || text == "source_url")
                        {
                            type = text;
                            found = true;
                        }
                        word.Clear();
                        continue;
                    }
                    word.Append(buffer[0]);
                    continue;
                }
                if (buffer[0] == '"')
                {
                    word.Append(buffer[0]);
                }
            }


            
            for (int j = 0; j < news.Length; j++)
            {
                if (news[j] != null)
                    Dispatcher.Invoke(() =>
                    News.Add(news[j]));
            }
            //Dispatcher.Invoke(() => ListBox.ItemsSource = news);
        }

        public event PropertyChangedEventHandler PropertyChanged;


        private ICommand _TestCommand;
        public ICommand TestCommand => _TestCommand ??= new LambdaCommand(OnTestCommand, CanTestCommand);
        private bool CanTestCommand(object parameter) => true;


        private Border _PreviousItem;
        public Border PreviousItem
        {
            get => _PreviousItem;
            set
            {
                _PreviousItem = value;
                PropertyChanged?.Invoke(this, new(nameof(PreviousItem)));
            }
        }

        private void OnTestCommand(object parameter)
        {
            if (parameter == null) return;

            var item = (Border)parameter;


            //if (PreviousItem != null)
            //{
            //    if (item == PreviousItem) return;
            //    PreviousItem.UpdateLayout();
            //    PreviousItem.Resources = item.Resources;
            //    PreviousItem.UpdateLayout();
            //}
            //item.Resources.Clear();
            //PreviousItem = item;
        }
    }
}
