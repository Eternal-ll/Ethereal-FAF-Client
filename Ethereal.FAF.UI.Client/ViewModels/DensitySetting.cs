namespace Ethereal.FAF.UI.Client.ViewModels
{
    public sealed class DensitySetting : JsonSettingsViewModel
    {
        public DensitySetting(string area) => Area = area;
        public DensitySetting(){}
        public string Area { get; set; }
        private bool _IsEnabled;
        public bool IsEnabled
        {
            get => _IsEnabled;
            set => Set(ref _IsEnabled, value, $"MapGenerator:Config:MapDensities:{Area}:IsEnabled");
        }
        private double _Value;
        public double Value
        {
            get => _Value;
            set => Set(ref _Value, value, $"MapGenerator:Config:MapDensities:{Area}:Value");
        }
    }
}
