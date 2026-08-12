using Microsoft.EntityFrameworkCore;
using ProductGrpc.Server.Data;
using ProductGrpc.Server.Services;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddGrpc(options =>
{
    // Handy while learning: send exception detail back to the client.
    options.EnableDetailedErrors = builder.Environment.IsDevelopment();
});

builder.Services.AddDbContext<ProductDbContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("ProductDb")));

var app = builder.Build();

// Apply migrations at startup so the sample runs with no manual DB setup.
using (var scope = app.Services.CreateScope())
{
    var db = scope.ServiceProvider.GetRequiredService<ProductDbContext>();
    await db.Database.MigrateAsync();
}

app.MapGrpcService<ProductGrpcService>();
app.MapGet("/", () => "gRPC server is running. Use a gRPC client to call ProductService.");

app.Run();
