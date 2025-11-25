using SqlServerDatabaseAccessLibrary;
using Training.Website;
using Training.Website.Components;

//DONE: 1.	When I select a session, then try to select a different Session, the window that says ‘There are no questions for this session’ blocks the dropdown. Can you add a ‘X’ to close that window or automatically close that window?
//DONE: 2.	On the Session ID dropdown, is it possible to type the digits and it will go to that session? Right now only it’s only checking the first digit
//TODO: SEARCH BY TITLE
//TODO: SEARCH BY ROLE
//TODO: REPORT PAGE
//TODO: SET UP SECURITY
//TODO: IMPERSONATION PAGE?
//TODO: PREVENT **THERE ARE NO QUESTIONS FOR THIS SESSION** FROM FLASHING ON SCREEN BEFORE REFRESH

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddRazorComponents()
    .AddInteractiveServerComponents();

/*  // TODO: UNCOMMENT THIS LATER
builder.Services.AddAuthentication(NegotiateDefaults.AuthenticationScheme).AddNegotiate();
builder.Services.AddAuthorization(options => { options.FallbackPolicy = options.DefaultPolicy; });
*/

string? dbConnectionString_OPS = Configuration.DatabaseConnectionString_OPS();

if (dbConnectionString_OPS is null)
    throw new InvalidOperationException("Database connection string for OPS is not configured.");
else
{
    builder.Services.AddScoped<IDatabase>(s => new SqlDatabase(dbConnectionString_OPS));

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