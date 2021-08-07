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
        [Index(nameof(MessageSnowflake),nameof(UserSnowflake),IsUnique = true)]
        public class Bookmark
        {
            public int BookmarkId { get; set; }
            public ulong UserSnowflake { get; set; }

            public ulong? GuildSnowFlake { get; set; }
            public ulong ChannelSnowflake { get; set; }
            public ulong MessageSnowflake { get; set; }

            public ulong AuthorSnowflake { get; set; }
            [MaxLength(50)]
            public string MessageSummary { get; set; }
        }
        #endregion

        public DbSet<Bookmark> Bookmarks { get; set; }

        public BookmarkContext(DbContextOptions<BookmarkContext> options)
           : base(options)
        { }
    }
}
