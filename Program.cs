using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.EntityFrameworkCore;
using BlindMatchPAS.Data;
using BlindMatchPAS.Models;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllersWithViews(options =>
{
    options.Filters.Add(new Microsoft.AspNetCore.Mvc.Authorization.AuthorizeFilter());
});

builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseSqlServer(
        builder.Configuration.GetConnectionString("DefaultConnection")
    )
);

builder.Services.AddAuthentication(CookieAuthenticationDefaults.AuthenticationScheme)
    .AddCookie(options =>
    {
        options.LoginPath = "/Auth/Login";
        options.LogoutPath = "/Auth/Logout";
        options.AccessDeniedPath = "/Auth/AccessDenied";
    });

var app = builder.Build();
/*
// Seed a test user due to  none exist yet
using (var scope = app.Services.CreateScope())
{
    var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
    db.Database.Migrate(); // make sure tables exist

    if (!db.Users.Any())
    {
        db.Users.Add(new BlindMatchPAS.Models.User
        {
            FullName = "Test Student",
            Email = "student@test.com",
            PasswordHash = BCrypt.Net.BCrypt.HashPassword("password123"),
            Role = "Student"
        });
        db.Users.Add(new BlindMatchPAS.Models.User
        {
            FullName = "Test Supervisor",
            Email = "supervisor@test.com",
            PasswordHash = BCrypt.Net.BCrypt.HashPassword("password123"),
            Role = "Supervisor"
        });
        db.Users.Add(new BlindMatchPAS.Models.User
        {
            FullName = "Test Admin",
            Email = "admin@test.com",
            PasswordHash = BCrypt.Net.BCrypt.HashPassword("password123"),
            Role = "Admin"
        });
        db.SaveChanges();
    }
}*/
app.UseStaticFiles();
app.UseRouting();

app.UseAuthentication();
app.UseAuthorization();

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Auth}/{action=Login}/{id?}"
);

app.Run();
