namespace MusicPlayer.Models
{
    using System.Drawing;

    public class WindowRect
    {
        private Rectangle rect = new Rectangle();

        public WindowRect()
        {
            X = Properties.Settings.Default.X;
            Y = Properties.Settings.Default.Y;
        }

        public int X
        {
            get => rect.Location.X;
            set
            {
                rect.Location = new Point(value, rect.Location.Y);
                Properties.Settings.Default.X = value;
                Properties.Settings.Default.Save();
            }
        }

        public int Y
        {
            get => rect.Location.Y;
            set
            {
                rect.Location = new Point(rect.Location.X, value);
                Properties.Settings.Default.Y = value;
                Properties.Settings.Default.Save();
            }
        }
    }
}
