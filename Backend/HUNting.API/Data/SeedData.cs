using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using HUNting.API.Models.Ads;
using HUNting.API.Models.Hunting;
using HUNting.API.Models.News;
using HUNting.API.Models.Societies;
using HUNting.API.Models.Users;
using HUNting.API.Models.Map;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

namespace HUNting.API.Data
{
    public static class SeedData
    {
        private const string DefaultPassword = "Hunter123!";
        private const string AdminEmail = "admin@gmail.com";
        private const string AdminPassword = "adminadmin";

        public static async Task SeedAsync(IServiceProvider services)
        {
            using var scope = services.CreateScope();
            var context = scope.ServiceProvider.GetRequiredService<HuntingContext>();
            var userManager = scope.ServiceProvider.GetRequiredService<UserManager<ApplicationUser>>();
            var roleManager = scope.ServiceProvider.GetRequiredService<RoleManager<IdentityRole<int>>>();

            await context.Database.MigrateAsync();

            await EnsureRolesAsync(roleManager);
            await EnsureReferenceDataAsync(context);
            var users = await EnsureUsersAsync(userManager);
            await EnsureSocietiesAsync(context);
            await context.SaveChangesAsync();

            var societies = await context.Societies.AsNoTracking().ToDictionaryAsync(s => s.Name);
            var societyRoles = await context.SocietyRoles.AsNoTracking().ToDictionaryAsync(r => r.Name);

            await EnsureSocietyMembersAsync(context, users, societies, societyRoles);
            await context.SaveChangesAsync();

            var categories = await context.AdCategories.AsNoTracking().ToDictionaryAsync(c => c.Name);
            await EnsureAdsAsync(context, users, societies, categories);
            await context.SaveChangesAsync();

            await EnsureNewsFeedAsync(context, users, societies);
            await EnsureHuntingLogsAsync(context, users);
            await context.SaveChangesAsync();
        }

        private static async Task EnsureRolesAsync(RoleManager<IdentityRole<int>> roleManager)
        {
            var roles = new[] { "Administrator", "SocietyManager", "Hunter" };
            foreach (var role in roles)
            {
                if (!await roleManager.RoleExistsAsync(role))
                {
                    await roleManager.CreateAsync(new IdentityRole<int>(role));
                }
            }
        }

        private static async Task EnsureReferenceDataAsync(HuntingContext context)
        {
            var adStatuses = new[]
            {
                new AdStatus { StatusCode = "ACTIVE", Description = "Aktiv hirdetes" },
                new AdStatus { StatusCode = "PENDING", Description = "Ellenorzes alatt" },
                new AdStatus { StatusCode = "SOLD", Description = "Eladva / lezarva" },
                new AdStatus { StatusCode = "HIDDEN", Description = "Rejtett" }
            };

            foreach (var status in adStatuses)
            {
                if (!await context.AdStatuses.AnyAsync(s => s.StatusCode == status.StatusCode))
                {
                    context.AdStatuses.Add(status);
                }
            }

            var newsVisibilities = new[]
            {
                new NewsVisibility { VisibilityCode = "MEMBERS", Description = "Tarsasagi tagoknak" },
                new NewsVisibility { VisibilityCode = "PUBLIC", Description = "Nyilvanos" },
                new NewsVisibility { VisibilityCode = "OFFICERS", Description = "Vezetosegnek" }
            };

            foreach (var visibility in newsVisibilities)
            {
                if (!await context.NewsVisibility.AnyAsync(v => v.VisibilityCode == visibility.VisibilityCode))
                {
                    context.NewsVisibility.Add(visibility);
                }
            }

            var mapVisibilities = new[]
            {
                new MapVisibility { VisibilityCode = "PUBLIC", Description = "Nyilvanos" },
                new MapVisibility { VisibilityCode = "MEMBERS", Description = "Tagok szamara" },
                new MapVisibility { VisibilityCode = "ADMIN", Description = "Csak adminisztratoroknak" }
            };

            foreach (var visibility in mapVisibilities)
            {
                if (!await context.MapVisibilities.AnyAsync(v => v.VisibilityCode == visibility.VisibilityCode))
                {
                    context.MapVisibilities.Add(visibility);
                }
            }

            var iconTypes = new[]
            {
                new MapIconType { Name = "Megfigyelopont", DefaultColor = "#f97316", Glyph = "binoculars" },
                new MapIconType { Name = "Eteto", DefaultColor = "#22c55e", Glyph = "wheat" },
                new MapIconType { Name = "Veszely", DefaultColor = "#ef4444", Glyph = "alert-triangle" },
                new MapIconType { Name = "Talalkozo", DefaultColor = "#3b82f6", Glyph = "map-pin" }
            };

            foreach (var icon in iconTypes)
            {
                if (!await context.MapIconTypes.AnyAsync(i => i.Name == icon.Name))
                {
                    context.MapIconTypes.Add(icon);
                }
            }

            var societyRoles = new[]
            {
                new SocietyRole { Name = "Vezeto" },
                new SocietyRole { Name = "Gazda" },
                new SocietyRole { Name = "Vador" },
                new SocietyRole { Name = "Tag" }
            };

            foreach (var role in societyRoles)
            {
                if (!await context.SocietyRoles.AnyAsync(r => r.Name == role.Name))
                {
                    context.SocietyRoles.Add(role);
                }
            }

            var categories = new[]
            {
                "Fegyverek",
                "Loszerek",
                "Optika (tavcso, celtavcso)",
                "Ruhazat, labbeli",
                "Kiegeszitok (tavcsoallvany, heveder, tok)",
                "Csapdak, teto, sozo",
                "Vadkamera",
                "Kutya felszereles",
                "Vadfeldolgozas",
                "Terepjaro / UTV tartozek",
                "Konyv, terkep",
                "Egyeb"
            };

            foreach (var name in categories)
            {
                if (!await context.AdCategories.AnyAsync(c => c.Name == name))
                {
                    context.AdCategories.Add(new AdCategory { Name = name });
                }
            }

            await context.SaveChangesAsync();
        }

        private static async Task<Dictionary<string, ApplicationUser>> EnsureUsersAsync(UserManager<ApplicationUser> userManager)
        {
            var seedUsers = new[]
            {
                new SeedUser("Admin",         AdminEmail,                 "+36 30 000 0000", new[] { "Administrator" }, AdminPassword),
                new SeedUser("Kovacs Bence",  "bence.kovacs@example.hu",  "+36 30 111 1111", new[] { "Hunter" }),
                new SeedUser("Nagy Zsofia",   "zsofia.nagy@example.hu",   "+36 30 222 2222", new[] { "Hunter" }),
                new SeedUser("Toth Andras",   "andras.toth@example.hu",   "+36 30 333 3333", new[] { "Hunter" }),
                new SeedUser("Szabo Katalin", "katalin.szabo@example.hu", "+36 30 444 4444", new[] { "Hunter" }),
                new SeedUser("Horvath Mate",  "mate.horvath@example.hu",  "+36 30 555 5555", new[] { "Hunter" }),
                new SeedUser("Varga Anna",    "anna.varga@example.hu",    "+36 30 666 6666", new[] { "Hunter" }),
                new SeedUser("Kiss Laszlo",   "laszlo.kiss@example.hu",   "+36 30 777 7777", new[] { "Hunter" }),
                new SeedUser("Molnar Peter",  "peter.molnar@example.hu",  "+36 30 888 8888", new[] { "Hunter" }),
                new SeedUser("Balogh Eszter", "eszter.balogh@example.hu", "+36 30 999 9999", new[] { "Hunter" }),
                new SeedUser("Farkas Gergo",  "gergo.farkas@example.hu",  "+36 70 123 4567", new[] { "Hunter" })
            };

            var lookup = new Dictionary<string, ApplicationUser>(StringComparer.OrdinalIgnoreCase);

            foreach (var seedUser in seedUsers)
            {
                var user = await userManager.FindByEmailAsync(seedUser.Email);
                if (user == null)
                {
                    user = new ApplicationUser
                    {
                        FullName = seedUser.FullName,
                        Email = seedUser.Email,
                        UserName = seedUser.Email,
                        Phone = seedUser.Phone,
                        PhoneNumber = seedUser.Phone,
                        EmailConfirmed = true,
                        PhoneNumberConfirmed = true
                    };

                    var password = seedUser.Password ?? DefaultPassword;
                    var createResult = await userManager.CreateAsync(user, password);
                    if (!createResult.Succeeded)
                    {
                        var message = string.Join(", ", createResult.Errors.Select(e => e.Description));
                        throw new InvalidOperationException($"Failed to create seed user {seedUser.Email}: {message}");
                    }
                }

                foreach (var role in seedUser.Roles)
                {
                    if (!await userManager.IsInRoleAsync(user, role))
                    {
                        await userManager.AddToRoleAsync(user, role);
                    }
                }

                lookup[seedUser.Email] = user;
            }

            return lookup;
        }

        private static async Task EnsureSocietiesAsync(HuntingContext context)
        {
            var societies = new[]
            {
                new Society { Name = "Bakonyi Vadasztarsasag",        Email = "kapcsolat@bakony-vt.example.hu",   PasswordHash = "HASHA", Phone = "+36 88 100 100", Description = "Bakony teruleten mukodo vegyes apro es nagyvadas terulet." },
                new Society { Name = "Gemenci Vadasztarsasag",        Email = "info@gemenc-vt.example.hu",        PasswordHash = "HASHB", Phone = "+36 74 200 200", Description = "Arteri erdok, gim es vaddiszno allomannyal." },
                new Society { Name = "Matrai Hegyhat Vadasztarsasag", Email = "elerhetoseg@matra-vt.example.hu",  PasswordHash = "HASHC", Phone = "+36 37 300 300", Description = "Heves varmegyei hegyvideki terulet, oz es gim dominanciaval." },
                new Society { Name = "Hortobagyi Puszta VT",          Email = "hello@hortobagy-vt.example.hu",    PasswordHash = "HASHD", Phone = "+36 52 400 400", Description = "Nyilt pusztai terulet, aprovad programok." },
                new Society { Name = "Orsegi Zold Hatar VT",          Email = "info@orsegi-vt.example.hu",        PasswordHash = "HASHE", Phone = "+36 94 500 500", Description = "Orseg mozgalmas erdei elovilaga, damszarvas." },
                new Society { Name = "Somogyi Erdok VT",              Email = "kontakt@somogy-vt.example.hu",     PasswordHash = "HASHF", Phone = "+36 82 600 600", Description = "Surun lakott erdok, kivallo vaddiszno es gim allomannyal." },
                new Society { Name = "Pilisi Harsak VT",              Email = "kapcsolat@pilis-vt.example.hu",    PasswordHash = "HASHG", Phone = "+36 26 700 700", Description = "Pilis es Budai-hegyseg peremteruleten kozos hajtasok." },
                new Society { Name = "Zempleni Bercek VT",            Email = "info@zemplen-vt.example.hu",       PasswordHash = "HASHH", Phone = "+36 47 800 800", Description = "Zempleni hegyseg muflon es oz jelenlettel." }
            };

            foreach (var society in societies)
            {
                if (!await context.Societies.AnyAsync(s => s.Email == society.Email))
                {
                    context.Societies.Add(society);
                }
            }
        }

        private static async Task EnsureSocietyMembersAsync(
            HuntingContext context,
            IDictionary<string, ApplicationUser> users,
            IDictionary<string, Society> societies,
            IDictionary<string, SocietyRole> roles)
        {
            async Task AddMembershipAsync(string societyName, string userEmail, string roleName)
            {
                if (!societies.TryGetValue(societyName, out var society)) return;
                if (!users.TryGetValue(userEmail, out var user)) return;
                if (!roles.TryGetValue(roleName, out var role)) return;

                var exists = context.SocietyMembers.Local.Any(sm => sm.SocietyId == society.SocietyId && sm.UserId == user.Id)
                    || await context.SocietyMembers.AnyAsync(sm => sm.SocietyId == society.SocietyId && sm.UserId == user.Id);

                if (exists) return;

                context.SocietyMembers.Add(new SocietyMember
                {
                    SocietyId = society.SocietyId,
                    UserId = user.Id,
                    RoleId = role.RoleId,
                    JoinedAt = DateTime.UtcNow.AddDays(-30)
                });
            }

            await AddMembershipAsync("Bakonyi Vadasztarsasag", "bence.kovacs@example.hu", "Vezeto");
            await AddMembershipAsync("Gemenci Vadasztarsasag", "andras.toth@example.hu", "Vador");
            await AddMembershipAsync("Bakonyi Vadasztarsasag", "zsofia.nagy@example.hu", "Tag");

            var extraMembers = users.Keys.Take(3).ToArray();
            foreach (var society in societies.Values)
            {
                foreach (var email in extraMembers)
                {
                    await AddMembershipAsync(society.Name, email, "Tag");
                }
            }
        }

        private static async Task EnsureAdsAsync(
            HuntingContext context,
            IDictionary<string, ApplicationUser> users,
            IDictionary<string, Society> societies,
            IDictionary<string, AdCategory> categories)
        {
            var ads = new[]
            {
                new SeedAd("Bock BBF soretes - megkimelt", "Fegyverek", 320000m, "Megkimelt allapot, 12/70, fix szukitesek.", "Veszprem megye", "ACTIVE", "kapcsolat@bakony-vt.example.hu", "+36 88 100 100")
                {
                    OwnerSocietyName = "Bakonyi Vadasztarsasag"
                },
                new SeedAd("Swarovski Z6i 2-12x50 celtavcso", "Optika (tavcso, celtavcso)", 690000m, "Fenyes, eles kep, eredeti doboz.", "Varpalota", "ACTIVE", "bence.kovacs@example.hu", "+36 30 111 1111")
                {
                    OwnerUserEmail = "bence.kovacs@example.hu"
                },
                new SeedAd("Harkila vadaszkabat (L)", "Ruhazat, labbeli", 45000m, "Vizallo, szelallo, keveset hasznalt.", "Baja", "ACTIVE", "info@gemenc-vt.example.hu", "+36 74 200 200")
                {
                    OwnerSocietyName = "Gemenci Vadasztarsasag"
                },
                new SeedAd("Browning BAR II 30-06", "Fegyverek", 380000m, "Gazdugattyus felautomata, pontos.", "Gyongyos", "PENDING", "zsofia.nagy@example.hu", "+36 30 222 2222")
                {
                    OwnerUserEmail = "zsofia.nagy@example.hu"
                },
                new SeedAd("Reconyx HyperFire 2 vadkamera", "Vadkamera", 85000m, "Gyors kioldas, hosszu uzemido.", "Szekszard", "ACTIVE", "andras.toth@example.hu", "+36 30 333 3333")
                {
                    OwnerUserEmail = "andras.toth@example.hu"
                },
                new SeedAd("Niggeloh fegyverszij", "Kiegeszitok (tavcsoallvany, heveder, tok)", 12000m, "Csuszasmentes, halk.", "Veszprem", "ACTIVE", "bence.kovacs@example.hu", "+36 30 111 1111")
                {
                    OwnerUserEmail = "bence.kovacs@example.hu"
                },
                new SeedAd("Mendota apport-poraz", "Kutya felszereles", 6000m, "Tartos, kenyelmes fogas.", "Bonyhad", "ACTIVE", "andras.toth@example.hu", "+36 30 333 3333")
                {
                    OwnerUserEmail = "andras.toth@example.hu"
                }
            };

            foreach (var ad in ads)
            {
                if (await context.Ads.AnyAsync(a => a.Title == ad.Title))
                {
                    continue;
                }

                var entity = new Ad
                {
                    Title = ad.Title,
                    Description = ad.Description,
                    CategoryId = categories.TryGetValue(ad.CategoryName, out var category) ? category.CategoryId : null,
                    Price = ad.Price,
                    LocationText = ad.LocationText,
                    StatusCode = ad.StatusCode,
                    ContactEmail = ad.ContactEmail,
                    ContactPhone = ad.ContactPhone
                };

                if (!string.IsNullOrWhiteSpace(ad.OwnerSocietyName) && societies.TryGetValue(ad.OwnerSocietyName, out var society))
                {
                    entity.OwnerSocietyId = society.SocietyId;
                }
                else if (!string.IsNullOrWhiteSpace(ad.OwnerUserEmail) && users.TryGetValue(ad.OwnerUserEmail, out var user))
                {
                    entity.OwnerUserId = user.Id;
                }

                context.Ads.Add(entity);
            }

            await context.SaveChangesAsync();

            await EnsureAdImagesAsync(context);
        }

        private static async Task EnsureAdImagesAsync(HuntingContext context)
        {
            var imageMap = new Dictionary<string, string>
            {
                ["Swarovski Z6i 2-12x50 celtavcso"] = "https://images.unsplash.com/photo-1511765224389-37f0e77cf0eb",
                ["Bock BBF soretes - megkimelt"]    = "https://images.unsplash.com/photo-1517649763962-0c623066013b",
                ["Harkila vadaszkabat (L)"]        = "https://images.unsplash.com/photo-1533681018184-68bd1d883546",
                ["Browning BAR II 30-06"]          = "https://images.unsplash.com/photo-1525547719571-a2d4ac8945e2",
                ["Reconyx HyperFire 2 vadkamera"]  = "https://images.unsplash.com/photo-1520975693410-0010fdf7a4a3",
                ["Niggeloh fegyverszij"]           = "https://images.unsplash.com/photo-1499952127939-9bbf5af6c51c",
                ["Mendota apport-poraz"]           = "https://images.unsplash.com/photo-1558944351-c6ae7b2b37d7"
            };

            var ads = await context.Ads.Where(a => imageMap.Keys.Contains(a.Title)).ToListAsync();
            foreach (var ad in ads)
            {
                var imageUrl = imageMap[ad.Title];
                if (!await context.AdImages.AnyAsync(i => i.AdId == ad.AdId && i.ImageUrl == imageUrl))
                {
                    context.AdImages.Add(new AdImage
                    {
                        AdId = ad.AdId,
                        ImageUrl = imageUrl,
                        SortOrder = 1
                    });
                }
            }
        }

        private static async Task EnsureNewsFeedAsync(
            HuntingContext context,
            IDictionary<string, ApplicationUser> users,
            IDictionary<string, Society> societies)
        {
            var entries = new[]
            {
                new SeedNews(
                    Title: "Sikeres gim bikavadaszat",
                    SocietyName: "Bakonyi Vadasztarsasag",
                    AuthorEmail: "bence.kovacs@example.hu",
                    Content: "Az oszi szezon elso bikaja Bakonyban. Merlegeles holnap 10-kor.",
                    ImageUrl: "https://images.unsplash.com/photo-1500530855697-b586d89ba3ee",
                    Visibility: "MEMBERS"),
                new SeedNews(
                    Title: "Hajtas elokeszitese - jelentkezes",
                    SocietyName: "Gemenci Vadasztarsasag",
                    AuthorEmail: "andras.toth@example.hu",
                    Content: "Szombaton kozos vaddiszno-hajtas, jelentkezes a gazdanal pentekig.",
                    ImageUrl: "https://images.unsplash.com/photo-1519681393784-d120267933ba",
                    Visibility: "PUBLIC")
            };

            foreach (var news in entries)
            {
                if (await context.NewsFeed.AnyAsync(n => n.Title == news.Title))
                {
                    continue;
                }

                if (!societies.TryGetValue(news.SocietyName, out var society)) continue;
                if (!users.TryGetValue(news.AuthorEmail, out var author)) continue;

                context.NewsFeed.Add(new NewsFeed
                {
                    SocietyId = society.SocietyId,
                    AuthorUserId = author.Id,
                    Title = news.Title,
                    Content = news.Content,
                    ImageUrl = news.ImageUrl,
                    VisibilityCode = news.Visibility
                });
            }
        }

        private static async Task EnsureHuntingLogsAsync(
            HuntingContext context,
            IDictionary<string, ApplicationUser> users)
        {
            var logs = new[]
            {
                new SeedLog("bence.kovacs@example.hu", DateTime.UtcNow.AddDays(-10), "Bakony - Csetenyi volgy", "Gimszarvas bika", "200 m-en tiszta loves.", "https://images.unsplash.com/photo-1516387938699-a93567ec168e"),
                new SeedLog("zsofia.nagy@example.hu",  DateTime.UtcNow.AddDays(-7),  "Matra - Paradsasvar",     "Oz suta",        "Rovid cserkeles, tiszta talalat.", "https://images.unsplash.com/photo-1518791841217-8f162f1e1131"),
                new SeedLog("andras.toth@example.hu", DateTime.UtcNow.AddDays(-2),  "Gemenc - arter",          "Vaddiszno suldo","Hajtasban, biztonsagos szog.", "https://images.unsplash.com/photo-1508672019048-805c876b67e2"),
                new SeedLog("bence.kovacs@example.hu",DateTime.UtcNow.AddDays(-1),  "Bakony - ret szele",       "Facan kakas",    "Vizsla jelezte, gyors loves.", "https://images.unsplash.com/photo-1484406566174-9da000fda645")
            };

            foreach (var log in logs)
            {
                if (!users.TryGetValue(log.UserEmail, out var user)) continue;

                var exists = await context.HuntingLogs.AnyAsync(h =>
                    h.UserId == user.Id &&
                    h.OccurredAt == log.OccurredAt &&
                    h.AnimalType == log.AnimalType);

                if (exists) continue;

                context.HuntingLogs.Add(new HuntingLog
                {
                    UserId = user.Id,
                    OccurredAt = log.OccurredAt,
                    LocationText = log.LocationText,
                    AnimalType = log.AnimalType,
                    ShotFired = true,
                    Notes = log.Notes,
                    ImageUrl = log.ImageUrl,
                    ShareWithSociety = true
                });
            }
        }

        private record SeedUser(string FullName, string Email, string Phone, string[] Roles, string? Password = null);

        private record SeedAd(
            string Title,
            string CategoryName,
            decimal Price,
            string Description,
            string LocationText,
            string StatusCode,
            string ContactEmail,
            string ContactPhone)
        {
            public string? OwnerSocietyName { get; init; }
            public string? OwnerUserEmail { get; init; }
        }

        private record SeedNews(
            string Title,
            string SocietyName,
            string AuthorEmail,
            string Content,
            string ImageUrl,
            string Visibility);

        private record SeedLog(
            string UserEmail,
            DateTime OccurredAt,
            string LocationText,
            string AnimalType,
            string Notes,
            string ImageUrl);
    }
}

