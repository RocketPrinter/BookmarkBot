﻿// <auto-generated />
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;
using Server.Db;

namespace Server.Db.Migrations
{
    [DbContext(typeof(BookmarkContext))]
    partial class BookmarkContextModelSnapshot : ModelSnapshot
    {
        protected override void BuildModel(ModelBuilder modelBuilder)
        {
#pragma warning disable 612, 618
            modelBuilder
                .HasAnnotation("Relational:MaxIdentifierLength", 63)
                .HasAnnotation("ProductVersion", "5.0.8")
                .HasAnnotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn);

            modelBuilder.Entity("Server.Db.BookmarkContext+Bookmark", b =>
                {
                    b.Property<int>("BookmarkId")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("integer")
                        .HasAnnotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn);

                    b.Property<decimal>("AuthorSnowflake")
                        .HasColumnType("numeric(20,0)");

                    b.Property<decimal>("ChannelSnowflake")
                        .HasColumnType("numeric(20,0)");

                    b.Property<decimal?>("GuildSnowFlake")
                        .HasColumnType("numeric(20,0)");

                    b.Property<decimal>("MessageSnowflake")
                        .HasColumnType("numeric(20,0)");

                    b.Property<string>("MessageSummary")
                        .HasMaxLength(50)
                        .HasColumnType("character varying(50)");

                    b.Property<decimal>("UserSnowflake")
                        .HasColumnType("numeric(20,0)");

                    b.HasKey("BookmarkId");

                    b.HasIndex("MessageSnowflake", "UserSnowflake")
                        .IsUnique();

                    b.ToTable("Bookmarks");
                });
#pragma warning restore 612, 618
        }
    }
}
