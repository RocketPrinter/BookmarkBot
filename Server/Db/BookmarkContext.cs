using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;

namespace Server.Db
{
    public class BookmarkContext : DbContext
    {
        #region Entities
        //entities
        public class UserSnowflake
        {
            public int Id { get; set; }
            public string Title { get; set; }
        }
        #endregion

        public DbSet<UserSnowflake> Test { get; set; }
    }
}
