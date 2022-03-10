using beta.ViewModels.Base;
using System.Collections.Generic;
using System.Windows.Media;

namespace beta.Models
{
    public abstract class Map : ViewModel
    {
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
                var desc = Scenario?["description"];
                if (desc != null)
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
                var size = Scenario?["size"];

                if (size == null) return null;

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
    }

    public class CoopMap : Map
    {
        public override string Name => "Neroxis Map Generator";
        public override string Version => base.Version;
        public CoopMap() => SmallPreview = App.Current.Resources["CoopIcon"] as ImageSource;
    }
    public class NeroxisMap : Map
    {
        public NeroxisMap() => SmallPreview = App.Current.Resources["MapGenIcon"] as ImageSource;
    }
}
