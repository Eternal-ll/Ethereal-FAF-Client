using beta.Infrastructure.Utils;
using beta.ViewModels.Base;
using System.Collections.Generic;
using System.Windows.Media;
using System.Windows.Media.Imaging;

namespace beta.Models
{
    public abstract class Map : ViewModel
    {
        public virtual ImageSource NewPreview { get; }

        #region SmallPreview
        public ImageSource _SmallPreview;
        public ImageSource SmallPreview
        {
            get => _SmallPreview;
            set => Set(ref _SmallPreview, value);
        } 
        #endregion

        public virtual string Name { get; }
        public virtual string Version { get; }
        public string OriginalName { get; set; }
    }
    public class GameMap : Map
    {
        public GameMap(string originalName) => OriginalName = originalName;

        #region ImageSource
        private byte[] _ImageSource;
        public byte[] ImageSource
        {
            get => _ImageSource;
            set
            {
                if (Set(ref _ImageSource, value))
                {
                    OnPropertyChanged(nameof(NewPreview));
                    if (value is not null)
                    {
                        IsPreviewLoading = false;
                    }
                }
            }
        }
        #endregion

        private BitmapImage BitmapImage;
        public override ImageSource NewPreview
        {
            get
            {
                if (IsPreviewLoading) return null;
                if (ImageSource is null)
                {
                    return App.Current.Resources["QuestionIcon"] as ImageSource;
                }
                if (ImageSource.Length == 0)
                {
                    return App.Current.Resources["QuestionIcon"] as ImageSource;
                }
                if (ImageSource.Length == 1)
                {
                    return App.Current.Resources["MapGenIcon"] as ImageSource;
                }
                return BitmapImage ??= ImageSource.ToBitmapImage();
            }
        }

        #region IsPreviewLoading
        private bool _IsPreviewLoading = true;
        public bool IsPreviewLoading
        {
            get => _IsPreviewLoading;
            set => Set(ref _IsPreviewLoading, value);
        }
        #endregion

        #region Scenario
        private Dictionary<string, string> _Scenario;
        public Dictionary<string, string> Scenario
        {
            get => _Scenario;
            set
            {
                if (Set(ref _Scenario, value))
                {
                    OnPropertyChanged(nameof(Name));
                    OnPropertyChanged(nameof(Description));
                    OnPropertyChanged(nameof(Size));
                    OnPropertyChanged(nameof(Version));
                    OnPropertyChanged(nameof(IsAdaptive));
                    OnPropertyChanged(nameof(MexesCount));
                    OnPropertyChanged(nameof(HydrosCount));
                    OnPropertyChanged(nameof(Type));
                }
            }
        }
        #endregion

        #region Scenario properties
        public override string Name => Scenario?["name"];
        public string Description
        {
            get
            {
                if (Scenario?["description"] is string desc)
                {
                    var isArr = desc.IndexOf('>');
                    if (isArr != -1)
                        desc = desc[(isArr + 1)..];
                    return desc;
                }
                return null;
            }
        }
        public string Size
        {
            get
            {
                if (Scenario?["size"] is not string size) return null;

                var sizes = size.Replace(" ", string.Empty).Split(',');

                if (sizes.Length == 2)
                    if (int.TryParse(sizes[0], out int width))
                    {
                        size = (width / 51.2).ToString() + " x ";
                        if (sizes[0] == sizes[1])
                        {
                            size += size.Split()[0] + " km";
                        }
                        else
                        {
                            if (int.TryParse(sizes[1], out int height))
                            {
                                size += (height / 51.2).ToString() + " km";
                            }
                        }
                    }
                return size;
            }
        }
        public override string Version => Scenario?["map_version"];
        public bool IsAdaptive => bool.TryParse(Scenario?["AdaptiveMap"], out bool isAdaptive) ? isAdaptive : false;
        public string MexesCount => Scenario?["Mass"];
        public string HydrosCount => Scenario?["Hydrocarbon"];
        public string Type => Scenario?["type"];
        #endregion

        // for future
        public bool IsLegacy { get; set; }

        //public bool IsRanked = false;

        //public IPlayer Author = null;

        //public double AverageRating = 0;
        /*
          public Review[] Reviews

        */
        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                if (BitmapImage is not null)
                {
                    BitmapImage.StreamSource.Close();
                    BitmapImage.StreamSource.Dispose();
                    BitmapImage = null;
                }
                _ImageSource = null;
            }
            base.Dispose(disposing);
        }
    }

    public class NeroxisMap : Map
    {
        public override string Name => "Neroxis Map Generator";
        public override string Version => base.Version;
        public override ImageSource NewPreview => App.Current.Resources["MapGenIcon"] as ImageSource;
        public NeroxisMap(string originalName) => OriginalName = originalName;
    }
    public class CoopMap : Map
    {
        public override ImageSource NewPreview => App.Current.Resources["CoopIcon"] as ImageSource;
        public CoopMap(string originalName) => OriginalName = originalName;
    }
}
