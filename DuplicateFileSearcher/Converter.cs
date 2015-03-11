using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Data;

namespace DuplicateFileSearcher
{
    [ValueConversion(typeof(string), typeof(string))]
    public class ShorterPathConverter : IValueConverter
    {
        public object Convert(object value, Type targetType,
        object parameter, CultureInfo culture)
        {
            // Do the conversion from bool to visibility
            var s = value as string;
            if (s.Length > 140)
            {
                var separator = System.IO.Path.DirectorySeparatorChar;
                var arr = s.Split(separator);
                for (int i = 1; i < arr.Length - 1; i++)
                    arr[i] = "...";
                return System.IO.Path.Combine(arr);
            }
            return s;
        }

        public object ConvertBack(object value, Type targetType,
            object parameter, CultureInfo culture)
        {
            return string.Empty;
        }
    }

    [ValueConversion(typeof(long), typeof(string))]
    public class FileSizeConverter : IValueConverter
    {
        private static string[] units = new string[] { "B", "KB", "MB", "GB", "TB" };
        
        public object Convert(object value, Type targetType,
                                object parameter, CultureInfo culture)
        {
            var index = 0;
            var _size = (long)value;
            double size = (double)_size;
            while (size > 1024)
            {
                size /= 1024;
                index++;
            }
            return string.Format("{0:0.00} {1}", size, units[index]);
        }

        public object ConvertBack(object value, Type targetType,
                                    object parameter, CultureInfo culture)
        {
            return 0;
        }
    }

    [ValueConversion(typeof(Double), typeof(string))]
    public class DoubleToStringConverter: IValueConverter
    {
        public object Convert(object value, Type targetType,
                                object parameter, CultureInfo culture)
        {
            var size = (double)value;
            return string.Format("{0:0.00}", size);
        }

        public object ConvertBack(object value, Type targetType,
                                    object parameter, CultureInfo culture)
        {
            var s = value as string;
            if(string.IsNullOrEmpty(s))
                return 0.0;
            return System.Convert.ToDouble(s);
        }
    }
}
