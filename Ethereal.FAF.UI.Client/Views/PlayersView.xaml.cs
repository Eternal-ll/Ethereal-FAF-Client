using Ethereal.FAF.UI.Client.Infrastructure.Commands.Base;
using Ethereal.FAF.UI.Client.ViewModels;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Input;
using System.Windows.Media;
using Wpf.Ui.Common.Interfaces;

namespace Ethereal.FAF.UI.Client.Views
{
    /// <summary>
    /// Interaction logic for PlayersView.xaml
    /// </summary>
    public partial class PlayersView : INavigableView<PlayersViewModel>
    {
        private readonly ScrollViewer OriginalScrollViewer;
        public PlayersView(PlayersViewModel model)
        {
            ViewModel = model;
            InitializeComponent();
            Resources["OpenPrivateCommand"] = model.OpenPrivateCommand;


            OriginalScrollViewer = FindChild<ScrollViewer>(OriginalSource);
        }

        public void EnableInvitePlayerCommand(ICommand inviteCommand, ICommand backCommand)
        {
            Resources["InvitePlayerCommand"] = inviteCommand;
            Resources["BackCommand"] = backCommand;
        }
        public void DisableInvitePlayerCommand()
        {
            Resources.Remove("InvitePlayerCommand");
            Resources.Remove("BackCommand");
        }

        public PlayersViewModel ViewModel { get; }

        //private void GroupedSource_SelectionChanged(object sender, SelectionChangedEventArgs e)
        //{
        //    foreach (var item in e.AddedItems)
        //    {
        //        OriginalSource.ScrollIntoView(item);
        //        break;
        //    }
        //}

        private void ListBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (!GroupsSource.IsMouseCaptured || ViewModel.Groups is null) return;
            foreach (var item in e.AddedItems)
            {
                var group = (CollectionViewGroup)item;
                var first = (Player)group.Items[0];
                if (!group.Items.Cast<Player>().Any(p => p.Id == ViewModel.SelectedPlayer?.Id))
                {
                    ViewModel.SelectedPlayer = first;
                }
                //ViewModel.SelectedGroup = group;
                //GroupedSource.ScrollIntoView(player);
                OriginalSource.ScrollIntoView(first);
                break;
            }
        }
        private void OriginalSource_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (!OriginalSource.IsMouseCaptured || ViewModel.Groups is null) return;
            foreach (var item in e.AddedItems)
            {
                var player = (Player)item;
                var group = ViewModel.Groups
                    .Cast<CollectionViewGroup>()
                    .First(g => g.Items.Cast<Player>().Any(p => p.Id == player.Id));
                ViewModel.SelectedGroup = group;
                //GroupedSource.ScrollIntoView(player);
                break;
            }
        }

        private void OriginalSource_ScrollChanged(object sender, ScrollChangedEventArgs e)
        {
            if (e.VerticalChange == 0 || ViewModel.Groups is null) return;
            var stackpanel = FindChild<VirtualizingStackPanel>(OriginalScrollViewer);
            var latest = FindLatestChild<ListBoxItem>(stackpanel, 1);
            
            var player = (Player)latest.DataContext;
            var group = ViewModel.Groups
                .Cast<CollectionViewGroup>()
                .FirstOrDefault(g => g.Items.Cast<Player>().Any(p => p.Id == player.Id));
            ViewModel.SelectedGroup = group;
            if (group is null) return;
            ViewModel.SelectedPlayer ??= (Player)group.Items.First();
        }


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
        public T FindLatestChild<T>(DependencyObject parent, int offset = 0)
            where T : DependencyObject
        {
            // confirm parent is valid.
            if (parent is null) return null;
            if (parent is T) return parent as T;

            DependencyObject foundChild = null;

            (parent as FrameworkElement)?.ApplyTemplate();

            int childrenCount = VisualTreeHelper.GetChildrenCount(parent);
            for (int i = 0; i < childrenCount - offset; i++)
            {
                var child = VisualTreeHelper.GetChild(parent, i);
                var found = FindChild<T>(child);
                if (found is not null)
                {
                    foundChild = found;
                }
            }

            return foundChild as T;
        }
    }
}
