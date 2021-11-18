namespace MusicPlayer.Models
{
    using System.Data.SQLite;
    using System.IO;
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
    }
}
