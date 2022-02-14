using beta.ViewModels.Base;
using System;

namespace beta.Models
{
    public class NewsItem : ViewModel
    {
        public string Title { get; set; }
        public string Description { get; set; }
        public Uri DestinationUri { get; set; }
        public string Author { get; set; }
        public Uri MediaUri { get; set; }

        private bool _IsMouseOver;
        public bool IsMouseOver
        {
            get=> _IsMouseOver;
            set => Set(ref _IsMouseOver, value);
        }

        private bool _IsSelected;
        public bool IsSelected
        {
            get => _IsSelected;
            set => Set(ref _IsSelected, value);
        }
    }
}
