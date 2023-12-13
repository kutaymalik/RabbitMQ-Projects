using Azure;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Hosting;
using RabbitMQ.Client;
using UdemyRabbitMQ.ExcelCreate.Hubs;
using UdemyRabbitMQ.ExcelCreate.Models;
using UdemyRabbitMQ.ExcelCreate.Services;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddControllersWithViews();

builder.Services.AddDbContext<AppDbContext>(options =>
{
    options.UseSqlServer(builder.Configuration.GetConnectionString("SqlServer"));
});

builder.Services.AddIdentity<IdentityUser, IdentityRole>(opt =>
{
    opt.User.RequireUniqueEmail = true;

}).AddEntityFrameworkStores<AppDbContext>();

var rabbitMQConnectionString = builder.Configuration.GetConnectionString("RabbitMQ");

builder.Services.AddSingleton(sp => new ConnectionFactory() { Uri = new Uri(rabbitMQConnectionString), DispatchConsumersAsync = true });

builder.Services.AddSingleton<RabbitMQPublisher>();

builder.Services.AddSingleton<RabbitMQClientService>();

builder.Services.AddSignalR();

var app = builder.Build();

using(var scope = app.Services.CreateScope())
{
    var appDbContext = scope.ServiceProvider.GetRequiredService<AppDbContext>();

    var userManager = scope.ServiceProvider.GetRequiredService<UserManager<IdentityUser>>();

    appDbContext.Database.Migrate();

    if (!appDbContext.Users.Any())
    {
        userManager.CreateAsync(new IdentityUser() { UserName = "test1", Email = "test1@mail.com" }, "Parola1234!!").Wait();

        userManager.CreateAsync(new IdentityUser() { UserName = "test2", Email = "test2@mail.com" }, "Parola1234!!").Wait();
    }
}

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles();

app.UseRouting();
app.UseAuthentication();

app.UseAuthorization();

app.MapHub<MyHub>("/MyHub");

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");

app.Run();
