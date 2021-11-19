namespace MusicPlayer.Models
{
    using System;
    using System.ComponentModel.DataAnnotations;
    using System.ComponentModel.DataAnnotations.Schema;

    public class History
    {
        [Key]
        [Required]
        [Column("id")]
        public int Id { get; set; }

        [Required]
        [Column("full_name")]
        public string FullName { get; set; }

        [Required]
        [Column("directory_name")]
        public string DirectoryName { get; set; }

        [Required]
        [Column("last_listen_date")]
        public DateTime LastListenDate { get; set; }

        [Required]
        [Column("listen_count")]
        public int ListenCount { get; set; } = 1;
    }
}
