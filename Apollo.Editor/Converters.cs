using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Data;
using System.Windows.Media;

namespace Apollo.Editor
{
    public class PathToNameConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (parameter != null && parameter is bool && (bool)parameter == true)
                return System.IO.Path.GetFileName(value as string);
            else
                return System.IO.Path.GetFileNameWithoutExtension(value as string);
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }

    public class PathToDirectoryConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return System.IO.Path.GetDirectoryName(value as string);
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }

    public class ReverseBoolConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return !((bool)value);
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }

    public class ConsoleColorConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value == null) return null;
            return new SolidColorBrush(((ConsoleColor)value).ToColor());
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }

    public class ConsoleLegacyColorConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value == null) return null;
            return new SolidColorBrush(((ConsoleColor)value).ToLegacyColor());
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }

    public class DrawingMediaColorConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value == null) return null;
            if (value is System.Drawing.Color)
                return ((System.Drawing.Color)value).ToMediaColor();
            else
                return ((Color)value).ToDrawingColor();
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value == null) return null;
            if (value is System.Drawing.Color)
                return ((System.Drawing.Color)value).ToMediaColor();
            else
                return ((Color)value).ToDrawingColor();
        }
    }

    public class TagItemConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value == null || App.Current.MainWindow == null || (App.Current.MainWindow as MainWindow).Story == null) return null;
            var res = (App.Current.MainWindow as MainWindow).Story.Tags.FirstOrDefault(x => x.ID == (value as string));
            return res;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value == null) return "";
            return (value as Tag).ID;
        }
    }

    public class TagNameConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value == null || App.Current.MainWindow == null || (App.Current.MainWindow as MainWindow).Story == null) return "";
            var res = (App.Current.MainWindow as MainWindow).Story.Tags.FirstOrDefault(x => x.ID == (value as string));
            return res == null ? "" : res.Name;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }

    public class TagBrushConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value == null || App.Current.MainWindow == null || (App.Current.MainWindow as MainWindow).Story == null) return Brushes.Transparent;
            var res = (App.Current.MainWindow as MainWindow).Story.Tags.FirstOrDefault(x => x.ID == (value as string));
            return res == null ? Brushes.Transparent : new SolidColorBrush(res.Color.ToMediaColor());
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}