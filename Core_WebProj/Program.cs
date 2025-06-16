using Core_WebProj.Data;
using Core_WebProj.Models;
using DinkToPdf;
using DinkToPdf.Contracts;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);

// Note: builder.Services.AddDatabaseDeveloperPageExceptionFilter(); is commented out.
// This is typically for development to show detailed EF Core exceptions. You can uncomment it if needed for debugging.
//builder.Services.AddDatabaseDeveloperPageExceptionFilter();

// Register DinkToPdf converter as a singleton
builder.Services.AddSingleton<IConverter>(new SynchronizedConverter(new PdfTools()));

// Configure ApplicationDbContext to use SQL Server
builder.Services.AddDbContext<ApplicationDbContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));

// Configure ASP.NET Core Identity to use your custom ApplicationUser and IdentityRole
// This correctly registers UserManager<ApplicationUser> and RoleManager<IdentityRole>
builder.Services.AddIdentity<Core_WebProj.Models.ApplicationUser, IdentityRole>()
    .AddEntityFrameworkStores<ApplicationDbContext>() // Link Identity to your DbContext
    .AddDefaultTokenProviders(); // Required for token generation (e.g., password reset)

// The AddDefaultIdentity<IdentityUser> line was commented out, which is good
// as you are using custom user and role types with AddIdentity.
//builder.Services.AddDefaultIdentity<IdentityUser>(options =>
//{
//    options.SignIn.RequireConfirmedAccount = true;
//})
//.AddEntityFrameworkStores<ApplicationDbContext>();

// Add Razor Pages support
builder.Services.AddRazorPages();

// Register services for PDF generation from Razor Views
builder.Services.AddScoped<RazorViewToStringRenderer>();
builder.Services.AddScoped<PdfService>(provider =>
    new PdfService(
        provider.GetRequiredService<DinkToPdf.Contracts.IConverter>()
    ));

// Configure Email Settings from appsettings.json
builder.Services.Configure<EmailSettings>(builder.Configuration.GetSection("EmailSettings"));

// Register Email Service
builder.Services.AddScoped<IEmailService, EmailService>();

// Add MVC controllers support
builder.Services.AddControllersWithViews();

var app = builder.Build();

// Configure the HTTP request pipeline
if (app.Environment.IsDevelopment())
{
    // Use the migrations endpoint in development for database updates
    app.UseMigrationsEndPoint();
}
else
{
    // Configure error handling for production
    app.UseExceptionHandler("/Home/Error");
}

// Enable routing
app.UseRouting();

// Enable Authentication middleware (must come before UseAuthorization)
app.UseAuthentication();

// Enable Authorization middleware
app.UseAuthorization();

// Map static assets (assuming this is an extension method for your project)
app.MapStaticAssets();

// Map default controller route with static assets
app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}")
    .WithStaticAssets();

// Map Razor Pages with static assets
app.MapRazorPages()
    .WithStaticAssets();

// Run the application
app.Run();