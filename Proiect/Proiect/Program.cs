using Microsoft.EntityFrameworkCore;
using Microsoft.OpenApi.Models;
using Proiect.Data;
using Proiect.Data.Services;

namespace Proiect;

public class Program
{
    public static void Main(string[] args)
    {
        var builder = WebApplication.CreateBuilder(args);

        // Add services to the container.
        builder.Services.AddControllers();

        // Configure Entity Framework and SQL Server
        builder.Services.AddDbContext<ApplicationDbContext>(options =>
            options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));

        // Register application services
        builder.Services.AddScoped<IOrderStateService, OrderStateService>();
        builder.Services.AddScoped<IInvoiceStateService, InvoiceStateService>();
        builder.Services.AddScoped<IPackageStateService, PackageStateService>();
        builder.Services.AddScoped<IWorkflowOrchestrationService, WorkflowOrchestrationService>();

        // Learn more about configuring OpenAPI at https://aka.ms/aspnet/openapi
        builder.Services.AddOpenApi();

        // Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
        builder.Services.AddEndpointsApiExplorer();
        builder.Services.AddSwaggerGen(c =>
        {
            c.SwaggerDoc("v1", new OpenApiInfo { Title = "Example.Api", Version = "v1" });
        });

        WebApplication app = builder.Build();

        // Ensure database is created and seeded
        using (var scope = app.Services.CreateScope())
        {
            var context = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
            context.Database.EnsureCreated();
        }

        // Configure the HTTP request pipeline.
        if (app.Environment.IsDevelopment())
        {
            app.UseSwagger();
            app.UseSwaggerUI();
            app.MapOpenApi();
        }

        app.UseHttpsRedirection();
        app.UseAuthorization();

        app.MapControllers();

        app.Run();
    }
}
