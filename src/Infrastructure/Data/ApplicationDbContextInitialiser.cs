using DemianzxBackend.Domain.Constants;
using DemianzxBackend.Domain.Entities;
using DemianzxBackend.Infrastructure.Identity;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace DemianzxBackend.Infrastructure.Data;

public static class InitialiserExtensions
{
    public static async Task InitialiseDatabaseAsync(this WebApplication app)
    {
        using var scope = app.Services.CreateScope();

        var initialiser = scope.ServiceProvider.GetRequiredService<ApplicationDbContextInitialiser>();

        await initialiser.InitialiseAsync();

        await initialiser.SeedAsync();
    }
}

public class ApplicationDbContextInitialiser
{
    private readonly ILogger<ApplicationDbContextInitialiser> _logger;
    private readonly ApplicationDbContext _context;
    private readonly UserManager<ApplicationUser> _userManager;
    private readonly RoleManager<IdentityRole> _roleManager;

    public ApplicationDbContextInitialiser(ILogger<ApplicationDbContextInitialiser> logger, ApplicationDbContext context, UserManager<ApplicationUser> userManager, RoleManager<IdentityRole> roleManager)
    {
        _logger = logger;
        _context = context;
        _userManager = userManager;
        _roleManager = roleManager;
    }

    public async Task InitialiseAsync()
    {
        try
        {
            await _context.Database.MigrateAsync();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "An error occurred while initialising the database.");
            throw;
        }
    }

    public async Task SeedAsync()
    {
        try
        {
            await TrySeedAsync();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "An error occurred while seeding the database.");
            throw;
        }
    }

    public async Task TrySeedAsync()
    {
        // Default roles
        var administratorRole = new IdentityRole(Roles.Administrator);

        if (_roleManager.Roles.All(r => r.Name != administratorRole.Name))
        {
            await _roleManager.CreateAsync(administratorRole);
        }

        // Default users
        var administrator = new ApplicationUser { UserName = "administrator@localhost", Email = "administrator@localhost" };

        if (_userManager.Users.All(u => u.UserName != administrator.UserName))
        {
            await _userManager.CreateAsync(administrator, "Administrator1!");
            if (!string.IsNullOrWhiteSpace(administratorRole.Name))
            {
                await _userManager.AddToRolesAsync(administrator, new [] { administratorRole.Name });
            }
        }

        // Default data
        if (!_context.Categories.Any())
        {
            _context.Categories.AddRange(
                new Category
                {
                    Name = "RPG",
                    Slug = "rpg",
                    Description = "Role-playing games"
                },
                new Category
                {
                    Name = "FPS",
                    Slug = "fps",
                    Description = "First-person shooters"
                },
                new Category
                {
                    Name = "Strategy",
                    Slug = "strategy",
                    Description = "Strategy games"
                }
            );

            await _context.SaveChangesAsync();
        }

        if (!_context.Tags.Any())
        {
            _context.Tags.AddRange(
                new Tag { Name = "PC", Slug = "pc" },
                new Tag { Name = "PlayStation", Slug = "playstation" },
                new Tag { Name = "Xbox", Slug = "xbox" },
                new Tag { Name = "Nintendo", Slug = "nintendo" },
                new Tag { Name = "Mobile", Slug = "mobile" }
            );

            await _context.SaveChangesAsync();
        }

        // Crear algunos posts de blog si no existen
        if (!_context.BlogPosts.Any())
        {
            var adminUser = await _userManager.FindByNameAsync("administrator@localhost");
            if (adminUser != null)
            {
                var rpgCategory = await _context.Categories.FirstOrDefaultAsync(c => c.Slug == "rpg");
                var pcTag = await _context.Tags.FirstOrDefaultAsync(t => t.Slug == "pc");

                if (rpgCategory != null && pcTag != null)
                {
                    var post = new BlogPost
                    {
                        Title = "The Evolution of RPG Games",
                        Slug = "evolution-of-rpg-games",
                        Content = "Role-playing games have evolved significantly over the decades...",
                        AuthorId = adminUser.Id,
                        IsPublished = true,
                        PublishedDate = DateTime.UtcNow.AddDays(-5)
                    };

                    _context.BlogPosts.Add(post);
                    await _context.SaveChangesAsync();

                    _context.PostCategories.Add(new PostCategory
                    {
                        PostId = post.Id,
                        CategoryId = rpgCategory.Id
                    });

                    _context.PostTags.Add(new PostTag
                    {
                        PostId = post.Id,
                        TagId = pcTag.Id
                    });

                    await _context.SaveChangesAsync();
                }
            }
        }

    }
}
