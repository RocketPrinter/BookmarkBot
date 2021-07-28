using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;

namespace Server.Db
{
    public class BookmarkContext : DbContext
    {
        #region Entities

        #endregion

        public BookmarkContext(DbContextOptions<BookmarkContext> options)
           : base(options)
        { }


    }
}
