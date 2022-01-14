using System;
using System.Windows;
using System.Windows.Data;

namespace beta.Infrastructure.Converters
{
    public class PointXConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            double xValue = (double)value;
            double yValue = System.Convert.ToDouble(parameter);
            return new Point(xValue, yValue);
        }
        public object ConvertBack(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }

    public class PointXExtendedConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            double xValue = (double)value;
            double yValue = 0;
            var data = ((string)parameter).Split(" ");
            if (data.Length > 1)
            {
                char condition = data[1][0];
                //char targetValue = data[1][1];
                string param = data[1].Substring(1, data[1].Length - 1);
                if (condition == '+')
                    xValue += double.Parse(param);
                //if(targetValue == 'x')
                //else yValue += double.Parse(param);
                else xValue -= double.Parse(param);
            }

            yValue = double.Parse(data[0]);
            return new Point(xValue, yValue);
        }

        public object ConvertBack(object value, Type targetType, object parameter,
            System.Globalization.CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }

    public class PointYExtendedConverter : IValueConverter 
    {
        public object Convert(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            double yValue = (double)value;
            double xValue = 0;
            var data = ((string)parameter).Split(" ");
            if (data.Length > 1)
            {
                char condition = data[1][0];
                //char targetValue = data[1][1];
                string param = data[1].Substring(1, data[1].Length - 1);
                if (condition == '+')
                    yValue += double.Parse(param);
                //if(targetValue == 'x')
                //else yValue += double.Parse(param);
                else yValue -= double.Parse(param);
            }
            xValue = double.Parse(data[0]);
            return new Point(xValue, yValue);
        }
        public object ConvertBack(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
    
    public class PointYConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            double yValue = (double)value;
            double xValue = System.Convert.ToDouble(parameter);
            return new Point(xValue, yValue);
        }
        public object ConvertBack(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
    
    public class DoubleConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            double progressBarValue = (double)value;
            double yValue = System.Convert.ToDouble(parameter);
            return progressBarValue;
        }
        public object ConvertBack(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
