using CleanArchitecture.Blazor.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;
using Microsoft.Extensions.Configuration;

namespace CleanArchitecture.Blazor.Migrators.SqLite;

public class ApplicationDbContextFactory : IDesignTimeDbContextFactory<ApplicationDbContext>
{
    public ApplicationDbContext CreateDbContext(string[] args)
    {
        var configuration = new ConfigurationBuilder()
            .SetBasePath(Directory.GetCurrentDirectory())
            .AddJsonFile("appsettings.json", optional: false)
            .Build();

        var builder = new DbContextOptionsBuilder<ApplicationDbContext>();
        var connectionString = configuration.GetConnectionString("DefaultConnection") 
            ?? "Data Source=../../../Database/BlazorDashboardDb.db";

        builder.UseSqlite(connectionString, b => b.MigrationsAssembly("CleanArchitecture.Blazor.Migrators.SqLite"));

        return new ApplicationDbContext(builder.Options);
    }
}
