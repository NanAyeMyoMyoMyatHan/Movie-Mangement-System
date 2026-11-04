using System;
using System.Collections.Generic;
using Microsoft.EntityFrameworkCore;

namespace DotNetProject.Database.AppDbModels;

public partial class AppDbContext : DbContext
{
    public AppDbContext()
    {
    }

    public AppDbContext(DbContextOptions<AppDbContext> options)
        : base(options)
    {
    }

    public virtual DbSet<Content> Contents { get; set; }

    public virtual DbSet<ContentGenre> ContentGenres { get; set; }

    public virtual DbSet<ContentType> ContentTypes { get; set; }

    public virtual DbSet<Episode> Episodes { get; set; }

    public virtual DbSet<Genre> Genres { get; set; }

    public virtual DbSet<Movie> Movies { get; set; }

    public virtual DbSet<Rating> Ratings { get; set; }

    public virtual DbSet<User> Users { get; set; }

    public virtual DbSet<WatchlistItem> WatchlistItems { get; set; }

    public virtual DbSet<UserFavorite> UserFavorites { get; set; }

    public virtual DbSet<UserWatchLater> UserWatchLaters { get; set; }

    public virtual DbSet<UserContinueWatching> UserContinueWatchings { get; set; }

    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
    {
        if (!optionsBuilder.IsConfigured)
        {
            optionsBuilder.UseSqlServer("Server=localhost;Database=MyFlixDb;User Id=sa;Password=sasa@123;TrustServerCertificate=True;");
        }
    }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Content>(entity =>
        {
            entity.HasKey(e => e.ContentId).HasName("PK__Content__2907A81E0665AB0A");

            entity.ToTable("Content");

            entity.Property(e => e.BannerUrl)
                .HasMaxLength(500)
                .IsUnicode(false);
            entity.Property(e => e.Country)
                .HasMaxLength(100)
                .IsUnicode(false);
            entity.Property(e => e.CreatedAt).HasDefaultValueSql("(getdate())");
            entity.Property(e => e.Language)
                .HasMaxLength(100)
                .IsUnicode(false);
            entity.Property(e => e.PosterUrl)
                .HasMaxLength(500)
                .IsUnicode(false);
            entity.Property(e => e.Status)
                .HasMaxLength(30)
                .IsUnicode(false)
                .HasDefaultValue("Active");
            entity.Property(e => e.Title)
                .HasMaxLength(255)
                .IsUnicode(false);

            entity.HasOne(d => d.ContentType).WithMany(p => p.Contents)
                .HasForeignKey(d => d.ContentTypeId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_Content_ContentType");
        });

        modelBuilder.Entity<ContentGenre>(entity =>
        {
            entity.HasKey(e => new { e.ContentId, e.GenreId });

            entity.ToTable("ContentGenre");

            entity.HasOne(d => d.Content).WithMany(p => p.ContentGenres)
                .HasForeignKey(d => d.ContentId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_ContentGenre_Content");

            entity.HasOne(d => d.Genre).WithMany(p => p.ContentGenres)
                .HasForeignKey(d => d.GenreId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_ContentGenre_Genre");
        });

        modelBuilder.Entity<ContentType>(entity =>
        {
            entity.HasKey(e => e.ContentTypeId).HasName("PK__ContentT__2026064A6773585F");

            entity.ToTable("ContentType");

            entity.HasIndex(e => e.Name, "UQ_ContentType_Name").IsUnique();

            entity.Property(e => e.CreatedAt).HasDefaultValueSql("(getdate())");
            entity.Property(e => e.Name)
                .HasMaxLength(50)
                .IsUnicode(false);
        });

        modelBuilder.Entity<Episode>(entity =>
        {
            entity.HasKey(e => e.EpisodeId).HasName("PK__Episode__AC6609F5606ECBA7");

            entity.ToTable("Episode");

            entity.HasIndex(e => new { e.ContentId, e.EpisodeNumber }, "UQ_Episode_Content_EpisodeNumber").IsUnique();

            entity.Property(e => e.CreatedAt).HasDefaultValueSql("(getdate())");
            entity.Property(e => e.Status)
                .HasMaxLength(30)
                .IsUnicode(false)
                .HasDefaultValue("Active");
            entity.Property(e => e.ThumbnailUrl)
                .HasMaxLength(500)
                .IsUnicode(false);
            entity.Property(e => e.Title)
                .HasMaxLength(255)
                .IsUnicode(false);
            entity.Property(e => e.VideoUrl)
                .HasMaxLength(500)
                .IsUnicode(false);

            entity.HasOne(d => d.Content).WithMany(p => p.Episodes)
                .HasForeignKey(d => d.ContentId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_Episode_Content");
        });

        modelBuilder.Entity<Genre>(entity =>
        {
            entity.HasKey(e => e.GenreId).HasName("PK__Genre__0385057ED7C96ED8");

            entity.ToTable("Genre");

            entity.HasIndex(e => e.Name, "UQ_Genre_Name").IsUnique();

            entity.Property(e => e.CreatedAt).HasDefaultValueSql("(getdate())");
            entity.Property(e => e.Name)
                .HasMaxLength(100)
                .IsUnicode(false);
        });

        modelBuilder.Entity<Movie>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK__Movies__3214EC07C32E54B7");

            entity.Property(e => e.CreatedAt).HasDefaultValueSql("(getutcdate())");
            entity.Property(e => e.DeletedAt).HasColumnType("datetime");
            entity.Property(e => e.Genre).HasMaxLength(100);
            entity.Property(e => e.PosterUrl).HasMaxLength(500);
            entity.Property(e => e.Title).HasMaxLength(200);
        });

        modelBuilder.Entity<Rating>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK__Ratings__3214EC07A711E7FE");

            entity.HasIndex(e => new { e.UserId, e.MovieId }, "UX_Ratings_UserId_MovieId_Active")
                .IsUnique()
                .HasFilter("([IsDeleted]=(0))");

            entity.Property(e => e.CreatedAt).HasDefaultValueSql("(getutcdate())");
            entity.Property(e => e.UpdatedAt).HasDefaultValueSql("(getutcdate())");

            entity.HasOne(d => d.Movie).WithMany(p => p.Ratings)
                .HasForeignKey(d => d.MovieId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_Ratings_Movies");

            entity.HasOne(d => d.User).WithMany(p => p.Ratings)
                .HasForeignKey(d => d.UserId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_Ratings_Users");
        });

        modelBuilder.Entity<User>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK__Users__3214EC07130933FC");

            entity.HasIndex(e => e.UserName, "UX_Users_UserName_Active")
                .IsUnique()
                .HasFilter("([IsDeleted]=(0))");

            entity.Property(e => e.CreatedAt).HasDefaultValueSql("(getutcdate())");
            entity.Property(e => e.DisplayName).HasMaxLength(150);
            entity.Property(e => e.Email)
                .HasMaxLength(50)
                .IsUnicode(false);
            entity.Property(e => e.Password)
                .HasMaxLength(50)
                .IsUnicode(false);
            entity.Property(e => e.UserName).HasMaxLength(100);
            entity.Property(e => e.Role).HasMaxLength(50).HasDefaultValue("User");
        });

        modelBuilder.Entity<WatchlistItem>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK__Watchlis__3214EC0798EB2021");

            entity.HasIndex(e => new { e.UserId, e.MovieId }, "UX_WatchlistItems_UserId_MovieId_Active")
                .IsUnique()
                .HasFilter("([IsDeleted]=(0))");

            entity.Property(e => e.AddedAt).HasDefaultValueSql("(getutcdate())");
            entity.Property(e => e.UpdatedAt).HasDefaultValueSql("(getutcdate())");

            entity.HasOne(d => d.Movie).WithMany(p => p.WatchlistItems)
                .HasForeignKey(d => d.MovieId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_WatchlistItems_Movies");

            entity.HasOne(d => d.User).WithMany(p => p.WatchlistItems)
                .HasForeignKey(d => d.UserId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_WatchlistItems_Users");
        });

        modelBuilder.Entity<UserFavorite>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.ToTable("UserFavorites");
        });

        modelBuilder.Entity<UserWatchLater>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.ToTable("UserWatchLater");
        });

        modelBuilder.Entity<UserContinueWatching>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.ToTable("UserContinueWatching");
        });

        OnModelCreatingPartial(modelBuilder);
    }

    partial void OnModelCreatingPartial(ModelBuilder modelBuilder);
}
