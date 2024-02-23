using EncryptLocker;
using EncryptLocker.Website.Components;
using EncryptLocker.Website.Services;
using MudBlazor.Services;

var builder = WebApplication.CreateBuilder(args);

// Data
builder.Services.AddGrpcClient<LockerSvc.LockerSvcClient>(o =>
{
    o.Address = new Uri("https://localhost:7292");
});
builder.Services.AddScoped<LockerDataService>();

//UI
builder.Services.AddRazorComponents()
    .AddInteractiveServerComponents()
    .AddCircuitOptions(options =>
    {
        if (builder.Environment.IsDevelopment()) // Only in development environment
        {
            options.DetailedErrors = true;
        }
    }); ;

builder.Services.AddMudServices();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Error", createScopeForErrors: true);
    // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
    app.UseHsts();
}

app.UseHttpsRedirection();

app.UseStaticFiles();
app.UseAntiforgery();

app.MapRazorComponents<App>()
    .AddInteractiveServerRenderMode();

app.Run();
