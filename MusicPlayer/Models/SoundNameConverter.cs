namespace MusicPlayer.Models
{
    using System;
    using System.Collections.Generic;
    using System.Collections.ObjectModel;
    using System.Globalization;
    using System.IO;
    using System.Linq;
    using System.Text;
    using System.Threading.Tasks;
    using System.Windows.Data;

    public class SoundNameConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            var sounds = (ObservableCollection<ISound>)value;

            if (sounds.Count <= 1)
            {
                return string.Join("", sounds.Select(s => Path.GetFileName(s.URL)).ToArray());
            }
            else
            {
                return $"{Path.GetFileName(sounds[0].URL)} >>> {Path.GetFileName(sounds[1].URL)}";
            }

        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
