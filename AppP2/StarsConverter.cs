using System.Globalization;
using Microsoft.Maui.Controls;

namespace AppP2.Converters
{
    public class StarsConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is int rating)
            {
                var fullStars = rating;
                var emptyStars = 5 - rating;

                var result = new string('⭐', fullStars) + new string('⭐', emptyStars);
                return result;
            }
            return "⭐⭐⭐⭐⭐";
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}