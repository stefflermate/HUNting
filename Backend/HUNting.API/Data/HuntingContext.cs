using HUNting.API.Models.Ads;
using HUNting.API.Models.Hunting;
using HUNting.API.Models.News;
using HUNting.API.Models.Societies;
using HUNting.API.Models.Users;
using Microsoft.EntityFrameworkCore;

namespace HUNting.API.Data
{
    public class HuntingContext : DbContext
    {
        public HuntingContext(DbContextOptions<HuntingContext> options) : base(options) { }

        public DbSet<User> Users => Set<User>();
        public DbSet<Society> Societies => Set<Society>();
        public DbSet<SocietyRole> SocietyRoles => Set<SocietyRole>();
        public DbSet<SocietyMember> SocietyMembers => Set<SocietyMember>();

        public DbSet<Ad> Ads => Set<Ad>();
        public DbSet<AdCategory> AdCategories => Set<AdCategory>();
        public DbSet<AdImage> AdImages => Set<AdImage>();
        public DbSet<AdStatus> AdStatus => Set<AdStatus>();

        public DbSet<NewsFeed> NewsFeed => Set<NewsFeed>();
        public DbSet<NewsVisibility> NewsVisibility => Set<NewsVisibility>();

        public DbSet<HuntingLog> HuntingLogs => Set<HuntingLog>();

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<SocietyMember>()
                .HasKey(x => new { x.SocietyId, x.UserId });

            modelBuilder.Entity<SocietyMember>()
                .HasOne(x => x.Society)
                .WithMany(x => x.Members)
                .HasForeignKey(x => x.SocietyId)
                .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<SocietyMember>()
                .HasOne(x => x.User)
                .WithMany(x => x.Societies)
                .HasForeignKey(x => x.UserId)
                .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<SocietyMember>()
                .HasOne(x => x.Role)
                .WithMany(x => x.Members)
                .HasForeignKey(x => x.RoleId)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<Ad>()
                .HasOne(a => a.OwnerUser)
                .WithMany()
                .HasForeignKey(a => a.OwnerUserId)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<Ad>()
                .HasOne(a => a.OwnerSociety)
                .WithMany()
                .HasForeignKey(a => a.OwnerSocietyId)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<Ad>()
                .HasOne(a => a.Category)
                .WithMany(c => c.Ads)
                .HasForeignKey(a => a.CategoryId)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<Ad>()
                .HasOne(a => a.Status)
                .WithMany(s => s.Ads)
                .HasForeignKey(a => a.StatusCode)
                .HasPrincipalKey(s => s.StatusCode)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<AdImage>()
                .HasOne(i => i.Ad)
                .WithMany(a => a.Images)
                .HasForeignKey(i => i.AdId);

            modelBuilder.Entity<NewsFeed>()
                .HasOne(n => n.Society)
                .WithMany(s => s.News)
                .HasForeignKey(n => n.SocietyId);

            modelBuilder.Entity<NewsFeed>()
                .HasOne(n => n.Author)
                .WithMany()
                .HasForeignKey(n => n.AuthorUserId);

            modelBuilder.Entity<NewsFeed>()
                .HasOne(n => n.Visibility)
                .WithMany(v => v.News)
                .HasForeignKey(n => n.VisibilityCode)
                .HasPrincipalKey(v => v.VisibilityCode);

            modelBuilder.Entity<HuntingLog>()
                .HasOne(h => h.User)
                .WithMany(u => u.HuntingLogs)
                .HasForeignKey(h => h.UserId);
        }
    }
}
