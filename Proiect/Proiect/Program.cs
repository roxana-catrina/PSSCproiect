using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Azure;
using Microsoft.OpenApi.Models;
using Proiect.Data;
using Proiect.Data.Services;
using Proiect.Domain.Workflows;
using Proiect.Messaging;
using Proiect.Messaging.Events;

namespace Proiect;

public class Program
{
    public static void Main(string[] args)
    {
        WebApplicationBuilder builder = WebApplication.CreateBuilder(args);
        
        // Load local configuration with secrets (not committed to Git)
        builder.Configuration.AddJsonFile("appsettings.Local.json", optional: true, reloadOnChange: true);

        // Add services to the container.

        builder.Services.AddDbContext<ApplicationDbContext>
            (options => options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));

        builder.Services.AddTransient<IOrderStateService, OrderStateService>();
        builder.Services.AddTransient<IInvoiceStateService, InvoiceStateService>();
        builder.Services.AddTransient<IPackageStateService, PackageStateService>();
        builder.Services.AddTransient<OrderProcessingWorkflow>();
        builder.Services.AddTransient<BillingWorkflow>();
        builder.Services.AddTransient<ShippingWorkflow>();

        builder.Services.AddSingleton<IEventSender, AzureEventSender>();

        builder.Services.AddAzureClients(client =>
        {
            client.AddServiceBusClient(builder.Configuration.GetConnectionString("ServiceBus"));
        });

        builder.Services.AddHttpClient();

        builder.Services.AddControllers();

        // Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
        builder.Services.AddEndpointsApiExplorer();
        builder.Services.AddSwaggerGen(c =>
        {
            c.SwaggerDoc("v1", new OpenApiInfo { Title = "Proiect.Api", Version = "v1" });
        });


        WebApplication app = builder.Build();

        // Ensure database is created
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
        }

        app.UseHttpsRedirection();

        // Enable static files serving
        app.UseDefaultFiles();
        app.UseStaticFiles();

        app.UseAuthorization();


        app.MapControllers();

        app.Run();
    }
}
