using DemianzxBackend.Infrastructure.Data;
using NSwag;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.AddKeyVaultIfConfigured();
builder.AddApplicationServices();
builder.AddInfrastructureServices();
builder.AddWebServices();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.Logger.LogInformation("Running in Development environment");
    await app.InitialiseDatabaseAsync();
}
else
{
    app.Logger.LogInformation("Running in Production environment");
    await ProductionInitializer.InitializeProductionDatabaseAsync(app);

    // The default HSTS value is 30 days
    app.UseHsts();
    app.UseHttpsRedirection();
}


app.UseCors("CorsPolicy");
app.UseHealthChecks("/health");
app.UseStaticFiles();
app.UseAuthentication();
app.UseAuthorization();
app.UseAntiforgery();

app.UseSwaggerUi(settings =>
{
    settings.Path = "/api";
    settings.DocumentPath = "/api/specification.json";
});


app.UseExceptionHandler(options => { });

app.Map("/", () => Results.Redirect("/api"));

app.MapEndpoints();

app.Run();

public partial class Program { }
