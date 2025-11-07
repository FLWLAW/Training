using SqlServerDatabaseAccessLibrary;
using Training.Website;
using Training.Website.Components;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddRazorComponents()
    .AddInteractiveServerComponents();

/*  // TODO: UNCOMMENT THIS LATER
builder.Services.AddAuthentication(NegotiateDefaults.AuthenticationScheme).AddNegotiate();
builder.Services.AddAuthorization(options => { options.FallbackPolicy = options.DefaultPolicy; });
*/

string? dbConnectionString = Configuration.DatabaseConnectionString();

if (dbConnectionString is null)
    throw new InvalidOperationException("Database connection string is not configured.");
else
{
    builder.Services.AddScoped<IDatabase>(s => new SqlDatabase(dbConnectionString));

    builder.Services.AddTelerikBlazor();

    var app = builder.Build();

    // Configure the HTTP request pipeline.
    if (!app.Environment.IsDevelopment())
    {
        app.UseExceptionHandler("/Error", createScopeForErrors: true);
        // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
        app.UseHsts();
    }

    app.UseHttpsRedirection();


    app.UseAntiforgery();

    app.MapStaticAssets();
    app.MapRazorComponents<App>()
        .AddInteractiveServerRenderMode();

    app.Run();
}