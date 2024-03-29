﻿namespace MusicPlayer.Models
{
    using System.Collections.Generic;
    using System.Data.SQLite;
    using System.IO;
    using System.Linq;
    using Microsoft.Data.Sqlite;
    using Microsoft.EntityFrameworkCore;

    public class HistoryDbContext : DbContext, IDatabase
    {
        public DbSet<History> Histories { get; set; }

        public List<History> List => Histories.ToList();

        public void Write(History history)
        {
            var f = Histories.Where(h => history.DirectoryName == h.DirectoryName).FirstOrDefault(h => history.FullName == h.FullName);

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
