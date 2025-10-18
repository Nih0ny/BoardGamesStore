using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using BoardGamesStore.Data;
using BoardGamesStore.Models;
using BoardGamesStore.Services;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using System.Text;
using BoardGamesStore.Services.Settings;

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
var secretKey = jwtSettings["Secret"] ?? throw new InvalidOperationException("JWT Secret key not configured.");
var issuer = jwtSettings["Issuer"];
var audience = jwtSettings["Audience"];

builder.Services.AddAuthentication(options =>
{
	// Встановлення JwtBearer як схеми за замовчуванням
	options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
	options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
})
.AddJwtBearer(options =>
{
	options.TokenValidationParameters = new TokenValidationParameters
	{
		// 1. Параметри валідації
		ValidateIssuer = true, // Валідувати видавця токена
		ValidateAudience = true, // Валідувати одержувача токена
		ValidateLifetime = true, // Валідувати час життя токена (expirence)
		ValidateIssuerSigningKey = true, // Валідувати підпис (найважливіше!)

		// 2. Фактичні значення для валідації
		ValidIssuer = issuer,
		ValidAudience = audience,
		IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(secretKey)),

		// 3. Додаткові налаштування
		// Невеликий запас часу для валідації через розбіжність часу між серверами
		ClockSkew = TimeSpan.Zero
	};
});
builder.Services.AddControllersWithViews();

builder.Services.AddScoped(typeof(IGenericService<>), typeof(GenericService<>));
builder.Services.AddScoped<IAccountService, AccountService>();
builder.Services.AddScoped<IEmailService, EmailService>();
builder.Services.AddSingleton<IPasswordHasher<User>, PasswordHasher<User>>();
builder.Services.Configure<JwtSettings>(
		builder.Configuration.GetSection("JwtSettings"));

builder.Services.AddRazorPages();


builder.Environment.EnvironmentName = "Development"; // Set environment to Development

var app = builder.Build();

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
