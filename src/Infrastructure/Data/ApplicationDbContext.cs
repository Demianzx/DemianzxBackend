using System.Reflection;
using DemianzxBackend.Application.Common.Interfaces;
using DemianzxBackend.Domain.Entities;
using DemianzxBackend.Infrastructure.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;

namespace DemianzxBackend.Infrastructure.Data;

public class ApplicationDbContext : IdentityDbContext<ApplicationUser>, IApplicationDbContext
{
    private readonly IConfiguration _configuration;
    private readonly string _dbProvider;

    public ApplicationDbContext(
        DbContextOptions<ApplicationDbContext> options,
        IConfiguration configuration)
        : base(options)
    {
        _configuration = configuration;
        _dbProvider = _configuration.GetValue<string>("DbProvider")?.ToLower() ?? "sqlserver";
    }

    public DbSet<BlogPost> BlogPosts => Set<BlogPost>();
    public DbSet<Category> Categories => Set<Category>();
    public DbSet<Tag> Tags => Set<Tag>();
    public DbSet<Comment> Comments => Set<Comment>();
    public DbSet<PostCategory> PostCategories => Set<PostCategory>();
    public DbSet<PostTag> PostTags => Set<PostTag>();

    protected override void OnModelCreating(ModelBuilder builder)
    {
        base.OnModelCreating(builder);

        // Aplicar configuraciones desde el ensamblado
        builder.ApplyConfigurationsFromAssembly(Assembly.GetExecutingAssembly());

        // Configuraciones necesarias para entidades de unión si no están configuradas correctamente
        if (!builder.Model.FindEntityType(typeof(PostCategory))!.FindPrimaryKey()!.Properties.Any())
        {
            builder.Entity<PostCategory>()
                .HasKey(pc => new { pc.PostId, pc.CategoryId });
        }

        if (!builder.Model.FindEntityType(typeof(PostTag))!.FindPrimaryKey()!.Properties.Any())
        {
            builder.Entity<PostTag>()
                .HasKey(pt => new { pt.PostId, pt.TagId });
        }

        // Aplicar configuraciones específicas según el proveedor de la base de datos
        if (_dbProvider == "postgres")
        {
            // Configuraciones específicas para PostgreSQL
            ConfigurePostgresModels(builder);
        }
    }

    private void ConfigurePostgresModels(ModelBuilder builder)
    {
        // Reemplazar los tipos de datos específicos de SQL Server con tipos compatibles con PostgreSQL
        foreach (var entity in builder.Model.GetEntityTypes())
        {
            // Obtener todas las propiedades de la entidad
            foreach (var property in entity.GetProperties())
            {
                // Cambiar el tipo de datos para las propiedades de tipo texto
                if (property.ClrType == typeof(string))
                {
                    property.SetColumnType("text");

                    // Si la propiedad tiene un MaxLength, mantener esa restricción
                    if (property.GetMaxLength() != null)
                    {
                        // En PostgreSQL, text puede tener una restricción CHECK para limitar la longitud
                        // pero mantenemos la compatibilidad del tipo
                        property.SetColumnType("text");
                    }
                }
            }
        }
    }
}
