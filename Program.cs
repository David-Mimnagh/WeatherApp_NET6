using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.UI;
using Microsoft.EntityFrameworkCore;
using WeatherApp_NET6.Data;
using WeatherApp_NET6.Models;
using Azure.Identity;
using Microsoft.Extensions.Configuration.AzureAppConfiguration;
using WeatherApp_NET6.Models;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
var connectionString = builder.Configuration.GetConnectionString("DefaultConnection");

builder.Services.AddDbContext<ApplicationDbContext>(options =>
    options.UseSqlServer(connectionString));
builder.Services.AddDatabaseDeveloperPageExceptionFilter();

builder.Services.AddDefaultIdentity<ApplicationUser>(options => options.SignIn.RequireConfirmedAccount = true)
    .AddEntityFrameworkStores<ApplicationDbContext>();

builder.Services.AddIdentityServer()
    .AddApiAuthorization<ApplicationUser, ApplicationDbContext>();

builder.Services.AddAuthentication()
    .AddIdentityServerJwt();

builder.Services.AddControllersWithViews();
builder.Services.AddRazorPages();
builder.Services.AddApplicationInsightsTelemetry(builder.Configuration["APPLICATIONINSIGHTS_CONNECTION_STRING"]);

// Add Azure App Configuration to the container.
var azAppConfigConnection = builder.Configuration["AppConfig"];
if (!string.IsNullOrEmpty(azAppConfigConnection))
{
    // Use the connection string if it is available.
    builder.Configuration.AddAzureAppConfiguration(options =>
    {
        options.Connect(azAppConfigConnection)
                .ConfigureKeyVault(kv =>
                {
                    kv.SetCredential(new DefaultAzureCredential(true));
                })
                .Select(KeyFilter.Any, "BradyTech:WeatherApp");
    });
}
else if (Uri.TryCreate(builder.Configuration["Endpoints:AppConfig"], UriKind.Absolute, out var endpoint))
{
    // Use Azure Active Directory authentication.
    // The identity of this app should be assigned 'App Configuration Data Reader' or 'App Configuration Data Owner' role in App Configuration.
    // For more information, please visit https://aka.ms/vs/azure-app-configuration/concept-enable-rbac
    builder.Configuration.AddAzureAppConfiguration(options =>
    {
        options.Connect(endpoint, new DefaultAzureCredential())
                .ConfigureKeyVault(kv =>
                {
                    kv.SetCredential(new DefaultAzureCredential(true));
                })
                .Select(KeyFilter.Any, "BradyTech:WeatherApp");
    });
}
builder.Services.AddAzureAppConfiguration();
builder.Services.AddOptions<Settings>().Configure<IConfiguration>((settings, configuration) =>
{
    configuration.GetSection("AcuWeather").Bind(settings);
});

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseMigrationsEndPoint();
}
else
{
    // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles();
app.UseAzureAppConfiguration();
app.UseRouting();

app.UseAuthentication();
app.UseIdentityServer();
app.UseAuthorization();

app.MapControllerRoute(
    name: "default",
    pattern: "{controller}/{action=Index}/{id?}");
app.MapRazorPages();

app.MapFallbackToFile("index.html"); ;

app.Run();
