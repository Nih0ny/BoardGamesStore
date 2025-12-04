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
using Microsoft.Extensions.Options;
using BoardGamesStore.Models.Entities;

var builder = WebApplication.CreateBuilder(args);

builder.Services.Configure<JwtSettings>(
	builder.Configuration.GetSection("JWT"));
builder.Services.Configure<SmtpSettings>(
	builder.Configuration.GetSection("Smtp"));
builder.Services.Configure<PaymentSettings>(
	builder.Configuration.GetSection("LiqPay"));

builder.Services.AddCors(options =>
{
	options.AddPolicy("AllowAll", policy =>
		{
			policy.AllowAnyOrigin()
						.AllowAnyMethod()
						.AllowAnyHeader();
		});

	options.AddPolicy("AllowSpecificOrigins", policy =>
		{
			policy.WithOrigins("http://localhost:3000", "https://yourdomain.com")
						.AllowAnyMethod()
						.AllowAnyHeader()
						.AllowCredentials();
		});
});

var connectionString = builder.Configuration.GetConnectionString("DefaultConnection") ?? throw new InvalidOperationException("Connection string 'DefaultConnection' not found.");
builder.Services.AddDbContext<ApplicationDbContext>(options =>
	options.UseNpgsql(connectionString));
builder.Services.AddDatabaseDeveloperPageExceptionFilter();

builder.Services.AddIdentity<User, IdentityRole>(options =>
	{
		options.Password.RequireDigit = true;
		options.Password.RequiredLength = 8;
		options.SignIn.RequireConfirmedAccount = false; // FIXME: видалити пізніше
	})
	.AddEntityFrameworkStores<ApplicationDbContext>()
	.AddDefaultTokenProviders()
	.AddRoles<IdentityRole>();

var secret = builder.Configuration["JWT:Secret"];
var issuer = builder.Configuration["JWT:Issuer"];
var audience = builder.Configuration["JWT:Audience"];

builder.Services.AddAuthentication(options =>
{
	options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
	options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
	options.DefaultScheme = JwtBearerDefaults.AuthenticationScheme;
})
.AddJwtBearer(options =>
{
	options.SaveToken = true;
	options.RequireHttpsMetadata = false;
	options.TokenValidationParameters = new TokenValidationParameters
	{
		ValidateIssuer = true,
		ValidateAudience = true,
		ValidateLifetime = true,
		ValidateIssuerSigningKey = false,

		ValidIssuer = issuer,
		ValidAudience = audience,
		IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(secret!)),
		ClockSkew = TimeSpan.Zero
	};
});

builder.Services.AddAuthorization();

//options =>
// {
// 	options.AddPolicy("AdminOnly", policy =>
// 			policy.RequireRole("Admin"));
// }

builder.Services.AddControllersWithViews();

// builder.Services.AddStackExchangeRedisCache(options =>
// {
// 	options.Configuration = builder.Configuration.GetConnectionString("Valkey");
// 	options.InstanceName = "BGS_";
// });

builder.Services.AddSingleton<IPasswordHasher<User>, PasswordHasher<User>>();
builder.Services.AddSingleton<IPooledObjectPolicy<SmtpClient>, SmtpClientPooledObjectPolicy>();

builder.Services.AddSingleton(serviceProvider =>
{
	var policy = serviceProvider.GetRequiredService<IPooledObjectPolicy<SmtpClient>>();
	var provider = new DefaultObjectPoolProvider
	{
		MaximumRetained = 10
	};
	return provider.Create(policy);
});
builder.Services.AddSingleton<IHttpContextAccessor, HttpContextAccessor>();
// builder.Services.AddScoped(typeof(IGenericService<>), typeof(GenericService<>));
builder.Services.AddScoped<ITokenService, TokenService>();
builder.Services.AddScoped<IAccountService, AccountService>();
builder.Services.AddScoped<IEmailService, EmailService>();

builder.Services.AddScoped<ICartService, CartService>();
//builder.Services.AddScoped<ICartItemService, CartService>();
builder.Services.AddScoped<IOrderService, OrderService>();
//builder.Services.AddScoped<IOrderItemService, OrderItemService>();
builder.Services.AddScoped<IProductService, ProductService>();
builder.Services.AddScoped<IProductReportService, ProductReportService>();
builder.Services.AddScoped<IWishlistService, WishlistService>();
builder.Services.AddScoped<IPaymentService, PaymentService>();

builder.Services.AddLogging(configure => configure.AddConsole());
builder.Services.AddHostedService<ProductRatingRefreshService>();

builder.Services.AddRazorPages();

builder.Environment.EnvironmentName = "Development";

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

	var roleManager = scope.ServiceProvider.GetRequiredService<RoleManager<IdentityRole>>();

	string[] roleNames = ["Admin", "User"];
	IdentityResult roleResult;

	foreach (var roleName in roleNames)
	{
		var roleExist = await roleManager.RoleExistsAsync(roleName);
		if (!roleExist)
		{
			roleResult = await roleManager.CreateAsync(new IdentityRole(roleName));
		}
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

//app.UseHttpsRedirection();
app.UseRouting();

app.UseCors("AllowAll");

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
