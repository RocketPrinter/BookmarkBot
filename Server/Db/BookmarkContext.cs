using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using DSharpPlus.Entities;

namespace Server.Db
{
    public class BookmarkContext : DbContext
    {
        #region Entities
        public class User
        {
            [Key]
            public ulong UserSnowflake { get; set; }

            public List<Bookmark> Bookmarks { get; set; }
        }

        public class Bookmark
        {
            public int BookmarkId { get; set; }
            public ulong AuthorSnowflake { get; set; } 
            public ulong ChannelSnowflake { get; set; }
            public ulong MessageSnowflake { get; set; }

            public ulong UserSnowflake { get; set; }
            public User User { get; set; }
        }
        #endregion

        DbSet<User> Users { get; set; }

        public BookmarkContext(DbContextOptions<BookmarkContext> options)
           : base(options)
        { }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<Bookmark>()
                .HasIndex(b => new { b.MessageSnowflake, b.UserSnowflake })
                .IsUnique(true);
        }
    }
}
