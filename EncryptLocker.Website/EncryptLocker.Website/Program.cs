using EncryptLocker;
using MudBlazor.Services;

var builder = WebApplication.CreateBuilder(args);

// Data
builder.Services.AddGrpcClient<LockerSvc.LockerSvcClient>(o =>
{
    o.Address = new Uri("https://localhost:7292");
});


//UI
builder.Services.AddRazorComponents()
    .AddInteractiveServerComponents()
    .AddInteractiveWebAssemblyComponents();

builder.Services.AddMudServices();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseWebAssemblyDebugging();
}
else
{
    app.UseExceptionHandler("/Error", createScopeForErrors: true);
    // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
    app.UseHsts();
}

app.UseHttpsRedirection();

app.UseStaticFiles();
app.UseAntiforgery();

app.MapRazorComponents<EncryptLocker.Website.Components.App>()
    .AddInteractiveServerRenderMode()
    .AddInteractiveWebAssemblyRenderMode()
    .AddAdditionalAssemblies(typeof(EncryptLocker.Website.Client._Imports).Assembly);

app.Run();
