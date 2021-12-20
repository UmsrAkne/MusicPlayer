namespace MusicPlayer.Models
{
    using System.Drawing;
    using System.Windows.Forms;

    public class WindowRect
    {
        private Rectangle rect = new Rectangle();

        public WindowRect()
        {
            X = Properties.Settings.Default.X;
            Y = Properties.Settings.Default.Y;

            foreach (Screen scr in Screen.AllScreens)
            {
                if (!scr.WorkingArea.Contains(X, Y))
                {
                    X = 0;
                    Y = 0;
                }
            }
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
