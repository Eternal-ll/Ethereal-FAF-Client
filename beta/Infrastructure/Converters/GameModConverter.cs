using beta.Models.Server;
using System;
using System.Globalization;
using System.Windows;
using System.Windows.Data;

namespace beta.Infrastructure.Converters
{
    public class GameModConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            if (value == null) return "Unknown";
            string mod = string.Empty;
            if (value is GameInfoMessage gameInfoMessage) mod = gameInfoMessage.featured_mod;
            else mod = value.ToString();
            int length = mod.Length;
            return length switch
            {
                3 => "FAF",
                4 => mod[0] == 'c' ? "Coop" : "King of the Hill",//koth
                6 => "Nomads",
                7 => mod[0] == 'f' ? "FAF Beta" : "LabWars", //fafbeta
                8 => "Phantom-X",
                10 => mod[0] == 'f' ? "FAF Develop": "Extreme Wars",
                11 => "Murder Party",
                14 => "Claustrophobia",
                _ => "Unknown"
            };
        }
        public object ConvertBack(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }

    
    public class IsInternalGroupSelectedConverter : IMultiValueConverter
    {
        public object Convert(object[] values, Type targetType, object parameter, CultureInfo culture)
        {
            if (values[0] == null | values[1] == null) return Visibility.Visible;
            var currentGroup = values[1].ToString();
            var t = values[0].ToString();
            if (values[0].ToString()=="{DependencyProperty.UnsetValue}") return Visibility.Collapsed;
            var selectedGroups = (string[])values[0];
            for (int i = 0; i < selectedGroups.Length; i++)
            {
                var selected = selectedGroups[i];
                if (selected == currentGroup)
                    return Visibility.Visible;
            }
            return Visibility.Collapsed;
        }

        public object[] ConvertBack(object value, Type[] targetTypes, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
    public class MoreThanConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            int num = (int)value;
            if (parameter == null)
                return num > 0;
            return num > int.Parse(parameter.ToString());
        }
        public object ConvertBack(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
    public class TextLongerThanConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            int num = value.ToString().Length;
            if (parameter == null)
                return num > 0;
            return num > int.Parse(parameter.ToString());
        }
        public object ConvertBack(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}