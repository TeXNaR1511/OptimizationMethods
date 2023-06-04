using Avalonia.Data.Converters;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OptimizationMethods
{
    public class Converter : IValueConverter
    {
        public object? Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
        {
            
            //string param = parameter as string;
            int a = System.Convert.ToInt32(parameter);
            //System.Diagnostics.Debug.WriteLine(a);
            double x = System.Convert.ToDouble(value, CultureInfo.InvariantCulture);
            //System.Diagnostics.Debug.WriteLine(x);
            //System.Diagnostics.Debug.WriteLine(Math.Round(x, a));
            return Math.Round(x, a).ToString();
        }

        public object? ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
        {
            // If we want to convert back, we need to subtract instead of add.
            return (double?)value - (double?)parameter;
            //double a = GetDoubleValue(parameter, A);

            //double y = GetDoubleValue(value, 0.0);

            //return (y - B) / a;
        }
    }
}