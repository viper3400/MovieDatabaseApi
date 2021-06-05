using Microsoft.EntityFrameworkCore;
using Jaxx.VideoDb.Data.DatabaseModels;
using Jaxx.WebApi.Shared;

namespace Jaxx.VideoDb.Data.Context
{
    public class VideoDbContext : DbContext
    {
        private readonly string _viewGroup;
        private readonly string _userName;

        public VideoDbContext(DbContextOptions<VideoDbContext> options, IUserContextInformationProvider userContextProvider)
        : base(options)
        {
            _viewGroup = userContextProvider.GetViewGroup();
            _userName = userContextProvider.UserName;
        }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<videodb_actors>()
                .ToTable("videodb_actors")
                .HasKey(t => t.actorid);

            modelBuilder.Entity<videodb_cache>()
                .ToTable("videodb_cache")
                .HasKey(t => t.tag);

            modelBuilder.Entity<videodb_config>()
                .ToTable("videodb_config")
                .HasKey(t => t.opt);

            modelBuilder.Entity<videodb_videogenre>()
                .ToTable("videodb_videogenre")
                .HasKey(t => new { t.video_id, t.genre_id });

            modelBuilder.Entity<videodb_genres>()
                .ToTable("videodb_genres");

            modelBuilder.Entity<videodb_lent>()
                .ToTable("videodb_lent")
                .HasKey(t => t.diskid);

            modelBuilder.Entity<videodb_mediatypes>()
                .ToTable("videodb_mediatypes");

            modelBuilder.Entity<videodb_permissions>()
                .ToTable("videodb_permissions")
                .HasKey(t => new { t.from_uid, t.to_uid });

            modelBuilder.Entity<videodb_userconfig>()
                .ToTable("videodb_userconfig")
                .HasKey(t => new { t.user_id, t.opt });

            modelBuilder.Entity<videodb_users>()
                .ToTable("videodb_users");

            modelBuilder.Entity<videodb_userseen>()
                .ToTable("videodb_userseen")
                .HasKey(t => new { t.user_id, t.video_id });

            modelBuilder.Entity<videodb_videodata>()
                .ToTable("videodb_videodata");

            modelBuilder.Entity<videodb_videodata>()
                .Property(p => p.plot).HasColumnType("text");

            modelBuilder.Entity<homewebbridge_userseen>()
                .ToTable("homewebbridge_userseen");

            modelBuilder.Entity<homewebbridge_userseen>()
                .HasQueryFilter(s => EF.Property<string>(s, "asp_viewgroup") == _viewGroup);

            // for information about global query filters see
            // - http://gunnarpeipman.com/net/ef-core-global-query-filters/
            // - https://docs.microsoft.com/en-us/ef/core/querying/filters

            modelBuilder.Entity<homewebbridge_usermoviesettings>()
               .ToTable("homewebbridge_usermoviesettings");

            /* The context filters HomeWebUserMovieSettings for current user in a global filter query
            /* Filters may be disabled for individual LINQ queries by using the IgnoreQueryFilters() operator.
            /* see: https://docs.microsoft.com/en-us/ef/core/querying/filters
            */
            modelBuilder.Entity<homewebbridge_usermoviesettings>()
                .HasQueryFilter(s => EF.Property<string>(s, "asp_username") == _userName);

            modelBuilder.Entity<homewebbridge_inventory>()
                .ToTable("homewebbridge_inventory");

            modelBuilder.Entity<homewebbridge_inventorydata>()
                .ToTable("homewebbridge_inventorydata");

            MapForeignKeys(modelBuilder);
        }

        private void MapForeignKeys(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<videodb_videodata>()
                .HasOne(o => o.VideoOwner)
                .WithMany(p => p.UserVideos)
                .HasForeignKey(f => f.owner_id);

            modelBuilder.Entity<videodb_users>()
                .HasMany(v => v.UserVideos)
                .WithOne(u => u.VideoOwner);

            modelBuilder.Entity<videodb_videogenre>()
                .HasKey(vg => new { vg.genre_id, vg.video_id });

            modelBuilder.Entity<videodb_videogenre>()
                .HasOne(vg => vg.Video)
                .WithMany(g => g.VideoGenres)
                .HasForeignKey(vg => vg.video_id);

            modelBuilder.Entity<videodb_videogenre>()
                .HasOne(vg => vg.Genre)
                .WithMany(v => v.VideosForGenre)
                .HasForeignKey(vg => vg.genre_id);

            modelBuilder.Entity<videodb_videodata>()
                .HasMany(k => k.SeenInformation)
                .WithOne(s => s.MovieInformation)
                .HasForeignKey(k => k.vdb_videoid);

            modelBuilder.Entity<videodb_videodata>()
                .HasMany(s => s.UserSettings)
                .WithOne(k => k.MovieInformation)
                .HasForeignKey(k => k.vdb_movieid);

            modelBuilder.Entity<videodb_videodata>()
                .HasOne(m => m.VideoMediaType)
                .WithMany(b => b.Videos)
                .HasForeignKey(k => k.mediatype);

            modelBuilder.Entity<videodb_mediatypes>()
                .HasMany(v => v.Videos)
                .WithOne(m => m.VideoMediaType);
        }

        public DbSet<videodb_actors> Actors { get; set; }
        public DbSet<videodb_cache> Cache { get; set; }
        public DbSet<videodb_config> Config { get; set; }
        public DbSet<videodb_videogenre> Genre { get; set; }
        public DbSet<videodb_genres> Genres { get; set; }
        public DbSet<videodb_lent> Lent { get; set; }
        public DbSet<videodb_mediatypes> MediaTypes { get; set; }
        public DbSet<videodb_permissions> Permissions { get; set; }
        public DbSet<videodb_userconfig> UserConfig { get; set; }
        public DbSet<videodb_users> Users { get; set; }
        public DbSet<videodb_userseen> UserSeen { get; set; }
        public DbSet<videodb_videodata> VideoData { get; set; }
        public DbSet<homewebbridge_userseen> HomeWebUserSeen { get; set; }
        public DbSet<homewebbridge_usermoviesettings> HomeWebUserMovieSettings { get; set; }
        public DbSet<homewebbridge_inventory> HomeWebInventory { get; set; }
        public DbSet<homewebbridge_inventorydata> HomeWebInventoryData { get; set; }

    }
}
