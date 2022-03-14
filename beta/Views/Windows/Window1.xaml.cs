using beta.Models.Server.Enums;
using System;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Threading;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Input;
using System.Windows.Media;

namespace beta.Views.Windows
{
    /// <summary>
    /// Interaction logic for Window1.xaml
    /// </summary>
    public class Test : IComparable
    {
        public int id { get; set; }
        public bool IsChatModerator { get; set; }
        public PlayerRelationShip RelantionShip { get; set; }
        public string login { get; set; }
        public string name { get; set; }

        public int CompareTo(object obj)
        {
            return id.CompareTo(obj);
        }
    }
    public partial class Window1 : Window, INotifyPropertyChanged
    {
        /// <summary>
        /// Looks for a child control within a parent by name
        /// </summary>
        public static DependencyObject FindChild(DependencyObject parent, string name)
        {
            // confirm parent and name are valid.
            if (parent is null || string.IsNullOrEmpty(name)) return null;

            if ((parent as FrameworkElement)?.Name == name) return parent;
         
            DependencyObject result = null;

           (parent as FrameworkElement)?.ApplyTemplate();

            int childrenCount = VisualTreeHelper.GetChildrenCount(parent);
            for (int i = 0; i < childrenCount; i++)
            {
                var child = VisualTreeHelper.GetChild(parent, i);
                result = FindChild(child, name);
                if (result is not null) break;
            }

            return result;
        }

        /// <summary>
        /// Looks for a child control within a parent by type
        /// </summary>
        public static T FindChild<T>(DependencyObject parent)
            where T : DependencyObject
        {
            // confirm parent is valid.
            if (parent is null) return null;
            if (parent is T) return parent as T;

            DependencyObject foundChild = null;

             (parent as FrameworkElement)?.ApplyTemplate();

            int childrenCount = VisualTreeHelper.GetChildrenCount(parent);
            for (int i = 0; i < childrenCount; i++)
            {
                var child = VisualTreeHelper.GetChild(parent, i);
                foundChild = FindChild<T>(child);
                if (foundChild is not null) break;
            }

            return foundChild as T;
        }

        public string WordFinder2(int requestedLength)
        {
            Random rnd = new Random();
            string[] consonants = { "b", "c", "d", "f", "g", "h", "j", "k", "l", "m", "n", "p", "q", "r", "s", "t", "v", "w", "x", "y", "z" };
            string[] vowels = { "a", "e", "i", "o", "u" };
            string word = "";
            if (requestedLength == 1)
            {
                word = GetRandomLetter(rnd, vowels);
            }
            else
            {
                for (int i = 0; i < requestedLength; i += 2)
                {
                    word += GetRandomLetter(rnd, consonants) + GetRandomLetter(rnd, vowels);
                }
                word = word.Replace("q", "qu").Substring(0, requestedLength); // We may generate a string longer than requested length, but it doesn't matter if cut off the excess.
            }
            return word;
        }
        private static string GetRandomLetter(Random rnd, string[] letters)
        {
            return letters[rnd.Next(0, letters.Length - 1)];
        }

        public event PropertyChangedEventHandler PropertyChanged;
        public ObservableCollection<Test> Titles { get; set; } = new();
        public CollectionViewSource Source { get; } = new();
        public ICollectionView View => Source.View;


        private int _Columns = 1;
        public int Columns
        {
            get => _Columns;
            set
            {
                if (value != _Columns)
                {
                    _Columns = value;
                    PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(Columns)));
                }
            }
        }
        public SortDescription[] SortDescriptions => new SortDescription[]
        {
            new SortDescription(nameof(Test.id), ListSortDirection.Ascending),
            new SortDescription(nameof(Test.login), ListSortDirection.Ascending),
            new SortDescription(nameof(Test.name), ListSortDirection.Ascending)
        };

        private SortDescription _SelectedSort;
        public SortDescription SelectedSort
        {
            get => _SelectedSort;
            set
            {
                if (value != _SelectedSort)
                {
                    _SelectedSort = value;
                    PropertyChanged?.Invoke(this, new(nameof(SelectedSort)));
                    PropertyChanged?.Invoke(this, new(nameof(SortDirection)));
                    //SortDirection = ListSortDirection.Ascending;
                    if (Source.SortDescriptions.Count > 2)
                    {
                        Source.SortDescriptions.Remove(Source.SortDescriptions[^1]);
                        Source.SortDescriptions.Add(value);
                    }
                    else Source.SortDescriptions.Add(value);
                }
            }
        }
        public ListSortDirection SortDirection
        {
            get => SelectedSort.Direction;
            set
            {
                if(SelectedSort.Direction == ListSortDirection.Ascending)
                {
                    SelectedSort = new(SelectedSort.PropertyName, ListSortDirection.Descending);
                }
                else SelectedSort = new(SelectedSort.PropertyName, ListSortDirection.Ascending);
            }
        }

        public ListSortDirection[] ListSortDirections => new ListSortDirection[]
        {
            ListSortDirection.Ascending,
            ListSortDirection.Descending
        };

        private double _ContentVerticalOffset;
        public double ContentVerticalOffset
        {
            get => _ContentVerticalOffset;
            set
            {
                _ContentVerticalOffset = value;
                PropertyChanged?.Invoke(this, new(nameof(ContentVerticalOffset)));
            }
        }
        public ScrollViewer Scroll { get; }
        public Window1()
        {
            InitializeComponent();
            Scroll = FindChild<ScrollViewer>(ListBox);

            Source.GroupDescriptions.Add(new PropertyGroupDescription(nameof(Test.IsChatModerator)));
            Source.SortDescriptions.Add(new(nameof(Test.IsChatModerator), ListSortDirection.Descending));
            Source.SortDescriptions.Add(new(nameof(Test.RelantionShip), ListSortDirection.Descending));

            DataContext = this;
            Source.Source = Titles;
            new Thread(() =>
            {
                int i = 0;
                while (true)
                {
                    if (i == 100) break;
                    i++;
                    Dispatcher.Invoke(() => Titles.Add(new()
                    {
                        id = new Random().Next(0, 10),
                        login = WordFinder2(10),
                        name = "test " + new Random().Next(-100, 1000),
                        IsChatModerator = new Random().Next(-10, 10) == 1,
                        RelantionShip = new Random().Next(-10, 10) switch
                        {
                            -1 => PlayerRelationShip.Foe,
                            1 => PlayerRelationShip.Friend,
                            _ => PlayerRelationShip.None
                        }
                    }));
                    Thread.Sleep(250);
                }
            }).Start();
        }

        private void ListBox_SizeChanged(object sender, SizeChangedEventArgs e)
        {
            if (e.NewSize.Width < 600) Columns = 1;
            else Columns = Convert.ToInt32(e.NewSize.Width / 300);
        }

        private void ListBox_PreviewMouseWheel(object sender, System.Windows.Input.MouseWheelEventArgs e)
        {
            e.Handled = true;
        }

        private void ListBox_MouseWheel(object sender, MouseWheelEventArgs e)
        {

        }
    }
}
