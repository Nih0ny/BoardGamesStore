using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using BoardGamesStore.Data;
using BoardGamesStore.Models;
using BoardGamesStore.Services;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using System.Text;
using BoardGamesStore.Services.Settings;
using Microsoft.Extensions.ObjectPool;
using MailKit.Net.Smtp;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
var connectionString = builder.Configuration.GetConnectionString("DefaultConnection") ?? throw new InvalidOperationException("Connection string 'DefaultConnection' not found.");
builder.Services.AddDbContext<ApplicationDbContext>(options =>
	options.UseNpgsql(connectionString,
		npgsqlOptions => npgsqlOptions.MigrationsHistoryTable("__EFMigrationsHistory", "board_games_store")
));
builder.Services.AddDatabaseDeveloperPageExceptionFilter();

builder.Services.AddIdentity<User, Role>(options =>
	{
		options.Password.RequireDigit = true;
		options.Password.RequiredLength = 8;
	})
	.AddEntityFrameworkStores<ApplicationDbContext>()
	.AddDefaultTokenProviders();

var jwtSettings = builder.Configuration.GetSection("JwtSettings");
var accessSecretKey = jwtSettings["AccessSecret"];
var refreshSecretKey = jwtSettings["RefreshSecret"];
var issuer = jwtSettings["Issuer"];
var apiAudience = jwtSettings["AccessAudience"];
var refreshAudience = jwtSettings["RefreshAudience"];

builder.Services.AddAuthentication(options =>
{
	// Схемою за замовчуванням залишаємо перевірку Access токена
	options.DefaultAuthenticateScheme = "AccessToken";
	options.DefaultChallengeScheme = "AccessToken";
})
// 1. Схема для Access Token
.AddJwtBearer("AccessToken", options =>
{
	options.TokenValidationParameters = new TokenValidationParameters
	{
		ValidateIssuer = true,
		ValidateAudience = true,
		ValidateLifetime = true, // Перевіряємо, чи токен не прострочений
		ValidateIssuerSigningKey = true,

		ValidIssuer = issuer,
		ValidAudience = apiAudience, // Аудиторія для API
		IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(accessSecretKey)),
		ClockSkew = TimeSpan.Zero
	};
})
// 2. Схема для Refresh Token
.AddJwtBearer("RefreshToken", options =>
{
	options.TokenValidationParameters = new TokenValidationParameters
	{
		ValidateIssuer = true,
		ValidateAudience = true,
		ValidateLifetime = true, // Також перевіряємо час життя, це важливо!
		ValidateIssuerSigningKey = true,

		ValidIssuer = issuer,
		ValidAudience = refreshAudience, // Інша аудиторія для сервісу оновлення
		IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(refreshSecretKey)),
		ClockSkew = TimeSpan.Zero
	};
});
builder.Services.AddControllersWithViews();

builder.Services.Configure<JwtSettings>(
	builder.Configuration.GetSection("JwtSettings"));
builder.Services.Configure<SmtpSettings>(
	builder.Configuration.GetSection("SmtpSettings"));

// builder.Services.AddStackExchangeRedisCache(options =>
// {
// 	// Беремо рядок підключення з appsettings.json
// 	options.Configuration = builder.Configuration.GetConnectionString("Valkey");
// 	options.InstanceName = "BGS_"; // Префікс для ключів кешу (корисно, якщо кеш спільний)
// });

builder.Services.AddSingleton<IPooledObjectPolicy<SmtpClient>, SmtpClientPooledObjectPolicy>();

builder.Services.AddSingleton(serviceProvider =>
{
	var policy = serviceProvider.GetRequiredService<IPooledObjectPolicy<SmtpClient>>();
	var provider = new DefaultObjectPoolProvider
	{
		// Можна налаштувати максимальну кількість клієнтів у пулі, наприклад 10
		MaximumRetained = 10
	};
	return provider.Create(policy);
});

builder.Services.AddScoped(typeof(IGenericService<>), typeof(GenericService<>));
builder.Services.AddScoped<IAccountService, AccountService>();
builder.Services.AddScoped<IEmailService, EmailService>();
builder.Services.AddSingleton<IPasswordHasher<User>, PasswordHasher<User>>();

builder.Services.AddRazorPages();

builder.Environment.EnvironmentName = "Development"; // Set environment to Development

var app = builder.Build();

using (var scope = app.Services.CreateScope())
{
	var services = scope.ServiceProvider;
	try
	{
		var dbContext = services.GetRequiredService<ApplicationDbContext>();
		dbContext.Database.Migrate();
		Console.WriteLine("Migrations applied successfully.");
	}
	catch (Exception ex)
	{
		var logger = services.GetRequiredService<ILogger<Program>>();
		logger.LogError(ex, "An error occurred while applying migrations.");
	}
}

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
	app.UseMigrationsEndPoint();
}
else
{
	app.UseExceptionHandler("/Home/Error");
	// The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
	app.UseHsts();
}

app.UseHttpsRedirection();
app.UseRouting();

app.UseAuthentication();
app.UseAuthorization();

app.MapStaticAssets();
app.MapControllers();

app.MapControllerRoute(
	name: "default",
	pattern: "{controller=Home}/{action=Index}/{id?}")
	.WithStaticAssets();

app.MapRazorPages()
	.WithStaticAssets();

app.Run();
