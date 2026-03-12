using HUNting.API.Models.Ads;
using HUNting.API.Models.Hunting;
using HUNting.API.Models.Map;
using HUNting.API.Models.News;
using HUNting.API.Models.Societies;
using HUNting.API.Models.Users;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

namespace HUNting.API.Data
{
    public class HuntingContext : IdentityDbContext<ApplicationUser, IdentityRole<int>, int>
    {
        public HuntingContext(DbContextOptions<HuntingContext> options) : base(options) { }

        public new DbSet<ApplicationUser> Users => base.Users;
        public DbSet<Society> Societies => Set<Society>();
        public DbSet<SocietyRole> SocietyRoles => Set<SocietyRole>();
        public DbSet<SocietyMember> SocietyMembers => Set<SocietyMember>();
        public DbSet<SocietyMembershipRequest> SocietyMembershipRequests => Set<SocietyMembershipRequest>();
        public DbSet<SocietyPost> SocietyPosts => Set<SocietyPost>();

        public DbSet<Ad> Ads => Set<Ad>();
        public DbSet<AdCategory> AdCategories => Set<AdCategory>();
        public DbSet<AdImage> AdImages => Set<AdImage>();
        public DbSet<AdStatus> AdStatuses => Set<AdStatus>();

        public DbSet<NewsFeed> NewsFeed => Set<NewsFeed>();
        public DbSet<NewsVisibility> NewsVisibility => Set<NewsVisibility>();

        public DbSet<HuntingLog> HuntingLogs => Set<HuntingLog>();

        public DbSet<MapVisibility> MapVisibilities => Set<MapVisibility>();
        public DbSet<MapIconType> MapIconTypes => Set<MapIconType>();
        public DbSet<MapMarker> MapMarkers => Set<MapMarker>();
        public DbSet<MapPolygon> MapPolygons => Set<MapPolygon>();
        public DbSet<MapPolygonPoint> MapPolygonPoints => Set<MapPolygonPoint>();
        public DbSet<MapPath> MapPaths => Set<MapPath>();
        public DbSet<MapPathPoint> MapPathPoints => Set<MapPathPoint>();

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            modelBuilder.Entity<ApplicationUser>().ToTable("Users");
            modelBuilder.Entity<IdentityRole<int>>().ToTable("Roles");
            modelBuilder.Entity<IdentityUserRole<int>>().ToTable("UserRoles");
            modelBuilder.Entity<IdentityUserClaim<int>>().ToTable("UserClaims");
            modelBuilder.Entity<IdentityUserLogin<int>>().ToTable("UserLogins");
            modelBuilder.Entity<IdentityRoleClaim<int>>().ToTable("RoleClaims");
            modelBuilder.Entity<IdentityUserToken<int>>().ToTable("UserTokens");

            modelBuilder.Entity<ApplicationUser>()
                .Property(u => u.FullName)
                .HasMaxLength(100)
                .IsRequired();

            modelBuilder.Entity<ApplicationUser>()
                .Property(u => u.Phone)
                .HasMaxLength(20);

            modelBuilder.Entity<ApplicationUser>()
                .Property(u => u.CreatedAt)
                .HasDefaultValueSql("sysdatetime()");

            modelBuilder.Entity<ApplicationUser>()
                .HasIndex(u => u.Email)
                .IsUnique();

            modelBuilder.Entity<Society>()
                .HasIndex(s => s.Email)
                .IsUnique();

            modelBuilder.Entity<Society>()
                .Property(s => s.CreatedAt)
                .HasDefaultValueSql("sysdatetime()");

            modelBuilder.Entity<ApplicationUser>()
                .HasOne(u => u.OwnedSociety)
                .WithOne(s => s.Owner)
                .HasForeignKey<Society>(s => s.OwnerUserId)
                .OnDelete(DeleteBehavior.SetNull);

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

            modelBuilder.Entity<SocietyMember>()
                .Property(m => m.JoinedAt)
                .HasDefaultValueSql("sysdatetime()");

            modelBuilder.Entity<SocietyMembershipRequest>()
                .HasKey(r => new { r.SocietyId, r.UserId });

            modelBuilder.Entity<SocietyMembershipRequest>()
                .HasOne(r => r.Society)
                .WithMany(s => s.MembershipRequests)
                .HasForeignKey(r => r.SocietyId)
                .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<SocietyMembershipRequest>()
                .HasOne(r => r.User)
                .WithMany(u => u.MembershipRequests)
                .HasForeignKey(r => r.UserId)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<SocietyMembershipRequest>()
                .HasOne(r => r.ProcessedBy)
                .WithMany(u => u.ProcessedMembershipRequests)
                .HasForeignKey(r => r.ProcessedByUserId)
                .OnDelete(DeleteBehavior.SetNull);

            modelBuilder.Entity<SocietyPost>()
                .HasOne(p => p.Society)
                .WithMany(s => s.Posts)
                .HasForeignKey(p => p.SocietyId)
                .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<SocietyPost>()
                .HasOne(p => p.Author)
                .WithMany()
                .HasForeignKey(p => p.AuthorUserId)
                .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<SocietyRole>()
                .HasIndex(r => r.Name)
                .IsUnique();

            modelBuilder.Entity<AdCategory>()
                .HasIndex(c => c.Name)
                .IsUnique();

            modelBuilder.Entity<Ad>()
                .Property(a => a.CreatedAt)
                .HasDefaultValueSql("sysdatetime()");

            modelBuilder.Entity<Ad>()
                .HasOne(a => a.OwnerUser)
                .WithMany()
                .HasForeignKey(a => a.OwnerUserId)
                .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<Ad>()
                .HasOne(a => a.OwnerSociety)
                .WithMany()
                .HasForeignKey(a => a.OwnerSocietyId)
                .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<Ad>()
                .ToTable(t => t.HasCheckConstraint("CK_Ads_SingleOwner", "(OwnerUserId IS NOT NULL AND OwnerSocietyId IS NULL) OR (OwnerUserId IS NULL AND OwnerSocietyId IS NOT NULL)"));

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
                .HasForeignKey(i => i.AdId)
                .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<NewsFeed>()
                .Property(n => n.CreatedAt)
                .HasDefaultValueSql("sysdatetime()");

            modelBuilder.Entity<NewsFeed>()
                .HasOne(n => n.Society)
                .WithMany(s => s.News)
                .HasForeignKey(n => n.SocietyId)
                .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<NewsFeed>()
                .HasOne(n => n.Author)
                .WithMany()
                .HasForeignKey(n => n.AuthorUserId)
                .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<NewsFeed>()
                .HasOne(n => n.Visibility)
                .WithMany(v => v.News)
                .HasForeignKey(n => n.VisibilityCode)
                .HasPrincipalKey(v => v.VisibilityCode)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<HuntingLog>()
                .Property(h => h.CreatedAt)
                .HasDefaultValueSql("sysdatetime()");

            modelBuilder.Entity<HuntingLog>()
                .HasOne(h => h.User)
                .WithMany(u => u.HuntingLogs)
                .HasForeignKey(h => h.UserId)
                .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<MapVisibility>()
                .HasKey(v => v.VisibilityCode);

            modelBuilder.Entity<MapIconType>()
                .Property(i => i.Name)
                .HasMaxLength(100)
                .IsRequired();

            modelBuilder.Entity<MapMarker>()
                .Property(m => m.CreatedAt)
                .HasDefaultValueSql("sysdatetime()");

            modelBuilder.Entity<MapMarker>()
                .HasOne(m => m.CreatedByUser)
                .WithMany()
                .HasForeignKey(m => m.CreatedByUserId)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<MapMarker>()
                .HasOne(m => m.Society)
                .WithMany()
                .HasForeignKey(m => m.SocietyId)
                .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<MapMarker>()
                .HasOne(m => m.IconType)
                .WithMany()
                .HasForeignKey(m => m.IconTypeId)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<MapMarker>()
                .HasOne(m => m.Visibility)
                .WithMany()
                .HasForeignKey(m => m.VisibilityCode)
                .HasPrincipalKey(v => v.VisibilityCode)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<MapPolygon>()
                .Property(p => p.CreatedAt)
                .HasDefaultValueSql("sysdatetime()");

            modelBuilder.Entity<MapPolygon>()
                .HasOne(p => p.CreatedByUser)
                .WithMany()
                .HasForeignKey(p => p.CreatedByUserId)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<MapPolygon>()
                .HasOne(p => p.Society)
                .WithMany()
                .HasForeignKey(p => p.SocietyId)
                .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<MapPolygon>()
                .HasOne(p => p.Visibility)
                .WithMany()
                .HasForeignKey(p => p.VisibilityCode)
                .HasPrincipalKey(v => v.VisibilityCode)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<MapPolygonPoint>()
                .HasOne(pp => pp.Polygon)
                .WithMany(p => p.Points)
                .HasForeignKey(pp => pp.PolygonId)
                .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<MapPolygonPoint>()
                .HasIndex(pp => new { pp.PolygonId, pp.Seq })
                .IsUnique();

            modelBuilder.Entity<MapPath>()
                .Property(p => p.CreatedAt)
                .HasDefaultValueSql("sysdatetime()");

            modelBuilder.Entity<MapPath>()
                .HasOne(p => p.CreatedByUser)
                .WithMany()
                .HasForeignKey(p => p.CreatedByUserId)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<MapPath>()
                .HasOne(p => p.Society)
                .WithMany()
                .HasForeignKey(p => p.SocietyId)
                .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<MapPath>()
                .HasOne(p => p.Visibility)
                .WithMany()
                .HasForeignKey(p => p.VisibilityCode)
                .HasPrincipalKey(v => v.VisibilityCode)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<MapPathPoint>()
                .HasOne(pp => pp.Path)
                .WithMany(p => p.Points)
                .HasForeignKey(pp => pp.PathId)
                .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<MapPathPoint>()
                .HasIndex(pp => new { pp.PathId, pp.Seq })
                .IsUnique();
        }
    }
}



