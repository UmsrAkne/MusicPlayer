namespace MusicPlayer.Models
{
    using System.Data.SQLite;
    using System.IO;
    using System.Linq;
    using Microsoft.Data.Sqlite;
    using Microsoft.EntityFrameworkCore;

    public class HistoryDbContext : DbContext
    {
        public DbSet<History> Histories { get; set; }

        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            if (!File.Exists("History.sqlite"))
            {
                SQLiteConnection.CreateFile("History.sqlite");
            }

            var connectionString = new SqliteConnectionStringBuilder { DataSource = @"History.sqlite" }.ToString();
            optionsBuilder.UseSqlite(new SqliteConnection(connectionString));
        }

        public void write(History history)
        {
            var f = Histories.FirstOrDefault(h => history.FullName == h.FullName);

            if (f == null)
            {
                Histories.Add(history);
            }
            else
            {
                f.FullName = history.FullName;
                f.LastListenDate = history.LastListenDate;
                f.ListenCount++;
            }

            SaveChanges();
        }
    }
}
