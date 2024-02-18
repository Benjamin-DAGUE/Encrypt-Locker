using EncryptLocker.Database;
using EncryptLocker.DataService.Services;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddDbContext<EncryptLockerContext>(o =>
{
    o.UseQueryTrackingBehavior(QueryTrackingBehavior.NoTracking);
    o.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection"));
}, ServiceLifetime.Scoped, ServiceLifetime.Singleton);
builder.Services.AddDbContextFactory<EncryptLockerContext>();

builder.Services.AddGrpc();

var app = builder.Build();

// Configure the HTTP request pipeline.
app.MapGrpcService<LockerService>();

app.Run();
